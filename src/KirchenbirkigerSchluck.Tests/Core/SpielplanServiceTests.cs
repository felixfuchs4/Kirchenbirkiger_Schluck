/*************************************************************
 * Datei:        SpielplanServiceTests.cs
 * Zweck:        Unit-Tests für SpielplanService (Round-Robin, Spielreihenfolge)
 * Bereich:      Tests – Spielplanung
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
/// Testet die Spielplangenerierung und -verwaltung durch den <see cref="SpielplanService"/>.
/// </summary>
public class SpielplanServiceTests
{
    private readonly ISpielplanService _sut = new SpielplanService();

    // ──────────────────────────────────────────────────────────
    // Hilfsmethoden
    // ──────────────────────────────────────────────────────────

    private static Gruppe GruppeMitTeamsBauen(int anzahlTeams)
    {
        var gruppe = new Gruppe { Name = "Gruppe Test" };
        for (int i = 0; i < anzahlTeams; i++)
            gruppe.TeamIds.Add(Guid.NewGuid());
        return gruppe;
    }

    private static Turnier TurnierMitEinerGruppeBauen(int teamAnzahl)
    {
        var turnier = new Turnier();
        turnier.Gruppen.Add(GruppeMitTeamsBauen(teamAnzahl));
        return turnier;
    }

    // ──────────────────────────────────────────────────────────
    // GruppenspielplanGenerieren – Spielanzahl
    // ──────────────────────────────────────────────────────────

    /// <summary>3 Teams in einer Gruppe → 3C2 = 3 Spiele.</summary>
    [Fact]
    public void GruppenspielplanGenerieren_DreiTeams_ErzeugtDreiSpiele()
    {
        // Arrange
        var turnier = TurnierMitEinerGruppeBauen(teamAnzahl: 3);

        // Act
        _sut.GruppenspielplanGenerieren(turnier);

        // Assert
        turnier.Gruppen[0].Spiele.Should().HaveCount(3);
    }

    /// <summary>4 Teams in einer Gruppe → 4C2 = 6 Spiele.</summary>
    [Fact]
    public void GruppenspielplanGenerieren_VierTeams_ErzeugtSechsSpiele()
    {
        // Arrange
        var turnier = TurnierMitEinerGruppeBauen(teamAnzahl: 4);

        // Act
        _sut.GruppenspielplanGenerieren(turnier);

        // Assert
        turnier.Gruppen[0].Spiele.Should().HaveCount(6);
    }

    // ──────────────────────────────────────────────────────────
    // GruppenspielplanGenerieren – Interleaving
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Zwei Gruppen mit je 3 Teams → 6 Spiele total.
    /// Spielnummern werden abwechselnd vergeben: Gruppe A, Gruppe B, Gruppe A, …
    /// </summary>
    [Fact]
    public void GruppenspielplanGenerieren_ZweiGruppen_SpieleWerdenInterleaved()
    {
        // Arrange
        var turnier = new Turnier();
        var gruppeA = GruppeMitTeamsBauen(3);
        var gruppeB = GruppeMitTeamsBauen(3);
        turnier.Gruppen.AddRange([gruppeA, gruppeB]);

        // Act
        _sut.GruppenspielplanGenerieren(turnier);

        // Assert – 3 Spiele je Gruppe
        gruppeA.Spiele.Should().HaveCount(3);
        gruppeB.Spiele.Should().HaveCount(3);

        // Interleaving: Gruppe A erhält ungerade Spielnummern 1,3,5; Gruppe B 2,4,6
        gruppeA.Spiele.Select(s => s.Spielnummer).Should().BeEquivalentTo([1, 3, 5]);
        gruppeB.Spiele.Select(s => s.Spielnummer).Should().BeEquivalentTo([2, 4, 6]);
    }

    // ──────────────────────────────────────────────────────────
    // NaechstesSpielErmitteln
    // ──────────────────────────────────────────────────────────

    /// <summary>Drei geplante Spiele → das mit der kleinsten Spielnummer wird zurückgegeben.</summary>
    [Fact]
    public void NaechstesSpielErmitteln_MehrerePlanteSpiele_GibtSpielnummerEinsZurueck()
    {
        // Arrange
        var turnier = TurnierMitEinerGruppeBauen(teamAnzahl: 3);
        _sut.GruppenspielplanGenerieren(turnier); // Spielnummern 1, 2, 3

        // Act
        var naechstes = _sut.NaechstesSpielErmitteln(turnier);

        // Assert
        naechstes.Should().NotBeNull();
        naechstes!.Spielnummer.Should().Be(1);
    }

    /// <summary>Kein Spiel im Status Geplant → null wird zurückgegeben.</summary>
    [Fact]
    public void NaechstesSpielErmitteln_AlleSpieleLaufen_GibtNullZurueck()
    {
        // Arrange
        var turnier = TurnierMitEinerGruppeBauen(teamAnzahl: 2);
        _sut.GruppenspielplanGenerieren(turnier);
        foreach (var spiel in turnier.Gruppen[0].Spiele)
            spiel.Status = SpielStatus.Laeuft;

        // Act
        var naechstes = _sut.NaechstesSpielErmitteln(turnier);

        // Assert
        naechstes.Should().BeNull();
    }

    // ──────────────────────────────────────────────────────────
    // SpielNachHintenVerschieben
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Spiel in Position 2 von 3 wird verschoben:
    /// Spielnummer 2 → 3, Spielnummer 3 → 2.
    /// </summary>
    [Fact]
    public void SpielNachHintenVerschieben_SpielInPosition2Von3_TauschtMitNaechstem()
    {
        // Arrange
        var turnier = TurnierMitEinerGruppeBauen(teamAnzahl: 3);
        _sut.GruppenspielplanGenerieren(turnier); // Spielnummern 1, 2, 3

        var spiele = turnier.Gruppen[0].Spiele.OrderBy(s => s.Spielnummer).ToList();
        var zielSpiel = spiele[1]; // Spielnummer 2
        var naechstesSpiel = spiele[2]; // Spielnummer 3

        // Act
        _sut.SpielNachHintenVerschieben(turnier, zielSpiel.Id);

        // Assert
        zielSpiel.Spielnummer.Should().Be(3);
        naechstesSpiel.Spielnummer.Should().Be(2);
    }

    // ──────────────────────────────────────────────────────────
    // PlatzierungsStechenErzeugen
    // ──────────────────────────────────────────────────────────

    private static Spiel AbgeschlossenesSpielBauen(Guid t1, Guid t2, Guid sieger) => new()
    {
        Team1Id = t1,
        Team2Id = t2,
        Status = SpielStatus.Abgeschlossen,
        Ergebnis = new SpielErgebnis
        {
            SiegerId = sieger,
            DuellpunkteTeam1 = sieger == t1 ? 4 : 1,
            DuellpunkteTeam2 = sieger == t2 ? 4 : 1,
            EntschiedenDurch = EntscheidungsArt.RegulaereSpielzeit
        }
    };

    /// <summary>
    /// Bei B/C-Gleichstand (gleiche Punkte, kein direkter Vergleich) wird genau ein
    /// Platzierungs-Stechen B–C erzeugt.
    /// </summary>
    [Fact]
    public void PlatzierungsStechenErzeugen_BeiGleichstand_ErzeugtGenauEinStechen()
    {
        // Arrange
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teamC = Guid.NewGuid();
        var teamD = Guid.NewGuid();

        var gruppe = new Gruppe
        {
            TeamIds = [teamA, teamB, teamC, teamD],
            Spiele =
            [
                AbgeschlossenesSpielBauen(teamA, teamC, teamA),
                AbgeschlossenesSpielBauen(teamB, teamD, teamB),
                AbgeschlossenesSpielBauen(teamC, teamD, teamC)
            ]
        };
        var turnier = new Turnier { Wertungssystem = Wertungssystem.Einfach };
        turnier.Gruppen.Add(gruppe);

        // Act
        var anzahl = _sut.PlatzierungsStechenErzeugen(turnier);

        // Assert
        anzahl.Should().Be(1);
        var stechen = gruppe.Spiele.Where(s => s.IstPlatzierungsStechen).ToList();
        stechen.Should().HaveCount(1);
        var teilnehmer = new[] { stechen[0].Team1Id, stechen[0].Team2Id };
        teilnehmer.Should().BeEquivalentTo([teamB, teamC]);

        // Erneuter Aufruf erzeugt kein doppeltes Stechen
        _sut.PlatzierungsStechenErzeugen(turnier).Should().Be(0);
    }
}
