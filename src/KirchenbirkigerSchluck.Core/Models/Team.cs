/*************************************************************
 * Datei:        Team.cs
 * Zweck:        Domänenmodell für eine teilnehmende Mannschaft
 * Bereich:      Domänenmodell – Mannschaftsverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert eine teilnehmende Mannschaft im Turnier.
/// </summary>
/// <remarks>
/// Verantwortung: Datenhaltung für Mannschaftsdaten und Spielerliste.
/// Einsatzkontext: Teams sind einer Gruppe zugeordnet und nehmen an Spielen teil.
/// Besonderheiten: Bei Status <c>Zurueckgezogen</c> werden alle noch offenen
/// Spiele automatisch als abgesetzt markiert.
/// </remarks>
public class Team
{
    /// <summary>Eindeutiger Bezeichner der Mannschaft.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Vollständiger Mannschaftsname, z. B. „Die Bierbrauer".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optionale Kurzbezeichnung für platzbeschränkte Anzeigen, z. B. „BBR".
    /// </summary>
    public string? Kurzname { get; set; }

    /// <summary>Aktueller Teilnahmestatus der Mannschaft.</summary>
    public TeamStatus Status { get; set; } = TeamStatus.Aktiv;

    /// <summary>
    /// Optionaler relativer Pfad zur Logo-Bilddatei (z. B. „logos/&lt;id&gt;.png").
    /// Null, wenn kein Logo hinterlegt ist.
    /// </summary>
    public string? LogoPfad { get; set; }

    /// <summary>Liste der Spieler in dieser Mannschaft.</summary>
    public List<Spieler> Spieler { get; set; } = [];
}
