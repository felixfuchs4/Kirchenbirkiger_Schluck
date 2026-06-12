/*************************************************************
 * Datei:        Spiel.cs
 * Zweck:        Domänenmodell für eine Partie zwischen zwei Mannschaften
 * Bereich:      Domänenmodell – Spielplanung und -ablauf
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert eine vollständige Partie zwischen zwei Mannschaften.
/// </summary>
/// <remarks>
/// Verantwortung: Datenhaltung für Planung und Durchführung einer Partie
/// inklusive aller Einzelduelle und des Abschlussergebnisses.
/// Einsatzkontext: Spiele entstehen entweder als Gruppenspiele (mit GruppeId)
/// oder als Finalrundenspiele (GruppeId = null).
/// Besonderheiten: <c>Ergebnis</c> ist null solange das Spiel läuft oder
/// noch nicht begonnen hat. Bei <c>SpielStatus.Abgesetzt</c> bleiben die
/// Einzelduelle leer und die Partie wird nicht gewertet.
/// </remarks>
public class Spiel
{
    /// <summary>Eindeutiger Bezeichner der Partie.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Id der Gruppe, zu der diese Partie gehört.
    /// Null für Finalrundenspiele.
    /// </summary>
    public Guid? GruppeId { get; set; }

    /// <summary>Laufende Spielnummer innerhalb der Gruppenphase oder Finalrunde.</summary>
    public int Spielnummer { get; set; }

    /// <summary>Id von Team 1 (in der Spielplan-Reihenfolge).</summary>
    public Guid Team1Id { get; set; }

    /// <summary>Id von Team 2 (in der Spielplan-Reihenfolge).</summary>
    public Guid Team2Id { get; set; }

    /// <summary>Aktueller Status der Partie.</summary>
    public SpielStatus Status { get; set; } = SpielStatus.Geplant;

    /// <summary>
    /// Geplanter Startzeitpunkt der Partie. Optional; kann zur Anzeigesteuerung genutzt werden.
    /// </summary>
    public DateTime? GeplantUm { get; set; }

    /// <summary>Tatsächlicher Startzeitpunkt (UTC). Wird beim Beginn gesetzt.</summary>
    public DateTime? StartZeitpunktUtc { get; set; }

    /// <summary>Tatsächlicher Endzeitpunkt (UTC). Wird beim Abschluss gesetzt.</summary>
    public DateTime? EndZeitpunktUtc { get; set; }

    /// <summary>Alle Einzelduelle dieser Partie in chronologischer Reihenfolge.</summary>
    public List<Einzelduell> Einzelduelle { get; set; } = [];

    /// <summary>
    /// Abschlussergebnis der Partie. Null solange die Partie noch läuft.
    /// </summary>
    public SpielErgebnis? Ergebnis { get; set; }
}
