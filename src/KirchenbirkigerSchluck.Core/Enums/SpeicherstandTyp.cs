/*************************************************************
 * Datei:        SpeicherstandTyp.cs
 * Zweck:        Art eines ladbaren Speicherstands (benannt oder automatisches Backup)
 * Bereich:      Datenhaltung – Speicherstände
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Enums;

/// <summary>Unterscheidet die Herkunft eines ladbaren Speicherstands.</summary>
public enum SpeicherstandTyp
{
    /// <summary>Manuell mit Titel (und optionaler Beschreibung) gespeicherter Stand.</summary>
    Benannt,

    /// <summary>Automatisch erzeugtes Backup (z. B. nach einem Spielabschluss).</summary>
    Backup
}
