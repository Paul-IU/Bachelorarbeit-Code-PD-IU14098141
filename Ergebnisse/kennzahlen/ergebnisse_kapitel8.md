# Ergebnisse für Kapitel 8.3

Erzeugt von `Ergebnisse/kennzahlen/kapitel8.py`. Aktueller Stand: Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung, ohne die Zusatzvariante dmg +20 %. Basis = Basislauf Seed 2026 des jeweiligen Agenten.

**‡ = gegenüber früheren Berichten verändert**, früherer Wert in Klammern (vor dem 13.10.2026: Unentschieden der Helden nicht als Sieg; Simulationsumfang nur über das Vorzeichen der Schildträger-Siegquote). Verglichen wird auf der angezeigten Stellenzahl.

## 1 Δ der bereinigten Siegquote des Schildträgers (pp)

Basis (Seed 2026): byactiontype 47,23 %, uniform 51,25 %, aggressive 55,40 %, defensive 44,65 ‡ (43,17) %

| Variante | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| dmg +10 % | +2,83 | +3,00 | +2,78 | +2,44 ‡ (+2,57) |
| dmg +5 % | +1,46 | +1,60 | +1,20 | +1,15 ‡ (+1,27) |
| dmg +2,5 % | +0,64 | +0,69 | +0,52 | +0,49 ‡ (+0,57) |
| dmg -2,5 % | -1,59 | -1,71 | -2,23 | -1,59 ‡ (-1,50) |
| dmg -5 % | -2,54 | -2,69 | -3,40 | -2,28 ‡ (-2,19) |
| dmg -10 % | -4,11 | -4,20 | -5,13 | -3,55 ‡ (-3,47) |
| hp +10 % | +2,62 | +2,72 | +4,25 | +2,05 |
| hp +5 % | +1,35 | +1,39 | +2,05 | +1,15 |
| hp +2,5 % | +0,71 | +0,73 | +1,10 | +0,65 |
| hp -2,5 % | -0,71 | -0,69 | -1,33 | -0,48 |
| hp -5 % | -1,46 | -1,48 | -2,30 | -1,11 |
| hp -10 % | -2,97 | -3,18 | -4,73 | -2,40 |
| ability +10 % | +0,31 | +0,56 | -0,27 | +0,06 ‡ (+0,09) |
| ability +5 % | +0,17 | +0,33 | -0,07 | -0,21 ‡ (-0,18) |
| ability +2,5 % | +0,11 | +0,19 | +0,01 | -0,28 ‡ (-0,29) |
| ability -2,5 % | -0,05 | -0,09 | -0,05 | -0,14 ‡ (-0,12) |
| ability -5 % | -0,09 | -0,23 | -0,07 | -0,27 ‡ (-0,26) |
| ability -10 % | -0,23 | -0,48 | +0,06 | -0,66 ‡ (-0,60) |

## 2 Verhältnis Abschwächung zu Verstärkung, |Δ(−p)| / |Δ(+p)|

| Attribut | Agent | ±2,5 % | ±5 % | ±10 % |
|---|---|---|---|---|
| dmg | byactiontype | 2,49 | 1,73 | 1,45 |
| dmg | uniform | 2,48 | 1,68 | 1,40 |
| dmg | aggressive | 4,32 | 2,83 | 1,85 |
| dmg | defensive | 3,23 ‡ (2,65) | 1,99 ‡ (1,73) | 1,45 ‡ (1,35) |
| hp | byactiontype | 1,00 | 1,08 | 1,13 |
| hp | uniform | 0,94 | 1,07 | 1,17 |
| hp | aggressive | 1,21 | 1,12 | 1,11 |
| hp | defensive | 0,74 | 0,97 | 1,17 |
| ability | byactiontype | 0,41 | 0,52 | 0,76 |
| ability | uniform | 0,46 | 0,71 | 0,86 |
| ability | aggressive | entgegengesetzt, beide in Richtung der Manipulation: +0,01 / -0,05 | gleiches Vorzeichen (beide −): -0,07 / -0,07 | entgegengesetzt, beide gegen die Manipulation: -0,27 / +0,06 |
| ability | defensive | gleiches Vorzeichen (beide −): -0,28 / -0,14 ‡ (gleiches Vorzeichen (beide −): -0,29 / -0,12) | gleiches Vorzeichen (beide −): -0,21 / -0,27 ‡ (gleiches Vorzeichen (beide −): -0,18 / -0,26) | entgegengesetzt, beide in Richtung der Manipulation: +0,06 / -0,66 ‡ (entgegengesetzt, beide in Richtung der Manipulation: +0,09 / -0,60) |

