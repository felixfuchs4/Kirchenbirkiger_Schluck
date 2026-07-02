/*************************************************************
 * Datei:        StartscreenViewModel.cs
 * Zweck:        ViewModel für den Startscreen der Anzeigeoberfläche
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Anzeige;

/// <summary>
/// Verwaltet die Darstellung des Startscreens (Logo, Turniername, Uhrzeit).
/// Wird angezeigt, bevor das Turnier beginnt.
/// </summary>
public partial class StartscreenViewModel : ObservableObject
{
    private readonly DispatcherTimer _uhrzeitTimer;

    /// <summary>Initialisiert den Startscreen-ViewModel und startet den Uhrzeit-Timer.</summary>
    public StartscreenViewModel(TurnierZustandService turnierZustandService)
    {
        _uhrzeitTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _uhrzeitTimer.Tick += (_, _) => AktuelleUhrzeit = DateTime.Now.ToString("HH:mm:ss");

        turnierZustandService.TurnierGeaendert += (_, _) =>
            TurnierName = turnierZustandService.AktuellesTurnier?.Anlass ?? string.Empty;

        TurnierName = turnierZustandService.AktuellesTurnier?.Anlass ?? string.Empty;
        AktuelleUhrzeit = DateTime.Now.ToString("HH:mm:ss");
    }

    /// <summary>Name des aktuellen Turniers, angezeigt unter dem Logo.</summary>
    [ObservableProperty]
    private string _turnierName = string.Empty;

    /// <summary>Aktuelle Uhrzeit im Format HH:mm:ss, wird sekündlich aktualisiert.</summary>
    [ObservableProperty]
    private string _aktuelleUhrzeit = string.Empty;

    /// <summary>Startet die sekündliche Uhrzeitaktualisierung.</summary>
    public void Aktivieren() => _uhrzeitTimer.Start();

    /// <summary>Stoppt die sekündliche Uhrzeitaktualisierung.</summary>
    public void Deaktivieren() => _uhrzeitTimer.Stop();
}
