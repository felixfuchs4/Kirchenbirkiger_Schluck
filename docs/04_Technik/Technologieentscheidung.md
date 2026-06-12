# Technologieentscheidung

Dieses Dokument beschreibt die Technologiewahl für das Projekt **Kirchenbirkiger Schluck Programm** und hält die Entscheidungsgründe fest.

## 1. Entscheidungsrelevante Anforderungen

Die folgenden Anforderungen aus den Fachkonzept- und Anforderungsdokumenten sind maßgeblich für die Framework-Wahl:

| Kriterium | Anforderung |
|-----------|------------|
| Plattform | Windows-only (explizit festgelegt) |
| Fenster | 2 unabhängige Fenster: Bedienoberfläche (Laptop) und Zuschaueranzeige (externer Bildschirm, Vollbild) |
| Deployment | Portable EXE (Ordner + `.exe`, kein Installer bevorzugt) |
| UI-Anspruch | Modern – explizit kein klassisches WinForms-Look |
| Live-Updates | Matchday-Screen reagiert sofort auf Eingaben der Turnierleitung |
| Datenhaltung | JSON-Dateien (kein Datenbankserver) |
| Animationen | V1 funktional; Animationen und Webcam-Integration für spätere Versionen geplant |
| Runtime | .NET-Runtime muss auf Zielrechner vorhanden sein |
| Netzwerk | Kein Internetzugang nötig, rein lokaler Betrieb |

---

## 2. Bewertete Frameworks

### 2.1 WPF – Windows Presentation Foundation (.NET 8+)

WPF ist das etablierte XAML-basierte UI-Framework von Microsoft für Windows-Desktop-Anwendungen.

**Vorteile**

- Ausgereift und stabil (seit .NET 3.0, weiterentwickelt auf .NET 8+)
- Sehr große Community, ausgezeichnete Dokumentation
- Zwei unabhängige Fenster und Vollbild-Betrieb: Standardszenario, gut dokumentiert
- MVVM und DataBinding: erstklassige Unterstützung, ideal für reaktive Live-Updates
- Portable self-contained Deployment: trivial via `dotnet publish --self-contained`
- Breite Auswahl moderner Drittanbieter-UI-Bibliotheken (MaterialDesignInXamlToolkit, MahApps.Metro)
- Volle Kontrolle über benutzerdefinierte Layouts – wichtig für Vollbild-Anzeigescreen

**Nachteile**

- Kerntechnologie ist älter (~2006); kein natives Fluent Design ohne Drittanbieter-Bibliothek
- Animations-API weniger modern als WinUI 3 (Storyboards statt Composition-API)
- Kein Hot Reload für XAML im gleichen Umfang wie neuere Frameworks

---

### 2.2 WinUI 3 – Windows App SDK

WinUI 3 ist Microsofts modernste Windows-UI-Technologie, nachfolger der UWP-Plattform.

**Vorteile**

- Modernste Microsoft-UI-Technologie mit nativem Fluent Design System
- Hochperformante Composition-Animationen ohne Drittanbieter-Bibliothek
- Strategische Investition von Microsoft (langfristige Unterstützung erwartet)
- Besseres Rendering bei hochauflösenden Displays und DPI-Skalierung

**Nachteile**

- Deployment-Komplexität: MSIX-Paket oder deutlich größere self-contained-Pakete nötig; portable EXE-Ablage aufwändiger
- Multi-Fenster-Verwaltung grundlegend anders als WPF (kein einfaches `new Window()`, eigenes AppWindow-API)
- Kleinere Community, deutlich weniger StackOverflow-Ressourcen und Beispielprojekte
- Framework noch reifend: bekannte Edge-Cases bei 2-Monitor-Szenarien und Vollbild-Handling
- Lernkurve für das neue Programmiermodell (DispatcherQueue, AppWindow, etc.)

---

### 2.3 Avalonia UI

Avalonia ist ein quelloffenes, plattformübergreifendes XAML-Framework, das WPF-ähnliche Syntax auf Windows, macOS und Linux bringt.

**Vorteile**

- Cross-Platform XAML mit WPF-ähnlicher Syntax (Migration von WPF einfach)
- Modernes Rendering-Backend (Skia-basiert, smooth und performant)
- Aktive Open-Source-Community, kommerzieller Support verfügbar

**Nachteile**

- Cross-Platform-Vorteil ist für dieses Projekt irrelevant (Windows-only-Anforderung)
- Kleinere Community als WPF für Windows-spezifische Probleme
- Windows-Systemintegrationen (Monitor-Erkennung, DPI-Handling) weniger ausgereift als WPF
- Geringere Anzahl verfügbarer Windows-nativer UI-Bibliotheken

---

### 2.4 Windows Forms (WinForms) – ausgeschlossen

WinForms ist das älteste .NET-UI-Framework und für einfache Desktopanwendungen geeignet.

**Ausschlussgrund**: Die Projektentscheidungen fordern ausdrücklich „moderne grafische Möglichkeiten" und schließen einen klassischen WinForms-Look aus. WinForms hat keine MVVM-Unterstützung und ist für die Vollbild-Zuschaueranzeige ungeeignet.

---

### 2.5 .NET MAUI – ausgeschlossen

MAUI ist das plattformübergreifende Nachfolge-Framework für Xamarin.Forms.

