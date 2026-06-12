/*************************************************************
 * Datei:        Aenderungseintrag.cs
 * Zweck:        Domänenmodell für einen Eintrag im Änderungsprotokoll
 * Bereich:      Domänenmodell – Änderungsprotokoll
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert einen einzelnen Eintrag im Änderungsprotokoll des Turniers.
/// </summary>
/// <remarks>
/// Verantwortung: Revisionssichere Aufzeichnung aller manuellen Änderungen
/// an Spielergebnissen oder anderen Turnierdaten.
/// Einsatzkontext: Änderungseinträge werden beim Korrigieren von Ergebnissen
/// automatisch durch den <c>AenderungsprotokollService</c> angelegt.
/// Besonderheiten: Einträge sind unveränderlich (Append-Only).
/// Die Reihenfolge ergibt sich aus dem Zeitstempel.
/// </remarks>
public class Aenderungseintrag
{
    /// <summary>Eindeutiger Bezeichner des Änderungseintrags.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Zeitpunkt der Änderung (UTC).</summary>
    public DateTime ZeitpunktUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Bezeichnung der betroffenen Entität, z. B. „Spiel" oder „Team".</summary>
    public string Entitaet { get; set; } = string.Empty;

    /// <summary>Id der geänderten Entität.</summary>
    public Guid EntitaetId { get; set; }

    /// <summary>Name des geänderten Felds.</summary>
    public string Feld { get; set; } = string.Empty;

    /// <summary>Wert vor der Änderung als serialisierter String.</summary>
    public string? AlterWert { get; set; }

    /// <summary>Wert nach der Änderung als serialisierter String.</summary>
    public string NeuerWert { get; set; } = string.Empty;

    /// <summary>
    /// Optionale Begründung der Turnierleitung für diese Änderung.
    /// </summary>
    public string? Begruendung { get; set; }
}
