/*************************************************************
 * Datei:        TeamverwaltungViewModel.cs
 * Zweck:        ViewModel für die dedizierte Team- und Spielerverwaltung
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;
using Microsoft.Win32;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Verwaltet Teams und deren Spieler in einer eigenen, großflächigen Ansicht:
/// Teams anlegen, umbenennen, entfernen sowie Spieler ansehen, ändern und hinzufügen.
/// </summary>
public partial class TeamverwaltungViewModel : ObservableObject
{
    private readonly ITurnierService _turnierService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly LogoService _logoService;

    // Verhindert Speicher-Rückkopplungen, während Detailfelder neu befüllt werden.
    private bool _ladevorgang;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public TeamverwaltungViewModel(
        ITurnierService turnierService,
        TurnierZustandService turnierZustandService,
        LogoService logoService)
    {
        _turnierService = turnierService;
        _turnierZustand = turnierZustandService;
        _logoService = logoService;

        turnierZustandService.TurnierGeaendert += (_, _) => TeamsAktualisieren();
        TeamsAktualisieren();
    }

    // ──── Teamliste ──────────────────────────────────────────────────────────

    /// <summary>Alle Teams des aktuellen Turniers.</summary>
    public ObservableCollection<TeamAnzeigeModel> Teams { get; } = [];

    /// <summary>Aktuell ausgewähltes Team.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HatAusgewaehltesTeam))]
    [NotifyCanExecuteChangedFor(nameof(SpielerHinzufuegenCommand))]
    [NotifyCanExecuteChangedFor(nameof(TeamEntfernenCommand))]
    [NotifyCanExecuteChangedFor(nameof(LogoHochladenCommand))]
    [NotifyCanExecuteChangedFor(nameof(LogoEntfernenCommand))]
    private TeamAnzeigeModel? _ausgewaehltesTeam;

    /// <summary>Gibt an, ob ein Team ausgewählt ist.</summary>
    public bool HatAusgewaehltesTeam => AusgewaehltesTeam is not null;

    /// <summary>Gibt an, ob ein Turnier geladen ist.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TeamHinzufuegenCommand))]
    private bool _hatGeladenesTurnier;

    /// <summary>Gibt an, ob sich das Turnier noch in der Vorbereitung befindet (Teams änderbar).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TeamHinzufuegenCommand))]
    [NotifyCanExecuteChangedFor(nameof(TeamEntfernenCommand))]
    private bool _istInVorbereitung;

    // ──── Detailfelder des ausgewählten Teams ────────────────────────────────

    /// <summary>Editierbarer Name des ausgewählten Teams (speichert bei Änderung).</summary>
    [ObservableProperty]
    private string _detailName = string.Empty;

    /// <summary>Editierbares Kürzel des ausgewählten Teams (speichert bei Änderung).</summary>
    [ObservableProperty]
    private string _detailKurzname = string.Empty;

    /// <summary>Logo-Pfad des ausgewählten Teams (für die Vorschau).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HatLogo))]
    [NotifyCanExecuteChangedFor(nameof(LogoEntfernenCommand))]
    private string? _detailLogoPfad;

    /// <summary>Gibt an, ob das ausgewählte Team ein Logo hat.</summary>
    public bool HatLogo => !string.IsNullOrWhiteSpace(DetailLogoPfad);

    /// <summary>Spieler des ausgewählten Teams – Namen editierbar, Änderungen werden gespeichert.</summary>
    public ObservableCollection<SpielerBearbeitungsModel> Spieler { get; } = [];

    // ──── Formularfelder: neues Team / neuer Spieler ─────────────────────────

    /// <summary>Name für ein neues Team.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TeamHinzufuegenCommand))]
    private string _neuerTeamName = string.Empty;

    /// <summary>Optionales Kürzel für ein neues Team.</summary>
    [ObservableProperty]
    private string _neuerKurzname = string.Empty;

    /// <summary>Name für einen neuen Spieler im ausgewählten Team.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpielerHinzufuegenCommand))]
    private string _neuerSpielerName = string.Empty;

    // ──── Kommandos: Teams ───────────────────────────────────────────────────

    /// <summary>Fügt ein neues Team hinzu.</summary>
    [RelayCommand(CanExecute = nameof(KannTeamHinzufuegen))]
    private void TeamHinzufuegen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var kurzname = string.IsNullOrWhiteSpace(NeuerKurzname) ? null : NeuerKurzname.Trim();
        var team = _turnierService.TeamHinzufuegen(turnier, NeuerTeamName.Trim(), kurzname);
        _turnierService.TurnierSpeichern(turnier);

        NeuerTeamName = string.Empty;
        NeuerKurzname = string.Empty;

        _turnierZustand.AenderungMelden();

        // Neu angelegtes Team direkt auswählen
        AusgewaehltesTeam = Teams.FirstOrDefault(t => t.Id == team.Id);
    }

    private bool KannTeamHinzufuegen() =>
        HatGeladenesTurnier && IstInVorbereitung && !string.IsNullOrWhiteSpace(NeuerTeamName);

    /// <summary>Entfernt das ausgewählte Team (nur in der Vorbereitungsphase).</summary>
    [RelayCommand(CanExecute = nameof(KannTeamEntfernen))]
    private void TeamEntfernen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (team is null) return;

        var bestaetigung = MessageBox.Show(
            $"Team \"{team.Name}\" wirklich entfernen?\nAlle zugehörigen Spieler werden ebenfalls gelöscht.",
            "Team entfernen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (bestaetigung != MessageBoxResult.Yes) return;

        turnier.Teams.Remove(team);
        _turnierService.TurnierSpeichern(turnier);
        AusgewaehltesTeam = null;
        _turnierZustand.AenderungMelden();
    }

    private bool KannTeamEntfernen() =>
        HatAusgewaehltesTeam && IstInVorbereitung;

    // ──── Kommandos: Logo ────────────────────────────────────────────────────

    /// <summary>Öffnet einen Dateidialog und hinterlegt das gewählte Bild als Team-Logo.</summary>
    [RelayCommand(CanExecute = nameof(KannLogoBearbeiten))]
    private void LogoHochladen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (team is null) return;

        var dialog = new OpenFileDialog
        {
            Title = "Team-Logo auswählen",
            Filter = "Bilddateien (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var relativerPfad = _logoService.LogoSpeichern(team.Id, dialog.FileName);
            team.LogoPfad = relativerPfad;
            _turnierService.TurnierSpeichern(turnier);
            DetailLogoPfad = relativerPfad;
            _turnierZustand.AenderungMelden();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Logo konnte nicht gespeichert werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Entfernt das Logo des ausgewählten Teams.</summary>
    [RelayCommand(CanExecute = nameof(KannLogoEntfernen))]
    private void LogoEntfernen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (team is null) return;

        _logoService.LogoEntfernen(team.Id);
        team.LogoPfad = null;
        _turnierService.TurnierSpeichern(turnier);
        DetailLogoPfad = null;
        _turnierZustand.AenderungMelden();
    }

    private bool KannLogoBearbeiten() => HatAusgewaehltesTeam;

    private bool KannLogoEntfernen() => HatAusgewaehltesTeam && HatLogo;

    // ──── Kommandos: Spieler ─────────────────────────────────────────────────

    /// <summary>Fügt dem ausgewählten Team einen Spieler hinzu.</summary>
    [RelayCommand(CanExecute = nameof(KannSpielerHinzufuegen))]
    private void SpielerHinzufuegen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (team is null) return;

        var spieler = new Spieler { Name = NeuerSpielerName.Trim() };
        team.Spieler.Add(spieler);
        _turnierService.TurnierSpeichern(turnier);

        Spieler.Add(SpielerModelErstellen(spieler));
        NeuerSpielerName = string.Empty;
    }

    private bool KannSpielerHinzufuegen() =>
        HatAusgewaehltesTeam && !string.IsNullOrWhiteSpace(NeuerSpielerName);

    /// <summary>Entfernt einen Spieler aus dem ausgewählten Team.</summary>
    [RelayCommand]
    private void SpielerEntfernen(SpielerBearbeitungsModel? model)
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null || model is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        var spieler = team?.Spieler.FirstOrDefault(s => s.Id == model.Id);
        if (team is null || spieler is null) return;

        team.Spieler.Remove(spieler);
        _turnierService.TurnierSpeichern(turnier);

        model.PropertyChanged -= OnSpielerNameGeaendert;
        Spieler.Remove(model);
    }

    // ──── Reaktion auf Auswahl- und Feldänderungen ───────────────────────────

    /// <summary>Lädt Detailfelder und Spielerliste, wenn ein anderes Team gewählt wird.</summary>
    partial void OnAusgewaehltesTeamChanged(TeamAnzeigeModel? value)
    {
        _ladevorgang = true;

        // Alte Spieler-Subscriptions abmelden
        foreach (var m in Spieler)
            m.PropertyChanged -= OnSpielerNameGeaendert;
        Spieler.Clear();

        var turnier = _turnierZustand.AktuellesTurnier;
        var team = value is null ? null : turnier?.Teams.FirstOrDefault(t => t.Id == value.Id);

        DetailName = team?.Name ?? string.Empty;
        DetailKurzname = team?.Kurzname ?? string.Empty;
        DetailLogoPfad = team?.LogoPfad;

        if (team is not null)
            foreach (var sp in team.Spieler)
                Spieler.Add(SpielerModelErstellen(sp));

        _ladevorgang = false;
    }

    /// <summary>Speichert den geänderten Teamnamen.</summary>
    partial void OnDetailNameChanged(string value)
    {
        if (_ladevorgang || AusgewaehltesTeam is null) return;
        if (string.IsNullOrWhiteSpace(value)) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        var team = turnier?.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (turnier is null || team is null) return;

        team.Name = value.Trim();
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    /// <summary>Speichert das geänderte Kürzel.</summary>
    partial void OnDetailKurznameChanged(string value)
    {
        if (_ladevorgang || AusgewaehltesTeam is null) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        var team = turnier?.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        if (turnier is null || team is null) return;

        team.Kurzname = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    // ──── Aktualisierung ─────────────────────────────────────────────────────

    /// <summary>Baut die Teamliste neu auf und stellt die Auswahl wieder her.</summary>
    private void TeamsAktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        HatGeladenesTurnier = turnier is not null;
        IstInVorbereitung = turnier?.Status == TurnierStatus.InVorbereitung;

        var vorherigeId = AusgewaehltesTeam?.Id;

        Teams.Clear();
        if (turnier is not null)
        {
            foreach (var team in turnier.Teams)
            {
                var anzeige = team.Kurzname is not null
                    ? $"{team.Name} ({team.Kurzname})"
                    : team.Name;
                Teams.Add(new TeamAnzeigeModel(team.Id, anzeige, team.LogoPfad));
            }
        }

        AusgewaehltesTeam = vorherigeId.HasValue
            ? Teams.FirstOrDefault(t => t.Id == vorherigeId)
            : null;
    }

    // ──── Hilfsmethoden ──────────────────────────────────────────────────────

    /// <summary>Speichert eine Spieler-Namensänderung sofort ins Turnier.</summary>
    private void OnSpielerNameGeaendert(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SpielerBearbeitungsModel.Name)) return;
        if (sender is not SpielerBearbeitungsModel model) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehltesTeam.Id);
        var spieler = team?.Spieler.FirstOrDefault(s => s.Id == model.Id);
        if (spieler is null) return;

        spieler.Name = model.Name;
        _turnierService.TurnierSpeichern(turnier);
    }

    /// <summary>Erstellt ein editierbares Spieler-Modell und registriert den Rename-Hook.</summary>
    private SpielerBearbeitungsModel SpielerModelErstellen(Spieler spieler)
    {
        var model = new SpielerBearbeitungsModel { Id = spieler.Id, Name = spieler.Name };
        model.PropertyChanged += OnSpielerNameGeaendert;
        return model;
    }
}
