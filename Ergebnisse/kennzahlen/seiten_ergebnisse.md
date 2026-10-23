# Seitenvorteil: normale und vertauschte Basisläufe

Erzeugt von `Ergebnisse/kennzahlen/seiten_auswertung.py`. Normal = Team A auf Blüte A (bestehender Basislauf Seed 2026), vertauscht = Team A auf Blüte B (Option `--swap-sides`, sonst identisch), seitenbereinigt = Mittel aus beiden. Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung.

Ausgewertete Agenten: byactiontype, uniform, aggressive, defensive

## Spiegelzeilen (84 × 72 Partien)

In den Spiegelzeilen spielt dieselbe Komposition gegen sich selbst; ein Abstand von 50 % ist ein Seitenvorteil. Normal steht Team A auf Blüte A, vertauscht auf Blüte B.

| Agent | Team A normal | Team A vertauscht | Blüte A gewinnt (Mittel) | Unentschieden normal / vertauscht |
|---|---|---|---|---|
| byactiontype | 50,19 % | 49,69 % | 50,25 % | 43 / 41 |
| uniform | 48,88 % | 50,29 % | 49,30 % | 54 / 59 |
| aggressive | 60,37 % | 38,85 % | 60,76 % | 14 / 10 |
| defensive | 57,53 % | 40,70 % | 58,42 % | 870 / 854 |

## Helden: bereinigte Siegquote (%)

### byactiontype

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 63,31 | 63,17 | 63,24 | -0,14 | 1 | 1 |
| elfe | 60,29 | 60,31 | 60,30 | +0,02 | 2 | 2 |
| berserker | 56,88 | 56,71 | 56,79 | -0,16 | 3 | 3 |
| illusionist | 55,96 | 55,89 | 55,92 | -0,08 | 4 | 4 |
| wand | 52,96 | 53,04 | 53,00 | +0,09 | 5 | 5 |
| bogenschuetze | 47,69 | 47,99 | 47,84 | +0,30 | 6 | 6 |
| schildtraeger | 47,23 | 47,28 | 47,25 | +0,05 | 7 | 7 |
| musiker | 37,45 | 37,13 | 37,29 | -0,32 | 8 | 8 |
| hugger | 28,23 | 28,48 | 28,35 | +0,25 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### uniform

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 62,66 | 62,48 | 62,57 | -0,18 | 1 | 1 |
| elfe | 61,81 | 61,99 | 61,90 | +0,18 | 2 | 2 |
| wand | 55,19 | 55,01 | 55,10 | -0,18 | 3 | 3 |
| berserker | 54,75 | 54,55 | 54,65 | -0,19 | 4 | 4 |
| bogenschuetze | 53,43 | 53,55 | 53,49 | +0,12 | 5 | 5 |
| illusionist | 52,90 | 52,99 | 52,94 | +0,10 | 6 | 6 |
| schildtraeger | 51,25 | 51,31 | 51,28 | +0,06 | 7 | 7 |
| musiker | 34,99 | 35,21 | 35,10 | +0,22 | 8 | 8 |
| hugger | 23,04 | 22,91 | 22,98 | -0,13 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### aggressive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| wand | 63,35 | 74,57 | 68,96 | +11,22 | 2 | 1 |
| schildtraeger | 55,40 | 61,91 | 58,66 | +6,51 | 3 | 2 |
| berserker | 68,07 | 45,96 | 57,01 | -22,11 | 1 | 3 |
| illusionist | 51,19 | 59,36 | 55,28 | +8,17 | 5 | 4 |
| bogenschuetze | 54,67 | 41,85 | 48,26 | -12,83 | 4 | 5 |
| elfe | 44,27 | 46,48 | 45,38 | +2,20 | 6 | 6 |
| musiker | 41,47 | 47,41 | 44,44 | +5,94 | 8 | 7 |
| elementarmagier | 42,15 | 39,53 | 40,84 | -2,62 | 7 | 8 |
| hugger | 29,41 | 32,92 | 31,17 | +3,51 | 9 | 9 |

Spearman normal / bereinigt: 0,917; normal / vertauscht: 0,517

### defensive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elfe | 71,31 | 76,63 | 73,97 | +5,32 | 1 | 1 |
| bogenschuetze | 62,85 | 54,35 | 58,60 | -8,50 | 3 | 2 |
| berserker | 63,10 | 51,56 | 57,33 | -11,54 | 2 | 3 |
| wand | 49,50 | 54,25 | 51,87 | +4,76 | 4 | 4 |
| elementarmagier | 47,95 | 46,16 | 47,06 | -1,79 | 5 | 5 |
| schildtraeger | 44,65 | 46,76 | 45,70 | +2,10 | 6 | 6 |
| musiker | 41,35 | 45,08 | 43,22 | +3,74 | 7 | 7 |
| illusionist | 39,42 | 43,68 | 41,55 | +4,26 | 8 | 8 |
| hugger | 29,88 | 31,53 | 30,70 | +1,65 | 9 | 9 |

