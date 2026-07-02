# Build und Start

Kurzanleitung zum Bauen und Starten der Anwendung. Alle Befehle in **PowerShell** aus dem
Projekt-Stammverzeichnis (`d:\Documents\Projekte\Kirchenbirkiger_Schluck`).

> Hinweis: Es gibt **keine** `.sln`-Datei. Befehle zielen direkt auf das App-Projekt.

## Schnellstart (Bauen + Starten in einem Schritt)

```powershell
dotnet run --project src\KirchenbirkigerSchluck.App\KirchenbirkigerSchluck.App.csproj
```

## Nur Bauen

```powershell
dotnet build src\KirchenbirkigerSchluck.App\KirchenbirkigerSchluck.App.csproj
```

## Gebaute EXE direkt starten (ohne Neu-Build)

```powershell
.\src\KirchenbirkigerSchluck.App\bin\Debug\net8.0-windows\KirchenbirkigerSchluck.App.exe
```

## Tests ausführen

```powershell
# Alle Tests
dotnet test src\KirchenbirkigerSchluck.Tests\KirchenbirkigerSchluck.Tests.csproj

# Einzelnen Test über Namensfilter
dotnet test src\KirchenbirkigerSchluck.Tests\KirchenbirkigerSchluck.Tests.csproj --filter "FullyQualifiedName~TestName"
```

## Release-Build (für die Weitergabe / den Einsatz vor Ort)

```powershell
dotnet build src\KirchenbirkigerSchluck.App\KirchenbirkigerSchluck.App.csproj -c Release
```

Ergebnis liegt unter `src\KirchenbirkigerSchluck.App\bin\Release\net8.0-windows\`.

## Häufige Stolpersteine

| Problem | Ursache / Lösung |
|---|---|
| `MSB1009: Die Projektdatei ist nicht vorhanden` | Relativer Pfad mit Backslash falsch interpretiert – vollständigen oder korrekten Pfad nutzen. |
| `Die Datei wird durch ein anderes Programm gesperrt` | Die Anwendung läuft noch. Vor dem Neu-Build schließen. |
| Anzeigefenster erscheint nicht auf dem Beamer | Beamer als erweiterten Bildschirm (nicht Duplizieren) einrichten; Fenster wird auf den zweiten Bildschirm positioniert. |
| `error BG1002` / `error MC3024`-Folgefehler oder fehlende `.baml` / `.g.cs` nach einem Teil-Clean | WPF-Markup-Compiler in flakigem Zustand. Lösung: `obj` und `bin` **vollständig** löschen, dann neu bauen: `dotnet build-server shutdown; rm -rf src/KirchenbirkigerSchluck.App/obj src/KirchenbirkigerSchluck.App/bin; dotnet build src/KirchenbirkigerSchluck.App/KirchenbirkigerSchluck.App.csproj` (ggf. zweimal ausführen). |
