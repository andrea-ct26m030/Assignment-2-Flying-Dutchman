# Flying Dutchman - Dokumentation

## Implementierte Funktionen

### Kamera und Steuerung

- Die Kamera ist als Kindobjekt am Spielerraumschiff angebracht und erzeugt eine Third-Person-Ansicht.
- Beim Start wird der Mauszeiger gesperrt und ausgeblendet.
- Die Steuerung verwendet Unitys neues Input System:
  - `W` und `S`: Beschleunigen vorwaerts beziehungsweise rueckwaerts entlang der aktuellen Flugrichtung.
  - Maus: Pitch und Yaw des Raumschiffs.
  - `A` und `D`: Rollen nach links beziehungsweise rechts.
  - Linke Maustaste: Projektil abfeuern.
- Die Bewegung und Drehung erfolgen mit einem `Rigidbody` und Kraeften beziehungsweise Drehmomenten. Dadurch bleibt die Steuerung physikbasiert und relativ zur aktuellen Ausrichtung des Schiffs.

### Szene und Kollisionen

- Der Spieler besitzt einen nicht-kinematischen `Rigidbody`, einen `BoxCollider` und deaktivierte Schwerkraft.
- Ein Spawner erzeugt zufaellig platzierte und bewegte Raumschiffe. Ein zweiter Spawner erzeugt Asteroiden mit zufaelliger Groesse, Geschwindigkeit und Flugrichtung.
- Vor jedem Spawn prueft der `ObstacleSpawner` per `Physics.CheckSphere`, ob die zufaellige Position frei ist. Bis zu zehn Positionen werden probiert, damit Objekte nicht in der Umgebung oder ineinander erscheinen.
- Gespawnte Objekte sind nicht-kinematische Rigidbodies ohne Schwerkraft. Ihre Masse ergibt sich aus `Base Mass` mal Skalierungsfaktor hoch drei. Groessere Schiffe sind damit schwerer und stossen den Spieler bei Kollisionen staerker ab, kleine Schiffe lassen sich wegschieben.
- Objekte, die den festgelegten Bereich verlassen, werden entfernt und ersetzt. So bleibt die Anzahl der Hindernisse konstant.
- Der `SpaceshipController` reagiert auf Kollisionen und schreibt die Namen der getroffenen Objekte in die Unity Console.

### Bewegung und Hindernisvermeidung der gespawnten Objekte

- `FloatingSpaceship` bewegt die Objekte ueber die Rigidbody-Geschwindigkeit mit konstantem Tempo. Die Rotation ist eingefroren, das Script richtet das Objekt selbst in Flugrichtung aus.
- Per `SphereCast` in Flugrichtung wird geprueft, ob ein Hindernis vor dem Objekt liegt. Die Reichweite ergibt sich aus Geschwindigkeit mal `Look Ahead Time` plus der eigenen Collider-Groesse.
- Liegt ein Hindernis voraus, wird die Flugrichtung an der getroffenen Oberflaeche gespiegelt. Ist auch die gespiegelte Richtung blockiert, werden bis zu acht zufaellige Richtungen probiert, die vom Hindernis wegzeigen.
- Nach jedem Richtungswechsel haelt das Objekt den neuen Kurs fuer `Turn Cooldown` Sekunden. Das verhindert, dass es zwischen zwei Hindernissen hin und her springt.
- Projektile und das Spielerraumschiff werden nicht als Hindernis gewertet. Dadurch bleiben Kollisionen mit dem Spieler moeglich und die Schiffe weichen keinen Schuessen aus.
- Nach einer Kollision mit dem Spieler, einem anderen Schiff oder der Umgebung behaelt das Schiff den physikalischen Impuls, uebernimmt die neue Richtung und beschleunigt mit `Recovery Acceleration` zurueck auf seine Reisegeschwindigkeit. Projektiltreffer werden ignoriert, damit ein Treffer den Kurs nicht veraendert.
- Eine Stuck-Erkennung prueft alle drei Sekunden die zurueckgelegte Strecke. Hat sich ein Objekt kaum bewegt, zerstoert es sich selbst, und der Spawner ersetzt es an einer freien Position.

### Projektile und Schiesslogik

