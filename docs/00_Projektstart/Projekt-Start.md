# Projekt-Start: Kirchenbirkiger Schluck Programm

## 1. Ausgangslage

Für den Kirchenbirkiger Schluck soll eine eigene Anwendung entstehen, die den Turnierablauf übersichtlich, flexibel und optisch ansprechend darstellt. Die Anwendung soll die bisherige Excel-/Makro-Lösung ablösen oder zumindest funktional ersetzen.

Die Anzeige soll großformatig auf einem Fernseher oder PC-Bildschirm laufen. Die Bedienung erfolgt gleichzeitig über einen Laptop, der per HDMI mit dem Bildschirm verbunden ist. Dort werden Mannschaften, Spielplan, Ergebnisse und Turnierstände gepflegt.

Das Programm wird voraussichtlich in C#/.NET umgesetzt.

## 2. Ziel der Anwendung

Ziel ist eine lokale Turniersteuerung mit zwei klar getrennten Bereichen:

1. **Bedienoberfläche für die Turnierleitung**
   - Mannschaften anlegen und bearbeiten
   - Teilnehmerzahl flexibel verwalten
   - Gruppen und Spielplan erstellen oder anpassen
   - Ergebnisse eintragen
   - Tabellen und Platzierungen automatisch aktualisieren
   - Anzeigeansicht steuern

2. **Anzeigeoberfläche für Zuschauer**
   - große, gut lesbare Darstellung auf Fernseher/Monitor
   - aktuelles Spiel anzeigen
   - nächste Spiele anzeigen
   - Gruppenstände und Tabellen anzeigen
   - Final-/Platzierungsspiele anzeigen
   - Abschlussplatzierung anzeigen

Die Anwendung soll für unterschiedliche Teilnehmerzahlen geeignet sein. Für das kommende Turnier sind 12 Gruppen eingeladen, die endgültige Teilnehmerzahl steht aber noch nicht fest. Deshalb darf das System nicht starr auf eine feste Anzahl von Mannschaften oder Gruppen begrenzt sein.

## 3. Verbindliche Design-Grundlage

Als Logo wird immer das vom Nutzer bereitgestellte Logo verwendet:

```text
Logo_Kirchenbirkiger_Schluck.png
```

Das Logo aus der Einladung des jeweiligen Jahres ist nicht maßgeblich für die Anwendung.

Die visuelle Gestaltung soll sich am hochgeladenen Logo orientieren:

- Hauptfarben: Rot, Schwarz, Weiß / helle Kontrastflächen
- Stil: kräftig, modern, veranstaltungstauglich
- gute Lesbarkeit auf größerer Entfernung
- geeignet für TV-/Monitoranzeige

## 4. Ausgewertete Quellen

Für die erste Projektplanung wurden folgende Quellen berücksichtigt:

- Einladung / Anleitung zum Kirchenbirkiger Schluck 2025
- hochgeladenes Kirchenbirkiger-Schluck-Logo
- bisherige Excel-Datei mit Makros für Spielplan, Gruppenphase und Abschlusstabelle
- Einladung zum Turnier 2026 als aktueller Veranstaltungskontext

Die Einladung 2026 ist für die Kernlogik der Anwendung nicht maßgeblich. Turniername, Datum, Ort und Ausrichter sollen später als konfigurierbare Turnierdaten behandelt werden.

## 5. Fachlicher Rahmen

Der Kirchenbirkiger Schluck ist ein humorvoller Präzisionswettbewerb. Zwei Teams treten gegeneinander an. Ein Team besteht aus 5 Personen. Ziel ist es, nach dem Trinken den Bierpegel zwischen den Linien „Feigling“ und „Schluckspecht“ zu treffen.

Ein Duell besteht grundsätzlich aus 5 Runden. Nach jeder Runde werden die Spieler gewechselt. Das Team mit den meisten Punkten gewinnt das Duell. Bei Gleichstand erfolgt ein Stechen.

Für die Anwendung bedeutet das:

- Ein Spiel/Duell hat zwei Teams.
- Ein Duell kann mehrere Einzelrunden enthalten.
- Es muss ein Ergebnis pro Duell erfasst werden können.
- Optional kann später auch eine detaillierte Rundenerfassung ergänzt werden.
- Die Tabellenlogik muss zwischen Duellwertung und Turnierwertung unterscheiden.

## 6. Erkenntnisse aus der bisherigen Excel-Lösung

Die vorhandene Excel-Datei bildet bereits eine einfache Turnierverwaltung ab. Sie enthält unter anderem:

- Teilnehmerliste
- Gruppenzuordnung
- Spielplan
- Gruppenphase
- Abschlusstabelle
- Hilfslisten für Dropdowns
- Makros zum Erzeugen von Gruppen und Aktualisieren der Tabellen

Die bisherige Struktur wirkt auf ein festes Szenario ausgelegt:

- 8 Mannschaften
- 2 Gruppen
- Gruppenphase
- Platzierungsspiele um Platz 1, 3, 5 und 7

Für das neue Programm soll diese Grundidee aufgegriffen, aber nicht starr übernommen werden. Die neue Anwendung soll flexibler, übersichtlicher und wartbarer sein.

