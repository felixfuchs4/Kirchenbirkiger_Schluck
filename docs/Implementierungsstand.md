# Implementierungsstand – Kirchenbirkiger Schluck

> **Stand:** 2026-06-12 | Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 37/37 grün

---

## Abgeschlossene Schritte ✅

### Grundstruktur

| Bereich | Enthaltene Klassen / Dateien |
|---|---|
| **Enums** (5) | `TurnierStatus`, `SpielStatus`, `TeamStatus`, `Wertungssystem`, `EntscheidungsArt` |
| **Modelle** (10) | `Turnier`, `Gruppe`, `Team`, `Spieler`, `Spiel`, `SpielErgebnis`, `Einzelduell`, `EinzelduellErgebnis`, `Versuch`, `Aenderungseintrag` |
| **Interfaces** (6) | `ITurnierService`, `ITurnierRepository`, `ISpielsteuerungService`, `ISpielplanService`, `IWertungsService` (+ `GruppenTabellenEintrag`), `IAenderungsprotokollService` |
| **App-Scaffolding** | `App.xaml`, `BedienWindow` (Placeholder), `AnzeigeWindow` (Placeholder) |

### Services – Core

| Klasse | Implementierte Methoden | Tests |
|---|---|---|
| `WertungsService` | `TabellenPunkteBerechnen`, `GruppenRanglisteBerechnen` (inkl. Direkter-Vergleich-Tiebreaker + Stechen-Markierung) | **8** |
| `SpielsteuerungService` | `SpielStarten`, `NaechesDuellStarten`, `StechenStarten`, `VersuchErfassen`, `SpielAbschliessen` | **8** |
| `SpielplanService` | `GruppenspielplanGenerieren` (Round-Robin, interleaved), `NaechstesSpielErmitteln`, `SpielNachHintenVerschieben` | **6** |

### Services & Klassen – Data

| Klasse | Implementierte Methoden | Tests |
|---|---|---|
| `BackupManager` | `DateinameGenerieren`, `DateinameSanitieren`, `BackupErstellen`, `AlleDateien` | **4** |
| `TurnierRepository` | `Laden`, `Speichern`, `ExistiertDatei` | **3** |
| `TurnierService` | `TurnierErstellen`, `TurnierLaden`, `TurnierSpeichern`, `StatusWechseln`, `TeamHinzufuegen`, `TeamZurueckziehen` | **8** |
| `SchemaMigration` | `MigrationErforderlich`, `AktuelleVersion` (trivial) | – |

---

## Offene Schritte 🔲

### Schritt A – Data Layer abschließen ✅ *(abgeschlossen)*

| Methode | Beschreibung |
|---|---|
| `TurnierRepository.Laden()` | JSON deserialisieren (`System.Text.Json` + `JsonKonfiguration`) |
| `TurnierRepository.Speichern()` | JSON serialisieren + atomar in Datei schreiben |
| `BackupManager.BackupErstellen()` | Turnier serialisieren + Datei im Backup-Verzeichnis anlegen |
| `BackupManager.AlleDateien()` | Alle Backup-Dateien sortiert nach Erstellzeit zurückgeben |

Tests: TurnierRepository JSON-Roundtrip (inkl. Sonderzeichen, fehlende Datei), BackupManager-Integration

---

### Schritt B – TurnierService ✅ *(abgeschlossen)*

| Methode | Beschreibung |
|---|---|
| `TurnierErstellen()` | Neues Turnier mit Defaults anlegen |
| `TurnierLaden()` / `TurnierSpeichern()` | Delegation an `ITurnierRepository` |
| `StatusWechseln()` | Zustandsautomat: `InVorbereitung` → `Gruppenphase` → `Finalrunde` → `Abgeschlossen` |
| `TeamHinzufuegen()` | Team erstellen und zu `Turnier.Teams` hinzufügen |
| `TeamZurueckziehen()` | `TeamStatus.Zurueckgezogen`, offene Spiele → `Abgesetzt` |

Tests: TurnierErstellen, TeamHinzufuegen (mit/ohne Kurzname), TeamZurueckziehen (Status + Spiele), StatusWechseln (3 Übergänge + Exception)

---

### Schritt C – AenderungsprotokollService *(nach Schritt A)*

| Methode | Beschreibung |
|---|---|
| `EintragErstellen()` | `Aenderungseintrag` anlegen + an `Turnier.Aenderungsprotokoll` appenden |
| `EintraegeAbfragen()` | Einträge für eine Entität filtern, absteigend nach Zeitstempel |

Tests: EintragErstellen, EintraegeAbfragen

---

### Schritt D – FinalrundeGenerieren ⛔ *blockiert*

`SpielplanService.FinalrundeGenerieren()` – Fachkonzept für Finalrundenstruktur ist noch nicht spezifiziert.
Erst implementieren, wenn Anzahl Weiterkommer, Bracket-Aufbau und Platzierungsspiele festgelegt sind.

---

### Schritt E – SchemaMigration *(niedrige Priorität, V2-relevant)*

`SchemaMigration.Migrieren()` – in V1 existiert nur eine Schemaversion; kein echtes Migrationsszenario.
Wird relevant, wenn ein zweites Datenformat (V2) eingeführt wird.

---

### Schritt F – UI: BedienWindow & AnzeigeWindow *(nach A + B + C)*

| Bereich | Beschreibung |
|---|---|
| ViewModels | MVVM-Klassen für Spielplan, Ergebniseingabe, Gruppenrangliste |
| BedienWindow | Spielplan-Ansicht, Duell-/Versuchserfassung, Korrekturfunktionen |
| AnzeigeWindow | Beamer-View: Spielstand live, Gruppenrangliste, Bracket, Siegeranzeige, Infoscreen |

*Abhängigkeiten: Schritte A, B und C müssen fertig sein.*

---

## Abhängigkeiten auf einen Blick

```
Enums / Modelle / Interfaces  (fertig ✅)
         │
         ▼
   Core Services              (fertig ✅)
   (Wertung, Steuerung, Plan)
         │
         ▼
     Schritt A                (Data Layer)
     ├── Schritt B            (TurnierService)
     └── Schritt C            (AenderungsprotokollService)
              │
              ▼
         Schritt F            (UI)

Schritt D  ──── blockiert (Fachkonzept fehlt)
Schritt E  ──── niedrige Priorität (V2)
```
