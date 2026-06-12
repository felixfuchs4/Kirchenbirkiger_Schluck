# Speicherformat

Dieses Dokument beschreibt das Dateiformat, die Verzeichnisstruktur, das JSON-Schema und das Backup-System für das Projekt **Kirchenbirkiger Schluck Programm**.

Das Datenmodell ist in [Datenmodell.md](Datenmodell.md) beschrieben.

## 1. Format

Alle Turnierdaten werden als **JSON** gespeichert. Kein Datenbankserver, kein proprietäres Format.

Vorteile:
- Menschenlesbar und im Notfall manuell editierbar
- Einfaches Backup durch schlichte Dateikopie
- `System.Text.Json` (im .NET 8 SDK enthalten) ohne weitere Abhängigkeit nutzbar

## 2. Verzeichnisstruktur

Die Anwendung speichert alle Dateien in einem frei wählbaren Turnierdaten-Ordner.

```
<Turnierdaten-Ordner>/
├── turnier.json           ← aktuelle Hauptdatei (wird bei jeder Änderung überschrieben)
└── backups/
    ├── 2026-06-12_09-00-00_start.json
    ├── 2026-06-12_14-35-22_nach-Spiel-3_DieBierbrauer-vs-DieSchlucker.json
    ├── 2026-06-12_15-10-00_manuell.json
    └── 2026-06-12_19-45-00_abschluss.json
```

Der Speicherort wird beim ersten Speichern ausgewählt und bleibt für das Turnier bestehen.

## 3. Schema-Versionierung

Jede Datei enthält ein `$schema`-Feld zur Identifikation der Formatversion.

```json
"$schema": "kirchenbirkiger-schluck/v1"
```

Bei Formatänderungen in zukünftigen Versionen:
- Die neue Version erhält ein neues Schema-Kürzel, z. B. `v2`
- Migrationslogik liegt in `KirchenbirkigerSchluck.Data/Migration/SchemaMigration.cs`
- Die Anwendung erkennt ältere Versionen und migriert automatisch beim Öffnen

## 4. Vollständiges JSON-Beispiel

Das folgende Beispiel zeigt eine minimale, aber vollständige Turnierdatei mit einer Gruppe, zwei Teams und einem abgeschlossenen Spiel.

