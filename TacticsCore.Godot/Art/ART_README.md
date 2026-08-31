# Grafiken für die Godot-Oberfläche

Dieser Ordner enthält alle Bilddateien, mit denen die bisherige Form-/
Farb-Darstellung (Kreise, farbige Sechsecke) Stück für Stück durch eigene
Grafiken ersetzt werden kann. **Nichts hier ist Pflicht.** Fehlt eine Datei,
zeichnet die Anwendung für genau dieses Feld bzw. diesen Helden automatisch
weiter die bisherige Form/Farbe - kein Absturz, kein leeres Feld. Grafiken
können deshalb jederzeit einzeln nachgereicht werden.

Zuständig für das Laden ist `Scripts/Shared/VisualAssetCatalog.cs` - dort
stehen auch die unten genannten Pfade als Vorgabewerte im Code. Diese Datei
muss für den Standardfall (Dateien unter den vorgegebenen Pfaden ablegen)
NICHT angefasst werden.

## Ordnerstruktur

```
Art/
  Fields/
    blossom_a.png        Feldtyp "Blüte A" (Heimatzone Team A)
    blossom_b.png        Feldtyp "Blüte B" (Heimatzone Team B)
    connector.png         Verbindungsfeld zwischen den beiden Blüten
    start_overlay.png     Zusätzliche Markierung über einem Startfeld (optional)
    locked_overlay.png    Zusätzliche Markierung über einem gesperrten Feld (optional)
  Heroes/
    schildtraeger.png
    elfe.png
    berserker.png
    wand.png
    illusionist.png
    musiker.png
    hugger.png
    elementarmagier.png
    bogenschuetze.png
  VisualAssetCatalog.tres   (optional, siehe "Abweichende Pfade" unten)
```

Die Heldendateinamen entsprechen exakt der `Id` aus `HeroDefinition`
(siehe `TacticsCore/Heroes/DefaultHeroes.cs`) - nicht dem angezeigten Namen.
Die neun Standardhelden und ihre Kennung:

| Datei                     | Held (DisplayName)     |
|---------------------------|-------------------------|
| schildtraeger.png         | Schildträger            |
| elfe.png                  | Elfe                    |
| berserker.png             | Berserker                |
| wand.png                  | Wand                     |
| illusionist.png           | Illusionist              |
| musiker.png                | Musiker                  |
| hugger.png                 | Hugger                   |
| elementarmagier.png       | Elementarmagier          |
| bogenschuetze.png         | Bogenschütze             |

## Maße und Ausrichtung

### Feldtexturen (`Fields/*.png`)

Die Hexfelder sind **flat-top** ausgerichtet (zwei Ecken zeigen nach links/
rechts, oben und unten verläuft je eine gerade Kante) - siehe
`Scripts/Shared/BoardGeometry.cs`. Ein Feldbild muss exakt die Bounding-Box
eines solchen Sechsecks abdecken, das Seitenverhältnis ist dadurch fest
vorgegeben:

- Breite : Höhe = 2 : √3 ≈ 1,1547 : 1
- Empfohlene Größe: **512 × 444 Pixel** (oder jedes andere Vielfache mit
  demselben Seitenverhältnis, z.B. 256 × 222)
- Das Bild wird auf diese Bounding-Box gestreckt, unabhängig von der
  aktuellen Zoomstufe - die vier Ecken des Bildes liegen dabei außerhalb des
  eigentlichen Sechsecks. Wer keine rechteckige Kante sehen möchte, lässt
  diese Eckbereiche transparent (Alphakanal) und zeichnet nur die
  Sechseckform ein.
- `start_overlay.png`/`locked_overlay.png` werden in derselben Bounding-Box
  ÜBER der Basisgrafik gezeichnet - hier bietet sich ein Bild mit
  überwiegend transparentem Hintergrund an (z.B. nur ein Rahmen oder Symbol).

### Heldentexturen (`Heroes/*.png`)

- Quadratisches Bild, empfohlen **256 × 256 Pixel**
- Wird als Portrait mittig auf dem Feld gezeichnet und skaliert mit der
  aktuellen Zoomstufe der Einheit mit
- Die Teamzugehörigkeit wird unabhängig von der Grafik über einen
  farbigen Rand dargestellt (blau = Team A, orange = Team B) - das Portrait
  selbst muss also nicht teamspezifisch eingefärbt sein, derselbe Held kann
  in beiden Teams dieselbe Datei verwenden
- Klone werden automatisch halbtransparent gezeichnet - auch hierfür ist
  keine eigene Klon-Grafik nötig

## Dateiformat

PNG mit Alphakanal wird empfohlen (Transparenz für die Eckbereiche bzw.
Overlays). Godot importiert daneben auch JPG, WebP und weitere gängige
Formate automatisch - für alles außer PNG geht dabei allerdings die
Transparenz verloren.

## Eigene Grafiken einbinden

1. Datei unter dem passenden Pfad und Namen aus der Tabelle oben ablegen
   (z.B. `Art/Heroes/berserker.png`).
2. Godot importiert die Datei beim nächsten Start/Fokuswechsel automatisch.
3. Fertig - kein Code, keine weitere Konfiguration nötig.

### Abweichende Pfade oder Dateinamen

Wer nicht diese Namenskonvention nutzen möchte, kann im Godot-Editor über
"Neue Ressource..." eine Ressource vom Typ `VisualAssetCatalog` anlegen, die
gewünschten Pfade in den Feldern im Inspector eintragen und die Ressource
als `Art/VisualAssetCatalog.tres` speichern. Diese Datei wird automatisch
geladen, wenn sie existiert, und übersteuert dann die Vorgabewerte aus dem
Code - ganz ohne C#-Änderung.

