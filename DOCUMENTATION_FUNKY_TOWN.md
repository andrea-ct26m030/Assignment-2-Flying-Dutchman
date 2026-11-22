# Funky Town – Dokumentation

Dieses Projekt baut auf den Abgaben *Flying Dutchman* (`DOCUMENTATION.md`) und *Dirty Dancing* (`DOCUMENTATION_DIRTY_DANCING.md`) auf. Raumschiff, Spawner, Projektile, Tanzplattform und Gäste stammen aus diesen Abgaben und werden hier nicht erneut beschrieben. Die Szene `AssignmentScene` wurde um ein UI (Distanz, Kompass, Magazin), Partikel, animierte Lichter und Musik an der Party erweitert, auf Performance optimiert und als Desktop-Build exportiert.

## Implementierte Funktionen

### Distanzanzeige (World Space Canvas)

- Am Spielerraumschiff `Player Ship` hängt ein Canvas mit Render Mode **World Space**. Er liegt als Kindobjekt am Schiff und bewegt sich daher mit.
- Das Script `DistanceDisplay` liegt auf dem Raumschiff und schreibt in jedem Frame die Distanz zur Plattform `Dirty Dancing Platform` in ein TextMeshPro-Textfeld (`12.3m`).
  - Die Distanz wird mit `Vector3.Distance` zwischen Schiff und Plattform berechnet.
  - Berührt das Schiff die Plattform (`OnCollisionEnter` / `OnCollisionExit`), wird `0.0m` angezeigt.

### Kompass (Mini-Map)

- Der Kompass ist eine kleine runde Anzeige links oben unter dem Magazin. Er liegt auf einem eigenen Canvas `UI/Compass` (Screen Space – Overlay), damit er das Raumschiff nicht verdeckt.
- Das Schiff befindet sich in der Mitte der Scheibe, „oben“ ist die Flugrichtung (Markierung am Rand).
- Das Script `Compass` rechnet die Position der Plattform mit `InverseTransformPoint` in den lokalen Raum des Schiffs um und verwendet davon die X/Z-Ebene.
  - **Pfeil:** Aus `Atan2(x, z)` ergibt sich der Winkel zur Flugrichtung; der Pfeil in der Mitte wird um die Z-Achse gedreht. Oben = Plattform liegt voraus, rechts = rechts, unten = hinter dem Schiff.
  - **Party-Punkt:** Zeigt die Lage der Plattform maßstäblich an (`radarRange = 150` entspricht dem Rand). Ist die Plattform weiter entfernt, bleibt der Punkt am Rand kleben und wandert erst beim Annähern nach innen.

### Magazin (Screen Space – Overlay Canvas)

- Das Objekt `UI/Magazine` besitzt einen Canvas mit Render Mode **Screen Space – Overlay** und eine Horizontal Layout Group (Ausrichtung oben links, Padding `20`, Spacing `10`).
- `MagazineUI` erzeugt beim Start pro Schuss im Magazin ein Icon (Prefab `Bullet`, Sprite `Assets/UI/Bullet.png`) und blendet je nach Munitionsstand Icons ein oder aus.
- `Cannon` verwaltet die Munition (`magazineSize = 10`):
  - Jeder Schuss reduziert die Munition um eins und löst das Event `AmmunitionChanged` aus.
  - Bei Munition `0` wird der Schuss in `OnAttack` verworfen – das Schiff kann nicht mehr schießen.
  - `Bullet` löst in `OnDestroy` das Event `Destroyed` aus (Treffer oder Ablauf der Lebensdauer). `Cannon` gibt daraufhin eine Patrone zurück.
- UI und Logik sind über Events entkoppelt: `MagazineUI` kennt nur `Cannon.AmmunitionChanged`.
- Magazin und Kompass verwenden einen Canvas Scaler im Modus *Scale With Screen Size* (Referenz `960 × 540`, Match Height), damit das HUD auf hochauflösenden Displays nicht winzig wird.

### Atmosphäre (Partikel)

Unter der Plattform liegt das Objekt `Funky Town/Party Particles` mit sechs Particle Systems. Alle simulieren im **World Space**, damit die Partikel zurückbleiben, wenn sich die Plattform dreht und hebt.

- **Confetti:** Box-Emitter über der Tanzfläche, bunte Quadrate in zufälligen Farben (Start Color im Modus *Random Color*), leichte Schwerkraft, Rotation over Lifetime und Noise für das Flattern.
- **Floor Sparkles:** Kreis-Emitter am Boden, leuchtende Punkte steigen auf, Farbe wechselt über die Lebensdauer von Cyan zu Magenta, Größe schrumpft auf null.
- **Fountain 1–4:** Vier Kegel-Emitter am Rand der Tanzfläche. Statt einer konstanten Rate werden **Bursts** verwendet, die im Takt der Musik (155 BPM, alle zwei Schläge) feuern; benachbarte Fontänen sind um einen Schlag versetzt.
- Materialien: `M_PartyGlow` (URP *Particles/Unlit*, additiv, weiche Kreistextur) und `M_PartyConfetti` (Alpha Blending, ohne Textur).
- Zusätzlich hinterlässt das Raumschiff einen Trail, der mit dem Visual Effect Graph erstellt wurde (`Assets/Effects/Trail.vfx`).

