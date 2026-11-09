#!/bin/bash

# This script should help with building and installing the flatpak version of Vistava locally.
#
# Before running this script, ensure the following preconditions are met:
# - 'flatpak' and 'flatpak-builder' are installed
# - The Flathub remote is added (for this script, specifically the second command to set it up userwide)
#   * flatpak remote-add --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo
#   * flatpak remote-add --if-not-exists --user flathub https://dl.flathub.org/repo/flathub.flatpakrepo
#
# When any changes on the NPM packages or NuGet sources are made, follow the following steps:
# - After changing (adding/removing) NPM packages and updating package-lock.json:
#   * Ensure 'flatpak-node-generator' is installed (see https://github.com/flatpak/flatpak-builder-tools/tree/master/node)
#   * In the repository root, run the following command:
#     flatpak-node-generator npm package-lock.json -o build/flatpak/generated-sources.json
# - After changing (adding/removing) NuGet packages, run this command in build/flatpak:
#   python3 flatpak-dotnet-generator.py --dotnet 10 --freedesktop 25.08 nuget-sources.json ../../service/Vistava.Service.csproj

echo Starting flatpak build...
flatpak-builder build net.vistava.vistava.yml --repo=build-repo --install-deps-from=flathub --force-clean --user

echo Flatpak build completed!
echo "Creating single-file bundle in ../../dist/vistava-linux-1.0.0.flatpak... (this could take a few minutes)"

flatpak build-bundle build-repo ../../dist/vistava-linux-1.0.0.flatpak net.vistava.vistava --runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo

echo Flatpak bundle completed! 