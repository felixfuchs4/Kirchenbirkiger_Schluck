/*************************************************************
 * Datei:        WertungsService.cs
 * Zweck:        Berechnung von Tabellenpunkten und Gruppenranglisten
 * Bereich:      Anwendungslogik – Wertung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Berechnung von Tabellenpunkten und Gruppenranglisten bereit.
/// </summary>
public class WertungsService : IWertungsService
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Spiel ist nicht abgeschlossen oder hat kein Ergebnis.</exception>
    public (int PunkteTeam1, int PunkteTeam2) TabellenPunkteBerechnen(Spiel spiel, Wertungssystem wertungssystem)
    {
        if (spiel.Status != SpielStatus.Abgeschlossen || spiel.Ergebnis is null)
            throw new InvalidOperationException(
                $"Tabellenpunkte können nur für abgeschlossene Spiele berechnet werden (SpielId: {spiel.Id}).");

        var ergebnis = spiel.Ergebnis;
        bool team1Gewonnen = ergebnis.SiegerId == spiel.Team1Id;

        return wertungssystem switch
        {
            Wertungssystem.Eishockey => ergebnis.EntschiedenDurch switch
            {
                EntscheidungsArt.RegulaereSpielzeit => team1Gewonnen ? (3, 0) : (0, 3),
                EntscheidungsArt.Stechen            => team1Gewonnen ? (2, 1) : (1, 2),
                _ => throw new ArgumentOutOfRangeException(nameof(ergebnis.EntschiedenDurch))
            },
            Wertungssystem.Einfach => team1Gewonnen ? (1, 0) : (0, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(wertungssystem))
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<GruppenTabellenEintrag> GruppenRanglisteBerechnen(Gruppe gruppe, Wertungssystem wertungssystem)
    {
        // Basiseinträge für alle Teams der Gruppe initialisieren
        var eintraege = gruppe.TeamIds
            .ToDictionary(id => id, id => new GruppenTabellenEintrag { TeamId = id });

        var gewerteteSpiele = gruppe.Spiele
            .Where(s => s.Status == SpielStatus.Abgeschlossen && s.Ergebnis is not null)
            .ToList();

        // Statistiken aus abgeschlossenen Spielen akkumulieren
        foreach (var spiel in gewerteteSpiele)
        {
            var (pts1, pts2) = TabellenPunkteBerechnen(spiel, wertungssystem);

            if (eintraege.TryGetValue(spiel.Team1Id, out var e1))
            {
                e1.Spiele++;
                e1.Tabellenpunkte += pts1;
                e1.DuellpunkteGewonnen += spiel.Ergebnis!.DuellpunkteTeam1;
                e1.DuellpunkteVerloren += spiel.Ergebnis.DuellpunkteTeam2;
                if (spiel.Ergebnis.SiegerId == spiel.Team1Id) e1.Siege++;
                else e1.Niederlagen++;
            }

            if (eintraege.TryGetValue(spiel.Team2Id, out var e2))
            {
                e2.Spiele++;
                e2.Tabellenpunkte += pts2;
                e2.DuellpunkteGewonnen += spiel.Ergebnis!.DuellpunkteTeam2;
                e2.DuellpunkteVerloren += spiel.Ergebnis.DuellpunkteTeam1;
                if (spiel.Ergebnis.SiegerId == spiel.Team2Id) e2.Siege++;
                else e2.Niederlagen++;
            }
        }

        // Primärsortierung nach Tabellenpunkten (absteigend)
        var sortiert = eintraege.Values
            .OrderByDescending(e => e.Tabellenpunkte)
            .ToList();

        // Tiebreaker: Direkter Vergleich innerhalb jeder Gleichstand-Gruppe
        GleichstandAufloesen(sortiert, gewerteteSpiele, wertungssystem);

        // Positionen 1-basiert vergeben
        for (int i = 0; i < sortiert.Count; i++)
            sortiert[i].Position = i + 1;

        return sortiert.AsReadOnly();
    }

    /// <summary>
    /// Löst Gleichstände in der sortierten Rangliste per Direktem Vergleich auf.
    /// Verbleibende Gleichstände werden mit <c>StehenErforderlich = true</c> markiert.
    /// </summary>
    private void GleichstandAufloesen(
        List<GruppenTabellenEintrag> sortiert,
        List<Spiel> gewerteteSpiele,
        Wertungssystem wertungssystem)
    {
        var gleichstandGruppen = sortiert
            .GroupBy(e => e.Tabellenpunkte)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var gleichstandGruppe in gleichstandGruppen)
        {
            var gleichstandTeams = gleichstandGruppe.Select(e => e.TeamId).ToHashSet();

            // Sub-Punkte aus direkten Spielen nur zwischen den gleichpunktigen Teams
            var subPunkte = gleichstandTeams.ToDictionary(id => id, _ => 0);

            foreach (var spiel in gewerteteSpiele.Where(s =>
                gleichstandTeams.Contains(s.Team1Id) && gleichstandTeams.Contains(s.Team2Id)))
            {
                var (pts1, pts2) = TabellenPunkteBerechnen(spiel, wertungssystem);
                subPunkte[spiel.Team1Id] += pts1;
                subPunkte[spiel.Team2Id] += pts2;
            }

            // Positionen der Gleichstand-Teams in der Hauptliste merken
            var indices = sortiert
                .Select((e, i) => (e, i))
                .Where(x => gleichstandTeams.Contains(x.e.TeamId))
                .Select(x => x.i)
                .ToList();

            // Innerhalb der Gruppe nach Sub-Punkten absteigend sortieren
            var neuSortiert = indices
                .Select(i => sortiert[i])
                .OrderByDescending(e => subPunkte[e.TeamId])
                .ToList();

            // Noch immer punktgleiche Teams als Stechen-pflichtig markieren
            foreach (var subGleichstand in neuSortiert
                .GroupBy(e => subPunkte[e.TeamId])
                .Where(g => g.Count() > 1)
                .SelectMany(g => g))
            {
                subGleichstand.StehenErforderlich = true;
            }

            // Neu sortierte Einträge an die ursprünglichen Positionen zurückschreiben
            for (int i = 0; i < indices.Count; i++)
                sortiert[indices[i]] = neuSortiert[i];
        }
    }
}
