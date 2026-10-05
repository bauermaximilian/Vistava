# Advanced topics

Vistava can be customized and extended in a few ways. In the current version, this admittedly isn't very user-friendly and requires editing configuration files or moving files around, but this documentation should hopefully make it a bit easier.

Application configuration files, extensions, and certificates are stored in the application data folder, which is created automatically the first time Vistava starts. The location of this folder may vary depending on the operating system and how Vistava was installed. You can open it by clicking the extensions button (with the "puzzle piece" symbol) in the title bar using the middle or right mouse button.

## Media cache

As you browse, Vistava generates thumbnails for any media files it encounters. For video clips, it also attempts to determine the video's duration. This data is stored in a cache file named `cache.temp` in the application data folder.

To clear the cache and its thumbnails and media information, simply delete that file while Vistava is closed. It will be regenerated when you restart Vistava.

> [!TIP]
> **Tip:** If you encounter issues with thumbnail generation (e.g. blank thumbnails or error icons everywhere), try deleting the `cache.temp` file.

Any storage data (e.g. browser cache) that is created by Electron itself is cleared automatically every time when Vistava is closed.

## Managing configurations

When you open the Vistava application data folder, you'll see a folder called "include". This folder contains a subfolder called "Configurations", which holds the JSON files for key bindings and application settings.

> [!WARNING]
> **Warning:** The configuration files *must* be valid JSON for Vistava to load it. Stray commas, missing braces, or other syntax errors will prevent the configuration file from loading. Use a JSON validator before saving to ensure the syntax is valid.

> [!TIP]
> **Tip:** To reset a specific configuration file back to default, delete that file and restart Vistava.

### Keyboard settings

