// SPDX-License-Identifier: GPL-3.0-or-later

import { MainApplicationView } from "./Components/MainApplication/MainApplicationView.js";
import { BrowserUtils, cua } from "./Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { Assert } from "./Dependencies/vistava.js/src/Shared/Assert.js";

Assert.isActive = false;
MainApplicationView.initializeDocument();

BrowserUtils.executeWhenDocumentReady(() => cua(null, MainApplicationView, document.body, async e => {
   let urlSearchParams = new URL(location.href).searchParams;
   let showTitleBar = navigator.userAgent?.toLowerCase().includes("electron") ||
   urlSearchParams.has("forceAppBar");

   e.showTitleBar = showTitleBar;

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
