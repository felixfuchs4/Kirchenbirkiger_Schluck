/*************************************************************
 * Datei:        SpielplanService.cs
 * Zweck:        Round-Robin-Spielplangenerierung, Spielreihenfolge und Nächstes-Spiel-Ermittlung
 * Bereich:      Anwendungslogik – Spielplanung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Generierung und Verwaltung des Spielplans bereit.
/// </summary>
public class SpielplanService : ISpielplanService
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Das Turnier hat keine Gruppen, oder eine Gruppe enthält weniger als zwei Teams.
    /// </exception>
    public void GruppenspielplanGenerieren(Turnier turnier)
    {
        if (turnier.Gruppen.Count == 0)
            throw new InvalidOperationException("Das Turnier enthält keine Gruppen.");

        foreach (var gruppe in turnier.Gruppen)
        {
            if (gruppe.TeamIds.Count < 2)
                throw new InvalidOperationException(
                    $"Gruppe \"{gruppe.Name}\" enthält weniger als zwei Teams.");
        }

        // Globale Spielnummer fortsetzen, falls bereits Spiele existieren
        int spielnummer = AlleSpiele(turnier)
            .Select(s => s.Spielnummer)
            .DefaultIfEmpty(0)
            .Max() + 1;

        // Alle Round-Robin-Paare pro Gruppe vorberechnen
        var paareProGruppe = turnier.Gruppen
            .Select(g => RoundRobinPaare(g))
            .ToList();

        // Interleaved nummerieren: Runde 0 aus Gruppe A, Runde 0 aus Gruppe B, …
        int index = 0;
        while (paareProGruppe.Any(p => index < p.Count))
        {
            for (int gi = 0; gi < turnier.Gruppen.Count; gi++)
            {
                if (index >= paareProGruppe[gi].Count)
                    continue;

                var (team1Id, team2Id) = paareProGruppe[gi][index];
                var gruppe = turnier.Gruppen[gi];

                gruppe.Spiele.Add(new Spiel
                {
                    GruppeId    = gruppe.Id,
                    Team1Id     = team1Id,
                    Team2Id     = team2Id,
                    Spielnummer = spielnummer++,
                    Status      = SpielStatus.Geplant
                });
            }

            index++;
        }
    }

    /// <inheritdoc/>
    public Spiel? NaechstesSpielErmitteln(Turnier turnier)
        => AlleSpiele(turnier)
            .Where(s => s.Status == SpielStatus.Geplant)
            .OrderBy(s => s.Spielnummer)
            .FirstOrDefault();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Spiel hat keinen Nachfolger im Spielplan.</exception>
    public void SpielNachHintenVerschieben(Turnier turnier, Guid spielId)
    {
        var alle = AlleSpiele(turnier)
            .OrderBy(s => s.Spielnummer)
            .ToList();

        var ziel = alle.Single(s => s.Id == spielId);
        var naechstes = alle.FirstOrDefault(s => s.Spielnummer > ziel.Spielnummer);

        if (naechstes is null)
            throw new InvalidOperationException(
                $"Spiel {spielId} ist bereits das letzte im Spielplan und kann nicht weiter verschoben werden.");

        // Spielnummern tauschen
        (ziel.Spielnummer, naechstes.Spielnummer) = (naechstes.Spielnummer, ziel.Spielnummer);
    }

    /// <inheritdoc/>
    public void FinalrundeGenerieren(Turnier turnier)
        => throw new NotImplementedException();

    /// <summary>
    /// Gibt alle Spiele des Turniers zurück (Gruppenspiele aller Gruppen + Finalrundenspiele).
    /// </summary>
    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier)
        => turnier.Gruppen.SelectMany(g => g.Spiele)
                  .Concat(turnier.Finalrundenspiele);

    /// <summary>
    /// Berechnet alle Round-Robin-Paare für eine Gruppe in lexikografischer Reihenfolge.
    /// </summary>
    /// <param name="gruppe">Die Gruppe, für die Paare gebildet werden sollen.</param>
    /// <returns>Liste der Team-Id-Paare (Team1Id, Team2Id).</returns>
    private static List<(Guid Team1Id, Guid Team2Id)> RoundRobinPaare(Gruppe gruppe)
    {
        var paare = new List<(Guid, Guid)>();
        var ids = gruppe.TeamIds;

        for (int i = 0; i < ids.Count; i++)
            for (int j = i + 1; j < ids.Count; j++)
                paare.Add((ids[i], ids[j]));

        return paare;
    }
}
