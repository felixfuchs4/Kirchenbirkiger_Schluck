/*************************************************************
 * Datei:        JsonKonfiguration.cs
 * Zweck:        Zentrale JSON-Serialisierungsoptionen für das Projekt
 * Bereich:      Datenhaltung – Serialisierung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Text.Json;
using System.Text.Json.Serialization;

namespace KirchenbirkigerSchluck.Data.Serialization;

/// <summary>
/// Stellt vorkonfigurierte <see cref="JsonSerializerOptions"/> bereit,
/// die im gesamten Projekt einheitlich verwendet werden.
/// </summary>
public static class JsonKonfiguration
{
    /// <summary>
    /// Standardoptionen: camelCase-Eigenschaftsnamen, Enums als Strings, eingerücktes JSON.
    /// </summary>
    public static readonly JsonSerializerOptions Standard = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}
