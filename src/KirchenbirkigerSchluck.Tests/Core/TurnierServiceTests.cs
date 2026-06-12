/*************************************************************
 * Datei:        TurnierServiceTests.cs
 * Zweck:        Unit-Tests für den TurnierService
 * Bereich:      Tests – Anwendungslogik
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
/// Tests für <see cref="TurnierService"/> – Erstellung, Team-Verwaltung und Statuswechsel.
/// </summary>
public class TurnierServiceTests
{
    private readonly TurnierService _service = new(new InMemoryRepository());

    // ---------------------------------------------------------------------------
    // TurnierErstellen
    // ---------------------------------------------------------------------------

    [Fact]
    public void TurnierErstellen_SetzteAllePflichtfelder()
    {
        // Arrange
        var datum = new DateOnly(2026, 7, 19);

        // Act
        var turnier = _service.TurnierErstellen("Kirchenbirkiger Schluck 2026", datum, Wertungssystem.Eishockey);

        // Assert
        turnier.Anlass.Should().Be("Kirchenbirkiger Schluck 2026");
        turnier.Datum.Should().Be(datum);
        turnier.Wertungssystem.Should().Be(Wertungssystem.Eishockey);
        turnier.Status.Should().Be(TurnierStatus.InVorbereitung);
        turnier.Id.Should().NotBe(Guid.Empty);
    }

    // ---------------------------------------------------------------------------
    // TeamHinzufuegen
    // ---------------------------------------------------------------------------

    [Fact]
    public void TeamHinzufuegen_FuegtTeamHinzu()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        var team = _service.TeamHinzufuegen(turnier, "Dorfkickers");

        // Assert
        turnier.Teams.Should().ContainSingle();
        team.Name.Should().Be("Dorfkickers");
        team.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void TeamHinzufuegen_MitKurzname_SetzKurzname()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        var team = _service.TeamHinzufuegen(turnier, "Dorfkickers", "DK");

        // Assert
        team.Kurzname.Should().Be("DK");
    }

    // ---------------------------------------------------------------------------
    // TeamZurueckziehen
    // ---------------------------------------------------------------------------

    [Fact]
    public void TeamZurueckziehen_SetzStatusZurueckgezogen()
    {
        // Arrange
        var turnier = new Turnier();
        var team = _service.TeamHinzufuegen(turnier, "Rückzieher");

        // Act
        _service.TeamZurueckziehen(turnier, team.Id);

        // Assert
        team.Status.Should().Be(TeamStatus.Zurueckgezogen);
    }

    [Fact]
    public void TeamZurueckziehen_MarkiertOffeneSpiele()
    {
        // Arrange
        var turnier = new Turnier();
        var team = _service.TeamHinzufuegen(turnier, "Rückzieher");
        var anderes = _service.TeamHinzufuegen(turnier, "Gegner");

        var gruppe = new Gruppe();
        var offenes = new Spiel { Team1Id = team.Id, Team2Id = anderes.Id, Status = SpielStatus.Geplant };
        var abgeschlossenes = new Spiel { Team1Id = team.Id, Team2Id = anderes.Id, Status = SpielStatus.Abgeschlossen };
        gruppe.Spiele.AddRange([offenes, abgeschlossenes]);
        turnier.Gruppen.Add(gruppe);

        // Act
        _service.TeamZurueckziehen(turnier, team.Id);

        // Assert
        offenes.Status.Should().Be(SpielStatus.Abgesetzt);
        abgeschlossenes.Status.Should().Be(SpielStatus.Abgeschlossen);
    }

    // ---------------------------------------------------------------------------
    // StatusWechseln
    // ---------------------------------------------------------------------------

    [Fact]
    public void StatusWechseln_InVorbereitung_WirdGruppenphase()
    {
        // Arrange
        var turnier = new Turnier { Status = TurnierStatus.InVorbereitung };

        // Act
        _service.StatusWechseln(turnier);

        // Assert
        turnier.Status.Should().Be(TurnierStatus.Gruppenphase);
    }

    [Fact]
    public void StatusWechseln_Gruppenphase_WirdFinalrunde()
    {
        // Arrange
        var turnier = new Turnier { Status = TurnierStatus.Gruppenphase };

        // Act
        _service.StatusWechseln(turnier);

        // Assert
        turnier.Status.Should().Be(TurnierStatus.Finalrunde);
    }

    [Fact]
    public void StatusWechseln_Abgeschlossen_WirftException()
    {
        // Arrange
        var turnier = new Turnier { Status = TurnierStatus.Abgeschlossen };

        // Act
        var aktion = () => _service.StatusWechseln(turnier);

        // Assert
        aktion.Should().Throw<InvalidOperationException>();
    }

    // ---------------------------------------------------------------------------
    // Hilfsmittel
    // ---------------------------------------------------------------------------

    /// <summary>
    /// In-Memory-Implementierung des <see cref="ITurnierRepository"/> für Unit-Tests.
    /// </summary>
    private class InMemoryRepository : ITurnierRepository
    {
        private Turnier? _turnier;

        /// <inheritdoc/>
        public Turnier Laden() => _turnier ?? throw new FileNotFoundException("Kein Turnier gespeichert.");

        /// <inheritdoc/>
        public void Speichern(Turnier turnier) => _turnier = turnier;

        /// <inheritdoc/>
        public bool ExistiertDatei() => _turnier is not null;
    }
}
