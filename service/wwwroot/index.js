// SPDX-License-Identifier: GPL-3.0-or-later

import { MainApplicationView } from "./Components/MainApplication/MainApplicationView.js";
import { BrowserUtils, cua } from "./Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { Assert } from "./Dependencies/vistava.js/src/Shared/Assert.js";


MainApplicationView.initializeDocument();

BrowserUtils.executeWhenDocumentReady(() => cua(null, MainApplicationView, document.body, async e => {
   let urlSearchParams = new URL(location.href).searchParams;
   let showTitleBar = navigator.userAgent?.toLowerCase().includes("electron") ||
   urlSearchParams.has("forceAppBar");

   e.showTitleBar = showTitleBar;

   try {
      let serviceInfo = await (await fetch("./api/options/info")).json();
      Assert.isActive = serviceInfo.debugMode;
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
}, async e => {
   BrowserUtils.tryLoadConfiguration("Configurations/gamepad.json", c => e.importGamepadConfiguration(c));
   BrowserUtils.tryLoadConfiguration("Configurations/keyboard.json", c => e.importKeyboardConfiguration(c));
}));
