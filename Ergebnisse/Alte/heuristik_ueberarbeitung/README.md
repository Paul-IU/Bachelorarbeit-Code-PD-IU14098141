# Überarbeitung der Bewertungsfunktion der heuristischen Agenten

Dokumentation im Code: `TacticsCore/Agents/TurnEvaluator.cs`, Abschnitt „Erster Entwurf und
Überarbeitung“. Gewichte: `HeuristicAgentSettings` (`DamagePreventedWeight` 1,0,
`AttackOpportunityWeight` erst 1,0, seit Überarbeitung 3 2,0, siehe unten).

1. **Schutz als Flussgröße**: Schadensreduktion und Umleitung zählen mit dem Anteil der
   aktuellen Bedrohung, den sie abfangen, also T × (1 − m). Früher zählte der Zugewinn an
   effektiven Lebenspunkten (HP / m − HP). Ohne Gegner in Reichweite ist der Schutz 0 wert.
2. **Angriffsmöglichkeit**: Das Spiegelbild der Bedrohung ist die Summe der eigenen
   Schadenswerte gegenüber erreichbaren Gegnern. Die Zunahme zählt offensiv. Gegner, die
   im selben Zug besiegt werden, sind ausgenommen.

## Stichprobe (vor dem vollständigen Lauf)

20 Paarungen (Index 0, 178, …, 3382) × 12 Ausgangslagen (0, 6, …, 66), Seed 2026,
Release, 12 Threads.

Erster Entwurf, frühere Messung: 87 bis 99 % der Partien am Rundenlimit. Illusionist und
Musiker bewegten sich praktisch nie.

| | aggressive (2:1) | defensive (1:2) |
|---|---|---|
| Rundenlimit | **5,8 %** | **83,3 %** |
| Runden im Schnitt | 17,4 | 86,1 |
| je Heldenzug: Bewegungen / Angriffe / Fähigkeiten | 0,670 / 0,425 / 0,221 | 0,394 / 0,073 / 0,298 |
| Mauer je Partie | 5,80 | 53,22 |
| Schildwall je Partie | 1,68 | 11,34 |
| Kampflied je Partie | 2,88 | 23,59 |
| Laufzeit (Wandzeit je Partie) | 45,7 ms | 313,7 ms |
| geschätzter Vollauf (257.040 Partien) | ≈ 3,3 h | ≈ 22 h |

Bewegungen je Zug (aggressive / defensive):

| Held | aggressive | defensive |
|---|---|---|
| Bogenschütze | 1,000 | 0,266 |
| Musiker | 1,000 | 1,000 |
| Schildträger | 0,994 | 0,498 |
| Elfe | 0,986 | 0,428 |
| Elementarmagier | 0,973 | 0,359 |
| Illusionist | 0,969 | 0,215 |
| Wand | 0,920 | 0,833 |
| Berserker | 0,512 | 0,243 |
| Hugger | 0,157 | 0,015 |

## Überarbeitung 3: Grundgewicht der Angriffsmöglichkeit

`AttackOpportunityWeight` 1,0 → **2,0** gilt für beide Agenten und wirkt vor der Gewichtung
2:1 bzw. 1:2. Mit dem Spiegelbild 1:1 war symmetrisches Heranrücken beim defensiven Agenten
immer −X. Mit 2,0 ist es neutral (Test
`SymmetrischesHeranruecken_IstBeimDefensivenAgentenNeutral_BeimAggressivenPositiv`).
Stand der Mauer: bereits abgeschwächt (siehe `../mauer_abschwaechen`).

Stichprobe wie oben. Die Spalte „1,0“ ist nach der Mauer-Abschwächung neu gemessen.

