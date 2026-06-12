/*************************************************************
 * Datei:        BedienWindow.xaml.cs
 * Zweck:        Hauptfenster der Turnierleitung (Bedienoberfläche)
 * Bereich:      Präsentation – Bedienoberfläche
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows;

namespace KirchenbirkigerSchluck.App.Windows;

/// <summary>
/// Hauptfenster für die Turnierleitung. Stellt alle Funktionen der
/// Bedienoberfläche bereit (Spielplan, Ergebniseingabe, Verwaltung).
/// </summary>
public partial class BedienWindow : Window
{
    /// <summary>
    /// Initialisiert das Bedien-Fenster.
    /// </summary>
    public BedienWindow()
    {
        InitializeComponent();
    }
}
