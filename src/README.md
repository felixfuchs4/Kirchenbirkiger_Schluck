# Quellcode – Kirchenbirkiger Schluck

## Projektübersicht

Die Solution besteht aus vier Projekten:

```
src/
├── KirchenbirkigerSchluck.App/     net8.0-windows  WPF-Anwendung
├── KirchenbirkigerSchluck.Core/    net8.0          Domänenlogik (keine UI-Abhängigkeit)
├── KirchenbirkigerSchluck.Data/    net8.0          JSON-Persistenz & Backup
└── KirchenbirkigerSchluck.Tests/   net8.0          xUnit-Tests
```

### KirchenbirkigerSchluck.Core

Enthält das gesamte Domänenmodell und die Geschäftslogik – unabhängig von UI und Dateisystem.

```
Core/
├── Enums/          TurnierStatus, SpielStatus, Wertungssystem, EntscheidungsArt, TeamStatus
├── Models/         Turnier, Gruppe, Team, Spieler, Spiel, Einzelduell, Versuch, ...
├── Interfaces/     IWertungsService, ISpielsteuerungService, ISpielplanService, ...
└── Services/       WertungsService, SpielsteuerungService, SpielplanService, ...
```

### KirchenbirkigerSchluck.Data

Kümmert sich ausschließlich um Persistenz – kein Domänenwissen.

```
Data/
├── Repositories/   TurnierRepository (JSON lesen/schreiben, atomar)
├── Backup/         BackupManager (automatische Sicherung nach jedem Spiel)
├── Serialization/  JsonKonfiguration (zentrale JsonSerializerOptions)
└── Migration/      SchemaMigration (Versionierung für zukünftige Formatänderungen)
```

### KirchenbirkigerSchluck.App

WPF-Anwendung mit zwei Fenstern.

```
App/
├── Windows/
│   ├── BedienWindow.xaml    Turnierleitung: Spielplan, Ergebniseingabe
│   └── AnzeigeWindow.xaml   Beamer/Zuschauer: Live-Spielstand, Rangliste, Bracket
└── App.xaml                 DI-Konfiguration, App-Start
```

### KirchenbirkigerSchluck.Tests

```
Tests/
├── Core/   WertungsServiceTests, SpielsteuerungServiceTests, SpielplanServiceTests
└── Data/   BackupManagerTests, TurnierRepositoryTests
```

## Build

```powershell
# Solution bauen
dotnet build src/

# Nur ein Projekt bauen
dotnet build src/KirchenbirkigerSchluck.Core
```

## Tests

```powershell
# Alle Tests
dotnet test src/

# Nach Bereich filtern
dotnet test src/ --filter "FullyQualifiedName~Wertung"
dotnet test src/ --filter "FullyQualifiedName~Spielsteuerung"
dotnet test src/ --filter "FullyQualifiedName~Spielplan"
dotnet test src/ --filter "FullyQualifiedName~Backup"
dotnet test src/ --filter "FullyQualifiedName~Repository"

# Mit Coverage
dotnet test src/ --collect:"XPlat Code Coverage"
```

## Abhängigkeiten

| Paket | Version | Verwendung |
|---|---|---|
| MaterialDesignThemes | 5.3.2 | WPF-UI-Komponenten |
| CommunityToolkit.Mvvm | 8.4.0 | MVVM, Source Generators |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | DI-Container |
| xUnit | 2.9.3 | Test-Runner |
| FluentAssertions | 8.10.0 | Assertions |
