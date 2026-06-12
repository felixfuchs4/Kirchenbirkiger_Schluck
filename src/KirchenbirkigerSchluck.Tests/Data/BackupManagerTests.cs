/*************************************************************
 * Datei:        BackupManagerTests.cs
 * Zweck:        Unit-Tests für BackupManager (Dateinamen-Generierung)
 * Bereich:      Tests – Datensicherung
 * Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
 * Urheberrecht: Copyright (c) 2026
 *************************************************************/

using FluentAssertions;
using KirchenbirkigerSchluck.Core.Models;
using KirchenbirkigerSchluck.Data.Backup;

namespace KirchenbirkigerSchluck.Tests.Data;

/// <summary>
/// Testet die Dateinamen-Generierung des <see cref="BackupManager"/>.
/// </summary>
public class BackupManagerTests
{
    private readonly BackupManager _sut = new("backups");

    private static Turnier TurnierBauen(string anlass = "Schluck 2026")
        => new()
        {
            Anlass = anlass,
            Datum = new DateOnly(2026, 6, 12)
        };

    /// <summary>Dateiname ohne Spieldetails enthält Zeitstempel und Anlass.</summary>
    [Fact]
    public void DateinameGenerieren_OhneSpieldetails_EnthaltZeitstempelUndAnlass()
    {
        // Arrange
        var turnier = TurnierBauen("Kirchenbirkiger Schluck 2026");
        var zeitpunkt = new DateTime(2026, 6, 12, 14, 30, 0);

        // Act
        var name = _sut.DateinameGenerieren(turnier, zeitpunkt);

        // Assert
        name.Should().StartWith("2026-06-12_14-30-00_");
        name.Should().EndWith(".json");
        name.Should().Contain("Kirchenbirkiger");
    }

    /// <summary>Dateiname mit Spieldetails enthält beide Teamnamen.</summary>
    [Fact]
    public void DateinameGenerieren_MitSpieldetails_EnthaltBeideTeamnamen()
    {
        // Arrange
        var turnier = TurnierBauen("Schluck");
        var zeitpunkt = new DateTime(2026, 6, 12, 15, 0, 0);

        // Act
        var name = _sut.DateinameGenerieren(turnier, zeitpunkt, "Bierbrauer", "Hopfenpflücker");

        // Assert
        name.Should().Contain("Bierbrauer");
        name.Should().Contain("Hopfenpfl");
    }

    /// <summary>Dateiname enthält keine ungültigen Datei-Zeichen.</summary>
    [Fact]
    public void DateinameGenerieren_Sonderzeichen_DateinameIstGueltig()
    {
        // Arrange
        var turnier = TurnierBauen("Schluck & Co. / 2026");
        var zeitpunkt = new DateTime(2026, 6, 12, 12, 0, 0);

        // Act
        var name = _sut.DateinameGenerieren(turnier, zeitpunkt);
        var ungueltigeZeichen = Path.GetInvalidFileNameChars();

        // Assert
        name.Should().NotContainAny(ungueltigeZeichen.Select(c => c.ToString()));
    }

    // ──────────────────────────────────────────────────────────
    // BackupErstellen – Smoke-Test
    // ──────────────────────────────────────────────────────────

    /// <summary>BackupErstellen legt die Backup-Datei im angegebenen Verzeichnis an.</summary>
    [Fact]
    public void BackupErstellen_ErzeugtDateiImVerzeichnis()
    {
        // Arrange
        var tempVerzeichnis = Path.Combine(Path.GetTempPath(), $"ks_backup_{Guid.NewGuid()}");
        var sut = new BackupManager(tempVerzeichnis);
        var turnier = TurnierBauen("Backup-Test");

        try
        {
            // Act
            var vollpfad = sut.BackupErstellen(turnier);

            // Assert
            File.Exists(vollpfad).Should().BeTrue();
            vollpfad.Should().StartWith(tempVerzeichnis);
        }
        finally
        {
            // Aufräumen
            if (Directory.Exists(tempVerzeichnis))
                Directory.Delete(tempVerzeichnis, recursive: true);
        }
    }
}
