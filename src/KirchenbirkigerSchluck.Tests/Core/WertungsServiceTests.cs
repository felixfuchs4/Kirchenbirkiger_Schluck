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
    /// Drei punktgleiche Teams (Zirkel), aber unterschiedliches Torverhältnis (Duelldifferenz):
    /// Das Torverhältnis entscheidet die Reihenfolge – kein direkter Vergleich, kein Stechen.
    /// </summary>
    [Fact]
    public void GruppenRanglisteBerechnen_PunktgleichUnterschiedlichesTorverhaeltnis_TorverhaeltnisEntscheidet()
    {
        // Arrange – Einfach-System, Zirkel A→B→C→A, aber verschiedene Duelldifferenzen:
        // A: +2, C: 0, B: -2 (alle 1 Punkt)
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamB, teamA, EntscheidungsArt.RegulaereSpielzeit, 5, 2),
                AbgeschlossenesSpielBauen(teamC, teamA, teamC, EntscheidungsArt.RegulaereSpielzeit, 5, 4),
                AbgeschlossenesSpielBauen(teamB, teamC, teamB, EntscheidungsArt.RegulaereSpielzeit, 5, 4)
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Einfach);

        // Assert – Reihenfolge rein nach Torverhältnis, keine Tiebreak-Markierung
        rangliste.Single(e => e.TeamId == teamA).Position.Should().Be(1); // Torv +2
        rangliste.Single(e => e.TeamId == teamC).Position.Should().Be(2); // Torv  0
        rangliste.Single(e => e.TeamId == teamB).Position.Should().Be(3); // Torv -2
        rangliste.Should().OnlyContain(e =>
            !e.DurchDirektenVergleich && !e.DurchStechen && !e.StehenErforderlich);
    }

    /// <summary>
    /// A und B sind auf Punkten UND Torverhältnis gleichauf; A hat B direkt besiegt → Direkter
    /// Vergleich entscheidet (Markierung „DV"). C ist bereits durch das Torverhältnis darüber.
    /// </summary>
    [Fact]
    public void GruppenRanglisteBerechnen_GleichPunkteUndTorverhaeltnis_DirekterVergleichEntscheidet()
    {
        // Arrange – Eishockey: A schlägt B, C schlägt A, B schlägt D
        // Punkte je 3 für A/B/C; Torverhältnis A=0, B=0, C=+1 → C oben, A/B per DV
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();
        var teamD = Guid.NewGuid();

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC, teamD],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamB, teamA, EntscheidungsArt.RegulaereSpielzeit, 3, 2),
                AbgeschlossenesSpielBauen(teamA, teamC, teamC, EntscheidungsArt.RegulaereSpielzeit, 2, 3),
                AbgeschlossenesSpielBauen(teamB, teamD, teamB, EntscheidungsArt.RegulaereSpielzeit, 3, 2)
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Eishockey);

        var eintrA = rangliste.Single(e => e.TeamId == teamA);
        var eintrB = rangliste.Single(e => e.TeamId == teamB);
        var eintrC = rangliste.Single(e => e.TeamId == teamC);

        // Assert – C durch Torverhältnis vorne (keine Markierung), A vor B durch Direkten Vergleich
        eintrC.Position.Should().Be(1);
        eintrC.DurchDirektenVergleich.Should().BeFalse();
        eintrC.DurchStechen.Should().BeFalse();

        eintrA.Position.Should().BeLessThan(eintrB.Position);
        eintrA.DurchDirektenVergleich.Should().BeTrue();
        eintrB.DurchDirektenVergleich.Should().BeTrue();
        eintrA.StehenErforderlich.Should().BeFalse();
        eintrB.StehenErforderlich.Should().BeFalse();
    }

    /// <summary>
    /// Drei Teams gleichauf auf Punkten und Torverhältnis im Zirkel (jeder schlägt jeden einmal):
    /// Direkter Vergleich löst nicht auf → Stechen erforderlich (Markierung „S").
    /// </summary>
    [Fact]
    public void GruppenRanglisteBerechnen_KeinDirekterSieger_StechenErforderlich()
    {
        // Arrange – Eishockey: A→B→C→A, alle gleiche Duelldifferenz (Torv 0), je 3 Punkte
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamB, teamA, EntscheidungsArt.RegulaereSpielzeit, 3, 1),
                AbgeschlossenesSpielBauen(teamB, teamC, teamB, EntscheidungsArt.RegulaereSpielzeit, 3, 1),
                AbgeschlossenesSpielBauen(teamC, teamA, teamC, EntscheidungsArt.RegulaereSpielzeit, 3, 1)
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Eishockey);

        // Assert – alle drei brauchen ein Stechen
        rangliste.Should().OnlyContain(e => e.StehenErforderlich && e.DurchStechen && !e.DurchDirektenVergleich);
    }

    /// <summary>
    /// A und B gleichauf (Punkte + Torverhältnis) ohne direktes Spiel; ein gespieltes Platzierungs-
    /// Stechen A-&gt;B löst auf: A vor B, „S"-Markierung, kein Stechen mehr nötig.
    /// Das Stechen zählt nicht für die Tabellenpunkte.
    /// </summary>
    [Fact]
    public void GruppenRanglisteBerechnen_GespieltesStechen_LoestGleichstandAuf()
    {
        // Arrange – Einfach: A schlägt C, B schlägt D, C schlägt D → A,B je 1 Pt., Torv +3 (kein A-B-Spiel)
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();
        var teamD = Guid.NewGuid();

        var stechen = AbgeschlossenesSpielBauen(teamA, teamB, teamA, EntscheidungsArt.RegulaereSpielzeit, 4, 1);
        stechen.IstPlatzierungsStechen = true;

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC, teamD],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamC, teamA, EntscheidungsArt.RegulaereSpielzeit, 4, 1),
                AbgeschlossenesSpielBauen(teamB, teamD, teamB, EntscheidungsArt.RegulaereSpielzeit, 4, 1),
                AbgeschlossenesSpielBauen(teamC, teamD, teamC, EntscheidungsArt.RegulaereSpielzeit, 4, 1),
                stechen
            ]
        };

        // Act
        var rangliste = _sut.GruppenRanglisteBerechnen(gruppe, Wertungssystem.Einfach);

        var eintrA = rangliste.Single(e => e.TeamId == teamA);
        var eintrB = rangliste.Single(e => e.TeamId == teamB);

        // Assert – Stechen-Sieger A vor B, per Stechen aufgelöst
        eintrA.Position.Should().BeLessThan(eintrB.Position);
        eintrA.DurchStechen.Should().BeTrue();
        eintrB.DurchStechen.Should().BeTrue();
        eintrA.StehenErforderlich.Should().BeFalse();
        eintrB.StehenErforderlich.Should().BeFalse();
        // Stechen zählt nicht für die Tabellenpunkte
        eintrA.Tabellenpunkte.Should().Be(1);
        eintrB.Tabellenpunkte.Should().Be(1);
    }
}
