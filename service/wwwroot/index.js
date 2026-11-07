// SPDX-License-Identifier: GPL-3.0-or-later

import { MainApplicationView } from "./Components/MainApplication/MainApplicationView.js";
import { BrowserUtils, cua } from "./Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { Assert } from "./Dependencies/vistava.js/src/Shared/Assert.js";
import { GlobalConfiguration } from "./Dependencies/vistava.js/src/Shared/GlobalConfiguration.js";
import { GamepadInputManagerSettings } from "./Dependencies/vistava.js/src/Components/Shared/UserInput/Gamepad/GamepadInputManagerSettings.js";
import { KeyboardInputManagerSettings } from "./Dependencies/vistava.js/src/Components/Shared/UserInput/KeyboardInputManagerSettings.js";
import { TileGridSettings } from "./Dependencies/vistava.js/src/Shared/TileGridSettings.js";
import { Dialog } from "./Dependencies/vistava.js/src/Components/Dialog/Dialog.js";
import { NotFoundError } from "./Dependencies/vistava.js/src/Errors/NotFoundError.js";
import { InvalidOperationError } from "./Dependencies/vistava.js/src/Errors/InvalidOperationError.js";
import { ExtensionError } from "./Dependencies/vistava.js/src/Errors/ExtensionError.js";

MainApplicationView.initializeDocument();

/** @type {Error[]} */
let errors = [];

const protocolError = (/** @type {Error?} */ error) => {
   if (error != null && error instanceof Error && !(error instanceof NotFoundError)) {
      errors.push(error);
   }
}

protocolError(await BrowserUtils.tryLoadConfiguration("./api/options/config/gamepad.json",
   c => GlobalConfiguration.gamepadSettings = GamepadInputManagerSettings.fromConfiguration(c)));
protocolError(await BrowserUtils.tryLoadConfiguration("./api/options/config/keyboard.json",
   c => GlobalConfiguration.keyboardSettings = KeyboardInputManagerSettings.fromConfiguration(c)));
protocolError(await BrowserUtils.tryLoadConfiguration("./api/options/config/tilegrid.json",
   c => GlobalConfiguration.tileGridSettings = TileGridSettings.fromConfiguration(c)));

BrowserUtils.executeWhenDocumentReady(() => cua(null, MainApplicationView, document.body, async e => {
   let urlSearchParams = new URL(location.href).searchParams;
   let showTitleBar = navigator.userAgent?.toLowerCase().includes("electron") ||
   urlSearchParams.has("forceAppBar");

   e.showTitleBar = showTitleBar;

   try {
      let serviceInfo = await (await fetch("./api/options/info")).json();
      Assert.isActive = serviceInfo.debugMode;
      e.appDataFolderUri = serviceInfo.appDataFolderUri ?? null;
      console.info(`Connected to service version ${serviceInfo.version}${(Assert.isActive ? " (debug mode)" : "")}.`);
   } catch (error) {
      protocolError(new InvalidOperationError(`Couldn't retrieve service information: ${error}`));
   }   

   let sources = {};
   try {
      let sourcesRequest = await fetch("./api/options/sources");
      sources = await sourcesRequest.json();
   } catch (error) {
      protocolError(new ExtensionError(`Couldn't load extension source definitions: ${error}`));
   }

   for (let sourceIdentifier of Object.keys(sources).filter(key => !key.startsWith("$"))) {
      try {
         let sourceDefinition = sources[sourceIdentifier];
         await e.sourceProvider.addSource(sourceIdentifier,
            sourceDefinition.configuration, `../../Sources/${sourceDefinition.moduleFileName}`);
      } catch (error) {
         protocolError(
            new ExtensionError(`Couldn't load extension source "${sourceIdentifier}": ${error}`));
      }
   }

   for (let sourceError of sources["$errors"]) {
      protocolError(new ExtensionError(sourceError));
   }
}, async (e, s) => {
   if (errors.length > 0) {
      let errorMessages = errors.map(error => `${error.constructor.name}: ${error.message}`).join("\n");
      await Dialog.showErrorAsync("One or more errors were encountered while initializing Vistava or " +
         "its extensions.\n" + errorMessages, "Initialization failed", ["Ok"]);
   }   
}));
