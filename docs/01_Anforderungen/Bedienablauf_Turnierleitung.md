# Bedienablauf Turnierleitung

Dieses Dokument beschreibt den geplanten Bedienablauf für die Turnierleitung im Projekt **Kirchenbirkiger Schluck Programm**.

Es dient als fachliche Grundlage für die spätere Bedienoberfläche, die Spielsteuerung, die Ergebniserfassung und die Steuerung der Zuschaueranzeige.

## 1. Zweck des Dokuments

Die Turnierleitung benötigt während des Turniers eine robuste und schnelle Bedienoberfläche.

Die Anwendung soll die Turnierleitung dabei unterstützen,

- ein Turnier vorzubereiten,
- Mannschaften und Spieler zu verwalten,
- den Spielplan zu steuern,
- Ergebnisse während einer Partie zu erfassen,
- Korrekturen vorzunehmen,
- die Zuschaueranzeige zu steuern,
- Daten zu speichern und bei Bedarf wiederherzustellen.

Dieses Dokument beschreibt den fachlichen Bedienablauf. Es enthält noch keine technische Umsetzung.

## 2. Grundprinzip

Die Anwendung besteht im laufenden Betrieb aus zwei Bereichen:

- **Bedienoberfläche** auf dem Laptop für die Turnierleitung
- **Zuschaueranzeige** auf einem externen Bildschirm oder Fernseher

Standardzuordnung:

```text
Laptopbildschirm: Turnierleitung / Bedienung
Externer Bildschirm: Zuschaueranzeige / Infoscreen
```

Die Zuschaueranzeige läuft grundsätzlich im Vollbild. Die Bedienoberfläche bleibt für die Turnierleitung sichtbar und bedienbar.

## 3. Vor dem Turnier

Vor Turnierbeginn muss die Turnierleitung ein Turnier anlegen oder ein bestehendes Turnier laden können.

### 3.1 Turnier anlegen

Beim Anlegen eines neuen Turniers müssen mindestens folgende Pflichtangaben erfasst werden:

| Feld | Pflicht | Nachträglich änderbar |
|------|---------|----------------------|
| Mannschaften (mind. 2) | Ja | Ja |
| Datum | Ja | Ja |
| Turniername | Nein | Ja |
| Veranstaltungsjahr | Nein | Ja |
| Uhrzeit | Nein | Ja |
| Ort | Nein | Ja |
| Hinweistext | Nein | Ja |
| Turnierformat | Nein | Ja (vor Turnierbeginn) |
| Spieler je Mannschaft | Nein | Ja |

Alle optionalen Felder können auch nach dem Anlegen des Turniers ergänzt oder geändert werden. Datum und Mannschaften sind zwar Pflichtfelder, müssen aber ebenfalls nachträglich korrigierbar bleiben.

Die Turnierdaten sollen später auf der Zuschaueranzeige und in Übersichten verwendet werden können.

### 3.2 Turnier speichern

Ein angelegtes Turnier muss gespeichert werden können.

Die Speicherung erfolgt lokal als JSON-Datei.

Das Speichern soll sowohl manuell als auch automatisch möglich sein.

### 3.3 Turnier laden

Ein bereits gespeichertes Turnier muss wieder geladen werden können.

Dies ist wichtig für:

- Vorbereitung vor dem Turniertag
- Fortsetzen der Arbeit am Turnierplan
- Wiederherstellung nach einem Programmabbruch
- erneutes Öffnen am Veranstaltungstag

### 3.4 Mannschaften anlegen

Die Turnierleitung kann Mannschaften anlegen und bearbeiten.

Pro Mannschaft sollen mindestens folgende Angaben möglich sein:

- Mannschaftsname
- optionale Kurzbezeichnung
- Gruppenzuordnung
- Spielerliste
- optionaler Status, z. B. aktiv oder zurückgezogen

### 3.5 Spieler anlegen

Spieler können den Mannschaften zugeordnet werden.

Spielernamen sollen auch nach Turnierbeginn noch geändert oder ergänzt werden können.

Dies ist wichtig, falls Mannschaften am Turniertag noch nicht vollständig gemeldet sind oder sich Spieler kurzfristig ändern.

