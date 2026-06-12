/*************************************************************
 * Datei:        ITurnierRepository.cs
 * Zweck:        Schnittstelle für den Datenzugriff auf das Turnier-Objekt
 * Bereich:      Datenhaltung – Abstraktion
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Definiert Operationen zum Laden und Speichern des Turnier-Wurzelobjekts.
/// </summary>
public interface ITurnierRepository
{
    /// <summary>Lädt das gespeicherte Turnier und gibt es zurück.</summary>
    /// <exception cref="System.IO.FileNotFoundException">Wenn keine Turnierdatei vorhanden ist.</exception>
    Turnier Laden();

    /// <summary>Speichert das übergebene Turnierobjekt persistent.</summary>
    /// <param name="turnier">Das zu speichernde Turnierobjekt.</param>
    void Speichern(Turnier turnier);

    /// <summary>Gibt an, ob bereits eine gespeicherte Turnierdatei existiert.</summary>
    bool ExistiertDatei();
}
