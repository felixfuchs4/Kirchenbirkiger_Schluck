/*************************************************************
 * Datei:        BackupManager.cs
 * Zweck:        Verwaltung automatischer JSON-Backups des Turnierstands
 * Bereich:      Datenhaltung – Backup
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Text.Json;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Serialization;

namespace KirchenbirkigerSchluck.Data.Backup;

/// <summary>
/// Erzeugt und verwaltet Backup-Dateien des Turnierstands im Backup-Verzeichnis.
/// </summary>
/// <remarks>
/// Backup-Dateiname:
/// <c>YYYY-MM-DD_HH-MM-SS_Anlass[_Team1-Team2].json</c>
/// Alle erzeugten Backups werden dauerhaft aufbewahrt (kein automatisches Löschen).
/// </remarks>
public class BackupManager
{
    private readonly string _backupVerzeichnis;

    /// <summary>
    /// Initialisiert den BackupManager mit dem Pfad zum Backup-Verzeichnis.
    /// </summary>
    /// <param name="backupVerzeichnis">Absoluter Pfad zum Verzeichnis für Backup-Dateien.</param>
    public BackupManager(string backupVerzeichnis)
    {
        _backupVerzeichnis = backupVerzeichnis;
    }

    /// <summary>
    /// Erstellt eine Backup-Datei des aktuellen Turnierstands.
    /// </summary>
    /// <param name="turnier">Der zu sichernde Turnierstand.</param>
    /// <param name="aktuellesSpiel">
    /// Optionales aktuelles Spiel; wenn angegeben, werden die Teamnamen im Dateinamen ergänzt.
    /// </param>
    /// <returns>Vollständiger Pfad zur erzeugten Backup-Datei.</returns>
    public string BackupErstellen(Turnier turnier, Spiel? aktuellesSpiel = null)
    {
        // Teamnamen für den Dateinamen aus dem Turnier nachschlagen
        string? team1Name = null, team2Name = null;
        if (aktuellesSpiel is not null)
        {
            team1Name = turnier.Teams.FirstOrDefault(t => t.Id == aktuellesSpiel.Team1Id)?.Name;
            team2Name = turnier.Teams.FirstOrDefault(t => t.Id == aktuellesSpiel.Team2Id)?.Name;
        }

        var dateiname = DateinameGenerieren(turnier, DateTime.Now, team1Name, team2Name);
        Directory.CreateDirectory(_backupVerzeichnis);

        var vollpfad = Path.Combine(_backupVerzeichnis, dateiname);
        var json = JsonSerializer.Serialize(turnier, JsonKonfiguration.Standard);
        File.WriteAllText(vollpfad, json);

        return vollpfad;
    }

    /// <summary>
    /// Generiert den Dateinamen für ein Backup nach der definierten Konvention.
    /// </summary>
    /// <param name="turnier">Turnier (für Anlass und Datum).</param>
    /// <param name="zeitpunkt">Zeitstempel des Backups.</param>
    /// <param name="team1Name">Optionaler Name von Team 1 des aktuellen Spiels.</param>
    /// <param name="team2Name">Optionaler Name von Team 2 des aktuellen Spiels.</param>
    /// <returns>Dateiname ohne Verzeichnispfad, z. B. <c>2026-06-12_14-30-00_Schluck_Bierbrauer-Hopfenpfluecker.json</c>.</returns>
    public string DateinameGenerieren(Turnier turnier, DateTime zeitpunkt, string? team1Name = null, string? team2Name = null)
    {
        var zeitstempel = zeitpunkt.ToString("yyyy-MM-dd_HH-mm-ss");
        var anlass = DateinameSanitieren(turnier.Anlass, maxLaenge: 40);

        var name = $"{zeitstempel}_{anlass}";

        if (team1Name is not null && team2Name is not null)
        {
            var t1 = DateinameSanitieren(team1Name, maxLaenge: 20);
            var t2 = DateinameSanitieren(team2Name, maxLaenge: 20);
            name += $"_{t1}-{t2}";
        }

        return name + ".json";
    }

    /// <summary>
    /// Gibt alle vorhandenen Backup-Dateien im Backup-Verzeichnis sortiert nach Erstellzeit zurück.
    /// </summary>
    public IReadOnlyList<string> AlleDateien()
    {
        if (!Directory.Exists(_backupVerzeichnis))
            return [];

        return Directory.GetFiles(_backupVerzeichnis, "*.json")
            .OrderBy(File.GetCreationTimeUtc)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Bereinigt einen String für die Verwendung als Dateiname:
    /// Ersetzt ungültige Zeichen und Leerzeichen durch '_', kürzt auf <paramref name="maxLaenge"/>.
    /// </summary>
    /// <param name="input">Eingabestring.</param>
    /// <param name="maxLaenge">Maximale Zeichenanzahl des Ergebnissttrings.</param>
    private static string DateinameSanitieren(string input, int maxLaenge = 40)
    {
        var ungueltig = Path.GetInvalidFileNameChars().ToHashSet();

        var sanitiert = new string(input.Select(c =>
            ungueltig.Contains(c) || c == ' ' ? '_' : c).ToArray());

        return sanitiert.Length > maxLaenge
            ? sanitiert[..maxLaenge]
            : sanitiert;
    }
}