### 3.6 Turnierformat wählen

Die Turnierleitung wählt ein Turnierformat aus.

Für Version 1 kann zunächst ein fester oder halbflexibler Modus verwendet werden, z. B. der Vorjahresmodus mit zwei Gruppen und anschließenden Platzierungsspielen.

Langfristig soll die Anwendung nach Eingabe der Mannschaften passende Turnierformate vorschlagen.

## 4. Start der Anwendung am Turniertag

Beim Start der Anwendung sollen einige technische und organisatorische Punkte geprüft werden.

### 4.1 Bildschirmprüfung

Die Anwendung prüft, ob ein externer Bildschirm vorhanden ist.

Wenn kein externer Bildschirm erkannt wird, soll ein deutlicher Hinweis erscheinen.

Wenn Windows auf duplizierte Anzeige eingestellt ist, soll ebenfalls ein Hinweis erscheinen, da der reguläre Betrieb den erweiterten Bildschirmmodus benötigt.

### 4.2 Bildschirmzuordnung

Die Turnierleitung soll beim Start auswählen können, welcher Bildschirm für welche Ansicht verwendet wird.

Standard:

```text
Laptop: Bedienoberfläche
Fernseher / externer Bildschirm: Zuschaueranzeige
```

Bei Bedarf soll die Zuordnung geändert werden können.

### 4.3 Zuschaueranzeige starten

Nach der Bildschirmzuordnung wird die Zuschaueranzeige auf dem externen Bildschirm im Vollbild gestartet.

Wenn noch kein Spiel läuft, zeigt sie standardmäßig einen Infoscreen.

## 5. Bedienung während des Turniers

Während des Turniers muss die Turnierleitung schnell und ohne unnötige Umwege arbeiten können.

Die wichtigsten Aufgaben sind:

- nächstes Spiel starten
- Spielreihenfolge bei Bedarf ändern
- Spieler für Duelle auswählen oder bestätigen
- Treffer pro Versuch erfassen
- automatische Ergebnisse prüfen
- Korrekturen vornehmen
- Spiel abschließen
- Anzeige manuell steuern
- Backups ausführen oder laden

## 6. Spielsteuerung

### 6.1 Nächstes Spiel starten

Die Turnierleitung startet das jeweils nächste vorgesehene Spiel aus dem Spielplan.

Beim Start werden die beteiligten Mannschaften auf der Zuschaueranzeige angezeigt.

### 6.2 Spielreihenfolge ändern

Die Turnierleitung soll bei Bedarf den Ablauf der Spiele ändern können.

Beispiele:

- ein Spiel wird nach vorne gezogen
- ein Spiel wird nach hinten verschoben
- ein Team ist noch nicht bereit
- eine Pause muss eingeschoben werden
- ein Final- oder Platzierungsspiel soll erst später stattfinden

Die Verschiebung erfolgt direkt im Ablaufplan der Bedienoberfläche. Das nächste Spiel muss von der Turnierleitung durch einen bewussten Klick ausgewählt und gestartet werden. Es gibt kein automatisches Weiterschalten zum nächsten Spiel.

Die Änderung soll den Spielplan nicht zerstören, sondern den Status und die Reihenfolge nachvollziehbar anpassen.

### 6.3 Spielstatus

Ein Spiel kann unterschiedliche Zustände haben.

Mögliche Statuswerte:

- geplant
- vorbereitet
- läuft
- pausiert
- abgeschlossen
- korrigiert
- verschoben

Die genaue Statuslogik wird später im technischen Datenmodell definiert.

## 7. Spielerzuordnung während eines Spiels

Pro Einzelduell treten jeweils ein Spieler von Mannschaft 1 und ein Spieler von Mannschaft 2 gegeneinander an.

Die Turnierleitung soll die Spieler auswählen oder bestätigen können.

Bei den regulären fünf Einzelduellen können vorhandene Spieler automatisch vorgeschlagen werden.

Im Stechen kann es vorkommen, dass kein Spieler 6 oder 7 existiert. In diesem Fall wählt die Turnierleitung je einen Teilnehmer der beiden Teams manuell aus.

Spielernamen sollen auch während des Turniers noch geändert oder ergänzt werden können.

