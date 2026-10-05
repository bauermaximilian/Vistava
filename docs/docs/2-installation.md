# Installing Vistava

Vistava is currently supported on Windows and Linux. To download either version, go to the "Download" section on the [Vistava website](index.html#download ':ignore') and click one of the download links, or navigate to the "Releases" page on the right side of the GitHub repository and select the latest release at the top of the list.

## Windows

To install Vistava on Windows, you'll need **Windows 10 or newer (64-bit)**. You can choose between an installer (recommended for most users) and a portable ZIP archive.

### Installer

You can download the installer by clicking the "Installer (recommended)" link in the "Download" section on the [Vistava website](index.html#download ':ignore'), or by downloading the `.exe` asset (e.g., `vistava-win-setup-1.0.0.exe`) from the latest release on the GitHub releases page.

After downloading, open the installer. Windows might warn you that it prevented an unrecognized app from starting. This happens because I didn't want to pay $100 a year for an expensive developer certificate subscription. To continue with the installation, click "More info" and then "Run anyway".

An installation wizard should open shortly after. Click "Next" a few times, be sure to read the entire GPL-3 license (as one usually does), and then wait for setup to complete. Once installation is complete, you can run the application directly from the installer or use the shortcuts created in your Start menu or on your desktop.

You can also right-click a folder in File Explorer to open Vistava directly in that directory. On Windows 11, you might need to show all entries in the context menu to see this option. Note that this option isn't available with the portable version of Vistava described below.

### ZIP archive

You can download the portable ZIP archive by clicking the "Portable ZIP" link in the "Download" section on the [Vistava website](index.html#download ':ignore'), or by downloading the `.zip` asset (e.g., `vistava-win-1.0.0.zip`) from the latest release on the GitHub releases page.

After downloading the archive, extract it to a folder where you have sufficient permissions, such as your desktop or a USB stick. If you already have the FFmpeg dependency installed, start the application by opening the `Vistava.exe` (or `Vistava`) executable with the "V" icon in the extracted directory. Windows might warn you that it prevented an unrecognized app from starting. To continue, click "More info" and then "Run anyway".

> [!NOTE]
> **Note:** Vistava does store temporary files and its configuration in the current user's application data folder, not in the program directory. Keep this in mind when using Vistava on different computers!

## Linux

To install Vistava on Linux, you'll need a **64-bit distribution** that supports Flatpak (as most modern distributions do, including Debian, Mint, Ubuntu, and Fedora) or is Arch-based (such as Arch itself, SteamOS 3+, CachyOS, or Manjaro).

### Flatpak

You can download the Flatpak package by clicking the "Flatpak (recommended)" link in the "Download" section on the [Vistava website](index.html#download ':ignore'), or by downloading the `.flatpak` asset (e.g., `vistava-linux-1.0.0.flatpak`) from the latest release on the GitHub releases page.

After downloading, either open and install the file using an app like "Discover" (included in KDE) or run the following command in the directory containing the downloaded Flatpak file to install Vistava for the current user. Replace `vistava-linux-1.0.0.flatpak` with the actual name of the downloaded file:

```bash
flatpak --user install vistava-linux-1.0.0.flatpak
```

To install Vistava system-wide for all users, run:

```bash
flatpak install vistava-linux-1.0.0.flatpak
```

After installation, you can launch the application using the shortcuts created in your application launcher (in the "Graphics" and "Multimedia" categories) or by opening a directory with Vistava. The latter launches Vistava with that directory as its starting directory.

#### Flatpak permissions

By default, the Flatpak version **has read-only access to your home directory only**. To make other parts of your file system accessible, you can use an application like "Flatseal". In Flatseal, add a new entry under "Filesystem > Other files", then restart Vistava.

You can do the same from the command line. The following command allows read-only access to the example path `/media/Data`:

```bash
flatpak override com.bauermaximilian.vistava --filesystem=/media/Data:ro
```

### Pacman

Arch-based distributions (such as Arch itself, SteamOS 3+, CachyOS, or Manjaro) can install Vistava using the provided `pacman` package, either by downloading the file directly from the [Vistava website](index.html#download ':ignore') or through the AUR, using the package name `vistava-bin`.

After installing Vistava using your preferred method below, you can launch it using the shortcuts created in your application launcher (in the "Graphics" and "Multimedia" categories), by opening a directory with Vistava in your file explorer, or by running the `vistava` command in a terminal.

#### Direct download

You can download the `pacman` package by clicking the "Pacman package" link in the "Download" section on the Vistava website, or by downloading the `.pacman` asset (e.g., `vistava-linux-1.0.0.pacman`) from the latest release on the GitHub releases page.

After downloading, run the following command in the directory containing the downloaded `pacman` file. Replace `vistava-linux-1.0.0.pacman` with the actual name of the downloaded file:

```bash
sudo pacman -U vistava-linux-1.0.0.pacman
```

Confirm the installation of the package and wait for the process to complete.

## Other OSes and platforms

Vistava is currently not supported on operating systems or platforms other than those described above.

Adding macOS support would require only minor adjustments. However, building and maintaining applications for macOS requires owning an Apple computer running a relatively modern version of macOS (which I don't have).