/*************************************************************
 * Datei:        StatistikService.cs
 * Zweck:        Ermittelt Spieler- und Teamstatistiken (Treffer/Versuche/Sieg-Bilanz)
 *               sowie die Torschützen-, Spieler- und Team-Ranglisten
 * Bereich:      Anwendungslogik – Statistik
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>Trefferstatistik eines Spielers über das gesamte Turnier.</summary>
public sealed class SpielerStatistik
{
    /// <summary>Id des Spielers.</summary>
    public Guid SpielerId { get; init; }

    /// <summary>Id des zugehörigen Teams.</summary>
    public Guid TeamId { get; init; }

    /// <summary>Name des Spielers.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Name des zugehörigen Teams.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad des Teams.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Anzahl erzielter Treffer.</summary>
    public int Treffer { get; init; }

    /// <summary>Anzahl absolvierter Versuche.</summary>
    public int Versuche { get; init; }

    /// <summary>Trefferquote (0–1); 0 wenn keine Versuche.</summary>
    public double Quote => Versuche > 0 ? (double)Treffer / Versuche : 0;

    /// <summary>Anzahl abgeschlossener Einzelduelle des Spielers.</summary>
    public int Spiele { get; init; }

    /// <summary>Anzahl gewonnener Einzelduelle.</summary>
    public int Siege { get; init; }

    /// <summary>Anzahl unentschiedener Einzelduelle.</summary>
    public int Unentschieden { get; init; }

    /// <summary>Anzahl verlorener Einzelduelle.</summary>
    public int Niederlagen { get; init; }

    /// <summary>
    /// Platzierung in der Rangliste (1 = bester). Bei exaktem Gleichstand im
    /// gewählten Wertungskriterium teilen sich mehrere Spieler denselben Platz – außer auf
    /// Platz 1, wo ein Stechen (siehe <see cref="Turnier.TorschuetzenStechenSiegerId"/>) über
    /// die Alleinstellung entscheidet.
    /// </summary>
    public int Platz { get; set; }
}

/// <summary>Trefferstatistik eines Teams über das gesamte Turnier (Summe aller Spieler des Teams).</summary>
public sealed class TeamStatistik
{
    /// <summary>Id des Teams.</summary>
    public Guid TeamId { get; init; }

    /// <summary>Name des Teams.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad des Teams.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Anzahl erzielter Treffer aller Spieler des Teams.</summary>
    public int Treffer { get; init; }

    /// <summary>Anzahl absolvierter Versuche aller Spieler des Teams.</summary>
    public int Versuche { get; init; }

    /// <summary>Trefferquote (0–1); 0 wenn keine Versuche.</summary>
    public double Quote => Versuche > 0 ? (double)Treffer / Versuche : 0;

    /// <summary>Platzierung in der Team-Rangliste nach Trefferquote (1 = bestes Team).</summary>
    public int Platz { get; set; }
}

/// <summary>Turnierweite Gesamtstatistik über alle Versuche und Treffer aller Spieler.</summary>
public sealed class GesamtStatistik
{
    /// <summary>Gesamtzahl erzielter Treffer im Turnier.</summary>
    public int Treffer { get; init; }

    /// <summary>Gesamtzahl absolvierter Versuche im Turnier.</summary>
    public int Versuche { get; init; }

    /// <summary>Trefferquote (0–1); 0 wenn keine Versuche.</summary>
    public double Quote => Versuche > 0 ? (double)Treffer / Versuche : 0;
}

/// <summary>
/// Berechnet Spieler- und Teamstatistiken aus allen Einzelduellen eines Turniers und liefert
/// die Torschützen-, Spieler- und Team-Ranglisten gemäß der gewählten Wertungsart.
/// </summary>
public class StatistikService
{
    /// <summary>Rohwerte eines Spielers vor der Zuordnung zu Name/Team.</summary>
    private struct SpielerAkkumulator
    {
        public int Treffer;
        public int Versuche;
        public int Spiele;
        public int Siege;
        public int Unentschieden;
        public int Niederlagen;
    }

    /// <summary>
    /// Liefert die Torschützen-Rangliste für die Siegerehrung: bei <see cref="TorschuetzenWertung.Absolut"/>
    /// nach Treffern, bei <see cref="TorschuetzenWertung.Prozentual"/> nach Trefferquote sortiert.
    /// Spieler ohne Versuche werden nicht gewertet. Bei exaktem Gleichstand teilen sich Spieler
    /// denselben Platz; ist Platz 1 betroffen, wird ein in
    /// <see cref="Turnier.TorschuetzenStechenSiegerId"/> hinterlegtes Stechen-Ergebnis angewendet,
    /// sofern der Sieger noch zur aktuellen Gleichstandsgruppe gehört.
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Sortierte Spielerstatistiken (bester zuerst) mit gesetztem <see cref="SpielerStatistik.Platz"/>.</returns>
    public IReadOnlyList<SpielerStatistik> TorschuetzenRangliste(Turnier turnier) =>
        RanglisteAusGruppen(turnier, GleichstandsGruppen(turnier, nurMitVersuchen: true));

    /// <summary>
    /// Liefert die vollständige Spieler-Rangliste für die Statistik-Ansicht: wie
    /// <see cref="TorschuetzenRangliste"/>, jedoch inklusive aller Spieler ohne Versuche
    /// (Zeile mit 0 Treffern/Versuchen), damit die Ansicht bereits vor Turnierbeginn nutzbar ist.
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Sortierte Spielerstatistiken (bester zuerst) mit gesetztem <see cref="SpielerStatistik.Platz"/>.</returns>
    public IReadOnlyList<SpielerStatistik> AlleSpielerRangliste(Turnier turnier) =>
        RanglisteAusGruppen(turnier, GleichstandsGruppen(turnier, nurMitVersuchen: false));

    /// <summary>
    /// Liefert die Team-Rangliste nach Trefferquote (unabhängig vom Turnier-Wertungskriterium),
    /// inklusive aller Teams ohne Versuche. Es gibt kein Stechen auf Team-Ebene; bei exaktem
    /// Gleichstand teilen sich mehrere Teams denselben Platz.
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Sortierte Teamstatistiken (bestes Team zuerst) mit gesetztem <see cref="TeamStatistik.Platz"/>.</returns>
    public IReadOnlyList<TeamStatistik> TeamRangliste(Turnier turnier)
    {
        var akkumulation = SpielerAkkumulierenAlle(turnier);

        var teamStatistiken = turnier.Teams
            .Select(team =>
            {
                int treffer = 0, versuche = 0;
                foreach (var spieler in team.Spieler)
                {
                    var a = akkumulation.GetValueOrDefault(spieler.Id);
                    treffer += a.Treffer;
                    versuche += a.Versuche;
                }

                return new TeamStatistik
                {
                    TeamId   = team.Id,
                    TeamName = team.Name,
                    LogoPfad = team.LogoPfad,
                    Treffer  = treffer,
                    Versuche = versuche
                };
            })
            .OrderByDescending(t => t.Quote)
            .ThenBy(t => t.TeamName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return PlatzVergeben(Gruppieren(teamStatistiken, GleicherWertTeam), (t, p) => t.Platz = p);
    }

    /// <summary>Ermittelt die turnierweite Gesamtstatistik (Summe aller Versuche/Treffer aller Spieler).</summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Gesamtzahl der Versuche und Treffer über das gesamte Turnier.</returns>
    public GesamtStatistik GesamtStatistikErmitteln(Turnier turnier)
    {
        var akkumulation = SpielerAkkumulierenAlle(turnier);
        return new GesamtStatistik
        {
            Treffer  = akkumulation.Values.Sum(a => a.Treffer),
            Versuche = akkumulation.Values.Sum(a => a.Versuche)
        };
    }

    /// <summary>
    /// Liefert die Spieler, die aktuell auf Platz 1 exakt gleichauf liegen (vor Anwendung
    /// eines evtl. hinterlegten Stechen-Ergebnisses). Leer, wenn kein Gleichstand besteht.
    /// </summary>
    public IReadOnlyList<SpielerStatistik> GleichstandPlatz1(Turnier turnier)
    {
        var gruppen = GleichstandsGruppen(turnier, nurMitVersuchen: true);
        return gruppen.Count > 0 && gruppen[0].Count > 1 ? gruppen[0] : [];
    }

    /// <summary>
    /// Gibt an, ob für Platz 1 noch ein Stechen entschieden werden muss: es gibt einen
    /// Gleichstand, und entweder wurde noch kein Sieger hinterlegt, oder der hinterlegte
    /// Sieger gehört nicht (mehr) zur aktuellen Gleichstandsgruppe.
    /// </summary>
    public bool StechenPlatz1Offen(Turnier turnier)
    {
        var gleichstand = GleichstandPlatz1(turnier);
        return gleichstand.Count > 1 &&
            (turnier.TorschuetzenStechenSiegerId is not { } siegerId ||
             gleichstand.All(s => s.SpielerId != siegerId));
    }

    /// <summary>Wendet das hinterlegte Stechen-Ergebnis an und vergibt anschließend die Plätze.</summary>
    private static List<SpielerStatistik> RanglisteAusGruppen(Turnier turnier, List<List<SpielerStatistik>> gruppen)
    {
        StechenAufloesungAnwenden(turnier, gruppen);
        return PlatzVergeben(gruppen, (s, p) => s.Platz = p);
    }

    /// <summary>Hebt den hinterlegten Stechen-Sieger an die Spitze der Platz-1-Gleichstandsgruppe.</summary>
    private static void StechenAufloesungAnwenden(Turnier turnier, List<List<SpielerStatistik>> gruppen)
    {
        if (gruppen.Count == 0 || gruppen[0].Count <= 1) return;
        if (turnier.TorschuetzenStechenSiegerId is not { } siegerId) return;

        var sieger = gruppen[0].FirstOrDefault(s => s.SpielerId == siegerId);
        if (sieger is null) return; // hinterlegter Sieger gehört nicht mehr zur Gleichstandsgruppe

        gruppen[0].Remove(sieger);
        gruppen.Insert(0, [sieger]);
    }

    /// <summary>
    /// Sortiert alle Spielerstatistiken absteigend nach dem gewählten Wertungskriterium und
    /// fasst exakt gleichauf liegende Spieler zu Gruppen zusammen.
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <param name="nurMitVersuchen">
    /// Wenn <c>true</c>, werden Spieler ohne Versuche ausgeschlossen (Torschützen-Rangliste);
    /// wenn <c>false</c>, werden alle Spieler inklusive 0-Versuche-Zeilen berücksichtigt.
    /// </param>
    private List<List<SpielerStatistik>> GleichstandsGruppen(Turnier turnier, bool nurMitVersuchen)
    {
        bool prozentual = turnier.TorschuetzenWertung == TorschuetzenWertung.Prozentual;

        var sortiert = StatistikenErmitteln(turnier, nurMitVersuchen)
            .OrderByDescending(s => prozentual ? s.Quote : s.Treffer)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Gruppieren(sortiert, (a, b) => GleicherWert(a, b, prozentual));
    }

    /// <summary>Fasst eine bereits sortierte Liste zu Gruppen exakt gleichauf liegender Einträge zusammen.</summary>
    private static List<List<T>> Gruppieren<T>(List<T> sortiert, Func<T, T, bool> exaktGleich)
    {
        var gruppen = new List<List<T>>();
        foreach (var eintrag in sortiert)
        {
            var letzteGruppe = gruppen.Count > 0 ? gruppen[^1] : null;
            if (letzteGruppe is not null && exaktGleich(letzteGruppe[0], eintrag))
                letzteGruppe.Add(eintrag);
            else
                gruppen.Add([eintrag]);
        }

        return gruppen;
    }

    /// <summary>Vergibt Plätze je Gruppe (gleicher Platz innerhalb einer Gruppe, nächster Platz springt um die Gruppengröße).</summary>
    private static List<T> PlatzVergeben<T>(List<List<T>> gruppen, Action<T, int> platzSetzen)
    {
        var ergebnis = new List<T>();
        int naechsterPlatz = 1;
        foreach (var gruppe in gruppen)
        {
            foreach (var eintrag in gruppe)
            {
                platzSetzen(eintrag, naechsterPlatz);
                ergebnis.Add(eintrag);
            }
            naechsterPlatz += gruppe.Count;
        }

        return ergebnis;
    }

    /// <summary>
    /// Vergleicht zwei Spieler auf exakten Gleichstand im Wertungskriterium. Die Trefferquote
    /// wird per Kreuzmultiplikation (Bruchvergleich) statt Fließkomma-Gleichheit geprüft, um
    /// Rundungsfehler zu vermeiden.
    /// </summary>
    private static bool GleicherWert(SpielerStatistik a, SpielerStatistik b, bool prozentual) =>
        prozentual ? (long)a.Treffer * b.Versuche == (long)b.Treffer * a.Versuche : a.Treffer == b.Treffer;

    /// <summary>Vergleicht zwei Teams auf exakten Gleichstand der Trefferquote (Kreuzmultiplikation).</summary>
    private static bool GleicherWertTeam(TeamStatistik a, TeamStatistik b) =>
        (long)a.Treffer * b.Versuche == (long)b.Treffer * a.Versuche;

    /// <summary>
    /// Ermittelt für jeden Spieler die Trefferstatistik. Ist <paramref name="nurMitVersuchen"/>
    /// gesetzt, werden Spieler ohne Versuche ausgeschlossen.
    /// </summary>
    private static List<SpielerStatistik> StatistikenErmitteln(Turnier turnier, bool nurMitVersuchen)
    {
        var akkumulation = SpielerAkkumulierenAlle(turnier);

        var statistiken = new List<SpielerStatistik>();
        foreach (var team in turnier.Teams)
        {
            foreach (var spieler in team.Spieler)
            {
                var a = akkumulation.GetValueOrDefault(spieler.Id);
                if (nurMitVersuchen && a.Versuche == 0)
                    continue; // Spieler ohne Versuche nicht werten

                statistiken.Add(new SpielerStatistik
                {
                    SpielerId     = spieler.Id,
                    TeamId        = team.Id,
                    Name          = spieler.Name,
                    TeamName      = team.Name,
                    LogoPfad      = team.LogoPfad,
                    Treffer       = a.Treffer,
                    Versuche      = a.Versuche,
                    Spiele        = a.Spiele,
                    Siege         = a.Siege,
                    Unentschieden = a.Unentschieden,
                    Niederlagen   = a.Niederlagen
                });
            }
        }

        return statistiken;
    }

    /// <summary>
    /// Akkumuliert Versuche/Treffer sowie Sieg-Bilanz je Spieler über alle Einzelduelle des
    /// Turniers. Versuche/Treffer werden unabhängig vom Ergebnis-Status gezählt (auch bei
    /// laufendem Duell); die Sieg-Bilanz nur für abgeschlossene Duelle mit gesetzten Team-Ids.
    /// </summary>
    private static Dictionary<Guid, SpielerAkkumulator> SpielerAkkumulierenAlle(Turnier turnier)
    {
        var akkumulation = new Dictionary<Guid, SpielerAkkumulator>();

        foreach (var spiel in AlleSpiele(turnier))
        {
            foreach (var duell in spiel.Einzelduelle)
            {
                VersucheAkkumulieren(akkumulation, duell.Spieler1Id, duell.Versuche.Count(v => v.Spieler1Getroffen), duell.Versuche.Count);
                VersucheAkkumulieren(akkumulation, duell.Spieler2Id, duell.Versuche.Count(v => v.Spieler2Getroffen), duell.Versuche.Count);

                if (duell.Ergebnis is not null && spiel.Team1Id is { } team1 && spiel.Team2Id is { } team2)
                {
                    ErgebnisAkkumulieren(akkumulation, duell.Spieler1Id, duell.Ergebnis.SiegerId, team1);
                    ErgebnisAkkumulieren(akkumulation, duell.Spieler2Id, duell.Ergebnis.SiegerId, team2);
                }
            }
        }

        return akkumulation;
    }

    private static void VersucheAkkumulieren(
        Dictionary<Guid, SpielerAkkumulator> akkumulation, Guid spielerId, int neueTreffer, int neueVersuche)
    {
        if (spielerId == Guid.Empty) return;
        var eintrag = akkumulation.GetValueOrDefault(spielerId);
        eintrag.Treffer += neueTreffer;
        eintrag.Versuche += neueVersuche;
        akkumulation[spielerId] = eintrag;
    }

    /// <summary>Wertet das Ergebnis eines Einzelduells für einen Spieler in dessen Sieg-Bilanz.</summary>
    /// <param name="siegerId">Team-Id des Duell-Siegers, oder <c>null</c> bei Unentschieden.</param>
    /// <param name="eigenesTeamId">Team-Id, für die der Spieler in diesem Duell antritt.</param>
    private static void ErgebnisAkkumulieren(
        Dictionary<Guid, SpielerAkkumulator> akkumulation, Guid spielerId, Guid? siegerId, Guid eigenesTeamId)
    {
        if (spielerId == Guid.Empty) return;
        var eintrag = akkumulation.GetValueOrDefault(spielerId);
        eintrag.Spiele++;
        if (siegerId is null) eintrag.Unentschieden++;
        else if (siegerId == eigenesTeamId) eintrag.Siege++;
        else eintrag.Niederlagen++;
        akkumulation[spielerId] = eintrag;
    }

    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier) =>
        turnier.Gruppen.SelectMany(g => g.Spiele).Concat(turnier.Finalrundenspiele);
}
