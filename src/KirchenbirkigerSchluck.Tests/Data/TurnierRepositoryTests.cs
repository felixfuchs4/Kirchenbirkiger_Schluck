/*************************************************************
 * Datei:        TurnierRepositoryTests.cs
 * Zweck:        Integrationstests für TurnierRepository (JSON-Roundtrip)
 * Bereich:      Tests – Datenhaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Repositories;

namespace KirchenbirkigerSchluck.Tests.Data;

/// <summary>
/// Integrationstests für <see cref="TurnierRepository"/>: schreiben und lesen über echtes Dateisystem.
/// Jede Testinstanz erhält ein eigenes temporäres Verzeichnis, das in <see cref="Dispose"/> gelöscht wird.
/// </summary>
public class TurnierRepositoryTests : IDisposable
{
    private readonly string _tempVerzeichnis;
    private readonly TurnierRepository _sut;

    /// <summary>Legt ein eindeutiges Temp-Verzeichnis an und initialisiert das Repository.</summary>
    public TurnierRepositoryTests()
    {
        _tempVerzeichnis = Path.Combine(Path.GetTempPath(), $"ks_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempVerzeichnis);
        _sut = new TurnierRepository(Path.Combine(_tempVerzeichnis, "turnier.json"));
    }

    /// <summary>Räumt das Temp-Verzeichnis nach jedem Test auf.</summary>
    public void Dispose()
        => Directory.Delete(_tempVerzeichnis, recursive: true);

    // ──────────────────────────────────────────────────────────
    // Roundtrip
    // ──────────────────────────────────────────────────────────

    /// <summary>Turnier serialisieren und laden → Id, Anlass, Datum und Wertungssystem sind identisch.</summary>
    [Fact]
    public void Speichern_Laden_Roundtrip_ObjektIstIdentisch()
    {
        // Arrange
        var turnier = new Turnier
        {
            Anlass       = "Kirchenbirkiger Schluck 2026",
            Datum        = new DateOnly(2026, 6, 12),
            Wertungssystem = Wertungssystem.Eishockey
        };

        // Act
        _sut.Speichern(turnier);
        var geladen = _sut.Laden();

        // Assert
        geladen.Id.Should().Be(turnier.Id);
        geladen.Anlass.Should().Be(turnier.Anlass);
        geladen.Datum.Should().Be(turnier.Datum);
        geladen.Wertungssystem.Should().Be(turnier.Wertungssystem);
    }

    /// <summary>Teamname mit Umlauten (ä, ö, ü, ß) wird nach Roundtrip korrekt wiederhergestellt.</summary>
    [Fact]
    public void Speichern_Laden_TeamNameMitUmlauten_WirdKorrektWiederhergestellt()
    {
        // Arrange
        const string teamName = "Münchner Öl-Brüder & Söhne ß";
        var turnier = new Turnier { Anlass = "Test" };
        turnier.Teams.Add(new Team { Name = teamName });

        // Act
        _sut.Speichern(turnier);
        var geladen = _sut.Laden();

        // Assert
        geladen.Teams.Should().HaveCount(1);
        geladen.Teams[0].Name.Should().Be(teamName);
    }

    // ──────────────────────────────────────────────────────────
    // Fehlerfall
    // ──────────────────────────────────────────────────────────

    /// <summary>Laden ohne existierende Datei wirft FileNotFoundException.</summary>
    [Fact]
    public void Laden_DateiFehlt_WirftFileNotFoundException()
    {
        // Arrange – Repository mit Pfad ohne Datei (Konstruktor legt keine Datei an)

        // Act
        var act = _sut.Laden;

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }
}
