// ============================================================================
// Datei:   WertPulsVerhalten.cs
// Zweck:   Attached Property, das bei jeder Wertänderung einen kurzen
//          Skalierungs-Puls (Pop) auf dem Zielelement abspielt –
//          z. B. für Score-Änderungen auf dem Matchday-Screen
// Bereich: Präsentation – Behaviors
// ============================================================================

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KirchenbirkigerSchluck.App.Behaviors;

/// <summary>
/// Lässt ein Element bei jeder Änderung des gebundenen
/// <see cref="WertProperty"/> kurz aufpulsen (Scale 1 → 1,12 → 1, 250 ms).
/// Gedacht für Zuschauer-Feedback bei Score-Änderungen:
/// <c>verhalten:WertPulsVerhalten.Wert="{Binding DuellsiegeTeam1}"</c>.
/// </summary>
public static class WertPulsVerhalten
{
    /// <summary>Maximale Skalierung des Pulses.</summary>
    private const double PulsSkalierung = 1.12;

    /// <summary>Gesamtdauer des Pulses.</summary>
    private static readonly Duration PulsDauer = new(TimeSpan.FromMilliseconds(250));

    /// <summary>
    /// Beobachteter Wert: Bei jeder Änderung (außer der Erstzuweisung)
    /// wird der Puls auf dem Element abgespielt.
    /// </summary>
    public static readonly DependencyProperty WertProperty =
        DependencyProperty.RegisterAttached(
            "Wert",
            typeof(object),
            typeof(WertPulsVerhalten),
            new PropertyMetadata(null, BeiWertGeaendert));

    /// <summary>Liest den beobachteten Wert.</summary>
    public static object? GetWert(DependencyObject element)
        => element.GetValue(WertProperty);

    /// <summary>Setzt den beobachteten Wert.</summary>
    public static void SetWert(DependencyObject element, object? wert)
        => element.SetValue(WertProperty, wert);

    /// <summary>
    /// Spielt den Skalierungs-Puls ab, sobald sich der Wert ändert.
    /// Die Erstzuweisung wird übersprungen, damit das Laden des Screens
    /// keinen Puls auslöst.
    /// </summary>
    private static void BeiWertGeaendert(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || e.OldValue is null || Equals(e.OldValue, e.NewValue))
        {
            return;
        }

        // Zentrierte Skalierung sicherstellen
        if (element.RenderTransform is not ScaleTransform skalierung)
        {
            skalierung = new ScaleTransform(1.0, 1.0);
            element.RenderTransform = skalierung;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        // Hin- und Rückweg in einer Animation (AutoReverse bei halber Dauer)
        var puls = new DoubleAnimation(1.0, PulsSkalierung, new Duration(PulsDauer.TimeSpan / 2))
        {
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        skalierung.BeginAnimation(ScaleTransform.ScaleXProperty, puls);
        skalierung.BeginAnimation(ScaleTransform.ScaleYProperty, puls);
    }
}
