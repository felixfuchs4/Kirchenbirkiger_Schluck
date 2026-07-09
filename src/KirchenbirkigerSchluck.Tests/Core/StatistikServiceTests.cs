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
}
