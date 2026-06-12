/*************************************************************
 * Datei:        SchemaMigration.cs
 * Zweck:        Stub für zukünftige Schema-Migrationslogik
 * Bereich:      Datenhaltung – Migration
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Data.Migration;

/// <summary>
/// Verantwortlich für die Migration von Turnierdaten zwischen Schemaversionen.
/// </summary>
/// <remarks>
/// In V1 existiert nur Schemaversion <c>kirchenbirkiger-schluck/v1</c>;
/// Migrationslogik ist hier als Erweiterungspunkt vorbereitet.
/// </remarks>
public class SchemaMigration
{
    /// <summary>Aktuelle Schemaversion der Anwendung.</summary>
    public const string AktuelleVersion = "kirchenbirkiger-schluck/v1";

    /// <summary>
    /// Prüft, ob die übergebene Schemaversion migriert werden muss.
    /// </summary>
    /// <param name="schemaVersion">Schemaversion aus der geladenen JSON-Datei.</param>
    /// <returns>True, wenn eine Migration notwendig ist.</returns>
    public bool MigrationErforderlich(string schemaVersion)
        => schemaVersion != AktuelleVersion;

    /// <summary>
    /// Führt alle notwendigen Migrationsschritte für die gegebene Version durch.
    /// </summary>
    /// <param name="schemaVersion">Quellversion der geladenen Datei.</param>
    /// <param name="jsonRaw">Roher JSON-Inhalt der Datei.</param>
    /// <returns>Migrierter JSON-Inhalt in der aktuellen Schemaversion.</returns>
    public string Migrieren(string schemaVersion, string jsonRaw)
        => throw new NotImplementedException();
}
