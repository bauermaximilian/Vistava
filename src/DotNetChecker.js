// SPDX-License-Identifier: GPL-3.0-or-later

import { exec } from "child_process";
import { promisify } from "util";
import { MissingDotNetDependencyError } from "./Errors.js";

const defaultVersion = "8.0";
const cliFlagSdk = "--check-sdk";
const cliFlagRuntime = "--check-runtime";

/**
 * @param {string} command 
 * @returns {Promise<{stdout:string,stderr:string}>}
 */
const execAsync = async (command) => await (promisify(exec))(command, { windowsHide: true });

export class DotNetChecker {
   /**
    * Gets the default version, which is currently being used by the dotnet-related parts of the application.
    */
   static get defaultVersion() { return defaultVersion; }

   /** 
    * Checks for a specific dotnet SDK installation. 
    * @param {string} [version] The major version number ({@link defaultVersion} by default).
    * @returns {Promise<string>} The full version number and installation location.
    * @throws {MissingDotNetDependencyError} Is thrown when the specified version of the dotnet SDK wasn't found.
    */
   static async ensureSdkInstalled(version = defaultVersion) {
      return await DotNetChecker.#execAndSearchVersionInOutput(true, version);
   }

   /** 
    * Checks for a specific dotnet SDK installation. 
    * @param {string} [version] The major version number ({@link defaultVersion} by default).
    * @returns {Promise<string>} The full version number and installation location.
    * @throws {MissingDotNetDependencyError} Is thrown when the specified version of the dotnet runtime wasn't found.
    */
   static async ensureRuntimeInstalled(version = defaultVersion) {
      return await DotNetChecker.#execAndSearchVersionInOutput(false, version);
   }

   /**
    * Opens the default web browser and navigates to a page where the user can download dotnet.
    * @param {string} [version] The major version number ({@link defaultVersion} by default).
    */
   static async openBrowserWithDownloadLink(version = defaultVersion) {
      let url = DotNetChecker.generateDotnetDownloadUrl(version);
      if (process.platform === "linux") {
         await execAsync(`xdg-open ${url}`);
      } else if (process.platform === "win32") {
         await execAsync(`start ${url}`);
      }
   }

   /**
    * @param {boolean} checkSdk 
    * @param {string} version 
    */
   static async #execAndSearchVersionInOutput(checkSdk, version) {
      let sdkVersionString, runtimeVersionString, aspRuntimeVersionString;

      try {
         let output = await execAsync(`dotnet --list-${checkSdk ? "sdks" : "runtimes"}`);
         for (let row of output.stdout.split(/\n/) ?? "") {
            if (checkSdk && row.startsWith(`${version}`) && !sdkVersionString) {
               sdkVersionString = row;
            } else if (!checkSdk && row.startsWith(`Microsoft.NETCore.App ${version}`) && !runtimeVersionString) {
               runtimeVersionString = row;
            } else if (!checkSdk && row.startsWith(`Microsoft.AspNetCore.App ${version}`) && !aspRuntimeVersionString) {
               aspRuntimeVersionString = row;
            }
         }
      } catch { }

      let missingDependencies = [];
      if (checkSdk && sdkVersionString == null) {
         missingDependencies.push("'.NET SDK'");
      } else if (!checkSdk && runtimeVersionString == null) {
         missingDependencies.push("'.NET Core Runtime'");
      } else if (!checkSdk && aspRuntimeVersionString == null) {
         missingDependencies.push("'ASP.NET Core Runtime'");
      }

      if (missingDependencies.length > 0) {
         throw new MissingDotNetDependencyError(missingDependencies.join(" and ") +
            ` (version ${version}) ${missingDependencies.length > 1 ? "are" : "is"} not installed.`);
      } else {
         let versions = [];
         if (sdkVersionString != null) {
            versions.push(sdkVersionString);
         }
         if (runtimeVersionString != null) {
            versions.push(runtimeVersionString);
         }
         if (aspRuntimeVersionString != null) {
            versions.push(aspRuntimeVersionString);
         }
         return versions.join(", ");
      }
   }

   /**
    * @param {string} version 
    * @returns {string}
    */
   static generateDotnetDownloadUrl(version) {
      return `https://dotnet.microsoft.com/en-us/download/dotnet/${version}`
   }
}

// If the script is launched directly using nodejs (e.g. "node DotNetChecker.js --check-sdk"),
// the check is performed directly.
if (process.argv.length > 1 && process.argv[1].endsWith("DotNetChecker.js")) {
   let cliArgSdk = process.argv.find(arg => arg.toLowerCase().startsWith(cliFlagSdk)) ?? null;
   let cliArgRuntime = process.argv.find(arg => arg.toLowerCase().startsWith(cliFlagRuntime)) ?? null;

   try {
      if (cliArgSdk !== null) {
         console.info(`Searching for suitable dotnet SDK version...`);
         let version = await DotNetChecker.ensureSdkInstalled(defaultVersion);
         console.info(`Found suitable dotnet SDK version (${version}).`);
      }
      if (cliArgRuntime !== null) {
         console.info(`Searching for suitable dotnet runtime version...`);
         let version = await DotNetChecker.ensureRuntimeInstalled(defaultVersion);
         console.info(`Found suitable runtime version (${version}).`);
      }
   } catch (error) {
      if (error instanceof MissingDotNetDependencyError) {
         console.error(`ERROR: ${error.message}`);
         console.info("You can find more information on the missing dependencies under the following URL:\n" +
            DotNetChecker.generateDotnetDownloadUrl(defaultVersion));
         process.exit(1);
      } else {
         throw error;
      }
   }
   if (cliArgSdk === null && cliArgRuntime === null) {
      console.warn(`WARNING: No valid flag for checking a dotnet SDK or runtime installation was specified - no check was performed!`);
   }
}