## 8. Ergebniserfassung während eines Versuchs

Nach jedem Versuch trägt die Turnierleitung ein, welcher Spieler getroffen hat und welcher nicht.

Pro Versuch müssen mindestens folgende Eingaben möglich sein:

```text
Spieler Team 1: getroffen / nicht getroffen
Spieler Team 2: getroffen / nicht getroffen
```

Die Anzahl der Versuche wird automatisch gezählt.

Die Anwendung soll daraus automatisch ableiten:

- ob das Einzelduell entschieden ist
- ob ein weiterer Versuch nötig ist
- ob nach drei Versuchen ein unentschiedenes Einzelduell vorliegt
- wie der Zwischenstand der Partie lautet

## 9. Automatische Berechnung

Die Turnierleitung erfasst nicht direkt jede Punktzahl manuell.

Stattdessen werden Rundenergebnisse und Duellergebnisse automatisch aus den Trefferangaben berechnet.

### Beispiel

```text
Versuch 1:
Spieler Team 1 trifft
Spieler Team 2 trifft nicht

Ergebnis:
Team 1 gewinnt das Einzelduell
```

### Weiteres Beispiel

```text
Versuch 1:
beide treffen

Ergebnis:
kein Sieger, weiterer Versuch erforderlich
```

Die automatische Berechnung soll jederzeit nachvollziehbar angezeigt werden.

## 10. Korrekturen

Während eines Turniers können Eingabefehler passieren.

Deshalb muss die Turnierleitung Ergebnisse korrigieren können.

Korrigierbar sein sollen mindestens:

- Trefferangabe eines Versuchs
- Ergebnis eines Einzelduells
- Gesamtstand einer Partie
- Sieger einer Partie
- Spielerzuordnung eines Einzelduells
- Spielernamen
- Turnierdaten (Datum, Mannschaften, sonstige Felder)

### Absicherung von Korrekturen

Korrekturen werden technisch sinnvoll abgesichert, ohne den Ablauf unnötig zu unterbrechen.

Leitprinzip:

- Einfache Korrekturen (z. B. Spielername) erfordern keine gesonderte Bestätigung.
- Rückwirkende Korrekturen, die bereits berechnete Ergebnisse oder Tabellen beeinflussen, erfordern eine explizite Bestätigung (z. B. Bestätigungsdialog).
- Es soll keine mehrstufige Absicherung oder Passwortschutz nötig sein.

### Änderungsprotokoll

Jede Korrektur wird in einem Änderungsprotokoll festgehalten. Das Protokoll enthält mindestens:

- Zeitstempel der Änderung
- Art der Änderung
- vorheriger Wert
- neuer Wert

Das Änderungsprotokoll ist für Version 1 vorgesehen.

## 11. Stechen

Wenn nach den regulären Einzelduellen Gleichstand herrscht, soll das Stechen automatisch erkannt und gestartet werden.

Die Anwendung soll die Turnierleitung darauf hinweisen, dass ein Stechen erforderlich ist.

Im Stechen wählt die Turnierleitung je einen Teilnehmer beider Teams aus.

Dies ist besonders wichtig, wenn keine zusätzlichen Spieler wie Spieler 6 oder Spieler 7 existieren.

Das Stechen läuft so lange weiter, bis ein Sieger feststeht.

Die genaue Spielerlogik im Stechen wird im Fachkonzept weiter definiert.

## 12. Spiel abschließen

Wenn die Anwendung ein automatisches Endergebnis berechnet hat, muss die Turnierleitung das Spiel abschließen können.

Vor dem Abschluss soll eine Zusammenfassung angezeigt werden:

- Mannschaft 1
- Mannschaft 2
- Endstand
- Sieger
- regulär entschieden oder im Stechen entschieden
- gespielte Einzelduelle
- ggf. Hinweise zu Korrekturen

Wenn das Ergebnis korrekt ist, bestätigt die Turnierleitung den Spielabschluss.

Wenn das Ergebnis nicht korrekt ist, kann es vor Abschluss manuell bearbeitet werden.

Nach Abschluss des Spiels werden automatisch:

- Tabellen aktualisiert
- Platzierungen neu berechnet
- Zuschaueranzeige aktualisiert
- Daten gespeichert
- Backup erzeugt

