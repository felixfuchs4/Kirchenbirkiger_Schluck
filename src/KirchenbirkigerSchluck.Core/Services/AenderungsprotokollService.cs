/*************************************************************
 * Datei:        AenderungsprotokollService.cs
 * Zweck:        Implementierung des Änderungsprotokolls
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
    {
        var eintrag = new Aenderungseintrag
        {
            Entitaet    = entitaet,
            EntitaetId  = entitaetId,
            Feld        = feld,
            AlterWert   = alterWert,
            NeuerWert   = neuerWert,
            Begruendung = begruendung
        };
        turnier.Aenderungsprotokoll.Add(eintrag);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Aenderungseintrag> EintraegeAbfragen(Turnier turnier, Guid entitaetId)
    {
        return turnier.Aenderungsprotokoll
            .Where(e => e.EntitaetId == entitaetId)
            .OrderByDescending(e => e.ZeitpunktUtc)
            .ToList()
            .AsReadOnly();
    }
}
