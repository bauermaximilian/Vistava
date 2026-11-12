// SPDX-License-Identifier: GPL-3.0-or-later

import { dirname } from "path";
import { MissingDependenciesError } from "./Errors.js";
import { execAsync, VistavaServiceManager } from "./VistavaServiceManager.js";

export class FFmpegChecker {
   static async ensureFFmpegInstalled() {
      let servicePath = VistavaServiceManager.getServicePath();
      let serviceDirectoryPath = dirname(servicePath);

      let ffmpegVersion = await execAsync("ffmpeg -version", serviceDirectoryPath);
      let ffprobeVersion = await execAsync("ffprobe -version", serviceDirectoryPath);

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