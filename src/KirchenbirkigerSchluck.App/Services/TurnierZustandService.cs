/*************************************************************
 * Datei:        TurnierZustandService.cs
 * Zweck:        Gemeinsamer Turnier-Zustandshalter für alle ViewModels der Bedienoberfläche
 * Bereich:      Präsentation – Dienste
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.App.Services;

/// <summary>
/// Hält das aktuell geladene Turnier und benachrichtigt alle ViewModels bei Änderungen.
/// Wird als Singleton im DI-Container registriert.
/// </summary>
public class TurnierZustandService
{
    private Turnier? _aktuellesTurnier;

    /// <summary>Das aktuell geladene Turnier, oder <c>null</c> wenn keines geladen ist.</summary>
    public Turnier? AktuellesTurnier => _aktuellesTurnier;

    /// <summary>Wird ausgelöst, wenn ein neues Turnier gesetzt oder das bestehende geändert wurde.</summary>
    public event EventHandler? TurnierGeaendert;

    /// <summary>Setzt das aktuelle Turnier und benachrichtigt alle Abonnenten.</summary>
    public void TurnierSetzen(Turnier turnier)
    {
        _aktuellesTurnier = turnier;
        TurnierGeaendert?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Löst <see cref="TurnierGeaendert"/> aus, ohne das Turnier-Objekt zu ersetzen (z.B. nach Mutationen).</summary>
    public void AenderungMelden()
    {
        TurnierGeaendert?.Invoke(this, EventArgs.Empty);
    }
}
