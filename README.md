# Vistava

A no-nonsense multimedia browser for your files, with local network sharing support.

## Overview

Vistava shows your local folders and files in a masonry-style gallery and lets you browse your photos, artwork, comics, or videos with ease - whether you use a keyboard, mouse, touchscreen, or even gamepad. 

When the sharing feature is enabled, Vistava can be accessed through the web browser of any smartphone, tablet, game console, TV or VR/AR headset within the same local network; no internet access required.

Vistava is completely free, open source and supported on Windows and Linux, with the sharing feature supported by Chrome and other modern web browsers on PC and mobile devices.

### Example use-cases

- Easily watch videos from your PC on your VR headset without additional apps
- Watch movies from your computer on your smart TV, Playstation or Xbox
- Browse through your local collection of artwork or reference images without distractions
- Read comics or manga comfortably on every device
- Make slideshow presentations of detailled pictures or diagrams

## Installation and building

### Windows

Go to the "Releases" section and download the most recent Vistava installer (recommended) or the ZIP archive for running the application without installing. After installing or extracting the application, run the application using the Desktop shortcut or by opening the "Vistava" executable in the extracted directory.

### Linux

Depending on your distribution, you can choose between downloading a Pacman package (for Arch and derivatives), a DEB package (for Debian, Ubuntu and derivatives) or an AppImage. 

When using the DEB package, you might have to install the .NET Runtime (8.0.20 or newer) _and_ the ASP.NET Runtime (8.0.20 or newer) manually - see [this link](https://learn.microsoft.com/en-us/dotnet/core/install/linux) for setup instructions.

When using the Appimage, additionally to installing the .NET Runtime (8.0.20 or newer) _and_ the ASP.NET Runtime (8.0.20 or newer) manually, you might have to install the dependencies "[tumbler](https://docs.xfce.org/xfce/tumbler/start)" and "[ffmpegthumbnailer](https://apps.kde.org/ffmpegthumbs/)" - otherwise, the app might not start correctly or display no file thumbnails.

### Build from source

To build the application from source, Node.js (24.9 or newer), npm (11.6.2 or newer) and the .NET SDK (8.0.20 or newer) are required. Either open the root repository folder in VSCode and run the "Build app" task, or open a command line interface inside the root folder and run `npm install` and `npm run build` (or, depending on your target platform, `npm run build:windows` or `npm run build:linux`). See the `/dist` folder for the build output.

## Usage

Vistava can either be opened directly, or by right-clicking on a directory on your file explorer and clicking on "Open with Vistava". 

### Local network sharing

Upon starting the application, the "sharing" feature is initially disabled - so Vistava can not be accessed over the local network until the "Share" button (in the top right of the window) is clicked and local network sharing is enabled. 

When enabled, the "Link" button becomes accessible - which will, upon hovering over it, show the URL and a QR code to access Vistava from anywhere within your local network. Clicking onto the "Link" button will copy the URL to the clipboard.

### User input

Vistava can be navigated using a mouse, a keyboard, a touchscreen or certain supported gamepads.

When navigating through folders or opening files, the previous/following directory or view can be accessed by using the "back" or "forward" functionality. Unless when in the root directory, the parent directory can also always be reached by clicking on the first tile (the directory symbol with the two dots in it).

The thumbnail and detail views are synchronized - meaning that, when opening the first media file in a directory and then navigating to the next items there, returning to the thumbnail view will change the selection to the media item that was last viewed in the detail view.

#### Mouse

The application can be navigated using a mouse only - by clicking on available files and directories with the left mouse button and scrolling through the available content using the mouse wheel. 

Navigating back or forward in history can be done either using the buttons in the top left corner of the window, or dedicated "back" and "forward" buttons on your mouse - if available.

When opening the "detail view" of an image or video, the controls for zooming, controlling playback (for videos) or toggling fullscreen can be found by moving the cursor to the bottom of the window. For zooming both images and videos, double-clicking on the media item can be used as well as the mouse wheel. The detail view can be left by using the back button.

#### Keyboard

Vistava can be controlled using only a keyboard as well: Using the arrow keys moves the selection around, while hitting the "enter" key will open the selected folder or file. Slowly scrolling through the available content can be done by holding down the arrow keys in the desired direction.

The "backspace" key will navigate one step back in history - which works both when navigating folders, or when switching from the detail view back to the thumbnail view.

In the detail view, the different zoom modes can be cycled through by using "right shift". Zooming in/out can also be done using the "+"/"-" keys or the ","/"." keys. When zoomed in, the media item can be moved around using the arrow keys. By pressing the "F" key, fullscreen will be toggled. To start or pause playback of a video, the "spacebar" key can be used. When a video is being played back, the arrow keys can be used to skip through the video (using "left" and "right") or to adjust the volume ("up" and "down"). The "M" key can be used to mute or unmute audio of video playback.

#### Touchscreen

When using the "sharing" feature with a smartphone (or when having a touchscreen on the computer where Vistava is running on), Vistava can also be controlled using touch gestures. Scrolling through content and opening items can be done with swiping and tapping, while the detail view also supports drag and pinch gestures to zoom in or out of an image or video, to move it around the screen, or to switch to the next one in the directory.

#### Gamepads

For certain scenarios (e.g. presentations), the application can also be controlled using a game controller. Currently supported are standard Xbox controllers, PlayStation 4 controllers and the Nintendo Switch Joy Cons on the Vistava application itself. Note that this does _not_ work when accessing the application over the "Sharing" feature.

While the button mappings and functionality may vary across the different controller types, different operating systems or connection mode (bluetooth vs. cable), the input scheme is similar to the keyboard-based one: Analog sticks or D-Pads are equivalent to the arrow keys, the primary action button (e.g. "A" on an Xbox Controller) is the equivalent of the "enter" key, and the secondary action button (e.g. "B" on an Xbox Controller) is the equivalent of the "backspace" key.

## License

Copyright (C) 2025 Maximilian Bauer.

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.