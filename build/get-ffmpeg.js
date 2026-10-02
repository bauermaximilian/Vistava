// SPDX-License-Identifier: GPL-3.0-or-later

import { unpack } from '7zip-min';
import { createWriteStream, existsSync, rmSync } from "fs";
import { mkdir, readdir, stat, copyFile, rmdir, rm } from "fs/promises";
import { finished } from "stream/promises";
import { resolve, join, basename, dirname } from "path";
import { Readable } from "stream";

const ffmpegFilePath = "build/ffmpeg/ffmpeg.exe";
const ffprobeFilePath = "build/ffmpeg/ffprobe.exe";

const url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.7z";
const tempDirectoryPathRelative = "build/ffmpeg";
const downloadFileName = "ffmpeg-release-essentials.7z";
const ffmpegSourceFileName = "ffmpeg.exe";
const ffprobeSourceFileName = "ffprobe.exe";

/**
 * @param {string} pathRelative 
 * @returns {Promise<string>}
 */
const getFullTempPathAsync = async (pathRelative) => {
   let tempPath = resolve(pathRelative);
   if (!existsSync(tempPath)) {
      await mkdir(tempPath);
   }
   return tempPath;
}

/**
 * @param {string} url 
 * @param {string} targetPath 
 * @returns {Promise<void>}
 */
const downloadFileAsync = async (url, targetPath) => {
   if (existsSync(targetPath)) {
      try {
         rmSync(targetPath, { force: true, maxRetries: 3, retryDelay: 1000 });
      } catch (error) {
         throw new Error(`Downloaded file already exists and can't be overwritten! Reason: ${error}`);
      }
   }

   let filename = basename(targetPath);
   console.info(`Downloading from "${url}"...`);

   let fileStream = null;
   try {
      fileStream = createWriteStream(targetPath, { flags: 'wx' });
   } catch (error) {
      throw new Error(`Couldn't create file at specified download file location. Reason: ${error}`);
   }

   try {
      let response = await fetch(url);
      //@ts-ignore
      await finished(Readable.fromWeb(response.body).pipe(fileStream));
      console.info(`Download of file "${filename}" finished.`);
      fileStream.close();
   } catch (error) {
      fileStream.close();
      try {
         rmSync(targetPath);
      } catch { }
      throw new Error(`Download failed. Reason: ${error}`);
   }
};

/**
 * @param {string} filePath 
 * @returns {Promise<void>}
 */
const extractArchiveAsync = async (filePath) => {
   let fileName = basename(filePath);
   let outputDir = dirname(filePath);
   console.info(`Unpacking "${fileName}"...`)
   try {
      await unpack(filePath, outputDir);
   } catch (error) {
      throw new Error(`Couldn't unpack downloaded archive "${fileName}". Reason: ${error}`);
   }
};

/**
 * @param {string} rootPath 
 * @param {string} expectedPrefix 
 * @returns {Promise<string?>}
 */
const findExtractedRootFolderAsync = async (rootPath, expectedPrefix) => {
   let files = await readdir(rootPath);
   for (let file of files) {
      let filePath = join(rootPath, file);
      let fileBasename = basename(filePath);
      let fileStats = await stat(filePath);
      if (fileStats.isDirectory() && fileBasename.startsWith(expectedPrefix)) {
         return filePath;
      }
   }
   return null;
};

/**
 * @param {string} rootPath 
 * @param {string} fileName 
 * @returns {Promise<string?>}
 */
const findFileAsync = async (rootPath, fileName) => {
   let files = await readdir(rootPath);

   for (let file of files) {
      let filePath = join(rootPath, file);
      let fileStats = await stat(filePath);
      if (fileStats.isDirectory()) {
         let result = await findFileAsync(filePath, fileName);
         if (result !== null) {
            return result;
         }
      } else if (fileName === file) {
         return filePath;
      }
   }

   return null;
};

/**
 * @param {string} path 
 * @returns {Promise<void>}
 */
const removeDirectoryAsync = async (path) => {
   let files = await readdir(path);

   for (let file of files) {
      let filePath = join(path, file);
      let fileStats = await stat(filePath);
      if (fileStats.isDirectory()) {
         await removeDirectoryAsync(filePath);
      } else {
         await rm(filePath, { retryDelay: 250, maxRetries: 2, force: true });
      }
   }

   await rmdir(path);
};

console.info("** FFMPEG DEPENDENCY CHECK **");
console.info("Searching for ffmpeg.exe and ffprobe.exe in \"build\"...");

if (existsSync(ffmpegFilePath) && existsSync(ffprobeFilePath)) {
   console.info("ffmpeg.exe and ffprobe.exe found in \"build\" directory! Skipping download.");
} else {
   console.info("ffmpeg.exe and ffprobe.exe not found in \"build\" directory! Initiating download...");
   let tempDirectoryPath = await getFullTempPathAsync(tempDirectoryPathRelative);

   let downloadedArchivePath = join(tempDirectoryPath, downloadFileName);
   let extractedRoot = null;

   try {
      await downloadFileAsync(url, downloadedArchivePath);

      await extractArchiveAsync(downloadedArchivePath);

      extractedRoot = await findExtractedRootFolderAsync(tempDirectoryPath, "ffmpeg");
      if (extractedRoot === null) {
         throw new Error("Extracted directory couldn't be found or had an unexpected name.");
      }

      console.info("Copying relevant files from extracted archive...");
      let ffmpegSourcePath = await findFileAsync(extractedRoot, ffmpegSourceFileName);
      if (ffmpegSourcePath == null) {
         throw new Error(`The downloaded archive didn't contain the required ${ffmpegSourceFileName} file!`);
      }
      let ffprobeSourcePath = await findFileAsync(extractedRoot, ffprobeSourceFileName);
      if (ffprobeSourcePath == null) {
         throw new Error(`The downloaded archive didn't contain the required ${ffprobeSourceFileName} file!`);
      }
      await copyFile(ffmpegSourcePath, ffmpegFilePath);
      await copyFile(ffprobeSourcePath, ffprobeFilePath);

      console.info("Dependencies copied successfully!");
   } catch (error) {
      console.error("ERROR: Couldn't download ffmpeg dependencies! The produced build might not work.\n" +
         "Please copy ffmpeg.exe/ffprobe.exe (version 8+) into the \"build\" directory and try again.\n" +
         `Error details: ${error}`);
   } finally {
      console.info("Cleaning up temporary files...");
      try {
         if (extractedRoot !== null) {
            await removeDirectoryAsync(extractedRoot);
         }
      } catch { }
      try {
         await rm(downloadedArchivePath);
      } catch { }
   }
}