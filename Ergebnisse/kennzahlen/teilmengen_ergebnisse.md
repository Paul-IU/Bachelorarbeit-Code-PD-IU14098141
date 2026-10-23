# Simulationsumfang über Teilmengen (Schritt 6)

Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: `numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.

Unentschieden zählen als halber Sieg. Erkannt = (a) Siegquote des Schildträgers in Richtung der Manipulation UND (b) einseitiger Vorzeichentest (α = 0,05) über die veränderten Kompositionen mit Schildträger. ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp); nur (a) = Anteil der Wiederholungen mit richtigem Vorzeichen von ΔST; Test unmöglich = Wiederholungen mit weniger als 5 veränderten Kompositionen.

## Übersicht: Anteil erkannter Wiederholungen und kleinste Stichprobe mit ≥ 95 %

In Klammern: Wiederholungen, in denen der Vorzeichentest mangels veränderter Kompositionen nicht anschlagen konnte (wenn > 0).

| Variante | voller Lauf | 1.000 | 10.000 | 50.000 | 100.000 | ab |
|---|---|---|---|---|---|---|
| schildtraeger_ability_-10_aggressive | nicht erkannt | 0 % (22) | 0 % | 1 % | 0 % | nie |
| schildtraeger_ability_-10_byactiontype | erkannt | 1 % (61) | 23 % | 79 % | 99 % | 100.000 |
| schildtraeger_ability_-10_defensive | erkannt | 10 % | 46 % | 89 % | 99 % | 100.000 |
| schildtraeger_ability_-10_uniform | erkannt | 1 % (35) | 60 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-2.5_aggressive | nicht erkannt | 0 % (97) | 4 % (2) | 10 % | 14 % | nie |
| schildtraeger_ability_-2.5_byactiontype | erkannt | 0 % (99) | 1 % (11) | 12 % | 44 % | nie |
| schildtraeger_ability_-2.5_defensive | nicht erkannt | 3 % (15) | 5 % | 19 % | 17 % | nie |
| schildtraeger_ability_-2.5_uniform | erkannt | 0 % (99) | 4 % (3) | 61 % | 84 % | nie |
| schildtraeger_ability_-5_aggressive | nicht erkannt | 0 % (71) | 3 % | 8 % | 13 % | nie |
| schildtraeger_ability_-5_byactiontype | erkannt | 0 % (94) | 4 % | 31 % | 59 % | nie |
| schildtraeger_ability_-5_defensive | erkannt | 3 % (1) | 5 % | 25 % | 51 % | nie |
| schildtraeger_ability_-5_uniform | erkannt | 1 % (79) | 27 % | 96 % | 100 % | 50.000 |
| schildtraeger_ability_10_aggressive | nicht erkannt | 0 % (6) | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_10_byactiontype | erkannt | 2 % (39) | 32 % | 88 % | 100 % | 100.000 |
| schildtraeger_ability_10_defensive | nicht erkannt | 1 % | 2 % | 2 % | 5 % | nie |
| schildtraeger_ability_10_uniform | erkannt | 3 % (12) | 70 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_2.5_aggressive | nicht erkannt | 0 % (74) | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_2.5_byactiontype | erkannt | 0 % (78) | 7 % | 34 % | 73 % | nie |
| schildtraeger_ability_2.5_defensive | nicht erkannt | 0 % | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_2.5_uniform | erkannt | 0 % (75) | 25 % | 87 % | 99 % | 100.000 |
| schildtraeger_ability_5_aggressive | nicht erkannt | 0 % (37) | 1 % | 0 % | 0 % | nie |
| schildtraeger_ability_5_byactiontype | erkannt | 0 % (60) | 17 % | 65 % | 95 % | 100.000 |
| schildtraeger_ability_5_defensive | nicht erkannt | 1 % | 0 % | 0 % | 0 % | nie |
| schildtraeger_ability_5_uniform | erkannt | 1 % (41) | 56 % | 98 % | 100 % | 50.000 |
| schildtraeger_dmg_-10_aggressive | erkannt | 94 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_byactiontype | erkannt | 73 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_defensive | erkannt | 50 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_uniform | erkannt | 67 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_aggressive | erkannt | 53 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_byactiontype | erkannt | 20 % | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_defensive | erkannt | 16 % | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_-2.5_uniform | erkannt | 25 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_aggressive | erkannt | 76 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_byactiontype | erkannt | 51 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_defensive | erkannt | 31 % | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-5_uniform | erkannt | 46 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_aggressive | erkannt | 56 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_byactiontype | erkannt | 51 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_defensive | erkannt | 37 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_10_uniform | erkannt | 51 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_aggressive | erkannt | 4 % (50) | 61 % | 99 % | 100 % | 50.000 |
| schildtraeger_dmg_2.5_byactiontype | erkannt | 6 % (16) | 83 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_2.5_defensive | erkannt | 5 % (2) | 27 % | 68 % | 90 % | nie |
| schildtraeger_dmg_2.5_uniform | erkannt | 6 % (11) | 84 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_20_byactiontype | erkannt | 96 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_5_aggressive | erkannt | 13 % (2) | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_byactiontype | erkannt | 23 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_defensive | erkannt | 15 % | 71 % | 100 % | 100 % | 50.000 |
| schildtraeger_dmg_5_uniform | erkannt | 27 % | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_aggressive | erkannt | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_byactiontype | erkannt | 65 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_defensive | erkannt | 72 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-10_uniform | erkannt | 65 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_aggressive | erkannt | 34 % (11) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_byactiontype | erkannt | 4 % (13) | 89 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_defensive | erkannt | 4 % (43) | 83 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-2.5_uniform | erkannt | 4 % (19) | 89 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_-5_aggressive | erkannt | 67 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_byactiontype | erkannt | 21 % (1) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_defensive | erkannt | 20 % (10) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_uniform | erkannt | 22 % (1) | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_aggressive | erkannt | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_byactiontype | erkannt | 56 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_defensive | erkannt | 52 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_10_uniform | erkannt | 67 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_aggressive | erkannt | 18 % (10) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_byactiontype | erkannt | 8 % (12) | 86 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_defensive | erkannt | 6 % (45) | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_2.5_uniform | erkannt | 6 % (14) | 87 % | 100 % | 100 % | 50.000 |
| schildtraeger_hp_5_aggressive | erkannt | 56 % (1) | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_byactiontype | erkannt | 17 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_defensive | erkannt | 26 % (12) | 99 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_uniform | erkannt | 29 % (1) | 100 % | 100 % | 100 % | 10.000 |

## Einzelwerte je Variante

### schildtraeger_ability_-10_aggressive

Alle 257.040 Partien: ΔST +0,06 pp, Kompositionen in Richtung / dagegen 14 / 14, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 22 | 42 % | +0,07 | -1,35 … +2,50 | 3,3 (0 … 7) |
| 10.000 | 0 % | 0 | 24 % | +0,08 | -0,25 … +0,52 | 10,7 (6 … 17) |
| 50.000 | 1 % | 0 | 15 % | +0,06 | -0,12 … +0,29 | 13,2 (9 … 19) |
| 100.000 | 0 % | 0 | 3 % | +0,07 | -0,05 … +0,17 | 13,6 (11 … 17) |

### schildtraeger_ability_-10_byactiontype

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung / dagegen 24 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 61 | 60 % | -0,19 | -1,36 … +0,69 | 2,4 (0 … 6) |
| 10.000 | 23 % | 0 | 98 % | -0,23 | -0,53 … +0,05 | 12,2 (6 … 18) |
| 50.000 | 79 % | 0 | 100 % | -0,23 | -0,33 … -0,13 | 18,4 (13 … 23) |
| 100.000 | 99 % | 0 | 100 % | -0,23 | -0,32 … -0,16 | 21,7 (17 … 25) |

### schildtraeger_ability_-10_defensive

Alle 257.040 Partien: ΔST -0,66 pp, Kompositionen in Richtung / dagegen 24 / 4, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 10 % | 0 | 78 % | -0,69 | -3,14 … +1,23 | 7,8 (4 … 14) |
| 10.000 | 46 % | 0 | 100 % | -0,66 | -1,21 … -0,11 | 16,8 (12 … 21) |
| 50.000 | 89 % | 0 | 100 % | -0,65 | -0,98 … -0,43 | 20,6 (17 … 24) |
| 100.000 | 99 % | 0 | 100 % | -0,66 | -0,81 … -0,49 | 22,3 (18 … 26) |

### schildtraeger_ability_-10_uniform

Alle 257.040 Partien: ΔST -0,48 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 35 | 72 % | -0,36 | -1,67 … +0,47 | 3,6 (0 … 9) |
| 10.000 | 60 % | 0 | 100 % | -0,47 | -0,80 … -0,18 | 14,9 (9 … 20) |
| 50.000 | 100 % | 0 | 100 % | -0,48 | -0,63 … -0,37 | 23,1 (19 … 27) |
| 100.000 | 100 % | 0 | 100 % | -0,48 | -0,58 … -0,37 | 25,5 (22 … 28) |

### schildtraeger_ability_-2.5_aggressive

Alle 257.040 Partien: ΔST -0,05 pp, Kompositionen in Richtung / dagegen 18 / 10, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 97 | 34 % | -0,06 | -0,47 … +0,48 | 0,8 (0 … 3) |
| 10.000 | 4 % | 2 | 63 % | -0,03 | -0,20 … +0,09 | 5,2 (2 … 11) |
| 50.000 | 10 % | 0 | 98 % | -0,05 | -0,10 … +0,00 | 11,9 (6 … 16) |
| 100.000 | 14 % | 0 | 100 % | -0,05 | -0,08 … -0,01 | 14,6 (10 … 19) |

### schildtraeger_ability_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,05 pp, Kompositionen in Richtung / dagegen 16 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 99 | 17 % | -0,02 | -0,24 … +0,25 | 0,4 (0 … 3) |
| 10.000 | 1 % | 11 | 72 % | -0,04 | -0,20 … +0,07 | 4,4 (1 … 10) |
| 50.000 | 12 % | 0 | 100 % | -0,04 | -0,10 … -0,01 | 10,8 (6 … 17) |
| 100.000 | 44 % | 0 | 100 % | -0,04 | -0,08 … -0,01 | 14,3 (10 … 20) |

### schildtraeger_ability_-2.5_defensive

Alle 257.040 Partien: ΔST -0,14 pp, Kompositionen in Richtung / dagegen 15 / 12, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 15 | 52 % | -0,17 | -1,47 … +1,18 | 3,9 (0 … 9) |
| 10.000 | 5 % | 0 | 83 % | -0,14 | -0,62 … +0,21 | 12,4 (7 … 19) |
| 50.000 | 19 % | 0 | 97 % | -0,13 | -0,36 … +0,00 | 15,8 (10 … 20) |
| 100.000 | 17 % | 0 | 99 % | -0,14 | -0,23 … +0,01 | 16,5 (11 … 21) |

### schildtraeger_ability_-2.5_uniform

Alle 257.040 Partien: ΔST -0,09 pp, Kompositionen in Richtung / dagegen 22 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 99 | 35 % | -0,08 | -0,48 … +0,45 | 0,8 (0 … 3) |
| 10.000 | 4 % | 3 | 96 % | -0,09 | -0,28 … +0,05 | 6,0 (1 … 10) |
| 50.000 | 61 % | 0 | 100 % | -0,09 | -0,14 … -0,03 | 14,8 (9 … 20) |
| 100.000 | 84 % | 0 | 100 % | -0,09 | -0,12 … -0,03 | 17,8 (13 … 22) |

### schildtraeger_ability_-5_aggressive

Alle 257.040 Partien: ΔST -0,07 pp, Kompositionen in Richtung / dagegen 18 / 9, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 71 | 45 % | -0,07 | -1,13 … +1,14 | 1,9 (0 … 6) |
| 10.000 | 3 % | 0 | 68 % | -0,05 | -0,28 … +0,34 | 8,9 (2 … 13) |
| 50.000 | 8 % | 0 | 96 % | -0,07 | -0,16 … +0,02 | 14,1 (11 … 19) |
| 100.000 | 13 % | 0 | 99 % | -0,06 | -0,13 … +0,01 | 15,2 (10 … 20) |

### schildtraeger_ability_-5_byactiontype

Alle 257.040 Partien: ΔST -0,09 pp, Kompositionen in Richtung / dagegen 21 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 94 | 32 % | -0,06 | -0,70 … +0,46 | 1,1 (0 … 3) |
| 10.000 | 4 % | 0 | 81 % | -0,09 | -0,32 … +0,07 | 8,0 (4 … 13) |
| 50.000 | 31 % | 0 | 100 % | -0,09 | -0,15 … -0,01 | 14,8 (10 … 20) |
| 100.000 | 59 % | 0 | 100 % | -0,09 | -0,15 … -0,04 | 17,1 (12 … 21) |

### schildtraeger_ability_-5_defensive

Alle 257.040 Partien: ΔST -0,27 pp, Kompositionen in Richtung / dagegen 21 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 1 | 54 % | -0,27 | -1,82 … +0,94 | 5,3 (1 … 12) |
| 10.000 | 5 % | 0 | 94 % | -0,26 | -0,64 … +0,20 | 13,7 (8 … 18) |
| 50.000 | 25 % | 0 | 100 % | -0,27 | -0,54 … -0,12 | 16,9 (12 … 22) |
| 100.000 | 51 % | 0 | 100 % | -0,27 | -0,40 … -0,11 | 18,3 (14 … 22) |

### schildtraeger_ability_-5_uniform

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 79 | 57 % | -0,21 | -1,43 … +0,47 | 2,0 (0 … 7) |
| 10.000 | 27 % | 0 | 100 % | -0,23 | -0,50 … -0,05 | 11,3 (6 … 18) |
| 50.000 | 96 % | 0 | 100 % | -0,24 | -0,33 … -0,14 | 19,6 (14 … 25) |
| 100.000 | 100 % | 0 | 100 % | -0,24 | -0,28 … -0,17 | 23,2 (18 … 26) |

### schildtraeger_ability_10_aggressive

Alle 257.040 Partien: ΔST -0,27 pp, Kompositionen in Richtung / dagegen 11 / 17, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 6 | 22 % | -0,30 | -1,78 … +1,90 | 3,6 (0 … 8) |
| 10.000 | 1 % | 0 | 5 % | -0,26 | -0,75 … +0,21 | 9,3 (4 … 16) |
| 50.000 | 0 % | 0 | 0 % | -0,28 | -0,48 … -0,07 | 9,8 (6 … 16) |
| 100.000 | 0 % | 0 | 0 % | -0,28 | -0,39 … -0,17 | 9,8 (7 … 13) |

### schildtraeger_ability_10_byactiontype

Alle 257.040 Partien: ΔST +0,31 pp, Kompositionen in Richtung / dagegen 25 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 2 % | 39 | 64 % | +0,27 | -0,72 … +1,67 | 3,3 (1 … 8) |
| 10.000 | 32 % | 0 | 98 % | +0,31 | -0,09 … +0,64 | 13,6 (6 … 20) |
| 50.000 | 88 % | 0 | 100 % | +0,30 | +0,15 … +0,44 | 19,8 (15 … 25) |
| 100.000 | 100 % | 0 | 100 % | +0,30 | +0,22 … +0,38 | 22,2 (18 … 27) |

### schildtraeger_ability_10_defensive

Alle 257.040 Partien: ΔST +0,06 pp, Kompositionen in Richtung / dagegen 16 / 12, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 59 % | +0,26 | -1,60 … +2,54 | 8,0 (3 … 13) |
| 10.000 | 2 % | 0 | 57 % | +0,05 | -0,49 … +0,67 | 13,3 (9 … 18) |
| 50.000 | 2 % | 0 | 71 % | +0,07 | -0,21 … +0,36 | 14,7 (11 … 19) |
| 100.000 | 5 % | 0 | 83 % | +0,06 | -0,08 … +0,35 | 15,5 (10 … 20) |

### schildtraeger_ability_10_uniform

Alle 257.040 Partien: ΔST +0,56 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 3 % | 12 | 77 % | +0,52 | -0,70 … +1,90 | 4,5 (1 … 9) |
| 10.000 | 70 % | 0 | 100 % | +0,56 | +0,21 … +0,94 | 16,2 (12 … 22) |
| 50.000 | 100 % | 0 | 100 % | +0,58 | +0,44 … +0,83 | 23,8 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,56 | +0,45 … +0,66 | 26,2 (23 … 28) |

### schildtraeger_ability_2.5_aggressive

Alle 257.040 Partien: ΔST +0,01 pp, Kompositionen in Richtung / dagegen 15 / 13, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 74 | 32 % | -0,01 | -0,72 … +1,16 | 1,6 (0 … 5) |
| 10.000 | 1 % | 0 | 50 % | +0,01 | -0,23 … +0,27 | 8,9 (4 … 14) |
| 50.000 | 0 % | 0 | 53 % | +0,00 | -0,12 … +0,11 | 12,7 (7 … 17) |
| 100.000 | 0 % | 0 | 52 % | +0,00 | -0,08 … +0,07 | 13,5 (9 … 18) |

### schildtraeger_ability_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,11 pp, Kompositionen in Richtung / dagegen 21 / 7, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 78 | 45 % | +0,07 | -0,72 … +0,93 | 1,7 (0 … 5) |
| 10.000 | 7 % | 0 | 85 % | +0,11 | -0,07 … +0,37 | 9,6 (5 … 16) |
| 50.000 | 34 % | 0 | 100 % | +0,11 | +0,01 … +0,19 | 15,6 (10 … 21) |
| 100.000 | 73 % | 0 | 100 % | +0,11 | +0,06 … +0,18 | 18,2 (14 … 23) |

### schildtraeger_ability_2.5_defensive

Alle 257.040 Partien: ΔST -0,28 pp, Kompositionen in Richtung / dagegen 9 / 18, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 0 | 31 % | -0,23 | -1,80 … +1,59 | 5,3 (2 … 8) |
| 10.000 | 0 % | 0 | 8 % | -0,30 | -0,78 … +0,11 | 10,0 (4 … 16) |
| 50.000 | 0 % | 0 | 0 % | -0,28 | -0,46 … -0,08 | 10,0 (6 … 13) |
| 100.000 | 0 % | 0 | 0 % | -0,28 | -0,36 … -0,11 | 10,0 (7 … 14) |

### schildtraeger_ability_2.5_uniform

Alle 257.040 Partien: ΔST +0,19 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 75 | 57 % | +0,18 | -0,48 … +1,18 | 2,0 (0 … 6) |
| 10.000 | 25 % | 0 | 97 % | +0,19 | -0,09 … +0,44 | 11,2 (7 … 18) |
| 50.000 | 87 % | 0 | 100 % | +0,20 | +0,11 … +0,31 | 18,6 (14 … 24) |
| 100.000 | 99 % | 0 | 100 % | +0,19 | +0,13 … +0,26 | 21,8 (16 … 27) |

### schildtraeger_ability_5_aggressive

Alle 257.040 Partien: ΔST -0,07 pp, Kompositionen in Richtung / dagegen 11 / 17, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 37 | 30 % | -0,11 | -0,99 … +1,19 | 2,5 (0 … 8) |
| 10.000 | 1 % | 0 | 29 % | -0,07 | -0,36 … +0,30 | 9,6 (3 … 16) |
| 50.000 | 0 % | 0 | 9 % | -0,08 | -0,29 … +0,07 | 10,4 (7 … 17) |
| 100.000 | 0 % | 0 | 1 % | -0,07 | -0,17 … +0,03 | 10,6 (6 … 14) |

### schildtraeger_ability_5_byactiontype

Alle 257.040 Partien: ΔST +0,17 pp, Kompositionen in Richtung / dagegen 23 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 0 % | 60 | 55 % | +0,15 | -0,72 … +1,16 | 2,3 (0 … 7) |
| 10.000 | 17 % | 0 | 94 % | +0,18 | -0,11 … +0,43 | 11,4 (7 … 16) |
| 50.000 | 65 % | 0 | 100 % | +0,17 | +0,05 … +0,29 | 17,2 (12 … 24) |
| 100.000 | 95 % | 0 | 100 % | +0,17 | +0,11 … +0,24 | 20,2 (15 … 26) |

### schildtraeger_ability_5_defensive

Alle 257.040 Partien: ΔST -0,21 pp, Kompositionen in Richtung / dagegen 11 / 17, nicht erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 0 | 47 % | -0,04 | -1,69 … +1,71 | 6,6 (2 … 11) |
| 10.000 | 0 % | 0 | 14 % | -0,24 | -0,71 … +0,22 | 11,0 (7 … 15) |
| 50.000 | 0 % | 0 | 1 % | -0,21 | -0,52 … +0,02 | 11,5 (5 … 17) |
| 100.000 | 0 % | 0 | 0 % | -0,21 | -0,33 … -0,05 | 11,2 (7 … 15) |

### schildtraeger_ability_5_uniform

Alle 257.040 Partien: ΔST +0,33 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 1 % | 41 | 69 % | +0,32 | -0,48 … +1,42 | 3,0 (0 … 7) |
| 10.000 | 56 % | 0 | 100 % | +0,33 | +0,07 … +0,64 | 14,0 (9 … 18) |
| 50.000 | 98 % | 0 | 100 % | +0,34 | +0,21 … +0,52 | 21,1 (16 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,33 | +0,24 … +0,41 | 24,1 (20 … 28) |

### schildtraeger_dmg_-10_aggressive

Alle 257.040 Partien: ΔST -5,13 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 94 % | 0 | 100 % | -5,11 | -7,38 … -2,86 | 15,3 (10 … 21) |
| 10.000 | 100 % | 0 | 100 % | -5,15 | -5,96 … -4,11 | 26,4 (24 … 28) |
| 50.000 | 100 % | 0 | 100 % | -5,11 | -5,54 … -4,76 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -5,14 | -5,34 … -4,91 | 27,9 (27 … 28) |

### schildtraeger_dmg_-10_byactiontype

Alle 257.040 Partien: ΔST -4,11 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 73 % | 0 | 100 % | -4,21 | -7,26 … -1,10 | 14,6 (10 … 21) |
| 10.000 | 100 % | 0 | 100 % | -4,07 | -4,99 … -2,91 | 25,7 (23 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,13 | -4,51 … -3,82 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,13 | -4,43 … -3,88 | 28,0 (28 … 28) |

### schildtraeger_dmg_-10_defensive

Alle 257.040 Partien: ΔST -3,55 pp, Kompositionen in Richtung / dagegen 27 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 50 % | 0 | 100 % | -3,45 | -5,88 … -0,46 | 13,7 (9 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,58 | -4,40 … -2,78 | 24,3 (22 … 27) |
| 50.000 | 100 % | 0 | 100 % | -3,57 | -3,98 … -3,20 | 27,0 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,56 | -3,86 … -3,28 | 27,5 (26 … 28) |

### schildtraeger_dmg_-10_uniform

Alle 257.040 Partien: ΔST -4,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 67 % | 0 | 100 % | -4,24 | -7,57 … -0,68 | 14,7 (9 … 21) |
| 10.000 | 100 % | 0 | 100 % | -4,19 | -5,21 … -3,24 | 25,9 (23 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,15 | -4,58 … -3,65 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,20 | -4,44 … -3,97 | 28,0 (28 … 28) |

### schildtraeger_dmg_-2.5_aggressive

Alle 257.040 Partien: ΔST -2,23 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 53 % | 0 | 100 % | -2,31 | -4,09 … -0,47 | 9,8 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | -2,22 | -2,67 … -1,46 | 23,2 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,23 | -2,48 … -1,98 | 27,4 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,24 | -2,41 … -2,06 | 27,9 (26 … 28) |

### schildtraeger_dmg_-2.5_byactiontype

Alle 257.040 Partien: ΔST -1,59 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 20 % | 0 | 99 % | -1,60 | -3,85 … +0,00 | 9,1 (4 … 16) |
| 10.000 | 99 % | 0 | 100 % | -1,59 | -2,37 … -0,89 | 21,3 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,59 | -1,82 … -1,33 | 26,9 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,59 | -1,76 … -1,48 | 27,8 (26 … 28) |

### schildtraeger_dmg_-2.5_defensive

Alle 257.040 Partien: ΔST -1,59 pp, Kompositionen in Richtung / dagegen 26 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 16 % | 0 | 93 % | -1,51 | -3,73 … +0,91 | 9,7 (5 … 15) |
| 10.000 | 94 % | 0 | 100 % | -1,62 | -2,30 … -0,82 | 20,8 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,60 | -1,85 … -1,34 | 24,6 (22 … 27) |
| 100.000 | 100 % | 0 | 100 % | -1,59 | -1,76 … -1,40 | 25,5 (23 … 28) |

### schildtraeger_dmg_-2.5_uniform

Alle 257.040 Partien: ΔST -1,71 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 25 % | 0 | 98 % | -1,75 | -3,79 … +0,00 | 9,5 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | -1,70 | -2,26 … -1,15 | 21,4 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,70 | -1,95 … -1,47 | 27,0 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,71 | -1,87 … -1,57 | 27,8 (27 … 28) |

### schildtraeger_dmg_-5_aggressive

Alle 257.040 Partien: ΔST -3,40 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 76 % | 0 | 100 % | -3,43 | -5,92 … -1,09 | 12,8 (7 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,42 | -4,05 … -2,71 | 24,7 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -3,39 | -3,77 … -3,15 | 27,5 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,41 | -3,64 … -3,25 | 27,7 (27 … 28) |

### schildtraeger_dmg_-5_byactiontype

Alle 257.040 Partien: ΔST -2,54 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 51 % | 0 | 100 % | -2,52 | -5,90 … -0,22 | 11,6 (6 … 19) |
| 10.000 | 100 % | 0 | 100 % | -2,48 | -3,21 … -1,62 | 23,6 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,55 | -2,83 … -2,22 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,54 | -2,74 … -2,34 | 28,0 (28 … 28) |

### schildtraeger_dmg_-5_defensive

Alle 257.040 Partien: ΔST -2,28 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 31 % | 0 | 97 % | -2,15 | -4,99 … +0,55 | 11,3 (6 … 16) |
| 10.000 | 98 % | 0 | 100 % | -2,31 | -3,16 … -1,54 | 22,1 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | -2,30 | -2,67 … -1,99 | 25,4 (22 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,30 | -2,54 … -2,09 | 26,2 (23 … 28) |

### schildtraeger_dmg_-5_uniform

Alle 257.040 Partien: ΔST -2,69 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 46 % | 0 | 99 % | -2,69 | -4,82 … +0,22 | 12,1 (6 … 17) |
| 10.000 | 100 % | 0 | 100 % | -2,66 | -3,43 … -1,97 | 23,7 (20 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,67 | -2,99 … -2,40 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,69 | -2,88 … -2,48 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_aggressive

Alle 257.040 Partien: ΔST +2,78 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 56 % | 0 | 100 % | +2,75 | +0,70 … +5,15 | 11,2 (6 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,84 | +2,32 … +3,52 | 23,1 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,77 | +2,48 … +3,02 | 26,6 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,78 | +2,57 … +2,98 | 27,3 (26 … 28) |

### schildtraeger_dmg_10_byactiontype

Alle 257.040 Partien: ΔST +2,83 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 51 % | 0 | 100 % | +2,85 | +0,24 … +5,33 | 12,5 (8 … 18) |
| 10.000 | 100 % | 0 | 100 % | +2,86 | +2,22 … +3,89 | 24,4 (19 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,84 | +2,50 … +3,21 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,84 | +2,55 … +3,10 | 28,0 (27 … 28) |

### schildtraeger_dmg_10_defensive

Alle 257.040 Partien: ΔST +2,44 pp, Kompositionen in Richtung / dagegen 26 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 37 % | 0 | 98 % | +2,48 | -1,14 … +4,65 | 12,0 (6 … 17) |
| 10.000 | 100 % | 0 | 100 % | +2,45 | +1,72 … +3,24 | 22,2 (19 … 26) |
| 50.000 | 100 % | 0 | 100 % | +2,45 | +2,11 … +2,80 | 25,1 (22 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,45 | +2,27 … +2,65 | 25,6 (24 … 27) |

### schildtraeger_dmg_10_uniform

Alle 257.040 Partien: ΔST +3,00 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 51 % | 0 | 100 % | +3,24 | +0,68 … +6,72 | 12,9 (8 … 20) |
| 10.000 | 100 % | 0 | 100 % | +3,05 | +2,14 … +3,83 | 24,4 (19 … 28) |
| 50.000 | 100 % | 0 | 100 % | +3,02 | +2,73 … +3,33 | 27,8 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +3,01 | +2,78 … +3,18 | 28,0 (27 … 28) |

### schildtraeger_dmg_2.5_aggressive

Alle 257.040 Partien: ΔST +0,52 pp, Kompositionen in Richtung / dagegen 23 / 5, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 50 | 84 % | +0,52 | -0,24 … +1,67 | 3,3 (0 … 8) |
| 10.000 | 61 % | 0 | 100 % | +0,52 | +0,30 … +0,92 | 14,7 (10 … 21) |
| 50.000 | 99 % | 0 | 100 % | +0,52 | +0,41 … +0,63 | 20,6 (17 … 24) |
| 100.000 | 100 % | 0 | 100 % | +0,52 | +0,43 … +0,61 | 21,9 (19 … 24) |

### schildtraeger_dmg_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,64 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 16 | 92 % | +0,66 | -0,70 … +2,00 | 4,7 (1 … 10) |
| 10.000 | 83 % | 0 | 100 % | +0,66 | +0,28 … +1,12 | 16,7 (11 … 25) |
| 50.000 | 100 % | 0 | 100 % | +0,63 | +0,50 … +0,77 | 24,6 (19 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,65 | +0,53 … +0,73 | 26,6 (23 … 28) |

### schildtraeger_dmg_2.5_defensive

Alle 257.040 Partien: ΔST +0,49 pp, Kompositionen in Richtung / dagegen 22 / 6, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 5 % | 2 | 66 % | +0,49 | -1,59 … +2,31 | 6,6 (2 … 12) |
| 10.000 | 27 % | 0 | 99 % | +0,50 | -0,03 … +1,00 | 15,5 (10 … 22) |
| 50.000 | 68 % | 0 | 100 % | +0,49 | +0,28 … +0,70 | 19,3 (15 … 25) |
| 100.000 | 90 % | 0 | 100 % | +0,49 | +0,37 … +0,62 | 20,2 (17 … 24) |

### schildtraeger_dmg_2.5_uniform

Alle 257.040 Partien: ΔST +0,69 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 11 | 87 % | +0,69 | -0,42 … +1,87 | 4,9 (1 … 9) |
| 10.000 | 84 % | 0 | 100 % | +0,70 | +0,21 … +1,10 | 16,9 (12 … 24) |
| 50.000 | 100 % | 0 | 100 % | +0,70 | +0,51 … +0,84 | 25,0 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,69 | +0,61 … +0,78 | 26,7 (25 … 28) |

### schildtraeger_dmg_20_byactiontype

Alle 257.040 Partien: ΔST +6,56 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 96 % | 0 | 100 % | +6,44 | +3,49 … +10,67 | 17,3 (12 … 22) |
| 10.000 | 100 % | 0 | 100 % | +6,57 | +5,65 … +7,72 | 27,3 (25 … 28) |
| 50.000 | 100 % | 0 | 100 % | +6,56 | +6,20 … +6,96 | 28,0 (28 … 28) |
| 100.000 | 100 % | 0 | 100 % | +6,56 | +6,25 … +6,84 | 28,0 (28 … 28) |

### schildtraeger_dmg_5_aggressive

Alle 257.040 Partien: ΔST +1,20 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 13 % | 2 | 97 % | +1,22 | -0,23 … +3,13 | 7,1 (3 … 12) |
| 10.000 | 98 % | 0 | 100 % | +1,22 | +0,64 … +1,69 | 19,8 (16 … 24) |
| 50.000 | 100 % | 0 | 100 % | +1,19 | +0,96 … +1,51 | 24,3 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,21 | +1,08 … +1,34 | 26,0 (23 … 28) |

### schildtraeger_dmg_5_byactiontype

Alle 257.040 Partien: ΔST +1,46 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 23 % | 0 | 98 % | +1,48 | -0,23 … +4,15 | 8,8 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | +1,48 | +0,86 … +2,08 | 21,0 (17 … 25) |
| 50.000 | 100 % | 0 | 100 % | +1,47 | +1,26 … +1,70 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,46 | +1,32 … +1,62 | 27,8 (27 … 28) |

### schildtraeger_dmg_5_defensive

Alle 257.040 Partien: ΔST +1,15 pp, Kompositionen in Richtung / dagegen 25 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 15 % | 0 | 85 % | +1,19 | -1,36 … +3,49 | 9,1 (3 … 15) |
| 10.000 | 71 % | 0 | 100 % | +1,15 | +0,56 … +1,61 | 18,4 (13 … 23) |
| 50.000 | 100 % | 0 | 100 % | +1,15 | +0,86 … +1,45 | 22,3 (19 … 25) |
| 100.000 | 100 % | 0 | 100 % | +1,15 | +1,00 … +1,34 | 23,4 (21 … 25) |

### schildtraeger_dmg_5_uniform

Alle 257.040 Partien: ΔST +1,60 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 27 % | 0 | 98 % | +1,72 | -0,23 … +4,02 | 9,3 (5 … 14) |
| 10.000 | 99 % | 0 | 100 % | +1,66 | +0,91 … +2,20 | 21,6 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,61 | +1,31 … +1,86 | 27,2 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,60 | +1,46 … +1,75 | 27,8 (27 … 28) |

### schildtraeger_hp_-10_aggressive

Alle 257.040 Partien: ΔST -4,73 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 98 % | 0 | 100 % | -4,67 | -7,57 … -2,75 | 14,6 (10 … 19) |
| 10.000 | 100 % | 0 | 100 % | -4,70 | -5,62 … -4,00 | 27,3 (25 … 28) |
| 50.000 | 100 % | 0 | 100 % | -4,73 | -5,06 … -4,49 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -4,73 | -4,93 … -4,57 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_byactiontype

Alle 257.040 Partien: ΔST -2,97 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 65 % | 0 | 100 % | -2,92 | -5,54 … -0,46 | 12,4 (6 … 17) |
| 10.000 | 100 % | 0 | 100 % | -2,98 | -3,69 … -2,29 | 25,3 (22 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,96 | -3,30 … -2,72 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,97 | -3,13 … -2,73 | 28,0 (28 … 28) |

### schildtraeger_hp_-10_defensive

Alle 257.040 Partien: ΔST -2,40 pp, Kompositionen in Richtung / dagegen 27 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 72 % | 0 | 100 % | -2,54 | -4,49 … -0,92 | 10,3 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | -2,41 | -3,25 … -1,66 | 23,9 (20 … 27) |
| 50.000 | 100 % | 0 | 100 % | -2,38 | -2,67 … -2,12 | 27,0 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,39 | -2,61 … -2,25 | 27,4 (26 … 28) |

### schildtraeger_hp_-10_uniform

Alle 257.040 Partien: ΔST -3,18 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 65 % | 0 | 100 % | -3,02 | -5,95 … -0,24 | 12,3 (5 … 19) |
| 10.000 | 100 % | 0 | 100 % | -3,20 | -3,77 … -2,57 | 25,5 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | -3,18 | -3,52 … -2,89 | 28,0 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -3,18 | -3,35 … -2,99 | 28,0 (28 … 28) |

### schildtraeger_hp_-2.5_aggressive

Alle 257.040 Partien: ΔST -1,33 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 34 % | 11 | 99 % | -1,28 | -2,57 … +0,00 | 6,4 (2 … 11) |
| 10.000 | 100 % | 0 | 100 % | -1,34 | -1,84 … -0,93 | 22,2 (18 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,34 | -1,51 … -1,15 | 27,5 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,33 | -1,44 … -1,23 | 27,9 (27 … 28) |

### schildtraeger_hp_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,71 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 13 | 87 % | -0,65 | -2,12 … +0,50 | 4,8 (0 … 9) |
| 10.000 | 89 % | 0 | 100 % | -0,70 | -1,05 … -0,34 | 17,4 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | -0,70 | -0,89 … -0,54 | 25,1 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,70 | -0,78 … -0,62 | 27,4 (26 … 28) |

### schildtraeger_hp_-2.5_defensive

Alle 257.040 Partien: ΔST -0,48 pp, Kompositionen in Richtung / dagegen 24 / 3, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 43 | 84 % | -0,54 | -1,89 … +0,24 | 3,6 (0 … 8) |
| 10.000 | 83 % | 0 | 100 % | -0,47 | -0,77 … -0,16 | 14,9 (10 … 22) |
| 50.000 | 100 % | 0 | 100 % | -0,48 | -0,61 … -0,37 | 21,5 (18 … 26) |
| 100.000 | 100 % | 0 | 100 % | -0,48 | -0,55 … -0,42 | 23,9 (21 … 27) |

### schildtraeger_hp_-2.5_uniform

Alle 257.040 Partien: ΔST -0,69 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 4 % | 19 | 85 % | -0,59 | -1,67 … +0,68 | 4,6 (1 … 9) |
| 10.000 | 89 % | 0 | 100 % | -0,70 | -1,01 … -0,34 | 17,1 (12 … 23) |
| 50.000 | 100 % | 0 | 100 % | -0,69 | -0,87 … -0,50 | 24,7 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | -0,68 | -0,78 … -0,59 | 27,1 (25 … 28) |

### schildtraeger_hp_-5_aggressive

Alle 257.040 Partien: ΔST -2,30 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 67 % | 0 | 100 % | -2,23 | -4,31 … -0,95 | 9,6 (4 … 15) |
| 10.000 | 100 % | 0 | 100 % | -2,29 | -2,88 … -1,75 | 25,1 (20 … 28) |
| 50.000 | 100 % | 0 | 100 % | -2,31 | -2,65 … -2,09 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | -2,30 | -2,40 … -2,16 | 28,0 (27 … 28) |

### schildtraeger_hp_-5_byactiontype

Alle 257.040 Partien: ΔST -1,46 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 21 % | 1 | 96 % | -1,36 | -2,67 … +0,45 | 8,0 (3 … 13) |
| 10.000 | 100 % | 0 | 100 % | -1,47 | -1,94 … -1,14 | 21,7 (18 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,45 | -1,64 … -1,25 | 27,3 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,44 | -1,56 … -1,30 | 27,9 (27 … 28) |

### schildtraeger_hp_-5_defensive

Alle 257.040 Partien: ΔST -1,11 pp, Kompositionen in Richtung / dagegen 26 / 2, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 20 % | 10 | 97 % | -1,19 | -2,38 … +0,23 | 6,3 (1 … 11) |
| 10.000 | 100 % | 0 | 100 % | -1,12 | -1,68 … -0,55 | 19,9 (16 … 25) |
| 50.000 | 100 % | 0 | 100 % | -1,10 | -1,24 … -0,93 | 25,4 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,11 | -1,21 … -0,99 | 26,3 (24 … 28) |

### schildtraeger_hp_-5_uniform

Alle 257.040 Partien: ΔST -1,48 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 22 % | 1 | 96 % | -1,34 | -2,68 … +0,24 | 7,9 (4 … 12) |
| 10.000 | 99 % | 0 | 100 % | -1,47 | -1,96 … -0,86 | 21,2 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | -1,48 | -1,73 … -1,22 | 27,3 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | -1,47 | -1,60 … -1,36 | 27,9 (27 … 28) |

### schildtraeger_hp_10_aggressive

Alle 257.040 Partien: ΔST +4,25 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 100 % | 0 | 100 % | +4,22 | +2,13 … +6,45 | 14,1 (10 … 19) |
| 10.000 | 100 % | 0 | 100 % | +4,33 | +3,48 … +5,05 | 27,2 (24 … 28) |
| 50.000 | 100 % | 0 | 100 % | +4,25 | +3,90 … +4,59 | 28,0 (28 … 28) |
| 100.000 | 100 % | 0 | 100 % | +4,25 | +4,07 … +4,51 | 28,0 (28 … 28) |

### schildtraeger_hp_10_byactiontype

Alle 257.040 Partien: ΔST +2,62 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 56 % | 0 | 99 % | +2,49 | -0,71 … +5,07 | 10,8 (5 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,66 | +1,80 … +3,40 | 24,9 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,61 | +2,36 … +2,83 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,61 | +2,44 … +2,82 | 28,0 (27 … 28) |

### schildtraeger_hp_10_defensive

Alle 257.040 Partien: ΔST +2,05 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 52 % | 0 | 100 % | +1,96 | +0,24 … +4,09 | 8,8 (3 … 16) |
| 10.000 | 100 % | 0 | 100 % | +2,04 | +1,52 … +2,62 | 23,1 (19 … 26) |
| 50.000 | 100 % | 0 | 100 % | +2,06 | +1,81 … +2,26 | 26,3 (24 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,05 | +1,95 … +2,16 | 26,9 (25 … 28) |

### schildtraeger_hp_10_uniform

Alle 257.040 Partien: ΔST +2,72 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 67 % | 0 | 100 % | +2,90 | +0,23 … +5,76 | 11,7 (7 … 19) |
| 10.000 | 100 % | 0 | 100 % | +2,71 | +2,12 … +3,39 | 25,1 (21 … 28) |
| 50.000 | 100 % | 0 | 100 % | +2,73 | +2,38 … +3,04 | 27,9 (27 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,74 | +2,55 … +2,91 | 28,0 (27 … 28) |

### schildtraeger_hp_2.5_aggressive

Alle 257.040 Partien: ΔST +1,10 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 18 % | 10 | 98 % | +1,08 | -0,24 … +2,38 | 6,0 (2 … 11) |
| 10.000 | 100 % | 0 | 100 % | +1,11 | +0,68 … +1,61 | 20,8 (17 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,11 | +0,98 … +1,26 | 26,7 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,10 | +0,97 … +1,20 | 27,7 (26 … 28) |

### schildtraeger_hp_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,71 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 8 % | 12 | 82 % | +0,60 | -0,48 … +1,67 | 5,0 (1 … 9) |
| 10.000 | 86 % | 0 | 100 % | +0,72 | +0,37 … +1,06 | 17,5 (12 … 21) |
| 50.000 | 100 % | 0 | 100 % | +0,71 | +0,53 … +0,89 | 25,0 (21 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,70 | +0,60 … +0,82 | 27,0 (24 … 28) |

### schildtraeger_hp_2.5_defensive

Alle 257.040 Partien: ΔST +0,65 pp, Kompositionen in Richtung / dagegen 27 / 1, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 45 | 92 % | +0,64 | +0,00 … +1,59 | 3,9 (1 … 8) |
| 10.000 | 94 % | 0 | 100 % | +0,65 | +0,30 … +1,03 | 16,4 (11 … 21) |
| 50.000 | 100 % | 0 | 100 % | +0,65 | +0,53 … +0,80 | 24,5 (21 … 27) |
| 100.000 | 100 % | 0 | 100 % | +0,65 | +0,58 … +0,73 | 26,2 (24 … 28) |

### schildtraeger_hp_2.5_uniform

Alle 257.040 Partien: ΔST +0,73 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 6 % | 14 | 88 % | +0,71 | -0,46 … +2,06 | 5,1 (0 … 10) |
| 10.000 | 87 % | 0 | 100 % | +0,71 | +0,43 … +1,18 | 17,5 (13 … 22) |
| 50.000 | 100 % | 0 | 100 % | +0,74 | +0,60 … +0,92 | 24,9 (20 … 28) |
| 100.000 | 100 % | 0 | 100 % | +0,73 | +0,63 … +0,84 | 27,0 (24 … 28) |

### schildtraeger_hp_5_aggressive

Alle 257.040 Partien: ΔST +2,05 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 56 % | 1 | 100 % | +2,01 | +0,44 … +3,92 | 8,9 (3 … 15) |
| 10.000 | 100 % | 0 | 100 % | +2,06 | +1,56 … +2,55 | 24,3 (20 … 27) |
| 50.000 | 100 % | 0 | 100 % | +2,06 | +1,84 … +2,31 | 27,7 (26 … 28) |
| 100.000 | 100 % | 0 | 100 % | +2,05 | +1,91 … +2,22 | 28,0 (27 … 28) |

### schildtraeger_hp_5_byactiontype

Alle 257.040 Partien: ΔST +1,35 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 17 % | 0 | 92 % | +1,21 | -0,45 … +3,69 | 7,6 (3 … 13) |
| 10.000 | 100 % | 0 | 100 % | +1,37 | +0,85 … +1,94 | 21,0 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,35 | +1,14 … +1,50 | 27,1 (25 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,34 | +1,20 … +1,50 | 27,9 (26 … 28) |

### schildtraeger_hp_5_defensive

Alle 257.040 Partien: ΔST +1,15 pp, Kompositionen in Richtung / dagegen 27 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 26 % | 12 | 97 % | +1,09 | +0,00 … +2,74 | 5,9 (1 … 12) |
| 10.000 | 99 % | 0 | 100 % | +1,12 | +0,67 … +1,66 | 20,1 (16 … 23) |
| 50.000 | 100 % | 0 | 100 % | +1,15 | +0,95 … +1,33 | 25,7 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,15 | +1,07 … +1,26 | 26,6 (24 … 28) |

### schildtraeger_hp_5_uniform

Alle 257.040 Partien: ΔST +1,39 pp, Kompositionen in Richtung / dagegen 28 / 0, erkannt.

| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |
|---|---|---|---|---|---|---|
| 1.000 | 29 % | 1 | 98 % | +1,46 | -0,69 … +3,11 | 8,0 (3 … 14) |
| 10.000 | 100 % | 0 | 100 % | +1,40 | +0,87 … +2,06 | 21,0 (16 … 26) |
| 50.000 | 100 % | 0 | 100 % | +1,41 | +1,22 … +1,62 | 27,1 (23 … 28) |
| 100.000 | 100 % | 0 | 100 % | +1,40 | +1,22 … +1,56 | 27,9 (27 … 28) |

