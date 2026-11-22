# Developer information

This page contains information for advanced users and developers who want to work with Vistava or create new extensions for additional sources.

## Building from source

To build Vistava from source, you need Node.js (24.9 or newer), npm (11.6.2 or newer), and the .NET SDK with ASP.NET (10 or newer). Either open the root repository folder in VS Code and run the "Build app (Windows)" or "Build app (Linux)" task, or open a command line interface in the root folder and run `npm install` followed by `npm run build:windows` or `npm run build:linux`. See the `/dist` folder for the build output.

For running Vistava, the following dependencies need to be installed for the application to work correctly:
- [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/install/linux), specifically the ".NET Runtime" and the "ASP.NET Core Runtime"
- ffmpeg-full

If the application doesn't start at all with an error message claiming that the service couldn't be started or doesn't respond, this most likely refers to an issue with .NET. If the application starts but fails to generate any (or only a few) video thumbnails, the ffmpeg installation might lack some required codecs.

### Debugging and troubleshooting

To troubleshoot Vistava, it is recommended to launch it via command line and append the `--debug-mode` flag. This flag will enable verbose logging on the console itself and open the Electron "Developer Tools" window with Vistava, allowing to troubleshoot issues in the UI or with Electron.

## "Headless mode"

Most of Vistava's functionality is provided through a background service. This service is usually started by the Electron wrapper, which is what users normally run and see when using Vistava. However, the background service can also be started separately, allowing Vistava to run as a headless service.

> [!CAUTION]
> **Caution:** Running the Vistava background service directly should only be done by advanced users who are aware of the implications of running Vistava without its security features. **Never run the service on a port that is openly accessible from the internet** (for example, via port forwarding)!

To run the Vistava service, open a terminal in the directory where the application is installed. The service executable is located at `resources/app/bin/win/Vistava.Service.exe` on Windows or `resources/app/bin/linux/Vistava.Service.elf` on Linux. You can either run the application directly in the terminal or view the available command-line flags with `--help`:

```
** Vistava.Service 1.1 **
--help: Print this help. 
--debug=true: Set default log level to 'debug'.
--port=PORT: Accept for HTTP/S traffic on the specified port.
--randomizeBasePath=true: Randomize the application URL root.
--allowCors=true: Allow CORS (for any origins).
--public=true: Accept connections from all hosts and not just localhost.
--disableHttps=true: Ignore any HTTPS certificates and always disable HTTPS.
```

## Writing extensions

Vistava supports additional media sources as extensions. These extensions are loaded when Vistava starts and become available in the extension menu, which can be opened by clicking the extensions button (the puzzle piece symbol) in the application title bar.