| | defensive 1,0 | defensive **2,0** | defensive 3,0 | aggressive 2,0 | aggressive 3,0 |
|---|---|---|---|---|---|
| Rundenlimit | 79,6 % | **72,5 %** | 59,6 % | 0,0 % | 0,0 % |
| Runden im Schnitt | 83,1 | 76,7 | 67,8 | 10,2 | 10,0 |
| je Heldenzug: Bewegungen / Angriffe / Fähigkeiten | 0,51 / 0,10 / 0,28 | 0,49 / 0,15 / 0,26 | 0,53 / 0,18 / 0,27 | 0,90 / 0,67 / 0,20 | 0,90 / 0,67 / 0,20 |
| Mauer je Partie | 21,0 | 14,1 | 14,3 | 1,50 | 1,49 |
| Schildwall je Partie | 13,6 | 9,6 | 6,0 | 1,35 | 1,42 |
| Kampflied je Partie | 21,5 | 14,1 | 18,1 | 2,25 | 2,25 |
| geschätzter Vollauf | ≈ 20 h | ≈ 13 h | ≈ 12 h | ≈ 2,4 h | ≈ 2,4 h |

**Der defensive Agent bleibt überwiegend passiv, auch bei 3,0.** Die formale Ursache (−X) ist
beseitigt. Die Diagnose (defensiv, 2,0, dieselbe Stichprobe) zeigt, dass die verbleibende
Passivität nichts mit dem Heranrücken zu tun hat:

- **Die Einheiten stehen bereits nah beieinander.** In 42 % der Heldenzüge ist ein
  Sofortangriff möglich, in 48 % sind beide Seiten gegenseitig in Reichweite. Außer
  Reichweite sind die Einheiten nur in 2,5 % der Züge. Ist ein Sofortangriff möglich,
  greift der Agent nur in 5 % der Fälle als erste Teilaktion an und passt in 35 %.
- **Heilung hebt den Schaden auf.** In den Partien am Rundenlimit werden je Partie
  3.543 von 5.036 Schadenspunkten wieder geheilt (70 %), das sind 50 Schaden je Runde.
  Mit Elfe enden 83 % der Partien am Rundenlimit, ohne Elfe 52 %.
- **Ursache 1, ein Bewertungsfehler: Besiegen eines umklammerten Gegners.** Die
  Umklammerung des Huggers senkt den Schaden des Gegners um 100 (meist auf 0). Der
  Besiegt-Bonus wird aus dem effektiven Schaden berechnet und ist dann 0. Gleichzeitig
  zählt die angekündigte Schwächung (100 × 2 Runden) als Nutzen. Sie verschwindet mit dem
  Tod des Gegners und wird so zum Verlust, beim defensiven Profil doppelt gewichtet. Das
  betraf rund 5.000 Züge: Angriffe, die den Gegner besiegt hätten, wurden negativ bewertet.
  **Experiment** (danach zurückgesetzt): Angekündigte Wirkungen auf Einheiten, die im
  selben Zug besiegt werden, zählen weder vorher noch nachher (wie bei der
  Angriffsmöglichkeit). Ergebnis: Rundenlimit defensiv **72,5 → 57,5 %**, bester
  Sofortangriff im Mittel −19 → +51.
- **Ursache 2, die Berserker-Passive.** Sein Schaden steigt mit sinkenden Lebenspunkten.
  Ein Treffer erhöht daher die Bedrohung durch ihn, und die zählt beim defensiven Profil
  doppelt. Von den verbleibenden nicht positiv bewerteten besten Sofortangriffen zielen
  70 % auf den Berserker. Dass er dadurch schneller fällt, sieht die einstufige Bewertung
  nicht.
- **Ursache 3, der Umklammerungs-Abzug.** Ein umklammerter Angreifer richtet mit −100 bei
  60 bis 100 Grundschaden keinen Schaden an. Ein Angriff bringt dann nichts und wird zu
  Recht nicht positiv bewertet (4.644 Fälle). Das ist eine Balance-Frage, kein Fehler der
  Bewertung.

Korrektur zur Diagnose oben: Die „5 %“ beziehen sich nur auf die **erste** Teilaktion eines
Zugs. Wie oft über den ganzen Zug angegriffen wird, wurde für diesen Stand nicht gemessen
(Werte nach Überarbeitung 4 siehe unten).

## Überarbeitung 4: Fehlerkorrektur beim Besiegen eines geschwächten Gegners

Das behebt Ursache 1. Die Korrektur ist generisch und gilt für beide Agenten
(Dokumentation in `TurnEvaluator`, Abschnitt „Überarbeitung 4“):

