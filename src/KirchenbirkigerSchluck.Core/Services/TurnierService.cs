/*************************************************************
 * Datei:        TurnierService.cs
 * Zweck:        Stub-Implementierung des ITurnierService
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
    /// <inheritdoc/>
    public Turnier TurnierErstellen(string anlass, DateOnly datum, Wertungssystem wertungssystem)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Turnier TurnierLaden()
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void TurnierSpeichern(Turnier turnier)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void StatusWechseln(Turnier turnier)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Team TeamHinzufuegen(Turnier turnier, string teamName, string? kurzname = null)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void TeamZurueckziehen(Turnier turnier, Guid teamId)
        => throw new NotImplementedException();
}
