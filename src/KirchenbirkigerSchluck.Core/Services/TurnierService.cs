/*************************************************************
 * Datei:        TurnierService.cs
 * Zweck:        Implementierung der Turnierverwaltungs-Operationen
 * Bereich:      Anwendungslogik – Turnierverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Turnierverwaltung bereit.
/// </summary>
public class TurnierService : ITurnierService
{
    private readonly ITurnierRepository _repository;

    /// <summary>
    /// Initialisiert den Service mit dem Persistenz-Repository.
    /// </summary>
    /// <param name="repository">Repository für Laden und Speichern des Turniers.</param>
    public TurnierService(ITurnierRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Turnier TurnierErstellen(string anlass, DateOnly datum, Wertungssystem wertungssystem)
    {
        return new Turnier
        {
            Anlass = anlass,
            Datum = datum,
            Wertungssystem = wertungssystem
        };
    }

    /// <inheritdoc/>
    public Turnier TurnierLaden() => _repository.Laden();

    /// <inheritdoc/>
    public void TurnierSpeichern(Turnier turnier) => _repository.Speichern(turnier);

    /// <inheritdoc/>
    public void StatusWechseln(Turnier turnier)
    {
        turnier.Status = turnier.Status switch
        {
            TurnierStatus.InVorbereitung => TurnierStatus.Gruppenphase,
            TurnierStatus.Gruppenphase   => TurnierStatus.Finalrunde,
            TurnierStatus.Finalrunde     => TurnierStatus.Abgeschlossen,
            TurnierStatus.Abgeschlossen  => throw new InvalidOperationException(
                "Das Turnier ist bereits abgeschlossen und kann nicht weitergeschaltet werden."),
            _ => throw new InvalidOperationException($"Unbekannter Turnierstatus: {turnier.Status}")
        };
    }

    /// <inheritdoc/>
    public Team TeamHinzufuegen(Turnier turnier, string teamName, string? kurzname = null)
    {
        var team = new Team
        {
            Name = teamName,
            Kurzname = kurzname
        };
        turnier.Teams.Add(team);
        return team;
    }

    /// <inheritdoc/>
    public void TeamZurueckziehen(Turnier turnier, Guid teamId)
    {
        var team = turnier.Teams.FirstOrDefault(t => t.Id == teamId)
            ?? throw new InvalidOperationException($"Team mit Id '{teamId}' wurde nicht gefunden.");

        team.Status = TeamStatus.Zurueckgezogen;

        var alleSpiele = turnier.Gruppen.SelectMany(g => g.Spiele)
            .Concat(turnier.Finalrundenspiele);

        foreach (var spiel in alleSpiele)
        {
            if ((spiel.Team1Id == teamId || spiel.Team2Id == teamId)
                && spiel.Status is SpielStatus.Geplant or SpielStatus.Vorbereitet or SpielStatus.Verschoben)
            {
                spiel.Status = SpielStatus.Abgesetzt;
            }
        }
    }
}
