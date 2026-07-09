/*************************************************************
 * Datei:        SiegerehrungViewModel.cs
 * Zweck:        Steuerung der schrittweisen Siegerehrung aus der Verwaltung
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.App.ViewModels.Anzeige;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Bedienseitige Steuerung der Siegerehrung: startet die Ehrung am Anzeigefenster, schaltet
/// die einzelnen Schritte (Spieler und Teams) manuell weiter und erlaubt die Auflösung eines
/// Gleichstands auf Platz 1 der Torschützen-Rangliste per Stechen.
/// </summary>
public partial class SiegerehrungViewModel : ObservableObject
{
    private readonly GewinnerViewModel _gewinnerVm;
    private readonly AnzeigeZustandService _anzeigeZustand;
    private readonly TurnierZustandService _turnierZustand;
    private readonly ITurnierService _turnierService;
    private readonly StatistikService _statistikService;
    private bool _ladevorgang;

    /// <summary>Initialisiert das ViewModel mit dem Anzeige-Gewinner-ViewModel und dem Vermittlerdienst.</summary>
    public SiegerehrungViewModel(
        GewinnerViewModel gewinnerViewModel,
        AnzeigeZustandService anzeigeZustandService,
        TurnierZustandService turnierZustandService,
        ITurnierService turnierService,
        StatistikService statistikService)
    {
        _gewinnerVm = gewinnerViewModel;
        _anzeigeZustand = anzeigeZustandService;
        _turnierZustand = turnierZustandService;
        _turnierService = turnierService;
        _statistikService = statistikService;

        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();
        Aktualisieren();
    }

    /// <summary>Das Gewinner-ViewModel (für die Vorschau im UI).</summary>
    public GewinnerViewModel GewinnerVm => _gewinnerVm;

    /// <summary>Verfügbare Torschützen-Wertungsarten.</summary>
    public IReadOnlyList<TorschuetzenWertung> AlleWertungen { get; } =
        Enum.GetValues<TorschuetzenWertung>().ToList().AsReadOnly();

    /// <summary>Aktuell gewählte Torschützen-Wertung (auch nachträglich umstellbar).</summary>
    [ObservableProperty]
    private TorschuetzenWertung _gewaehlteWertung = TorschuetzenWertung.Absolut;

    /// <summary>Spieler, die aktuell auf Platz 1 der Torschützen-Rangliste exakt gleichauf liegen.</summary>
    public ObservableCollection<SpielerStatistik> StechenKandidaten { get; } = [];

    /// <summary>Gibt an, ob auf Platz 1 ein Gleichstand besteht (unabhängig davon, ob bereits entschieden).</summary>
    [ObservableProperty]
    private bool _stechenErforderlich;

    /// <summary>Gibt an, ob das Stechen um Platz 1 bereits entschieden wurde.</summary>
    [ObservableProperty]
    private bool _stechenEntschieden;

    /// <summary>Ausgewählter bzw. bereits bestätigter Sieger des Stechens.</summary>
    [ObservableProperty]
    private SpielerStatistik? _stechenAuswahl;

    private void Aktualisieren()
    {
        WertungUebernehmen();
        StechenStatusAktualisieren();
    }

    private void WertungUebernehmen()
    {
        _ladevorgang = true;
        GewaehlteWertung = _turnierZustand.AktuellesTurnier?.TorschuetzenWertung ?? TorschuetzenWertung.Absolut;
        _ladevorgang = false;
    }

    private void StechenStatusAktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        StechenKandidaten.Clear();

        if (turnier is null)
        {
            StechenErforderlich = false;
            StechenEntschieden = false;
            StechenAuswahl = null;
            return;
        }

        var kandidaten = _statistikService.GleichstandPlatz1(turnier);
        foreach (var kandidat in kandidaten)
            StechenKandidaten.Add(kandidat);

        StechenErforderlich = kandidaten.Count > 1;
        StechenEntschieden = StechenErforderlich && !_statistikService.StechenPlatz1Offen(turnier);
        StechenAuswahl = turnier.TorschuetzenStechenSiegerId is { } siegerId
            ? kandidaten.FirstOrDefault(k => k.SpielerId == siegerId)
            : null;
    }

    partial void OnStechenAuswahlChanged(SpielerStatistik? value) =>
        StechenSiegerBestaetigenCommand.NotifyCanExecuteChanged();

    /// <summary>Speichert den ausgewählten Spieler als Sieger des Stechens um Platz 1.</summary>
    [RelayCommand(CanExecute = nameof(KannStechenBestaetigen))]
    private void StechenSiegerBestaetigen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || StechenAuswahl is null) return;

        turnier.TorschuetzenStechenSiegerId = StechenAuswahl.SpielerId;
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    private bool KannStechenBestaetigen() => StechenAuswahl is not null;

    partial void OnGewaehlteWertungChanged(TorschuetzenWertung value)
    {
        if (_ladevorgang) return;
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        turnier.TorschuetzenWertung = value;
        turnier.TorschuetzenStechenSiegerId = null; // Wertungsartwechsel kann die Gleichstandsgruppe ändern
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
