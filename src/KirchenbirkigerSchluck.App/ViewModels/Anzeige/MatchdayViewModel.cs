/*************************************************************
 * Datei:        MatchdayViewModel.cs
 * Zweck:        ViewModel für den Live-Matchday-Screen der Anzeigeoberfläche
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Anzeige;

/// <summary>
/// Verwaltet die Live-Anzeige einer laufenden Partie mit Spielstand,
/// Duell-Übersicht und aktuellem Versuch.
/// Wird bei jedem Ereignis aus dem Bedienungs-Layer aktualisiert.
/// </summary>
public partial class MatchdayViewModel : ObservableObject
{
    /// <summary>Name von Team 1 der aktuellen Partie.</summary>
    [ObservableProperty]
    private string _team1Name = string.Empty;

    /// <summary>Name von Team 2 der aktuellen Partie.</summary>
    [ObservableProperty]
    private string _team2Name = string.Empty;

    /// <summary>Logo-Pfad von Team 1 (optional).</summary>
    [ObservableProperty]
    private string? _team1LogoPfad;

    /// <summary>Logo-Pfad von Team 2 (optional).</summary>
    [ObservableProperty]
    private string? _team2LogoPfad;

    /// <summary>Anzahl der gewonnenen Einzelduelle von Team 1.</summary>
    [ObservableProperty]
    private int _duellsiegeTeam1;

    /// <summary>Anzahl der gewonnenen Einzelduelle von Team 2.</summary>
    [ObservableProperty]
    private int _duellsiegeTeam2;

    /// <summary>Gibt an, ob sich die Partie im Stechen befindet.</summary>
    [ObservableProperty]
    private bool _istStechen;

    /// <summary>Text zum aktuellen Versuch (z.B. „Versuch 2 von 3").</summary>
    [ObservableProperty]
    private string _aktuellesVersuchText = string.Empty;

    /// <summary>Gibt an, ob gerade eine Partie läuft.</summary>
    [ObservableProperty]
    private bool _spielLaeuft;

    /// <summary>Duellzeilen der aktuellen Partie (abgeschlossene + aktives Duell).</summary>
    public ObservableCollection<DuellZeileModel> Duelle { get; } = [];

    /// <summary>
    /// Aktualisiert alle Anzeigedaten anhand des übergebenen Spiels und Turniers.
    /// </summary>
    /// <param name="spiel">Das laufende oder abgeschlossene Spiel.</param>
    /// <param name="turnier">Das übergeordnete Turnier (für Teamnamen und Spieler).</param>
    public void SpielAktualisieren(Spiel spiel, Turnier turnier)
    {
        SpielLaeuft = spiel.Status == Core.Enums.SpielStatus.Laeuft;

        Team1Name = TeamName(turnier, spiel.Team1Id);
        Team2Name = TeamName(turnier, spiel.Team2Id);
        Team1LogoPfad = TeamLogo(turnier, spiel.Team1Id);
        Team2LogoPfad = TeamLogo(turnier, spiel.Team2Id);

        // Duellsiege zählen (Sieger = Team-Id im Ergebnis)
        DuellsiegeTeam1 = spiel.Einzelduelle
            .Count(d => d.Ergebnis?.SiegerId == spiel.Team1Id);
        DuellsiegeTeam2 = spiel.Einzelduelle
            .Count(d => d.Ergebnis?.SiegerId == spiel.Team2Id);

        IstStechen = spiel.Einzelduelle.Any(d => d.IstStechen);

        // Alle 5 regulären Slots anzeigen – gespielte mit Ergebnis, noch nicht gestartete als Vorschau
        var team1 = turnier.Teams.FirstOrDefault(t => t.Id == spiel.Team1Id);
        var team2 = turnier.Teams.FirstOrDefault(t => t.Id == spiel.Team2Id);
        var gespielteDuelle = spiel.Einzelduelle
            .Where(d => !d.IstStechen)
            .ToDictionary(d => d.Duellnummer);

        Duelle.Clear();
        for (var nr = 1; nr <= 5; nr++)
        {
            if (gespielteDuelle.TryGetValue(nr, out var gespielt))
            {
                Duelle.Add(new DuellZeileModel
                {
                    Duellnummer   = gespielt.Duellnummer,
                    Spieler1Name  = SpielerName(turnier, spiel.Team1Id, gespielt.Spieler1Id),
                    Spieler2Name  = SpielerName(turnier, spiel.Team2Id, gespielt.Spieler2Id),
                    ErgebnisText  = ErgebnisText(spiel, gespielt),
                    IstAktiv      = gespielt.Ergebnis is null,
                    IstStechen    = false,
                    IstGeplant    = false
                });
            }
            else
            {
                var idx = nr - 1;
                var sp1 = VorschauName(team1, spiel.Spieler1Reihenfolge, idx, nr);
                var sp2 = VorschauName(team2, spiel.Spieler2Reihenfolge, idx, nr);
                Duelle.Add(new DuellZeileModel
                {
                    Duellnummer   = nr,
                    Spieler1Name  = sp1,
                    Spieler2Name  = sp2,
                    ErgebnisText  = "–",
                    IstAktiv      = false,
                    IstStechen    = false,
                    IstGeplant    = true
                });
            }
        }

        // Stechen-Duelle separat anhängen
        foreach (var duell in spiel.Einzelduelle.Where(d => d.IstStechen))
        {
            Duelle.Add(new DuellZeileModel
            {
                Duellnummer   = duell.Duellnummer,
                Spieler1Name  = SpielerName(turnier, spiel.Team1Id, duell.Spieler1Id),
                Spieler2Name  = SpielerName(turnier, spiel.Team2Id, duell.Spieler2Id),
                ErgebnisText  = ErgebnisText(spiel, duell),
                IstAktiv      = duell.Ergebnis is null,
                IstStechen    = true,
                IstGeplant    = false
            });
        }

        // Versuch-Anzeige für das aktive Duell
        var aktivesDuell = spiel.Einzelduelle.FirstOrDefault(d => d.Ergebnis is null);
        if (aktivesDuell is not null)
        {
            var versuchNr = aktivesDuell.Versuche.Count + 1;
            AktuellesVersuchText = $"Versuch {versuchNr} von 3";
        }
        else
        {
            AktuellesVersuchText = string.Empty;
        }
    }

    /// <summary>Setzt den Screen zurück (wenn kein Spiel aktiv ist).</summary>
    public void Zuruecksetzen()
    {
        SpielLaeuft = false;
        Team1Name = string.Empty;
        Team2Name = string.Empty;
        Team1LogoPfad = null;
        Team2LogoPfad = null;
        DuellsiegeTeam1 = 0;
        DuellsiegeTeam2 = 0;
        IstStechen = false;
        AktuellesVersuchText = string.Empty;
        Duelle.Clear();
    }

    // ──── Hilfsmethoden ─────────────────────────────────────────────────────

    /// <summary>
    /// Name des Spielers an Position <paramref name="index"/> gemäß ausgeloster Reihenfolge (für die
    /// Vorschau noch nicht gestarteter Duelle); Fallback auf feste Aufstellung bzw. Platzhaltertext.
    /// </summary>
    private static string VorschauName(Team? team, IReadOnlyList<Guid> reihenfolge, int index, int nr)
    {
        if (team is not null && index < reihenfolge.Count)
        {
            var s = team.Spieler.FirstOrDefault(x => x.Id == reihenfolge[index]);
            if (s is not null) return s.Name;
        }
        return team is not null && index < team.Spieler.Count ? team.Spieler[index].Name : $"Spieler {nr}";
    }

    private static string TeamName(Turnier turnier, Guid? teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? "?";

    private static string? TeamLogo(Turnier turnier, Guid? teamId) =>
        turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.LogoPfad;

    private static string SpielerName(Turnier turnier, Guid? teamId, Guid spielerId)
    {
        var team = turnier.Teams.FirstOrDefault(t => t.Id == teamId);
        return team?.Spieler.FirstOrDefault(s => s.Id == spielerId)?.Name ?? "?";
    }

    private static string ErgebnisText(Spiel spiel, Einzelduell duell)
    {
        if (duell.Versuche.Count == 0) return "–";
        // Tatsächliche Trefferzahl je Spieler (z. B. 3:3, 2:1, 0:0)
        var treffen1 = duell.Versuche.Count(v => v.Spieler1Getroffen);
        var treffen2 = duell.Versuche.Count(v => v.Spieler2Getroffen);
        return $"{treffen1} : {treffen2}";
    }
}
