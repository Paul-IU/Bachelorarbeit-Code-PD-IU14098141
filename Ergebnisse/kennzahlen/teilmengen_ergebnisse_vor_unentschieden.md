# Simulationsumfang über Teilmengen (Schritt 6)

Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: `numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.

ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp). Richtung = Anteil der Wiederholungen, in denen ΔST das Vorzeichen der Manipulation hat. Kompositionen = Anteil der 28 Kompositionen mit Schildträger, die sich in Richtung der Manipulation verschieben.

## Übersicht: kleinste Stichprobe, ab der der Schildträger in ≥ 95 % der Wiederholungen richtig verschoben ist

| Variante | 1.000 | 10.000 | 50.000 | 100.000 | ab |
|---|---|---|---|---|---|
| schildtraeger_ability_-10_aggressive | 42 % | 24 % | 15 % | 3 % | nicht erreicht |
| schildtraeger_ability_-10_byactiontype | 60 % | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_-10_defensive | 70 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_-10_uniform | 72 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_-2.5_aggressive | 34 % | 63 % | 98 % | 100 % | 50.000 |
| schildtraeger_ability_-2.5_byactiontype | 17 % | 72 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-2.5_defensive | 49 % | 79 % | 96 % | 99 % | 50.000 |
| schildtraeger_ability_-2.5_uniform | 35 % | 96 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_-5_aggressive | 45 % | 68 % | 96 % | 99 % | 50.000 |
| schildtraeger_ability_-5_byactiontype | 32 % | 81 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-5_defensive | 51 % | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_-5_uniform | 57 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_10_aggressive | 22 % | 5 % | 0 % | 0 % | nicht erreicht |
| schildtraeger_ability_10_byactiontype | 64 % | 98 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_10_defensive | 57 % | 59 % | 75 % | 89 % | nicht erreicht |
| schildtraeger_ability_10_uniform | 77 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_2.5_aggressive | 32 % | 50 % | 53 % | 52 % | nicht erreicht |
| schildtraeger_ability_2.5_byactiontype | 45 % | 85 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_2.5_defensive | 31 % | 7 % | 0 % | 0 % | nicht erreicht |
| schildtraeger_ability_2.5_uniform | 57 % | 97 % | 100 % | 100 % | 10.000 |
| schildtraeger_ability_5_aggressive | 30 % | 29 % | 9 % | 1 % | nicht erreicht |
| schildtraeger_ability_5_byactiontype | 55 % | 94 % | 100 % | 100 % | 50.000 |
| schildtraeger_ability_5_defensive | 46 % | 16 % | 2 % | 0 % | nicht erreicht |
| schildtraeger_ability_5_uniform | 69 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-10_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-10_byactiontype | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-10_defensive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-10_uniform | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-2.5_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-2.5_byactiontype | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-2.5_defensive | 92 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_-2.5_uniform | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-5_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-5_byactiontype | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-5_defensive | 97 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_-5_uniform | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_10_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_10_byactiontype | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_10_defensive | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_10_uniform | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_2.5_aggressive | 84 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_byactiontype | 92 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_defensive | 68 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_2.5_uniform | 87 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_20_byactiontype | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_5_aggressive | 97 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_5_byactiontype | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_dmg_5_defensive | 88 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_dmg_5_uniform | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_byactiontype | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_defensive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-10_uniform | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-2.5_aggressive | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-2.5_byactiontype | 87 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_defensive | 84 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-2.5_uniform | 85 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_-5_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-5_byactiontype | 96 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-5_defensive | 97 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_-5_uniform | 96 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_byactiontype | 99 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_defensive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_10_uniform | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_2.5_aggressive | 98 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_2.5_byactiontype | 82 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_defensive | 92 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_2.5_uniform | 88 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_aggressive | 100 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_5_byactiontype | 92 % | 100 % | 100 % | 100 % | 10.000 |
| schildtraeger_hp_5_defensive | 97 % | 100 % | 100 % | 100 % | 1.000 |
| schildtraeger_hp_5_uniform | 98 % | 100 % | 100 % | 100 % | 1.000 |

## Einzelwerte je Variante

### schildtraeger_ability_-10_aggressive

Alle 257.040 Partien: ΔST +0,06 pp, Kompositionen in Richtung 50,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,07 | -1,35 … +2,50 | 42 % | 11,9 % | 0,0 … 25,0 % |
| 10.000 | +0,08 | -0,25 … +0,52 | 24 % | 38,0 % | 21,4 … 60,7 % |
| 50.000 | +0,06 | -0,12 … +0,29 | 15 % | 47,0 % | 32,1 … 67,9 % |
| 100.000 | +0,07 | -0,05 … +0,17 | 3 % | 48,4 % | 39,3 … 60,7 % |

### schildtraeger_ability_-10_byactiontype

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung 85,7 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,19 | -1,36 … +0,69 | 60 % | 8,6 % | 0,0 … 21,4 % |
| 10.000 | -0,23 | -0,53 … +0,05 | 98 % | 43,6 % | 21,4 … 64,3 % |
| 50.000 | -0,23 | -0,33 … -0,13 | 100 % | 65,5 % | 46,4 … 82,1 % |
| 100.000 | -0,23 | -0,32 … -0,16 | 100 % | 77,4 % | 60,7 … 89,3 % |

### schildtraeger_ability_-10_defensive

Alle 257.040 Partien: ΔST -0,60 pp, Kompositionen in Richtung 78,6 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,62 | -3,08 … +1,34 | 70 % | 26,1 % | 10,7 … 50,0 % |
| 10.000 | -0,60 | -1,16 … -0,05 | 100 % | 55,6 % | 35,7 … 71,4 % |
| 50.000 | -0,59 | -0,95 … -0,37 | 100 % | 67,9 % | 53,6 … 82,1 % |
| 100.000 | -0,60 | -0,74 … -0,43 | 100 % | 73,3 % | 64,3 … 92,9 % |

### schildtraeger_ability_-10_uniform

Alle 257.040 Partien: ΔST -0,48 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,36 | -1,67 … +0,47 | 72 % | 12,7 % | 0,0 … 32,1 % |
| 10.000 | -0,47 | -0,80 … -0,18 | 100 % | 53,1 % | 32,1 … 71,4 % |
| 50.000 | -0,48 | -0,63 … -0,37 | 100 % | 82,4 % | 67,9 … 96,4 % |
| 100.000 | -0,48 | -0,58 … -0,37 | 100 % | 91,0 % | 78,6 … 100,0 % |

### schildtraeger_ability_-2.5_aggressive

Alle 257.040 Partien: ΔST -0,05 pp, Kompositionen in Richtung 64,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,06 | -0,47 … +0,48 | 34 % | 2,9 % | 0,0 … 10,7 % |
| 10.000 | -0,03 | -0,20 … +0,09 | 63 % | 18,7 % | 7,1 … 39,3 % |
| 50.000 | -0,05 | -0,10 … +0,00 | 98 % | 42,5 % | 21,4 … 57,1 % |
| 100.000 | -0,05 | -0,08 … -0,01 | 100 % | 52,2 % | 35,7 … 67,9 % |

### schildtraeger_ability_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,05 pp, Kompositionen in Richtung 57,1 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,02 | -0,24 … +0,25 | 17 % | 1,4 % | 0,0 … 10,7 % |
| 10.000 | -0,04 | -0,20 … +0,07 | 72 % | 15,8 % | 3,6 … 35,7 % |
| 50.000 | -0,04 | -0,10 … -0,01 | 100 % | 38,4 % | 21,4 … 60,7 % |
| 100.000 | -0,04 | -0,08 … -0,01 | 100 % | 50,9 % | 35,7 … 71,4 % |

### schildtraeger_ability_-2.5_defensive

Alle 257.040 Partien: ΔST -0,12 pp, Kompositionen in Richtung 50,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,16 | -1,38 … +1,18 | 49 % | 13,3 % | 0,0 … 32,1 % |
| 10.000 | -0,12 | -0,60 … +0,22 | 79 % | 40,8 % | 25,0 … 67,9 % |
| 50.000 | -0,12 | -0,35 … +0,02 | 96 % | 51,1 % | 32,1 … 67,9 % |
| 100.000 | -0,12 | -0,21 … +0,02 | 99 % | 53,6 % | 39,3 … 75,0 % |

### schildtraeger_ability_-2.5_uniform

Alle 257.040 Partien: ΔST -0,09 pp, Kompositionen in Richtung 78,6 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,08 | -0,48 … +0,45 | 35 % | 2,7 % | 0,0 … 10,7 % |
| 10.000 | -0,09 | -0,28 … +0,05 | 96 % | 21,6 % | 3,6 … 35,7 % |
| 50.000 | -0,09 | -0,14 … -0,03 | 100 % | 53,0 % | 32,1 … 71,4 % |
| 100.000 | -0,09 | -0,12 … -0,03 | 100 % | 63,7 % | 46,4 … 78,6 % |

### schildtraeger_ability_-5_aggressive

Alle 257.040 Partien: ΔST -0,07 pp, Kompositionen in Richtung 64,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,07 | -1,13 … +1,14 | 45 % | 6,8 % | 0,0 … 21,4 % |
| 10.000 | -0,05 | -0,28 … +0,34 | 68 % | 31,8 % | 7,1 … 46,4 % |
| 50.000 | -0,07 | -0,16 … +0,02 | 96 % | 50,2 % | 39,3 … 67,9 % |
| 100.000 | -0,06 | -0,13 … +0,01 | 99 % | 54,5 % | 35,7 … 71,4 % |

### schildtraeger_ability_-5_byactiontype

Alle 257.040 Partien: ΔST -0,09 pp, Kompositionen in Richtung 75,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,06 | -0,70 … +0,46 | 32 % | 3,9 % | 0,0 … 10,7 % |
| 10.000 | -0,09 | -0,32 … +0,07 | 81 % | 28,7 % | 14,3 … 46,4 % |
| 50.000 | -0,09 | -0,15 … -0,01 | 100 % | 53,0 % | 35,7 … 71,4 % |
| 100.000 | -0,09 | -0,15 … -0,04 | 100 % | 60,9 % | 42,9 … 75,0 % |

### schildtraeger_ability_-5_defensive

Alle 257.040 Partien: ΔST -0,26 pp, Kompositionen in Richtung 71,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,25 | -1,82 … +0,94 | 51 % | 18,5 % | 3,6 … 42,9 % |
| 10.000 | -0,24 | -0,63 … +0,25 | 94 % | 46,1 % | 28,6 … 64,3 % |
| 50.000 | -0,25 | -0,53 … -0,10 | 100 % | 57,2 % | 35,7 … 71,4 % |
| 100.000 | -0,26 | -0,39 … -0,10 | 100 % | 61,7 % | 50,0 … 75,0 % |

### schildtraeger_ability_-5_uniform

Alle 257.040 Partien: ΔST -0,23 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,21 | -1,43 … +0,47 | 57 % | 7,2 % | 0,0 … 25,0 % |
| 10.000 | -0,23 | -0,50 … -0,05 | 100 % | 40,4 % | 21,4 … 64,3 % |
| 50.000 | -0,24 | -0,33 … -0,14 | 100 % | 69,8 % | 50,0 … 89,3 % |
| 100.000 | -0,24 | -0,28 … -0,17 | 100 % | 82,8 % | 64,3 … 92,9 % |

### schildtraeger_ability_10_aggressive

Alle 257.040 Partien: ΔST -0,27 pp, Kompositionen in Richtung 39,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,30 | -1,78 … +1,90 | 22 % | 13,0 % | 0,0 … 28,6 % |
| 10.000 | -0,26 | -0,75 … +0,21 | 5 % | 33,3 % | 14,3 … 57,1 % |
| 50.000 | -0,28 | -0,48 … -0,07 | 0 % | 35,1 % | 21,4 … 57,1 % |
| 100.000 | -0,28 | -0,39 … -0,17 | 0 % | 35,2 % | 25,0 … 46,4 % |

### schildtraeger_ability_10_byactiontype

Alle 257.040 Partien: ΔST +0,31 pp, Kompositionen in Richtung 89,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,27 | -0,72 … +1,67 | 64 % | 11,7 % | 3,6 … 28,6 % |
| 10.000 | +0,31 | -0,09 … +0,64 | 98 % | 48,5 % | 21,4 … 71,4 % |
| 50.000 | +0,30 | +0,16 … +0,44 | 100 % | 70,6 % | 53,6 … 89,3 % |
| 100.000 | +0,30 | +0,21 … +0,38 | 100 % | 79,2 % | 64,3 … 96,4 % |

### schildtraeger_ability_10_defensive

Alle 257.040 Partien: ΔST +0,09 pp, Kompositionen in Richtung 60,7 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,28 | -1,69 … +2,54 | 57 % | 27,6 % | 10,7 … 46,4 % |
| 10.000 | +0,07 | -0,45 … +0,70 | 59 % | 45,4 % | 28,6 … 64,3 % |
| 50.000 | +0,09 | -0,20 … +0,39 | 75 % | 50,8 % | 35,7 … 64,3 % |
| 100.000 | +0,09 | -0,05 … +0,36 | 89 % | 53,5 % | 35,7 … 67,9 % |

### schildtraeger_ability_10_uniform

Alle 257.040 Partien: ΔST +0,56 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,52 | -0,70 … +1,90 | 77 % | 16,2 % | 3,6 … 32,1 % |
| 10.000 | +0,56 | +0,21 … +0,94 | 100 % | 58,0 % | 42,9 … 78,6 % |
| 50.000 | +0,58 | +0,44 … +0,83 | 100 % | 85,0 % | 71,4 … 100,0 % |
| 100.000 | +0,56 | +0,45 … +0,66 | 100 % | 93,7 % | 82,1 … 100,0 % |

### schildtraeger_ability_2.5_aggressive

Alle 257.040 Partien: ΔST +0,01 pp, Kompositionen in Richtung 53,6 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,01 | -0,72 … +1,16 | 32 % | 5,8 % | 0,0 … 17,9 % |
| 10.000 | +0,01 | -0,23 … +0,27 | 50 % | 31,8 % | 14,3 … 50,0 % |
| 50.000 | +0,00 | -0,12 … +0,11 | 53 % | 45,2 % | 25,0 … 60,7 % |
| 100.000 | +0,00 | -0,08 … +0,07 | 52 % | 48,3 % | 32,1 … 64,3 % |

### schildtraeger_ability_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,11 pp, Kompositionen in Richtung 75,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,07 | -0,72 … +0,93 | 45 % | 5,9 % | 0,0 … 17,9 % |
| 10.000 | +0,11 | -0,07 … +0,37 | 85 % | 34,2 % | 17,9 … 57,1 % |
| 50.000 | +0,11 | +0,01 … +0,19 | 100 % | 55,7 % | 35,7 … 75,0 % |
| 100.000 | +0,11 | +0,06 … +0,18 | 100 % | 65,0 % | 50,0 … 82,1 % |

### schildtraeger_ability_2.5_defensive

Alle 257.040 Partien: ΔST -0,29 pp, Kompositionen in Richtung 32,1 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,23 | -1,82 … +1,59 | 31 % | 18,2 % | 7,1 … 28,6 % |
| 10.000 | -0,31 | -0,79 … +0,09 | 7 % | 34,2 % | 14,3 … 53,6 % |
| 50.000 | -0,29 | -0,46 … -0,09 | 0 % | 33,5 % | 17,9 … 50,0 % |
| 100.000 | -0,28 | -0,37 … -0,12 | 0 % | 32,8 % | 21,4 … 46,4 % |

### schildtraeger_ability_2.5_uniform

Alle 257.040 Partien: ΔST +0,19 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,18 | -0,48 … +1,18 | 57 % | 7,0 % | 0,0 … 21,4 % |
| 10.000 | +0,19 | -0,09 … +0,44 | 97 % | 39,9 % | 25,0 … 64,3 % |
| 50.000 | +0,20 | +0,11 … +0,31 | 100 % | 66,2 % | 50,0 … 85,7 % |
| 100.000 | +0,19 | +0,13 … +0,26 | 100 % | 77,9 % | 57,1 … 96,4 % |

### schildtraeger_ability_5_aggressive

Alle 257.040 Partien: ΔST -0,07 pp, Kompositionen in Richtung 39,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,11 | -0,99 … +1,19 | 30 % | 9,1 % | 0,0 … 28,6 % |
| 10.000 | -0,07 | -0,36 … +0,30 | 29 % | 34,1 % | 10,7 … 57,1 % |
| 50.000 | -0,08 | -0,29 … +0,07 | 9 % | 37,1 % | 25,0 … 60,7 % |
| 100.000 | -0,07 | -0,17 … +0,03 | 1 % | 38,0 % | 21,4 … 50,0 % |

### schildtraeger_ability_5_byactiontype

Alle 257.040 Partien: ΔST +0,17 pp, Kompositionen in Richtung 82,1 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,15 | -0,72 … +1,16 | 55 % | 8,3 % | 0,0 … 25,0 % |
| 10.000 | +0,18 | -0,11 … +0,43 | 94 % | 40,7 % | 25,0 … 57,1 % |
| 50.000 | +0,17 | +0,05 … +0,29 | 100 % | 61,5 % | 42,9 … 85,7 % |
| 100.000 | +0,17 | +0,11 … +0,24 | 100 % | 72,3 % | 53,6 … 92,9 % |

### schildtraeger_ability_5_defensive

Alle 257.040 Partien: ΔST -0,18 pp, Kompositionen in Richtung 42,9 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,00 | -1,59 … +1,83 | 46 % | 22,9 % | 7,1 … 39,3 % |
| 10.000 | -0,21 | -0,65 … +0,27 | 16 % | 38,6 % | 21,4 … 57,1 % |
| 50.000 | -0,18 | -0,47 … +0,05 | 2 % | 41,4 % | 17,9 … 60,7 % |
| 100.000 | -0,18 | -0,28 … -0,02 | 0 % | 41,3 % | 28,6 … 60,7 % |

### schildtraeger_ability_5_uniform

Alle 257.040 Partien: ΔST +0,33 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,32 | -0,48 … +1,42 | 69 % | 10,9 % | 0,0 … 25,0 % |
| 10.000 | +0,33 | +0,07 … +0,64 | 100 % | 49,9 % | 32,1 … 64,3 % |
| 50.000 | +0,34 | +0,21 … +0,52 | 100 % | 75,2 % | 57,1 … 96,4 % |
| 100.000 | +0,33 | +0,24 … +0,41 | 100 % | 86,2 % | 71,4 … 100,0 % |

### schildtraeger_dmg_-10_aggressive

Alle 257.040 Partien: ΔST -5,13 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -5,11 | -7,38 … -2,86 | 100 % | 54,7 % | 35,7 … 75,0 % |
| 10.000 | -5,15 | -5,96 … -4,11 | 100 % | 94,2 % | 85,7 … 100,0 % |
| 50.000 | -5,11 | -5,54 … -4,76 | 100 % | 99,6 % | 96,4 … 100,0 % |
| 100.000 | -5,14 | -5,34 … -4,91 | 100 % | 99,8 % | 96,4 … 100,0 % |

### schildtraeger_dmg_-10_byactiontype

Alle 257.040 Partien: ΔST -4,11 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -4,21 | -7,26 … -1,10 | 100 % | 52,0 % | 35,7 … 75,0 % |
| 10.000 | -4,07 | -4,99 … -2,91 | 100 % | 91,9 % | 82,1 … 100,0 % |
| 50.000 | -4,13 | -4,51 … -3,82 | 100 % | 99,8 % | 96,4 … 100,0 % |
| 100.000 | -4,13 | -4,43 … -3,88 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_dmg_-10_defensive

Alle 257.040 Partien: ΔST -3,47 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -3,38 | -5,99 … -0,46 | 100 % | 46,8 % | 28,6 … 67,9 % |
| 10.000 | -3,50 | -4,31 … -2,69 | 100 % | 84,7 % | 71,4 … 92,9 % |
| 50.000 | -3,48 | -3,91 … -3,14 | 100 % | 93,9 % | 85,7 … 100,0 % |
| 100.000 | -3,48 | -3,79 … -3,18 | 100 % | 96,5 % | 89,3 … 100,0 % |

### schildtraeger_dmg_-10_uniform

Alle 257.040 Partien: ΔST -4,20 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -4,24 | -7,57 … -0,68 | 100 % | 52,3 % | 32,1 … 75,0 % |
| 10.000 | -4,19 | -5,21 … -3,24 | 100 % | 92,5 % | 82,1 … 100,0 % |
| 50.000 | -4,15 | -4,58 … -3,65 | 100 % | 99,8 % | 96,4 … 100,0 % |
| 100.000 | -4,20 | -4,44 … -3,97 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_dmg_-2.5_aggressive

Alle 257.040 Partien: ΔST -2,23 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,31 | -4,09 … -0,47 | 100 % | 35,1 % | 17,9 … 57,1 % |
| 10.000 | -2,22 | -2,67 … -1,46 | 100 % | 83,0 % | 67,9 … 96,4 % |
| 50.000 | -2,23 | -2,48 … -1,98 | 100 % | 98,0 % | 89,3 … 100,0 % |
| 100.000 | -2,24 | -2,41 … -2,06 | 100 % | 99,5 % | 92,9 … 100,0 % |

### schildtraeger_dmg_-2.5_byactiontype

Alle 257.040 Partien: ΔST -1,59 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,60 | -3,85 … +0,00 | 99 % | 32,5 % | 14,3 … 57,1 % |
| 10.000 | -1,59 | -2,37 … -0,89 | 100 % | 76,0 % | 57,1 … 92,9 % |
| 50.000 | -1,59 | -1,82 … -1,33 | 100 % | 96,2 % | 85,7 … 100,0 % |
| 100.000 | -1,59 | -1,76 … -1,48 | 100 % | 99,4 % | 92,9 … 100,0 % |

### schildtraeger_dmg_-2.5_defensive

Alle 257.040 Partien: ΔST -1,50 pp, Kompositionen in Richtung 89,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,44 | -3,62 … +0,91 | 92 % | 31,8 % | 17,9 … 50,0 % |
| 10.000 | -1,54 | -2,23 … -0,73 | 100 % | 69,4 % | 46,4 … 85,7 % |
| 50.000 | -1,52 | -1,76 … -1,29 | 100 % | 81,2 % | 71,4 … 92,9 % |
| 100.000 | -1,51 | -1,70 … -1,30 | 100 % | 85,0 % | 71,4 … 92,9 % |

### schildtraeger_dmg_-2.5_uniform

Alle 257.040 Partien: ΔST -1,71 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,75 | -3,79 … +0,00 | 98 % | 33,9 % | 14,3 … 53,6 % |
| 10.000 | -1,70 | -2,26 … -1,15 | 100 % | 76,5 % | 60,7 … 92,9 % |
| 50.000 | -1,70 | -1,95 … -1,47 | 100 % | 96,3 % | 89,3 … 100,0 % |
| 100.000 | -1,71 | -1,87 … -1,57 | 100 % | 99,4 % | 96,4 … 100,0 % |

### schildtraeger_dmg_-5_aggressive

Alle 257.040 Partien: ΔST -3,40 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -3,43 | -5,92 … -1,09 | 100 % | 45,7 % | 25,0 … 67,9 % |
| 10.000 | -3,42 | -4,05 … -2,71 | 100 % | 88,3 % | 75,0 … 100,0 % |
| 50.000 | -3,39 | -3,77 … -3,15 | 100 % | 98,2 % | 92,9 … 100,0 % |
| 100.000 | -3,41 | -3,64 … -3,25 | 100 % | 98,9 % | 96,4 … 100,0 % |

### schildtraeger_dmg_-5_byactiontype

Alle 257.040 Partien: ΔST -2,54 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,52 | -5,90 … -0,22 | 100 % | 41,4 % | 21,4 … 67,9 % |
| 10.000 | -2,48 | -3,21 … -1,62 | 100 % | 84,3 % | 67,9 … 96,4 % |
| 50.000 | -2,55 | -2,83 … -2,22 | 100 % | 99,0 % | 92,9 … 100,0 % |
| 100.000 | -2,54 | -2,74 … -2,34 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_dmg_-5_defensive

Alle 257.040 Partien: ΔST -2,19 pp, Kompositionen in Richtung 85,7 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,07 | -4,99 … +0,66 | 97 % | 37,7 % | 17,9 … 57,1 % |
| 10.000 | -2,22 | -3,06 … -1,44 | 100 % | 75,3 % | 53,6 … 89,3 % |
| 50.000 | -2,21 | -2,56 … -1,91 | 100 % | 87,1 % | 75,0 … 96,4 % |
| 100.000 | -2,21 | -2,46 … -1,98 | 100 % | 88,0 % | 78,6 … 96,4 % |

### schildtraeger_dmg_-5_uniform

Alle 257.040 Partien: ΔST -2,69 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,70 | -4,82 … +0,22 | 99 % | 43,4 % | 21,4 … 60,7 % |
| 10.000 | -2,66 | -3,43 … -1,97 | 100 % | 84,5 % | 71,4 … 96,4 % |
| 50.000 | -2,67 | -2,99 … -2,40 | 100 % | 98,9 % | 92,9 … 100,0 % |
| 100.000 | -2,69 | -2,88 … -2,48 | 100 % | 99,9 % | 96,4 … 100,0 % |

### schildtraeger_dmg_10_aggressive

Alle 257.040 Partien: ΔST +2,78 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,75 | +0,70 … +5,15 | 100 % | 40,1 % | 21,4 … 57,1 % |
| 10.000 | +2,84 | +2,32 … +3,52 | 100 % | 82,6 % | 67,9 … 96,4 % |
| 50.000 | +2,77 | +2,48 … +3,02 | 100 % | 95,1 % | 85,7 … 100,0 % |
| 100.000 | +2,78 | +2,57 … +2,98 | 100 % | 97,4 % | 92,9 … 100,0 % |

### schildtraeger_dmg_10_byactiontype

Alle 257.040 Partien: ΔST +2,83 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,85 | +0,24 … +5,33 | 100 % | 44,5 % | 28,6 … 64,3 % |
| 10.000 | +2,86 | +2,22 … +3,89 | 100 % | 87,0 % | 67,9 … 96,4 % |
| 50.000 | +2,84 | +2,50 … +3,21 | 100 % | 99,2 % | 92,9 … 100,0 % |
| 100.000 | +2,84 | +2,55 … +3,10 | 100 % | 100,0 % | 96,4 … 100,0 % |

### schildtraeger_dmg_10_defensive

Alle 257.040 Partien: ΔST +2,57 pp, Kompositionen in Richtung 92,9 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,61 | -1,14 … +4,89 | 98 % | 43,0 % | 21,4 … 60,7 % |
| 10.000 | +2,58 | +1,80 … +3,35 | 100 % | 81,5 % | 67,9 … 96,4 % |
| 50.000 | +2,58 | +2,28 … +2,92 | 100 % | 91,7 % | 85,7 … 100,0 % |
| 100.000 | +2,57 | +2,38 … +2,77 | 100 % | 92,2 % | 89,3 … 96,4 % |

### schildtraeger_dmg_10_uniform

Alle 257.040 Partien: ΔST +3,00 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +3,24 | +0,68 … +6,72 | 100 % | 46,2 % | 28,6 … 71,4 % |
| 10.000 | +3,05 | +2,14 … +3,83 | 100 % | 87,1 % | 67,9 … 100,0 % |
| 50.000 | +3,02 | +2,73 … +3,33 | 100 % | 99,2 % | 92,9 … 100,0 % |
| 100.000 | +3,01 | +2,78 … +3,18 | 100 % | 99,9 % | 96,4 … 100,0 % |

### schildtraeger_dmg_2.5_aggressive

Alle 257.040 Partien: ΔST +0,52 pp, Kompositionen in Richtung 82,1 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,52 | -0,24 … +1,67 | 84 % | 11,7 % | 0,0 … 28,6 % |
| 10.000 | +0,52 | +0,30 … +0,92 | 100 % | 52,4 % | 35,7 … 75,0 % |
| 50.000 | +0,52 | +0,41 … +0,63 | 100 % | 73,5 % | 60,7 … 85,7 % |
| 100.000 | +0,52 | +0,43 … +0,61 | 100 % | 78,2 % | 67,9 … 85,7 % |

### schildtraeger_dmg_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,64 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,66 | -0,70 … +2,00 | 92 % | 16,6 % | 3,6 … 35,7 % |
| 10.000 | +0,66 | +0,28 … +1,12 | 100 % | 59,7 % | 39,3 … 89,3 % |
| 50.000 | +0,63 | +0,50 … +0,77 | 100 % | 87,8 % | 67,9 … 100,0 % |
| 100.000 | +0,65 | +0,53 … +0,73 | 100 % | 94,9 % | 82,1 … 100,0 % |

### schildtraeger_dmg_2.5_defensive

Alle 257.040 Partien: ΔST +0,57 pp, Kompositionen in Richtung 82,1 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,57 | -1,59 … +2,31 | 68 % | 23,7 % | 7,1 … 42,9 % |
| 10.000 | +0,58 | +0,05 … +1,11 | 100 % | 58,1 % | 35,7 … 78,6 % |
| 50.000 | +0,57 | +0,37 … +0,79 | 100 % | 75,3 % | 60,7 … 100,0 % |
| 100.000 | +0,57 | +0,44 … +0,70 | 100 % | 77,9 % | 64,3 … 92,9 % |

### schildtraeger_dmg_2.5_uniform

Alle 257.040 Partien: ΔST +0,69 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,69 | -0,42 … +1,87 | 87 % | 17,6 % | 3,6 … 32,1 % |
| 10.000 | +0,70 | +0,21 … +1,10 | 100 % | 60,3 % | 42,9 … 85,7 % |
| 50.000 | +0,70 | +0,51 … +0,84 | 100 % | 89,3 % | 75,0 … 100,0 % |
| 100.000 | +0,69 | +0,61 … +0,78 | 100 % | 95,3 % | 89,3 … 100,0 % |

### schildtraeger_dmg_20_byactiontype

Alle 257.040 Partien: ΔST +6,56 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +6,44 | +3,49 … +10,67 | 100 % | 61,9 % | 42,9 … 78,6 % |
| 10.000 | +6,57 | +5,65 … +7,72 | 100 % | 97,5 % | 89,3 … 100,0 % |
| 50.000 | +6,56 | +6,20 … +6,96 | 100 % | 100,0 % | 100,0 … 100,0 % |
| 100.000 | +6,56 | +6,25 … +6,84 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_dmg_5_aggressive

Alle 257.040 Partien: ΔST +1,20 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,22 | -0,23 … +3,13 | 97 % | 25,2 % | 10,7 … 42,9 % |
| 10.000 | +1,22 | +0,64 … +1,69 | 100 % | 70,7 % | 57,1 … 85,7 % |
| 50.000 | +1,19 | +0,96 … +1,51 | 100 % | 86,9 % | 71,4 … 100,0 % |
| 100.000 | +1,21 | +1,08 … +1,34 | 100 % | 92,7 % | 82,1 … 100,0 % |

### schildtraeger_dmg_5_byactiontype

Alle 257.040 Partien: ΔST +1,46 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,48 | -0,23 … +4,15 | 98 % | 31,3 % | 14,3 … 53,6 % |
| 10.000 | +1,48 | +0,86 … +2,08 | 100 % | 75,0 % | 60,7 … 89,3 % |
| 50.000 | +1,47 | +1,26 … +1,70 | 100 % | 96,8 % | 89,3 … 100,0 % |
| 100.000 | +1,46 | +1,32 … +1,62 | 100 % | 99,3 % | 96,4 … 100,0 % |

### schildtraeger_dmg_5_defensive

Alle 257.040 Partien: ΔST +1,27 pp, Kompositionen in Richtung 89,3 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,31 | -1,36 … +3,60 | 88 % | 32,7 % | 10,7 … 53,6 % |
| 10.000 | +1,26 | +0,69 … +1,73 | 100 % | 69,4 % | 50,0 … 82,1 % |
| 50.000 | +1,27 | +0,99 … +1,56 | 100 % | 85,5 % | 75,0 … 92,9 % |
| 100.000 | +1,27 | +1,13 … +1,43 | 100 % | 88,0 % | 78,6 … 92,9 % |

### schildtraeger_dmg_5_uniform

Alle 257.040 Partien: ΔST +1,60 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,72 | -0,23 … +4,02 | 98 % | 33,2 % | 17,9 … 50,0 % |
| 10.000 | +1,66 | +0,91 … +2,20 | 100 % | 77,1 % | 57,1 … 92,9 % |
| 50.000 | +1,61 | +1,31 … +1,86 | 100 % | 97,2 % | 89,3 … 100,0 % |
| 100.000 | +1,60 | +1,46 … +1,75 | 100 % | 99,4 % | 96,4 … 100,0 % |

### schildtraeger_hp_-10_aggressive

Alle 257.040 Partien: ΔST -4,73 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -4,67 | -7,57 … -2,75 | 100 % | 52,2 % | 35,7 … 67,9 % |
| 10.000 | -4,70 | -5,62 … -4,00 | 100 % | 97,4 % | 89,3 … 100,0 % |
| 50.000 | -4,73 | -5,06 … -4,49 | 100 % | 99,9 % | 96,4 … 100,0 % |
| 100.000 | -4,73 | -4,93 … -4,57 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_hp_-10_byactiontype

Alle 257.040 Partien: ΔST -2,97 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,92 | -5,54 … -0,46 | 100 % | 44,2 % | 21,4 … 60,7 % |
| 10.000 | -2,98 | -3,69 … -2,29 | 100 % | 90,5 % | 78,6 … 100,0 % |
| 50.000 | -2,96 | -3,30 … -2,72 | 100 % | 99,7 % | 96,4 … 100,0 % |
| 100.000 | -2,97 | -3,13 … -2,73 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_hp_-10_defensive

Alle 257.040 Partien: ΔST -2,40 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,54 | -4,49 … -0,92 | 100 % | 36,7 % | 17,9 … 57,1 % |
| 10.000 | -2,41 | -3,25 … -1,66 | 100 % | 85,0 % | 71,4 … 96,4 % |
| 50.000 | -2,38 | -2,67 … -2,12 | 100 % | 96,4 % | 89,3 … 100,0 % |
| 100.000 | -2,39 | -2,61 … -2,25 | 100 % | 97,7 % | 92,9 … 100,0 % |

### schildtraeger_hp_-10_uniform

Alle 257.040 Partien: ΔST -3,18 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -3,02 | -5,95 … -0,24 | 100 % | 44,0 % | 17,9 … 67,9 % |
| 10.000 | -3,20 | -3,77 … -2,57 | 100 % | 90,9 % | 75,0 … 100,0 % |
| 50.000 | -3,18 | -3,52 … -2,89 | 100 % | 99,9 % | 96,4 … 100,0 % |
| 100.000 | -3,18 | -3,35 … -2,99 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_hp_-2.5_aggressive

Alle 257.040 Partien: ΔST -1,33 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,28 | -2,57 … +0,00 | 99 % | 22,8 % | 7,1 … 39,3 % |
| 10.000 | -1,34 | -1,84 … -0,93 | 100 % | 79,4 % | 64,3 … 92,9 % |
| 50.000 | -1,34 | -1,51 … -1,15 | 100 % | 98,1 % | 89,3 … 100,0 % |
| 100.000 | -1,33 | -1,44 … -1,23 | 100 % | 99,6 % | 96,4 … 100,0 % |

### schildtraeger_hp_-2.5_byactiontype

Alle 257.040 Partien: ΔST -0,71 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,65 | -2,12 … +0,50 | 87 % | 17,1 % | 0,0 … 32,1 % |
| 10.000 | -0,69 | -1,05 … -0,34 | 100 % | 62,1 % | 42,9 … 82,1 % |
| 50.000 | -0,70 | -0,89 … -0,54 | 100 % | 89,3 % | 75,0 … 100,0 % |
| 100.000 | -0,69 | -0,78 … -0,62 | 100 % | 97,6 % | 92,9 … 100,0 % |

### schildtraeger_hp_-2.5_defensive

Alle 257.040 Partien: ΔST -0,48 pp, Kompositionen in Richtung 85,7 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,54 | -1,89 … +0,24 | 84 % | 12,7 % | 0,0 … 28,6 % |
| 10.000 | -0,47 | -0,77 … -0,16 | 100 % | 52,8 % | 35,7 … 78,6 % |
| 50.000 | -0,48 | -0,61 … -0,37 | 100 % | 76,2 % | 64,3 … 89,3 % |
| 100.000 | -0,48 | -0,55 … -0,42 | 100 % | 84,7 % | 71,4 … 96,4 % |

### schildtraeger_hp_-2.5_uniform

Alle 257.040 Partien: ΔST -0,69 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -0,59 | -1,67 … +0,68 | 85 % | 16,3 % | 3,6 … 32,1 % |
| 10.000 | -0,70 | -1,01 … -0,34 | 100 % | 61,2 % | 42,9 … 82,1 % |
| 50.000 | -0,69 | -0,87 … -0,50 | 100 % | 88,3 % | 71,4 … 100,0 % |
| 100.000 | -0,68 | -0,78 … -0,59 | 100 % | 96,6 % | 89,3 … 100,0 % |

### schildtraeger_hp_-5_aggressive

Alle 257.040 Partien: ΔST -2,30 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -2,23 | -4,31 … -0,95 | 100 % | 34,1 % | 14,3 … 53,6 % |
| 10.000 | -2,29 | -2,88 … -1,75 | 100 % | 89,5 % | 71,4 … 100,0 % |
| 50.000 | -2,31 | -2,65 … -2,09 | 100 % | 99,5 % | 96,4 … 100,0 % |
| 100.000 | -2,30 | -2,40 … -2,16 | 100 % | 100,0 % | 96,4 … 100,0 % |

### schildtraeger_hp_-5_byactiontype

Alle 257.040 Partien: ΔST -1,46 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,36 | -2,67 … +0,45 | 96 % | 28,7 % | 10,7 … 46,4 % |
| 10.000 | -1,47 | -1,94 … -1,14 | 100 % | 77,6 % | 64,3 … 89,3 % |
| 50.000 | -1,45 | -1,64 … -1,24 | 100 % | 97,4 % | 89,3 … 100,0 % |
| 100.000 | -1,44 | -1,56 … -1,30 | 100 % | 99,8 % | 96,4 … 100,0 % |

### schildtraeger_hp_-5_defensive

Alle 257.040 Partien: ΔST -1,11 pp, Kompositionen in Richtung 92,9 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,19 | -2,38 … +0,23 | 97 % | 22,5 % | 3,6 … 39,3 % |
| 10.000 | -1,12 | -1,68 … -0,55 | 100 % | 70,9 % | 57,1 … 89,3 % |
| 50.000 | -1,10 | -1,24 … -0,93 | 100 % | 90,4 % | 82,1 … 100,0 % |
| 100.000 | -1,11 | -1,21 … -0,99 | 100 % | 93,7 % | 85,7 … 100,0 % |

### schildtraeger_hp_-5_uniform

Alle 257.040 Partien: ΔST -1,48 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | -1,34 | -2,68 … +0,24 | 96 % | 28,2 % | 14,3 … 42,9 % |
| 10.000 | -1,47 | -1,96 … -0,86 | 100 % | 75,8 % | 60,7 … 92,9 % |
| 50.000 | -1,48 | -1,73 … -1,22 | 100 % | 97,4 % | 89,3 … 100,0 % |
| 100.000 | -1,47 | -1,60 … -1,36 | 100 % | 99,8 % | 96,4 … 100,0 % |

### schildtraeger_hp_10_aggressive

Alle 257.040 Partien: ΔST +4,25 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +4,22 | +2,13 … +6,45 | 100 % | 50,5 % | 35,7 … 67,9 % |
| 10.000 | +4,33 | +3,48 … +5,05 | 100 % | 97,0 % | 85,7 … 100,0 % |
| 50.000 | +4,25 | +3,90 … +4,59 | 100 % | 100,0 % | 100,0 … 100,0 % |
| 100.000 | +4,25 | +4,07 … +4,51 | 100 % | 100,0 % | 100,0 … 100,0 % |

### schildtraeger_hp_10_byactiontype

Alle 257.040 Partien: ΔST +2,62 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,49 | -0,71 … +5,07 | 99 % | 38,6 % | 17,9 … 57,1 % |
| 10.000 | +2,66 | +1,80 … +3,40 | 100 % | 88,9 % | 75,0 … 100,0 % |
| 50.000 | +2,61 | +2,36 … +2,83 | 100 % | 99,8 % | 96,4 … 100,0 % |
| 100.000 | +2,61 | +2,44 … +2,83 | 100 % | 100,0 % | 96,4 … 100,0 % |

### schildtraeger_hp_10_defensive

Alle 257.040 Partien: ΔST +2,05 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,96 | +0,24 … +4,09 | 100 % | 31,3 % | 10,7 … 57,1 % |
| 10.000 | +2,04 | +1,52 … +2,62 | 100 % | 82,5 % | 67,9 … 92,9 % |
| 50.000 | +2,06 | +1,81 … +2,26 | 100 % | 94,0 % | 85,7 … 100,0 % |
| 100.000 | +2,05 | +1,95 … +2,16 | 100 % | 96,2 % | 89,3 … 100,0 % |

### schildtraeger_hp_10_uniform

Alle 257.040 Partien: ΔST +2,72 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,90 | +0,23 … +5,76 | 100 % | 41,7 % | 25,0 … 67,9 % |
| 10.000 | +2,71 | +2,12 … +3,39 | 100 % | 89,7 % | 75,0 … 100,0 % |
| 50.000 | +2,73 | +2,38 … +3,04 | 100 % | 99,8 % | 96,4 … 100,0 % |
| 100.000 | +2,74 | +2,55 … +2,91 | 100 % | 100,0 % | 96,4 … 100,0 % |

### schildtraeger_hp_2.5_aggressive

Alle 257.040 Partien: ΔST +1,10 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,08 | -0,24 … +2,38 | 98 % | 21,2 % | 7,1 … 39,3 % |
| 10.000 | +1,11 | +0,68 … +1,61 | 100 % | 74,3 % | 60,7 … 92,9 % |
| 50.000 | +1,11 | +0,98 … +1,26 | 100 % | 95,4 % | 82,1 … 100,0 % |
| 100.000 | +1,10 | +0,97 … +1,20 | 100 % | 99,0 % | 92,9 … 100,0 % |

### schildtraeger_hp_2.5_byactiontype

Alle 257.040 Partien: ΔST +0,71 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,60 | -0,48 … +1,67 | 82 % | 17,9 % | 3,6 … 32,1 % |
| 10.000 | +0,72 | +0,37 … +1,06 | 100 % | 62,5 % | 42,9 … 75,0 % |
| 50.000 | +0,71 | +0,54 … +0,89 | 100 % | 89,1 % | 75,0 … 100,0 % |
| 100.000 | +0,70 | +0,61 … +0,82 | 100 % | 96,4 % | 85,7 … 100,0 % |

### schildtraeger_hp_2.5_defensive

Alle 257.040 Partien: ΔST +0,65 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,64 | +0,00 … +1,59 | 92 % | 13,8 % | 3,6 … 28,6 % |
| 10.000 | +0,65 | +0,30 … +1,03 | 100 % | 58,7 % | 39,3 … 75,0 % |
| 50.000 | +0,65 | +0,53 … +0,80 | 100 % | 87,7 % | 75,0 … 96,4 % |
| 100.000 | +0,65 | +0,58 … +0,73 | 100 % | 93,6 % | 85,7 … 100,0 % |

### schildtraeger_hp_2.5_uniform

Alle 257.040 Partien: ΔST +0,73 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +0,71 | -0,46 … +2,06 | 88 % | 18,1 % | 0,0 … 35,7 % |
| 10.000 | +0,71 | +0,43 … +1,18 | 100 % | 62,5 % | 46,4 … 78,6 % |
| 50.000 | +0,74 | +0,60 … +0,92 | 100 % | 89,1 % | 71,4 … 100,0 % |
| 100.000 | +0,73 | +0,63 … +0,84 | 100 % | 96,5 % | 85,7 … 100,0 % |

### schildtraeger_hp_5_aggressive

Alle 257.040 Partien: ΔST +2,05 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +2,01 | +0,44 … +3,92 | 100 % | 32,0 % | 10,7 … 53,6 % |
| 10.000 | +2,06 | +1,56 … +2,55 | 100 % | 86,7 % | 71,4 … 96,4 % |
| 50.000 | +2,06 | +1,84 … +2,31 | 100 % | 99,0 % | 92,9 … 100,0 % |
| 100.000 | +2,05 | +1,91 … +2,22 | 100 % | 100,0 % | 96,4 … 100,0 % |

### schildtraeger_hp_5_byactiontype

Alle 257.040 Partien: ΔST +1,35 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,21 | -0,45 … +3,69 | 92 % | 27,3 % | 10,7 … 46,4 % |
| 10.000 | +1,37 | +0,85 … +1,94 | 100 % | 75,1 % | 57,1 … 92,9 % |
| 50.000 | +1,35 | +1,14 … +1,50 | 100 % | 96,6 % | 89,3 … 100,0 % |
| 100.000 | +1,34 | +1,20 … +1,50 | 100 % | 99,5 % | 92,9 … 100,0 % |

### schildtraeger_hp_5_defensive

Alle 257.040 Partien: ΔST +1,15 pp, Kompositionen in Richtung 96,4 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,09 | +0,00 … +2,74 | 97 % | 21,0 % | 3,6 … 42,9 % |
| 10.000 | +1,12 | +0,67 … +1,66 | 100 % | 71,6 % | 57,1 … 82,1 % |
| 50.000 | +1,15 | +0,95 … +1,33 | 100 % | 91,9 % | 82,1 … 100,0 % |
| 100.000 | +1,15 | +1,07 … +1,26 | 100 % | 94,8 % | 85,7 … 100,0 % |

### schildtraeger_hp_5_uniform

Alle 257.040 Partien: ΔST +1,39 pp, Kompositionen in Richtung 100,0 %.

| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |
|---|---|---|---|---|---|
| 1.000 | +1,46 | -0,69 … +3,11 | 98 % | 28,5 % | 10,7 … 50,0 % |
| 10.000 | +1,40 | +0,87 … +2,06 | 100 % | 75,0 % | 57,1 … 92,9 % |
| 50.000 | +1,41 | +1,22 … +1,62 | 100 % | 96,8 % | 82,1 … 100,0 % |
| 100.000 | +1,40 | +1,22 … +1,56 | 100 % | 99,7 % | 96,4 … 100,0 % |

