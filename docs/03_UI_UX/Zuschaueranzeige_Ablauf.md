# Zuschaueranzeige Ablauf

Dieses Dokument beschreibt den geplanten Ablauf der Zuschaueranzeige im Projekt **Kirchenbirkiger Schluck Programm**.

Die konkreten Inhalte und Layouts der einzelnen Screens werden im Dokument `Anzeigeansichten.md` beschrieben.  
Dieses Dokument legt vor allem fest, **wann** welcher Screen angezeigt wird und wie automatische und manuelle Wechsel funktionieren sollen.

## 1. Zweck des Dokuments

Die Zuschaueranzeige soll den Turnierablauf für Zuschauer klar und dynamisch darstellen.

Dieses Dokument beschreibt:

- wann welcher Screen aktiv ist
- welche Screens automatisch wechseln
- wann die Anzeige auf Eingaben der Turnierleitung reagiert
- welche Screens manuell aktiviert werden können
- wie sich die Anzeige vor, während und nach Spielen verhält

## 2. Grundprinzip

Die Zuschaueranzeige läuft dauerhaft im Vollbild auf dem externen Bildschirm.

Sie wird grundsätzlich automatisch vom aktuellen Turnierzustand gesteuert:

- kein Spiel aktiv: Infoscreen oder Startscreen
- Spiel läuft: Matchday-Screen
- Turnier beendet: Gewinner-Screen
- Pause aktiv: Pausenscreen

Die Turnierleitung kann alle Screens bei Bedarf manuell umschalten.

## 3. Screen-Arten

Für den Ablauf werden folgende Screen-Arten betrachtet:

| Screen | Einsatz |
|---|---|
| Startscreen | vor Beginn des Turniers |
| Infoscreen | vor und zwischen Spielen |
| Matchday-Screen | während einer laufenden Partie |
| Pausenscreen | manuell aktivierbare Pause |
| Gewinner-Screen | nach Ende des Turniers |

Die inhaltlichen Details stehen in `Anzeigeansichten.md`.

## 4. Startphase vor Turnierbeginn

Vor Beginn des Turniers kann der Startscreen angezeigt werden.

Der Startscreen zeigt grundsätzlich:

- verbindliches Logo
- Uhrzeit
- optional Turniername
- optional Begrüßung oder Hinweis

Der Startscreen bleibt aktiv, bis die Turnierleitung entweder den Infoscreen aktiviert oder das erste Spiel vorbereitet bzw. startet.

## 5. Infoscreen zwischen den Spielen

Der Infoscreen ist die Standardansicht vor und zwischen Spielen.

Er wird angezeigt, wenn aktuell keine Partie läuft.

Der Infoscreen zeigt Informationen wie:

- nächste Partie
- letzte Partie inklusive Ergebnis
- Gruppentabellen
- Turnierfortschritt
- ggf. Turnierbaum oder Platzierungslogik nach der Gruppenphase

Die konkreten Inhalte sind in `Anzeigeansichten.md` beschrieben.

## 6. Rotation des Infoscreens

Zwischen den Spielen soll der Infoscreen automatisch zwischen einzelnen Informationsblöcken rotieren.

Mögliche Rotationsinhalte:

- nächste Partie
- letzte Partie
- Tabelle Gruppe A
- Tabelle Gruppe B
- weitere Gruppentabellen
- Turnierbaum, falls vorhanden
- Platzierungsspiele oder Finalspiele
- allgemeine Turnierinformationen

Die Rotation läuft automatisch, solange kein Spiel gestartet wurde und kein anderer Screen manuell aktiviert ist.

Die konkrete Dauer je Informationsblock wird später festgelegt.

## 7. Start einer Partie

Wenn die Turnierleitung eine Partie startet, wechselt die Zuschaueranzeige automatisch auf den Matchday-Screen.

Der Matchday-Screen zeigt die aktuellen Matchinformationen.

Mindestens sichtbar sein sollen:

