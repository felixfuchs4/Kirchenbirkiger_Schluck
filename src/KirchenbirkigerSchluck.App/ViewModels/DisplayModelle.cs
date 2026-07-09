/*************************************************************
 * Datei:        DisplayModelle.cs
 * Zweck:        Leichtgewichtige Anzeigemodelle für Listen und Tabellen in der UI
 * Bereich:      Präsentation – ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.App.ViewModels;

/// <summary>Eine Zeile in der Spielplanliste der Bedienoberfläche.</summary>
public sealed class SpielZeileModel
{
    /// <summary>Laufende Spielnummer.</summary>
    public int Nummer { get; init; }

    /// <summary>Name von Team 1.</summary>
    public string Team1 { get; init; } = string.Empty;

    /// <summary>Name von Team 2.</summary>
    public string Team2 { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad von Team 1.</summary>
    public string? Team1LogoPfad { get; init; }

    /// <summary>Optionaler Logo-Pfad von Team 2.</summary>
    public string? Team2LogoPfad { get; init; }

    /// <summary>Lesbare Status-Bezeichnung (z.B. „Geplant", „Läuft").</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Ergebnis als Text (z.B. „3:2"), oder leer wenn noch nicht gespielt.</summary>
    public string Ergebnis { get; init; } = string.Empty;

    /// <summary>Gruppenname oder Bracket-Runde (z.B. „Gruppe A", „Finale").</summary>
    public string Runde { get; init; } = string.Empty;

    /// <summary>Kennzeichnet das nächste zu spielende Spiel.</summary>
    public bool IstNaechstes { get; init; }

    /// <summary>Die interne Spiel-Id für Kommandos (z.B. Verschieben).</summary>
    public Guid SpielId { get; init; }
}

/// <summary>Eine Zeile in der Duellanzeige (Matchday-Screen und Spielsteuerung).</summary>
public sealed class DuellZeileModel
{
    /// <summary>Laufende Duellnummer (1–5 regulär, ab 6 Stechen).</summary>
    public int Duellnummer { get; init; }

    /// <summary>Name des Spielers von Team 1.</summary>
    public string Spieler1Name { get; init; } = string.Empty;

    /// <summary>Name des Spielers von Team 2.</summary>
    public string Spieler2Name { get; init; } = string.Empty;

    /// <summary>Ergebnistext, z.B. „1:0", „0:1", „1:1", oder „–" für ausstehend.</summary>
    public string ErgebnisText { get; init; } = "–";

    /// <summary>Dieses Duell ist das aktuell laufende.</summary>
    public bool IstAktiv { get; init; }

    /// <summary>Dieses Duell ist Teil des Stechens.</summary>
    public bool IstStechen { get; init; }

    /// <summary>Dieses Duell wurde noch nicht gestartet; wird als Vorschau angezeigt.</summary>
    public bool IstGeplant { get; init; }
}

/// <summary>Eine Zeile in der Gruppentabelle der Anzeigeoberfläche.</summary>
public sealed class GruppenTabellenEintragAnzeigeModel
{
    /// <summary>Tabellenposition (1-basiert).</summary>
    public int Platz { get; init; }

    /// <summary>Mannschaftsname.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Anzahl ausgetragener Spiele.</summary>
    public int Spiele { get; init; }

    /// <summary>Anzahl Siege.</summary>
    public int Siege { get; init; }

    /// <summary>Gewonnene Einzelduelle (für den Direkten Vergleich / Differenz).</summary>
    public int DuelleGewonnen { get; init; }

    /// <summary>Verlorene Einzelduelle.</summary>
    public int DuelleVerloren { get; init; }

    /// <summary>Duell-Differenz als Text mit Vorzeichen (z. B. „+3").</summary>
    public string DuellDifferenz { get; init; } = string.Empty;

    /// <summary>Gesamtpunktzahl.</summary>
    public int Punkte { get; init; }

    /// <summary>
    /// Kürzel zur Tiebreak-Kennzeichnung neben der Platzierung: „DV" (Direkter Vergleich),
    /// „S" (Stechen) oder leer, wenn die Platzierung ohne diese Kriterien feststeht.
    /// </summary>
    public string TiebreakKuerzel { get; init; } = string.Empty;

    /// <summary>Gibt an, ob ein Tiebreak-Kürzel angezeigt werden soll.</summary>
    public bool HatTiebreak => !string.IsNullOrEmpty(TiebreakKuerzel);
}

/// <summary>Eine Zeile in der Ehrung des treffsichersten Spielers (bei geteiltem Platz mehrere Zeilen je Schritt).</summary>
public sealed class SpielerZeileModel
{
    /// <summary>Name des Spielers.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Name des zugehörigen Teams.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad des Teams.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Detailangabe (Treffer bzw. Trefferquote).</summary>
    public string Detail { get; init; } = string.Empty;
}

/// <summary>Eine Zeile in der Endplatzierungsanzeige (Gewinner-Screen).</summary>
public sealed class EndplatzierungZeileModel
{
    /// <summary>Endplatzierung (1-basiert).</summary>
    public int Platz { get; init; }

    /// <summary>Mannschaftsname.</summary>
    public string TeamName { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad.</summary>
    public string? LogoPfad { get; init; }

    /// <summary>Gesamtpunktzahl über alle Spiele.</summary>
    public int Punkte { get; init; }

    /// <summary>Der Turniersieger wird auf der Anzeigeoberfläche besonders hervorgehoben.</summary>
    public bool IstSieger { get; init; }
}
