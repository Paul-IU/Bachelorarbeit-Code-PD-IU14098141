# Korrektur "Wirkungsdauer hängt am Anwender"

Vergleicht den vollständigen Basislauf (3.570 Paarungen, 84 Kompositionen,
72 Ausgangslagen je Paarung, 257.040 Partien je Agent, Basis-Seed `2026`)
**vor** und **nach** der Korrektur des Effekt-Zeitpunkts, für beide
Zufallsagenten (`byactiontype`, `uniform`).

- **Vorher** = Stand nach der ×10-Skalierung (`vor_korrektur/`).
- **Nachher** = dieses Verzeichnis (`basislauf_<agent>_*.csv`).
- Alles andere (Katalogwerte, Agenten, Seeds, Ausgangslagen) ist identisch.

## Was geändert wurde

Die Restdauer **jedes** Effekts mit begrenzter Dauer zählt jetzt zu Beginn des
nächsten eigenen Zugs seines **Anwenders** herunter und endet dort - egal ob
unit- oder feld-verankert und egal, auf wie viele Einheiten er wirkt. Der
Schadens-Tick (Verbrennung) bleibt beim Zugbeginn des **Ziels**. Zugreihenfolge
und Abklingzeiten sind unverändert. Umsetzung: `EffectSystem.TickUnitEffects`
= `TickDamageOverTime` (Ziel) + `TickDurationsOfSource` (Anwender); der
Rundenbeginn-Tick für Felder (`TickFieldEffects`, `MatchController.Run`) entfällt.

## Ursache des Fehlbilds

Schildwall und Mauer sind **feld-verankert** und wurden einmal zu Beginn
**jeder Runde** getickt (`MatchController.Run` → `EffectSystem.TickFieldEffects`).
Ein Effekt mit Dauer 1 lief damit am nächsten Rundenbeginn ab - vor den Zügen
aller Einheiten, die in der Reihenfolge vor dem Anwender stehen. Belegt am
Beispiel des Fehlerberichts (Reihenfolge Illusionist, Bogenschütze,
Schildträger, ...): der Bogenschütze an Rang 2 traf in Runde N+1 den bereits
ungeschützten Schildträger (volle 120 statt 60 Schaden).

Dokumentation vs. Implementierung: `EffectSystem` beschrieb den Rundenbeginn für
Feld-Effekte korrekt, die Implementierung entsprach dem. **Falsch war die
Aussage in der Dokumentation von `MatchController.ExecuteTurn`**, das Ablaufen
"JEDER Effektdauer" erfolge zu Beginn des Zugs der betroffenen Einheit - für
Feld-Effekte traf das nie zu. Beides ist jetzt korrigiert und einheitlich.

## Ergebnisse

### Bereinigte Siegquote je Held (Prozent)

**Agent `byactiontype`**

| Held | Rolle | vorher | nachher | Δ pp | Rang |
|---|---|---:|---:|---:|---|
| Elementarmagier | Dmg | 62,70 | 58,98 | −3,72 | 1 → 2 |
| Elfe | Support | 60,21 | 60,70 | +0,49 | 2 → 1 |
| Berserker | Dmg | 58,53 | 56,30 | −2,23 | 3 → 4 |
| Illusionist | Tank | 58,10 | 57,87 | −0,23 | 4 → 3 |
| Bogenschütze | Dmg | 49,46 | 45,13 | −4,33 | 5 → 6 |
| **Schildträger** | Tank | 45,51 | 47,62 | **+2,11** | 6 → 5 |
| Hugger | Support | 40,97 | 40,37 | −0,60 | 7 → 9 |
| **Wand** | Tank | 37,93 | 42,64 | **+4,71** | 8 → 7 |
| Musiker | Support | 36,59 | 40,38 | +3,79 | 9 → 8 |

**Agent `uniform`**

| Held | Rolle | vorher | nachher | Δ pp | Rang |
|---|---|---:|---:|---:|---|
| Elfe | Support | 62,25 | 62,40 | +0,15 | 1 → 1 |
| Elementarmagier | Dmg | 61,69 | 57,68 | −4,01 | 2 → 2 |
| Berserker | Dmg | 56,28 | 53,61 | −2,67 | 3 → 4 |
| Bogenschütze | Dmg | 55,17 | 49,57 | −5,60 | 4 → 6 |
| Illusionist | Tank | 53,91 | 53,62 | −0,29 | 5 → 3 |
| **Schildträger** | Tank | 47,44 | 51,29 | **+3,85** | 6 → 5 |
| **Wand** | Tank | 42,25 | 47,90 | **+5,65** | 7 → 7 |
| Hugger | Support | 39,35 | 38,80 | −0,55 | 8 → 8 |
| Musiker | Support | 31,66 | 35,13 | +3,47 | 9 → 9 |

