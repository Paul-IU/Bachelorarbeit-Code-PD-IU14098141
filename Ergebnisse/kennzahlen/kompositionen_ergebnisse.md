# Kompositionen

Erzeugt von `Ergebnisse/kennzahlen/kompositionen.py`. Siegquote einer Komposition ohne Spiegelpaarung, Unentschieden als halber Sieg; Basis = Basislauf Seed 2026 des Agenten; 72 Varianten (ohne dmg +20 %). Rohdaten aller Änderungen: `kompositionen_delta.csv`.

## Teil A – Kompositionen in den Basisläufen

### A1 Lage und Streuung der 84 Kompositionssiegquoten

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % | 14,74 pp | 13,91 % | 76,32 % |
| uniform | 50,00 % | 16,73 pp | 10,07 % | 76,32 % |
| aggressive | 50,00 % | 21,46 pp | 10,74 % | 90,26 % |
| defensive | 50,00 % | 21,83 pp | 10,07 % | 94,88 % |

| Agent | Rang | fünf stärkste | Siegquote | fünf schwächste | Siegquote |
|---|---|---|---|---|---|
| byactiontype | 1 | elementarmagier+elfe+illusionist | 76,32 % | hugger+musiker+schildtraeger | 13,91 % |
|  | 2 | berserker+elementarmagier+elfe | 75,50 % | hugger+musiker+wand | 19,33 % |
|  | 3 | berserker+elfe+illusionist | 73,59 % | hugger+illusionist+musiker | 19,48 % |
|  | 4 | berserker+elementarmagier+illusionist | 72,77 % | bogenschuetze+hugger+musiker | 19,54 % |
|  | 5 | elementarmagier+illusionist+wand | 71,90 % | bogenschuetze+hugger+schildtraeger | 25,49 % |
| uniform | 1 | berserker+elementarmagier+elfe | 76,32 % | hugger+illusionist+musiker | 10,07 % |
|  | 2 | bogenschuetze+elementarmagier+elfe | 76,27 % | hugger+musiker+schildtraeger | 11,98 % |
|  | 3 | elementarmagier+elfe+illusionist | 75,25 % | hugger+musiker+wand | 14,42 % |
|  | 4 | elementarmagier+elfe+wand | 73,53 % | berserker+hugger+musiker | 18,41 % |
|  | 5 | berserker+bogenschuetze+elfe | 72,42 % | bogenschuetze+hugger+musiker | 19,61 % |
| aggressive | 1 | bogenschuetze+elfe+wand | 90,26 % | elfe+hugger+musiker | 10,74 % |
|  | 2 | bogenschuetze+schildtraeger+wand | 89,73 % | elementarmagier+elfe+hugger | 11,43 % |
|  | 3 | bogenschuetze+musiker+wand | 88,94 % | berserker+elfe+hugger | 13,15 % |
|  | 4 | bogenschuetze+illusionist+wand | 88,09 % | elementarmagier+hugger+musiker | 14,47 % |
|  | 5 | berserker+bogenschuetze+wand | 86,18 % | hugger+illusionist+musiker | 15,01 % |
| defensive | 1 | bogenschuetze+elfe+wand | 94,88 % | hugger+musiker+schildtraeger | 10,07 % |
|  | 2 | bogenschuetze+elfe+schildtraeger | 91,73 % | hugger+illusionist+musiker | 10,57 % |
|  | 3 | bogenschuetze+elfe+illusionist | 86,92 % | hugger+illusionist+schildtraeger | 11,35 % |
|  | 4 | bogenschuetze+elfe+musiker | 85,90 % | hugger+illusionist+wand | 12,64 % |
|  | 5 | bogenschuetze+elfe+hugger | 85,47 % | hugger+musiker+wand | 14,30 % |

### A2 Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman |
|---|---|
| byactiontype / uniform | 0,966 |
| byactiontype / aggressive | 0,255 |
| byactiontype / defensive | 0,463 |
| uniform / aggressive | 0,391 |
| uniform / defensive | 0,571 |
| aggressive / defensive | 0,411 |

## Teil B – Trennung betroffen / nicht betroffen

Änderung = Variante − Basis in pp. Getrennt = keine Komposition ohne Schildträger ist stärker in Richtung der Manipulation verschoben als die schwächste mit Schildträger. Überlappung = Anzahl der Kompositionen ohne Schildträger (von 56), die stärker in Richtung verschoben sind als die schwächste mit Schildträger.

