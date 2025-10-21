// SPDX-License-Identifier: GPL-3.0-or-later

import { HashRouter } from "../../Dependencies/vistava.js/src/Components/Shared/HashRouter.js";
import { Assert } from "../../Dependencies/vistava.js/src/Shared/Assert.js";

export class MainApplicationHashRouter extends HashRouter {
   /** @readonly @type {string} */
   static #indexName = "index";
   /** @readonly @type {string} */
   static #detailViewName = "detail";

   get directoryPath() {
      return super.pathname;
   }

   set directoryPath(value) {
      super.pathname = value;
   }

   /** @type {number?} */
   get index() {
      return MainApplicationHashRouter.getIndex(this.searchParams);
   }

   /** @type {number?} */
   set index(value) {
      if (value === null) {
         this.searchParams.delete(MainApplicationHashRouter.#indexName);
      } else {
         Assert.number(value);
         this.searchParams.set(MainApplicationHashRouter.#indexName, value.toString());
      }
   }

   get detailViewVisible() {
      return MainApplicationHashRouter.getDetailViewVisible(this.searchParams);
   }

   set detailViewVisible(value) {
      if (value) {
         this.searchParams.set("view", MainApplicationHashRouter.#detailViewName)
      } else {
         this.searchParams.delete("view");
      }
   }

   /**
    * @param {URLSearchParams} searchParams 
    * @returns {number?}
    */
   static getIndex(searchParams) {
      let value = searchParams.get(MainApplicationHashRouter.#indexName);
      if (value !== null && value.length > 0) {
         let valueNumber = parseInt(value);
         if (!isNaN(valueNumber)) {
            return valueNumber;
         }
      }
      return null;
   }

   /**
    * @param {URLSearchParams} searchParams 
    * @returns {boolean}
    */
   static getDetailViewVisible(searchParams) {
      return searchParams.get("view") === MainApplicationHashRouter.#detailViewName;
   }
}