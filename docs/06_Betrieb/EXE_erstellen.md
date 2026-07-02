# EXE / Installer erstellen

Dieses Dokument beschreibt, wie aus dem Quellcode eine **fertige `Setup.exe`** wird, mit der sich
**Kirchenbirkiger Schluck** per Doppelklick installieren und anschließend über eine Verknüpfung
starten lässt.

## Überblick

- **Auslieferungsform:** klassischer Windows-Installer (`Setup.exe`, erstellt mit Inno Setup).
- **Self-contained:** Auf dem Ziel-PC muss **kein .NET installiert** sein – die Laufzeit ist enthalten.
- **Ohne Adminrechte:** Der Installer installiert pro Benutzer nach
  `%LOCALAPPDATA%\Programs\KirchenbirkigerSchluck` (kein UAC-Dialog).
- **Feste Datenablage:** Alle Turnierdaten liegen getrennt vom Programm unter
  `%LOCALAPPDATA%\KirchenbirkigerSchluck` – konkret:
  - `turnier.json` – aktueller Spielstand
  - `backups/` – automatische Sicherungen
  - `logos/` – hochgeladene Team-Logos

  Dieser Datenordner bleibt bei einer **Deinstallation erhalten**.

## Voraussetzungen (nur auf dem Build-PC)

1. **.NET SDK** (bereits vorhanden).
2. **Inno Setup** – einmalig installieren:
   ```powershell
   winget install --id JRSoftware.InnoSetup -e
   ```

## Bauen

Im Projekt-Root in PowerShell:

```powershell
.\veroeffentlichen.ps1
```

Das Skript führt zwei Schritte aus:

1. `dotnet publish` (self-contained, `win-x64`) → Ordner `.\publish`
2. Inno Setup → `.\installer\Output\KirchenbirkigerSchluck_Setup.exe`

Ist Inno Setup noch nicht installiert, meldet das Skript den `winget`-Befehl und bricht nach dem
Publish-Schritt sauber ab. Nach der Installation von Inno Setup das Skript erneut ausführen.

## Weitergeben & Installieren

- Die Datei `KirchenbirkigerSchluck_Setup.exe` an den Ziel-PC kopieren und doppelklicken.
- **Windows SmartScreen** kann bei der (nicht signierten) Datei eine Warnung zeigen:
  „Weitere Informationen“ → „Trotzdem ausführen“.
- Nach der Installation startet die Anwendung über die **Desktop-** oder **Startmenü-Verknüpfung**.

## Neue Version veröffentlichen

Bei Änderungen die Version an **zwei** Stellen gemeinsam erhöhen:

- `src/KirchenbirkigerSchluck.App/KirchenbirkigerSchluck.App.csproj` → `<Version>`
- `installer/KirchenbirkigerSchluck.iss` → `#define MeinVersion`

Danach erneut `.\veroeffentlichen.ps1` ausführen.
