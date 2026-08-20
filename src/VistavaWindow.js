// SPDX-License-Identifier: GPL-3.0-or-later

import { BrowserWindow, dialog, screen } from "electron";
import { fileURLToPath } from "url";
import path from "path";

export class VistavaWindow {
   /** @type {BrowserWindow} */
   #window;

   constructor() {
      const primaryDisplay = screen.getPrimaryDisplay()
      const { width, height, x, y } = primaryDisplay.workArea;

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
    * @returns {Promise<Electron.MessageBoxReturnValue>}
    */
   async showDialog(options) {
      return await dialog.showMessageBox(this.#window, options);
   }

   #handleOnClose = (/** @type {{ preventDefault: () => void; }} */ event) => {
      this.#window.webContents.session.clearCache().finally(() => this.#window.destroy());
      event.preventDefault();
   };
}