## Eine Grafik für einen NEU hinzugefügten Helden ergänzen

Wird später ein zehnter Held definiert (neuer Eintrag in `HeroCatalog`, z.B.
in `DefaultHeroes.cs`, mit einer neuen `HeroDefinition.Id`), reicht:

1. Bilddatei unter `Art/Heroes/<neue-id>.png` ablegen (z.B. bei
   `Id: "schamane"` also `Art/Heroes/schamane.png`).
2. In `Scripts/Shared/VisualAssetCatalog.cs` im Dictionary
   `HeroTexturePaths` eine Zeile ergänzen:
   ```csharp
   ["schamane"] = "res://Art/Heroes/schamane.png",
   ```

Das ist die EINZIGE Codeänderung, die für eine neue Heldengrafik nötig ist -
und auch die lässt sich vermeiden: legt man stattdessen (oder zusätzlich)
eine `Art/VisualAssetCatalog.tres` gemäß dem Abschnitt "Abweichende Pfade"
an, kann auch der neue Eintrag rein im Editor über das Dictionary-Feld
`Hero Texture Paths` im Inspector ergänzt werden, ganz ohne Code.

Ohne diesen Eintrag (in Code oder .tres) wird der neue Held einfach weiter
als teamfarbener Kreis mit Namen/HP-Balken dargestellt - die Anwendung
funktioniert auch ohne diesen Schritt vollständig, nur eben ohne eigene
Grafik für den neuen Helden.

---

# Animationsgrafiken

Zusätzlich zu den Feld-/Heldengrafiken oben verwendet die Animationsschicht
(`Scripts/Animation/EventAnimationPlayer.cs`) eigene, kleine Geschossgrafiken
für Fernkampf-Angriffe und Fähigkeiten mit Ziel. Genau dasselbe Prinzip wie
oben: **nichts hier ist Pflicht.** Fehlt eine Grafik, zeichnet die Animation
stattdessen eine einfache eingefärbte Kreisform (Farbe je nach Wirkungsart:
Schaden rot, Heilung grün, Buff blau, Debuff violett, Schild gelb) - die
Animation selbst fällt nie aus, nur die Grafik bleibt schlicht.

Zuständig ist `Scripts/Animation/AnimationAssetCatalog.cs`.

## Ordnerstruktur

```
Art/
  Projectiles/
    arrow.png        Fernkampf-Grundangriff (Bogenschütze, Elementarmagier auf Distanz) - zugleich allgemeine Rückfallgrafik
    fireball.png      Elementarschlag (Elementarmagier)
    heal_orb.png       Heilung (Elfe)
    grab_orb.png        Umklammerung (Hugger)
    note.png             Lied (Musiker)
  AnimationAssetCatalog.tres   (optional, siehe "Abweichende Pfade" bei den Feld-/Heldengrafiken - gilt hier analog)
```

Bewusst NUR fünf Dateien für neun Helden: Fähigkeiten ohne eigene Zeile
(Schildwall, Mauer, Trugbild, Aufladen) sind reine Selbst-/Feld-Wirkungen
ohne Geschossflug und brauchen daher keine eigene Grafik - `arrow.png` deckt
zusätzlich jeden Fernkampf-Grundangriff sowie jede Fähigkeit ohne eigene
Geschosszeile ab (Rückfallebene).

## Maße

Geschosse werden quadratisch mit Alphakanal-Transparenz erwartet, empfohlen
**128 × 128 Pixel** - werden während des Flugs auf ca. die halbe Hexfeldgröße
skaliert und sind daher bewusst kleiner als Feld-/Heldengrafiken. Eine feste
Ausrichtung (z.B. "Pfeil zeigt nach rechts") ist NICHT nötig - das Geschoss
wird nicht in Flugrichtung gedreht, ein rundes/symmetrisches Motiv (Kugel,
Funke, Note, Kristall, ...) funktioniert am unkompliziertesten.

## Eine Animation für eine neue Fähigkeit ergänzen

Für die MEISTEN neuen Fähigkeiten ist gar keine Codeänderung nötig: die
Landewirkung (Schaden/Heilung/Buff/Debuff) leitet `EventAnimationPlayer`
bereits generisch aus der HP-/Effekt-Änderung des Ziels ab (siehe
`PlayImpactLanding`) - eine neue Fähigkeit mit `InstantDamage`,
`InstantHeal` oder `StatModifiers` wird also automatisch richtig animiert.

1. Für eine EIGENE Geschossgrafik (statt der allgemeinen Pfeil-Rückfallgrafik):
   Bilddatei unter `Art/Projectiles/<name>.png` ablegen und in
   `AnimationAssetCatalog.cs` im Dictionary `AbilityProjectileTexturePaths`
   eine Zeile mit der `AbilityDefinition.Id` der neuen Fähigkeit ergänzen:
   ```csharp
   ["meineneuefaehigkeit"] = "res://Art/Projectiles/meine_grafik.png",
   ```
   (Oder ganz ohne Code über eine `Art/AnimationAssetCatalog.tres`, siehe
   "Abweichende Pfade" weiter oben - gilt hier identisch.)
2. Für eine WIRKLICH neue Animations-ART (nicht nur eine neue Grafik für
   eine bestehende Art), z.B. ein neuer Bewegungsstil: eine neue Methode in
   `EventAnimationPlayer.cs` ergänzen und im `switch` in `PlayJobAsync` bzw.
   in `PlayImpactLanding` einhängen. Das ist der einzige Fall, der
   tatsächlichen Code in der Animationsschicht braucht - die Spiellogik
   selbst (TacticsCore) bleibt davon in jedem Fall unberührt.
