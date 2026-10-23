# Erkennung der Ungleichgewichte

Erzeugt von `Ergebnisse/kennzahlen/erkennung.py`. Je Variantenlauf gegen den Basislauf (Seed 2026) desselben Agenten.

**Kriterium vollständiger Lauf:** (a) bereinigte Siegquote des Schildträgers verschiebt sich in Richtung der Manipulation UND (b) mindestens k der veränderten Kompositionen mit Schildträger bewegen sich in dieselbe Richtung (Siegquote ohne Spiegelpaarung, Unentschieden als halber Sieg). Unveränderte Kompositionen zählen nicht; k = Schwelle des einseitigen Vorzeichentests, α = 0,05, für n = Anzahl veränderter Kompositionen (n = 28 → k = 19).

**Kriterium Simulationsumfang:** kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der der Schildträger in ≥ 95 % der 100 Wiederholungen in Richtung der Manipulation verschoben ist (aus `teilmengen_ergebnisse.md`).

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
| ability +10 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability +5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability +2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -2,5 % | erkannt | erkannt | nicht erkannt | nicht erkannt |
| ability -5 % | erkannt | erkannt | nicht erkannt | erkannt |
| ability -10 % | erkannt | erkannt | nicht erkannt | erkannt |

## Einzelwerte

| Agent | Variante | Δ Schildträger (pp) | in Richtung | dagegen | unverändert | Schwelle | erfüllt | kleinste Stichprobe |
|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,82 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | dmg +5 % | +1,46 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | dmg +2,5 % | +0,63 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | dmg -2,5 % | -1,60 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | dmg -5 % | -2,54 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | dmg -10 % | -4,11 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | hp +10 % | +2,61 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | hp +5 % | +1,34 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp +2,5 % | +0,70 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp -2,5 % | -0,71 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | hp -5 % | -1,46 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | hp -10 % | -2,97 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| byactiontype | ability +10 % | +0,30 | 25 | 3 | 0 | 19 von 28 | ja | 10.000 |
| byactiontype | ability +5 % | +0,17 | 23 | 3 | 2 | 18 von 26 | ja | 50.000 |
| byactiontype | ability +2,5 % | +0,11 | 21 | 7 | 0 | 19 von 28 | ja | 50.000 |
| byactiontype | ability -2,5 % | -0,05 | 16 | 6 | 6 | 16 von 22 | ja | 50.000 |
| byactiontype | ability -5 % | -0,09 | 21 | 6 | 1 | 19 von 27 | ja | 50.000 |
| byactiontype | ability -10 % | -0,24 | 24 | 4 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg +10 % | +2,99 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | dmg +5 % | +1,59 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | dmg +2,5 % | +0,69 | 27 | 1 | 0 | 19 von 28 | ja | 10.000 |
| uniform | dmg -2,5 % | -1,72 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | dmg -5 % | -2,70 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | dmg -10 % | -4,20 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | hp +10 % | +2,72 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | hp +5 % | +1,39 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | hp +2,5 % | +0,72 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp -2,5 % | -0,69 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | hp -5 % | -1,49 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | hp -10 % | -3,19 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| uniform | ability +10 % | +0,55 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability +5 % | +0,32 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability +2,5 % | +0,19 | 27 | 1 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability -2,5 % | -0,09 | 22 | 2 | 4 | 17 von 24 | ja | 10.000 |
| uniform | ability -5 % | -0,24 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| uniform | ability -10 % | -0,48 | 28 | 0 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg +10 % | +2,78 | 27 | 1 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | dmg +5 % | +1,20 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | dmg +2,5 % | +0,52 | 23 | 5 | 0 | 19 von 28 | ja | 10.000 |
| aggressive | dmg -2,5 % | -2,23 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | dmg -5 % | -3,40 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | dmg -10 % | -5,13 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp +10 % | +4,25 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp +5 % | +2,05 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp +2,5 % | +1,11 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp -2,5 % | -1,33 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp -5 % | -2,30 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | hp -10 % | -4,72 | 28 | 0 | 0 | 19 von 28 | ja | 1.000 |
| aggressive | ability +10 % | -0,27 | 11 | 17 | 0 | 19 von 28 | nein | nie |
| aggressive | ability +5 % | -0,07 | 11 | 17 | 0 | 19 von 28 | nein | nie |
| aggressive | ability +2,5 % | +0,01 | 15 | 13 | 0 | 19 von 28 | nein | nie |
| aggressive | ability -2,5 % | -0,04 | 18 | 10 | 0 | 19 von 28 | nein | 50.000 |
| aggressive | ability -5 % | -0,07 | 18 | 9 | 1 | 19 von 27 | nein | 50.000 |
| aggressive | ability -10 % | +0,06 | 14 | 14 | 0 | 19 von 28 | nein | nie |
| defensive | dmg +10 % | +2,58 | 26 | 2 | 0 | 19 von 28 | ja | 1.000 |
| defensive | dmg +5 % | +1,27 | 25 | 3 | 0 | 19 von 28 | ja | 10.000 |
| defensive | dmg +2,5 % | +0,57 | 22 | 6 | 0 | 19 von 28 | ja | 10.000 |
| defensive | dmg -2,5 % | -1,50 | 26 | 2 | 0 | 19 von 28 | ja | 10.000 |
| defensive | dmg -5 % | -2,19 | 27 | 1 | 0 | 19 von 28 | ja | 1.000 |
| defensive | dmg -10 % | -3,47 | 27 | 0 | 1 | 19 von 27 | ja | 1.000 |
| defensive | hp +10 % | +2,05 | 27 | 1 | 0 | 19 von 28 | ja | 1.000 |
| defensive | hp +5 % | +1,15 | 27 | 0 | 1 | 19 von 27 | ja | 1.000 |
| defensive | hp +2,5 % | +0,66 | 27 | 1 | 0 | 19 von 28 | ja | 10.000 |
| defensive | hp -2,5 % | -0,47 | 24 | 3 | 1 | 19 von 27 | ja | 10.000 |
| defensive | hp -5 % | -1,10 | 26 | 2 | 0 | 19 von 28 | ja | 1.000 |
| defensive | hp -10 % | -2,39 | 27 | 0 | 1 | 19 von 27 | ja | 1.000 |
| defensive | ability +10 % | +0,09 | 16 | 12 | 0 | 19 von 28 | nein | nie |
| defensive | ability +5 % | -0,17 | 11 | 17 | 0 | 19 von 28 | nein | nie |
| defensive | ability +2,5 % | -0,28 | 9 | 18 | 1 | 19 von 27 | nein | nie |
| defensive | ability -2,5 % | -0,12 | 15 | 12 | 1 | 19 von 27 | nein | 50.000 |
| defensive | ability -5 % | -0,25 | 21 | 6 | 1 | 19 von 27 | ja | 50.000 |
| defensive | ability -10 % | -0,60 | 24 | 4 | 0 | 19 von 28 | ja | 10.000 |
