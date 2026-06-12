/*************************************************************
 * Datei:        TurnierStatus.cs
 * Zweck:        Aufzählung der möglichen Turnierstatus-Werte
 * Bereich:      Domänenmodell – Turnierverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Beschreibt den aktuellen Fortschrittsstand eines Turniers.
/// </summary>
/// <remarks>
/// Verantwortung: Zustandssteuerung des Turnierablaufs.
/// Einsatzkontext: Wird im <c>Turnier</c>-Modell gesetzt und steuert,
/// welche Aktionen in der Bedienoberfläche verfügbar sind.
/// Besonderheiten: Der Status schreitet nur vorwärts (keine Rücksprünge vorgesehen).
/// </remarks>
public enum TurnierStatus
{
    /// <summary>Turnier wurde angelegt, aber noch nicht gestartet.</summary>
    InVorbereitung,

    /// <summary>Gruppenspiele laufen aktiv.</summary>
    Gruppenphase,

    /// <summary>Platzierungsspiele und Finalrunde laufen.</summary>
    Finalrunde,

    /// <summary>Turnier ist beendet, Endstand liegt vor.</summary>
    Abgeschlossen
}
