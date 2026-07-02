/*************************************************************
 * Datei:        App.xaml.cs
 * Zweck:        Anwendungseinstiegspunkt mit DI-Konfiguration
 * Bereich:      Präsentation – Anwendungsstart
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.IO;
using System.Windows;
using KirchenbirkigerSchluck.App.Services;
using KirchenbirkigerSchluck.App.ViewModels;
using KirchenbirkigerSchluck.App.ViewModels.Anzeige;
using KirchenbirkigerSchluck.App.ViewModels.Bedienung;
using KirchenbirkigerSchluck.App.Windows;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Services;
using KirchenbirkigerSchluck.Data.Backup;
using KirchenbirkigerSchluck.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KirchenbirkigerSchluck.App;

/// <summary>
/// Einstiegspunkt der WPF-Anwendung. Konfiguriert den DI-Container
/// und öffnet das Bedien-Hauptfenster sowie die Anzeigeoberfläche.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Wird beim Anwendungsstart ausgeführt; richtet DI-Container ein und öffnet beide Fenster.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Feste, beschreibbare Datenablage einrichten: %LOCALAPPDATA%\KirchenbirkigerSchluck.
        // Alle relativen Laufzeitpfade (turnier.json, backups/, logos/) lösen gegen das
        // Arbeitsverzeichnis auf; durch das Setzen hier landen sie unabhängig vom Startort
        // stets im selben Ordner. So bleibt die Ordnerstruktur bei Installer-Betrieb konstant.
        var datenVerzeichnis = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KirchenbirkigerSchluck");
        Directory.CreateDirectory(datenVerzeichnis);
        Environment.CurrentDirectory = datenVerzeichnis;

        var services = new ServiceCollection();
        DienstRegistrieren(services);
        _serviceProvider = services.BuildServiceProvider();

        // Auto-Load: letzten Spielstand wiederherstellen (stilles Scheitern bei Fehler)
        var repository = _serviceProvider.GetRequiredService<TurnierRepository>();
        if (repository.ExistiertDatei())
        {
            try
            {
                var turnier = _serviceProvider.GetRequiredService<ITurnierService>().TurnierLaden();
                _serviceProvider.GetRequiredService<TurnierZustandService>().TurnierSetzen(turnier);
            }
            catch { /* Turnier bleibt leer; Benutzer kann manuell laden */ }
        }

        // Anzeigeoberfläche auf zweitem Bildschirm öffnen (falls vorhanden)
        var anzeigeFenster = _serviceProvider.GetRequiredService<AnzeigeWindow>();
        AnzeigeAufBildschirmPositionieren(anzeigeFenster);
        anzeigeFenster.Show();

        // Bedienoberfläche als Hauptfenster öffnen
        var hauptFenster = _serviceProvider.GetRequiredService<BedienWindow>();
        hauptFenster.Show();
    }

    /// <summary>Registriert alle Dienste und Fenster im DI-Container.</summary>
    private static void DienstRegistrieren(IServiceCollection services)
    {
        const string turnierPfad = "turnier.json";
        const string backupPfad = "backups";

        // Datenschicht – als Konkretyp UND Interface registrieren
        var turnierRepo = new TurnierRepository(turnierPfad);
        services.AddSingleton(turnierRepo);
        services.AddSingleton<ITurnierRepository>(turnierRepo);
        services.AddSingleton(new BackupManager(backupPfad));
        services.AddSingleton(new LogoService("logos"));

        // Anwendungslogik (Core-Services)
        services.AddSingleton<ITurnierService, TurnierService>();
        services.AddSingleton<ISpielplanService, SpielplanService>();
        services.AddSingleton<ISpielsteuerungService, SpielsteuerungService>();
        services.AddSingleton<IWertungsService, WertungsService>();
        services.AddSingleton<StatistikService>();
        services.AddSingleton<IAenderungsprotokollService, AenderungsprotokollService>();

        // App-Dienste
        services.AddSingleton<TurnierZustandService>();
        services.AddSingleton<AnzeigeZustandService>();
        services.AddSingleton<InfoscreenEinstellungen>();

        // Anzeige-ViewModels (Singleton)
        services.AddSingleton<StartscreenViewModel>();
        services.AddSingleton<InfoscreenViewModel>();
        services.AddSingleton<MatchdayViewModel>();
        services.AddSingleton<GewinnerViewModel>();
        services.AddSingleton<AuslosungAnzeigeViewModel>();
        services.AddSingleton<AnzeigeWindowViewModel>();

        // Bedienungs-ViewModels (Singleton)
        services.AddSingleton<TurnierVerwaltungViewModel>();
        services.AddSingleton<TeamverwaltungViewModel>();
        services.AddSingleton<GruppenAuslosungViewModel>();
        services.AddSingleton<SpielplanViewModel>();
        services.AddSingleton<TabellenViewModel>();
        services.AddSingleton<SpielsteuerungViewModel>();
        services.AddSingleton<KorrekturViewModel>();
        services.AddSingleton<SiegerehrungViewModel>();
        services.AddSingleton<EinstellungenViewModel>();
        services.AddSingleton<BedienWindowViewModel>();

        // Fenster (Transient)
        services.AddTransient<BedienWindow>();
        services.AddTransient<AnzeigeWindow>();
    }

    /// <summary>
    /// Verschiebt das Anzeigefenster auf den zweiten Bildschirm, wenn verfügbar.
    /// Einfache Heuristik über SystemParameters.VirtualScreenWidth.
    /// </summary>
    private static void AnzeigeAufBildschirmPositionieren(AnzeigeWindow fenster)
    {
        var primBreite = SystemParameters.PrimaryScreenWidth;
        var gesamtBreite = SystemParameters.VirtualScreenWidth;

        if (gesamtBreite > primBreite)
        {
            // Zweiter Bildschirm rechts neben dem primären
            fenster.Left = primBreite;
            fenster.Top = 0;
        }
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
