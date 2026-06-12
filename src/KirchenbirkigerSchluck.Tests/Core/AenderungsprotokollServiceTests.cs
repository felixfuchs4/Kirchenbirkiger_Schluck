/*************************************************************
 * Datei:        AenderungsprotokollServiceTests.cs
 * Zweck:        Unit-Tests für den AenderungsprotokollService
 * Bereich:      Tests – Anwendungslogik
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.Tests.Core;

/// <summary>
/// Tests für <see cref="AenderungsprotokollService"/> – Eintrag erstellen und abfragen.
/// </summary>
public class AenderungsprotokollServiceTests
{
    private readonly AenderungsprotokollService _service = new();
    private readonly Guid _spielId = Guid.NewGuid();

    // ---------------------------------------------------------------------------
    // EintragErstellen
    // ---------------------------------------------------------------------------

    [Fact]
    public void EintragErstellen_FuegtEintragHinzu()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        _service.EintragErstellen(turnier, "Spiel", _spielId, "Status", "Geplant", "Abgesetzt");

        // Assert
        turnier.Aenderungsprotokoll.Should().ContainSingle();
    }

    [Fact]
    public void EintragErstellen_SetzteAlleFelder()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        _service.EintragErstellen(turnier, "Spiel", _spielId, "Status", "Geplant", "Abgesetzt", "Mannschaft zurückgezogen");

        // Assert
        var eintrag = turnier.Aenderungsprotokoll.Single();
        eintrag.Entitaet.Should().Be("Spiel");
        eintrag.EntitaetId.Should().Be(_spielId);
        eintrag.Feld.Should().Be("Status");
        eintrag.AlterWert.Should().Be("Geplant");
        eintrag.NeuerWert.Should().Be("Abgesetzt");
        eintrag.Begruendung.Should().Be("Mannschaft zurückgezogen");
        eintrag.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void EintragErstellen_OhneBegruendung_IstNull()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        _service.EintragErstellen(turnier, "Spiel", _spielId, "Status", null, "Laeuft");

        // Assert
        turnier.Aenderungsprotokoll.Single().Begruendung.Should().BeNull();
    }

    // ---------------------------------------------------------------------------
    // EintraegeAbfragen
    // ---------------------------------------------------------------------------

    [Fact]
    public void EintraegeAbfragen_FiltertNachEntitaetId()
    {
        // Arrange
        var turnier = new Turnier();
        var andereId = Guid.NewGuid();
        _service.EintragErstellen(turnier, "Spiel", _spielId,  "Status", null, "Laeuft");
        _service.EintragErstellen(turnier, "Spiel", andereId,  "Status", null, "Laeuft");

        // Act
        var ergebnis = _service.EintraegeAbfragen(turnier, _spielId);

        // Assert
        ergebnis.Should().ContainSingle();
        ergebnis[0].EntitaetId.Should().Be(_spielId);
    }

    [Fact]
    public void EintraegeAbfragen_SortiertNeusteZuerst()
    {
        // Arrange
        var turnier = new Turnier();
        var aelter = new Aenderungseintrag
        {
            EntitaetId   = _spielId,
            ZeitpunktUtc = DateTime.UtcNow.AddMinutes(-5),
            NeuerWert    = "alt"
        };
        var neuer = new Aenderungseintrag
        {
            EntitaetId   = _spielId,
            ZeitpunktUtc = DateTime.UtcNow,
            NeuerWert    = "neu"
        };
        turnier.Aenderungsprotokoll.AddRange([aelter, neuer]);

        // Act
        var ergebnis = _service.EintraegeAbfragen(turnier, _spielId);

        // Assert
        ergebnis[0].NeuerWert.Should().Be("neu");
        ergebnis[1].NeuerWert.Should().Be("alt");
    }

    [Fact]
    public void EintraegeAbfragen_KeineEintraege_GibtLeereListe()
    {
        // Arrange
        var turnier = new Turnier();

        // Act
        var ergebnis = _service.EintraegeAbfragen(turnier, _spielId);

        // Assert
        ergebnis.Should().BeEmpty();
    }
}
