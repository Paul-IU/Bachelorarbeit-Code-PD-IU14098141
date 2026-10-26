# Erkennung der Ungleichgewichte

Erzeugt von `Ergebnisse/kennzahlen/erkennung.py`. Je Variantenlauf gegen den Basislauf (Seed 2026) desselben Agenten.

Unentschieden zählen überall als halber Sieg (seit 13.10.2026).

**Kriterium vollständiger Lauf:** (a) bereinigte Siegquote des Schildträgers verschiebt sich in Richtung der Manipulation UND (b) mindestens k der veränderten Kompositionen mit Schildträger bewegen sich in dieselbe Richtung (Siegquote ohne Spiegelpaarung, Unentschieden als halber Sieg). Unveränderte Kompositionen zählen nicht; k = Schwelle des einseitigen Vorzeichentests, α = 0,05, für n = Anzahl veränderter Kompositionen (n = 28 → k = 19).

**Kriterium Simulationsumfang:** kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der dasselbe Kriterium (a) UND (b) in ≥ 95 % der 100 Wiederholungen erfüllt ist (aus `teilmengen_ergebnisse.md`).

## Übersicht

| Variante | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| dmg +10 % | erkannt | erkannt | erkannt | erkannt |
| dmg +5 % | erkannt | erkannt | erkannt | erkannt |
| dmg +2,5 % | erkannt | erkannt | erkannt | nicht erkannt |
| dmg -2,5 % | erkannt | erkannt | erkannt | erkannt |
| dmg -5 % | erkannt | erkannt | erkannt | erkannt |
| dmg -10 % | erkannt | erkannt | erkannt | erkannt |
| hp +10 % | erkannt | erkannt | erkannt | erkannt |
| hp +5 % | erkannt | erkannt | erkannt | erkannt |
| hp +2,5 % | erkannt | erkannt | erkannt | erkannt |
| hp -2,5 % | erkannt | erkannt | erkannt | erkannt |
| hp -5 % | erkannt | erkannt | erkannt | erkannt |
| hp -10 % | erkannt | erkannt | erkannt | erkannt |
| ability +10 % | erkannt | erkannt | nicht erkannt | erkannt |
| ability +5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability +2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -5 % | erkannt | erkannt | nicht erkannt | erkannt |
| ability -10 % | erkannt | erkannt | nicht erkannt | erkannt |

## Einzelwerte

| Agent | Variante | Δ Schildträger (pp) | in Richtung | dagegen | unverändert | Schwelle | erfüllt | kleinste Stichprobe |
|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,79 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg +5 % | +1,39 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg +2,5 % | +0,60 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | dmg -2,5 % | -1,53 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg -5 % | -2,40 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg -10 % | -3,93 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +10 % | +2,67 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +5 % | +1,41 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +2,5 % | +0,76 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | hp -2,5 % | -0,65 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | hp -5 % | -1,39 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp -10 % | -2,94 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | ability +10 % | +0,33 | 25 | 3 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | ability +5 % | +0,20 | 25 | 2 | 1 | 19 von 27 | ja | 100.000 |
| byactiontype | ability +2,5 % | +0,12 | 22 | 5 | 1 | 19 von 27 | ja | nie |
| byactiontype | ability -2,5 % | -0,04 | 19 | 7 | 2 | 18 von 26 | ja | nie |
| byactiontype | ability -5 % | -0,10 | 22 | 4 | 2 | 18 von 26 | ja | nie |
| byactiontype | ability -10 % | -0,25 | 25 | 2 | 1 | 19 von 27 | ja | 100.000 |
| uniform | dmg +10 % | +3,05 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg +5 % | +1,59 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg +2,5 % | +0,72 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| uniform | dmg -2,5 % | -1,75 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg -5 % | -2,66 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg -10 % | -4,20 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +10 % | +2,83 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +5 % | +1,53 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +2,5 % | +0,80 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | hp -2,5 % | -0,68 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | hp -5 % | -1,43 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp -10 % | -2,99 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability +10 % | +0,64 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | ability +5 % | +0,34 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| uniform | ability +2,5 % | +0,19 | 24 | 3 | 1 | 19 von 27 | ja | 100.000 |
| uniform | ability -2,5 % | -0,08 | 22 | 4 | 2 | 18 von 26 | ja | nie |
| uniform | ability -5 % | -0,22 | 27 | 1 | 0 | 19 von 28 | ja | 100.000 |
| uniform | ability -10 % | -0,45 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| aggressive | dmg +10 % | +2,51 | 27 | 1 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg +5 % | +1,43 | 26 | 2 | 0 | 19 von 28 | ja | 50.000 |
| aggressive | dmg +2,5 % | +0,51 | 20 | 7 | 1 | 19 von 27 | ja | nie |
| aggressive | dmg -2,5 % | -1,97 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg -5 % | -3,38 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg -10 % | -5,18 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp +10 % | +5,03 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp +5 % | +2,66 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp +2,5 % | +1,54 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -2,5 % | -1,34 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -5 % | -2,74 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -10 % | -5,63 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | ability +10 % | -0,23 | 9 | 18 | 1 | 19 von 27 | nein | nie |
| aggressive | ability +5 % | -0,02 | 10 | 18 | 0 | 19 von 28 | nein | nie |
| aggressive | ability +2,5 % | +0,05 | 14 | 13 | 1 | 19 von 27 | nein | nie |
| aggressive | ability -2,5 % | +0,05 | 13 | 13 | 2 | 18 von 26 | nein | nie |
| aggressive | ability -5 % | +0,07 | 8 | 20 | 0 | 19 von 28 | nein | nie |
| aggressive | ability -10 % | +0,57 | 5 | 23 | 0 | 19 von 28 | nein | nie |
| defensive | dmg +10 % | +1,63 | 25 | 3 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg +5 % | +0,89 | 24 | 4 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg +2,5 % | +0,46 | 18 | 9 | 1 | 19 von 27 | nein | nie |
| defensive | dmg -2,5 % | -1,12 | 25 | 3 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg -5 % | -1,61 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg -10 % | -2,47 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +10 % | +2,25 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +5 % | +1,23 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +2,5 % | +0,65 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| defensive | hp -2,5 % | -0,59 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| defensive | hp -5 % | -1,18 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp -10 % | -2,57 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | ability +10 % | +0,77 | 19 | 9 | 0 | 19 von 28 | ja | nie |
| defensive | ability +5 % | +0,19 | 16 | 11 | 1 | 19 von 27 | nein | nie |
| defensive | ability +2,5 % | -0,02 | 12 | 15 | 1 | 19 von 27 | nein | nie |
| defensive | ability -2,5 % | +0,12 | 16 | 12 | 0 | 19 von 28 | nein | nie |
| defensive | ability -5 % | -0,40 | 19 | 9 | 0 | 19 von 28 | ja | nie |
| defensive | ability -10 % | -0,96 | 23 | 4 | 1 | 19 von 27 | ja | 50.000 |
