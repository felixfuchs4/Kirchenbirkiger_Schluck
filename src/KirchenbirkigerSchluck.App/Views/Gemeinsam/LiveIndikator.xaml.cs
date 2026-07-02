// ============================================================================
// Datei:   LiveIndikator.xaml.cs
// Zweck:   Code-Behind des LIVE-Indikators (nur Initialisierung)
// Bereich: Präsentation – Gemeinsame Controls
// ============================================================================

using System.Windows.Controls;

namespace KirchenbirkigerSchluck.App.Views.Gemeinsam;

/// <summary>
/// Pulsierender LIVE-Indikator (roter Punkt mit Schriftzug) für
/// laufende Spiele auf der Anzeigeoberfläche.
/// </summary>
public partial class LiveIndikator : UserControl
{
    /// <summary>Initialisiert das Control.</summary>
    public LiveIndikator()
    {
        InitializeComponent();
    }
}
