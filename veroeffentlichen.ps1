# *************************************************************
# Datei:        veroeffentlichen.ps1
# Zweck:        Baut die self-contained Anwendung und erzeugt den Windows-Installer
# Bereich:      Betrieb - Auslieferung
# Ersteller:    Kirchenbirkiger Schluck Entwicklungsteam
# Urheberrecht: Copyright (c) 2026
# *************************************************************
#
# Ablauf:
#   1. dotnet publish (self-contained, win-x64) -> Ordner .\publish
#   2. Inno Setup (ISCC) -> .\installer\Output\KirchenbirkigerSchluck_Setup.exe
#
# Aufruf (in PowerShell, im Projekt-Root):
#   .\veroeffentlichen.ps1
#
# Nur zum BAUEN wird Inno Setup benoetigt, nicht beim Endnutzer.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "== Schritt 1/2: dotnet publish ==" -ForegroundColor Cyan
dotnet publish "src\KirchenbirkigerSchluck.App\KirchenbirkigerSchluck.App.csproj" `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false `
    -o "publish"
if ($LASTEXITCODE -ne 0) { throw "dotnet publish ist fehlgeschlagen." }

Write-Host "== Schritt 2/2: Installer bauen (Inno Setup) ==" -ForegroundColor Cyan

# ISCC.exe suchen (PATH oder Standard-Installationspfade)
$iscc = (Get-Command iscc.exe -ErrorAction SilentlyContinue).Source
if (-not $iscc) {
    foreach ($p in @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe")) {
        if (Test-Path $p) { $iscc = $p; break }
    }
}

if (-not $iscc) {
    Write-Warning "Inno Setup (ISCC.exe) wurde nicht gefunden."
    Write-Host "Einmalig installieren mit:" -ForegroundColor Yellow
    Write-Host "    winget install --id JRSoftware.InnoSetup -e" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Die App wurde bereits nach '.\publish' veroeffentlicht." -ForegroundColor Green
    Write-Host "Nach der Inno-Setup-Installation dieses Skript erneut ausfuehren." -ForegroundColor Green
    exit 1
}

& $iscc "installer\KirchenbirkigerSchluck.iss"
if ($LASTEXITCODE -ne 0) { throw "Inno Setup (ISCC) ist fehlgeschlagen." }

$setup = Join-Path $root "installer\Output\KirchenbirkigerSchluck_Setup.exe"
Write-Host ""
Write-Host "Fertig! Installer erstellt:" -ForegroundColor Green
Write-Host "    $setup" -ForegroundColor Green
