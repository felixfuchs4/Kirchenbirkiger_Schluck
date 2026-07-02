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
    /// </summary>
    /// <param name="turnier">Das auszuwertende Turnier.</param>
    /// <returns>Sortierte Spielerstatistiken (bester zuerst).</returns>
    public IReadOnlyList<SpielerStatistik> TorschuetzenRangliste(Turnier turnier)
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

        IEnumerable<SpielerStatistik> sortiert = turnier.TorschuetzenWertung == TorschuetzenWertung.Prozentual
            ? statistiken.OrderByDescending(s => s.Quote).ThenByDescending(s => s.Treffer)
            : statistiken.OrderByDescending(s => s.Treffer).ThenByDescending(s => s.Quote);

        return sortiert.ToList();
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
