@echo off

echo Checking ffmpeg/ffprobe...
timeout 1 > NUL

WHERE ffmpeg >nul 2>nul
IF %ERRORLEVEL% NEQ 0 goto not_found

WHERE ffprobe >nul 2>nul
IF %ERRORLEVEL% NEQ 0 goto not_found

echo ffmpeg/ffprobe found in path!
goto end

:not_found
echo ffmpeg/ffprobe not found!
echo:
echo Installing via winget...
timeout 1 > NUL
winget install ffmpeg --accept-package-agreements --accept-source-agreements
goto end

:end
timeout 2 > NUL