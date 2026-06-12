/*************************************************************
 * Datei:        Einzelduell.cs
 * Zweck:        Domänenmodell für ein Einzelduell zwischen zwei Spielern
 * Bereich:      Domänenmodell – Spielablauf
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

namespace KirchenbirkigerSchluck.Core.Models;

/// <summary>
/// Repräsentiert ein Einzelduell zwischen je einem Spieler beider Teams.
/// </summary>
/// <remarks>
/// Verantwortung: Datenhaltung für einen Duell-Abschnitt einer Partie
/// inklusive aller Versuche und des Ergebnisses.
/// Einsatzkontext: Pro regulärer Partie gibt es 5 Einzelduelle (Duellnummer 1–5),
/// danach weitere bei Stechen (Duellnummer ab 6).
/// Besonderheiten: <c>Ergebnis</c> ist null solange das Duell läuft.
/// </remarks>
public class Einzelduell
{
    /// <summary>Eindeutiger Bezeichner des Einzelduells.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Id der zugehörigen Partie.</summary>
    public Guid SpielId { get; set; }

    /// <summary>
    /// Laufende Duellnummer innerhalb der Partie.
    /// 1–5 für reguläre Duelle, ab 6 für Stechen-Duelle.
    /// </summary>
    public int Duellnummer { get; set; }

    /// <summary>Gibt an, ob dieses Duell Teil des Stechens ist.</summary>
    public bool IstStechen { get; set; }

    /// <summary>Id des antretenden Spielers von Team 1.</summary>
    public Guid Spieler1Id { get; set; }

    /// <summary>Id des antretenden Spielers von Team 2.</summary>
    public Guid Spieler2Id { get; set; }

    /// <summary>Liste aller Versuche in diesem Duell (maximal 3).</summary>
    public List<Versuch> Versuche { get; set; } = [];

    /// <summary>
    /// Ergebnis des Duells. Null solange das Duell noch läuft.
    /// </summary>
    public EinzelduellErgebnis? Ergebnis { get; set; }
}
