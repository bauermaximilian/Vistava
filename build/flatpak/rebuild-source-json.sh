#!/bin/bash

# This script should be run if any NPM packages or C#/NuGet packages were changed or updated.
# The generated files need to go into the "vistava-flatpak" repository.
# Ensure the 'flatpak-node-generator' is installed 
# (see https://github.com/flatpak/flatpak-builder-tools/tree/master/node) 

if ! command -v python3 >/dev/null 2>&1
then
    echo "python3 is not available! Please install and try again."
    exit 1
fi

if ! command -v flatpak-node-generator >/dev/null 2>&1
then
    echo "flatpak-node-generator is not available! Please install and try again."
    exit 1
fi

echo Rebuilding nuget-sources.json...
python3 flatpak-dotnet-generator.py --dotnet 10 --freedesktop 25.08 nuget-sources.json ../../service/Vistava.Service.csproj

echo Rebuilding npm-sources.json...
flatpak-node-generator npm ../../package-lock.json -o npm-sources.json

echo JSON files rebuilt.