# Developer information

This page contains information for advanced users and developers who want to work with Vistava or create new extensions for additional sources.

## Building from source

To build Vistava from source, you need Node.js (24.9 or newer), npm (11.6.2 or newer), and the .NET SDK (8.0.20 or newer). Either open the root repository folder in VS Code and run the "Build app (Windows)" or "Build app (Linux)" task, or open a command line interface in the root folder and run `npm install` followed by `npm run build:windows` or `npm run build:linux`. See the `/dist` folder for the build output.

For building the Flatpak version, make sure you have `flatpak` and `flatpak-builder` installed. You also need the runtime and SDK installed with commands such as `flatpak install org.freedesktop.Platform/x86_64/25.08`, `flatpak install org.freedesktop.Sdk/x86_64/25.08`, and `flatpak install app/org.electronjs.Electron2.BaseApp/x86_64/25.08`. Then run `npm run build:linux-flatpak` to start the build.

The Flatpak bundler pulls the sources for FFmpeg, libdav1d, and libx264 from their respective Git repositories and installs them into the Flatpak. This can take 5-10 minutes or longer, depending on your hardware. If the process fails, you can check the verbose log that is enabled by default for Flatpak builds.

## "Headless mode"

Most of Vistava's functionality is provided through a background service. This service is usually started by the Electron wrapper, which is what users normally run and see when using Vistava. However, the background service can also be started separately, allowing Vistava to run as a headless service.

> [!CAUTION]
> **Caution:** Running the Vistava background service directly should only be done by advanced users who are aware of the implications of running Vistava without its security features. **Never run the service on a port that is openly accessible from the internet** (for example, via port forwarding)!

To run the Vistava service, open a terminal in the directory where the application is installed. The service executable is located at `resources/app/bin/win/Vistava.Service.exe` on Windows or `resources/app/bin/linux/Vistava.Service.elf` on Linux. You can either run the application directly in the terminal or view the available command-line flags with `--help`:

```
** Vistava.Service 1.0 **
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

### Implementation

A "definition file" both defines the main entry point for the extension and specifies configuration parameters that can be edited by the user — for example, to add API keys or change the extension's behaviour. These files must end with `.source.json`, so a valid name for such a file would be `example.source.json`. The content of this file could look like this:

```json
{
   "moduleFileName": "SourceExample.js",
   "configuration": {
      "imageTextPrefix": "Ahoj"
   }
}
```

This definition file would set `SourceExample.js` as the entry point for the extension module, prompting Vistava to look for a default class module export that implements the `Source` base class (or derived classes thereof). Vistava would then instantiate this class and pass the object associated with the `configuration` key as the constructor parameter. A simple implementation of the `SourceExample` class could look like this:

```javascript
// Vistava makes the "vistava.js" library available at the path
// "../Dependencies/vistava.js/src/", so it does not have to be
// shipped with extensions (and really shouldn't, actually).
import { Source } from "../Dependencies/vistava.js/src/Shared/Source.js";

export class SourceExample extends Source {
   // The "Example source" label will be visible in Vistava
   // as the extension name.
   get name() { return "Example source"; }

   /**
    * @template T
    * @typedef {import(
    * "../Dependencies/vistava.js/src/Shared/CachedCollection.js")
    * .CollectionRetriever<T>} CollectionRetriever<T>
    */

   /**
    * @param {object} configuration The value of the "configuration"
    * object from the definition file.
    */
   constructor(configuration) {
      super(configuration);
   }

   /**
    * @param {string} query
    * @returns {CollectionRetriever<object>}
    */
   createCollectionRetriever(query) {
      return async (offset, count) => {
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
    * @returns {object}
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

> [!TIP]
> **Tip:** Check out the available Vistava extensions on GitHub using the link on the main Vistava website to see implementations of "real" media sources for more in-depth examples of how to write extensions.

### Development and debugging

An extension project repository should ideally follow the following directory structure for easier development and deployment:

```
.
├── Dependencies
│   └── vistava.js (Git submodule)
│       ├── src
|       ├── ...
├── Sources
│   ├── example.source.json
│   ├── SourceExample.js
│   └── ...
└── README.md
```

While the root folder `.` can be the `include` directory inside the Vistava application data directory, it is highly recommended to instead create a symbolic link inside the `include` directory called `Sources` that points to the `Sources` directory inside your repository. This allows you to develop the extension and test it in Vistava at the same time.

For debugging and troubleshooting your implementation, you can run Vistava in debug mode with the command line flag `--debug-mode`. This opens the Chromium developer console on launch, which you can use to inspect console output and investigate your code.

> [!TIP]
> **Tip:** Refreshing Vistava (using the "Refresh" button in the application title bar) also reloads the extensions, so you do not have to restart the application for code changes to take effect.

Alternatively, you can also develop extensions directly as part of Vistava by placing your extension source files in the `service/wwwroot/Sources` directory inside the Vistava repository. This makes debugging in VS Code easier, but it requires rebuilding the application after every code change and makes it less straightforward to distribute the extension on its own. The `.gitignore` file in that directory also excludes any files besides the standard file system source from being committed to the repository by accident.