# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projekt

**Kirchenbirkiger Schluck** – C#-Anwendung zur Turnierleitung und Anzeige beim gleichnamigen Heimturnier.
Die Anwendung besteht aus zwei getrennten Oberflächen:

- **Bedienoberfläche** – für die Turnierleitung (Spielplan, Ergebniseingabe, Verwaltung)
- **Anzeigeoberfläche** – Beamer-/Zuschaueransicht (Spielstand, Gruppenrangliste, Bracket, Siegeranzeige)

Der Turnierablauf besteht aus zwei Phasen:
1. **Gruppenphase** – Teams spielen innerhalb ihrer Gruppen gegeneinander
2. **Finalrunde / Platzierungsspiele** – K.O.-Bracket aus den Gruppenweitern

Fachliches Detail zu Spielregeln, Wertungslogik, Sonderfällen und Anzahl Teams/Gruppen ist in [`docs/02_Fachkonzept/`](docs/02_Fachkonzept/) zu finden.

## Architektur

Geplante 3-Schichten-Architektur (Details in [`docs/04_Technik/Architektur.md`](docs/04_Technik/Architektur.md)):

```
Präsentation      (Bedien- und Anzeigeoberfläche)
Anwendungslogik   (Turnierverwaltung, Wertung, Ablaufsteuerung)
Datenhaltung      (JSON-Dateien)
```

Das Speicherformat ist JSON; Beispielstruktur in [`docs/04_Technik/Speicherformat.md`](docs/04_Technik/Speicherformat.md).

Kernentitäten (Details in [`docs/04_Technik/Datenmodell.md`](docs/04_Technik/Datenmodell.md)):
`Turnier` → `Gruppe` → `Team` / `Spiel` → `Ergebnis`

## Build und Tests

Sobald `src/` eingerichtet ist, gelten Standard-.NET-CLI-Befehle:

```powershell
# Bauen
dotnet build

# Alle Tests ausführen
dotnet test

# Einzelnen Test ausführen
dotnet test --filter "FullyQualifiedName~TestName"

# Anwendung starten
dotnet run --project src/<Projektname>
```

## Dokumentationsstruktur

| Ordner | Inhalt |
|--------|--------|
| `docs/00_Projektstart/` | Projektziel, Entscheidungslog, offene Fragen |
| `docs/01_Anforderungen/` | Funktionale/nicht-funktionale Anforderungen, Nutzerrollen |
| `docs/02_Fachkonzept/` | Spielregeln, Wertungslogik, Turniermodus, Sonderfälle |
| `docs/03_UI_UX/` | Oberflächen-Specs, Screens, Designrichtlinie |
| `docs/04_Technik/` | Architektur, Datenmodell, Speicherformat, Projektstruktur |
| `docs/05_Test_und_Abnahme/` | Testszenarien, Beispielturniere, Abnahmeliste |
| `docs/06_Betrieb/` | Aufbau vor Ort, Bedienanleitung, Checkliste |

Vor Implementierungsarbeit immer die relevanten Fachkonzept-Dokumente lesen — viele Anforderungen (Tiebreaker, Sonderfälle, Anzeigewechsel) sind dort spezifiziert.

## Kommentierungsstandard

Für alle C#-Quelldateien gilt der globale Kommentierungsstandard (`~/.claude/commenting-guidelines.md`):

- **Sprache**: Deutsch
- **Datei-Header**: Pflichtkommentar am Dateianfang (Box-Format)
- **XML-Dokumentationskommentare** (`///`): für alle Klassen, Interfaces, Methoden, Properties, Enums, Konstanten
- **Inline-Kommentare**: nur für nicht-offensichtliche Logik, Hintergrund, Constraints

Verstöße gegen diese Regeln müssen vor der finalen Ausgabe behoben werden.
