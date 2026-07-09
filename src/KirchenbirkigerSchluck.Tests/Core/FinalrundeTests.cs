/*************************************************************
 * Datei:        FinalrundeTests.cs
 * Zweck:        Unit-Tests für FinalrundeGenerieren und BracketFortsetzungAktualisieren
 * Bereich:      Tests – Spielplanung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.Tests.Core;

/// <summary>
/// Tests für <see cref="SpielplanService.FinalrundeGenerieren"/> und
/// <see cref="SpielplanService.BracketFortsetzungAktualisieren"/>.
/// </summary>
public class FinalrundeTests
{
    private readonly SpielplanService _sut = new();

    // ---------------------------------------------------------------------------
    // Kurze Finalrunde
    // ---------------------------------------------------------------------------

    [Fact]
    public void Kurz_ZweiGruppen_ErzeugtKorrektePaarungen()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.Kurz);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: A1 vs B1, A2 vs B2, A3 vs B3 … (Paarung je Platzierung, reihenfolge-unabhängig)
        turnier.Finalrundenspiele.Should().HaveCount(6);
        var g1Teams = turnier.Gruppen[0].TeamIds;
        var g2Teams = turnier.Gruppen[1].TeamIds;

        for (int i = 0; i < 6; i++)
        {
            var spiel = turnier.Finalrundenspiele.Single(s => s.BracketRunde == $"Platz {i * 2 + 1}/{i * 2 + 2}");
            spiel.Team1Id.Should().Be(g1Teams[i]);
            spiel.Team2Id.Should().Be(g2Teams[i]);
        }
    }

    [Fact]
    public void Kurz_SpielUmPlatzEins_WirdZuletztGespielt()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.Kurz);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: Das Spiel um Platz 1/2 hat die höchste Spielnummer aller Finalrundenspiele
        var platzEins = turnier.Finalrundenspiele.Single(s => s.BracketRunde == "Platz 1/2");
        var maxNummer = turnier.Finalrundenspiele.Max(s => s.Spielnummer);
        platzEins.Spielnummer.Should().Be(maxNummer);
    }

    [Fact]
    public void Kurz_AnzahlSpieleEntsprichtKleinstenGruppe()
    {
        // Arrange: Gruppe A hat 4 Teams, Gruppe B hat 6 Teams
        var turnier = TurnierMitZweiGruppenBauen(4, 6, FinalrundenModus.Kurz);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: nur 4 Spiele (min der beiden Gruppengrößen)
        turnier.Finalrundenspiele.Should().HaveCount(4);
    }

    // ---------------------------------------------------------------------------
    // KO-Baum: Achtelfinale
    // ---------------------------------------------------------------------------

    [Fact]
    public void KoBaum_ZweiGruppenJe6_ErzeugtVierAchtelfinaleSpiele()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: 4 Achtelfinale-Spiele (A3 vs B6, A4 vs B5, B3 vs A6, B4 vs A5)
        var achtel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Achtelfinale").ToList();
        achtel.Should().HaveCount(4);

        // Alle Achtelfinale-Spiele haben bekannte Team-IDs (keine Platzhalter)
        achtel.Should().OnlyContain(s => s.Team1Id.HasValue && s.Team2Id.HasValue);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe6_ErzeugtViertelfinaleAlsPlatzhalter()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: 4 Viertelfinale-Spiele mit mindestens einer null-Team-Id
        var viertel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Viertelfinale").ToList();
        viertel.Should().HaveCount(4);

        // Jedes VF-Spiel hat mindestens einen TBD-Slot (der Vorgänger-Spielsieger)
        viertel.Should().OnlyContain(s => s.Team1Id == null || s.Team2Id == null);

        // Jedes VF-Spiel verweist auf ein Vorgänger-Spiel
        viertel.Should().OnlyContain(s => s.VorgaengerSpiel1Id.HasValue || s.VorgaengerSpiel2Id.HasValue);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe6_KoBaumEin_ErzeugtFinale()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: genau 1 Finale
        turnier.Finalrundenspiele.Count(s => s.BracketRunde == "Finale").Should().Be(1);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe5_ErzeugtZweiAchtelfinaleSpiele()
    {
        // Arrange: Gruppe A hat 5 Teams → nur B5 als Füller (B6 fehlt)
        var turnier = TurnierMitZweiGruppenBauen(5, 5, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: nur 2 Achtelfinale (A3 vs B5, B3 vs A5 — A4/B6 und B4/A6 fehlen)
        var achtel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Achtelfinale").ToList();
        achtel.Should().HaveCount(2);
    }

    [Fact]
    public void KoBaum_KoBaumEin_ErzeugtSpielUmPlatz3()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: genau 1 Spiel um Platz 3, das den Verlierer der Vorgänger übernimmt
        var platz3 = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Spiel um Platz 3").ToList();
        platz3.Should().HaveCount(1);
        platz3[0].VorgaengerVerlierer.Should().BeTrue();
        platz3[0].VorgaengerSpiel1Id.Should().NotBeNull();
        platz3[0].VorgaengerSpiel2Id.Should().NotBeNull();
    }

    [Fact]
    public void BracketFortsetzung_SpielUmPlatz3_TraegtVerliererEin()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);
        _sut.FinalrundeGenerieren(turnier);

        var halbfinale = turnier.Finalrundenspiele.First(s => s.BracketRunde == "Halbfinale");
        halbfinale.Team1Id = Guid.NewGuid();
        halbfinale.Team2Id = Guid.NewGuid();
        var verliererId = halbfinale.Team2Id!.Value;

        halbfinale.Status   = SpielStatus.Abgeschlossen;
        halbfinale.Ergebnis = new SpielErgebnis { SiegerId = halbfinale.Team1Id!.Value };

        // Act
        _sut.BracketFortsetzungAktualisieren(turnier, halbfinale);

        // Assert: Im Spiel um Platz 3 steht der Verlierer, im Finale der Sieger
        var platz3 = turnier.Finalrundenspiele.First(s => s.BracketRunde == "Spiel um Platz 3");
        var finale = turnier.Finalrundenspiele.First(s => s.BracketRunde == "Finale");

        bool verliererEingetragen =
            (platz3.VorgaengerSpiel1Id == halbfinale.Id && platz3.Team1Id == verliererId) ||
            (platz3.VorgaengerSpiel2Id == halbfinale.Id && platz3.Team2Id == verliererId);
        verliererEingetragen.Should().BeTrue("Der Halbfinal-Verlierer muss ins Spiel um Platz 3.");

        bool siegerImFinale =
            (finale.VorgaengerSpiel1Id == halbfinale.Id && finale.Team1Id == halbfinale.Ergebnis.SiegerId) ||
            (finale.VorgaengerSpiel2Id == halbfinale.Id && finale.Team2Id == halbfinale.Ergebnis.SiegerId);
        siegerImFinale.Should().BeTrue("Der Halbfinal-Sieger muss ins Finale.");
    }

    // ---------------------------------------------------------------------------
    // BracketFortsetzungAktualisieren
    // ---------------------------------------------------------------------------

    [Fact]
    public void BracketFortsetzung_TraegtSiegerInNaechsteRunde()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);
        _sut.FinalrundeGenerieren(turnier);

        var achtelSpiel = turnier.Finalrundenspiele.First(s => s.BracketRunde == "Achtelfinale");
        var siegerId    = achtelSpiel.Team1Id!.Value;

        // Abschlussergebnis simulieren
        achtelSpiel.Status   = SpielStatus.Abgeschlossen;
        achtelSpiel.Ergebnis = new SpielErgebnis { SiegerId = siegerId };

        // Act
        _sut.BracketFortsetzungAktualisieren(turnier, achtelSpiel);

        // Assert: das VF-Spiel, das dieses Achtelfinale als Vorgänger hat,
        //         bekommt jetzt den Sieger als Team-Id eingetragen
        var vfSpiel = turnier.Finalrundenspiele.First(s =>
            s.VorgaengerSpiel1Id == achtelSpiel.Id || s.VorgaengerSpiel2Id == achtelSpiel.Id);

        bool siegerId1 = vfSpiel.VorgaengerSpiel1Id == achtelSpiel.Id && vfSpiel.Team1Id == siegerId;
        bool siegerId2 = vfSpiel.VorgaengerSpiel2Id == achtelSpiel.Id && vfSpiel.Team2Id == siegerId;
        (siegerId1 || siegerId2).Should().BeTrue("Der Sieger muss in das VF-Spiel übernommen worden sein.");
    }

    // ---------------------------------------------------------------------------
    // KO-Baum: einheitlicher generischer Builder für jede Gruppenanzahl
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData(6)]                 // 1 Gruppe
    [InlineData(3)]                 // 1 Gruppe, ungerade Teamzahl → Freilose
    [InlineData(3, 3)]              // 2 Gruppen à 3 (früher toter Slot!)
    [InlineData(4, 4)]              // 2 Gruppen à 4
    [InlineData(5, 4)]              // 2 Gruppen, ungleich/ungerade gesamt
    [InlineData(6, 6)]             // 2 Gruppen à 6 (Standardfall)
    [InlineData(6, 6, 6)]           // 3 Gruppen
    [InlineData(4, 4, 4)]           // 3 Gruppen, kleiner
    [InlineData(5, 6, 4)]           // 3 Gruppen, unterschiedlich groß
    [InlineData(6, 6, 6, 6)]        // 4 Gruppen
    [InlineData(3, 3, 3, 3)]        // 4 Gruppen, klein
    public void KoBaum_ErzeugtLueckenlosesBracket(params int[] gruppenGroessen)
    {
        // Arrange
        var turnier = TurnierMitNGruppenBauen(FinalrundenModus.KoBaumEin, gruppenGroessen);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier, new Random(12345));

        // Assert
        var finale = turnier.Finalrundenspiele;
        int teamAnzahl = gruppenGroessen.Sum();

        // 1) Keine toten Slots: jede Seite hat entweder ein Team oder einen Vorgänger.
        finale.Should().OnlyContain(s =>
            (s.Team1Id.HasValue || s.VorgaengerSpiel1Id.HasValue) &&
            (s.Team2Id.HasValue || s.VorgaengerSpiel2Id.HasValue),
            "kein Bracket-Slot darf ohne Team und ohne Vorgängerspiel bleiben");

        // 2) Jedes Team ist genau einmal Startteilnehmer (direkt gesetzt); alle Gruppen vertreten.
        var starter = finale
            .SelectMany(s => new[] { s.Team1Id, s.Team2Id })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
        starter.Should().HaveCount(teamAnzahl);
        starter.Should().OnlyHaveUniqueItems();
        starter.Should().BeEquivalentTo(turnier.Teams.Select(t => t.Id));

        // 3) Genau ein Finale.
        finale.Count(s => s.BracketRunde == "Finale").Should().Be(1);

        // 4) Spiel um Platz 3 genau dann, wenn es zwei echte Halbfinal-Spiele gibt.
        int halbfinals = finale.Count(s => s.BracketRunde == "Halbfinale");
        int platz3 = finale.Count(s => s.BracketRunde == "Spiel um Platz 3");
        platz3.Should().Be(halbfinals == 2 ? 1 : 0);

        // 5) Anzahl Spiele = (Teams − 1) KO-Spiele + optionales Spiel um Platz 3.
        finale.Should().HaveCount(teamAnzahl - 1 + platz3);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(3, 3)]              // 2 Gruppen à 3 (früher toter Slot!)
    [InlineData(4, 4)]
    [InlineData(5, 4)]
    [InlineData(6, 6)]
    [InlineData(6, 6, 6)]
    [InlineData(4, 4, 4)]
    [InlineData(6, 6, 6, 6)]
    [InlineData(3, 3, 3, 3)]
    public void KoBaum_IstVollstaendigDurchspielbar(params int[] gruppenGroessen)
    {
        // Arrange
        var turnier = TurnierMitNGruppenBauen(FinalrundenModus.KoBaumEin, gruppenGroessen);
        GruppenspielplanAbschliessen(turnier);
        _sut.FinalrundeGenerieren(turnier, new Random(999));

        // Act: Bracket komplett durchspielen (jeweils Team1 gewinnt)
        FinalrundeDurchspielen(turnier);

        // Assert: kein Spiel bleibt hängen – wären Slots tot, käme ein Spiel nie zu zwei Teams.
        turnier.Finalrundenspiele.Should().OnlyContain(s => s.Status == SpielStatus.Abgeschlossen,
            "ein lückenloses Bracket muss vollständig spielbar sein");
    }

    [Theory]
    [InlineData(4, 4)]              // 2 Gruppen à 4 (8er-Bracket, keine Freilose)
    [InlineData(8, 8)]              // 2 Gruppen à 8 (16er-Bracket)
    [InlineData(4, 4, 4, 4)]        // 4 Gruppen à 4 (16er-Bracket)
    [InlineData(2, 2, 2, 2)]        // 4 Gruppen à 2 (8er-Bracket)
    public void KoBaum_GruppenWerdenDurchmischt_KeineErsteRundeGleicherGruppe(params int[] gruppenGroessen)
    {
        // Arrange
        var turnier = TurnierMitNGruppenBauen(FinalrundenModus.KoBaumEin, gruppenGroessen);
        GruppenspielplanAbschliessen(turnier);

        // Gruppenzugehörigkeit je Team
        var gruppeVon = new Dictionary<Guid, int>();
        for (int gi = 0; gi < turnier.Gruppen.Count; gi++)
            foreach (var id in turnier.Gruppen[gi].TeamIds)
                gruppeVon[id] = gi;

        // Über mehrere Auslosungen prüfen: in der ersten Runde tritt nie Gruppe gegen sich selbst an.
        for (int seed = 0; seed < 20; seed++)
        {
            _sut.FinalrundeGenerieren(turnier, new Random(seed));

            string ersteRunde = RundenNameFuer(NaechsteZweierpotenz(gruppenGroessen.Sum()));
            var ersteRundenSpiele = turnier.Finalrundenspiele
                .Where(s => s.BracketRunde == ersteRunde && s.Team1Id.HasValue && s.Team2Id.HasValue);

            foreach (var spiel in ersteRundenSpiele)
                gruppeVon[spiel.Team1Id!.Value].Should().NotBe(gruppeVon[spiel.Team2Id!.Value],
                    $"Erste-Runde-Spiele sollen Gruppen durchmischen (Seed {seed})");
        }
    }

    [Theory]
    [InlineData(2, 2, 2)]           // 3 Gruppen, T=6 → N=8, 2 Freilose
    [InlineData(6, 6, 6)]           // 3 Gruppen, T=18 → N=32, 14 Freilose
    [InlineData(3, 3, 3, 3)]        // 4 Gruppen, T=12 → N=16, 4 Freilose
    public void KoBaum_Freilose_GehenAnDieBestplatziertenTeams(params int[] gruppenGroessen)
    {
        // Arrange
        var turnier = TurnierMitNGruppenBauen(FinalrundenModus.KoBaumEin, gruppenGroessen);
        GruppenspielplanAbschliessen(turnier);
        _sut.FinalrundeGenerieren(turnier, new Random(7));

        // Platzierungs-Ebene je Team (0 = Gruppensieger) aus den Gruppen-Ranglisten
        var wertung = new WertungsService();
        var ebeneVon = new Dictionary<Guid, int>();
        foreach (var gruppe in turnier.Gruppen)
        {
            var rang = wertung.GruppenRanglisteBerechnen(gruppe, turnier.Wertungssystem).ToList();
            for (int i = 0; i < rang.Count; i++)
                ebeneVon[rang[i].TeamId] = i;
        }

        // Freilos-Teams = Teams, die nicht in der ersten Runde (Runde der Bracketgröße) starten
        int bracketGroesse = NaechsteZweierpotenz(gruppenGroessen.Sum());
        string ersteRunde = RundenNameFuer(bracketGroesse);
        var ersteRundenTeams = turnier.Finalrundenspiele
            .Where(s => s.BracketRunde == ersteRunde)
            .SelectMany(s => new[] { s.Team1Id, s.Team2Id })
            .Where(id => id.HasValue).Select(id => id!.Value)
            .ToHashSet();

        var freilosTeams = turnier.Teams.Select(t => t.Id).Where(id => !ersteRundenTeams.Contains(id)).ToList();
        var nichtFreilos = turnier.Teams.Select(t => t.Id).Where(ersteRundenTeams.Contains).ToList();

        // Assert: kein Freilos-Team ist schlechter platziert als ein Team, das die erste Runde spielt.
        freilosTeams.Should().NotBeEmpty();
        int schlechtesteFreilosEbene = freilosTeams.Max(id => ebeneVon[id]);
        int besteSpielEbene = nichtFreilos.Min(id => ebeneVon[id]);
        schlechtesteFreilosEbene.Should().BeLessThanOrEqualTo(besteSpielEbene,
            "Freilose gehen zuerst an die bestplatzierten Teams");
    }

    [Fact]
    public void KoBaum_EineGruppeDreiTeams_ErzeugtEinHalbfinaleUndFinaleOhnePlatz3()
    {
        // Arrange: 1 Gruppe mit 3 Teams → Bester bekommt Freilos ins Finale
        var turnier = TurnierMitNGruppenBauen(FinalrundenModus.KoBaumEin, 3);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier, new Random(1));

        // Assert: 1 Halbfinale + 1 Finale, kein Spiel um Platz 3 (nur ein Halbfinal-Verlierer)
        var finale = turnier.Finalrundenspiele;
        finale.Count(s => s.BracketRunde == "Halbfinale").Should().Be(1);
        finale.Count(s => s.BracketRunde == "Finale").Should().Be(1);
        finale.Count(s => s.BracketRunde == "Spiel um Platz 3").Should().Be(0);
        finale.Should().HaveCount(2);
    }

    // ---------------------------------------------------------------------------
    // Hilfsmethoden
    // ---------------------------------------------------------------------------

    /// <summary>Kleinste Zweierpotenz ≥ <paramref name="wert"/> (Spiegel der Service-Logik für Tests).</summary>
    private static int NaechsteZweierpotenz(int wert)
    {
        int n = 1;
        while (n < wert) n <<= 1;
        return n;
    }

    /// <summary>Rundenname für eine Runde mit <paramref name="teilnehmer"/> Teilnehmern (für Tests).</summary>
    private static string RundenNameFuer(int teilnehmer) => teilnehmer switch
    {
        2  => "Finale",
        4  => "Halbfinale",
        8  => "Viertelfinale",
        16 => "Achtelfinale",
        32 => "Sechzehntelfinale",
        64 => "Zweiunddreißigstelfinale",
        _  => $"Runde der letzten {teilnehmer}"
    };

    /// <summary>Spielt alle Finalrundenspiele durch (jeweils Team 1 gewinnt) und schaltet das Bracket weiter.</summary>
    private void FinalrundeDurchspielen(Turnier turnier)
    {
        while (true)
        {
            var spiel = turnier.Finalrundenspiele.FirstOrDefault(s =>
                s.Status != SpielStatus.Abgeschlossen && s.Team1Id.HasValue && s.Team2Id.HasValue);
            if (spiel is null) break;

            spiel.Status   = SpielStatus.Abgeschlossen;
            spiel.Ergebnis = new SpielErgebnis { SiegerId = spiel.Team1Id!.Value };
            _sut.BracketFortsetzungAktualisieren(turnier, spiel);
        }
    }

    /// <summary>
    /// Erstellt ein Turnier mit beliebig vielen Gruppen der angegebenen Größen, ohne Gruppenspiele.
    /// </summary>
    private static Turnier TurnierMitNGruppenBauen(FinalrundenModus modus, params int[] gruppenGroessen)
    {
        var turnier = new Turnier
        {
            Wertungssystem   = Wertungssystem.Einfach,
            FinalrundenModus = modus
        };

        for (int g = 0; g < gruppenGroessen.Length; g++)
        {
            char buchstabe = (char)('A' + g);
            var gruppe = new Gruppe { Name = buchstabe.ToString(), Nummer = g + 1 };
            for (int i = 0; i < gruppenGroessen[g]; i++)
            {
                var id = Guid.NewGuid();
                turnier.Teams.Add(new Team { Id = id, Name = $"{buchstabe}{i + 1}" });
                gruppe.TeamIds.Add(id);
            }
            turnier.Gruppen.Add(gruppe);
        }

        return turnier;
    }

    /// <summary>
    /// Erstellt ein Turnier mit zwei Gruppen und je <paramref name="aCount"/> / <paramref name="bCount"/>
    /// Teams, ohne Gruppenspiele zu generieren.
    /// </summary>
    private static Turnier TurnierMitZweiGruppenBauen(int aCount, int bCount, FinalrundenModus modus)
    {
        var turnier = new Turnier
        {
            Wertungssystem  = Wertungssystem.Einfach,
            FinalrundenModus = modus
        };

        var gruppeA = new Gruppe { Name = "A", Nummer = 1 };
        for (int i = 0; i < aCount; i++)
        {
            var id = Guid.NewGuid();
            turnier.Teams.Add(new Team { Id = id, Name = $"A{i + 1}" });
            gruppeA.TeamIds.Add(id);
        }

        var gruppeB = new Gruppe { Name = "B", Nummer = 2 };
        for (int i = 0; i < bCount; i++)
        {
            var id = Guid.NewGuid();
            turnier.Teams.Add(new Team { Id = id, Name = $"B{i + 1}" });
            gruppeB.TeamIds.Add(id);
        }

        turnier.Gruppen.AddRange([gruppeA, gruppeB]);
        return turnier;
    }

    /// <summary>
    /// Generiert den Gruppenspielplan und schließt alle Spiele ab, damit die Rangliste berechnet
    /// werden kann. Team1 gewinnt jedes Spiel (reicht für Ranking-Eindeutigkeit nicht, daher
    /// werden Spiele mit aufsteigender Spielnummer ausgewertet).
    /// </summary>
    private void GruppenspielplanAbschliessen(Turnier turnier)
    {
        _sut.GruppenspielplanGenerieren(turnier);

        foreach (var gruppe in turnier.Gruppen)
        {
            foreach (var spiel in gruppe.Spiele)
            {
                spiel.Status   = SpielStatus.Abgeschlossen;
                var siegerId   = spiel.Team1Id!.Value;
                spiel.Ergebnis = new SpielErgebnis
                {
                    SiegerId           = siegerId,
                    DuellpunkteTeam1   = 3,
                    DuellpunkteTeam2   = 0,
                    EntschiedenDurch   = EntscheidungsArt.RegulaereSpielzeit,
                    TabellenPunkteTeam1 = 1,
                    TabellenPunkteTeam2 = 0
                };
            }
        }
    }
}