Fähigkeitsstärke: Verhältnis nur für byactiontype und uniform. Für aggressive und defensive steht statt eines Werts, ob Δ(+p) und Δ(−p) dasselbe Vorzeichen haben, dahinter Δ(+p) / Δ(−p) in pp. Bei gleichem Vorzeichen wirkt eine der beiden Stufen gegen ihre Richtung; bei entgegengesetztem Vorzeichen ist zusätzlich angegeben, ob beide in oder beide gegen die Richtung der Manipulation liegen. Ein Verhältnis wäre dort nicht sinnvoll. Alle Verhältnisse aus den ungerundeten Werten.

## 3 Steigung (pp je % Manipulation) und R²

Kleinste-Quadrate-Gerade mit Achsenabschnitt über die Stufen und den Basispunkt (0 %, 0 pp): positiv = 0/2,5/5/10, negativ = −10/−5/−2,5/0, gesamt = alle sieben Punkte.

| Attribut | Agent | positiv: Steigung | R² | negativ: Steigung | R² | gesamt: Steigung | R² |
|---|---|---|---|---|---|---|---|
| dmg | byactiontype | +0,286 | 0,999 | +0,397 | 0,970 | +0,362 | 0,986 |
| dmg | uniform | +0,303 | 0,998 | +0,404 | 0,959 | +0,379 | 0,984 |
| dmg | aggressive | +0,282 | 0,993 | +0,490 | 0,945 | +0,415 | 0,969 |
| dmg | defensive | +0,248 ‡ (+0,260) | 0,997 ‡ (0,999) | +0,337 ‡ (+0,331) | 0,945 ‡ (0,952) | +0,314 ‡ (+0,316) | 0,982 ‡ (0,985) |
| hp | byactiontype | +0,260 | 0,999 | +0,298 | 1,000 | +0,280 | 0,998 |
| hp | uniform | +0,271 | 1,000 | +0,321 | 0,998 | +0,293 | 0,997 |
| hp | aggressive | +0,422 | 0,999 | +0,467 | 0,998 | +0,448 | 0,999 |
| hp | defensive | +0,202 | 0,993 | +0,243 | 0,997 | +0,223 | 0,994 |
| ability | byactiontype | +0,030 | 0,985 | +0,024 | 0,983 | +0,027 | 0,992 |
| ability | uniform | +0,054 | 0,984 | +0,049 | 0,995 | +0,053 | 0,996 |
| ability | aggressive | -0,028 | 0,906 | -0,007 | 0,294 | -0,012 | 0,584 |
| ability | defensive | +0,014 ‡ (+0,017) | 0,124 ‡ (0,175) | +0,067 ‡ (+0,060) | 0,990 ‡ (0,993) | +0,027 ‡ (+0,026) | 0,583 ‡ (0,596) |

## 4 Erkennung

**Kriterium:** (a) Siegquote des Schildträgers in Richtung der Manipulation UND (b) einseitiger Vorzeichentest (α = 0,05) über die veränderten Kompositionen mit Schildträger; unveränderte Kompositionen zählen nicht.

### Erkennungsmatrix (vollständiger Lauf, 257.040 Partien)