### Lichter

Unter `Funky Town/Party Lights` liegen sechs Lichter. Jedes besitzt das Script `PartyLight`, das mehrere Eigenschaften gleichzeitig animiert:

| Eigenschaft | Umsetzung in `PartyLight` |
|---|---|
| Farbe | Der Farbton wandert mit `Color.HSVToRGB` über den Farbkreis (`hueSpeed`, `hueOffset`). |
| Intensität | Pulsiert im Takt (`beatsPerMinute = 155`, Tempo des Tracks): voller Wert auf dem Schlag, danach exponentieller Abfall (`pulseDepth`). |
| Range | Kann auf dem Schlag vergrößert werden (`rangePulse`). |
| Rotation | Schwenkt per Sinus um die lokale X- und Z-Achse (`sweepAngle`, `sweepSpeed`). |
| Position | Kreist um die Startposition (`orbitRadius`, `orbitSpeed`). |

- **Disco Spot 1–3:** Spotlights über der Tanzfläche, jedes beleuchtet ein anderes Drittel. Farbe und Takt sind jeweils um ein Drittel versetzt, die Kegel schwenken über den Boden.
- **Disco Orbiter 1–2:** Point Lights, die knapp über dem Boden in entgegengesetzter Richtung kreisen.
- **Disco Strobe:** Point Light in der Mitte, geht zwischen den Schlägen ganz aus und vergrößert auf dem Schlag seine Range.

### Musik

- Als Musik läuft `Assets/Audio/Glatteis_XetexEdit.wav` (Nina Chuba – „Glatteis“, XETEX Edit, ca. 155 BPM). Der Clip wird als Vorbis komprimiert und gestreamt (Load Type *Streaming*), damit der lange Track nicht komplett im Speicher liegt.
- Die `AudioSource` liegt auf dem Objekt `Funky Town` an der Plattform und bewegt sich mit ihr.
  - **Spatial Blend `1` (3D):** Die Lautstärke hängt von der Distanz zum `AudioListener` an der Kamera ab.
  - **Volume Rolloff `Linear`**, Min Distance knapp außerhalb der Tanzfläche, Max Distance `110`: Auf der Plattform ist die Musik voll hörbar, wird beim Wegfliegen gleichmäßig leiser und ist ab der Max Distance stumm.
  - **Doppler Level `0`:** Kein Tonhöhenwechsel beim schnellen Vorbeifliegen.
  - Loop und Play On Awake sind aktiv.

### Performance

- **Static Batching:** Alle unbewegten Mesh Renderer der Umgebung sind als *Batching Static* markiert. Ausgenommen sind Objekte, die sich zur Laufzeit bewegen (Hierarchien mit Animator, Rigidbody, eigenen Scripts, Particle Systems sowie der Sky Dome).
- **Occlusion Culling:** Dieselben Objekte sind als *Occludee Static* markiert, opake Objekte zusätzlich als *Occluder Static*. Die Occlusion-Daten wurden gebacken (`Assets/Scenes/AssignmentScene/OcclusionCullingData.asset`).
- Kontrolle im Editor:
  - *Window → Rendering → Occlusion Culling → Visualization*: Verdeckte Objekte verschwinden im Scene View, wenn sich die Kamera hinter Gebäuden befindet.
  - *Statistics-Fenster* im Game View: Draw Calls, Set Pass Calls und FPS mit und ohne Static Batching vergleichen (Schalter unter *Project Settings → Player → Other Settings → Static Batching*).
- **Gibt es einen Unterschied?** Ja, deutlich. Gemessen im Play-Modus im Statistics-Fenster:

  | Wert | ohne Static Batching | mit Static Batching |
  |---|---|---|
  | Draw Calls | 3244 | 765 |
  | Set Pass Calls | 82 | 75 |
  | Triangles | 1986,0k | 1504,8k |
  | Vertices | 3615,0k | 1747,6k |
  | Global Frametime | 7,1 ms | 5,1 ms |
  | FPS | 140,2 | 196,5 |

  - Die Zahl der Draw Calls sinkt um rund 76 %, weil die statischen Meshes mit gleichem Material zu gemeinsamen Batches zusammengefasst werden.
  - Die Set Pass Calls ändern sich kaum: Static Batching spart Draw Calls, nicht Materialwechsel.
  - Die Framerate steigt um rund 40 % (Frametime von 7,1 ms auf 5,1 ms).
  - Für Occlusion Culling wurden keine eigenen Vergleichswerte gemessen; die Wirkung ist in der Visualization sichtbar (verdeckte Objekte werden nicht gerendert).

### Build

