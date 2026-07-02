/*************************************************************
 * Datei:        SiegerehrungViewModel.cs
 * Zweck:        Steuerung der schrittweisen Siegerehrung aus der Verwaltung
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.App.ViewModels.Anzeige;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Bedienseitige Steuerung der Siegerehrung: startet die Ehrung am Anzeigefenster und
/// schaltet die einzelnen Schritte (Spieler und Teams) manuell weiter.
/// </summary>
public partial class SiegerehrungViewModel : ObservableObject
{
    private readonly GewinnerViewModel _gewinnerVm;
    private readonly AnzeigeZustandService _anzeigeZustand;
    private readonly TurnierZustandService _turnierZustand;
    private readonly ITurnierService _turnierService;
    private bool _ladevorgang;

    /// <summary>Initialisiert das ViewModel mit dem Anzeige-Gewinner-ViewModel und dem Vermittlerdienst.</summary>
    public SiegerehrungViewModel(
        GewinnerViewModel gewinnerViewModel,
        AnzeigeZustandService anzeigeZustandService,
        TurnierZustandService turnierZustandService,
        ITurnierService turnierService)
    {
        _gewinnerVm = gewinnerViewModel;
        _anzeigeZustand = anzeigeZustandService;
        _turnierZustand = turnierZustandService;
        _turnierService = turnierService;

        turnierZustandService.TurnierGeaendert += (_, _) => WertungUebernehmen();
        WertungUebernehmen();
    }

    /// <summary>Das Gewinner-ViewModel (für die Vorschau im UI).</summary>
    public GewinnerViewModel GewinnerVm => _gewinnerVm;

    /// <summary>Verfügbare Torschützen-Wertungsarten.</summary>
    public IReadOnlyList<TorschuetzenWertung> AlleWertungen { get; } =
        Enum.GetValues<TorschuetzenWertung>().ToList().AsReadOnly();

    /// <summary>Aktuell gewählte Torschützen-Wertung (auch nachträglich umstellbar).</summary>
    [ObservableProperty]
    private TorschuetzenWertung _gewaehlteWertung = TorschuetzenWertung.Absolut;

    private void WertungUebernehmen()
    {
        _ladevorgang = true;
        GewaehlteWertung = _turnierZustand.AktuellesTurnier?.TorschuetzenWertung ?? TorschuetzenWertung.Absolut;
        _ladevorgang = false;
    }

    partial void OnGewaehlteWertungChanged(TorschuetzenWertung value)
    {
        if (_ladevorgang) return;
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        turnier.TorschuetzenWertung = value;
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    /// <summary>Startet die Siegerehrung von vorne und zeigt den Gewinner-Screen.</summary>
    [RelayCommand]
    private void Starten()
    {
        _gewinnerVm.Aktualisieren();
        _gewinnerVm.Starten();
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Gewinner);
    }

    /// <summary>Schaltet zum nächsten Ehrungsschritt weiter.</summary>
    [RelayCommand]
    private void Weiter() => _gewinnerVm.Weiter();

    /// <summary>Geht einen Ehrungsschritt zurück.</summary>
    [RelayCommand]
    private void Zurueck() => _gewinnerVm.Zurueck();

    /// <summary>Blendet den Gewinner-Screen ein, ohne die Ehrung zurückzusetzen.</summary>
    [RelayCommand]
    private void GewinnerScreenZeigen() => _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Gewinner);
}
