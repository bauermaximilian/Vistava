// SPDX-License-Identifier: GPL-3.0-or-later

import { Source } from "../../../Dependencies/vistava.js/src/Shared/Source.js";
import { Assert } from "../../../Dependencies/vistava.js/src/Shared/Assert.js";
import { ArgumentError } from "../../../Dependencies/vistava.js/src/Errors/ArgumentError.js";

export class SourceProvider {
   /** @type {Iterator<string>} */
   get sourceIdentifiers() { return this.#sources.keys(); }

   /** @typedef {import("../../../Dependencies/vistava.js/src/Shared/Source.js").SourceConfiguration} SourceConfiguration */

   /** @type {Map<string,Source>} */
   #sources = new Map();

   /**
    * @param {Map<typeof Source, SourceConfiguration>} providerConfiguration 
    */
   static create(providerConfiguration) {
      let provider = new SourceProvider();

      for (let source of providerConfiguration) {
         let sourceType = source[0];
         let sourceConfiguration = source[1];

         provider.addSource(sourceType, sourceConfiguration);
      }

      return provider;
   }

   /**
    * @template {SourceConfiguration} TConfig
    * @param {typeof Source} sourceType
    * @param {TConfig} sourceConfiguration 
    * @returns {Source}
    */
   addSource(sourceType, sourceConfiguration) {
      Assert.stringNotEmptyOrWhitespacesOnly(sourceConfiguration.identifier, "sourceConfiguration.identifier");

      if (this.#sources.has(sourceConfiguration.identifier)) {
         throw new ArgumentError("Another source with the same identifier already exisits.");
      }

      try {
         let source = new sourceType(sourceConfiguration);
         this.#sources.set(source.identifier, source);
         return source;
      } catch (error) {
         throw new ArgumentError("The specified source couldn't be created.");
      }
   }

   /**
    * @param {string} sourceIdentifier 
    * @returns {Source?}
    */
   getSource(sourceIdentifier) {
      return this.#sources.get(sourceIdentifier) ?? null;
   }
}