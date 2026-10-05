# Basic usage

Vistava was designed to be intuitive, easy and non-frustrating to use in different scenarios: whether you're sitting on the couch and trying to find the right show or episode to watch, drawing and trying to find the right reference image, or reading your favourite comics without the controls ruining your immersion.

## Local network sharing

The "local network sharing" feature included in Vistava allows you to share your media files in your local network. When enabled, Vistava generates a local HTTP URL. Open it on another device in the same network (private WiFi or LAN) with a modern browser or scan the QR code shown in the app. That second device shows the same UI and the same media library, so you can easily watch your content on a Smart TV, tablet, phone, or VR headset.

For security reasons, the generated URL is randomized and only works while network sharing is enabled. This should make it harder for others in the same network to gain access to your media without knowing the exact URL. To avoid accidentally exposing your media files to strangers, only enable this feature in its intended setting of a private home network behind a router.

> [!CAUTION]
> **Caution:** Never enable the local network sharing feature in unencrypted or public WiFi networks like at cafés or airports!

Advanced users can (try to) configure HTTPS for the network sharing feature to increase security. See the [related section](/4-advanced?id=enabling-https-support) for more information.

## Navigating Vistava

Depending on your use case, you can control Vistava using the input devices described in the following sections — either on their own or in combination. Keep in mind that the controls described below are the default settings, which can be changed (as described in the section about [managing configurations](/4-advanced?id=managing-configurations)).

Before getting into the different input schemes, it should be noted that there are two different view types in Vistava: the initial **thumbnail view**, with its masonry-like overview of the available media, and the **detail view**, which (like a slideshow) shows media items at their original size. As the two views have different purposes, the controls differ slightly and are described separately.

> [!TIP]
> **Tip:** The currently selected item is synced between the two views. This means that if you open an item in the thumbnail view to see it in the detail view, move a few items ahead, and then return to the thumbnail view, your selection will have moved to the last item you viewed in the detail view.

### Keyboard

In the initial thumbnail view, you can navigate around the grid of thumbnails using the arrow keys on your keyboard. This changes the currently selected item, which has a faint white glow around it.

Press `Enter` to open the currently selected item. Depending on the type of item, this either opens the item itself in the detail view (for media items) or displays the contents of the directory or playlist represented by the thumbnail icon.

Press `Shift+Enter` to open the selected item in your operating system's file browser. If the selected item is a media item, its parent directory will open with the file selected.

After opening a media item (and entering the detail view) or another directory or playlist, press `Backspace` to return to the previous view or directory.

In the detail view, you can navigate between the previous and next items in the list using the `Left` and `Right` arrow keys.

You can zoom in and out of a media item (both images and videos) in steps using the `+` and `-` keys. Alternatively, you can zoom in using the `,` and `.` keys, though this might not make much sense on non-German keyboard layouts. Press `Shift` to toggle between the available scale levels: "fit to height", original size and "fit to width". To make comics easier to read, this zooms into the top of the image by default; you can turn this behaviour off in the configuration files. When zoomed in, tap or hold down the arrow keys to move the view.

If the current item is a video clip, press `Space` to start or pause playback. Press `M` to mute or unmute the audio, and `L` to turn looping on or off. Unlike with images, the arrow keys behave differently here: the `Up` and `Down` arrow keys change the playback volume, while the `Left` and `Right` arrow keys either move the playback position within the clip (if playback is running) or move to the previous or next item (if playback is paused).

> [!TIP]
> **Tip:** You can still move the view on zoomed-in video items by holding down the arrow keys.

Press `F` to toggle between fullscreen and windowed modes. This also dims the background to black and switches between the zoom modes: windowed mode uses the original scale by default, while fullscreen mode uses "fit to height". Press `Escape` to exit fullscreen mode.

### Mouse

In the initial thumbnail view, use the mouse wheel to scroll through the available items. Click an item with the left mouse button to open it in the detail view (for media items) or display the contents of the directory or playlist represented by the thumbnail icon. Click an item with the right mouse button to select it. To open an item in your operating system's file browser, click it with the middle mouse button. If the item is a media item, its parent directory will open with the file selected.

After opening a media item (and entering the detail view) or another directory or playlist, return to the previous view or directory by clicking the "Back" button in the application title bar or pressing the back button on your mouse (if it has one). Similarly, navigate forward by clicking the "Forward" button in the title bar or pressing the forward button on your mouse.

In the detail view, navigate between the previous and next items in the list by clicking the arrows on the centre-left or centre-right side of the window. These buttons become visible when you move the cursor over the sides of the window and disappear after a few seconds. You can still click the buttons when they are invisible.

Moving the mouse cursor to the bottom of the window reveals the fullscreen button, which toggles between fullscreen and windowed modes, and—if the current media item is a video—the playback controls. Click the "Play" button to start or pause playback. Click or drag inside the adjacent progress bar to seek to a position in the video.

To zoom in or out of a media item, use the mouse wheel; the cursor position is used as the zoom target. Right-click the media item to toggle between the available zoom steps. When zoomed in, left-click and drag to move the view.

Double-click the left mouse button to toggle between fullscreen and windowed modes. This also dims the background to black and switches between the zoom modes: windowed mode uses the original scale by default, while fullscreen mode uses "fit to height".

### Gamepads

Vistava can also be controlled using gamepads. This can be useful, for example, when running it on a PC or console (such as a Steam Deck) connected to a TV usually used for gaming. Two types of gamepads are supported by default, but you can add other currently unsupported game controllers via the configuration (as described in the section about [managing gamepad configurations](/4-advanced?id=gamepad-settings)).

