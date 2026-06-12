/*************************************************************
 * Datei:        AnzeigeWindow.xaml.cs
 * Zweck:        Vollbild-Anzeigeoberfläche für Beamer und Zuschauer
 * Bereich:      Präsentation – Anzeigeoberfläche
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows;

namespace KirchenbirkigerSchluck.App.Windows;

/// <summary>
/// Vollbild-Fenster für die Beamer-/Zuschaueransicht.
/// Wird auf dem zweiten Bildschirm geöffnet und zeigt Spielstand,
/// Gruppenrangliste, Bracket und Siegeranzeige.
/// </summary>
public partial class AnzeigeWindow : Window
{
    /// <summary>
    /// Initialisiert das Anzeige-Fenster.
    /// </summary>
    public AnzeigeWindow()
    {
        InitializeComponent();
    }
}
