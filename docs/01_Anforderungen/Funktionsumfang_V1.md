# Funktionsumfang Version 1

Dieses Dokument definiert den verbindlichen Umfang für Version 1 der Anwendung.  
Grundlage: [Projektentscheidungen.md §19](../00_Projektstart/Projektentscheidungen.md).

## Muss (V1)

| # | Funktion | Bereich |
|---|----------|---------|
| 1 | Lokale Windows-Anwendung | Plattform |
| 2 | Start per `.exe`, bevorzugt portabel | Bereitstellung |
| 3 | Zwei Bildschirmfenster (Bedienung + Zuschauer) | UI |
| 4 | Vollbild-Zuschaueranzeige auf externem Bildschirm | UI |
| 5 | Mannschaften anlegen und bearbeiten | Turnierverwaltung |
| 6 | Spielernamen erfassen und nachträglich ändern | Turnierverwaltung |
| 7 | Zwei Gruppen als erster Turniermodus | Turniermodus |
| 8 | Spiele erfassen | Spielbetrieb |
| 9 | Einzelrunden erfassen | Spielbetrieb |
| 10 | Rundenergebnisse live auf Zuschaueranzeige | Anzeige |
| 11 | Tabellen automatisch berechnen | Wertung |
| 12 | Final- und Platzierungsspiele abbilden | Turniermodus |
| 13 | Automatische Speicherung als JSON | Datenhaltung |
| 14 | Backup nach jedem Spiel | Datenhaltung |
| 15 | Verbindliches Logo integriert | Design |

## Nice-to-have (nicht V1)

| Funktion | Vorgesehen für |
|----------|---------------|
| Automatische Spielplanerzeugung | V2 |
| Mehrere Turniermodi je Teilnehmerzahl | V2 |
| Export als PDF / Excel / CSV | später |
| Druck Spielplan / Abschlusstabelle | später |
| Finale Animationen und Designausarbeitung | nach V1-Stabilisierung |

## Offen (vor V1 zu klären)

> Siehe [Offene-Fragen.md](../00_Projektstart/Offene-Fragen.md) und [Projektentscheidungen.md §20](../00_Projektstart/Projektentscheidungen.md).

- Konkrete UI-Technologie (WPF / WinUI / Avalonia)
- Genaues JSON-Speicherformat
- Tabellenkriterien bei Punktgleichheit
- Genaue Anzeigeansichten der Zuschaueranzeige
