// SPDX-License-Identifier: GPL-3.0-or-later

import { Assert } from "../../Dependencies/vistava.js/src/Shared/Assert.js";
import { EventController } from "../../Dependencies/vistava.js/src/Shared/Event.js";

/**
 * @typedef {{ 
 *    sourceUpdated: boolean,
 *    pathUpdated: boolean, 
 *    searchParamsUpdated: boolean, 
 *    externalOrigin:boolean 
 * }} HashRouterUpdatedEventArgs
 */

export class HashRouter {
   /** @readonly @type {string} */
   static #hashPrefix = "#";
   /** @readonly @type {string} */
   static #sourcePathSeparator = ":";
   /** @readonly @type {string} */
   static #searchParamsSeparator = "?";

   /** @type {string} */
   #path = "";
   /** @type {string} */
   #source = "";
   /** @type {HashURLSearchParams} */
   #searchParams = new HashURLSearchParams();
   /** @type {boolean} */
   #autoUpdateWindowHash = true;
   /** @type {boolean} */
   #ignoreHashChanges = false;
   /** @type {boolean} */
   #ignoreSearchParamsChanges = false;
   /** @type {boolean} */
   #disableHistoryChanges = false;

   /** @type {EventController<HashRouterUpdatedEventArgs>} */
   #onUpdated = new EventController();

   get onUpdated() { return this.#onUpdated.event; }

   get path() { return this.#path; }

   set path(value) { 
      if (this.#path !== value) {
         Assert.string(value);
         this.#path = value;
         this.#considerUpdatingWindowHash();
         this.#onUpdated.trigger({
            sourceUpdated: false,
            pathUpdated: true,
            searchParamsUpdated: false,
            externalOrigin: false 
         });
      }
   }

