!macro customInstall
  DetailPrint "Creating context menu actions..."
  
  WriteRegStr HKCR "Directory\shell\Vistava" "" "Open in Vistava"
  WriteRegExpandStr HKCR "Directory\shell\Vistava" "Icon" '"$INSTDIR\Vistava.exe"'
  WriteRegExpandStr HKCR "Directory\shell\Vistava\command" "" '"$INSTDIR\Vistava.exe" "%V"'

  WriteRegStr HKCR "Directory\Background\shell\Vistava" "" "Open in Vistava"
  WriteRegExpandStr HKCR "Directory\Background\shell\Vistava" "Icon" '"$INSTDIR\Vistava.exe"'
  WriteRegExpandStr HKCR "Directory\Background\shell\Vistava\command" "" '"$INSTDIR\Vistava.exe" "%V"'

  DetailPrint "Downloading .NET Core Runtime 8..."
  inetc::get https://aka.ms/dotnetcore-8-0-windowshosting $TEMP/dotnet-hosting-win-x64.exe /end
  Pop $0
  StrCmp $0 "OK" execFile
  MessageBox MB_OK|MB_ICONEXCLAMATION "The .NET Hosting Bundle couldn't be downloaded. The application might not start correctly without that dependency." /SD IDOK
  Goto done

execFile:
   DetailPrint "Installing .NET Core Runtime 8..."
   ExecWait '"$TEMP/dotnet-hosting-win-x64.exe" /passive'

done:
!macroend

!macro customWelcomePage
  !insertMacro MUI_PAGE_WELCOME
!macroend

!macro customUnInstall
  DeleteRegKey HKCR "Directory\Background\shell\Vistava"
  DeleteRegKey HKCR "Directory\shell\Vistava"
!macroend