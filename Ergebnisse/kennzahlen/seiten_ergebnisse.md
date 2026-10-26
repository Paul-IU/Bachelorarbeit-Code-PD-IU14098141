# Seitenvorteil: normale und vertauschte Basisläufe

Erzeugt von `Ergebnisse/kennzahlen/seiten_auswertung.py`. Normal = Team A auf Blüte A (bestehender Basislauf Seed 2026), vertauscht = Team A auf Blüte B (Option `--swap-sides`, sonst identisch), seitenbereinigt = Mittel aus beiden. Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung.

Ausgewertete Agenten: byactiontype, uniform, aggressive, defensive

## Spiegelzeilen (84 × 72 Partien)

In den Spiegelzeilen spielt dieselbe Komposition gegen sich selbst; ein Abstand von 50 % ist ein Seitenvorteil. Normal steht Team A auf Blüte A, vertauscht auf Blüte B.

| Agent | Team A normal | Team A vertauscht | Blüte A gewinnt (Mittel) | Unentschieden normal / vertauscht |
|---|---|---|---|---|
| byactiontype | 49,90 % | 50,36 % | 49,77 % | 54 / 47 |
| uniform | 49,70 % | 49,90 % | 49,90 % | 46 / 72 |
| aggressive | 50,00 % | 50,00 % | 50,00 % | 8 / 8 |
| defensive | 50,00 % | 50,00 % | 50,00 % | 1264 / 1264 |

## Helden: bereinigte Siegquote (%)

### byactiontype

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 63,10 | 63,17 | 63,13 | +0,07 | 1 | 1 |
| elfe | 60,42 | 60,40 | 60,41 | -0,02 | 2 | 2 |
| berserker | 56,86 | 56,86 | 56,86 | +0,00 | 3 | 3 |
| illusionist | 56,10 | 56,05 | 56,07 | -0,06 | 4 | 4 |
| wand | 52,91 | 53,08 | 52,99 | +0,17 | 5 | 5 |
| bogenschuetze | 47,95 | 47,70 | 47,82 | -0,25 | 6 | 6 |
| schildtraeger | 47,31 | 47,13 | 47,22 | -0,18 | 7 | 7 |
| musiker | 37,45 | 37,30 | 37,38 | -0,15 | 8 | 8 |
| hugger | 27,90 | 28,32 | 28,11 | +0,42 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### uniform

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 62,50 | 62,40 | 62,45 | -0,10 | 1 | 1 |
| elfe | 61,81 | 61,93 | 61,87 | +0,12 | 2 | 2 |
| wand | 55,26 | 55,13 | 55,19 | -0,13 | 3 | 3 |
| berserker | 54,59 | 54,61 | 54,60 | +0,02 | 4 | 4 |
| bogenschuetze | 53,50 | 53,53 | 53,52 | +0,03 | 5 | 5 |
| illusionist | 53,13 | 53,02 | 53,08 | -0,11 | 6 | 6 |
| schildtraeger | 51,21 | 51,36 | 51,29 | +0,15 | 7 | 7 |
| musiker | 35,13 | 35,15 | 35,14 | +0,02 | 8 | 8 |
| hugger | 22,87 | 22,87 | 22,87 | -0,00 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### aggressive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| bogenschuetze | 75,34 | 75,34 | 75,34 | +0,00 | 1 | 1 |
| wand | 70,73 | 70,73 | 70,73 | +0,00 | 2 | 2 |
| schildtraeger | 58,77 | 58,77 | 58,77 | +0,00 | 3 | 3 |
| illusionist | 50,48 | 50,48 | 50,48 | +0,00 | 4 | 4 |
| berserker | 47,26 | 47,26 | 47,26 | +0,00 | 5 | 5 |
| elementarmagier | 42,03 | 42,03 | 42,03 | +0,00 | 6 | 6 |
| musiker | 41,18 | 41,18 | 41,18 | +0,00 | 7 | 7 |
| elfe | 38,33 | 38,33 | 38,33 | +0,00 | 8 | 8 |
| hugger | 25,88 | 25,88 | 25,88 | +0,00 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### defensive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elfe | 79,86 | 79,86 | 79,86 | +0,00 | 1 | 1 |
| bogenschuetze | 73,09 | 73,09 | 73,09 | +0,00 | 2 | 2 |
| wand | 51,65 | 51,65 | 51,65 | +0,00 | 3 | 3 |
| berserker | 47,15 | 47,15 | 47,15 | +0,00 | 4 | 4 |
| schildtraeger | 44,77 | 44,77 | 44,77 | +0,00 | 5 | 5 |
| elementarmagier | 44,64 | 44,64 | 44,64 | +0,00 | 6 | 6 |
| illusionist | 41,35 | 41,35 | 41,35 | +0,00 | 7 | 7 |
| musiker | 37,65 | 37,65 | 37,65 | +0,00 | 8 | 8 |
| hugger | 29,85 | 29,85 | 29,85 | +0,00 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### Spearman der neun Helden zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,917 | 0,917 | +0,000 |
| byactiontype / aggressive | 0,033 | 0,033 | +0,000 |
| byactiontype / defensive | 0,500 | 0,500 | +0,000 |
| uniform / aggressive | 0,167 | 0,167 | +0,000 |
| uniform / defensive | 0,667 | 0,667 | +0,000 |
| aggressive / defensive | 0,450 | 0,450 | +0,000 |