## 13. Steuerung der Zuschaueranzeige

Die Zuschaueranzeige soll grundsätzlich automatisch auf den Spielverlauf reagieren.

Trotzdem kann die Turnierleitung jederzeit manuell zwischen den Hauptansichten wechseln.

### Manuell wählbare Ansichten

| Ansicht | Beschreibung |
|---------|-------------|
| Infoansicht | Automatisch rotierender Infoscreen zwischen den Spielen (Tabellen, Spielplan, nächste Begegnung, letzte Ergebnisse) |
| Spielinfo / Live-Ansicht | Live-Anzeige des laufenden Spiels mit Duellstand, Spielernamen und Versuchsergebnis |
| Webcam-Ansicht | Live-Bild der Webcam (optional, nur wenn Kamera verfügbar) |

Die Turnierleitung wechselt zwischen diesen Hauptansichten über die Bedienoberfläche. Innerhalb der Infoansicht läuft die Rotation automatisch weiter.

### Weitere Steuerungsoptionen

- Zuschaueranzeige neu starten
- Bildschirmzuordnung prüfen oder ändern

Dies ist wichtig, falls die automatische Anzeige nicht zur aktuellen Situation passt oder ein falscher Bildschirm verwendet wird.

## 14. Infoscreen

Wenn kein Spiel läuft, zeigt die Zuschaueranzeige standardmäßig den Infoscreen.

Der Infoscreen soll mindestens enthalten:

- letzte Begegnung
- Ergebnis der letzten Begegnung
- nächste Begegnung
- aktuelle Tabelle oder Tabellen

Zwischen den Spielen können mehrere Infoscreens automatisch wechseln.

Während eines Spiels reagiert die Zuschaueranzeige auf die Eingaben der Turnierleitung.

## 15. Speichern und Backup

### 15.1 Automatisches Speichern

Die Anwendung muss automatisch speichern.

Automatisch gespeichert werden soll insbesondere:

- nach Änderungen an Mannschaften
- nach Änderungen an Spielern
- nach Änderung des Spielplans
- nach jedem erfassten Versuch
- nach jedem abgeschlossenen Einzelduell
- nach Abschluss eines Spiels
- nach Korrekturen

### 15.2 Backup nach jedem Spiel

Nach jedem abgeschlossenen Spiel soll automatisch ein Backup erzeugt werden.

Das Backup soll den Stand nach dem Spielabschluss sichern.

### 15.3 Manuelles Backup

Die Turnierleitung soll jederzeit manuell ein Backup ausführen können.

Dies ist wichtig vor größeren Änderungen oder Korrekturen.

### 15.4 Backup-Aufbewahrung

Alle erzeugten Backups werden aufbewahrt. Es gibt kein automatisches Löschen älterer Backups.

Da die anfallenden Datenmengen bei einem Turnier gering sind, ist der Speicherbedarf vernachlässigbar.

### 15.5 Backup-Beschriftung

Jedes Backup muss eindeutig identifizierbar sein. Die Beschriftung enthält mindestens:

- Datum und Uhrzeit der Erzeugung
- Anlass des Backups (automatisch nach Spiel, manuell, nach Korrektur o. ä.)
- Spielnummer und Begegnung, nach der das Backup erstellt wurde (falls zutreffend)

Beispielformat:

```text
2026-06-12_14-35-22_nach-Spiel-3_TeamA-vs-TeamB.json
2026-06-12_14-40-00_manuell.json
```

Beim Laden eines Backups soll die Beschriftung vollständig angezeigt werden, damit die Turnierleitung eindeutig erkennt, welchen Spielstand sie wiederherstellt.

### 15.6 Backup laden

Nach einem Absturz oder einer fehlerhaften Änderung soll die Turnierleitung ein altes Backup laden können.

Beim Laden eines Backups soll deutlich angezeigt werden, von welchem Zeitpunkt und welchem Spielstand das Backup stammt.

## 16. Verhalten nach Absturz

Nach einem Programmabbruch soll die Anwendung beim nächsten Start möglichst einfach fortgesetzt werden können.

Möglicher Ablauf:

