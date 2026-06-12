# Datenmodell

Dieses Dokument beschreibt alle Domänenentitäten des Projekts **Kirchenbirkiger Schluck Programm**, ihre Felder, Beziehungen und Validierungsregeln.

Das Datenmodell bildet die Grundlage für die C#-Klassen in `KirchenbirkigerSchluck.Core/Models/` und das JSON-Speicherformat in [Speicherformat.md](Speicherformat.md).

## 1. Entitätenübersicht

| Entität | Beschreibung |
|---------|-------------|
| `Turnier` | Wurzelobjekt, enthält alle Turnierdaten |
| `Gruppe` | Eine Spielgruppe innerhalb des Turniers |
| `Team` | Eine teilnehmende Mannschaft |
| `Spieler` | Ein einzelner Spieler innerhalb eines Teams |
| `Spiel` | Eine Partie zwischen zwei Teams |
| `SpielErgebnis` | Abschluss-Ergebnis einer Partie (eingebettet) |
| `Einzelduell` | Ein Duel zwischen je einem Spieler beider Teams |
| `EinzelduellErgebnis` | Ergebnis eines Einzelduells (eingebettet) |
| `Versuch` | Ein einzelner Trinkversuch innerhalb eines Einzelduells |
| `Aenderungseintrag` | Eintrag im Änderungsprotokoll |

---

## 2. Entitäten im Detail

### 2.1 Turnier

Wurzelobjekt. Jede gespeicherte Datei enthält genau ein `Turnier`.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `SchemaVersion` | `int` | Ja | Version des Dateiformats (aktuell: 1) |
| `Name` | `string?` | Nein | Turniername, z. B. „Kirchenbirkiger Schluck 2026" |
| `Datum` | `DateOnly` | Ja | Veranstaltungsdatum |
| `Uhrzeit` | `TimeOnly?` | Nein | Startuhrzeit |
| `Ort` | `string?` | Nein | Veranstaltungsort |
| `Wertungssystem` | `Wertungssystem` | Ja | Eishockey (Standard) oder Einfach |
| `Status` | `TurnierStatus` | Ja | Aktueller Turnierfortschritt |
| `ErstelltAm` | `DateTime` | Ja | Zeitstempel der Erstellung |
| `GeaendertAm` | `DateTime` | Ja | Zeitstempel der letzten Änderung |
| `Gruppen` | `List<Gruppe>` | Ja | Mindestens eine Gruppe mit je einem Team |
| `Aenderungsprotokoll` | `List<Aenderungseintrag>` | Ja | Alle Korrekturen und Änderungen |

---

### 2.2 Gruppe

Eine Gruppe fasst Teams zusammen, die in der Gruppenphase gegeneinander spielen.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `Name` | `string` | Ja | Gruppenname, z. B. „Gruppe A" |
| `Teams` | `List<Team>` | Ja | Teams in dieser Gruppe |
| `Spiele` | `List<Spiel>` | Ja | Spiele innerhalb dieser Gruppe |

---

### 2.3 Team

Eine teilnehmende Mannschaft.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `Name` | `string` | Ja | Vollständiger Mannschaftsname |
| `Kurzname` | `string?` | Nein | Kurzbezeichnung für enge Anzeigen |
| `Status` | `TeamStatus` | Ja | Aktiv oder Zurueckgezogen |
| `Spieler` | `List<Spieler>` | Ja | Spieler in dieser Mannschaft |

---

### 2.4 Spieler

Ein einzelner Spieler innerhalb eines Teams.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `Name` | `string` | Ja | Spielername |

---

### 2.5 Spiel

Eine Partie zwischen genau zwei Teams.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `GruppeId` | `Guid` | Ja | Zugehörige Gruppe |
| `Team1Id` | `Guid` | Ja | Referenz auf Team 1 |
| `Team2Id` | `Guid` | Ja | Referenz auf Team 2 |
| `Reihenfolge` | `int` | Ja | Position im Spielplan (aufsteigend) |
| `Status` | `SpielStatus` | Ja | Aktueller Spielzustand |
| `Einzelduelle` | `List<Einzelduell>` | Ja | Alle Einzelduelle dieser Partie |
| `Ergebnis` | `SpielErgebnis?` | Nein | Nur gesetzt wenn `Status == Abgeschlossen` |