> [!NOTE]
> **Note:** Gamepad navigation is only supported over the Vistava desktop app. Advanced users may be able to get gamepad navigation to work on other devices (using local network sharing) by [setting up HTTPS](/4-advanced?id=enabling-https-support), but this has not been tested extensively.

#### Standard gamepads

The standard gamepad layout usually mimics that of Xbox or PlayStation controllers, with two analog sticks, a D-pad, left and right shoulder buttons and triggers, and four buttons on the right side of the controller.

In the initial thumbnail view, use the D-pad to navigate around the grid of thumbnails. This changes the currently selected item, which has a faint white glow around it. Use the left analog stick to scroll slowly through the available items, or the right analog stick to scroll a little faster.

Press the lower `A` button (`Cross` on a PlayStation controller) to open the currently selected item. Depending on the type of item, this either opens the item itself in the detail view (for media items) or displays the contents of the directory or playlist represented by the thumbnail icon.

After opening a media item (and entering the detail view) or another directory or playlist, press the right `B` button (`Circle` on a PlayStation controller) to return to the previous view or directory.

In the detail view, use the D-pad to navigate between the previous and next items in the list.

Use the right analog stick to zoom in or out of a media item gradually. Press the upper `Y` button (`Triangle` on a PlayStation controller) to toggle between the available zoom steps (as described in the keyboard section above). When zoomed in, use the left analog stick or the D-pad to move the view.

If the current item is a video clip, press the `A` button (`Cross` on a PlayStation controller) to start or pause playback. Unlike with images, the D-pad behaves differently here: `Up` and `Down` change the playback volume, while `Left` and `Right` either move the playback position within the clip (if playback is running) or move to the previous or next item (if playback is paused).

> [!TIP]
> **Tip:** You can still move the view on zoomed-in video items using the left analog stick.

Press the menu button (`Options` on a PlayStation controller) to toggle between fullscreen and windowed modes. This also dims the background to black and switches between the zoom modes: windowed mode uses the original scale by default, while fullscreen mode uses "fit to height".

#### 8BitDo Micro gamepad

For slideshows or presentations, this [very small, presenter-like gamepad](https://www.8bitdo.com/micro/) can be used. Since it only has two shoulder buttons, a D-pad and six other buttons, its control options are more limited. When used with Vistava, hold the gamepad *90° sideways*, like a presenter remote. It doesn't matter which way you turn it or which hand you hold it in, as the controls are duplicated for each orientation.

In other words, both the D-pad and the four main action buttons mimic the arrow keys. They can be used to navigate the thumbnail and detail views, change video playback position, or adjust video volume. The outer shoulder buttons open media items or toggle video playback; the inner shoulder buttons navigate to the previous view or directory.

Use the `+` and `-` buttons to toggle between zoom steps, and the `Star` button to toggle between fullscreen and windowed modes.

### Touchscreen

In the initial thumbnail view, you can navigate around the grid of thumbnails by touching the screen with your finger, dragging it up or down, and lifting it when you want to stop. Tap an item with your finger to open it in the detail view (for media items) or display the contents of the directory or playlist represented by the thumbnail icon.

After opening a media item (and entering the detail view) or another directory or playlist, return to the previous view or directory either by using the "Back" button on your smartphone or by performing the system back gesture associated with your phone. On Android devices, this is usually done by swiping from the leftmost edge of the screen to the right.

In the detail view, you can zoom in or out of a media item using a pinch-to-zoom gesture: place two fingers on the screen and spread them apart (or bring them closer together). You can also double-tap to cycle through the available zoom steps. While zoomed in, you can move the media item by dragging it around.

Tapping near the bottom of the screen reveals the usually hidden controls for toggling fullscreen, video playback, and volume. These controls also work when hidden, so tapping in the lower-right corner of the screen will still toggle video playback even if the button is currently hidden.

You can navigate between the previous and next items in the list by dragging the current item to the left or right side of the screen; you may have to zoom out first to see the full media item.

### VR/AR headset

> [!NOTE]
> **Note:** Support for VR/AR headsets is experimental and was only tested using a Meta Quest 2. It should work on other headsets too, but navigation may differ from the description below.

In the initial thumbnail view, you can navigate around the grid of thumbnails either by dragging the page up and down or by using the analog stick on either controller. Click an item to open it in the detail view (for media items) or display the contents of the directory or playlist represented by the thumbnail icon.

After opening a media item (and entering the detail view) or another directory or playlist, return to the previous view or directory either by using the "Back" button in the browser.

In the detail view, you can zoom in or out of a media item by simulating a pinch-to-zoom gesture (with two hands or two controllers) or by pushing the analog stick on either controller up or down. You can navigate between the previous and next items in the list either by pushing the analog stick on either controller left or right or by dragging the current item to the left or right side of the screen; you may have to zoom out first to see the full media item.

Moving the cursor to the bottom of the screen should reveal the usually hidden controls for toggling fullscreen, video playback, and volume. These controls also work when hidden, so tapping in the lower-right corner of the screen will still toggle video playback even if the button is currently hidden.

## Supported file formats

Vistava supports media file formats commonly used on the modern web, as well as a few additional formats to support the workflows of photographers and artists.

For video, Vistava supports the following two container formats and their associated codecs:
- MP4
   - AV1
   - AVC (H.264)
   - HEVC (H.265)
   - MP4V-ES
   - MPEG-2
   - VP8
   - VP9
- WebM
   - AV1
   - VP8
   - VP9

The following standard image formats are supported natively:
- JPG/JPEG
- PNG
- WebP
- GIF
- SVG

The following image formats are also supported through an experimental in-place conversion powered by ImageMagick:
- TIF/TIFF
- PSD
- DDS

The video and image formats listed above can be used in playlists, which can be opened in Vistava and behave mostly like directories. The supported playlist file formats are:
- M3U
- M3U8
- XSPF