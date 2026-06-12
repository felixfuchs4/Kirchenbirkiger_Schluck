/*************************************************************
 * Datei:        App.xaml.cs
 * Zweck:        Anwendungseinstiegspunkt mit DI-Konfiguration
 * Bereich:      Präsentation – Anwendungsstart
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows;
using KirchenbirkigerSchluck.App.Windows;
using KirchenbirkigerSchluck.Core.Interfaces;
using KirchenbirkigerSchluck.Core.Services;
using KirchenbirkigerSchluck.Data.Backup;
using KirchenbirkigerSchluck.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KirchenbirkigerSchluck.App;

/// <summary>
/// Einstiegspunkt der WPF-Anwendung. Konfiguriert den DI-Container
/// und öffnet das Bedien-Hauptfenster.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Wird beim Anwendungsstart ausgeführt; richtet DI-Container ein und öffnet das Hauptfenster.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        DienstRegistrieren(services);
        _serviceProvider = services.BuildServiceProvider();

        var hauptFenster = _serviceProvider.GetRequiredService<BedienWindow>();
        hauptFenster.Show();
    }

    /// <summary>Registriert alle Dienste und Fenster im DI-Container.</summary>
    private static void DienstRegistrieren(IServiceCollection services)
    {
        // Datenpfade (später konfigurierbar)
        const string turnierPfad = "turnier.json";
        const string backupPfad = "backups";

        // Datenschicht
        services.AddSingleton(new TurnierRepository(turnierPfad));
        services.AddSingleton(new BackupManager(backupPfad));

        // Anwendungslogik
        services.AddSingleton<ITurnierService, TurnierService>();
        services.AddSingleton<ISpielplanService, SpielplanService>();
        services.AddSingleton<ISpielsteuerungService, SpielsteuerungService>();
        services.AddSingleton<IWertungsService, WertungsService>();
        services.AddSingleton<IAenderungsprotokollService, AenderungsprotokollService>();

        // Fenster
        services.AddTransient<BedienWindow>();
        services.AddTransient<AnzeigeWindow>();
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
