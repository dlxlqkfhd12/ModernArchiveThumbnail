[Setup]
AppName=Modern Archive Thumbnail Handler
AppVersion=1.0
AppPublisher=dlxlqkfhd12
AppId={{9E8F7ABC-1234-4D22-9AA0-123456789111}

DefaultDirName={commonpf64}\ModernArchiveThumbnail
DefaultGroupName=ModernArchiveThumbnail

ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

OutputDir=.
OutputBaseFilename=ModernArchiveThumbnail_Setup
Compression=lzma2/max
SolidCompression=yes
PrivilegesRequired=admin

CloseApplications=force
RestartApplications=yes

[Files]
Source: "bin\Release\net48\ModernArchiveThumbnail.dll"; DestDir: "{app}"; Flags: ignoreversion restartreplace
Source: "bin\Release\net48\SharpShell.dll"; DestDir: "{app}"; Flags: ignoreversion restartreplace
Source: "CREDITS.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "Settings.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Settings"; Filename: "{app}\Settings.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"

[Run]
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ModernArchiveThumbnail.dll"""; Flags: runhidden
Filename: "{app}\Settings.exe"; Description: "설정 및 모드 변경하기 (Open Settings)"; Flags: postinstall nowait shellexec skipifsilent

[UninstallRun]
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/unregister ""{app}\ModernArchiveThumbnail.dll"""; Flags: runhidden; RunOnceId: "UnregASM"

[Code]
var
  ResultCode: Integer;

function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  ReleaseValue: Cardinal;
  IsNet48Installed: Boolean;
begin
  IsNet48Installed := False;

  if RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', ReleaseValue) then
  begin
    if ReleaseValue >= 528040 then
    begin
      IsNet48Installed := True;
    end;
  end;

  if IsNet48Installed then
  begin
    Result := True;
  end
  else
  begin
    if MsgBox('This application requires .NET Framework 4.8 or later.' #13#13 'Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://go.microsoft.com/fwlink/?linkid=2088631', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
    Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Program uninstalled.' #13#13 'Do you want to clear the Thumbnail Cache?' #13 '(Screen will flicker)', mbConfirmation, MB_YESNO) = IDYES then
    begin
      Exec('taskkill', '/F /IM explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      Exec('taskkill', '/F /IM dllhost.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      Exec(ExpandConstant('{cmd}'), '/C del /f /q /a "' + ExpandConstant('{localappdata}\Microsoft\Windows\Explorer\thumbcache_*.db') + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      Exec(ExpandConstant('{cmd}'), '/C start explorer.exe', '', SW_HIDE, ewNoWait, ResultCode);
      MsgBox('Cache cleared.', mbInformation, MB_OK);
    end;
  end;
end;