/*************************************************************
 * Datei:        IAenderungsprotokollService.cs
 * Zweck:        Schnittstelle für das Änderungsprotokoll
 * Bereich:      Anwendungslogik – Revisionsverwaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using KirchenbirkigerSchluck.Core.Models;

namespace KirchenbirkigerSchluck.Core.Interfaces;

/// <summary>
/// Definiert Operationen zum Erstellen und Abfragen von Änderungsprotokolleinträgen.
/// </summary>
public interface IAenderungsprotokollService
{
    /// <summary>
    /// Legt einen neuen Eintrag im Änderungsprotokoll des Turniers an.
    /// </summary>
    /// <param name="turnier">Das Turnierobjekt (Protokoll wird direkt ergänzt).</param>
    /// <param name="entitaet">Name der geänderten Entitätsklasse, z. B. „Spiel".</param>
    /// <param name="entitaetId">Id der betroffenen Entität.</param>
    /// <param name="feld">Name des geänderten Felds.</param>
    /// <param name="alterWert">Vorheriger Wert als String. Null wenn neu angelegt.</param>
    /// <param name="neuerWert">Neuer Wert als String.</param>
    /// <param name="begruendung">Optionale Begründung der Turnierleitung.</param>
    void EintragErstellen(
        Turnier turnier,
        string entitaet,
        Guid entitaetId,
        string feld,
        string? alterWert,
        string neuerWert,
        string? begruendung = null);

    /// <summary>
    /// Gibt alle Änderungseinträge für eine bestimmte Entität zurück, neueste zuerst.
    /// </summary>
    /// <param name="turnier">Das Turnierobjekt.</param>
    /// <param name="entitaetId">Id der abzufragenden Entität.</param>
    IReadOnlyList<Aenderungseintrag> EintraegeAbfragen(Turnier turnier, Guid entitaetId);
}
