# Kirchenbirkiger Schluck

C#/WPF-Anwendung zur Turnierleitung und Zuschaueranzeige beim gleichnamigen Heimturnier.

## Was ist das?

Der **Kirchenbirkiger Schluck** ist ein jährliches Heimturnier. Diese Anwendung ersetzt den bisherigen
Excel-Spielplan durch eine dedizierte Software mit zwei getrennten Oberflächen:

| Oberfläche | Zweck |
|---|---|
| **Bedienoberfläche** | Turnierleitung: Spielplan, Ergebniseingabe, Korrekturen |
| **Anzeigeoberfläche** | Beamer/Zuschauer: Spielstand live, Gruppenrangliste, Bracket, Siegeranzeige |

Der Turnierverlauf besteht aus einer **Gruppenphase** (Round-Robin) gefolgt von einer **Finalrunde**.

## Technologie

| Komponente | Entscheidung |
|---|---|
| Sprache | C# 12 / .NET 8 |
| UI-Framework | WPF + MaterialDesignThemes 5.3 |
| MVVM | CommunityToolkit.Mvvm 8.4 |
| Persistenz | JSON via System.Text.Json |
| Tests | xUnit + FluentAssertions |
| DI | Microsoft.Extensions.DependencyInjection |

## Projektstruktur

```
Kirchenbirkiger-Schluck/
├── src/                           Quellcode
│   ├── KirchenbirkigerSchluck.App/     WPF-Anwendung (Bedien- & Anzeigeoberfläche)
│   ├── KirchenbirkigerSchluck.Core/    Domänenmodelle, Interfaces, Services
│   ├── KirchenbirkigerSchluck.Data/    JSON-Persistenz, Backup, Migration
│   └── KirchenbirkigerSchluck.Tests/   xUnit-Tests
├── docs/                          Vollständige Projektdokumentation
│   ├── 02_Fachkonzept/            Spielregeln, Wertungslogik, Turniermodus
│   ├── 04_Technik/                Architektur, Datenmodell, Speicherformat
│   └── ...
├── assets/                        Logo, Screenshots, Mockups
├── quellen/                       Referenzmaterial (Excel-Altdaten, Einladungen)
└── CLAUDE.md                      Hinweise für KI-gestützte Entwicklung
```

## Build & Start

```powershell
# Alle Projekte bauen
dotnet build src/

# Anwendung starten
dotnet run --project src/KirchenbirkigerSchluck.App

# Tests ausführen
dotnet test src/

# Einzelnen Testbereich ausführen
dotnet test src/ --filter "FullyQualifiedName~Wertung"
dotnet test src/ --filter "FullyQualifiedName~Spielsteuerung"
dotnet test src/ --filter "FullyQualifiedName~Spielplan"
```

## Implementierungsstand

Aktueller Stand: siehe [docs/Implementierungsstand.md](docs/Implementierungsstand.md).

## Dokumentation

Die vollständige Projektdokumentation befindet sich im [docs/](docs/)-Verzeichnis.
Fachliche Regeln (Spielablauf, Wertungslogik, Sonderfälle) sind in
[docs/02_Fachkonzept/](docs/02_Fachkonzept/) beschrieben.
