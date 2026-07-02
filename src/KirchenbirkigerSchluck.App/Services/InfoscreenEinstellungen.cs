/*************************************************************
 * Datei:        InfoscreenEinstellungen.cs
 * Zweck:        Steuert, welche Folien auf dem Infoscreen angezeigt werden
 * Bereich:      Präsentation – Dienste
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;

namespace KirchenbirkigerSchluck.App.Services;

/// <summary>
/// Singleton-Einstellungen für den Infoscreen: welche Folien (Nächste Partie, Letzte Partie,
/// Tabellen, Spielplan, Finalrunden-Baum) in der Rotation erscheinen.
/// Wird in der Verwaltung über Schalter gesteuert.
/// </summary>
public partial class InfoscreenEinstellungen : ObservableObject
{
    /// <summary>Nächste Partie anzeigen.</summary>
    [ObservableProperty] private bool _zeigeNaechste = true;

    /// <summary>Letzte Partie anzeigen.</summary>
    [ObservableProperty] private bool _zeigeLetzte = true;

    /// <summary>Gruppentabellen anzeigen.</summary>
    [ObservableProperty] private bool _zeigeTabellen = true;

    /// <summary>Spielplan-Übersicht anzeigen.</summary>
    [ObservableProperty] private bool _zeigeSpielplan = true;

    /// <summary>Finalrunden-Baum anzeigen.</summary>
    [ObservableProperty] private bool _zeigeBracket = true;

    /// <summary>Wird ausgelöst, sobald sich eine Einstellung ändert.</summary>
    public event EventHandler? Geaendert;

    partial void OnZeigeNaechsteChanged(bool value) => Melden();
    partial void OnZeigeLetzteChanged(bool value) => Melden();
    partial void OnZeigeTabellenChanged(bool value) => Melden();
    partial void OnZeigeSpielplanChanged(bool value) => Melden();
    partial void OnZeigeBracketChanged(bool value) => Melden();

    private void Melden() => Geaendert?.Invoke(this, EventArgs.Empty);
}
