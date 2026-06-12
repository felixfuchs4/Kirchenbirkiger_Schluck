# Projektentscheidungen

Dieses Dokument sammelt die aktuell festgelegten Grundsatzentscheidungen für das Projekt **Kirchenbirkiger Schluck Programm**.  
Es dient als verbindliche Grundlage für die weitere Planung und Entwicklung.

## 1. Plattform und Betriebsart

### Entscheidung

Die Anwendung wird ausschließlich für **Windows** entwickelt.

Ein Betrieb auf anderen Betriebssystemen ist nicht geplant.

### Begründung

Die Anwendung wird lokal auf einem Laptop beim Turnier eingesetzt. Es gibt aktuell keinen Bedarf für macOS, Linux, Web oder mobile Plattformen.

## 2. Lokaler Betrieb

### Entscheidung

Die Anwendung läuft ausschließlich **lokal**.

Es ist keine Cloud-Anbindung, kein Serverbetrieb und keine Internetverbindung erforderlich.

### Begründung

Die Anwendung muss am Turniertag zuverlässig funktionieren, unabhängig von Internetverbindung, WLAN oder externen Diensten.

## 3. Zwei-Bildschirm-Betrieb

### Entscheidung

Bedienung und Zuschaueranzeige laufen über **einen Laptop**.

Die Anwendung muss mit **zwei Bildschirmen gleichzeitig** arbeiten können:

- Laptopbildschirm: Bedienoberfläche für die Turnierleitung
- externer Bildschirm / Fernseher: Zuschaueranzeige im Vollbild

Der externe Bildschirm wird per HDMI angeschlossen.

### Anforderungen

Die Anwendung soll beim Start prüfen, ob ein zweiter Bildschirm vorhanden ist.

Wenn kein zweiter Bildschirm erkannt wird oder die Anzeige falsch eingestellt ist, soll ein deutlicher Hinweis erscheinen.

Wichtig ist, dass Windows auf **erweiterten Bildschirmbetrieb** eingestellt ist. Eine duplizierte Anzeige ist für den regulären Turnierbetrieb nicht geeignet.

Beim Start der Anwendung soll die Turnierleitung auswählen können, welcher Bildschirm für die Bedienung und welcher für die Zuschaueranzeige verwendet wird.

## 4. Zuschaueranzeige

### Entscheidung

Die Zuschaueranzeige läuft dauerhaft im **Vollbildmodus** auf dem externen Bildschirm.

### Verhalten

Zwischen den Spielen sollen Informationsansichten automatisch wechseln können.

Während eines laufenden Spiels reagiert die Anzeige auf Eingaben der Turnierleitung, zum Beispiel bei der Erfassung einzelner Runden oder Ergebnisse.

Die konkreten Anzeigeansichten werden später separat definiert.

## 5. Bedienoberfläche

### Entscheidung

Die Bedienoberfläche wird für die Turnierleitung optimiert.

Sie muss nicht identisch mit der Zuschaueranzeige sein.

### Ziel

Die Bedienung soll schnell, robust und übersichtlich sein. Während des Turniers muss die Turnierleitung ohne unnötige Klickwege Mannschaften, Spieler, Spiele und Ergebnisse verwalten können.

## 6. UI-Technologie und grafische Qualität

### Entscheidung

Die konkrete .NET-Oberfläche ist noch nicht final festgelegt.

Wichtig ist, dass moderne grafische und animatorische Möglichkeiten vorhanden sind.

Die Anwendung soll nicht wie eine klassische Windows-Forms-Anwendung aus früheren Jahren wirken.

### Anforderungen an die Oberfläche

Die Anwendung soll:

- modern wirken
- optisch ansprechend sein
- Bewegung und Animationen ermöglichen
- dynamische Anzeigen unterstützen
- für eine große Zuschaueranzeige geeignet sein
- sauber mit zwei Fenstern auf zwei Bildschirmen umgehen können

### Noch offen

Die konkrete Technologieentscheidung steht noch aus.

Mögliche Kandidaten:

- WPF
- WinUI
- Avalonia UI

