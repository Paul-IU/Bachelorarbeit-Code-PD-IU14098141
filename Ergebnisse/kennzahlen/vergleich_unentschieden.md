# Unentschieden als halber Sieg und einheitliches Kriterium: alt gegen neu

Erzeugt von `Ergebnisse/kennzahlen/vergleich_unentschieden.py`. Alt = Unentschieden zählen bei den Helden nicht als Sieg (Heldendatei); neu = Unentschieden als halber Sieg (wie bisher schon bei den Kompositionen).

## 1 Basis-Siegquoten der Helden (Seed 2026, %)

### byactiontype

| Held | alt | neu | Δ | Rang alt | Rang neu |
|---|---|---|---|---|---|
| elementarmagier | 63,31 | 63,31 | +0,00 | 1 | 1 |
| elfe | 60,29 | 60,29 | +0,00 | 2 | 2 |
| berserker | 56,87 | 56,88 | +0,00 | 3 | 3 |
| illusionist | 55,96 | 55,96 | +0,00 | 4 | 4 |
| wand | 52,96 | 52,96 | +0,00 | 5 | 5 |
| bogenschuetze | 47,69 | 47,69 | +0,00 | 6 | 6 |
| schildtraeger | 47,23 | 47,23 | +0,00 | 7 | 7 |
| musiker | 37,45 | 37,45 | +0,00 | 8 | 8 |
| hugger | 28,23 | 28,23 | +0,00 | 9 | 9 |

Unentschieden-Anteil aller Partien: 0,02 %; Rangfolge unverändert.

### uniform

| Held | alt | neu | Δ | Rang alt | Rang neu |
|---|---|---|---|---|---|
| elementarmagier | 62,66 | 62,66 | +0,00 | 1 | 1 |
| elfe | 61,81 | 61,81 | +0,00 | 2 | 2 |
| wand | 55,19 | 55,19 | +0,00 | 3 | 3 |
| berserker | 54,75 | 54,75 | +0,00 | 4 | 4 |
| bogenschuetze | 53,43 | 53,43 | +0,00 | 5 | 5 |
| illusionist | 52,90 | 52,90 | +0,00 | 6 | 6 |
| schildtraeger | 51,25 | 51,25 | +0,00 | 7 | 7 |
| musiker | 34,99 | 34,99 | +0,00 | 8 | 8 |
| hugger | 23,04 | 23,04 | +0,00 | 9 | 9 |

Unentschieden-Anteil aller Partien: 0,02 %; Rangfolge unverändert.

### aggressive

| Held | alt | neu | Δ | Rang alt | Rang neu |
|---|---|---|---|---|---|
| berserker | 68,07 | 68,07 | +0,00 | 1 | 1 |
| wand | 63,35 | 63,35 | +0,00 | 2 | 2 |
| schildtraeger | 55,40 | 55,40 | +0,00 | 3 | 3 |
| bogenschuetze | 54,67 | 54,67 | +0,00 | 4 | 4 |
| illusionist | 51,19 | 51,19 | +0,00 | 5 | 5 |
| elfe | 44,27 | 44,27 | +0,00 | 6 | 6 |
| elementarmagier | 42,15 | 42,15 | +0,00 | 7 | 7 |
| musiker | 41,47 | 41,47 | +0,00 | 8 | 8 |
| hugger | 29,41 | 29,41 | +0,00 | 9 | 9 |

Unentschieden-Anteil aller Partien: 0,01 %; Rangfolge unverändert.

### defensive

| Held | alt | neu | Δ | Rang alt | Rang neu |
|---|---|---|---|---|---|
| elfe | 69,53 | 71,31 | +1,78 | 1 | 1 |
| berserker | 61,45 | 63,10 | +1,65 | 2 | 2 |
| bogenschuetze | 61,37 | 62,85 | +1,48 | 3 | 3 |
| wand | 48,17 | 49,50 | +1,32 | 4 | 4 |
| elementarmagier | 46,57 | 47,95 | +1,38 | 5 | 5 |
| schildtraeger | 43,17 | 44,65 | +1,48 | 6 | 6 |
| musiker | 40,91 | 41,35 | +0,44 | 7 | 7 |
| illusionist | 37,82 | 39,42 | +1,61 | 8 | 8 |
| hugger | 28,28 | 29,88 | +1,60 | 9 | 9 |

Unentschieden-Anteil aller Partien: 3,35 %; Rangfolge unverändert.

## 2 Rangkorrelationen der Helden zwischen den Agenten (Seed 2026)

