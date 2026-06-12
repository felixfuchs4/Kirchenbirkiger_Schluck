/*************************************************************
 * Datei:        SpielStatus.cs
 * Zweck:        Aufzählung der möglichen Spielstatus-Werte
 * Bereich:      Domänenmodell – Spielverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>
/// Beschreibt den aktuellen Zustand einer einzelnen Partie.
/// </summary>
/// <remarks>
/// Verantwortung: Steuerung des Spielverlaufs und der zulässigen Folgeaktionen.
/// Einsatzkontext: Wird in <c>Spiel</c> gesetzt; steuert Anzeige und Bedienung.
/// Besonderheiten: <c>Korrigiert</c> kennzeichnet nachträgliche Änderungen am
/// Ergebnis und ist kein ausschließender Zustand (wird zusätzlich gesetzt).
/// </remarks>
public enum SpielStatus
{
    /// <summary>Spiel ist im Spielplan vorgesehen, aber noch nicht vorbereitet.</summary>
    Geplant,

    /// <summary>Spiel wurde als nächstes ausgewählt und wird bald gestartet.</summary>
    Vorbereitet,

    /// <summary>Partie läuft aktuell; Einzelduelle werden erfasst.</summary>
    Laeuft,

    /// <summary>Partie ist abgeschlossen; Ergebnis liegt vor.</summary>
    Abgeschlossen,

    /// <summary>Spiel wird nicht ausgetragen, z. B. wegen Teamrückzug; keine Punkte.</summary>
    Abgesetzt,

    /// <summary>Spiel wurde im Ablaufplan nach hinten verschoben.</summary>
    Verschoben,

    /// <summary>Ergebnis wurde nachträglich korrigiert.</summary>
    Korrigiert
}
