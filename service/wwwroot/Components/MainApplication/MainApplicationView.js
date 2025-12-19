// SPDX-License-Identifier: GPL-3.0-or-later

import { ViewBase } from "../../Dependencies/vistava.js/src/Components/Shared/ViewBase.js";
import { MainApplicationHashRouter } from "./MainApplicationHashRouter.js";
import { SourceFileSystem } from "./Sources/SourceFileSystem.js";
import { VistavaPresenter } from "../../Dependencies/vistava.js/src/Components/Vistava/VistavaPresenter.js";
import { VistavaView } from "../../Dependencies/vistava.js/src/Components/Vistava/VistavaView.js";
import { Source } from "../../Dependencies/vistava.js/src/Shared/Source.js";
import { ImplementationError } from "../../Dependencies/vistava.js/src/Errors/ImplementationError.js";
import { TileGridLayoutType } from "../../Dependencies/vistava.js/src/Components/TileGrid/Shared/TileGridLayoutType.js";
import { VistavaLayoutTypes } from "../../Dependencies/vistava.js/src/Components/Vistava/VistavaLayoutTypes.js";
import { BrowserUtils, BU, cu } from "../../Dependencies/vistava.js/src/Utils/BrowserUtils.js";
import { VU } from "../../Dependencies/vistava.js/src/Utils/VectorUtils.js";
import { GuiIconView } from "../../Dependencies/vistava.js/src/Components/GuiIcon/GuiIconView.js";
import { GuiIconPresenter } from "../../Dependencies/vistava.js/src/Components/GuiIcon/GuiIconPresenter.js";
import { AU } from "../../Dependencies/vistava.js/src/Utils/ArrayUtils.js";
import QrCreator from "../../Dependencies/qr-creator/qr-creator.js";

const tagName = "main-application";
export class MainApplicationView extends ViewBase {
   static get tagName() { return tagName; }
  
