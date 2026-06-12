/*************************************************************
 * Datei:        SpielErgebnis.cs
 * Zweck:        Domänenmodell für das Abschlussergebnis einer Partie
 * Bereich:      Domänenmodell – Spielergebnis
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Enthält das vollständige Abschlussergebnis einer Partie.
/// Wird direkt im <c>Spiel</c>-Objekt gespeichert (eingebettetes Value Object).
/// </summary>
/// <remarks>
/// Verantwortung: Unveränderliche Festschreibung des Partieergebnisses nach Abschluss.
/// Einsatzkontext: Wird vom <c>SpielsteuerungService</c> nach Abschluss der letzten
/// Runde berechnet und im <c>Spiel</c> gespeichert.
/// Besonderheiten: Tabellenpunkte werden abhängig vom <c>Wertungssystem</c>
/// des Turniers und der <c>EntscheidungsArt</c> berechnet.
/// </remarks>
public class SpielErgebnis
{
    /// <summary>Id des siegreichen Teams.</summary>
    public Guid SiegerId { get; set; }

    /// <summary>Summe der gewonnenen Einzelduelle von Team 1.</summary>
    public int DuellpunkteTeam1 { get; set; }

    /// <summary>Summe der gewonnenen Einzelduelle von Team 2.</summary>
    public int DuellpunkteTeam2 { get; set; }

    /// <summary>Gibt an, ob der Sieger in regulärer Spielzeit oder im Stechen ermittelt wurde.</summary>
    public EntscheidungsArt EntschiedenDurch { get; set; }

    /// <summary>Tabellenpunkte für Team 1 gemäß gewähltem Wertungssystem.</summary>
    public int TabellenPunkteTeam1 { get; set; }

    /// <summary>Tabellenpunkte für Team 2 gemäß gewähltem Wertungssystem.</summary>
    public int TabellenPunkteTeam2 { get; set; }
}
