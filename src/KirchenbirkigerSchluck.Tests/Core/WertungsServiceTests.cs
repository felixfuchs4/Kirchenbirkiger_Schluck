/*************************************************************
 * Datei:        WertungsServiceTests.cs
 * Zweck:        Unit-Tests für WertungsService (Tabellenpunkt-Berechnung und Rangliste)
 * Bereich:      Tests – Wertungslogik
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.Tests.Core;

/// <summary>
/// Testet die Berechnung von Tabellenpunkten und Gruppenranglisten durch den <see cref="WertungsService"/>.
/// </summary>
public class WertungsServiceTests
{
    private readonly IWertungsService _sut = new WertungsService();

    // ──────────────────────────────────────────────────────────
    // Hilfsmethoden
    // ──────────────────────────────────────────────────────────

    private static Spiel AbgeschlossenesSpielBauen(
        Guid team1Id, Guid team2Id,
        Guid siegerId,
        EntscheidungsArt art,
        int duellpunkteTeam1,
        int duellpunkteTeam2)
        => new()
        {
            Team1Id = team1Id,
            Team2Id = team2Id,
            Status = SpielStatus.Abgeschlossen,
            Ergebnis = new SpielErgebnis
            {
                SiegerId = siegerId,
                DuellpunkteTeam1 = duellpunkteTeam1,
                DuellpunkteTeam2 = duellpunkteTeam2,
                EntschiedenDurch = art
            }
        };

    // ──────────────────────────────────────────────────────────
    // TabellenPunkteBerechnen – Eishockey
    // ──────────────────────────────────────────────────────────

    /// <summary>Eishockey-System: Regulärer Sieg von Team 1 ergibt 3:0 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EishockeySiegRegulaer_Gibt3Zu0()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t1, EntscheidungsArt.RegulaereSpielzeit, 4, 1);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Eishockey);

        // Assert
        pts1.Should().Be(3);
        pts2.Should().Be(0);
    }

    /// <summary>Eishockey-System: Reguläre Niederlage von Team 1 ergibt 0:3 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EishockeyNiederlageRegulaer_Gibt0Zu3()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t2, EntscheidungsArt.RegulaereSpielzeit, 1, 4);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Eishockey);

        // Assert
        pts1.Should().Be(0);
        pts2.Should().Be(3);
    }

    /// <summary>Eishockey-System: Stechen-Sieg von Team 1 ergibt 2:1 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EishockeySiegStechen_Gibt2Zu1()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t1, EntscheidungsArt.Stechen, 3, 2);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Eishockey);

        // Assert
        pts1.Should().Be(2);
        pts2.Should().Be(1);
    }

    /// <summary>Eishockey-System: Stechen-Niederlage von Team 1 ergibt 1:2 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EishockeyNiederlageStechen_Gibt1Zu2()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t2, EntscheidungsArt.Stechen, 2, 3);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Eishockey);

        // Assert
        pts1.Should().Be(1);
        pts2.Should().Be(2);
    }

    // ──────────────────────────────────────────────────────────
    // TabellenPunkteBerechnen – Einfach
    // ──────────────────────────────────────────────────────────

    /// <summary>Einfaches System: Sieg von Team 1 ergibt 1:0 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EinfachSieg_Gibt1Zu0()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t1, EntscheidungsArt.RegulaereSpielzeit, 4, 1);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Einfach);

        // Assert
        pts1.Should().Be(1);
        pts2.Should().Be(0);
    }

    /// <summary>Einfaches System: Niederlage von Team 1 ergibt 0:1 Tabellenpunkte.</summary>
    [Fact]
    public void TabellenPunkteBerechnen_EinfachNiederlage_Gibt0Zu1()
    {
        // Arrange
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var spiel = AbgeschlossenesSpielBauen(t1, t2, t2, EntscheidungsArt.RegulaereSpielzeit, 1, 4);

        // Act
        var (pts1, pts2) = _sut.TabellenPunkteBerechnen(spiel, Wertungssystem.Einfach);

        // Assert
        pts1.Should().Be(0);
        pts2.Should().Be(1);
    }

    // ──────────────────────────────────────────────────────────
    // GruppenRanglisteBerechnen
    // ──────────────────────────────────────────────────────────

    /// <summary>Zwei Teams, klarer Sieger: Verlierer landet auf Position 2.</summary>
    [Fact]
    public void GruppenRanglisteBerechnen_ZweiTeams_VerliererHatPlatz2()
    {
        // Arrange
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamB, teamA, EntscheidungsArt.RegulaereSpielzeit, 4, 1)
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Eishockey);

        // Assert
        rangliste.Should().HaveCount(2);
        rangliste.Single(e => e.TeamId == teamA).Position.Should().Be(1);
        rangliste.Single(e => e.TeamId == teamB).Position.Should().Be(2);
    }

    /// <summary>
    /// Vier Teams, A und C punktgleich nach zwei Spielen.
    /// Direkter Vergleich: A hat C besiegt → C rutscht hinter A ab.
    /// C und B bleiben ohne direkten Vergleich unentschieden → StehenErforderlich.
    /// </summary>
    [Fact]
    public void GruppenRanglisteBerechnen_PunktgleichDirektSieg_VerliererRutschtAb()
    {
        // Arrange
        // 4 Teams, Einfach-System (1 Pt. pro Sieg)
        // Spiele: A schlägt C, B schlägt D, C schlägt D
        // Punkte: A=1, B=1, C=1, D=0
        // Direkter Vergleich zwischen A,B,C: A hat C (1 Sub-Pt.), B und C haben 0 Sub-Pts.
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();
        var teamD = Guid.NewGuid();

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC, teamD],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamC, teamA, EntscheidungsArt.RegulaereSpielzeit, 4, 1),
                AbgeschlossenesSpielBauen(teamB, teamD, teamB, EntscheidungsArt.RegulaereSpielzeit, 4, 1),
                AbgeschlossenesSpielBauen(teamC, teamD, teamC, EntscheidungsArt.RegulaereSpielzeit, 4, 1)
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Einfach);

        // Assert
        var eintrA = rangliste.Single(e => e.TeamId == teamA);
        var eintrC = rangliste.Single(e => e.TeamId == teamC);

        // A hat C direkt besiegt → A rangiert vor C
        eintrA.Position.Should().BeLessThan(eintrC.Position);
        // A ist durch Direkten Vergleich eindeutig platziert → kein Stechen nötig
        eintrA.StehenErforderlich.Should().BeFalse();
        // C ist immer noch mit B gleichgestellt → Stechen notwendig
        eintrC.StehenErforderlich.Should().BeTrue();
    }
}
