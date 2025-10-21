// SPDX-License-Identifier: GPL-3.0-or-later

import { MainApplicationView } from "./Components/MainApplication/MainApplicationView.js";
import { BrowserUtils, cu } from "./Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { Assert } from "./Dependencies/vistava.js/src/Shared/Assert.js";

Assert.isActive = false;

MainApplicationView.initializeDocument();
BrowserUtils.executeWhenDocumentReady(() => cu(null, MainApplicationView, document.body, (e, s) => {
   e.showTitleBar = (new URL(import.meta.url).searchParams.get("showTitleBar")?.toLowerCase() ?? "false") === "true";
}));
