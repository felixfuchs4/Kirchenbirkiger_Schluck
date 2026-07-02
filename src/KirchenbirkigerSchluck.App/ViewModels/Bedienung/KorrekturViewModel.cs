/*************************************************************
 * Datei:        KorrekturViewModel.cs
 * Zweck:        ViewModel für die Anzeige des Änderungsprotokolls
 * Bereich:      Präsentation – Bedienungs-ViewModels
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KirchenbirkigerSchluck.App.Services;

namespace KirchenbirkigerSchluck.App.ViewModels.Bedienung;

/// <summary>Anzeigemodell für einen einzelnen Eintrag im Änderungsprotokoll.</summary>
public sealed class AenderungseintragAnzeigeModel
{
    /// <summary>Zeitpunkt der Änderung (lokal formatiert).</summary>
    public string Zeitpunkt { get; init; } = string.Empty;

    /// <summary>Name der geänderten Entität (z.B. „Spiel", „Team").</summary>
    public string Entitaet { get; init; } = string.Empty;

    /// <summary>Geändertes Feld.</summary>
    public string Feld { get; init; } = string.Empty;

    /// <summary>Wert vor der Änderung.</summary>
    public string AlterWert { get; init; } = string.Empty;

    /// <summary>Wert nach der Änderung.</summary>
    public string NeuerWert { get; init; } = string.Empty;

    /// <summary>Optionale Begründung der Turnierleitung.</summary>
    public string Begruendung { get; init; } = string.Empty;
}

/// <summary>
/// Zeigt das Änderungsprotokoll des aktuellen Turniers an.
/// Einträge werden neueste zuerst angezeigt.
/// </summary>
public partial class KorrekturViewModel : ObservableObject
{
    private readonly TurnierZustandService _turnierZustand;

    /// <summary>Initialisiert das ViewModel und abonniert Turnierändrungs-Events.</summary>
    public KorrekturViewModel(TurnierZustandService turnierZustandService)
    {
        _turnierZustand = turnierZustandService;
        turnierZustandService.TurnierGeaendert += (_, _) => ProtokollAktualisieren();
        ProtokollAktualisieren();
    }

    /// <summary>Alle Änderungseinträge, neueste zuerst.</summary>
    public ObservableCollection<AenderungseintragAnzeigeModel> Eintraege { get; } = [];

    /// <summary>Gibt an, ob Einträge vorhanden sind.</summary>
    [ObservableProperty]
    private bool _hatEintraege;

    private void ProtokollAktualisieren()
    {
        Eintraege.Clear();
        var turnier = _turnierZustand.AktuellesTurnier;

        if (turnier is null)
        {
            HatEintraege = false;
            return;
        }

        foreach (var eintrag in turnier.Aenderungsprotokoll.OrderByDescending(e => e.ZeitpunktUtc))
        {
            Eintraege.Add(new AenderungseintragAnzeigeModel
            {
                Zeitpunkt = eintrag.ZeitpunktUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"),
                Entitaet = eintrag.Entitaet,
                Feld = eintrag.Feld,
                AlterWert = eintrag.AlterWert ?? "–",
                NeuerWert = eintrag.NeuerWert,
                Begruendung = eintrag.Begruendung ?? string.Empty
            });
        }

        HatEintraege = Eintraege.Count > 0;
    }
}
