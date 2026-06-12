/*************************************************************
 * Datei:        IWertungsService.cs
 * Zweck:        Schnittstelle für Tabellenberechnung und Ranglisten
 * Bereich:      Anwendungslogik – Wertung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Tabelleneintrag für ein Team in einer Gruppe, berechnet aus den Spielergebnissen.
/// </summary>
public class GruppenTabellenEintrag
{
    /// <summary>Id des Teams.</summary>
    public Guid TeamId { get; set; }

    /// <summary>Tabellenposition (1-basiert).</summary>
    public int Position { get; set; }

    /// <summary>Anzahl gewerteter Spiele.</summary>
    public int Spiele { get; set; }

    /// <summary>Anzahl gewonnener Spiele.</summary>
    public int Siege { get; set; }

    /// <summary>Anzahl unentschiedener Spiele (Eishockey: 0; Einfach: 0).</summary>
    public int Unentschieden { get; set; }

    /// <summary>Anzahl verlorener Spiele.</summary>
    public int Niederlagen { get; set; }

    /// <summary>Gesamtanzahl erzielte Tabellenpunkte.</summary>
    public int Tabellenpunkte { get; set; }

    /// <summary>Gesamtanzahl gewonnener Einzelduelle (Duellpunkte).</summary>
    public int DuellpunkteGewonnen { get; set; }

    /// <summary>Gesamtanzahl verlorener Einzelduelle (Duellpunkte).</summary>
    public int DuellpunkteVerloren { get; set; }

    /// <summary>
    /// Gibt an, ob für diese Position noch ein Stechen gegen ein anderes Team
    /// gespielt werden muss, weil Tabellenpunkte und Direkter Vergleich keine
    /// eindeutige Reihenfolge ergeben haben.
    /// </summary>
    public bool StehenErforderlich { get; set; }
}

/// <summary>
/// Definiert Operationen zur Berechnung von Tabellenpunkten und Gruppenranglisten.
/// </summary>
public interface IWertungsService
{
    /// <summary>
    /// Berechnet die Tabellenpunkte für beide Teams nach Abschluss einer Partie
    /// gemäß dem gewählten Wertungssystem.
    /// </summary>
    /// <param name="spiel">Die abgeschlossene Partie.</param>
    /// <param name="wertungssystem">Das aktive Wertungssystem des Turniers.</param>
    /// <returns>Tuple mit (PunkteTeam1, PunkteTeam2).</returns>
    (int PunkteTeam1, int PunkteTeam2) TabellenPunkteBerechnen(Spiel spiel, Wertungssystem wertungssystem);

    /// <summary>
    /// Berechnet die sortierte Gruppenrangliste aus allen Spielen der Gruppe.
    /// </summary>
    /// <param name="gruppe">Die Gruppe mit abgeschlossenen Spielen.</param>
    /// <param name="wertungssystem">Das aktive Wertungssystem des Turniers.</param>
    /// <returns>Sortierte Liste der Tabelleneinträge (Platz 1 zuerst).</returns>
    IReadOnlyList<GruppenTabellenEintrag> GruppenRanglisteBerechnen(Gruppe gruppe, Wertungssystem wertungssystem);
}