| Paar | Spearman alt | Spearman neu | Kendall alt | Kendall neu |
|---|---|---|---|---|
| byactiontype / uniform | 0,917 | 0,917 | 0,833 | 0,833 |
| byactiontype / aggressive | 0,283 | 0,283 | 0,167 | 0,167 |
| byactiontype / defensive | 0,617 | 0,617 | 0,500 | 0,500 |
| uniform / aggressive | 0,333 | 0,333 | 0,222 | 0,222 |
| uniform / defensive | 0,733 | 0,733 | 0,556 | 0,556 |
| aggressive / defensive | 0,550 | 0,550 | 0,444 | 0,444 |

## 3 Streuung über die fünf Seeds (SD, n−1, pp)

### byactiontype

| Held | SD alt | SD neu |
|---|---|---|
| berserker | 0,075 | 0,075 |
| bogenschuetze | 0,161 | 0,160 |
| elementarmagier | 0,158 | 0,157 |
| elfe | 0,162 | 0,162 |
| hugger | 0,179 | 0,179 |
| illusionist | 0,148 | 0,147 |
| musiker | 0,151 | 0,151 |
| schildtraeger | 0,128 | 0,129 |
| wand | 0,127 | 0,127 |

Helden: mittlere SD alt 0,143 / neu 0,143 pp, größte alt 0,179 / neu 0,179 pp.
Kompositionen (n = 84, ohne Spiegel): mittlere SD alt 0,569 / neu 0,569 pp, größte alt 1,078 / neu 1,078 pp.

### uniform

| Held | SD alt | SD neu |
|---|---|---|
| berserker | 0,158 | 0,158 |
| bogenschuetze | 0,115 | 0,115 |
| elementarmagier | 0,124 | 0,123 |
| elfe | 0,103 | 0,103 |
| hugger | 0,051 | 0,051 |
| illusionist | 0,184 | 0,184 |
| musiker | 0,025 | 0,026 |
| schildtraeger | 0,130 | 0,130 |
| wand | 0,108 | 0,108 |

Helden: mittlere SD alt 0,111 / neu 0,111 pp, größte alt 0,184 / neu 0,184 pp.
Kompositionen (n = 84, ohne Spiegel): mittlere SD alt 0,521 / neu 0,521 pp, größte alt 1,086 / neu 1,086 pp.

## 4 Δ Schildträger, Erkennung und Simulationsumfang je Variante

Δ = bereinigte Siegquote des Schildträgers, Variante − Basis (pp). Erkannt = Kriterium vollständiger Lauf. ab = kleinste Stichprobe mit ≥ 95 % (alt: nur Vorzeichen von Δ; neu: (a) UND (b)).