> [!TIP]
> **Tip:** It is highly recommended to use the [template repository for Vistava extensions](https://github.com/bauermaximilian/Vistava.Extension), which includes the following example code, build scripts and basic instructions.

### Implementation

A "definition file" both defines the extension identifier (through its name), the main entry point for the extension, and any configuration parameters that can be edited by the user — for example, to change the name/label of the extension in the UI, to add API keys or change the extension's behaviour. These files must end with `.source.json`, so a valid name for such a file would be `your_extension_id.source.json` (with "your_extension_id" being the extension identifier then). The content of this file could look like this:

```json
{
   "moduleFileName": "your_extension_id/SourceYourExtension.js",
   "configuration": {
      "name": "Extension Label displayed in Vistava",
      "imageTextPrefix": "ugh"
   }
}
```

This definition file would set `SourceYourExtension.js` (in the `your_extension_id` subfolder) as the entry point for the extension module, prompting Vistava to look for a default class module export that implements the `Source` base class (or derived classes thereof). Vistava would then instantiate this class and pass the object associated with the `configuration` key as the constructor parameter. 

> [!NOTE]
> **Note:** The source file (and all other code files related to that extension) _must_ be inside a subdirectory with the exact same name as the extension identifier itself, which should consist of lowercase characters, numbers and underscores only! The display name can be different and is defined by the `configuration.name` parameter value in the `.source.json` file.

A simple implementation of the `SourceYourExtension` class could look like this:

```javascript
// SPDX-License-Identifier: GPL-3.0-or-later

// Vistava makes the "vistava.js" library available at the path 
// "../../Dependencies/vistava.js/src/" (relative to this file when deployed).
import { Source } from "../../Dependencies/vistava.js/src/Shared/Source.js";

// If your source provides paginated results, consider using "SourceSegmented" instead.
export default class SourceYourExtension extends Source {
   // The following JSdoc typedefs work similar to a file-scoped import.
   /**
    * @template T
    * @typedef {import(
    * "../../Dependencies/vistava.js/src/Shared/CachedCollection.js")
    * .CollectionRetriever<T>} CollectionRetriever<T>
    */

   /**
    * @typedef {import(
    * "../../Dependencies/vistava.js/src/Components/TileGrid/Shared/TileValue.js")
    * .TileValueInitializer} TileValueInitializer
    */

   /**
    * @param {object} configuration The value of the "configuration"
    * object from the definition file.
    */
   constructor(configuration) {
      super(configuration);
   }

   /**
    * @param {string} query The query string, provided by Vistava, user input or the "queryTarget"
    * value of a tile the user clicked.
    * @returns {CollectionRetriever<TileValueInitializer>}
    */
   createCollectionRetriever(query) {
      return async (offset, count) => {
         /** @type {TileValueInitializer[]} */
         let results = [];
         for (let i = 0; i < count; i++) {
            results.push(this.#generateValue(query, i + offset));
         }
         return results;
      }
   }

   /**
    * @param {string} query
    * @param {number} index
    * @returns {TileValueInitializer} An object initializer used to populate the tiles shown 
    * to the user.
    */
   #generateValue(query, index) {
      // Generate an image text using the "imageTextPrefix"
      // configuration parameter.
      let imageText = ((this.configuration.imageTextPrefix ?? "") +
         " #" + index).trim();
      imageText = encodeURIComponent(imageText);
      return {
         // The label may be hidden depending on Vistava configuration.
         label: `${query} #${index}`,
         mediaType: "image/png",
         mediaUrl: `https://placehold.co/400x400.png?text=${imageText}`,
         thumbnailType: "image/png",
         thumbnailUrl: `https://placehold.co/200x200.png?text=${imageText}`
      }
   }
}
```

This is a very simple example of an (infinite) source of mock media items. It does not implement any caching mechanism, which is recommended for real web sources to avoid unnecessary API calls.

The abstract `SourceSegmented` class, which implements the `Source` base class, adds caching and pagination and can be used instead of the `Source` base class. There, the `createContentRetriever` method must be overridden; instead of returning an asynchronous function, it returns a `SourceSegmentedContentRetriever` instance. Any classes implementing that base class should override the `pageLength` property (returning the number of items per page) and the `getPageTilesAsync` method, which is used internally if a certain region of the item cache has not been loaded yet. The number of items returned by this method should match the `pageLength` property value unless the requested page is partially outside the bounds of the available content. In that case, the returned array may contain fewer items (or zero items). This information is used by the base class to determine the end of the item collection.

### Development and debugging

An extension project repository should ideally follow the following directory structure for easier development and deployment:

```
.
├── Dependencies
│   └── vistava.js (Git submodule)
│       ├── src
|       ├── ...
├── Sources
│   ├── your_extension_id.source.json
|   ├── your_extension_id
│   |   ├── SourceYourExtension.js
│   |   └── ...
|   └── ...
└── README.md
```

While the root folder `.` can be the `include` directory inside the Vistava application data directory, it is highly recommended to instead create a symbolic link inside the `include` directory called `Sources` that points to the `Sources` directory inside your repository. This allows you to develop the extension and test it in Vistava at the same time.

For debugging and troubleshooting your implementation, you can run Vistava in debug mode with the command line flag `--debug-mode`. This opens the Chromium developer console on launch, which you can use to inspect console output and investigate your code.

> [!TIP]
> **Tip:** Refreshing Vistava (using the "Refresh" button in the application title bar) also reloads the extensions, so you do not have to restart the application for code changes to take effect.

Alternatively, you can also develop extensions directly as part of Vistava by placing your extension source files in the `service/wwwroot/Sources` directory inside the Vistava repository. This makes debugging in VS Code easier, but it requires rebuilding the application after every code change and makes it less straightforward to distribute the extension on its own. The `.gitignore` file in that directory also excludes any files besides the standard file system source from being committed to the repository by accident.

### Deploying extensions

> [!TIP]
> **Tip:** If the template repository was used, the "build" action will do this automatically for you.

Extensions should be deployed as a compressed ZIP archive with the file extension `.vex.zip`. The archive should, in its root, only contain the extension definition file (e.g. `example.source.json`) and a single directory (e.g. `example`) with all source files. As an example, the archive content of the example extension above could look as follows:

```
your_extension_id-1.0.0.vex.zip
├── your_extension_id.source.json
└── your_extension_id
    ├── SourceYourExtension.js
    └── ...
```

That `your_extension_id-1.0.0.vex` file can then be provided to users, who then would import it through the "Install extension..." option in Vistava.