- **a) Besiegt-Bonus aus dem ungeschwächten Grundschaden** (`BaseAttackDamage` ×
  `KillBonusRounds`) statt aus dem effektiven Schaden. Besiegen beendet eine Einheit
  dauerhaft, maßgeblich ist daher ihr dauerhafter Kampfwert. Das gilt in beide Richtungen:
  Eine Schwächung senkt den Bonus nicht, ein Buff hebt ihn nicht. Verworfen wurde
  max(effektiv, Grundwert), weil es Buffs zählt, Schwächungen aber nicht. Folge: Die
  Berserker-Passive geht nicht mehr in den Besiegt-Bonus ein. Ihre Bewertung als
  Bedrohung bleibt unverändert.
- **b) Angekündigte Wirkungen auf Einheiten, die im selben Zug besiegt werden, zählen
  nicht**, weder vorher noch nachher. Das ist dieselbe Regel wie bei der
  Angriffsmöglichkeit.

Nicht geändert: die Berserker-Passive (Grenze der einstufigen Bewertung, ein Befund) und
die Stärke der Umklammerung (eine Balance-Frage).

Tests: `AngriffDerEinenUmklammertenGegnerBesiegt_WirdPositivBewertet` und
`BesiegtBonus_HaengtNichtVomAktuellenEffektivenSchadenAb` (geschwächt, gebufft,
unverändert).

Stichprobe wie oben, Grundgewicht 2,0:

| | defensive vorher | defensive **nachher** | aggressive vorher | aggressive nachher |
|---|---|---|---|---|
| Rundenlimit | 72,5 % | **57,5 %** | 0,0 % | 0,0 % |
| Runden im Schnitt | 76,7 | 64,8 | 10,2 | 10,3 |
| je Heldenzug: Bewegungen / Angriffe / Fähigkeiten | 0,49 / 0,15 / 0,26 | 0,52 / 0,17 / 0,26 | 0,90 / 0,67 / 0,20 | 0,90 / 0,67 / 0,20 |
| Mauer / Schildwall / Kampflied je Partie | 14,1 / 9,6 / 14,1 | 11,5 / 8,2 / 13,9 | 1,50 / 1,35 / 2,25 | 1,50 / 1,37 / 2,25 |
| geschätzter Vollauf | ≈ 13 h | ≈ 31 h | ≈ 2,4 h | ≈ 2,9 h |

Die Schätzung der Laufzeit ist grob, weil sie aus nur 240 Partien stammt. Die Wandzeit je
defensiver Partie schwankte zwischen den Läufen stark (185 bis 437 ms), obwohl die Partien
sogar kürzer wurden.

**Diagnose nach der Korrektur** (defensiv, dieselbe Stichprobe):

| Lage zu Zugbeginn | Anteil der Heldenzüge | Angriff im Zug | erste Teilaktion |
|---|---|---|---|
| Sofortangriff möglich | 36,3 % | **39 %** | Bewegung 67 %, Passen 25 %, Angriff 7 % |
| gegenseitig in Reichweite (nächster Zug) | 52,2 % | 4 % | Passen 64 %, Bewegung 31 % |
| nur eigene Chance | 1,4 % | 7 % | Passen 85 % |
| nur bedroht | 7,3 % | 1 % | Bewegung 86 % |
| außer Reichweite | 2,8 % | 0 % | Bewegung 63 % |

- Es gibt keinen Fall mehr, in dem ein Angriff den Gegner besiegt und der Besiegt-Bonus
  trotzdem 0 ist.
- Der beste Sofortangriff wird in 59 % der Fälle positiv bewertet (vorher 45 %), im
  Mittel mit +50 (vorher −19).
- Von den nicht positiv bewerteten besten Sofortangriffen zielen 70 % auf den Berserker.
  Die Bedrohung steigt, weil seine Passive seinen Schaden erhöht. Dazu kommen 4.634 Fälle
  ohne Schaden, weil der eigene Angreifer umklammert ist.
- Die Heilung gleicht in den Partien am Rundenlimit weiter 75 % des Schadens aus. Mit Elfe
  enden 69 % der Partien am Rundenlimit, ohne Elfe 36 %.

