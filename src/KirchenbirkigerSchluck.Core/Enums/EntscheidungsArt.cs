/*************************************************************
 * Datei:        EntscheidungsArt.cs
 * Zweck:        Aufzählung der möglichen Entscheidungsarten einer Partie
 * Bereich:      Domänenmodell – Spielergebnis
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Gibt an, in welcher Phase der Sieger einer Partie ermittelt wurde.
/// </summary>
/// <remarks>
/// Verantwortung: Unterscheidung für die Tabellenpunktberechnung im Eishockey-System.
/// Einsatzkontext: Wird im <c>SpielErgebnis</c> gesetzt und vom
/// <c>WertungsService</c> zur Punkteermittlung ausgewertet.
/// </remarks>
public enum EntscheidungsArt
{
    /// <summary>
    /// Sieger wurde innerhalb der regulären fünf Einzelduelle ermittelt.
    /// Im Eishockey-System: Sieger erhält 3 Punkte, Verlierer 0.
    /// </summary>
    RegulaereSpielzeit,

    /// <summary>
    /// Sieger wurde erst im Stechen (Overtime) ermittelt.
    /// Im Eishockey-System: Sieger erhält 2 Punkte, Verlierer 1.
    /// </summary>
    Stechen
}