```json
{
  "$schema": "kirchenbirkiger-schluck/v1",
  "turnier": {
    "id": "a1b2c3d4-0001-0000-0000-000000000000",
    "schemaVersion": 1,
    "name": "Kirchenbirkiger Schluck 2026",
    "datum": "2026-06-12",
    "uhrzeit": "18:00:00",
    "ort": "Kirchenbirkig",
    "wertungssystem": "Eishockey",
    "status": "Gruppenphase",
    "erstelltAm": "2026-06-10T14:00:00",
    "geaendertAm": "2026-06-12T19:35:22",
    "gruppen": [
      {
        "id": "b1000000-0000-0000-0000-000000000001",
        "name": "Gruppe A",
        "teams": [
          {
            "id": "c1000000-0000-0000-0000-000000000001",
            "name": "Die Bierbrauer",
            "kurzname": "BBR",
            "status": "Aktiv",
            "spieler": [
              { "id": "d1000000-0000-0000-0000-000000000001", "name": "Hans Wurst" },
              { "id": "d1000000-0000-0000-0000-000000000002", "name": "Franz Maier" },
              { "id": "d1000000-0000-0000-0000-000000000003", "name": "Klaus Bauer" },
              { "id": "d1000000-0000-0000-0000-000000000004", "name": "Peter Huber" },
              { "id": "d1000000-0000-0000-0000-000000000005", "name": "Stefan Schuster" }
            ]
          },
          {
            "id": "c1000000-0000-0000-0000-000000000002",
            "name": "Die Schlucker",
            "kurzname": "SCH",
            "status": "Aktiv",
            "spieler": [
              { "id": "d2000000-0000-0000-0000-000000000001", "name": "Max Muster" },
              { "id": "d2000000-0000-0000-0000-000000000002", "name": "Paul Wagner" },
              { "id": "d2000000-0000-0000-0000-000000000003", "name": "Karl Fischer" },
              { "id": "d2000000-0000-0000-0000-000000000004", "name": "Anton Berger" },
              { "id": "d2000000-0000-0000-0000-000000000005", "name": "Josef Müller" }
            ]
          }
        ],
        "spiele": [
          {
            "id": "e1000000-0000-0000-0000-000000000001",
            "gruppeId": "b1000000-0000-0000-0000-000000000001",
            "team1Id": "c1000000-0000-0000-0000-000000000001",
            "team2Id": "c1000000-0000-0000-0000-000000000002",
            "reihenfolge": 1,
            "status": "Abgeschlossen",
            "einzelduelle": [
              {
                "id": "f1000000-0000-0000-0000-000000000001",
                "spielId": "e1000000-0000-0000-0000-000000000001",
                "duellnummer": 1,
                "istStechen": false,
                "spieler1Id": "d1000000-0000-0000-0000-000000000001",
                "spieler2Id": "d2000000-0000-0000-0000-000000000001",
                "versuche": [
                  {
                    "id": "g1000000-0000-0000-0000-000000000001",
                    "einzelduellId": "f1000000-0000-0000-0000-000000000001",
                    "versuchnummer": 1,
                    "spieler1Getroffen": true,
                    "spieler2Getroffen": false
                  }
                ],
                "ergebnis": {
                  "siegerId": "c1000000-0000-0000-0000-000000000001",
                  "duellpunktTeam1": 1,
                  "duellpunktTeam2": 0
                }
              },
              {
                "id": "f1000000-0000-0000-0000-000000000002",
                "spielId": "e1000000-0000-0000-0000-000000000001",
                "duellnummer": 2,
                "istStechen": false,
                "spieler1Id": "d1000000-0000-0000-0000-000000000002",
                "spieler2Id": "d2000000-0000-0000-0000-000000000002",
                "versuche": [
                  {
                    "id": "g2000000-0000-0000-0000-000000000001",
                    "einzelduellId": "f1000000-0000-0000-0000-000000000002",
                    "versuchnummer": 1,
                    "spieler1Getroffen": true,
                    "spieler2Getroffen": true
                  },
                  {
                    "id": "g2000000-0000-0000-0000-000000000002",
                    "einzelduellId": "f1000000-0000-0000-0000-000000000002",
                    "versuchnummer": 2,
                    "spieler1Getroffen": false,
                    "spieler2Getroffen": true
                  }
                ],
                "ergebnis": {
                  "siegerId": "c1000000-0000-0000-0000-000000000002",
                  "duellpunktTeam1": 0,
                  "duellpunktTeam2": 1
                }
              }
            ],
            "ergebnis": {
              "siegerId": "c1000000-0000-0000-0000-000000000001",
              "duellpunkteTeam1": 3,
              "duellpunkteTeam2": 2,
              "entschiedenDurch": "RegulaereSpielzeit",
              "tabellenPunkteTeam1": 3,
              "tabellenPunkteTeam2": 0
            }
          }
        ]
      }
    ],
    "aenderungsprotokoll": [
      {
        "zeitstempel": "2026-06-12T19:35:22",
        "art": "Versuchskorrektur",
        "kontext": "Spiel 1, Duell 2, Versuch 1",
        "vorherigerWert": "Spieler1Getroffen: false",
        "neuerWert": "Spieler1Getroffen: true"
      }
    ]
  }
}
```

## 5. JSON-Serialisierungskonventionen

| Konvention | Einstellung |
|-----------|------------|
| Property-Namen | `camelCase` (z. B. `duellpunktTeam1`) |
| Enum-Werte | Als Zeichenkette (z. B. `"Eishockey"`, nicht `0`) |
| Datum | ISO 8601 (`"2026-06-12"`) |
| Uhrzeit | ISO 8601 (`"18:00:00"`) |
| DateTime | ISO 8601 mit Zeitzone (`"2026-06-12T19:35:22"`) |
| Null-Werte | Werden weggelassen (`JsonIgnore(Condition = WhenWritingNull)`) |
| Guid | Kleinbuchstaben ohne geschweifte Klammern |
| Einrückung | 2 Leerzeichen (Produktionsdatei: minifiziert optional) |

## 6. Backup

### Aufbewahrung

Alle erzeugten Backups werden dauerhaft aufbewahrt. Es gibt kein automatisches Löschen.

### Dateinamenskonvention

```text
<YYYY-MM-DD>_<HH-MM-SS>_<Anlass>[_<SpielNr>-<TeamA>-vs-<TeamB>].json
```

Beispiele:

```text
2026-06-12_09-00-00_start.json
2026-06-12_14-35-22_nach-Spiel-3_DieBierbrauer-vs-DieSchlucker.json
2026-06-12_15-10-00_manuell.json
2026-06-12_19-00-00_nach-korrektur.json
2026-06-12_20-45-00_abschluss.json
```

### Anlässe

| Kürzel | Auslöser |
|--------|---------|
| `start` | Beim ersten Start eines Turniers |
| `nach-Spiel-N` | Automatisch nach jedem abgeschlossenen Spiel |
| `manuell` | Manuell durch die Turnierleitung ausgelöst |
| `nach-korrektur` | Nach einer rückwirkenden Korrektur mit Tabellenauswirkung |
| `abschluss` | Beim Abschluss des gesamten Turniers |
