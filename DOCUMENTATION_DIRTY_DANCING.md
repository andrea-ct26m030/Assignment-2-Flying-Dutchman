# Dirty Dancing – Dokumentation

Dieses Projekt baut auf der Abgabe *Flying Dutchman* auf (siehe `DOCUMENTATION.md`). Raumschiffsteuerung, Kamera, Spawner und Projektile stammen aus dieser Abgabe und werden hier nicht erneut beschrieben. Die Szene `AssignmentScene` wurde um eine schwebende Tanzplattform, Mixamo-Charaktere und animierte Solarpanels am Spielerraumschiff erweitert.

## Implementierte Funktionen

### Plattform-Animation

- Das Plattform-Objekt `SM_Veh_Barge_01` wurde mit einem Animator Controller ausgestattet.
- Der Animator Controller verwendet zwei Animation Layers, um die Bewegung und Rotation unabhängig voneinander zu steuern.
  - **Base Layer:** Animiert die vertikale Position der Plattform, sodass sie sich kontinuierlich auf und ab bewegt.
  - **Rotation Layer:** Animiert die Rotation der Plattform um die Y-Achse.
- Der Rotation Layer verwendet ein Weight von `1`, damit die Rotation gleichzeitig mit der vertikalen Bewegung abgespielt wird.
- Beide Animationen sind als Loop eingerichtet, sodass sich die Plattform dauerhaft bewegt und dreht.

### Animation Events und Materialwechsel

- In der Animation für die vertikale Bewegung wurden zwei Animation Events eingefügt, die beim Erreichen des höchsten und niedrigsten Punktes ausgelöst werden.
- Die Events rufen die Methoden `OnReachedTop()` und `OnReachedBottom()` auf.
- Da die Animationen auf übergeordneten GameObjects liegen, werden die Events über ein separates Script an den `PlatformController` weitergeleitet.
- Der `PlatformController` greift auf den `MeshRenderer` der Plattform zu und ändert dessen Material.
  - **Höchster Punkt:** Wechsel zum alternativen Material `PolygonScifiWorlds_04_C`.
  - **Niedrigster Punkt:** Wechsel zum Standardmaterial `PolygonScifiWorlds_01_A`.
- Dadurch verändert sich die Beleuchtungsfarbe der Plattform abhängig von ihrer vertikalen Position.

### Charaktere und Tanzanimationen

- Sechs verschiedene Charaktermodelle mit unterschiedlichen Tanzanimationen wurden von Mixamo importiert und auf der Plattform platziert.
- Die Charaktere wurden als Prefabs angelegt, damit sie mehrfach verwendet und zentral bearbeitet werden können.
- Alle Charaktere verwenden denselben Animator Controller `CharacterAnimationController` mit gemeinsamen Animationszuständen.
- Für die unterschiedlichen Tanzanimationen werden **Animator Override Controller** verwendet.
  - Jeder Charakter erhält einen eigenen Override Controller.
  - Der `Dancing`-State wird durch die jeweilige Mixamo-Tanzanimation ersetzt.
  - Die übrigen States und Transitions bleiben für alle Charaktere identisch.
- Die Tanzanimationen sind als Loop konfiguriert, damit die Charaktere dauerhaft tanzen können.
- Die Root Motion wurde angepasst, damit die Charaktere während des Tanzens nicht ungewollt ihre Position auf der Plattform verlassen.
- Das Script `RandomDanceOffset` startet die Tanzanimation jedes Charakters an einer zufälligen Stelle, damit nicht alle Gäste synchron tanzen.

### Interaktion mit dem Raumschiff

- Der gemeinsame Animator Controller `CharacterAnimationController` besitzt zusätzlich zum `Dancing`-State einen `Waving`-State mit der Mixamo-Animation `Waving` (Loop).
- Über den Bool-Parameter `IsWaving` wird zwischen den beiden States gewechselt.
  - **Dancing → Waving:** Transition mit der Bedingung `IsWaving == true`.
  - **Waving → Dancing:** Transition mit der Bedingung `IsWaving == false`.
  - Beide Transitions haben keine Exit Time und eine Blend-Dauer von `0,25` Sekunden, damit die Gäste sofort und weich reagieren.
- Da der Parameter und die Transitions im Basis-Controller liegen, übernehmen alle Override Controller das Verhalten automatisch. Nur die Tanzanimation unterscheidet sich pro Charakter.
- Das Script `PartyGuestReaction` liegt auf dem Objekt `Dirty Dancing Platform` und misst in jedem Frame die Distanz zwischen dem Spielerraumschiff und der Plattform.
  - Das Raumschiff wird beim Start automatisch über den `SpaceshipController` gefunden, kann aber auch im Inspector zugewiesen werden.
  - **Wave Distance (`40`):** Unterschreitet die Distanz diesen Wert, wird `IsWaving` bei allen Gästen auf `true` gesetzt.
  - **Dance Distance (`50`):** Überschreitet die Distanz diesen Wert, wird `IsWaving` wieder auf `false` gesetzt.
  - Die zwei unterschiedlichen Schwellen bilden eine Hysterese, damit die Gäste an der Grenze nicht zwischen Tanzen und Winken flackern.
- Das Script sammelt alle `Animator`-Komponenten unterhalb der Plattform und setzt den Parameter nur bei Animatoren, die ihn tatsächlich besitzen. Der Animator der Plattform selbst wird dadurch übersprungen.
- Bei ausgewählter Plattform werden die beiden Radien als Gizmo-Kugeln im Scene View angezeigt.

