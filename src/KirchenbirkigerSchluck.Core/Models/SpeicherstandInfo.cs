/*************************************************************
 * Datei:        SpeicherstandInfo.cs
 * Zweck:        Leichtgewichtige Metadaten eines ladbaren Speicherstands (für die Auswahlliste)
 * Bereich:      Datenhaltung – Speicherstände
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Beschreibt einen ladbaren Speicherstand ohne den vollständigen Turnierinhalt – für die Anzeige
/// in der Auswahlliste und als Referenz zum Laden bzw. Löschen.
/// </summary>
/// <param name="Pfad">Vollständiger Dateipfad des Speicherstands.</param>
/// <param name="Typ">Art des Speicherstands (benannt oder automatisches Backup).</param>
/// <param name="Titel">Anzeigetitel (bei Backups aus dem Turnier-Anlass abgeleitet).</param>
/// <param name="Beschreibung">Optionale Kurzbeschreibung (nur bei benannten Speicherständen).</param>
/// <param name="Anlass">Anlass/Name des enthaltenen Turniers.</param>
/// <param name="Status">Fortschrittsstand des enthaltenen Turniers.</param>
/// <param name="GespeichertAm">Zeitpunkt der Speicherung.</param>
public sealed record SpeicherstandInfo(
    string Pfad,
    SpeicherstandTyp Typ,
    string Titel,
    string? Beschreibung,
    string Anlass,
    TurnierStatus Status,
    DateTime GespeichertAm);
