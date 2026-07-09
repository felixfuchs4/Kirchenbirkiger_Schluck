# Implementierungsstand – Kirchenbirkiger Schluck

> **Stand:** 2026-07-09 | Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 103/103 grün | UI-Redesign: einheitliches Token-Designsystem, dunkles Bedien-Theme, Broadcast-Look der Anzeige

---

## Abgeschlossene Schritte ✅

### Schritt AF – Gruppen-Tiebreaker: Torverhältnis + DV/S-Kennzeichnung ✅ *(abgeschlossen, 2026-07-09)*

| Bereich | Änderung |
|---|---|
| **Tiebreak-Reihenfolge** | Neue Sortierung bei Punktgleichstand: Tabellenpunkte → **Torverhältnis** (Duelldifferenz) → Direkter Vergleich → Stechen. Bisher fehlte das Torverhältnis komplett |
| **WertungsService** | Primärsortierung um `ThenByDescending(Torverhältnis)` erweitert; `GleichstandAufloesen` gruppiert nun nach (Punkte, Torverhältnis) und setzt je Team `DurchDirektenVergleich` bzw. `DurchStechen`; `StehenErforderlich` behält seine Bedeutung (Stechen noch auszuspielen, nur bei vollständiger Gruppe) |
| **SpielplanService** | `PlatzierungsStechenErzeugen` gruppiert Stechen-pflichtige Teams jetzt nach (Punkte, Torverhältnis), damit getrennte Gleichstände gleicher Punktzahl nicht vermischt werden |
| **Anzeige** | `GruppenTabellenEintragAnzeigeModel.StechenNoetig` ersetzt durch `TiebreakKuerzel`/`HatTiebreak`. Bedien-Tabelle **und** Beamer-Infoscreen zeigen das kompakte Kürzel „DV"/„S" neben der Platzierung (statt des bisherigen „STECHEN"-Wortes); „DV" in Akzentfarbe, „S" in Stechen-Farbe |
| **Tests** | `WertungsServiceTests` um Fälle für Torverhältnis, Direkten Vergleich, Stechen-erforderlich und gespieltes Stechen erweitert (bestehende Tiebreak-Tests an neue Reihenfolge angepasst); `SpielplanServiceTests`-Stechen-Fall aktualisiert → 103/103 |
| **Doku** | `docs/02_Fachkonzept/Wertungslogik.md` (Tiebreaker-Reihenfolge + DV/S-Kennzeichnung) gefüllt |

### Schritt AE – Benannte Speicherstände (Speichern unter / Laden mit Auswahl) ✅ *(abgeschlossen, 2026-07-09)*

| Bereich | Änderung |
|---|---|
| **Problem** | Speichern erlaubte keinen eigenen Namen, und beim Laden gab es keine Auswahl – es existierte nur die feste `turnier.json` (Autospeichern/-laden) + stille Backups |
| **Speicherstand-Modell** | Neuer Wrapper `Speicherstand` (Titel, optionale Beschreibung, Zeitpunkt + vollständiges `Turnier`) sowie `SpeicherstandInfo` (Metadaten für die Liste) und Enum `SpeicherstandTyp` (Benannt/Backup). Ein Speicherstand enthält den **kompletten** Stand (Teams, Spieler, Gruppen, Spielplan, Ergebnisse; Tabellen werden daraus berechnet) |
| **SpeicherstandService (Data)** | `SpeichernUnter(turnier, titel, beschreibung)` schreibt einen benannten Stand ins neue Verzeichnis `speicherstaende/` (atomar, Titel = Dateiname, Überschreiben bei gleichem Titel). `Alle()` listet benannte Stände **und** die automatischen Backups (Metadaten via `JsonDocument`, sortiert nach Zeit). `Laden(info)`/`Loeschen(info)`. Über `ISpeicherstandService` im DI registriert |
| **Bedienoberfläche** | Karte „Speicherstände" im Turnierverwaltungs-Tab: Schnellspeichern des Arbeitsstands, „Speichern unter" mit Titel- und Beschreibungsfeld, Auswahlliste aller Stände (Titel, Beschreibung, Typ · Datum · Status) mit „Laden" und „Löschen" + Aktualisieren. Laden ersetzt den Arbeitsstand (mit Bestätigung); Autospeichern/-laden bleibt unverändert |
| **Tests** | +5 in `SpeicherstandServiceTests`: Roundtrip (voller Turnierstand), Überschreiben bei gleichem Titel, leerer Titel abgelehnt, Backups in der Liste + ladbar, Löschen → 101/101 |

### Schritt AD – Best-of-5-Duelle & Auslosung der Spielerreihenfolge ✅ *(abgeschlossen, 2026-07-09)*

| Bereich | Änderung |
|---|---|
| **Best-of-5** | Eine Partie endet vorzeitig, sobald ein Team uneinholbar führt (Vorsprung > verbleibende reguläre Duelle), statt starr alle fünf Duelle zu spielen. Solange ein Ausgleich möglich ist, wird weitergespielt (Weg ins Stechen bleibt offen); Unentschieden-Duelle (0:0/1:1) verändern den Vorsprung nicht |
| **Core (testbar)** | Die bisher im ViewModel hartcodierte „5"-Ablauflogik liegt jetzt zentral in `SpielsteuerungService.Auswerten(Spiel)` → `SpielFortschritt` (Duellsiege, verbleibend, Stechen nötig, kann abschließen); Konstante `RegulaereDuelle = 5`. Zusätzlich korrigiert: das Stechen gilt erst mit **klarem Sieger** als entschieden (vorher war schon ein unentschiedenes Stechen-Duell abschließbar) |
| **Spielerreihenfolge** | Neue Felder `Spiel.Spieler1Reihenfolge`/`Spieler2Reihenfolge`; `SpielsteuerungService.SpielerReihenfolgeFestlegen(...)` lost je Team zu Spielbeginn einmalig eine Reihenfolge aus (idempotent, persistiert). Vorschlag/Vorschau in Bedien- und Anzeige-Screen nutzen die Reihenfolge statt fester `Spieler[idx]`. **Manuelles Ändern** der Spieler im Duell-Bildschirm bleibt unverändert möglich |
| **UI** | Score-Header zeigt Chip „ENTSCHIEDEN"; Abschluss-Bereich zeigt „Spiel entschieden – bitte abschließen"; irreführender Hinweis „Alle 5 Duelle …" entfernt |
| **Tests** | +9 in `SpielsteuerungServiceTests`: vorzeitige Entscheidung (3:0/3), Weiterspielen (2:1/3), Grenzfall (3:1/4), Unentschieden ändert Vorsprung nicht, Gleichstand→Stechen, Stechen erst bei klarem Sieger entschieden, Reihenfolge-Auslosung (Permutation + idempotent) → 96/96 |
| **Doku** | `docs/02_Fachkonzept/Spielablauf.md`: Abschnitt 8 auf Best-of-5 umgestellt, Auslosung der Spielerreihenfolge ergänzt |

### Schritt AC – KO-Phase vereinheitlicht: generischer Baum für alle Gruppenanzahlen + Gruppendurchmischung ✅ *(abgeschlossen, 2026-07-08)*

| Bereich | Änderung |
|---|---|
| **Fehlerbild** | `FinalrundeGenerieren` las fest nur `Gruppen[0]`/`Gruppen[1]` und der alte `GenerierenKoBaum` war auf genau zwei Gruppen (à ~6 Teams) hart verdrahtet. Bei 3/4 Gruppen fielen Gruppe C/D aus dem Bracket und es entstanden tote Slots (`TeamXId == null` **und** `VorgaengerSpielXId == null`) → Team ohne Gegner, Turnier blockiert; bei 1 Gruppe Exception. Auch 2×3 und ungerade/ungleiche 2-Gruppen-Größen (z. B. 5+4) waren im alten Pfad fehlerhaft |
| **Vereinheitlichung** | `FinalrundeGenerieren` nutzt jetzt für **jede** Gruppenanzahl den einen generischen Builder `GenerierenKoBaumGenerisch`; der alte 2-Gruppen-Sonderbaum wurde entfernt. Einzige Ausnahme: der **Kurz-Modus** bleibt 2-Gruppen-exklusiv (`GenerierenKurz`). Neue Überladung `FinalrundeGenerieren(Turnier, Random)` für deterministische Tests |
| **Generischer KO-Baum** | Alle Teams ziehen ein; Setzung ebenenweise nach Gruppenplatzierung. Bracketgröße = kleinste Zweierpotenz ≥ Teamzahl; Freilose nur bei Bedarf, stets an die Bestplatzierten. Rundennamen dynamisch (Finale/Halbfinale/Viertelfinale/Achtelfinale/…); Spiel um Platz 3 nur bei zwei echten Halbfinals. Keine toten Slots; `BracketFortsetzungAktualisieren` unverändert |
| **Gruppendurchmischung** | Neue Setzung mischt die Gruppen: über Zufalls-Restarts wird die Belegung mit den geringsten „Trennungskosten“ gewählt (`BesteBelegungFinden`/`TrennungsKosten`/`BegegnungsEbene`), sodass Teams derselben Gruppe möglichst spät aufeinandertreffen. Permutiert wird nur innerhalb einer Platzierungs-Ebene → Freilos-Vergabe an die Bestplatzierten bleibt erhalten |
| **UI** | Hinweistext am Finalrunden-Modus: die kurze Finalrunde gilt nur für zwei Gruppen; sonst wird stets ein KO-Baum erzeugt |
| **Tests** | +12 Theorie-Fälle in `FinalrundeTests` (1–4 Gruppen, inkl. 2×3, 2×4, 5+4, gleiche/ungleiche Größen): lückenloses Bracket ohne tote Slots, alle Teams genau einmal Startteilnehmer, vollständige Durchspielbarkeit, Freilose an Bestplatzierte, Spielanzahl = Teams−1 (+ Platz 3) sowie Gruppendurchmischung (keine Erste-Runde-Begegnung gleicher Gruppe über 20 Auslosungen). Bestehende 2-Gruppen-Tests laufen jetzt über den generischen Pfad → 87/87 |
| **Doku** | `docs/02_Fachkonzept/Finalrunde_und_Platzierungsspiele.md` auf einheitlichen Baum + Gruppendurchmischung aktualisiert |

### Schritt AB – Torschützen-Rangliste: geteilte Plätze & Stechen um Platz 1 ✅ *(abgeschlossen, 2026-07-08)*

| Bereich | Änderung |
|---|---|
| **StatistikService** | `TorschuetzenRangliste` vergibt jetzt geteilte Plätze bei exaktem Gleichstand im gewählten Wertungskriterium (Treffer bzw. Trefferquote per Bruchvergleich, keine Fließkomma-Rundungsfehler). Nur Platz 1 erfordert bei Gleichstand ein Stechen; alle anderen Plätze werden einfach geteilt (Standard-Sportranking, z. B. 1,2,2,4). Neue Methoden `GleichstandPlatz1` (liefert die gleichauf liegenden Spieler) und `StechenPlatz1Offen` (ob noch ein Sieger fehlt) |
| **Turnier** | Neues Feld `TorschuetzenStechenSiegerId` (nullable) speichert den manuell bestimmten Stechen-Sieger; wird ungültig (ignoriert), sobald sich die Gleichstandsgruppe ändert (z. B. durch Ergebniskorrektur) |
| **SiegerehrungViewModel (Bedienung)** | Neuer Bereich „Stechen um Platz 1 erforderlich": zeigt die gleichauf liegenden Spieler zur Auswahl, Turnierleitung führt das Stechen live durch und bestätigt den Sieger per Klick. Wechsel der Wertungsart (Absolut/Prozentual) setzt einen hinterlegten Stechen-Sieger zurück, da sich die Gleichstandsgruppe ändern kann |
| **GewinnerViewModel/-View (Anzeige)** | Spieler-Phase der Ehrung wird jetzt nach Platz gruppiert (`SiegerehrungSchritt.Spieler`, analog zur bestehenden Team-Gruppierung): mehrere gleichauf liegende Spieler erscheinen gemeinsam auf einer Folie (`ItemsControl` + `WrapPanel`) statt künstlich getrennter Plätze |
| **Tests** | +3 Tests in `StatistikServiceTests`: Gleichstand auf Platz 1 ohne Stechen (alle teilen sich Platz 1), mit hinterlegtem Stechen-Sieger (Sieger allein auf Platz 1, Rest teilt sich Platz 2), Gleichstand außerhalb Platz 1 (kein Stechen nötig) → 59/59 grün |

### Schritt AA – Auslieferung: Installer & feste Datenablage ✅ *(abgeschlossen, 2026-07-02)*

| Bereich | Änderung |
|---|---|
| **Feste Datenablage** | `App.xaml.cs` setzt beim Start das Arbeitsverzeichnis auf `%LOCALAPPDATA%\KirchenbirkigerSchluck`. Dadurch landen `turnier.json`, `backups/` und `logos/` unabhängig vom Startort immer im selben Ordner. Keine Änderung an LogoService/Convertern/Repository nötig (relative Pfade bleiben portabel) |
| **App-Icon** | `Resources/Assets/AppIcon.ico` (Multi-Resolution, aus dem Logo generiert); als `<ApplicationIcon>` in der csproj eingebunden (Explorer/Taskleiste/Fenster). Produktversion `<Version>1.0.0</Version>` |
| **Veröffentlichung** | PublishProfile `Properties/PublishProfiles/win-x64.pubxml` (self-contained, win-x64, Ordner-Publish) |
| **Installer** | Inno-Setup-Skript `installer/KirchenbirkigerSchluck.iss`: per-user Installation (ohne Admin) nach `%LOCALAPPDATA%\Programs\KirchenbirkigerSchluck`, Desktop-/Startmenü-Verknüpfung, Uninstaller (Datenordner bleibt erhalten) |
| **Build-Skript** | `veroeffentlichen.ps1` (Root): `dotnet publish` + ISCC in einem Schritt; weist auf `winget install JRSoftware.InnoSetup` hin, falls Inno Setup fehlt |
| **Doku** | `docs/06_Betrieb/EXE_erstellen.md` (Build- und Auslieferungsanleitung); `.gitignore` um `publish/` + `installer/Output/` ergänzt |

### Schritt Z – UI/UX-Redesign: Token-Designsystem & dunkles Theme ✅ *(abgeschlossen, 2026-07-02)*

| Bereich | Änderung |
|---|---|
| **Designsystem** | Neues Token-System unter `Resources/Design/` (`00_Konverter`, `01_Tokens`, `02_Effekte`, `03_Typografie`, `04_Bedienung`, `05_Anzeige`): primitive Farben → semantische Pinsel, Abstands-Skala (4er-Raster), Radien, Motion-Dauern/Easings. Hex-Werte nur noch in `01_Tokens.xaml` (Ausnahme: Bühnenbild der Auslosung). `Resources/Styles.xaml` aufgelöst |
| **Bedienoberfläche** | Umstellung auf dunkles Theme (`CustomColorTheme BaseTheme=Dark`, Primär Gold `#FFC84B`, Sekundär Violett `#7E57C2`) mit Marken-Nachttönen statt Material-Grau. Nav-Rail mit animiertem Selektions-Indikator (150 ms). Gemeinsame Styles: Status-Pillen, Spielplan-Zeilen, Hinweis-Flächen |
| **Anzeigeoberfläche** | Broadcast-Look: weiche Screen-Übergänge (`ScreenUebergangVerhalten`, 400 ms Crossfade+Slide), Infoscreen-Folienwechsel animiert, Score-Pop bei Wertänderung (`WertPulsVerhalten`), Stechen mit rotem Score-Glow, Siegerehrung mit 700-ms-Bühneneinblendung, atmende Uhr am Startscreen. Gedämpfter Text `#9090B0 → #A6A6C6`, Beamer-Minimum 22 px. `AutoScrollVerhalten` an der Spielplan-Folie verdrahtet (vorher gestauchtes UniformGrid) |
| **Wiederverwendbare Controls** | `Views/Gemeinsam/`: `TeamAnzeige` (Logo+Name, ersetzt 6 duplizierte Blöcke), `LiveIndikator`, `LeererZustand` (erklärende Leerzustände in Spielsteuerung/Teams/Gruppen/Tabellen/Korrektur) |
| **Unverändert** | Alle ViewModels, Bindings, DataTemplate-Zuordnungen und die Auslosungs-Choreografie (`AuslosungView.xaml.cs`) – reine XAML-/Ressourcen-Änderungen |
| **Doku** | `docs/03_UI_UX/Designrichtlinie.md` als kanonische Designrichtlinie gefüllt (Tokens, Typo-Rampen, Motion-Regeln, Kontrast) |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 56/56 grün

### Schritt Y – Siegerehrung: geteilte Plätze & Punkte ✅ *(abgeschlossen, 2026-07-02)*

| Bereich | Änderung |
|---|---|
| **GewinnerViewModel** | Team-Phase der Ehrung wird nach Platz gruppiert (`SiegerehrungSchritt.Teams`): Mannschaften mit geteiltem Platz erscheinen gemeinsam auf **einer** Folie statt nacheinander. Team-Schritte tragen keine Punktangabe mehr (`Detail` leer) |
| **GewinnerView** | Zwei Layouts je Schritt: Spieler-Layout (einzeln, mit Detail) und Team-Layout (`ItemsControl` + `WrapPanel`, Teams nebeneinander). Platznummer prominent und pro Folie nur einmal oben; keine Team-Punkte |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 56/56 grün

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
| `SpielplanService` | `GruppenspielplanGenerieren` (Round-Robin, interleaved), `NaechstesSpielErmitteln`, `SpielNachHintenVerschieben`, `FinalrundeGenerieren` (KoBaumEin/Kurz), `BracketFortsetzungAktualisieren` | **14** |

### Services & Klassen – Data

| Klasse | Implementierte Methoden | Tests |
|---|---|---|
| `BackupManager` | `DateinameGenerieren`, `DateinameSanitieren`, `BackupErstellen`, `AlleDateien` | **4** |
| `TurnierRepository` | `Laden`, `Speichern`, `ExistiertDatei` | **3** |
| `TurnierService` | `TurnierErstellen`, `TurnierLaden`, `TurnierSpeichern`, `StatusWechseln`, `TeamHinzufuegen`, `TeamZurueckziehen` | **8** |
| `AenderungsprotokollService` | `EintragErstellen`, `EintraegeAbfragen` | **6** |
| `SchemaMigration` | `MigrationErforderlich`, `AktuelleVersion` (trivial) | – |

---

## Abgeschlossene Implementierungsschritte

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

### Schritt C – AenderungsprotokollService ✅ *(abgeschlossen)*

| Methode | Beschreibung |
|---|---|
| `EintragErstellen()` | `Aenderungseintrag` anlegen + an `Turnier.Aenderungsprotokoll` appenden |
| `EintraegeAbfragen()` | Einträge für eine Entität filtern, absteigend nach Zeitstempel |

Tests: EintragErstellen (Felder, Begruendung null), EintraegeAbfragen (Filter, Sortierung, leere Liste)

---

### Schritt D – FinalrundeGenerieren ✅ *(abgeschlossen)*

| Methode | Beschreibung |
|---|---|
| `FinalrundeGenerieren()` – Modus `Kurz` | Gleichplatzierte aus beiden Gruppen direkt gegeneinander (A1 vs B1, A2 vs B2, …); das Spiel um Platz 1/2 wird zuletzt gespielt (höchste Spielnummer) |
| `FinalrundeGenerieren()` – Modus `KoBaumEin` | Cross-geseedeter KO-Baum (Achtelfinale → Viertelfinale → Halbfinale → Spiel um Platz 3 → Finale); alle Spiele sofort generiert, TBD-Slots mit `null`-Team-IDs und `VorgaengerSpielId` |
| `BracketFortsetzungAktualisieren()` | Trägt Sieger eines abgeschlossenen Bracket-Spiels automatisch in die Folgerunde ein |

Modeländerungen: `Spiel.Team1Id/Team2Id` → `Guid?` (nullable), `VorgaengerSpiel1Id/2Id`, `BracketRunde` ergänzt.
`FinalrundenModus`-Enum (`KoBaumEin`, `Kurz`) neu angelegt. (Der frühere Modus `KoBaumZwei` – zwei separate Brackets ohne gemeinsames Finale – wurde entfernt, da fachlich nicht benötigt.)

Tests: Kurz_ZweiGruppen_ErzeugtKorrektePaarungen, Kurz_SpielUmPlatzEins_WirdZuletztGespielt, Kurz_AnzahlSpieleEntsprichtKleinstenGruppe, KoBaum_ZweiGruppenJe6_ErzeugtVierAchtelfinaleSpiele, KoBaum_ZweiGruppenJe6_ErzeugtViertelfinaleAlsPlatzhalter, KoBaum_ZweiGruppenJe6_KoBaumEin_ErzeugtFinale, KoBaum_ZweiGruppenJe5_ErzeugtZweiAchtelfinaleSpiele, BracketFortsetzung_TraegtSiegerInNaechsteRunde

---

### Schritt X – KO-Platzierung, Platz-3 vor Finale, Torschützen-Wertung umstellbar ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Siegerehrung nach KO-Baum** | Endplatzierung wird nicht mehr nach Tabellenpunkten, sondern nach dem Abschneiden im Turnierbaum berechnet: Platz 1/2 aus dem Finale, 3/4 aus dem Spiel um Platz 3, danach geteilte Plätze je nach Ausscheide-Runde (Viertelfinal-Verlierer teilen Platz 5, Achtelfinal-Verlierer den nächsten Block usw.). Kurz-Modus über die „Platz X/Y"-Spiele, Fallback nach Gruppenpunkten |
| **Spiel um Platz 3 vor dem Finale** | Wird jetzt mit kleinerer Spielnummer als das Finale erzeugt → erscheint im Spielplan und Baum direkt vor dem Finale |
| **Torschützen-Wertung umstellbar** | Im Siegerehrungs-Tab kann die Wertung (Absolut/Prozentual) auch nachträglich geändert werden; bei „Prozentual" zeigt die Ehrung „60% – 6 von 10 Versuchen" |
| **Finalrunde neu erzeugen** | Button im Turnier-Tab (aktiv in der Finalrunde): baut den Finalrunden-Spielplan inkl. Spiel um Platz 3 aus den aktuellen Gruppentabellen neu auf – nötig für Turniere, deren Finalrunde noch aus einer Version ohne Platz-3-Spiel stammt. Übernimmt den gewählten Finalrunden-Modus |
| **Bracket: Spiel um Platz 3 unter dem Finale** | Im Infoscreen-Baum wird das Spiel um Platz 3 unterhalb des Finales platziert (eigene Beschriftung „Spiel um Platz 3") und mit **gestrichelter** Linie von den Halbfinals verbunden (Verlierer-Weg), das Finale weiterhin mit durchgezogener Linie (Sieger-Weg) |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 56/56 grün

---

### Schritt W – Bracket-Linien, Folien-Auswahl, Torschützenkönig, Siegerehrung ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Umsetzung |
|---|---|
| **Echte Bracket-Linien** | Finalrunden-Baum als positionierte Knoten auf Canvas (Viewbox-skaliert) mit Elbow-Verbindungslinien je Vorgänger → durchgehende Verbindung sichtbar, wer gegen wen weiterkommt |
| **Finalphase-Infoscreen** | In Finalrunde/Abgeschlossen zeigt der Infoscreen nur noch Nächste/Letzte Partie + Baum (Tabellen & Spielplan automatisch ausgeblendet) |
| **Folien manuell wählbar** | Neue `InfoscreenEinstellungen` (Singleton) + Schalter im Einstellungen-Tab für jede Folie |
| **Torschützenkönig-Wertung** | Neue `Turnier.TorschuetzenWertung` (Absolut/Prozentual), wählbar bei Turniererstellung; `StatistikService` aggregiert Treffer/Versuche je Spieler und rankt entsprechend |
| **Schrittweise Siegerehrung** | `GewinnerViewModel` als Ehrungs-Abfolge: erst 5 treffsicherste Spieler (Platz 5→1, Detail je nach Wertung „12 Treffer" bzw. „60% – 6 von 10 Versuchen"), dann Teams (schlechtester→Sieger). Jeder Schritt zeigt eine Person/Team groß mit Logo + Detail |
| **Siegerehrungs-Menü** | Neuer Verwaltungs-Tab „Siegerehrung" mit Start / Weiter / Zurück und Vorschau des aktuellen Schritts |
| **Tests** | +1 Statistik-Test (Absolut vs. Prozentual) → 56/56 grün |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 56/56 grün

---

### Schritt V – VS-Zentrierung + Spiel um Platz 3 ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **VS/Ergebnis zentriert** | „Nächste Partie" und „Letzte Partie" nutzen `Grid.IsSharedSizeScope` + `SharedSizeGroup` → beide Teamspalten gleich breit, VS bzw. Ergebnis sitzt immer exakt mittig, unabhängig von der Namenslänge |
| **Spiel um Platz 3** | Neues `Spiel.VorgaengerVerlierer`-Flag; KoBaumEin erzeugt zusätzlich „Spiel um Platz 3" aus den beiden Halbfinal-Verlierern. `BracketFortsetzungAktualisieren` trägt bei diesem Spiel den Verlierer statt des Siegers ein |
| **Bracket-Verbinder** | Verbinder-Stummel (goldene Linien) links/rechts an den Bracket-Karten deuten den Weg durch den Baum an |
| **Tests** | +2 Finalrunden-Tests (Platz-3-Erzeugung + Verlierer-Weiterleitung) → 55/55 grün |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 55/55 grün

---

### Schritt U – Treffer-Spinner + Spielplan-Aktionsleiste ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Treffer-Spinner** | Trefferzahl je Spieler nur noch per +/−-Buttons (0–3 begrenzt), keine freie Tastatureingabe mehr; bei Änderung werden Sieger, Duellpunkte und der Live-Spielstand sofort neu berechnet. Tabellenpunkte aktualisieren sich beim erneuten Abschließen des Spiels |
| **Spielplan-Aktionsleiste** | Beim Auswählen eines Matches erscheinen Buttons: **Bearbeiten** (öffnet das Spiel ausführlich in der Spielsteuerung – Ergebnis, Treffer, Spielerauswahl editierbar), **Spiel starten**, **Matchscreen anzeigen**, **Neustarten** (mit Sicherheitsabfrage), **Nach hinten**. Buttons sind je nach Spielstatus aktiv/inaktiv. Kontextmenü durch sichtbare Aktionsleiste ersetzt |
| **Mehrfach-Schutz** | „Bearbeiten" verhindert, dass zwei Spiele gleichzeitig laufen |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 53/53 grün

---

### Schritt T – Spiellogik-Korrekturen + Duell-Bearbeitung ✅ *(abgeschlossen, 2026-06-13)*

| Nr | Punkt | Umsetzung |
|----|-------|-----------|
| 1 | Stechen-Marker | `StehenErforderlich` wird nur noch gesetzt, wenn alle regulären Gruppenspiele abgeschlossen sind (kein „Stechen" mehr bei 0 Spielen) |
| 2 | Spielplan-Reihenfolge | Verwaltungs-Spielplan durchgehend nach Spielnummer sortiert (Gruppen abwechselnd); ListView-Items füllen die volle Breite → Spalten (Status/Ergebnis/Runde) korrekt untereinander |
| 3 | Duell-Ergebnis | Anzeige zeigt jetzt die echte Trefferzahl (z. B. 3:3, 2:1, 0:0) statt „½:½". Punkteregel unverändert korrekt (beide 1 Punkt bei ≥1 Treffer, 0:0 keiner) |
| 4/5 | Duell-Bearbeitung | Neues editierbares Duell-Modell: Spieler per ComboBox tauschbar, Trefferzahl per Tastatur änderbar; Sieger/Duellpunkte werden aus der Trefferzahl abgeleitet, Versuche rekonstruiert. Ersetzt den fehleranfälligen „Letzten Versuch rückgängig"-Button |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 53/53 grün

---

### Schritt S – Automatisches KO-Stechen (Punkt 8 vervollständigt) ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Modell** | `Spiel.IstPlatzierungsStechen` – KO-Spiel zur Auflösung gleicher Gruppenplatzierungen (zählt nicht für Tabellenpunkte) |
| **WertungsService** | Stechen-Spiele werden aus der Punktewertung ausgenommen und als dritte Tiebreaker-Stufe (nach Direktem Vergleich) zur Reihenfolge genutzt; `StehenErforderlich` nur noch wenn auch nach Stechen gleichauf |
| **SpielplanService** | `PlatzierungsStechenErzeugen(turnier)` legt für gleichplatzierte Teams Round-Robin-Stechenspiele an (ohne Duplikate) |
| **Auto-Abschluss** | Bei offenem Gleichstand erzeugt die Spielsteuerung automatisch die Stechen-Spiele (statt zu blockieren); nach deren Austragung wird die Finalrunde automatisch erstellt |
| **Tests** | +2 Tests: Stechen löst Gleichstand auf (Wertung), Stechen wird genau einmal erzeugt (Spielplan) → 53/53 grün |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 53/53 grün

---

### Schritt R – 11-Punkte-Paket ✅ *(abgeschlossen, 2026-06-13)*

| Nr | Punkt | Umsetzung |
|----|-------|-----------|
| 1 | Logos überall | Logos neben Teamnamen in Teamverwaltung, Spielplan, Matchday, Infoscreen (Nächste/Letzte Partie, Tabelle, Spielplan, Bracket), Gewinner, Auslosung |
| 2 | Bildschirmfüllend | Infoscreen-Tabelle/Spielplan und Gewinner-Liste über `UniformGrid Columns=1` → Zeilen verteilen sich über die volle Höhe |
| 3 | Auslosungs-Button | „Zur Gruppenauslosung" im Turnier-Tab (navigiert zum Gruppen-Tab) |
| 4 | Tabellen-Tab | Neuer Tab „Tabellen" im Verwaltungsbildschirm (Gruppenranglisten mit Logo, Duelldifferenz, Stechen-Markierung) |
| 5 | „Duelle"-Text | Im Infoscreen entfernt (Letztes Spiel zeigt nur noch „4 : 3") |
| 6 | Spielplan-Reihenfolge | Round-Robin nach Kreis-Verfahren: jede Mannschaft einmal pro Runde (keine Doppelspiele hintereinander), Gruppen game-by-game abwechselnd |
| 7 | Finalrunde automatisch | Nach dem letzten Gruppenspiel wird die Finalrunde automatisch erzeugt + Status gewechselt; Bracket-Baum als Folie im Infoscreen |
| 8 | Tiebreaker/Stechen | Duelldifferenz + „Stechen"-Badge in Tabelle/Infoscreen sichtbar; Auto-Finalrunde wird bei offenem Stechen blockiert (Hinweis). **Offen:** automatisches Erzeugen eines KO-Stechen-Spiels zur Platzierungsauflösung |
| 9 | Korrektur im Match | „Letzten Versuch rückgängig" in der Spielsteuerung (öffnet ein entschiedenes Duell ggf. wieder) |
| 10 | Spielplan-Optionen | Kontextmenü je Spiel: Bearbeiten (Ergebnis korrigieren – öffnet abgeschlossenes Spiel wieder), Matchscreen anzeigen, Spiel/Duell starten |
| 11 | Spielplan im Infoscreen | Neue Spielplan-Folie (kommende Spiele mit Logos) in der Infoscreen-Rotation |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt Q – Team-Logos + animierte Beamer-Auslosung ✅ *(abgeschlossen, 2026-06-13)*

**Team-Logos:**

| Bereich | Änderung |
|---|---|
| **Datenmodell** | `Team.LogoPfad` (optionaler relativer Pfad) |
| **LogoService** | Kopiert hochgeladene Bilder nach `logos/<teamId>.<ext>`, liefert relativen Pfad; entfernt alte Logos |
| **Konverter** | `PfadZuBitmapConverter` (lädt Bild ohne Dateisperre via OnLoad), `PfadZuVisibilityConverter` (blendet Logo-Element aus wenn keine Datei) |
| **Teamverwaltung** | „Logo hochladen"/„Entfernen" mit Vorschau; Logo-Thumbnail im Team-Listeneintrag |
| **Matchday (Beamer)** | Team-Logos über den Teamnamen eingeblendet, wenn vorhanden |

**Animierte Auslosung am Beamer (DFB-Pokal-Stil):**

| Bereich | Änderung |
|---|---|
| **AnzeigeZustandService** | Neuer Screen `Auslosung`, Datentyp `AuslosungDaten`/`AuslosungEintrag`, Events `AuslosungGestartet`/`AuslosungAbgeschlossen`, Methoden `AuslosungAmBeamerStarten`/`AuslosungBeenden` |
| **AuslosungAnzeigeViewModel** | Hält Ziehungsreihenfolge + Live-Zustand (Kugelinhalt, Gruppenspalten); ordnet Teams ein; meldet Fertigstellung |
| **AuslosungView** | Lostrommel (rotierende Glaskugel mit Kugeln), aufsteigende Loskugel die sich in zwei Halbschalen öffnet und Logo+Name enthüllt, danach Einordnung in die Gruppenspalte; Choreografie im Code-Behind (async + DoubleAnimations, ~5 s/Team für Spannung) |
| **Ablauf** | Operator startet im Gruppen-Tab → `AuslosungAmBeamerStarten` (Anzeige wechselt auf Auslosung, Animation läuft) → nach Abschluss schreibt der Operator-VM die Einteilung ins Turnier (`AuslosungAbgeschlossen`). Operator-Tab zeigt während der Animation „läuft am Beamer" |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt P – Turnier-Reset ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Reset-Button** | Neben „Status weiterschalten" im Turnier-Tab gibt es jetzt „Zurücksetzen" (rot, mit Sicherheitsabfrage). Setzt das Turnier auf „In Vorbereitung" zurück: Gruppeneinteilung, Spielplan, Finalrunde und Ergebnisse werden verworfen, **Teams und Spieler bleiben erhalten**. Anzeige wechselt zurück auf den Startscreen |
| **Command-Logik** | Neues `TurnierZuruecksetzenCommand` (nur aktiv wenn Status ≠ InVorbereitung). `StatusWechselnCommand` und das Reset-Command werden in `TurnierInfoAktualisieren` explizit neu bewertet, da ein reiner Statuswechsel `HatGeladenesTurnier` nicht ändert |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt O – Teamverwaltung-Fix, Gruppenauslosung, Spielplan-Politur ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Teamverwaltung-Fix** | `TeamverwaltungView.xaml.cs` (Code-Behind) fehlte → die View wurde leer instanziiert (kein `InitializeComponent`). Nachgereicht; Teams werden jetzt angezeigt und sind editierbar |
| **Gruppen-Tab + Auslosung** | Neuer Tab „Gruppen" mit animierter Auslosung (DFB-Pokal-Prinzip): Teams werden gemischt und per Timer abwechselnd je Gruppe gezogen; jede Team-Kugel poppt animiert in ihre Gruppe (BackEase). Ziehungs-Hero zeigt das aktuell gezogene Team + Zielgruppe. Danach Gruppen-Einteilungsübersicht; „Neu auslosen" möglich solange in Vorbereitung. Neues `GruppenAuslosungViewModel` + View, DI, Tab-Index-Anpassung (Spielstart → Index 4) |
| **Gruppenbildung verlagert** | Die Zuteilung passiert jetzt in der Auslosung (persistiert in `turnier.Gruppen`). `TurnierVerwaltungViewModel.StatusWechseln` erzeugt keine Gruppen mehr selbst, sondern verlangt eine vorhandene Auslosung (sonst Hinweis). `AnzahlGruppen`-Auswahl vom Turnier-Tab in den Gruppen-Tab verschoben |
| **Spielplan-Politur** | Hero-Karte „Nächstes Spiel" mit Verlauf + Akzent-Button; Spielzeilen als Karten mit farbigen Status-Pills (Läuft=grün, Abgeschlossen=grau, Verschoben=orange, Abgesetzt=rot), goldener Akzentrand für das nächste Spiel, „vs"-Chip, klarer Spaltenheader |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt N – Crash-Fix, Teamverwaltung, Auto-Scroll ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **Crash-Fix Spielplan** | `SpielplanView` stürzte ab, weil `ItemContainerStyle` auf den nicht auflösbaren Ressourcen-Key `MaterialDesignListViewItem` verwies. Statusfarben (Läuft/Abgeschlossen/Abgesetzt) jetzt über einen Border in der Zeilenvorlage – ohne fremde Style-Abhängigkeit |
| **Teamverwaltung (neuer Tab)** | Eigener Menüpunkt „Teamverwaltung" mit großflächigem 2-Spalten-Layout: links Teamliste + Team anlegen, rechts Teamdetails (Name/Kürzel editierbar) und Spielerverwaltung (hinzufügen, umbenennen mit Auto-Save, entfernen). Team entfernen nur in Vorbereitungsphase. Neues `TeamverwaltungViewModel` + `TeamverwaltungView`, DI-Registrierung, Tab-Index-Anpassung (Spielstart-Navigation → Index 3) |
| **Turnier-Tab entschlackt** | Beengte Spielerverwaltung entfernt; Verweis-Box auf den neuen Teamverwaltungs-Tab |
| **Speichern/Laden mit Feedback** | Beide Buttons gaben keine Rückmeldung → wirkten funktionslos. Jetzt Bestätigungs-Dialoge; Laden fängt `FileNotFoundException` mit klarer Meldung ab |
| **Auto-Scroll Anzeige** | Neues Attached-Behavior `AutoScrollVerhalten`: scrollt lange Listen langsam (Credits-Stil: Pause oben → abwärts → Pause unten → aufwärts → Schleife), nur bei Überlauf. Angewendet auf Gewinner-Platzierungen und Infoscreen-Gruppentabelle |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt M – Visuelles Redesign ✅ *(abgeschlossen, 2026-06-13)*

Ziel: Anzeige (Beamer) maximal modern/aufregend für das Publikum, Bedienung maximal effektiv/intuitiv.

**Anzeige (Beamer):**

| Bereich | Änderung |
|---|---|
| **Styles.xaml** | Komplett neues Design-System: diagonaler Mitternachts-Farbverlauf als Hintergrund, goldener Akzent-Verlauf, Glow-Effekte (Gold/Rot), Karten-Schatten, schwebende Karten-Style |
| **Score-Typografie** | Score auf 120px mit goldenem Glühen; Teamnamen 44px; Sieger 84px mit Glow |
| **MatchdayView** | Dramatischer Score-Header mit Verlauf, pulsierender „LIVE"-Indikator (Storyboard-Animation), Duell-Zeilen als abgerundete Karten mit Nummern-Kreisen und Ergebnis-Pillen |
| **StartscreenView** | Radialer Lichtschein hinter Logo, Logo-Schatten, goldene Trennlinie, größere Uhrzeit, Begrüßungszeile |
| **InfoscreenView** | Karten-basierte Folien mit Schatten, „VS"-Badge, Akzent-Balken im Header, modernisierte Gruppentabelle mit Zeilen-Karten |
| **GewinnerView** | Goldener Lichtschein, Sieger mit Stern-Rahmung + Glow, Platzierungen als Karten mit Platz-Kreisen |
| **AnzeigeWindow** | Hintergrund auf `#0B0B1A` (tiefer) angepasst |

**Bedienung (Operator):**

| Bereich | Änderung |
|---|---|
| **App.xaml** | Sekundärfarbe Lime → Amber (Markenkohärenz mit dem Gold der Anzeige) |
| **BedienWindow** | Neue dunkle Navigationsleiste (220px) mit Marken-Header („Kirchenbirkiger Schluck / TURNIERLEITUNG"), goldenem Akzentbalken, custom TabItem-Template mit klarer Auswahl-Markierung (goldener Randbalken + Hervorhebung), Hover-Zustand |
| **Versuch-Buttons** | Höhe 60→72px für schnellere, treffsicherere Bedienung während des Spiels |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt L – Operator-UX ✅ *(abgeschlossen, 2026-06-13)*

| Bereich | Änderung |
|---|---|
| **SpielplanView** | `ItemContainerStyle` mit DataTriggern: laufende Spiele grün + halbfett, abgeschlossene Spiele auf 50 % Opacity gedimmt, abgesetzte auf 25 % |
| **SpielsteuerungView** | „Spiel abschließen" immer sichtbar (war: nur bei KannSpielAbschliessen=True); Button ist disabled solange Bedingung nicht erfüllt; erklärender Hinweistext sichtbar wenn noch nicht alle 5 Duelle gespielt |
| **TurnierVerwaltungViewModel** | `KannTeamHinzufuegen()` prüft jetzt Status = InVorbereitung — Teams können nach Gruppenphase-Start nicht mehr hinzugefügt werden |
| **TurnierVerwaltungView** | Hinweis-TextBlock erscheint ab Gruppenphase: „Teams können nach dem Start nicht mehr geändert werden." |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt K – UX-Feinschliff ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Änderung |
|---|---|
| **EinstellungenViewModel** | Neue `BackupEintragModel`-Record mit `Pfad` + `Anzeigename`; Backup-Liste zeigt jetzt nur Dateinamen, nicht mehr volle Pfade |
| **EinstellungenView** | `SelectedItem="{Binding AusgewaehltesBackup}"` + `{Binding Anzeigename}` statt `{Binding}` auf vollem Pfad |
| **MatchdayViewModel** | Duell-Ergebnis: `✓ –` / `– ✓` → `1 : 0` / `0 : 1` / `½ : ½` — für Beamerpublikum eindeutig lesbar |
| **SpielsteuerungViewModel** | Gleiches Format für Konsistenz zwischen Bedien- und Anzeigeoberfläche |
| **AnzeigeWindowViewModel** | `ScreenWechseln()` ruft beim Wechsel auf Infoscreen `InfoscreenVm.Aktualisieren()` auf — Rotation startet immer ab Folie 1 (Nächste Partie) statt an zufälliger Stelle |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt J – UI-Polishing ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Änderung |
|---|---|
| **VersuchButtonStyle** | `BasedOn="{StaticResource MaterialDesignRaisedButton}"` ergänzt — vorher verloren die 4 Buttons in SpielsteuerungView das gesamte Material-Design-Styling |
| **GewinnerView** | Sieger-Box von `#1A1A00` (praktisch unsichtbar auf `#1A1A2E`) auf `#2A2400` mit goldenem Top/Bottom-Border geändert — der Höhepunkt des Abends ist jetzt visuell präsent |
| **PlatzierungZeileStyle** | Sieger-Zeile: `#2D2D00` → `#3A3200` + goldener Linker Rand (4px) für klare Hervorhebung |
| **KorrekturView** | DataGrid-Card wird ausgeblendet wenn keine Einträge — vorher zeigte sie eine leere Tabelle auf demselben Grid.Row wie der Hinweistext |
| **GewinnerView** | Emoji-Text „🏆 Turniersieger" durch profesionelles „— Turniersieger —" ersetzt |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt I – Anzeige-Qualität ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Änderung |
|---|---|
| **MatchdayViewModel** | `SpielAktualisieren()` zeigt jetzt alle 5 geplanten Duell-Slots sofort ab Spielstart (IstGeplant-Vorschau), nicht erst nach dem ersten erfassten Duell |
| **MatchdayView** | Opacity-Trigger (0.4) für `IstGeplant`-Zeilen — geplante Duelle werden gedimmt dargestellt |
| **InfoscreenView** | Tabellen-Header mit Spaltenbezeichnungen Pl. / Team / Sp. / S. / Pkt. — vorher war der Header leer und nach der Datentabelle positioniert |
| **GewinnerViewModel** | Sieger-Erkennung funktioniert jetzt auch für Kurz-Modus: Fallback auf „Platz 1/2"-Spiel wenn kein „Finale"-Bracket vorhanden |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt H – UI-Bugfixes + Setup-Führung ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Änderung |
|---|---|
| **SpielsteuerungView** | XAML-Spalten-Bug behoben: `vs.`-Label und ComboBox2 lagen beide in Grid.Column 2 und überlagerten sich |
| **Platzhalter-Spieler** | Beim Übergang zu Gruppenphase erhalten Teams ohne Spieler automatisch „Spieler 1–5" |
| **Auto-Screen-Wechsel** | Anzeigeoberfläche wechselt bei Start der Gruppenphase/Finalrunde automatisch auf Infoscreen |
| **Setup-Führung** | Hinweistext (NaechsterSchrittHinweis) unter dem Status-Chip; Spielerverwaltungs-Header immer sichtbar; Hinweis wenn kein Team ausgewählt |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt G – Erste lauffähige Version ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Änderung |
|---|---|
| **Gruppen-Setup** | `TurnierVerwaltungViewModel`: Anzahl Gruppen (1–4) und FinalrundenModus konfigurierbar |
| **Spielplan-Generierung** | `StatusWechseln` InVorbereitung→Gruppenphase: erstellt Gruppen, verteilt Teams, ruft `GruppenspielplanGenerieren()` auf |
| **Finalrunden-Generierung** | `StatusWechseln` Gruppenphase→Finalrunde: setzt `FinalrundenModus` + ruft `FinalrundeGenerieren()` auf |
| **Spieler-Verwaltung** | Teams-Card: Team auswählen → Spieler per Name hinzufügen; wird für Duell-ComboBoxen verwendet |
| **Auto-Load beim Start** | `App.xaml.cs`: lädt `turnier.json` beim Start automatisch (stilles Scheitern wenn nicht vorhanden) |
| **Auto-Backup** | `SpielsteuerungViewModel.SpielAbschliessen`: `BackupManager.BackupErstellen()` nach jedem Spielabschluss |
| **BracketFortsetzung** | `SpielsteuerungViewModel.SpielAbschliessen`: `BracketFortsetzungAktualisieren()` bei Finalrundenspielen |
| **Gewinner-Screen** | `StatusWechseln` Finalrunde→Abgeschlossen: schaltet Anzeige automatisch auf Gewinner-Screen |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

---

### Schritt E – SchemaMigration *(niedrige Priorität, V2-relevant)*

`SchemaMigration.Migrieren()` – in V1 existiert nur eine Schemaversion; kein echtes Migrationsszenario.
Wird relevant, wenn ein zweites Datenformat (V2) eingeführt wird.

---

### Schritt F – UI: BedienWindow & AnzeigeWindow ✅ *(abgeschlossen, 2026-06-12)*

| Bereich | Beschreibung |
|---|---|
| **Services** | `TurnierZustandService` (Turnier-State + Event), `AnzeigeZustandService` (Screen-Steuerung + SpielZustand-Event) |
| **Anzeige-ViewModels** (5) | `StartscreenViewModel` (Uhrzeit-Timer), `InfoscreenViewModel` (rotierende Slides), `MatchdayViewModel` (Live-Duell-Anzeige), `GewinnerViewModel` (Endplatzierung), `AnzeigeWindowViewModel` (Shell + DataTemplate-Dispatch) |
| **Bedienungs-ViewModels** (6) | `TurnierVerwaltungViewModel`, `SpielplanViewModel`, `SpielsteuerungViewModel`, `KorrekturViewModel`, `EinstellungenViewModel`, `BedienWindowViewModel` (Shell + Tab-Navigation) |
| **Anzeige-Views** (4) | `StartscreenView`, `InfoscreenView`, `MatchdayView`, `GewinnerView` (UserControls) |
| **Bedienungs-Views** (5) | `TurnierVerwaltungView`, `SpielplanView`, `SpielsteuerungView`, `KorrekturView`, `EinstellungenView` (UserControls) |
| **BedienWindow** | TabControl (links) + ContentControl (rechts) mit DataTemplate-Dispatch; 5 Tabs (Icon + Label) |
| **AnzeigeWindow** | ContentControl mit DataTemplates für alle 4 Screen-VMs; dunkel, Vollbild |
| **Ressourcen** | `Resources/Styles.xaml` (Anzeige-Typografie, Score-Styles, Versuch-Button-Styles), Logo-Asset |
| **DI-Verdrahtung** | Alle Services + VMs als Singleton, Fenster als Transient; Multi-Screen-Positionierung |

Build: ✅ 0 Fehler, 0 Warnungen | Tests: ✅ 51/51 grün

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

Schritt D  ──── fertig ✅ (FinalrundeGenerieren + BracketFortsetzung)
Schritt E  ──── niedrige Priorität (V2)
```
