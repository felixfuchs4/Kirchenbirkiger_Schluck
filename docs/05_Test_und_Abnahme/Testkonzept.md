# Testkonzept – Kirchenbirkiger Schluck

## 1. Ziel und Strategie

Ziel ist ein robustes, wartbares Testfundament für die Kernlogik der Anwendung.

**Test-Pyramide (V1):**

```
         [   E2E / UI   ]   → nicht in V1
        [  Integration   ]   → optional (JSON-Roundtrip)
       [    Unit Tests    ]   → Hauptanteil
```

V1 konzentriert sich vollständig auf **Unit Tests** der Geschäftslogik in `KirchenbirkigerSchluck.Core` und ausgewählter Datenschicht-Klassen. UI-Tests (WPF/XAML) werden erst in einer späteren Version ergänzt.

---

## 2. Framework und Werkzeuge

| Werkzeug | Version | Zweck |
|---|---|---|
| **xUnit** | ≥ 2.9 | Test-Runner, Testklassen (`[Fact]`, `[Theory]`) |
| **FluentAssertions** | ≥ 8.10 | Lesbare Assertions (`.Should().Be(...)`) |
| **.NET CLI** | –  | Testausführung (`dotnet test`) |

Keine Mocking-Bibliothek in V1 – Services werden direkt instanziiert; Abhängigkeiten via einfachen Testdatenklassen.

---

## 3. Zu testende Klassen (V1-Pflicht)

### 3.1 WertungsService

| Testfall | Eingabe | Erwartetes Ergebnis |
|---|---|---|
| Eishockey: regulärer Sieg | Sieger in regulärer Spielzeit | 3 : 0 Punkte |
| Eishockey: Stechen-Sieg | Sieger im Stechen | 2 : 1 Punkte |
| Eishockey: Stechen-Niederlage | Verlierer im Stechen | 1 Punkt (korrekt) |
| Einfach: regulärer Sieg | Sieger beliebig | 1 : 0 Punkte |
| Gruppenrangliste: 3 Teams | Vollständige Ergebnisliste | Korrekte Reihenfolge nach Punkten, dann Duellpunktdifferenz |

### 3.2 SpielsteuerungService

| Testfall | Eingabe | Erwartetes Ergebnis |
|---|---|---|
| Spielstart | Geplantes Spiel | Status = `Laeuft`, Startzeitpunkt gesetzt |
| Versuch: nur T1 trifft | Spieler 1 trifft, Spieler 2 nicht | Duell endet, T1 gewinnt |
| Versuch: nur T2 trifft | Spieler 2 trifft, Spieler 1 nicht | Duell endet, T2 gewinnt |
| Versuch: beide treffen | Beide treffen | Versuch unentschieden, Duell läuft weiter |
| Versuch: keiner trifft | Keiner trifft | Versuch unentschieden, Duell läuft weiter |
| Max. 3 Versuche | Drei unentschiedene Versuche | Duell endet 0:0, kein Sieger |
| Stechen-Erkennung | 5 Duelle, 2:3 Duellpunkte | Stechen wird eingeleitet (Status/Flag korrekt) |
| 0:0-Partie | Alle 5 Duelle 0:0 | `SpielErgebnis.DuellpunkteTeam1 = 0`, Stechen erforderlich |

### 3.3 SpielplanService

| Testfall | Eingabe | Erwartetes Ergebnis |
|---|---|---|
| Spielplan verschieben | Spiel in Position 2 von 3 | Spiel wandert auf Position 3 |
| Status-Übergang | Spiel `Geplant` → start | Status = `Laeuft` |

### 3.4 TurnierRepository (JSON-Roundtrip)

| Testfall | Eingabe | Erwartetes Ergebnis |
|---|---|---|
| Vollständiger Roundtrip | Turnier serialisieren + deserialisieren | Identisches Objekt (alle Felder) |
| Sonderzeichen | Teamnamen mit Umlauten und Sonderzeichen | Korrekte UTF-8-Kodierung und Wiederherstellung |
| Fehlende Datei | Laden ohne existierende Datei | `FileNotFoundException` |

### 3.5 BackupManager

| Testfall | Eingabe | Erwartetes Ergebnis |
|---|---|---|
| Dateiname ohne Spieldetails | Turnier, Zeitstempel | Format `YYYY-MM-DD_HH-MM-SS_Anlass.json` |
| Dateiname mit Teams | Turnier + Teamnamen | Teamnamen im Dateinamen enthalten |
| Sonderzeichen im Namen | Anlass mit `/`, `&`, `:` | Keine ungültigen Datei-Zeichen im Namen |

---

## 4. Was nicht getestet wird (V1)

| Bereich | Begründung |
|---|---|
| WPF Views / XAML | Kein UI-Test-Framework in V1; Validierung erfolgt manuell |
| ViewModels | Optional für V2; Logik liegt im Service-Layer |
| DI-Konfiguration | Framework-Verantwortung; wird beim App-Start implizit geprüft |
| Migrations-Code | Erst relevant, wenn ein zweites Schema existiert |
| Finalrunden-Generierung | Komplexer, wird bei Implementierung mit Tests ergänzt |

---

## 5. Testkonventionen

### Methodenname-Schema

```
Methode_Szenario_ErwartetesErgebnis
```

Beispiele:
- `TabellenPunkteBerechnen_EishockeySiegRegulaer_Gibt3Zu0`
- `VersuchErfassen_BeideTreffen_VersuchUnentschieden`
- `DateinameGenerieren_Sonderzeichen_DateinameIstGueltig`

### Testaufbau (AAA-Muster)

```csharp
// Arrange – Testdaten vorbereiten
// Act    – Methode aufrufen
// Assert – Ergebnis prüfen
```

### Weitere Regeln

- Eine Testklasse pro zu testender Klasse
- Testdaten als lokale Variablen – kein gemeinsamer Zustand zwischen Tests (`[SetUp]` vermeiden)
- Kein Dateisystem- oder Netzwerkzugriff in reinen Unit-Tests
- Hilfsmethoden (`SpielBauen(...)`) erlaubt um Boilerplate zu reduzieren

---

## 6. Coverage-Ziel

| Klasse | Ziel |
|---|---|
| `WertungsService` | Vollständige Branch-Coverage aller Wertungsszenarien |
| `SpielsteuerungService` | Vollständige Branch-Coverage der Versuchslogik und Stechen-Erkennung |
| Weitere Services | Happy-Path + wichtigste Fehlerpfade |
| Data-Schicht | JSON-Roundtrip + Dateinamen-Generierung |

---

## 7. Testausführung

```powershell
# Alle Tests ausführen
dotnet test src/

# Nur Tests eines Bereichs
dotnet test src/ --filter "FullyQualifiedName~Wertung"
dotnet test src/ --filter "FullyQualifiedName~Spielsteuerung"
dotnet test src/ --filter "FullyQualifiedName~Backup"

# Mit Coverage-Report
dotnet test src/ --collect:"XPlat Code Coverage"

# Einzelnen Test
dotnet test src/ --filter "FullyQualifiedName~TabellenPunkteBerechnen_EishockeySiegRegulaer_Gibt3Zu0"
```

---

## 8. Erweiterung in zukünftigen Versionen

| Version | Ergänzung |
|---|---|
| V2 | ViewModel-Tests für kritische Berechnungen |
| V2 | Integrationstests für JSON-Persistenz mit echtem Dateisystem |
| V3 | UI-Smoke-Tests für die wichtigsten Flows (bei Bedarf mit FlaUI oder ähnlichem) |
