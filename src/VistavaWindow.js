// SPDX-License-Identifier: GPL-3.0-or-later

import { BrowserWindow, dialog, screen, ipcMain, app, shell } from "electron";
import { fileURLToPath } from "url";
import { unpack, list } from '7zip-min';
import fs from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import path from "path";
import { execAsync } from "./VistavaServiceManager.js";

const extensionBundleFileExtension = "vex.zip";
const extensionDefinitionFileExtension = ".source.json";
const extensionTargetSubPath = "Service/v1.0/include/Sources";
const shownMessagesFileSubPath = "Service/v1.0/.messageHistory.json";

export class VistavaWindow {
   /** @type {BrowserWindow} */
   #window;
   /** @readonly @type {string} */
   #extensionTargetPath;
   /** @readonly @type {string} */
   #shownMessagesFilePath;

   constructor() {
      this.#extensionTargetPath = path.join(app.getPath("userData"), extensionTargetSubPath);
      this.#shownMessagesFilePath = path.join(app.getPath("userData"), shownMessagesFileSubPath);

      const primaryDisplay = screen.getPrimaryDisplay()
      const { width, height, x, y } = primaryDisplay.workArea;

      ipcMain.handle('showExtensionImportDialog', this.#handleShowExtensionImportDialogAsync);

      this.#window = new BrowserWindow({
         titleBarStyle: "hidden",
         titleBarOverlay: {
            color: '#141414',
            symbolColor: '#b7b7b7',
            height: 36
         },
         width, height, x, y,
         show: false,
         webPreferences: {
            preload: path.join(path.dirname(fileURLToPath(import.meta.url)), "VistavaBridge.js"),
            nodeIntegration: true
         }
      });
      
