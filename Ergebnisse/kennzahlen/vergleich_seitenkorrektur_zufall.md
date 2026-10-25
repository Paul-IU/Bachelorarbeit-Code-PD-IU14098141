# Zufallsagenten vor und nach der Seitenkorrektur

Erzeugt von `Ergebnisse/kennzahlen/vergleich_seitenkorrektur_zufall.py`. Basisläufe Seed 2026, gleiche Seeds, nur die beiden korrigierten Regelstellen (Sichtlinie, Reihenfolge der Flächenwirkungen) unterscheiden sich.

## Partien mit anderem Ausgang

| Agent | Partien | anderer Sieger | anderes Ergebnis (Sieger, Endgrund, Runden, Überlebende oder Rest-LP) | davon Paarungen |
|---|---|---|---|---|
| byactiontype | 257.040 | 690 (0,27 %) | 4.385 (1,71 %) | 1.468 von 3.570 |
| uniform | 257.040 | 883 (0,34 %) | 4.900 (1,91 %) | 1.574 von 3.570 |

## Helden: Änderung der Siegquote im Verhältnis zur bisherigen Seed-Streuung

z = (neu − alt) / SD; SD = Standardabweichung über die alten Seeds 2026-2030. Die Differenz zweier unabhängiger Läufe hätte die Streuung √2 · SD; hier sind die Läufe über die Seeds gekoppelt und weichen nur an den Kantenfällen ab, die Änderung sollte also deutlich unter √2 · SD liegen.

### byactiontype (SD aus 5 Seeds)

| Held | alt | neu | Δ (pp) | SD (pp) | z |
|---|---|---|---|---|---|
| elementarmagier | 63,31 | 63,30 | -0,016 | 0,157 | -0,10 |
| elfe | 60,29 | 60,31 | +0,014 | 0,147 | +0,10 |
| berserker | 56,88 | 56,87 | -0,002 | 0,073 | -0,02 |
| illusionist | 55,96 | 55,95 | -0,017 | 0,163 | -0,10 |
| wand | 52,96 | 52,95 | -0,007 | 0,120 | -0,06 |
| bogenschuetze | 47,69 | 47,70 | +0,005 | 0,162 | +0,03 |
| schildtraeger | 47,23 | 47,23 | +0,006 | 0,141 | +0,04 |
| musiker | 37,45 | 37,43 | -0,022 | 0,153 | -0,14 |
| hugger | 28,23 | 28,27 | +0,038 | 0,182 | +0,21 |

Innerhalb einer SD: 9 von 9; größtes |z| 0,21; Rangfolge unverändert.

### uniform (SD aus 5 Seeds)

| Held | alt | neu | Δ (pp) | SD (pp) | z |
|---|---|---|---|---|---|
| elementarmagier | 62,66 | 62,65 | -0,009 | 0,114 | -0,08 |
| elfe | 61,81 | 61,78 | -0,030 | 0,093 | -0,33 |
| wand | 55,19 | 55,22 | +0,031 | 0,106 | +0,29 |
| berserker | 54,75 | 54,72 | -0,027 | 0,159 | -0,17 |
| bogenschuetze | 53,43 | 53,39 | -0,035 | 0,117 | -0,30 |
| illusionist | 52,90 | 52,90 | +0,009 | 0,187 | +0,05 |
| schildtraeger | 51,25 | 51,29 | +0,041 | 0,126 | +0,32 |
| musiker | 34,99 | 35,02 | +0,032 | 0,025 | +1,27 |
| hugger | 23,04 | 23,03 | -0,012 | 0,048 | -0,24 |

Innerhalb einer SD: 8 von 9; größtes |z| 1,27; Rangfolge unverändert.

