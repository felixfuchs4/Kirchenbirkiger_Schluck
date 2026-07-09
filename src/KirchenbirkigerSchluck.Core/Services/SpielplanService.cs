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
    /// <exception cref="InvalidOperationException">Das Turnier hat keine Gruppen oder zu wenige qualifizierte Teams.</exception>
    public void FinalrundeGenerieren(Turnier turnier) => FinalrundeGenerieren(turnier, new Random());

    /// <summary>
    /// Wie <see cref="FinalrundeGenerieren(Turnier)"/>, jedoch mit injizierbarer Zufallsquelle für
    /// die Auslosung der Freilose (deterministische Tests). Die parameterlose Überladung verwendet
    /// eine frische <see cref="Random"/>-Instanz.
    /// </summary>
    /// <param name="turnier">Das Turnier, für das die Finalrunde erzeugt wird.</param>
    /// <param name="rng">Zufallsquelle für die ebeneninterne Auslosung.</param>
    /// <exception cref="InvalidOperationException">Das Turnier hat keine Gruppen oder zu wenige qualifizierte Teams.</exception>
    public void FinalrundeGenerieren(Turnier turnier, Random rng)
    {
        if (turnier.Gruppen.Count == 0)
            throw new InvalidOperationException("Die Finalrunde benötigt mindestens eine Gruppe.");

        turnier.Finalrundenspiele.Clear();

        int nr = AlleSpiele(turnier).Select(s => s.Spielnummer).DefaultIfEmpty(0).Max() + 1;

        // Die kurze Finalrunde ist ausschließlich für genau zwei Gruppen definiert.
        if (turnier.Gruppen.Count == 2 && turnier.FinalrundenModus == FinalrundenModus.Kurz)
        {
            var wertung = new WertungsService();
            var a = wertung.GruppenRanglisteBerechnen(turnier.Gruppen[0], turnier.Wertungssystem)
                           .Select(e => e.TeamId).ToList();
            var b = wertung.GruppenRanglisteBerechnen(turnier.Gruppen[1], turnier.Wertungssystem)
                           .Select(e => e.TeamId).ToList();
            GenerierenKurz(turnier, a, b, ref nr);
            return;
        }

        // Alle übrigen Fälle (jede Gruppenanzahl, Modus KoBaumEin): einheitlicher generischer
        // KO-Baum über alle Teams – lückenlos, mit Freilosen an die Bestplatzierten und
        // gruppentrennender Setzung.
        GenerierenKoBaumGenerisch(turnier, ref nr, rng);
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
    /// Einheitlicher KO-Turnierbaum über <b>alle</b> Teams sämtlicher Gruppen (jede Gruppenanzahl).
    /// Setzt die Teams ebenenweise (alle Gruppensieger, dann alle Zweiten, …), baut einen
    /// Single-Elimination-Baum der nächsten Zweierpotenz auf und vergibt Freilose nur, wenn die
    /// Teamzahl keine Zweierpotenz ist – stets an die bestplatzierten Teams. Die Zuordnung trennt
    /// Teams derselben Gruppe so weit wie möglich (späteste mögliche Begegnung).
    /// </summary>
    /// <param name="turnier">Das Turnier, dessen Finalrunde befüllt wird.</param>
    /// <param name="nr">Laufende Spielnummer (wird fortgeschrieben).</param>
    /// <param name="rng">Zufallsquelle für die Auslosung.</param>
    /// <exception cref="InvalidOperationException">Weniger als zwei qualifizierte Teams.</exception>
    private void GenerierenKoBaumGenerisch(Turnier turnier, ref int nr, Random rng)
    {
        var wertung = new WertungsService();

        // Rangliste je Gruppe (bester → schlechtester)
        var ranglisten = turnier.Gruppen
            .Select(g => wertung.GruppenRanglisteBerechnen(g, turnier.Wertungssystem)
                                .Select(e => e.TeamId).ToList())
            .ToList();

        // Platzierungs-Ebenen: Ebene k enthält je ein Team pro Gruppe mit mindestens k+1 Teams
        // (Ebene 0 = alle Gruppensieger, Ebene 1 = alle Zweiten, …). Die Gruppenzugehörigkeit je
        // Team wird für die spätere Gruppentrennung mitgeführt.
        int maxTiefe = ranglisten.Select(r => r.Count).DefaultIfEmpty(0).Max();
        var ebenen = new List<List<Guid>>();
        var gruppeVon = new Dictionary<Guid, int>();
        int teamAnzahl = 0;
        for (int ebene = 0; ebene < maxTiefe; ebene++)
        {
            var ebenenTeams = new List<Guid>();
            for (int gi = 0; gi < ranglisten.Count; gi++)
            {
                if (ebene >= ranglisten[gi].Count) continue;
                var teamId = ranglisten[gi][ebene];
                gruppeVon[teamId] = gi;
                ebenenTeams.Add(teamId);
            }
            ebenen.Add(ebenenTeams);
            teamAnzahl += ebenenTeams.Count;
        }

        if (teamAnzahl < 2)
            throw new InvalidOperationException(
                "Die Finalrunde benötigt mindestens zwei qualifizierte Teams.");

        // Bracketgröße = kleinste Zweierpotenz ≥ Teamzahl; Freilose = Bracketgröße − Teamzahl.
        int n = NaechsteZweierpotenz(teamAnzahl);
        var positionen = SetzPositionen(n);

        // Jedem Bracket-Slot seine Ebene zuordnen: Slot mit Seed-Rang r gehört zur Ebene, in deren
        // kumulierten Bereich r fällt; Ränge > Teamzahl sind Freilose. So bleiben Freilose an den
        // besten Rängen (= mit den Bestplatzierten gepaart).
        var slotBelegung = new BracketSlot?[n];
        var slotsProEbene = new List<List<int>>();
        for (int e = 0; e < ebenen.Count; e++) slotsProEbene.Add([]);

        int grenzeUnten = 0;
        var rangGrenzen = new List<(int Start, int Ende)>();
        foreach (var ebeneTeams in ebenen)
        {
            rangGrenzen.Add((grenzeUnten, grenzeUnten + ebeneTeams.Count)); // [Start, Ende)
            grenzeUnten += ebeneTeams.Count;
        }

        for (int slot = 0; slot < n; slot++)
        {
            int rang = positionen[slot]; // 1-basiert
            if (rang > teamAnzahl)
            {
                slotBelegung[slot] = BracketSlot.Freilos();
                continue;
            }
            int rang0 = rang - 1;
            int ebeneIndex = rangGrenzen.FindIndex(g => rang0 >= g.Start && rang0 < g.Ende);
            slotsProEbene[ebeneIndex].Add(slot);
        }

        // Teams auf ihre Ebenen-Slots verteilen und dabei die Gruppen so gut wie möglich durchmischen
        // (Teams derselben Gruppe treffen erst möglichst spät aufeinander).
        BesteBelegungFinden(slotBelegung, ebenen, slotsProEbene, gruppeVon, n, rng);

        var slots = slotBelegung.Select(s => s!).ToList();

        // Runden erzeugen, bis nur noch zwei Zubringer des Finales übrig sind.
        while (slots.Count > 2)
        {
            string runde = RundenName(slots.Count);
            var naechste = new List<BracketSlot>();
            for (int i = 0; i < slots.Count; i += 2)
                naechste.Add(PaarungVerarbeiten(turnier, slots[i], slots[i + 1], runde, ref nr));
            slots = naechste;
        }

        var zubringer1 = slots[0];
        var zubringer2 = slots[1];

        // Spiel um Platz 3 nur, wenn beide Zubringer echte Halbfinal-Spiele sind. Bei einem
        // freilosbedingt einzelnen Halbfinale ist dessen Verlierer automatisch Dritter.
        if (zubringer1.VorgaengerSpielId is { } hf1 && zubringer2.VorgaengerSpielId is { } hf2)
        {
            var platz3 = ErstelleSpiel(null, null, hf1, hf2, "Spiel um Platz 3", nr++);
            platz3.VorgaengerVerlierer = true;
            turnier.Finalrundenspiele.Add(platz3);
        }

        // Finale (höchste Spielnummer)
        turnier.Finalrundenspiele.Add(ErstelleSpiel(
            zubringer1.TeamId, zubringer2.TeamId,
            zubringer1.VorgaengerSpielId, zubringer2.VorgaengerSpielId,
            "Finale", nr++));
    }

    /// <summary>
    /// Verarbeitet eine Bracket-Paarung: Bei zwei echten Teilnehmern entsteht ein Spiel (dessen
    /// Sieger nachrückt); trifft ein Teilnehmer auf ein Freilos, rückt er ohne Spiel weiter.
    /// </summary>
    private static BracketSlot PaarungVerarbeiten(
        Turnier turnier, BracketSlot a, BracketSlot b, string runde, ref int nr)
    {
        if (a.IstFreilos && b.IstFreilos)
            throw new InvalidOperationException(
                "Ungültige Bracket-Konfiguration: zwei Freilose wurden gepaart.");

        // Freilos: der andere Teilnehmer rückt kampflos in die nächste Runde.
        if (a.IstFreilos) return b;
        if (b.IstFreilos) return a;

        var spiel = ErstelleSpiel(
            a.TeamId, b.TeamId, a.VorgaengerSpielId, b.VorgaengerSpielId, runde, nr++);
        turnier.Finalrundenspiele.Add(spiel);
        return BracketSlot.AusVorgaenger(spiel.Id);
    }

    /// <summary>Kleinste Zweierpotenz, die ≥ <paramref name="wert"/> ist.</summary>
    private static int NaechsteZweierpotenz(int wert)
    {
        int n = 1;
        while (n < wert) n <<= 1;
        return n;
    }

    /// <summary>
    /// Liefert die Standard-Setzpositionen eines KO-Baums der Größe <paramref name="n"/> (Zweierpotenz)
    /// als 1-basierte Seed-Nummern in Slot-Reihenfolge (Seed 1 gegen Seed n, Seed 2 gegen n−1 usw.).
    /// </summary>
    private static List<int> SetzPositionen(int n)
    {
        var positionen = new List<int> { 1 };
        while (positionen.Count < n)
        {
            int summe = positionen.Count * 2 + 1;
            var erweitert = new List<int>(positionen.Count * 2);
            foreach (var seed in positionen)
            {
                erweitert.Add(seed);
                erweitert.Add(summe - seed);
            }
            positionen = erweitert;
        }
        return positionen;
    }

    /// <summary>Rundenname anhand der Teilnehmerzahl <paramref name="teilnehmer"/> dieser Runde.</summary>
    private static string RundenName(int teilnehmer) => teilnehmer switch
    {
        2  => "Finale",
        4  => "Halbfinale",
        8  => "Viertelfinale",
        16 => "Achtelfinale",
        32 => "Sechzehntelfinale",
        64 => "Zweiunddreißigstelfinale",
        _  => $"Runde der letzten {teilnehmer}"
    };

    /// <summary>Mischt die Liste in-place nach Fisher-Yates (Auslosung).</summary>
    private static void Mischen<T>(IList<T> liste, Random rng)
    {
        for (int i = liste.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (liste[i], liste[j]) = (liste[j], liste[i]);
        }
    }

    /// <summary>
    /// Belegt die Ebenen-Slots des Brackets mit Teams so, dass Teams derselben Gruppe möglichst spät
    /// aufeinandertreffen (Gruppendurchmischung). Es werden mehrere ausgeloste Belegungen erzeugt und
    /// die mit den geringsten Trennungskosten gewählt; permutiert wird nur innerhalb einer Ebene, die
    /// Freilos-Slots (an den besten Rängen) bleiben unberührt.
    /// </summary>
    private static void BesteBelegungFinden(
        BracketSlot?[] belegung, List<List<Guid>> ebenen, List<List<int>> slotsProEbene,
        IReadOnlyDictionary<Guid, int> gruppeVon, int n, Random rng)
    {
        const int Versuche = 300;
        int maxRunden = BegegnungsEbene(0, n - 1); // = log2(n)

        var arbeit = new Guid[n];              // Slot → Team (Guid.Empty = Freilos/leer)
        Guid[]? besteBelegung = null;
        long besteKosten = long.MaxValue;

        for (int versuch = 0; versuch < Versuche; versuch++)
        {
            Array.Clear(arbeit);
            for (int e = 0; e < ebenen.Count; e++)
            {
                var teams = new List<Guid>(ebenen[e]);
                Mischen(teams, rng); // Auslosung innerhalb der Ebene
                var eslots = slotsProEbene[e];
                for (int i = 0; i < eslots.Count; i++)
                    arbeit[eslots[i]] = teams[i];
            }

            long kosten = TrennungsKosten(arbeit, gruppeVon, n, maxRunden);
            if (kosten < besteKosten)
            {
                besteKosten = kosten;
                besteBelegung = (Guid[])arbeit.Clone();
                if (kosten == 0) break; // optimale Durchmischung gefunden
            }
        }

        foreach (var slot in slotsProEbene.SelectMany(s => s))
            belegung[slot] = BracketSlot.AusTeam(besteBelegung![slot]);
    }

    /// <summary>
    /// Bewertet eine Slot-Belegung: für jedes Paar gleicher Gruppe fließt eine Strafe ein, die umso
    /// höher ist, je früher sich die beiden treffen würden (Runde-1-Begegnungen dominieren). 0 = keine
    /// zwei Teams derselben Gruppe treffen früher als nötig aufeinander.
    /// </summary>
    private static long TrennungsKosten(
        Guid[] slotTeam, IReadOnlyDictionary<Guid, int> gruppeVon, int n, int maxRunden)
    {
        long kosten = 0;
        for (int i = 0; i < n; i++)
        {
            if (slotTeam[i] == Guid.Empty || !gruppeVon.TryGetValue(slotTeam[i], out var gi)) continue;
            for (int j = i + 1; j < n; j++)
            {
                if (slotTeam[j] == Guid.Empty || !gruppeVon.TryGetValue(slotTeam[j], out var gj)) continue;
                if (gi != gj) continue;
                int runde = BegegnungsEbene(i, j);
                kosten += 1L << (maxRunden - runde); // frühere Begegnung → höhere Strafe
            }
        }
        return kosten;
    }

    /// <summary>
    /// Runde, in der sich die beiden Leaf-Slots <paramref name="slotA"/> und <paramref name="slotB"/>
    /// frühestens treffen (1 = direkte Paarung in Runde 1; größer = später). Entspricht der Anzahl
    /// Halbierungen, bis beide im selben Teilbaum liegen.
    /// </summary>
    private static int BegegnungsEbene(int slotA, int slotB)
    {
        int k = 0;
        while (slotA != slotB)
        {
            slotA >>= 1;
            slotB >>= 1;
            k++;
        }
        return k;
    }

    /// <summary>
    /// Ein Platz im KO-Baum: entweder ein bereits feststehendes Team (direkte Setzung oder kampflos
    /// vorgerückt), der Sieger eines Vorgängerspiels, oder ein Freilos (beides null).
    /// </summary>
    private sealed class BracketSlot
    {
        /// <summary>Feststehendes Team dieses Slots; null bei Vorgänger-Slot oder Freilos.</summary>
        public Guid? TeamId { get; private init; }

        /// <summary>Vorgängerspiel, dessen Sieger einzieht; null bei Team-Slot oder Freilos.</summary>
        public Guid? VorgaengerSpielId { get; private init; }

        /// <summary>Wahr, wenn dieser Slot ein Freilos ist (weder Team noch Vorgängerspiel).</summary>
        public bool IstFreilos => TeamId is null && VorgaengerSpielId is null;

        /// <summary>Erzeugt einen Slot mit feststehendem Team.</summary>
        public static BracketSlot AusTeam(Guid teamId) => new() { TeamId = teamId };

        /// <summary>Erzeugt einen Slot, der vom Sieger eines Vorgängerspiels befüllt wird.</summary>
        public static BracketSlot AusVorgaenger(Guid spielId) => new() { VorgaengerSpielId = spielId };

        /// <summary>Erzeugt ein Freilos.</summary>
        public static BracketSlot Freilos() => new();
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

            // Teams, die noch ein Stechen benötigen, nach Punkten UND Torverhältnis (Tie-Ebene)
            // gruppieren – getrennte Gleichstände gleicher Punktzahl dürfen nicht vermischt werden.
            var stechenGruppen = rangliste
                .Where(e => e.StehenErforderlich)
                .GroupBy(e => (e.Tabellenpunkte, e.DuellpunkteGewonnen - e.DuellpunkteVerloren))
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
