/*************************************************************
 * Datei:        AnzeigeZustandService.cs
 * Zweck:        Vermittler zwischen Bedienoberfläche und Anzeigeoberfläche
 * Bereich:      Präsentation – Dienste
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.Services;

/// <summary>Identifiziert die aktuell auf der Anzeigeoberfläche gezeigte Ansicht.</summary>
public enum AnzeigeScreen
{
    /// <summary>Startscreen vor Turnierbeginn (Logo + Uhrzeit).</summary>
    Startscreen,

    /// <summary>Infoscreen zwischen Spielen (rotierende Informationen).</summary>
    Infoscreen,

    /// <summary>Matchday-Screen während eines laufenden Spiels (Live-Ansicht).</summary>
    Matchday,

    /// <summary>Gewinner-Screen nach Turnierende (Endplatzierung).</summary>
    Gewinner,

    /// <summary>Animierte Gruppenauslosung für das Publikum.</summary>
    Auslosung
}

/// <summary>Ein einzelner Ziehungsschritt der Auslosung (ein Team in eine Gruppe).</summary>
/// <param name="TeamName">Name des gezogenen Teams.</param>
/// <param name="LogoPfad">Optionaler Pfad zum Team-Logo.</param>
/// <param name="GruppenIndex">Index der Zielgruppe (0-basiert).</param>
/// <param name="GruppenName">Anzeigename der Zielgruppe.</param>
public sealed record AuslosungEintrag(string TeamName, string? LogoPfad, int GruppenIndex, string GruppenName);

/// <summary>Vollständige Daten einer Auslosung für die animierte Beamer-Darstellung.</summary>
public sealed class AuslosungDaten
{
    /// <summary>Anzahl der Gruppen.</summary>
    public int AnzahlGruppen { get; init; }

    /// <summary>Namen der Gruppen in Reihenfolge.</summary>
    public IReadOnlyList<string> Gruppennamen { get; init; } = [];

    /// <summary>Ziehungsreihenfolge: jeder Eintrag wird nacheinander enthüllt.</summary>
    public IReadOnlyList<AuslosungEintrag> Reihenfolge { get; init; } = [];
}

/// <summary>
/// Singleton-Dienst, der den Anzeigestatus kontrolliert und Live-Updates
/// vom Bedienungs-Layer an das AnzeigeWindowViewModel weiterleitet.
/// </summary>
public class AnzeigeZustandService
{
    private AnzeigeScreen _aktuellerScreen = AnzeigeScreen.Startscreen;

    /// <summary>Der aktuell angezeigte Screen.</summary>
    public AnzeigeScreen AktuellerScreen => _aktuellerScreen;

    /// <summary>Wird ausgelöst, wenn der anzuzeigende Screen gewechselt werden soll.</summary>
    public event EventHandler<AnzeigeScreen>? ScreenGewechselt;

    /// <summary>Wird ausgelöst, wenn der Spielzustand aktualisiert wurde und die Anzeige neu laden soll.</summary>
    public event EventHandler<SpielZustandEventArgs>? SpielZustandAktualisiert;

    /// <summary>Wird ausgelöst, wenn die animierte Auslosung am Beamer starten soll.</summary>
    public event EventHandler<AuslosungDaten>? AuslosungGestartet;

    /// <summary>Wird ausgelöst, wenn die animierte Auslosung vollständig abgespielt wurde.</summary>
    public event EventHandler? AuslosungAbgeschlossen;

    /// <summary>Wechselt den aktuellen Screen und benachrichtigt das AnzeigeWindow.</summary>
    public void ZeigeScreen(AnzeigeScreen screen)
    {
        _aktuellerScreen = screen;
        ScreenGewechselt?.Invoke(this, screen);
    }

    /// <summary>Benachrichtigt das AnzeigeWindow, den Spielzustand neu zu laden (z.B. nach jedem Versuch).</summary>
    public void SpielZustandMelden(Spiel spiel, Turnier turnier)
    {
        SpielZustandAktualisiert?.Invoke(this, new SpielZustandEventArgs(spiel, turnier));
    }

    /// <summary>Startet die animierte Auslosung am Beamer und wechselt auf den Auslosungs-Screen.</summary>
    public void AuslosungAmBeamerStarten(AuslosungDaten daten)
    {
        AuslosungGestartet?.Invoke(this, daten);
        ZeigeScreen(AnzeigeScreen.Auslosung);
    }

    /// <summary>Meldet, dass die animierte Auslosung vollständig abgespielt wurde.</summary>
    public void AuslosungBeenden()
    {
        AuslosungAbgeschlossen?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>Enthält Spiel und Turnier für Live-Update-Events an die Anzeigeoberfläche.</summary>
public sealed class SpielZustandEventArgs(Spiel spiel, Turnier turnier) : EventArgs
{
    /// <summary>Das betroffene Spiel.</summary>
    public Spiel Spiel { get; } = spiel;

    /// <summary>Das übergeordnete Turnier (für Kontext wie Teamnamen).</summary>
    public Turnier Turnier { get; } = turnier;
}
