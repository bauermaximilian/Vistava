// SPDX-License-Identifier: GPL-3.0-or-later

import { SourceSegmented, SourceSegmentedContentRetriever } from "../../../Dependencies/vistava.js/src/Shared/SourceSegmented.js";
import { Assert } from "../../../Dependencies/vistava.js/src/Shared/Assert.js";
import { RateLimiter } from "../../../Dependencies/vistava.js/src/Shared/RateLimiter.js";
import { TileValue } from "../../../Dependencies/vistava.js/src/Components/TileGrid/Shared/TileValue.js";
import { ArgumentError } from "../../../Dependencies/vistava.js/src/Errors/ArgumentError.js";

/**
 * @typedef {object} AuthenticationFileSystem
 */

/**
 * @typedef {import("../../../Dependencies/vistava.js/src/Shared/Source.js").SourceConfiguration & AdditionalSourceFileSystemConfiguration
 * } SourceFileSystemConfiguration
 */

export class SourceFileSystem extends SourceSegmented {
   /** 
    * @typedef {object} AdditionalSourceFileSystemConfiguration
    * @property {string} hostUrl
    * @property {AuthenticationFileSystem?} [authentication]
    */

   /**  @readonly @type {AuthenticationFileSystem?} */
   #authentication = null;
   /** @readonly @type {string} */
   #hostUrl;

   /**
    * @param {SourceFileSystemConfiguration} configuration 
    */   
   constructor(configuration) {
      super(configuration);

      Assert.stringNotEmptyOrWhitespacesOnly(configuration.hostUrl, "configuration.hostUrl");

      try {
         let hostUrl = new URL(configuration.hostUrl);
         hostUrl.search = "";
         hostUrl.hash = "";
         let pathSegments = hostUrl.pathname.split('/');
         if (pathSegments[pathSegments.length - 1].includes('.')) {
            pathSegments.pop();
         }
         hostUrl.pathname = pathSegments.join('/');
         
         this.#hostUrl = hostUrl.toString();
      } catch (error) {
         throw new ArgumentError("The specified configuration.hostUrl is invalid.");
      }

      if (configuration.authentication != null) {
         this.#authentication = configuration.authentication;
      }    
   }

   /**
    * @override
    * @protected
    * @param {string} query 
    * @returns {SourceFileSystemContentRetriever}
    */
   createContentRetriever(query) {
      return new SourceFileSystemContentRetriever(this.#hostUrl, query, this.#authentication);
   }
}

class SourceFileSystemContentRetriever extends SourceSegmentedContentRetriever {
   get pageLength() { return 10; }

   /** @readonly @type {RateLimiter} */
   #rateLimiter = new RateLimiter(25);
   /** @readonly @type {string} */
   #hostUrl;

   /**
    * @param {string} hostUrl 
    * @param {string} query 
    * @param {AuthenticationFileSystem?} [authentication]
    */
   constructor(hostUrl, query, authentication) {
      super(query);

      this.#hostUrl = hostUrl;
      if (!this.#hostUrl.endsWith("/")) {
         this.#hostUrl += "/";
      }
   }

   /**
    * @override
    * @param {number} page 
    * @returns {Promise<TileValue[]>}
    */
   async getPageTilesAsync(page) {
      let path = encodeURIComponent(this.query ?? "");
      let start = page * this.pageLength;
      let url = `${this.#hostUrl}api/fileEntries/${path}?start=${start}&limit=${this.pageLength}`;

      let response = await this.#rateLimiter.executeThrottledAsync(
         async () => await fetch(url));
      
      if (!response.ok) {
         throw new Error(`The API request failed (code ${response.status}).`);
      } else {
         let responseData = await response.json();
         return responseData?.map(value => value) ?? [];
      }
   }
}
