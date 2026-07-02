/*************************************************************
 * Datei:        PfadZuBitmapConverter.cs
 * Zweck:        Lädt eine Bilddatei anhand ihres Pfads ohne dauerhafte Dateisperre
 * Bereich:      Präsentation – Konverter
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace KirchenbirkigerSchluck.App.Converters;

/// <summary>
/// Wandelt einen (relativen oder absoluten) Dateipfad in ein <see cref="BitmapImage"/> um.
/// Das Bild wird vollständig in den Speicher geladen (<c>OnLoad</c>), damit die Datei
/// nicht gesperrt bleibt und z. B. überschrieben werden kann.
/// Liefert <c>null</c>, wenn kein Pfad gesetzt ist oder die Datei nicht existiert.
/// </summary>
public sealed class PfadZuBitmapConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string pfad || string.IsNullOrWhiteSpace(pfad))
            return null;

        var vollerPfad = Path.IsPathRooted(pfad) ? pfad : Path.GetFullPath(pfad);
        if (!File.Exists(vollerPfad))
            return null;

        try
        {
            var bild = new BitmapImage();
            bild.BeginInit();
            bild.CacheOption = BitmapCacheOption.OnLoad;
            bild.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bild.UriSource = new Uri(vollerPfad, UriKind.Absolute);
            bild.EndInit();
            bild.Freeze();
            return bild;
        }
        catch
        {
            // Beschädigte/ungültige Bilddatei – kein Logo anzeigen
            return null;
        }
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Wandelt einen Bool in Sichtbarkeit um – invertiert: <c>true</c> → Collapsed, <c>false</c> → Visible.
/// </summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Liefert <see cref="Visibility.Visible"/>, wenn der übergebene Pfad auf eine vorhandene
/// Datei zeigt, sonst <see cref="Visibility.Collapsed"/>. Für das Ein-/Ausblenden von Logos.
/// </summary>
public sealed class PfadZuVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string pfad || string.IsNullOrWhiteSpace(pfad))
            return Visibility.Collapsed;

        var vollerPfad = Path.IsPathRooted(pfad) ? pfad : Path.GetFullPath(pfad);
        return File.Exists(vollerPfad) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
