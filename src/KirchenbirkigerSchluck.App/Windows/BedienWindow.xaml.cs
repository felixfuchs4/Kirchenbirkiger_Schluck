/*************************************************************
 * Datei:        BedienWindow.xaml.cs
 * Zweck:        Hauptfenster der Turnierleitung (Bedienoberfläche)
 * Bereich:      Präsentation – Bedienoberfläche
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.ComponentModel;
using System.Linq;
using System.Windows;
using KirchenbirkigerSchluck.App.ViewModels;

namespace KirchenbirkigerSchluck.App.Windows;

/// <summary>
/// Hauptfenster für die Turnierleitung. Stellt alle Funktionen der
/// Bedienoberfläche bereit (Turnierverwaltung, Spielplan, Spielsteuerung, Korrekturen, Einstellungen).
/// Das DataContext wird per DI-Konstruktorinjektion gesetzt.
/// </summary>
public partial class BedienWindow : Window
{
    /// <summary>
    /// Initialisiert das Bedien-Fenster mit dem ViewModel aus dem DI-Container.
    /// </summary>
    /// <param name="viewModel">Das Shell-ViewModel der Bedienoberfläche.</param>
    public BedienWindow(BedienWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.BeendenAngefordert += (_, _) => Close();
        Closing += BedienWindow_Closing;
    }

    /// <summary>
    /// Fragt vor jedem Schließen dieses Fensters (Beenden-Button, Alt+F4, Titelleisten-„X")
    /// nach Bestätigung. Bei Zustimmung wird zusätzlich die Anzeigeoberfläche geschlossen,
    /// sodass das Programm vollständig beendet wird; bei Ablehnung bleibt alles geöffnet.
    /// </summary>
    private void BedienWindow_Closing(object? sender, CancelEventArgs e)
    {
        var bestaetigt = MessageBox.Show(
            "Turnierleitung und Anzeige wirklich beenden?",
            "Programm beenden", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        if (!bestaetigt)
        {
            e.Cancel = true;
            return;
        }

        // Übrige Fenster (Anzeigeoberfläche) mitschließen; dieses Fenster schließt danach regulär weiter.
        foreach (var fenster in Application.Current.Windows.OfType<Window>().Where(f => f != this))
            fenster.Close();
    }
}
