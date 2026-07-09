# Finalrunde und Platzierungsspiele

## Finalrunde

Nach der Gruppenphase folgt eine K.-o.-Finalrunde. Es gibt zwei Modi:

- **K.-o.-Baum** (`KoBaumEin`, Standard): klassischer Turnierbaum bis zum Finale, inkl. Spiel um Platz 3.
- **Kurze Finalrunde** (`Kurz`): Gleichplatzierte beider Gruppen spielen direkt gegeneinander
  (Platz 1 Gruppe A gegen Platz 1 Gruppe B usw.). **Nur für genau zwei Gruppen definiert.**

### Teilnehmer und Setzung (K.-o.-Baum)

Der K.-o.-Baum wird für **jede** Gruppenanzahl (1, 2, 3, 4 …) nach denselben Regeln erzeugt:

- **Alle** Teams aller Gruppen ziehen in die Finalrunde ein; niemand scheidet nach der Gruppenphase aus.
- Die Setzung erfolgt **ebenenweise nach Gruppenplatzierung**: zuerst alle Gruppensieger, dann alle
  Gruppenzweiten, danach alle Gruppendritten usw. Innerhalb einer Platzierungs-Ebene (z. B. mehrere
  Gruppensieger) wird die Reihenfolge **ausgelost**.
- Die Bracketgröße ist die kleinste Zweierpotenz ≥ Teamzahl. Ist die Teamzahl keine Zweierpotenz,
  entstehen **Freilose**: sie gehen an die **bestplatzierten** Teams zuerst; bei mehreren gleich gut
  platzierten Teams entscheidet die Auslosung, wer das Freilos erhält. Ein Freilos-Team rückt ohne
  Spiel eine Runde weiter (z. B. direkt ins Viertelfinale).
- **Gruppendurchmischung:** Die Teams werden so ins Bracket gesetzt, dass Mannschaften derselben
  Gruppe möglichst **spät** aufeinandertreffen – frühe K.-o.-Spiele sind also nach Möglichkeit
  Begegnungen unterschiedlicher Gruppen. Wo eine vollständige Trennung nicht möglich ist (mehr Teams
  je Gruppe als Bracket-Bereiche), wird die Begegnung so weit wie möglich nach hinten geschoben.

## Spielbaum

```
… → Achtelfinale → Viertelfinale → Halbfinale → Finale
                                              → Spiel um Platz 3
```

Die Rundennamen richten sich nach der Teilnehmerzahl der Runde (2 = Finale, 4 = Halbfinale,
8 = Viertelfinale, 16 = Achtelfinale, 32 = Sechzehntelfinale …).

## Platzierungsspiele

- **Finale**: die beiden Halbfinal-Sieger; bestimmt Platz 1 und 2.
- **Spiel um Platz 3**: die beiden Halbfinal-Verlierer; bestimmt Platz 3 und 4. Es wird **nur**
  erzeugt, wenn es zwei echte Halbfinal-Spiele gibt. Führt ein Freilos dazu, dass nur ein Halbfinale
  ausgetragen wird, ist dessen Verlierer automatisch Dritter (kein separates Spiel um Platz 3).

## Regelung bei Gleichstand

Endet ein K.-o.-Spiel unentschieden, entscheidet das **Stechen / Overtime** (siehe
[Spielablauf.md](Spielablauf.md), Abschnitt „Stechen / Overtime"). Ein K.-o.-Spiel endet damit immer
mit einem eindeutigen Sieger, der in die nächste Runde einzieht.