---

### 2.6 SpielErgebnis *(eingebettetes Value Object)*

Enthält das Abschlussergebnis einer Partie. Wird direkt im `Spiel`-Objekt gespeichert.

| Feld | Typ | Beschreibung |
|------|-----|-------------|
| `SiegerId` | `Guid` | Id des siegreichen Teams |
| `DuellpunkteTeam1` | `int` | Gewonnene Einzelduelle von Team 1 |
| `DuellpunkteTeam2` | `int` | Gewonnene Einzelduelle von Team 2 |
| `EntschiedenDurch` | `EntscheidungsArt` | RegulaereSpielzeit oder Stechen |
| `TabellenPunkteTeam1` | `int` | Tabellenpunkte für Team 1 (abhängig vom Wertungssystem) |
| `TabellenPunkteTeam2` | `int` | Tabellenpunkte für Team 2 |

---

### 2.7 Einzelduell

Ein direkter Vergleich zwischen je einem Spieler beider Teams innerhalb einer Partie.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `SpielId` | `Guid` | Ja | Zugehöriges Spiel |
| `Duellnummer` | `int` | Ja | 1–5 regulär; ab 6 Stechen |
| `IstStechen` | `bool` | Ja | Kennzeichnung ob Stechen-Duell |
| `Spieler1Id` | `Guid` | Ja | Spieler von Team 1 |
| `Spieler2Id` | `Guid` | Ja | Spieler von Team 2 |
| `Versuche` | `List<Versuch>` | Ja | Bis zu 3 Versuche |
| `Ergebnis` | `EinzelduellErgebnis?` | Nein | Nur gesetzt wenn Duell entschieden |

---

### 2.8 EinzelduellErgebnis *(eingebettetes Value Object)*

Enthält das Ergebnis eines Einzelduells.

| Feld | Typ | Beschreibung |
|------|-----|-------------|
| `SiegerId` | `Guid?` | Id des Siegers; `null` bei Unentschieden |
| `DuellpunktTeam1` | `int` | 0 oder 1 |
| `DuellpunktTeam2` | `int` | 0 oder 1 |

Mögliche Kombinationen:
- Klarer Sieg: `DuellpunktTeam1 = 1, DuellpunktTeam2 = 0` (oder umgekehrt)
- Unentschieden nach 3 Versuchen, beide haben mind. 1× getroffen: `1 / 1`, `SiegerId = null`
- Unentschieden nach 3 Versuchen, niemand hat getroffen: `0 / 0`, `SiegerId = null`

---

### 2.9 Versuch

