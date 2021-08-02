# Vistava

A media file server with a responsive web browser interface.

[Download Vistava 0.2 Installer](https://github.com/bauermaximilian/Vistava/releases/download/0.2/Vistava-0.2_setup.exe)

[Download Vistava 0.2 portable (as ZIP archive)](https://github.com/bauermaximilian/Vistava/releases/download/0.2/Vistava-0.2_portable.zip)

## About

There are many good reasons not to put all of your private pictures or videos into the cloud - be it privacy concerns or just economic reasons. However, missing out on the ability to easily access these media files from anywhere else but the computer they're stored on is a heavy price to pay. If only there was a solution to that problem...

## The solution to that problem

![screencast](media/screencast.gif)

Vistava is a HTTP media file server, which can be executed on your PC and will create a share of a selected directory of media files, which can then be accessed on every other device in your network using a (modern) web browser.

## Features

- Support of common image and video formats (like JPEG, PNG, GIF, WEBP, MP4, WEBM,...)
- Easy setup of one or more accounts with different shared folders
- Fully responsive web interface 
- Supports Windows and Linux (using mono)
- Direct navigation in the media file viewer
  Click left or right (besides the media element) to view the previous or next element, close the media viewer by clicking on the header bar

## Project state

This project was initially written without any additional dependencies or libraries. While this was a good practice in "bare" HTML, CSS and JS, it also limits the growth of the application into something more sophisticated. For this reason, the next version of Vistava will be rewritten in ASP.Net Core and React - with lots of new features, like a "moodboard" view, file collections and much more. So... stay tuned!