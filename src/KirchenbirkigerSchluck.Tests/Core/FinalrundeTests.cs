/*************************************************************
 * Datei:        FinalrundeTests.cs
 * Zweck:        Unit-Tests für FinalrundeGenerieren und BracketFortsetzungAktualisieren
 * Bereich:      Tests – Spielplanung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Core.Services;

namespace KirchenbirkigerSchluck.Tests.Core;

/// <summary>
/// Tests für <see cref="SpielplanService.FinalrundeGenerieren"/> und
/// <see cref="SpielplanService.BracketFortsetzungAktualisieren"/>.
/// </summary>
public class FinalrundeTests
{
    private readonly SpielplanService _sut = new();

    // ---------------------------------------------------------------------------
    // Kurze Finalrunde
    // ---------------------------------------------------------------------------

    [Fact]
    public void Kurz_ZweiGruppen_ErzeugtKorrektePaarungen()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.Kurz);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: A1 vs B1, A2 vs B2, A3 vs B3
        turnier.Finalrundenspiele.Should().HaveCount(6);
        var g1Teams = turnier.Gruppen[0].TeamIds;
        var g2Teams = turnier.Gruppen[1].TeamIds;

        for (int i = 0; i < 6; i++)
        {
            var spiel = turnier.Finalrundenspiele[i];
            spiel.Team1Id.Should().Be(g1Teams[i]);
            spiel.Team2Id.Should().Be(g2Teams[i]);
        }
    }

    [Fact]
    public void Kurz_AnzahlSpieleEntsprichtKleinstenGruppe()
    {
        // Arrange: Gruppe A hat 4 Teams, Gruppe B hat 6 Teams
        var turnier = TurnierMitZweiGruppenBauen(4, 6, FinalrundenModus.Kurz);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: nur 4 Spiele (min der beiden Gruppengrößen)
        turnier.Finalrundenspiele.Should().HaveCount(4);
    }

    // ---------------------------------------------------------------------------
    // KO-Baum: Achtelfinale
    // ---------------------------------------------------------------------------

    [Fact]
    public void KoBaum_ZweiGruppenJe6_ErzeugtVierAchtelfinaleSpiele()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: 4 Achtelfinale-Spiele (A3 vs B6, A4 vs B5, B3 vs A6, B4 vs A5)
        var achtel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Achtelfinale").ToList();
        achtel.Should().HaveCount(4);

        // Alle Achtelfinale-Spiele haben bekannte Team-IDs (keine Platzhalter)
        achtel.Should().OnlyContain(s => s.Team1Id.HasValue && s.Team2Id.HasValue);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe6_ErzeugtViertelfinaleAlsPlatzhalter()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: 4 Viertelfinale-Spiele mit mindestens einer null-Team-Id
        var viertel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Viertelfinale").ToList();
        viertel.Should().HaveCount(4);

        // Jedes VF-Spiel hat mindestens einen TBD-Slot (der Vorgänger-Spielsieger)
        viertel.Should().OnlyContain(s => s.Team1Id == null || s.Team2Id == null);

        // Jedes VF-Spiel verweist auf ein Vorgänger-Spiel
        viertel.Should().OnlyContain(s => s.VorgaengerSpiel1Id.HasValue || s.VorgaengerSpiel2Id.HasValue);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe6_KoBaumEin_ErzeugtFinale()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: genau 1 Finale
        turnier.Finalrundenspiele.Count(s => s.BracketRunde == "Finale").Should().Be(1);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe6_KoBaumZwei_KeinFinale()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumZwei);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: kein gemeinsames Finale
        turnier.Finalrundenspiele.Count(s => s.BracketRunde == "Finale").Should().Be(0);
        // Aber 2 Halbfinale (die je eigene Gruppenfinale sind)
        turnier.Finalrundenspiele.Count(s => s.BracketRunde == "Halbfinale").Should().Be(2);
    }

    [Fact]
    public void KoBaum_ZweiGruppenJe5_ErzeugtZweiAchtelfinaleSpiele()
    {
        // Arrange: Gruppe A hat 5 Teams → nur B5 als Füller (B6 fehlt)
        var turnier = TurnierMitZweiGruppenBauen(5, 5, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);

        // Act
        _sut.FinalrundeGenerieren(turnier);

        // Assert: nur 2 Achtelfinale (A3 vs B5, B3 vs A5 — A4/B6 und B4/A6 fehlen)
        var achtel = turnier.Finalrundenspiele.Where(s => s.BracketRunde == "Achtelfinale").ToList();
        achtel.Should().HaveCount(2);
    }

    // ---------------------------------------------------------------------------
    // BracketFortsetzungAktualisieren
    // ---------------------------------------------------------------------------

    [Fact]
    public void BracketFortsetzung_TraegtSiegerInNaechsteRunde()
    {
        // Arrange
        var turnier = TurnierMitZweiGruppenBauen(6, 6, FinalrundenModus.KoBaumEin);
        GruppenspielplanAbschliessen(turnier);
        _sut.FinalrundeGenerieren(turnier);

        var achtelSpiel = turnier.Finalrundenspiele.First(s => s.BracketRunde == "Achtelfinale");
        var siegerId    = achtelSpiel.Team1Id!.Value;

        // Abschlussergebnis simulieren
        achtelSpiel.Status   = SpielStatus.Abgeschlossen;
        achtelSpiel.Ergebnis = new SpielErgebnis { SiegerId = siegerId };

        // Act
        _sut.BracketFortsetzungAktualisieren(turnier, achtelSpiel);

        // Assert: das VF-Spiel, das dieses Achtelfinale als Vorgänger hat,
        //         bekommt jetzt den Sieger als Team-Id eingetragen
        var vfSpiel = turnier.Finalrundenspiele.First(s =>
            s.VorgaengerSpiel1Id == achtelSpiel.Id || s.VorgaengerSpiel2Id == achtelSpiel.Id);

        bool siegerId1 = vfSpiel.VorgaengerSpiel1Id == achtelSpiel.Id && vfSpiel.Team1Id == siegerId;
        bool siegerId2 = vfSpiel.VorgaengerSpiel2Id == achtelSpiel.Id && vfSpiel.Team2Id == siegerId;
        (siegerId1 || siegerId2).Should().BeTrue("Der Sieger muss in das VF-Spiel übernommen worden sein.");
    }

    // ---------------------------------------------------------------------------
    // Hilfsmethoden
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Erstellt ein Turnier mit zwei Gruppen und je <paramref name="aCount"/> / <paramref name="bCount"/>
    /// Teams, ohne Gruppenspiele zu generieren.
    /// </summary>
    private static Turnier TurnierMitZweiGruppenBauen(int aCount, int bCount, FinalrundenModus modus)
    {
        var turnier = new Turnier
        {
            Wertungssystem  = Wertungssystem.Einfach,
            FinalrundenModus = modus
        };

        var gruppeA = new Gruppe { Name = "A", Nummer = 1 };
        for (int i = 0; i < aCount; i++)
        {
            var id = Guid.NewGuid();
            turnier.Teams.Add(new Team { Id = id, Name = $"A{i + 1}" });
            gruppeA.TeamIds.Add(id);
        }

        var gruppeB = new Gruppe { Name = "B", Nummer = 2 };
        for (int i = 0; i < bCount; i++)
        {
            var id = Guid.NewGuid();
            turnier.Teams.Add(new Team { Id = id, Name = $"B{i + 1}" });
            gruppeB.TeamIds.Add(id);
        }

        turnier.Gruppen.AddRange([gruppeA, gruppeB]);
        return turnier;
    }

    /// <summary>
    /// Generiert den Gruppenspielplan und schließt alle Spiele ab, damit die Rangliste berechnet
    /// werden kann. Team1 gewinnt jedes Spiel (reicht für Ranking-Eindeutigkeit nicht, daher
    /// werden Spiele mit aufsteigender Spielnummer ausgewertet).
    /// </summary>
    private void GruppenspielplanAbschliessen(Turnier turnier)
    {
        _sut.GruppenspielplanGenerieren(turnier);

        foreach (var gruppe in turnier.Gruppen)
        {
            foreach (var spiel in gruppe.Spiele)
            {
                spiel.Status   = SpielStatus.Abgeschlossen;
                var siegerId   = spiel.Team1Id!.Value;
                spiel.Ergebnis = new SpielErgebnis
                {
                    SiegerId           = siegerId,
                    DuellpunkteTeam1   = 3,
                    DuellpunkteTeam2   = 0,
                    EntschiedenDurch   = EntscheidungsArt.RegulaereSpielzeit,
                    TabellenPunkteTeam1 = 1,
                    TabellenPunkteTeam2 = 0
                };
            }
        }
    }
}
