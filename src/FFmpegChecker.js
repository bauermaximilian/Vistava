// SPDX-License-Identifier: GPL-3.0-or-later

import { exec } from "child_process";
import { promisify } from "util";
import { MissingDependenciesError } from "./Errors.js";

/**
 * @param {string} command 
 * @param {boolean} [hideWindow=true]
 * @returns {Promise<{stdout:string,stderr:string}>}
 */
const execAsync = async (command, hideWindow = true) => await (promisify(exec))(command, { windowsHide: hideWindow });

export class FFmpegChecker {
   static async ensureFFmpegInstalled() {
      var ffmpegVersion = await execAsync("ffmpeg -version");
      var ffprobeVersion = await execAsync("ffprobe -version");

      if (ffmpegVersion.stderr != null && ffmpegVersion.stderr.trim().length > 0) {
         throw new MissingDependenciesError("ffmpeg isn't available.");
      }
      if (ffprobeVersion.stderr != null && ffprobeVersion.stderr.trim().length > 0) {
         throw new MissingDependenciesError("ffprobe isn't available.");
      }
   }

   static async openBrowserToDownloadFFmpeg() {
      if (process.platform === "linux") {
         await execAsync(`xdg-open https://ffmpeg.org/download.html`);
      } else if (process.platform === "win32") {
         await execAsync(`start https://ffmpeg.org/download.html`);
      }
   }
}