      this.#window.on("close", this.#handleOnClose);
   }

   /**
    * @param {string} url 
    * @returns {Promise<void>}
    */
   async show(url) {
      await this.#window.loadURL(url);
      this.#window.maximize();
   }

   close() {
      this.#window.close();
   }

   openDevTools() {
      this.#window?.webContents.openDevTools();
   }

   /**
    * @param {Electron.MessageBoxOptions} options
    * @param {string} [oneTimeDialogIdentifier]
    * @param {number} [defaultResponse]
    * @returns {Promise<Electron.MessageBoxReturnValue>}
    */
   async showDialog(options, oneTimeDialogIdentifier, defaultResponse) {
      let shownMessageIdentifiers = new Set();
      try {
         if (fs.existsSync(this.#shownMessagesFilePath)) {
            let identifiers = JSON.parse(await readFile(this.#shownMessagesFilePath, "utf8"));
            if (identifiers != null && Array.isArray(identifiers)) {
               shownMessageIdentifiers = new Set(identifiers.map(i => String(i)));
            }
         }
      } catch (error) {
         console.warn(`The message history file "${this.#shownMessagesFilePath}" can't be parsed: ${error}`);
      }

      /** @type {Electron.MessageBoxReturnValue} */
      let dialogResult;
      if (oneTimeDialogIdentifier == null || !shownMessageIdentifiers.has(oneTimeDialogIdentifier)) {
         dialogResult = await dialog.showMessageBox(this.#window, options);
      } else {
         dialogResult = { response: defaultResponse ?? 0, checkboxChecked: false}
      }

      if (oneTimeDialogIdentifier != null) {
         shownMessageIdentifiers.add(oneTimeDialogIdentifier);
         try {
            await writeFile(this.#shownMessagesFilePath, JSON.stringify(Array.from(shownMessageIdentifiers)), "utf8");
         } catch (error) {
            console.warn(`The message history file "${this.#shownMessagesFilePath}" can't be written: ${error}`);
         }
      }

      return dialogResult;
   }

   async showFlatpakHintIfApplicable() {
      let containerEnv = process.env["container"];
      if (containerEnv != null && process.platform === "linux") {
         let dialogResult = await this.showDialog({
            type: "info",
            title: "Limited file access",
            message: "Flatpak limits Vistava's file access to your images and videos folders. " +
               "The documentation explains how to grant access to other locations. Open it now?",
            buttons: ["No", "Yes"],
            defaultId: 1,
            cancelId: 0
         }, "flatpakInfo", 0);
         if (dialogResult.response === 1) {
            await execAsync(`xdg-open https://vistava.net/docs.html#/2-installation?id=flatpak`);
         }
      }
   }

   #handleOnClose = (/** @type {{ preventDefault: () => void; }} */ event) => {
      this.#window.webContents.session.clearStorageData().finally(() => this.#window.destroy());
      event.preventDefault();
   };

   #handleShowExtensionImportDialogAsync = async () => {
      let warningResult = await this.showDialog({
         type: "warning",
         title: "Extension security warning",
         message: "Extensions could contain malicious code or leak your personal data. " +
            "Only install them from trusted sources! Do you want to proceed?",
         buttons: ["Yes", "No"],
         defaultId: 1,
         cancelId: 1
      }, "extensionWarning", 0);

      if (warningResult.response === 0) {
         let extensionResult = await dialog.showOpenDialog(this.#window, {
            filters: [
               { name: "Vistava extension", extensions: [extensionBundleFileExtension] },
               { name: "All Files", extensions: ["*"] }
            ],
            properties: ["openFile", "dontAddToRecent"]
         });

         let extensionFilePath = !extensionResult.canceled ? extensionResult.filePaths[0] ?? null : null;

         if (extensionFilePath != null) {
            await this.#installExtensionAsync(extensionFilePath);
         }         
      }      
   }

   /**
    * @param {string} extensionFilePath 
    * @returns {Promise<string>}
    */
   async #getExtensionIdentiferAsync(extensionFilePath) {
      let extensionFileEntries;
      try {
         extensionFileEntries = await list(extensionFilePath);
      } catch (error) {
         throw new Error("Couldn't list content of extension archive file. " + error);
      }

      // Check if there's exactly one definition file in the root directory
      let definitionFileEntries = extensionFileEntries.filter(
         e => e.name.endsWith(extensionDefinitionFileExtension) && !e.name.includes(path.sep) &&
            (e.size != null && parseInt(e.size) > 0) && e.encrypted === "-");
      
      if (definitionFileEntries.length === 1) {
         let definitionFileName = definitionFileEntries[0].name;
         let extensionIdentifier = definitionFileName.replace(extensionDefinitionFileExtension, "");

         // Check if any file in the archive is not in its respective extension subdirectory
         // with the same name as the definition file (without its extension)
         let forbiddenFileEntries = extensionFileEntries.filter(
            e => !e.name.startsWith(extensionIdentifier + path.sep) && e.name !== definitionFileName &&
               !(e.name === extensionIdentifier && e.attr?.startsWith("D") === true));
         
         if (forbiddenFileEntries.length > 0) {
            throw new Error("The following entries in the extension archive are not allowed: " +
               forbiddenFileEntries.map(e => e.name).join(", "));
         }

         return extensionIdentifier;
      } else {
         throw new Error("The extension archive file didn't contain exactly one valid accessible " +
            "extension definition file with the '.source.json' extension in the archive root.");
      }
   }

   /**
    * @param {string} extensionIdentifier 
    * @returns {boolean}
    */
   #isExtensionAlreadyInstalled(extensionIdentifier) {
      let extensionDefinitionFile = path.join(this.#extensionTargetPath,
         extensionIdentifier + extensionDefinitionFileExtension);
      let extensionDirectory = path.join(this.#extensionTargetPath, extensionIdentifier);
      return fs.existsSync(extensionDefinitionFile) || fs.existsSync(extensionDirectory);
   }

   /**
    * @param {string} extensionFilePath 
    * @returns {Promise<void>}
    */
   async #installExtensionAsync(extensionFilePath) {
      let extensionIdentifier;
      try {
         extensionIdentifier = await this.#getExtensionIdentiferAsync(extensionFilePath);
      } catch (error) {
         console.error(`Failed loading invalid extension. Error: ${error}`);
         dialog.showErrorBox("Invalid extension file",
            "The specified file is not a valid Vistava extension file and can't be installed. " +
            "Please consult the extension author for assistance.");
         return;
      }
      let extensionConfigurationFilePath = path.join(this.#extensionTargetPath,
         extensionIdentifier + extensionDefinitionFileExtension);

      if (this.#isExtensionAlreadyInstalled(extensionIdentifier)) {
         let warningResult = await this.showDialog({
            type: "warning",
            title: "Extension already exists",
            message: `The extension "${extensionIdentifier}" is already installed. ` +
               "The existing extension will be overwritten and any existing configuration will " +
               "be reset to default. Are you sure you want to proceed?",
            buttons: ["Yes", "No"],
            defaultId: 1,
            cancelId: 1
         });
         if (warningResult.response !== 0) {
            return;
         }
      }

      try {
         await unpack(extensionFilePath, this.#extensionTargetPath)
      } catch (error) {
         dialog.showErrorBox("Extension installation failed",
            "The installation of the extension failed. Please make sure you have sufficient " +
            "privileges and sufficient free disk space and try again.");
         return;
      }

      let successResult = await this.showDialog({
         type: "info",
         title: "Extension installed successfully",
         message: `The extension "${extensionIdentifier}" was installed successfully! ` +
            "It will be available after you restarted Vistava. Do you want to open the " +
            "extension configuration now?",
         buttons: ["Yes", "No"],
         defaultId: 0,
         cancelId: 1
      });

      if (successResult.response === 0) {
         shell.openPath(extensionConfigurationFilePath);
      }
   }
}
