# Anzeigeansichten

Dieses Dokument beschreibt die geplanten Anzeigeansichten für die Zuschaueranzeige im Projekt **Kirchenbirkiger Schluck Programm**.

Die Zuschaueranzeige läuft auf dem externen Bildschirm oder Fernseher im Vollbildmodus.  
Sie soll den Turnierablauf groß, klar lesbar und optisch ansprechend darstellen.

## 1. Zweck des Dokuments

Die Anzeigeansichten definieren, welche Informationen dem Publikum in welcher Turniersituation gezeigt werden.

Dieses Dokument beschreibt fachlich:

- welche Screens benötigt werden
- welche Inhalte je Screen angezeigt werden
- wann welche Ansicht verwendet wird
- welche Ansichten automatisch oder manuell gewechselt werden
- wie optionale Funktionen wie Webcam-Livebild eingeordnet werden

Die konkrete grafische Gestaltung, Animation und technische Umsetzung werden später separat definiert.

## 2. Grundprinzip der Zuschaueranzeige

Die Zuschaueranzeige soll dauerhaft im Vollbild auf dem externen Bildschirm laufen.

Sie wechselt je nach Turniersituation zwischen verschiedenen Ansichten:

- vor Turnierbeginn
- zwischen Spielen
- während eines laufenden Spiels
- nach Abschluss der Gruppenphase
- am Ende des Turniers

Zwischen Spielen können mehrere Informationsansichten automatisch rotieren.  
Während eines laufenden Spiels reagiert die Anzeige auf Eingaben der Turnierleitung.

Die Turnierleitung soll die aktuell angezeigte Ansicht bei Bedarf manuell wechseln können.

## 3. Übersicht der geplanten Ansichten

Für die erste fachliche Planung sind folgende Hauptansichten vorgesehen:

| Ansicht | Einsatzzeitpunkt | Zweck |
|---|---|---|
| Startscreen | vor Beginn des Turniers | Begrüßung / Veranstaltungskontext |
| Infoscreen | vor und zwischen Spielen | Turnierübersicht und Orientierung |
| Matchday-Screen | während eines laufenden Spiels | Live-Spielstand und Einzelduelle |
| Gewinner-Infoscreen | am Ende der Veranstaltung | Gesamtranking und Siegerdarstellung |

Weitere Ansichten können später ergänzt werden.

## 4. Startscreen

### 4.1 Einsatzzeitpunkt

Der Startscreen wird vor Beginn des Turniers angezeigt.

Er ist die Standardansicht, solange noch kein Spiel gestartet wurde und die Turnierleitung noch keine andere Anzeige aktiviert hat.

### 4.2 Inhalt

Der Startscreen soll mindestens enthalten:

- verbindliches Kirchenbirkiger-Schluck-Logo
- aktuelle Uhrzeit
- Turniername
- optional Datum
- optional Veranstaltungsort
- optional kurzer Begrüßungstext

### 4.3 Logo

Als Logo wird ausschließlich das verbindliche Projektlogo verwendet:

```text
Logo_Kirchenbirkiger_Schluck.png
```

Das Logo aus der jeweiligen Jahreseinladung wird nicht als App-Logo verwendet.

### 4.4 Beispielinhalt

```text
[Logo]

Kirchenbirkiger Schluck

18:00 Uhr
Turnier startet in Kürze
```

### 4.5 Verhalten

Der Startscreen kann dauerhaft angezeigt werden, bis das Turnier beginnt.

Optional kann später eine leichte Animation ergänzt werden, zum Beispiel:

- dezente Bewegung im Hintergrund
- animiertes Einblenden des Logos
- Uhrzeit mit ruhiger Aktualisierung
- wechselnde Begrüßungshinweise

Für Version 1 reicht eine funktionale Darstellung.

## 5. Infoscreen

### 5.1 Einsatzzeitpunkt

Der Infoscreen wird vor und zwischen Spielen angezeigt.

Er ist die wichtigste Standardansicht, wenn aktuell kein Spiel läuft.

### 5.2 Inhalt

Der Infoscreen soll mindestens enthalten:

- nächste Partie
- letzte Partie inklusive Ergebnis
- aktuelle Gruppentabelle oder Gruppentabellen
- optional aktueller Turnierfortschritt
- optional Hinweis auf nächste Turnierphase

### 5.3 Nächste Partie

Die nächste Partie soll klar hervorgehoben werden.

Beispiel:

```text
Nächste Partie

Team 1
gegen
Team 2
```

Optional können später ergänzt werden:

- Gruppe
- Spielnummer
- Turnierphase
- geplante Reihenfolge
- Hinweis, ob es sich um ein Platzierungsspiel oder Finalspiel handelt