| Agent | Variante | mit ST: Mittel | mit ST: min | mit ST: max | ohne ST: Mittel | ohne ST: min | ohne ST: max | getrennt | Überlappung |
|---|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +1,88 | +0,76 | +3,16 | -0,94 | -1,61 | -0,42 | ja | – |
| byactiontype | dmg +5 % | +0,94 | +0,32 | +1,81 | -0,47 | -0,92 | -0,10 | ja | – |
| byactiontype | dmg +2,5 % | +0,41 | +0,03 | +0,90 | -0,20 | -0,54 | +0,00 | ja | – |
| byactiontype | dmg -2,5 % | -1,03 | -1,87 | -0,13 | +0,52 | +0,18 | +1,02 | ja | – |
| byactiontype | dmg -5 % | -1,62 | -2,98 | -0,64 | +0,81 | +0,33 | +1,34 | ja | – |
| byactiontype | dmg -10 % | -2,65 | -4,80 | -1,05 | +1,33 | +0,70 | +1,94 | ja | – |
| byactiontype | hp +10 % | +1,80 | +0,79 | +3,71 | -0,90 | -1,36 | -0,42 | ja | – |
| byactiontype | hp +5 % | +0,95 | +0,44 | +1,96 | -0,48 | -0,87 | -0,22 | ja | – |
| byactiontype | hp +2,5 % | +0,52 | +0,10 | +1,24 | -0,26 | -0,55 | -0,08 | ja | – |
| byactiontype | hp -2,5 % | -0,44 | -0,99 | -0,15 | +0,22 | +0,05 | +0,52 | ja | – |
| byactiontype | hp -5 % | -0,94 | -1,69 | -0,32 | +0,47 | +0,08 | +0,92 | ja | – |
| byactiontype | hp -10 % | -1,98 | -3,20 | -1,26 | +0,99 | +0,49 | +1,66 | ja | – |
| byactiontype | ability +10 % | +0,23 | -0,13 | +0,64 | -0,11 | -0,33 | +0,05 | nein | 36 |
| byactiontype | ability +5 % | +0,14 | -0,13 | +0,37 | -0,07 | -0,25 | +0,07 | nein | 47 |
| byactiontype | ability +2,5 % | +0,08 | -0,12 | +0,32 | -0,04 | -0,18 | +0,07 | nein | 51 |
| byactiontype | ability -2,5 % | -0,03 | -0,23 | +0,10 | +0,01 | -0,03 | +0,07 | nein | 56 |
| byactiontype | ability -5 % | -0,07 | -0,27 | +0,10 | +0,03 | -0,05 | +0,12 | nein | 52 |
| byactiontype | ability -10 % | -0,17 | -0,44 | +0,05 | +0,09 | -0,03 | +0,25 | nein | 14 |
| uniform | dmg +10 % | +2,06 | +0,50 | +3,41 | -1,03 | -1,56 | -0,52 | ja | – |
| uniform | dmg +5 % | +1,07 | +0,22 | +2,07 | -0,54 | -0,97 | -0,17 | ja | – |
| uniform | dmg +2,5 % | +0,49 | -0,17 | +1,02 | -0,24 | -0,67 | -0,03 | nein | 18 |
| uniform | dmg -2,5 % | -1,18 | -2,09 | -0,52 | +0,59 | +0,13 | +1,04 | ja | – |
| uniform | dmg -5 % | -1,79 | -2,93 | -0,82 | +0,90 | +0,41 | +1,32 | ja | – |
| uniform | dmg -10 % | -2,84 | -4,47 | -1,51 | +1,42 | +0,69 | +2,14 | ja | – |
| uniform | hp +10 % | +1,91 | +1,17 | +3,53 | -0,95 | -1,49 | -0,27 | ja | – |
| uniform | hp +5 % | +1,03 | +0,52 | +2,01 | -0,51 | -0,84 | -0,13 | ja | – |
| uniform | hp +2,5 % | +0,54 | +0,17 | +1,07 | -0,27 | -0,59 | -0,03 | ja | – |
| uniform | hp -2,5 % | -0,46 | -0,79 | -0,17 | +0,23 | +0,07 | +0,55 | ja | – |
| uniform | hp -5 % | -0,97 | -1,87 | -0,45 | +0,48 | +0,12 | +0,92 | ja | – |
| uniform | hp -10 % | -2,02 | -3,70 | -1,20 | +1,01 | +0,50 | +1,79 | ja | – |
| uniform | ability +10 % | +0,43 | +0,03 | +0,82 | -0,22 | -0,42 | -0,02 | ja | – |
| uniform | ability +5 % | +0,23 | -0,08 | +0,50 | -0,11 | -0,25 | +0,03 | nein | 14 |
| uniform | ability +2,5 % | +0,13 | -0,12 | +0,27 | -0,06 | -0,17 | +0,03 | nein | 46 |
| uniform | ability -2,5 % | -0,05 | -0,13 | +0,03 | +0,03 | -0,03 | +0,13 | nein | 34 |
| uniform | ability -5 % | -0,15 | -0,33 | +0,02 | +0,07 | -0,07 | +0,27 | nein | 8 |
| uniform | ability -10 % | -0,31 | -0,55 | -0,03 | +0,15 | +0,00 | +0,45 | ja | – |
| aggressive | dmg +10 % | +1,69 | -0,38 | +5,19 | -0,85 | -2,51 | -0,07 | nein | 8 |
| aggressive | dmg +5 % | +0,96 | -0,35 | +3,93 | -0,48 | -2,26 | +0,13 | nein | 21 |
| aggressive | dmg +2,5 % | +0,35 | -0,17 | +1,69 | -0,17 | -0,75 | +0,08 | nein | 33 |
| aggressive | dmg -2,5 % | -1,33 | -2,91 | -0,22 | +0,67 | +0,10 | +4,99 | ja | – |
| aggressive | dmg -5 % | -2,28 | -5,71 | -0,30 | +1,14 | +0,18 | +5,39 | ja | – |
| aggressive | dmg -10 % | -3,49 | -7,11 | -0,94 | +1,75 | +0,03 | +6,28 | ja | – |
| aggressive | hp +10 % | +3,39 | +0,84 | +6,29 | -1,70 | -2,86 | -0,45 | ja | – |
| aggressive | hp +5 % | +1,79 | +0,25 | +3,38 | -0,90 | -1,62 | -0,17 | ja | – |
| aggressive | hp +2,5 % | +1,04 | +0,35 | +2,09 | -0,52 | -0,99 | -0,07 | ja | – |
| aggressive | hp -2,5 % | -0,90 | -1,81 | -0,18 | +0,45 | +0,03 | +1,71 | ja | – |
| aggressive | hp -5 % | -1,85 | -3,63 | -0,42 | +0,92 | +0,17 | +1,86 | ja | – |
| aggressive | hp -10 % | -3,80 | -7,04 | -0,84 | +1,90 | +0,37 | +3,83 | ja | – |
| aggressive | ability +10 % | -0,15 | -0,80 | +1,77 | +0,08 | -2,43 | +4,02 | nein | 53 |
| aggressive | ability +5 % | -0,01 | -0,62 | +1,49 | +0,01 | -0,60 | +4,10 | nein | 56 |
| aggressive | ability +2,5 % | +0,03 | -0,38 | +0,60 | -0,02 | -0,54 | +4,17 | nein | 54 |
| aggressive | ability -2,5 % | +0,03 | -0,28 | +0,55 | -0,02 | -0,60 | +0,20 | nein | 56 |
| aggressive | ability -5 % | +0,04 | -0,59 | +0,77 | -0,02 | -1,72 | +0,38 | nein | 56 |
| aggressive | ability -10 % | +0,38 | -1,76 | +2,23 | -0,19 | -3,61 | +1,10 | nein | 56 |
| defensive | dmg +10 % | +1,10 | -0,71 | +2,58 | -0,55 | -1,47 | +0,45 | nein | 36 |
| defensive | dmg +5 % | +0,60 | -0,80 | +2,13 | -0,30 | -1,10 | +0,82 | nein | 51 |
| defensive | dmg +2,5 % | +0,31 | -0,79 | +1,58 | -0,15 | -1,06 | +0,67 | nein | 53 |
| defensive | dmg -2,5 % | -0,76 | -2,59 | +0,52 | +0,38 | -0,47 | +1,00 | nein | 40 |
| defensive | dmg -5 % | -1,09 | -2,89 | +0,55 | +0,54 | -0,16 | +1,07 | nein | 32 |
| defensive | dmg -10 % | -1,66 | -3,51 | -0,39 | +0,83 | +0,24 | +1,61 | ja | – |
| defensive | hp +10 % | +1,52 | +0,23 | +4,62 | -0,76 | -1,62 | -0,10 | ja | – |
| defensive | hp +5 % | +0,83 | +0,08 | +2,39 | -0,42 | -0,84 | -0,02 | ja | – |
| defensive | hp +2,5 % | +0,44 | +0,02 | +1,54 | -0,22 | -0,60 | +0,05 | nein | 2 |
| defensive | hp -2,5 % | -0,40 | -1,24 | +0,05 | +0,20 | -0,07 | +0,71 | nein | 10 |
| defensive | hp -5 % | -0,79 | -2,01 | -0,05 | +0,40 | +0,02 | +0,97 | ja | – |
| defensive | hp -10 % | -1,73 | -3,80 | -0,50 | +0,87 | +0,12 | +1,99 | ja | – |
| defensive | ability +10 % | +0,52 | -1,10 | +2,20 | -0,26 | -0,84 | +0,51 | nein | 56 |
| defensive | ability +5 % | +0,13 | -1,35 | +1,31 | -0,07 | -0,54 | +0,57 | nein | 56 |
| defensive | ability +2,5 % | -0,01 | -1,25 | +1,16 | +0,01 | -0,54 | +0,76 | nein | 56 |
| defensive | ability -2,5 % | +0,08 | -1,23 | +1,31 | -0,04 | -0,67 | +0,52 | nein | 56 |
| defensive | ability -5 % | -0,27 | -2,15 | +0,79 | +0,13 | -0,52 | +0,80 | nein | 55 |
| defensive | ability -10 % | -0,65 | -3,45 | +1,70 | +0,32 | -0,28 | +0,97 | nein | 56 |

