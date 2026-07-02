; *************************************************************
; Datei:        KirchenbirkigerSchluck.iss
; Zweck:        Inno-Setup-Skript zur Erzeugung des Windows-Installers
; Bereich:      Betrieb - Auslieferung
; Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
; Urheberrecht: Copyright (c) 2026
; *************************************************************
;
; Voraussetzung: Zuvor muss "dotnet publish" den Ordner ..\publish erzeugt haben
; (self-contained, win-x64). Siehe veroeffentlichen.ps1.
;
; Der Installer laeuft OHNE Administratorrechte (per-user) und installiert nach
; %LOCALAPPDATA%\Programs\KirchenbirkigerSchluck. Die Turnierdaten selbst liegen
; getrennt davon in %LOCALAPPDATA%\KirchenbirkigerSchluck (von der App verwaltet)
; und bleiben bei einer Deinstallation erhalten.

#define MeinName "Kirchenbirkiger Schluck"
#define MeinVersion "1.0.0"
#define MeinAutor "Kirchenbirkiger Schluck Entwicklungsteam"
#define MeinExe "KirchenbirkigerSchluck.App.exe"

[Setup]
AppId={{8F3B2C1A-4D6E-4A7B-9C2D-1E5F7A9B3C4D}
AppName={#MeinName}
AppVersion={#MeinVersion}
AppPublisher={#MeinAutor}
DefaultDirName={localappdata}\Programs\KirchenbirkigerSchluck
DefaultGroupName={#MeinName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=KirchenbirkigerSchluck_Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\KirchenbirkigerSchluck.App\Resources\Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#MeinExe}
UninstallDisplayName={#MeinName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
; Gesamten Publish-Ordner rekursiv uebernehmen (EXE + .NET-Runtime + Ressourcen)
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#MeinName}"; Filename: "{app}\{#MeinExe}"
Name: "{group}\{cm:UninstallProgram,{#MeinName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MeinName}"; Filename: "{app}\{#MeinExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MeinExe}"; Description: "{cm:LaunchProgram,{#MeinName}}"; Flags: nowait postinstall skipifsilent
