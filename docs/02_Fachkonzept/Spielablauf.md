# Spielablauf

Dieses Dokument beschreibt den fachlichen Ablauf einer einzelnen Partie im Projekt **Kirchenbirkiger Schluck Programm**.  
Es dient als Grundlage für Bedienlogik, Datenmodell, Zuschaueranzeige und spätere technische Umsetzung.

## 1. Zweck des Dokuments

Der Spielablauf beschreibt, was während einer Partie passiert:

- welche Informationen angezeigt werden
- welche Eingaben die Turnierleitung machen muss
- wie Einzelduelle ablaufen
- wie Versuche bewertet werden
- wann ein Spiel entschieden ist
- wie Tabellen und Anzeige nach dem Spiel aktualisiert werden

Dieses Dokument beschreibt noch keine technische Umsetzung, sondern den fachlichen Ablauf.

## 2. Begriffe

### Partie

Eine Partie ist ein Spiel zwischen zwei Mannschaften.

Beispiel:

```text
Team 1 gegen Team 2
```

Eine Partie besteht aus mehreren Einzelduellen.

### Einzelduell

Ein Einzelduell ist der direkte Vergleich zwischen je einem Spieler beider Mannschaften.

Beispiel:

```text
Spieler 1 von Team 1 gegen Spieler 1 von Team 2
```

### Versuch

Ein Versuch ist ein einzelner Trinkversuch beider Spieler innerhalb eines Einzelduells.

### Reguläre Einzelduelle

Eine Partie besteht aus **maximal** fünf regulären Einzelduellen (Best of 5), da grundsätzlich fünf Spieler je Mannschaft vorgesehen sind. Sie endet vorzeitig, sobald ein Team einen **uneinholbaren** Vorsprung hat (siehe Abschnitt 8).

### Stechen / Overtime

Wenn nach den fünf regulären Einzelduellen Gleichstand herrscht, wird die Partie in einem Stechen fortgesetzt, bis ein Sieger feststeht.

## 3. Start einer Partie

Eine Partie startet mit der Auswahl oder dem Aufruf der nächsten Begegnung im Spielplan.

Zu Beginn jeder Partie wird je Mannschaft die **Spielerreihenfolge ausgelost**, sodass die Duelle variieren und nicht immer derselbe Spieler beginnt. Die ausgeloste Reihenfolge bestimmt die Vorbelegung der regulären Einzelduelle; die Turnierleitung kann die Spieler eines Duells jederzeit manuell in der Bedienoberfläche ändern.

Beim Start der Partie werden auf dem Anzeigebildschirm mindestens angezeigt:

- Name von Mannschaft 1
- Name von Mannschaft 2
- aktueller Gesamtpunktestand der Partie
- aktuelles Einzelduell
- aktueller Spieler von Mannschaft 1
- aktueller Spieler von Mannschaft 2

Der Gesamtpunktestand zeigt die gewonnenen Einzelduelle beziehungsweise die Duellpunkte innerhalb der laufenden Partie an. Die Darstellung soll sich vom Prinzip her wie beim Elfmeterschießen im Fußball anfühlen.

Beispiel:

```text
Team 1     2 : 1     Team 2
```

## 4. Ablauf eines Einzelduells

In jedem Einzelduell treten je ein Spieler beider Mannschaften gegeneinander an.

Der Ablauf ist:

1. Die Turnierleitung startet oder öffnet das aktuelle Einzelduell.
2. Beide Spieler treten an.
3. Beide Spieler führen ihren Trinkversuch aus.
4. Die Turnierleitung erfasst das Ergebnis des Versuchs.
5. Falls ein eindeutiger Sieger feststeht, wird das Einzelduell abgeschlossen.
6. Falls kein eindeutiger Sieger feststeht, wird der Versuch wiederholt.
7. Nach maximal drei Versuchen ohne eindeutigen Sieger endet das Einzelduell unentschieden.
8. Danach folgt das nächste Einzelduell.

## 5. Bewertung eines Versuchs

Ein Versuch wird anhand der Treffer beider Spieler bewertet.

### Spieler von Mannschaft 1 trifft, Spieler von Mannschaft 2 trifft nicht

Mannschaft 1 gewinnt das Einzelduell.

```text
Mannschaft 1: Punkt
Mannschaft 2: kein Punkt
```

### Spieler von Mannschaft 2 trifft, Spieler von Mannschaft 1 trifft nicht

Mannschaft 2 gewinnt das Einzelduell.

```text
Mannschaft 1: kein Punkt
Mannschaft 2: Punkt
```

### Beide Spieler treffen

Das Einzelduell ist noch nicht entschieden.

