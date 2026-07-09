/*************************************************************
 * Datei:        SpeicherstandServiceTests.cs
 * Zweck:        Integrationstests für SpeicherstandService (benannte Stände + Backups)
 * Bereich:      Tests – Datenhaltung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Enums;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Backup;
using KirchenbirkigerSchluck.Data.Speicherstaende;

namespace KirchenbirkigerSchluck.Tests.Data;

/// <summary>
/// Integrationstests für <see cref="SpeicherstandService"/> über echtes Dateisystem.
/// Jede Testinstanz erhält eigene Temp-Verzeichnisse, die in <see cref="Dispose"/> gelöscht werden.
/// </summary>
public class SpeicherstandServiceTests : IDisposable
{
    private readonly string _tempVerzeichnis;
    private readonly string _speicherVerzeichnis;
    private readonly string _backupVerzeichnis;
    private readonly SpeicherstandService _sut;

    /// <summary>Legt eindeutige Temp-Verzeichnisse an und initialisiert den Dienst.</summary>
    public SpeicherstandServiceTests()
    {
        _tempVerzeichnis     = Path.Combine(Path.GetTempPath(), $"ks_stand_{Guid.NewGuid()}");
        _speicherVerzeichnis = Path.Combine(_tempVerzeichnis, "speicherstaende");
        _backupVerzeichnis   = Path.Combine(_tempVerzeichnis, "backups");
        Directory.CreateDirectory(_tempVerzeichnis);
        _sut = new SpeicherstandService(_speicherVerzeichnis, _backupVerzeichnis);
    }

    /// <summary>Räumt die Temp-Verzeichnisse nach jedem Test auf.</summary>
    public void Dispose() => Directory.Delete(_tempVerzeichnis, recursive: true);

    private static Turnier BeispielTurnier(string anlass = "Test-Turnier")
    {
        var turnier = new Turnier
        {
            Anlass         = anlass,
            Datum          = new DateOnly(2026, 7, 9),
            Wertungssystem = Wertungssystem.Eishockey,
            Status         = TurnierStatus.Gruppenphase
        };
        turnier.Teams.Add(new Team { Name = "Bierbrauer", Spieler = [new Spieler { Name = "Anna" }] });
        turnier.Gruppen.Add(new Gruppe { Name = "Gruppe A", Nummer = 1 });
        return turnier;
    }

    // ──────────────────────────────────────────────────────────
    // Speichern + Laden
    // ──────────────────────────────────────────────────────────

    /// <summary>Benannter Speicherstand: Roundtrip stellt den vollständigen Turnierstand wieder her.</summary>
    [Fact]
    public void SpeichernUnter_UndLaden_StelltVollenTurnierstandWiederHer()
    {
        var turnier = BeispielTurnier("Kirchenbirkiger Schluck 2026");

        _sut.SpeichernUnter(turnier, "Vor der Finalrunde", "Alle Gruppenspiele fertig");
        var alle = _sut.Alle();

        alle.Should().HaveCount(1);
        var info = alle[0];
        info.Typ.Should().Be(SpeicherstandTyp.Benannt);
        info.Titel.Should().Be("Vor der Finalrunde");
        info.Beschreibung.Should().Be("Alle Gruppenspiele fertig");
        info.Anlass.Should().Be("Kirchenbirkiger Schluck 2026");
        info.Status.Should().Be(TurnierStatus.Gruppenphase);

        var geladen = _sut.Laden(info);
        geladen.Id.Should().Be(turnier.Id);
        geladen.Teams.Should().ContainSingle(t => t.Name == "Bierbrauer");
        geladen.Teams[0].Spieler.Should().ContainSingle(s => s.Name == "Anna");
        geladen.Gruppen.Should().ContainSingle(g => g.Name == "Gruppe A");
    }

    /// <summary>Gleicher Titel überschreibt den vorhandenen Stand (nur eine Datei).</summary>
    [Fact]
    public void SpeichernUnter_GleicherTitel_Ueberschreibt()
    {
        _sut.SpeichernUnter(BeispielTurnier(), "Mein Stand", null);
        _sut.SpeichernUnter(BeispielTurnier(), "Mein Stand", "aktualisiert");

        var benannte = _sut.Alle().Where(i => i.Typ == SpeicherstandTyp.Benannt).ToList();
        benannte.Should().HaveCount(1);
        benannte[0].Beschreibung.Should().Be("aktualisiert");
    }

    /// <summary>Leerer Titel wird abgelehnt.</summary>
    [Fact]
    public void SpeichernUnter_LeererTitel_WirftArgumentException()
    {
        var act = () => _sut.SpeichernUnter(BeispielTurnier(), "   ", null);
        act.Should().Throw<ArgumentException>();
    }

    // ──────────────────────────────────────────────────────────
    // Backups einbeziehen
    // ──────────────────────────────────────────────────────────

    /// <summary>Automatische Backups erscheinen ebenfalls in der Liste und sind ladbar.</summary>
    [Fact]
    public void Alle_EnthaeltAuchAutomatischeBackups()
    {
        // Ein Backup über den BackupManager erzeugen (reines Turnier-JSON)
        var backupManager = new BackupManager(_backupVerzeichnis);
        var turnier = BeispielTurnier("Backup-Turnier");
        backupManager.BackupErstellen(turnier);

        // Zusätzlich einen benannten Stand
        _sut.SpeichernUnter(BeispielTurnier("Benannt-Turnier"), "Manuell", null);

        var alle = _sut.Alle();
        alle.Should().HaveCount(2);
        alle.Should().Contain(i => i.Typ == SpeicherstandTyp.Backup && i.Anlass == "Backup-Turnier");
        alle.Should().Contain(i => i.Typ == SpeicherstandTyp.Benannt && i.Titel == "Manuell");

        var backupInfo = alle.First(i => i.Typ == SpeicherstandTyp.Backup);
        var geladen = _sut.Laden(backupInfo);
        geladen.Anlass.Should().Be("Backup-Turnier");
    }

    // ──────────────────────────────────────────────────────────
    // Löschen
    // ──────────────────────────────────────────────────────────

    /// <summary>Löschen entfernt den Speicherstand aus der Liste.</summary>
    [Fact]
    public void Loeschen_EntferntSpeicherstand()
    {
        _sut.SpeichernUnter(BeispielTurnier(), "Zum Löschen", null);
        var info = _sut.Alle().Single();

        _sut.Loeschen(info);

        _sut.Alle().Should().BeEmpty();
    }
}
