/*************************************************************
 * Datei:        TeamStatus.cs
 * Zweck:        Aufzählung der möglichen Team-Teilnahmestatus-Werte
 * Bereich:      Domänenmodell – Mannschaftsverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Beschreibt den Teilnahmestatus einer Mannschaft im Turnier.
/// </summary>
/// <remarks>
/// Verantwortung: Steuerung der Sonderfall-Logik bei zurückgezogenen Teams.
/// Einsatzkontext: Wird in <c>Team</c> gesetzt; bei <c>Zurueckgezogen</c>
/// werden alle offenen Spiele automatisch als <c>Abgesetzt</c> markiert.
/// </remarks>
public enum TeamStatus
{
    /// <summary>Mannschaft nimmt regulär am Turnier teil.</summary>
    Aktiv,

    /// <summary>
    /// Mannschaft hat sich zurückgezogen. Alle noch nicht gespielten Partien
    /// werden abgesetzt und ohne Punktvergabe gewertet.
    /// </summary>
    Zurueckgezogen
}
