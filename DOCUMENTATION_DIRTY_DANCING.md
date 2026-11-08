# Dirty Dancing – Dokumentation

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
- Alle Charaktere verwenden denselben Animator Controller mit gemeinsamen Animationszuständen.
- Für die unterschiedlichen Tanzanimationen werden **Animator Override Controller** verwendet.
  - Jeder Charakter erhält einen eigenen Override Controller.
  - Der `Dancing`-State wird durch die jeweilige Mixamo-Tanzanimation ersetzt.
  - Die übrigen States und Transitions bleiben für alle Charaktere identisch.
- Die Tanzanimationen sind als Loop konfiguriert, damit die Charaktere dauerhaft tanzen können.
- Die Root Motion wurde angepasst, damit die Charaktere während des Tanzens nicht ungewollt ihre Position auf der Plattform verlassen.

## Wichtige Scripts und Komponenten

| Script / Komponente | Aufgabe |
|---|---|
| `CharacterAnimationController` | Steuert die Animationszustände sowie die Rotation und vertikale Bewegung der Plattform. |
| `PlatformController` | Reagiert auf die Animation Events und wechselt das Material der Plattform über den MeshRenderer. |

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