| Agent | Variante | Δ alt | Δ neu | erkannt alt | erkannt neu | ab alt | ab neu |
|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,83 | +2,83 | ja | ja | 1.000 | 10.000 |
| byactiontype | dmg +5 % | +1,46 | +1,46 | ja | ja | 1.000 | 10.000 |
| byactiontype | dmg +2,5 % | +0,64 | +0,64 | ja | ja | 10.000 | 50.000 |
| byactiontype | dmg -2,5 % | -1,59 | -1,59 | ja | ja | 1.000 | 10.000 |
| byactiontype | dmg -5 % | -2,54 | -2,54 | ja | ja | 1.000 | 10.000 |
| byactiontype | dmg -10 % | -4,11 | -4,11 | ja | ja | 1.000 | 10.000 |
| byactiontype | hp +10 % | +2,62 | +2,62 | ja | ja | 1.000 | 10.000 |
| byactiontype | hp +5 % | +1,35 | +1,35 | ja | ja | 10.000 | 10.000 |
| byactiontype | hp +2,5 % | +0,71 | +0,71 | ja | ja | 10.000 | 50.000 |
| byactiontype | hp -2,5 % | -0,71 | -0,71 | ja | ja | 10.000 | 50.000 |
| byactiontype | hp -5 % | -1,46 | -1,46 | ja | ja | 1.000 | 10.000 |
| byactiontype | hp -10 % | -2,97 | -2,97 | ja | ja | 1.000 | 10.000 |
| byactiontype | ability +10 % | +0,31 | +0,31 | ja | ja | 10.000 | 100.000 |
| byactiontype | ability +5 % | +0,17 | +0,17 | ja | ja | 50.000 | 100.000 |
| byactiontype | ability +2,5 % | +0,11 | +0,11 | ja | ja | 50.000 | nie |
| byactiontype | ability -2,5 % | -0,05 | -0,05 | ja | ja | 50.000 | nie |
| byactiontype | ability -5 % | -0,09 | -0,09 | ja | ja | 50.000 | nie |
| byactiontype | ability -10 % | -0,23 | -0,23 | ja | ja | 10.000 | 100.000 |
| uniform | dmg +10 % | +3,00 | +3,00 | ja | ja | 1.000 | 10.000 |
| uniform | dmg +5 % | +1,60 | +1,60 | ja | ja | 1.000 | 10.000 |
| uniform | dmg +2,5 % | +0,69 | +0,69 | ja | ja | 10.000 | 50.000 |
| uniform | dmg -2,5 % | -1,71 | -1,71 | ja | ja | 1.000 | 10.000 |
| uniform | dmg -5 % | -2,69 | -2,69 | ja | ja | 1.000 | 10.000 |
| uniform | dmg -10 % | -4,20 | -4,20 | ja | ja | 1.000 | 10.000 |
| uniform | hp +10 % | +2,72 | +2,72 | ja | ja | 1.000 | 10.000 |
| uniform | hp +5 % | +1,39 | +1,39 | ja | ja | 1.000 | 10.000 |
| uniform | hp +2,5 % | +0,73 | +0,73 | ja | ja | 10.000 | 50.000 |
| uniform | hp -2,5 % | -0,69 | -0,69 | ja | ja | 10.000 | 50.000 |
| uniform | hp -5 % | -1,48 | -1,48 | ja | ja | 1.000 | 10.000 |
| uniform | hp -10 % | -3,18 | -3,18 | ja | ja | 1.000 | 10.000 |
| uniform | ability +10 % | +0,56 | +0,56 | ja | ja | 10.000 | 50.000 |
| uniform | ability +5 % | +0,33 | +0,33 | ja | ja | 10.000 | 50.000 |
| uniform | ability +2,5 % | +0,19 | +0,19 | ja | ja | 10.000 | 100.000 |
| uniform | ability -2,5 % | -0,09 | -0,09 | ja | ja | 10.000 | nie |
| uniform | ability -5 % | -0,23 | -0,23 | ja | ja | 10.000 | 50.000 |
| uniform | ability -10 % | -0,48 | -0,48 | ja | ja | 10.000 | 50.000 |
| aggressive | dmg +10 % | +2,78 | +2,78 | ja | ja | 1.000 | 10.000 |
| aggressive | dmg +5 % | +1,20 | +1,20 | ja | ja | 1.000 | 10.000 |
| aggressive | dmg +2,5 % | +0,52 | +0,52 | ja | ja | 10.000 | 50.000 |
| aggressive | dmg -2,5 % | -2,23 | -2,23 | ja | ja | 1.000 | 10.000 |
| aggressive | dmg -5 % | -3,40 | -3,40 | ja | ja | 1.000 | 10.000 |
| aggressive | dmg -10 % | -5,13 | -5,13 | ja | ja | 1.000 | 10.000 |
| aggressive | hp +10 % | +4,25 | +4,25 | ja | ja | 1.000 | 1.000 |
| aggressive | hp +5 % | +2,05 | +2,05 | ja | ja | 1.000 | 10.000 |
| aggressive | hp +2,5 % | +1,10 | +1,10 | ja | ja | 1.000 | 10.000 |
| aggressive | hp -2,5 % | -1,33 | -1,33 | ja | ja | 1.000 | 10.000 |
| aggressive | hp -5 % | -2,30 | -2,30 | ja | ja | 1.000 | 10.000 |
| aggressive | hp -10 % | -4,73 | -4,73 | ja | ja | 1.000 | 1.000 |
| aggressive | ability +10 % | -0,27 | -0,27 | nein | nein | nie | nie |
| aggressive | ability +5 % | -0,07 | -0,07 | nein | nein | nie | nie |
| aggressive | ability +2,5 % | +0,01 | +0,01 | nein | nein | nie | nie |
| aggressive | ability -2,5 % | -0,05 | -0,05 | nein | nein | 50.000 | nie |
| aggressive | ability -5 % | -0,07 | -0,07 | nein | nein | 50.000 | nie |
| aggressive | ability -10 % | +0,06 | +0,06 | nein | nein | nie | nie |
| defensive | dmg +10 % | +2,57 | +2,44 | ja | ja | 1.000 | 10.000 |
| defensive | dmg +5 % | +1,27 | +1,15 | ja | ja | 10.000 | 50.000 |
| defensive | dmg +2,5 % | +0,57 | +0,49 | ja | ja | 10.000 | nie |
| defensive | dmg -2,5 % | -1,50 | -1,59 | ja | ja | 10.000 | 50.000 |
| defensive | dmg -5 % | -2,19 | -2,28 | ja | ja | 1.000 | 10.000 |
| defensive | dmg -10 % | -3,47 | -3,55 | ja | ja | 1.000 | 10.000 |
| defensive | hp +10 % | +2,05 | +2,05 | ja | ja | 1.000 | 10.000 |
| defensive | hp +5 % | +1,15 | +1,15 | ja | ja | 1.000 | 10.000 |
| defensive | hp +2,5 % | +0,65 | +0,65 | ja | ja | 10.000 | 50.000 |
| defensive | hp -2,5 % | -0,48 | -0,48 | ja | ja | 10.000 | 50.000 |
| defensive | hp -5 % | -1,11 | -1,11 | ja | ja | 1.000 | 10.000 |
| defensive | hp -10 % | -2,40 | -2,40 | ja | ja | 1.000 | 10.000 |
| defensive | ability +10 % | +0,09 | +0,06 | nein | nein | nie | nie |
| defensive | ability +5 % | -0,18 | -0,21 | nein | nein | nie | nie |
| defensive | ability +2,5 % | -0,29 | -0,28 | nein | nein | nie | nie |
| defensive | ability -2,5 % | -0,12 | -0,14 | nein | nein | 50.000 | nie |
| defensive | ability -5 % | -0,26 | -0,27 | ja | ja | 50.000 | nie |
| defensive | ability -10 % | -0,60 | -0,66 | ja | ja | 10.000 | 100.000 |