- Mannschaft 1
- Mannschaft 2
- aktueller Gesamtstand
- aktuelle Einzelduelle
- Teilnehmernamen oder Platzhalter
- aktueller Stand innerhalb der laufenden Partie

## 8. Während einer Partie

Während einer laufenden Partie bleibt der Matchday-Screen aktiv.

Die Anzeige reagiert auf Eingaben der Turnierleitung, insbesondere auf:

- Auswahl oder Änderung der Spieler
- Erfassung eines Versuchs
- Treffer / Nicht-Treffer je Spieler
- automatisches Ergebnis eines Einzelduells
- Änderung des Gesamtstands
- Beginn eines Stechens
- Abschluss des Spiels

Während eines Spiels soll die Anzeige nicht automatisch zu einem Infoscreen wechseln.

Der Matchday-Screen hat während einer laufenden Partie Priorität.

## 9. Nach Abschluss einer Partie

Wenn eine Partie abgeschlossen wird, soll die Zuschaueranzeige zunächst das Endergebnis anzeigen.

Danach wechselt die Anzeige wieder in den Infoscreen-Modus.

Der Infoscreen zeigt dann aktualisierte Informationen, zum Beispiel:

- letzte Partie inklusive Ergebnis
- nächste Partie
- aktualisierte Tabelle
- aktualisierte Platzierung
- ggf. aktualisierter Turnierbaum

Die genaue Zeitspanne, wie lange das Endergebnis stehen bleibt, wird später festgelegt.

## 10. Nach Abschluss der Gruppenphase

Nach Abschluss der Gruppenphase bleibt grundsätzlich der Infoscreen aktiv.

Die Inhalte ändern sich abhängig vom gewählten Turniermodus.

Wenn ein Turnierbaum vorhanden ist, kann der Infoscreen diesen anzeigen oder in die Rotation aufnehmen.

Wenn kein klassischer Turnierbaum vorhanden ist, werden weiterhin Gruppentabellen oder die daraus abgeleiteten Platzierungsspiele angezeigt.

Beispiel:

```text
Platz 1 Gruppe A gegen Platz 1 Gruppe B um Gesamtrang 1
Platz 2 Gruppe A gegen Platz 2 Gruppe B um Gesamtrang 3
```

## 11. Turnierende

Nach Ende des Turniers wechselt die Zuschaueranzeige auf den Gewinner-Screen.

Der Gewinner-Screen zeigt das Gesamtranking der Teams.

Wenn ein Turnierbaum gespielt wurde, kann zusätzlich der Turnierbaum angezeigt werden.

Der Gewinner-Screen bleibt aktiv, bis die Turnierleitung manuell eine andere Anzeige wählt oder die Anwendung beendet.

## 12. Pausenscreen

Ein Pausenscreen kann manuell durch die Turnierleitung aktiviert werden.

Er ist für Situationen vorgesehen wie:

- organisatorische Pause
- technische Unterbrechung
- Essenspause
- Verzögerung im Spielplan
- Umbau oder Klärung am Spielort

Der Pausenscreen unterbricht die automatische Infoscreen-Rotation.

Wenn die Pause beendet wird, kann die Turnierleitung wieder zum Infoscreen, Matchday-Screen oder einer anderen Ansicht wechseln.

### Möglicher Inhalt des Pausenscreens

Der Pausenscreen kann später zum Beispiel enthalten:

- Logo
- Hinweis „Pause“
- Uhrzeit
- optional Hinweistext
- optional nächste Partie

Für Version 1 reicht eine einfache funktionale Darstellung.

## 13. Manuelle Screen-Steuerung

Alle Screens sollen durch die Turnierleitung manuell geschaltet werden können.

Mögliche manuelle Aktionen:

- Startscreen anzeigen
- Infoscreen anzeigen
- Matchday-Screen anzeigen
- Pausenscreen anzeigen
- Gewinner-Screen anzeigen
- nächste Info innerhalb der Rotation anzeigen
- vorherige Info innerhalb der Rotation anzeigen
- automatische Rotation pausieren
- automatische Rotation fortsetzen

