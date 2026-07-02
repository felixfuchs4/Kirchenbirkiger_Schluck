/*************************************************************
 * Datei:        SpielplanService.cs
 * Zweck:        Round-Robin-Spielplangenerierung, Spielreihenfolge und Nächstes-Spiel-Ermittlung
 * Bereich:      Anwendungslogik – Spielplanung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Services;

/// <summary>
/// Stellt Operationen zur Generierung und Verwaltung des Spielplans bereit.
/// </summary>
public class SpielplanService : ISpielplanService
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Das Turnier hat keine Gruppen, oder eine Gruppe enthält weniger als zwei Teams.
    /// </exception>
    public void GruppenspielplanGenerieren(Turnier turnier)
    {
        if (turnier.Gruppen.Count == 0)
            throw new InvalidOperationException("Das Turnier enthält keine Gruppen.");

        foreach (var gruppe in turnier.Gruppen)
        {
            if (gruppe.TeamIds.Count < 2)
                throw new InvalidOperationException(
                    $"Gruppe \"{gruppe.Name}\" enthält weniger als zwei Teams.");
        }

        // Globale Spielnummer fortsetzen, falls bereits Spiele existieren
        int spielnummer = AlleSpiele(turnier)
            .Select(s => s.Spielnummer)
            .DefaultIfEmpty(0)
            .Max() + 1;

        // Round-Robin nach Kreis-Verfahren: je Gruppe Runden, in denen jede
        // Mannschaft höchstens einmal spielt (kein Team zweimal hintereinander).
        var rundenProGruppe = turnier.Gruppen
            .Select(g => RoundRobinRunden(g.TeamIds))
            .ToList();

        int maxRunden = rundenProGruppe.Select(r => r.Count).DefaultIfEmpty(0).Max();

        // Reihenfolge: Runde für Runde, je Begegnung abwechselnd Gruppe A, Gruppe B, …
        for (int runde = 0; runde < maxRunden; runde++)
        {
            int maxBegegnungen = rundenProGruppe
                .Where(r => runde < r.Count)
                .Select(r => r[runde].Count)
                .DefaultIfEmpty(0)
                .Max();

            for (int slot = 0; slot < maxBegegnungen; slot++)
            {
                for (int gi = 0; gi < turnier.Gruppen.Count; gi++)
                {
                    var runden = rundenProGruppe[gi];
                    if (runde >= runden.Count || slot >= runden[runde].Count)
                        continue;

                    var (team1Id, team2Id) = runden[runde][slot];
                    var gruppe = turnier.Gruppen[gi];

                    gruppe.Spiele.Add(new Spiel
                    {
                        GruppeId    = gruppe.Id,
                        Team1Id     = team1Id,
                        Team2Id     = team2Id,
                        Spielnummer = spielnummer++,
                        Status      = SpielStatus.Geplant
                    });
                }
            }
        }
    }

    /// <inheritdoc/>
    public Spiel? NaechstesSpielErmitteln(Turnier turnier)
        => AlleSpiele(turnier)
            .Where(s => s.Status == SpielStatus.Geplant)
            .OrderBy(s => s.Spielnummer)
            .FirstOrDefault();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Spiel hat keinen Nachfolger im Spielplan.</exception>
    public void SpielNachHintenVerschieben(Turnier turnier, Guid spielId)
    {
        var alle = AlleSpiele(turnier)
            .OrderBy(s => s.Spielnummer)
            .ToList();

        var ziel = alle.Single(s => s.Id == spielId);
        var naechstes = alle.FirstOrDefault(s => s.Spielnummer > ziel.Spielnummer);

        if (naechstes is null)
            throw new InvalidOperationException(
                $"Spiel {spielId} ist bereits das letzte im Spielplan und kann nicht weiter verschoben werden.");

        // Spielnummern tauschen
        (ziel.Spielnummer, naechstes.Spielnummer) = (naechstes.Spielnummer, ziel.Spielnummer);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Das Turnier hat weniger als zwei Gruppen.</exception>
    public void FinalrundeGenerieren(Turnier turnier)
    {
        if (turnier.Gruppen.Count < 2)
            throw new InvalidOperationException(
                "Die Finalrunde benötigt mindestens zwei Gruppen.");

        var wertung = new WertungsService();

        // Gruppen-Rankings als geordnete Team-Id-Listen (bester → schlechtester)
        var a = wertung.GruppenRanglisteBerechnen(turnier.Gruppen[0], turnier.Wertungssystem)
                       .Select(e => e.TeamId).ToList();
        var b = wertung.GruppenRanglisteBerechnen(turnier.Gruppen[1], turnier.Wertungssystem)
                       .Select(e => e.TeamId).ToList();

        turnier.Finalrundenspiele.Clear();

        int nr = AlleSpiele(turnier).Select(s => s.Spielnummer).DefaultIfEmpty(0).Max() + 1;

        if (turnier.FinalrundenModus == FinalrundenModus.Kurz)
            GenerierenKurz(turnier, a, b, ref nr);
        else
            GenerierenKoBaum(turnier, a, b, ref nr);
    }

    /// <inheritdoc/>
    public void BracketFortsetzungAktualisieren(Turnier turnier, Spiel abgeschlossenesSpiel)
    {
        if (abgeschlossenesSpiel.Ergebnis is null)
            return;

        var siegerId = abgeschlossenesSpiel.Ergebnis.SiegerId;

        // Verlierer ermitteln (das Team, das nicht gewonnen hat)
        Guid? verliererId = abgeschlossenesSpiel.Team1Id == siegerId
            ? abgeschlossenesSpiel.Team2Id
            : abgeschlossenesSpiel.Team1Id;

        foreach (var spiel in turnier.Finalrundenspiele)
        {
            // Bei „Spiel um Platz 3" rückt der Verlierer nach, sonst der Sieger
            var nachrueckend = spiel.VorgaengerVerlierer ? verliererId : siegerId;

            if (spiel.VorgaengerSpiel1Id == abgeschlossenesSpiel.Id)
                spiel.Team1Id = nachrueckend;

            if (spiel.VorgaengerSpiel2Id == abgeschlossenesSpiel.Id)
                spiel.Team2Id = nachrueckend;
        }
    }

    // ---------------------------------------------------------------------------
    // Private Hilfsmethoden – Finalrunden-Generierung
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Kurze Finalrunde: Gleichplatzierte aus beiden Gruppen spielen direkt gegeneinander.
    /// </summary>
    private static void GenerierenKurz(Turnier turnier, List<Guid> a, List<Guid> b, ref int nr)
    {
        int anzahl = Math.Min(a.Count, b.Count);
        // Absteigend erzeugen, damit die niedrigste Platzierung (z. B. „Platz 5/6") zuerst
        // gespielt wird und das Spiel um Platz 1/2 die höchste Spielnummer erhält (= letztes Spiel).
        for (int i = anzahl - 1; i >= 0; i--)
        {
            turnier.Finalrundenspiele.Add(new Spiel
            {
                Team1Id      = a[i],
                Team2Id      = b[i],
                Spielnummer  = nr++,
                BracketRunde = $"Platz {i * 2 + 1}/{i * 2 + 2}",
                Status       = SpielStatus.Geplant
            });
        }
    }

    /// <summary>
    /// KO-Turnierbaum: obere Hälfte (Gruppe A primär) und untere Hälfte (Gruppe B primär).
    /// Generiert alle Runden sofort; Folgerunden erhalten Platzhalter (Team1Id/Team2Id = null).
    /// </summary>
    private static void GenerierenKoBaum(Turnier turnier, List<Guid> a, List<Guid> b, ref int nr)
    {
        // Obere Hälfte: a[0]=A1 Freilos, a[1]=A2 Freilos, a[2..3] spielen gegen B-Füller von unten
        // Untere Hälfte: b[0]=B1 Freilos, b[1]=B2 Freilos, b[2..3] spielen gegen A-Füller von unten

        // B-Füller für obere Hälfte: b[n-2], b[n-1] (die schwächsten B-Teams, die NICHT Primärspieler sind)
        var bFueller = b.Skip(4).ToList();  // B5, B6 für n=6
        // A-Füller für untere Hälfte: a[n-2], a[n-1] (die schwächsten A-Teams)
        var aFueller = a.Skip(4).ToList();  // A5, A6 für n=6

        // Achtelfinale – obere Hälfte: A3/A4 vs B6/B5
        var topAchtel = new List<Spiel>();
        var aPlayer = a.Skip(2).Take(2).ToList();  // A3, A4
        for (int i = 0; i < Math.Min(aPlayer.Count, bFueller.Count); i++)
        {
            var spiel = ErstelleSpiel(aPlayer[i], bFueller[bFueller.Count - 1 - i],
                                      null, null, "Achtelfinale", nr++);
            topAchtel.Add(spiel);
            turnier.Finalrundenspiele.Add(spiel);
        }

        // Achtelfinale – untere Hälfte: B3/B4 vs A6/A5
        var bottomAchtel = new List<Spiel>();
        var bPlayer = b.Skip(2).Take(2).ToList();  // B3, B4
        for (int i = 0; i < Math.Min(bPlayer.Count, aFueller.Count); i++)
        {
            var spiel = ErstelleSpiel(bPlayer[i], aFueller[aFueller.Count - 1 - i],
                                      null, null, "Achtelfinale", nr++);
            bottomAchtel.Add(spiel);
            turnier.Finalrundenspiele.Add(spiel);
        }

        // Viertelfinale – obere Hälfte
        // VF-TL: A2 (Freilos) vs Sieger(Achtelfinale-Top-0)
        var vfTL = ErstelleSpiel(
            team1: a.Count > 1 ? a[1] : null,
            team2: topAchtel.Count > 0 ? null : (a.Count > 2 ? a[2] : null),
            vorg1: null,
            vorg2: topAchtel.Count > 0 ? topAchtel[0].Id : null,
            runde: "Viertelfinale", nr: nr++);

        // VF-TR: Sieger(Achtelfinale-Top-1) vs A1 (Freilos)
        var vfTR = ErstelleSpiel(
            team1: topAchtel.Count > 1 ? null : (a.Count > 3 ? a[3] : null),
            team2: a.Count > 0 ? a[0] : null,
            vorg1: topAchtel.Count > 1 ? topAchtel[1].Id : null,
            vorg2: null,
            runde: "Viertelfinale", nr: nr++);

        // Viertelfinale – untere Hälfte
        // VF-BL: B2 (Freilos) vs Sieger(Achtelfinale-Bottom-0)
        var vfBL = ErstelleSpiel(
            team1: b.Count > 1 ? b[1] : null,
            team2: bottomAchtel.Count > 0 ? null : (b.Count > 2 ? b[2] : null),
            vorg1: null,
            vorg2: bottomAchtel.Count > 0 ? bottomAchtel[0].Id : null,
            runde: "Viertelfinale", nr: nr++);

        // VF-BR: Sieger(Achtelfinale-Bottom-1) vs B1 (Freilos)
        var vfBR = ErstelleSpiel(
            team1: bottomAchtel.Count > 1 ? null : (b.Count > 3 ? b[3] : null),
            team2: b.Count > 0 ? b[0] : null,
            vorg1: bottomAchtel.Count > 1 ? bottomAchtel[1].Id : null,
            vorg2: null,
            runde: "Viertelfinale", nr: nr++);

        turnier.Finalrundenspiele.AddRange([vfTL, vfTR, vfBL, vfBR]);

        // Halbfinale
        var hfTop = ErstelleSpiel(null, null, vfTL.Id, vfTR.Id, "Halbfinale", nr++);
        var hfBot = ErstelleSpiel(null, null, vfBL.Id, vfBR.Id, "Halbfinale", nr++);
        turnier.Finalrundenspiele.AddRange([hfTop, hfBot]);

        // Spiel um Platz 3 + Finale.
        // Das Spiel um Platz 3 wird direkt vor dem Finale gespielt (kleinere Spielnummer).
        // Spiel um Platz 3: die beiden Halbfinal-Verlierer treten gegeneinander an
        var platz3 = ErstelleSpiel(null, null, hfTop.Id, hfBot.Id, "Spiel um Platz 3", nr++);
        platz3.VorgaengerVerlierer = true;
        turnier.Finalrundenspiele.Add(platz3);

        turnier.Finalrundenspiele.Add(
            ErstelleSpiel(null, null, hfTop.Id, hfBot.Id, "Finale", nr++));
    }

    /// <summary>
    /// Erstellt ein einzelnes Finalrundenspiel mit Bracket-Metadaten.
    /// </summary>
    private static Spiel ErstelleSpiel(
        Guid? team1, Guid? team2,
        Guid? vorg1, Guid? vorg2,
        string runde, int nr)
        => new Spiel
        {
            Team1Id            = team1,
            Team2Id            = team2,
            VorgaengerSpiel1Id = vorg1,
            VorgaengerSpiel2Id = vorg2,
            BracketRunde       = runde,
            Spielnummer        = nr,
            Status             = SpielStatus.Geplant
        };

    /// <summary>
    /// Gibt alle Spiele des Turniers zurück (Gruppenspiele aller Gruppen + Finalrundenspiele).
    /// </summary>
    private static IEnumerable<Spiel> AlleSpiele(Turnier turnier)
        => turnier.Gruppen.SelectMany(g => g.Spiele)
                  .Concat(turnier.Finalrundenspiele);

    /// <summary>
    /// Berechnet alle Round-Robin-Paare für eine Gruppe in lexikografischer Reihenfolge.
    /// </summary>
    /// <param name="gruppe">Die Gruppe, für die Paare gebildet werden sollen.</param>
    /// <returns>Liste der Team-Id-Paare (Team1Id, Team2Id).</returns>
    /// <inheritdoc/>
    public int PlatzierungsStechenErzeugen(Turnier turnier)
    {
        var wertung = new WertungsService();
        int erzeugt = 0;
        int nr = AlleSpiele(turnier).Select(s => s.Spielnummer).DefaultIfEmpty(0).Max() + 1;

        foreach (var gruppe in turnier.Gruppen)
        {
            var rangliste = wertung.GruppenRanglisteBerechnen(gruppe, turnier.Wertungssystem);

            // Teams, die noch ein Stechen benötigen, nach Punktstand (Tie-Ebene) gruppieren
            var stechenGruppen = rangliste
                .Where(e => e.StehenErforderlich)
                .GroupBy(e => e.Tabellenpunkte)
                .Where(g => g.Count() > 1);

            foreach (var tieGruppe in stechenGruppen)
            {
                var ids = tieGruppe.Select(e => e.TeamId).ToList();

                // Round-Robin unter den gleichplatzierten Teams – fehlende Begegnungen anlegen
                for (int i = 0; i < ids.Count; i++)
                {
                    for (int j = i + 1; j < ids.Count; j++)
                    {
                        var a = ids[i];
                        var b = ids[j];

                        bool existiert = gruppe.Spiele.Any(s => s.IstPlatzierungsStechen
                            && ((s.Team1Id == a && s.Team2Id == b)
                             || (s.Team1Id == b && s.Team2Id == a)));
                        if (existiert) continue;

                        gruppe.Spiele.Add(new Spiel
                        {
                            GruppeId               = gruppe.Id,
                            Team1Id                = a,
                            Team2Id                = b,
                            Spielnummer            = nr++,
                            Status                 = SpielStatus.Geplant,
                            IstPlatzierungsStechen = true,
                            BracketRunde           = "Platzierungs-Stechen"
                        });
                        erzeugt++;
                    }
                }
            }
        }

        return erzeugt;
    }

    private static List<List<(Guid Team1Id, Guid Team2Id)>> RoundRobinRunden(List<Guid> teamIds)
    {
        var teams = new List<Guid>(teamIds);
        if (teams.Count < 2)
            return [];

        // Bei ungerader Teamzahl ein „Freilos" (Guid.Empty) ergänzen
        if (teams.Count % 2 == 1)
            teams.Add(Guid.Empty);

        int n = teams.Count;
        int rundenAnzahl = n - 1;
        int haelfte = n / 2;

        var ergebnis = new List<List<(Guid, Guid)>>();
        var liste = teams.ToList();

        for (int r = 0; r < rundenAnzahl; r++)
        {
            var paare = new List<(Guid, Guid)>();
            for (int i = 0; i < haelfte; i++)
            {
                var a = liste[i];
                var b = liste[n - 1 - i];

                // Freilos-Begegnungen überspringen
                if (a != Guid.Empty && b != Guid.Empty)
                    paare.Add((a, b));
            }
            ergebnis.Add(paare);

            // Rotation: erstes Element bleibt fix, der Rest dreht im Uhrzeigersinn
            var letzte = liste[n - 1];
            for (int i = n - 1; i > 1; i--)
                liste[i] = liste[i - 1];
            liste[1] = letzte;
        }

        return ergebnis;
    }
}
