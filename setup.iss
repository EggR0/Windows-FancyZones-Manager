[Setup]
AppName=FancyZonesHotkeys
AppVersion=2.0.0
AppPublisher=FancyZonesHotkeys
DefaultDirName={autopf}\FancyZonesHotkeys
DefaultGroupName=FancyZonesHotkeys
OutputDir=dist
OutputBaseFilename=FancyZonesHotkeys_Setup_v2.0.0
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\FancyZonesHotkeys.exe
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "dist\FancyZonesHotkeys.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\presets.yaml"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist onlyifdoesntexist
Source: "legacy\en\QUICKSTART.txt"; DestDir: "{app}\en"; Flags: ignoreversion skipifsourcedoesntexist
Source: "legacy\en\README.md"; DestDir: "{app}\en"; Flags: ignoreversion skipifsourcedoesntexist
Source: "legacy\ko\QUICKSTART.txt"; DestDir: "{app}\ko"; Flags: ignoreversion skipifsourcedoesntexist
Source: "legacy\ko\README.md"; DestDir: "{app}\ko"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
Name: "{group}\FancyZonesHotkeys"; Filename: "{app}\FancyZonesHotkeys.exe"
Name: "{group}\Uninstall FancyZonesHotkeys"; Filename: "{uninstallexe}"

[Registry]
; Automatically register to startup on install, remove on uninstall
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "FancyZonesHotkeys"; ValueData: """{app}\FancyZonesHotkeys.exe"""; Flags: uninsdeletevalue
