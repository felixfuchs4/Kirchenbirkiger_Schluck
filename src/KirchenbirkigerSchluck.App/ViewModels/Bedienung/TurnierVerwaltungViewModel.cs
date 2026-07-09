/*************************************************************
 * Datei:        TurnierVerwaltungViewModel.cs
 * Zweck:        ViewModel für Turnierverwaltung (Erstellen, Laden, Speichern, Teams, Spieler)
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Verwaltet das Anlegen, Laden und Speichern von Turnieren sowie Team- und Spielerverwaltung.
/// Löst beim Statuswechsel automatisch die Spielplan- bzw. Finalrunden-Generierung aus.
/// </summary>
public partial class TurnierVerwaltungViewModel : ObservableObject
{
    private readonly ITurnierService _turnierService;
    private readonly ISpielplanService _spielplanService;
    private readonly ISpeicherstandService _speicherstandService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly AnzeigeZustandService _anzeigeZustand;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public TurnierVerwaltungViewModel(
        ITurnierService turnierService,
        ISpielplanService spielplanService,
        ISpeicherstandService speicherstandService,
        TurnierZustandService turnierZustandService,
        AnzeigeZustandService anzeigeZustandService)
    {
        _turnierService  = turnierService;
        _spielplanService = spielplanService;
        _speicherstandService = speicherstandService;
        _turnierZustand  = turnierZustandService;
        _anzeigeZustand  = anzeigeZustandService;

        turnierZustandService.TurnierGeaendert += (_, _) => TurnierInfoAktualisieren();
        TurnierInfoAktualisieren();
        SpeicherstaendeAktualisieren();
        NeuesTournierDatum = DateTime.Today;
    }

    // ──── Formularfelder: Neues Turnier ──────────────────────────────────────

    /// <summary>Anlass des neuen Turniers (Pflichtfeld).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TurnierErstellenCommand))]
    private string _neuerAnlass = string.Empty;

    /// <summary>Datum des neuen Turniers als DateTime für den DatePicker.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TurnierErstellenCommand))]
    private DateTime _neuesTournierDatum = DateTime.Today;

    /// <summary>Gewähltes Wertungssystem für das neue Turnier.</summary>
    [ObservableProperty]
    private Wertungssystem _gewaehltesBewertungssystem = Wertungssystem.Eishockey;

    /// <summary>Finalrunden-Modus, der beim Übergang zur Finalrunde angewendet wird.</summary>
    [ObservableProperty]
    private FinalrundenModus _gewaehlterFinalrundenModus = FinalrundenModus.KoBaumEin;

    /// <summary>Gewählte Wertungsart für den Torschützenkönig.</summary>
    [ObservableProperty]
    private TorschuetzenWertung _gewaehlteTorschuetzenWertung = TorschuetzenWertung.Absolut;

    /// <summary>Verfügbare Torschützen-Wertungsarten.</summary>
    public IReadOnlyList<TorschuetzenWertung> AlleTorschuetzenWertungen { get; } =
        Enum.GetValues<TorschuetzenWertung>().ToList().AsReadOnly();

    /// <summary>Verfügbare Wertungssysteme.</summary>
    public IReadOnlyList<Wertungssystem> AlleWertungssysteme { get; } =
        Enum.GetValues<Wertungssystem>().ToList().AsReadOnly();

    /// <summary>Verfügbare Finalrunden-Modi.</summary>
    public IReadOnlyList<FinalrundenModus> AlleFinalrundenModi { get; } =
        Enum.GetValues<FinalrundenModus>().ToList().AsReadOnly();

    // ──── Formularfelder: Team hinzufügen ────────────────────────────────────

    /// <summary>Name des neuen Teams (Pflichtfeld).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TeamHinzufuegenCommand))]
    private string _neuerTeamName = string.Empty;

    /// <summary>Optionale Kurzbezeichnung des neuen Teams.</summary>
    [ObservableProperty]
    private string _neuerKurzname = string.Empty;

    // ──── Aktuelles Turnier ──────────────────────────────────────────────────

    /// <summary>Zusammenfassung des geladenen Turniers als Anzeigetext.</summary>
    [ObservableProperty]
    private string _turnierInfo = "Kein Turnier geladen";

    /// <summary>Status-Anzeigetext des aktuellen Turniers.</summary>
    [ObservableProperty]
    private string _turnierStatusText = string.Empty;

    /// <summary>Hinweistext, was als nächster Schritt durchzuführen ist.</summary>
    [ObservableProperty]
    private string _naechsterSchrittHinweis = string.Empty;

    /// <summary>Gibt an, ob ein Turnier geladen ist.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TurnierSpeichernCommand))]
    [NotifyCanExecuteChangedFor(nameof(StatusWechselnCommand))]
    [NotifyCanExecuteChangedFor(nameof(TurnierZuruecksetzenCommand))]
    [NotifyCanExecuteChangedFor(nameof(TeamHinzufuegenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpielerHinzufuegenCommand))]
    private bool _hatGeladenesTurnier;

    /// <summary>Anzeige-Liste aller Teams im aktuellen Turnier.</summary>
    public ObservableCollection<TeamAnzeigeModel> Teams { get; } = [];

    // ──── Spieler-Verwaltung ─────────────────────────────────────────────────

    /// <summary>Aktuell zur Spielerverwaltung ausgewähltes Team.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpielerHinzufuegenCommand))]
    [NotifyPropertyChangedFor(nameof(HatAusgewaehlterTeam))]
    private TeamAnzeigeModel? _ausgewaehlterTeam;

    /// <summary>Gibt an, ob ein Team für die Spielerverwaltung ausgewählt wurde.</summary>
    public bool HatAusgewaehlterTeam => AusgewaehlterTeam is not null;

    /// <summary>Editierbare Spielerliste des aktuell ausgewählten Teams.</summary>
    public ObservableCollection<SpielerBearbeitungsModel> SpielerDesAusgewaehltenTeams { get; } = [];

    /// <summary>Name des neuen Spielers für das ausgewählte Team.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpielerHinzufuegenCommand))]
    private string _neuerSpielerName = string.Empty;

    // ──── Navigation ─────────────────────────────────────────────────────────

    /// <summary>Wird ausgelöst, wenn zum Gruppen-/Auslosungs-Tab gewechselt werden soll.</summary>
    public event EventHandler? ZuGruppenauslosungGewuenscht;

    /// <summary>Navigiert zur Gruppenauslosung.</summary>
    [RelayCommand]
    private void ZurGruppenauslosung() => ZuGruppenauslosungGewuenscht?.Invoke(this, EventArgs.Empty);

    // ──── Kommandos ──────────────────────────────────────────────────────────

    /// <summary>Erstellt ein neues Turnier mit den Pflichtfeldern.</summary>
    [RelayCommand(CanExecute = nameof(KannTurnierErstellen))]
    private void TurnierErstellen()
    {
        var datum   = DateOnly.FromDateTime(NeuesTournierDatum);
        var turnier = _turnierService.TurnierErstellen(NeuerAnlass, datum, GewaehltesBewertungssystem);
        turnier.FinalrundenModus = GewaehlterFinalrundenModus;
        turnier.TorschuetzenWertung = GewaehlteTorschuetzenWertung;
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.TurnierSetzen(turnier);
        NeuerAnlass = string.Empty;
    }

    private bool KannTurnierErstellen() =>
        !string.IsNullOrWhiteSpace(NeuerAnlass);

    /// <summary>Lädt das zuletzt gespeicherte Turnier.</summary>
    [RelayCommand]
    private void TurnierLaden()
    {
        try
        {
            var turnier = _turnierService.TurnierLaden();
            _turnierZustand.TurnierSetzen(turnier);
            MessageBox.Show(
                $"Turnier geladen:\n{turnier.Anlass} ({turnier.Datum:dd.MM.yyyy})",
                "Turnier geladen", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show(
                "Es wurde noch kein gespeichertes Turnier gefunden.\n" +
                "Lege zunächst ein neues Turnier an oder speichere das aktuelle.",
                "Kein Turnier vorhanden", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Turnier konnte nicht geladen werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>Speichert das aktuelle Turnier manuell.</summary>
    [RelayCommand(CanExecute = nameof(KannSpeichern))]
    private void TurnierSpeichern()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        try
        {
            _turnierService.TurnierSpeichern(turnier);
            MessageBox.Show("Turnier wurde gespeichert.",
                "Gespeichert", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Turnier konnte nicht gespeichert werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool KannSpeichern() => HatGeladenesTurnier;

    // ──── Speicherstände (benannt + automatische Backups) ────────────────────

    /// <summary>Titel für einen neuen benannten Speicherstand (Pflicht für „Speichern unter").</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpeichernUnterCommand))]
    private string _speicherstandTitel = string.Empty;

    /// <summary>Optionale Kurzbeschreibung zur besseren Auffindbarkeit.</summary>
    [ObservableProperty]
    private string _speicherstandBeschreibung = string.Empty;

    /// <summary>Alle ladbaren Speicherstände (benannte Stände und automatische Backups).</summary>
    public ObservableCollection<SpeicherstandInfo> Speicherstaende { get; } = [];

    /// <summary>Aktuell in der Liste ausgewählter Speicherstand.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpeicherstandLadenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpeicherstandLoeschenCommand))]
    private SpeicherstandInfo? _ausgewaehlterSpeicherstand;

    /// <summary>Speichert den aktuellen Turnierstand unter dem eingegebenen Titel (mit optionaler Beschreibung).</summary>
    [RelayCommand(CanExecute = nameof(KannSpeichernUnter))]
    private void SpeichernUnter()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var titel = SpeicherstandTitel.Trim();

        // Vorhandenen benannten Stand gleichen Titels? → Überschreiben bestätigen
        bool existiert = Speicherstaende.Any(s =>
            s.Typ == SpeicherstandTyp.Benannt &&
            string.Equals(s.Titel, titel, StringComparison.OrdinalIgnoreCase));
        if (existiert)
        {
            var frage = MessageBox.Show(
                $"Ein Speicherstand \"{titel}\" existiert bereits. Überschreiben?",
                "Speicherstand überschreiben", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (frage != MessageBoxResult.Yes) return;
        }

        try
        {
            _speicherstandService.SpeichernUnter(turnier, titel, SpeicherstandBeschreibung);
            SpeicherstandTitel = string.Empty;
            SpeicherstandBeschreibung = string.Empty;
            SpeicherstaendeAktualisieren();
            MessageBox.Show("Speicherstand gespeichert.", "Gespeichert",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Speicherstand konnte nicht gespeichert werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool KannSpeichernUnter() =>
        HatGeladenesTurnier && !string.IsNullOrWhiteSpace(SpeicherstandTitel);

    /// <summary>Lädt den ausgewählten Speicherstand und ersetzt damit den aktuellen Arbeitsstand.</summary>
    [RelayCommand(CanExecute = nameof(KannAusgewaehltenSpeicherstand))]
    private void SpeicherstandLaden()
    {
        var info = AusgewaehlterSpeicherstand;
        if (info is null) return;

        var frage = MessageBox.Show(
            $"Speicherstand laden?\n\n{info.Titel} ({info.GespeichertAm:dd.MM.yyyy HH:mm})\n\n" +
            "Der aktuelle Arbeitsstand wird ersetzt.",
            "Speicherstand laden", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (frage != MessageBoxResult.Yes) return;

        try
        {
            var turnier = _speicherstandService.Laden(info);
            _turnierZustand.TurnierSetzen(turnier);
            _turnierService.TurnierSpeichern(turnier); // als Arbeitsstand übernehmen
            MessageBox.Show(
                $"Geladen: {turnier.Anlass} ({turnier.Datum:dd.MM.yyyy})",
                "Speicherstand geladen", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Speicherstand konnte nicht geladen werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>Löscht die Datei des ausgewählten Speicherstands (nach Bestätigung).</summary>
    [RelayCommand(CanExecute = nameof(KannAusgewaehltenSpeicherstand))]
    private void SpeicherstandLoeschen()
    {
        var info = AusgewaehlterSpeicherstand;
        if (info is null) return;

        var frage = MessageBox.Show(
            $"Speicherstand \"{info.Titel}\" ({info.GespeichertAm:dd.MM.yyyy HH:mm}) wirklich löschen?",
            "Speicherstand löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (frage != MessageBoxResult.Yes) return;

        try
        {
            _speicherstandService.Loeschen(info);
            SpeicherstaendeAktualisieren();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Speicherstand konnte nicht gelöscht werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool KannAusgewaehltenSpeicherstand() => AusgewaehlterSpeicherstand is not null;

    /// <summary>Lädt die Liste der Speicherstände neu aus dem Dateisystem.</summary>
    [RelayCommand]
    private void SpeicherstaendeAktualisieren()
    {
        var vorherPfad = AusgewaehlterSpeicherstand?.Pfad;
        Speicherstaende.Clear();
        foreach (var info in _speicherstandService.Alle())
            Speicherstaende.Add(info);
        AusgewaehlterSpeicherstand = Speicherstaende.FirstOrDefault(s => s.Pfad == vorherPfad);
    }

    /// <summary>
    /// Wechselt den Turnierstatus zum nächsten Schritt.
    /// InVorbereitung→Gruppenphase: erstellt Gruppen und generiert den Gruppenspielplan.
    /// Gruppenphase→Finalrunde: generiert die Finalrunde gemäß gewähltem Modus.
    /// Finalrunde→Abgeschlossen: schaltet die Anzeige auf den Gewinner-Screen.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannStatusWechseln))]
    private void StatusWechseln()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        try
        {
            if (turnier.Status == TurnierStatus.InVorbereitung)
            {
                var aktiveTeams = turnier.Teams
                    .Where(t => t.Status != TeamStatus.Zurueckgezogen)
                    .ToList();

                if (aktiveTeams.Count < 2)
                {
                    MessageBox.Show("Mindestens 2 aktive Teams erforderlich.",
                        "Turnier starten", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Gruppeneinteilung muss zuvor im Tab „Gruppen" ausgelost worden sein
                if (!turnier.Gruppen.Any(g => g.TeamIds.Count > 0))
                {
                    MessageBox.Show(
                        "Es wurde noch keine Gruppeneinteilung ausgelost.\n" +
                        "Bitte zuerst im Tab 'Gruppen' die Auslosung durchführen.",
                        "Gruppen fehlen", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Fehlende Spieler-Slots auf 5 auffüllen (auch bei Teilbefüllung)
                foreach (var team in aktiveTeams.Where(t => t.Spieler.Count < 5))
                {
                    while (team.Spieler.Count < 5)
                        team.Spieler.Add(new Spieler { Name = $"Spieler {team.Spieler.Count + 1}" });
                }

                _spielplanService.GruppenspielplanGenerieren(turnier);
            }
            else if (turnier.Status == TurnierStatus.Gruppenphase)
            {
                turnier.FinalrundenModus = GewaehlterFinalrundenModus;
                _spielplanService.FinalrundeGenerieren(turnier);
            }

            _turnierService.StatusWechseln(turnier);

            // Anzeige automatisch umschalten
            if (turnier.Status == TurnierStatus.Gruppenphase || turnier.Status == TurnierStatus.Finalrunde)
                _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Infoscreen);
            else if (turnier.Status == TurnierStatus.Abgeschlossen)
                _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Gewinner);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Statuswechsel fehlgeschlagen:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    private bool KannStatusWechseln() =>
        HatGeladenesTurnier &&
        _turnierZustand.AktuellesTurnier?.Status != TurnierStatus.Abgeschlossen;

    /// <summary>
    /// Setzt das Turnier auf den Anfangszustand (In Vorbereitung) zurück.
    /// Gruppen, Spielplan, Finalrunde und alle Ergebnisse werden verworfen;
    /// Teams und ihre Spieler bleiben erhalten.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannZuruecksetzen))]
    private void TurnierZuruecksetzen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var bestaetigung = MessageBox.Show(
            "Turnier wirklich zurücksetzen?\n\n" +
            "Gruppeneinteilung, Spielplan, Finalrunde und alle Ergebnisse werden gelöscht.\n" +
            "Teams und Spieler bleiben erhalten.",
            "Turnier zurücksetzen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (bestaetigung != MessageBoxResult.Yes) return;

        turnier.Gruppen.Clear();
        turnier.Finalrundenspiele.Clear();
        turnier.Status = TurnierStatus.InVorbereitung;

        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();

        // Anzeigeoberfläche zurück auf den Startscreen
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Startscreen);
    }

    private bool KannZuruecksetzen() =>
        HatGeladenesTurnier &&
        _turnierZustand.AktuellesTurnier?.Status != TurnierStatus.InVorbereitung;

    /// <summary>
    /// Erzeugt die Finalrunde neu aus den aktuellen Gruppentabellen (inkl. Spiel um Platz 3).
    /// Nützlich, wenn die Finalrunde aus einer älteren Version stammt.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannFinalrundeNeuErzeugen))]
    private void FinalrundeNeuErzeugen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var bestaetigung = MessageBox.Show(
            "Finalrunde neu erzeugen?\n\n" +
            "Der komplette Finalrunden-Spielplan (inkl. Spiel um Platz 3) wird aus den aktuellen " +
            "Gruppentabellen neu aufgebaut. Bereits erfasste Finalrunden-Ergebnisse gehen verloren.",
            "Finalrunde neu erzeugen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (bestaetigung != MessageBoxResult.Yes) return;

        try
        {
            // Modus aus der Auswahl übernehmen (z. B. KoBaumEin für Baum mit Spiel um Platz 3)
            turnier.FinalrundenModus = GewaehlterFinalrundenModus;
            _spielplanService.FinalrundeGenerieren(turnier);
            _turnierService.TurnierSpeichern(turnier);
            _turnierZustand.AenderungMelden();
            _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Infoscreen);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Finalrunde konnte nicht erzeugt werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool KannFinalrundeNeuErzeugen() =>
        HatGeladenesTurnier &&
        _turnierZustand.AktuellesTurnier?.Status == TurnierStatus.Finalrunde;

    /// <summary>Fügt ein neues Team zum aktuellen Turnier hinzu.</summary>
    [RelayCommand(CanExecute = nameof(KannTeamHinzufuegen))]
    private void TeamHinzufuegen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var kurzname = string.IsNullOrWhiteSpace(NeuerKurzname) ? null : NeuerKurzname.Trim();
        _turnierService.TeamHinzufuegen(turnier, NeuerTeamName.Trim(), kurzname);
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();

        NeuerTeamName = string.Empty;
        NeuerKurzname = string.Empty;
    }

    private bool KannTeamHinzufuegen() =>
        HatGeladenesTurnier &&
        !string.IsNullOrWhiteSpace(NeuerTeamName) &&
        _turnierZustand.AktuellesTurnier?.Status == TurnierStatus.InVorbereitung;

    /// <summary>Fügt einen Spieler dem aktuell ausgewählten Team hinzu.</summary>
    [RelayCommand(CanExecute = nameof(KannSpielerHinzufuegen))]
    private void SpielerHinzufuegen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehlterTeam is null) return;

        var team = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehlterTeam.Id);
        if (team is null) return;

        var spieler = new Spieler { Name = NeuerSpielerName.Trim() };
        team.Spieler.Add(spieler);
        _turnierService.TurnierSpeichern(turnier);

        var model = SpielerModelErstellen(spieler);
        SpielerDesAusgewaehltenTeams.Add(model);
        NeuerSpielerName = string.Empty;
    }

    private bool KannSpielerHinzufuegen() =>
        HatGeladenesTurnier &&
        AusgewaehlterTeam is not null &&
        !string.IsNullOrWhiteSpace(NeuerSpielerName);

    // ──── Source-Generator-Hooks ──────────────────────────────────────────────

    /// <summary>Aktualisiert die Spielerliste wenn das ausgewählte Team wechselt.</summary>
    partial void OnAusgewaehlterTeamChanged(TeamAnzeigeModel? value)
    {
        SpielerListeNeuLaden(value?.Id);
    }

    /// <summary>Baut die editierbare Spielerliste für das Team mit der gegebenen Id auf.</summary>
    private void SpielerListeNeuLaden(Guid? teamId)
    {
        // Alte Rename-Subscriptions abmelden
        foreach (var m in SpielerDesAusgewaehltenTeams)
            m.PropertyChanged -= OnSpielerNameGeaendert;

        SpielerDesAusgewaehltenTeams.Clear();
        if (teamId is null) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        var team    = turnier?.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team is null) return;

        foreach (var sp in team.Spieler)
            SpielerDesAusgewaehltenTeams.Add(SpielerModelErstellen(sp));
    }

    /// <summary>Wird aufgerufen wenn der Name eines Spieler-Modells geändert wurde; speichert sofort.</summary>
    private void OnSpielerNameGeaendert(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SpielerBearbeitungsModel.Name)) return;
        if (sender is not SpielerBearbeitungsModel model) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null || AusgewaehlterTeam is null) return;

        var team    = turnier.Teams.FirstOrDefault(t => t.Id == AusgewaehlterTeam.Id);
        var spieler = team?.Spieler.FirstOrDefault(s => s.Id == model.Id);
        if (spieler is null) return;

        spieler.Name = model.Name;
        _turnierService.TurnierSpeichern(turnier);
    }

    /// <summary>Erstellt ein SpielerBearbeitungsModel und meldet es für Rename-Events an.</summary>
    private SpielerBearbeitungsModel SpielerModelErstellen(Spieler spieler)
    {
        var model = new SpielerBearbeitungsModel { Id = spieler.Id, Name = spieler.Name };
        model.PropertyChanged += OnSpielerNameGeaendert;
        return model;
    }

    // ──── Interne Aktualisierung ─────────────────────────────────────────────

    private void TurnierInfoAktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        HatGeladenesTurnier = turnier is not null;

        if (turnier is null)
        {
            TurnierInfo      = "Kein Turnier geladen";
            TurnierStatusText = string.Empty;
            Teams.Clear();
            AusgewaehlterTeam = null;
            return;
        }

        TurnierInfo = $"{turnier.Anlass}  |  {turnier.Datum:dd.MM.yyyy}  |  {turnier.Teams.Count} Teams";
        TurnierStatusText = turnier.Status switch
        {
            TurnierStatus.InVorbereitung => "In Vorbereitung",
            TurnierStatus.Gruppenphase   => "Gruppenphase läuft",
            TurnierStatus.Finalrunde     => "Finalrunde",
            TurnierStatus.Abgeschlossen  => "Abgeschlossen",
            _ => turnier.Status.ToString()
        };

        NaechsterSchrittHinweis = turnier.Status switch
        {
            TurnierStatus.InVorbereitung =>
                "Teams und Spieler eintragen, dann Gruppenphase starten.",
            TurnierStatus.Gruppenphase =>
                "Spiele starten und Ergebnisse erfassen. Danach Finalrunde starten.",
            TurnierStatus.Finalrunde =>
                "Finalspiele abarbeiten. Danach Turnier abschließen.",
            TurnierStatus.Abgeschlossen =>
                "Turnier abgeschlossen. Gewinner-Screen aktiv.",
            _ => string.Empty
        };

        // Teams-Liste neu aufbauen; Auswahl nach Refresh wiederherstellen
        var prevId = AusgewaehlterTeam?.Id;
        Teams.Clear();
        foreach (var team in turnier.Teams)
        {
            var anzeige = team.Kurzname is not null
                ? $"{team.Name} ({team.Kurzname})"
                : team.Name;
            Teams.Add(new TeamAnzeigeModel(team.Id, anzeige));
        }

        AusgewaehlterTeam = prevId.HasValue
            ? Teams.FirstOrDefault(t => t.Id == prevId)
            : null;

        // Status-abhängige Kommandos neu bewerten (Status ändert HatGeladenesTurnier nicht)
        StatusWechselnCommand.NotifyCanExecuteChanged();
        TurnierZuruecksetzenCommand.NotifyCanExecuteChanged();
        FinalrundeNeuErzeugenCommand.NotifyCanExecuteChanged();
    }

}
