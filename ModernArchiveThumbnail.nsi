Unicode true
!include "MUI2.nsh"
!include "x64.nsh"
!include "LogicLib.nsh"

!define PRODUCT_NAME "ModernArchiveThumbnail"
!define NEW_GUID "{1330CB22-9D87-4DA5-A852-9CADD6634708}"
!define OLD_GUID "{9E8F7ABC-1234-4D22-9AA0-123456789111}"
!define THUMB_HANDLER_GUID "{e357fccd-a995-4576-b01f-234630154e96}"

Name "${PRODUCT_NAME}"
OutFile "ModernArchive_Setup_v1.1.2_Release.exe"
InstallDir "$PROGRAMFILES64\${PRODUCT_NAME}"
RequestExecutionLevel admin

!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!insertmacro MUI_PAGE_LICENSE "license.txt"
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Korean"

!macro BackupExistingHandler EXT
    ReadRegStr $0 HKCR ".${EXT}\ShellEx\${THUMB_HANDLER_GUID}" ""
    ${If} $0 != ""
    ${AndIf} $0 != "${NEW_GUID}"
    ${AndIf} $0 != "${OLD_GUID}"
        WriteRegStr HKLM "SOFTWARE\${PRODUCT_NAME}\Backup\HKCR" ".${EXT}" "$0"
    ${EndIf}
    
    ReadRegStr $1 HKLM "SOFTWARE\Classes\SystemFileAssociations\.${EXT}\ShellEx\${THUMB_HANDLER_GUID}" ""
    ${If} $1 != ""
    ${AndIf} $1 != "${NEW_GUID}"
    ${AndIf} $1 != "${OLD_GUID}"
        WriteRegStr HKLM "SOFTWARE\${PRODUCT_NAME}\Backup\SystemFile" ".${EXT}" "$1"
    ${EndIf}
!macroend

!macro CoexistRegister EXT TYPE
    WriteRegStr HKLM "SOFTWARE\Classes\SystemFileAssociations\.${EXT}\ShellEx\${THUMB_HANDLER_GUID}" "" "${NEW_GUID}"
    
    ReadRegStr $2 HKLM "SOFTWARE\${PRODUCT_NAME}\Backup\HKCR" ".${EXT}"
    ${If} $2 != ""
        WriteRegStr HKCR ".${EXT}\ShellEx\${THUMB_HANDLER_GUID}" "" "$2"
    ${Else}
        WriteRegStr HKCR ".${EXT}\ShellEx\${THUMB_HANDLER_GUID}" "" "${NEW_GUID}"
    ${EndIf}

    ReadRegStr $3 HKLM "SOFTWARE\Classes\.${EXT}" "PerceivedType"
    ${If} $3 == ""
        WriteRegStr HKLM "SOFTWARE\Classes\.${EXT}" "PerceivedType" "${TYPE}"
    ${EndIf}
!macroend

!macro RegisterDynamicProgID EXT
    ReadRegStr $0 HKCR ".${EXT}" ""
    ${If} $0 != ""
        WriteRegStr HKLM "SOFTWARE\Classes\$0\ShellEx\${THUMB_HANDLER_GUID}" "" "${NEW_GUID}"
        WriteRegStr HKCR "$0\ShellEx\${THUMB_HANDLER_GUID}" "" "${NEW_GUID}"
    ${EndIf}
!macroend

!macro SetFallback EXT
    ReadRegStr $3 HKLM "SOFTWARE\${PRODUCT_NAME}\Backup\HKCR" ".${EXT}"
    ${If} $3 != ""
        WriteRegStr HKLM "SOFTWARE\Classes\CLSID\${NEW_GUID}\TreatAs" "" "$3"
    ${EndIf}
!macroend

!macro RemoveSystemFileHandler EXT
    DeleteRegKey HKLM "SOFTWARE\Classes\SystemFileAssociations\.${EXT}\ShellEx\${THUMB_HANDLER_GUID}"
!macroend