The keyboard settings can be found in the file `keyboard.json` (inside the `include/Configurations` subfolder). When opened with a standard text editor (like [Notepad++](https://notepad-plus-plus.org/) or [Kate](https://kate-editor.org/)), you will see a file that should look somewhat like this:

```json
{
   "movement": {
      "speed": 350,
      "speedupDuration": 0.5,
      "speedupStart": 0.1,
      "upKey": "ArrowUp",
      "rightKey": "ArrowRight",
      "downKey": "ArrowDown",
      "leftKey": "ArrowLeft"
   },
   "actions": {
      "ArrowUp": "up",
      "ArrowRight": "right",
      "ArrowDown": "down",
      "ArrowLeft": "left",
      "Shift": "zoom",
      "Enter": "confirm",
      "Escape": "cancel",
      "Backspace": "back",
      " ": "play",
      "+": "zoomIn",
      ",": "zoomIn",
      ".": "zoomOut",
      "-": "zoomOut",
      "f": "fullscreen",
      "m": "toggleMute",
      "l": "toggleLoop",
      "Shift+Enter": "popup"
   }
}
```

#### Movement

The upper part of the file defines movement behaviour in the `movement` section. The parameter names (such as `"speed"` and `"upKey"`) are fixed and must not be changed. You can adjust their values (such as `350` for `speed` or `"ArrowUp"` for `upKey`) to change how movement works. The following rules apply to each parameter:
- `speed`: Defines the maximum speed of movement (in screen units per second) when the directional keys are pressed for longer. Must be a positive number.
- `speedupDuration`: Defines the amount of time in seconds for the movement to accelerate from zero to its maximum speed (defined by `speed`). Must be a positive number.
- `speedupStart`: Defines the amount of time in seconds between pressing down a directional key and the acceleration (defined by `speedupDuration`) to start. Must be a positive number.
- `upKey`, `rightKey`, `downKey`, and `leftKey`: Define the [key](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key) that triggers movement in the respective direction. Each must be a non-empty, case-sensitive string.

#### Actions

The lower part of the file defines which keyboard keys trigger which application actions. Each entry is a pair of strings (enclosed in quotes): the left value must be a valid [key](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key), and the right value must be one of the supported application actions listed below. You can prefix a key name (the left value) with a modifier such as `Shift` or `Ctrl` (the left or right modifier key), for example, `Shift+Enter` or `Ctrl+u`.

**Info:** Key names and action identifiers are case-sensitive.

You can freely add or remove key-action pairs within the curly braces after `"actions": {`. The following application action identifiers are recognized:

- `up`: Moves the view or selection up, or increases the volume of a video.
- `down`: Moves the view or selection down, or decreases the volume of a video.
- `right`: Moves the view or selection right, or skips forward in a playing video.
- `left`: Moves the view or selection left, or skips backward in a playing video.
- `zoom`: Cycles through the zoom modes: "fit to height", "original size", and "fit to width".
- `confirm`: Confirms dialogs, opens a subdirectory or a media item in the detail view, or toggles video playback between playing and paused.
- `cancel`: Cancels dialogs, returns to a previous directory, or goes back from the detail view to the thumbnail view.
- `play`: Toggles video playback between playing and paused.
- `zoomIn`: Zooms in on (enlarges) a media item in the detail view.
- `zoomOut`: Zooms out from (shrinks) a media item in the detail view.
- `fullscreen`: Toggles fullscreen mode.
- `toggleMute`: Toggles a video between muted and unmuted.
- `toggleLoop`: Turns video looping on or off.
- `popup`: Opens a media item or directory in the file explorer.

### Gamepad settings

The gamepad settings are in `gamepad.json` (inside the `include/Configurations` subfolder). When opened with a standard text editor (such as [Notepad++](https://notepad-plus-plus.org/) or [Kate](https://kate-editor.org/)), the file should look something like this:

```json
{
   "scrollSpeed": 1,
   "movementSpeed": 300,
   "movementSpeedupFactor": 2,
   "movementSpeedupTime": 3,
   "gamepads": {
      "(xbox|microsoft|standard)": {
         "axisMovement": {
            "horizontal": {
               "index": 0,
               "invert": true
            },
            "vertical": {
               "index": 1,
               "invert": true
            },
            "scroll": {
               "index": 3,
               "invert": false
            }
         },
         "buttonActions": {
            "0": "confirm",
            "1": "back",
            "2": "toggleMute",
            "3": "zoom",
            "4": "zoomOut",
            "5": "zoomIn",
            "9": "fullscreen",
            "12": "up",
            "13": "down",
            "14": "left",
            "15": "right"
         }
      }
   }
}
```

#### Generic

The short upper part of the file defines general movement behaviour, while the lower part (after `"gamepads"`) defines the settings for each type of supported gamepad. The parameter names (such as `"scrollSpeed"` and `"movementSpeedupTime"`) are fixed and must not be changed, but you can adjust their values (such as `300` for `movementSpeed`). The following rules apply to each parameter:
- `scrollSpeed`: Defines the scroll factor. Must be a positive number.
- `movementSpeed`: Defines the maximum movement speed (in screen units per second) when a directional input is held. Must be a positive number.
- `movementSpeedupFactor`: Defines the acceleration factor until movement reaches its maximum speed. Must be a positive number.
- `movementSpeedupTime`: Defines the time in seconds it takes to reach maximum acceleration. Must be a positive number.

#### Gamepad definitions

The longer, lower part of the file defines the supported gamepads. Each gamepad name is a case-insensitive [regular expression](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions), and its associated value is the configuration for any gamepad whose name matches that expression. This lets you reuse the same configuration for similar gamepads with different names.

> [!NOTE]
> **Note:** Depending on the operating system, browser, or connection method (Bluetooth or cable), the same gamepad may have a different name or button and axis mappings.

Each gamepad configuration has two sections: `"axisMovement"` and `"buttonActions"`.

The `"axisMovement"` section supports the following three names:
- `horizontal`: Defines horizontal movement using an axis (of an analog stick).
- `vertical`: Defines vertical movement using an axis (of an analog stick).
- `scroll`: Defines scrolling or zooming using an axis (of an analog stick).

All three are optional, but each must define the following two parameters if specified:
- `index`: An integer defining the axis index (starting at 0).
- `invert`: A boolean (`true` or `false`) indicating whether to flip the direction of the axis input.

The `"buttonActions"` section works similarly to the keyboard actions: the left value defines the button name (usually the button index as a string), and the right value is the associated action. See the [Actions](/4-advanced?id=actions) section above for a list of supported actions.

### Application settings

The application settings are in `tilegrid.json` (inside the `include/Configurations` subfolder). When opened with a standard text editor (such as [Notepad++](https://notepad-plus-plus.org/) or [Kate](https://kate-editor.org/)), the file should look something like this:

```json
{
   "thumbnails": {
      "showVideoLabels": true,
      "showImageLabels": false
   },
   "gallery": {
      "muteVideosByDefault": false,
      "loopVideos": true,
      "zoomToTop": true,
      "doubleClickZooms": false,
      "reverseSeekRolloverOnLoop": false
   }
}
```

The structure of this file is fixed and cannot be extended with more items, but you can change the parameter values. All currently supported parameters are boolean (`true` or `false`):
- `showVideoLabels`: Specifies whether video filenames are shown in the thumbnail view.
- `showImageLabels`: Specifies whether image filenames are shown in the thumbnail view.
- `muteVideosByDefault`: Specifies whether videos are muted by default.
- `loopVideos`: Specifies whether videos loop by default.
- `zoomToTop`: When switching to the "fit to width" zoom step, specifies whether the view moves to the top (`true`) or centre (`false`) of the image.
- `doubleClickZooms`: When you double-click a media item, `true` toggles to the next zoom step, while `false` toggles fullscreen mode.
- `reverseSeekRolloverOnLoop`: When seeking backwards in a playing video using a keyboard or gamepad, specifies whether reaching the beginning of the video makes the playback position roll over to the end (`true`) or stay at the beginning (`false`).

## Installing extensions

Vistava supports extensions, which provide additional sources of media content. An extension usually consists of a definition file (with a `.source.json` extension, e.g., `example.source.json`) and one or more source files (with a `.js` extension). The source files may be organized in subfolders or placed directly alongside the definition file.

To install an extension, copy both the definition file and the extension's source files directly into the "Sources" folder (`include/Sources/`).

**Example:** Let's say we have an extension called "SourceExample", provided by its developer as a ZIP archive. Extracting the archive creates a folder containing `example.source.json` and `SourceExample.js`. Copy both files (not the parent folder) into the "Sources" directory. If that directory was empty, it will now contain those two files. Depending on the extension developer's instructions, you may need to edit `example.source.json` and make sure its JSON syntax is still valid before saving. After restarting Vistava, the extension should appear when you click the extensions button (with the "puzzle piece" symbol) in the title bar.

## Enabling HTTPS support

To enable HTTPS support for the local sharing feature, place a valid PFX certificate file into the application data folder of Vistava (alongside the `cache.temp` file). Name the certificate file `https.pfx` and make sure it isn't password-protected. After restarting Vistava, the URL generated by the local network sharing feature should start with `https` instead of `http`.

Make sure the certificate/certificate authority is trusted on the devices you want to access Vistava from through the local network sharing feature to avoid security warnings and establish the secure context needed for certain browser features (e.g. gamepad support).

Explaining how to generate HTTPS certificates and get them working in other browsers without warnings or issues is beyond the scope of this documentation. The [mkcert](https://github.com/FiloSottile/mkcert) project makes this process much easier. Use the `-pkcs12` flag, rename the generated file to `https.pfx`, and it should work.