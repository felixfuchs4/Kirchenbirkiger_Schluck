# Smoketest-Checkliste – Kirchenbirkiger Schluck

> Manueller Abnahmetest der ersten lauffähigen Version.
> Jede Zeile mit ✅ (bestanden), ❌ (Fehler) oder ⏭ (übersprungen) markieren.

---

## Block 1 – Anwendungsstart

| # | Was prüfen | Erwartet |
|---|---|---|
| 1.1 | App starten (kein turnier.json vorhanden) | Beide Fenster öffnen sich. Bedienoberfläche: „Kein Turnier geladen". Anzeigeoberfläche: Startscreen mit Uhrzeit |
| 1.2 | Uhrzeit im Startscreen | Aktualisiert sich sekündlich |
| 1.3 | Beide Fenster schließbar | App beendet sich sauber, kein Absturz |

---

## Block 2 – Turnier anlegen (Setup-Flow)

| # | Was prüfen | Erwartet |
|---|---|---|
| 2.1 | „Turnier erstellen"-Button ohne Anlass | Button deaktiviert |
| 2.2 | Anlass eingeben, Datum wählen, Erstellen klicken | Chip wechselt auf „In Vorbereitung". Hinweistext: „Teams und Spieler eintragen…" |
| 2.3 | Anzeigeoberfläche nach Erstellung | Bleibt auf Startscreen, zeigt aber Turniernamen |
| 2.4 | „Status weiterschalten" ohne Teams | Fehlermeldung: „Mindestens 2 aktive Teams erforderlich" |

---

## Block 3 – Teams und Spieler verwalten

| # | Was prüfen | Erwartet |
|---|---|---|
| 3.1 | Team ohne Namen hinzufügen | „+"-Button deaktiviert |
| 3.2 | Team „Team A" hinzufügen | Erscheint in der Liste |
| 3.3 | Team „Team B" hinzufügen | Erscheint in der Liste |
| 3.4 | Auf „Team A" klicken | Spielerverwaltungs-Bereich erscheint mit Header „Spieler von Team A" |
| 3.5 | Spieler „Max Mustermann" hinzufügen | Erscheint in der Spielerliste unterhalb |
| 3.6 | Spieler „Anna Muster" hinzufügen | Erscheint ebenfalls |
| 3.7 | Auf „Team B" wechseln | Spielerliste leert sich, zeigt Team B's Spieler (leer) |
| 3.8 | Spieler zu Team B hinzufügen | Funktioniert analog |
| 3.9 | Kein Team ausgewählt (Klick ins Leere) | Hinweis „← Team aus der Liste auswählen…" erscheint |

---

## Block 4 – Gruppenphase starten

| # | Was prüfen | Erwartet |
|---|---|---|
| 4.1 | Anzahl Gruppen auf 1 setzen, „Status weiterschalten" | Chip: „Gruppenphase läuft". Hinweistext: „Spiele starten und Ergebnisse erfassen…" |
| 4.2 | Anzeigeoberfläche nach Statuswechsel | Wechselt automatisch auf Infoscreen |
| 4.3 | Teams ohne Spieler erhalten Platzhalter | Im Spielsteuerungs-Tab (nach Spielstart) ComboBox zeigt „Spieler 1"–„Spieler 5" |
| 4.4 | Spielplan-Tab öffnen | Spielliste mit allen Gruppenspielen sichtbar. Nächstes Spiel hervorgehoben. |

---

## Block 5 – Spiel starten und steuern