!macro UnregisterDynamicProgID EXT
    ReadRegStr $0 HKCR ".${EXT}" ""
    ${If} $0 != ""
        DeleteRegKey HKLM "SOFTWARE\Classes\$0\ShellEx\${THUMB_HANDLER_GUID}"
        DeleteRegKey HKCR "$0\ShellEx\${THUMB_HANDLER_GUID}"
    ${EndIf}
!macroend

Section "Install"
    ${If} ${RunningX64}
        ${DisableX64FSRedirection}
    ${EndIf}

    nsExec::Exec 'taskkill /f /im explorer.exe'
    nsExec::Exec 'taskkill /f /im dllhost.exe'
    nsExec::Exec 'taskkill /f /im ShellExperienceHost.exe'
    Sleep 2000

    SetRegView 64
    DeleteRegKey HKCR "CLSID\${OLD_GUID}"
    DeleteRegKey HKLM "SOFTWARE\Classes\CLSID\${OLD_GUID}"
    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${OLD_GUID}"
    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Cached" "${OLD_GUID}"
    SetRegView 32
    DeleteRegKey HKCR "CLSID\${OLD_GUID}"
    DeleteRegKey HKLM "SOFTWARE\Classes\CLSID\${OLD_GUID}"
    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${OLD_GUID}"
    SetRegView 64
    DeleteRegKey HKCU "Software\Classes\CLSID\${OLD_GUID}"
    DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Shell Extensions\Cached" "${OLD_GUID}"
    SetRegView 64

    SetOutPath "$TEMP"
    File "VC_redist.x64.exe" 
    DetailPrint "필수 시스템 구성 요소(C++ Runtime) 확인 및 설치 중..."
    ExecWait '"$TEMP\VC_redist.x64.exe" /install /quiet /norestart'
    Delete "$TEMP\VC_redist.x64.exe"

    !insertmacro BackupExistingHandler "zip"
    !insertmacro BackupExistingHandler "rar"
    !insertmacro BackupExistingHandler "7z"
    !insertmacro BackupExistingHandler "alz"
    !insertmacro BackupExistingHandler "egg"
    !insertmacro BackupExistingHandler "tar"
    !insertmacro BackupExistingHandler "gz"
    !insertmacro BackupExistingHandler "cb7"
    !insertmacro BackupExistingHandler "cbr"
    !insertmacro BackupExistingHandler "cbz"
    !insertmacro BackupExistingHandler "lzh"

    SetOutPath "$INSTDIR"
    File "*.dll"
    File "license.txt"
    File "credits.txt"
    
    nsExec::Exec 'powershell.exe -NoProfile -Command "Unblock-File -Path ''$INSTDIR\*.dll''"'

    IfFileExists "$INSTDIR\ModernArchiveThumbnail_old.dll" 0 +2
        ExecWait 'regsvr32.exe /u /s "$INSTDIR\ModernArchiveThumbnail_old.dll"'

    ExecWait 'regsvr32.exe /s "$INSTDIR\ModernArchiveThumbnail.dll"'

    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${NEW_GUID}" "ModernArchive Thumbnail Provider"
    WriteRegStr HKLM "SOFTWARE\Classes\CLSID\${NEW_GUID}" "" "ModernArchive Thumbnail Provider"
    WriteRegDWORD HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Cached" "${NEW_GUID}" 1

    !insertmacro CoexistRegister "zip" "compressed"
    !insertmacro CoexistRegister "rar" "compressed"
    !insertmacro CoexistRegister "7z"  "compressed"
    !insertmacro CoexistRegister "alz" "compressed"
    !insertmacro CoexistRegister "egg" "compressed"
    !insertmacro CoexistRegister "tar" "compressed"
    !insertmacro CoexistRegister "gz"  "compressed"
    !insertmacro CoexistRegister "cb7" "compressed"
    !insertmacro CoexistRegister "cbr" "compressed"
    !insertmacro CoexistRegister "cbz" "compressed"
    !insertmacro CoexistRegister "lzh" "compressed"

    !insertmacro RegisterDynamicProgID "zip"
    !insertmacro RegisterDynamicProgID "rar"
    !insertmacro RegisterDynamicProgID "7z"
    !insertmacro RegisterDynamicProgID "alz"
    !insertmacro RegisterDynamicProgID "egg"
    !insertmacro RegisterDynamicProgID "tar"
    !insertmacro RegisterDynamicProgID "gz"
    !insertmacro RegisterDynamicProgID "cb7"
    !insertmacro RegisterDynamicProgID "cbr"
    !insertmacro RegisterDynamicProgID "cbz"
    !insertmacro RegisterDynamicProgID "lzh"

    !insertmacro SetFallback "zip"

    nsExec::Exec 'cmd.exe /c "del /f /s /q %LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db"'
    nsExec::Exec 'cmd.exe /c "del /f /s /q %LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db"'
    nsExec::Exec 'cmd.exe /c "del /f /q %LOCALAPPDATA%\IconCache.db"'

    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked" "${NEW_GUID}"
    DeleteRegValue HKCU "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Blocked" "${NEW_GUID}"

    WriteUninstaller "$INSTDIR\uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayName" "${PRODUCT_NAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "QuietUninstallString" "$\"$INSTDIR\uninstall.exe$\" /S"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayIcon" "$INSTDIR\ModernArchiveThumbnail.dll"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "Publisher" "ModernArchive Project"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayVersion" "1.1.2"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "NoModify" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "NoRepair" 1

    System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'
    Sleep 500
    System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'
    Exec 'explorer.exe'

    ${If} ${RunningX64}
        ${EnableX64FSRedirection}
    ${EndIf}
