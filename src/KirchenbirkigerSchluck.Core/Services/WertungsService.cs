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
            .Where(s => s.Status == SpielStatus.Abgeschlossen
                     && s.Ergebnis is not null
                     && s.Team1Id.HasValue && s.Team2Id.HasValue
                     && !s.IstPlatzierungsStechen)
            .ToList();

        // Platzierungs-Stechen werden nicht gewertet, sondern lösen nur Gleichstände auf
        var stechenSpiele = gruppe.Spiele
            .Where(s => s.IstPlatzierungsStechen
                     && s.Status == SpielStatus.Abgeschlossen
                     && s.Ergebnis is not null
                     && s.Team1Id.HasValue && s.Team2Id.HasValue)
            .ToList();

        // Statistiken aus abgeschlossenen Spielen akkumulieren
        foreach (var spiel in gewerteteSpiele)
        {
            var (pts1, pts2) = TabellenPunkteBerechnen(spiel, wertungssystem);

            if (spiel.Team1Id is { } t1Id && eintraege.TryGetValue(t1Id, out var e1))
            {
                e1.Spiele++;
                e1.Tabellenpunkte += pts1;
                e1.DuellpunkteGewonnen += spiel.Ergebnis!.DuellpunkteTeam1;
                e1.DuellpunkteVerloren += spiel.Ergebnis.DuellpunkteTeam2;
                if (spiel.Ergebnis.SiegerId == t1Id) e1.Siege++;
                else e1.Niederlagen++;
            }

            if (spiel.Team2Id is { } t2Id && eintraege.TryGetValue(t2Id, out var e2))
            {
                e2.Spiele++;
                e2.Tabellenpunkte += pts2;
                e2.DuellpunkteGewonnen += spiel.Ergebnis!.DuellpunkteTeam2;
                e2.DuellpunkteVerloren += spiel.Ergebnis.DuellpunkteTeam1;
                if (spiel.Ergebnis.SiegerId == t2Id) e2.Siege++;
                else e2.Niederlagen++;
            }
        }

        // Primärsortierung nach Tabellenpunkten (absteigend)
        var sortiert = eintraege.Values
            .OrderByDescending(e => e.Tabellenpunkte)
            .ToList();

        // Ein Stechen ist erst sinnvoll, wenn alle regulären Gruppenspiele abgeschlossen sind
        var regulaereSpiele = gruppe.Spiele.Where(s => !s.IstPlatzierungsStechen).ToList();
        bool gruppeKomplett = regulaereSpiele.Count > 0
            && regulaereSpiele.All(s => s.Status is SpielStatus.Abgeschlossen or SpielStatus.Korrigiert);

        // Tiebreaker: Direkter Vergleich, dann Platzierungs-Stechen
        GleichstandAufloesen(sortiert, gewerteteSpiele, stechenSpiele, wertungssystem, gruppeKomplett);

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
        List<Spiel> stechenSpiele,
        Wertungssystem wertungssystem,
        bool gruppeKomplett)
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
                s.Team1Id.HasValue && gleichstandTeams.Contains(s.Team1Id.Value) &&
                s.Team2Id.HasValue && gleichstandTeams.Contains(s.Team2Id.Value)))
            {
                var (pts1, pts2) = TabellenPunkteBerechnen(spiel, wertungssystem);
                subPunkte[spiel.Team1Id!.Value] += pts1;
                subPunkte[spiel.Team2Id!.Value] += pts2;
            }

            // Stechen-Siege nur zwischen den gleichpunktigen Teams zählen
            var stechenSiege = gleichstandTeams.ToDictionary(id => id, _ => 0);
            foreach (var spiel in stechenSpiele.Where(s =>
                s.Team1Id.HasValue && gleichstandTeams.Contains(s.Team1Id.Value) &&
                s.Team2Id.HasValue && gleichstandTeams.Contains(s.Team2Id.Value)))
            {
                if (spiel.Ergebnis!.SiegerId is { } sieger && stechenSiege.ContainsKey(sieger))
                    stechenSiege[sieger]++;
            }

            // Positionen der Gleichstand-Teams in der Hauptliste merken
            var indices = sortiert
                .Select((e, i) => (e, i))
                .Where(x => gleichstandTeams.Contains(x.e.TeamId))
                .Select(x => x.i)
                .ToList();

            // Sortierung: erst Direkter Vergleich, dann Stechen-Siege
            var neuSortiert = indices
                .Select(i => sortiert[i])
                .OrderByDescending(e => subPunkte[e.TeamId])
                .ThenByDescending(e => stechenSiege[e.TeamId])
                .ToList();

            // Teams, die nach beidem noch gleichauf sind, brauchen (noch) ein Stechen –
            // aber nur, wenn die Gruppenphase vollständig gespielt ist.
            if (gruppeKomplett)
            {
                foreach (var rest in neuSortiert
                    .GroupBy(e => (subPunkte[e.TeamId], stechenSiege[e.TeamId]))
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g))
                {
                    rest.StehenErforderlich = true;
                }
            }

            // Neu sortierte Einträge an die ursprünglichen Positionen zurückschreiben
            for (int i = 0; i < indices.Count; i++)
                sortiert[indices[i]] = neuSortiert[i];
        }
    }
}
