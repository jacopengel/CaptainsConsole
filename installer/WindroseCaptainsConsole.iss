#define MyAppName "Windrose Captain's Console"
#ifndef MyAppVersion
  #define MyAppVersion "0.9.5"
#endif
#ifndef MyAppPublisher
  #define MyAppPublisher "Kliphard"
#endif
#ifndef MyAppExeName
  #define MyAppExeName "WindroseCaptainsConsole.exe"
#endif
#ifndef MyAppSourceDir
  #define MyAppSourceDir "..\dist\installer-stage"
#endif
#ifndef MyOutputBaseFilename
  #define MyOutputBaseFilename "WindroseCaptainsConsoleSetup"
#endif
#ifndef MySetupIconFile
  #define MySetupIconFile "..\dist\installer-assets\setup-icon.ico"
#endif
#ifndef MyWizardImageFile
  #define MyWizardImageFile "..\dist\installer-assets\wizard-image.bmp"
#endif
#ifndef MyWizardSmallImageFile
  #define MyWizardSmallImageFile "..\dist\installer-assets\wizard-small.bmp"
#endif

[Setup]
AppId={{B0F3E9C8-8E30-4EB6-A1E8-8F30CE46D9FD}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Windrose Captain's Console
DefaultGroupName=Windrose Captain's Console
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename={#MyOutputBaseFilename}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile={#MySetupIconFile}
WizardImageFile={#MyWizardImageFile}
WizardSmallImageFile={#MyWizardSmallImageFile}
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
RestartApplications=no
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Windrose Captain's Console Installer
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoCopyright=Copyright (c) Kliphard

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSourceDir}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSourceDir}\update-feed-url.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Windrose Captain's Console"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Windrose Captain's Console"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Windrose Captain's Console"; Flags: nowait postinstall skipifsilent