## Kompositionen

| Agent | Spearman normal / bereinigt | größte Änderung durch die Bereinigung | fünf stärkste (bereinigt) |
|---|---|---|---|
| byactiontype | 0,999 | berserker+hugger+musiker +1,11 pp | elementarmagier+elfe+illusionist 76,40, berserker+elementarmagier+elfe 75,53, berserker+elementarmagier+illusionist 73,34, berserker+elfe+illusionist 72,90, elementarmagier+illusionist+wand 71,63 |
| uniform | 0,999 | illusionist+schildtraeger+wand -1,20 pp | bogenschuetze+elementarmagier+elfe 76,46, berserker+elementarmagier+elfe 75,54, elementarmagier+elfe+illusionist 75,22, elementarmagier+elfe+wand 73,31, berserker+bogenschuetze+elfe 72,62 |
| aggressive | 1,000 | berserker+bogenschuetze+elementarmagier +0,00 pp | bogenschuetze+elfe+wand 90,26, bogenschuetze+schildtraeger+wand 89,73, bogenschuetze+musiker+wand 88,94, bogenschuetze+illusionist+wand 88,09, berserker+bogenschuetze+wand 86,18 |
| defensive | 1,000 | berserker+bogenschuetze+elementarmagier +0,00 pp | bogenschuetze+elfe+wand 94,88, bogenschuetze+elfe+schildtraeger 91,73, bogenschuetze+elfe+illusionist 86,92, bogenschuetze+elfe+musiker 85,90, bogenschuetze+elfe+hugger 85,47 |

### Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,966 | 0,966 | -0,000 |
| byactiontype / aggressive | 0,255 | 0,249 | -0,006 |
| byactiontype / defensive | 0,463 | 0,463 | +0,000 |
| uniform / aggressive | 0,391 | 0,391 | +0,000 |
| uniform / defensive | 0,571 | 0,572 | +0,001 |
| aggressive / defensive | 0,411 | 0,411 | +0,000 |

## Top-D und Top-B

| Agent | Fall | c_top | Top-D 0,01 | Top-D 0,02 | Top-D 0,04 | Top-D 0,08 | Top-B |
|---|---|---|---|---|---|---|---|
| byactiontype | normal | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 35 |
| byactiontype | vertauscht | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 34 |
| byactiontype | bereinigt | elementarmagier+elfe+illusionist | 1 | 2 | 2 | 6 | 25 |
| uniform | normal | berserker+elementarmagier+elfe | 2 | 3 | 3 | 7 | 37 |
| uniform | vertauscht | bogenschuetze+elementarmagier+elfe | 1 | 1 | 3 | 8 | 40 |
| uniform | bereinigt | bogenschuetze+elementarmagier+elfe | 1 | 3 | 3 | 8 | 33 |
| aggressive | normal | bogenschuetze+elfe+wand | 1 | 2 | 3 | 4 | 47 |
| aggressive | vertauscht | bogenschuetze+elfe+wand | 1 | 2 | 3 | 4 | 47 |
| aggressive | bereinigt | bogenschuetze+elfe+wand | 1 | 2 | 3 | 4 | 47 |
| defensive | normal | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |
| defensive | vertauscht | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |
| defensive | bereinigt | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |

## Kompositionen: Lage und Streuung (seitenbereinigt)

SD mit n − 1; in Klammern der Wert des normalen Laufs.

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % (50,00) | 14,70 pp (14,74) | 14,11 % (13,91) | 76,40 % (76,32) |
| uniform | 50,00 % (50,00) | 16,71 pp (16,73) | 10,31 % (10,07) | 76,46 % (76,32) |
| aggressive | 50,00 % (50,00) | 21,46 pp (21,46) | 10,74 % (10,74) | 90,26 % (90,26) |
| defensive | 50,00 % (50,00) | 21,83 pp (21,83) | 10,07 % (10,07) | 94,88 % (94,88) |

## Grundwertsumme und Siegquote

Grundwertsumme wie in kennzahlen.py (Schritt 5): z(Lebenspunkte) + z(Grundschaden) über die neun Helden, für Kompositionen die Summe ihrer drei Helden.

| Agent | Helden normal | Helden seitenbereinigt | Kompositionen normal | Kompositionen seitenbereinigt |
|---|---|---|---|---|
| byactiontype | -0,117 | -0,117 | 0,217 | 0,211 |
| uniform | 0,150 | 0,150 | 0,383 | 0,383 |
| aggressive | 0,783 | 0,783 | 0,837 | 0,837 |
| defensive | 0,350 | 0,350 | 0,302 | 0,302 |

