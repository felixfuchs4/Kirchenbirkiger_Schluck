/*************************************************************
 * Datei:        SpielsteuerungViewModel.cs
 * Zweck:        ViewModel für die Live-Spielsteuerung (Duell-Auswahl, Versuch-Erfassung)
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
using KirchenbirkigerSchluck.Data.Backup;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Steuert den Ablauf einer laufenden Partie:
/// Spieler auswählen, Duell starten, Versuche erfassen, Spiel abschließen.
/// </summary>
public partial class SpielsteuerungViewModel : ObservableObject
{
    private readonly ISpielsteuerungService _spielsteuerungService;
    private readonly ISpielplanService _spielplanService;
    private readonly ITurnierService _turnierService;
    private readonly IWertungsService _wertungsService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly AnzeigeZustandService _anzeigeZustand;
    private readonly BackupManager _backupManager;

    private Spiel? _aktuellesSpiel;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public SpielsteuerungViewModel(
        ISpielsteuerungService spielsteuerungService,
        ISpielplanService spielplanService,
        ITurnierService turnierService,
        IWertungsService wertungsService,
        TurnierZustandService turnierZustandService,
        AnzeigeZustandService anzeigeZustandService,
        BackupManager backupManager)
    {
        _spielsteuerungService = spielsteuerungService;
        _spielplanService      = spielplanService;
        _turnierService        = turnierService;
        _wertungsService       = wertungsService;
        _turnierZustand        = turnierZustandService;
        _anzeigeZustand        = anzeigeZustandService;
        _backupManager         = backupManager;

        turnierZustandService.TurnierGeaendert += (_, _) => SpielAktualisieren();
        SpielAktualisieren();
    }

    // ──── Spielinfos ─────────────────────────────────────────────────────────

    /// <summary>Gibt an, ob gerade eine Partie läuft.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(Team1TrifftCommand))]
    [NotifyCanExecuteChangedFor(nameof(Team2TrifftCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeideNichtTreffenCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeidetreffenCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpielAbschliessenCommand))]
    [NotifyCanExecuteChangedFor(nameof(StechenStartenCommand))]
    private bool _spielLaeuft;

    /// <summary>Name von Team 1.</summary>
    [ObservableProperty]
    private string _team1Name = string.Empty;

    /// <summary>Name von Team 2.</summary>
    [ObservableProperty]
    private string _team2Name = string.Empty;

    /// <summary>Aktuelle Duellsiege von Team 1.</summary>
    [ObservableProperty]
    private int _duellsiegeTeam1;

    /// <summary>Aktuelle Duellsiege von Team 2.</summary>
    [ObservableProperty]
    private int _duellsiegeTeam2;

    /// <summary>Gibt an, ob sich das Spiel im Stechen befindet.</summary>
    [ObservableProperty]
    private bool _istStechen;

    /// <summary>Text für den aktuellen Versuch (z.B. „Versuch 1 von 3").</summary>
    [ObservableProperty]
    private string _aktuellesVersuchText = string.Empty;

    // ──── Duell-Auswahl ──────────────────────────────────────────────────────

    /// <summary>Verfügbare Spieler von Team 1 für die Duell-ComboBox.</summary>
    public ObservableCollection<Spieler> Team1Spieler { get; } = [];

    /// <summary>Verfügbare Spieler von Team 2 für die Duell-ComboBox.</summary>
    public ObservableCollection<Spieler> Team2Spieler { get; } = [];

    /// <summary>Ausgewählter Spieler 1 für das nächste Duell.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DuellStartenCommand))]
    [NotifyCanExecuteChangedFor(nameof(StechenStartenCommand))]
    private Spieler? _gewaehlterSpieler1;

    /// <summary>Ausgewählter Spieler 2 für das nächste Duell.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DuellStartenCommand))]
    [NotifyCanExecuteChangedFor(nameof(StechenStartenCommand))]
    private Spieler? _gewaehlterSpieler2;

    /// <summary>Gibt an, ob gerade ein Duell läuft (Versuch-Buttons aktiv).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(Team1TrifftCommand))]
    [NotifyCanExecuteChangedFor(nameof(Team2TrifftCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeideNichtTreffenCommand))]
    [NotifyCanExecuteChangedFor(nameof(BeidetreffenCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuellStartenCommand))]
    private bool _duellLaeuft;

    // ──── Duell-Übersicht (editierbar) ───────────────────────────────────────

    /// <summary>Editierbare Duelle der aktuellen Partie (Spieler und Trefferzahl korrigierbar).</summary>
    public ObservableCollection<DuellBearbeitenModel> BearbeitbareDuelle { get; } = [];

    // ──── Spielabschluss ─────────────────────────────────────────────────────

    /// <summary>Gibt an, ob das Spiel abgeschlossen werden kann.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SpielAbschliessenCommand))]
    private bool _kannSpielAbschliessen;

    /// <summary>Zusammenfassung für den Abschluss-Dialog.</summary>
    [ObservableProperty]
    private string _zusammenfassungsText = string.Empty;

    // ──── Kommandos: Duell-Start ─────────────────────────────────────────────

    /// <summary>Startet das nächste reguläre Duell.</summary>
    [RelayCommand(CanExecute = nameof(KannDuellStarten))]
    private void DuellStarten()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (_aktuellesSpiel is null || turnier is null ||
            GewaehlterSpieler1 is null || GewaehlterSpieler2 is null) return;

        _spielsteuerungService.NaechesDuellStarten(
            _aktuellesSpiel, GewaehlterSpieler1.Id, GewaehlterSpieler2.Id);

        SaveUndAktualisieren(turnier);
    }

    private bool KannDuellStarten() =>
        SpielLaeuft && !DuellLaeuft &&
        GewaehlterSpieler1 is not null && GewaehlterSpieler2 is not null &&
        !KannSpielAbschliessen && !IstStechen;

    /// <summary>Startet ein Stechen-Duell.</summary>
    [RelayCommand(CanExecute = nameof(KannStechenStarten))]
    private void StechenStarten()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (_aktuellesSpiel is null || turnier is null ||
            GewaehlterSpieler1 is null || GewaehlterSpieler2 is null) return;

        _spielsteuerungService.StechenStarten(
            _aktuellesSpiel, GewaehlterSpieler1.Id, GewaehlterSpieler2.Id);

        SaveUndAktualisieren(turnier);
    }

    private bool KannStechenStarten() =>
        SpielLaeuft && IstStechen && !DuellLaeuft &&
        GewaehlterSpieler1 is not null && GewaehlterSpieler2 is not null;

    // ──── Kommandos: Versuch-Buttons (2×2-Grid) ──────────────────────────────

    /// <summary>Erfasst: nur Team 1 trifft.</summary>
    [RelayCommand(CanExecute = nameof(KannVersuchErfassen))]
    private void Team1Trifft() => VersuchErfassen(true, false);

    /// <summary>Erfasst: nur Team 2 trifft.</summary>
    [RelayCommand(CanExecute = nameof(KannVersuchErfassen))]
    private void Team2Trifft() => VersuchErfassen(false, true);

    /// <summary>Erfasst: beide treffen.</summary>
    [RelayCommand(CanExecute = nameof(KannVersuchErfassen))]
    private void Beidetreffen() => VersuchErfassen(true, true);

    /// <summary>Erfasst: keiner trifft.</summary>
    [RelayCommand(CanExecute = nameof(KannVersuchErfassen))]
    private void BeideNichtTreffen() => VersuchErfassen(false, false);

    private bool KannVersuchErfassen() => SpielLaeuft && DuellLaeuft;

    private void VersuchErfassen(bool sp1Getroffen, bool sp2Getroffen)
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (_aktuellesSpiel is null || turnier is null) return;

        _spielsteuerungService.VersuchErfassen(_aktuellesSpiel, sp1Getroffen, sp2Getroffen);
        SaveUndAktualisieren(turnier);

        // Anzeigeoberfläche live aktualisieren
        _anzeigeZustand.SpielZustandMelden(_aktuellesSpiel, turnier);
    }

    // ──── Kommando: Spielabschluss ────────────────────────────────────────────

    /// <summary>Schließt die Partie ab (nach Bestätigungsdialog).</summary>
    [RelayCommand(CanExecute = nameof(KannSpielAbschliessen_Check))]
    private void SpielAbschliessen()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (_aktuellesSpiel is null || turnier is null) return;

        var ergebnis = MessageBox.Show(
            $"Partie abschließen?\n\n{ZusammenfassungsText}",
            "Spiel abschließen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (ergebnis != MessageBoxResult.Yes) return;

        var abgeschlossenesSpiel = _aktuellesSpiel;
        _spielsteuerungService.SpielAbschliessen(abgeschlossenesSpiel, turnier);

        // BracketFortsetzung: Sieger in die nächste Finalrundenrunde eintragen
        if (turnier.Finalrundenspiele.Contains(abgeschlossenesSpiel))
            _spielplanService.BracketFortsetzungAktualisieren(turnier, abgeschlossenesSpiel);

        _turnierService.TurnierSpeichern(turnier);

        // Auto-Backup nach Spielabschluss
        _backupManager.BackupErstellen(turnier);

        _turnierZustand.AenderungMelden();

        // Anzeigeoberfläche auf Infoscreen zurückschalten
        _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Infoscreen);

        // Gruppenphase ggf. automatisch abschließen und Finalrunde erzeugen
        GruppenphaseAutomatischAbschliessen(turnier);
    }

    /// <summary>
    /// Prüft nach einem Gruppenspiel, ob alle Gruppenspiele abgeschlossen sind.
    /// Falls ja und keine Platzierung ein Stechen erfordert, wird automatisch die
    /// Finalrunde generiert und der Turnierstatus auf „Finalrunde" gesetzt.
    /// </summary>
    private void GruppenphaseAutomatischAbschliessen(Turnier turnier)
    {
        if (turnier.Status != TurnierStatus.Gruppenphase) return;
        if (turnier.Finalrundenspiele.Count > 0) return; // bereits erzeugt

        var alleGruppenspiele = turnier.Gruppen.SelectMany(g => g.Spiele).ToList();
        if (alleGruppenspiele.Count == 0) return;

        var alleFertig = alleGruppenspiele.All(s =>
            s.Status is SpielStatus.Abgeschlossen or SpielStatus.Korrigiert or SpielStatus.Abgesetzt);
        if (!alleFertig) return;

        // Bei ungeklärten Platzierungen automatisch ein KO-Stechen erzeugen
        var stechenOffen = turnier.Gruppen.Any(g =>
            _wertungsService.GruppenRanglisteBerechnen(g, turnier.Wertungssystem)
                .Any(e => e.StehenErforderlich));
        if (stechenOffen)
        {
            var anzahl = _spielplanService.PlatzierungsStechenErzeugen(turnier);
            if (anzahl > 0)
            {
                _turnierService.TurnierSpeichern(turnier);
                _turnierZustand.AenderungMelden();
                MessageBox.Show(
                    $"Die Gruppenphase ist abgeschlossen, aber mindestens eine Platzierung ist gleichauf.\n" +
                    $"Es wurde(n) {anzahl} Stechen-Spiel(e) erstellt. Bitte austragen – " +
                    "danach wird die Finalrunde automatisch erstellt.",
                    "Stechen erforderlich", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return;
        }

        try
        {
            _spielplanService.FinalrundeGenerieren(turnier);
            _turnierService.StatusWechseln(turnier); // Gruppenphase → Finalrunde
            _turnierService.TurnierSpeichern(turnier);
            _turnierZustand.AenderungMelden();
            _anzeigeZustand.ZeigeScreen(AnzeigeScreen.Infoscreen);

            MessageBox.Show(
                "Die Gruppenphase ist abgeschlossen. Die Finalrunde wurde automatisch erstellt.",
                "Finalrunde erstellt", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Finalrunde konnte nicht automatisch erstellt werden:\n{ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool KannSpielAbschliessen_Check() => SpielLaeuft && KannSpielAbschliessen;

    // ──── Aktualisierung ─────────────────────────────────────────────────────

    /// <summary>
    /// Synchronisiert den Spielzustand mit dem Turnier-Objekt.
    /// Wird bei jeder Turnierändeurng aufgerufen.
    /// </summary>
    public void SpielAktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null)
        {
            _aktuellesSpiel = null;
            SpielLaeuft = false;
            return;
        }

        // Laufendes Spiel suchen
        _aktuellesSpiel = AlleSpiele(turnier).FirstOrDefault(s => s.Status == SpielStatus.Laeuft);
        SpielLaeuft = _aktuellesSpiel is not null;

        if (_aktuellesSpiel is null)
        {
            KannSpielAbschliessen = false;
            DuellLaeuft = false;
            BearbeitbareDuelle.Clear();
            AktuellesVersuchText = string.Empty;
            return;
        }

        Team1Name = TeamName(turnier, _aktuellesSpiel.Team1Id);
        Team2Name = TeamName(turnier, _aktuellesSpiel.Team2Id);

        DuellsiegeTeam1 = _aktuellesSpiel.Einzelduelle
            .Count(d => d.Ergebnis?.SiegerId == _aktuellesSpiel.Team1Id);
        DuellsiegeTeam2 = _aktuellesSpiel.Einzelduelle
            .Count(d => d.Ergebnis?.SiegerId == _aktuellesSpiel.Team2Id);

        IstStechen = _aktuellesSpiel.Einzelduelle.Any(d => d.IstStechen);

        var aktivesDuell = _aktuellesSpiel.Einzelduelle.FirstOrDefault(d => d.Ergebnis is null);
        DuellLaeuft = aktivesDuell is not null;

        // Versuch-Text
        if (aktivesDuell is not null)
            AktuellesVersuchText = $"Versuch {aktivesDuell.Versuche.Count + 1} von 3";
        else
            AktuellesVersuchText = string.Empty;

        // Stechen erkennen: 5 reguläre Duelle fertig, kein eindeutiger Sieger
        var regulaereAbgeschlossen = _aktuellesSpiel.Einzelduelle.Count(d => !d.IstStechen && d.Ergebnis is not null);
        var bereitsFuerStechen = regulaereAbgeschlossen >= 5 && DuellsiegeTeam1 == DuellsiegeTeam2;
        if (bereitsFuerStechen && !IstStechen)
            IstStechen = true;

        // Spielabschluss möglich: Ergebnis eindeutig (ein Team hat mehr Duelsiege nach 5 regulären)
        var regulaereGesamt = _aktuellesSpiel.Einzelduelle.Count(d => !d.IstStechen);
        var stechenAbgeschlossen = _aktuellesSpiel.Einzelduelle.Any(d => d.IstStechen && d.Ergebnis is not null);
        KannSpielAbschliessen = (regulaereGesamt >= 5 && DuellsiegeTeam1 != DuellsiegeTeam2 && aktivesDuell is null)
                               || stechenAbgeschlossen;

        // Teams auflösen – vor DuellUebersicht benötigt
        var team1 = turnier.Teams.FirstOrDefault(t => t.Id == _aktuellesSpiel.Team1Id);
        var team2 = turnier.Teams.FirstOrDefault(t => t.Id == _aktuellesSpiel.Team2Id);

        // Fehlende Spieler-Slots auf 5 auffüllen (auch bei Teilbefüllung)
        bool spielerErgaenzt = false;
        if (team1 is not null && team1.Spieler.Count < 5)
        {
            while (team1.Spieler.Count < 5)
                team1.Spieler.Add(new Spieler { Name = $"Spieler {team1.Spieler.Count + 1}" });
            spielerErgaenzt = true;
        }
        if (team2 is not null && team2.Spieler.Count < 5)
        {
            while (team2.Spieler.Count < 5)
                team2.Spieler.Add(new Spieler { Name = $"Spieler {team2.Spieler.Count + 1}" });
            spielerErgaenzt = true;
        }
        if (spielerErgaenzt)
            _turnierService.TurnierSpeichern(turnier);

        // ComboBoxen befüllen
        Team1Spieler.Clear();
        foreach (var sp in team1?.Spieler ?? [])
            Team1Spieler.Add(sp);

        Team2Spieler.Clear();
        foreach (var sp in team2?.Spieler ?? [])
            Team2Spieler.Add(sp);

        // Spieler für nächstes Duell vorschlagen (nur wenn kein Duell läuft)
        if (!DuellLaeuft && team1 is not null && team2 is not null)
        {
            var regulaereCount = _aktuellesSpiel.Einzelduelle.Count(d => !d.IstStechen);
            var idx = regulaereCount; // 0-basiert → Duell 1 = Spieler[0]
            GewaehlterSpieler1 = idx < team1.Spieler.Count ? team1.Spieler[idx] : null;
            GewaehlterSpieler2 = idx < team2.Spieler.Count ? team2.Spieler[idx] : null;
        }

        // Editierbare Duell-Liste: alle 5 regulären Slots (gespielt, aktiv oder geplant)
        BearbeitbareDuelle.Clear();
        var team1Spieler = team1?.Spieler.ToList() ?? [];
        var team2Spieler = team2?.Spieler.ToList() ?? [];
        var gespielteDuelle = _aktuellesSpiel.Einzelduelle
            .Where(d => !d.IstStechen)
            .ToDictionary(d => d.Duellnummer);

        for (int nr = 1; nr <= 5; nr++)
        {
            if (gespielteDuelle.TryGetValue(nr, out var gespielt))
                BearbeitbareDuelle.Add(DuellModellErstellen(gespielt, team1Spieler, team2Spieler, false));
            else
            {
                var idx = nr - 1;
                var sp1 = idx < team1Spieler.Count ? team1Spieler[idx].Name : $"Spieler {nr}";
                var sp2 = idx < team2Spieler.Count ? team2Spieler[idx].Name : $"Spieler {nr}";
                BearbeitbareDuelle.Add(new DuellBearbeitenModel
                {
                    Duellnummer = nr,
                    IstGeplant = true,
                    Spieler1Vorschau = sp1,
                    Spieler2Vorschau = sp2
                });
            }
        }

        // Stechen-Duelle anhängen (ebenfalls editierbar)
        foreach (var duell in _aktuellesSpiel.Einzelduelle.Where(d => d.IstStechen))
            BearbeitbareDuelle.Add(DuellModellErstellen(duell, team1Spieler, team2Spieler, true));

        // Zusammenfassung
        var siegerName = DuellsiegeTeam1 > DuellsiegeTeam2 ? Team1Name : Team2Name;
        ZusammenfassungsText = $"{Team1Name} {DuellsiegeTeam1} : {DuellsiegeTeam2} {Team2Name}\nSieger: {siegerName}";
    }

    /// <summary>Erstellt ein editierbares Modell aus einem vorhandenen Einzelduell.</summary>
    private DuellBearbeitenModel DuellModellErstellen(
        Einzelduell duell, List<Spieler> team1Spieler, List<Spieler> team2Spieler, bool istStechen)
    {
        var s1 = team1Spieler.FirstOrDefault(s => s.Id == duell.Spieler1Id);
        var s2 = team2Spieler.FirstOrDefault(s => s.Id == duell.Spieler2Id);
        var t1 = duell.Versuche.Count(v => v.Spieler1Getroffen);
        var t2 = duell.Versuche.Count(v => v.Spieler2Getroffen);

        var modell = new DuellBearbeitenModel
        {
            DuellId      = duell.Id,
            Duellnummer  = duell.Duellnummer,
            IstStechen   = istStechen,
            IstGeplant   = false,
            Team1Spieler = team1Spieler,
            Team2Spieler = team2Spieler
        };
        modell.Initialisieren(s1, s2, t1, t2, DuellBearbeitenAnwenden);
        return modell;
    }

    /// <summary>
    /// Übernimmt geänderte Spieler/Trefferzahlen eines Duells, rekonstruiert die Versuche
    /// und leitet Sieger und Duellpunkte aus der Trefferzahl ab.
    /// </summary>
    private void DuellBearbeitenAnwenden(DuellBearbeitenModel modell)
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (_aktuellesSpiel is null || turnier is null) return;

        var duell = _aktuellesSpiel.Einzelduelle.FirstOrDefault(d => d.Id == modell.DuellId);
        if (duell is null) return;

        if (modell.GewaehlterSpieler1 is not null) duell.Spieler1Id = modell.GewaehlterSpieler1.Id;
        if (modell.GewaehlterSpieler2 is not null) duell.Spieler2Id = modell.GewaehlterSpieler2.Id;

        int t1 = Math.Clamp(modell.Treffer1, 0, 3);
        int t2 = Math.Clamp(modell.Treffer2, 0, 3);

        // Versuche aus der Trefferzahl rekonstruieren (gemeinsame Treffer zuerst)
        duell.Versuche.Clear();
        int beide = Math.Min(t1, t2);
        for (int i = 0; i < beide; i++)
            duell.Versuche.Add(VersuchBauen(duell, true, true));
        for (int i = beide; i < t1; i++)
            duell.Versuche.Add(VersuchBauen(duell, true, false));
        for (int i = beide; i < t2; i++)
            duell.Versuche.Add(VersuchBauen(duell, false, true));

        // Sieger und Duellpunkte aus der Trefferzahl ableiten
        Guid? sieger = t1 > t2 ? _aktuellesSpiel.Team1Id
                     : t2 > t1 ? _aktuellesSpiel.Team2Id
                     : null;
        int dp1 = t1 > t2 ? 1 : (t1 == t2 && t1 > 0 ? 1 : 0);
        int dp2 = t2 > t1 ? 1 : (t1 == t2 && t2 > 0 ? 1 : 0);

        duell.Ergebnis = new EinzelduellErgebnis
        {
            SiegerId        = sieger,
            DuellpunktTeam1 = dp1,
            DuellpunktTeam2 = dp2
        };

        SaveUndAktualisieren(turnier);
        _anzeigeZustand.SpielZustandMelden(_aktuellesSpiel, turnier);
    }

    private static Versuch VersuchBauen(Einzelduell duell, bool s1, bool s2) => new()
    {
        EinzelduellId     = duell.Id,
        Versuchnummer     = duell.Versuche.Count + 1,
        Spieler1Getroffen = s1,
        Spieler2Getroffen = s2
    };

    // ──── Hilfsmethoden ─────────────────────────────────────────────────────

    private void SaveUndAktualisieren(Turnier turnier)
    {
        _turnierService.TurnierSpeichern(turnier);
        _turnierZustand.AenderungMelden();
    }

    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier) =>
        turnier.Gruppen.SelectMany(g => g.Spiele).Concat(turnier.Finalrundenspiele);

    private static string TeamName(Turnier turnier, Guid? teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "?";

    private static string SpielerName(Turnier turnier, Guid? teamId, Guid spielerId)
    {
        var team = turnier.Teams.FirstOrDefault(t => t.Id == teamId);
        return team?.Spieler.FirstOrDefault(s => s.Id == spielerId)?.Name ?? "?";
    }

    private static string DuellErgebnisText(Einzelduell duell, Spiel spiel)
    {
        if (duell.Versuche.Count == 0) return "–";
        // Tatsächliche Trefferzahl je Spieler anzeigen (z. B. 3:3, 2:1, 0:0)
        int treffer1 = duell.Versuche.Count(v => v.Spieler1Getroffen);
        int treffer2 = duell.Versuche.Count(v => v.Spieler2Getroffen);
        return $"{treffer1} : {treffer2}";
    }
}
