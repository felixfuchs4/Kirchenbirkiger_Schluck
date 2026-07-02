/*************************************************************
 * Datei:        LogoService.cs
 * Zweck:        Speichert und verwaltet Team-Logodateien im Logo-Verzeichnis
 * Bereich:      Präsentation – Dienste
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.IO;

namespace KirchenbirkigerSchluck.App.Services;

/// <summary>
/// Kopiert hochgeladene Team-Logos in ein lokales Verzeichnis und liefert relative Pfade,
/// die im Turnier gespeichert werden. So bleiben Logos zusammen mit dem Spielstand portabel.
/// </summary>
public class LogoService
{
    private readonly string _logoVerzeichnis;

    /// <summary>Initialisiert den Service mit dem Zielverzeichnis für Logos.</summary>
    /// <param name="logoVerzeichnis">Relativer oder absoluter Pfad zum Logo-Verzeichnis.</param>
    public LogoService(string logoVerzeichnis)
    {
        _logoVerzeichnis = logoVerzeichnis;
    }

    /// <summary>
    /// Kopiert die Quelldatei als Logo für das angegebene Team ins Logo-Verzeichnis.
    /// </summary>
    /// <param name="teamId">Id des Teams (bestimmt den Dateinamen).</param>
    /// <param name="quellPfad">Pfad zur ausgewählten Bilddatei.</param>
    /// <returns>Relativer Pfad zur gespeicherten Logodatei (z. B. „logos/&lt;id&gt;.png").</returns>
    public string LogoSpeichern(Guid teamId, string quellPfad)
    {
        Directory.CreateDirectory(_logoVerzeichnis);

        var endung = Path.GetExtension(quellPfad);
        if (string.IsNullOrWhiteSpace(endung))
            endung = ".png";

        var zielDateiname = $"{teamId}{endung}";
        var zielPfad = Path.Combine(_logoVerzeichnis, zielDateiname);

        // Eventuell vorhandene ältere Logos desselben Teams (andere Endung) entfernen
        AlteLogosEntfernen(teamId, endung);

        File.Copy(quellPfad, zielPfad, overwrite: true);

        return Path.Combine(_logoVerzeichnis, zielDateiname).Replace('\\', '/');
    }

    /// <summary>Entfernt das Logo eines Teams, falls vorhanden.</summary>
    public void LogoEntfernen(Guid teamId)
    {
        AlteLogosEntfernen(teamId, string.Empty);
    }

    private void AlteLogosEntfernen(Guid teamId, string ausnahmeEndung)
    {
        if (!Directory.Exists(_logoVerzeichnis)) return;

        foreach (var datei in Directory.GetFiles(_logoVerzeichnis, $"{teamId}.*"))
        {
            if (!string.IsNullOrEmpty(ausnahmeEndung) &&
                Path.GetExtension(datei).Equals(ausnahmeEndung, StringComparison.OrdinalIgnoreCase))
                continue;

            try { File.Delete(datei); } catch { /* nicht kritisch */ }
        }
    }
}
