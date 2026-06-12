/*************************************************************
 * Datei:        SpielsteuerungService.cs
 * Zweck:        Steuerung des Spielablaufs: Duellerfassung, Versuchsbewertung, Spielabschluss
 * Bereich:      Anwendungslogik – Spielsteuerung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Steuerung des Spielablaufs bereit.
/// </summary>
public class SpielsteuerungService : ISpielsteuerungService
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Spiel ist nicht im Status <c>Geplant</c>.</exception>
    public void SpielStarten(Spiel spiel)
    {
        if (spiel.Status != SpielStatus.Geplant)
            throw new InvalidOperationException(
                $"Nur geplante Spiele können gestartet werden (aktueller Status: {spiel.Status}).");

        spiel.Status = SpielStatus.Laeuft;
        spiel.StartZeitpunktUtc = DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public void NaechesDuellStarten(Spiel spiel, Guid spieler1Id, Guid spieler2Id)
    {
        var duell = new Einzelduell
        {
            SpielId    = spiel.Id,
            Duellnummer = spiel.Einzelduelle.Count + 1,
            IstStechen  = false,
            Spieler1Id  = spieler1Id,
            Spieler2Id  = spieler2Id
        };

        spiel.Einzelduelle.Add(duell);
    }

    /// <inheritdoc/>
    public void StechenStarten(Spiel spiel, Guid spieler1Id, Guid spieler2Id)
    {
        var duell = new Einzelduell
        {
            SpielId     = spiel.Id,
            Duellnummer = spiel.Einzelduelle.Count + 1,
            IstStechen  = true,
            Spieler1Id  = spieler1Id,
            Spieler2Id  = spieler2Id
        };

        spiel.Einzelduelle.Add(duell);
    }

    /// <inheritdoc/>
    public void VersuchErfassen(Spiel spiel, bool spieler1Getroffen, bool spieler2Getroffen)
    {
        var duell = spiel.Einzelduelle.Last();

        var versuch = new Versuch
        {
            EinzelduellId  = duell.Id,
            Versuchnummer  = duell.Versuche.Count + 1,
            Spieler1Getroffen = spieler1Getroffen,
            Spieler2Getroffen = spieler2Getroffen
        };

        duell.Versuche.Add(versuch);

        if (spieler1Getroffen && !spieler2Getroffen)
        {
            // Team 1 hat eindeutig gewonnen
            DuellAbschliessen(duell, spiel.Team1Id, dp1: 1, dp2: 0);
        }
        else if (spieler2Getroffen && !spieler1Getroffen)
        {
            // Team 2 hat eindeutig gewonnen
            DuellAbschliessen(duell, spiel.Team2Id, dp1: 0, dp2: 1);
        }
        else if (duell.Versuche.Count == 3)
        {
            // Maximale Versuchsanzahl erreicht: Entscheidung nach Trefferhistorie
            // Fall A – keiner hat je getroffen: 0:0, kein Sieger
            // Fall B – beide haben mindestens einmal getroffen: 1:1, kein Sieger
            bool jemandHatGetroffen = duell.Versuche.Any(v => v.Spieler1Getroffen);
            int dp = jemandHatGetroffen ? 1 : 0;
            DuellAbschliessen(duell, siegerId: null, dp1: dp, dp2: dp);
        }
        // sonst: Versuch unentschieden, Duell läuft weiter
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Spiel ist nicht im Status <c>Laeuft</c>.</exception>
    public void SpielAbschliessen(Spiel spiel, Turnier turnier)
    {
        if (spiel.Status != SpielStatus.Laeuft)
            throw new InvalidOperationException(
                $"Nur laufende Spiele können abgeschlossen werden (aktueller Status: {spiel.Status}).");

        var abgeschlosseneDuelle = spiel.Einzelduelle
            .Where(d => d.Ergebnis is not null)
            .ToList();

        int dp1 = abgeschlosseneDuelle.Sum(d => d.Ergebnis!.DuellpunktTeam1);
        int dp2 = abgeschlosseneDuelle.Sum(d => d.Ergebnis!.DuellpunktTeam2);

        Guid siegerId = dp1 >= dp2 ? spiel.Team1Id : spiel.Team2Id;

        // Stechen liegt vor, wenn mindestens ein Duell als Stechen markiert wurde
        var art = spiel.Einzelduelle.Any(d => d.IstStechen)
            ? EntscheidungsArt.Stechen
            : EntscheidungsArt.RegulaereSpielzeit;

        var ergebnis = new SpielErgebnis
        {
            SiegerId         = siegerId,
            DuellpunkteTeam1 = dp1,
            DuellpunkteTeam2 = dp2,
            EntschiedenDurch = art
        };

        // Status und Zeitstempel vor TabellenPunkteBerechnen setzen,
        // da WertungsService Status == Abgeschlossen voraussetzt
        spiel.Status          = SpielStatus.Abgeschlossen;
        spiel.EndZeitpunktUtc = DateTime.UtcNow;
        spiel.Ergebnis        = ergebnis;

        var (tp1, tp2) = new WertungsService().TabellenPunkteBerechnen(spiel, turnier.Wertungssystem);
        ergebnis.TabellenPunkteTeam1 = tp1;
        ergebnis.TabellenPunkteTeam2 = tp2;
    }

    /// <summary>
    /// Setzt das Ergebnis eines Einzelduells nach Entscheidung.
    /// </summary>
    /// <param name="duell">Das zu schließende Duell.</param>
    /// <param name="siegerId">Team-Id des Siegers; null bei Unentschieden.</param>
    /// <param name="dp1">Duellpunkt für Team 1 (0 oder 1).</param>
    /// <param name="dp2">Duellpunkt für Team 2 (0 oder 1).</param>
    private static void DuellAbschliessen(Einzelduell duell, Guid? siegerId, int dp1, int dp2)
    {
        duell.Ergebnis = new EinzelduellErgebnis
        {
            SiegerId        = siegerId,
            DuellpunktTeam1 = dp1,
            DuellpunktTeam2 = dp2
        };
    }
}
