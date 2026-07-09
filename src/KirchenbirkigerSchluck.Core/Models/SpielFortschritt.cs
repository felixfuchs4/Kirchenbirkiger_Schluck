/*************************************************************
 * Datei:        SpielFortschritt.cs
 * Zweck:        Momentaufnahme des Spielablaufs (Best-of-5) einer Partie
 * Bereich:      Anwendungslogik – Spielsteuerung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Rein lesende Momentaufnahme des Ablaufs einer Partie im Best-of-5-Modus. Wird aus den
/// Einzelduellen berechnet und von der Spielsteuerung zur Ablaufkontrolle verwendet.
/// </summary>
/// <param name="DuellsiegeTeam1">Anzahl klar gewonnener Einzelduelle von Team 1 (inkl. Stechen).</param>
/// <param name="DuellsiegeTeam2">Anzahl klar gewonnener Einzelduelle von Team 2 (inkl. Stechen).</param>
/// <param name="RegulaereAbgeschlossen">Anzahl abgeschlossener regulärer (Nicht-Stechen-)Duelle.</param>
/// <param name="Verbleibend">Anzahl noch möglicher regulärer Duelle (bis maximal fünf).</param>
/// <param name="DuellLaeuft">Gibt an, ob gerade ein Duell läuft (noch ohne Ergebnis).</param>
/// <param name="StechenNoetig">
/// Gibt an, ob sich die Partie im Stechen befindet bzw. eines nötig ist (Gleichstand nach fünf
/// regulären Duellen).
/// </param>
/// <param name="KannAbschliessen">
/// Gibt an, ob die Partie abgeschlossen werden kann: entweder regulär uneinholbar entschieden
/// (Vorsprung größer als die verbleibenden Duelle) oder das Stechen ist mit klarem Sieger entschieden.
/// </param>
public sealed record SpielFortschritt(
    int DuellsiegeTeam1,
    int DuellsiegeTeam2,
    int RegulaereAbgeschlossen,
    int Verbleibend,
    bool DuellLaeuft,
    bool StechenNoetig,
    bool KannAbschliessen);
