/*************************************************************
 * Datei:        Versuch.cs
 * Zweck:        Domänenmodell für einen einzelnen Trinkversuch
 * Bereich:      Domänenmodell – Spielablauf
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert einen einzelnen Trinkversuch innerhalb eines Einzelduells.
/// </summary>
/// <remarks>
/// Verantwortung: Speicherung des Trefferresultats beider Spieler für einen Versuch.
/// Einsatzkontext: Pro Einzelduell sind maximal 3 Versuche möglich.
/// Besonderheiten: Wenn beide Spieler dasselbe Ergebnis haben (beide treffen oder
/// beide verfehlen), wird der Versuch als unentschieden gewertet und wiederholt.
/// </remarks>
public class Versuch
{
    /// <summary>Eindeutiger Bezeichner des Versuchs.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Id des zugehörigen Einzelduells.</summary>
    public Guid EinzelduellId { get; set; }

    /// <summary>Laufende Nummer des Versuchs innerhalb eines Einzelduells (1–3).</summary>
    public int Versuchnummer { get; set; }

    /// <summary>Gibt an, ob Spieler 1 (Team 1) in diesem Versuch getroffen hat.</summary>
    public bool Spieler1Getroffen { get; set; }

    /// <summary>Gibt an, ob Spieler 2 (Team 2) in diesem Versuch getroffen hat.</summary>
    public bool Spieler2Getroffen { get; set; }
}
