// SPDX-License-Identifier: GPL-3.0-or-later

import { Source } from "../../Dependencies/vistava.js/src/Shared/Source.js";
import { Assert } from "../../Dependencies/vistava.js/src/Shared/Assert.js";
import { EventController } from "../../Dependencies/vistava.js/src/Shared/Event.js";
import { InvalidOperationError } from "../../Dependencies/vistava.js/src/Errors/InvalidOperationError.js";

export class SourceProvider {
   /** @type {number} */
   get count() { return this.#sources.size; }
   /** @type {MapIterator<string>} */
   get sourceIdentifiers() { return this.#sources.keys(); }
   /** @type {string} */
   get defaultSourceIdentifier() { return this.#defaultSourceIdentifier; }
   /** @type {Source} */
   get currentSource() { return this.#currentSource; }
   /** @type {string} */
   get currentSourceIdentifier() { return this.#currentSourceIdentifier; }

   get onSourceChanged() { return this.#onSourceChanged.event; }

   /** @type {EventController<void>} */
   #onSourceChanged = new EventController();
   /** @type {string} */
   #defaultSourceIdentifier;   

   /**
    * @template {object} TValue
    * @typedef {import("../../Dependencies/vistava.js/src/Shared/CachedCollection.js").CollectionRetrieverConstructor<TValue>
    * } CollectionRetrieverConstructor<TValue>
    */
   
   /** @type {Map<string,Source>} */
   #sources = new Map();

   /** @type {Source} */
   #currentSource;
   /** @type {string} */
   #currentSourceIdentifier;

   /**
    * @param {string} defaultSourceIdentifier 
    * @param {Source} defaultSource 
    */
   constructor(defaultSourceIdentifier, defaultSource) {
      Assert.stringNotEmptyOrWhitespacesOnly(defaultSourceIdentifier, "defaultSourceIdentifier");
      Assert.class(defaultSource, Source, "defaultSource");

      this.#defaultSourceIdentifier = defaultSourceIdentifier;      
      this.#currentSource = defaultSource;
      this.#currentSourceIdentifier = defaultSourceIdentifier;

      this.#sources.set(defaultSourceIdentifier, defaultSource);
   }

   /**
    * @param {string} sourceIdentifier 
    * @throws {InvalidOperationError} Is thrown when no source with the specified {@link sourceIdentifier} exists.
    */
   changeSource(sourceIdentifier) {
      Assert.stringNotEmptyOrWhitespacesOnly(sourceIdentifier, "sourceIdentifier");
      if (sourceIdentifier !== this.#currentSourceIdentifier) {
         let newSource = this.#sources.get(sourceIdentifier);
         if (newSource != null) {
            this.#currentSource = newSource;
            this.#currentSourceIdentifier = sourceIdentifier;
            this.#onSourceChanged.trigger();
         } else {
            throw new InvalidOperationError("The specified source wasn't found.");
         }
      }
   }

   /**
    * @param {string} sourceIdentifier 
    * @returns {boolean}
    */
   hasSource(sourceIdentifier) {
      return this.#sources.has(sourceIdentifier);
   }

   /**
    * @param {string} sourceIdentifier 
    * @returns {string?}
    */
   getSourceName(sourceIdentifier) {
      return this.#sources.get(sourceIdentifier)?.name ?? null;
   }

   /**
    * @param {string} sourceIdentifier 
    * @param {import("../../Dependencies/vistava.js/src/Shared/Source.js").SourceConfiguration} sourceConfiguration 
    * @param {string} sourceModuleUrl 
    */
   async addSource(sourceIdentifier, sourceConfiguration, sourceModuleUrl) {
      Assert.stringNotEmptyOrWhitespacesOnly(sourceIdentifier, "sourceIdentifier");
      if (this.#sources.has(sourceIdentifier)) {
         throw new InvalidOperationError(`Another source with the identifier '${sourceIdentifier}' already exists.`);
      }

      let sourceModule;
      try {
         sourceModule = await import(sourceModuleUrl);         
      } catch (error) {
         throw new InvalidOperationError("The source module file couldn't be loaded. " + error);
      }

      try {
         let sourceInstance = new sourceModule.default(sourceConfiguration);
         if (sourceInstance != null && typeof (sourceInstance) === "object" &&
            "createContentRetriever" in sourceInstance) {
            this.#sources.set(sourceIdentifier, sourceInstance);
         } else {
            throw new Error("The imported source module wasn't a valid 'Source' class instance.");
         }
      } catch (error) {
         throw new InvalidOperationError("The source module couldn't be initialized. " + error);
      }
   }

   /** 
    * @type {CollectionRetrieverConstructor<object>} 
    */
   createCollectionRetriever(query) {
      return this.#currentSource.createCollectionRetriever(query);
   }
}