### 5.4 Letzte Partie

Die letzte abgeschlossene Begegnung soll inklusive Ergebnis angezeigt werden.

Beispiel:

```text
Letzte Partie

Team 3   4 : 2   Team 4
```

Optional können später ergänzt werden:

- Sieger
- Entscheidung regulär oder im Stechen
- kurze Ergebnisanimation

### 5.5 Gruppentabellen

Während der Gruppenphase sollen die aktuellen Gruppentabellen angezeigt werden.

Je nach Anzahl der Gruppen können eine oder mehrere Tabellen dargestellt werden.

Mögliche Tabelleninformationen:

- Platz
- Mannschaft
- Spiele
- Siege
- Niederlagen
- Punkte
- ggf. Duellpunkte
- ggf. Differenz

Die genaue Tabellenlogik wird im Dokument zur Wertungslogik festgelegt.

### 5.6 Verhalten nach Abschluss der Gruppenphase

Wenn die Gruppenphase abgeschlossen ist, hängt die Anzeige vom gewählten Turniermodus ab.

#### Variante A: Turnierbaum vorhanden

Wenn ein Turnierbaum gespielt wird, soll dieser angezeigt werden.

Beispiele:

- Halbfinale / Finale
- Viertelfinale / Halbfinale / Finale
- K.-o.-Baum mit Platzierungsspielen

Der Turnierbaum kann auf dem Infoscreen oder als eigener Unterbereich angezeigt werden.

#### Variante B: Kein klassischer Turnierbaum

Wenn kein Turnierbaum gespielt wird, sondern die gleichplatzierten Mannschaften aus den Gruppen gegeneinander antreten, werden weiterhin die Gruppentabellen oder eine daraus abgeleitete Platzierungsübersicht angezeigt.

Beispiel:

```text
Platz 1 Gruppe A gegen Platz 1 Gruppe B um Gesamtrang 1
Platz 2 Gruppe A gegen Platz 2 Gruppe B um Gesamtrang 3
Platz 3 Gruppe A gegen Platz 3 Gruppe B um Gesamtrang 5
```

### 5.7 Automatische Rotation

Zwischen Spielen können mehrere Infoscreen-Varianten automatisch wechseln.

Beispiele:

- nächste Partie
- letzte Partie
- Gruppentabelle Gruppe A
- Gruppentabelle Gruppe B
- Turnierbaum
- Platzierungsspiele

Die konkrete Rotationslogik wird später im Dokument `Zuschaueranzeige_Ablauf.md` definiert.

## 6. Matchday-Screen

### 6.1 Einsatzzeitpunkt

Der Matchday-Screen wird während eines laufenden Spiels angezeigt.

Er ist die zentrale Live-Ansicht für Zuschauer.

### 6.2 Hauptinhalt

Der Matchday-Screen zeigt oben groß das aktuelle Gesamtergebnis der Partie.

Beispiel:

```text
Team 1     4 : 3     Team 2
```

Der obere Bereich soll mindestens enthalten:

- Teamname Mannschaft 1
- Teamname Mannschaft 2
- aktueller Gesamtstand der Partie
- optional Gruppen- oder Turnierphasenhinweis
- optional Kennzeichnung, ob reguläre Runde oder Stechen läuft

### 6.3 Einzelduell-Liste

Unter dem Gesamtergebnis soll eine Liste der einzelnen Duelle angezeigt werden.

Die Liste zeigt die Ergebnisse der Einzelduelle inklusive Teilnehmernamen.

Beispiel:

```text
Hans Wurst      2 : 1      Max Muster
Teilnehmer 2    0 : 1      Teilnehmer 2
Teilnehmer 3    - : -      Teilnehmer 3
Teilnehmer 4    - : -      Teilnehmer 4
Teilnehmer 5    - : -      Teilnehmer 5
```

Die konkrete Anzeige `2 : 1` steht dabei für das Ergebnis beziehungsweise die Versuchssituation des Einzelduells. Die genaue Notation wird später im UI-Konzept verfeinert.

### 6.4 Spielernamen und Platzhalter

Teilnehmer können entweder bereits vor der Partie angelegt sein oder während der Partie ergänzt werden.

Wenn noch kein konkreter Spielername hinterlegt ist, soll die Anzeige Platzhalter verwenden.

Beispiele:

```text
Teilnehmer 1
Teilnehmer 2
Teilnehmer 3
Teilnehmer 4
Teilnehmer 5
```

Sobald ein Name eingetragen wird, ersetzt dieser den Platzhalter automatisch.

Beispiel:

```text
Teilnehmer 1
```

wird zu:

```text
Hans Wurst
```

### 6.5 Aktuelles Einzelduell

Das aktuell laufende Einzelduell soll deutlich hervorgehoben werden.

Mögliche Informationen:

- aktuelles Einzelduell
- aktueller Versuch
- Spieler links
- Spieler rechts
- Trefferstatus je Spieler
- Hinweis auf Wiederholung
- Hinweis auf dritten Versuch
- Hinweis auf Stechen

### 6.6 Darstellung von Versuchen

Da pro Einzelduell bis zu drei Versuche möglich sind, soll die Anzeige den aktuellen Versuch erkennbar machen.

Beispiel:

```text
Einzelduell 3
Versuch 2 von 3
```

Optional kann später eine Symbolanzeige verwendet werden, zum Beispiel:

```text
Versuch 1: beide getroffen
Versuch 2: Team 1 getroffen, Team 2 nicht getroffen
```

### 6.7 Stechen

Wenn das Spiel nach den regulären fünf Einzelduellen nicht entschieden ist, wechselt der Matchday-Screen in einen Stechen-/Overtime-Zustand.

Mögliche Anzeige:

```text
Stechen

Team 1     5 : 5     Team 2
Nächstes Duell entscheidet
```

Die genaue Darstellung wird später festgelegt.

## 7. Webcam-Bereich im Matchday-Screen

### 7.1 Grundidee

Im Matchday-Screen soll optional eine Webcam-Ansicht zuschaltbar sein.

Diese Webcam-Ansicht kann verwendet werden, um dem Publikum eine Liveansicht des Tisches oder der Gläser zu zeigen.

Besonders sinnvoll ist dies in Entscheidungssituationen, wenn beide Gläser am Tisch stehen und geprüft wird, wer getroffen hat und wer nicht.

### 7.2 Verhalten

Die Webcam-Ansicht soll nicht dauerhaft angezeigt werden.

Sie soll nur bei Bedarf zugeschaltet werden können.

Mögliche Nutzungsarten:

- manuell durch die Turnierleitung aktivieren
- nur während der Bewertung eines Versuchs anzeigen
- kurzzeitig als Bild-in-Bild einblenden
- als größerer Bereich auf dem Matchday-Screen anzeigen

### 7.3 Positionierung

Die Webcam kann in einem Teilbereich des Matchday-Screens angezeigt werden.

Mögliche Varianten:

- rechts neben der Einzelduell-Liste
- unten rechts als Bild-in-Bild
- temporär groß eingeblendet
- als eigener Modus innerhalb des Matchday-Screens

### 7.4 Einordnung für Version 1

Die Webcam-Anbindung ist fachlich sinnvoll, aber nicht zwingend für Version 1.

Für die Planung soll die Anzeige so gedacht werden, dass ein Webcam-Bereich später ergänzt werden kann, ohne die gesamte Anzeige neu aufzubauen.

### 7.5 Entscheidungen zur Webcam

Alle technischen und gestalterischen Fragen zur Webcam (Anschlussart, Aktivierung, Verzögerung, Spiegelung, Zuschnitt, Datenschutz) werden erst entschieden, wenn die Webcam-Integration konkret umgesetzt wird.

Bereits festgelegt:

- **Ton**: nicht vorgesehen.
- **Datenschutz / Einverständnis**: wird entschieden, wenn die Webcam-Funktion umgesetzt wird.

## 8. Gewinner-Infoscreen

### 8.1 Einsatzzeitpunkt

Der Gewinner-Infoscreen wird am Ende der Veranstaltung angezeigt.

Er ist die Abschlussansicht des Turniers.

### 8.2 Inhalt

Der Gewinner-Infoscreen soll das Gesamtranking der Teams anzeigen.

Mindestens enthalten:

- Platzierung
- Mannschaftsname
- ggf. Siege
- ggf. Punkte
- ggf. Finalergebnis
- Hervorhebung des Turniersiegers

Beispiel:

```text
Endstand

1. Team A
2. Team B
3. Team C
4. Team D
```

### 8.3 Darstellung bei Turnierbaum

Wenn ein Turnierbaum gespielt wurde, soll dieser zusätzlich zur tabellarischen Darstellung angezeigt werden.

Mögliche Aufteilung:

```text
Links: Gesamtranking als Tabelle
Rechts: Turnierbaum
```

Oder:

```text
Oben: Sieger / Gewinner
Unten: Turnierbaum und Platzierungen
```

### 8.4 Darstellung ohne Turnierbaum

Wenn kein Turnierbaum gespielt wurde, reicht eine tabellarische Endplatzierung.

