# Designrichtlinie

> **Kanonische Quelle im Code:** `src/KirchenbirkigerSchluck.App/Resources/Design/`
> (`01_Tokens.xaml` = Farben/Abstände/Radien/Motion, `02_Effekte.xaml`, `03_Typografie.xaml`,
> `04_Bedienung.xaml`, `05_Anzeige.xaml`). Hex-Werte dürfen ausschließlich in
> `01_Tokens.xaml` definiert werden – Views verwenden semantische Tokens
> (`Pinsel…`, `Verlauf…`, `Radius…`, `Dauer…`).
> Einzige Ausnahme: das Bühnenbild der `AuslosungView` (Loskugel, Trommel) mit
> bewussten Einmal-Werten sowie Schwarz-/Alpha-Weiß-Dekor.

## Designprinzip

Ein Produkt, ein Token-System, **zwei Register**:

| Register | Charakter | Motion |
|---|---|---|
| **Anzeige** (Beamer) | Bühne/Broadcast: dunkel, kontrastreich, große Typografie, Gold-Glow gezielt (Score, Sieger) | ruhig: 400 ms Screen-/Folienwechsel, 700 ms Zeremonie |
| **Bedienung** (Turnierleitung) | Werkzeug: MaterialDesign Dark, Gold als Primärfarbe, Dichte und Konsistenz | Zustands-Feedback: 150–250 ms |

## Farbpalette (semantische Tokens)

| Token | Hex | Verwendung |
|-------|-----|------------|
| `PinselAkzent` (Gold 500) | `#FFC84B` | Markenfarbe: Primäraktionen, Scores, Selektion, Platzierungen |
| `PinselAkzent2` (Orange) | `#FF7A45` | Zweitakzent in Verläufen (`VerlaufAkzent`: `#FFD86B → #FFC84B → #FF9A3D`) |
| `PinselSekundaer` (Violett 500/700) | `#7E57C2` / `#5E35B1` | Gruppenköpfe, sekundäre Betonungen (`VerlaufSekundaer`) |
| `PinselFensterAnzeige` (Nacht 950) | `#0B0B1A` | Beamer-Fensterhintergrund (`VerlaufAnzeigeHintergrund`: 950 → 800 → 700) |
| `PinselFensterBedienung` (Nacht 850) | `#14152B` | Bedien-Fensterhintergrund (überschreibt `MaterialDesign.Brush.Background`) |
| `PinselNavigation` (Nacht 900) | `#101120` | Navigationsleiste |
| `PinselFlaeche` (Fläche 800) | `#1E2038` | Karten der Bedienung (`MaterialDesign.Brush.Card.Background`) |
| `PinselFlaecheDunkel` / `Erhaben` / `Hover` / `Aktiv` | `#191733` / `#22203F` / `#2A2550` / `#2E2858` | Listenzeilen, Chips, Interaktionszustände |
| `PinselText` | `#F5F5FA` | Primärtext |
| `PinselTextGedaempft` | `#A6A6C6` | Sekundärtext (Kontrast ≥ 6:1 auf Kartenflächen) |
| `PinselTextAufAkzent` | `#14152B` | Text auf Gold/Statusfarben |
| `PinselRand` / `PinselTrennlinie` | `#3A3768` / `#34315E` | Konturen |
| `PinselErfolg` / `Warnung` / `Gefahr` / `Info` | `#4CAF50` / `#FFA726` / `#EF5350` / `#4FC3F7` | Status-Pillen, destruktive Aktionen |
| `PinselLive` / `PinselStechen` | `#FF4B4B` / `#D43A2F` | LIVE-Indikator, Stechen-Badges |

## Typografie

Anzeige: `Segoe UI Variable Display` (Fallback `Segoe UI`); Bedienung: MaterialDesign-Schrift.

