/*************************************************************
 * Datei:        ITurnierService.cs
 * Zweck:        Schnittstelle für Turnierverwaltungs-Operationen
 * Bereich:      Anwendungslogik – Turnierverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Definiert Operationen zum Erstellen, Laden und Verwalten eines Turniers.
/// </summary>
public interface ITurnierService
{
    /// <summary>Erstellt ein neues Turnier mit Pflichtfeldern und gibt es zurück.</summary>
    /// <param name="anlass">Name/Anlass des Turniers (Pflicht).</param>
    /// <param name="datum">Turnierdatum (Pflicht).</param>
    /// <param name="wertungssystem">Zu verwendendes Punktesystem.</param>
    Turnier TurnierErstellen(string anlass, DateOnly datum, Wertungssystem wertungssystem);

    /// <summary>Lädt das aktuell gespeicherte Turnier.</summary>
    Turnier TurnierLaden();

    /// <summary>Speichert das übergebene Turnier persistent.</summary>
    /// <param name="turnier">Das zu speichernde Turnierobjekt.</param>
    void TurnierSpeichern(Turnier turnier);

    /// <summary>
    /// Wechselt den Turnierstatus zum nächsten Schritt im Ablauf.
    /// Z. B. von <c>InVorbereitung</c> zu <c>Gruppenphase</c>.
    /// </summary>
    /// <param name="turnier">Das zu aktualisierende Turnierobjekt.</param>
    void StatusWechseln(Turnier turnier);

    /// <summary>Fügt dem Turnier eine neue Mannschaft hinzu.</summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    /// <param name="teamName">Vollständiger Mannschaftsname.</param>
    /// <param name="kurzname">Optionale Kurzbezeichnung.</param>
    Team TeamHinzufuegen(Turnier turnier, string teamName, string? kurzname = null);

    /// <summary>Markiert ein Team als zurückgezogen und setzt alle offenen Spiele auf abgesetzt.</summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    /// <param name="teamId">Id des betroffenen Teams.</param>
    void TeamZurueckziehen(Turnier turnier, Guid teamId);
}
