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

    // ──────────────────────────────────────────────────────────
    // Auswerten – Best-of-5
    // ──────────────────────────────────────────────────────────

    /// <summary>Fügt dem Spiel ein reguläres Unentschieden-Duell hinzu (3 gleiche Versuche).</summary>
    private void RegulaeresUnentschieden(Spiel spiel, bool beideTreffen)
    {
        _sut.NaechesDuellStarten(spiel, Guid.NewGuid(), Guid.NewGuid());
        _sut.VersuchErfassen(spiel, beideTreffen, beideTreffen);
        while (spiel.Einzelduelle.Last().Ergebnis is null)
            _sut.VersuchErfassen(spiel, beideTreffen, beideTreffen);
    }

    /// <summary>Fügt ein Stechen-Duell mit dem angegebenen (wiederholten) Versuchsausgang hinzu.</summary>
    private void StechenDuell(Spiel spiel, bool team1Trifft, bool team2Trifft)
    {
        _sut.StechenStarten(spiel, Guid.NewGuid(), Guid.NewGuid());
        _sut.VersuchErfassen(spiel, team1Trifft, team2Trifft);
        while (spiel.Einzelduelle.Last().Ergebnis is null)
            _sut.VersuchErfassen(spiel, team1Trifft, team2Trifft);
    }

    /// <summary>3:0 nach drei Duellen → uneinholbar, Partie kann vorzeitig abgeschlossen werden.</summary>
    [Fact]
    public void Auswerten_UneinholbarerVorsprung_KannVorzeitigAbschliessen()
    {
        var spiel = SpielMitDuellenBauen(siege1: 3, siege2: 0);

        var f = _sut.Auswerten(spiel);

        f.KannAbschliessen.Should().BeTrue();
        f.StechenNoetig.Should().BeFalse();
        f.Verbleibend.Should().Be(2);
        f.RegulaereAbgeschlossen.Should().Be(3);
    }

    /// <summary>2:1 nach drei Duellen → Ausgleich noch möglich, es wird weitergespielt.</summary>
    [Fact]
    public void Auswerten_AusgleichMoeglich_KannNichtAbschliessen()
    {
        var spiel = SpielMitDuellenBauen(siege1: 2, siege2: 1);

        var f = _sut.Auswerten(spiel);

        f.KannAbschliessen.Should().BeFalse();
        f.StechenNoetig.Should().BeFalse();
    }

    /// <summary>3:1 nach vier Duellen (Vorsprung 2 &gt; 1 verbleibendes) → entschieden.</summary>
    [Fact]
    public void Auswerten_Grenzfall_DreiZuEinsNachVier_KannAbschliessen()
    {
        var spiel = SpielMitDuellenBauen(siege1: 3, siege2: 1);

        _sut.Auswerten(spiel).KannAbschliessen.Should().BeTrue();
    }

    /// <summary>Unentschieden-Duelle verändern den Vorsprung nicht (2:0 + Unentschieden bleibt aufholbar).</summary>
    [Fact]
    public void Auswerten_UnentschiedenAendertVorsprungNicht()
    {
        var spiel = SpielMitDuellenBauen(siege1: 2, siege2: 0);
        RegulaeresUnentschieden(spiel, beideTreffen: true); // 1:1-Duell, drittes reguläres Duell

        var f = _sut.Auswerten(spiel);

        f.DuellsiegeTeam1.Should().Be(2);
        f.DuellsiegeTeam2.Should().Be(0);
        f.RegulaereAbgeschlossen.Should().Be(3);
        f.Verbleibend.Should().Be(2);
        f.KannAbschliessen.Should().BeFalse("Vorsprung 2 ist bei 2 verbleibenden Duellen noch einholbar");
    }

    /// <summary>2:2 nach fünf regulären Duellen (inkl. Unentschieden) → Stechen nötig, nicht abschließbar.</summary>
    [Fact]
    public void Auswerten_GleichstandNachFuenf_StechenNoetig()
    {
        var spiel = SpielMitDuellenBauen(siege1: 2, siege2: 2);
        RegulaeresUnentschieden(spiel, beideTreffen: false); // fünftes reguläres Duell 0:0

        var f = _sut.Auswerten(spiel);

        f.RegulaereAbgeschlossen.Should().Be(5);
        f.Verbleibend.Should().Be(0);
        f.StechenNoetig.Should().BeTrue();
        f.KannAbschliessen.Should().BeFalse();
    }

    /// <summary>Unentschiedenes Stechen-Duell entscheidet die Partie noch nicht.</summary>
    [Fact]
    public void Auswerten_StechenUnentschieden_KannNichtAbschliessen()
    {
        var spiel = SpielMitDuellenBauen(siege1: 2, siege2: 2);
        RegulaeresUnentschieden(spiel, beideTreffen: false); // 5. Duell → Gleichstand
        StechenDuell(spiel, team1Trifft: true, team2Trifft: true); // Stechen 1:1

        var f = _sut.Auswerten(spiel);

        f.StechenNoetig.Should().BeTrue();
        f.KannAbschliessen.Should().BeFalse("ein unentschiedenes Stechen-Duell entscheidet nichts");
    }

    /// <summary>Stechen-Duell mit klarem Sieger → Partie kann abgeschlossen werden.</summary>
    [Fact]
    public void Auswerten_StechenKlarerSieger_KannAbschliessen()
    {
        var spiel = SpielMitDuellenBauen(siege1: 2, siege2: 2);
        RegulaeresUnentschieden(spiel, beideTreffen: false); // 5. Duell → Gleichstand
        StechenDuell(spiel, team1Trifft: true, team2Trifft: false); // Stechen für Team 1

        _sut.Auswerten(spiel).KannAbschliessen.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────
    // SpielerReihenfolgeFestlegen
    // ──────────────────────────────────────────────────────────

    /// <summary>Legt für beide Teams eine Reihenfolge fest, die eine Permutation der Spieler ist.</summary>
    [Fact]
    public void SpielerReihenfolgeFestlegen_ErzeugtPermutationJeTeam()
    {
        var spiel = new Spiel();
        var team1 = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var team2 = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        _sut.SpielerReihenfolgeFestlegen(spiel, team1, team2, new Random(42));

        spiel.Spieler1Reihenfolge.Should().BeEquivalentTo(team1);
        spiel.Spieler2Reihenfolge.Should().BeEquivalentTo(team2);
    }

    /// <summary>Eine bereits gesetzte Reihenfolge wird nicht überschrieben (idempotent).</summary>
    [Fact]
    public void SpielerReihenfolgeFestlegen_IstIdempotent()
    {
        var bestehend = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var spiel = new Spiel { Spieler1Reihenfolge = [.. bestehend] };
        var team1 = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var team2 = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();

        _sut.SpielerReihenfolgeFestlegen(spiel, team1, team2, new Random(1));

        spiel.Spieler1Reihenfolge.Should().Equal(bestehend);          // unverändert
        spiel.Spieler2Reihenfolge.Should().BeEquivalentTo(team2);     // neu gesetzt
    }
}