**Ausschlussgrund**: Desktop-Unterstützung unter MAUI ist noch nicht ausgereift. MAUI ist primär auf Mobile-Szenarien ausgelegt und nicht für komplexe Dual-Screen-Desktop-Anwendungen konzipiert.

---

## 3. Vergleichsmatrix

| Kriterium | WPF | WinUI 3 | Avalonia |
|-----------|-----|---------|----------|
| Windows-Only Eignung | ✓✓ | ✓✓ | ✓ |
| 2-Fenster / Vollbild | ✓✓ | ✓ | ✓✓ |
| Portable EXE | ✓✓ | ✓ | ✓✓ |
| Modernes Erscheinungsbild | ✓ (mit Libs) | ✓✓ | ✓ |
| MVVM / DataBinding | ✓✓ | ✓✓ | ✓✓ |
| Live-Update-Fähigkeit | ✓✓ | ✓✓ | ✓✓ |
| Community / Ressourcen | ✓✓ | ✓ | ✓ |
| Reife / Stabilität | ✓✓ | ✓ | ✓ |
| Erweiterbarkeit (Animationen) | ✓ | ✓✓ | ✓ |
| Deployment-Einfachheit | ✓✓ | ✓ | ✓✓ |

Legende: ✓✓ = gut geeignet, ✓ = geeignet, ✗ = nicht geeignet

---

## 4. Empfehlung und Entscheidung

### Gewähltes Framework: WPF (.NET 8)

**Begründung:**

Die Windows-only-Anforderung macht Cross-Platform-Frameworks (Avalonia, MAUI) überflüssig.

Zwischen WPF und WinUI 3 fällt die Entscheidung für **WPF**, weil:

- Das Zwei-Fenster-Vollbild-Setup ist bei WPF ein Standardszenario mit tausenden dokumentierten Beispielen.
- Portable self-contained Deployment ist bei WPF der einfachste und zuverlässigste aller Kandidaten.
- Die größte Community bedeutet schnellste Problemlösung bei unerwarteten Hürden.
- WinUI 3 bietet für Version 1 keine entscheidenden Vorteile, bringt aber erheblich höhere Einstiegskomplexität (MSIX, AppWindow-API).
- Moderne Optik ist durch **MaterialDesignInXamlToolkit** ohne Eigenbau erreichbar.

WinUI 3 bleibt als spätere Migrationsoption offen, falls in Folgeversionen native Fluent-Animationen oder neue Windows-11-Integrationen benötigt werden.

---

## 5. Gewählter Technologiestack

| Schicht | Technologie | Begründung |
|---------|-------------|------------|
| Sprache | C# 12 (.NET 8) | Festgelegt in Projektentscheidungen |
| UI-Framework | WPF (.NET 8) | Stabil, 2-Fenster-Vollbild, portable EXE |
| UI-Bibliothek | MaterialDesignInXamlToolkit | Modernes Erscheinungsbild ohne WinForms-Charakter |
| MVVM | CommunityToolkit.Mvvm | RelayCommand, ObservableObject, Source Generators |
| JSON | System.Text.Json (BCL) | Im .NET 8 SDK enthalten, kein extra Paket nötig |
| Build | `dotnet build` / `dotnet publish` | Standard .NET CLI |

---

## 6. Verwendete NuGet-Pakete (V1)

| Paket | Version | Zweck |
|-------|---------|-------|
| `MaterialDesignThemes` | aktuell | Modernes UI-Theming (Buttons, Cards, Icons, Farben) |
| `CommunityToolkit.Mvvm` | aktuell | MVVM-Boilerplate (Commands, ObservableObject, Messaging) |
| `System.Text.Json` | im SDK | JSON-Serialisierung und -Deserialisierung |

Keine weiteren Abhängigkeiten für V1 geplant. Webcam-Integration (z. B. über DirectShow-Wrapper) wird erst bei tatsächlicher Umsetzung evaluiert.

---

## 7. Offene technische Folgeentscheidungen

Diese Punkte werden in separaten Dokumenten festgelegt:

| Thema | Dokument |
|-------|---------|
| Genaues JSON-Speicherschema (Entitäten, Felder, Versioning) | [Speicherformat.md](Speicherformat.md) |
| Solution-Struktur, Projektaufteilung, Namenskonventionen | [Projektstruktur.md](Projektstruktur.md) |
| Datenmodell (Klassen, Beziehungen, ER-Diagramm) | [Datenmodell.md](Datenmodell.md) |
| Tiebreaker-Regelung bei Punktgleichheit | [Wertungslogik.md](Wertungslogik.md) |

---

## 8. Entscheidungsprotokoll

| Datum | Entscheidung | Grund |
|-------|-------------|-------|
| 2026-06-12 | C# als Sprache | Ersetzt bisherige Excel-Makro-Lösung; .NET-Ökosystem |
| 2026-06-12 | WPF (.NET 8) als UI-Framework | Stabilität, 2-Fenster-Vollbild, Community, portables Deployment |
| 2026-06-12 | MaterialDesignInXamlToolkit als UI-Bibliothek | Modernes Erscheinungsbild ohne nativen WinForms-Charakter |
| 2026-06-12 | JSON als Speicherformat | Kein Datenbankserver nötig, menschenlesbar, einfaches Backup |

Siehe auch [Projektentscheidungen.md](../00_Projektstart/Projektentscheidungen.md).
