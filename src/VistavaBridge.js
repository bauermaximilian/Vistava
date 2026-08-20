// SPDX-License-Identifier: GPL-3.0-or-later

const { contextBridge, shell } = require("electron");
const url = require("url");
const fs = require("node:fs");

contextBridge.exposeInMainWorld("vistavaBridge", {
   openUrl: (/** @type {string} */ urlString) => {
      let urlObject;
      try { urlObject = new URL(urlString); }
      catch { urlObject = null; }

      if (urlObject === null) {
         return false;
      }

      if (urlObject.protocol === "http:" ||
         urlObject.protocol === "https:") {
         shell.openExternal(urlString);
         return true;
      } else if (urlObject.protocol === "file:") {
         let urlPath = url.fileURLToPath(urlObject);
         if (fs.existsSync(urlPath)) {
            if (fs.lstatSync(urlPath).isDirectory()) {
               shell.openPath(urlPath);
            } else {
               shell.showItemInFolder(urlPath);
            }
            return true;
         } else {
            console.warn(`File or directory "${urlPath}" couldn't be found.`);
            return false;
         }
      } else {
         console.warn(`Protocol "${urlObject.protocol}" not supported.`);
         return false;
      }
   }
});