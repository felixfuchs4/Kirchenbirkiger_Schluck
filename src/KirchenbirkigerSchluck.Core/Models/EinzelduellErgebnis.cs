/*************************************************************
 * Datei:        EinzelduellErgebnis.cs
 * Zweck:        Domänenmodell für das Ergebnis eines Einzelduells
 * Bereich:      Domänenmodell – Spielablauf
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Enthält das Abschlussergebnis eines Einzelduells.
/// Wird direkt im <c>Einzelduell</c>-Objekt gespeichert (eingebettetes Value Object).
/// </summary>
/// <remarks>
/// Verantwortung: Festhalten von Sieger und Duellpunkten nach Abschluss des Duells.
/// Einsatzkontext: Wird gesetzt, sobald ein Einzelduell entschieden ist oder
/// nach drei Versuchen ohne klaren Sieger endet.
/// Besonderheiten: <c>SiegerId</c> ist null bei Unentschieden.
/// Duellpunkte können 0/0 (keiner getroffen), 1/1 (beide getroffen)
/// oder 1/0 bzw. 0/1 (klarer Sieger) sein.
/// </remarks>
public class EinzelduellErgebnis
{
    /// <summary>
    /// Id des Siegers (Team-Id). Null wenn das Duell unentschieden endet.
    /// </summary>
    public Guid? SiegerId { get; set; }

    /// <summary>
    /// Duellpunkt für Team 1 (0 oder 1).
    /// 1 wenn Team 1 gewonnen hat oder beide mind. einmal getroffen haben.
    /// </summary>
    public int DuellpunktTeam1 { get; set; }

    /// <summary>
    /// Duellpunkt für Team 2 (0 oder 1).
    /// 1 wenn Team 2 gewonnen hat oder beide mind. einmal getroffen haben.
    /// </summary>
    public int DuellpunktTeam2 { get; set; }
}