| # | Was prüfen | Erwartet |
|---|---|---|
| 5.1 | „Nächstes Spiel starten" im Spielplan-Tab | Anzeigeoberfläche wechselt auf Matchday-Screen. Bedienoberfläche navigiert automatisch zu Spielsteuerung |
| 5.2 | Spielsteuerungs-Tab: Score-Header | Team-Namen korrekt, Score 0:0 |
| 5.3 | Duell-Auswahl: ComboBox 1 + ComboBox 2 | Beide nebeneinander (kein Überlappen), „vs." dazwischen |
| 5.4 | Spieler auswählen, „Duell starten" | Duell startet. Versuch-Buttons aktiv. Text „Versuch 1 von 3" |
| 5.5 | „Team A trifft" klicken | Versuch gezählt. Text wechselt auf „Versuch 2 von 3" |
| 5.6 | 3 Versuche bis Duell-Ende | Duell-Übersicht unten zeigt Ergebnis. Nächstes Duell wählbar. |
| 5.7 | Matchday-Screen live | Duellergebnis-Stand aktualisiert sich sofort bei jedem Versuch |
| 5.8 | 5 Duelle abgeschlossen, ein Sieger | „Spiel abschließen"-Button erscheint |
| 5.9 | „Spiel abschließen" – Bestätigungsdialog | Dialog mit Ergebnis. Bestätigen wechselt Anzeige auf Infoscreen. |
| 5.10 | Nach Spielabschluss: Spielplan aktualisiert | Spiel als „Abgeschlossen" markiert, nächstes Spiel bereit |

---

## Block 6 – Infoscreen (Anzeigeoberfläche)

| # | Was prüfen | Erwartet |
|---|---|---|
| 6.1 | Infoscreen rotiert automatisch | Alle 5 Sekunden wechselt der Slide (Nächste Partie / Letzte Partie / Tabelle) |
| 6.2 | Letzte Partie nach Spielabschluss | Ergebnis des gerade abgeschlossenen Spiels erscheint |
| 6.3 | Gruppenrangliste | Teams mit Punkten korrekt sortiert |

---

## Block 7 – Finalrunde starten

| # | Was prüfen | Erwartet |
|---|---|---|
| 7.1 | Alle Gruppenspiele abgeschlossen → Statuswechsel | Finalrunde generiert. Chip: „Finalrunde". Anzeige → Infoscreen |
| 7.2 | Spielplan-Tab: Finalrundenspiele vorhanden | KO-Baum oder Kurz-Spiele je nach Modus |
| 7.3 | Finalspiel starten + abschließen | Bracket-Weiterkommen: Sieger erscheint im nächsten Spiel |

---

## Block 8 – Turnier abschließen

| # | Was prüfen | Erwartet |
|---|---|---|
| 8.1 | Statuswechsel nach Finalrunde | Chip: „Abgeschlossen". Anzeige → Gewinner-Screen |
| 8.2 | Gewinner-Screen | Sieger hervorgehoben, Endplatzierungsliste korrekt |
| 8.3 | „Status weiterschalten" nach Abschluss | Button deaktiviert |

---

## Block 9 – Einstellungen und Backup

| # | Was prüfen | Erwartet |
|---|---|---|
| 9.1 | „Backup jetzt erstellen" | Datei in `backups/` erscheint |
| 9.2 | Manuell Anzeige umschalten (Startscreen) | Anzeigeoberfläche zeigt Startscreen |
| 9.3 | Manuell auf Infoscreen | Anzeige wechselt |
| 9.4 | Rotationsintervall auf 10 Sek. ändern | Infoscreen rotiert langsamer |

---

## Block 10 – Auto-Load beim Neustart

| # | Was prüfen | Erwartet |
|---|---|---|
| 10.1 | App schließen (mit laufendem Turnier) | Kein Absturz |
| 10.2 | App neu starten | `turnier.json` wird automatisch geladen. Letzter Stand wiederhergestellt |
| 10.3 | Turnierstatus nach Reload | Gleicher Status wie vor dem Schließen |

---

## Block 11 – Robustheit / Randfall

| # | Was prüfen | Erwartet |
|---|---|---|
| 11.1 | Spiel starten ohne laufendes Turnier | Spielsteuerungs-Tab zeigt „Kein laufendes Spiel" |
| 11.2 | Stechen-Szenario: 5 Duelle 2:3 → kein Stechen nötig | Spiel direkt abschließbar |
| 11.3 | Stechen-Szenario: 5 Duelle 2:2 + 1 Unentschieden | Stechen-Chip erscheint, Stechen-Duell startbar |

---

*Erstellt: 2026-06-12*