SectionEnd

Section "Uninstall"
    ${If} ${RunningX64}
        ${DisableX64FSRedirection}
    ${EndIf}

    nsExec::Exec 'taskkill /f /im explorer.exe'
    Sleep 2000

    ExecWait 'regsvr32.exe /u /s "$INSTDIR\ModernArchiveThumbnail.dll"'

    DeleteRegKey HKCR "CLSID\${NEW_GUID}"
    DeleteRegKey HKLM "SOFTWARE\Classes\CLSID\${NEW_GUID}"
    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${NEW_GUID}"
    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Cached" "${NEW_GUID}"

    !insertmacro RemoveSystemFileHandler "zip"
    !insertmacro RemoveSystemFileHandler "rar"
    !insertmacro RemoveSystemFileHandler "7z"
    !insertmacro RemoveSystemFileHandler "alz"
    !insertmacro RemoveSystemFileHandler "egg"
    !insertmacro RemoveSystemFileHandler "tar"
    !insertmacro RemoveSystemFileHandler "gz"
    !insertmacro RemoveSystemFileHandler "cb7"
    !insertmacro RemoveSystemFileHandler "cbr"
    !insertmacro RemoveSystemFileHandler "cbz"
    !insertmacro RemoveSystemFileHandler "lzh"

    !insertmacro UnregisterDynamicProgID "zip"
    !insertmacro UnregisterDynamicProgID "rar"
    !insertmacro UnregisterDynamicProgID "7z"
    !insertmacro UnregisterDynamicProgID "alz"
    !insertmacro UnregisterDynamicProgID "egg"
    !insertmacro UnregisterDynamicProgID "tar"
    !insertmacro UnregisterDynamicProgID "gz"
    !insertmacro UnregisterDynamicProgID "cb7"
    !insertmacro UnregisterDynamicProgID "cbr"
    !insertmacro UnregisterDynamicProgID "cbz"
    !insertmacro UnregisterDynamicProgID "lzh"

    DeleteRegKey HKLM "SOFTWARE\${PRODUCT_NAME}"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
    
    Delete "$INSTDIR\*.dll"
    Delete "$INSTDIR\license.txt"
    Delete "$INSTDIR\credits.txt"
    Delete "$INSTDIR\uninstall.exe"
    RMDir "$INSTDIR"

    nsExec::Exec 'cmd.exe /c "del /f /s /q %LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db"'
    nsExec::Exec 'cmd.exe /c "del /f /s /q %LOCALAPPDATA%\Microsoft\Windows\Explorer\iconcache_*.db"'

    System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'
    Exec 'explorer.exe'

    ${If} ${RunningX64}
        ${EnableX64FSRedirection}
    ${EndIf}
SectionEnd