Vollständig getrennt: byactiontype 12/18, uniform 13/18, aggressive 9/18, defensive 5/18; zusammen 39 von 72.

## Teil C – Unterschiedlich starke Reaktion

Reaktion = Änderung in Richtung der Manipulation; die Spalten „Änderung“ zeigen die Änderung mit Vorzeichen (pp). Spearman = Rangkorrelation zwischen Basissiegquote und Reaktion über die 28 Kompositionen mit Schildträger; negativ heißt: schwache Kompositionen reagieren stärker.

| Agent | Variante | am stärksten | Änderung | am schwächsten | Änderung | Spearman Basis / Reaktion |
|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | hugger+schildtraeger+wand | +3,16 | berserker+schildtraeger+wand | +0,76 | -0,747 |
| byactiontype | dmg +5 % | hugger+schildtraeger+wand | +1,81 | berserker+illusionist+schildtraeger | +0,32 | -0,660 |
| byactiontype | dmg +2,5 % | musiker+schildtraeger+wand | +0,90 | bogenschuetze+illusionist+schildtraeger | +0,03 | -0,720 |
| byactiontype | dmg -2,5 % | hugger+illusionist+schildtraeger | -1,87 | berserker+elfe+schildtraeger | -0,13 | -0,023 |
| byactiontype | dmg -5 % | hugger+illusionist+schildtraeger | -2,98 | berserker+elfe+schildtraeger | -0,64 | -0,195 |
| byactiontype | dmg -10 % | hugger+illusionist+schildtraeger | -4,80 | berserker+elfe+schildtraeger | -1,05 | -0,421 |
| byactiontype | hp +10 % | hugger+schildtraeger+wand | +3,71 | bogenschuetze+elfe+schildtraeger | +0,79 | -0,155 |
| byactiontype | hp +5 % | hugger+schildtraeger+wand | +1,96 | bogenschuetze+musiker+schildtraeger | +0,44 | -0,034 |
| byactiontype | hp +2,5 % | musiker+schildtraeger+wand | +1,24 | bogenschuetze+musiker+schildtraeger | +0,10 | 0,204 |
| byactiontype | hp -2,5 % | hugger+illusionist+schildtraeger | -0,99 | elementarmagier+illusionist+schildtraeger | -0,15 | -0,124 |
| byactiontype | hp -5 % | hugger+illusionist+schildtraeger | -1,69 | elfe+musiker+schildtraeger | -0,32 | -0,275 |
| byactiontype | hp -10 % | illusionist+schildtraeger+wand | -3,20 | berserker+elfe+schildtraeger | -1,26 | -0,308 |
| byactiontype | ability +10 % | musiker+schildtraeger+wand | +0,64 | bogenschuetze+illusionist+schildtraeger | -0,13 | 0,013 |
| byactiontype | ability +5 % | berserker+elfe+schildtraeger | +0,37 | bogenschuetze+illusionist+schildtraeger | -0,13 | -0,162 |
| byactiontype | ability +2,5 % | musiker+schildtraeger+wand | +0,32 | elfe+illusionist+schildtraeger | -0,12 | -0,173 |
| byactiontype | ability -2,5 % | elementarmagier+hugger+schildtraeger | -0,23 | illusionist+musiker+schildtraeger | +0,10 | -0,198 |
| byactiontype | ability -5 % | berserker+illusionist+schildtraeger | -0,27 | illusionist+musiker+schildtraeger | +0,10 | -0,273 |
| byactiontype | ability -10 % | hugger+illusionist+schildtraeger | -0,44 | illusionist+musiker+schildtraeger | +0,05 | 0,058 |
| uniform | dmg +10 % | hugger+illusionist+schildtraeger | +3,41 | berserker+elfe+schildtraeger | +0,50 | -0,702 |
| uniform | dmg +5 % | elementarmagier+hugger+schildtraeger | +2,07 | elfe+illusionist+schildtraeger | +0,22 | -0,750 |
| uniform | dmg +2,5 % | elementarmagier+hugger+schildtraeger | +1,02 | elfe+illusionist+schildtraeger | -0,17 | -0,758 |
| uniform | dmg -2,5 % | illusionist+schildtraeger+wand | -2,09 | berserker+elfe+schildtraeger | -0,52 | -0,173 |
| uniform | dmg -5 % | illusionist+musiker+schildtraeger | -2,93 | berserker+elfe+schildtraeger | -0,82 | -0,279 |
| uniform | dmg -10 % | illusionist+musiker+schildtraeger | -4,47 | berserker+bogenschuetze+schildtraeger | -1,51 | -0,371 |
| uniform | hp +10 % | hugger+schildtraeger+wand | +3,53 | berserker+bogenschuetze+schildtraeger | +1,17 | -0,458 |
| uniform | hp +5 % | hugger+schildtraeger+wand | +2,01 | bogenschuetze+elfe+schildtraeger | +0,52 | -0,417 |
| uniform | hp +2,5 % | hugger+schildtraeger+wand | +1,07 | berserker+bogenschuetze+schildtraeger | +0,17 | -0,281 |
| uniform | hp -2,5 % | illusionist+musiker+schildtraeger | -0,79 | hugger+musiker+schildtraeger | -0,17 | -0,164 |
| uniform | hp -5 % | hugger+schildtraeger+wand | -1,87 | berserker+elementarmagier+schildtraeger | -0,45 | -0,341 |
| uniform | hp -10 % | hugger+schildtraeger+wand | -3,70 | bogenschuetze+elfe+schildtraeger | -1,20 | -0,438 |
| uniform | ability +10 % | elfe+hugger+schildtraeger | +0,82 | berserker+elementarmagier+schildtraeger | +0,03 | -0,400 |
| uniform | ability +5 % | berserker+hugger+schildtraeger | +0,50 | berserker+elementarmagier+schildtraeger | -0,08 | -0,272 |
| uniform | ability +2,5 % | berserker+illusionist+schildtraeger | +0,27 | berserker+elementarmagier+schildtraeger | -0,12 | -0,075 |
| uniform | ability -2,5 % | berserker+bogenschuetze+schildtraeger | -0,13 | elfe+musiker+schildtraeger | +0,03 | -0,075 |
| uniform | ability -5 % | elfe+illusionist+schildtraeger | -0,33 | elfe+musiker+schildtraeger | +0,02 | -0,086 |
| uniform | ability -10 % | elfe+illusionist+schildtraeger | -0,55 | bogenschuetze+musiker+schildtraeger | -0,03 | -0,041 |
| aggressive | dmg +10 % | hugger+musiker+schildtraeger | +5,19 | berserker+elfe+schildtraeger | -0,38 | -0,432 |
| aggressive | dmg +5 % | hugger+musiker+schildtraeger | +3,93 | berserker+hugger+schildtraeger | -0,35 | -0,375 |
| aggressive | dmg +2,5 % | hugger+musiker+schildtraeger | +1,69 | berserker+hugger+schildtraeger | -0,17 | -0,127 |
| aggressive | dmg -2,5 % | hugger+illusionist+schildtraeger | -2,91 | bogenschuetze+musiker+schildtraeger | -0,22 | -0,555 |
| aggressive | dmg -5 % | elfe+musiker+schildtraeger | -5,71 | bogenschuetze+elfe+schildtraeger | -0,30 | -0,738 |
| aggressive | dmg -10 % | elfe+musiker+schildtraeger | -7,11 | bogenschuetze+illusionist+schildtraeger | -0,94 | -0,754 |
| aggressive | hp +10 % | berserker+hugger+schildtraeger | +6,29 | bogenschuetze+schildtraeger+wand | +0,84 | -0,673 |
| aggressive | hp +5 % | berserker+hugger+schildtraeger | +3,38 | elfe+schildtraeger+wand | +0,25 | -0,639 |
| aggressive | hp +2,5 % | berserker+musiker+schildtraeger | +2,09 | illusionist+schildtraeger+wand | +0,35 | -0,529 |
| aggressive | hp -2,5 % | berserker+musiker+schildtraeger | -1,81 | bogenschuetze+schildtraeger+wand | -0,18 | -0,454 |
| aggressive | hp -5 % | berserker+musiker+schildtraeger | -3,63 | bogenschuetze+schildtraeger+wand | -0,42 | -0,701 |
| aggressive | hp -10 % | berserker+elementarmagier+schildtraeger | -7,04 | bogenschuetze+schildtraeger+wand | -0,84 | -0,662 |
| aggressive | ability +10 % | bogenschuetze+elfe+schildtraeger | +1,77 | berserker+hugger+schildtraeger | -0,80 | 0,328 |
| aggressive | ability +5 % | bogenschuetze+elfe+schildtraeger | +1,49 | berserker+hugger+schildtraeger | -0,62 | 0,144 |
| aggressive | ability +2,5 % | berserker+musiker+schildtraeger | +0,60 | berserker+hugger+schildtraeger | -0,38 | -0,214 |
| aggressive | ability -2,5 % | berserker+elfe+schildtraeger | -0,28 | berserker+musiker+schildtraeger | +0,55 | 0,175 |
| aggressive | ability -5 % | berserker+elfe+schildtraeger | -0,59 | bogenschuetze+elementarmagier+schildtraeger | +0,77 | -0,093 |
| aggressive | ability -10 % | elfe+musiker+schildtraeger | -1,76 | hugger+illusionist+schildtraeger | +2,23 | -0,022 |
| defensive | dmg +10 % | berserker+musiker+schildtraeger | +2,58 | bogenschuetze+hugger+schildtraeger | -0,71 | -0,055 |
| defensive | dmg +5 % | elementarmagier+elfe+schildtraeger | +2,13 | bogenschuetze+illusionist+schildtraeger | -0,80 | 0,069 |
| defensive | dmg +2,5 % | elementarmagier+elfe+schildtraeger | +1,58 | bogenschuetze+illusionist+schildtraeger | -0,79 | 0,177 |
| defensive | dmg -2,5 % | elfe+musiker+schildtraeger | -2,59 | berserker+illusionist+schildtraeger | +0,52 | -0,018 |
| defensive | dmg -5 % | elfe+musiker+schildtraeger | -2,89 | berserker+illusionist+schildtraeger | +0,55 | -0,079 |
| defensive | dmg -10 % | elfe+musiker+schildtraeger | -3,51 | berserker+illusionist+schildtraeger | -0,39 | 0,007 |
| defensive | hp +10 % | berserker+elementarmagier+schildtraeger | +4,62 | bogenschuetze+elfe+schildtraeger | +0,23 | -0,592 |
| defensive | hp +5 % | berserker+elementarmagier+schildtraeger | +2,39 | bogenschuetze+elfe+schildtraeger | +0,08 | -0,553 |
| defensive | hp +2,5 % | berserker+elementarmagier+schildtraeger | +1,54 | elfe+schildtraeger+wand | +0,02 | -0,500 |
| defensive | hp -2,5 % | berserker+elementarmagier+schildtraeger | -1,24 | bogenschuetze+schildtraeger+wand | +0,05 | -0,131 |
| defensive | hp -5 % | berserker+elementarmagier+schildtraeger | -2,01 | bogenschuetze+schildtraeger+wand | -0,05 | -0,151 |
| defensive | hp -10 % | berserker+elementarmagier+schildtraeger | -3,80 | bogenschuetze+schildtraeger+wand | -0,50 | -0,237 |
| defensive | ability +10 % | elfe+musiker+schildtraeger | +2,20 | bogenschuetze+schildtraeger+wand | -1,10 | 0,082 |
| defensive | ability +5 % | elfe+musiker+schildtraeger | +1,31 | bogenschuetze+schildtraeger+wand | -1,35 | 0,065 |
| defensive | ability +2,5 % | berserker+musiker+schildtraeger | +1,16 | bogenschuetze+schildtraeger+wand | -1,25 | -0,102 |
| defensive | ability -2,5 % | elfe+musiker+schildtraeger | -1,23 | elfe+hugger+schildtraeger | +1,31 | 0,176 |
| defensive | ability -5 % | elfe+musiker+schildtraeger | -2,15 | elfe+hugger+schildtraeger | +0,79 | 0,063 |
| defensive | ability -10 % | elfe+musiker+schildtraeger | -3,45 | bogenschuetze+schildtraeger+wand | +1,70 | 0,004 |

