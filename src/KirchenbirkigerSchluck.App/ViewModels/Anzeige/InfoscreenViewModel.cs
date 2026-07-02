/*************************************************************
 * Datei:        InfoscreenViewModel.cs
 * Zweck:        ViewModel für den rotierenden Infoscreen zwischen Spielen
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Anzeige;

/// <summary>Kennzeichnet den aktuell sichtbaren Folien-Typ im Infoscreen.</summary>
public enum InfoFolienTyp
{
    /// <summary>Nächste anstehende Partie.</summary>
    NaechstePartie,

    /// <summary>Ergebnis der zuletzt abgeschlossenen Partie.</summary>
    LetztePartie,

    /// <summary>Gruppentabelle (eine pro Gruppe).</summary>
    GruppenTabelle,

    /// <summary>Spielplan-Übersicht (kommende Spiele).</summary>
    Spielplan,

    /// <summary>Finalrunden-Baum (Bracket).</summary>
    Bracket
}

/// <summary>Eine Zeile in der Spielplan-Folie des Infoscreens.</summary>
public sealed class InfoSpielZeile
{
    /// <summary>Spielnummer.</summary>
    public int Nummer { get; init; }

    /// <summary>Name von Team 1.</summary>
    public string Team1 { get; init; } = string.Empty;

    /// <summary>Name von Team 2.</summary>
    public string Team2 { get; init; } = string.Empty;

    /// <summary>Logo-Pfad von Team 1.</summary>
    public string? Team1LogoPfad { get; init; }

    /// <summary>Logo-Pfad von Team 2.</summary>
    public string? Team2LogoPfad { get; init; }

    /// <summary>Ergebnistext oder leer, falls noch nicht gespielt.</summary>
    public string Ergebnis { get; init; } = string.Empty;

    /// <summary>Runde/Gruppe als Text.</summary>
    public string Runde { get; init; } = string.Empty;
}

/// <summary>Ein positionierter Knoten (Spiel) im Finalrunden-Baum.</summary>
public sealed class BracketKnoten
{
    /// <summary>X-Position auf der Bracket-Leinwand.</summary>
    public double X { get; init; }

    /// <summary>Y-Position auf der Bracket-Leinwand.</summary>
    public double Y { get; init; }

    /// <summary>Breite der Karte.</summary>
    public double Breite { get; init; }

    /// <summary>Name von Team 1 (oder „—" wenn noch offen).</summary>
    public string Team1 { get; init; } = "—";

    /// <summary>Name von Team 2 (oder „—" wenn noch offen).</summary>
    public string Team2 { get; init; } = "—";

    /// <summary>Logo-Pfad von Team 1.</summary>
    public string? Team1LogoPfad { get; init; }

    /// <summary>Logo-Pfad von Team 2.</summary>
    public string? Team2LogoPfad { get; init; }

    /// <summary>Ergebnis als Text oder leer.</summary>
    public string Ergebnis { get; init; } = string.Empty;
}

/// <summary>Eine Verbindungslinie zwischen zwei Bracket-Knoten.</summary>
public sealed class BracketLinie
{
    /// <summary>Punkte der Polyline (Elbow-Verbinder).</summary>
    public System.Windows.Media.PointCollection Punkte { get; init; } = new();

    /// <summary>Gibt an, ob die Linie gestrichelt dargestellt wird (z. B. zum Spiel um Platz 3).</summary>
    public bool Gestrichelt { get; init; }
}

/// <summary>Beschriftung (Rundenname) im Bracket.</summary>
public sealed class BracketTitel
{
    /// <summary>X-Position der Beschriftung.</summary>
    public double X { get; init; }

    /// <summary>Y-Position der Beschriftung.</summary>
    public double Y { get; init; }

    /// <summary>Breite der Beschriftung.</summary>
    public double Breite { get; init; }

    /// <summary>Rundenname (z. B. „Halbfinale").</summary>
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Verwaltet den rotierenden Infoscreen mit nächster Partie, letztem Ergebnis
/// und Gruppentabellen. Die Rotation wird extern durch AnzeigeWindowViewModel gesteuert.
/// </summary>
public partial class InfoscreenViewModel : ObservableObject
{
    private readonly IWertungsService _wertungsService;
    private readonly TurnierZustandService _turnierZustand;
    private readonly InfoscreenEinstellungen _einstellungen;
    private readonly List<(InfoFolienTyp Typ, int GruppenIndex)> _folienReihenfolge = [];
    private int _folienIndex;

    /// <summary>Initialisiert das InfoscreenViewModel und abonniert Turnierändrungs-Events.</summary>
    public InfoscreenViewModel(
        IWertungsService wertungsService,
        TurnierZustandService turnierZustandService,
        InfoscreenEinstellungen einstellungen)
    {
        _wertungsService = wertungsService;
        _turnierZustand = turnierZustandService;
        _einstellungen = einstellungen;
        turnierZustandService.TurnierGeaendert += (_, _) => Aktualisieren();
        einstellungen.Geaendert += (_, _) => Aktualisieren();
        Aktualisieren();
    }

    // ──── Sichtbarkeitssteuerung der Folien ─────────────────────────────────

    /// <summary>Aktuell anzuzeigender Folien-Typ.</summary>
    [ObservableProperty]
    private InfoFolienTyp _aktuellerFolienTyp = InfoFolienTyp.NaechstePartie;

    /// <summary>Beschriftung des Infoscreen-Headers (z.B. „Nächste Partie").</summary>
    [ObservableProperty]
    private string _folienTitel = string.Empty;

    // ──── Daten: Nächste Partie ──────────────────────────────────────────────

    /// <summary>Name von Team 1 der nächsten Partie.</summary>
    [ObservableProperty]
    private string _naechstesTeam1 = string.Empty;

    /// <summary>Name von Team 2 der nächsten Partie.</summary>
    [ObservableProperty]
    private string _naechstesTeam2 = string.Empty;

    /// <summary>Logo-Pfad von Team 1 der nächsten Partie.</summary>
    [ObservableProperty]
    private string? _naechstesTeam1Logo;

    /// <summary>Logo-Pfad von Team 2 der nächsten Partie.</summary>
    [ObservableProperty]
    private string? _naechstesTeam2Logo;

    /// <summary>Kontextinfo zur nächsten Partie (z.B. „Gruppe A – Spiel 3").</summary>
    [ObservableProperty]
    private string _naechstesSpielKontext = string.Empty;

    /// <summary>Gibt an, ob eine nächste Partie vorhanden ist.</summary>
    [ObservableProperty]
    private bool _hatNaechstesSpiel;

    // ──── Daten: Letzte Partie ───────────────────────────────────────────────

    /// <summary>Name von Team 1 der letzten Partie.</summary>
    [ObservableProperty]
    private string _letztesTeam1 = string.Empty;

    /// <summary>Name von Team 2 der letzten Partie.</summary>
    [ObservableProperty]
    private string _letztesTeam2 = string.Empty;

    /// <summary>Logo-Pfad von Team 1 der letzten Partie.</summary>
    [ObservableProperty]
    private string? _letztesTeam1Logo;

    /// <summary>Logo-Pfad von Team 2 der letzten Partie.</summary>
    [ObservableProperty]
    private string? _letztesTeam2Logo;

    /// <summary>Ergebnis als Text (z.B. „3 : 2 Duelle").</summary>
    [ObservableProperty]
    private string _letztesErgebnis = string.Empty;

    /// <summary>Sieger-Name der letzten Partie.</summary>
    [ObservableProperty]
    private string _letzterSieger = string.Empty;

    /// <summary>Gibt an, ob eine letzte Partie mit Ergebnis vorhanden ist.</summary>
    [ObservableProperty]
    private bool _hatLetztesSpiel;

    // ──── Daten: Gruppentabelle ──────────────────────────────────────────────

    /// <summary>Name der aktuell angezeigten Gruppe.</summary>
    [ObservableProperty]
    private string _aktuelleGruppenName = string.Empty;

    /// <summary>Tabellenzeilen der aktuell angezeigten Gruppe.</summary>
    public ObservableCollection<GruppenTabellenEintragAnzeigeModel> AktuelleTabellenZeilen { get; } = [];

    /// <summary>Zeilen der Spielplan-Folie (kommende Spiele).</summary>
    public ObservableCollection<InfoSpielZeile> SpielplanZeilen { get; } = [];

    /// <summary>Positionierte Knoten (Spiele) des Finalrunden-Baums.</summary>
    public ObservableCollection<BracketKnoten> BracketKnoten { get; } = [];

    /// <summary>Verbindungslinien zwischen den Bracket-Knoten.</summary>
    public ObservableCollection<BracketLinie> BracketLinien { get; } = [];

    /// <summary>Spaltentitel (Rundennamen) des Brackets.</summary>
    public ObservableCollection<BracketTitel> BracketTitel { get; } = [];

    /// <summary>Gesamtbreite der Bracket-Leinwand.</summary>
    [ObservableProperty]
    private double _bracketBreite;

    /// <summary>Gesamthöhe der Bracket-Leinwand.</summary>
    [ObservableProperty]
    private double _bracketHoehe;

    // ──── Logik ─────────────────────────────────────────────────────────────

    /// <summary>Lädt alle Daten neu aus dem aktuellen Turnier und baut die Folienreihenfolge auf.</summary>
    public void Aktualisieren()
    {
        var turnier = _turnierZustand.AktuellesTurnier;
        _folienReihenfolge.Clear();

        if (turnier is null)
        {
            HatNaechstesSpiel = false;
            HatLetztesSpiel = false;
            FolienTitel = "Kein Turnier geladen";
            return;
        }

        // Nächste Partie ermitteln – nach Spielnummer sortiert, da der Turnierablauf
        // zwischen den Gruppen wechselt und AlleSpiele() gruppenweise konkateniert liefert.
        var naechstesSpiel = AlleSpiele(turnier)
            .OrderBy(s => s.Spielnummer)
            .FirstOrDefault(s => s.Status is Core.Enums.SpielStatus.Geplant or Core.Enums.SpielStatus.Vorbereitet or Core.Enums.SpielStatus.Verschoben);

        // In der Finalphase nur Nächste/Letzte Partie und Baum zeigen
        bool istFinale = turnier.Status is Core.Enums.TurnierStatus.Finalrunde
                                          or Core.Enums.TurnierStatus.Abgeschlossen;

        HatNaechstesSpiel = naechstesSpiel is not null;
        if (naechstesSpiel is not null)
        {
            NaechstesTeam1 = TeamName(turnier, naechstesSpiel.Team1Id) ?? "TBD";
            NaechstesTeam2 = TeamName(turnier, naechstesSpiel.Team2Id) ?? "TBD";
            NaechstesTeam1Logo = TeamLogo(turnier, naechstesSpiel.Team1Id);
            NaechstesTeam2Logo = TeamLogo(turnier, naechstesSpiel.Team2Id);
            NaechstesSpielKontext = SpielKontext(turnier, naechstesSpiel);
            if (_einstellungen.ZeigeNaechste)
                _folienReihenfolge.Add((InfoFolienTyp.NaechstePartie, 0));
        }

        // Letzte abgeschlossene Partie ermitteln – nach Spielnummer sortiert, damit das
        // gemäß Turnierablauf zuletzt gespielte Spiel (höchste Spielnummer) gewählt wird.
        var letztesSpiel = AlleSpiele(turnier)
            .OrderBy(s => s.Spielnummer)
            .LastOrDefault(s => s.Status is Core.Enums.SpielStatus.Abgeschlossen or Core.Enums.SpielStatus.Korrigiert
                             && s.Ergebnis is not null);

        HatLetztesSpiel = letztesSpiel?.Ergebnis is not null;
        if (letztesSpiel?.Ergebnis is { } ergebnis)
        {
            LetztesTeam1 = TeamName(turnier, letztesSpiel.Team1Id) ?? "?";
            LetztesTeam2 = TeamName(turnier, letztesSpiel.Team2Id) ?? "?";
            LetztesTeam1Logo = TeamLogo(turnier, letztesSpiel.Team1Id);
            LetztesTeam2Logo = TeamLogo(turnier, letztesSpiel.Team2Id);
            var s1 = ergebnis.DuellpunkteTeam1;
            var s2 = ergebnis.DuellpunkteTeam2;
            LetztesErgebnis = $"{s1} : {s2}";
            LetzterSieger = TeamName(turnier, ergebnis.SiegerId) ?? "?";
            if (_einstellungen.ZeigeLetzte)
                _folienReihenfolge.Add((InfoFolienTyp.LetztePartie, 0));
        }

        // Spielplan-Übersicht (kommende Spiele) – nur in der Gruppenphase
        SpielplanZeilenAufbauen(turnier);
        if (_einstellungen.ZeigeSpielplan && !istFinale && SpielplanZeilen.Count > 0)
            _folienReihenfolge.Add((InfoFolienTyp.Spielplan, 0));

        // Gruppenranglisten – nur in der Gruppenphase
        if (_einstellungen.ZeigeTabellen && !istFinale)
            for (var i = 0; i < turnier.Gruppen.Count; i++)
                _folienReihenfolge.Add((InfoFolienTyp.GruppenTabelle, i));

        // Finalrunden-Baum, sobald Finalrundenspiele existieren
        BracketAufbauen(turnier);
        if (_einstellungen.ZeigeBracket && turnier.Finalrundenspiele.Count > 0)
            _folienReihenfolge.Add((InfoFolienTyp.Bracket, 0));

        if (_folienReihenfolge.Count == 0)
            _folienReihenfolge.Add((InfoFolienTyp.NaechstePartie, 0));

        _folienIndex = 0;
        FolieAnzeigen(0);
    }

    /// <summary>Wechselt zur nächsten Folie (zyklisch).</summary>
    public void NaechsterSlide()
    {
        if (_folienReihenfolge.Count == 0) return;
        _folienIndex = (_folienIndex + 1) % _folienReihenfolge.Count;
        FolieAnzeigen(_folienIndex);
    }

    private void FolieAnzeigen(int index)
    {
        if (_folienReihenfolge.Count == 0 || index >= _folienReihenfolge.Count) return;

        var (typ, gruppenIndex) = _folienReihenfolge[index];
        AktuellerFolienTyp = typ;

        switch (typ)
        {
            case InfoFolienTyp.NaechstePartie:
                FolienTitel = "Nächste Partie";
                break;

            case InfoFolienTyp.LetztePartie:
                FolienTitel = "Letzte Partie";
                break;

            case InfoFolienTyp.GruppenTabelle:
                var turnier = _turnierZustand.AktuellesTurnier;
                if (turnier is null || gruppenIndex >= turnier.Gruppen.Count) return;
                var gruppe = turnier.Gruppen[gruppenIndex];
                FolienTitel = $"Tabelle – {gruppe.Name}";
                AktuelleGruppenName = gruppe.Name;
                AktuelleTabellenZeilen.Clear();
                foreach (var eintrag in _wertungsService.GruppenRanglisteBerechnen(gruppe, turnier.Wertungssystem))
                {
                    var diff = eintrag.DuellpunkteGewonnen - eintrag.DuellpunkteVerloren;
                    AktuelleTabellenZeilen.Add(new GruppenTabellenEintragAnzeigeModel
                    {
                        Platz = eintrag.Position,
                        TeamName = TeamName(turnier, eintrag.TeamId) ?? "?",
                        LogoPfad = TeamLogo(turnier, eintrag.TeamId),
                        Spiele = eintrag.Spiele,
                        Siege = eintrag.Siege,
                        DuelleGewonnen = eintrag.DuellpunkteGewonnen,
                        DuelleVerloren = eintrag.DuellpunkteVerloren,
                        DuellDifferenz = diff > 0 ? $"+{diff}" : diff.ToString(),
                        Punkte = eintrag.Tabellenpunkte,
                        StechenNoetig = eintrag.StehenErforderlich
                    });
                }
                break;

            case InfoFolienTyp.Spielplan:
                FolienTitel = "Spielplan";
                break;

            case InfoFolienTyp.Bracket:
                FolienTitel = "Finalrunde";
                break;
        }
    }

    // ──── Hilfsmethoden ─────────────────────────────────────────────────────

    private static string? TeamName(Turnier turnier, Guid? teamId) =>
        teamId is null ? null : turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.Name;

    private static string? TeamLogo(Turnier turnier, Guid? teamId) =>
        teamId is null ? null : turnier.Teams.FirstOrDefault(t => t.Id == teamId)?.LogoPfad;

    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier) =>
        turnier.Gruppen.SelectMany(g => g.Spiele).Concat(turnier.Finalrundenspiele);

    /// <summary>Baut die Spielplan-Folie: zeigt kommende und laufende Spiele (max. 12).</summary>
    private void SpielplanZeilenAufbauen(Turnier turnier)
    {
        SpielplanZeilen.Clear();

        var offene = AlleSpiele(turnier)
            .Where(s => s.Status is Core.Enums.SpielStatus.Geplant
                        or Core.Enums.SpielStatus.Vorbereitet
                        or Core.Enums.SpielStatus.Verschoben
                        or Core.Enums.SpielStatus.Laeuft)
            .OrderBy(s => s.Spielnummer)
            .Take(12);

        foreach (var spiel in offene)
        {
            var ergebnis = spiel.Ergebnis is not null
                ? $"{spiel.Ergebnis.DuellpunkteTeam1} : {spiel.Ergebnis.DuellpunkteTeam2}"
                : string.Empty;

            SpielplanZeilen.Add(new InfoSpielZeile
            {
                Nummer = spiel.Spielnummer,
                Team1 = TeamName(turnier, spiel.Team1Id) ?? "TBD",
                Team2 = TeamName(turnier, spiel.Team2Id) ?? "TBD",
                Team1LogoPfad = TeamLogo(turnier, spiel.Team1Id),
                Team2LogoPfad = TeamLogo(turnier, spiel.Team2Id),
                Ergebnis = ergebnis,
                Runde = SpielKontext(turnier, spiel)
            });
        }
    }

    // Layout-Konstanten des Brackets
    private const double KartenBreite = 260;
    private const double KartenHoehe = 84;
    private const double SpaltenAbstand = 360;
    private const double TitelHoehe = 56;
    private const double BaumHoehe = 760;

    /// <summary>
    /// Baut den Finalrunden-Baum als positionierte Knoten mit Elbow-Verbindungslinien.
    /// Jeder Knoten wird mit seinem Vorgänger verbunden, sodass der Weg durch den Baum sichtbar ist.
    /// </summary>
    private void BracketAufbauen(Turnier turnier)
    {
        BracketKnoten.Clear();
        BracketLinien.Clear();
        BracketTitel.Clear();

        if (turnier.Finalrundenspiele.Count == 0)
        {
            BracketBreite = 0;
            BracketHoehe = 0;
            return;
        }

        // Spiel um Platz 3 gesondert behandeln (kommt unter das Finale)
        var platz3Spiel = turnier.Finalrundenspiele.FirstOrDefault(s => s.BracketRunde == "Spiel um Platz 3");
        var hauptSpiele = turnier.Finalrundenspiele.Where(s => s != platz3Spiel).ToList();

        // Runden (Spalten) nach kleinster Spielnummer ordnen
        var runden = hauptSpiele
            .GroupBy(s => s.BracketRunde ?? "Finalrunde")
            .OrderBy(g => g.Min(s => s.Spielnummer))
            .ToList();

        var spielMitte = new Dictionary<Guid, (double X, double CenterY)>();

        for (int ri = 0; ri < runden.Count; ri++)
        {
            var spiele = runden[ri].OrderBy(s => s.Spielnummer).ToList();
            int n = spiele.Count;
            double x = ri * SpaltenAbstand;

            BracketTitel.Add(new BracketTitel { X = x, Y = 6, Breite = KartenBreite, Name = runden[ri].Key });

            for (int gi = 0; gi < n; gi++)
            {
                var spiel = spiele[gi];
                double centerY = TitelHoehe + (gi + 0.5) / n * BaumHoehe;
                BracketKnoten.Add(KnotenErstellen(turnier, spiel, x, centerY));
                spielMitte[spiel.Id] = (x, centerY);
            }
        }

        // Spiel um Platz 3 unter dem Finale platzieren
        if (platz3Spiel is not null)
        {
            var finaleSpiel = hauptSpiele
                .Where(s => (s.BracketRunde ?? "").Contains("Finale", StringComparison.OrdinalIgnoreCase))
                .MaxBy(s => s.Spielnummer);

            if (finaleSpiel is not null && spielMitte.TryGetValue(finaleSpiel.Id, out var finalePos))
            {
                double centerY = finalePos.CenterY + KartenHoehe + 130;
                BracketKnoten.Add(KnotenErstellen(turnier, platz3Spiel, finalePos.X, centerY));
                spielMitte[platz3Spiel.Id] = (finalePos.X, centerY);

                BracketTitel.Add(new BracketTitel
                {
                    X = finalePos.X,
                    Y = centerY - KartenHoehe / 2 - 34,
                    Breite = KartenBreite,
                    Name = "Spiel um Platz 3"
                });
            }
        }

        // Verbindungslinien: von jedem Vorgänger zur linken Kante des Folgespiels
        foreach (var spiel in turnier.Finalrundenspiele)
        {
            if (!spielMitte.TryGetValue(spiel.Id, out var ziel)) continue;
            bool gestrichelt = spiel.BracketRunde == "Spiel um Platz 3";

            foreach (var vorgId in new[] { spiel.VorgaengerSpiel1Id, spiel.VorgaengerSpiel2Id })
            {
                if (vorgId is null || !spielMitte.TryGetValue(vorgId.Value, out var quelle)) continue;

                double startX = quelle.X + KartenBreite;
                double mitteX = (startX + ziel.X) / 2;

                BracketLinien.Add(new BracketLinie
                {
                    Gestrichelt = gestrichelt,
                    Punkte = new System.Windows.Media.PointCollection
                    {
                        new System.Windows.Point(startX, quelle.CenterY),
                        new System.Windows.Point(mitteX, quelle.CenterY),
                        new System.Windows.Point(mitteX, ziel.CenterY),
                        new System.Windows.Point(ziel.X, ziel.CenterY)
                    }
                });
            }
        }

        BracketBreite = (runden.Count - 1) * SpaltenAbstand + KartenBreite;
        BracketHoehe = TitelHoehe + BaumHoehe;
    }

    /// <summary>Erstellt einen Bracket-Knoten aus einem Spiel an der gegebenen Position.</summary>
    private static BracketKnoten KnotenErstellen(Turnier turnier, Spiel spiel, double x, double centerY)
    {
        var ergebnis = spiel.Ergebnis is not null
            ? $"{spiel.Ergebnis.DuellpunkteTeam1} : {spiel.Ergebnis.DuellpunkteTeam2}"
            : string.Empty;

        return new BracketKnoten
        {
            X = x,
            Y = centerY - KartenHoehe / 2,
            Breite = KartenBreite,
            Team1 = TeamName(turnier, spiel.Team1Id) ?? "—",
            Team2 = TeamName(turnier, spiel.Team2Id) ?? "—",
            Team1LogoPfad = TeamLogo(turnier, spiel.Team1Id),
            Team2LogoPfad = TeamLogo(turnier, spiel.Team2Id),
            Ergebnis = ergebnis
        };
    }

    private static string SpielKontext(Turnier turnier, Spiel spiel)
    {
        if (spiel.GruppeId is not null)
        {
            var gruppe = turnier.Gruppen.FirstOrDefault(g => g.Id == spiel.GruppeId);
            return gruppe is not null ? $"{gruppe.Name} – Spiel {spiel.Spielnummer}" : $"Spiel {spiel.Spielnummer}";
        }
        return spiel.BracketRunde ?? $"Finalrunde – Spiel {spiel.Spielnummer}";
    }
}
