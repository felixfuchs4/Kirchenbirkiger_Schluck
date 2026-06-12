/*************************************************************
 * Datei:        AenderungsprotokollService.cs
 * Zweck:        Stub-Implementierung des IAenderungsprotokollService
 * Bereich:      Anwendungslogik – Revisionsverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Verwaltung des Änderungsprotokolls bereit.
/// </summary>
public class AenderungsprotokollService : IAenderungsprotokollService
{
    /// <inheritdoc/>
    public void EintragErstellen(
        Turnier turnier,
        string entitaet,
        Guid entitaetId,
        string feld,
        string? alterWert,
        string neuerWert,
        string? begruendung = null)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public IReadOnlyList<Aenderungseintrag> EintraegeAbfragen(Turnier turnier, Guid entitaetId)
        => throw new NotImplementedException();
}
