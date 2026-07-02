/*************************************************************
 * Datei:        FinalrundenModus.cs
 * Zweck:        Enum für den Spielmodus der Finalrunde
 * Bereich:      Domänenmodell – Enumerationen
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Legt den Spielmodus der Finalrunde fest.
/// </summary>
public enum FinalrundenModus
{
    /// <summary>
    /// Klassischer KO-Turnierbaum: alle Teams in einem Bracket,
    /// die beiden Halbfinalsieger treffen sich im gemeinsamen Finale.
    /// </summary>
    KoBaumEin,

    /// <summary>
    /// Kurze Finalrunde: Gleichplatzierte aus beiden Gruppen treten
    /// direkt gegeneinander an (A1 vs. B1, A2 vs. B2 usw.).
    /// </summary>
    Kurz
}