- Player Settings: Product Name `Funky Town`, Company Name `Windisch Vogl`, eigenes App-Icon (`Assets/UI/AppIcon.png`).
- Splash Screen: Das Unity-Logo ist deaktiviert (*Show Unity Logo* aus), stattdessen wird das eigene Logo `Assets/UI/SplashLogo.png` auf dunklem Hintergrund gezeigt.
- Es gibt zwei Desktop-Builds: macOS (`Builds/FunkyTown-macOS/Funky Town.app`) und Windows (`Builds/FunkyTown-Windows/Funky Town.exe`). Der macOS-Build wurde außerhalb des Editors gestartet.

### Editor-Werkzeug

Das Editor-Script `Assets/Editor/FunkyTownSetup.cs` stellt die Schritte im Menü **Tools → Funky Town** bereit, damit sie reproduzierbar sind:

1. *Setup Party* – erzeugt Lichter, Partikel, Musik und den Kompass (ersetzt eine frühere Version). *1b Rebuild Compass + UI scaling* baut nur den Kompass neu und setzt die UI-Skalierung, *1c Apply Music only* weist nur Clip und Tempo neu zu.
2. *Mark Static + Bake Occlusion Culling* – setzt die Static-Flags und backt die Occlusion-Daten.
3. *Apply Player Settings* – Name, Icon und Splash Screen.
4. *Build macOS Player* – erstellt den macOS-Build.
5. *Build Windows Player* – erstellt den Windows-Build.

## Wichtige Scripts

| Script | Aufgabe |
|---|---|
| `DistanceDisplay` | Distanz Schiff ↔ Plattform im World Space Canvas |
| `Compass` | Mini-Map-Kompass: Pfeil und Party-Punkt relativ zur Flugrichtung |
| `MagazineUI` | Zeigt die Munition als Icons im Overlay Canvas |
| `Cannon` / `Bullet` | Munitionslogik, Events `AmmunitionChanged` und `Destroyed` |
| `PartyLight` | Animiert Farbe, Intensität, Range, Rotation und Position eines Lichts |
| `FunkyTownSetup` (Editor) | Szenenaufbau, Static-Flags, Occlusion-Bake, Player Settings, Build |

## Arbeitsteilung

### Andrea Windisch

- World Space Canvas am Raumschiff und Script `DistanceDisplay`.
- Magazin-UI: Overlay Canvas, Layout, Bullet-Icon, Script `MagazineUI`.
- Munitionslogik in `Cannon` und `Bullet` (Events, Schusssperre bei leerem Magazin, Rückgabe bei Zerstörung).
- Trail des Raumschiffs mit dem Visual Effect Graph.
- Visual Effect Graph `Assets/Effects/Test.vfx` als weiterer Effekt-Prototyp.
- Einbindung der Distanzanzeige, des Trails und der Magazin-Anzeige in `AssignmentScene`.

Diese Teile sind in der Git-Historie in den Commits `0b3eb69` (Distanzanzeige, Trail und VFX) und `3e28eec` (Magazin-UI und Munitionslogik) nachvollziehbar. Die älteren Dirty-Dancing-Beiträge (Plattformbewegung, Materialwechsel und Tänzer mit zufälligem Animationsstart) sind in `DOCUMENTATION_DIRTY_DANCING.md` beschrieben und bilden die Basis der Funky-Town-Szene.

### Markus Vogl

- Kompass als Mini-Map (`Compass`, eigener Overlay Canvas).
- Partikel an der Party (Confetti, Sparkles, Fontänen) und Partikel-Materialien.
- Party-Lichter und Script `PartyLight`.
- Musik und 3D-`AudioSource`.
- Static Batching, Occlusion Culling, Player Settings und Build (`FunkyTownSetup`).
- Zusammenführung der Dokumentation.

## Testhinweise für das Video

1. Losfliegen und das Schiff drehen: Pfeil und Party-Punkt im Kompass links oben zeigen immer zur Plattform; beim Annähern wandert der Punkt vom Rand zur Mitte.
2. Zur Plattform und wieder weg fliegen: Die Distanzanzeige zählt herunter bzw. hoch.
3. Schießen: Pro Schuss verschwindet ein Icon oben links; bei leerem Magazin fällt kein Schuss mehr; sobald Projektile zerstört werden, kommen Icons zurück.
4. An der Plattform: Konfetti, Sparkles und Fontänen zeigen.
5. Lichter zeigen: Farbwechsel, Pulsieren im Takt, schwenkende Spots, kreisende Point Lights.
6. Von der Plattform wegfliegen und zurückkehren: Die Musik wird leiser und wieder lauter.
7. Im Editor: Occlusion-Culling-Visualization und Statistics-Fenster zeigen.
8. Den Build (`Funky Town.app`) starten: eigener Splash Screen, Name und Icon.
9. Code und Inspector zeigen: `DistanceDisplay`, `Compass`, `MagazineUI`, `Cannon`, `PartyLight`, die AudioSource-Einstellungen und ein Particle System.

## Code-Review und Video-Leitfaden

Für die konkrete Reihenfolge, Fundstellen in Unity, Erklärung der wichtigsten Methoden, Git-Vergleich, Build-Prüfung und bekannte Einschränkungen siehe `REVIEW_VIDEO_GUIDE.md`.
