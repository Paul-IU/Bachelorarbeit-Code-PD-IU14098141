# Seitenvorteil: normale und vertauschte Basisläufe

Erzeugt von `Ergebnisse/kennzahlen/seiten_auswertung.py`. Normal = Team A auf Blüte A (bestehender Basislauf Seed 2026), vertauscht = Team A auf Blüte B (Option `--swap-sides`, sonst identisch), seitenbereinigt = Mittel aus beiden. Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung.

Ausgewertete Agenten: byactiontype, uniform, aggressive, defensive

## Spiegelzeilen (84 × 72 Partien)

In den Spiegelzeilen spielt dieselbe Komposition gegen sich selbst; ein Abstand von 50 % ist ein Seitenvorteil. Normal steht Team A auf Blüte A, vertauscht auf Blüte B.

| Agent | Team A normal | Team A vertauscht | Blüte A gewinnt (Mittel) | Unentschieden normal / vertauscht |
|---|---|---|---|---|
| byactiontype | 50,12 % | 49,80 % | 50,16 % | 43 / 40 |
| uniform | 48,83 % | 50,31 % | 49,26 % | 54 / 59 |
| aggressive | 52,96 % | 52,96 % | 50,00 % | 8 / 8 |
| defensive | 52,10 % | 52,10 % | 50,00 % | 1230 / 1230 |

## Helden: bereinigte Siegquote (%)

### byactiontype

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 63,30 | 63,17 | 63,24 | -0,12 | 1 | 1 |
| elfe | 60,31 | 60,31 | 60,31 | +0,00 | 2 | 2 |
| berserker | 56,87 | 56,73 | 56,80 | -0,14 | 3 | 3 |
| illusionist | 55,95 | 55,86 | 55,90 | -0,08 | 4 | 4 |
| wand | 52,95 | 53,04 | 52,99 | +0,09 | 5 | 5 |
| bogenschuetze | 47,70 | 48,01 | 47,85 | +0,31 | 6 | 6 |
| schildtraeger | 47,23 | 47,26 | 47,25 | +0,03 | 7 | 7 |
| musiker | 37,43 | 37,13 | 37,28 | -0,30 | 8 | 8 |
| hugger | 28,27 | 28,48 | 28,37 | +0,21 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### uniform

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elementarmagier | 62,65 | 62,50 | 62,57 | -0,15 | 1 | 1 |
| elfe | 61,78 | 61,97 | 61,87 | +0,19 | 2 | 2 |
| wand | 55,22 | 55,01 | 55,12 | -0,21 | 3 | 3 |
| berserker | 54,72 | 54,54 | 54,63 | -0,18 | 4 | 4 |
| bogenschuetze | 53,39 | 53,56 | 53,47 | +0,16 | 5 | 5 |
| illusionist | 52,90 | 52,98 | 52,94 | +0,08 | 6 | 6 |
| schildtraeger | 51,29 | 51,33 | 51,31 | +0,04 | 7 | 7 |
| musiker | 35,02 | 35,19 | 35,11 | +0,17 | 8 | 8 |
| hugger | 23,03 | 22,92 | 22,97 | -0,11 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### aggressive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| bogenschuetze | 76,28 | 76,28 | 76,28 | +0,00 | 1 | 1 |
| wand | 68,70 | 68,70 | 68,70 | +0,00 | 2 | 2 |
| schildtraeger | 56,56 | 56,56 | 56,56 | +0,00 | 3 | 3 |
| illusionist | 50,00 | 50,00 | 50,00 | +0,00 | 4 | 4 |
| berserker | 49,85 | 49,85 | 49,85 | +0,00 | 5 | 5 |
| elementarmagier | 42,87 | 42,87 | 42,87 | +0,00 | 6 | 6 |
| musiker | 40,63 | 40,63 | 40,63 | +0,00 | 7 | 7 |
| elfe | 39,41 | 39,41 | 39,41 | +0,00 | 8 | 8 |
| hugger | 25,69 | 25,69 | 25,69 | +0,00 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### defensive

| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |
|---|---|---|---|---|---|---|
| elfe | 79,92 | 79,92 | 79,92 | +0,00 | 1 | 1 |
| bogenschuetze | 73,20 | 73,20 | 73,20 | +0,00 | 2 | 2 |
| wand | 50,95 | 50,95 | 50,95 | +0,00 | 3 | 3 |
| berserker | 47,63 | 47,63 | 47,63 | +0,00 | 4 | 4 |
| elementarmagier | 44,68 | 44,68 | 44,68 | +0,00 | 5 | 5 |
| schildtraeger | 44,51 | 44,51 | 44,51 | +0,00 | 6 | 6 |
| illusionist | 41,31 | 41,31 | 41,31 | +0,00 | 7 | 7 |
| musiker | 37,80 | 37,80 | 37,80 | +0,00 | 8 | 8 |
| hugger | 30,00 | 30,00 | 30,00 | +0,00 | 9 | 9 |

Spearman normal / bereinigt: 1,000; normal / vertauscht: 1,000