Ein einzelner Trinkversuch innerhalb eines Einzelduells.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Id` | `Guid` | Ja | Eindeutiger Bezeichner |
| `EinzelduellId` | `Guid` | Ja | Zugehöriges Einzelduell |
| `Versuchnummer` | `int` | Ja | 1, 2 oder 3 |
| `Spieler1Getroffen` | `bool` | Ja | Hat Spieler 1 getroffen? |
| `Spieler2Getroffen` | `bool` | Ja | Hat Spieler 2 getroffen? |

---

### 2.10 Aenderungseintrag

Ein Eintrag im Änderungsprotokoll. Wird automatisch bei jeder rückwirkenden Korrektur erzeugt.

| Feld | Typ | Pflicht | Beschreibung |
|------|-----|---------|-------------|
| `Zeitstempel` | `DateTime` | Ja | Zeitpunkt der Änderung |
| `Art` | `string` | Ja | Art der Änderung (siehe Tabelle unten) |
| `Kontext` | `string` | Ja | Betroffenes Objekt, z. B. „Spiel 2, Duell 1, Versuch 2" |
| `VorherigerWert` | `string` | Ja | Wert vor der Änderung |
| `NeuerWert` | `string` | Ja | Wert nach der Änderung |

Bekannte Arten:

| Wert | Bedeutung |
|------|-----------|
| `Versuchskorrektur` | Trefferangabe eines Versuchs nachträglich geändert |
| `Spielernamen` | Name eines Spielers geändert |
| `Spielerauswahl` | Anderer Spieler für ein Duell ausgewählt |
| `SpielerHinzugefuegt` | Neuer Spieler (z. B. 6. Spieler für Stechen) ergänzt |
| `Turnierdaten` | Datum, Name oder andere Turnierdaten geändert |
| `TeamStatus` | Team als zurückgezogen markiert |

---

## 3. Enumerationen

### `TurnierStatus`
| Wert | Bedeutung |
|------|-----------|
| `InVorbereitung` | Turnier angelegt, noch nicht gestartet |
| `Gruppenphase` | Gruppenspiele laufen |
| `Finalrunde` | Platzierungs- und Finalspiele laufen |
| `Abgeschlossen` | Turnier beendet |

### `SpielStatus`
| Wert | Bedeutung |
|------|-----------|
| `Geplant` | Im Spielplan, noch nicht gestartet |
| `Vorbereitet` | Wird als nächstes gestartet |
| `Laeuft` | Partie ist aktiv |
| `Abgeschlossen` | Ergebnis liegt vor |
| `Abgesetzt` | Nicht gewertet (z. B. wegen Teamrückzug) |
| `Verschoben` | Im Spielplan weiter nach hinten gerückt |
| `Korrigiert` | Nachträgliche Korrektur wurde vorgenommen |

### `TeamStatus`
| Wert | Bedeutung |
|------|-----------|
| `Aktiv` | Team nimmt regulär teil |
| `Zurueckgezogen` | Team hat sich zurückgezogen; alle offenen Spiele werden abgesetzt |

### `Wertungssystem`
| Wert | Bedeutung |
|------|-----------|
| `Eishockey` | Standard: Sieg regulär 3 Pt, Sieg Stechen 2 Pt, Niederlage Stechen 1 Pt, Niederlage regulär 0 Pt |
| `Einfach` | Sieg 1 Pt, Niederlage 0 Pt |

### `EntscheidungsArt`
| Wert | Bedeutung |
|------|-----------|
| `RegulaereSpielzeit` | Sieger nach 5 regulären Einzelduellen |
| `Stechen` | Sieger erst im Stechen ermittelt |

---

## 4. ER-Beziehungen

```
Turnier       1 ──── N  Gruppe
Gruppe        1 ──── N  Team
Team          1 ──── N  Spieler
Gruppe        1 ──── N  Spiel
Spiel         N ──── 2  Team         (via Team1Id / Team2Id)
Spiel         1 ──── N  Einzelduell
Einzelduell   N ──── 2  Spieler      (via Spieler1Id / Spieler2Id)
Einzelduell   1 ──── N  Versuch
Turnier       1 ──── N  Aenderungseintrag
```

Alle Referenzen zwischen Entitäten erfolgen über `Guid`-Felder (keine verschachtelten Objekt-Referenzen über Grenzen der Aggregat-Wurzeln hinaus), um die JSON-Serialisierung einfach zu halten.

---

## 5. Validierungsregeln

| Regel | Beschreibung |
|-------|-------------|
| Turnier: Datum | Darf nicht leer sein |
| Turnier: Gruppen | Mindestens 1 Gruppe mit mindestens 2 Teams |
| Spiel: Teams | `Team1Id ≠ Team2Id` |
| Spiel: Gruppenangehörigkeit | Beide Teams müssen in derselben Gruppe sein |
| Einzelduell: Versuche | Maximal 3 Versuche pro Duell |
| Einzelduell: Nummer | Duellnummer innerhalb eines Spiels eindeutig |
| Versuch: Nummer | Versuchnummer 1–3; pro Duell je Nummer genau 1 Eintrag |
| SpielErgebnis: Tabellenpunkte (Eishockey) | Summe der Punkte beider Teams ∈ {3, 3} (3+0 oder 2+1) |
| SpielErgebnis: Tabellenpunkte (Einfach) | Summe ∈ {1} (1+0) |
| Aenderungseintrag | `VorherigerWert ≠ NeuerWert` |
