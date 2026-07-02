/*************************************************************
 * Datei:        SpielplanViewModel.cs
 * Zweck:        ViewModel für den Spielplan-Tab der Bedienoberfläche
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Verwaltet die Spielplanliste und ermöglicht das Starten und Verschieben von Spielen.
/// </summary>
public partial class SpielplanViewModel : ObservableObject
{
    private readonly ISpielplanService _spielplanService;
    private readonly ISpielsteuerungService _spielsteuerungService;
    private readonly ITurnierService _turnierService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly AnzeigeZustandService _anzeigeZustand;

    /// <summary>Initialisiert das Spielplan-ViewModel und abonniert Turnierändrungs-Events.</summary>
    public SpielplanViewModel(
        ISpielplanService spielplanService,
        ISpielsteuerungService spielsteuerungService,
        ITurnierService turnierService,
        TurnierZustandService turnierZustandService,
        AnzeigeZustandService anzeigeZustandService)
    {
        _spielplanService = spielplanService;
        _spielsteuerungService = spielsteuerungService;
        _turnierService = turnierService;
        _turnierZustand = turnierZustandService;
        _anzeigeZustand = anzeigeZustandService;

        turnierZustandService.TurnierGeaendert += (_, _) => SpielplanAktualisieren();
        SpielplanAktualisieren();
    }

    // ──── Spielliste ─────────────────────────────────────────────────────────

    /// <summary>Alle Spiele des Turniers als Anzeigemodelle.</summary>
    public ObservableCollection<SpielZeileModel> Spiele { get; } = [];

    /// <summary>Das in der Liste ausgewählte Spiel.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HatAuswahl))]
    [NotifyCanExecuteChangedFor(nameof(SpielNachHintenVerschiebenCommand))]
    [NotifyCanExecuteChangedFor(nameof(MatchscreenAnzeigenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpielHierStartenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpielBearbeitenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpielNeustartenCommand))]
    private SpielZeileModel? _ausgewaehltesSpiel;

    /// <summary>Gibt an, ob ein Spiel in der Liste ausgewählt ist.</summary>
    public bool HatAuswahl => AusgewaehltesSpiel is not null;

    /// <summary>Gibt an, ob das nächste Spiel gestartet werden kann.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NaechstesSpielStartenCommand))]
    private bool _kannNaechstesSpielStarten;

    /// <summary>Name-Anzeige des nächsten Spiels.</summary>
    [ObservableProperty]
    private string _naechstesSpielText = "Kein Spiel vorhanden";

    // ──── Event: Navigation zur Spielsteuerung nach Spielstart ───────────────

    /// <summary>Wird ausgelöst, wenn ein Spiel gestartet wurde und zur Spielsteuerung navigiert werden soll.</summary>
    public event EventHandler? SpielGestartet;

    // ──── Kommandos ──────────────────────────────────────────────────────────

    /// <summary>Startet das nächste Spiel in der Reihenfolge.</summary>
    [RelayCommand(CanExecute = nameof(KannNaechstesSpielStarten))]
    private void NaechstesSpielStarten()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var naechstesSpiel = _spielplanService.NaechstesSpielErmitteln(turnier);
        if (naechstesSpiel is null) return;

        _spielsteuerungService.SpielStarten(naechstesSpiel);
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();

        // Anzeigeoberfläche auf Matchday-Screen schalten
        _anzeigeZustand.SpielZustandMelden(naechstesSpiel, turnier);
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Matchday);

        // Navigation in der Bedienoberfläche
        SpielGestartet?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Zeigt den Matchscreen für das gewählte Spiel auf der Anzeige (ohne es zu starten).</summary>
    [RelayCommand(CanExecute = nameof(HatAuswahl))]
    private void MatchscreenAnzeigen()
    {
        var (turnier, spiel) = AusgewaehltesSpielAufloesen();
        if (turnier is null || spiel is null) return;

        _anzeigeZustand.SpielZustandMelden(spiel, turnier);
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Matchday);
    }

    /// <summary>Startet das gewählte Spiel (sofern noch nicht gestartet) und öffnet die Spielsteuerung.</summary>
    [RelayCommand(CanExecute = nameof(KannStarten))]
    private void SpielHierStarten()
    {
        var (turnier, spiel) = AusgewaehltesSpielAufloesen();
        if (turnier is null || spiel is null) return;

        if (spiel.Status is SpielStatus.Geplant or SpielStatus.Vorbereitet or SpielStatus.Verschoben)
            _spielsteuerungService.SpielStarten(spiel);

        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();

        _anzeigeZustand.SpielZustandMelden(spiel, turnier);
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Matchday);
        SpielGestartet?.Invoke(this, EventArgs.Empty);
    }

    private bool KannStarten() =>
        AusgewaehltesSpiel?.Status is "Geplant" or "Vorbereitet" or "Verschoben";

    /// <summary>
    /// Öffnet das gewählte Spiel zur ausführlichen Bearbeitung in der Spielsteuerung.
    /// Ein bereits abgeschlossenes Spiel wird dazu wieder geöffnet; die Einzelduelle
    /// bleiben erhalten und können (Spieler, Treffer) korrigiert werden.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannBearbeiten))]
    private void SpielBearbeiten()
    {
        var (turnier, spiel) = AusgewaehltesSpielAufloesen();
        if (turnier is null || spiel is null) return;

        // Sicherstellen, dass nicht zwei Spiele gleichzeitig laufen
        var anderesLaeuft = AlleSpiele(turnier).Any(s => s.Id != spiel.Id && s.Status == SpielStatus.Laeuft);
        if (anderesLaeuft)
        {
            MessageBox.Show("Es läuft bereits ein anderes Spiel. Bitte dieses zuerst abschließen.",
                "Bearbeiten nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (spiel.Status is SpielStatus.Abgeschlossen or SpielStatus.Korrigiert)
        {
            spiel.Status = SpielStatus.Laeuft;
            spiel.Ergebnis = null;
        }
        else if (spiel.Status is SpielStatus.Geplant or SpielStatus.Vorbereitet or SpielStatus.Verschoben)
        {
            _spielsteuerungService.SpielStarten(spiel);
        }

        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();

        _anzeigeZustand.SpielZustandMelden(spiel, turnier);
        SpielGestartet?.Invoke(this, EventArgs.Empty);
    }

    private bool KannBearbeiten() =>
        AusgewaehltesSpiel?.Status is "Läuft" or "Abgeschlossen" or "Korrigiert";

    /// <summary>Setzt ein bereits begonnenes/abgeschlossenes Spiel zurück und startet es neu.</summary>
    [RelayCommand(CanExecute = nameof(KannNeustarten))]
    private void SpielNeustarten()
    {
        var (turnier, spiel) = AusgewaehltesSpielAufloesen();
        if (turnier is null || spiel is null) return;

        var bestaetigung = MessageBox.Show(
            "Spiel wirklich neu starten?\nAlle bisherigen Duelle und das Ergebnis dieses Spiels werden gelöscht.",
            "Spiel neu starten", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (bestaetigung != MessageBoxResult.Yes) return;

        spiel.Einzelduelle.Clear();
        spiel.Ergebnis = null;
        spiel.Status = SpielStatus.Geplant;
        spiel.StartZeitpunktUtc = null;
        spiel.EndZeitpunktUtc = null;

        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    private bool KannNeustarten() =>
        AusgewaehltesSpiel?.Status is "Läuft" or "Abgeschlossen" or "Korrigiert";

    /// <summary>Löst das aktuell ausgewählte Spielmodell und das Turnier auf.</summary>
    private (Turnier? Turnier, Spiel? Spiel) AusgewaehltesSpielAufloesen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesSpiel is null) return (turnier, null);
        var spiel = AlleSpiele(turnier).FirstOrDefault(s => s.Id == AusgewaehltesSpiel.SpielId);
        return (turnier, spiel);
    }

    /// <summary>Verschiebt das ausgewählte Spiel im Spielplan nach hinten.</summary>
    [RelayCommand(CanExecute = nameof(KannVerschieben))]
    private void SpielNachHintenVerschieben()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehltesSpiel is null) return;

        _spielplanService.SpielNachHintenVerschieben(turnier, AusgewaehltesSpiel.SpielId);
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    private bool KannVerschieben() =>
        AusgewaehltesSpiel is not null &&
        AusgewaehltesSpiel.Status is "Geplant" or "Verschoben";

    // ──── Aktualisierung ─────────────────────────────────────────────────────

    /// <summary>Lädt alle Spiele neu aus dem aktuellen Turnier.</summary>
    public void SpielplanAktualisieren()
    {
        Spiele.Clear();
        var turnier = _turnierZustand.AktuellesTurnier;

        if (turnier is null)
        {
            KannNaechstesSpielStarten = false;
            NaechstesSpielText = "Kein Turnier geladen";
            return;
        }

        var naechstesSpiel = _spielplanService.NaechstesSpielErmitteln(turnier);
        KannNaechstesSpielStarten = KannNaechstesSpielStarten_Berechnen();

        if (naechstesSpiel is not null)
        {
            var t1 = TeamName(turnier, naechstesSpiel.Team1Id);
            var t2 = TeamName(turnier, naechstesSpiel.Team2Id);
            NaechstesSpielText = $"{t1} vs. {t2}";
        }
        else
        {
            NaechstesSpielText = "Alle Spiele abgeschlossen";
        }

        // Alle Spiele sammeln (Gruppe + Finalrunde) und durchgehend nach Spielnummer sortieren
        var alle = new List<(Spiel Spiel, string Runde)>();

        foreach (var gruppe in turnier.Gruppen)
            foreach (var spiel in gruppe.Spiele)
            {
                var runde = spiel.IstPlatzierungsStechen ? $"{gruppe.Name} – Stechen" : gruppe.Name;
                alle.Add((spiel, runde));
            }

        foreach (var spiel in turnier.Finalrundenspiele)
            alle.Add((spiel, spiel.BracketRunde ?? "Finalrunde"));

        foreach (var (spiel, runde) in alle.OrderBy(x => x.Spiel.Spielnummer))
            Spiele.Add(SpielZeileErstellen(turnier, spiel, runde, spiel.Id == naechstesSpiel?.Id));
    }

    // ──── Hilfsmethoden ─────────────────────────────────────────────────────

    private bool KannNaechstesSpielStarten_Berechnen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return false;
        return _spielplanService.NaechstesSpielErmitteln(turnier) is not null;
    }

    private SpielZeileModel SpielZeileErstellen(Turnier turnier, Spiel spiel, string runde, bool istNaechstes)
    {
        var ergebnis = spiel.Ergebnis is not null
            ? $"{spiel.Ergebnis.DuellpunkteTeam1}:{spiel.Ergebnis.DuellpunkteTeam2}"
            : string.Empty;

        return new SpielZeileModel
        {
            Nummer = spiel.Spielnummer,
            Team1 = TeamName(turnier, spiel.Team1Id),
            Team2 = TeamName(turnier, spiel.Team2Id),
            Team1LogoPfad = TeamLogo(turnier, spiel.Team1Id),
            Team2LogoPfad = TeamLogo(turnier, spiel.Team2Id),
            Status = SpielStatusText(spiel.Status),
            Ergebnis = ergebnis,
            Runde = runde,
            IstNaechstes = istNaechstes,
            SpielId = spiel.Id
        };
    }

    private static string TeamName(Turnier turnier, Guid? teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "TBD";

    private static string? TeamLogo(Turnier turnier, Guid? teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.LogoPfad;

    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier) =>
        turnier.Gruppen.SelectMany(g => g.Spiele).Concat(turnier.Finalrundenspiele);

    private static string SpielStatusText(SpielStatus status) => status switch
    {
        SpielStatus.Geplant       => "Geplant",
        SpielStatus.Vorbereitet   => "Vorbereitet",
        SpielStatus.Laeuft        => "Läuft",
        SpielStatus.Abgeschlossen => "Abgeschlossen",
        SpielStatus.Abgesetzt     => "Abgesetzt",
        SpielStatus.Verschoben    => "Verschoben",
        SpielStatus.Korrigiert    => "Korrigiert",
        _ => status.ToString()
    };
}
