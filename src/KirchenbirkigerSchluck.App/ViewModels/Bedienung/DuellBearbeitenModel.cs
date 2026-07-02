/*************************************************************
 * Datei:        DuellBearbeitenModel.cs
 * Zweck:        Editierbares Anzeigemodell für ein Einzelduell in der Spielsteuerung
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>
/// Editierbares Modell eines Einzelduells: erlaubt das nachträgliche Ändern der
/// Spieler und der Trefferzahlen direkt in der Spielsteuerung (Korrektur per Tastatur).
/// Geplante (noch nicht gespielte) Slots werden nur als Vorschau angezeigt.
/// </summary>
public sealed partial class DuellBearbeitenModel : ObservableObject
{
    private bool _laden;
    private Action<DuellBearbeitenModel>? _aenderung;

    /// <summary>Id des zugrunde liegenden Einzelduells (leer bei geplanten Slots).</summary>
    public Guid DuellId { get; init; }

    /// <summary>Laufende Duellnummer.</summary>
    public int Duellnummer { get; init; }

    /// <summary>Gibt an, ob dieses Duell Teil des Stechens ist.</summary>
    public bool IstStechen { get; init; }

    /// <summary>Gibt an, ob es sich nur um einen geplanten Vorschau-Slot handelt.</summary>
    public bool IstGeplant { get; init; }

    /// <summary>Vorschauname Spieler 1 (für geplante Slots).</summary>
    public string Spieler1Vorschau { get; init; } = string.Empty;

    /// <summary>Vorschauname Spieler 2 (für geplante Slots).</summary>
    public string Spieler2Vorschau { get; init; } = string.Empty;

    /// <summary>Auswählbare Spieler von Team 1.</summary>
    public IReadOnlyList<Spieler> Team1Spieler { get; init; } = [];

    /// <summary>Auswählbare Spieler von Team 2.</summary>
    public IReadOnlyList<Spieler> Team2Spieler { get; init; } = [];

    /// <summary>Aktuell gewählter Spieler von Team 1.</summary>
    [ObservableProperty]
    private Spieler? _gewaehlterSpieler1;

    /// <summary>Aktuell gewählter Spieler von Team 2.</summary>
    [ObservableProperty]
    private Spieler? _gewaehlterSpieler2;

    /// <summary>Trefferzahl Spieler 1 (0–3).</summary>
    [ObservableProperty]
    private int _treffer1;

    /// <summary>Trefferzahl Spieler 2 (0–3).</summary>
    [ObservableProperty]
    private int _treffer2;

    /// <summary>
    /// Befüllt das Modell ohne Auslösen der Änderungsmeldung und registriert den Callback.
    /// </summary>
    public void Initialisieren(
        Spieler? spieler1, Spieler? spieler2, int treffer1, int treffer2,
        Action<DuellBearbeitenModel> aenderung)
    {
        _laden = true;
        GewaehlterSpieler1 = spieler1;
        GewaehlterSpieler2 = spieler2;
        Treffer1 = treffer1;
        Treffer2 = treffer2;
        _aenderung = aenderung;
        _laden = false;
    }

    partial void OnGewaehlterSpieler1Changed(Spieler? value) => Melden();
    partial void OnGewaehlterSpieler2Changed(Spieler? value) => Melden();
    partial void OnTreffer1Changed(int value) => Melden();
    partial void OnTreffer2Changed(int value) => Melden();

    /// <summary>Erhöht die Trefferzahl von Spieler 1 (max. 3).</summary>
    [RelayCommand]
    private void Treffer1Erhoehen() { if (Treffer1 < 3) Treffer1++; }

    /// <summary>Senkt die Trefferzahl von Spieler 1 (min. 0).</summary>
    [RelayCommand]
    private void Treffer1Senken() { if (Treffer1 > 0) Treffer1--; }

    /// <summary>Erhöht die Trefferzahl von Spieler 2 (max. 3).</summary>
    [RelayCommand]
    private void Treffer2Erhoehen() { if (Treffer2 < 3) Treffer2++; }

    /// <summary>Senkt die Trefferzahl von Spieler 2 (min. 0).</summary>
    [RelayCommand]
    private void Treffer2Senken() { if (Treffer2 > 0) Treffer2--; }

    private void Melden()
    {
        if (!_laden)
            _aenderung?.Invoke(this);
    }
}
