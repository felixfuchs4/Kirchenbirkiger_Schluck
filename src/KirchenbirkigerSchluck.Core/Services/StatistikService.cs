/*************************************************************
 * Datei:        StatistikService.cs
 * Zweck:        Ermittelt Spielerstatistiken (Treffer/Versuche) und die Torschützen-Rangliste
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

    /// <summary>
    /// Platzierung in der Torschützen-Rangliste (1 = bester). Bei exaktem Gleichstand im
    /// gewählten Wertungskriterium teilen sich mehrere Spieler denselben Platz – außer auf
    /// Platz 1, wo ein Stechen (siehe <see cref="Turnier.TorschuetzenStechenSiegerId"/>) über
    /// die Alleinstellung entscheidet.
    /// </summary>
    public int Platz { get; set; }
}

/// <summary>
/// Berechnet Spielerstatistiken aus allen Einzelduellen eines Turniers und liefert die
/// Torschützen-Rangliste gemäß der gewählten Wertungsart.
/// </summary>
public class StatistikService
{
    /// <summary>
    /// Liefert die Torschützen-Rangliste: bei <see cref="TorschuetzenWertung.Absolut"/> nach
    /// Treffern, bei <see cref="TorschuetzenWertung.Prozentual"/> nach Trefferquote sortiert.
    /// Bei exaktem Gleichstand teilen sich Spieler denselben Platz; ist Platz 1 betroffen,
    /// wird ein in <see cref="Turnier.TorschuetzenStechenSiegerId"/> hinterlegtes Stechen-
    /// Ergebnis angewendet, sofern der Sieger noch zur aktuellen Gleichstandsgruppe gehört.
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Sortierte Spielerstatistiken (bester zuerst) mit gesetztem <see cref="SpielerStatistik.Platz"/>.</returns>
    public IReadOnlyList<SpielerStatistik> TorschuetzenRangliste(Turnier turnier)
    {
        var gruppen = GleichstandsGruppen(turnier);
        StechenAufloesungAnwenden(turnier, gruppen);

        var ergebnis = new List<SpielerStatistik>();
        int naechsterPlatz = 1;
        foreach (var gruppe in gruppen)
        {
            foreach (var spieler in gruppe)
            {
                spieler.Platz = naechsterPlatz;
                ergebnis.Add(spieler);
            }
            naechsterPlatz += gruppe.Count;
        }

        return ergebnis;
    }

    /// <summary>
    /// Liefert die Spieler, die aktuell auf Platz 1 exakt gleichauf liegen (vor Anwendung
    /// eines evtl. hinterlegten Stechen-Ergebnisses). Leer, wenn kein Gleichstand besteht.
    /// </summary>
    public IReadOnlyList<SpielerStatistik> GleichstandPlatz1(Turnier turnier)
    {
        var gruppen = GleichstandsGruppen(turnier);
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
    private List<List<SpielerStatistik>> GleichstandsGruppen(Turnier turnier)
    {
        bool prozentual = turnier.TorschuetzenWertung == TorschuetzenWertung.Prozentual;

        var sortiert = StatistikenErmitteln(turnier)
            .OrderByDescending(s => prozentual ? s.Quote : s.Treffer)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var gruppen = new List<List<SpielerStatistik>>();
        foreach (var spieler in sortiert)
        {
            var letzteGruppe = gruppen.Count > 0 ? gruppen[^1] : null;
            if (letzteGruppe is not null && GleicherWert(letzteGruppe[0], spieler, prozentual))
                letzteGruppe.Add(spieler);
            else
                gruppen.Add([spieler]);
        }

        return gruppen;
    }

    /// <summary>
    /// Vergleicht zwei Spieler auf exakten Gleichstand im Wertungskriterium. Die Trefferquote
    /// wird per Kreuzmultiplikation (Bruchvergleich) statt Fließkomma-Gleichheit geprüft, um
    /// Rundungsfehler zu vermeiden.
    /// </summary>
    private static bool GleicherWert(SpielerStatistik a, SpielerStatistik b, bool prozentual) =>
        prozentual ? (long)a.Treffer * b.Versuche == (long)b.Treffer * a.Versuche : a.Treffer == b.Treffer;

    /// <summary>Ermittelt für jeden Spieler mit mindestens einem Versuch die Trefferstatistik.</summary>
    private static List<SpielerStatistik> StatistikenErmitteln(Turnier turnier)
    {
        var treffer = new Dictionary<Guid, int>();
        var versuche = new Dictionary<Guid, int>();

        foreach (var duell in AlleSpiele(turnier).SelectMany(s => s.Einzelduelle))
        {
            int anzahl = duell.Versuche.Count;
            Akkumulieren(treffer, versuche, duell.Spieler1Id, duell.Versuche.Count(v => v.Spieler1Getroffen), anzahl);
            Akkumulieren(treffer, versuche, duell.Spieler2Id, duell.Versuche.Count(v => v.Spieler2Getroffen), anzahl);
        }

        // Spieler-zu-Team-Zuordnung aufbauen
        var statistiken = new List<SpielerStatistik>();
        foreach (var team in turnier.Teams)
        {
            foreach (var spieler in team.Spieler)
            {
                if (!versuche.TryGetValue(spieler.Id, out var v) || v == 0)
                    continue; // Spieler ohne Versuche nicht werten

                statistiken.Add(new SpielerStatistik
                {
                    SpielerId = spieler.Id,
                    Name      = spieler.Name,
                    TeamName  = team.Name,
                    LogoPfad  = team.LogoPfad,
                    Treffer   = treffer.GetValueOrDefault(spieler.Id),
                    Versuche  = v
                });
            }
        }

        return statistiken;
    }

    private static void Akkumulieren(
        Dictionary<Guid, int> treffer, Dictionary<Guid, int> versuche,
        Guid spielerId, int neueTreffer, int neueVersuche)
    {
        if (spielerId == Guid.Empty) return;
        treffer[spielerId] = treffer.GetValueOrDefault(spielerId) + neueTreffer;
        versuche[spielerId] = versuche.GetValueOrDefault(spielerId) + neueVersuche;
    }

    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier) =>
        turnier.Gruppen.SelectMany(g => g.Spiele).Concat(turnier.Finalrundenspiele);
}