| Variante | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| dmg +10 % | erkannt (28/0) | erkannt (28/0) | erkannt (27/1) | erkannt (26/2) |
| dmg +5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (25/3) |
| dmg +2,5 % | erkannt (28/0) | erkannt (27/1) | erkannt (23/5) | erkannt (22/6) |
| dmg -2,5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (26/2) |
| dmg -5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/1) |
| dmg -10 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/0) |
| hp +10 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/1) |
| hp +5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/0) |
| hp +2,5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/1) |
| hp -2,5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (24/3) |
| hp -5 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (26/2) |
| hp -10 % | erkannt (28/0) | erkannt (28/0) | erkannt (28/0) | erkannt (27/0) |
| ability +10 % | erkannt (25/3) | erkannt (28/0) | nicht erkannt (11/17) | nicht erkannt (16/12) |
| ability +5 % | erkannt (23/3) | erkannt (28/0) | nicht erkannt (11/17) | nicht erkannt (11/17) |
| ability +2,5 % | erkannt (21/7) | erkannt (27/1) | nicht erkannt (15/13) | nicht erkannt (9/18) |
| ability -2,5 % | erkannt (16/6) | erkannt (22/2) | nicht erkannt (18/10) | nicht erkannt (15/12) |
| ability -5 % | erkannt (21/6) | erkannt (28/0) | nicht erkannt (18/9) | erkannt (21/6) |
| ability -10 % | erkannt (24/4) | erkannt (28/0) | nicht erkannt (14/14) | erkannt (24/4) |

In Klammern: Kompositionen in Richtung / dagegen. Erkannt: 62 von 72.

### Simulationsumfang

Kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der das Kriterium in ≥ 95 % der 100 Wiederholungen erfüllt ist; nie = auch bei 100.000 nicht.

| Variante | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| dmg +10 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) |
| dmg +5 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 50.000 ‡ (10.000) |
| dmg +2,5 % | 50.000 ‡ (10.000) | 50.000 ‡ (10.000) | 50.000 ‡ (10.000) | nie ‡ (10.000) |
| dmg -2,5 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 50.000 ‡ (10.000) |
| dmg -5 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) |
| dmg -10 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) |
| hp +10 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 1.000 | 10.000 ‡ (1.000) |
| hp +5 % | 10.000 | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) |
| hp +2,5 % | 50.000 ‡ (10.000) | 50.000 ‡ (10.000) | 10.000 ‡ (1.000) | 50.000 ‡ (10.000) |
| hp -2,5 % | 50.000 ‡ (10.000) | 50.000 ‡ (10.000) | 10.000 ‡ (1.000) | 50.000 ‡ (10.000) |
| hp -5 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) |
| hp -10 % | 10.000 ‡ (1.000) | 10.000 ‡ (1.000) | 1.000 | 10.000 ‡ (1.000) |
| ability +10 % | 100.000 ‡ (10.000) | 50.000 ‡ (10.000) | nie | nie |
| ability +5 % | 100.000 ‡ (50.000) | 50.000 ‡ (10.000) | nie | nie |
| ability +2,5 % | nie ‡ (50.000) | 100.000 ‡ (10.000) | nie | nie |
| ability -2,5 % | nie ‡ (50.000) | nie ‡ (10.000) | nie ‡ (50.000) | nie ‡ (50.000) |
| ability -5 % | nie ‡ (50.000) | 50.000 ‡ (10.000) | nie ‡ (50.000) | nie ‡ (50.000) |
| ability -10 % | 100.000 ‡ (10.000) | 50.000 ‡ (10.000) | nie | 100.000 ‡ (10.000) |

Anteil erkannter Wiederholungen je Stichprobengröße (in Klammern: Wiederholungen, in denen der Vorzeichentest mangels veränderter Kompositionen nicht anschlagen konnte):

