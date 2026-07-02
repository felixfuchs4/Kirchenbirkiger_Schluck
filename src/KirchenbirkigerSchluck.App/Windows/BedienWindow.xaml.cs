/*************************************************************
 * Datei:        BedienWindow.xaml.cs
 * Zweck:        Hauptfenster der Turnierleitung (Bedienoberfläche)
 * Bereich:      Präsentation – Bedienoberfläche
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

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
    }
}
