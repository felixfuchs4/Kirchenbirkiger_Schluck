/*************************************************************
 * Datei:        BedienWindowViewModel.cs
 * Zweck:        Shell-ViewModel für die Bedienoberfläche; steuert Tab-Navigation
 * Bereich:      Präsentation – ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    /// <summary>ViewModel für den Statistik-Tab.</summary>
    public StatistikViewModel StatistikVm { get; }

    /// <summary>ViewModel für den Spielsteuerungs-Tab.</summary>
    public SpielsteuerungViewModel SpielsteuerungVm { get; }

    /// <summary>ViewModel für den Korrektur-Tab.</summary>
    public KorrekturViewModel KorrekturVm { get; }

    /// <summary>ViewModel für den Siegerehrungs-Tab.</summary>
    public SiegerehrungViewModel SiegerehrungVm { get; }

    /// <summary>ViewModel für den Einstellungs-Tab.</summary>
    public EinstellungenViewModel EinstellungenVm { get; }

    // ──── Tab-Navigation ─────────────────────────────────────────────────────

    /// <summary>Index des aktuell aktiven Tabs (0 = Turnier, 1 = Teamverwaltung, 2 = Gruppen, 3 = Spielplan, 4 = Tabellen, 5 = Statistik, 6 = Spiel, 7 = Korrektur, 8 = Siegerehrung, 9 = Einstellungen).</summary>
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
        5 => StatistikVm,
        6 => SpielsteuerungVm,
        7 => KorrekturVm,
        8 => SiegerehrungVm,
        9 => EinstellungenVm,
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
        StatistikViewModel statistikVm,
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
        StatistikVm = statistikVm;
        SpielsteuerungVm = spielsteuerungVm;
        KorrekturVm = korrekturVm;
        SiegerehrungVm = siegerehrungVm;
        EinstellungenVm = einstellungenVm;

        // Nach Spielstart automatisch zum Spielsteuerungs-Tab navigieren (Index 6)
        spielplanVm.SpielGestartet += (_, _) => AusgewaehlterTabIndex = 6;

        // Aus der Turnierverwaltung zur Gruppenauslosung (Index 2) springen
        turnierVerwaltungVm.ZuGruppenauslosungGewuenscht += (_, _) => AusgewaehlterTabIndex = 2;
    }

    /// <summary>Navigiert zu einem bestimmten Tab per Index.</summary>
    public void NavigiereZuTab(int index) => AusgewaehlterTabIndex = index;

    // ──── Beenden ────────────────────────────────────────────────────────────

    /// <summary>
    /// Ausgelöst, wenn der Nutzer über den „Beenden"-Button das Programm schließen möchte.
    /// Die Bestätigungsabfrage und das tatsächliche Schließen beider Fenster übernimmt das
    /// Bedien-Fenster selbst (Closing-Handler), damit dieselbe Rückfrage auch beim Schließen
    /// über die Fenster-Titelleiste (Alt+F4, „X") greift.
    /// </summary>
    public event EventHandler? BeendenAngefordert;

    /// <summary>Fordert das Beenden des Programms an.</summary>
    [RelayCommand]
    private void Beenden() => BeendenAngefordert?.Invoke(this, EventArgs.Empty);
}
