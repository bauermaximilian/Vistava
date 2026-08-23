// SPDX-License-Identifier: GPL-3.0-or-later

import { MainApplicationView } from "./Components/MainApplication/MainApplicationView.js";
import { BrowserUtils, cua } from "./Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { Assert } from "./Dependencies/vistava.js/src/Shared/Assert.js";
import { GlobalConfiguration } from "./Dependencies/vistava.js/src/Shared/GlobalConfiguration.js";
import { GamepadInputManagerSettings } from "./Dependencies/vistava.js/src/Components/Shared/UserInput/Gamepad/GamepadInputManagerSettings.js";
import { KeyboardInputManagerSettings } from "./Dependencies/vistava.js/src/Components/Shared/UserInput/KeyboardInputManagerSettings.js";
import { TileGridSettings } from "./Dependencies/vistava.js/src/Shared/TileGridSettings.js";


MainApplicationView.initializeDocument();

await BrowserUtils.tryLoadConfiguration("./api/options/config/gamepad.json",
   c => GlobalConfiguration.gamepadSettings = GamepadInputManagerSettings.fromConfiguration(c));
await BrowserUtils.tryLoadConfiguration("./api/options/config/keyboard.json",
   c => GlobalConfiguration.keyboardSettings = KeyboardInputManagerSettings.fromConfiguration(c));
await BrowserUtils.tryLoadConfiguration("./api/options/config/tilegrid.json",
   c => GlobalConfiguration.tileGridSettings = TileGridSettings.fromConfiguration(c));

BrowserUtils.executeWhenDocumentReady(() => cua(null, MainApplicationView, document.body, async e => {
   let urlSearchParams = new URL(location.href).searchParams;
   let showTitleBar = navigator.userAgent?.toLowerCase().includes("electron") ||
   urlSearchParams.has("forceAppBar");

   e.showTitleBar = showTitleBar;

   try {
      let serviceInfo = await (await fetch("./api/options/info")).json();
      Assert.isActive = serviceInfo.debugMode;
      e.includePathUrl = serviceInfo.includeFolderUrl ?? null;
      console.info(`Connected to service version ${serviceInfo.version}${(Assert.isActive ? " (debug mode)" : "")}.`);
   } catch (error) {
      console.error("Couldn't retrieve service information!");
   }   

   let sources = {};
   try {
      let sourcesRequest = await fetch("./api/options/sources");
      sources = await sourcesRequest.json();
   } catch (error) {
      console.warn("Couldn't load extension source definitions. " + error);
   }

   for (let sourceIdentifier of Object.keys(sources)) {
      try {
         let sourceDefinition = sources[sourceIdentifier];
         await e.sourceProvider.addSource(sourceIdentifier,
            sourceDefinition.configuration, `../../Sources/${sourceDefinition.moduleFileName}`);
      } catch (error) {
         console.warn(`Couldn't load extension source '${sourceIdentifier}'. ` + error);
      }
   }
}));
