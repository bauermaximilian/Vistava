// SPDX-License-Identifier: GPL-3.0-or-later

import { app, dialog } from "electron";
import fs from "fs";
import { VistavaServiceManager } from "./VistavaServiceManager.js";
import { VistavaWindow } from "./VistavaWindow.js";
import { InvalidOperationError, MissingDependenciesError, MissingDotNetDependencyError } from "./Errors.js";

const cliFlagSkipDotnetCheck = "--skip-dotnet-check";

export class Vistava {
   /** @type {VistavaWindow} */
   #window;
   /** @type {VistavaServiceManager} */
   #service;

   constructor(skipDotnetCheck = false) {
      this.#window = new VistavaWindow();
      this.#service = new VistavaServiceManager(skipDotnetCheck);
   }

   /**
    * @param {string[]?} argv 
    * @returns {Promise<void>}
    */
   static async start(argv) {
      /** @type{Vistava?} */
      let vistava = null;

      let onClosing = () => {
         if (vistava !== null) {
            vistava.#service.stop();
         }
         app.quit();
      }

      app.on("window-all-closed", onClosing);

      await app.whenReady();

      let skipDotnetCheck = argv?.find(arg => arg.toLowerCase().startsWith(cliFlagSkipDotnetCheck)) != null;
      let urlFragment = "#/";
      try {
         if (argv != null && argv.length > 0) {
            let entryPath = argv[1].trim();         
            if (entryPath !== "." && fs.lstatSync(entryPath).isDirectory()) {
               urlFragment += entryPath;
            }
         }
      } catch { }
      
      try {
         vistava = new Vistava(skipDotnetCheck);
         await vistava.#service.start();
         if (vistava.#service.url === null) {
            throw new InvalidOperationError("The service did not provide a valid application URL.");
         }
         await vistava.#window.show(`${vistava.#service.url}${urlFragment}`);
      } catch (error) {
         if (vistava == null) {
            dialog.showErrorBox("Initialization failed", "The application couldn't be initialized properly.");
         } else if (error instanceof MissingDotNetDependencyError) {
            let result = await vistava.#window.showDialog({
               title: "Missing dependencies",
               message: `The application couldn't be started: ${error.message} ` +
                  "Do you want to open the website to download the missing dependencies?",
               buttons: ["OK", "Cancel"],
               type: "warning"
            });
            if (result.response === 0) {
               await vistava.#service.openBrowserWithDownloadLink();
            }
         }
         else if (error instanceof MissingDependenciesError) {
            dialog.showErrorBox("Missing dependencies",
               `The application couldn't be started due to missing dependencies.\n${error.message}`);
         } else {
            let errorMessage = error instanceof Error ? error.message : error;
            dialog.showErrorBox("Application error",
               `The application terminated unexpectedly.\n${errorMessage}`);
         }
         
         onClosing();
         return;
      }
   }
}

Vistava.start(process.argv).then(() => { });