   get source() { return this.#source; }

   set source(value) {
      if (this.#source !== value) {
         Assert.string(value);
         this.#source = value;
         this.#considerUpdatingWindowHash();
         this.#onUpdated.trigger({
            sourceUpdated: true,
            pathUpdated: false,
            searchParamsUpdated: false,
            externalOrigin: false 
         });
      }
   }

   /** @type {URLSearchParams} */
   get searchParams() {
      return this.#searchParams;
   }

   /** @type {string} */
   get hash() {
      return this.hashPathAndSourceOnly + HashRouter.#searchParamsSeparator + this.#searchParams.toString();
   }

   get hashPathAndSourceOnly() {
      return HashRouter.#hashPrefix + encodeURI(this.#source) +
         HashRouter.#sourcePathSeparator + encodeURI(this.#path);
   }

   get autoUpdateWindowHash() {
      return this.#autoUpdateWindowHash;
   }
   set autoUpdateWindowHash(value) {
      this.#autoUpdateWindowHash = value;
   }
   
   get disableHistoryChanges() {
      return this.#disableHistoryChanges;
   }
   set disableHistoryChanges(value) {
      this.#disableHistoryChanges = value;
   }

   constructor() {
   }

   attach() {
      window.addEventListener("hashchange", this.#handleOnHashChanged);
      this.#searchParams.onChange.subscribe(this.#handleOnSearchParamsChanged);

      // Covers the cases "first loaded page hash contains parameters to apply" and
      // "first broadcast of hash parameters to external subscribers".
      if (!this.#handleOnHashChanged()) {
         this.#onUpdated.trigger({
            sourceUpdated: true,
            externalOrigin: true,
            pathUpdated: true,
            searchParamsUpdated: true
         });
      }

      this.#considerUpdatingWindowHash();
   }

   detach() {
      window.removeEventListener("hashchange", this.#handleOnHashChanged);
      this.#searchParams.onChange.unsubscribe(this.#handleOnSearchParamsChanged);
   }

   /**
    * @param {boolean} [hideFromHistory]
    */
   updateWindowHash(hideFromHistory) {
      this.#ignoreHashChanges = true;

      let hash = this.hash;

      if ((hideFromHistory ?? this.#disableHistoryChanges) &&
         window.location.hash.trim().length > 0) {
         location.replace(hash);
      } else {
         location.assign(hash);
      }

      this.#ignoreHashChanges = false;
   }

   #considerUpdatingWindowHash() {
      if (this.#autoUpdateWindowHash) {
         this.updateWindowHash();
      }
   }

   #handleOnSearchParamsChanged = () => {
      if (!this.#ignoreSearchParamsChanges) {
         this.#considerUpdatingWindowHash();
         this.#onUpdated.trigger({ 
            sourceUpdated: false,
            pathUpdated: false,
            searchParamsUpdated: true,
            externalOrigin: false 
         });
      }
   };

   #handleOnHashChanged = () => {
      let triggeredEvent = false;

      if (!this.#ignoreHashChanges) {
         this.#ignoreSearchParamsChanges = true;
         let sourceUpdated = false;
         let pathUpdated = false;
         let searchParamsUpdated = false;

         let hashParts = HashRouter.parseHash(window.location.hash);

         if (hashParts !== null) {
            if (hashParts.source !== this.#source) {
               this.#source = hashParts.source;
               sourceUpdated = true;
            }

            if (hashParts.path !== this.#path) {
               this.#path = hashParts.path;
               pathUpdated = true;
            }

            for (let searchParam of hashParts.searchParams) {
               if (this.#searchParams.get(searchParam[0]) !== searchParam[1]) {
                  this.#searchParams.set(searchParam[0], searchParam[1]);
                  searchParamsUpdated = true;
               }
            }

            let currentSearchParamNames = Array.from(this.#searchParams.keys());
            for (let searchParamName of currentSearchParamNames) {
               if (!hashParts.searchParams.has(searchParamName)) {
                  this.#searchParams.delete(searchParamName);
                  searchParamsUpdated = true;
               }
            }
         } else {
            if (this.#path.length !== 0) {
               this.#source = "";
               this.#path = "";
               sourceUpdated = true;
               pathUpdated = true;
            }
            if (this.#searchParams.size > 0) {
               let currentSearchParamNames = Array.from(this.#searchParams.keys());
               for (let searchParamName of currentSearchParamNames) {
                  this.#searchParams.delete(searchParamName);
               }
               searchParamsUpdated = true;
            }
         }

         if (sourceUpdated || pathUpdated || searchParamsUpdated) {
            this.#onUpdated.trigger({ 
               sourceUpdated: sourceUpdated,
               pathUpdated: pathUpdated,
               searchParamsUpdated: searchParamsUpdated,
               externalOrigin: true 
            });
            triggeredEvent = true;
         }

         this.#ignoreSearchParamsChanges = false;
      }

      return triggeredEvent;
   };

   /**
    * @param {string|null|undefined} locationHash
    * @returns {{source:string, path:string, searchParams:URLSearchParams}?}
    */
   static parseHash(locationHash) {
      if (locationHash != null && locationHash.startsWith(HashRouter.#hashPrefix)) {
         /** @type {{source:string, path:string, searchParams:URLSearchParams}} */
         let result = {
            /** @type {string} */
            source: "",
            /** @type {string} */
            path: "",
            /** @type {URLSearchParams} */
            searchParams: new URLSearchParams()
         };

         let hash = locationHash.substring(HashRouter.#hashPrefix.length);

         let sourceAndPath;
         let searchParamsSeparatorIndex = hash.lastIndexOf(HashRouter.#searchParamsSeparator);
         if (searchParamsSeparatorIndex >= 0) {
            sourceAndPath = decodeURI(hash.substring(0, searchParamsSeparatorIndex));
            let searchParamsString = hash.substring(
               searchParamsSeparatorIndex + HashRouter.#searchParamsSeparator.length);
            if (searchParamsString.length > 0) {
               result.searchParams = new URLSearchParams(searchParamsString);
            }
         } else {
            sourceAndPath = decodeURI(hash);
         }

         let sourcePathSeparatorIndex = sourceAndPath.indexOf(HashRouter.#sourcePathSeparator);
         if (sourcePathSeparatorIndex >= 0) {
            result.source = sourceAndPath.substring(0, sourcePathSeparatorIndex);
            result.path = sourceAndPath.substring(
               sourcePathSeparatorIndex + HashRouter.#sourcePathSeparator.length);
         } else {
            result.source = "";
            result.path = sourceAndPath;
         }

         return result;
      } else {
         return null;
      }
   }
}

class HashURLSearchParams extends URLSearchParams {
   /** @type {EventController<void>} */
   #onChange = new EventController;

   get onChange() { return this.#onChange.event; }

   constructor() {
      super();
   }

   /**
    * @param {string} name 
    * @param {string} value 
    * @returns {void}
    */
   append(name, value) {
      super.append(name, value);
      this.#onChange.trigger();
   }

   /**
    * @param {string} name
    */
   delete(name) {
      if (super.has(name)) {
         super.delete(name);
         this.#onChange.trigger();
      }
   }

   /**
    * @param {string} name
    * @param {string} value
    */
   set(name, value) {
      if (this.get(name) !== value) {
         super.set(name, value);
         this.#onChange.trigger();
      }
   }

   sort() {
      super.sort();
      this.#onChange.trigger();
   }
}