### Spearman der neun Helden zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,917 | 0,917 | +0,000 |
| byactiontype / aggressive | 0,033 | 0,033 | +0,000 |
| byactiontype / defensive | 0,600 | 0,600 | +0,000 |
| uniform / aggressive | 0,167 | 0,167 | +0,000 |
| uniform / defensive | 0,767 | 0,767 | +0,000 |
| aggressive / defensive | 0,400 | 0,400 | +0,000 |

## Kompositionen

| Agent | Spearman normal / bereinigt | größte Änderung durch die Bereinigung | fünf stärkste (bereinigt) |
|---|---|---|---|
| byactiontype | 0,999 | elementarmagier+elfe+musiker -1,47 pp | elementarmagier+elfe+illusionist 76,35, berserker+elementarmagier+elfe 75,51, berserker+elementarmagier+illusionist 73,64, berserker+elfe+illusionist 72,82, elementarmagier+illusionist+wand 71,39 |
| uniform | 0,999 | bogenschuetze+elfe+illusionist +1,08 pp | bogenschuetze+elementarmagier+elfe 76,25, berserker+elementarmagier+elfe 75,97, elementarmagier+elfe+illusionist 75,14, elementarmagier+elfe+wand 73,64, berserker+bogenschuetze+elfe 72,59 |
| aggressive | 1,000 | berserker+bogenschuetze+elementarmagier +0,00 pp | bogenschuetze+elfe+wand 90,56, berserker+bogenschuetze+wand 86,85, bogenschuetze+musiker+wand 86,70, bogenschuetze+illusionist+wand 86,58, bogenschuetze+schildtraeger+wand 85,96 |
| defensive | 1,000 | berserker+bogenschuetze+elementarmagier +0,00 pp | bogenschuetze+elfe+wand 94,60, bogenschuetze+elfe+schildtraeger 91,50, bogenschuetze+elfe+illusionist 87,01, bogenschuetze+elfe+musiker 86,92, bogenschuetze+elfe+hugger 86,15 |

### Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman normal | Spearman seitenbereinigt | Δ |
|---|---|---|---|
| byactiontype / uniform | 0,965 | 0,966 | +0,000 |
| byactiontype / aggressive | 0,265 | 0,265 | +0,000 |
| byactiontype / defensive | 0,454 | 0,458 | +0,004 |
| uniform / aggressive | 0,396 | 0,399 | +0,003 |
| uniform / defensive | 0,563 | 0,571 | +0,008 |
| aggressive / defensive | 0,445 | 0,445 | +0,000 |

## Top-D und Top-B

| Agent | Fall | c_top | Top-D 0,01 | Top-D 0,02 | Top-D 0,04 | Top-D 0,08 | Top-B |
|---|---|---|---|---|---|---|---|
| byactiontype | normal | elementarmagier+elfe+illusionist | 2 | 2 | 3 | 6 | 35 |
| byactiontype | vertauscht | elementarmagier+elfe+illusionist | 1 | 2 | 2 | 6 | 33 |
| byactiontype | bereinigt | elementarmagier+elfe+illusionist | 1 | 2 | 3 | 6 | 24 |
| uniform | normal | bogenschuetze+elementarmagier+elfe | 2 | 2 | 4 | 6 | 38 |
| uniform | vertauscht | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 9 | 36 |
| uniform | bereinigt | bogenschuetze+elementarmagier+elfe | 2 | 3 | 4 | 8 | 34 |
| aggressive | normal | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 49 |
| aggressive | vertauscht | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 49 |
| aggressive | bereinigt | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 49 |
| defensive | normal | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |
| defensive | vertauscht | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |
| defensive | bereinigt | bogenschuetze+elfe+wand | 1 | 1 | 1 | 1 | 47 |

## Kompositionen: Lage und Streuung (seitenbereinigt)

SD mit n − 1; in Klammern der Wert des normalen Laufs.

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % (50,00) | 14,62 pp (14,66) | 14,47 % (14,17) | 76,35 % (76,17) |
| uniform | 50,00 % (50,00) | 16,68 pp (16,69) | 10,58 % (10,36) | 76,25 % (76,56) |
| aggressive | 50,00 % (50,00) | 21,36 pp (21,36) | 9,96 % (9,96) | 90,56 % (90,56) |
| defensive | 50,00 % (50,00) | 21,76 pp (21,76) | 10,36 % (10,36) | 94,60 % (94,60) |

## Grundwertsumme und Siegquote

Grundwertsumme wie in kennzahlen.py (Schritt 5): z(Lebenspunkte) + z(Grundschaden) über die neun Helden, für Kompositionen die Summe ihrer drei Helden.

| Agent | Helden normal | Helden seitenbereinigt | Kompositionen normal | Kompositionen seitenbereinigt |
|---|---|---|---|---|
| byactiontype | -0,117 | -0,117 | 0,212 | 0,213 |
| uniform | 0,150 | 0,150 | 0,379 | 0,380 |
| aggressive | 0,783 | 0,783 | 0,799 | 0,799 |
| defensive | 0,317 | 0,317 | 0,290 | 0,290 |

