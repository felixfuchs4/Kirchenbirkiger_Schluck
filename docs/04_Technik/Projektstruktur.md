# Projektstruktur

Dieses Dokument beschreibt die Solution-Struktur, Schichtentrennung, Namenskonventionen und den Build-Prozess für das Projekt **Kirchenbirkiger Schluck Programm**.

## 1. Gesamtübersicht

Das Projekt folgt einer 3-Schichten-Architektur und ist als .NET-Solution mit vier Projekten organisiert.

```
KirchenbirkigerSchluck.sln
├── src/
│   ├── KirchenbirkigerSchluck.App/       ← Präsentation (WPF)
│   ├── KirchenbirkigerSchluck.Core/      ← Anwendungslogik
│   ├── KirchenbirkigerSchluck.Data/      ← Datenhaltung
│   └── KirchenbirkigerSchluck.Tests/     ← Unit-Tests
├── docs/                                  ← Dokumentation
├── assets/                                ← Grafiken, Logo, Mockups
└── quellen/                               ← Quelldateien (alte Excel, Einladungen)
```

## 2. Präsentationsschicht – `KirchenbirkigerSchluck.App`

WPF-Anwendungsprojekt. Enthält alle Fenster, Views, ViewModels und UI-Ressourcen.

```
App/
├── App.xaml                               ← Globale Ressourcen, Theme-Einbindung
├── App.xaml.cs                            ← Einstiegspunkt, DI-Container-Setup
├── Windows/
│   ├── BedienWindow.xaml                  ← Hauptfenster für die Turnierleitung (Laptop)
│   ├── BedienWindow.xaml.cs
│   ├── AnzeigeWindow.xaml                 ← Vollbild-Fenster für Zuschaueranzeige (externer Bildschirm)
│   └── AnzeigeWindow.xaml.cs
├── Views/
│   ├── Bedienung/
│   │   ├── TurnierAnlegenView.xaml        ← Turnier erstellen / laden
│   │   ├── SpielplanView.xaml             ← Spielplan, Reihenfolge anpassen
│   │   ├── SpielsteuerungView.xaml        ← Laufendes Spiel steuern, Treffer erfassen
│   │   ├── KorrekturView.xaml             ← Nachträgliche Korrekturen
│   │   └── EinstellungenView.xaml         ← Bildschirmzuordnung, Wertungssystem
│   └── Anzeige/
│       ├── StartscreenView.xaml           ← Vor Turnierbeginn
│       ├── InfoscreenView.xaml            ← Zwischen den Spielen (rotierende Infos)
│       ├── MatchdayView.xaml              ← Laufendes Spiel (Live-Anzeige)
│       └── GewinnerView.xaml              ← Turnierende, Endplatzierungen
├── ViewModels/
│   ├── Bedienung/
│   │   ├── TurnierAnlegenViewModel.cs
│   │   ├── SpielplanViewModel.cs
│   │   ├── SpielsteuerungViewModel.cs
│   │   ├── KorrekturViewModel.cs
│   │   └── EinstellungenViewModel.cs
│   └── Anzeige/
│       ├── StartscreenViewModel.cs
│       ├── InfoscreenViewModel.cs
│       ├── MatchdayViewModel.cs
│       └── GewinnerViewModel.cs
└── Resources/
    ├── Styles.xaml                        ← Globale Styles (Buttons, Cards, Schriften)
    ├── Colors.xaml                        ← Farbpalette (MaterialDesign-Theme)
    └── Assets/
        └── Logo_Kirchenbirkiger_Schluck.png
```

## 3. Anwendungslogikschicht – `KirchenbirkigerSchluck.Core`

Klassenbibliotek ohne UI-Abhängigkeiten. Enthält Domänenmodelle, Enums, Dienste und Interfaces.

```
Core/
├── Models/
│   ├── Turnier.cs
│   ├── Gruppe.cs
│   ├── Team.cs
│   ├── Spieler.cs
│   ├── Spiel.cs
│   ├── SpielErgebnis.cs
│   ├── Einzelduell.cs
│   ├── EinzelduellErgebnis.cs
│   ├── Versuch.cs
│   └── Aenderungseintrag.cs
├── Enums/
│   ├── TurnierStatus.cs
│   ├── SpielStatus.cs
│   ├── TeamStatus.cs
│   ├── Wertungssystem.cs
│   └── EntscheidungsArt.cs
├── Services/
│   ├── TurnierService.cs                  ← Turnier anlegen, laden, speichern
│   ├── SpielplanService.cs                ← Spielplan generieren, Reihenfolge ändern
│   ├── SpielsteuerungService.cs           ← Partie starten, Versuche erfassen, Stechen
│   ├── WertungsService.cs                 ← Tabellenpunkte berechnen, Tabelle sortieren
│   └── AenderungsprotokollService.cs      ← Korrektureinträge erzeugen und verwalten
└── Interfaces/
    ├── ITurnierService.cs
    ├── ISpielplanService.cs
    ├── ISpielsteuerungService.cs
    ├── IWertungsService.cs
    └── IAenderungsprotokollService.cs
```