| Agent | Variante | 1.000 | 10.000 | 50.000 | 100.000 |
|---|---|---|---|---|---|
| byactiontype | dmg +10 % | 51 % | 100 % | 100 % | 100 % |
| byactiontype | dmg +5 % | 23 % | 100 % | 100 % | 100 % |
| byactiontype | dmg +2,5 % | 6 % (16) | 83 % | 100 % | 100 % |
| byactiontype | dmg -2,5 % | 20 % | 99 % | 100 % | 100 % |
| byactiontype | dmg -5 % | 51 % | 100 % | 100 % | 100 % |
| byactiontype | dmg -10 % | 73 % | 100 % | 100 % | 100 % |
| byactiontype | hp +10 % | 56 % | 100 % | 100 % | 100 % |
| byactiontype | hp +5 % | 17 % | 100 % | 100 % | 100 % |
| byactiontype | hp +2,5 % | 8 % (12) | 86 % | 100 % | 100 % |
| byactiontype | hp -2,5 % | 4 % (13) | 89 % | 100 % | 100 % |
| byactiontype | hp -5 % | 21 % (1) | 100 % | 100 % | 100 % |
| byactiontype | hp -10 % | 65 % | 100 % | 100 % | 100 % |
| byactiontype | ability +10 % | 2 % (39) | 32 % | 88 % | 100 % |
| byactiontype | ability +5 % | 0 % (60) | 17 % | 65 % | 95 % |
| byactiontype | ability +2,5 % | 0 % (78) | 7 % | 34 % | 73 % |
| byactiontype | ability -2,5 % | 0 % (99) | 1 % (11) | 12 % | 44 % |
| byactiontype | ability -5 % | 0 % (94) | 4 % | 31 % | 59 % |
| byactiontype | ability -10 % | 1 % (61) | 23 % | 79 % | 99 % |
| uniform | dmg +10 % | 51 % | 100 % | 100 % | 100 % |
| uniform | dmg +5 % | 27 % | 99 % | 100 % | 100 % |
| uniform | dmg +2,5 % | 6 % (11) | 84 % | 100 % | 100 % |
| uniform | dmg -2,5 % | 25 % | 100 % | 100 % | 100 % |
| uniform | dmg -5 % | 46 % | 100 % | 100 % | 100 % |
| uniform | dmg -10 % | 67 % | 100 % | 100 % | 100 % |
| uniform | hp +10 % | 67 % | 100 % | 100 % | 100 % |
| uniform | hp +5 % | 29 % (1) | 100 % | 100 % | 100 % |
| uniform | hp +2,5 % | 6 % (14) | 87 % | 100 % | 100 % |
| uniform | hp -2,5 % | 4 % (19) | 89 % | 100 % | 100 % |
| uniform | hp -5 % | 22 % (1) | 99 % | 100 % | 100 % |
| uniform | hp -10 % | 65 % | 100 % | 100 % | 100 % |
| uniform | ability +10 % | 3 % (12) | 70 % | 100 % | 100 % |
| uniform | ability +5 % | 1 % (41) | 56 % | 98 % | 100 % |
| uniform | ability +2,5 % | 0 % (75) | 25 % | 87 % | 99 % |
| uniform | ability -2,5 % | 0 % (99) | 4 % (3) | 61 % | 84 % |
| uniform | ability -5 % | 1 % (79) | 27 % | 96 % | 100 % |
| uniform | ability -10 % | 1 % (35) | 60 % | 100 % | 100 % |
| aggressive | dmg +10 % | 56 % | 100 % | 100 % | 100 % |
| aggressive | dmg +5 % | 13 % (2) | 98 % | 100 % | 100 % |
| aggressive | dmg +2,5 % | 4 % (50) | 61 % | 99 % | 100 % |
| aggressive | dmg -2,5 % | 53 % | 100 % | 100 % | 100 % |
| aggressive | dmg -5 % | 76 % | 100 % | 100 % | 100 % |
| aggressive | dmg -10 % | 94 % | 100 % | 100 % | 100 % |
| aggressive | hp +10 % | 100 % | 100 % | 100 % | 100 % |
| aggressive | hp +5 % | 56 % (1) | 100 % | 100 % | 100 % |
| aggressive | hp +2,5 % | 18 % (10) | 100 % | 100 % | 100 % |
| aggressive | hp -2,5 % | 34 % (11) | 100 % | 100 % | 100 % |
| aggressive | hp -5 % | 67 % | 100 % | 100 % | 100 % |
| aggressive | hp -10 % | 98 % | 100 % | 100 % | 100 % |
| aggressive | ability +10 % | 0 % (6) | 1 % | 0 % | 0 % |
| aggressive | ability +5 % | 0 % (37) | 1 % | 0 % | 0 % |
| aggressive | ability +2,5 % | 0 % (74) | 1 % | 0 % | 0 % |
| aggressive | ability -2,5 % | 0 % (97) | 4 % (2) | 10 % | 14 % |
| aggressive | ability -5 % | 0 % (71) | 3 % | 8 % | 13 % |
| aggressive | ability -10 % | 0 % (22) | 0 % | 1 % | 0 % |
| defensive | dmg +10 % | 37 % | 100 % | 100 % | 100 % |
| defensive | dmg +5 % | 15 % | 71 % | 100 % | 100 % |
| defensive | dmg +2,5 % | 5 % (2) | 27 % | 68 % | 90 % |
| defensive | dmg -2,5 % | 16 % | 94 % | 100 % | 100 % |
| defensive | dmg -5 % | 31 % | 98 % | 100 % | 100 % |
| defensive | dmg -10 % | 50 % | 100 % | 100 % | 100 % |
| defensive | hp +10 % | 52 % | 100 % | 100 % | 100 % |
| defensive | hp +5 % | 26 % (12) | 99 % | 100 % | 100 % |
| defensive | hp +2,5 % | 6 % (45) | 94 % | 100 % | 100 % |
| defensive | hp -2,5 % | 4 % (43) | 83 % | 100 % | 100 % |
| defensive | hp -5 % | 20 % (10) | 100 % | 100 % | 100 % |
| defensive | hp -10 % | 72 % | 100 % | 100 % | 100 % |
| defensive | ability +10 % | 1 % | 2 % | 2 % | 5 % |
| defensive | ability +5 % | 1 % | 0 % | 0 % | 0 % |
| defensive | ability +2,5 % | 0 % | 0 % | 0 % | 0 % |
| defensive | ability -2,5 % | 3 % (15) | 5 % | 19 % | 17 % |
| defensive | ability -5 % | 3 % (1) | 5 % | 25 % | 51 % |
| defensive | ability -10 % | 10 % | 46 % | 89 % | 99 % |

