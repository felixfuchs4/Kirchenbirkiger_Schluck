// ============================================================================
// Datei:   ScreenUebergangVerhalten.cs
// Zweck:   Attached Property, das bei jedem Wechsel des Auslöser-Werts
//          einen weichen Einblend-Übergang (Crossfade + Aufwärts-Slide)
//          auf dem Zielelement abspielt
// Bereich: Präsentation – Behaviors
// ============================================================================

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KirchenbirkigerSchluck.App.Behaviors;

/// <summary>
/// Blendet ein Element bei jedem Wechsel des gebundenen
/// <see cref="AusloeserProperty"/>-Werts weich ein (Opacity 0 → 1 plus
/// 16 px Aufwärts-Slide, 400 ms, QuarticOut). Gedacht für die
/// Screen-Umschaltung der Anzeige und die Folienrotation des Infoscreens:
/// <c>verhalten:ScreenUebergangVerhalten.Ausloeser="{Binding AktuellerScreenViewModel}"</c>.
/// Nur Opacity/Transform werden animiert – kein Layout-Ruckeln.
/// </summary>
public static class ScreenUebergangVerhalten
{
    /// <summary>Standard-Dauer des Einblend-Übergangs in Millisekunden.</summary>
    private const double StandardDauerMs = 400.0;

    /// <summary>Vertikaler Startversatz des Slide-Anteils in Pixeln.</summary>
    private const double StartVersatzY = 16.0;

    /// <summary>
    /// Optionale Übergangsdauer in Millisekunden (Standard: 400).
    /// Für Bühnen-Momente wie die Siegerehrung auf 700 erhöhen.
    /// </summary>
    public static readonly DependencyProperty DauerMillisekundenProperty =
        DependencyProperty.RegisterAttached(
            "DauerMillisekunden",
            typeof(double),
            typeof(ScreenUebergangVerhalten),
            new PropertyMetadata(StandardDauerMs));

    /// <summary>Liest die Übergangsdauer in Millisekunden.</summary>
    public static double GetDauerMillisekunden(DependencyObject element)
        => (double)element.GetValue(DauerMillisekundenProperty);

    /// <summary>Setzt die Übergangsdauer in Millisekunden.</summary>
    public static void SetDauerMillisekunden(DependencyObject element, double wert)
        => element.SetValue(DauerMillisekundenProperty, wert);

    /// <summary>
    /// Auslöser-Wert: Bei jeder Änderung (außer der Erstzuweisung)
    /// wird der Übergang auf dem Element abgespielt.
    /// </summary>
    public static readonly DependencyProperty AusloeserProperty =
        DependencyProperty.RegisterAttached(
            "Ausloeser",
            typeof(object),
            typeof(ScreenUebergangVerhalten),
            new PropertyMetadata(null, BeiAusloeserGeaendert));

    /// <summary>Liest den Auslöser-Wert.</summary>
    public static object? GetAusloeser(DependencyObject element)
        => element.GetValue(AusloeserProperty);

    /// <summary>Setzt den Auslöser-Wert.</summary>
    public static void SetAusloeser(DependencyObject element, object? wert)
        => element.SetValue(AusloeserProperty, wert);

    /// <summary>
    /// Spielt den Einblend-Übergang ab, sobald sich der Auslöser ändert.
    /// Die Erstzuweisung (alter Wert null) wird übersprungen, damit der
    /// App-Start nicht animiert wirkt.
    /// </summary>
    private static void BeiAusloeserGeaendert(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element || e.OldValue is null)
        {
            return;
        }

        // Transform sicherstellen, ohne fremde RenderTransforms zu zerstören
        if (element.RenderTransform is not TranslateTransform verschiebung)
        {
            verschiebung = new TranslateTransform();
            element.RenderTransform = verschiebung;
        }

        var dauer = new Duration(TimeSpan.FromMilliseconds(GetDauerMillisekunden(d)));
        var easing = new QuarticEase { EasingMode = EasingMode.EaseOut };

        var einblenden = new DoubleAnimation(0.0, 1.0, dauer)
        {
            EasingFunction = easing
        };
        var hochschieben = new DoubleAnimation(StartVersatzY, 0.0, dauer)
        {
            EasingFunction = easing
        };

        element.BeginAnimation(UIElement.OpacityProperty, einblenden);
        verschiebung.BeginAnimation(TranslateTransform.YProperty, hochschieben);
    }
}