## Schritte 6 und 7: Mehrfachzählung und gesperrte Felder

Stand vor Schritt 6: nach der Behebung der Hugger-Regellücke (siehe `../hugger_letzter_held`).

- **Schritt 6, Mehrfachzählung:** Jeder Gegner geht einmal in die Bedrohung ein, mit
  Schaden × größtem Multiplikator der eigenen Einheiten, die er erreicht. Spiegelbildlich
  geht jede eigene Einheit einmal in die Angriffsmöglichkeit ein. Die abgefangene
  Bedrohung wird nur noch als Differenz zur ungeschützten Rechnung ausgewiesen
  (`TurnScore.DamagePreventedGain`); sie geht nicht mehr in die Summe ein.
  `DamagePreventedWeight` entfällt.
- **Schritt 7, gesperrte Felder:** Die Reichweite wird über die Wegsuche bestimmt, also
  Standfeld oder erreichbares Feld und von dort Angriffsreichweite. Die Mauer schneidet
  so tatsächlich Wege ab.

Tests:
- Neu: `Bedrohung_ZaehltEinenGegnerNurEinmal` und
  `GegnerDemDieMauerDenWegVersperrt_ZaehltFuerDasAbgeschnitteneZielNicht`.
- Angepasst:
  - Schildwall: 100 → 50, weil der Gegner nur einmal zuschlägt.
  - Mauer: 50 → 25, aus demselben Grund.

| | aggr. vorher | aggr. Schritt 6 | aggr. Schritt 7 | def. vorher | def. Schritt 6 | def. Schritt 7 |
|---|---|---|---|---|---|---|
| Rundenlimit | 0,0 % | 4,2 % | 4,2 % | 29,6 % | **18,3 %** | 18,3 % |
| Runden im Schnitt | 10,3 | 13,6 | 13,6 | 42,6 | 31,3 | 31,4 |
| je Heldenzug: Bew. / Angr. / Fäh. | 0,90 / 0,67 / 0,20 | 0,65 / 0,49 / 0,10 | 0,65 / 0,49 / 0,10 | 0,79 / 0,23 / 0,35 | 0,54 / 0,27 / 0,22 | 0,54 / 0,27 / 0,22 |
| Mauer / Schildwall / Kampflied je Partie | 1,50 / 1,37 / 2,25 | 0,38 / 0,33 / 2,30 | 0,41 / 0,33 / 2,30 | 14,0 / 7,5 / 9,1 | 5,0 / 3,1 / 5,9 | 5,1 / 3,1 / 5,9 |

**Laufzeit der Bewertung** (fester Messsatz: 2.164 Zustände aus Zufallspartien, einfädig,
Bestwert aus 3 Durchgängen, `ExplorationBewertungsZeitTemp`):

| Stand | ms je Entscheidung |
|---|---|
| Schritt 6 | 1,10 |
| Schritt 7, erste Umsetzung (volle Wegsuche je Einheit und Zustand) | 2,78 (+153 %) |
| **Schritt 7, optimiert** | **1,36 (+23 %)** |

- **Optimierung:** Einheiten blockieren keine Bewegung. Ohne Feldsperre gegen das eigene
  Team entspricht die Wegsuche daher den vorab berechneten Wegdistanzen des festen
  Bretts; die volle Wegsuche läuft nur, wenn eine Sperre aktiv ist. Dazu kommt ein
  Zwischenspeicher für die Schadensmultiplikatoren.
- **Gleichheit geprüft:** Die Stichprobe ist vor und nach der Optimierung in allen
  Kennzahlen identisch.
- **Offene Lücke:** Für den Stand vor Schritt 6 gibt es keinen Messsatz-Wert. Die
  Wandzeit je Partie der Stichprobe (vorher 144 ms bei 42,6 Runden, nach Schritt 7
  173 ms bei 31,4 Runden) deutet darauf hin, dass schon Schritt 6 je Entscheidung
  teurer war. Diese Wandzeiten schwanken aber stark.

**Diagnose defensiv** (nach Schritt 6; Schritt 7 identisch bis auf ±1):

