/*************************************************************
 * Datei:        TorschuetzenWertung.cs
 * Zweck:        Wertungsart zur Ermittlung des treffsichersten Spielers
 * Bereich:      Domänenmodell – Aufzählungen
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Bestimmt, wie der „treffsicherste Spieler" (Torschützenkönig) ermittelt wird.
/// </summary>
public enum TorschuetzenWertung
{
    /// <summary>Nach der absoluten Anzahl der Treffer.</summary>
    Absolut,

    /// <summary>Nach der prozentualen Trefferquote (Treffer pro Versuche).</summary>
    Prozentual
}
