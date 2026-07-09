/*************************************************************
 * Datei:        ISpeicherstandService.cs
 * Zweck:        Schnittstelle für benannte Speicherstände und das Laden vorhandener Stände/Backups
 * Bereich:      Datenhaltung – Speicherstände
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Verwaltet benannte Speicherstände (mit Titel und optionaler Beschreibung) und liefert die Liste
/// aller ladbaren Stände – einschließlich der automatischen Backups.
/// </summary>
public interface ISpeicherstandService
{
    /// <summary>
    /// Speichert den vollständigen Turnierstand unter einem Titel (überschreibt einen vorhandenen
    /// Stand gleichen Titels).
    /// </summary>
    /// <param name="turnier">Der vollständige, zu sichernde Turnierstand.</param>
    /// <param name="titel">Titel des Speicherstands (Pflicht).</param>
    /// <param name="beschreibung">Optionale Kurzbeschreibung.</param>
    /// <returns>Der Dateipfad des erzeugten Speicherstands.</returns>
    string SpeichernUnter(Turnier turnier, string titel, string? beschreibung);

    /// <summary>
    /// Liefert alle ladbaren Speicherstände (benannte Stände und automatische Backups),
    /// absteigend nach Speicherzeitpunkt sortiert.
    /// </summary>
    IReadOnlyList<SpeicherstandInfo> Alle();

    /// <summary>Lädt den vollständigen Turnierstand des angegebenen Speicherstands.</summary>
    /// <param name="info">Der zu ladende Speicherstand.</param>
    Turnier Laden(SpeicherstandInfo info);

    /// <summary>Löscht die Datei des angegebenen Speicherstands.</summary>
    /// <param name="info">Der zu löschende Speicherstand.</param>
    void Loeschen(SpeicherstandInfo info);
}
