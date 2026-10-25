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
| dmg +2,5 % | erkannt | erkannt | erkannt | erkannt |
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
| ability +5 % | erkannt | erkannt | nicht erkannt | erkannt |
| ability +2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -5 % | erkannt | erkannt | nicht erkannt | erkannt |
| ability -10 % | erkannt | erkannt | nicht erkannt | erkannt |

## Einzelwerte

| Agent | Variante | Δ Schildträger (pp) | in Richtung | dagegen | unverändert | Schwelle | erfüllt | kleinste Stichprobe |
|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,82 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg +5 % | +1,47 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg +2,5 % | +0,64 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | dmg -2,5 % | -1,60 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg -5 % | -2,53 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg -10 % | -4,09 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +10 % | +2,62 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +5 % | +1,34 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +2,5 % | +0,70 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | hp -2,5 % | -0,71 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | hp -5 % | -1,46 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp -10 % | -2,97 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | ability +10 % | +0,30 | 24 | 2 | 2 | 18 von 26 | ja | 100.000 |
| byactiontype | ability +5 % | +0,17 | 24 | 4 | 0 | 19 von 28 | ja | nie |
| byactiontype | ability +2,5 % | +0,11 | 20 | 7 | 1 | 19 von 27 | ja | nie |
| byactiontype | ability -2,5 % | -0,05 | 16 | 6 | 6 | 16 von 22 | ja | nie |
| byactiontype | ability -5 % | -0,09 | 20 | 5 | 3 | 18 von 25 | ja | nie |
| byactiontype | ability -10 % | -0,23 | 24 | 3 | 1 | 19 von 27 | ja | 100.000 |
| uniform | dmg +10 % | +2,99 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg +5 % | +1,60 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg +2,5 % | +0,69 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| uniform | dmg -2,5 % | -1,72 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg -5 % | -2,68 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg -10 % | -4,20 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +10 % | +2,72 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +5 % | +1,39 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp +2,5 % | +0,73 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | hp -2,5 % | -0,68 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | hp -5 % | -1,48 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp -10 % | -3,20 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability +10 % | +0,56 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | ability +5 % | +0,33 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| uniform | ability +2,5 % | +0,19 | 27 | 1 | 0 | 19 von 28 | ja | 100.000 |
| uniform | ability -2,5 % | -0,08 | 21 | 3 | 4 | 17 von 24 | ja | nie |
| uniform | ability -5 % | -0,24 | 28 | 0 | 0 | 19 von 28 | ja | 100.000 |
| uniform | ability -10 % | -0,48 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| aggressive | dmg +10 % | +2,54 | 27 | 1 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg +5 % | +1,50 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg +2,5 % | +0,39 | 19 | 8 | 1 | 19 von 27 | ja | nie |
| aggressive | dmg -2,5 % | -2,08 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg -5 % | -3,23 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg -10 % | -5,01 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp +10 % | +5,21 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp +5 % | +2,73 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp +2,5 % | +1,60 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -2,5 % | -1,43 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -5 % | -2,72 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | hp -10 % | -5,73 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | ability +10 % | -0,15 | 11 | 16 | 1 | 19 von 27 | nein | nie |
| aggressive | ability +5 % | +0,06 | 15 | 13 | 0 | 19 von 28 | nein | nie |
| aggressive | ability +2,5 % | +0,03 | 13 | 15 | 0 | 19 von 28 | nein | nie |
| aggressive | ability -2,5 % | -0,02 | 14 | 12 | 2 | 18 von 26 | nein | nie |
| aggressive | ability -5 % | +0,03 | 13 | 15 | 0 | 19 von 28 | nein | nie |
| aggressive | ability -10 % | +0,44 | 5 | 21 | 2 | 18 von 26 | nein | nie |
| defensive | dmg +10 % | +1,66 | 24 | 4 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg +5 % | +0,97 | 20 | 8 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg +2,5 % | +0,52 | 20 | 8 | 0 | 19 von 28 | ja | nie |
| defensive | dmg -2,5 % | -1,16 | 23 | 5 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg -5 % | -1,72 | 28 | 0 | 0 | 19 von 28 | ja | 50.000 |
| defensive | dmg -10 % | -2,52 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +10 % | +2,20 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +5 % | +1,16 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp +2,5 % | +0,63 | 26 | 1 | 1 | 19 von 27 | ja | 50.000 |
| defensive | hp -2,5 % | -0,65 | 27 | 1 | 0 | 19 von 28 | ja | 50.000 |
| defensive | hp -5 % | -1,30 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp -10 % | -2,67 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| defensive | ability +10 % | +0,95 | 22 | 6 | 0 | 19 von 28 | ja | 100.000 |
| defensive | ability +5 % | +0,33 | 19 | 8 | 1 | 19 von 27 | ja | nie |
| defensive | ability +2,5 % | +0,10 | 16 | 12 | 0 | 19 von 28 | nein | nie |
| defensive | ability -2,5 % | +0,04 | 12 | 15 | 1 | 19 von 27 | nein | nie |
| defensive | ability -5 % | -0,52 | 21 | 6 | 1 | 19 von 27 | ja | nie |
| defensive | ability -10 % | -1,08 | 23 | 5 | 0 | 19 von 28 | ja | 50.000 |
