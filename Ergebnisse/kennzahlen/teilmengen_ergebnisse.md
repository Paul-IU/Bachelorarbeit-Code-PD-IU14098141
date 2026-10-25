# Simulationsumfang über Teilmengen (Schritt 6)

Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: `numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.

Unentschieden zählen als halber Sieg. Erkannt = (a) Siegquote des Schildträgers in Richtung der Manipulation UND (b) einseitiger Vorzeichentest (α = 0,05) über die veränderten Kompositionen mit Schildträger. ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp); nur (a) = Anteil der Wiederholungen mit richtigem Vorzeichen von ΔST; Test unmöglich = Wiederholungen mit weniger als 5 veränderten Kompositionen.

## Übersicht: Anteil erkannter Wiederholungen und kleinste Stichprobe mit ≥ 95 %

In Klammern: Wiederholungen, in denen der Vorzeichentest mangels veränderter Kompositionen nicht anschlagen konnte (wenn > 0).

| Variante | voller Lauf | 1.000 | 10.000 | 50.000 | 100.000 | ab |
|---|---|---|---|---|---|---|
| schildtraeger_ability_-10_aggressive | nicht erkannt | 0 % | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_-10_byactiontype | erkannt | 1 % (60) | 20 % | 81 % | 99 % | 100.000 |
| schildtraeger_ability_-10_defensive | erkannt | 10 % | 53 % | 97 % | 100 % | 50.000 |
| schildtraeger_ability_-10_uniform | erkannt | 1 % (33) | 61 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-2.5_aggressive | nicht erkannt | 0 % (72) | 1 % | 5 % | 6 % | nie |
| schildtraeger_ability_-2.5_byactiontype | erkannt | 0 % (99) | 1 % (9) | 12 % | 51 % | nie |
| schildtraeger_ability_-2.5_defensive | nicht erkannt | 0 % | 2 % | 0 % | 0 % | nie |
| schildtraeger_ability_-2.5_uniform | erkannt | 0 % (99) | 3 % (3) | 56 % | 81 % | nie |
| schildtraeger_ability_-5_aggressive | nicht erkannt | 0 % (28) | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_-5_byactiontype | erkannt | 0 % (94) | 5 % | 35 % | 59 % | nie |
| schildtraeger_ability_-5_defensive | erkannt | 5 % | 28 % | 57 % | 86 % | nie |
| schildtraeger_ability_-5_uniform | erkannt | 1 % (78) | 28 % | 93 % | 100 % | 100.000 |
| schildtraeger_ability_10_aggressive | nicht erkannt | 0 % | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_10_byactiontype | erkannt | 2 % (38) | 34 % | 91 % | 100 % | 100.000 |
| schildtraeger_ability_10_defensive | erkannt | 12 % | 42 % | 78 % | 100 % | 100.000 |
| schildtraeger_ability_10_uniform | erkannt | 3 % (9) | 70 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_2.5_aggressive | nicht erkannt | 1 % (45) | 1 % | 0 % | 1 % | nie |
| schildtraeger_ability_2.5_byactiontype | erkannt | 0 % (78) | 8 % | 39 % | 72 % | nie |
| schildtraeger_ability_2.5_defensive | nicht erkannt | 1 % | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_2.5_uniform | erkannt | 0 % (77) | 23 % | 89 % | 100 % | 100.000 |
| schildtraeger_ability_5_aggressive | nicht erkannt | 0 % (6) | 4 % | 3 % | 3 % | nie |
| schildtraeger_ability_5_byactiontype | erkannt | 0 % (59) | 15 % | 61 % | 90 % | nie |
| schildtraeger_ability_5_defensive | erkannt | 1 % | 6 % | 19 % | 21 % | nie |
| schildtraeger_ability_5_uniform | erkannt | 0 % (42) | 51 % | 98 % | 100 % | 50.000 |
| schildtraeger_dmg_-10_aggressive | erkannt | 90 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_byactiontype | erkannt | 73 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_defensive | erkannt | 31 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_uniform | erkannt | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_aggressive | erkannt | 49 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_byactiontype | erkannt | 19 % | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_defensive | erkannt | 9 % | 81 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_-2.5_uniform | erkannt | 25 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_aggressive | erkannt | 64 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_byactiontype | erkannt | 49 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_defensive | erkannt | 15 % | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_-5_uniform | erkannt | 46 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_aggressive | erkannt | 46 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_byactiontype | erkannt | 50 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_defensive | erkannt | 14 % | 88 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_10_uniform | erkannt | 50 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_aggressive | erkannt | 0 % (37) | 25 % | 52 % | 71 % | nie |
| schildtraeger_dmg_2.5_byactiontype | erkannt | 7 % (19) | 83 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_2.5_defensive | erkannt | 2 % | 14 % | 40 % | 61 % | nie |
| schildtraeger_dmg_2.5_uniform | erkannt | 6 % (8) | 83 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_5_aggressive | erkannt | 24 % | 97 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_byactiontype | erkannt | 25 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_defensive | erkannt | 6 % | 38 % | 96 % | 99 % | 50.000 |
| schildtraeger_dmg_5_uniform | erkannt | 28 % | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_aggressive | erkannt | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_byactiontype | erkannt | 62 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_defensive | erkannt | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_uniform | erkannt | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_aggressive | erkannt | 30 % (2) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_byactiontype | erkannt | 5 % (13) | 89 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_defensive | erkannt | 3 % (24) | 77 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_uniform | erkannt | 4 % (20) | 91 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-5_aggressive | erkannt | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_byactiontype | erkannt | 17 % (1) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_defensive | erkannt | 21 % (3) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_uniform | erkannt | 20 % (1) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_aggressive | erkannt | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_byactiontype | erkannt | 57 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_defensive | erkannt | 50 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_uniform | erkannt | 66 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_aggressive | erkannt | 40 % (2) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_byactiontype | erkannt | 8 % (12) | 89 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_defensive | erkannt | 6 % (24) | 85 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_uniform | erkannt | 6 % (15) | 90 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_5_aggressive | erkannt | 73 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_byactiontype | erkannt | 17 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_defensive | erkannt | 23 % (3) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_uniform | erkannt | 28 % (1) | 100 % | 100 % | 100 % | 10.000 |

## Einzelwerte je Variante

### schildtraeger_ability_-10_aggressive

Alle 257.040 Partien: ΔST +0,44 pp, Kompositionen in Richtung / dagegen 5 / 21, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 20 % | +0,50 | -1,32 … +2,02 | 4,8 (1 … 10) |
| 10.000 | 0 % | 0 | 2 % | +0,47 | -0,13 … +1,07 | 8,8 (4 … 14) |
| 50.000 | 0 % | 0 | 0 % | +0,45 | +0,23 … +0,70 | 8,1 (4 … 13) |
| 100.000 | 0 % | 0 | 0 % | +0,43 | +0,27 … +0,56 | 7,8 (3 … 12) |

### schildtraeger_ability_-10_byactiontype

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung / dagegen 24 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 60 | 60 % | -0,20 | -1,18 … +0,69 | 2,4 (0 … 6) |
| 10.000 | 20 % | 0 | 99 % | -0,24 | -0,53 … +0,07 | 12,2 (7 … 18) |
| 50.000 | 81 % | 0 | 100 % | -0,23 | -0,32 … -0,12 | 18,4 (14 … 23) |
| 100.000 | 99 % | 0 | 100 % | -0,23 | -0,32 … -0,16 | 21,6 (18 … 25) |

### schildtraeger_ability_-10_defensive

Alle 257.040 Partien: ΔST -1,08 pp, Kompositionen in Richtung / dagegen 23 / 5, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 10 % | 0 | 81 % | -1,12 | -3,92 … +1,14 | 10,4 (4 … 17) |
| 10.000 | 53 % | 0 | 100 % | -1,08 | -1,95 … -0,40 | 18,0 (13 … 23) |
| 50.000 | 97 % | 0 | 100 % | -1,08 | -1,58 … -0,72 | 21,2 (17 … 25) |
| 100.000 | 100 % | 0 | 100 % | -1,09 | -1,28 … -0,87 | 22,5 (20 … 26) |

### schildtraeger_ability_-10_uniform

Alle 257.040 Partien: ΔST -0,48 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 33 | 72 % | -0,36 | -1,67 … +0,47 | 3,5 (0 … 9) |
| 10.000 | 61 % | 0 | 100 % | -0,47 | -0,80 … -0,14 | 14,8 (9 … 20) |
| 50.000 | 100 % | 0 | 100 % | -0,48 | -0,63 … -0,38 | 22,9 (18 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,48 | -0,59 … -0,37 | 25,6 (22 … 28) |

### schildtraeger_ability_-2.5_aggressive

Alle 257.040 Partien: ΔST -0,02 pp, Kompositionen in Richtung / dagegen 14 / 12, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 72 | 36 % | +0,01 | -0,70 … +0,87 | 1,7 (0 … 5) |
| 10.000 | 1 % | 0 | 50 % | -0,00 | -0,29 … +0,28 | 8,9 (2 … 14) |
| 50.000 | 5 % | 0 | 69 % | -0,02 | -0,10 … +0,09 | 13,4 (9 … 18) |
| 100.000 | 6 % | 0 | 78 % | -0,02 | -0,09 … +0,04 | 14,8 (11 … 19) |

### schildtraeger_ability_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,05 pp, Kompositionen in Richtung / dagegen 16 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 99 | 18 % | -0,02 | -0,24 … +0,25 | 0,4 (0 … 3) |
| 10.000 | 1 % | 9 | 74 % | -0,04 | -0,22 … +0,07 | 4,5 (1 … 11) |
| 50.000 | 12 % | 0 | 100 % | -0,04 | -0,10 … -0,00 | 10,8 (7 … 17) |
| 100.000 | 51 % | 0 | 100 % | -0,05 | -0,08 … -0,01 | 14,4 (9 … 20) |

### schildtraeger_ability_-2.5_defensive

Alle 257.040 Partien: ΔST +0,04 pp, Kompositionen in Richtung / dagegen 12 / 15, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 45 % | +0,01 | -1,85 … +1,28 | 6,7 (3 … 11) |
| 10.000 | 2 % | 0 | 40 % | +0,05 | -0,78 … +0,50 | 12,6 (7 … 18) |
| 50.000 | 0 % | 0 | 33 % | +0,04 | -0,15 … +0,26 | 13,3 (9 … 18) |
| 100.000 | 0 % | 0 | 26 % | +0,03 | -0,11 … +0,16 | 14,2 (11 … 18) |

### schildtraeger_ability_-2.5_uniform

Alle 257.040 Partien: ΔST -0,08 pp, Kompositionen in Richtung / dagegen 21 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 99 | 35 % | -0,08 | -0,48 … +0,45 | 0,7 (0 … 3) |
| 10.000 | 3 % | 3 | 96 % | -0,08 | -0,25 … +0,05 | 6,0 (1 … 10) |
| 50.000 | 56 % | 0 | 100 % | -0,08 | -0,13 … -0,02 | 14,6 (9 … 19) |
| 100.000 | 81 % | 0 | 100 % | -0,08 | -0,12 … -0,03 | 17,4 (12 … 22) |

### schildtraeger_ability_-5_aggressive

Alle 257.040 Partien: ΔST +0,03 pp, Kompositionen in Richtung / dagegen 13 / 15, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 28 | 41 % | +0,01 | -1,09 … +1,12 | 3,0 (0 … 8) |
| 10.000 | 0 % | 0 | 30 % | +0,05 | -0,38 … +0,46 | 9,7 (6 … 15) |
| 50.000 | 0 % | 0 | 30 % | +0,03 | -0,13 … +0,20 | 11,8 (7 … 18) |
| 100.000 | 0 % | 0 | 28 % | +0,03 | -0,07 … +0,13 | 11,9 (6 … 16) |

### schildtraeger_ability_-5_byactiontype

Alle 257.040 Partien: ΔST -0,09 pp, Kompositionen in Richtung / dagegen 20 / 5, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 94 | 33 % | -0,06 | -0,70 … +0,46 | 1,1 (0 … 3) |
| 10.000 | 5 % | 0 | 81 % | -0,09 | -0,32 … +0,07 | 8,1 (4 … 13) |
| 50.000 | 35 % | 0 | 100 % | -0,09 | -0,15 … -0,01 | 14,9 (9 … 20) |
| 100.000 | 59 % | 0 | 100 % | -0,09 | -0,14 … -0,04 | 17,0 (12 … 21) |

### schildtraeger_ability_-5_defensive

Alle 257.040 Partien: ΔST -0,52 pp, Kompositionen in Richtung / dagegen 21 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 0 | 78 % | -0,63 | -2,72 … +1,12 | 8,4 (4 … 15) |
| 10.000 | 28 % | 0 | 99 % | -0,52 | -1,50 … +0,29 | 16,0 (12 … 22) |
| 50.000 | 57 % | 0 | 100 % | -0,52 | -0,78 … -0,32 | 18,9 (15 … 23) |
| 100.000 | 86 % | 0 | 100 % | -0,52 | -0,65 … -0,35 | 20,1 (16 … 23) |

### schildtraeger_ability_-5_uniform

Alle 257.040 Partien: ΔST -0,24 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 78 | 56 % | -0,20 | -1,43 … +0,47 | 2,0 (0 … 7) |
| 10.000 | 28 % | 0 | 100 % | -0,24 | -0,48 … -0,05 | 11,4 (6 … 18) |
| 50.000 | 93 % | 0 | 100 % | -0,24 | -0,33 … -0,14 | 19,7 (13 … 25) |
| 100.000 | 100 % | 0 | 100 % | -0,24 | -0,29 … -0,17 | 23,1 (18 … 27) |

### schildtraeger_ability_10_aggressive

Alle 257.040 Partien: ΔST -0,15 pp, Kompositionen in Richtung / dagegen 11 / 16, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 40 % | -0,12 | -2,58 … +1,61 | 5,7 (1 … 10) |
| 10.000 | 1 % | 0 | 24 % | -0,14 | -0,69 … +0,63 | 11,1 (6 … 17) |
| 50.000 | 0 % | 0 | 6 % | -0,14 | -0,35 … +0,13 | 11,7 (8 … 17) |
| 100.000 | 0 % | 0 | 1 % | -0,15 | -0,29 … +0,03 | 11,8 (8 … 16) |

### schildtraeger_ability_10_byactiontype

Alle 257.040 Partien: ΔST +0,30 pp, Kompositionen in Richtung / dagegen 24 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 38 | 64 % | +0,27 | -0,72 … +1,67 | 3,3 (1 … 8) |
| 10.000 | 34 % | 0 | 99 % | +0,31 | -0,11 … +0,64 | 13,7 (7 … 19) |
| 50.000 | 91 % | 0 | 100 % | +0,30 | +0,14 … +0,43 | 19,6 (15 … 23) |
| 100.000 | 100 % | 0 | 100 % | +0,30 | +0,20 … +0,38 | 22,1 (18 … 26) |

### schildtraeger_ability_10_defensive

Alle 257.040 Partien: ΔST +0,95 pp, Kompositionen in Richtung / dagegen 22 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 12 % | 0 | 82 % | +1,17 | -1,11 … +3,82 | 10,3 (5 … 16) |
| 10.000 | 42 % | 0 | 100 % | +0,95 | +0,17 … +1,66 | 17,1 (12 … 22) |
| 50.000 | 78 % | 0 | 100 % | +0,94 | +0,62 … +1,34 | 19,9 (17 … 24) |
| 100.000 | 100 % | 0 | 100 % | +0,96 | +0,76 … +1,17 | 21,0 (19 … 23) |

### schildtraeger_ability_10_uniform

Alle 257.040 Partien: ΔST +0,56 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 9 | 78 % | +0,52 | -0,94 … +1,82 | 4,6 (1 … 9) |
| 10.000 | 70 % | 0 | 100 % | +0,56 | +0,25 … +0,98 | 16,3 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | +0,58 | +0,44 … +0,83 | 23,9 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,56 | +0,46 … +0,67 | 26,5 (24 … 28) |

### schildtraeger_ability_2.5_aggressive

Alle 257.040 Partien: ΔST +0,03 pp, Kompositionen in Richtung / dagegen 13 / 15, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 45 | 43 % | +0,04 | -1,15 … +1,38 | 2,6 (0 … 7) |
| 10.000 | 1 % | 0 | 59 % | +0,04 | -0,34 … +0,36 | 9,7 (5 … 14) |
| 50.000 | 0 % | 0 | 75 % | +0,03 | -0,08 … +0,16 | 12,4 (8 … 17) |
| 100.000 | 1 % | 0 | 86 % | +0,03 | -0,06 … +0,12 | 13,2 (10 … 17) |

### schildtraeger_ability_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,11 pp, Kompositionen in Richtung / dagegen 20 / 7, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 78 | 45 % | +0,07 | -0,72 … +0,93 | 1,6 (0 … 5) |
| 10.000 | 8 % | 0 | 89 % | +0,12 | -0,07 … +0,34 | 9,7 (5 … 16) |
| 50.000 | 39 % | 0 | 100 % | +0,11 | +0,01 … +0,20 | 15,7 (9 … 21) |
| 100.000 | 72 % | 0 | 100 % | +0,11 | +0,06 … +0,18 | 18,4 (14 … 23) |

### schildtraeger_ability_2.5_defensive

Alle 257.040 Partien: ΔST +0,10 pp, Kompositionen in Richtung / dagegen 16 / 12, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 55 % | +0,10 | -2,32 … +2,07 | 6,9 (2 … 11) |
| 10.000 | 0 % | 0 | 67 % | +0,09 | -0,48 … +0,67 | 12,6 (9 … 17) |
| 50.000 | 0 % | 0 | 84 % | +0,09 | -0,17 … +0,35 | 14,7 (11 … 18) |
| 100.000 | 0 % | 0 | 95 % | +0,10 | -0,06 … +0,24 | 15,1 (10 … 18) |

### schildtraeger_ability_2.5_uniform

Alle 257.040 Partien: ΔST +0,19 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 77 | 59 % | +0,18 | -0,48 … +0,95 | 2,0 (0 … 6) |
| 10.000 | 23 % | 0 | 97 % | +0,19 | -0,09 … +0,42 | 11,1 (6 … 18) |
| 50.000 | 89 % | 0 | 100 % | +0,20 | +0,11 … +0,32 | 18,5 (14 … 23) |
| 100.000 | 100 % | 0 | 100 % | +0,20 | +0,13 … +0,26 | 21,9 (17 … 26) |

### schildtraeger_ability_5_aggressive

Alle 257.040 Partien: ΔST +0,06 pp, Kompositionen in Richtung / dagegen 15 / 13, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 6 | 50 % | +0,07 | -1,43 … +1,18 | 4,1 (1 … 9) |
| 10.000 | 4 % | 0 | 62 % | +0,07 | -0,41 … +0,55 | 11,4 (6 … 18) |
| 50.000 | 3 % | 0 | 85 % | +0,06 | -0,05 … +0,26 | 14,3 (10 … 19) |
| 100.000 | 3 % | 0 | 92 % | +0,06 | -0,07 … +0,19 | 14,9 (11 … 20) |

### schildtraeger_ability_5_byactiontype

Alle 257.040 Partien: ΔST +0,17 pp, Kompositionen in Richtung / dagegen 24 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 59 | 53 % | +0,15 | -0,72 … +1,16 | 2,3 (0 … 7) |
| 10.000 | 15 % | 0 | 95 % | +0,18 | -0,14 … +0,45 | 11,4 (6 … 16) |
| 50.000 | 61 % | 0 | 100 % | +0,17 | +0,05 … +0,27 | 17,1 (12 … 23) |
| 100.000 | 90 % | 0 | 100 % | +0,17 | +0,11 … +0,24 | 20,2 (15 … 26) |

### schildtraeger_ability_5_defensive

Alle 257.040 Partien: ΔST +0,33 pp, Kompositionen in Richtung / dagegen 19 / 8, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 66 % | +0,43 | -2,20 … +3,23 | 8,4 (2 … 13) |
| 10.000 | 6 % | 0 | 89 % | +0,33 | -0,23 … +1,05 | 14,2 (8 … 20) |
| 50.000 | 19 % | 0 | 100 % | +0,32 | +0,03 … +0,63 | 16,5 (12 … 21) |
| 100.000 | 21 % | 0 | 100 % | +0,34 | +0,18 … +0,49 | 17,5 (15 … 20) |

### schildtraeger_ability_5_uniform

Alle 257.040 Partien: ΔST +0,33 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 42 | 70 % | +0,32 | -0,48 … +1,59 | 3,1 (0 … 7) |
| 10.000 | 51 % | 0 | 100 % | +0,33 | +0,07 … +0,64 | 14,0 (9 … 19) |
| 50.000 | 98 % | 0 | 100 % | +0,34 | +0,21 … +0,52 | 21,1 (17 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,33 | +0,25 … +0,41 | 24,4 (21 … 28) |

### schildtraeger_dmg_-10_aggressive

Alle 257.040 Partien: ΔST -5,01 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 90 % | 0 | 100 % | -5,04 | -10,02 … -1,35 | 15,2 (10 … 20) |
| 10.000 | 100 % | 0 | 100 % | -5,02 | -6,03 … -3,94 | 25,5 (20 … 28) |
| 50.000 | 100 % | 0 | 100 % | -5,01 | -5,34 … -4,60 | 27,8 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -5,02 | -5,19 … -4,67 | 28,0 (27 … 28) |

### schildtraeger_dmg_-10_byactiontype

Alle 257.040 Partien: ΔST -4,09 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 73 % | 0 | 100 % | -4,21 | -7,26 … -1,10 | 14,6 (10 … 21) |
| 10.000 | 100 % | 0 | 100 % | -4,05 | -4,89 … -2,91 | 25,7 (22 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,11 | -4,51 … -3,79 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,11 | -4,40 … -3,84 | 28,0 (28 … 28) |

### schildtraeger_dmg_-10_defensive

Alle 257.040 Partien: ΔST -2,52 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 31 % | 0 | 97 % | -2,37 | -5,34 … +0,88 | 13,2 (8 … 18) |
| 10.000 | 100 % | 0 | 100 % | -2,54 | -3,86 … -1,48 | 22,9 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,53 | -2,89 … -2,12 | 26,8 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,52 | -2,71 … -2,31 | 27,6 (26 … 28) |

### schildtraeger_dmg_-10_uniform

Alle 257.040 Partien: ΔST -4,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 69 % | 0 | 100 % | -4,25 | -7,09 … -0,68 | 14,6 (9 … 21) |
| 10.000 | 100 % | 0 | 100 % | -4,17 | -5,28 … -3,12 | 25,8 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,16 | -4,55 … -3,75 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,21 | -4,44 … -3,98 | 28,0 (28 … 28) |

### schildtraeger_dmg_-2.5_aggressive

Alle 257.040 Partien: ΔST -2,08 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 49 % | 0 | 100 % | -2,15 | -4,53 … -0,23 | 9,9 (5 … 15) |
| 10.000 | 100 % | 0 | 100 % | -2,11 | -2,80 … -1,57 | 22,0 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | -2,05 | -2,29 … -1,77 | 26,9 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,09 | -2,22 … -1,93 | 27,8 (26 … 28) |

### schildtraeger_dmg_-2.5_byactiontype

Alle 257.040 Partien: ΔST -1,60 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 19 % | 0 | 100 % | -1,61 | -3,63 … -0,22 | 9,1 (4 … 16) |
| 10.000 | 99 % | 0 | 100 % | -1,59 | -2,32 … -0,98 | 21,2 (16 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,60 | -1,81 … -1,30 | 27,0 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,60 | -1,76 … -1,48 | 27,9 (27 … 28) |

### schildtraeger_dmg_-2.5_defensive

Alle 257.040 Partien: ΔST -1,16 pp, Kompositionen in Richtung / dagegen 23 / 5, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 9 % | 0 | 83 % | -1,15 | -3,60 … +1,10 | 10,1 (6 … 16) |
| 10.000 | 81 % | 0 | 100 % | -1,16 | -1,88 … -0,46 | 19,4 (12 … 24) |
| 50.000 | 100 % | 0 | 100 % | -1,18 | -1,46 … -0,91 | 23,2 (19 … 26) |
| 100.000 | 100 % | 0 | 100 % | -1,17 | -1,35 … -0,98 | 23,8 (21 … 26) |

### schildtraeger_dmg_-2.5_uniform

Alle 257.040 Partien: ΔST -1,72 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 25 % | 0 | 99 % | -1,76 | -3,59 … +0,00 | 9,5 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | -1,70 | -2,26 … -1,20 | 21,5 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,70 | -1,94 … -1,50 | 26,9 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,71 | -1,86 … -1,56 | 27,8 (27 … 28) |

### schildtraeger_dmg_-5_aggressive

Alle 257.040 Partien: ΔST -3,23 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 64 % | 0 | 99 % | -3,29 | -7,23 … +0,00 | 12,7 (5 … 20) |
| 10.000 | 100 % | 0 | 100 % | -3,23 | -4,22 … -2,36 | 23,5 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | -3,21 | -3,45 … -2,84 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,23 | -3,40 … -2,98 | 27,7 (26 … 28) |

### schildtraeger_dmg_-5_byactiontype

Alle 257.040 Partien: ΔST -2,53 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 49 % | 0 | 100 % | -2,53 | -5,67 … -0,44 | 11,6 (7 … 19) |
| 10.000 | 100 % | 0 | 100 % | -2,48 | -3,21 … -1,70 | 23,5 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,55 | -2,85 … -2,22 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,53 | -2,72 … -2,29 | 28,0 (28 … 28) |

### schildtraeger_dmg_-5_defensive

Alle 257.040 Partien: ΔST -1,72 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 15 % | 0 | 95 % | -1,66 | -4,25 … +0,88 | 11,3 (6 … 16) |
| 10.000 | 94 % | 0 | 100 % | -1,72 | -2,71 … -0,98 | 21,1 (16 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,74 | -2,05 … -1,44 | 25,1 (22 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,72 | -1,92 … -1,54 | 26,2 (23 … 28) |

### schildtraeger_dmg_-5_uniform

Alle 257.040 Partien: ΔST -2,68 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 46 % | 0 | 99 % | -2,71 | -5,06 … +0,00 | 12,1 (6 … 17) |
| 10.000 | 100 % | 0 | 100 % | -2,64 | -3,43 … -1,93 | 23,6 (20 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,66 | -2,99 … -2,40 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,68 | -2,88 … -2,48 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_aggressive

Alle 257.040 Partien: ΔST +2,54 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 46 % | 0 | 100 % | +2,60 | +0,24 … +5,34 | 11,5 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,54 | +1,77 … +3,29 | 22,5 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | +2,55 | +2,21 … +2,84 | 26,7 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,54 | +2,35 … +2,74 | 27,2 (26 … 28) |

### schildtraeger_dmg_10_byactiontype

Alle 257.040 Partien: ΔST +2,82 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 50 % | 0 | 100 % | +2,83 | +0,24 … +5,20 | 12,4 (8 … 18) |
| 10.000 | 100 % | 0 | 100 % | +2,86 | +2,25 … +3,82 | 24,4 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,83 | +2,51 … +3,21 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,83 | +2,58 … +3,13 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_defensive

Alle 257.040 Partien: ΔST +1,66 pp, Kompositionen in Richtung / dagegen 24 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 14 % | 0 | 90 % | +1,60 | -2,11 … +4,33 | 11,7 (5 … 17) |
| 10.000 | 88 % | 0 | 100 % | +1,64 | +0,71 … +2,23 | 19,8 (14 … 24) |
| 50.000 | 100 % | 0 | 100 % | +1,68 | +1,34 … +2,03 | 23,5 (20 … 27) |
| 100.000 | 100 % | 0 | 100 % | +1,66 | +1,47 … +1,84 | 24,4 (22 … 26) |

### schildtraeger_dmg_10_uniform

Alle 257.040 Partien: ΔST +2,99 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 50 % | 0 | 100 % | +3,25 | +0,83 … +6,72 | 13,0 (8 … 20) |
| 10.000 | 100 % | 0 | 100 % | +3,06 | +2,14 … +3,83 | 24,4 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | +3,01 | +2,73 … +3,34 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +3,01 | +2,77 … +3,20 | 28,0 (27 … 28) |

### schildtraeger_dmg_2.5_aggressive

Alle 257.040 Partien: ΔST +0,39 pp, Kompositionen in Richtung / dagegen 19 / 8, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 37 | 77 % | +0,44 | -0,47 … +1,61 | 3,6 (0 … 7) |
| 10.000 | 25 % | 0 | 100 % | +0,39 | +0,02 … +0,68 | 12,8 (7 … 17) |
| 50.000 | 52 % | 0 | 100 % | +0,39 | +0,29 … +0,53 | 17,2 (13 … 21) |
| 100.000 | 71 % | 0 | 100 % | +0,40 | +0,33 … +0,49 | 18,7 (16 … 23) |

### schildtraeger_dmg_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,64 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 7 % | 19 | 92 % | +0,66 | -0,70 … +2,00 | 4,6 (1 … 10) |
| 10.000 | 83 % | 0 | 100 % | +0,66 | +0,25 … +1,12 | 16,8 (11 … 25) |
| 50.000 | 100 % | 0 | 100 % | +0,64 | +0,52 … +0,78 | 24,6 (19 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,65 | +0,54 … +0,74 | 26,8 (23 … 28) |

### schildtraeger_dmg_2.5_defensive

Alle 257.040 Partien: ΔST +0,52 pp, Kompositionen in Richtung / dagegen 20 / 8, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 0 | 71 % | +0,50 | -1,43 … +2,80 | 8,2 (3 … 12) |
| 10.000 | 14 % | 0 | 98 % | +0,54 | -0,07 … +1,13 | 15,6 (10 … 22) |
| 50.000 | 40 % | 0 | 100 % | +0,53 | +0,27 … +0,84 | 18,0 (13 … 23) |
| 100.000 | 61 % | 0 | 100 % | +0,52 | +0,29 … +0,66 | 18,9 (16 … 22) |

### schildtraeger_dmg_2.5_uniform

Alle 257.040 Partien: ΔST +0,69 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 8 | 88 % | +0,71 | -0,42 … +1,87 | 5,1 (1 … 9) |
| 10.000 | 83 % | 0 | 100 % | +0,70 | +0,21 … +1,10 | 16,9 (12 … 24) |
| 50.000 | 100 % | 0 | 100 % | +0,70 | +0,52 … +0,86 | 25,1 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,69 | +0,61 … +0,78 | 26,7 (25 … 28) |

### schildtraeger_dmg_5_aggressive

Alle 257.040 Partien: ΔST +1,50 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 24 % | 0 | 95 % | +1,53 | -0,71 … +3,18 | 8,8 (3 … 14) |
| 10.000 | 97 % | 0 | 100 % | +1,47 | +0,66 … +2,02 | 19,8 (15 … 23) |
| 50.000 | 100 % | 0 | 100 % | +1,50 | +1,30 … +1,74 | 25,5 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,50 | +1,31 … +1,66 | 26,8 (24 … 28) |

### schildtraeger_dmg_5_byactiontype

Alle 257.040 Partien: ΔST +1,47 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 25 % | 0 | 98 % | +1,47 | -0,23 … +4,00 | 8,8 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | +1,49 | +0,91 … +2,11 | 21,0 (17 … 25) |
| 50.000 | 100 % | 0 | 100 % | +1,47 | +1,26 … +1,67 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,47 | +1,31 … +1,61 | 27,8 (27 … 28) |

### schildtraeger_dmg_5_defensive

Alle 257.040 Partien: ΔST +0,97 pp, Kompositionen in Richtung / dagegen 20 / 8, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 0 | 81 % | +0,89 | -1,89 … +3,31 | 9,9 (3 … 14) |
| 10.000 | 38 % | 0 | 100 % | +0,97 | +0,28 … +1,64 | 17,3 (13 … 22) |
| 50.000 | 96 % | 0 | 100 % | +1,00 | +0,68 … +1,29 | 20,7 (17 … 24) |
| 100.000 | 99 % | 0 | 100 % | +0,96 | +0,75 … +1,12 | 21,4 (18 … 24) |

### schildtraeger_dmg_5_uniform

Alle 257.040 Partien: ΔST +1,60 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 28 % | 0 | 97 % | +1,75 | -0,23 … +4,02 | 9,4 (5 … 14) |
| 10.000 | 99 % | 0 | 100 % | +1,67 | +0,96 … +2,22 | 21,5 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,61 | +1,31 … +1,87 | 27,2 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,60 | +1,45 … +1,75 | 27,8 (27 … 28) |

### schildtraeger_hp_-10_aggressive

Alle 257.040 Partien: ΔST -5,73 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 99 % | 0 | 100 % | -5,81 | -8,71 … -3,15 | 16,0 (9 … 22) |
| 10.000 | 100 % | 0 | 100 % | -5,72 | -6,86 … -4,79 | 27,4 (25 … 28) |
| 50.000 | 100 % | 0 | 100 % | -5,74 | -6,07 … -5,38 | 28,0 (28 … 28) |
| 100.000 | 100 % | 0 | 100 % | -5,74 | -5,99 … -5,55 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_byactiontype

Alle 257.040 Partien: ΔST -2,97 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 62 % | 0 | 100 % | -2,92 | -5,54 … -0,23 | 12,4 (6 … 18) |
| 10.000 | 100 % | 0 | 100 % | -2,98 | -3,67 … -2,35 | 25,3 (22 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,96 | -3,27 … -2,73 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,97 | -3,13 … -2,71 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_defensive

Alle 257.040 Partien: ΔST -2,67 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 69 % | 0 | 100 % | -2,67 | -4,31 … -0,23 | 10,7 (6 … 15) |
| 10.000 | 100 % | 0 | 100 % | -2,66 | -3,13 … -2,00 | 24,8 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,68 | -3,03 … -2,41 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,68 | -2,90 … -2,50 | 28,0 (27 … 28) |

### schildtraeger_hp_-10_uniform

Alle 257.040 Partien: ΔST -3,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 69 % | 0 | 100 % | -3,05 | -5,72 … -0,48 | 12,4 (5 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,22 | -3,74 … -2,55 | 25,4 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -3,20 | -3,53 … -2,90 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,20 | -3,36 … -3,02 | 28,0 (28 … 28) |

### schildtraeger_hp_-2.5_aggressive

Alle 257.040 Partien: ΔST -1,43 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 30 % | 2 | 100 % | -1,39 | -3,23 … -0,24 | 6,9 (2 … 14) |
| 10.000 | 100 % | 0 | 100 % | -1,41 | -1,86 … -0,96 | 21,9 (15 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,44 | -1,67 … -1,22 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,44 | -1,55 … -1,28 | 27,8 (26 … 28) |

### schildtraeger_hp_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,71 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 13 | 87 % | -0,66 | -1,89 … +0,50 | 4,8 (1 … 9) |
| 10.000 | 89 % | 0 | 100 % | -0,70 | -1,02 … -0,32 | 17,4 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | -0,70 | -0,88 … -0,55 | 25,2 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,70 | -0,79 … -0,61 | 27,4 (25 … 28) |

### schildtraeger_hp_-2.5_defensive

Alle 257.040 Partien: ΔST -0,65 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 24 | 90 % | -0,63 | -1,84 … +0,70 | 4,4 (0 … 8) |
| 10.000 | 77 % | 0 | 100 % | -0,64 | -0,98 … -0,30 | 15,7 (10 … 21) |
| 50.000 | 100 % | 0 | 100 % | -0,64 | -0,80 … -0,50 | 23,2 (19 … 27) |
| 100.000 | 100 % | 0 | 100 % | -0,65 | -0,76 … -0,56 | 25,4 (23 … 28) |

### schildtraeger_hp_-2.5_uniform

Alle 257.040 Partien: ΔST -0,68 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 20 | 82 % | -0,57 | -1,67 … +0,68 | 4,5 (1 … 9) |
| 10.000 | 91 % | 0 | 100 % | -0,69 | -1,01 … -0,34 | 17,1 (12 … 22) |
| 50.000 | 100 % | 0 | 100 % | -0,68 | -0,87 … -0,49 | 24,7 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,68 | -0,78 … -0,58 | 26,9 (25 … 28) |

### schildtraeger_hp_-5_aggressive

Alle 257.040 Partien: ΔST -2,72 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 69 % | 0 | 100 % | -2,76 | -4,57 … -0,68 | 10,6 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | -2,67 | -3,35 … -2,11 | 25,1 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,73 | -2,98 … -2,55 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,72 | -2,93 … -2,54 | 28,0 (28 … 28) |

### schildtraeger_hp_-5_byactiontype

Alle 257.040 Partien: ΔST -1,46 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 17 % | 1 | 96 % | -1,37 | -3,08 … +0,45 | 8,0 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | -1,47 | -1,96 … -1,14 | 21,8 (18 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,46 | -1,66 … -1,25 | 27,3 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,45 | -1,57 … -1,29 | 27,9 (27 … 28) |

### schildtraeger_hp_-5_defensive

Alle 257.040 Partien: ΔST -1,30 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 21 % | 3 | 95 % | -1,23 | -2,69 … +0,23 | 6,9 (2 … 12) |
| 10.000 | 100 % | 0 | 100 % | -1,29 | -1,65 … -0,75 | 20,7 (15 … 27) |
| 50.000 | 100 % | 0 | 100 % | -1,30 | -1,48 … -1,09 | 26,8 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,30 | -1,45 … -1,15 | 27,7 (26 … 28) |

### schildtraeger_hp_-5_uniform

Alle 257.040 Partien: ΔST -1,48 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 20 % | 1 | 95 % | -1,33 | -2,76 … +0,24 | 7,9 (4 … 12) |
| 10.000 | 100 % | 0 | 100 % | -1,47 | -1,93 … -0,86 | 21,3 (17 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,48 | -1,71 … -1,23 | 27,2 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,47 | -1,60 … -1,36 | 27,9 (27 … 28) |

### schildtraeger_hp_10_aggressive

Alle 257.040 Partien: ΔST +5,21 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 98 % | 0 | 100 % | +5,13 | +2,86 … +8,92 | 15,2 (11 … 21) |
| 10.000 | 100 % | 0 | 100 % | +5,22 | +4,30 … +5,91 | 27,0 (23 … 28) |
| 50.000 | 100 % | 0 | 100 % | +5,22 | +4,89 … +5,54 | 28,0 (28 … 28) |
| 100.000 | 100 % | 0 | 100 % | +5,22 | +4,99 … +5,45 | 28,0 (28 … 28) |

### schildtraeger_hp_10_byactiontype

Alle 257.040 Partien: ΔST +2,62 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 57 % | 0 | 99 % | +2,50 | -0,71 … +5,07 | 10,8 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,67 | +1,82 … +3,45 | 24,9 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,61 | +2,35 … +2,82 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,61 | +2,45 … +2,84 | 28,0 (27 … 28) |

### schildtraeger_hp_10_defensive

Alle 257.040 Partien: ΔST +2,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 50 % | 0 | 100 % | +2,24 | +0,24 … +3,79 | 9,6 (3 … 15) |
| 10.000 | 100 % | 0 | 100 % | +2,17 | +1,49 … +2,93 | 23,0 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,21 | +1,95 … +2,43 | 27,2 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,21 | +2,06 … +2,39 | 27,8 (27 … 28) |

### schildtraeger_hp_10_uniform

Alle 257.040 Partien: ΔST +2,72 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 66 % | 0 | 100 % | +2,87 | +0,23 … +6,00 | 11,6 (7 … 18) |
| 10.000 | 100 % | 0 | 100 % | +2,71 | +2,12 … +3,36 | 25,2 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,72 | +2,40 … +3,02 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,74 | +2,56 … +2,91 | 28,0 (27 … 28) |

### schildtraeger_hp_2.5_aggressive

Alle 257.040 Partien: ΔST +1,60 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 40 % | 2 | 100 % | +1,58 | +0,44 … +3,37 | 7,9 (4 … 13) |
| 10.000 | 100 % | 0 | 100 % | +1,61 | +1,17 … +2,02 | 22,4 (19 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,59 | +1,36 … +1,93 | 27,1 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,60 | +1,47 … +1,74 | 27,8 (26 … 28) |

### schildtraeger_hp_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,70 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 8 % | 12 | 82 % | +0,61 | -0,48 … +1,67 | 5,0 (1 … 9) |
| 10.000 | 89 % | 0 | 100 % | +0,73 | +0,39 … +1,11 | 17,5 (12 … 21) |
| 50.000 | 100 % | 0 | 100 % | +0,71 | +0,52 … +0,86 | 25,0 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,69 | +0,60 … +0,82 | 26,9 (22 … 28) |

### schildtraeger_hp_2.5_defensive

Alle 257.040 Partien: ΔST +0,63 pp, Kompositionen in Richtung / dagegen 26 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 24 | 85 % | +0,67 | -0,47 … +1,92 | 4,4 (1 … 10) |
| 10.000 | 85 % | 0 | 100 % | +0,66 | +0,25 … +0,99 | 16,1 (11 … 23) |
| 50.000 | 100 % | 0 | 100 % | +0,63 | +0,50 … +0,79 | 23,3 (19 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,64 | +0,56 … +0,76 | 25,4 (23 … 28) |

### schildtraeger_hp_2.5_uniform

Alle 257.040 Partien: ΔST +0,73 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 15 | 88 % | +0,70 | -0,48 … +2,06 | 5,1 (0 … 9) |
| 10.000 | 90 % | 0 | 100 % | +0,72 | +0,43 … +1,19 | 17,4 (14 … 22) |
| 50.000 | 100 % | 0 | 100 % | +0,74 | +0,60 … +0,92 | 24,9 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,73 | +0,63 … +0,84 | 27,1 (24 … 28) |

### schildtraeger_hp_5_aggressive

Alle 257.040 Partien: ΔST +2,73 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 73 % | 0 | 100 % | +2,74 | +1,05 … +5,30 | 11,0 (6 … 17) |
| 10.000 | 100 % | 0 | 100 % | +2,74 | +2,16 … +3,36 | 24,8 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,73 | +2,51 … +3,02 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,73 | +2,60 … +2,91 | 27,9 (27 … 28) |

### schildtraeger_hp_5_byactiontype

Alle 257.040 Partien: ΔST +1,34 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 17 % | 0 | 93 % | +1,21 | -0,47 … +3,69 | 7,6 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | +1,37 | +0,85 … +1,92 | 21,2 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,34 | +1,13 … +1,49 | 27,0 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,33 | +1,20 … +1,51 | 27,9 (26 … 28) |

### schildtraeger_hp_5_defensive

Alle 257.040 Partien: ΔST +1,16 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 23 % | 3 | 95 % | +1,18 | -0,23 … +2,59 | 6,6 (2 … 10) |
| 10.000 | 100 % | 0 | 100 % | +1,16 | +0,64 … +1,64 | 19,6 (14 … 24) |
| 50.000 | 100 % | 0 | 100 % | +1,15 | +0,98 … +1,35 | 26,2 (22 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,16 | +1,03 … +1,26 | 27,4 (25 … 28) |

### schildtraeger_hp_5_uniform

Alle 257.040 Partien: ΔST +1,39 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 28 % | 1 | 98 % | +1,46 | -0,69 … +3,11 | 8,0 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | +1,40 | +0,90 … +2,11 | 21,2 (13 … 27) |
| 50.000 | 100 % | 0 | 100 % | +1,41 | +1,21 … +1,62 | 27,0 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,40 | +1,22 … +1,57 | 27,9 (26 … 28) |