### Zusammenfassung: Spearman Basis / Reaktion

| Agent | Attribut | Median | negativ | Spannweite |
|---|---|---|---|---|
| byactiontype | dmg | -0,541 | 6 von 6 | -0,747 … -0,023 |
| byactiontype | hp | -0,140 | 5 von 6 | -0,308 … 0,204 |
| byactiontype | ability | -0,167 | 4 von 6 | -0,273 … 0,058 |
| uniform | dmg | -0,536 | 6 von 6 | -0,758 … -0,173 |
| uniform | hp | -0,379 | 6 von 6 | -0,458 … -0,164 |
| uniform | ability | -0,080 | 6 von 6 | -0,400 … -0,041 |
| aggressive | dmg | -0,494 | 6 von 6 | -0,754 … -0,127 |
| aggressive | hp | -0,651 | 6 von 6 | -0,701 … -0,454 |
| aggressive | ability | 0,061 | 3 von 6 | -0,214 … 0,328 |
| defensive | dmg | -0,005 | 3 von 6 | -0,079 … 0,177 |
| defensive | hp | -0,369 | 6 von 6 | -0,592 … -0,131 |
| defensive | ability | 0,064 | 1 von 6 | -0,102 … 0,176 |

Negativ in 58 von 72 Varianten.

## Teil D – Seitenvorteil, erster Schritt

