/*************************************************************
 * Datei:        AuslosungAnzeigeViewModel.cs
 * Zweck:        ViewModel für die animierte Gruppenauslosung auf der Anzeigeoberfläche
 * Bereich:      Präsentation – Anzeige-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Anzeige;

/// <summary>Ein Team in einer Gruppenspalte der Auslosungs-Anzeige.</summary>
public sealed class AuslosungTeamAnzeigeModel
{
    /// <summary>Teamname.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optionaler Logo-Pfad.</summary>
    public string? LogoPfad { get; init; }
}

/// <summary>Eine Gruppenspalte der Auslosungs-Anzeige mit ihren bereits gezogenen Teams.</summary>
public sealed class AuslosungGruppeAnzeigeModel
{
    /// <summary>Anzeigename der Gruppe.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Bereits in diese Gruppe gezogene Teams.</summary>
    public ObservableCollection<AuslosungTeamAnzeigeModel> Teams { get; } = [];
}

/// <summary>
/// Hält die Daten und den Live-Zustand der animierten Auslosung für das Publikum.
/// Die eigentliche Choreografie (Lostrommel, Kugel öffnen, Enthüllung) liegt im Code-Behind
/// der View; dieses ViewModel liefert die Daten und nimmt die Einordnung der Teams vor.
/// </summary>
public partial class AuslosungAnzeigeViewModel : ObservableObject
{
    private readonly AnzeigeZustandService _anzeigeZustand;

    /// <summary>Initialisiert das ViewModel mit dem Anzeige-Vermittlerdienst.</summary>
    public AuslosungAnzeigeViewModel(AnzeigeZustandService anzeigeZustandService)
    {
        _anzeigeZustand = anzeigeZustandService;
    }

    /// <summary>Gruppenspalten, die sich während der Auslosung füllen.</summary>
    public ObservableCollection<AuslosungGruppeAnzeigeModel> Gruppen { get; } = [];

    /// <summary>Die festgelegte Ziehungsreihenfolge.</summary>
    public IReadOnlyList<AuslosungEintrag> Reihenfolge { get; private set; } = [];

    /// <summary>Name des gerade gezogenen Teams (in der Kugel).</summary>
    [ObservableProperty]
    private string _aktuellerTeamName = string.Empty;

    /// <summary>Logo-Pfad des gerade gezogenen Teams.</summary>
    [ObservableProperty]
    private string? _aktuellesLogoPfad;

    /// <summary>Gibt an, ob das gerade gezogene Team ein Logo hat.</summary>
    [ObservableProperty]
    private bool _hatAktuellesLogo;

    /// <summary>Zielgruppe des gerade gezogenen Teams.</summary>
    [ObservableProperty]
    private string _aktuelleGruppe = string.Empty;

    /// <summary>Gibt an, ob die Auslosung gerade läuft.</summary>
    [ObservableProperty]
    private bool _laeuftAuslosung;

    /// <summary>Statuszeile unter dem Titel.</summary>
    [ObservableProperty]
    private string _statusText = string.Empty;

    /// <summary>Wird ausgelöst, sobald die Choreografie im Code-Behind starten soll.</summary>
    public event EventHandler? AnimationStarten;

    /// <summary>Bereitet die Anzeige für eine neue Auslosung vor und stößt die Animation an.</summary>
    public void Vorbereiten(AuslosungDaten daten)
    {
        Reihenfolge = daten.Reihenfolge;

        Gruppen.Clear();
        foreach (var name in daten.Gruppennamen)
            Gruppen.Add(new AuslosungGruppeAnzeigeModel { Name = name });

        AktuellerTeamName = string.Empty;
        AktuellesLogoPfad = null;
        HatAktuellesLogo = false;
        AktuelleGruppe = string.Empty;
        LaeuftAuslosung = true;
        StatusText = "Die Auslosung beginnt …";

        AnimationStarten?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Lädt den aktuellen Eintrag in die Kugel (vor dem Öffnen).</summary>
    public void KugelLaden(AuslosungEintrag eintrag)
    {
        AktuellerTeamName = eintrag.TeamName;
        AktuellesLogoPfad = eintrag.LogoPfad;
        HatAktuellesLogo = !string.IsNullOrWhiteSpace(eintrag.LogoPfad);
        AktuelleGruppe = eintrag.GruppenName;
        StatusText = "… und gezogen ist:";
    }

    /// <summary>Ordnet das gezogene Team in seine Gruppenspalte ein.</summary>
    public void TeamEinordnen(AuslosungEintrag eintrag)
    {
        if (eintrag.GruppenIndex >= 0 && eintrag.GruppenIndex < Gruppen.Count)
        {
            Gruppen[eintrag.GruppenIndex].Teams.Add(new AuslosungTeamAnzeigeModel
            {
                Name = eintrag.TeamName,
                LogoPfad = eintrag.LogoPfad
            });
        }
    }

    /// <summary>Schließt die Auslosung ab und meldet dies an den Vermittlerdienst.</summary>
    public void Fertigstellen()
    {
        LaeuftAuslosung = false;
        AktuellerTeamName = string.Empty;
        AktuellesLogoPfad = null;
        HatAktuellesLogo = false;
        StatusText = "Die Gruppen stehen fest!";
        _anzeigeZustand.AuslosungBeenden();
    }
}