Die manuelle Steuerung ist wichtig, falls der automatische Ablauf nicht zur aktuellen Turniersituation passt.

## 14. Priorität der Anzeigezustände

Damit die Anzeige eindeutig gesteuert werden kann, sollte es eine klare Priorität der Zustände geben.

Vorläufige Priorität:

1. Manuell aktivierter Screen
2. Laufende Partie
3. Turnier beendet
4. Pause
5. Infoscreen zwischen Spielen
6. Startscreen vor Turnierbeginn

Diese Priorität kann später technisch angepasst werden.

Wichtig ist: Während einer laufenden Partie soll der Matchday-Screen nicht versehentlich durch die automatische Infoscreen-Rotation überblendet werden.

## 15. Verhalten bei Fehlern

Wenn die Zuschaueranzeige nicht korrekt dargestellt wird, soll die Turnierleitung manuell eingreifen können.

Mögliche Probleme:

- falscher Bildschirm
- Anzeige nicht im Vollbild
- externer Bildschirm nicht verbunden
- Windows-Anzeige steht auf Duplizieren statt Erweitern
- Zuschaueranzeige wurde geschlossen
- automatischer Screenwechsel passt nicht zur Situation

Mögliche Bedienaktionen:

- Zuschaueranzeige neu starten
- Bildschirmzuordnung ändern
- gewünschten Screen manuell anzeigen
- Vollbild erneut aktivieren
- Infoscreen-Rotation neu starten

## 16. Einordnung Webcam / Livebild

Eine Webcam-Ansicht wird in diesem Dokument nicht weiter ausgearbeitet.

Die Datei `Livebild_Webcam.md` wird erst später erstellt, wenn die Webcam-Anbindung wirklich umgesetzt werden soll.

Für die aktuelle Planung gilt nur:

- Webcam ist eine optionale spätere Erweiterung.
- Der Matchday-Screen soll grundsätzlich so geplant werden, dass ein Livebild später ergänzt werden kann.
- Eine dauerhafte Webcam-Anzeige ist aktuell nicht vorgesehen.

## 17. Vorläufiger Ablauf als Zustandsmodell

```text
Anwendung startet
        |
        v
Startscreen oder Infoscreen
        |
        | Turnierleitung startet Partie
        v
Matchday-Screen
        |
        | Partie läuft, Eingaben werden erfasst
        v
Matchday-Screen aktualisiert sich live
        |
        | Partie wird abgeschlossen
        v
Endergebnis / aktualisierte Daten
        |
        v
Infoscreen-Rotation
        |
        | nächste Partie wird gestartet
        v
Matchday-Screen
        |
        | Turnier abgeschlossen
        v
Gewinner-Screen
```

Der Pausenscreen kann jederzeit manuell aktiviert werden und unterbricht den normalen Ablauf.

## 18. Vorläufiger Muss-Umfang für Version 1

Für Version 1 sollen mindestens folgende Ablaufpunkte unterstützt werden:

- Startscreen vor Turnierbeginn
- Infoscreen zwischen Spielen
- automatische Rotation im Infoscreen
- Matchday-Screen während eines Spiels
- automatische Aktualisierung des Matchday-Screens durch Eingaben der Turnierleitung
- Rückkehr zum Infoscreen nach Spielabschluss
- Gewinner-Screen nach Turnierende
- manuell aktivierbarer Pausenscreen
- manuelle Umschaltung aller Hauptscreens

## 19. Offene Punkte

Folgende Details müssen später noch festgelegt werden:

- Dauer je Infoscreen-Element
- genaue Reihenfolge der Infoscreen-Rotation
- ob die Rotation je Turnierphase unterschiedlich ist
- wie lange das Endergebnis nach einer Partie angezeigt wird
- ob der Pausenscreen automatisch eine nächste Partie anzeigen soll
- ob manuelle Screens nach einer gewissen Zeit automatisch zurückspringen
- wie die Turnierleitung erkennt, welcher Screen gerade aktiv ist
- wie Screenwechsel animiert werden
- welche Screenwechsel für Version 1 rein funktional bleiben