Der Versuch wird als unentschieden gewertet und das Einzelduell wird wiederholt, sofern noch nicht drei Versuche durchgeführt wurden.

### Beide Spieler treffen nicht

Das Einzelduell ist ebenfalls noch nicht entschieden.

Der Versuch wird als unentschieden gewertet und das Einzelduell wird wiederholt, sofern noch nicht drei Versuche durchgeführt wurden.

## 6. Maximale Anzahl von Versuchen je Einzelduell

Pro Einzelduell sind maximal drei Versuche vorgesehen, um einen eindeutigen Sieger zu ermitteln.

Wenn nach drei Versuchen kein eindeutiger Sieger feststeht, endet das Einzelduell unentschieden.

Das kann beispielsweise passieren, wenn beide Spieler immer treffen oder beide Spieler immer verfehlen.

## 7. Wertung eines unentschiedenen Einzelduells

Die Wertung eines nach drei Versuchen unentschiedenen Einzelduells hängt davon ab, ob in den Versuchen überhaupt Treffer erzielt wurden.

### Fall A: Kein Spieler hat in irgendeinem Versuch getroffen (0:0)

```text
Mannschaft 1: 0 Punkte
Mannschaft 2: 0 Punkte
```

Das Einzelduell gilt als nicht entschieden und neutral. Kein Duellpunkt wird vergeben.

### Fall B: Beide Spieler haben mindestens einmal getroffen (z. B. Treffer in Versuch 1, dann Unentschieden)

```text
Mannschaft 1: 1 Punkt
Mannschaft 2: 1 Punkt
```

Da beide Spieler bewiesen haben, dass sie treffen können, und kein Sieger ermittelt werden konnte, erhalten beide Mannschaften je einen Duellpunkt.

Diese Regel ermöglicht Partiestände wie:

```text
3 : 3
```

wenn mehrere Einzelduelle mit beidseitigen Treffern enden.

## 8. Abschluss im Best-of-5-Modus

Eine Partie läuft so lange, bis sie **entschieden** ist – höchstens jedoch über fünf reguläre Einzelduelle.

Sie gilt als **vorzeitig entschieden**, sobald der Vorsprung eines Teams (an gewonnenen Einzelduellen) **größer** ist als die Zahl der noch möglichen regulären Duelle. Das zurückliegende Team kann den Rückstand dann rechnerisch nicht mehr aufholen; es werden keine weiteren regulären Duelle gestartet.

Solange ein **Ausgleich noch möglich** ist (Vorsprung ≤ verbleibende Duelle), wird weitergespielt – so bleibt der Weg ins Stechen offen. Unentschiedene Einzelduelle (0:0 oder 1:1) verändern den Vorsprung nicht.

Beispiele:

```text
Team 1     3 : 0     Team 2   (nach 3 Duellen)  → entschieden, Team 1 gewinnt (max. 3:2 möglich)
Team 1     2 : 1     Team 2   (nach 3 Duellen)  → wird weitergespielt (Ausgleich möglich)
Team 1     3 : 2     Team 2   (nach 5 Duellen)  → entschieden, Team 1 gewinnt
Team 1     3 : 3     Team 2   (nach 5 Duellen)  → nicht entschieden, geht ins Stechen
```