### D1 Siegquote des Schildträgers nach Seite

Blüte A = Schildträger in Team A (Team A steht in allen 72 Ausgangslagen auf Blüte A), Blüte B = in Team B. Nur Paarungen mit ihm auf genau einer Seite, Unentschieden als halber Sieg. Weil Team A immer die alphabetisch frühere Komposition ist, unterscheiden sich die beiden Teilmengen auch in Mitspielern und Gegnern; der Vergleich trennt die Seite also nicht sauber von der Zusammensetzung.

| Agent | Partien auf A | Partien auf B | Basis auf A | Basis auf B | Differenz A − B |
|---|---|---|---|---|---|
| byactiontype | 37.800 | 75.096 | 53,21 % | 44,34 % | +8,88 pp |
| uniform | 37.800 | 75.096 | 57,85 % | 47,87 % | +9,99 pp |
| aggressive | 37.800 | 75.096 | 67,22 % | 54,52 % | +12,71 pp |
| defensive | 37.800 | 75.096 | 53,00 % | 40,62 % | +12,38 pp |

Änderung je Seite (Variante − Basis, pp); „in Richtung“ = Änderung × Vorzeichen der Manipulation:

| Agent | Variante | Δ auf A | Δ auf B | A − B (in Richtung) |
|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,71 | +2,83 | -0,12 |
| byactiontype | dmg +5 % | +1,35 | +1,41 | -0,06 |
| byactiontype | dmg +2,5 % | +0,57 | +0,62 | -0,05 |
| byactiontype | dmg -2,5 % | -1,27 | -1,66 | -0,39 |
| byactiontype | dmg -5 % | -2,18 | -2,52 | -0,34 |
| byactiontype | dmg -10 % | -3,72 | -4,04 | -0,32 |
| byactiontype | hp +10 % | +2,49 | +2,77 | -0,28 |
| byactiontype | hp +5 % | +1,28 | +1,48 | -0,20 |
| byactiontype | hp +2,5 % | +0,67 | +0,81 | -0,15 |
| byactiontype | hp -2,5 % | -0,58 | -0,68 | -0,11 |
| byactiontype | hp -5 % | -1,28 | -1,44 | -0,16 |
| byactiontype | hp -10 % | -2,71 | -3,06 | -0,35 |
| byactiontype | ability +10 % | +0,27 | +0,36 | -0,09 |
| byactiontype | ability +5 % | +0,18 | +0,22 | -0,04 |
| byactiontype | ability +2,5 % | +0,11 | +0,13 | -0,02 |
| byactiontype | ability -2,5 % | -0,03 | -0,05 | -0,02 |
| byactiontype | ability -5 % | -0,10 | -0,10 | -0,01 |
| byactiontype | ability -10 % | -0,22 | -0,27 | -0,06 |
| uniform | dmg +10 % | +2,65 | +3,26 | -0,61 |
| uniform | dmg +5 % | +1,42 | +1,68 | -0,26 |
| uniform | dmg +2,5 % | +0,65 | +0,76 | -0,10 |
| uniform | dmg -2,5 % | -1,48 | -1,89 | -0,41 |
| uniform | dmg -5 % | -2,29 | -2,84 | -0,55 |
| uniform | dmg -10 % | -3,68 | -4,47 | -0,79 |
| uniform | hp +10 % | +2,53 | +2,98 | -0,45 |
| uniform | hp +5 % | +1,38 | +1,60 | -0,21 |
| uniform | hp +2,5 % | +0,71 | +0,85 | -0,14 |
| uniform | hp -2,5 % | -0,62 | -0,71 | -0,09 |
| uniform | hp -5 % | -1,25 | -1,53 | -0,27 |
| uniform | hp -10 % | -2,60 | -3,19 | -0,59 |
| uniform | ability +10 % | +0,56 | +0,68 | -0,12 |
| uniform | ability +5 % | +0,33 | +0,35 | -0,02 |
| uniform | ability +2,5 % | +0,20 | +0,18 | +0,01 |
| uniform | ability -2,5 % | -0,09 | -0,07 | +0,02 |
| uniform | ability -5 % | -0,18 | -0,23 | -0,05 |
| uniform | ability -10 % | -0,38 | -0,49 | -0,11 |
| aggressive | dmg +10 % | +1,67 | +2,93 | -1,26 |
| aggressive | dmg +5 % | +0,96 | +1,66 | -0,71 |
| aggressive | dmg +2,5 % | +0,31 | +0,61 | -0,30 |
| aggressive | dmg -2,5 % | -1,53 | -2,20 | -0,67 |
| aggressive | dmg -5 % | -2,44 | -3,86 | -1,42 |
| aggressive | dmg -10 % | -3,79 | -5,87 | -2,09 |
| aggressive | hp +10 % | +5,48 | +4,80 | +0,68 |
| aggressive | hp +5 % | +2,96 | +2,51 | +0,45 |
| aggressive | hp +2,5 % | +1,86 | +1,38 | +0,48 |
| aggressive | hp -2,5 % | -1,36 | -1,33 | +0,03 |
| aggressive | hp -5 % | -2,90 | -2,66 | +0,24 |
| aggressive | hp -10 % | -6,01 | -5,44 | +0,57 |
| aggressive | ability +10 % | +0,03 | -0,36 | +0,39 |
| aggressive | ability +5 % | +0,29 | -0,18 | +0,47 |
| aggressive | ability +2,5 % | +0,24 | -0,05 | +0,29 |
| aggressive | ability -2,5 % | -0,04 | +0,09 | +0,14 |
| aggressive | ability -5 % | -0,06 | +0,13 | +0,19 |
| aggressive | ability -10 % | +0,20 | +0,75 | +0,55 |
| defensive | dmg +10 % | +1,51 | +1,69 | -0,17 |
| defensive | dmg +5 % | +0,74 | +0,97 | -0,23 |
| defensive | dmg +2,5 % | +0,22 | +0,58 | -0,35 |
| defensive | dmg -2,5 % | -0,90 | -1,23 | -0,33 |
| defensive | dmg -5 % | -1,23 | -1,80 | -0,57 |
| defensive | dmg -10 % | -2,15 | -2,63 | -0,48 |
| defensive | hp +10 % | +2,45 | +2,15 | +0,30 |
| defensive | hp +5 % | +1,29 | +1,20 | +0,09 |
| defensive | hp +2,5 % | +0,73 | +0,61 | +0,13 |
| defensive | hp -2,5 % | -0,63 | -0,57 | +0,07 |
| defensive | hp -5 % | -1,32 | -1,10 | +0,22 |
| defensive | hp -10 % | -2,80 | -2,46 | +0,34 |
| defensive | ability +10 % | +0,35 | +0,97 | -0,62 |
| defensive | ability +5 % | -0,16 | +0,37 | -0,52 |
| defensive | ability +2,5 % | -0,25 | +0,10 | -0,34 |
| defensive | ability -2,5 % | +0,02 | +0,17 | +0,15 |
| defensive | ability -5 % | -0,28 | -0,46 | -0,18 |
| defensive | ability -10 % | -0,73 | -1,08 | -0,35 |

