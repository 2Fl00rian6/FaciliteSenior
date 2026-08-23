#ifndef AppName
  #define AppName "FaciliteSenior"
#endif

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef AppPublisher
  #define AppPublisher "Florian Warther"
#endif

#ifndef AppExeName
  #define AppExeName "FaciliteSenior.exe"
#endif

#ifndef SourceDir
  #define SourceDir "..\artifacts\publish\win-x64"
#endif

#ifndef OutputDir
  #define OutputDir "..\artifacts\installer"
#endif

[Setup]
AppId={{8B26789E-1173-4C70-B340-7F7AFD8E15B8}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\FaciliteSenior
DisableDirPage=yes
DisableProgramGroupPage=yes
UsePreviousAppDir=yes
UsePreviousTasks=yes
UninstallDisplayIcon={app}\{#AppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
WizardResizable=no

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Creer un raccourci sur le Bureau"; Flags: unchecked
Name: "autostart"; Description: "Ouvrir automatiquement avec Windows"; Flags: checkedonce

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
#ifdef IncludeWebView2Bootstrapper
Source: "{#WebView2BootstrapperPath}"; DestDir: "{tmp}"; DestName: "MicrosoftEdgeWebview2Setup.exe"; Flags: deleteafterinstall
#endif

[Icons]
Name: "{autoprograms}\FaciliteSenior"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\FaciliteSenior"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "FaciliteSenior"; ValueData: """{app}\{#AppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
#ifdef IncludeWebView2Bootstrapper
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Installation du composant WebView2..."; Flags: waituntilterminated; Check: NeedsWebView2Runtime
#endif
Filename: "{app}\{#AppExeName}"; Description: "Ouvrir FaciliteSenior maintenant"; Flags: nowait postinstall skipifsilent

[Code]
function HasValidWebView2Version(const VersionText: string): Boolean;
begin
  Result := (Trim(VersionText) <> '') and (Trim(VersionText) <> '0.0.0.0');
end;

function IsWebView2RuntimeInstalled: Boolean;
var
  VersionText: string;
begin
  Result := False;

  if RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', VersionText) and HasValidWebView2Version(VersionText) then
  begin
    Result := True;
    exit;
  end;

  if RegQueryStringValue(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', VersionText) and HasValidWebView2Version(VersionText) then
  begin
    Result := True;
    exit;
  end;
end;

function NeedsWebView2Runtime: Boolean;
begin
  Result := not IsWebView2RuntimeInstalled;
end;

procedure InitializeWizard;
begin
#ifndef IncludeWebView2Bootstrapper
  if not IsWebView2RuntimeInstalled then
  begin
    SuppressibleMsgBox(
      'Le composant Microsoft WebView2 n''a pas ete detecte.' + #13#10 + #13#10 +
      'L''application a besoin de ce composant pour afficher les sites dans la fenetre integree.' + #13#10 +
      'Installez WebView2 puis relancez FaciliteSenior.',
      mbInformation,
      MB_OK,
      IDOK);
  end;
#endif
end;

