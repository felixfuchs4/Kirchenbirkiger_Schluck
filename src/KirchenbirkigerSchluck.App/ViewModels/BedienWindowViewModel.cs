/*************************************************************
 * Datei:        BedienWindowViewModel.cs
 * Zweck:        Shell-ViewModel für die Bedienoberfläche; steuert Tab-Navigation
 * Bereich:      Präsentation – ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.ViewModels.Bedienung;

namespace KirchenbirkigerSchluck.App.ViewModels;

/// <summary>
/// Koordiniert die fünf Tabs der Bedienoberfläche und leitet Navigationsanfragen weiter.
/// </summary>
public partial class BedienWindowViewModel : ObservableObject
{
    // ──── Tab-ViewModels ─────────────────────────────────────────────────────

    /// <summary>ViewModel für den Turnierverwaltungs-Tab.</summary>
    public TurnierVerwaltungViewModel TurnierVerwaltungVm { get; }

    /// <summary>ViewModel für den Teamverwaltungs-Tab.</summary>
    public TeamverwaltungViewModel TeamverwaltungVm { get; }

    /// <summary>ViewModel für den Gruppen-/Auslosungs-Tab.</summary>
    public GruppenAuslosungViewModel GruppenAuslosungVm { get; }

    /// <summary>ViewModel für den Spielplan-Tab.</summary>
    public SpielplanViewModel SpielplanVm { get; }

    /// <summary>ViewModel für den Tabellen-Tab.</summary>
    public TabellenViewModel TabellenVm { get; }

    /// <summary>ViewModel für den Spielsteuerungs-Tab.</summary>
    public SpielsteuerungViewModel SpielsteuerungVm { get; }

    /// <summary>ViewModel für den Korrektur-Tab.</summary>
    public KorrekturViewModel KorrekturVm { get; }

    /// <summary>ViewModel für den Siegerehrungs-Tab.</summary>
    public SiegerehrungViewModel SiegerehrungVm { get; }

    /// <summary>ViewModel für den Einstellungs-Tab.</summary>
    public EinstellungenViewModel EinstellungenVm { get; }

    // ──── Tab-Navigation ─────────────────────────────────────────────────────

    /// <summary>Index des aktuell aktiven Tabs (0 = Turnier, 1 = Teamverwaltung, 2 = Gruppen, 3 = Spielplan, 4 = Tabellen, 5 = Spiel, 6 = Korrektur, 7 = Siegerehrung, 8 = Einstellungen).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AktivesTabViewModel))]
    private int _ausgewaehlterTabIndex;

    /// <summary>Das aktuell im Inhaltsbereich angezeigte Tab-ViewModel (für DataTemplate-Dispatch).</summary>
    public ObservableObject AktivesTabViewModel => AusgewaehlterTabIndex switch
    {
        0 => TurnierVerwaltungVm,
        1 => TeamverwaltungVm,
        2 => GruppenAuslosungVm,
        3 => SpielplanVm,
        4 => TabellenVm,
        5 => SpielsteuerungVm,
        6 => KorrekturVm,
        7 => SiegerehrungVm,
        8 => EinstellungenVm,
        _ => TurnierVerwaltungVm
    };

    /// <summary>
    /// Initialisiert das Shell-ViewModel mit allen Tab-ViewModels und verbindet Navigations-Events.
    /// </summary>
    public BedienWindowViewModel(
        TurnierVerwaltungViewModel turnierVerwaltungVm,
        TeamverwaltungViewModel teamverwaltungVm,
        GruppenAuslosungViewModel gruppenAuslosungVm,
        SpielplanViewModel spielplanVm,
        TabellenViewModel tabellenVm,
        SpielsteuerungViewModel spielsteuerungVm,
        KorrekturViewModel korrekturVm,
        SiegerehrungViewModel siegerehrungVm,
        EinstellungenViewModel einstellungenVm)
    {
        TurnierVerwaltungVm = turnierVerwaltungVm;
        TeamverwaltungVm = teamverwaltungVm;
        GruppenAuslosungVm = gruppenAuslosungVm;
        SpielplanVm = spielplanVm;
        TabellenVm = tabellenVm;
        SpielsteuerungVm = spielsteuerungVm;
        KorrekturVm = korrekturVm;
        SiegerehrungVm = siegerehrungVm;
        EinstellungenVm = einstellungenVm;

        // Nach Spielstart automatisch zum Spielsteuerungs-Tab navigieren (Index 5)
        spielplanVm.SpielGestartet += (_, _) => AusgewaehlterTabIndex = 5;

        // Aus der Turnierverwaltung zur Gruppenauslosung (Index 2) springen
        turnierVerwaltungVm.ZuGruppenauslosungGewuenscht += (_, _) => AusgewaehlterTabIndex = 2;
    }

    /// <summary>Navigiert zu einem bestimmten Tab per Index.</summary>
    public void NavigiereZuTab(int index) => AusgewaehlterTabIndex = index;
}