## 5 Top-D und Top-B

Top-D(G) nach Bradley-Terry (Lin et al.), Top-B = Anzahl nicht dominierter Kompositionen (Proposition 5.8). Unentschieden zählten hier schon bisher als halber Sieg; alle Werte sind unverändert.

| Lauf | c_top | Top-D 0,01 | 0,02 | 0,04 | 0,08 | Top-B |
|---|---|---|---|---|---|---|
| basis_byactiontype_seed2026 | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 34 |
| basis_byactiontype_seed2027 | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 5 | 32 |
| basis_byactiontype_seed2028 | elementarmagier+elfe+illusionist | 1 | 2 | 2 | 5 | 37 |
| basis_byactiontype_seed2029 | elementarmagier+elfe+illusionist | 1 | 1 | 3 | 6 | 34 |
| basis_byactiontype_seed2030 | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 30 |
| basis_uniform_seed2026 | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| basis_uniform_seed2027 | berserker+elementarmagier+elfe | 1 | 2 | 3 | 7 | 37 |
| basis_uniform_seed2028 | bogenschuetze+elementarmagier+elfe | 2 | 2 | 3 | 7 | 38 |
| basis_uniform_seed2029 | bogenschuetze+elementarmagier+elfe | 1 | 1 | 3 | 7 | 37 |
| basis_uniform_seed2030 | berserker+elementarmagier+elfe | 3 | 3 | 3 | 9 | 40 |
| basis_aggressive_seed2026 | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| basis_defensive_seed2026 | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| schildtraeger_dmg_10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 7 | 32 |
| schildtraeger_dmg_5_byactiontype | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 33 |
| schildtraeger_dmg_2.5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 33 |
| schildtraeger_dmg_-2.5_byactiontype | elementarmagier+elfe+illusionist | 1 | 2 | 4 | 6 | 32 |
| schildtraeger_dmg_-5_byactiontype | elementarmagier+elfe+illusionist | 1 | 2 | 4 | 6 | 32 |
| schildtraeger_dmg_-10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 29 |
| schildtraeger_hp_10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 7 | 32 |
| schildtraeger_hp_5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 32 |
| schildtraeger_hp_2.5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 33 |
| schildtraeger_hp_-2.5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 32 |
| schildtraeger_hp_-5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 33 |
| schildtraeger_hp_-10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 31 |
| schildtraeger_ability_10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 34 |
| schildtraeger_ability_5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 33 |
| schildtraeger_ability_2.5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 34 |
| schildtraeger_ability_-2.5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 34 |
| schildtraeger_ability_-5_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 4 | 6 | 34 |
| schildtraeger_ability_-10_byactiontype | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 33 |
| schildtraeger_dmg_10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 5 | 7 | 38 |
| schildtraeger_dmg_5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 3 | 7 | 39 |
| schildtraeger_dmg_2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 37 |
| schildtraeger_dmg_-2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 3 | 5 | 36 |
| schildtraeger_dmg_-5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 3 | 5 | 36 |
| schildtraeger_dmg_-10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 3 | 6 | 35 |
| schildtraeger_hp_10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 5 | 7 | 40 |
| schildtraeger_hp_5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 8 | 38 |
| schildtraeger_hp_2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 7 | 38 |
| schildtraeger_hp_-2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 3 | 6 | 38 |
| schildtraeger_hp_-5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 7 | 39 |
| schildtraeger_hp_-10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 3 | 6 | 37 |
| schildtraeger_ability_10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| schildtraeger_ability_5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| schildtraeger_ability_2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| schildtraeger_ability_-2.5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| schildtraeger_ability_-5_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 6 | 39 |
| schildtraeger_ability_-10_uniform | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 6 | 38 |
| schildtraeger_dmg_10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 36 |
| schildtraeger_dmg_5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 37 |
| schildtraeger_dmg_2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 37 |
| schildtraeger_dmg_-2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_dmg_-5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_dmg_-10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_hp_10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 34 |
| schildtraeger_hp_5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 34 |
| schildtraeger_hp_2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 36 |
| schildtraeger_hp_-2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_hp_-5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_hp_-10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_-2.5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_-5_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| schildtraeger_ability_-10_aggressive | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 36 |
| schildtraeger_dmg_10_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 58 |
| schildtraeger_dmg_5_defensive | berserker+bogenschuetze+elfe | 1 | 3 | 3 | 3 | 56 |
| schildtraeger_dmg_2.5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| schildtraeger_dmg_-2.5_defensive | berserker+bogenschuetze+elfe | 2 | 2 | 3 | 3 | 54 |
| schildtraeger_dmg_-5_defensive | berserker+bogenschuetze+elfe | 2 | 2 | 3 | 3 | 53 |
| schildtraeger_dmg_-10_defensive | berserker+bogenschuetze+elfe | 2 | 2 | 3 | 4 | 54 |
| schildtraeger_hp_10_defensive | berserker+bogenschuetze+elfe | 2 | 2 | 3 | 3 | 57 |
| schildtraeger_hp_5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 56 |
| schildtraeger_hp_2.5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 55 |
| schildtraeger_hp_-2.5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| schildtraeger_hp_-5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 55 |
| schildtraeger_hp_-10_defensive | berserker+bogenschuetze+elfe | 1 | 1 | 3 | 3 | 54 |
| schildtraeger_ability_10_defensive | berserker+bogenschuetze+elfe | 1 | 1 | 3 | 3 | 55 |
| schildtraeger_ability_5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 55 |
| schildtraeger_ability_2.5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| schildtraeger_ability_-2.5_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| schildtraeger_ability_-5_defensive | berserker+bogenschuetze+elfe | 1 | 1 | 3 | 3 | 54 |
| schildtraeger_ability_-10_defensive | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |

Spannweite über die fünf Seeds (2026–2030), min … max:

| Agent | Top-D 0,01 | 0,02 | 0,04 | 0,08 | Top-B |
|---|---|---|---|---|---|
| byactiontype | 1 … 2 | 1 … 2 | 2 … 3 | 5 … 6 | 30 … 37 |
| uniform | 1 … 3 | 1 … 3 | 3 … 4 | 6 … 9 | 37 … 40 |

## 6 Schildwall (Basisläufe Seed 2026)

Aus der Heldendatei, nur einseitige Partien; der Schildwall ist die einzige Fähigkeit des Schildträgers (Abklingzeit 2).

| Agent | Züge des Schildträgers | mit Schildwall-Einsatz | Schildwall verfügbar | Einsatz je verfügbarem Zug |
|---|---|---|---|---|
| byactiontype | 2.838.458 | 43,50 % | 57,67 % | 75,42 % |
| uniform | 3.143.089 | 42,44 % | 58,52 % | 72,52 % |
| aggressive | 1.121.011 | 7,36 % | 92,79 % | 7,93 % |
| defensive | 2.151.420 | 25,86 % | 74,87 % | 34,54 % |