Die Turnierleitung schließt die entschiedene Partie über die Bedienoberfläche ab (der Button „Spiel abschließen" wird freigeschaltet, weitere reguläre Duelle werden gesperrt).

## 9. Stechen / Overtime

Wenn nach den fünf regulären Einzelduellen Gleichstand herrscht, wird die Partie fortgesetzt.

Das Stechen funktioniert vom Prinzip her wie das Elfmeterschießen im Fußball:

- Es treten weiterhin je ein Spieler beider Mannschaften gegeneinander an.
- Nach jedem zusätzlichen Einzelduell wird geprüft, ob ein Sieger feststeht.
- Sobald eine Mannschaft nach gleicher Anzahl zusätzlicher Einzelduelle vorne liegt, ist die Partie entschieden.
- Das Stechen läuft so lange weiter, bis ein Sieger gefunden ist.

### Spielerauswahl im Stechen

Die Turnierleitung wählt die Spieler für das Stechen frei aus. Es gelten folgende Regeln:

- Spieler dürfen erneut antreten, auch wenn sie bereits in den regulären Einzelduellen gespielt haben.
- Es muss keine bestimmte Reihenfolge eingehalten werden.
- Zusätzliche Spieler (z. B. ein 6. oder 7. Spieler) können ebenfalls für das Stechen eingesetzt werden.
- Die Turnierleitung kann Spieler für das Stechen über die Bedienoberfläche hinzufügen.

### Wertung eines unentschiedenen Einzelduells im Stechen

Im Stechen gelten dieselben Wertungsregeln wie in den regulären Einzelduellen (siehe Abschnitt 7):

- Kein Spieler hat getroffen (0:0) → 0 Punkte für beide Mannschaften.
- Beide Spieler haben mindestens einmal getroffen → 1 Punkt für jede Mannschaft.

Das Stechen läuft im KO-Modus weiter: Auch wenn ein unentschiedenes Einzelduell beiden Mannschaften einen Punkt gibt, wird das nächste Einzelduell sofort gespielt. Das Stechen endet erst, wenn nach einem Einzelduell ein Sieger feststeht.

## 10. Partieergebnis

Eine Partie hat am Ende immer einen Sieger.

Es gibt kein Unentschieden auf Spielebene.

Das finale Partieergebnis enthält mindestens:

- Mannschaft 1
- Mannschaft 2
- Duellpunkte Mannschaft 1
- Duellpunkte Mannschaft 2
- Sieger
- Information, ob die Partie regulär oder im Stechen entschieden wurde
- Anzahl der gespielten Einzelduelle
- Einzelduelle mit Versuchen und Ergebnissen

## 11. Tabellenwertung

Nach Abschluss einer Partie werden die Tabelle und die Platzierungen automatisch aktualisiert.

### Standardwertung: Eishockey-System (Standard)

Die Anwendung verwendet standardmäßig das Eishockey-Wertungssystem:

```text
Sieg in regulärer Spielzeit     = 3 Tabellenpunkte
Niederlage in regulärer Spielzeit = 0 Tabellenpunkte
Sieg nach Stechen / Overtime    = 2 Tabellenpunkte
Niederlage nach Stechen / Overtime = 1 Tabellenpunkt
```

Dieses System belohnt einen Sieg in den regulären fünf Einzelduellen stärker als einen Sieg im Stechen. Die Mannschaft, die im Stechen verliert, erhält trotzdem einen Punkt für ihr Erreichen des Stechens.

### Alternative Wertung: Einfaches System

Bei der Turniererstellung kann alternativ das einfache Wertungssystem ausgewählt werden:

```text
Sieg = 1 Tabellenpunkt
Niederlage = 0 Tabellenpunkte
```

Da es auf Spielebene kein Unentschieden gibt, erhält immer genau eine Mannschaft den Sieg.

### Auswahl bei Turniererstellung

Das Wertungssystem wird einmalig bei der Erstellung eines Turniers festgelegt und kann im laufenden Turnier nicht mehr geändert werden.

## 12. Zusammenfassung Tabellenwertungsmodelle

| Modell | Standard | Sieg regulär | Niederlage regulär | Sieg Stechen | Niederlage Stechen |
|--------|----------|-------------|-------------------|--------------|-------------------|
| Eishockey-System | Ja (Standard) | 3 | 0 | 2 | 1 |
| Einfaches System | Nein (optional) | 1 | 0 | 1 | 0 |

## 13. Live-Aktualisierung während der Partie

Während einer laufenden Partie soll die Zuschaueranzeige live auf Eingaben der Turnierleitung reagieren.

Angezeigt werden können unter anderem:

- aktuelle Partie
- aktuelles Einzelduell
- aktueller Versuch
- Treffer / kein Treffer je Spieler
- Zwischenstand nach jedem Einzelduell
- Hinweis auf Wiederholung
- Hinweis auf dritten Versuch
- Hinweis auf Stechen
- Sieger der Partie

Die genauen Inhalte und die grafische Ausgestaltung der Zuschaueranzeige während eines Versuchs werden in einem eigenen UI-/UX-Dokument definiert (noch ausstehend).

## 14. Aufgaben der Turnierleitung während einer Partie

Während einer Partie muss die Turnierleitung mindestens folgende Aktionen durchführen können:

- Partie starten
- aktuelle Spieler auswählen oder bestätigen
- Versuch starten oder erfassen
- Treffer für Spieler von Mannschaft 1 erfassen
- Treffer für Spieler von Mannschaft 2 erfassen
- Versuchsergebnis bestätigen
- Wiederholung eines Einzelduells auslösen
- Einzelduell abschließen
- nächstes Einzelduell starten
- Stechen starten, falls erforderlich
- Partie abschließen

### Korrekturfunktionen

Die Anwendung soll prinzipiell alles korrigierbar halten. Während einer laufenden Partie müssen mindestens folgende Korrekturen möglich sein:

- **Ergebnisse korrigieren**: bereits erfasste Versuchsergebnisse oder Einzelduell-Ergebnisse nachträglich ändern
- **Spielernamen anpassen**: Name eines bereits eingetragenen Spielers nachträglich ändern oder korrigieren
- **Anderen Spieler auswählen**: den für ein Einzelduell geplanten Spieler durch einen anderen ersetzen
- **Spieler für das Stechen hinzufügen**: einen zusätzlichen Spieler (z. B. 6. oder 7. Spieler) nachträglich ergänzen, damit er im Stechen eingesetzt werden kann

Optional sinnvoll:

- Kommentar oder Hinweis erfassen
- versehentliche Eingabe rückgängig machen
- manuell zur nächsten Anzeige wechseln

## 15. Ablauf nach Abschluss einer Partie

Sobald eine Partie entschieden ist:

1. Das finale Ergebnis wird gespeichert.
2. Der Sieger wird angezeigt.
3. Die Tabellenpunkte werden berechnet.
4. Die Tabelle wird automatisch aktualisiert.
5. Die Platzierungen werden automatisch neu berechnet.
6. Es wird ein Backup erzeugt.
7. Die Zuschaueranzeige wechselt nach einer definierten Zeit in den Infoscreen-Modus.

## 16. Infoscreen zwischen den Spielen

Zwischen zwei Spielen soll die Zuschaueranzeige automatisch Informationsbildschirme anzeigen.

Mindestens enthalten sein sollen:

- letzte Begegnung
- Ergebnis der letzten Begegnung
- nächste Begegnung
- aktuelle Tabelle oder Tabellen
- ggf. aktueller Turnierfortschritt

Der Infoscreen läuft automatisch, bis die Turnierleitung das nächste Spiel startet.

## 17. Beispielhafter Gesamtablauf

```text
1. Nächste Partie wird ausgewählt
2. Zuschaueranzeige zeigt Team 1 gegen Team 2
3. Einzelduell 1 startet
4. Versuch 1 wird erfasst
5. Falls kein Sieger: Versuch 2
6. Falls kein Sieger: Versuch 3
7. Einzelduell wird gewertet
8. Zwischenstand wird angezeigt
9. Einzelduell 2 startet
10. Ablauf wiederholt sich bis Einzelduell 5
11. Gesamtstand wird geprüft
12. Falls kein Gleichstand: Partie ist entschieden
13. Falls Gleichstand: Stechen startet
14. Stechen läuft bis ein Sieger feststeht
15. Sieger wird angezeigt
16. Tabelle wird aktualisiert
17. Backup wird erzeugt
18. Infoscreen zeigt letzte Begegnung, nächste Begegnung und Tabelle
19. Nächste Partie startet
```

## 18. Offene Entscheidungen aus diesem Dokument

Folgende Punkte sind noch offen:

- Welche Informationen werden während eines Versuchs exakt auf der Zuschaueranzeige gezeigt? → wird in einem eigenen UI-/UX-Dokument festgelegt

### Geklärte Entscheidungen (Referenz)

| Frage | Entscheidung |
|-------|-------------|
| Wertung unentschiedenes Einzelduell (0:0) | 0 Punkte für beide Mannschaften |
| Wertung unentschiedenes Einzelduell (beide haben mind. 1× getroffen) | Je 1 Punkt für beide Mannschaften |
| Tabellenwertung Standard | Eishockey-System (3/2/1/0); einfaches System (1/0) optional bei Turniererstellung wählbar |
| Spielerauswahl im Stechen | Freie Auswahl durch Turnierleitung; Spieler dürfen erneut antreten; Reihenfolge egal; zusätzliche Spieler möglich |
| Wertung unentschiedenes Einzelduell im Stechen | Gleiche Regeln wie im regulären Spiel; Stechen läuft im KO-Modus weiter |
| Korrekturfunktionen während einer Partie | Ergebnisse korrigieren, Spielernamen ändern, anderen Spieler wählen, Spieler für Stechen hinzufügen |

## 19. Vorläufige Einordnung für Version 1

Für Version 1 sollte der Spielablauf grundsätzlich vollständig unterstützt werden.

Muss für V1:

- Partie starten
- Einzelduelle erfassen
- bis zu drei Versuche je Einzelduell erfassen
- Zwischenstand live anzeigen
- reguläre fünf Einzelduelle abbilden
- Stechen ermöglichen
- Partie abschließen
- Tabelle automatisch aktualisieren
- Infoscreen nach Spielende anzeigen
- Daten automatisch speichern
- Backup nach Spielabschluss erzeugen

Kann später verfeinert werden:

- finale Animationen
- Webcam-Livebild
- detaillierte Statistik
- verschiedene Tabellenwertungsmodelle
- vollautomatischer Turnierplan für alle Teilnehmerzahlen
