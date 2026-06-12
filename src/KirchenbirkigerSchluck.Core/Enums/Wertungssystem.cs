/*************************************************************
 * Datei:        Wertungssystem.cs
 * Zweck:        Aufzählung der verfügbaren Tabellenwertungssysteme
 * Bereich:      Domänenmodell – Wertungslogik
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Legt das Punktesystem für die Tabellenwertung nach einer Partie fest.
/// </summary>
/// <remarks>
/// Verantwortung: Steuert die Tabellenpunktberechnung im <c>WertungsService</c>.
/// Einsatzkontext: Wird einmalig bei der Turniererstellung gewählt und kann
/// während des laufenden Turniers nicht mehr geändert werden.
/// Besonderheiten: <c>Eishockey</c> ist der Standard. <c>Einfach</c> ist
/// optional bei der Turniererstellung wählbar.
/// </remarks>
public enum Wertungssystem
{
    /// <summary>
    /// Standard-Wertung analog zum Eishockey:
    /// regulärer Sieg = 3 Punkte, Sieg im Stechen = 2 Punkte,
    /// Niederlage im Stechen = 1 Punkt, reguläre Niederlage = 0 Punkte.
    /// </summary>
    Eishockey,

    /// <summary>
    /// Einfache Wertung: Sieg = 1 Punkt, Niederlage = 0 Punkte.
    /// Kein Unterschied zwischen regulärem Sieg und Stechen-Sieg.
    /// </summary>
    Einfach
}
