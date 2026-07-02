/*************************************************************
 * Datei:        AuslosungView.xaml.cs
 * Zweck:        Code-Behind mit der Animations-Choreografie der Auslosung
 * Bereich:      Präsentation – Anzeige-Views
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using KirchenbirkigerSchluck.App.ViewModels.Anzeige;

namespace KirchenbirkigerSchluck.App.Views.Anzeige;

/// <summary>
/// Auslosungs-UserControl. Die Datenhaltung liegt im ViewModel; die zeitliche
/// Choreografie (Kugel steigt auf, öffnet sich, Enthüllung, Einordnung) wird hier gesteuert.
/// </summary>
public partial class AuslosungView : UserControl
{
    private AuslosungAnzeigeViewModel? _vm;
    private bool _laeuft;

    /// <summary>Initialisiert die Komponente und registriert die Lade-Ereignisse.</summary>
    public AuslosungView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _vm = DataContext as AuslosungAnzeigeViewModel;
        if (_vm is null) return;

        _vm.AnimationStarten -= OnAnimationStarten;
        _vm.AnimationStarten += OnAnimationStarten;

        // Falls die Auslosung bereits angestoßen wurde, bevor die View geladen war
        if (_vm.LaeuftAuslosung && !_laeuft)
            _ = ChoreografieAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            _vm.AnimationStarten -= OnAnimationStarten;
    }

    private void OnAnimationStarten(object? sender, EventArgs e)
    {
        if (!_laeuft)
            _ = ChoreografieAsync();
    }

    /// <summary>Spielt die gesamte Auslosung Schritt für Schritt ab.</summary>
    private async Task ChoreografieAsync()
    {
        if (_vm is null || _laeuft) return;
        _laeuft = true;

        try
        {
            await Task.Delay(900); // kurze Anlaufspannung

            foreach (var eintrag in _vm.Reihenfolge.ToList())
            {
                KugelZuruecksetzen();
                _vm.KugelLaden(eintrag);

                // Kugel steigt aus der Trommel und wächst
                var sx = AnimiereAsync(BallS, ScaleTransform.ScaleXProperty, 1.0, 0.9,
                    new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 });
                var sy = AnimiereAsync(BallS, ScaleTransform.ScaleYProperty, 1.0, 0.9,
                    new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 });
                _ = AnimiereAsync(BallT, TranslateTransform.YProperty, 0.0, 0.9,
                    new CubicEase { EasingMode = EasingMode.EaseOut });
                await AnimiereAsync(BallContainer, UIElement.OpacityProperty, 1.0, 0.4);
                await Task.WhenAll(sx, sy);

                await Task.Delay(1000); // Spannungspause vor dem Öffnen

                // Kugel öffnet sich, Enthüllung erscheint
                _ = AnimiereAsync(ObenT, TranslateTransform.YProperty, -120.0, 0.6,
                    new CubicEase { EasingMode = EasingMode.EaseOut });
                _ = AnimiereAsync(UntenT, TranslateTransform.YProperty, 120.0, 0.6,
                    new CubicEase { EasingMode = EasingMode.EaseOut });
                _ = AnimiereAsync(ObereHaelfte, UIElement.OpacityProperty, 0.0, 0.5);
                _ = AnimiereAsync(UntereHaelfte, UIElement.OpacityProperty, 0.0, 0.5);
                _ = AnimiereAsync(EnthuellKarte, UIElement.OpacityProperty, 1.0, 0.4);
                _ = AnimiereAsync(KarteScale, ScaleTransform.ScaleXProperty, 1.0, 0.6,
                    new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 });
                await AnimiereAsync(KarteScale, ScaleTransform.ScaleYProperty, 1.0, 0.6,
                    new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 });

                await Task.Delay(2000); // Enthüllung halten

                _vm.TeamEinordnen(eintrag); // Team poppt in seine Gruppenspalte

                await Task.Delay(400);

                // Kugel/Enthüllung ausblenden
                _ = AnimiereAsync(BallS, ScaleTransform.ScaleXProperty, 0.7, 0.5);
                _ = AnimiereAsync(BallS, ScaleTransform.ScaleYProperty, 0.7, 0.5);
                await AnimiereAsync(BallContainer, UIElement.OpacityProperty, 0.0, 0.5);

                await Task.Delay(500);
            }

            await Task.Delay(600);
            _vm.Fertigstellen();
        }
        catch
        {
            // Animation abgebrochen (z. B. Screenwechsel) – unkritisch
        }
        finally
        {
            _laeuft = false;
        }
    }

    /// <summary>Setzt die Kugel in den geschlossenen Ausgangszustand zurück.</summary>
    private void KugelZuruecksetzen()
    {
        BallContainer.BeginAnimation(OpacityProperty, null);
        BallContainer.Opacity = 0;

        BallS.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        BallS.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        BallS.ScaleX = 0.5;
        BallS.ScaleY = 0.5;

        BallT.BeginAnimation(TranslateTransform.YProperty, null);
        BallT.Y = 140;

        ObenT.BeginAnimation(TranslateTransform.YProperty, null);
        ObenT.Y = 0;
        UntenT.BeginAnimation(TranslateTransform.YProperty, null);
        UntenT.Y = 0;

        ObereHaelfte.BeginAnimation(OpacityProperty, null);
        ObereHaelfte.Opacity = 1;
        UntereHaelfte.BeginAnimation(OpacityProperty, null);
        UntereHaelfte.Opacity = 1;

        EnthuellKarte.BeginAnimation(OpacityProperty, null);
        EnthuellKarte.Opacity = 0;
        KarteScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        KarteScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        KarteScale.ScaleX = 0;
        KarteScale.ScaleY = 0;
    }

    /// <summary>Animiert eine Double-Eigenschaft auf den Zielwert und liefert ein await-bares Task.</summary>
    private static Task AnimiereAsync(
        IAnimatable ziel, DependencyProperty eigenschaft, double bis, double sekunden,
        IEasingFunction? easing = null)
    {
        var tcs = new TaskCompletionSource<bool>();
        var animation = new DoubleAnimation(bis, new Duration(TimeSpan.FromSeconds(sekunden)))
        {
            EasingFunction = easing
        };
        animation.Completed += (_, _) => tcs.TrySetResult(true);
        ziel.BeginAnimation(eigenschaft, animation);
        return tcs.Task;
    }
}