**Zusammenfassung:** A − B in Richtung der Manipulation (positiv = auf Blüte A reagiert der Schildträger stärker). p = zweiseitiger Vorzeichentest über die Varianten (nur ein grober Hinweis, die Varianten eines Agenten teilen denselben Basislauf).

| Agent | Attribut | Mittel A − B (pp) | A stärker | p |
|---|---|---|---|---|
| byactiontype | dmg | -0,22 | 0 von 6 | 0,031 |
| byactiontype | hp | -0,21 | 0 von 6 | 0,031 |
| byactiontype | ability | -0,04 | 0 von 6 | 0,031 |
| byactiontype | alle | -0,15 | 0 von 18 | 0,000 |
| uniform | dmg | -0,45 | 0 von 6 | 0,031 |
| uniform | hp | -0,29 | 0 von 6 | 0,031 |
| uniform | ability | -0,05 | 2 von 6 | 0,688 |
| uniform | alle | -0,26 | 2 von 18 | 0,001 |
| aggressive | dmg | -1,07 | 0 von 6 | 0,031 |
| aggressive | hp | +0,41 | 6 von 6 | 0,031 |
| aggressive | ability | +0,34 | 6 von 6 | 0,031 |
| aggressive | alle | -0,11 | 12 von 18 | 0,238 |
| defensive | dmg | -0,36 | 0 von 6 | 0,031 |
| defensive | hp | +0,19 | 6 von 6 | 0,031 |
| defensive | ability | -0,31 | 1 von 6 | 0,219 |
| defensive | alle | -0,16 | 7 von 18 | 0,481 |

