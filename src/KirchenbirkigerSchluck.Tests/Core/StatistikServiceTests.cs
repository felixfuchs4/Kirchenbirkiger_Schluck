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
}