## 7. Grober Funktionsumfang für die erste Version

### Mannschaften

- Mannschaft anlegen
- Mannschaft bearbeiten
- Mannschaft löschen oder deaktivieren
- optionale Gruppenzuordnung
- flexible Anzahl an Mannschaften

### Turnierkonfiguration

- Turniername
- Jahr / Datum
- Veranstaltungsort
- Logo
- Anzahl Gruppen
- Spielmodus
- Regel-/Wertungseinstellungen

### Spielplan

- Spielplan automatisch erzeugen
- Spielplan manuell nachbearbeiten
- Spiele nach Gruppen oder Phasen darstellen
- aktuelle und nächste Spiele markieren

### Ergebniserfassung

- Ergebnisse für Team 1 und Team 2 eintragen
- Sieger automatisch bestimmen
- Unentschieden oder Stechen berücksichtigen
- Tabellen automatisch aktualisieren

### Anzeige

- Vollbildfähige Anzeige für Fernseher/Monitor
- aktuelle Partie
- nächstes Spiel
- Tabellenstand
- Final-/Platzierungsspiele
- Abschlussplatzierung

### Datenhaltung

- Turnier lokal speichern
- Turnier später wieder öffnen
- Sicherung / Export ermöglichen

## 8. Grobe technische Richtung

Die Anwendung soll als lokale Desktop-Anwendung geplant werden. Da Laptop und Bildschirm per HDMI verbunden sind, bietet sich eine Lösung mit zwei Fenstern an:

- Fenster 1: Bedienoberfläche auf dem Laptop
- Fenster 2: Anzeigeoberfläche im Vollbild auf dem externen Bildschirm

Als Technologie wird zunächst C#/.NET vorgesehen. Die konkrete UI-Technologie wird später entschieden. Mögliche Kandidaten:

- WPF
- WinUI
- Avalonia UI
- .NET MAUI nur bei Bedarf an Plattformunabhängigkeit

Die interne Struktur sollte von Anfang an sauber getrennt werden:

- Turnierlogik
- Datenmodell
- Speicherlogik
- Bedienoberfläche
- Anzeigeoberfläche

## 9. Grobe Datenobjekte

Auf hoher Ebene werden voraussichtlich folgende Objekte benötigt:

- Turnier
- Mannschaft
- Gruppe
- Spiel
- Spielrunde / Duellrunde
- Ergebnis
- Tabelle
- Turnierphase
- Anzeigezustand

Diese Objekte werden später detailliert ausgearbeitet.

## 10. Erste Meilensteine

### Meilenstein 1: Projektgrundlage

- Projekt-Start.md erstellen
- grobe Anforderungen festhalten
- offene Fragen sammeln
- technische Richtung festlegen

### Meilenstein 2: Fachkonzept

- Turniermodus definieren
- Wertungslogik festlegen
- Tabellenlogik festlegen
- Sonderfälle klären

### Meilenstein 3: UI-Konzept

- Bedienansicht grob entwerfen
- Anzeigeansicht grob entwerfen
- Designrichtung anhand des Logos festlegen

### Meilenstein 4: Technischer Projektstart

- C#/.NET-Projektstruktur anlegen
- Datenmodell erstellen
- erste lokale Speicherung vorbereiten
- erste einfache Bedien- und Anzeigeansicht erstellen

### Meilenstein 5: MVP

- Mannschaften anlegen
- Gruppen einteilen
- Spielplan erzeugen
- Ergebnisse eintragen
- Tabelle berechnen
- Anzeige im Vollbild darstellen

## 11. Offene Fragen

Diese Punkte müssen vor der Detailplanung geklärt werden:

- Welche maximale Teamanzahl soll sicher unterstützt werden?
- Soll die Gruppenanzahl automatisch berechnet oder manuell festgelegt werden?
- Welcher Turniermodus wird bei 8, 10, 12 oder anderer Teamanzahl genutzt?
- Gibt es feste Vorgaben für Platzierungsspiele?
- Wie werden Tabellen bei Punktgleichheit sortiert?
- Sollen einzelne Spieler namentlich erfasst werden oder nur Mannschaften?
- Soll jede der 5 Duellrunden einzeln erfasst werden?
- Soll das Stechen detailliert dokumentiert werden?
- Soll es eine Exportfunktion für Ergebnisse geben?
- Soll die Anwendung auch ohne zweiten Bildschirm nutzbar sein?

## 12. Aktuelle Grundsatzentscheidungen

- Die Anwendung wird lokal genutzt.
- Die Anzeige läuft auf einem per HDMI angeschlossenen Bildschirm.
- Die Bedienung erfolgt parallel am Laptop.
- Das hochgeladene Logo ist verbindlich für die Anwendung.
- Die App soll flexibel für unterschiedliche Teilnehmerzahlen sein.
- Die bisherige Excel-Datei dient als fachliche Orientierung, aber nicht als starre technische Vorlage.
- Die Einladung des jeweiligen Jahres liefert nur Veranstaltungskontext, nicht die technische Basis.