Die Siegquoten sind zusammen nullsummig (bereinigt, Mittel 50 %): was Wand,
Schildträger und Musiker gewinnen, verlieren vor allem die Schadensverursacher
(Bogenschütze, Elementarmagier, Berserker), die gegen längere und stärkere
Schadensreduktion antreten.

**Wand und Schildträger:** die Wand gewinnt am stärksten (+4,7 / +5,7 pp,
relativ +12 % / +13 %), der Schildträger weniger (+2,1 / +3,9 pp). Der Abstand
zwischen beiden schrumpft von 7,6 auf 5,0 pp (`byactiontype`) bzw. von 5,2 auf
3,4 pp (`uniform`). Die Tank-Rangfolge bleibt in beiden Agenten
**Illusionist > Schildträger > Wand**.

### Partiedauer und Rundenlimit

| Agent | Ø Partiedauer (Runden) vorher → nachher | Rundenlimit-Ende vorher → nachher |
|---|---|---|
| `byactiontype` | 32,87 → 35,82 (+2,95; +9,0 %) | 2,96 % → 3,95 % (+0,98 pp) |
| `uniform` | 37,78 → 41,30 (+3,52; +9,3 %) | 4,91 % → 6,60 % (+1,69 pp) |

Am stärksten verlängert die Wand die Partien (Ø +4,3 Runden `byactiontype`,
+5,1 `uniform`; Rundenlimit-Anteil dort 3,0 → 4,9 % bzw. 5,1 → 8,1 %). Alle
Werte je Held: `helden_vergleich.csv`, Gesamtwerte: `gesamt_vergleich.csv`,
komplette Auswertung inkl. Kompositions-Extremen: `auswertung.txt`.

### Kontrollen

- **Determinismus** bei gleichem Seed, **Reproduzierbarkeit** bei 1 und mehreren
  Threads: bestehende Tests (`HeroDeterminismTests`, `MatchDeterminismTests`,
  `BatchSimulationRunnerTests`) laufen unverändert (256 Tests grün).
- **Unbeteiligte Paarungen bit-identisch:** die 55 Paarungen, an denen weder
  Schildträger, Wand, Musiker noch Elementarmagier (die einzigen Helden mit
  Effekten endlicher Dauer) beteiligt sind, liefern in beiden Agenten
  exakt dieselben Werte wie vorher (0 Abweichungen). Alle anderen Paarungen
  ändern sich erwartungsgemäß (3.255 bzw. 3.311 von 3.515 mit geänderter
  Sieganzahl - ein System aus verzweigenden Zufallsentscheidungen, in dem jede
  Verhaltensänderung den Rest der Partie verschiebt).
- **Spiegelpaarungen exakt gleiche Siegzahlen** (antithetische Paarbildung,
  alle 72 Ausgangslagen, beide Agenten): zum Zeitpunkt dieses Laufs galt es nur
  für die **56 Kompositionen ohne Hugger**; die 28 mit Hugger waren
  asymmetrisch (typisch 14:10 statt 12:12) - **bereits vor dieser Korrektur**.
  **Inzwischen behoben:** die Ursache war die Sortierung der Fähigkeitsoptionen
  in `AbilitySystem.GetLegalAbilityUses` (Id-abhängig, nur beim Hugger wirksam),
  siehe `../spiegel_symmetrie/README.md`. Die hier dokumentierten Läufe stammen
  vom Stand VOR dieser Behebung und dienen dort als Vorher-Referenz.
  Nebenbefund: der Befehl `full-baseline` nutzt keine antithetische
  Paarbildung, seine Spiegelzeilen sind daher nicht exakt 50 % - das ist
  gewollt und kein Fehler (Begründung ebenfalls dort).

## Wirkung je Effekt

| Effekt | Dauer / Anker | vorher | nachher |
|---|---|---|---|
| Schildwall | 1, Feld | endete am nächsten Rundenbeginn | wirkt bis zum nächsten eigenen Zug des Schildträgers (Abklingzeit 2 unverändert) |
| Mauer (2 gekoppelte Instanzen) | 1, Feld + Wand | wie Schildwall; die Selbstreduktion (unit-verankert) endete dadurch mit | beide enden gemeinsam im Dauer-Tick der Wand; ohne Abklingzeit kann sie jeden Zug neu wirken - Schutz praktisch lückenlos |
| Kampflied / Marschlied | 2, je Verbündeter | deckte genau **eine** Aktion des Verbündeten | deckt zwei Zyklen des Musikers (etwa doppelt so viele Aktionen) |
| Verbrennung | 3, Ziel | 3 Schadens-Ticks | Schadens-Tick weiterhin am Ziel (30/20/10); im Regelfall weiterhin 3 Ticks, unabhängig von der Reihenfolge |
| Verlangsamung (Erdschlag) | 2, Ziel | eine Aktion des Ziels | zwei Aktionen |
| Klon | keine Dauer | - | unberührt (lebt bis Tod/Ersetzen) |
| Aufladen, Umklammerung | keine Dauer | - | unberührt |

