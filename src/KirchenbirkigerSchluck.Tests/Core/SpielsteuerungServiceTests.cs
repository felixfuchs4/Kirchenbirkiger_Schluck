/*************************************************************
 * Datei:        SpielsteuerungServiceTests.cs
 * Zweck:        Unit-Tests für SpielsteuerungService (Versuchserfassung und Spielabschluss)
 * Bereich:      Tests – Spielsteuerung
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
/// Testet die Spielablaufsteuerung durch den <see cref="SpielsteuerungService"/>.
/// </summary>
public class SpielsteuerungServiceTests
{
    private readonly ISpielsteuerungService _sut = new SpielsteuerungService();

    // ──────────────────────────────────────────────────────────
    // Hilfsmethoden
    // ──────────────────────────────────────────────────────────

    private static Spiel LaufendesSpielBauen()
    {
        return new Spiel
        {
            Team1Id = Guid.NewGuid(),
            Team2Id = Guid.NewGuid(),
            Status  = SpielStatus.Laeuft
        };
    }

    /// <summary>
    /// Legt ein Spiel mit abgeschlossenen Duellen an, sodass T1 <paramref name="siege1"/>
    /// und T2 <paramref name="siege2"/> Duelle gewonnen hat (alle regulär in Versuch 1).
    /// </summary>
    private Spiel SpielMitDuellenBauen(int siege1, int siege2)
    {
        var spiel = LaufendesSpielBauen();

        for (int i = 0; i < siege1; i++)
        {
            _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());
            _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: false);
        }

        for (int i = 0; i < siege2; i++)
        {
            _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());
            _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: true);
        }

        return spiel;
    }

    // ──────────────────────────────────────────────────────────
    // SpielStarten
    // ──────────────────────────────────────────────────────────

    /// <summary>SpielStarten setzt den Status korrekt auf Laeuft.</summary>
    [Fact]
    public void SpielStarten_GeplantesSpiel_SetzStatusAufLaeuft()
    {
        // Arrange
        var spiel = new Spiel { Status = SpielStatus.Geplant };

        // Act
        _sut.SpielStarten(spiel);

        // Assert
        spiel.Status.Should().Be(SpielStatus.Laeuft);
    }

    // ──────────────────────────────────────────────────────────
    // VersuchErfassen – Entscheidung nach einem Versuch
    // ──────────────────────────────────────────────────────────

    /// <summary>VersuchErfassen: Nur Team 1 trifft → Duell geht an Team 1.</summary>
    [Fact]
    public void VersuchErfassen_NurTeam1Trifft_DuellWirdFuerTeam1Entschieden()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act
        _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: false);

        // Assert
        var aktuellesDuell = spiel.Einzelduelle.Last();
        aktuellesDuell.Ergebnis.Should().NotBeNull();
        aktuellesDuell.Ergebnis!.SiegerId.Should().Be(spiel.Team1Id);
    }

    /// <summary>VersuchErfassen: Nur Team 2 trifft → Duell geht an Team 2.</summary>
    [Fact]
    public void VersuchErfassen_NurTeam2Trifft_DuellWirdFuerTeam2Entschieden()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act
        _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: true);

        // Assert
        var aktuellesDuell = spiel.Einzelduelle.Last();
        aktuellesDuell.Ergebnis.Should().NotBeNull();
        aktuellesDuell.Ergebnis!.SiegerId.Should().Be(spiel.Team2Id);
    }

    /// <summary>VersuchErfassen: Beide treffen → Versuch unentschieden, kein Sieger gesetzt.</summary>
    [Fact]
    public void VersuchErfassen_BeideTreffen_VersuchUnentschieden()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act
        _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: true);

        // Assert
        var aktuellesDuell = spiel.Einzelduelle.Last();
        aktuellesDuell.Ergebnis.Should().BeNull();
        aktuellesDuell.Versuche.Should().HaveCount(1);
    }

    /// <summary>VersuchErfassen: Keiner trifft → Versuch unentschieden, Duell läuft weiter.</summary>
    [Fact]
    public void VersuchErfassen_KeinerTrifft_VersuchUnentschieden()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act
        _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: false);

        // Assert
        var aktuellesDuell = spiel.Einzelduelle.Last();
        aktuellesDuell.Ergebnis.Should().BeNull();
        aktuellesDuell.Versuche.Should().HaveCount(1);
    }

    // ──────────────────────────────────────────────────────────
    // VersuchErfassen – 3 Versuche ohne Entscheidung
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Drei Versuche, beide treffen jedes Mal →
    /// Duell endet 1:1 (beide haben getroffen), kein Sieger.
    /// </summary>
    [Fact]
    public void VersuchErfassen_DreiVersuche_BeideTrefferJedesmal_DuellEndet1zu1()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act – 3 unentschiedene Versuche
        _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: true);
        _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: true);
        _sut.VersuchErfassen(spiel, spieler1Getroffen: true, spieler2Getroffen: true);

        // Assert
        var duell = spiel.Einzelduelle.Last();
        duell.Ergebnis.Should().NotBeNull();
        duell.Ergebnis!.SiegerId.Should().BeNull();
        duell.Ergebnis.DuellpunktTeam1.Should().Be(1);
        duell.Ergebnis.DuellpunktTeam2.Should().Be(1);
    }

    /// <summary>
    /// Drei Versuche, keiner trifft →
    /// Duell endet 0:0 (niemand hat getroffen), kein Sieger.
    /// </summary>
    [Fact]
    public void VersuchErfassen_DreiVersuche_KeinerTrifftJemals_DuellEndet0zu0()
    {
        // Arrange
        var spiel = LaufendesSpielBauen();
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());

        // Act – 3 unentschiedene Versuche ohne Treffer
        _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: false);
        _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: false);
        _sut.VersuchErfassen(spiel, spieler1Getroffen: false, spieler2Getroffen: false);

        // Assert
        var duell = spiel.Einzelduelle.Last();
        duell.Ergebnis.Should().NotBeNull();
        duell.Ergebnis!.SiegerId.Should().BeNull();
        duell.Ergebnis.DuellpunktTeam1.Should().Be(0);
        duell.Ergebnis.DuellpunktTeam2.Should().Be(0);
    }

    // ──────────────────────────────────────────────────────────
    // SpielAbschliessen
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Klarer Sieg von Team 1 (3:2) in regulärer Zeit →
    /// Status Abgeschlossen, Sieger Team 1, EntschiedenDurch = RegulaereSpielzeit.
    /// </summary>
    [Fact]
    public void SpielAbschliessen_KlarerSieger_SetztStatusUndErgebnis()
    {
        // Arrange – 5 Duelle: T1 gewinnt 3, T2 gewinnt 2
        var turnier = new Turnier { Wertungssystem = Wertungssystem.Eishockey };
        var spiel = SpielMitDuellenBauen(siege1: 3, siege2: 2);

        // Act
        _sut.SpielAbschliessen(spiel, turnier);

        // Assert
        spiel.Status.Should().Be(SpielStatus.Abgeschlossen);
        spiel.Ergebnis.Should().NotBeNull();
        spiel.Ergebnis!.SiegerId.Should().Be(spiel.Team1Id!.Value);
        spiel.Ergebnis.EntschiedenDurch.Should().Be(EntscheidungsArt.RegulaereSpielzeit);
        spiel.Ergebnis.DuellpunkteTeam1.Should().Be(3);
        spiel.Ergebnis.DuellpunkteTeam2.Should().Be(2);
    }
}
