/*************************************************************
 * Datei:        TurnierRepository.cs
 * Zweck:        Persistenz des Turnier-Wurzelobjekts als JSON-Datei
 * Bereich:      Datenhaltung – Repository
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Text.Json;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Serialization;

namespace KirchenbirkigerSchluck.Data.Repositories;

/// <summary>
/// Liest und schreibt das <see cref="Turnier"/>-Objekt aus/in eine JSON-Datei.
/// </summary>
public class TurnierRepository
{
    private readonly string _dateipfad;

    /// <summary>
    /// Initialisiert das Repository mit dem Pfad zur Turnier-JSON-Datei.
    /// </summary>
    /// <param name="dateipfad">Absoluter oder relativer Pfad zur <c>turnier.json</c>.</param>
    public TurnierRepository(string dateipfad)
    {
        _dateipfad = dateipfad;
    }

    /// <summary>
    /// Lädt das Turnier aus der JSON-Datei.
    /// </summary>
    /// <returns>Das deserialisierte <see cref="Turnier"/>-Objekt.</returns>
    /// <exception cref="FileNotFoundException">Wenn keine Turnierdatei gefunden wurde.</exception>
    /// <exception cref="JsonException">Wenn die Datei kein gültiges JSON enthält.</exception>
    public Turnier Laden()
    {
        if (!File.Exists(_dateipfad))
            throw new FileNotFoundException(
                $"Keine Turnierdatei unter '{_dateipfad}' gefunden.", _dateipfad);

        var json = File.ReadAllText(_dateipfad);

        return JsonSerializer.Deserialize<Turnier>(json, JsonKonfiguration.Standard)
            ?? throw new JsonException("Die Turnierdatei enthält kein gültiges Turnierobjekt.");
    }

    /// <summary>
    /// Schreibt das Turnier vollständig in die JSON-Datei.
    /// Verwendet atomares Schreiben (temp-Datei + Umbenennen), um Datenverlust zu vermeiden.
    /// </summary>
    /// <param name="turnier">Das zu speichernde Turnierobjekt.</param>
    public void Speichern(Turnier turnier)
    {
        var verzeichnis = Path.GetDirectoryName(_dateipfad);
        if (!string.IsNullOrEmpty(verzeichnis))
            Directory.CreateDirectory(verzeichnis);

        var json = JsonSerializer.Serialize(turnier, JsonKonfiguration.Standard);

        // Atomares Schreiben: erst in .tmp, dann umbenennen
        var tempPfad = _dateipfad + ".tmp";
        File.WriteAllText(tempPfad, json);
        File.Move(tempPfad, _dateipfad, overwrite: true);
    }

    /// <summary>Gibt an, ob bereits eine Turnierdatei am konfigurierten Pfad existiert.</summary>
    public bool ExistiertDatei() => File.Exists(_dateipfad);
}