### D2 Code: Symmetrie des Spielfelds und Gleichstandsregel

*Historisch: Analyse vom 20.10.2026, vor der Seitenkorrektur. Die hier beschriebene Ursache ist seit dem 23.10.2026 behoben (CandidateKey, Sichtlinie und Flächenwirkungen aus Sicht der eigenen Blüte, Commit 9bf7b7f); Basislauf und vertauschter Basislauf stimmen jetzt in allen 257.040 Partien gespiegelt überein. Teil D1 oben ist mit den neuen Läufen gerechnet.*

**Spielfeld (`TacticsCore/Board/Board.cs`, Feldliste Z. 75–97).** Blüte A: (0,0) (0,1) (0,2) (−1,1) (−1,2) (1,0) (1,1); Verbindungsfelder (−1,3) (1,2); Blüte B: (0,3) (0,4) (0,5) (−1,4) (−1,5) (1,3) (1,4). Startfelder A: (0,0) (−1,1) (1,0), B: (0,5) (−1,5) (1,4). Das Feld ist auf zwei Arten symmetrisch zwischen A und B:
- Punktspiegelung (Drehung um 180°) (q, r) → (−q, 5 − r): bildet jedes Feld von A auf ein Feld von B ab, Startfeld auf Startfeld, und die beiden Verbindungsfelder aufeinander ((−1,3) ↔ (1,2)).
- Achsenspiegelung an der Mittellinie (q, r) → (q, 5 − r − q): ebenfalls A ↔ B, Startfeld ↔ Startfeld; die Verbindungsfelder bleiben fest.
Die 72 Ausgangslagen (`TacticsCore/Setup/AusgangslagenGenerator.cs`, Z. 31–55) belegen die drei Startfelder je Blüte mit allen 6 Permutationen; das Startteam wechselt. Die Aufstellungsmenge ist damit unter der Spiegelung ebenfalls symmetrisch. **Die Geometrie bevorzugt keine Seite.**