1. Anwendung startet.
2. Letzter Speicherstand wird erkannt.
3. Vorhandene Backups werden angeboten.
4. Turnierleitung wählt normalen Speicherstand oder Backup.
5. Anwendung stellt Spielplan, Tabellen und Anzeigezustand wieder her.

Die genaue Wiederherstellungslogik wird später im technischen Konzept beschrieben.

## 17. Ablauf nach Turnierende

Nach Abschluss des Turniers sollen die Endstände und Platzierungen sichtbar bleiben.

Die Turnierleitung soll mindestens folgende Möglichkeiten haben:

- Abschlusstabelle anzeigen
- Siegeranzeige anzeigen
- Turnier speichern
- manuelles Abschlussbackup erstellen
- Anwendung beenden

Export und Druck sind für Version 1 nicht zwingend erforderlich, können später ergänzt werden.

## 18. Vorläufiger Muss-Umfang für Version 1

Für Version 1 muss die Turnierleitung mindestens Folgendes bedienen können:

- Turnier anlegen (Pflichtfelder: Mannschaften und Datum)
- Turnierdaten nachträglich ändern
- Turnier speichern
- Turnier laden
- Bildschirmzuordnung auswählen
- Zuschaueranzeige starten
- Mannschaften anlegen und ändern
- Spieler anlegen und ändern
- Spiel starten (durch expliziten Klick im Ablaufplan)
- Spiel bei Bedarf im Ablaufplan verschieben
- Treffer je Versuch erfassen
- automatische Duellberechnung nutzen
- automatische Partieauswertung prüfen
- Ergebnis korrigieren (mit kontextabhängiger Absicherung)
- Änderungsprotokoll automatisch führen lassen
- Stechen starten und bedienen
- Spiel abschließen
- Tabelle automatisch aktualisieren lassen
- Anzeige bei Bedarf manuell wechseln
- manuelles Backup erstellen
- alle Backups aufbewahren (eindeutige Beschriftung mit Zeitstempel und Kontext)
- Backup nach Absturz laden

## 19. Spätere Erweiterungen

Weitere Funktionen können später ergänzt werden.

Mögliche Erweiterungen:

- vollautomatische Spielplanerzeugung für viele Turniermodi
- Benutzerrollen oder Sperrmodus für kritische Eingaben
- Webcam- oder Livebildintegration
- Export als PDF, Excel oder CSV
- Druckfunktion
- erweiterte Statistik
- Animationen und Showeffekte
- Bedienmodus für Touchscreen
- Sponsorenscreens oder Pausenanzeigen

## 20. Offene Punkte

Alle bisherigen Fragen sind geklärt. Neue Punkte werden hier ergänzt, sobald sie entstehen.

### Geklärte Entscheidungen (Referenz)

| Frage | Entscheidung |
|-------|-------------|
| Pflichtfelder beim Turnier anlegen | Mannschaften (mind. 2) und Datum; Rest optional und nachträglich ergänzbar; beide Pflichtfelder nachträglich änderbar |
| Spielverschiebung in der Bedienoberfläche | Verschieben im Ablaufplan; nächstes Spiel wird durch expliziten Klick der Turnierleitung ausgewählt |
| Absicherung von Korrekturen | Technisch sinnvoll: einfache Korrekturen ohne Bestätigung, rückwirkende Korrekturen mit Bestätigungsdialog |
| Änderungsprotokoll | Ja, in Version 1 – Zeitstempel, Art, vorheriger und neuer Wert |
| Backup-Aufbewahrung | Alle Backups werden dauerhaft aufbewahrt |
| Backup-Beschriftung | Eindeutig: Zeitstempel + Anlass + Spielnummer/Begegnung |
| Manuell wählbare Zuschaueransichten | Infoansicht (zwischen Spielen), Spielinfo/Live-Ansicht (während Spiels), Webcam-Ansicht (optional) |
| Pausieren / Abbrechen eines laufenden Spiels | Kein dedizierter Mechanismus vorgesehen – sollte im normalen Turnierbetrieb nicht vorkommen |
| Zurückgezogene Mannschaft | Alle Spiele der Mannschaft als „abgesetzt" markieren; keine Punkte für beide Seiten; Mannschaft aus der laufenden Wertung herausnehmen |
