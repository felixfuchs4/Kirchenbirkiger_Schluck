/*************************************************************
 * Datei:        AutoScrollVerhalten.cs
 * Zweck:        Angehängtes Verhalten für langsames automatisches Scrollen langer Listen
 * Bereich:      Präsentation – Verhalten (Behaviors)
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace KirchenbirkigerSchluck.App.Behaviors;

/// <summary>
/// Angehängtes Verhalten, das einen <see cref="ScrollViewer"/> langsam und endlos
/// auf und ab scrollt – nur wenn der Inhalt höher ist als der sichtbare Bereich.
/// Einsatz auf der Anzeigeoberfläche, damit auch lange Tabellen vollständig sichtbar werden.
/// </summary>
public static class AutoScrollVerhalten
{
    /// <summary>
    /// Angehängte Eigenschaft: aktiviert das automatische Scrollen auf einem <see cref="ScrollViewer"/>.
    /// </summary>
    public static readonly DependencyProperty IstAktivProperty =
        DependencyProperty.RegisterAttached(
            "IstAktiv",
            typeof(bool),
            typeof(AutoScrollVerhalten),
            new PropertyMetadata(false, OnIstAktivGeaendert));

    /// <summary>Liefert den Wert der angehängten Eigenschaft <c>IstAktiv</c>.</summary>
    public static bool GetIstAktiv(DependencyObject element) =>
        (bool)element.GetValue(IstAktivProperty);

    /// <summary>Setzt den Wert der angehängten Eigenschaft <c>IstAktiv</c>.</summary>
    public static void SetIstAktiv(DependencyObject element, bool wert) =>
        element.SetValue(IstAktivProperty, wert);

    // Interner Verweis auf den laufenden Steuerungsobjekt-Instanz je ScrollViewer.
    private static readonly DependencyProperty SteuerungProperty =
        DependencyProperty.RegisterAttached(
            "Steuerung",
            typeof(ScrollSteuerung),
            typeof(AutoScrollVerhalten),
            new PropertyMetadata(null));

    /// <summary>Reagiert auf das Ein- und Ausschalten des Verhaltens.</summary>
    private static void OnIstAktivGeaendert(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer) return;

        if ((bool)e.NewValue)
        {
            var steuerung = new ScrollSteuerung(scrollViewer);
            scrollViewer.SetValue(SteuerungProperty, steuerung);
        }
        else if (scrollViewer.GetValue(SteuerungProperty) is ScrollSteuerung alte)
        {
            alte.Stoppen();
            scrollViewer.ClearValue(SteuerungProperty);
        }
    }

    /// <summary>
    /// Steuert den eigentlichen Scrollvorgang per <see cref="DispatcherTimer"/> in vier Phasen:
    /// Pause oben → abwärts → Pause unten → aufwärts.
    /// </summary>
    private sealed class ScrollSteuerung
    {
        // Phasen des Scroll-Zyklus.
        private enum Phase { PauseOben, Abwaerts, PauseUnten, Aufwaerts }

        // Pixel pro Tick – bewusst klein für eine ruhige, gut lesbare Bewegung.
        private const double GeschwindigkeitProTick = 0.5;

        // Anzahl Ticks für die Pausen an den Endpunkten (ca. 3 Sekunden bei 30 ms Takt).
        private const int PauseTicks = 100;

        private readonly ScrollViewer _scrollViewer;
        private readonly DispatcherTimer _timer;
        private Phase _phase = Phase.PauseOben;
        private double _offset;
        private int _pauseRest = PauseTicks;

        /// <summary>Initialisiert die Steuerung und startet den Timer.</summary>
        public ScrollSteuerung(ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        /// <summary>Stoppt den Timer und setzt die Position zurück.</summary>
        public void Stoppen()
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
        }

        /// <summary>Bewegt den ScrollViewer pro Takt gemäß der aktuellen Phase.</summary>
        private void OnTick(object? sender, EventArgs e)
        {
            var maximum = _scrollViewer.ScrollableHeight;

            // Kein Überlauf → an den Anfang zurück und nichts tun.
            if (maximum <= 0)
            {
                if (_offset != 0)
                {
                    _offset = 0;
                    _scrollViewer.ScrollToVerticalOffset(0);
                }
                _phase = Phase.PauseOben;
                _pauseRest = PauseTicks;
                return;
            }

            switch (_phase)
            {
                case Phase.PauseOben:
                    if (--_pauseRest <= 0)
                        _phase = Phase.Abwaerts;
                    break;

                case Phase.Abwaerts:
                    _offset += GeschwindigkeitProTick;
                    if (_offset >= maximum)
                    {
                        _offset = maximum;
                        _phase = Phase.PauseUnten;
                        _pauseRest = PauseTicks;
                    }
                    _scrollViewer.ScrollToVerticalOffset(_offset);
                    break;

                case Phase.PauseUnten:
                    if (--_pauseRest <= 0)
                        _phase = Phase.Aufwaerts;
                    break;

                case Phase.Aufwaerts:
                    _offset -= GeschwindigkeitProTick;
                    if (_offset <= 0)
                    {
                        _offset = 0;
                        _phase = Phase.PauseOben;
                        _pauseRest = PauseTicks;
                    }
                    _scrollViewer.ScrollToVerticalOffset(_offset);
                    break;
            }
        }
    }
}