Diese Entscheidung wird später in einem eigenen technischen Dokument bewertet.

## 7. Bereitstellung der Anwendung

### Entscheidung

Bevorzugt wird eine Anwendung, die als kompletter Ordner verschoben und direkt per `.exe` gestartet werden kann.

Eine klassische Installation ist möglich, wenn sie technisch oder praktisch deutliche Vorteile bietet.

### Priorität

1. Portable Ausführung als Ordner mit `.exe`
2. Klassische Installation nur bei Bedarf

## 8. Logo und Design-Grundlage

### Entscheidung

Das verbindliche Logo für die Anwendung ist:

```text
Logo_Kirchenbirkiger_Schluck.png
```

Dieses Logo wurde separat hochgeladen und ist dauerhaft als App-Logo zu verwenden.

Das Logo aus einer jeweiligen Jahreseinladung ist **nicht** maßgeblich für die Anwendung.

### Designrichtung

Die Gestaltung soll sich am verbindlichen Logo orientieren.

Grundrichtung:

- Rot
- Schwarz
- helle Kontrastflächen
- kräftige Darstellung
- gute Lesbarkeit auf Entfernung
- modern und veranstaltungstauglich

## 9. Datenhaltung

### Entscheidung

Die Speicherung erfolgt lokal als **JSON**.

Eine Datenbank ist für den aktuellen Projektstand nicht erforderlich.

### Anforderungen

Die Anwendung muss automatisch speichern.

Zusätzlich soll ein Backup erstellt werden.

Backups sollen mindestens zyklisch **nach jedem Spiel** erzeugt werden.

### Ziel

Auch bei Absturz, Fehlbedienung oder Stromproblem sollen Turnierdaten möglichst nicht verloren gehen.

## 10. Flexibilität bei Mannschaften, Gruppen und Spielern

### Entscheidung

Die Anwendung soll flexibel mit unterschiedlichen Mannschafts-, Gruppen- und Spielerzahlen umgehen können.

### Anforderungen

Die App darf nicht starr auf eine feste Anzahl von Gruppen oder Mannschaften begrenzt sein.

Spielernamen sollen eingegeben werden können.

Spielernamen sollen auch nach Turnierbeginn noch geändert oder ergänzt werden können.

### Hinweis

Auch wenn die Spielregel grundsätzlich von Teams mit fünf Personen ausgeht, soll die Software strukturell nicht unnötig hart auf exakt fünf Spieler je Mannschaft festgelegt werden.

## 11. Bedeutung der alten Excel-Datei

### Entscheidung

Die alte Excel-Datei dient ausschließlich als Beispiel dafür, wie das Turnier im Vorjahr umgesetzt wurde.

Sie ist keine technische oder fachliche Einschränkung für die neue Anwendung.

### Konsequenz

Die neue Anwendung darf und soll von der Excel-Struktur abweichen, wenn dies für Flexibilität, Bedienbarkeit oder Wartbarkeit sinnvoll ist.

## 12. Turniermodus

### Entscheidung

Die Anwendung soll langfristig mehrere Turniersysteme unterstützen.

Die Gruppengröße und der Turniermodus richten sich nach der Anzahl der angemeldeten Mannschaften.

Nach Erfassung der Mannschaften soll die Anwendung passende Turnieroptionen vorschlagen, aus denen die Turnierleitung auswählen kann.

### Ziel

Für unterschiedliche Teilnehmerzahlen sollen automatisch geeignete Turnierpläne möglich sein.

Am Ende soll ein Sieger über K.-o.-Spiele, Finalspiele oder andere geeignete Platzierungslogiken ermittelt werden.

### Beispiele möglicher Modi

Je nach Teilnehmerzahl können unterschiedliche Systeme sinnvoll sein:

- Gruppenphase mit anschließendem Finale
- Gruppenphase mit Halbfinale und Finale
- Gruppenphase mit Viertelfinale, Halbfinale und Finale
- Platzierungsspiele nach Gruppenplatzierung
- System ähnlich dem Vorjahr: Erster gegen Erster, Zweiter gegen Zweiter usw.

