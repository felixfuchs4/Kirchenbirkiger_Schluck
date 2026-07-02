// ============================================================================
// Datei:   LeererZustand.xaml.cs
// Zweck:   Code-Behind des wiederverwendbaren Leer-Zustands
//          (Symbol, Titel, Hinweis auf den nächsten Schritt)
// Bereich: Präsentation – Gemeinsame Controls
// ============================================================================

using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace KirchenbirkigerSchluck.App.Views.Gemeinsam;

/// <summary>
/// Leer-Zustand für Bedien-Views: erklärt statt einer leeren Fläche,
/// was fehlt und was der nächste Schritt ist.
/// </summary>
public partial class LeererZustand : UserControl
{
    /// <summary>Symbol des Leer-Zustands (MaterialDesign-Icon).</summary>
    public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register(nameof(Symbol), typeof(PackIconKind), typeof(LeererZustand),
            new PropertyMetadata(PackIconKind.InformationOutline));

    /// <summary>Kurzer Titel (was fehlt).</summary>
    public static readonly DependencyProperty TitelProperty =
        DependencyProperty.Register(nameof(Titel), typeof(string), typeof(LeererZustand),
            new PropertyMetadata(string.Empty));

    /// <summary>Hinweistext (was als Nächstes zu tun ist).</summary>
    public static readonly DependencyProperty HinweisProperty =
        DependencyProperty.Register(nameof(Hinweis), typeof(string), typeof(LeererZustand),
            new PropertyMetadata(string.Empty));

    /// <summary>Initialisiert das Control.</summary>
    public LeererZustand()
    {
        InitializeComponent();
    }

    /// <summary>Symbol des Leer-Zustands.</summary>
    public PackIconKind Symbol
    {
        get => (PackIconKind)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>Kurzer Titel.</summary>
    public string Titel
    {
        get => (string)GetValue(TitelProperty);
        set => SetValue(TitelProperty, value);
    }

    /// <summary>Hinweistext.</summary>
    public string Hinweis
    {
        get => (string)GetValue(HinweisProperty);
        set => SetValue(HinweisProperty, value);
    }
}
