Unicode true
!include "MUI2.nsh"
!include "x64.nsh"

!define PRODUCT_NAME "ModernArchiveThumbnail"
!define PRODUCT_VERSION "1.1.2"
!define GUID "{9E8F7ABC-1234-4D22-9AA0-123456789111}"
!define THUMB "{E357FCCD-A995-4576-B01F-234630154E96}"
!define UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"

Name "${PRODUCT_NAME}"
OutFile "ModernArchiveThumbnail_Ultimate_v${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES64\${PRODUCT_NAME}"
RequestExecutionLevel admin

VIProductVersion "${PRODUCT_VERSION}.0"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "FileDescription" "High Quality Archive Thumbnail Provider"
VIAddVersionKey "FileVersion" "${PRODUCT_VERSION}"
VIAddVersionKey "ProductVersion" "${PRODUCT_VERSION}"
VIAddVersionKey "LegalCopyright" "Copyright (c) 2025 Modern Library."

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_LANGUAGE "Korean"

Section "Install"
    ${If} ${RunningX64}
        ${DisableX64FSRedirection}
    ${EndIf}

    nsExec::Exec 'taskkill /f /im explorer.exe'
    nsExec::Exec 'taskkill /f /im dllhost.exe'
    Sleep 1500

    SetOutPath "$INSTDIR"
    File "ModernArchiveThumbnail.dll"
    File "archive.dll"
    File "bz2.dll"
    File "libcrypto-3-x64.dll"
    File "liblzma.dll"
    File "lz4.dll"
    File "zlib1.dll"
    File "zstd.dll"
    File "LICENSE.txt"

    ExecWait 'regsvr32.exe /s "$INSTDIR\ModernArchiveThumbnail.dll"'

    WriteRegStr HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${GUID}" "${PRODUCT_NAME}"

    !macro KillAndTakeOver EXT
        DeleteRegKey HKCR "SystemFileAssociations\.${EXT}\ShellEx\${THUMB}"
        DeleteRegKey HKCU "Software\Classes\.${EXT}\ShellEx\${THUMB}"
        DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.${EXT}\UserChoice"
        
        WriteRegStr HKCR "SystemFileAssociations\.${EXT}\ShellEx\${THUMB}" "" "${GUID}"
        WriteRegStr HKLM "SOFTWARE\Classes\.${EXT}\ShellEx\${THUMB}" "" "${GUID}"
    !macroend

    !insertmacro KillAndTakeOver "zip"
    !insertmacro KillAndTakeOver "rar"
    !insertmacro KillAndTakeOver "7z"
    !insertmacro KillAndTakeOver "cbz"
    !insertmacro KillAndTakeOver "cbr"
    !insertmacro KillAndTakeOver "cb7"
    !insertmacro KillAndTakeOver "alz"
    !insertmacro KillAndTakeOver "egg"
    !insertmacro KillAndTakeOver "tar"
    !insertmacro KillAndTakeOver "gz"
    !insertmacro KillAndTakeOver "lzh"

    nsExec::Exec 'cmd.exe /c del /f /s /q "$LOCALAPPDATA\Microsoft\Windows\Explorer\thumbcache_*.db"'
    System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'

    WriteUninstaller "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "${UNINST_KEY}" "DisplayName" "${PRODUCT_NAME}"
    WriteRegStr HKLM "${UNINST_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
    WriteRegStr HKLM "${UNINST_KEY}" "DisplayIcon" "$INSTDIR\ModernArchiveThumbnail.dll"

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
    nsExec::Exec 'taskkill /f /im dllhost.exe'
    nsExec::Exec 'taskkill /f /im prevhost.exe'
    Sleep 2000

    ExecWait 'regsvr32.exe /u /s "$INSTDIR\ModernArchiveThumbnail.dll"'
    
    !macro CleanUp EXT
        DeleteRegKey HKCR "SystemFileAssociations\.${EXT}\ShellEx\${THUMB}"
        DeleteRegKey HKLM "SOFTWARE\Classes\.${EXT}\ShellEx\${THUMB}"
        DeleteRegKey HKCU "Software\Classes\.${EXT}\ShellEx\${THUMB}"
    !macroend

    !insertmacro CleanUp "zip"
    !insertmacro CleanUp "rar"
    !insertmacro CleanUp "7z"
    !insertmacro CleanUp "cbz"
    !insertmacro CleanUp "cbr"
    !insertmacro CleanUp "cb7"
    !insertmacro CleanUp "alz"
    !insertmacro CleanUp "egg"
    !insertmacro CleanUp "tar"
    !insertmacro CleanUp "gz"
    !insertmacro CleanUp "lzh"

    DeleteRegValue HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" "${GUID}"
    DeleteRegKey HKLM "${UNINST_KEY}"

    Delete /REBOOTOK "$INSTDIR\ModernArchiveThumbnail.dll"
    Delete /REBOOTOK "$INSTDIR\archive.dll"
    Delete /REBOOTOK "$INSTDIR\bz2.dll"
    Delete /REBOOTOK "$INSTDIR\libcrypto-3-x64.dll"
    Delete /REBOOTOK "$INSTDIR\liblzma.dll"
    Delete /REBOOTOK "$INSTDIR\lz4.dll"
    Delete /REBOOTOK "$INSTDIR\zlib1.dll"
    Delete /REBOOTOK "$INSTDIR\zstd.dll"
    Delete /REBOOTOK "$INSTDIR\LICENSE.txt"
    Delete /REBOOTOK "$INSTDIR\Uninstall.exe"

    RMDir /r /REBOOTOK "$INSTDIR"
    
    nsExec::Exec 'cmd.exe /c del /f /s /q "$LOCALAPPDATA\Microsoft\Windows\Explorer\thumbcache_*.db"'
    System::Call 'shell32::SHChangeNotify(i 0x08000000, i 0, i 0, i 0)'
    
    Exec 'explorer.exe'
SectionEnd