Die konkreten Modi werden später im Fachkonzept definiert.

## 13. Spielplanerzeugung

### Entscheidung

Eine automatische Spielplanerzeugung ist gewünscht, muss aber nicht zwingend Bestandteil von Version 1 sein.

### V1

Für Version 1 kann zunächst mit dem alten System aus dem Vorjahr gearbeitet werden:

- zwei Gruppen
- Gruppenphase
- anschließende Platzierungsspiele

### V2

Für Version 2 soll die automatische Spielplanerzeugung auf Basis der Mannschaftsanzahl und des gewählten Turniermodus umgesetzt werden.

## 14. Spiel- und Rundenerfassung

### Entscheidung

Alle Einzelrunden eines Spiels sollen einzeln erfasst werden.

Die Rundenergebnisse sollen den Zuschauern live angezeigt werden.

### Bedeutung

Die Anwendung speichert nicht nur das Endergebnis eines Spiels, sondern auch die einzelnen Runden innerhalb eines Duells.

Dadurch kann die Zuschaueranzeige während eines Spiels direkt auf den aktuellen Stand reagieren.

## 15. Wertungslogik

### Entscheidung

Es gibt kein Unentschieden auf Spielebene.

Ein Sieg zählt **1 Punkt**.

Eine Niederlage zählt **0 Punkte**.

### Punktgleichheit in Tabellen

Bei Punktgleichheit soll entweder

- ein Stechen erfolgen oder
- der direkte Vergleich zählen.

Die genaue Reihenfolge der Tabellenkriterien ist noch offen und wird später im Fachkonzept festgelegt.

## 16. Final- und Platzierungsspiele

### Entscheidung

Die Anwendung soll alle Spiele eines Turniers abbilden können, einschließlich:

- Gruppenphase
- Finalspiele
- Platzierungsspiele

### Ziel

Nicht nur der Sieger, sondern auch weitere Platzierungen sollen abgebildet werden können, sofern der gewählte Turniermodus dies vorsieht.

## 17. Export und Druck

### Entscheidung

Export- und Druckfunktionen sind zunächst **Nice-to-have**.

Sie sind für Version 1 nicht erforderlich.

### Mögliche spätere Funktionen

- Export als PDF
- Export als Excel
- Export als CSV
- Druck eines Spielplans
- Druck einer Abschlusstabelle

## 18. Designstand für Version 1

### Entscheidung

Das Design soll in Version 1 zunächst funktional sein.

Die finale optische Ausarbeitung erfolgt später.

### Ziel für V1

Die Anwendung muss sauber funktionieren und grundsätzlich ordentlich aussehen.

Animationen, Feinschliff und finale visuelle Gestaltung werden nach Stabilisierung der fachlichen Funktionen ausgearbeitet.

## 19. Vorläufiger Umfang Version 1

Version 1 soll mindestens folgende Punkte abdecken:

- lokale Windows-Anwendung
- Start per `.exe`, bevorzugt portabel
- zwei Bildschirmfenster
- Vollbild-Zuschaueranzeige
- Mannschaften anlegen und bearbeiten
- Spielernamen erfassen und nachträglich ändern
- zwei Gruppen als erster Turniermodus
- Spiele erfassen
- Einzelrunden erfassen
- Ergebnisse live anzeigen
- Tabellen berechnen
- Final- und Platzierungsspiele abbilden
- automatische Speicherung als JSON
- Backup nach jedem Spiel
- verbindliches Logo integrieren

## 20. Offene Entscheidungen

Folgende Entscheidungen sind noch offen und müssen später separat getroffen werden:

- konkrete UI-Technologie
- genaue Projektstruktur im Code
- genaues JSON-Speicherformat
- konkrete Turniermodi je Teilnehmerzahl
- automatische Spielplanlogik
- Tabellenkriterien bei Punktgleichheit
- genaue Anzeigeansichten
- konkreter Ablauf der Infoscreens
- Designsystem und Animationen
- Backup-Aufbewahrung und Wiederherstellung
- Exportformate für spätere Versionen
