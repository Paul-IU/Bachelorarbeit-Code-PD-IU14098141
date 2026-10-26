# Simulationsumfang über Teilmengen (Schritt 6)

Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: `numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.

Unentschieden zählen als halber Sieg. Erkannt = (a) Siegquote des Schildträgers in Richtung der Manipulation UND (b) einseitiger Vorzeichentest (α = 0,05) über die veränderten Kompositionen mit Schildträger. ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp); nur (a) = Anteil der Wiederholungen mit richtigem Vorzeichen von ΔST; Test unmöglich = Wiederholungen mit weniger als 5 veränderten Kompositionen.

## Übersicht: Anteil erkannter Wiederholungen und kleinste Stichprobe mit ≥ 95 %

In Klammern: Wiederholungen, in denen der Vorzeichentest mangels veränderter Kompositionen nicht anschlagen konnte (wenn > 0).

| Variante | voller Lauf | 1.000 | 10.000 | 50.000 | 100.000 | ab |
|---|---|---|---|---|---|---|
| schildtraeger_ability_-10_aggressive | nicht erkannt | 0 % | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_-10_byactiontype | erkannt | 1 % (65) | 32 % | 88 % | 100 % | 100.000 |
| schildtraeger_ability_-10_defensive | erkannt | 5 % | 32 % | 97 % | 99 % | 50.000 |
| schildtraeger_ability_-10_uniform | erkannt | 1 % (30) | 64 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-2.5_aggressive | nicht erkannt | 0 % (84) | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_-2.5_byactiontype | erkannt | 0 % (99) | 1 % (27) | 16 % | 25 % | nie |
| schildtraeger_ability_-2.5_defensive | nicht erkannt | 0 % | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_-2.5_uniform | erkannt | 0 % (95) | 6 % (6) | 51 % | 89 % | nie |
| schildtraeger_ability_-5_aggressive | nicht erkannt | 0 % (26) | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_-5_byactiontype | erkannt | 0 % (93) | 9 % | 48 % | 84 % | nie |
| schildtraeger_ability_-5_defensive | erkannt | 1 % | 8 % | 24 % | 45 % | nie |
| schildtraeger_ability_-5_uniform | erkannt | 0 % (70) | 32 % | 94 % | 100 % | 100.000 |
| schildtraeger_ability_10_aggressive | nicht erkannt | 0 % | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_10_byactiontype | erkannt | 3 % (33) | 28 % | 96 % | 100 % | 50.000 |
| schildtraeger_ability_10_defensive | erkannt | 0 % | 16 % | 76 % | 87 % | nie |
| schildtraeger_ability_10_uniform | erkannt | 4 % (13) | 66 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_2.5_aggressive | nicht erkannt | 0 % (50) | 1 % | 1 % | 0 % | nie |
| schildtraeger_ability_2.5_byactiontype | erkannt | 2 % (79) | 8 % | 52 % | 73 % | nie |
| schildtraeger_ability_2.5_defensive | nicht erkannt | 0 % | 1 % | 2 % | 0 % | nie |
| schildtraeger_ability_2.5_uniform | erkannt | 0 % (76) | 15 % | 87 % | 99 % | 100.000 |
| schildtraeger_ability_5_aggressive | nicht erkannt | 0 % (12) | 2 % | 0 % | 0 % | nie |
| schildtraeger_ability_5_byactiontype | erkannt | 2 % (61) | 12 % | 83 % | 99 % | 100.000 |
| schildtraeger_ability_5_defensive | nicht erkannt | 1 % | 3 % | 9 % | 9 % | nie |
| schildtraeger_ability_5_uniform | erkannt | 1 % (39) | 37 % | 99 % | 100 % | 50.000 |
| schildtraeger_dmg_-10_aggressive | erkannt | 88 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_byactiontype | erkannt | 58 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_defensive | erkannt | 21 % | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_uniform | erkannt | 70 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_aggressive | erkannt | 28 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_byactiontype | erkannt | 26 % | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_defensive | erkannt | 8 % | 72 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_-2.5_uniform | erkannt | 26 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_aggressive | erkannt | 55 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_byactiontype | erkannt | 35 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_defensive | erkannt | 14 % | 88 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_-5_uniform | erkannt | 39 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_aggressive | erkannt | 41 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_byactiontype | erkannt | 50 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_defensive | erkannt | 19 % | 81 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_10_uniform | erkannt | 56 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_aggressive | erkannt | 5 % (42) | 35 % | 67 % | 79 % | nie |
| schildtraeger_dmg_2.5_byactiontype | erkannt | 2 % (20) | 81 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_2.5_defensive | nicht erkannt | 4 % | 10 % | 29 % | 31 % | nie |
| schildtraeger_dmg_2.5_uniform | erkannt | 4 % (15) | 83 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_5_aggressive | erkannt | 16 % | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_5_byactiontype | erkannt | 18 % | 97 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_defensive | erkannt | 3 % | 38 % | 96 % | 99 % | 50.000 |
| schildtraeger_dmg_5_uniform | erkannt | 19 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_aggressive | erkannt | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_byactiontype | erkannt | 60 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_defensive | erkannt | 66 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_uniform | erkannt | 62 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_aggressive | erkannt | 24 % (2) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_byactiontype | erkannt | 4 % (20) | 85 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_defensive | erkannt | 5 % (31) | 79 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_uniform | erkannt | 6 % (9) | 86 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-5_aggressive | erkannt | 67 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_byactiontype | erkannt | 18 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_defensive | erkannt | 22 % (3) | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_uniform | erkannt | 23 % | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_aggressive | erkannt | 94 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_byactiontype | erkannt | 61 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_defensive | erkannt | 49 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_uniform | erkannt | 63 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_aggressive | erkannt | 35 % (2) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_byactiontype | erkannt | 9 % (11) | 84 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_defensive | erkannt | 6 % (27) | 92 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_uniform | erkannt | 11 % (13) | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_5_aggressive | erkannt | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_byactiontype | erkannt | 21 % (1) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_defensive | erkannt | 18 % (4) | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_uniform | erkannt | 32 % | 100 % | 100 % | 100 % | 10.000 |

## Einzelwerte je Variante

### schildtraeger_ability_-10_aggressive

Alle 257.040 Partien: ΔST +0,57 pp, Kompositionen in Richtung / dagegen 5 / 23, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 21 % | +0,50 | -0,95 … +3,18 | 4,7 (1 … 8) |
| 10.000 | 0 % | 0 | 1 % | +0,58 | -0,21 … +1,05 | 8,0 (4 … 15) |
| 50.000 | 0 % | 0 | 0 % | +0,58 | +0,32 … +0,80 | 6,3 (3 … 13) |
| 100.000 | 0 % | 0 | 0 % | +0,56 | +0,43 … +0,70 | 5,6 (3 … 9) |

### schildtraeger_ability_-10_byactiontype

Alle 257.040 Partien: ΔST -0,25 pp, Kompositionen in Richtung / dagegen 25 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 65 | 56 % | -0,19 | -1,36 … +0,46 | 2,2 (0 … 5) |
| 10.000 | 32 % | 0 | 99 % | -0,24 | -0,59 … +0,02 | 12,6 (7 … 17) |
| 50.000 | 88 % | 0 | 100 % | -0,26 | -0,35 … -0,16 | 19,0 (14 … 23) |
| 100.000 | 100 % | 0 | 100 % | -0,25 | -0,34 … -0,19 | 21,9 (17 … 25) |

### schildtraeger_ability_-10_defensive

Alle 257.040 Partien: ΔST -0,96 pp, Kompositionen in Richtung / dagegen 23 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 0 | 79 % | -1,00 | -3,30 … +1,15 | 10,2 (5 … 16) |
| 10.000 | 32 % | 0 | 100 % | -0,97 | -1,78 … -0,05 | 17,2 (12 … 21) |
| 50.000 | 97 % | 0 | 100 % | -0,97 | -1,30 … -0,50 | 20,8 (17 … 24) |
| 100.000 | 99 % | 0 | 100 % | -0,96 | -1,23 … -0,76 | 21,9 (18 … 25) |

### schildtraeger_ability_-10_uniform

Alle 257.040 Partien: ΔST -0,45 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 30 | 71 % | -0,43 | -1,38 … +0,50 | 3,8 (0 … 9) |
| 10.000 | 64 % | 0 | 99 % | -0,46 | -0,97 … +0,09 | 15,3 (11 … 21) |
| 50.000 | 100 % | 0 | 100 % | -0,45 | -0,58 … -0,31 | 22,7 (18 … 26) |
| 100.000 | 100 % | 0 | 100 % | -0,45 | -0,54 … -0,38 | 25,8 (23 … 28) |

### schildtraeger_ability_-2.5_aggressive

Alle 257.040 Partien: ΔST +0,05 pp, Kompositionen in Richtung / dagegen 13 / 13, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 84 | 30 % | +0,04 | -0,91 … +0,70 | 1,4 (0 … 5) |
| 10.000 | 0 % | 0 | 28 % | +0,04 | -0,16 … +0,27 | 7,6 (3 … 12) |
| 50.000 | 0 % | 0 | 8 % | +0,05 | -0,05 … +0,12 | 11,1 (6 … 16) |
| 100.000 | 0 % | 0 | 0 % | +0,05 | +0,01 … +0,12 | 11,8 (6 … 17) |

### schildtraeger_ability_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,04 pp, Kompositionen in Richtung / dagegen 19 / 7, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 99 | 15 % | -0,02 | -0,48 … +0,25 | 0,5 (0 … 3) |
| 10.000 | 1 % | 27 | 69 % | -0,03 | -0,16 … +0,07 | 3,6 (0 … 9) |
| 50.000 | 16 % | 0 | 100 % | -0,04 | -0,08 … -0,01 | 11,3 (6 … 16) |
| 100.000 | 25 % | 0 | 100 % | -0,04 | -0,07 … -0,01 | 13,7 (10 … 18) |

### schildtraeger_ability_-2.5_defensive

Alle 257.040 Partien: ΔST +0,12 pp, Kompositionen in Richtung / dagegen 16 / 12, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 40 % | +0,12 | -1,93 … +2,24 | 6,7 (3 … 13) |
| 10.000 | 1 % | 0 | 33 % | +0,09 | -0,42 … +0,73 | 12,3 (6 … 18) |
| 50.000 | 0 % | 0 | 8 % | +0,13 | -0,08 … +0,40 | 12,8 (8 … 17) |
| 100.000 | 0 % | 0 | 4 % | +0,11 | -0,03 … +0,23 | 13,8 (10 … 18) |

### schildtraeger_ability_-2.5_uniform

Alle 257.040 Partien: ΔST -0,08 pp, Kompositionen in Richtung / dagegen 22 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 95 | 37 % | -0,10 | -0,91 … +0,24 | 0,9 (0 … 4) |
| 10.000 | 6 % | 6 | 91 % | -0,08 | -0,32 … +0,07 | 5,8 (1 … 11) |
| 50.000 | 51 % | 0 | 100 % | -0,07 | -0,12 … -0,02 | 14,3 (11 … 18) |
| 100.000 | 89 % | 0 | 100 % | -0,08 | -0,11 … -0,05 | 17,9 (14 … 22) |

### schildtraeger_ability_-5_aggressive

Alle 257.040 Partien: ΔST +0,07 pp, Kompositionen in Richtung / dagegen 8 / 20, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 26 | 40 % | +0,02 | -1,43 … +1,39 | 3,1 (0 … 7) |
| 10.000 | 1 % | 0 | 26 % | +0,07 | -0,39 … +0,58 | 9,5 (4 … 17) |
| 50.000 | 0 % | 0 | 7 % | +0,07 | -0,06 … +0,20 | 10,4 (7 … 15) |
| 100.000 | 0 % | 0 | 3 % | +0,06 | -0,02 … +0,14 | 10,2 (7 … 14) |

### schildtraeger_ability_-5_byactiontype

Alle 257.040 Partien: ΔST -0,10 pp, Kompositionen in Richtung / dagegen 22 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 93 | 33 % | -0,06 | -0,91 … +0,46 | 1,2 (0 … 4) |
| 10.000 | 9 % | 0 | 90 % | -0,10 | -0,30 … +0,05 | 8,2 (3 … 15) |
| 50.000 | 48 % | 0 | 100 % | -0,11 | -0,18 … -0,05 | 15,4 (11 … 19) |
| 100.000 | 84 % | 0 | 100 % | -0,10 | -0,16 … -0,06 | 18,6 (14 … 24) |

### schildtraeger_ability_-5_defensive

Alle 257.040 Partien: ΔST -0,40 pp, Kompositionen in Richtung / dagegen 19 / 9, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 65 % | -0,45 | -2,61 … +1,53 | 8,1 (4 … 13) |
| 10.000 | 8 % | 0 | 92 % | -0,41 | -1,07 … +0,16 | 14,9 (9 … 20) |
| 50.000 | 24 % | 0 | 100 % | -0,39 | -0,63 … -0,05 | 17,2 (12 … 22) |
| 100.000 | 45 % | 0 | 100 % | -0,41 | -0,57 … -0,24 | 18,3 (15 … 23) |

### schildtraeger_ability_-5_uniform

Alle 257.040 Partien: ΔST -0,22 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 70 | 64 % | -0,24 | -0,93 … +0,66 | 2,2 (0 … 7) |
| 10.000 | 32 % | 0 | 99 % | -0,22 | -0,54 … +0,00 | 11,1 (6 … 16) |
| 50.000 | 94 % | 0 | 100 % | -0,21 | -0,30 … -0,10 | 18,8 (14 … 23) |
| 100.000 | 100 % | 0 | 100 % | -0,22 | -0,26 … -0,15 | 22,3 (18 … 25) |

### schildtraeger_ability_10_aggressive

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung / dagegen 9 / 18, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 30 % | -0,25 | -3,12 … +1,65 | 5,3 (1 … 11) |
| 10.000 | 1 % | 0 | 21 % | -0,21 | -0,93 … +0,46 | 10,6 (6 … 18) |
| 50.000 | 0 % | 0 | 0 % | -0,24 | -0,47 … -0,01 | 9,8 (5 … 14) |
| 100.000 | 0 % | 0 | 0 % | -0,22 | -0,36 … -0,04 | 9,4 (5 … 12) |

### schildtraeger_ability_10_byactiontype

Alle 257.040 Partien: ΔST +0,33 pp, Kompositionen in Richtung / dagegen 25 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 33 | 62 % | +0,29 | -0,86 … +1,62 | 3,4 (0 … 7) |
| 10.000 | 28 % | 0 | 97 % | +0,32 | +0,00 … +0,71 | 13,7 (9 … 18) |
| 50.000 | 96 % | 0 | 100 % | +0,34 | +0,21 … +0,49 | 20,4 (14 … 25) |
| 100.000 | 100 % | 0 | 100 % | +0,33 | +0,25 … +0,41 | 22,9 (19 … 26) |

### schildtraeger_ability_10_defensive

Alle 257.040 Partien: ΔST +0,77 pp, Kompositionen in Richtung / dagegen 19 / 9, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 75 % | +0,69 | -1,33 … +2,74 | 9,8 (5 … 15) |
| 10.000 | 16 % | 0 | 100 % | +0,76 | +0,02 … +1,72 | 16,2 (11 … 20) |
| 50.000 | 76 % | 0 | 100 % | +0,76 | +0,50 … +1,16 | 19,5 (13 … 23) |
| 100.000 | 87 % | 0 | 100 % | +0,77 | +0,50 … +0,95 | 20,1 (17 … 23) |

### schildtraeger_ability_10_uniform

Alle 257.040 Partien: ΔST +0,64 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 13 | 83 % | +0,60 | -0,45 … +1,77 | 4,7 (1 … 9) |
| 10.000 | 66 % | 0 | 100 % | +0,61 | +0,23 … +0,97 | 16,4 (11 … 21) |
| 50.000 | 100 % | 0 | 100 % | +0,65 | +0,50 … +0,80 | 24,0 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,64 | +0,53 … +0,73 | 26,3 (23 … 28) |

### schildtraeger_ability_2.5_aggressive

Alle 257.040 Partien: ΔST +0,05 pp, Kompositionen in Richtung / dagegen 14 / 13, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 50 | 48 % | +0,12 | -0,75 … +1,36 | 2,6 (0 … 7) |
| 10.000 | 1 % | 0 | 54 % | +0,04 | -0,35 … +0,38 | 9,7 (6 … 15) |
| 50.000 | 1 % | 0 | 80 % | +0,04 | -0,04 … +0,18 | 13,2 (8 … 18) |
| 100.000 | 0 % | 0 | 94 % | +0,05 | -0,02 … +0,12 | 14,0 (10 … 18) |

### schildtraeger_ability_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,12 pp, Kompositionen in Richtung / dagegen 22 / 5, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 79 | 47 % | +0,11 | -0,71 … +1,49 | 1,8 (0 … 7) |
| 10.000 | 8 % | 0 | 91 % | +0,12 | -0,14 … +0,31 | 9,7 (3 … 13) |
| 50.000 | 52 % | 0 | 99 % | +0,13 | -0,01 … +0,21 | 16,2 (12 … 21) |
| 100.000 | 73 % | 0 | 100 % | +0,12 | +0,07 … +0,19 | 18,2 (13 … 23) |

### schildtraeger_ability_2.5_defensive

Alle 257.040 Partien: ΔST -0,02 pp, Kompositionen in Richtung / dagegen 12 / 15, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 47 % | -0,03 | -2,23 … +1,74 | 6,7 (1 … 11) |
| 10.000 | 1 % | 0 | 49 % | -0,01 | -0,66 … +0,67 | 12,4 (7 … 19) |
| 50.000 | 2 % | 0 | 39 % | -0,02 | -0,26 … +0,20 | 13,7 (9 … 19) |
| 100.000 | 0 % | 0 | 38 % | -0,02 | -0,20 … +0,11 | 13,8 (10 … 17) |

### schildtraeger_ability_2.5_uniform

Alle 257.040 Partien: ΔST +0,19 pp, Kompositionen in Richtung / dagegen 24 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 76 | 54 % | +0,17 | -0,47 … +1,10 | 2,1 (0 … 5) |
| 10.000 | 15 % | 0 | 98 % | +0,19 | -0,02 … +0,43 | 11,2 (6 … 16) |
| 50.000 | 87 % | 0 | 100 % | +0,19 | +0,08 … +0,30 | 18,3 (13 … 22) |
| 100.000 | 99 % | 0 | 100 % | +0,19 | +0,11 … +0,25 | 21,1 (16 … 25) |

### schildtraeger_ability_5_aggressive

Alle 257.040 Partien: ΔST -0,02 pp, Kompositionen in Richtung / dagegen 10 / 18, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 12 | 36 % | +0,02 | -1,68 … +2,05 | 4,0 (0 … 9) |
| 10.000 | 2 % | 0 | 48 % | -0,01 | -0,64 … +0,52 | 10,9 (6 … 17) |
| 50.000 | 0 % | 0 | 35 % | -0,02 | -0,20 … +0,18 | 11,8 (6 … 16) |
| 100.000 | 0 % | 0 | 34 % | -0,01 | -0,10 … +0,10 | 11,2 (8 … 16) |

### schildtraeger_ability_5_byactiontype

Alle 257.040 Partien: ΔST +0,20 pp, Kompositionen in Richtung / dagegen 25 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 61 | 55 % | +0,18 | -0,86 … +1,28 | 2,4 (0 … 6) |
| 10.000 | 12 % | 0 | 97 % | +0,20 | -0,09 … +0,43 | 11,6 (7 … 17) |
| 50.000 | 83 % | 0 | 100 % | +0,20 | +0,05 … +0,32 | 18,4 (13 … 22) |
| 100.000 | 99 % | 0 | 100 % | +0,20 | +0,14 … +0,29 | 20,8 (17 … 26) |

### schildtraeger_ability_5_defensive

Alle 257.040 Partien: ΔST +0,19 pp, Kompositionen in Richtung / dagegen 16 / 11, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 56 % | +0,16 | -2,14 … +2,13 | 8,1 (3 … 12) |
| 10.000 | 3 % | 0 | 73 % | +0,19 | -0,43 … +0,95 | 13,7 (8 … 20) |
| 50.000 | 9 % | 0 | 95 % | +0,19 | -0,13 … +0,44 | 15,7 (10 … 21) |
| 100.000 | 9 % | 0 | 98 % | +0,19 | -0,04 … +0,36 | 16,7 (12 … 20) |

### schildtraeger_ability_5_uniform

Alle 257.040 Partien: ΔST +0,34 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 39 | 74 % | +0,34 | -0,47 … +1,35 | 3,2 (0 … 8) |
| 10.000 | 37 % | 0 | 100 % | +0,34 | +0,09 … +0,63 | 13,8 (8 … 21) |
| 50.000 | 99 % | 0 | 100 % | +0,35 | +0,21 … +0,45 | 21,1 (16 … 25) |
| 100.000 | 100 % | 0 | 100 % | +0,34 | +0,26 … +0,40 | 23,6 (20 … 27) |

### schildtraeger_dmg_-10_aggressive

Alle 257.040 Partien: ΔST -5,18 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 88 % | 0 | 100 % | -5,01 | -9,16 … -1,73 | 14,8 (8 … 21) |
| 10.000 | 100 % | 0 | 100 % | -5,14 | -6,39 … -4,17 | 25,8 (23 … 28) |
| 50.000 | 100 % | 0 | 100 % | -5,19 | -5,56 … -4,77 | 27,9 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -5,17 | -5,42 … -4,90 | 28,0 (28 … 28) |

### schildtraeger_dmg_-10_byactiontype

Alle 257.040 Partien: ΔST -3,93 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 58 % | 0 | 100 % | -3,86 | -7,32 … -1,15 | 14,1 (10 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,97 | -5,53 … -3,03 | 25,6 (22 … 28) |
| 50.000 | 100 % | 0 | 100 % | -3,93 | -4,30 … -3,64 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,94 | -4,17 … -3,69 | 28,0 (28 … 28) |

### schildtraeger_dmg_-10_defensive

Alle 257.040 Partien: ΔST -2,47 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 21 % | 0 | 98 % | -2,41 | -5,53 … +1,31 | 12,9 (7 … 20) |
| 10.000 | 98 % | 0 | 100 % | -2,43 | -3,35 … -1,60 | 22,2 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | -2,48 | -2,74 … -2,04 | 26,7 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,45 | -2,71 … -2,20 | 27,5 (26 … 28) |

### schildtraeger_dmg_-10_uniform

Alle 257.040 Partien: ΔST -4,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 70 % | 0 | 100 % | -4,20 | -7,24 … -0,67 | 14,7 (6 … 20) |
| 10.000 | 100 % | 0 | 100 % | -4,20 | -5,05 … -3,35 | 25,5 (22 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,20 | -4,53 … -3,76 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,21 | -4,42 … -3,93 | 28,0 (28 … 28) |

### schildtraeger_dmg_-2.5_aggressive

Alle 257.040 Partien: ΔST -1,97 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 28 % | 0 | 99 % | -1,91 | -4,59 … +0,25 | 9,0 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | -1,94 | -2,65 … -1,37 | 22,0 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,97 | -2,29 … -1,65 | 26,7 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,97 | -2,13 … -1,86 | 27,6 (26 … 28) |

### schildtraeger_dmg_-2.5_byactiontype

Alle 257.040 Partien: ΔST -1,53 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 26 % | 0 | 98 % | -1,55 | -5,03 … +0,48 | 9,0 (4 … 16) |
| 10.000 | 98 % | 0 | 100 % | -1,53 | -2,04 … -0,93 | 20,6 (16 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,51 | -1,77 … -1,29 | 27,0 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,53 | -1,69 … -1,40 | 27,5 (26 … 28) |

### schildtraeger_dmg_-2.5_defensive

Alle 257.040 Partien: ΔST -1,12 pp, Kompositionen in Richtung / dagegen 25 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 8 % | 0 | 90 % | -1,14 | -4,25 … +1,55 | 9,9 (5 … 16) |
| 10.000 | 72 % | 0 | 100 % | -1,15 | -1,90 … -0,40 | 19,1 (13 … 24) |
| 50.000 | 100 % | 0 | 100 % | -1,12 | -1,32 … -0,85 | 22,9 (19 … 26) |
| 100.000 | 100 % | 0 | 100 % | -1,11 | -1,27 … -0,93 | 23,9 (21 … 27) |

### schildtraeger_dmg_-2.5_uniform

Alle 257.040 Partien: ΔST -1,75 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 26 % | 0 | 97 % | -1,77 | -3,69 … +0,46 | 9,6 (3 … 16) |
| 10.000 | 100 % | 0 | 100 % | -1,73 | -2,45 … -1,00 | 21,5 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,74 | -1,97 … -1,51 | 27,2 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,76 | -1,91 … -1,62 | 28,0 (27 … 28) |

### schildtraeger_dmg_-5_aggressive

Alle 257.040 Partien: ΔST -3,38 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 55 % | 0 | 99 % | -3,16 | -6,65 … +0,00 | 12,1 (6 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,37 | -4,19 … -2,66 | 23,9 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | -3,38 | -3,73 … -2,99 | 27,4 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,38 | -3,63 … -3,16 | 27,7 (26 … 28) |

### schildtraeger_dmg_-5_byactiontype

Alle 257.040 Partien: ΔST -2,40 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 35 % | 0 | 99 % | -2,36 | -6,86 … +0,00 | 11,4 (7 … 19) |
| 10.000 | 100 % | 0 | 100 % | -2,39 | -3,61 … -1,56 | 23,1 (18 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,40 | -2,65 … -2,16 | 27,6 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,40 | -2,56 … -2,21 | 28,0 (27 … 28) |

### schildtraeger_dmg_-5_defensive

Alle 257.040 Partien: ΔST -1,61 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 14 % | 0 | 93 % | -1,63 | -5,20 … +2,50 | 11,4 (5 … 17) |
| 10.000 | 88 % | 0 | 100 % | -1,61 | -2,45 … -0,80 | 20,3 (16 … 24) |
| 50.000 | 100 % | 0 | 100 % | -1,61 | -1,84 … -1,27 | 24,7 (22 … 27) |
| 100.000 | 100 % | 0 | 100 % | -1,60 | -1,84 … -1,37 | 25,8 (24 … 27) |

### schildtraeger_dmg_-5_uniform

Alle 257.040 Partien: ΔST -2,66 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 39 % | 0 | 97 % | -2,53 | -4,91 … +0,22 | 12,0 (5 … 17) |
| 10.000 | 100 % | 0 | 100 % | -2,65 | -3,44 … -1,87 | 23,5 (18 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,65 | -2,91 … -2,27 | 27,6 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,66 | -2,87 … -2,47 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_aggressive

Alle 257.040 Partien: ΔST +2,51 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 41 % | 0 | 100 % | +2,37 | +0,44 … +4,70 | 11,0 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,53 | +1,68 … +3,29 | 22,1 (18 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,52 | +2,24 … +2,86 | 26,1 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,50 | +2,34 … +2,71 | 26,7 (25 … 28) |

### schildtraeger_dmg_10_byactiontype

Alle 257.040 Partien: ΔST +2,79 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 50 % | 0 | 100 % | +2,77 | +0,24 … +6,03 | 12,1 (6 … 18) |
| 10.000 | 100 % | 0 | 100 % | +2,82 | +2,23 … +3,62 | 23,8 (20 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,81 | +2,54 … +3,13 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,77 | +2,58 … +2,93 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_defensive

Alle 257.040 Partien: ΔST +1,63 pp, Kompositionen in Richtung / dagegen 25 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 19 % | 0 | 87 % | +1,58 | -1,43 … +4,29 | 11,8 (6 … 18) |
| 10.000 | 81 % | 0 | 100 % | +1,62 | +0,82 … +2,68 | 19,7 (13 … 24) |
| 50.000 | 100 % | 0 | 100 % | +1,63 | +1,30 … +2,03 | 23,5 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,62 | +1,40 … +1,79 | 24,6 (22 … 27) |

### schildtraeger_dmg_10_uniform

Alle 257.040 Partien: ΔST +3,05 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 56 % | 0 | 99 % | +2,93 | +0,00 … +6,19 | 12,5 (8 … 18) |
| 10.000 | 100 % | 0 | 100 % | +3,01 | +2,10 … +3,67 | 23,8 (18 … 27) |
| 50.000 | 100 % | 0 | 100 % | +3,05 | +2,74 … +3,42 | 27,5 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +3,06 | +2,82 … +3,23 | 27,9 (27 … 28) |

### schildtraeger_dmg_2.5_aggressive

Alle 257.040 Partien: ΔST +0,51 pp, Kompositionen in Richtung / dagegen 20 / 7, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 42 | 85 % | +0,54 | -0,48 … +1,59 | 3,6 (1 … 9) |
| 10.000 | 35 % | 0 | 100 % | +0,51 | +0,14 … +1,08 | 13,5 (7 … 19) |
| 50.000 | 67 % | 0 | 100 % | +0,51 | +0,41 … +0,64 | 18,1 (14 … 23) |
| 100.000 | 79 % | 0 | 100 % | +0,51 | +0,44 … +0,59 | 19,1 (15 … 23) |

### schildtraeger_dmg_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,60 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 20 | 89 % | +0,60 | -0,93 … +1,64 | 4,4 (1 … 9) |
| 10.000 | 81 % | 0 | 100 % | +0,61 | +0,21 … +1,09 | 16,3 (10 … 20) |
| 50.000 | 100 % | 0 | 100 % | +0,61 | +0,47 … +0,74 | 23,5 (19 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,61 | +0,51 … +0,71 | 25,9 (22 … 28) |

### schildtraeger_dmg_2.5_defensive

Alle 257.040 Partien: ΔST +0,46 pp, Kompositionen in Richtung / dagegen 18 / 9, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 0 | 78 % | +0,58 | -2,30 … +2,64 | 8,8 (3 … 16) |
| 10.000 | 10 % | 0 | 92 % | +0,42 | -0,17 … +1,14 | 14,8 (6 … 20) |
| 50.000 | 29 % | 0 | 100 % | +0,47 | +0,23 … +0,85 | 17,7 (13 … 22) |
| 100.000 | 31 % | 0 | 100 % | +0,45 | +0,25 … +0,62 | 17,6 (13 … 21) |

### schildtraeger_dmg_2.5_uniform

Alle 257.040 Partien: ΔST +0,72 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 15 | 86 % | +0,66 | -0,45 … +2,35 | 4,9 (0 … 10) |
| 10.000 | 83 % | 0 | 100 % | +0,72 | +0,33 … +1,09 | 16,9 (12 … 22) |
| 50.000 | 100 % | 0 | 100 % | +0,72 | +0,59 … +0,90 | 24,0 (19 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,72 | +0,64 … +0,83 | 26,1 (23 … 28) |

### schildtraeger_dmg_5_aggressive

Alle 257.040 Partien: ΔST +1,43 pp, Kompositionen in Richtung / dagegen 26 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 16 % | 0 | 93 % | +1,31 | -0,48 … +2,83 | 8,1 (2 … 13) |
| 10.000 | 94 % | 0 | 100 % | +1,44 | +0,62 … +2,04 | 19,5 (13 … 24) |
| 50.000 | 100 % | 0 | 100 % | +1,43 | +1,26 … +1,66 | 24,2 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,42 | +1,29 … +1,56 | 25,6 (23 … 28) |

### schildtraeger_dmg_5_byactiontype

Alle 257.040 Partien: ΔST +1,39 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 18 % | 0 | 96 % | +1,38 | -0,46 … +3,48 | 8,5 (3 … 14) |
| 10.000 | 97 % | 0 | 100 % | +1,41 | +0,81 … +2,05 | 20,4 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,40 | +1,18 … +1,67 | 26,5 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,39 | +1,25 … +1,55 | 27,7 (26 … 28) |

### schildtraeger_dmg_5_defensive

Alle 257.040 Partien: ΔST +0,89 pp, Kompositionen in Richtung / dagegen 24 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 0 | 84 % | +0,95 | -1,90 … +2,97 | 10,3 (4 … 16) |
| 10.000 | 38 % | 0 | 100 % | +0,88 | +0,26 … +1,81 | 17,1 (12 … 21) |
| 50.000 | 96 % | 0 | 100 % | +0,91 | +0,59 … +1,34 | 21,0 (17 … 26) |
| 100.000 | 99 % | 0 | 100 % | +0,88 | +0,63 … +1,03 | 22,1 (18 … 25) |

### schildtraeger_dmg_5_uniform

Alle 257.040 Partien: ΔST +1,59 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 19 % | 0 | 94 % | +1,46 | -0,46 … +3,99 | 8,9 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | +1,57 | +0,97 … +2,18 | 20,7 (16 … 27) |
| 50.000 | 100 % | 0 | 100 % | +1,58 | +1,33 … +1,86 | 26,3 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,60 | +1,40 … +1,75 | 27,2 (26 … 28) |

### schildtraeger_hp_-10_aggressive

Alle 257.040 Partien: ΔST -5,63 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 99 % | 0 | 100 % | -5,59 | -8,39 … -3,00 | 16,3 (10 … 23) |
| 10.000 | 100 % | 0 | 100 % | -5,64 | -6,68 … -4,86 | 27,4 (25 … 28) |
| 50.000 | 100 % | 0 | 100 % | -5,62 | -5,93 … -5,28 | 28,0 (28 … 28) |
| 100.000 | 100 % | 0 | 100 % | -5,64 | -5,83 … -5,41 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_byactiontype

Alle 257.040 Partien: ΔST -2,94 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 60 % | 0 | 100 % | -2,81 | -5,34 … -0,22 | 11,8 (8 … 18) |
| 10.000 | 100 % | 0 | 100 % | -2,95 | -3,88 … -2,18 | 24,8 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,94 | -3,21 … -2,63 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,93 | -3,12 … -2,76 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_defensive

Alle 257.040 Partien: ΔST -2,57 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 66 % | 0 | 100 % | -2,63 | -4,50 … -0,85 | 10,6 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | -2,58 | -3,37 … -2,01 | 24,7 (20 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,58 | -2,84 … -2,33 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,57 | -2,77 … -2,41 | 28,0 (27 … 28) |

### schildtraeger_hp_-10_uniform

Alle 257.040 Partien: ΔST -2,99 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 62 % | 0 | 100 % | -3,03 | -6,13 … -0,93 | 12,3 (7 … 18) |
| 10.000 | 100 % | 0 | 100 % | -2,97 | -3,85 … -2,29 | 25,1 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,99 | -3,32 … -2,70 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,00 | -3,19 … -2,84 | 28,0 (28 … 28) |

### schildtraeger_hp_-2.5_aggressive

Alle 257.040 Partien: ΔST -1,34 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 24 % | 2 | 100 % | -1,33 | -3,00 … -0,22 | 7,0 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | -1,34 | -1,90 … -0,93 | 21,2 (17 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,35 | -1,54 … -1,18 | 26,8 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,35 | -1,46 … -1,22 | 27,6 (26 … 28) |

### schildtraeger_hp_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,65 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 20 | 81 % | -0,57 | -1,90 … +0,47 | 4,4 (1 … 8) |
| 10.000 | 85 % | 0 | 100 % | -0,63 | -1,16 … -0,18 | 17,0 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | -0,64 | -0,82 … -0,44 | 24,5 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,64 | -0,78 … -0,54 | 27,0 (25 … 28) |

### schildtraeger_hp_-2.5_defensive

Alle 257.040 Partien: ΔST -0,59 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 31 | 91 % | -0,63 | -1,85 … +0,24 | 4,1 (0 … 8) |
| 10.000 | 79 % | 0 | 100 % | -0,57 | -1,05 … -0,07 | 15,4 (9 … 22) |
| 50.000 | 100 % | 0 | 100 % | -0,59 | -0,71 … -0,43 | 23,5 (20 … 27) |
| 100.000 | 100 % | 0 | 100 % | -0,59 | -0,71 … -0,50 | 25,6 (22 … 28) |

### schildtraeger_hp_-2.5_uniform

Alle 257.040 Partien: ΔST -0,68 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 9 | 83 % | -0,74 | -2,05 … +0,43 | 5,2 (0 … 11) |
| 10.000 | 86 % | 0 | 100 % | -0,70 | -1,13 … -0,27 | 17,5 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | -0,68 | -0,81 … -0,52 | 24,9 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,68 | -0,77 … -0,58 | 26,9 (24 … 28) |

### schildtraeger_hp_-5_aggressive

Alle 257.040 Partien: ΔST -2,74 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 67 % | 0 | 100 % | -2,66 | -4,38 … -0,91 | 11,0 (4 … 17) |
| 10.000 | 100 % | 0 | 100 % | -2,73 | -3,43 … -2,14 | 24,7 (21 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,74 | -2,97 … -2,47 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,74 | -2,88 … -2,53 | 28,0 (27 … 28) |

### schildtraeger_hp_-5_byactiontype

Alle 257.040 Partien: ΔST -1,39 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 18 % | 0 | 96 % | -1,29 | -3,18 … +0,23 | 7,6 (3 … 13) |
| 10.000 | 100 % | 0 | 100 % | -1,41 | -2,18 … -0,92 | 21,2 (15 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,38 | -1,62 … -1,15 | 26,9 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,38 | -1,53 … -1,24 | 27,7 (27 … 28) |

### schildtraeger_hp_-5_defensive

Alle 257.040 Partien: ΔST -1,18 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 22 % | 3 | 99 % | -1,23 | -2,77 … +0,23 | 6,6 (2 … 11) |
| 10.000 | 99 % | 0 | 100 % | -1,17 | -1,71 … -0,57 | 19,6 (14 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,18 | -1,35 … -1,00 | 26,4 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,18 | -1,33 … -1,03 | 27,3 (25 … 28) |

### schildtraeger_hp_-5_uniform

Alle 257.040 Partien: ΔST -1,43 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 23 % | 0 | 98 % | -1,47 | -3,40 … +0,23 | 8,3 (4 … 13) |
| 10.000 | 99 % | 0 | 100 % | -1,43 | -1,92 … -0,82 | 21,2 (17 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,45 | -1,66 … -1,17 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,44 | -1,58 … -1,31 | 27,9 (27 … 28) |

### schildtraeger_hp_10_aggressive

Alle 257.040 Partien: ΔST +5,03 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 94 % | 0 | 100 % | +4,86 | +2,27 … +7,89 | 15,1 (9 … 21) |
| 10.000 | 100 % | 0 | 100 % | +5,03 | +4,20 … +5,89 | 26,9 (25 … 28) |
| 50.000 | 100 % | 0 | 100 % | +5,06 | +4,76 … +5,45 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +5,03 | +4,78 … +5,22 | 28,0 (28 … 28) |

### schildtraeger_hp_10_byactiontype

Alle 257.040 Partien: ΔST +2,67 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 61 % | 0 | 100 % | +2,65 | +0,69 … +4,81 | 11,3 (6 … 15) |
| 10.000 | 100 % | 0 | 100 % | +2,71 | +1,95 … +3,46 | 24,6 (20 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,68 | +2,40 … +2,96 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,66 | +2,51 … +2,86 | 28,0 (27 … 28) |

### schildtraeger_hp_10_defensive

Alle 257.040 Partien: ΔST +2,25 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 49 % | 0 | 99 % | +2,11 | +0,00 … +5,30 | 9,2 (4 … 17) |
| 10.000 | 100 % | 0 | 100 % | +2,23 | +1,49 … +2,85 | 23,2 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,25 | +1,98 … +2,48 | 27,6 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,25 | +2,06 … +2,39 | 27,9 (27 … 28) |

### schildtraeger_hp_10_uniform

Alle 257.040 Partien: ΔST +2,83 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 63 % | 0 | 100 % | +2,94 | +0,45 … +4,90 | 11,8 (4 … 19) |
| 10.000 | 100 % | 0 | 100 % | +2,78 | +2,06 … +3,39 | 24,8 (20 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,84 | +2,54 … +3,12 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,84 | +2,68 … +2,99 | 28,0 (28 … 28) |

### schildtraeger_hp_2.5_aggressive

Alle 257.040 Partien: ΔST +1,54 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 35 % | 2 | 100 % | +1,47 | +0,43 … +3,25 | 7,5 (3 … 15) |
| 10.000 | 100 % | 0 | 100 % | +1,56 | +1,14 … +1,98 | 22,3 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,56 | +1,33 … +1,74 | 27,3 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,55 | +1,44 … +1,68 | 27,9 (27 … 28) |

### schildtraeger_hp_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,76 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 9 % | 11 | 78 % | +0,71 | -0,45 … +1,86 | 5,2 (1 … 10) |
| 10.000 | 84 % | 0 | 100 % | +0,77 | +0,34 … +1,26 | 18,1 (13 … 24) |
| 50.000 | 100 % | 0 | 100 % | +0,77 | +0,61 … +0,94 | 25,2 (22 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,75 | +0,66 … +0,88 | 26,7 (24 … 28) |

### schildtraeger_hp_2.5_defensive

Alle 257.040 Partien: ΔST +0,65 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 27 | 86 % | +0,62 | -0,44 … +1,88 | 4,2 (1 … 9) |
| 10.000 | 92 % | 0 | 100 % | +0,66 | +0,28 … +1,12 | 16,2 (9 … 21) |
| 50.000 | 100 % | 0 | 100 % | +0,64 | +0,50 … +0,83 | 23,2 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,65 | +0,52 … +0,74 | 25,4 (22 … 28) |

### schildtraeger_hp_2.5_uniform

Alle 257.040 Partien: ΔST +0,80 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 11 % | 13 | 92 % | +0,85 | -0,69 … +2,51 | 5,4 (1 … 10) |
| 10.000 | 94 % | 0 | 100 % | +0,79 | +0,29 … +1,20 | 18,0 (14 … 22) |
| 50.000 | 100 % | 0 | 100 % | +0,82 | +0,63 … +1,02 | 25,7 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,80 | +0,69 … +0,89 | 27,4 (26 … 28) |

### schildtraeger_hp_5_aggressive

Alle 257.040 Partien: ΔST +2,66 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 69 % | 0 | 100 % | +2,50 | +0,84 … +4,64 | 10,8 (6 … 18) |
| 10.000 | 100 % | 0 | 100 % | +2,67 | +2,24 … +3,39 | 24,8 (22 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,68 | +2,44 … +2,93 | 27,6 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,66 | +2,50 … +2,80 | 27,8 (27 … 28) |

### schildtraeger_hp_5_byactiontype

Alle 257.040 Partien: ΔST +1,41 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 21 % | 1 | 97 % | +1,31 | +0,00 … +3,12 | 7,7 (1 … 13) |
| 10.000 | 100 % | 0 | 100 % | +1,43 | +0,97 … +2,07 | 21,0 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,43 | +1,16 … +1,67 | 27,1 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,40 | +1,26 … +1,57 | 27,9 (27 … 28) |

### schildtraeger_hp_5_defensive

Alle 257.040 Partien: ΔST +1,23 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 18 % | 4 | 98 % | +1,18 | -0,44 … +3,18 | 6,3 (2 … 13) |
| 10.000 | 99 % | 0 | 100 % | +1,22 | +0,80 … +1,80 | 19,6 (15 … 25) |
| 50.000 | 100 % | 0 | 100 % | +1,23 | +1,04 … +1,44 | 26,2 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,23 | +1,11 … +1,33 | 27,4 (25 … 28) |

### schildtraeger_hp_5_uniform

Alle 257.040 Partien: ΔST +1,53 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 32 % | 0 | 97 % | +1,56 | -0,24 … +3,42 | 8,5 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | +1,52 | +1,15 … +2,04 | 21,5 (17 … 27) |
| 50.000 | 100 % | 0 | 100 % | +1,55 | +1,34 … +1,73 | 27,4 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,53 | +1,39 … +1,65 | 27,9 (27 … 28) |

