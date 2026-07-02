/*************************************************************
 * Datei:        GruppenAuslosungViewModel.cs
 * Zweck:        ViewModel für die animierte Gruppenauslosung und Einteilungsübersicht
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>Anzeigemodell einer Gruppe während der Auslosung und in der Übersicht.</summary>
public sealed partial class GruppenAnsichtModel : ObservableObject
{
    /// <summary>Anzeigename der Gruppe (z.B. „Gruppe A").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Namen der zugelosten Teams, in Ziehungsreihenfolge.</summary>
    public ObservableCollection<string> Teams { get; } = [];
}

/// <summary>
/// Steuert die animierte Auslosung der Gruppen (DFB-Pokal-Prinzip: abwechselnd ein Team
/// je Gruppe ziehen) und zeigt die resultierende Gruppeneinteilung.
/// </summary>
public partial class GruppenAuslosungViewModel : ObservableObject
{
    private readonly ITurnierService _turnierService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly AnzeigeZustandService _anzeigeZustand;

    // Festgelegte Ziehungsreihenfolge der laufenden Auslosung (für die spätere Einteilung).
    private List<Team> _gemischteTeams = [];

    /// <summary>Initialisiert das ViewModel und abonniert die relevanten Events.</summary>
    public GruppenAuslosungViewModel(
        ITurnierService turnierService,
        TurnierZustandService turnierZustandService,
        AnzeigeZustandService anzeigeZustandService)
    {
        _turnierService = turnierService;
        _turnierZustand = turnierZustandService;
        _anzeigeZustand = anzeigeZustandService;

        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();

        // Wenn die Beamer-Animation durchgelaufen ist, Einteilung festschreiben
        anzeigeZustandService.AuslosungAbgeschlossen += (_, _) => AuslosungFestschreiben();

        Aktualisieren();
    }

    // ──── Konfiguration ──────────────────────────────────────────────────────

    /// <summary>Mögliche Gruppenanzahlen.</summary>
    public IReadOnlyList<int> MoeglicheGruppenAnzahlen { get; } = [1, 2, 3, 4];

    /// <summary>Gewählte Anzahl der Gruppen.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuslosungStartenCommand))]
    private int _anzahlGruppen = 2;

    // ──── Zustand ────────────────────────────────────────────────────────────

    /// <summary>Gibt an, ob ein Turnier geladen ist.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuslosungStartenCommand))]
    private bool _hatGeladenesTurnier;

    /// <summary>Gibt an, ob das Turnier noch in der Vorbereitung ist (Auslosung möglich).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuslosungStartenCommand))]
    private bool _istInVorbereitung;

    /// <summary>Gibt an, ob bereits eine gültige Gruppeneinteilung existiert.</summary>
    [ObservableProperty]
    private bool _istAusgelost;

    /// <summary>Gibt an, ob die Ziehung gerade läuft.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuslosungStartenCommand))]
    private bool _ziehungLaeuft;

    /// <summary>Anzahl aktiver Teams.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AuslosungStartenCommand))]
    private int _teamAnzahl;

    /// <summary>Hinweistext zum aktuellen Zustand.</summary>
    [ObservableProperty]
    private string _statusText = string.Empty;

    /// <summary>Gruppen mit ihren (zugelosten) Teams.</summary>
    public ObservableCollection<GruppenAnsichtModel> Gruppen { get; } = [];

    // ──── Kommandos ──────────────────────────────────────────────────────────

    /// <summary>Startet die animierte Auslosung am Beamer (bzw. lost neu aus).</summary>
    [RelayCommand(CanExecute = nameof(KannAuslosen))]
    private void AuslosungStarten()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        var aktiveTeams = turnier.Teams
            .Where(t => t.Status != TeamStatus.Zurueckgezogen)
            .ToList();

        // Zufällige Ziehungsreihenfolge festlegen
        _gemischteTeams = aktiveTeams.OrderBy(_ => Random.Shared.Next()).ToList();

        var gruppennamen = Enumerable.Range(0, AnzahlGruppen)
            .Select(i => $"Gruppe {(char)('A' + i)}")
            .ToList();

        // Ziehungsreihenfolge für die Beamer-Animation aufbereiten (Team i → Gruppe i % n)
        var reihenfolge = new List<AuslosungEintrag>();
        for (var i = 0; i < _gemischteTeams.Count; i++)
        {
            var gi = i % AnzahlGruppen;
            reihenfolge.Add(new AuslosungEintrag(
                _gemischteTeams[i].Name, _gemischteTeams[i].LogoPfad, gi, gruppennamen[gi]));
        }

        // Leere Gruppenspalten für die Übersicht (füllen sich nach dem Festschreiben)
        Gruppen.Clear();
        foreach (var name in gruppennamen)
            Gruppen.Add(new GruppenAnsichtModel { Name = name });

        IstAusgelost = false;
        ZiehungLaeuft = true;
        StatusText = "Die Auslosung läuft am Beamer …";

        _anzeigeZustand.AuslosungAmBeamerStarten(new AuslosungDaten
        {
            AnzahlGruppen = AnzahlGruppen,
            Gruppennamen = gruppennamen,
            Reihenfolge = reihenfolge
        });
    }

    private bool KannAuslosen() =>
        HatGeladenesTurnier && IstInVorbereitung && !ZiehungLaeuft &&
        TeamAnzahl >= AnzahlGruppen * 2;

    /// <summary>
    /// Schreibt die ausgeloste Einteilung ins Turnier, sobald die Beamer-Animation
    /// vollständig durchgelaufen ist.
    /// </summary>
    private void AuslosungFestschreiben()
    {
        if (!ZiehungLaeuft) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        if (turnier is null) return;

        // Gruppen ins Datenmodell übertragen (Team i → Gruppe i % AnzahlGruppen)
        turnier.Gruppen.Clear();
        var neueGruppen = new List<Gruppe>();
        for (var i = 0; i < AnzahlGruppen; i++)
            neueGruppen.Add(new Gruppe { Name = $"Gruppe {(char)('A' + i)}", Nummer = i + 1 });

        for (var i = 0; i < _gemischteTeams.Count; i++)
            neueGruppen[i % AnzahlGruppen].TeamIds.Add(_gemischteTeams[i].Id);

        turnier.Gruppen.AddRange(neueGruppen);
        _turnierService.TurnierSpeichern(turnier);

        ZiehungLaeuft = false;
        IstAusgelost = true;
        StatusText = "Auslosung abgeschlossen. Gruppenphase kann im Tab „Turnier“ gestartet werden.";

        _turnierZustand.AenderungMelden();
    }

    // ──── Aktualisierung ─────────────────────────────────────────────────────

    /// <summary>Liest Teams und vorhandene Gruppen aus dem Turnier.</summary>
    private void Aktualisieren()
    {
        // Eine laufende Ziehung nicht stören
        if (ZiehungLaeuft) return;

        var turnier = _turnierZustand.AktuellesTurnier;
        HatGeladenesTurnier = turnier is not null;
        IstInVorbereitung = turnier?.Status == TurnierStatus.InVorbereitung;
        TeamAnzahl = turnier?.Teams.Count(t => t.Status != TeamStatus.Zurueckgezogen) ?? 0;

        Gruppen.Clear();

        if (turnier is null)
        {
            IstAusgelost = false;
            StatusText = "Kein Turnier geladen.";
            return;
        }

        var hatEinteilung = turnier.Gruppen.Any(g => g.TeamIds.Count > 0);
        IstAusgelost = hatEinteilung;

        if (hatEinteilung)
        {
            foreach (var gruppe in turnier.Gruppen)
            {
                var modell = new GruppenAnsichtModel { Name = gruppe.Name };
                foreach (var teamId in gruppe.TeamIds)
                    modell.Teams.Add(TeamName(turnier, teamId));
                Gruppen.Add(modell);
            }

            // AnzahlGruppen an bestehende Einteilung angleichen
            if (turnier.Gruppen.Count > 0)
                AnzahlGruppen = turnier.Gruppen.Count;
        }

        StatusText = StatusHinweis();
    }

    private string StatusHinweis()
    {
        if (!IstInVorbereitung && IstAusgelost)
            return "Das Turnier läuft – die Gruppeneinteilung ist festgelegt.";
        if (TeamAnzahl < AnzahlGruppen * 2)
            return $"Für {AnzahlGruppen} Gruppen werden mindestens {AnzahlGruppen * 2} Teams benötigt " +
                   $"(aktuell {TeamAnzahl}). Teams im Tab „Teamverwaltung“ anlegen.";
        if (IstAusgelost)
            return "Gruppen sind ausgelost. Du kannst neu auslosen oder im Tab „Turnier“ die Gruppenphase starten.";
        return "Bereit zur Auslosung.";
    }

    private static string TeamName(Turnier turnier, Guid teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "?";

    // Reagiert auf Änderung der Gruppenanzahl, um den Hinweistext zu aktualisieren.
    partial void OnAnzahlGruppenChanged(int value)
    {
        if (!ZiehungLaeuft)
            StatusText = StatusHinweis();
    }
}
