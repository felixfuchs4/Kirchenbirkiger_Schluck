/*************************************************************
 * Datei:        AnzeigeWindow.xaml.cs
 * Zweck:        Vollbild-Anzeigeoberfläche für Beamer und Zuschauer
 * Bereich:      Präsentation – Anzeigeoberfläche
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows;
using KirchenbirkigerSchluck.App.ViewModels;

namespace KirchenbirkigerSchluck.App.Windows;

/// <summary>
/// Vollbild-Fenster für die Beamer-/Zuschaueransicht.
/// Das DataContext wird per DI-Konstruktorinjektion gesetzt.
/// </summary>
public partial class AnzeigeWindow : Window
{
    /// <summary>
    /// Initialisiert das Anzeige-Fenster mit dem ViewModel aus dem DI-Container.
    /// </summary>
    /// <param name="viewModel">Das Shell-ViewModel der Anzeigeoberfläche.</param>
    public AnzeigeWindow(AnzeigeWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
