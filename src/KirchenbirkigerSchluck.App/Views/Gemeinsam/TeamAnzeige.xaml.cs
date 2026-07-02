// ============================================================================
// Datei:   TeamAnzeige.xaml.cs
// Zweck:   Code-Behind des wiederverwendbaren Logo-über-Name-Controls
//          mit konfigurierbaren Größen für verschiedene Anzeige-Kontexte
// Bereich: Präsentation – Gemeinsame Controls
// ============================================================================

using System.Windows;
using System.Windows.Controls;

namespace KirchenbirkigerSchluck.App.Views.Gemeinsam;

/// <summary>
/// Wiederverwendbare Einheit aus Team-Logo (oben) und Teamnamen (unten)
/// für die Anzeige-Views. Logo-Pfad, Name und Größen werden über
/// Dependency-Properties gesetzt, sodass das Control ohne eigenen
/// DataContext in beliebige Bindings eingebettet werden kann.
/// </summary>
public partial class TeamAnzeige : UserControl
{
    /// <summary>Dateipfad des Team-Logos (null/leer blendet das Logo aus).</summary>
    public static readonly DependencyProperty LogoPfadProperty =
        DependencyProperty.Register(nameof(LogoPfad), typeof(string), typeof(TeamAnzeige),
            new PropertyMetadata(null));

    /// <summary>Anzuzeigender Teamname.</summary>
    public static readonly DependencyProperty TeamNameProperty =
        DependencyProperty.Register(nameof(TeamName), typeof(string), typeof(TeamAnzeige),
            new PropertyMetadata(string.Empty));

    /// <summary>Höhe des Logos in Pixeln (Standard: 96).</summary>
    public static readonly DependencyProperty LogoGroesseProperty =
        DependencyProperty.Register(nameof(LogoGroesse), typeof(double), typeof(TeamAnzeige),
            new PropertyMetadata(96.0));

    /// <summary>Schriftgröße des Teamnamens (Standard: 44).</summary>
    public static readonly DependencyProperty NameGroesseProperty =
        DependencyProperty.Register(nameof(NameGroesse), typeof(double), typeof(TeamAnzeige),
            new PropertyMetadata(44.0));

    /// <summary>Initialisiert das Control.</summary>
    public TeamAnzeige()
    {
        InitializeComponent();
    }

    /// <summary>Dateipfad des Team-Logos.</summary>
    public string? LogoPfad
    {
        get => (string?)GetValue(LogoPfadProperty);
        set => SetValue(LogoPfadProperty, value);
    }

    /// <summary>Anzuzeigender Teamname.</summary>
    public string TeamName
    {
        get => (string)GetValue(TeamNameProperty);
        set => SetValue(TeamNameProperty, value);
    }

    /// <summary>Höhe des Logos in Pixeln.</summary>
    public double LogoGroesse
    {
        get => (double)GetValue(LogoGroesseProperty);
        set => SetValue(LogoGroesseProperty, value);
    }

    /// <summary>Schriftgröße des Teamnamens.</summary>
    public double NameGroesse
    {
        get => (double)GetValue(NameGroesseProperty);
        set => SetValue(NameGroesseProperty, value);
    }
}
