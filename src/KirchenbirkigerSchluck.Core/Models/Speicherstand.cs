/*************************************************************
 * Datei:        Speicherstand.cs
 * Zweck:        Benannter Speicherstand (Metadaten + vollständiger Turnierstand)
 * Bereich:      Datenhaltung – Speicherstände
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Umhüllt einen vollständigen Turnierstand für die benannte Speicherung. Enthält neben dem
/// kompletten <see cref="Turnier"/> (Teams, Spieler, Gruppen, Spielplan, Ergebnisse …) einen vom
/// Benutzer vergebenen Titel sowie eine optionale Kurzbeschreibung zum Wiederfinden.
/// </summary>
public class Speicherstand
{
    /// <summary>Schemaversion des Speicherstand-Formats (für spätere Migrationen).</summary>
    public string SchemaVersion { get; set; } = "kirchenbirkiger-schluck/speicherstand-v1";

    /// <summary>Vom Benutzer vergebener Titel des Speicherstands.</summary>
    public string Titel { get; set; } = string.Empty;

    /// <summary>Optionale Kurzbeschreibung zur besseren Auffindbarkeit.</summary>
    public string? Beschreibung { get; set; }

    /// <summary>Zeitpunkt der Speicherung (lokale Zeit).</summary>
    public DateTime GespeichertAm { get; set; } = DateTime.Now;

    /// <summary>Der vollständige Turnierstand zum Zeitpunkt der Speicherung.</summary>
    public Turnier Turnier { get; set; } = new();
}
