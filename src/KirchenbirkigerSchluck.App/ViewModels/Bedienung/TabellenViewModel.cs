/*************************************************************
 * Datei:        TabellenViewModel.cs
 * Zweck:        ViewModel für die Gruppentabellen-Übersicht im Verwaltungsbildschirm
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>Eine Gruppentabelle mit Namen und Tabellenzeilen.</summary>
public sealed class GruppenTabelleAnsicht
{
    /// <summary>Name der Gruppe.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Tabellenzeilen (Platz 1 zuerst).</summary>
    public ObservableCollection<GruppenTabellenEintragAnzeigeModel> Zeilen { get; } = [];
}

/// <summary>
/// Zeigt die aktuellen Gruppentabellen (Rangliste je Gruppe) im Verwaltungsbildschirm.
/// </summary>
public partial class TabellenViewModel : ObservableObject
{
    private readonly IWertungsService _wertungsService;
    private readonly TurnierZustandService _turnierZustand;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public TabellenViewModel(IWertungsService wertungsService, TurnierZustandService turnierZustandService)
    {
        _wertungsService = wertungsService;
        _turnierZustand = turnierZustandService;

        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();
        Aktualisieren();
    }

    /// <summary>Alle Gruppentabellen des Turniers.</summary>
    public ObservableCollection<GruppenTabelleAnsicht> Gruppen { get; } = [];

    /// <summary>Gibt an, ob Tabellen vorhanden sind.</summary>
    [ObservableProperty]
    private bool _hatTabellen;

    private void Aktualisieren()
    {
        Gruppen.Clear();
        var turnier = _turnierZustand.AktuellesTurnier;

        if (turnier is null || turnier.Gruppen.Count == 0)
        {
            HatTabellen = false;
            return;
        }

        foreach (var gruppe in turnier.Gruppen)
        {
            var ansicht = new GruppenTabelleAnsicht { Name = gruppe.Name };

            foreach (var eintrag in _wertungsService.GruppenRanglisteBerechnen(gruppe, turnier.Wertungssystem))
            {
                var diff = eintrag.DuellpunkteGewonnen - eintrag.DuellpunkteVerloren;
                ansicht.Zeilen.Add(new GruppenTabellenEintragAnzeigeModel
                {
                    Platz = eintrag.Position,
                    TeamName = TeamName(turnier, eintrag.TeamId),
                    LogoPfad = TeamLogo(turnier, eintrag.TeamId),
                    Spiele = eintrag.Spiele,
                    Siege = eintrag.Siege,
                    DuelleGewonnen = eintrag.DuellpunkteGewonnen,
                    DuelleVerloren = eintrag.DuellpunkteVerloren,
                    DuellDifferenz = diff > 0 ? $"+{diff}" : diff.ToString(),
                    Punkte = eintrag.Tabellenpunkte,
                    TiebreakKuerzel = TiebreakKuerzel(eintrag)
                });
            }

            Gruppen.Add(ansicht);
        }

        HatTabellen = Gruppen.Count > 0;
    }

    /// <summary>Liefert das Tiebreak-Kürzel („DV", „S" oder leer) für einen Tabelleneintrag.</summary>
    private static string TiebreakKuerzel(GruppenTabellenEintrag eintrag) =>
        eintrag.DurchDirektenVergleich ? "DV" : eintrag.DurchStechen ? "S" : string.Empty;

    private static string TeamName(Turnier turnier, Guid teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "?";

    private static string? TeamLogo(Turnier turnier, Guid teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.LogoPfad;
}