   get showTitleBar() { return this.#showTitleBar; }
   set showTitleBar(value) { this.#showTitleBar = value; }

   /**
    * @template T
    * @typedef {import("../../Dependencies/vistava.js/src/Shared/Event.js").EventHandler<T>} EventHandler<T>
    */

   /**
    * @typedef {import("../../Dependencies/vistava.js/src/Components/Vistava/VistavaPresenter.js").
    * VistavaPresenterExtendStateUpdatedEventArgs} VistavaPresenterExtendStateUpdatedEventArgs
    */
   
   /**
    * @typedef {import("../../Dependencies/vistava.js/src/Components/Vistava/VistavaView.js").
    * VistavaQueryChangeRequestedEventArgs } VistavaQueryChangeRequestedEventArgs
    */
   
   /**
    * @typedef {import("../../Dependencies/vistava.js/src/Components/Vistava/VistavaView.js").
    * TileActionEventArgs } TileActionEventArgs
    */
   
   /** 
    * @typedef {import("../../Dependencies/vistava.js/src/Components/Shared/HashRouter.js").
    * HashRouterUpdatedEventArgs} HashRouterUpdatedEventArgs
    */

   /** @readonly */
   #iconOpacityDisabled = "0.025";
   /** @readonly */
   #iconOpacityDefault = "0.3";
   /** @readonly */
   #iconOpacityHover = "0.8";
   /** @readonly */
   #iconOpacityActivated = "0.6";

   /** @readonly @type {Source} */
   #source;
   /** @readonly @type {MainApplicationHashRouter} */
   #hashRouter = new MainApplicationHashRouter();

   /** @type {boolean} */
   #showTitleBar = false;
   /** @type {boolean} */
   #sharingEnabled = false;


   get #shareLinkFull() {
      return (this.#shareLinkBase === null || this.#shareLinkHash === null) ?
         null : (this.#shareLinkBase + this.#shareLinkHash);
   }

   /** @type {string} */
   #qrCodeContent = "";
   /** @type {string?} */
   #shareLinkBase = null;
   /** @type {string?} */
   #shareLinkHash = null;
   /** @type {boolean} */
   #isLoading = false;
   
   /** @type {any?} */
   #loadingAnimationIntervalHandle = null;

   /** @type {VistavaPresenter?} */
   #vistavaPresenter = null;

   /** @type {VistavaView?} */
   #vistavaView = null;
   /** @type {HTMLDivElement?} */
   #titleBar = null;
   /** @type {GuiIconView?} */
   #backButton = null;
   /** @type {GuiIconView?} */
   #forwardButton = null;
   /** @type {GuiIconView?} */
   #refreshButton = null;
   /** @type {GuiIconView?} */
   #shareToggleButton = null;
   /** @type {GuiIconView?} */
   #shareLinkButton = null;
   /** @type {HTMLDivElement?} */
   #shareLinkPopupAnchor = null;
   /** @type {HTMLDivElement?} */
   #shareLinkPopup = null;
   /** @type {HTMLDivElement?} */
   #shareLinkPopupHint = null;
   /** @type {HTMLLabelElement?} */
   #shareLinkPopupHintText = null;
   /** @type {GuiIconView?} */
   #shareLinkPopupHintIcon = null;
   /** @type {HTMLLabelElement?} */
   #shareLinkPopupLinkText = null;
   /** @type {any?} */
   #shareLinkButtonAnimationHandle = null;
   /** @type {boolean} */
   #shareClickLocked = false;
   /** @type {GuiIconView?} */
   #logoDot = null;
   
   constructor() {
      super(true);
      
      this.#source = new SourceFileSystem({
         identifier: "local",
         hostUrl: this.#getApiUrl()
      });
      this.#hashRouter.onUpdated.subscribe(this.#handleOnHashUpdated);
      this.#hashRouter.autoUpdateWindowHash = false;
      this.#hashRouter.disableHistoryChanges = true;
      if (this.#hashRouter.index === null) {
         this.#hashRouter.index = 0;
      }
   }

   connectedCallback() {
      this.#loadingAnimationIntervalHandle = setInterval(this.#handleOnUpdateLoadingIndicator, 500);

      BrowserUtils.subscribeToFullscreenChange(this.#handleOnFullscreenChange);

      this.#render();

      if (this.#vistavaPresenter !== null) {
         this.#vistavaPresenter.onFocusUpdated.subscribe(this.#handleOnFocusUpdated);
         this.#vistavaPresenter.onExtendStateUpdated.subscribe(this.#handleOnExtendStateUpdated);
      } else {
         throw new ImplementationError();
      }

      if (this.#vistavaView !== null) {
         this.#vistavaView.onTilePrimaryAction.subscribe(this.#handleOnTilePrimaryAction);
         this.#vistavaView.onQueryChangeRequested.subscribe(this.#handleOnQueryChangeRequested);
         this.#vistavaView.onBack.subscribe(this.#handleOnBack);
      } else {
         throw new ImplementationError();
      }

      this.#hashRouter.attach();
   }

   disconnectedCallback() {
      BrowserUtils.unsubscribeFromFullscreenChange(this.#handleOnFullscreenChange);

      clearInterval(this.#loadingAnimationIntervalHandle);

      this.#vistavaPresenter?.onFocusUpdated.unsubscribe(this.#handleOnFocusUpdated);
      this.#vistavaPresenter?.onExtendStateUpdated.unsubscribe(this.#handleOnExtendStateUpdated);

      this.#hashRouter.detach();
   }

   /**
    * @param {string?} query 
    */
   updateQuery(query) {
      this.#hashRouter.pathname = query ?? "";
      this.#hashRouter.index = 0;
      this.#vistavaPresenter?.updateState({ query: query ?? "", index: 0 });
      this.#hashRouter.updateWindowHash(false);
   }

   static initializeDocument() {
      document.documentElement.style.margin = document.body.style.margin = "0";
      document.documentElement.style.width = document.body.style.width = "100%";
      document.documentElement.style.height = document.body.style.height = "100%";
      document.body.style.backgroundColor = "#1e1e1e";
      document.body.style.transition = "background 0.5s ease-in-out";
   
      BrowserUtils.subscribeToFullscreenChange(() => {
         if (BrowserUtils.isFullscreen) {
            document.body.style.backgroundColor = "#010101";
         } else {
            document.body.style.backgroundColor = "#1e1e1e";
         }
      });
   }

   #getApiUrl() {
      let urlParam = new URLSearchParams(location.search).get("apiPort");
      let urlParamPort = (urlParam != null && urlParam.trim().length > 0) ? parseInt(urlParam) : null;
      let urlParamAddress = urlParamPort != null ? `http://127.0.0.1:${urlParamPort}` : null;
      let sessionState = BU.getSessionState(MainApplicationView);
      let apiUrl = sessionState?.apiUrl ?? urlParamAddress ?? location.href;
      BU.setSessionState(MainApplicationView, { apiUrl });
      return apiUrl;
   }

   #handleOnFullscreenChange = () => {
      if (BrowserUtils.isFullscreen) {
         if (this.#titleBar !== null) {
            this.#titleBar.style.display = "none";
         }
      } else {
         if (this.#titleBar !== null) {
            this.#titleBar.style.display = "flex";
         }
      }
   };

   #render() {
      this.style.display = "flex";
      this.style.flexDirection = "column";
      this.style.height = "100%";

      if (this.#showTitleBar) {
         this.#titleBar = cu(this.#titleBar, HTMLDivElement, this.root, (e, s) => {
            s.textAlign = "center";
            s.display = "flex";
            s.flexDirection = "row";
            s.alignItems = "center";
            s.paddingRight = s.paddingLeft = "8px";            
            s.backgroundColor = "#141414";
            s.color = "#b7b7b7ff";
            s.height = "36px";
            
            cu(null, HTMLDivElement, e, (e, s) => {
               s.flexGrow = "1";
               s.height = "100%";
               s.setProperty("app-region", "drag");
            });

            cu(null, GuiIconView, e, (e, s) => {
               s.display = "block";
               s.position = "absolute";
               s.left = "calc(50% - 15px)";
               s.height = "30px";
               s.width = "30px";
               s.color = "#ddddddff";
               s.paddingTop = s.paddingBottom = "3px";
               e.presenter = new GuiIconPresenter({ icon: "logo-no-dot" });
            });

            this.#logoDot = cu(this.#logoDot, GuiIconView, e, (e, s) => {
               s.display = "block";
               s.position = "absolute";
               s.left = "calc(50% - 15px)";
               s.height = "30px";
               s.width = "30px";
               s.color = "#ddddddff";
               s.paddingTop = s.paddingBottom = "3px";
               s.transition = "opacity 0.5s ease-in-out";
               e.presenter = new GuiIconPresenter({ icon: "logo-only-dot" });
            });
         });

         this.#refreshButton = cu(this.#refreshButton, GuiIconView, this.#titleBar,
            e => this.#initializeToolbarIcon(e, "refresh", () => location.reload(), "Refresh"), 
            e => this.#updateToolbarIcon(e), null, true);
         this.#forwardButton = cu(this.#forwardButton, GuiIconView, this.#titleBar,
               e => this.#initializeToolbarIcon(e, "forward", () => history.forward(), "Forward"), 
               e => this.#updateToolbarIcon(e), null, true);
         this.#backButton = cu(this.#backButton, GuiIconView, this.#titleBar,
            e => this.#initializeToolbarIcon(e, "back", () => history.back(), "Back"), 
            e => this.#updateToolbarIcon(e), null, true);

         this.#shareLinkButton = cu(this.#shareLinkButton, GuiIconView, this.#titleBar, (e, s) => {
            this.#initializeToolbarIcon(e, "link", this.#handleOnShareLinkClick);
            e.addEventListener("mouseenter", () => {
               if (this.#shareLinkPopup !== null && this.#sharingEnabled) {
                  this.#shareLinkPopup.style.opacity = "1";
                  this.#shareLinkPopup.style.visibility = "visible";
               }
            });
            e.addEventListener("mouseleave", () => {
               if (this.#shareLinkPopup !== null) {
                  this.#shareLinkPopup.style.opacity = "0";
                  this.#shareLinkPopup.style.visibility = "collapsed";
               }
            });
         }, (e, s) => {
            if (e.presenter === null) { return; }
            this.#getSetElementAttribute(e, "data-disabled", !this.#sharingEnabled);
            this.#updateToolbarIcon(e);
         });

         this.#shareLinkPopupAnchor = cu(this.#shareLinkPopupAnchor, HTMLDivElement, this.#titleBar, (e, s) => {
            s.position = "relative";
            s.width = s.height = "0";
            s.alignSelf = "end";
         });
         
         this.#shareToggleButton = cu(this.#shareToggleButton, GuiIconView, this.#titleBar, (e, s) => {
            this.#initializeToolbarIcon(e, "share-network-disabled", this.#handleOnShareClick,
               "Toggle local network sharing");
            s.marginRight = "150px";
         }, (e, s) => {
            if (e.presenter === null) { return; }
            this.#updateToolbarIcon(e);
            e.presenter.model.icon = this.#getSetElementAttribute(e, "data-activated") ?
               "share-network" : "share-network-disabled";
         });

         this.#shareLinkPopup = cu(this.#shareLinkPopup, HTMLDivElement, this.#shareLinkPopupAnchor, (e, s) => {
            s.display = "flex";
            s.flexDirection = "column";
            s.right = "0px";
            s.top = "5px";
            s.position = "absolute";
            s.backgroundColor = "white";
            s.zIndex = "2";
            s.visibility = "collapsed";
            s.transition = "opacity 0.2s ease-out";
            s.borderRadius = "3px";
            s.opacity = "0";
            s.padding = "5px";
         }, (e, s) => {
            if (this.#shareLinkFull !== null && this.#qrCodeContent !== this.#shareLinkFull) {
               for (let child of e.children) {
                  if (child instanceof HTMLCanvasElement) {
                     child.remove();
                  }
               }
               QrCreator.render({
                  text: this.#shareLinkFull,
                  radius: 0,
                  ecLevel: "H",
                  size: "250"
               }, e);
               if (e.firstChild instanceof HTMLCanvasElement) {
                  e.firstChild.style.width = "100%";
                  e.firstChild.style.height = "";
               }
            }
         });

         this.#shareLinkPopupLinkText = cu(this.#shareLinkPopupLinkText, HTMLLabelElement, this.#shareLinkPopup, (e, s) => {
            s.fontFamily = `-apple-system, "Segoe UI", Roboto, sans-serif`;
            s.fontSize = "14px";
            s.padding = "9px 3px 0px 3px";
            s.color = "black";
            s.fontWeight = "600";
            s.order = "1";
         }, (e, s) => {
            e.innerText = this.#shareLinkBase ?? "";
         });
         this.#shareLinkPopupHint = cu(this.#shareLinkPopupHint, HTMLDivElement, this.#shareLinkPopup, (e, s) => {
            s.order = "2";
            this.#shareLinkPopupHintText = cu(this.#shareLinkPopupHintText, HTMLLabelElement, e, (e, s) => {
               s.fontFamily = `-apple-system, "Segoe UI", Roboto, sans-serif`;
               s.fontSize = "10px";
               s.color = "black";
               s.opacity = "50%";
               s.padding = "3px 3px 4px";
               s.fontStyle = "italic";
               e.innerText = "click to copy link to clipboard";
            });
            this.#shareLinkPopupHintIcon = cu(this.#shareLinkPopupHintIcon, GuiIconView, e, (e, s) => {
               s.width = s.height = "10px";
               s.display = "inline-block";
               s.transition = "opacity 0.2s ease-in-out";
               s.opacity = "0";
               e.presenter = new GuiIconPresenter({ icon: "check", isInteractive: false });
            });
         });
      }

      this.#vistavaView = cu(this.#vistavaView, VistavaView, this.root, (e, s) => {
         s.flexGrow = "1";
         s.overflow = "hidden";
         let layoutTypes = VistavaLayoutTypes.default;
         this.#vistavaPresenter = e.presenter ??= new VistavaPresenter(
            query => this.#source.createCollectionRetriever(query),
            layoutTypes, VU.new(e.clientWidth, e.clientHeight), { query: "" });
         e.layoutTypes = layoutTypes;
      });
   }

   /**
    * @param {HTMLElement} element 
    * @param {("data-toggle" | "data-activated" | "data-disabled" )} attribute 
    * @param {boolean} [shouldHaveAttribute]
    * @returns {boolean}
    */
   #getSetElementAttribute(element, attribute, shouldHaveAttribute) {
      if (typeof (shouldHaveAttribute) === "boolean") {
         BU.setHasAttribute(element, attribute, shouldHaveAttribute);
      }
      let hasAttribute = BU.hasAttribute(element, attribute);
      return hasAttribute;
   }

   /**
    * @param {GuiIconView} icon 
    * @param {string} iconName 
    * @param {()=>void} onClick
    * @param {string} [tooltip]
    */
   #initializeToolbarIcon(icon, iconName, onClick, tooltip) {
      icon.style.width = icon.style.height = "22px";
      icon.style.marginRight = "4px";
      icon.style.transition = "opacity 0.2s ease-out";
      icon.style.opacity = this.#iconOpacityDefault;
      icon.presenter = new GuiIconPresenter({ icon: iconName, isInteractive: true });
            
      icon.addEventListener("mouseenter", () => {
         this.#getSetElementAttribute(icon, "data-toggle", true);
         this.#render();
      });
      icon.addEventListener("mouseleave", () => {
         this.#getSetElementAttribute(icon, "data-toggle", false);
         this.#render();
      });
      icon.addEventListener("click", () => {
         if (!this.#getSetElementAttribute(icon, "data-disabled")) {
            onClick();
         }
      });

      if (tooltip != null) {
         icon.title = tooltip;
      }
   }

   /**
    * @param {GuiIconView} icon 
    */
   #updateToolbarIcon(icon) {
      if (icon.presenter === null) {
         return;
      }

      if (this.#getSetElementAttribute(icon, "data-disabled")) {
         icon.style.opacity = this.#iconOpacityDisabled;
         icon.presenter.model.isInteractive = false;
      } else {
         icon.presenter.model.isInteractive = true;
         if (this.#getSetElementAttribute(icon, "data-toggle")) {
            icon.style.opacity = this.#iconOpacityHover;
         } else if (this.#getSetElementAttribute(icon, "data-activated")) {
            icon.style.opacity = this.#iconOpacityActivated;
         } else {
            icon.style.opacity = this.#iconOpacityDefault;
         }
      }
   }

   #handleOnShareClick = async () => {
      if (!this.#shareClickLocked) {
         this.#shareClickLocked = true;

         try {
            let response = await fetch(`api/options/listenAnyIp/${!this.#sharingEnabled}`, { method: "POST" });
            /** @type {{isPublic:boolean, url:string}[]} */
            let endpoints = await response.json();
            let endpoint = AU.findFirstOrNull(endpoints, endpoint => endpoint.isPublic) ??
               AU.firstOrNull(endpoints);
            let url = endpoint?.url;

            if (this.#shareToggleButton != null && url != null) {
               this.#sharingEnabled = !this.#sharingEnabled;
               this.#getSetElementAttribute(this.#shareToggleButton, "data-activated", this.#sharingEnabled);
               this.#shareLinkBase = url;
            } else {
               this.#sharingEnabled = false;
            }

         } finally {
            this.#shareClickLocked = false;
            this.#render();
         }
      }
   };

   #handleOnShareLinkClick = async () => {
      const type = "text/plain";
      const clipboardItemData = {
         [type]: this.#shareLinkFull ?? "",
      };
      const clipboardItem = new ClipboardItem(clipboardItemData);
      await navigator.clipboard.write([clipboardItem]);
      
      this.#shareLinkButton?.presenter?.model.apply({ icon: "link-ok" });
      this.#shareLinkPopupHintIcon?.style.setProperty("opacity", "1");

      if (this.#shareLinkButtonAnimationHandle != null) {
         clearTimeout(this.#shareLinkButtonAnimationHandle);
      }
      this.#shareLinkButtonAnimationHandle =
         setTimeout(() => {
            this.#shareLinkButton?.presenter?.model.apply({ icon: "link" });
            this.#shareLinkPopupHintIcon?.style.setProperty("opacity", "0");
            this.#shareLinkButtonAnimationHandle = null;
         }, 2000);
   };

   /** @type {EventHandler<TileActionEventArgs>} */
   #handleOnTilePrimaryAction = (args) => {
      let model = args.tile.presenter?.model;
      if (model == null) { return; }

      let targetQuery = model?.getDataAsString("queryTarget", true) ?? null;
      if (targetQuery !== null) {
         this.#hashRouter.index = model.index;
         this.#hashRouter.updateWindowHash(true);
         this.updateQuery(targetQuery);
      } else {
         this.#hashRouter.index = model.index;
         this.#hashRouter.updateWindowHash(true);

         this.#vistavaPresenter?.updateState({ view: TileGridLayoutType.gallery.identifier });

         this.#hashRouter.detailViewVisible = true;
         this.#hashRouter.updateWindowHash(false);
      }
   };

   /** @type {EventHandler<VistavaQueryChangeRequestedEventArgs>} */
   #handleOnQueryChangeRequested = (args) => {
      if (args.query !== this.#vistavaPresenter?.state.query) {
         this.#hashRouter.index = 0;
         this.#hashRouter.updateWindowHash(true);
         this.updateQuery(args.query);
      }
   };

   #handleOnBack = () => {
      history.back();
   };

   /**
    * @param {HashRouterUpdatedEventArgs} args 
    */
   #handleOnHashUpdated = (args) => {
      if (!args.externalOrigin || this.#vistavaPresenter === null) {
         return;
      }
      
      let focussedTileIndex = this.#vistavaPresenter.focussedTileIndex;
      if (focussedTileIndex !== null && focussedTileIndex !== this.#hashRouter.index &&
         this.#vistavaPresenter.state.view === TileGridLayoutType.gallery.identifier &&
         !this.#hashRouter.detailViewVisible) {
         this.#hashRouter.index = focussedTileIndex;
         this.#hashRouter.updateWindowHash(true);
      }

      this.#shareLinkHash = this.#hashRouter.hashPathOnly;

      this.#vistavaPresenter.updateState({
         index: this.#hashRouter.index ?? 0,
         view: this.#hashRouter.detailViewVisible ?
            TileGridLayoutType.gallery.identifier : TileGridLayoutType.thumbnails.identifier,
         query: this.#hashRouter.pathname
      });
   };

   #handleOnFocusUpdated = () => {
      let focussedTileIndex = this.#vistavaPresenter?.focussedTileIndex ?? null;
      if (focussedTileIndex != null && focussedTileIndex !== this.#hashRouter.index) {
         this.#hashRouter.index = focussedTileIndex;
         this.#hashRouter.updateWindowHash(true);
      }
   };

   /**
    * @param {VistavaPresenterExtendStateUpdatedEventArgs} args 
    */
   #handleOnExtendStateUpdated = (args) => {     
      this.#isLoading = args.isExtending;
   };

   #handleOnUpdateLoadingIndicator = () => {
      if (this.#logoDot != null && this.#vistavaView != null) {
         if (this.#logoDot.style.opacity !== "0.5" && this.#isLoading) {
            this.#logoDot.style.opacity = "0.5";
         } else {
            this.#logoDot.style.opacity = "1";
         }
      }
   };
}