- Das Projektil ist ein eigenes Prefab mit sichtbarer Kugel, `SphereCollider`, `Rigidbody` und dem `Bullet`-Script. Die Masse betraegt `0,01`, damit ein Treffer die getroffenen Schiffe kaum anstoesst.
- Das `Cannon`-Script auf dem Spielerraumschiff instanziiert das Projektil am Objekt `Muzzle`.
- Die Schussgeschwindigkeit betraegt `60`; die aktuelle Geschwindigkeit des Spielerraumschiffs wird uebernommen, damit das Projektil nicht hinter dem Schiff zurueckbleibt.
- Ein Cooldown von `0,2` Sekunden begrenzt die Feuerrate.
- Beim Start eines Projektils werden Kollisionen mit den Collider-Komponenten des Spielerraumschiffs ignoriert. Das verhindert, dass das Projektil sofort das eigene Schiff trifft.

### Projektil-Lebensdauer und Logging

- Das `Bullet`-Script startet beim Aktivieren eine Coroutine. Sie zerstoert das Projektil nach der im Inspector einstellbaren Lebensdauer von drei Sekunden, falls es zuvor nichts trifft.
- Bei einer Kollision loest `Bullet` das statische Event `Hit` aus und zerstoert sich anschliessend.
- Das `MessageLogger`-Script ist einmal in der Szene vorhanden, registriert sich beim `Bullet.Hit`-Event und schreibt den Namen des getroffenen Objekts sowie den Treffpunkt in die Unity Console.

## Wichtige Scripts

| Script | Aufgabe |
| --- | --- |
| `SpaceshipController` | Liest die Flug-, Roll- und Maus-Eingaben und steuert den Rigidbody des Spielerraumschiffs. |
| `ObstacleSpawner` | Erzeugt die bewegten Raumschiffe und Asteroiden an freien Positionen, setzt Groesse und Masse und haelt die Anzahl konstant. |
| `FloatingSpaceship` | Bewegt gespawnte Objekte per Rigidbody, laesst sie vor Hindernissen umdrehen und entfernt festhaengende Objekte. |
| `Cannon` | Reagiert auf die Attack-Action und erzeugt Projektile am Muzzle. |
| `Bullet` | Setzt die Projektilgeschwindigkeit, behandelt Treffer und die Lebensdauer. |
| `MessageLogger` | Empfaengt das Treffer-Event und protokolliert Projektiltreffer in der Console. |

## Arbeitsteilung

### Andrea Windisch

- Aufbau der Szene sowie Einbindung der Raumschiff- und Asteroiden-Assets.
- Third-Person-Kamera, Cursor-Sperre und `SpaceshipController`.
- Einrichtung der Eingaben fuer Vorwaerts-/Rueckwaertsflug, Maussteuerung und Rollen.
- Physik- und Kollisionssetup des Spielerraumschiffs.
- Implementierung der Raumschiff- und Asteroiden-Spawner sowie der ersten Bewegungslogik fuer gespawnte Objekte.

### Markus Vogl

- Erstellung des Projektil-Prefabs und des zugehoerigen Materials.
- Implementierung von `Cannon` inklusive Attack-Input, Muzzle-Spawnpunkt, Schussgeschwindigkeit und Cooldown.
- Implementierung von `Bullet` mit Kollisionsbehandlung, Coroutine fuer die Lebensdauer und dem `Hit`-Event.
- Implementierung und Einbindung von `MessageLogger` fuer die Ereignis-basierte Ausgabe von Projektiltreffern in die Unity Console.
- Umstellung der gespawnten Objekte auf dynamische Rigidbodies mit groessenabhaengiger Masse und freie Spawn-Positionen im `ObstacleSpawner`.
- Hindernisvermeidung per `SphereCast`, Turn Cooldown und Stuck-Erkennung in `FloatingSpaceship`, damit Schiffe nicht mehr durch die Umgebung fliegen oder festhaengen.

## Testhinweise fuer das Video

1. Mit `W`, `S`, Maus sowie `A` und `D` durch die Szene fliegen.
2. Mit einem kleinen und einem grossen gespawnten Raumschiff kollidieren und den Unterschied in der physikalischen Reaktion zeigen.
3. Das `Projectile`-Prefab sowie die Inspector-Einstellungen von `Cannon` und `Bullet` zeigen.
4. Mit der linken Maustaste auf ein anderes Schiff schiessen und den Console-Log des `MessageLogger` zeigen.
5. Einen Schuss zeigen, der trifft, sowie einen Schuss, der nach drei Sekunden automatisch entfernt wird.
