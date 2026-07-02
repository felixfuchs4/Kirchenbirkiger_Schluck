/*************************************************************
 * Datei:        EinstellungenViewModel.cs
 * Zweck:        ViewModel für Backup-Verwaltung, Anzeige-Steuerung und Einstellungen
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Backup;
using KirchenbirkigerSchluck.Data.Serialization;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>Repräsentiert eine Backup-Datei mit Anzeigename und vollständigem Pfad.</summary>
/// <param name="Pfad">Vollständiger Dateipfad für die Wiederherstellung.</param>
/// <param name="Anzeigename">Dateiname ohne Verzeichnis für die Listbox-Darstellung.</param>
public sealed record BackupEintragModel(string Pfad, string Anzeigename);

/// <summary>
/// Verwaltet Backup-Erstellung/-Wiederherstellung, Anzeige-Zielbildschirm und Rotationsintervall.
/// </summary>
public partial class EinstellungenViewModel : ObservableObject
{
    private readonly BackupManager _backupManager;
    private readonly TurnierZustandService _turnierZustand;
    private readonly AnzeigeZustandService _anzeigeZustand;
    private readonly ITurnierService _turnierService;
    private readonly AnzeigeWindowViewModel _anzeigeWindowViewModel;

    /// <summary>Einstellungen, welche Folien der Infoscreen anzeigt (für die Schalter im UI).</summary>
    public InfoscreenEinstellungen InfoscreenEinstellungen { get; }

    /// <summary>Initialisiert das Einstellungen-ViewModel.</summary>
    public EinstellungenViewModel(
        BackupManager backupManager,
        TurnierZustandService turnierZustandService,
        AnzeigeZustandService anzeigeZustandService,
        ITurnierService turnierService,
        AnzeigeWindowViewModel anzeigeWindowViewModel,
        InfoscreenEinstellungen infoscreenEinstellungen)
    {
        _backupManager = backupManager;
        _turnierZustand = turnierZustandService;
        _anzeigeZustand = anzeigeZustandService;
        _turnierService = turnierService;
        _anzeigeWindowViewModel = anzeigeWindowViewModel;
        InfoscreenEinstellungen = infoscreenEinstellungen;

        turnierZustandService.TurnierGeaendert += (_, _) => BackuplisteAktualisieren();
        BackuplisteAktualisieren();
    }

    // ──── Backup ─────────────────────────────────────────────────────────────

    /// <summary>Alle vorhandenen Backup-Dateien mit Anzeigename und Pfad, neueste zuerst.</summary>
    public ObservableCollection<BackupEintragModel> VerfuegbareBackups { get; } = [];

    /// <summary>Die aktuell ausgewählte Backup-Datei.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackupWiederherstellenCommand))]
    private BackupEintragModel? _ausgewaehltesBackup;

    /// <summary>Anzeigename der ausgewählten Backup-Datei.</summary>
    [ObservableProperty]
    private string _ausgewaehltesBackupName = string.Empty;

    /// <summary>Gibt an, ob ein Turnier geladen und ein Backup möglich ist.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackupErstellenCommand))]
    private bool _kannBackupErstellen;

    // ──── Rotationsintervall ─────────────────────────────────────────────────

    /// <summary>Rotationsintervall des Infoscreens in Sekunden (Standard: 5).</summary>
    public int RotationsIntervallSekunden
    {
        get => _anzeigeWindowViewModel.RotationsIntervallSekunden;
        set
        {
            _anzeigeWindowViewModel.RotationsIntervallSekunden = value;
            OnPropertyChanged();
        }
    }

    // ──── Anzeige-Steuerung ──────────────────────────────────────────────────

    /// <summary>Schaltet die Anzeigeoberfläche manuell auf den Startscreen.</summary>
    [RelayCommand]
    private void ZeigeStartscreen() => _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Startscreen);

    /// <summary>Schaltet die Anzeigeoberfläche manuell auf den Infoscreen.</summary>
    [RelayCommand]
    private void ZeigeInfoscreen() => _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Infoscreen);

    /// <summary>Schaltet die Anzeigeoberfläche manuell auf den Matchday-Screen.</summary>
    [RelayCommand]
    private void ZeigeMatchday() => _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Matchday);

    /// <summary>Schaltet die Anzeigeoberfläche manuell auf den Gewinner-Screen.</summary>
    [RelayCommand]
    private void ZeigeGewinner() => _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Gewinner);

    // ──── Backup-Kommandos ───────────────────────────────────────────────────

    /// <summary>Erstellt ein manuelles Backup des aktuellen Turniers.</summary>
    [RelayCommand(CanExecute = nameof(KannBackupErstellen))]
    private void BackupErstellen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var pfad = _backupManager.BackupErstellen(turnier);
        BackuplisteAktualisieren();

        MessageBox.Show($"Backup erstellt:\n{Path.GetFileName(pfad)}",
            "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>Stellt den ausgewählten Backup-Stand wieder her.</summary>
    [RelayCommand(CanExecute = nameof(KannWiederherstellen))]
    private void BackupWiederherstellen()
    {
        if (AusgewaehltesBackup is null) return;

        var bestaetigung = MessageBox.Show(
            $"Backup wiederherstellen?\n\n{AusgewaehltesBackup.Anzeigename}\n\nAlle aktuellen Änderungen gehen verloren.",
            "Backup wiederherstellen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (bestaetigung != MessageBoxResult.Yes) return;

        try
        {
            var json = File.ReadAllText(AusgewaehltesBackup.Pfad);
            var turnier = JsonSerializer.Deserialize<Turnier>(json, JsonKonfiguration.Standard);
            if (turnier is null)
            {
                MessageBox.Show("Backup-Datei ist ungültig.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _turnierService.TurnierSpeichern(turnier);
            _turnierZustand.TurnierSetzen(turnier);
            MessageBox.Show("Backup wurde wiederhergestellt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Wiederherstellung fehlgeschlagen:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool KannWiederherstellen() => AusgewaehltesBackup is not null;

    // ──── Aktualisierung ─────────────────────────────────────────────────────

    private void BackuplisteAktualisieren()
    {
        KannBackupErstellen = KannBackupErstellen_Check();

        VerfuegbareBackups.Clear();
        foreach (var pfad in _backupManager.AlleDateien().Reverse())
            VerfuegbareBackups.Add(new BackupEintragModel(pfad, Path.GetFileName(pfad)));
    }

    private bool KannBackupErstellen_Check() => _turnierZustand.AktuellesTurnier is not null;

    partial void OnAusgewaehltesBackupChanged(BackupEintragModel? value)
    {
        AusgewaehltesBackupName = value?.Anzeigename ?? string.Empty;
    }
}
