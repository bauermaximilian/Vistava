// SPDX-License-Identifier: GPL-3.0-or-later

import { app } from "electron";
import fs from "fs";
import path from "path";
import { spawn } from "child_process";
import { InvalidOperationError } from "./Errors.js";
import { DotNetChecker } from "./DotNetChecker.js";

export class VistavaServiceManager {
   get url() { return this.#url; }

   /** @type {import("child_process").ChildProcessByStdio?} */
   #process = null;
   /** @type {string?} */
   #url = null;
   /** @type {boolean} */
   #skipDotnetCheck;
   
   constructor(skipDotnetCheck = false) {
      this.#skipDotnetCheck = skipDotnetCheck;
   }

   async start() {
      if (!this.#skipDotnetCheck) {
         await DotNetChecker.ensureRuntimeInstalled();
      }

      if (this.#process !== null) {
         throw new InvalidOperationError("The service is already running.");
      }

      let servicePath = this.#getServicePath();
      let serviceCwd = path.dirname(servicePath);

      this.#process = await new Promise((resolve, reject) => {
         let childProcess = spawn(servicePath, ["--randomizeBasePath=true"], {
            stdio: "pipe",
            cwd: serviceCwd,
            windowsHide: true
         });
         childProcess.on('close', (code, signal) => { console.log(`Vistava Server: Closed with code ${code} and signal ${signal}`) })
         childProcess.on('error', (error) => { reject(error) })
         childProcess.on('exit', (code, signal) => { console.log(`Vistava Server: Exited with code ${code} and signal ${signal}`) })
         childProcess.on('spawn', () => { resolve(childProcess) })
      });

      await /** @type {Promise<void>} */(new Promise((resolve, reject) => {
         if (this.#process === null) {
            reject(new InvalidOperationError("The process was already stopped."));
            return;
         }
         let isResolved = false;
         let apiProcessFileHandler = (data) => {
            let appUrlCandidate = this.#extractAppRootUrl(String(data));
            if (appUrlCandidate != null && appUrlCandidate.trim().length > 0) {
               this.#process?.stdout.removeListener("data", apiProcessFileHandler);
               if (!appUrlCandidate.endsWith("/")) {
                  appUrlCandidate += "/";
               }
               this.#url = `${appUrlCandidate}app.html`;
               isResolved = true;
               resolve();
            }
         };
         this.#process.stdout.on("data", apiProcessFileHandler);
         setTimeout(() => {
            if (!isResolved) {
               isResolved = true;
               reject(`App service executable start timeout.`);
            }
         }, 5000);
      }));
   }

   stop() {
      this.#process?.kill();
      this.#process = null;
   }

   /**
    * Opens the default web browser and navigates to a page where the user can download dotnet.
    */
   async openBrowserWithDownloadLink() {
      await DotNetChecker.openBrowserWithDownloadLink();
   }

   /**
    * @param {string} input 
    * @returns {string?}
    */
   #extractAppRootUrl(input) {
      let regex = /Application started under URL '(.*?)'/;
      let match = input.match(regex);
      if (match && match[1]) {
         return match[1];
      }
      return null;
   }

   /**
    * @returns {string}
    */
   #getServicePath() {
      let executablePath;
      if (process.platform === "win32") {
         executablePath = "win/Vistava.Service.exe"
      } else if (process.platform === "linux") {
         executablePath = "linux/Vistava.Service.elf"
      } else {
         throw new Error(`Unsupported platform "${process.platform}".`);
      }
      executablePath = path.join(executablePath);

      let executabePathDev = path.join(app.getAppPath(), "service/bin/Publish/", executablePath);
      let executablePathProd = path.join(process.resourcesPath, "app", "bin", executablePath);

      if (fs.existsSync(executabePathDev)) {
         return executabePathDev;
      } else if (fs.existsSync(executablePathProd)) {
         return executablePathProd;
      } else {
         console.log(`Couldn't find service executable under none of the following paths:\n` +
            `'${executabePathDev}', '${executablePathProd}'.`);
         throw new Error(`Service executable not found. Reinstall the application and try again.`);
      }
   }
}
