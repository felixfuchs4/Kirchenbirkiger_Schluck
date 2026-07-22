/*************************************************************
 * Datei:        StatistikViewModel.cs
 * Zweck:        ViewModel für die Spieler- und Team-Statistik-Ansicht im Verwaltungsbildschirm
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>Die beiden Darstellungsmodi der Statistik-Ansicht.</summary>
public enum StatistikAnsichtModus
{
    /// <summary>Flache Rangliste aller Spieler.</summary>
    Tabellenansicht,

    /// <summary>Teams mit aufklappbarer Spielerliste, sortiert nach Team-Trefferquote.</summary>
    TeamAnsicht
}

/// <summary>Eine Zeile der Spieler-Statistik (Tabellenansicht und aufgeklappte Team-Ansicht).</summary>
public sealed class SpielerStatistikZeileModel
{
    /// <summary>Id des Spielers.</summary>
    public Guid SpielerId { get; init; }

    /// <summary>Id des zugehörigen Teams (zur Zuordnung in der Team-Ansicht).</summary>
    public Guid TeamId { get; init; }

    /// <summary>Platzierung in der Rangliste (nur in der Tabellenansicht relevant).</summary>
    public int Platz { get; init; }

    /// <summary>Name des Spielers.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Name des zugehörigen Teams.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad des Teams.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Anzahl abgeschlossener Einzelduelle.</summary>
    public int Spiele { get; init; }

    /// <summary>Anzahl gewonnener Einzelduelle.</summary>
    public int Siege { get; init; }

    /// <summary>Anzahl unentschiedener Einzelduelle.</summary>
    public int Unentschieden { get; init; }

    /// <summary>Anzahl verlorener Einzelduelle.</summary>
    public int Niederlagen { get; init; }

    /// <summary>Anzahl absolvierter Versuche.</summary>
    public int Versuche { get; init; }

    /// <summary>Anzahl erzielter Treffer.</summary>
    public int Treffer { get; init; }

    /// <summary>Trefferquote als Bruch, z. B. „8/12".</summary>
    public string QuoteAbsolutText { get; init; } = "0/0";

    /// <summary>Trefferquote in Prozent, z. B. „66,7%".</summary>
    public string QuoteProzentText { get; init; } = "0,0%";
}

/// <summary>Eine Team-Zeile der Team-Ansicht mit aufklappbarer Spielerliste.</summary>
public sealed partial class TeamStatistikZeileModel : ObservableObject
{
    /// <summary>Id des Teams.</summary>
    public required Guid TeamId { get; init; }

    /// <summary>Name des Teams.</summary>
    public required string TeamName { get; init; }

    /// <summary>Optionaler Logo-Pfad des Teams.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Platzierung in der Team-Rangliste nach Trefferquote.</summary>
    public required int Platz { get; init; }

    /// <summary>Anzahl absolvierter Versuche aller Spieler des Teams.</summary>
    public required int Versuche { get; init; }

    /// <summary>Anzahl erzielter Treffer aller Spieler des Teams.</summary>
    public required int Treffer { get; init; }

    /// <summary>Trefferquote als Bruch, z. B. „24/40".</summary>
    public required string QuoteAbsolutText { get; init; }

    /// <summary>Trefferquote in Prozent, z. B. „60,0%".</summary>
    public required string QuoteProzentText { get; init; }

    /// <summary>Gibt an, ob die Spielerliste dieses Teams aktuell aufgeklappt ist.</summary>
    [ObservableProperty]
    private bool _istAufgeklappt;

    /// <summary>Klappt die Spielerliste dieses Teams auf bzw. zu.</summary>
    [RelayCommand]
    private void Umschalten() => IstAufgeklappt = !IstAufgeklappt;

    /// <summary>Spieler dieses Teams (dieselben Zeilenobjekte wie in der Tabellenansicht).</summary>
    public ObservableCollection<SpielerStatistikZeileModel> Spieler { get; } = [];
}

/// <summary>
/// Zeigt die live aktualisierte Spieler- und Team-Statistik im Verwaltungsbildschirm: eine
/// Torschützen-Rangliste über alle Spieler sowie eine nach Trefferquote sortierte Team-Ansicht
/// mit aufklappbarer Spielerliste je Team, plus einer turnierweiten Gesamtstatistik im Kopfbereich.
/// </summary>
public partial class StatistikViewModel : ObservableObject
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly StatistikService _statistikService;
    private readonly TurnierZustandService _turnierZustand;

    /// <summary>Initialisiert das ViewModel und abonniert Turnieränderungs-Events.</summary>
    public StatistikViewModel(StatistikService statistikService, TurnierZustandService turnierZustandService)
    {
        _statistikService = statistikService;
        _turnierZustand = turnierZustandService;

        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();
        Aktualisieren();
    }

    /// <summary>Aktuell gewählter Darstellungsmodus.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstTabellenModus))]
    [NotifyPropertyChangedFor(nameof(IstTeamModus))]
    private StatistikAnsichtModus _modus = StatistikAnsichtModus.Tabellenansicht;

    /// <summary>Gibt an, ob die Tabellenansicht aktiv ist.</summary>
    public bool IstTabellenModus => Modus == StatistikAnsichtModus.Tabellenansicht;

    /// <summary>Gibt an, ob die Team-Ansicht aktiv ist.</summary>
    public bool IstTeamModus => Modus == StatistikAnsichtModus.TeamAnsicht;

    /// <summary>Wechselt in die Tabellenansicht.</summary>
    [RelayCommand]
    private void TabellenAnsichtWaehlen() => Modus = StatistikAnsichtModus.Tabellenansicht;

    /// <summary>Wechselt in die Team-Ansicht.</summary>
    [RelayCommand]
    private void TeamAnsichtWaehlen() => Modus = StatistikAnsichtModus.TeamAnsicht;

    /// <summary>Gesamtzahl der Versuche über das gesamte Turnier (Kopfbereich).</summary>
    [ObservableProperty]
    private int _gesamtVersuche;

    /// <summary>Gesamtzahl der Treffer über das gesamte Turnier (Kopfbereich).</summary>
    [ObservableProperty]
    private int _gesamtTreffer;

    /// <summary>Gesamt-Trefferquote als Bruch (Kopfbereich).</summary>
    [ObservableProperty]
    private string _gesamtQuoteAbsolutText = "0/0";

    /// <summary>Gesamt-Trefferquote in Prozent (Kopfbereich).</summary>
    [ObservableProperty]
    private string _gesamtQuoteProzentText = "0,0%";

    /// <summary>Alle Spieler des Turniers, sortiert nach dem Torschützen-Wertungskriterium (Tabellenansicht).</summary>
    public ObservableCollection<SpielerStatistikZeileModel> AlleSpieler { get; } = [];

    /// <summary>Alle Teams des Turniers, sortiert nach Trefferquote (Team-Ansicht).</summary>
    public ObservableCollection<TeamStatistikZeileModel> Teams { get; } = [];

    /// <summary>Gibt an, ob überhaupt Teams im Turnier angelegt sind (Empty-State).</summary>
    [ObservableProperty]
    private bool _hatDaten;

    private void Aktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;

        if (turnier is null || turnier.Teams.Count == 0)
        {
            AlleSpieler.Clear();
            Teams.Clear();
            HatDaten = false;
            GesamtVersuche = 0;
            GesamtTreffer = 0;
            GesamtQuoteAbsolutText = "0/0";
            GesamtQuoteProzentText = "0,0%";
            return;
        }

        var gesamt = _statistikService.GesamtStatistikErmitteln(turnier);
        GesamtVersuche = gesamt.Versuche;
        GesamtTreffer = gesamt.Treffer;
        GesamtQuoteAbsolutText = $"{gesamt.Treffer}/{gesamt.Versuche}";
        GesamtQuoteProzentText = ProzentText(gesamt.Quote);

        AlleSpieler.Clear();
        foreach (var s in _statistikService.AlleSpielerRangliste(turnier))
        {
            AlleSpieler.Add(new SpielerStatistikZeileModel
            {
                SpielerId = s.SpielerId,
                TeamId = s.TeamId,
                Platz = s.Platz,
                Name = s.Name,
                TeamName = s.TeamName,
                LogoPfad = s.LogoPfad,
                Spiele = s.Spiele,
                Siege = s.Siege,
                Unentschieden = s.Unentschieden,
                Niederlagen = s.Niederlagen,
                Versuche = s.Versuche,
                Treffer = s.Treffer,
                QuoteAbsolutText = $"{s.Treffer}/{s.Versuche}",
                QuoteProzentText = ProzentText(s.Quote)
            });
        }

        // Aufklapp-Zustand je Team über den Neuaufbau hinweg erhalten, da TurnierGeaendert
        // nach jedem einzelnen Versuch feuert und die Liste sonst bei jedem Wurf zuklappen würde.
        var aufgeklappt = Teams.ToDictionary(t => t.TeamId, t => t.IstAufgeklappt);
        Teams.Clear();

        foreach (var teamStat in _statistikService.TeamRangliste(turnier))
        {
            var teamZeile = new TeamStatistikZeileModel
            {
                TeamId = teamStat.TeamId,
                TeamName = teamStat.TeamName,
                LogoPfad = teamStat.LogoPfad,
                Platz = teamStat.Platz,
                Versuche = teamStat.Versuche,
                Treffer = teamStat.Treffer,
                QuoteAbsolutText = $"{teamStat.Treffer}/{teamStat.Versuche}",
                QuoteProzentText = ProzentText(teamStat.Quote),
                IstAufgeklappt = aufgeklappt.GetValueOrDefault(teamStat.TeamId)
            };

            foreach (var spielerZeile in AlleSpieler.Where(z => z.TeamId == teamStat.TeamId))
                teamZeile.Spieler.Add(spielerZeile);

            Teams.Add(teamZeile);
        }

        HatDaten = true;
    }

    private static string ProzentText(double quote) =>
        (quote * 100).ToString("0.0", DeutscheKultur) + "%";
}
