/*************************************************************
 * Datei:        StatistikServiceTests.cs
 * Zweck:        Unit-Tests für die Torschützen-Rangliste
 * Bereich:      Tests – Statistik
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.Tests.Core;

/// <summary>Testet die Torschützen-Rangliste des <see cref="StatistikService"/>.</summary>
public class StatistikServiceTests
{
    private readonly StatistikService _sut = new();

    /// <summary>
    /// Spieler A: 3 Treffer / 4 Versuche (75 %), Spieler B: 4 Treffer / 10 Versuche (40 %).
    /// Absolut führt B, prozentual führt A.
    /// </summary>
    [Fact]
    public void TorschuetzenRangliste_AbsolutVsProzentual_LiefertUnterschiedlicheReihenfolge()
    {
        // Arrange – vier Spieler, A und B spielen getrennte Duelle
        var spielerA = new Spieler { Name = "A" };
        var spielerB = new Spieler { Name = "B" };
        var spielerC = new Spieler { Name = "C" };
        var spielerD = new Spieler { Name = "D" };
        var team = new Team { Name = "T", Spieler = [spielerA, spielerB, spielerC, spielerD] };

        // A: 3 Treffer / 4 Versuche (gegen C, 0 Treffer)
        var duellA = new Einzelduell
        {
            Spieler1Id = spielerA.Id,
            Spieler2Id = spielerC.Id,
            Versuche =
            [
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false }
            ]
        };
        // B: 4 Treffer / 10 Versuche (gegen D, 0 Treffer)
        var duellB = new Einzelduell
        {
            Spieler1Id = spielerB.Id,
            Spieler2Id = spielerD.Id,
            Versuche =
            [
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false }
            ]
        };

        var spiel = new Spiel { Einzelduelle = [duellA, duellB] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        var turnier = new Turnier { Teams = [team], Gruppen = [gruppe] };

        // A: 3 Treffer / 4 Versuche; B: 4 Treffer / 10 Versuche
        // Act + Assert – Absolut
        turnier.TorschuetzenWertung = TorschuetzenWertung.Absolut;
        var absolut = _sut.TorschuetzenRangliste(turnier);
        absolut[0].Name.Should().Be("B");
        absolut.Single(s => s.Name == "A").Treffer.Should().Be(3);
        absolut.Single(s => s.Name == "B").Treffer.Should().Be(4);

        // Act + Assert – Prozentual
        turnier.TorschuetzenWertung = TorschuetzenWertung.Prozentual;
        var prozentual = _sut.TorschuetzenRangliste(turnier);
        prozentual[0].Name.Should().Be("A");
        prozentual.Single(s => s.Name == "A").Quote.Should().BeApproximately(0.75, 0.001);
        prozentual.Single(s => s.Name == "B").Quote.Should().BeApproximately(0.40, 0.001);
    }

    /// <summary>Erzeugt ein Turnier mit drei Spielern, die je 3 von 5 Versuchen treffen (60 %).</summary>
    private static Turnier DreiGleichaufSpielerTurnier(out Spieler a, out Spieler b, out Spieler c)
    {
        a = new Spieler { Name = "A" };
        b = new Spieler { Name = "B" };
        c = new Spieler { Name = "C" };
        var gegner = new Spieler { Name = "Gegner" };
        var team = new Team { Name = "T", Spieler = [a, b, c, gegner] };

        Einzelduell Duell(Guid spielerId) => new()
        {
            Spieler1Id = spielerId,
            Spieler2Id = gegner.Id,
            Versuche =
            [
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = true,  Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false },
                new Versuch { Spieler1Getroffen = false, Spieler2Getroffen = false }
            ]
        };

        var spiel = new Spiel { Einzelduelle = [Duell(a.Id), Duell(b.Id), Duell(c.Id)] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        return new Turnier { Teams = [team], Gruppen = [gruppe], TorschuetzenWertung = TorschuetzenWertung.Prozentual };
    }

    /// <summary>
    /// Drei Spieler mit exakt 60 % Trefferquote teilen sich Platz 1 – ohne hinterlegtes
    /// Stechen-Ergebnis müssen alle drei Platz 1 tragen und ein Stechen ist offen.
    /// </summary>
    [Fact]
    public void TorschuetzenRangliste_GleichstandPlatz1OhneStechen_AlleTeilenSichPlatz1()
    {
        var turnier = DreiGleichaufSpielerTurnier(out _, out _, out _);

        var rangliste = _sut.TorschuetzenRangliste(turnier);

        rangliste.Where(s => s.Name is "A" or "B" or "C").Should().OnlyContain(s => s.Platz == 1);
        _sut.GleichstandPlatz1(turnier).Should().HaveCount(3);
        _sut.StechenPlatz1Offen(turnier).Should().BeTrue();
    }

    /// <summary>
    /// Ist ein Stechen-Sieger hinterlegt, erhält dieser allein Platz 1, die übrigen
    /// gleichauf liegenden Spieler teilen sich Platz 2.
    /// </summary>
    [Fact]
    public void TorschuetzenRangliste_GleichstandPlatz1MitStechenSieger_SiegerAlleinAufPlatz1()
    {
        var turnier = DreiGleichaufSpielerTurnier(out var a, out var b, out var c);
        turnier.TorschuetzenStechenSiegerId = a.Id;

        var rangliste = _sut.TorschuetzenRangliste(turnier);

        rangliste.Single(s => s.SpielerId == a.Id).Platz.Should().Be(1);
        rangliste.Where(s => s.SpielerId == b.Id || s.SpielerId == c.Id).Should().OnlyContain(s => s.Platz == 2);
        _sut.StechenPlatz1Offen(turnier).Should().BeFalse();
    }

    /// <summary>
    /// Ein Gleichstand außerhalb von Platz 1 (z. B. Platz 2) benötigt kein Stechen; die
    /// betroffenen Spieler teilen sich einfach den Platz, der nachfolgende Platz springt weiter.
    /// </summary>
    [Fact]
    public void TorschuetzenRangliste_GleichstandNichtAufPlatz1_TeiltSichPlatzOhneStechen()
    {
        var spielerA = new Spieler { Name = "A" };
        var spielerB = new Spieler { Name = "B" };
        var spielerC = new Spieler { Name = "C" };
        var gegner = new Spieler { Name = "Gegner" };
        var team = new Team { Name = "T", Spieler = [spielerA, spielerB, spielerC, gegner] };

        // A: alleiniger Spitzenreiter mit 4 Treffern
        var duellA = new Einzelduell
        {
            Spieler1Id = spielerA.Id,
            Spieler2Id = gegner.Id,
            Versuche =
            [
                new Versuch { Spieler1Getroffen = true }, new Versuch { Spieler1Getroffen = true },
                new Versuch { Spieler1Getroffen = true }, new Versuch { Spieler1Getroffen = true }
            ]
        };
        // B und C: je 2 Treffer – teilen sich Platz 2
        Einzelduell ZweiTreffer(Guid spielerId) => new()
        {
            Spieler1Id = spielerId,
            Spieler2Id = gegner.Id,
            Versuche =
            [
                new Versuch { Spieler1Getroffen = true }, new Versuch { Spieler1Getroffen = true },
                new Versuch { Spieler1Getroffen = false }
            ]
        };

        var spiel = new Spiel { Einzelduelle = [duellA, ZweiTreffer(spielerB.Id), ZweiTreffer(spielerC.Id)] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        var turnier = new Turnier { Teams = [team], Gruppen = [gruppe], TorschuetzenWertung = TorschuetzenWertung.Absolut };

        var rangliste = _sut.TorschuetzenRangliste(turnier);

        rangliste.Single(s => s.Name == "A").Platz.Should().Be(1);
        rangliste.Where(s => s.Name is "B" or "C").Should().OnlyContain(s => s.Platz == 2);
        _sut.GleichstandPlatz1(turnier).Should().BeEmpty();
        _sut.StechenPlatz1Offen(turnier).Should().BeFalse();
    }

    /// <summary>
    /// AlleSpielerRangliste muss – im Gegensatz zur Torschützen-Rangliste – auch Spieler ohne
    /// jeglichen Versuch enthalten, damit die Statistik-Ansicht vor Turnierbeginn nutzbar ist.
    /// </summary>
    [Fact]
    public void AlleSpielerRangliste_SpielerOhneVersuche_WirdMitAufgenommen()
    {
        var spielerA = new Spieler { Name = "A" };
        var spielerOhneVersuch = new Spieler { Name = "Ohne Versuch" };
        var gegner = new Spieler { Name = "Gegner" };
        var team = new Team { Name = "T", Spieler = [spielerA, spielerOhneVersuch, gegner] };

        var duellA = new Einzelduell
        {
            Spieler1Id = spielerA.Id,
            Spieler2Id = gegner.Id,
            Versuche = [new Versuch { Spieler1Getroffen = true, Spieler2Getroffen = false }]
        };

        var spiel = new Spiel { Einzelduelle = [duellA] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        var turnier = new Turnier { Teams = [team], Gruppen = [gruppe] };

        // Torschützen-Rangliste (Zeremonie) schließt Spieler ohne Versuche weiterhin aus
        _sut.TorschuetzenRangliste(turnier).Should().NotContain(s => s.Name == "Ohne Versuch");

        // AlleSpielerRangliste enthält ihn mit 0/0
        var alle = _sut.AlleSpielerRangliste(turnier);
        var ohneVersuch = alle.Single(s => s.Name == "Ohne Versuch");
        ohneVersuch.Treffer.Should().Be(0);
        ohneVersuch.Versuche.Should().Be(0);
        ohneVersuch.Quote.Should().Be(0);
        ohneVersuch.Platz.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Spiele/Siege/Unentschieden/Niederlagen eines Spielers werden aus dem Ergebnis seiner
    /// eigenen Einzelduelle abgeleitet (Vergleich von <c>Ergebnis.SiegerId</c> mit der Team-Id,
    /// für die der Spieler antritt) – unabhängig vom Team-Gesamtergebnis der Partie.
    /// </summary>
    [Fact]
    public void AlleSpielerRangliste_SiegUnentschiedenNiederlage_WerdenProSpielerErmittelt()
    {
        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();

        var spielerSieger = new Spieler { Name = "Sieger" };
        var spielerVerlierer = new Spieler { Name = "Verlierer" };
        var spielerRemis1 = new Spieler { Name = "Remis1" };
        var spielerRemis2 = new Spieler { Name = "Remis2" };

        var team1 = new Team { Id = team1Id, Name = "Team1", Spieler = [spielerSieger, spielerRemis1] };
        var team2 = new Team { Id = team2Id, Name = "Team2", Spieler = [spielerVerlierer, spielerRemis2] };

        var duellGewonnen = new Einzelduell
        {
            Spieler1Id = spielerSieger.Id,
            Spieler2Id = spielerVerlierer.Id,
            Versuche = [new Versuch { Spieler1Getroffen = true, Spieler2Getroffen = false }],
            Ergebnis = new EinzelduellErgebnis { SiegerId = team1Id, DuellpunktTeam1 = 1, DuellpunktTeam2 = 0 }
        };
        var duellUnentschieden = new Einzelduell
        {
            Spieler1Id = spielerRemis1.Id,
            Spieler2Id = spielerRemis2.Id,
            Versuche = [new Versuch { Spieler1Getroffen = true, Spieler2Getroffen = true }],
            Ergebnis = new EinzelduellErgebnis { SiegerId = null, DuellpunktTeam1 = 1, DuellpunktTeam2 = 1 }
        };

        var spiel = new Spiel { Team1Id = team1Id, Team2Id = team2Id, Einzelduelle = [duellGewonnen, duellUnentschieden] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        var turnier = new Turnier { Teams = [team1, team2], Gruppen = [gruppe] };

        var rangliste = _sut.AlleSpielerRangliste(turnier);

        var sieger = rangliste.Single(s => s.Name == "Sieger");
        sieger.Spiele.Should().Be(1);
        sieger.Siege.Should().Be(1);
        sieger.Niederlagen.Should().Be(0);
        sieger.Unentschieden.Should().Be(0);

        var verlierer = rangliste.Single(s => s.Name == "Verlierer");
        verlierer.Spiele.Should().Be(1);
        verlierer.Niederlagen.Should().Be(1);

        var remis1 = rangliste.Single(s => s.Name == "Remis1");
        remis1.Spiele.Should().Be(1);
        remis1.Unentschieden.Should().Be(1);
    }

    /// <summary>
    /// TeamRangliste sortiert nach Trefferquote des Teams (Summe aller Spieler), unabhängig vom
    /// Turnier-Wertungskriterium, und enthält auch Teams ohne jeglichen Versuch (0/0).
    /// </summary>
    [Fact]
    public void TeamRangliste_SortiertNachTrefferquote_UndEnthaeltTeamsOhneVersuche()
    {
        var teamGut = new Team { Name = "Gut", Spieler = [new Spieler { Name = "G1" }] };
        var teamSchlecht = new Team { Name = "Schlecht", Spieler = [new Spieler { Name = "S1" }] };
        var teamOhneVersuch = new Team { Name = "OhneVersuch", Spieler = [new Spieler { Name = "O1" }] };
        var gegner = new Spieler { Name = "Gegner" };
        var teamGegner = new Team { Name = "GegnerTeam", Spieler = [gegner] };

        var duellGut = new Einzelduell
        {
            Spieler1Id = teamGut.Spieler[0].Id,
            Spieler2Id = gegner.Id,
            Versuche = [new Versuch { Spieler1Getroffen = true }, new Versuch { Spieler1Getroffen = true }]
        };
        var duellSchlecht = new Einzelduell
        {
            Spieler1Id = teamSchlecht.Spieler[0].Id,
            Spieler2Id = gegner.Id,
            Versuche = [new Versuch { Spieler1Getroffen = true }, new Versuch { Spieler1Getroffen = false }]
        };

        var spiel = new Spiel { Einzelduelle = [duellGut, duellSchlecht] };
        var gruppe = new Gruppe { Spiele = [spiel] };
        var turnier = new Turnier { Teams = [teamGut, teamSchlecht, teamOhneVersuch, teamGegner], Gruppen = [gruppe] };

        var rangliste = _sut.TeamRangliste(turnier);

        rangliste[0].TeamName.Should().Be("Gut");
        rangliste.Single(t => t.TeamName == "OhneVersuch").Versuche.Should().Be(0);
        rangliste.Single(t => t.TeamName == "OhneVersuch").Quote.Should().Be(0);
    }

    /// <summary>GesamtStatistikErmitteln summiert Treffer und Versuche über alle Spieler des Turniers.</summary>
    [Fact]
    public void GesamtStatistikErmitteln_SummiertAlleSpieler()
    {
        var turnier = DreiGleichaufSpielerTurnier(out _, out _, out _);

        var gesamt = _sut.GesamtStatistikErmitteln(turnier);

        // 3 Duelle à 5 Versuche: A/B/C treffen je 3-mal, der gemeinsame Gegner nie.
        // Versuche werden für beide Duellteilnehmer gezählt: 3 Duelle * 2 Spieler * 5 Versuche = 30.
        gesamt.Treffer.Should().Be(9);
        gesamt.Versuche.Should().Be(30);
        gesamt.Quote.Should().BeApproximately(0.3, 0.001);
    }
}