| Element (Style) | Größe | Gewicht |
|---------|-------|---------|
| Score Matchday (`ScoreTextStyle`) | 120 | Bold, `VerlaufAkzent` + `GlowGold` |
| Uhrzeit Startscreen (`UhrzeitStyle`) | 110 | Thin |
| Sieger (`SiegerHervorhebungStyle`) | 84 | Bold, Gold + Glow |
| Teamname Anzeige (`TeamNameAnzeigeStyle`) | 44 | Bold |
| Screen-Überschrift (`UeberschriftAnzeigeStyle`) | 34 | Bold, Gold + sanfter Glow |
| Anzeigetext (`AnzeigeTextStyle`) | 26 | Regular |
| Gedämpfter Anzeigetext (`AnzeigeTextGedaempftStyle`) | **22 (Minimum am Beamer)** | Regular |
| Bedienung: Seitentitel / Abschnitt / Untertitel / Hinweis | 24 / 15 / 13 / 12 | SemiBold bzw. Regular |

## Abstände, Radien, Elevation

- **Abstands-Skala (4er-Raster):** 4 / 8 / 12 / 16 / 24 / 32 / 48 / 64 – Inline-Margins müssen Vielfache von 4 sein (`Abstand…`-Tokens für Paddings).
- **Radien:** `RadiusS` 8 (Hinweise, kleine Chips), `RadiusM` 12 (Zeilen, Pillen), `RadiusL` 16 (Karten – Maximum), `RadiusPille` (Badges/Status).
- **Effekte** (sparsam – DropShadow rendert auf der CPU): `GlowGold` (Score/Sieger), `GlowGoldSanft` (Überschriften), `GlowRot` (Stechen), `SchattenKarte` (Anzeige-Karten), `SchattenFlach` (Bedienung).

## Motion

| Token | Dauer | Einsatz |
|---|---|---|
| `DauerFlink` | 150 ms | Hover/Selektion Bedienung (Tab-Navigation) |
| `DauerStandard` | 250 ms | Score-Pop (`WertPulsVerhalten`, Scale 1 → 1,12 → 1) |
| `DauerRuhig` | 400 ms | Screen-/Folienwechsel (`ScreenUebergangVerhalten`: Crossfade + 16 px Slide, QuarticOut) |
| `DauerBuehne` | 700 ms | Siegerehrungs-Schritte |

Regeln: nur Opacity/Transform animieren (kein Layout), Easing immer EaseOut (`EasingStandard`/`EasingBuehne`), kein Bounce außer bewusst in der Auslosung (BackEase-Pop-in). Jede Animation braucht einen Zweck (Zustand, Feedback, Bühnenwirkung) – keine Dekoration. Endlos-Animationen: LIVE-Puls, Trommel-Rotation, atmende Uhr (6 s), AutoScroll langer Listen (`AutoScrollVerhalten`).

## Wiederverwendbare Controls (`Views/Gemeinsam/`)

| Control | Zweck |
|---|---|
| `TeamAnzeige` | Logo-über-Name-Einheit der Anzeige (DPs: `LogoPfad`, `TeamName`, `LogoGroesse`, `NameGroesse`) |
| `LiveIndikator` | Pulsierender LIVE-Punkt + Schriftzug |
| `LeererZustand` | Leerzustand der Bedienung: Symbol + Titel + Nächster-Schritt-Hinweis |

## Logo und Branding

> Logo befindet sich in [assets/logo/](../../assets/logo/); eingebettet unter
> `src/KirchenbirkigerSchluck.App/Resources/Assets/`.

## Barrierefreiheit / Beamer-Lesbarkeit

- Fließtext ≥ 4,5:1 Kontrast; gedämpfter Text `#A6A6C6` auf Kartenfläche ≈ 6,5:1.
- Kleinste Schrift am Beamer: 22 px; wenige Fakten pro Ansicht, keine überladenen Tabellen.
- Statusinformation nie nur über Farbe (Status-Pillen tragen immer Text).
- Bedienoberfläche: MaterialDesign-Dark-Basistheme liefert Fokus-/Disabled-Zustände; Primärfarbe Gold mit dunkler Foreground (automatisch berechnet).
