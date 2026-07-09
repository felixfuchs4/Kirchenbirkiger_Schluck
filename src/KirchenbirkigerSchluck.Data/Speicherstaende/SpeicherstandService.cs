/*************************************************************
 * Datei:        SpeicherstandService.cs
 * Zweck:        Persistenz benannter Speicherstände und Auflistung ladbarer Stände inkl. Backups
 * Bereich:      Datenhaltung – Speicherstände
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Text.Json;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Serialization;

namespace KirchenbirkigerSchluck.Data.Speicherstaende;

/// <summary>
/// Speichert benannte Speicherstände als JSON (Titel, Beschreibung + vollständiger Turnierstand) in
/// einem eigenen Verzeichnis und listet zum Laden zusätzlich die automatischen Backups auf.
/// </summary>
public class SpeicherstandService : ISpeicherstandService
{
    private readonly string _speicherVerzeichnis;
    private readonly string _backupVerzeichnis;

    /// <summary>
    /// Initialisiert den Dienst mit den Verzeichnissen für benannte Speicherstände und Backups.
    /// </summary>
    /// <param name="speicherVerzeichnis">Verzeichnis der benannten Speicherstände.</param>
    /// <param name="backupVerzeichnis">Verzeichnis der automatischen Backups.</param>
    public SpeicherstandService(string speicherVerzeichnis, string backupVerzeichnis)
    {
        _speicherVerzeichnis = speicherVerzeichnis;
        _backupVerzeichnis   = backupVerzeichnis;
    }

    /// <inheritdoc/>
    public string SpeichernUnter(Turnier turnier, string titel, string? beschreibung)
    {
        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Ein Speicherstand benötigt einen Titel.", nameof(titel));

        Directory.CreateDirectory(_speicherVerzeichnis);

        var stand = new Speicherstand
        {
            Titel         = titel.Trim(),
            Beschreibung  = string.IsNullOrWhiteSpace(beschreibung) ? null : beschreibung.Trim(),
            GespeichertAm = DateTime.Now,
            Turnier       = turnier
        };

        var pfad = Path.Combine(_speicherVerzeichnis, DateinameSanitieren(titel) + ".json");
        var json = JsonSerializer.Serialize(stand, JsonKonfiguration.Standard);

        // Atomares Schreiben: erst .tmp, dann umbenennen
        var tempPfad = pfad + ".tmp";
        File.WriteAllText(tempPfad, json);
        File.Move(tempPfad, pfad, overwrite: true);

        return pfad;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SpeicherstandInfo> Alle()
    {
        var liste = new List<SpeicherstandInfo>();

        if (Directory.Exists(_speicherVerzeichnis))
            foreach (var pfad in Directory.GetFiles(_speicherVerzeichnis, "*.json"))
                if (TryBenanntLesen(pfad, out var info))
                    liste.Add(info);

        if (Directory.Exists(_backupVerzeichnis))
            foreach (var pfad in Directory.GetFiles(_backupVerzeichnis, "*.json"))
                if (TryBackupLesen(pfad, out var info))
                    liste.Add(info);

        return liste.OrderByDescending(i => i.GespeichertAm).ToList();
    }

    /// <inheritdoc/>
    public Turnier Laden(SpeicherstandInfo info)
    {
        var json = File.ReadAllText(info.Pfad);

        if (info.Typ == SpeicherstandTyp.Benannt)
        {
            var stand = JsonSerializer.Deserialize<Speicherstand>(json, JsonKonfiguration.Standard)
                ?? throw new JsonException("Der Speicherstand enthält kein gültiges Turnierobjekt.");
            return stand.Turnier;
        }

        return JsonSerializer.Deserialize<Turnier>(json, JsonKonfiguration.Standard)
            ?? throw new JsonException("Das Backup enthält kein gültiges Turnierobjekt.");
    }

    /// <inheritdoc/>
    public void Loeschen(SpeicherstandInfo info)
    {
        if (File.Exists(info.Pfad))
            File.Delete(info.Pfad);
    }

    // ──── Metadaten lesen (leichtgewichtig via JsonDocument) ─────────────────

    /// <summary>Liest die Metadaten eines benannten Speicherstands ohne vollständige Deserialisierung.</summary>
    private static bool TryBenanntLesen(string pfad, out SpeicherstandInfo info)
    {
        info = null!;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(pfad));
            var root = doc.RootElement;

            var titel = root.TryGetProperty("titel", out var t) ? t.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(titel))
                titel = Path.GetFileNameWithoutExtension(pfad);

            var beschreibung = root.TryGetProperty("beschreibung", out var b) && b.ValueKind == JsonValueKind.String
                ? b.GetString()
                : null;

            var gespeichertAm = root.TryGetProperty("gespeichertAm", out var g) && g.TryGetDateTime(out var dt)
                ? dt
                : File.GetLastWriteTime(pfad);

            var anlass = "";
            var status = TurnierStatus.InVorbereitung;
            if (root.TryGetProperty("turnier", out var turnier) && turnier.ValueKind == JsonValueKind.Object)
            {
                anlass = turnier.TryGetProperty("anlass", out var a) ? a.GetString() ?? "" : "";
                status = StatusLesen(turnier);
            }

            info = new SpeicherstandInfo(pfad, SpeicherstandTyp.Benannt, titel, beschreibung, anlass, status, gespeichertAm);
            return true;
        }
        catch
        {
            return false; // beschädigte Datei überspringen
        }
    }

    /// <summary>Liest die Metadaten eines automatischen Backups (reines Turnier-JSON).</summary>
    private static bool TryBackupLesen(string pfad, out SpeicherstandInfo info)
    {
        info = null!;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(pfad));
            var root = doc.RootElement;

            var anlass = root.TryGetProperty("anlass", out var a) ? a.GetString() ?? "" : "";
            var status = StatusLesen(root);
            var titel  = string.IsNullOrWhiteSpace(anlass) ? Path.GetFileNameWithoutExtension(pfad) : anlass;

            info = new SpeicherstandInfo(
                pfad, SpeicherstandTyp.Backup, titel, Beschreibung: null,
                anlass, status, File.GetLastWriteTime(pfad));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Liest die Status-Enumeration aus einem Turnier-JSON-Element (camelCase-Wert).</summary>
    private static TurnierStatus StatusLesen(JsonElement turnierElement)
    {
        if (turnierElement.TryGetProperty("status", out var s) &&
            s.ValueKind == JsonValueKind.String &&
            Enum.TryParse<TurnierStatus>(s.GetString(), ignoreCase: true, out var status))
        {
            return status;
        }
        return TurnierStatus.InVorbereitung;
    }

    /// <summary>Ersetzt ungültige Dateinamenzeichen und Leerzeichen durch '_', kürzt auf 60 Zeichen.</summary>
    private static string DateinameSanitieren(string titel)
    {
        var ungueltig = Path.GetInvalidFileNameChars().ToHashSet();
        var sanitiert = new string(titel.Trim().Select(c => ungueltig.Contains(c) || c == ' ' ? '_' : c).ToArray());
        if (sanitiert.Length == 0) sanitiert = "Speicherstand";
        return sanitiert.Length > 60 ? sanitiert[..60] : sanitiert;
    }
}
