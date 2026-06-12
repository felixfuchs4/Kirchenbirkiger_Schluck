/*************************************************************
 * Datei:        Gruppe.cs
 * Zweck:        Domänenmodell für eine Turniergruppe der Gruppenphase
 * Bereich:      Domänenmodell – Gruppenphase
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert eine Gruppe der Gruppenphase mit allen zugehörigen Teams und Spielen.
/// </summary>
/// <remarks>
/// Verantwortung: Gruppierung von Teams und Verwaltung des Gruppenspielplans.
/// Einsatzkontext: Das Turnier besteht aus einer oder mehreren Gruppen.
/// Jede Gruppe spielt intern im Round-Robin-Modus.
/// Besonderheiten: Die Gruppenrangliste wird dynamisch aus den Spielergebnissen
/// durch den <c>WertungsService</c> berechnet und nicht persistent gespeichert.
/// </remarks>
public class Gruppe
{
    /// <summary>Eindeutiger Bezeichner der Gruppe.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Anzeigename der Gruppe, z. B. „Gruppe A".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Laufende Nummer der Gruppe innerhalb des Turniers (1-basiert).</summary>
    public int Nummer { get; set; }

    /// <summary>Ids der Teams, die dieser Gruppe angehören.</summary>
    public List<Guid> TeamIds { get; set; } = [];

    /// <summary>Alle Spiele der Gruppenphase innerhalb dieser Gruppe.</summary>
    public List<Spiel> Spiele { get; set; } = [];
}