### Solarpanels (More Energy!)

- Das Objekt `SM_Prop_SolarPanel_03` wurde zweimal dupliziert und als `SolarPanel_Left` und `SolarPanel_Right` als Kindobjekte an das Spielerraumschiff `Player Ship` gehängt.
- Die Mesh Collider der Panels wurden entfernt, damit sie nicht Teil des Rigidbody-Colliders des Raumschiffs werden.
- Der Animation Clip `SolarPanel` (Länge `4` Sekunden, Loop) animiert das Oberteil `SM_Prop_SolarPanel_03_Panel_01` um sein Scharnier.
  - Rotation X: `-30°` → `30°` → `-30°`, wodurch das Panel kontinuierlich hin und her kippt.
- Für die leuchtenden Kanten wurde das Material `PolygonScifiWorlds_01_A` als `M_SolarPanel_Emissive` kopiert und beiden Teilen des Panels zugewiesen.
- Im selben Clip wird die Eigenschaft `Material._Emission_Color` des Oberteils animiert.
  - Frame 0: Emission `(0, 0, 0)`, keine Emission.
  - Frame 120: Emission `(2.44, 3.87, 4)`, das entspricht der Originalfarbe mit HDR-Intensität `2`.
  - Frame 240: Emission `(0, 0, 0)`.
  - Die Kanten faden dadurch kontinuierlich von aus über volle Intensität zurück auf aus.
- Beide Panels verwenden denselben Animator Controller `SolarPanel` und laufen synchron.

## Wichtige Scripts und Komponenten

| Script / Komponente | Aufgabe |
|---|---|
| `PlatformController` | Reagiert auf die Animation Events und wechselt das Material der Plattform über den MeshRenderer. |
| `CharacterAnimationController` | Gemeinsamer Animator Controller der Gäste mit den States `Dancing` und `Waving` und dem Parameter `IsWaving`. |
| `RandomDanceOffset` | Startet die Tanzanimation jedes Gastes an einem zufälligen Zeitpunkt. |
| `PartyGuestReaction` | Misst die Distanz zum Spielerraumschiff und schaltet die Gäste zwischen Tanzen und Winken um. |
| `SolarPanel` (Clip + Controller) | Animiert die Scharnier-Rotation und die Emission der Solarpanels am Raumschiff. |

## Arbeitsteilung

### Andrea Windisch

- Einrichtung des Animator Controllers für die Plattform.
- Implementierung der vertikalen Bewegung und kontinuierlichen Rotation über zwei Animation Layers.
- Konfiguration der Animation Loops und des Layer Weights.
- Erstellung der Animation Events für den höchsten und niedrigsten Punkt der Plattformbewegung.
- Implementierung des `PlatformController` für den ereignisbasierten Materialwechsel.
- Weiterleitung der Animation Events von den übergeordneten GameObjects an den `PlatformController`.
- Import und Einrichtung von sechs Mixamo-Charaktermodellen mit unterschiedlichen Tanzanimationen.
- Erstellung der Charakter-Prefabs und Einrichtung eines gemeinsamen Animator Controllers.
- Konfiguration der Animator Override Controller für die individuellen Tanzanimationen.
- Anpassung der Animationseinstellungen für kontinuierliche Tanzbewegungen ohne unerwünschte Positionsänderung.

### Markus Vogl

- Import der Mixamo-Animation `Waving` und Einrichtung des `Waving`-States im gemeinsamen Animator Controller.
- Anlegen des Bool-Parameters `IsWaving` sowie der Transitions zwischen `Dancing` und `Waving`.
- Implementierung von `PartyGuestReaction` mit automatischer Suche des Spielerraumschiffs, Distanzprüfung mit Hysterese und Gizmo-Anzeige.
- Duplizieren und Anbringen der zwei Solarpanels am Spielerraumschiff inklusive Entfernen der Mesh Collider.
- Erstellung des Animation Clips `SolarPanel` für die Scharnier-Rotation des Oberteils.
- Kopie des Materials als `M_SolarPanel_Emissive` und Animation der Emission Color zwischen Intensität `0` und `2` im Animation Window.
- Zusammenführung der Dokumentation.

## Testhinweise für das Video

1. Plattform im Play-Modus zeigen: Sie dreht sich um die Y-Achse und bewegt sich auf und ab.
2. Am höchsten und niedrigsten Punkt wechselt das Material. In der Console erscheinen `Reached Top!` und `Reached Bottom!`.
3. Die sechs Gäste tanzen mit unterschiedlichen Animationen und zeitlichem Versatz auf der Plattform.
4. Mit dem Raumschiff zur Plattform fliegen: Die Gäste hören auf zu tanzen und winken. In der Console erscheint `Spaceship is close: guests start waving`.
5. Wieder wegfliegen: Die Gäste tanzen weiter. In der Console erscheint `Spaceship left: guests go back to dancing`.
6. Die zwei Solarpanels am Raumschiff zeigen: Die Oberteile kippen hin und her, die Kanten pulsieren.
7. Im Editor zeigen: Animator der Plattform mit beiden Layers, Animation Events im Clip `PlatformMoving`, `CharacterAnimationController` mit Parameter und Transitions, Inspector von `PartyGuestReaction`, Animation Window des Clips `SolarPanel`.