Optional können die Final- oder Platzierungsspiele zusätzlich angezeigt werden.

## 9. Manuelle Steuerung durch die Turnierleitung

Die Turnierleitung soll die Anzeige bei Bedarf manuell umschalten können.

Mögliche manuelle Anzeigeaktionen:

- Startscreen anzeigen
- Infoscreen anzeigen
- Matchday-Screen anzeigen
- Gewinner-Infoscreen anzeigen
- Webcam einblenden
- Webcam ausblenden
- Tabelle anzeigen
- Turnierbaum anzeigen
- Anzeige neu laden
- Zuschaueranzeige auf richtigen Bildschirm verschieben

Die manuelle Steuerung ist wichtig, falls die automatische Anzeige nicht zur aktuellen Situation passt.

## 10. Automatische Anzeigewechsel

Die Anwendung soll Anzeigewechsel teilweise automatisch durchführen.

Geplantes Verhalten:

| Situation | Automatische Anzeige |
|---|---|
| Anwendung gestartet, kein Spiel aktiv | Startscreen oder Infoscreen |
| Turnier vorbereitet, nächstes Spiel steht fest | Infoscreen |
| Spiel wird gestartet | Matchday-Screen |
| Versuch wird erfasst | Matchday-Screen aktualisiert sich |
| Einzelduell abgeschlossen | Zwischenstand im Matchday-Screen aktualisieren |
| Spiel abgeschlossen | Ergebnis kurz anzeigen, danach Infoscreen |
| Turnier abgeschlossen | Gewinner-Infoscreen |

Die genaue Zeitsteuerung wird später festgelegt.

## 11. Anforderungen an Lesbarkeit und Darstellung

Die Zuschaueranzeige muss aus einiger Entfernung gut lesbar sein.

Daraus ergeben sich folgende Anforderungen:

- große Schriftgrößen
- klare Kontraste
- wenige, aber wichtige Informationen pro Ansicht
- keine überladenen Tabellen
- deutliche Hervorhebung aktueller Informationen
- ruhige, nachvollziehbare Animationen
- klare Trennung zwischen Teams und Ergebnissen

## 12. Vorläufiger Muss-Umfang für Version 1

Für Version 1 sollen mindestens folgende Ansichten umgesetzt werden:

- Startscreen mit Logo und Uhrzeit
- Infoscreen mit nächster Partie, letzter Partie und Tabellen
- Matchday-Screen mit Teamnamen, Gesamtstand und Einzelduell-Liste
- Gewinner-Infoscreen mit Gesamtranking

Webcam-Anbindung ist für Version 1 optional und kann später ergänzt werden.

## 13. Spätere Erweiterungen

Mögliche spätere Erweiterungen:

- animierter Turnierbaum
- erweiterte Tabellenrotation
- Webcam-Bild-in-Bild
- Sponsorenscreens
- Pausenscreen
- Countdown bis zur nächsten Partie
- Highlight-Animationen bei Treffern
- Siegeranimation
- Soundeffekte
- Konfetti- oder Showanimation beim Turniersieg
- verschiedene Design-Themes

## 14. Offene Punkte

Folgende Gestaltungsfragen werden frei entschieden, sobald die Umsetzung der UI beginnt:

- genaue Bildschirmaufteilung des Matchday-Screens
- genaue Darstellung der Einzelduellergebnisse
- genaue Anzeige der Versuche innerhalb eines Einzelduells
- ob es einen eigenen Turnierbaum-Screen gibt oder der Baum im Infoscreen integriert wird
- wie lange Ergebnisanzeigen nach Spielende sichtbar bleiben
- wie viele Tabellen gleichzeitig angezeigt werden
- wie mit vielen Gruppen auf einem Bildschirm umgegangen wird
- welche Animationen für Version 1 sinnvoll sind

Webcam-spezifische Fragen (Einblendezeitpunkt, Positionierung, technische Details) werden entschieden, wenn die Webcam-Funktion konkret umgesetzt wird.

### Geklärte Entscheidungen (Referenz)

| Frage | Entscheidung |
|-------|-------------|
| Infoscreens automatisch rotieren oder manuell | Automatische Rotation; Turnierleitung kann jederzeit manuell übersteuern |
| Webcam – Ton | Nicht vorgesehen |
| Webcam – alle weiteren technischen Fragen | Werden bei Webcam-Umsetzung entschieden |
| Datenschutz / Einverständnis Webcam | Wird bei Webcam-Umsetzung entschieden |
| Gestaltungsdetails (Layout, Darstellung, Animationen) | Werden frei entschieden bei UI-Umsetzungsstart |
