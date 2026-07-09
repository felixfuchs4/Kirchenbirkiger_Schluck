/*************************************************************
 * Datei:        GewinnerViewModel.cs
 * Zweck:        ViewModel für die schrittweise Siegerehrung am Turnierende
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Anzeige;

/// <summary>Ein einzelner Schritt der Siegerehrung (eine Person bzw. ein Team).</summary>
public sealed class SiegerehrungSchritt
{
    /// <summary>Gibt an, ob dieser Schritt einen Spieler (true) oder ein Team (false) ehrt.</summary>
    public bool IstSpieler { get; init; }

    /// <summary>Titel der Ehrungsphase (z. B. „Torschützenkönig" oder „Endplatzierung").</summary>
    public string PhasenTitel { get; init; } = string.Empty;

    /// <summary>Platzierung (1 = bester).</summary>
    public int Platz { get; init; }

    /// <summary>Name (Spieler- oder Teamname(n)); dient nur der Verwaltungs-Vorschau.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Spieler dieses Schritts – bei geteiltem Platz mehrere; leer bei Team-Schritten.</summary>
    public IReadOnlyList<SpielerZeileModel> Spieler { get; init; } = [];

    /// <summary>Teams dieses Schritts – bei geteiltem Platz mehrere; leer bei Spieler-Schritten.</summary>
    public IReadOnlyList<EndplatzierungZeileModel> Teams { get; init; } = [];

    /// <summary>Gibt an, ob dies der Sieger der Phase ist (besondere Hervorhebung).</summary>
    public bool IstSieger { get; init; }
}

/// <summary>
/// Steuert die Siegerehrung als Abfolge einzelner Schritte: zuerst die fünf treffsichersten
/// Spieler (von Platz 5 bis 1), danach die Teams (vom schlechtesten zum besten).
/// Die Weiterschaltung erfolgt manuell aus der Verwaltung.
/// </summary>
public partial class GewinnerViewModel : ObservableObject
{
    private readonly StatistikService _statistikService;
    private readonly TurnierZustandService _turnierZustand;

    private readonly List<SiegerehrungSchritt> _schritte = [];
    private int _schrittIndex;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public GewinnerViewModel(
        StatistikService statistikService,
        TurnierZustandService turnierZustandService)
    {
        _statistikService = statistikService;
        _turnierZustand = turnierZustandService;
        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();
        Aktualisieren();
    }

    /// <summary>Name des Turniers.</summary>
    [ObservableProperty]
    private string _turnierName = string.Empty;

    /// <summary>Aktuell angezeigter Ehrungsschritt.</summary>
    [ObservableProperty]
    private SiegerehrungSchritt? _aktuellerSchritt;

    /// <summary>Fortschrittstext (z. B. „Schritt 3 von 12").</summary>
    [ObservableProperty]
    private string _fortschritt = string.Empty;

    /// <summary>Gibt an, ob bereits ein Schritt angezeigt werden kann.</summary>
    [ObservableProperty]
    private bool _hatSchritte;

    /// <summary>Endplatzierungen aller Teams (für die abschließende Gesamtübersicht).</summary>
    public ObservableCollection<EndplatzierungZeileModel> Platzierungen { get; } = [];

    // ──── Steuerung (aus der Verwaltung) ──────────────────────────────────────

    /// <summary>Startet die Siegerehrung von vorne (erster Schritt).</summary>
    public void Starten()
    {
        _schrittIndex = 0;
        SchrittAnzeigen();
    }

    /// <summary>Schaltet zum nächsten Schritt weiter.</summary>
    public void Weiter()
    {
        if (_schrittIndex < _schritte.Count - 1)
            _schrittIndex++;
        SchrittAnzeigen();
    }

    /// <summary>Geht einen Schritt zurück.</summary>
    public void Zurueck()
    {
        if (_schrittIndex > 0)
            _schrittIndex--;
        SchrittAnzeigen();
    }

    /// <summary>Text des aktuell ausgewählten Schritts (für die Vorschau in der Verwaltung).</summary>
    public string VorschauText =>
        AktuellerSchritt is null
            ? "Keine Siegerehrung verfügbar"
            : $"{AktuellerSchritt.PhasenTitel} · {AktuellerSchritt.Platz}. Platz: {AktuellerSchritt.Name}";

    // ──── Aufbau ──────────────────────────────────────────────────────────────

    /// <summary>Baut die Ehrungsschritte neu aus dem aktuellen Turnier auf.</summary>
    public void Aktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        _schritte.Clear();
        Platzierungen.Clear();

        if (turnier is null)
        {
            TurnierName = string.Empty;
            HatSchritte = false;
            AktuellerSchritt = null;
            return;
        }

        TurnierName = turnier.Anlass;

        PlatzierungenBerechnen(turnier);
        SchritteAufbauen(turnier);

        HatSchritte = _schritte.Count > 0;
        _schrittIndex = Math.Clamp(_schrittIndex, 0, Math.Max(0, _schritte.Count - 1));
        SchrittAnzeigen();
    }

    private void SchrittAnzeigen()
    {
        if (_schritte.Count == 0)
        {
            AktuellerSchritt = null;
            Fortschritt = string.Empty;
        }
        else
        {
            AktuellerSchritt = _schritte[_schrittIndex];
            Fortschritt = $"Schritt {_schrittIndex + 1} von {_schritte.Count}";
        }
        OnPropertyChanged(nameof(VorschauText));
    }

    private void SchritteAufbauen(Turnier turnier)
    {
        // Phase 1: Treffsicherste Spieler – nach Platz gruppiert (geteilte Plätze gemeinsam,
        // außer Platz 1 – dort entscheidet ein Stechen über die Alleinstellung), die 5 besten
        // Plätze, von hinten nach vorne
        var rangliste = _statistikService.TorschuetzenRangliste(turnier);
        var plaetze = rangliste.GroupBy(s => s.Platz).OrderBy(g => g.Key).Take(5).ToList();
        for (int i = plaetze.Count - 1; i >= 0; i--)
        {
            var gruppe = plaetze[i];
            var spieler = gruppe.Select(s => new SpielerZeileModel
            {
                Name     = s.Name,
                TeamName = s.TeamName,
                LogoPfad = s.LogoPfad,
                Detail   = turnier.TorschuetzenWertung == TorschuetzenWertung.Prozentual
                    ? $"{s.Quote * 100:0}% – {s.Treffer} von {s.Versuche} Versuchen"
                    : $"{s.Treffer} Treffer"
            }).ToList();

            _schritte.Add(new SiegerehrungSchritt
            {
                IstSpieler  = true,
                PhasenTitel = "Treffsicherster Spieler",
                Platz       = gruppe.Key,
                Name        = string.Join(", ", spieler.Select(s => s.Name)), // nur für die Verwaltungs-Vorschau
                Spieler     = spieler,
                IstSieger   = gruppe.Key == 1
            });
        }

        // Phase 2: Teams – nach Platz gruppiert (geteilte Plätze gemeinsam), vom schlechtesten zum besten
        foreach (var gruppe in Platzierungen.GroupBy(p => p.Platz).OrderByDescending(g => g.Key))
        {
            var teams = gruppe.ToList();
            _schritte.Add(new SiegerehrungSchritt
            {
                IstSpieler  = false,
                PhasenTitel = "Endplatzierung",
                Platz       = gruppe.Key,
                Name        = string.Join(", ", teams.Select(t => t.TeamName)), // nur für die Verwaltungs-Vorschau
                Teams       = teams,
                IstSieger   = gruppe.Key == 1
            });
        }
    }

    private void PlatzierungenBerechnen(Turnier turnier)
    {
        var punkte = GruppenPunkte(turnier);
        var finals = turnier.Finalrundenspiele;

        Dictionary<Guid, int> platzVon;

        if (finals.Count == 0)
            // Keine Finalrunde: nach Gruppenpunkten
            platzVon = NachPunkten(turnier, punkte);
        else if (finals.Any(s => (s.BracketRunde ?? "").StartsWith("Platz ", StringComparison.Ordinal)))
            // Kurze Finalrunde: jedes „Platz X/Y"-Spiel bestimmt direkt die Plätze
            platzVon = NachPlatzSpielen(turnier, finals, punkte);
        else
            // KO-Baum: Platzierung nach Abschneiden im Turnierbaum (geteilte Plätze möglich)
            platzVon = NachBracket(turnier, finals);

        foreach (var team in turnier.Teams
                     .OrderBy(t => platzVon.GetValueOrDefault(t.Id, int.MaxValue))
                     .ThenBy(t => t.Name))
        {
            int platz = platzVon.GetValueOrDefault(team.Id, turnier.Teams.Count);
            Platzierungen.Add(new EndplatzierungZeileModel
            {
                Platz = platz,
                TeamName = team.Name,
                LogoPfad = team.LogoPfad,
                Punkte = punkte.GetValueOrDefault(team.Id),
                IstSieger = platz == 1
            });
        }
    }

    /// <summary>Summiert die Tabellenpunkte je Team aus den Gruppenspielen (nur informativ).</summary>
    private static Dictionary<Guid, int> GruppenPunkte(Turnier turnier)
    {
        var punkte = new Dictionary<Guid, int>();
        foreach (var team in turnier.Teams)
        {
            punkte[team.Id] = turnier.Gruppen
                .SelectMany(g => g.Spiele)
                .Where(s => s.Ergebnis is not null)
                .Sum(s => s.Team1Id == team.Id ? s.Ergebnis!.TabellenPunkteTeam1
                        : s.Team2Id == team.Id ? s.Ergebnis!.TabellenPunkteTeam2
                        : 0);
        }
        return punkte;
    }

    /// <summary>Fallback ohne Finalrunde: Reihenfolge rein nach Gruppenpunkten.</summary>
    private static Dictionary<Guid, int> NachPunkten(Turnier turnier, Dictionary<Guid, int> punkte)
    {
        var platzVon = new Dictionary<Guid, int>();
        int platz = 1;
        foreach (var team in turnier.Teams.OrderByDescending(t => punkte.GetValueOrDefault(t.Id)))
            platzVon[team.Id] = platz++;
        return platzVon;
    }

    /// <summary>Kurze Finalrunde: „Platz X/Y"-Spiele bestimmen die Endplätze direkt.</summary>
    private static Dictionary<Guid, int> NachPlatzSpielen(
        Turnier turnier, List<Spiel> finals, Dictionary<Guid, int> punkte)
    {
        var platzVon = new Dictionary<Guid, int>();

        foreach (var spiel in finals.Where(s => (s.BracketRunde ?? "").StartsWith("Platz ", StringComparison.Ordinal)))
        {
            // BracketRunde-Format: „Platz 1/2"
            var teil = (spiel.BracketRunde ?? "").Replace("Platz ", "").Split('/');
            if (teil.Length != 2 || !int.TryParse(teil[0], out var oben) || !int.TryParse(teil[1], out var unten))
                continue;

            if (spiel.Ergebnis is not null)
            {
                var sieger = spiel.Ergebnis.SiegerId;
                var verlierer = spiel.Team1Id == sieger ? spiel.Team2Id : spiel.Team1Id;
                if (sieger != Guid.Empty) platzVon[sieger] = oben;
                if (verlierer is { } v) platzVon[v] = unten;
            }
        }

        // Übrige Teams nach Gruppenpunkten hinten anhängen
        int rest = (platzVon.Count == 0 ? 0 : platzVon.Values.Max()) + 1;
        foreach (var team in turnier.Teams
                     .Where(t => !platzVon.ContainsKey(t.Id))
                     .OrderByDescending(t => punkte.GetValueOrDefault(t.Id)))
            platzVon[team.Id] = rest++;

        return platzVon;
    }

    /// <summary>
    /// KO-Baum-Platzierung: Plätze 1–4 aus Finale und Spiel um Platz 3, danach geteilte Plätze
    /// je nach Ausscheide-Runde (je weiter im Baum, desto besser).
    /// </summary>
    private static Dictionary<Guid, int> NachBracket(Turnier turnier, List<Spiel> finals)
    {
        // Rundenreihenfolge (Index) nach kleinster Spielnummer
        var rundenIndex = finals
            .GroupBy(s => s.BracketRunde ?? "Finalrunde")
            .OrderBy(g => g.Min(s => s.Spielnummer))
            .Select((g, i) => (g.Key, i))
            .ToDictionary(x => x.Key, x => x.i);

        var finale = finals
            .Where(s => (s.BracketRunde ?? "").Contains("Finale", StringComparison.OrdinalIgnoreCase))
            .MaxBy(s => s.Spielnummer);
        var platz3 = finals.FirstOrDefault(s => s.BracketRunde == "Spiel um Platz 3");

        var platzVon = new Dictionary<Guid, int>();
        int naechster = 1;

        // Plätze 1/2 aus dem Finale
        if (finale?.Ergebnis is not null)
        {
            var sieger = finale.Ergebnis.SiegerId;
            var verlierer = finale.Team1Id == sieger ? finale.Team2Id : finale.Team1Id;
            platzVon[sieger] = 1;
            if (verlierer is { } v) platzVon[v] = 2;
            naechster = 3;
        }

        // Plätze 3/4 aus dem Spiel um Platz 3
        if (platz3?.Ergebnis is not null)
        {
            var sieger = platz3.Ergebnis.SiegerId;
            var verlierer = platz3.Team1Id == sieger ? platz3.Team2Id : platz3.Team1Id;
            platzVon[sieger] = 3;
            if (verlierer is { } v) platzVon[v] = 4;
            naechster = Math.Max(naechster, 5);
        }

        // Teilnehmer des Finals/Platz-3 von der Tiefen-Gruppierung ausnehmen
        var finalVier = new HashSet<Guid>();
        foreach (var s in new[] { finale, platz3 })
        {
            if (s?.Team1Id is { } t1) finalVier.Add(t1);
            if (s?.Team2Id is { } t2) finalVier.Add(t2);
        }

        // Tiefe je Team = größter Rundenindex (ohne Finale/Platz-3), in dem das Team auftritt
        var tiefe = new Dictionary<Guid, int>();
        foreach (var spiel in finals)
        {
            if (spiel == finale || spiel == platz3) continue;
            int idx = rundenIndex[spiel.BracketRunde ?? "Finalrunde"];
            foreach (var tid in new[] { spiel.Team1Id, spiel.Team2Id })
                if (tid is { } t)
                    tiefe[t] = Math.Max(tiefe.GetValueOrDefault(t, -1), idx);
        }

        // Übrige Teams nach Ausscheide-Tiefe gruppieren (tiefer = besser), geteilte Plätze
        var rest = turnier.Teams.Where(t => !platzVon.ContainsKey(t.Id)).ToList();
        var gruppen = rest
            .GroupBy(t => finalVier.Contains(t.Id) ? int.MaxValue : tiefe.GetValueOrDefault(t.Id, -1))
            .OrderByDescending(g => g.Key);

        foreach (var gruppe in gruppen)
        {
            foreach (var team in gruppe)
                platzVon[team.Id] = naechster;
            naechster += gruppe.Count();
        }

        return platzVon;
    }
}
