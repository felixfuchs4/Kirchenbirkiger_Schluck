/*************************************************************
 * Datei:        AnzeigeWindowViewModel.cs
 * Zweck:        Shell-ViewModel für die Anzeigeoberfläche; steuert Screen-Wechsel und Rotation
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.App.ViewModels.Anzeige;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels;

/// <summary>
/// Koordiniert die Anzeigeoberfläche: hört auf den AnzeigeZustandService und
/// schaltet zwischen den vier Screen-ViewModels um.
/// Der DispatcherTimer treibt die automatische Infoscreen-Rotation.
/// </summary>
public partial class AnzeigeWindowViewModel : ObservableObject
{
    private readonly AnzeigeZustandService _anzeigeZustand;
    private readonly DispatcherTimer _rotationsTimer;

    // ──── Screen-ViewModels ──────────────────────────────────────────────────

    /// <summary>ViewModel für den Startscreen.</summary>
    public StartscreenViewModel StartscreenVm { get; }

    /// <summary>ViewModel für den Infoscreen.</summary>
    public InfoscreenViewModel InfoscreenVm { get; }

    /// <summary>ViewModel für den Matchday-Screen.</summary>
    public MatchdayViewModel MatchdayVm { get; }

    /// <summary>ViewModel für den Gewinner-Screen.</summary>
    public GewinnerViewModel GewinnerVm { get; }

    /// <summary>ViewModel für die animierte Auslosung.</summary>
    public AuslosungAnzeigeViewModel AuslosungVm { get; }

    // ──── Aktiver Screen ─────────────────────────────────────────────────────

    /// <summary>
    /// Das aktuell angezeigte Screen-ViewModel.
    /// Das AnzeigeWindow bindet hieran über DataTemplates.
    /// </summary>
    [ObservableProperty]
    private ObservableObject _aktuellerScreenViewModel;

    /// <summary>Rotationsintervall in Sekunden (Standard: 5).</summary>
    public int RotationsIntervallSekunden
    {
        get => (int)_rotationsTimer.Interval.TotalSeconds;
        set
        {
            if (value < 1) return;
            _rotationsTimer.Interval = TimeSpan.FromSeconds(value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Initialisiert das Shell-ViewModel, verdrahtet Events und startet den Rotations-Timer.
    /// </summary>
    public AnzeigeWindowViewModel(
        AnzeigeZustandService anzeigeZustandService,
        StartscreenViewModel startscreenVm,
        InfoscreenViewModel infoscreenVm,
        MatchdayViewModel matchdayVm,
        GewinnerViewModel gewinnerVm,
        AuslosungAnzeigeViewModel auslosungVm)
    {
        _anzeigeZustand = anzeigeZustandService;
        StartscreenVm = startscreenVm;
        InfoscreenVm = infoscreenVm;
        MatchdayVm = matchdayVm;
        GewinnerVm = gewinnerVm;
        AuslosungVm = auslosungVm;

        _aktuellerScreenViewModel = startscreenVm;
        startscreenVm.Aktivieren();

        // Auf manuelle Screen-Wechsel reagieren
        anzeigeZustandService.ScreenGewechselt += (_, screen) => ScreenWechseln(screen);

        // Auf Live-Spielzustandsänderungen reagieren
        anzeigeZustandService.SpielZustandAktualisiert += (_, args) =>
            matchdayVm.SpielAktualisieren(args.Spiel, args.Turnier);

        // Auf den Start der animierten Auslosung reagieren
        anzeigeZustandService.AuslosungGestartet += (_, daten) =>
        {
            ScreenWechseln(AnzeigeScreen.Auslosung);
            auslosungVm.Vorbereiten(daten);
        };

        // Infoscreen-Rotation
        _rotationsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _rotationsTimer.Tick += (_, _) => InfoscreenRotationsSchritt();
        _rotationsTimer.Start();
    }

    // ──── Screen-Wechsel ─────────────────────────────────────────────────────

    /// <summary>Schaltet auf den angeforderten Screen um.</summary>
    public void ScreenWechseln(AnzeigeScreen screen)
    {
        // Startscreen-Timer nur starten/stoppen wenn nötig
        if (AktuellerScreenViewModel is StartscreenViewModel aktuellerStart)
            aktuellerStart.Deaktivieren();

        AktuellerScreenViewModel = screen switch
        {
            AnzeigeScreen.Startscreen => StartscreenVm,
            AnzeigeScreen.Infoscreen  => InfoscreenVm,
            AnzeigeScreen.Matchday    => MatchdayVm,
            AnzeigeScreen.Gewinner    => GewinnerVm,
            AnzeigeScreen.Auslosung   => AuslosungVm,
            _ => StartscreenVm
        };

        if (AktuellerScreenViewModel is StartscreenViewModel neuerStart)
            neuerStart.Aktivieren();

        // Infoscreen: Daten auffrischen und Rotation von Folie 1 starten
        if (AktuellerScreenViewModel is InfoscreenViewModel)
            InfoscreenVm.Aktualisieren();
    }

    /// <summary>
    /// Aktualisiert den Matchday-Screen mit den neuesten Spieldaten
    /// und wechselt automatisch auf den Matchday-Screen.
    /// </summary>
    public void SpielGestartet(Spiel spiel, Turnier turnier)
    {
        MatchdayVm.SpielAktualisieren(spiel, turnier);
        ScreenWechseln(AnzeigeScreen.Matchday);
    }

    // ──── Infoscreen-Rotation ────────────────────────────────────────────────

    private void InfoscreenRotationsSchritt()
    {
        if (_anzeigeZustand.AktuellerScreen == AnzeigeScreen.Infoscreen
            && AktuellerScreenViewModel is InfoscreenViewModel)
        {
            InfoscreenVm.NaechsterSlide();
        }
    }
}
