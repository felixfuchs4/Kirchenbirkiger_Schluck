/*************************************************************
 * Datei:        ISpielsteuerungService.cs
 * Zweck:        Schnittstelle für die Spielablaufsteuerung
 * Bereich:      Anwendungslogik – Spielsteuerung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Definiert Operationen zur Steuerung des Ablaufs innerhalb einer Partie.
/// </summary>
public interface ISpielsteuerungService
{
    /// <summary>Startet eine Partie und setzt den Status auf <c>Laeuft</c>.</summary>
    /// <param name="spiel">Das zu startende Spiel.</param>
    void SpielStarten(Spiel spiel);

    /// <summary>
    /// Erfasst das Ergebnis eines einzelnen Versuchs innerhalb des aktuellen Duells.
    /// Berechnet automatisch, ob das Duell entschieden ist oder ein weiterer Versuch folgt.
    /// </summary>
    /// <param name="spiel">Das laufende Spiel.</param>
    /// <param name="spieler1Getroffen">Hat Spieler 1 in diesem Versuch getroffen?</param>
    /// <param name="spieler2Getroffen">Hat Spieler 2 in diesem Versuch getroffen?</param>
    void VersuchErfassen(Spiel spiel, bool spieler1Getroffen, bool spieler2Getroffen);

    /// <summary>
    /// Startet das nächste Einzelduell im aktuellen Spiel.
    /// Wird nach explizitem Klick der Turnierleitung aufgerufen.
    /// </summary>
    /// <param name="spiel">Das laufende Spiel.</param>
    /// <param name="spieler1Id">Id des Spielers von Team 1 für dieses Duell.</param>
    /// <param name="spieler2Id">Id des Spielers von Team 2 für dieses Duell.</param>
    void NaechesDuellStarten(Spiel spiel, Guid spieler1Id, Guid spieler2Id);

    /// <summary>
    /// Startet ein Stechen, wenn nach 5 Duellen ein Unentschieden vorliegt.
    /// Spielerauswahl ist beim Stechen frei.
    /// </summary>
    /// <param name="spiel">Das Spiel mit 5 abgeschlossenen Duellen und Unentschieden.</param>
    /// <param name="spieler1Id">Frei gewählter Spieler von Team 1.</param>
    /// <param name="spieler2Id">Frei gewählter Spieler von Team 2.</param>
    void StechenStarten(Spiel spiel, Guid spieler1Id, Guid spieler2Id);

    /// <summary>Schließt ein Spiel ab und berechnet das endgültige Ergebnis und die Tabellenpunkte.</summary>
    /// <param name="spiel">Das abzuschließende Spiel.</param>
    /// <param name="turnier">Das Turnier (für Wertungssystem-Zugriff).</param>
    void SpielAbschliessen(Spiel spiel, Turnier turnier);
}
