/*************************************************************
 * Datei:        ISpielplanService.cs
 * Zweck:        Schnittstelle für die Spielplan-Generierung und -Verwaltung
 * Bereich:      Anwendungslogik – Spielplanung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Definiert Operationen zum Generieren und Anpassen des Spielplans.
/// </summary>
public interface ISpielplanService
{
    /// <summary>
    /// Generiert den Gruppenspielplan (Round-Robin) für alle Gruppen des Turniers.
    /// </summary>
    /// <param name="turnier">Das Turnierobjekt mit bereits zugeordneten Teams und Gruppen.</param>
    void GruppenspielplanGenerieren(Turnier turnier);

    /// <summary>
    /// Generiert den Finalrundenspielplan auf Basis der Gruppenranglisten.
    /// Wird aufgerufen, nachdem alle Gruppenspiele abgeschlossen sind.
    /// </summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    void FinalrundeGenerieren(Turnier turnier);

    /// <summary>Gibt das nächste noch nicht gestartete Spiel zurück.</summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    Spiel? NaechstesSpielErmitteln(Turnier turnier);

    /// <summary>Verschiebt ein Spiel in der Spielplanreihenfolge nach hinten.</summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    /// <param name="spielId">Id des zu verschiebenden Spiels.</param>
    void SpielNachHintenVerschieben(Turnier turnier, Guid spielId);

    /// <summary>
    /// Trägt nach Abschluss eines Finalrundenspiels den Sieger automatisch in die
    /// Folgerunde des Brackets ein (setzt Team1Id bzw. Team2Id des nächsten Spiels).
    /// </summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    /// <param name="abgeschlossenesSpiel">Das soeben abgeschlossene Finalrundenspiel.</param>
    void BracketFortsetzungAktualisieren(Turnier turnier, Spiel abgeschlossenesSpiel);
}
