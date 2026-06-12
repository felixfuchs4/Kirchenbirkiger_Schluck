# Rundenerfassung

Dieses Dokument beschreibt, wie Einzelrunden eines Spiels erfasst und angezeigt werden.

## Grundsatz

Alle Einzelrunden eines Spiels werden **einzeln** durch die Turnierleitung erfasst.  
Die Ergebnisse werden **live** auf der Zuschaueranzeige angezeigt.

## Ablauf einer Runde

1. Turnierleitung startet die Runde
2. Rundenausgang wird eingegeben (welche Mannschaft gewinnt die Runde)
3. Rundenstand wird sofort auf Zuschaueranzeige aktualisiert
4. Nächste Runde beginnt oder Spiel endet

## Erfassungsmaske

> TODO: Welche Eingaben sind pro Runde nötig?  
> Möglichkeiten: Sieger-Auswahl per Knopfdruck, Punkteingabe, sonstige Spieldetails.

## Anzeige während des Spiels

> TODO: Wie wird der aktuelle Rundenstand auf der Zuschaueranzeige dargestellt?  
> Siehe [Zuschaueranzeige_Ablauf.md](../03_UI_UX/Zuschaueranzeige_Ablauf.md).

## Speicherung

Jede Runde wird einzeln gespeichert.  
Nach Spielende wird automatisch ein Backup erstellt.

> Siehe [Notfall_und_Backup.md](../06_Betrieb/Notfall_und_Backup.md).

## Sonderfälle

> TODO: Runde abbrechen, Ergebnis korrigieren, Spiel neu starten.  
> Siehe [Sonderfaelle.md](Sonderfaelle.md).
