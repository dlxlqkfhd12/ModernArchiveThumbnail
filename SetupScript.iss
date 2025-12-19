[Setup]
AppName=Modern Archive Thumbnail Handler
AppVersion=2.0
AppPublisher=dlxlqkfhd12
AppId={{9E8F7ABC-1234-4D22-9AA0-123456789111}}
DefaultDirName={commonpf64}\ModernArchiveThumbnail
DefaultGroupName=Modern Archive Thumbnail Handler
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
OutputDir=.
OutputBaseFilename=ModernArchiveThumbnail_Setup
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=admin
CloseApplications=force
RestartApplications=no
UninstallDisplayIcon={app}\Settings.exe
; ▼ [수정됨] 잘못된 명령어 삭제하고 MinVersion만 남김
MinVersion=10.0

[Files]
Source: "C:\Users\pc\Desktop\ModernArchiveThumbnail\Settings\Settings.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\pc\Desktop\ModernArchiveThumbnail\bin\Release\net48\*.*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs; Excludes: "*.pdb,*.xml"

[Icons]
Name: "{group}\Settings"; Filename: "{app}\Settings.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
var ResultCode: Integer;

function InitializeSetup(): Boolean;
var ErrorCode: Integer;
    ReleaseValue: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', ReleaseValue) then
    if ReleaseValue >= 528040 then
      Result := True;
  if not Result then
    if MsgBox('.NET Framework 4.8+ required. Download now?', mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://go.microsoft.com/fwlink/?linkid=2088631', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    Exec('C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe',
         '/codebase "' + ExpandConstant('{app}\ModernArchiveThumbnailHandler.dll') + '"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec('C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ngen.exe',
         'install "' + ExpandConstant('{app}\ModernArchiveThumbnailHandler.dll') + '" /nologo',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM dllhost.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM explorer.exe & del /f /q "' + ExpandConstant('{localappdata}\Microsoft\Windows\Explorer\thumbcache_*.db') + '" & start explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    ShellExec('', ExpandConstant('{app}\Settings.exe'), '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\ModernArchiveThumbnail');
  if CurUninstallStep = usPostUninstall then
  begin
    Exec('C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe',
         '/unregister "' + ExpandConstant('{app}\ModernArchiveThumbnailHandler.dll') + '"',
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM dllhost.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM explorer.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    if DirExists(ExpandConstant('{app}')) then
      DelTree(ExpandConstant('{app}'), True, True, True);
    Exec(ExpandConstant('{cmd}'), '/C del /f /q "' + ExpandConstant('{localappdata}\Microsoft\Windows\Explorer\thumbcache_*.db') + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ExpandConstant('{cmd}'), '/C start explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    MsgBox('Uninstallation complete. All files deleted, cache cleared, and Explorer restarted.', mbInformation, MB_OK);
  end;
end;