## 5 Änderungen auf einen Blick

- Vorzeichen von Δ Schildträger gewechselt: keine
- Erkennung (vollständiger Lauf) geändert: keine
- Erkannt alt / neu: 62 / 62 von 72
- Kleinste Stichprobe geändert (62): byactiontype dmg +10 % (1.000 → 10.000), byactiontype dmg +5 % (1.000 → 10.000), byactiontype dmg +2,5 % (10.000 → 50.000), byactiontype dmg -2,5 % (1.000 → 10.000), byactiontype dmg -5 % (1.000 → 10.000), byactiontype dmg -10 % (1.000 → 10.000), byactiontype hp +10 % (1.000 → 10.000), byactiontype hp +2,5 % (10.000 → 50.000), byactiontype hp -2,5 % (10.000 → 50.000), byactiontype hp -5 % (1.000 → 10.000), byactiontype hp -10 % (1.000 → 10.000), byactiontype ability +10 % (10.000 → 100.000), byactiontype ability +5 % (50.000 → 100.000), byactiontype ability +2,5 % (50.000 → nie), byactiontype ability -2,5 % (50.000 → nie), byactiontype ability -5 % (50.000 → nie), byactiontype ability -10 % (10.000 → 100.000), uniform dmg +10 % (1.000 → 10.000), uniform dmg +5 % (1.000 → 10.000), uniform dmg +2,5 % (10.000 → 50.000), uniform dmg -2,5 % (1.000 → 10.000), uniform dmg -5 % (1.000 → 10.000), uniform dmg -10 % (1.000 → 10.000), uniform hp +10 % (1.000 → 10.000), uniform hp +5 % (1.000 → 10.000), uniform hp +2,5 % (10.000 → 50.000), uniform hp -2,5 % (10.000 → 50.000), uniform hp -5 % (1.000 → 10.000), uniform hp -10 % (1.000 → 10.000), uniform ability +10 % (10.000 → 50.000), uniform ability +5 % (10.000 → 50.000), uniform ability +2,5 % (10.000 → 100.000), uniform ability -2,5 % (10.000 → nie), uniform ability -5 % (10.000 → 50.000), uniform ability -10 % (10.000 → 50.000), aggressive dmg +10 % (1.000 → 10.000), aggressive dmg +5 % (1.000 → 10.000), aggressive dmg +2,5 % (10.000 → 50.000), aggressive dmg -2,5 % (1.000 → 10.000), aggressive dmg -5 % (1.000 → 10.000), aggressive dmg -10 % (1.000 → 10.000), aggressive hp +5 % (1.000 → 10.000), aggressive hp +2,5 % (1.000 → 10.000), aggressive hp -2,5 % (1.000 → 10.000), aggressive hp -5 % (1.000 → 10.000), aggressive ability -2,5 % (50.000 → nie), aggressive ability -5 % (50.000 → nie), defensive dmg +10 % (1.000 → 10.000), defensive dmg +5 % (10.000 → 50.000), defensive dmg +2,5 % (10.000 → nie), defensive dmg -2,5 % (10.000 → 50.000), defensive dmg -5 % (1.000 → 10.000), defensive dmg -10 % (1.000 → 10.000), defensive hp +10 % (1.000 → 10.000), defensive hp +5 % (1.000 → 10.000), defensive hp +2,5 % (10.000 → 50.000), defensive hp -2,5 % (10.000 → 50.000), defensive hp -5 % (1.000 → 10.000), defensive hp -10 % (1.000 → 10.000), defensive ability -2,5 % (50.000 → nie), defensive ability -5 % (50.000 → nie), defensive ability -10 % (10.000 → 100.000)

