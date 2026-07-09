/*************************************************************
 * Datei:        Turnier.cs
 * Zweck:        Domänenmodell für das Turnier-Wurzelobjekt
 * Bereich:      Domänenmodell – Turnierverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Wurzelobjekt des Domänenmodells; enthält alle Daten eines Turniers.
/// </summary>
/// <remarks>
/// Verantwortung: Aggregation sämtlicher Turnierdaten als einziger
/// Persistenz-Einstiegspunkt (gespeichert als turnier.json).
/// Einsatzkontext: Wird beim Start der Anwendung geladen und
/// nach jeder Änderung vollständig zurückgeschrieben.
/// Besonderheiten: Finalrundenspiele liegen direkt in <c>Finalrundenspiele</c>,
/// nicht in einer Gruppe. Das Änderungsprotokoll ist Append-Only.
/// </remarks>
public class Turnier
{
    /// <summary>Eindeutiger Bezeichner des Turniers.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Schemaversion für Migrationszwecke, z. B. „kirchenbirkiger-schluck/v1".</summary>
    public string SchemaVersion { get; set; } = "kirchenbirkiger-schluck/v1";

    /// <summary>Anlass bzw. Turniername, z. B. „Kirchenbirkiger Schluck 2026".</summary>
    public string Anlass { get; set; } = string.Empty;

    /// <summary>Datum des Turniers (nur Datum, keine Uhrzeit).</summary>
    public DateOnly Datum { get; set; }

    /// <summary>Optionaler Veranstaltungsort.</summary>
    public string? Ort { get; set; }

    /// <summary>Gewähltes Punktesystem für die Tabellenwertung.</summary>
    public Wertungssystem Wertungssystem { get; set; } = Wertungssystem.Eishockey;

    /// <summary>Spielmodus der Finalrunde.</summary>
    public FinalrundenModus FinalrundenModus { get; set; } = FinalrundenModus.KoBaumEin;

    /// <summary>Wertungsart für den treffsichersten Spieler (Torschützenkönig).</summary>
    public TorschuetzenWertung TorschuetzenWertung { get; set; } = TorschuetzenWertung.Absolut;

    /// <summary>
    /// Manuell bestimmter Sieger eines Stechens um Platz 1 der Torschützen-Rangliste, falls
    /// mehrere Spieler dort exakt gleichauf liegen. Null, solange kein Stechen nötig war,
    /// noch nicht entschieden wurde, oder sich die Gleichstandsgruppe seither geändert hat.
    /// </summary>
    public Guid? TorschuetzenStechenSiegerId { get; set; }

    /// <summary>Aktueller Fortschrittsstand des Turniers.</summary>
    public TurnierStatus Status { get; set; } = TurnierStatus.InVorbereitung;

    /// <summary>Alle teilnehmenden Mannschaften des Turniers.</summary>
    public List<Team> Teams { get; set; } = [];

    /// <summary>Alle Gruppen der Gruppenphase.</summary>
    public List<Gruppe> Gruppen { get; set; } = [];

    /// <summary>
    /// Spiele der Finalrunde (Halbfinale, Spiel um Platz 3, Finale usw.).
    /// Leer solange die Finalrunde noch nicht gestartet ist.
    /// </summary>
    public List<Spiel> Finalrundenspiele { get; set; } = [];

    /// <summary>Vollständiges Änderungsprotokoll aller manuellen Korrekturen.</summary>
    public List<Aenderungseintrag> Aenderungsprotokoll { get; set; } = [];
}