**Gleichstandsregel der heuristischen Agenten (`TacticsCore/Agents/HeuristicAgent.cs`).**
- `DecideAction` (Z. 47–88) durchläuft die Kandidaten in fester Reihenfolge und übernimmt einen Kandidaten nur bei `score > bestScore` (Z. 81). Bei gleicher Bewertung gewinnt also der **erste** Kandidat der Reihenfolge.
- Die Reihenfolge kommt aus `Candidates` (Z. 119–156, `list.OrderBy(c => c.Item2)`) mit dem Schlüssel `CandidateKey` (Z. 198–220): erst Art (0 Bewegung, 1 Angriff, 2 Fähigkeit), dann **absolute Koordinate Q aufsteigend, dann R aufsteigend** des Zielfelds bzw. der Zieleinheit, dann eigenes vor gegnerischem Team, Held vor Klon, Id, Modus.
- Die absolute Koordinate ist nicht seitenneutral. Kleines R liegt bei Blüte A (Startreihe r = 0/1), großes bei B. Bei gleicher Bewertung wählt eine Einheit von **Team B** daher bevorzugt das Feld mit kleinem R, also **in Richtung Gegner**, eine Einheit von **Team A** dagegen ebenfalls kleines R, also **zur eigenen Startreihe zurück**. Dazu kommt eine Links-rechts-Vorliebe (kleines Q). Unter der Spiegelung (q, r) → (−q, 5 − r) wird die Vorliebe „kleines Q, kleines R“ von Team B für Team A zu „großes Q, großes R“, die beiden Seiten folgen also nicht derselben Regel.
- Gleichstände sind bei diesen Agenten häufig, weil `TurnEvaluator` den Zustand nach dem Zug bewertet und viele Bewegungen dieselbe Bewertung ergeben (insbesondere ohne Kontakt zum Gegner und bei defensiven Gewichten).

Andere Sortierungen sind seitenneutral oder nicht entscheidungsrelevant: `TurnOrder.RankBySpeedDescending` (`TacticsCore/Match/TurnOrder.cs`, Z. 137–138) sortiert bei gleichem Speed nach UnitId; die Zuordnung der Ids ist durch die Etiketten-Prüfung abgedeckt (0 Abweichungen). `MatchController` (Z. 124–129) sortiert nach Startposition nur für das Protokoll. `AbilitySystem.GetLegalAbilityUses` ordnet nach Feld, aber die Agenten sortieren die Kandidaten anschließend ohnehin neu.

**Wahrscheinlichste Ursache:** die Gleichstandsauflösung über absolute Koordinaten in `HeuristicAgent.CandidateKey` in Verbindung mit `score > bestScore`. Sie wirkt bei den Zufallsagenten nicht, weil die ohne Bewertung zufällig wählen, und die Etiketten-Prüfung erfasst sie nicht, weil sie Etiketten bei gleichen Positionen tauscht, nicht die Seiten. Belegen ließe es sich nur mit Läufen, also außerhalb dieses Auftrags: etwa ein Lauf, in dem Team A auf Blüte B steht, oder ein Schlüssel relativ zur eigenen Blüte (r für Team B gespiegelt).