| Lage zu Zugbeginn | Anteil der Heldenzüge | Angriff im Zug |
|---|---|---|
| Sofortangriff möglich | 48,1 % | 52 % |
| gegenseitig in Reichweite (nächster Zug) | 15,5 % | 11 % |
| nur eigene Chance | 2,8 % | 8 % |
| nur bedroht | 14,2 % | 0 % |
| außer Reichweite | 19,5 % | 0 % |

- In 2.171 Zügen passt der Agent, obwohl ein Sofortangriff möglich ist. In 97 % davon ist
  der beste Angriff exakt 0 wert. Das sind ausschließlich Berserker-Gleichstände:
  80 Schaden gegen +40 Bedrohung × 2 (2.106 Fälle).
- Die frühere Hauptursache, der Anteil „gegenseitig in Reichweite, aber gepasst“, ist
  von 52 % auf 15 % der Heldenzüge gefallen.

**Schildträger:** Der Schildwall wird deutlich seltener eingesetzt (aggressiv 1,37 → 0,33,
defensiv 7,5 → 3,1 je Partie). Er zählt nur noch, wenn er alle Ziele deckt, die ein
Gegner erreicht. Grobe Siegquote des Schildträgers in der Stichprobe: aggressiv 54 %,
defensiv **29 %**, der niedrigste Wert aller Helden. Für den Stand vorher fehlt dieser
Wert, und die Stichprobe ist dafür klein (etwa 160 Einsätze je Held). Eine gesicherte
Aussage liefert erst der vollständige Lauf.

## Schritt 5: Umklammerung prozentual (Balance, Einzelmessung)

Die Umklammerung wirkt auf ein gegnerisches Ziel jetzt mit **×0,67 (33 % Abzug)** statt
−100 (`DefaultHeroes.HuggerUmklammerungSchadensmultiplikator`). Dafür gibt es neu den
generischen Wert `StatModifierSet.AttackDamageMultiplier`: Erst werden die Deltas
addiert, dann wird multipliziert, dann gerundet. Der Bonus für ein verbündetes Ziel
(+100) bleibt gleich. Tests: `UmklammerungProzentualTests` (alle 9 Helden, Reihenfolge
von Delta und Multiplikator).

Stichprobe wie oben; nur diese Änderung, noch ohne die Korrektur der Mehrfachzählung:

| | defensive vorher | defensive nachher | aggressive vorher | aggressive nachher |
|---|---|---|---|---|
| Rundenlimit | 57,5 % | **47,5 %** | 0,0 % | 0,0 % |
| mit Hugger (144 Partien) | 72,2 %¹ | **55,6 %** | | |
| ohne Hugger (96 Partien) | 35,4 %¹ | 35,4 % | | |
| Runden im Schnitt | 64,8 | 57,3 | 10,3 | 10,3 |
| je Heldenzug: Bewegungen / Angriffe / Fähigkeiten | 0,52 / 0,17 / 0,26 | 0,65 / 0,18 / 0,31 | 0,90 / 0,67 / 0,20 | 0,90 / 0,67 / 0,20 |
| Mauer / Schildwall / Kampflied je Partie | 11,5 / 8,2 / 13,9 | 13,9 / 7,5 / 13,9 | 1,50 / 1,37 / 2,25 | 1,50 / 1,37 / 2,25 |

¹ Hergeleitet, nicht gemessen: Partien ohne Hugger werden von der Änderung nicht berührt
(nachher 34 von 96 am Rundenlimit). Vorher waren insgesamt 138 von 240 am Rundenlimit;
für die Partien mit Hugger bleiben also 104 von 144.

Diagnose (defensiv): Ist ein Sofortangriff möglich, wird jetzt in 46 % dieser Züge
angegriffen (vorher 39 %). Die Zahl der Züge, in denen trotzdem gepasst wird, fällt von
6.514 auf 1.424. Fälle mit Angriffsschaden 0 durch Umklammerung gibt es nicht mehr. Der
beste Angriff ist dabei noch in 27,5 % exakt 0 (alles Berserker-Gleichstände) und in
72,5 % deutlich schlechter als Passen.
