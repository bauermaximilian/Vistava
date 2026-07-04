!macro customInstall
  WriteRegStr HKCR "Directory\shell\Vistava" "" "Open in Vistava"
  WriteRegExpandStr HKCR "Directory\shell\Vistava" "Icon" '"$INSTDIR\Vistava.exe"'
  WriteRegExpandStr HKCR "Directory\shell\Vistava\command" "" '"$INSTDIR\Vistava.exe" "%V"'

  WriteRegStr HKCR "Directory\Background\shell\Vistava" "" "Open in Vistava"
  WriteRegExpandStr HKCR "Directory\Background\shell\Vistava" "Icon" '"$INSTDIR\Vistava.exe"'
  WriteRegExpandStr HKCR "Directory\Background\shell\Vistava\command" "" '"$INSTDIR\Vistava.exe" "%V"'

  ExpandEnvStrings $0 %COMSPEC%
  ExecWait '"$0" /C "$INSTDIR\resources\app\bin\win\install-ffmpeg.cmd"'
!macroend

!macro customWelcomePage
  !insertMacro MUI_PAGE_WELCOME
!macroend

!macro customUnInstall
  DeleteRegKey HKCR "Directory\Background\shell\Vistava"
  DeleteRegKey HKCR "Directory\shell\Vistava"
!macroend