/* 
 * Vistava - A media file server with a responsive web browser interface.
 * Copyright (C) 2020 Maximilian Bauer
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

"use strict";
            
//Set this to true to not send any API requests and just work with 
//the default data in the HTML document to view the UI.
const mockupMode = false;

const ApiPathRoot = "/api/";  

const Page = {
    "Login": 0,
    "Directory" : 1,
    "File" : 2
};

const InternalError = {
    "Unknown" : 900,
    "InvalidJson" : 901, 
    "ConnectionFailure" : 902
};

const ElementId = {
    "UsernameInput" : "username",
    "PasswordInput" : "password",
    "LoginDialog" : "login",
    "DirectoryPage" : "browser",
    "Header" : "header",
    "MediaDisplay" : "viewer",
    "DirectoryList": "elements",
    "LoginStatus" : "loginStatus",
    "CurrentDirectoryPath" : "currentDirectoryPath"
};            

const CssClass = {
    "Hidden" : "hidden",
    "Shown" : "shown",
    "DirectoryEntry" : "elements",
    "FileEntry" : "file",
    "MediaViewer" : "viewer"
};

let setElementVisibility = function(elementId, setVisible) {
    let element = document.getElementById(elementId);

    if (element) {
        element.className = element.className.replace("hidden", "");
        element.className = element.className.replace("shown", "");
        element.className = element.className.trim();

        if (setVisible) element.className += " " + CssClass.Shown; 
        else element.className += " " + CssClass.Hidden;
    }
    else throw "Element to hide/show not found.";
};

let getCurrentCredentials = function() {
    let usernameInput = document.getElementById(ElementId.UsernameInput);
    let passwordInput = document.getElementById(ElementId.PasswordInput);

    if (!usernameInput || !passwordInput) throw "Input elements not found.";

    if (usernameInput.value.length > 0 && passwordInput.value.length > 0)
        return {
            "username" : usernameInput.value,
            "password" : passwordInput.value
        };
    else return null;
};

let clearCurrentCredentials = function(clearPasswordOnly = false) {
    let usernameInput = document.getElementById(ElementId.UsernameInput);
    let passwordInput = document.getElementById(ElementId.PasswordInput);

    if (!usernameInput || !passwordInput) throw "Input elements not found.";

    if (!clearPasswordOnly) usernameInput.value = "";
    passwordInput.value = "";
};

let clearDirectoryView = function() {
    let directoryElementsListElement = 
            document.getElementById(ElementId.DirectoryList);

    if (!directoryElementsListElement)
        throw "Directory list for clearing not found.";

    while (directoryElementsListElement.firstChild)
        directoryElementsListElement.removeChild(
            directoryElementsListElement.firstChild);
};

let addElementsToDirectoryView = function(data, clearBefore) {
    if (clearBefore) clearDirectoryView();

    let directoryElementsListElement = 
        document.getElementById(ElementId.DirectoryList);

    if (!directoryElementsListElement)
        throw "Directory list for adding elements not found.";

    let addListElements = function(dataElements, cssClass) {
        for (let i = 0; i < dataElements.length; i++) {
            let item = dataElements[i];

            let listElement = document.createElement("li");

            let elementLink = document.createElement("a");
            elementLink.href = "#" + item.substring(ApiPathRoot.length);
            elementLink.textContent = item.substring(item.lastIndexOf('/') + 1);
            elementLink.className = cssClass;

            listElement.appendChild(elementLink);

            directoryElementsListElement.appendChild(listElement);
        }
    };

    addListElements(data.directories, "elements " + CssClass.DirectoryEntry);
    addListElements(data.files, "elements " + CssClass.FileEntry);
};

let resetFileContainerContent = function() {
    let mediaContainerElement = 
            document.getElementById(ElementId.MediaDisplay);

    if (!mediaContainerElement) throw "Media container not found.";

    while (mediaContainerElement.firstChild)
        mediaContainerElement.removeChild(mediaContainerElement.firstChild);
};

let setFileContainerContent = function(resourcePath) {
    resetFileContainerContent();

    let mediaContainerElement = 
            document.getElementById(ElementId.MediaDisplay);

    if (!mediaContainerElement) throw "Media container not found.";

    let extensionSeparator = resourcePath.lastIndexOf('.');
    let extension = "";
    if (extensionSeparator >= 0) 
        extension = resourcePath.substring(resourcePath.lastIndexOf('.') + 1);
    extension = extension.toLocaleLowerCase().trim();

    let element;
    if (extension === "jpg" || extension === "jpeg" || extension === "png" || 
            extension === "gif" || extension === "svg" || extension === "bmp") {
        element = document.createElement("img");
        element.className = CssClass.MediaViewer;
        element.alt = extension + " file";
        element.src = resourcePath;
    } else if (extension === "mp4" || extension === "webm" || 
            extension === "ogg") {
        let videoElement = document.createElement("source");
        videoElement.src = resourcePath;
        videoElement.type = "video/" + extension;

        element = document.createElement("video");
        element.appendChild(videoElement);                    
        element.className = CssClass.MediaViewer;
        element.controls = true;
    }
    else return false;

    mediaContainerElement.appendChild(element);
    return true;
};

let sendApiRequestGet = function(fullApiPath, onSuccess, onError) {
    let request = new XMLHttpRequest();

    request.open("GET", fullApiPath, true);

    request.onload = function() {
        let responseData = {};
        try { responseData = JSON.parse(this.response); }
        catch (exc) { onError(InternalError.InvalidJson, exc); }

        if (this.status >= 200 && this.status < 400) 
            onSuccess(responseData);
        else onError(this.status, responseData);
    };

    request.onerror = function() {
        onError(InternalError.ConnectionFailure, 
        "Connection failure.");
    };

    request.send();
};

let sendApiRequestPost = function(fullApiPath, jsonData, onSuccess, 
    onError) {
    let request = new XMLHttpRequest();

    request.open("POST", fullApiPath, true);
    request.setRequestHeader("Content-Type", "application/json");

    request.onload = function() {
        let responseData = {};
        try { responseData = JSON.parse(this.response); }
        catch (exc) { onError(InternalError.InvalidJson, exc); }

        if (this.status >= 200 && this.status < 400) onSuccess(responseData);
        else onError(this.status, responseData);
    };

    request.onerror = function() {
        onError(InternalError.ConnectionFailure, "Connection failure.");
    };

    request.send(JSON.stringify(jsonData));
};

let tryLogin = function() {
    if (mockupMode) {
        window.location.href = "#directories/";
        return;
    }

    let loginStatusElement = document.getElementById(ElementId.LoginStatus);
    if (!loginStatusElement) return;

    sendApiRequestPost(ApiPathRoot + "authenticate",
    getCurrentCredentials(), function() {
        window.location.href = "#directories/";
    }, function(status) {
        if (status === 403) {
            loginStatusElement.textContent =
                "Invalid credentials - please try again.";
        }
    });
};

let logout = function() {
    document.cookie = 
            "sessionId=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    window.location.href = "#";
    let loginStatusElement = 
            document.getElementById(ElementId.LoginStatus);
    if (!loginStatusElement) return;
    loginStatusElement.textContent = "Please authenticate yourself.";
};

window.onhashchange = function() {
    let errorHandler = function(statusCode, data) {
        if (statusCode === 401) {
            logout();
        }
        else if (statusCode === 404) {
            window.history.back();
            alert("The requested resource wasn't found.");
        } else {
            if (data && data.status) 
                alert("Server failure: " + data.status
                    + "\nPlease refresh page.");
            else alert("Unknown server failure.\nPlease refresh page.");
        }
    };

    let currentHash = location.hash;

    let changeSectionVisibility = function(loginVisible, 
        directoryVisible, fileVisible) {
            setElementVisibility(ElementId.MediaDisplay, 
                fileVisible);
            setElementVisibility(ElementId.DirectoryPage, 
                directoryVisible);
            setElementVisibility(ElementId.LoginDialog, 
                loginVisible);
            setElementVisibility(ElementId.Header, 
                directoryVisible || fileVisible);
        };

    if (currentHash.startsWith("#directories")) {
        if (mockupMode) {
            changeSectionVisibility(false, true, false);
            return;
        }                    

        setTimeout(resetFileContainerContent, 500);
        let requestedPath = ApiPathRoot + currentHash.substring(1);
        sendApiRequestGet(requestedPath, function(data){
            addElementsToDirectoryView(data.data, true);
            changeSectionVisibility(false, true, false);
        }, errorHandler);
    } else if (currentHash.startsWith("#files")) {
        if (mockupMode) {
            changeSectionVisibility(false, false, true);
            return;
        }

        let requestedPath = ApiPathRoot + currentHash.substring(1);
        setFileContainerContent(requestedPath);
        changeSectionVisibility(false, false, true);
    } else {
        changeSectionVisibility(true, false, false);
        setTimeout(resetFileContainerContent, 500);
        setTimeout(clearDirectoryView, 500);
        clearCurrentCredentials();

        let loginStatusElement = 
            document.getElementById(ElementId.LoginStatus);
        if (!loginStatusElement) return;
        loginStatusElement.textContent = 
                "Please authenticate yourself.";
    }
};

window.onhashchange();