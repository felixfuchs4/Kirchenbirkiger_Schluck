/*************************************************************
 * Datei:        Spieler.cs
 * Zweck:        Domänenmodell für einen einzelnen Turnierspieler
 * Bereich:      Domänenmodell – Mannschaftsverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert einen einzelnen Spieler innerhalb einer Mannschaft.
/// </summary>
/// <remarks>
/// Verantwortung: Datenhaltung für Spieleridentifikation und -namen.
/// Einsatzkontext: Spieler werden in <c>Team.Spieler</c> verwaltet und
/// in Einzelduellen über ihre Id referenziert.
/// Besonderheiten: Namen können auch nach Turnierbeginn geändert werden;
/// Änderungen werden im Änderungsprotokoll festgehalten.
/// </remarks>
public class Spieler
{
    /// <summary>Eindeutiger Bezeichner des Spielers.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Vollständiger Name des Spielers, wie er auf der Anzeige erscheint.</summary>
    public string Name { get; set; } = string.Empty;
}