## 4. Datenhaltungsschicht – `KirchenbirkigerSchluck.Data`

Klassenbibliotek. Verantwortlich für das Lesen und Schreiben von JSON-Dateien sowie das Backup-Management.

```
Data/
├── Repositories/
│   └── TurnierRepository.cs               ← Lesen / Schreiben der turnier.json
├── Serialization/
│   └── JsonKonfiguration.cs               ← System.Text.Json-Optionen (CamelCase, Enum-Strings, etc.)
├── Backup/
│   └── BackupManager.cs                   ← Backup erzeugen, auflisten, laden
└── Migration/
    └── SchemaMigration.cs                 ← Format-Migrationen bei neuen Schema-Versionen
```

## 5. Testprojekt – `KirchenbirkigerSchluck.Tests`

xUnit-Testprojekt. Testet Logik in Core und Data, keine UI-Tests in V1.

```
Tests/
├── Core/
│   ├── WertungsServiceTests.cs            ← Tabellenpunkte, Eishockey- vs. Einfach-System
│   ├── SpielsteuerungServiceTests.cs      ← Versuchsbewertung, Stechen-Logik
│   └── SpielplanServiceTests.cs           ← Reihenfolge, Status-Übergänge
└── Data/
    └── TurnierRepositoryTests.cs          ← JSON-Roundtrip, Backup-Dateinamen
```

## 6. Namenskonventionen

| Element | Konvention | Beispiel |
|---------|-----------|---------|
| Klassen, Records | PascalCase | `SpielErgebnis` |
| Interfaces | `I` + PascalCase | `ITurnierService` |
| Methoden, Properties | PascalCase | `BerechneTabellenpunkte()` |
| Private Felder | `_camelCase` | `_turnierRepository` |
| Lokale Variablen | camelCase | `aktuelleDuellnummer` |
| Enums (Typ) | PascalCase | `SpielStatus` |
| Enum-Werte | PascalCase | `SpielStatus.Laeuft` |
| Namespaces | `KirchenbirkigerSchluck.<Schicht>` | `KirchenbirkigerSchluck.Core.Services` |
| XAML-Dateien | PascalCase + Suffix | `SpielsteuerungView.xaml` |
| ViewModels | Suffix `ViewModel` | `SpielsteuerungViewModel.cs` |
| Async-Methoden | Suffix `Async` | `SpeichernAsync()` |

Kommentare ausschließlich auf Deutsch (gemäß globalem Kommentierungsstandard).

## 7. Abhängigkeiten zwischen Projekten

```
App  →  Core
App  →  Data
Data →  Core
Tests → Core
Tests → Data
```

`Core` hat keine Abhängigkeit zu `App` oder `Data`. Dies ermöglicht das Testen der Geschäftslogik ohne UI oder Dateizugriff.

## 8. Build und Deployment

```powershell
# Alle Projekte bauen
dotnet build

# Tests ausführen
dotnet test

# Einzelnen Test ausführen
dotnet test --filter "FullyQualifiedName~WertungsServiceTests"

# Release-Paket erzeugen (portabler Ordner, kein Installer)
dotnet publish src/KirchenbirkigerSchluck.App `
  -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=false `
  -o dist/

# Anwendung starten (Entwicklung)
dotnet run --project src/KirchenbirkigerSchluck.App
```

Das `dist/`-Verzeichnis enthält die fertige Anwendung als Ordner mit `.exe` und DLLs, direkt ausführbar ohne Installation.

## 9. NuGet-Pakete

| Projekt | Paket | Zweck |
|---------|-------|-------|
| App | `MaterialDesignThemes` | Modernes UI-Theming |
| App | `CommunityToolkit.Mvvm` | MVVM-Boilerplate (Commands, ObservableObject) |
| Core | – | Keine externen Abhängigkeiten |
| Data | – | Nur BCL (System.Text.Json im SDK enthalten) |
| Tests | `xunit`, `xunit.runner.visualstudio` | Test-Framework |
| Tests | `Microsoft.NET.Test.Sdk` | Test-Host |