**Abklingzeiten** sind unberührt (Test `Abklingzeit_ZaehltWeiterhin...`).
**Stirbt der Anwender**, enden alle seine Effekte auf anderen Einheiten sofort
(`RemoveEffectsRelatedTo`) - schon vorher so, jetzt zwingend, weil sonst niemand
die Dauer herunterzählen würde (Tests `StirbtDerAnwender_...`).

### Auffälligkeiten (bewusst nicht geändert)

1. **Verbrennung:** ändert sich die relative Reihenfolge von Anwender und Ziel
   zwischen den Runden (Tod einer schnelleren Einheit), sind 2 oder 4 statt 3
   Schadens-Ticks möglich (der 4. verursacht wegen des Abklingens 0 Schaden).
2. **Kopplung gleichnamiger Instanzen** (`RemoveEffectAndLinkedCounterpart`,
   Name + Anwender): läuft bei einem Mehrfach-Buff eine versetzt aufgefrischte
   Instanz früher ab, wird eine andere Instanz desselben Buffs mit entfernt.
   Nachgestellt: Kampflied auf drei Einheiten, eine verlässt den Radius, zweiter
   Einsatz - beim Ablauf von B verliert A den Buff mit noch 2 Restdauer. Bestand
   auch vorher, ist nicht Teil dieser Korrektur.
3. **Fremdes Auffrischen:** frischt ein anderer Anwender einen bestehenden
   gleichnamigen Effekt auf (`RefreshDuration`, z.B. zwei Schildträger auf
   demselben Feld), bleibt die Quelle und damit der Dauer-Tick beim ersten
   Anwender (Nachgestellt).

## Beobachtungen zur Wand-Vergeltung (nur geprüft, nicht geändert)

Getestet in `EffectDurationTimingTests` (Abschnitt "Beobachtung_..."), Wand und
Schildträger auf demselben Feld, beide Fähigkeiten aktiv:

- **a) Bestätigt.** Greift der Angreifer den **Schildträger** an, wird der Angriff
  zur Wand umgeleitet und löst Vergeltung aus. Greift er die **Wand direkt**
  an, gibt es keine Vergeltung. Grund: `CombatSystem.ResolveRedirect` liefert bei
  einer Umleitung auf das Ziel selbst keine Umleitung zurück
  (`redirectUnit.Id != target.Id`), und die Vergeltung hängt am Umleiten
  (`redirectEffect != null`), nicht am Getroffenwerden der Wand.
- **b) Bestätigt.** Die Reduktionen wirken multiplikativ: Schildwall (Feld, 0,5)
  × Mauer-Selbstreduktion (Unit, 0,5) = 0,25; 80 → 20 (im Test 100 → 25). Der
  Schaden kommt bei der **Wand** an, der Schildträger bleibt unberührt. Die
  Vergeltung (50 % des **unreduzierten** Schadens) trifft den Angreifer.
  Die Wand erhält den Schaden in beiden Fällen gleich (0,25); nur die Vergeltung
  unterscheidet sich.

## Für die Arbeit (Designentscheidung)

> Die Wirkungsdauer eines Effekts wird in Zügen seines Anwenders gemessen und
> zu Beginn von dessen nächstem Zug heruntergezählt. Damit wirkt eine Fähigkeit
> mit Dauer 1 stets genau einen vollen Zyklus der Zugreihenfolge, unabhängig
> vom Rang des Helden - anders als bei einem rundenbasierten Tick, der
> Helden am Ende der Reihenfolge fast keine Wirkung ließ. Wirkt ein Effekt auf
> mehrere Einheiten, hängt die Dauer am Anwender: es gibt eine
> Effektinstanz statt mehrerer mit unterschiedlichen Restlaufzeiten, der
> Schutz endet für alle gleichzeitig, und der Anwender kontrolliert die Dauer
> seiner eigenen Fähigkeit. Schaden über Zeit wird weiterhin beim Ziel
> verrechnet; nur die Restlaufzeit hängt am Anwender. Stirbt der Anwender,
> enden seine Effekte sofort.
>
> Wirkung: die Partien werden im Mittel um ca. 9 % länger (32,9 → 35,8 bzw.
> 37,8 → 41,3 Runden), Wand (+4,7 / +5,7 pp), Schildträger (+2,1 / +3,9 pp) und
> Musiker gewinnen, die Schadensverursacher verlieren; die Tank-Rangfolge
> (Illusionist > Schildträger > Wand) bleibt erhalten.

## Reproduktion

```
dotnet run --project TacticsCore.Demo -c Release -- full-baseline \
  <paarungenCsv> <kompositionenCsv> <heldenCsv> 2026 byactiontype   # bzw. uniform
python auswertung_effektdauer.py
```
