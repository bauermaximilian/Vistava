// SPDX-License-Identifier: GPL-3.0-or-later

import { app, dialog } from "electron";
import fs from "fs";
import { VistavaServiceManager } from "./VistavaServiceManager.js";
import { VistavaWindow } from "./VistavaWindow.js";
import { InvalidOperationError, MissingDependenciesError } from "./Errors.js";
import { FFmpegChecker } from "./FFmpegChecker.js";

const cliFlagSkipFFmpegCheck = "--skip-ffmpeg-check";
const cliFlagDebug = "--debug-mode";

export class Vistava {
   /** @type {VistavaWindow} */
   #window;
   /** @type {VistavaServiceManager} */
   #service;

   constructor() {
      this.#window = new VistavaWindow();
      this.#service = new VistavaServiceManager();
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

      let skipFFmpegCheck = argv?.find(arg => arg.toLowerCase().startsWith(cliFlagSkipFFmpegCheck)) != null;
      
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
         vistava = new Vistava();
         
         await vistava.#service.start(argv?.find(arg => arg.toLowerCase().startsWith(cliFlagDebug)) != null);
         if (vistava.#service.url === null) {
            throw new InvalidOperationError("The service did not provide a valid application URL.");
         }

         if (!skipFFmpegCheck) {
            try {
               await FFmpegChecker.ensureFFmpegInstalled();
            } catch (error) {
               let result = await vistava.#window.showDialog({
                  title: "Missing dependencies",
                  message: "FFmpeg/FFprobe is not installed or wasn't found.\n" +
                     "The application can be used, but video thumbnails will not be available. " +
                     "Do you want to open a web browser to download ffmpeg?",
                  buttons: ["Yes", "No"],
                  type: "warning"
               });

               if (result.response === 0) {
                  await FFmpegChecker.openBrowserToDownloadFFmpeg();
                  onClosing();
                  return;
               }
            }
         }

         await vistava.#window.show(`${vistava.#service.url}${urlFragment}`);
      } catch (error) {
         if (vistava == null) {
            dialog.showErrorBox("Initialization failed", "The application couldn't be initialized properly.");
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
