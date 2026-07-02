/*************************************************************
 * Datei:        TeamAnzeigeModel.cs
 * Zweck:        Anzeigemodelle für Teams und Spieler in der Turnierverwaltung
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Leichtgewichtiges Anzeigemodell für einen Team-Listeneintrag.
/// Enthält die Id für Kommando-Dispatching, den Anzeigenamen und optional den Logo-Pfad.
/// </summary>
/// <param name="Id">Eindeutiger Bezeichner des Teams.</param>
/// <param name="AnzeigeName">Anzeigename inklusive optionalem Kürzel.</param>
/// <param name="LogoPfad">Optionaler Pfad zum Team-Logo.</param>
public sealed record TeamAnzeigeModel(Guid Id, string AnzeigeName, string? LogoPfad = null);

/// <summary>
/// Editierbares Anzeigemodell für einen Spieler in der Spielerverwaltung.
/// Änderungen am Namen werden über PropertyChanged an das übergeordnete ViewModel gemeldet.
/// </summary>
public sealed partial class SpielerBearbeitungsModel : ObservableObject
{
    /// <summary>Eindeutiger Bezeichner des Spielers im Turnier-Datenmodell.</summary>
    public Guid Id { get; init; }

    /// <summary>Spielername – editierbar per TextBox-Binding.</summary>
    [ObservableProperty]
    private string _name = string.Empty;
}