Spearman normal / bereinigt: 0,983; normal / vertauscht: 0,933

### Spearman der neun Helden zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,917 | 0,917 | +0,000 |
| byactiontype / aggressive | 0,283 | 0,100 | -0,183 |
| byactiontype / defensive | 0,617 | 0,567 | -0,050 |
| uniform / aggressive | 0,333 | 0,167 | -0,167 |
| uniform / defensive | 0,733 | 0,717 | -0,017 |
| aggressive / defensive | 0,550 | 0,300 | -0,250 |

## Kompositionen

| Agent | Spearman normal / bereinigt | größte Änderung durch die Bereinigung | fünf stärkste (bereinigt) |
|---|---|---|---|
| byactiontype | 0,999 | elementarmagier+elfe+musiker -1,42 pp | elementarmagier+elfe+illusionist 76,32, berserker+elementarmagier+elfe 75,48, berserker+elementarmagier+illusionist 73,65, berserker+elfe+illusionist 72,81, elementarmagier+illusionist+wand 71,42 |
| uniform | 0,999 | bogenschuetze+elfe+illusionist +1,07 pp | bogenschuetze+elementarmagier+elfe 76,25, berserker+elementarmagier+elfe 76,00, elementarmagier+elfe+illusionist 75,17, elementarmagier+elfe+wand 73,62, berserker+bogenschuetze+elfe 72,61 |
| aggressive | 0,888 | hugger+illusionist+wand +16,42 pp | berserker+schildtraeger+wand 89,27, illusionist+schildtraeger+wand 80,73, berserker+illusionist+wand 77,80, bogenschuetze+schildtraeger+wand 75,89, elfe+schildtraeger+wand 72,85 |
| defensive | 0,948 | illusionist+schildtraeger+wand +11,59 pp | bogenschuetze+elfe+musiker 85,95, berserker+bogenschuetze+elfe 84,32, bogenschuetze+elementarmagier+elfe 83,63, bogenschuetze+elfe+illusionist 81,00, bogenschuetze+elfe+wand 80,29 |

### Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,965 | 0,966 | +0,001 |
| byactiontype / aggressive | 0,461 | 0,389 | -0,072 |
| byactiontype / defensive | 0,562 | 0,589 | +0,028 |
| uniform / aggressive | 0,510 | 0,444 | -0,066 |
| uniform / defensive | 0,625 | 0,658 | +0,032 |
| aggressive / defensive | 0,536 | 0,327 | -0,209 |

## Top-D und Top-B

| Agent | Fall | c_top | Top-D 0,01 | Top-D 0,02 | Top-D 0,04 | Top-D 0,08 | Top-B |
|---|---|---|---|---|---|---|---|
| byactiontype | normal | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 34 |
| byactiontype | vertauscht | elementarmagier+elfe+illusionist | 1 | 2 | 2 | 6 | 32 |
| byactiontype | bereinigt | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 23 |
| uniform | normal | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| uniform | vertauscht | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 9 | 35 |
| uniform | bereinigt | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 8 | 33 |
| aggressive | normal | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 35 |
| aggressive | vertauscht | illusionist+schildtraeger+wand | 1 | 1 | 1 | 1 | 44 |
| aggressive | bereinigt | berserker+schildtraeger+wand | 1 | 1 | 1 | 1 | 30 |
| defensive | normal | berserker+bogenschuetze+elfe | 1 | 2 | 3 | 3 | 54 |
| defensive | vertauscht | bogenschuetze+elfe+musiker | 1 | 1 | 1 | 1 | 64 |
| defensive | bereinigt | bogenschuetze+elfe+musiker | 1 | 1 | 2 | 3 | 49 |

## Kompositionen: Lage und Streuung (seitenbereinigt)

SD mit n − 1; in Klammern der Wert des normalen Laufs.

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % (50,00) | 14,63 pp (14,67) | 14,47 % (14,19) | 76,32 % (76,10) |
| uniform | 50,00 % (50,00) | 16,69 pp (16,70) | 10,58 % (10,34) | 76,25 % (76,56) |
| aggressive | 50,00 % (50,00) | 15,23 pp (16,60) | 20,73 % (18,19) | 89,27 % (91,42) |
| defensive | 50,00 % (50,00) | 16,88 pp (17,97) | 14,04 % (11,83) | 85,95 % (86,55) |

## Grundwertsumme und Siegquote

Grundwertsumme wie in kennzahlen.py (Schritt 5): z(Lebenspunkte) + z(Grundschaden) über die neun Helden, für Kompositionen die Summe ihrer drei Helden.

| Agent | Helden normal | Helden seitenbereinigt | Kompositionen normal | Kompositionen seitenbereinigt |
|---|---|---|---|---|
| byactiontype | -0,117 | -0,117 | 0,213 | 0,215 |
| uniform | 0,150 | 0,150 | 0,382 | 0,380 |
| aggressive | 0,367 | 0,517 | 0,494 | 0,597 |
| defensive | 0,133 | 0,233 | 0,164 | 0,191 |

