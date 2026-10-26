# Kompositionen

Erzeugt von `Ergebnisse/kennzahlen/kompositionen.py`. Siegquote einer Komposition ohne Spiegelpaarung, Unentschieden als halber Sieg; Basis = Basislauf Seed 2026 des Agenten; 72 Varianten (ohne dmg +20 %). Rohdaten aller Änderungen: `kompositionen_delta.csv`.

## Teil A – Kompositionen in den Basisläufen

### A1 Lage und Streuung der 84 Kompositionssiegquoten

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % | 14,66 pp | 14,17 % | 76,17 % |
| uniform | 50,00 % | 16,69 pp | 10,36 % | 76,56 % |
| aggressive | 50,00 % | 21,36 pp | 9,96 % | 90,56 % |
| defensive | 50,00 % | 21,76 pp | 10,36 % | 94,60 % |

| Agent | Rang | fünf stärkste | Siegquote | fünf schwächste | Siegquote |
|---|---|---|---|---|---|
| byactiontype | 1 | elementarmagier+elfe+illusionist | 76,17 % | hugger+musiker+schildtraeger | 14,17 % |
|  | 2 | berserker+elementarmagier+elfe | 75,69 % | hugger+musiker+wand | 19,03 % |
|  | 3 | berserker+elementarmagier+illusionist | 74,08 % | hugger+illusionist+musiker | 19,34 % |
|  | 4 | berserker+elfe+illusionist | 73,28 % | bogenschuetze+hugger+musiker | 20,52 % |
|  | 5 | elementarmagier+illusionist+wand | 71,84 % | bogenschuetze+hugger+schildtraeger | 25,07 % |
| uniform | 1 | bogenschuetze+elementarmagier+elfe | 76,56 % | hugger+illusionist+musiker | 10,36 % |
|  | 2 | berserker+elementarmagier+elfe | 76,19 % | hugger+musiker+schildtraeger | 11,30 % |
|  | 3 | elementarmagier+elfe+illusionist | 75,13 % | hugger+musiker+wand | 14,11 % |
|  | 4 | elementarmagier+elfe+wand | 73,93 % | berserker+hugger+musiker | 18,32 % |
|  | 5 | berserker+bogenschuetze+elfe | 72,54 % | bogenschuetze+hugger+musiker | 18,51 % |
| aggressive | 1 | bogenschuetze+elfe+wand | 90,56 % | elfe+hugger+musiker | 9,96 % |
|  | 2 | berserker+bogenschuetze+wand | 86,85 % | elementarmagier+elfe+hugger | 11,50 % |
|  | 3 | bogenschuetze+musiker+wand | 86,70 % | hugger+illusionist+musiker | 13,25 % |
|  | 4 | bogenschuetze+illusionist+wand | 86,58 % | berserker+elfe+hugger | 14,68 % |
|  | 5 | bogenschuetze+schildtraeger+wand | 85,96 % | elementarmagier+hugger+musiker | 15,48 % |
| defensive | 1 | bogenschuetze+elfe+wand | 94,60 % | hugger+illusionist+schildtraeger | 10,36 % |
|  | 2 | bogenschuetze+elfe+schildtraeger | 91,50 % | hugger+illusionist+musiker | 10,83 % |
|  | 3 | bogenschuetze+elfe+illusionist | 87,01 % | hugger+musiker+schildtraeger | 10,95 % |
|  | 4 | bogenschuetze+elfe+musiker | 86,92 % | hugger+illusionist+wand | 12,42 % |
|  | 5 | bogenschuetze+elfe+hugger | 86,15 % | hugger+musiker+wand | 13,95 % |

### A2 Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman |
|---|---|
| byactiontype / uniform | 0,965 |
| byactiontype / aggressive | 0,265 |
| byactiontype / defensive | 0,454 |
| uniform / aggressive | 0,396 |
| uniform / defensive | 0,563 |
| aggressive / defensive | 0,445 |

## Teil B – Trennung betroffen / nicht betroffen

Änderung = Variante − Basis in pp. Getrennt = keine Komposition ohne Schildträger ist stärker in Richtung der Manipulation verschoben als die schwächste mit Schildträger. Überlappung = Anzahl der Kompositionen ohne Schildträger (von 56), die stärker in Richtung verschoben sind als die schwächste mit Schildträger.

| Agent | Variante | mit ST: Mittel | mit ST: min | mit ST: max | ohne ST: Mittel | ohne ST: min | ohne ST: max | getrennt | Überlappung |
|---|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +1,90 | +0,74 | +3,48 | -0,95 | -1,46 | -0,45 | ja | – |
| byactiontype | dmg +5 % | +0,99 | +0,42 | +1,67 | -0,49 | -0,97 | -0,03 | ja | – |
| byactiontype | dmg +2,5 % | +0,43 | +0,13 | +0,85 | -0,22 | -0,52 | +0,05 | ja | – |
| byactiontype | dmg -2,5 % | -1,08 | -1,99 | -0,31 | +0,54 | +0,22 | +1,19 | ja | – |
| byactiontype | dmg -5 % | -1,71 | -2,86 | -0,97 | +0,85 | +0,38 | +1,51 | ja | – |
| byactiontype | dmg -10 % | -2,76 | -4,80 | -1,61 | +1,38 | +0,77 | +2,11 | ja | – |
| byactiontype | hp +10 % | +1,77 | +1,10 | +2,78 | -0,88 | -1,46 | -0,35 | ja | – |
| byactiontype | hp +5 % | +0,90 | +0,37 | +1,44 | -0,45 | -0,82 | -0,17 | ja | – |
| byactiontype | hp +2,5 % | +0,47 | +0,17 | +0,90 | -0,24 | -0,42 | -0,07 | ja | – |
| byactiontype | hp -2,5 % | -0,48 | -0,82 | -0,27 | +0,24 | +0,07 | +0,50 | ja | – |
| byactiontype | hp -5 % | -0,98 | -1,64 | -0,52 | +0,49 | +0,17 | +0,85 | ja | – |
| byactiontype | hp -10 % | -2,00 | -3,61 | -1,22 | +1,00 | +0,49 | +1,59 | ja | – |
| byactiontype | ability +10 % | +0,21 | -0,15 | +0,49 | -0,10 | -0,28 | +0,03 | nein | 38 |
| byactiontype | ability +5 % | +0,12 | -0,17 | +0,37 | -0,06 | -0,25 | +0,07 | nein | 52 |
| byactiontype | ability +2,5 % | +0,08 | -0,15 | +0,30 | -0,04 | -0,23 | +0,07 | nein | 54 |
| byactiontype | ability -2,5 % | -0,03 | -0,17 | +0,03 | +0,02 | -0,03 | +0,08 | nein | 46 |
| byactiontype | ability -5 % | -0,06 | -0,27 | +0,07 | +0,03 | -0,08 | +0,17 | nein | 45 |
| byactiontype | ability -10 % | -0,16 | -0,50 | +0,12 | +0,08 | -0,05 | +0,27 | nein | 40 |
| uniform | dmg +10 % | +2,02 | +0,85 | +3,41 | -1,01 | -1,76 | -0,62 | ja | – |
| uniform | dmg +5 % | +1,08 | +0,47 | +1,77 | -0,54 | -0,99 | -0,10 | ja | – |
| uniform | dmg +2,5 % | +0,47 | -0,13 | +0,79 | -0,23 | -0,47 | +0,03 | nein | 11 |
| uniform | dmg -2,5 % | -1,16 | -2,02 | -0,35 | +0,58 | +0,25 | +1,15 | ja | – |
| uniform | dmg -5 % | -1,81 | -3,03 | -0,87 | +0,91 | +0,44 | +1,46 | ja | – |
| uniform | dmg -10 % | -2,83 | -4,62 | -1,57 | +1,42 | +0,80 | +2,23 | ja | – |
| uniform | hp +10 % | +1,83 | +1,07 | +3,06 | -0,92 | -1,61 | -0,25 | ja | – |
| uniform | hp +5 % | +0,94 | +0,55 | +1,64 | -0,47 | -0,94 | -0,15 | ja | – |
| uniform | hp +2,5 % | +0,49 | +0,18 | +0,92 | -0,25 | -0,52 | -0,05 | ja | – |
| uniform | hp -2,5 % | -0,46 | -0,84 | -0,12 | +0,23 | +0,00 | +0,45 | ja | – |
| uniform | hp -5 % | -1,00 | -1,72 | -0,44 | +0,50 | +0,20 | +0,94 | ja | – |
| uniform | hp -10 % | -2,16 | -3,65 | -1,15 | +1,08 | +0,44 | +1,71 | ja | – |
| uniform | ability +10 % | +0,38 | +0,13 | +0,64 | -0,19 | -0,47 | -0,03 | ja | – |
| uniform | ability +5 % | +0,22 | +0,03 | +0,42 | -0,11 | -0,28 | +0,03 | ja | – |
| uniform | ability +2,5 % | +0,13 | -0,05 | +0,28 | -0,07 | -0,27 | +0,10 | nein | 19 |
| uniform | ability -2,5 % | -0,06 | -0,27 | +0,07 | +0,03 | -0,02 | +0,12 | nein | 51 |
| uniform | ability -5 % | -0,16 | -0,52 | -0,02 | +0,08 | -0,03 | +0,28 | nein | 1 |
| uniform | ability -10 % | -0,32 | -0,67 | -0,10 | +0,16 | +0,02 | +0,37 | ja | – |
| aggressive | dmg +10 % | +1,72 | -0,03 | +4,77 | -0,86 | -2,39 | +0,03 | nein | 1 |
| aggressive | dmg +5 % | +1,01 | +0,10 | +3,61 | -0,51 | -2,39 | +0,00 | ja | – |
| aggressive | dmg +2,5 % | +0,27 | -0,32 | +2,01 | -0,13 | -0,62 | +0,12 | nein | 50 |
| aggressive | dmg -2,5 % | -1,41 | -3,16 | -0,32 | +0,70 | -0,27 | +4,37 | ja | – |
| aggressive | dmg -5 % | -2,18 | -6,39 | -0,38 | +1,09 | -0,02 | +4,40 | ja | – |
| aggressive | dmg -10 % | -3,38 | -8,07 | -0,64 | +1,69 | +0,15 | +5,17 | ja | – |
| aggressive | hp +10 % | +3,52 | +1,15 | +6,31 | -1,76 | -3,48 | -0,45 | ja | – |
| aggressive | hp +5 % | +1,84 | +0,52 | +3,56 | -0,92 | -1,71 | -0,28 | ja | – |
| aggressive | hp +2,5 % | +1,08 | +0,33 | +2,07 | -0,54 | -1,22 | -0,05 | ja | – |
| aggressive | hp -2,5 % | -0,96 | -1,64 | -0,20 | +0,48 | +0,03 | +1,81 | ja | – |
| aggressive | hp -5 % | -1,83 | -3,36 | -0,42 | +0,92 | +0,23 | +1,99 | ja | – |
| aggressive | hp -10 % | -3,87 | -6,24 | -0,94 | +1,93 | +0,60 | +3,90 | ja | – |
| aggressive | ability +10 % | -0,10 | -1,09 | +0,85 | +0,05 | -1,84 | +3,50 | nein | 54 |
| aggressive | ability +5 % | +0,04 | -0,72 | +0,80 | -0,02 | -0,85 | +3,65 | nein | 55 |
| aggressive | ability +2,5 % | +0,02 | -0,40 | +1,14 | -0,01 | -0,79 | +3,70 | nein | 53 |
| aggressive | ability -2,5 % | -0,01 | -0,40 | +0,80 | +0,01 | -0,55 | +0,28 | nein | 56 |
| aggressive | ability -5 % | +0,02 | -1,00 | +1,02 | -0,01 | -1,89 | +0,52 | nein | 56 |
| aggressive | ability -10 % | +0,30 | -1,59 | +1,66 | -0,15 | -3,10 | +1,47 | nein | 56 |
| defensive | dmg +10 % | +1,12 | -1,38 | +3,08 | -0,56 | -1,29 | +0,08 | nein | 56 |
| defensive | dmg +5 % | +0,65 | -1,33 | +2,76 | -0,33 | -1,00 | +0,32 | nein | 56 |
| defensive | dmg +2,5 % | +0,35 | -1,02 | +2,16 | -0,18 | -0,69 | +0,35 | nein | 56 |
| defensive | dmg -2,5 % | -0,78 | -1,77 | +0,64 | +0,39 | -0,28 | +0,99 | nein | 46 |
| defensive | dmg -5 % | -1,16 | -2,13 | -0,08 | +0,58 | -0,03 | +1,26 | ja | – |
| defensive | dmg -10 % | -1,70 | -2,84 | -0,28 | +0,85 | +0,06 | +1,66 | ja | – |
| defensive | hp +10 % | +1,48 | +0,12 | +3,93 | -0,74 | -1,61 | -0,05 | ja | – |
| defensive | hp +5 % | +0,78 | +0,12 | +2,19 | -0,39 | -0,95 | -0,02 | ja | – |
| defensive | hp +2,5 % | +0,43 | -0,08 | +1,37 | -0,21 | -0,67 | +0,08 | nein | 9 |
| defensive | hp -2,5 % | -0,44 | -1,69 | +0,02 | +0,22 | -0,05 | +0,52 | nein | 5 |
| defensive | hp -5 % | -0,88 | -2,48 | -0,18 | +0,44 | +0,05 | +1,12 | ja | – |
| defensive | hp -10 % | -1,80 | -4,75 | -0,52 | +0,90 | +0,08 | +1,74 | ja | – |
| defensive | ability +10 % | +0,64 | -0,87 | +3,30 | -0,32 | -1,14 | +0,49 | nein | 54 |
| defensive | ability +5 % | +0,22 | -1,13 | +2,18 | -0,11 | -0,79 | +0,76 | nein | 56 |
| defensive | ability +2,5 % | +0,06 | -0,88 | +1,36 | -0,03 | -0,60 | +0,83 | nein | 56 |
| defensive | ability -2,5 % | +0,03 | -1,02 | +1,38 | -0,01 | -0,59 | +0,48 | nein | 56 |
| defensive | ability -5 % | -0,35 | -1,36 | +0,90 | +0,17 | -0,32 | +0,69 | nein | 56 |
| defensive | ability -10 % | -0,73 | -2,32 | +1,09 | +0,37 | -0,27 | +1,15 | nein | 54 |

Vollständig getrennt: byactiontype 12/18, uniform 14/18, aggressive 10/18, defensive 6/18; zusammen 42 von 72.

## Teil C – Unterschiedlich starke Reaktion

Reaktion = Änderung in Richtung der Manipulation; die Spalten „Änderung“ zeigen die Änderung mit Vorzeichen (pp). Spearman = Rangkorrelation zwischen Basissiegquote und Reaktion über die 28 Kompositionen mit Schildträger; negativ heißt: schwache Kompositionen reagieren stärker.

| Agent | Variante | am stärksten | Änderung | am schwächsten | Änderung | Spearman Basis / Reaktion |
|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | musiker+schildtraeger+wand | +3,48 | berserker+elfe+schildtraeger | +0,74 | -0,741 |
| byactiontype | dmg +5 % | musiker+schildtraeger+wand | +1,67 | illusionist+schildtraeger+wand | +0,42 | -0,712 |
| byactiontype | dmg +2,5 % | musiker+schildtraeger+wand | +0,85 | bogenschuetze+schildtraeger+wand | +0,13 | -0,722 |
| byactiontype | dmg -2,5 % | bogenschuetze+schildtraeger+wand | -1,99 | berserker+elfe+schildtraeger | -0,31 | 0,011 |
| byactiontype | dmg -5 % | hugger+illusionist+schildtraeger | -2,86 | bogenschuetze+musiker+schildtraeger | -0,97 | -0,109 |
| byactiontype | dmg -10 % | hugger+illusionist+schildtraeger | -4,80 | berserker+elfe+schildtraeger | -1,61 | -0,245 |
| byactiontype | hp +10 % | hugger+schildtraeger+wand | +2,78 | berserker+illusionist+schildtraeger | +1,10 | -0,238 |
| byactiontype | hp +5 % | hugger+illusionist+schildtraeger | +1,44 | berserker+illusionist+schildtraeger | +0,37 | -0,076 |
| byactiontype | hp +2,5 % | musiker+schildtraeger+wand | +0,90 | elementarmagier+hugger+schildtraeger | +0,17 | 0,021 |
| byactiontype | hp -2,5 % | hugger+illusionist+schildtraeger | -0,82 | berserker+elfe+schildtraeger | -0,27 | -0,437 |
| byactiontype | hp -5 % | hugger+schildtraeger+wand | -1,64 | elementarmagier+elfe+schildtraeger | -0,52 | -0,197 |
| byactiontype | hp -10 % | hugger+schildtraeger+wand | -3,61 | elfe+musiker+schildtraeger | -1,22 | -0,168 |
| byactiontype | ability +10 % | berserker+elfe+schildtraeger | +0,49 | illusionist+musiker+schildtraeger | -0,15 | 0,024 |
| byactiontype | ability +5 % | berserker+elfe+schildtraeger | +0,37 | illusionist+musiker+schildtraeger | -0,17 | -0,022 |
| byactiontype | ability +2,5 % | berserker+elfe+schildtraeger | +0,30 | illusionist+musiker+schildtraeger | -0,15 | -0,154 |
| byactiontype | ability -2,5 % | hugger+illusionist+schildtraeger | -0,17 | bogenschuetze+illusionist+schildtraeger | +0,03 | -0,297 |
| byactiontype | ability -5 % | hugger+illusionist+schildtraeger | -0,27 | berserker+elfe+schildtraeger | +0,07 | -0,262 |
| byactiontype | ability -10 % | hugger+illusionist+schildtraeger | -0,50 | berserker+elfe+schildtraeger | +0,12 | -0,118 |
| uniform | dmg +10 % | musiker+schildtraeger+wand | +3,41 | berserker+schildtraeger+wand | +0,85 | -0,547 |
| uniform | dmg +5 % | musiker+schildtraeger+wand | +1,77 | bogenschuetze+elfe+schildtraeger | +0,47 | -0,550 |
| uniform | dmg +2,5 % | hugger+illusionist+schildtraeger | +0,79 | berserker+schildtraeger+wand | -0,13 | -0,432 |
| uniform | dmg -2,5 % | illusionist+schildtraeger+wand | -2,02 | berserker+bogenschuetze+schildtraeger | -0,35 | -0,178 |
| uniform | dmg -5 % | berserker+schildtraeger+wand | -3,03 | berserker+bogenschuetze+schildtraeger | -0,87 | -0,272 |
| uniform | dmg -10 % | illusionist+musiker+schildtraeger | -4,62 | berserker+bogenschuetze+schildtraeger | -1,57 | -0,449 |
| uniform | hp +10 % | hugger+schildtraeger+wand | +3,06 | elementarmagier+elfe+schildtraeger | +1,07 | -0,380 |
| uniform | hp +5 % | hugger+schildtraeger+wand | +1,64 | berserker+schildtraeger+wand | +0,55 | -0,216 |
| uniform | hp +2,5 % | musiker+schildtraeger+wand | +0,92 | berserker+schildtraeger+wand | +0,18 | -0,267 |
| uniform | hp -2,5 % | hugger+schildtraeger+wand | -0,84 | elfe+schildtraeger+wand | -0,12 | -0,561 |
| uniform | hp -5 % | musiker+schildtraeger+wand | -1,72 | elementarmagier+elfe+schildtraeger | -0,44 | -0,545 |
| uniform | hp -10 % | musiker+schildtraeger+wand | -3,65 | elementarmagier+elfe+schildtraeger | -1,15 | -0,458 |
| uniform | ability +10 % | musiker+schildtraeger+wand | +0,64 | bogenschuetze+elfe+schildtraeger | +0,13 | -0,246 |
| uniform | ability +5 % | berserker+hugger+schildtraeger | +0,42 | bogenschuetze+schildtraeger+wand | +0,03 | -0,295 |
| uniform | ability +2,5 % | musiker+schildtraeger+wand | +0,28 | berserker+illusionist+schildtraeger | -0,05 | -0,461 |
| uniform | ability -2,5 % | hugger+schildtraeger+wand | -0,27 | elementarmagier+hugger+schildtraeger | +0,07 | -0,052 |
| uniform | ability -5 % | hugger+schildtraeger+wand | -0,52 | bogenschuetze+elementarmagier+schildtraeger | -0,02 | -0,067 |
| uniform | ability -10 % | hugger+schildtraeger+wand | -0,67 | elfe+schildtraeger+wand | -0,10 | -0,270 |
| aggressive | dmg +10 % | hugger+musiker+schildtraeger | +4,77 | berserker+elfe+schildtraeger | -0,03 | -0,508 |
| aggressive | dmg +5 % | hugger+musiker+schildtraeger | +3,61 | berserker+bogenschuetze+schildtraeger | +0,10 | -0,395 |
| aggressive | dmg +2,5 % | elementarmagier+musiker+schildtraeger | +2,01 | elementarmagier+illusionist+schildtraeger | -0,32 | -0,068 |
| aggressive | dmg -2,5 % | hugger+schildtraeger+wand | -3,16 | bogenschuetze+musiker+schildtraeger | -0,32 | -0,628 |
| aggressive | dmg -5 % | elfe+musiker+schildtraeger | -6,39 | bogenschuetze+schildtraeger+wand | -0,38 | -0,678 |
| aggressive | dmg -10 % | elfe+musiker+schildtraeger | -8,07 | berserker+bogenschuetze+schildtraeger | -0,64 | -0,717 |
| aggressive | hp +10 % | berserker+elementarmagier+schildtraeger | +6,31 | bogenschuetze+schildtraeger+wand | +1,15 | -0,672 |
| aggressive | hp +5 % | berserker+hugger+schildtraeger | +3,56 | elementarmagier+schildtraeger+wand | +0,52 | -0,605 |
| aggressive | hp +2,5 % | berserker+hugger+schildtraeger | +2,07 | elementarmagier+schildtraeger+wand | +0,33 | -0,373 |
| aggressive | hp -2,5 % | berserker+elementarmagier+schildtraeger | -1,64 | bogenschuetze+schildtraeger+wand | -0,20 | -0,433 |
| aggressive | hp -5 % | berserker+musiker+schildtraeger | -3,36 | bogenschuetze+schildtraeger+wand | -0,42 | -0,624 |
| aggressive | hp -10 % | berserker+musiker+schildtraeger | -6,24 | bogenschuetze+schildtraeger+wand | -0,94 | -0,643 |
| aggressive | ability +10 % | elementarmagier+musiker+schildtraeger | +0,85 | berserker+elfe+schildtraeger | -1,09 | 0,186 |
| aggressive | ability +5 % | berserker+musiker+schildtraeger | +0,80 | berserker+elfe+schildtraeger | -0,72 | -0,036 |
| aggressive | ability +2,5 % | berserker+musiker+schildtraeger | +1,14 | berserker+elfe+schildtraeger | -0,40 | -0,239 |
| aggressive | ability -2,5 % | elfe+illusionist+schildtraeger | -0,40 | berserker+musiker+schildtraeger | +0,80 | -0,139 |
| aggressive | ability -5 % | elfe+hugger+schildtraeger | -1,00 | berserker+musiker+schildtraeger | +1,02 | -0,379 |
| aggressive | ability -10 % | elfe+musiker+schildtraeger | -1,59 | elementarmagier+musiker+schildtraeger | +1,66 | 0,082 |
| defensive | dmg +10 % | elfe+hugger+schildtraeger | +3,08 | berserker+hugger+schildtraeger | -1,38 | -0,015 |
| defensive | dmg +5 % | elfe+hugger+schildtraeger | +2,76 | berserker+hugger+schildtraeger | -1,33 | 0,189 |
| defensive | dmg +2,5 % | elfe+hugger+schildtraeger | +2,16 | berserker+hugger+schildtraeger | -1,02 | 0,249 |
| defensive | dmg -2,5 % | bogenschuetze+hugger+schildtraeger | -1,77 | elfe+hugger+schildtraeger | +0,64 | -0,076 |
| defensive | dmg -5 % | bogenschuetze+hugger+schildtraeger | -2,13 | elfe+illusionist+schildtraeger | -0,08 | -0,159 |
| defensive | dmg -10 % | illusionist+musiker+schildtraeger | -2,84 | berserker+schildtraeger+wand | -0,28 | -0,137 |
| defensive | hp +10 % | berserker+elementarmagier+schildtraeger | +3,93 | bogenschuetze+elfe+schildtraeger | +0,12 | -0,558 |
| defensive | hp +5 % | berserker+elementarmagier+schildtraeger | +2,19 | bogenschuetze+elfe+schildtraeger | +0,12 | -0,471 |
| defensive | hp +2,5 % | berserker+elementarmagier+schildtraeger | +1,37 | hugger+schildtraeger+wand | -0,08 | -0,309 |
| defensive | hp -2,5 % | berserker+elementarmagier+schildtraeger | -1,69 | bogenschuetze+musiker+schildtraeger | +0,02 | -0,473 |
| defensive | hp -5 % | berserker+elementarmagier+schildtraeger | -2,48 | bogenschuetze+elementarmagier+schildtraeger | -0,18 | -0,434 |
| defensive | hp -10 % | berserker+elementarmagier+schildtraeger | -4,75 | bogenschuetze+schildtraeger+wand | -0,52 | -0,375 |
| defensive | ability +10 % | elfe+musiker+schildtraeger | +3,30 | bogenschuetze+schildtraeger+wand | -0,87 | 0,164 |
| defensive | ability +5 % | elfe+musiker+schildtraeger | +2,18 | bogenschuetze+schildtraeger+wand | -1,13 | 0,195 |
| defensive | ability +2,5 % | elfe+musiker+schildtraeger | +1,36 | bogenschuetze+schildtraeger+wand | -0,88 | 0,015 |
| defensive | ability -2,5 % | illusionist+musiker+schildtraeger | -1,02 | berserker+illusionist+schildtraeger | +1,38 | -0,082 |
| defensive | ability -5 % | illusionist+musiker+schildtraeger | -1,36 | elfe+hugger+schildtraeger | +0,90 | -0,112 |
| defensive | ability -10 % | elfe+musiker+schildtraeger | -2,32 | bogenschuetze+schildtraeger+wand | +1,09 | -0,038 |

### Zusammenfassung: Spearman Basis / Reaktion

| Agent | Attribut | Median | negativ | Spannweite |
|---|---|---|---|---|
| byactiontype | dmg | -0,479 | 5 von 6 | -0,741 … 0,011 |
| byactiontype | hp | -0,182 | 5 von 6 | -0,437 … 0,021 |
| byactiontype | ability | -0,136 | 5 von 6 | -0,297 … 0,024 |
| uniform | dmg | -0,440 | 6 von 6 | -0,550 … -0,178 |
| uniform | hp | -0,419 | 6 von 6 | -0,561 … -0,216 |
| uniform | ability | -0,258 | 6 von 6 | -0,461 … -0,052 |
| aggressive | dmg | -0,568 | 6 von 6 | -0,717 … -0,068 |
| aggressive | hp | -0,615 | 6 von 6 | -0,672 … -0,373 |
| aggressive | ability | -0,087 | 4 von 6 | -0,379 … 0,186 |
| defensive | dmg | -0,046 | 4 von 6 | -0,159 … 0,249 |
| defensive | hp | -0,452 | 6 von 6 | -0,558 … -0,309 |
| defensive | ability | -0,011 | 3 von 6 | -0,112 … 0,195 |

Negativ in 62 von 72 Varianten.

## Teil D – Seitenvorteil, erster Schritt

### D1 Siegquote des Schildträgers nach Seite

Blüte A = Schildträger in Team A (Team A steht in allen 72 Ausgangslagen auf Blüte A), Blüte B = in Team B. Nur Paarungen mit ihm auf genau einer Seite, Unentschieden als halber Sieg. Weil Team A immer die alphabetisch frühere Komposition ist, unterscheiden sich die beiden Teilmengen auch in Mitspielern und Gegnern; der Vergleich trennt die Seite also nicht sauber von der Zusammensetzung.

| Agent | Partien auf A | Partien auf B | Basis auf A | Basis auf B | Differenz A − B |
|---|---|---|---|---|---|
| byactiontype | 37.800 | 75.096 | 53,35 % | 44,15 % | +9,20 pp |
| uniform | 37.800 | 75.096 | 57,93 % | 47,94 % | +9,99 pp |
| aggressive | 37.800 | 75.096 | 68,60 % | 50,50 % | +18,11 pp |
| defensive | 37.800 | 75.096 | 53,44 % | 40,01 % | +13,43 pp |

Änderung je Seite (Variante − Basis, pp); „in Richtung“ = Änderung × Vorzeichen der Manipulation:

| Agent | Variante | Δ auf A | Δ auf B | A − B (in Richtung) |
|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,68 | +2,90 | -0,22 |
| byactiontype | dmg +5 % | +1,35 | +1,52 | -0,17 |
| byactiontype | dmg +2,5 % | +0,56 | +0,68 | -0,12 |
| byactiontype | dmg -2,5 % | -1,33 | -1,73 | -0,39 |
| byactiontype | dmg -5 % | -2,27 | -2,67 | -0,39 |
| byactiontype | dmg -10 % | -3,83 | -4,22 | -0,39 |
| byactiontype | hp +10 % | +2,33 | +2,76 | -0,43 |
| byactiontype | hp +5 % | +1,21 | +1,41 | -0,20 |
| byactiontype | hp +2,5 % | +0,60 | +0,75 | -0,15 |
| byactiontype | hp -2,5 % | -0,77 | -0,68 | +0,09 |
| byactiontype | hp -5 % | -1,49 | -1,45 | +0,04 |
| byactiontype | hp -10 % | -2,81 | -3,05 | -0,24 |
| byactiontype | ability +10 % | +0,29 | +0,31 | -0,02 |
| byactiontype | ability +5 % | +0,20 | +0,16 | +0,04 |
| byactiontype | ability +2,5 % | +0,14 | +0,10 | +0,04 |
| byactiontype | ability -2,5 % | -0,03 | -0,05 | -0,02 |
| byactiontype | ability -5 % | -0,08 | -0,10 | -0,02 |
| byactiontype | ability -10 % | -0,23 | -0,24 | -0,01 |
| uniform | dmg +10 % | +2,75 | +3,12 | -0,36 |
| uniform | dmg +5 % | +1,58 | +1,61 | -0,03 |
| uniform | dmg +2,5 % | +0,69 | +0,69 | +0,01 |
| uniform | dmg -2,5 % | -1,51 | -1,82 | -0,32 |
| uniform | dmg -5 % | -2,48 | -2,79 | -0,31 |
| uniform | dmg -10 % | -3,91 | -4,35 | -0,44 |
| uniform | hp +10 % | +2,31 | +2,92 | -0,61 |
| uniform | hp +5 % | +1,22 | +1,48 | -0,26 |
| uniform | hp +2,5 % | +0,63 | +0,78 | -0,15 |
| uniform | hp -2,5 % | -0,60 | -0,73 | -0,12 |
| uniform | hp -5 % | -1,32 | -1,56 | -0,24 |
| uniform | hp -10 % | -2,83 | -3,39 | -0,56 |
| uniform | ability +10 % | +0,45 | +0,62 | -0,17 |
| uniform | ability +5 % | +0,26 | +0,36 | -0,10 |
| uniform | ability +2,5 % | +0,15 | +0,22 | -0,07 |
| uniform | ability -2,5 % | -0,07 | -0,09 | -0,03 |
| uniform | ability -5 % | -0,21 | -0,25 | -0,04 |
| uniform | ability -10 % | -0,43 | -0,50 | -0,07 |
| aggressive | dmg +10 % | +1,79 | +2,93 | -1,14 |
| aggressive | dmg +5 % | +1,08 | +1,71 | -0,63 |
| aggressive | dmg +2,5 % | +0,29 | +0,45 | -0,16 |
| aggressive | dmg -2,5 % | -1,64 | -2,31 | -0,66 |
| aggressive | dmg -5 % | -2,33 | -3,68 | -1,35 |
| aggressive | dmg -10 % | -3,79 | -5,62 | -1,83 |
| aggressive | hp +10 % | +5,34 | +5,15 | +0,19 |
| aggressive | hp +5 % | +2,86 | +2,66 | +0,20 |
| aggressive | hp +2,5 % | +1,80 | +1,50 | +0,30 |
| aggressive | hp -2,5 % | -1,56 | -1,36 | +0,19 |
| aggressive | hp -5 % | -2,94 | -2,61 | +0,34 |
| aggressive | hp -10 % | -5,84 | -5,68 | +0,16 |
| aggressive | ability +10 % | +0,00 | -0,22 | +0,22 |
| aggressive | ability +5 % | +0,22 | -0,01 | +0,23 |
| aggressive | ability +2,5 % | +0,20 | -0,05 | +0,25 |
| aggressive | ability -2,5 % | -0,02 | -0,02 | +0,01 |
| aggressive | ability -5 % | +0,05 | +0,02 | -0,03 |
| aggressive | ability -10 % | +0,15 | +0,58 | +0,43 |
| defensive | dmg +10 % | +1,65 | +1,66 | -0,01 |
| defensive | dmg +5 % | +0,81 | +1,05 | -0,24 |
| defensive | dmg +2,5 % | +0,42 | +0,57 | -0,15 |
| defensive | dmg -2,5 % | -0,91 | -1,29 | -0,38 |
| defensive | dmg -5 % | -1,23 | -1,96 | -0,73 |
| defensive | dmg -10 % | -2,04 | -2,76 | -0,72 |
| defensive | hp +10 % | +2,40 | +2,09 | +0,31 |
| defensive | hp +5 % | +1,19 | +1,14 | +0,06 |
| defensive | hp +2,5 % | +0,72 | +0,59 | +0,12 |
| defensive | hp -2,5 % | -0,67 | -0,64 | +0,03 |
| defensive | hp -5 % | -1,39 | -1,25 | +0,14 |
| defensive | hp -10 % | -2,85 | -2,58 | +0,27 |
| defensive | ability +10 % | +0,47 | +1,18 | -0,71 |
| defensive | ability +5 % | -0,09 | +0,54 | -0,63 |
| defensive | ability +2,5 % | -0,23 | +0,26 | -0,49 |
| defensive | ability -2,5 % | +0,00 | +0,06 | +0,06 |
| defensive | ability -5 % | -0,35 | -0,60 | -0,26 |
| defensive | ability -10 % | -0,87 | -1,19 | -0,31 |

**Zusammenfassung:** A − B in Richtung der Manipulation (positiv = auf Blüte A reagiert der Schildträger stärker). p = zweiseitiger Vorzeichentest über die Varianten (nur ein grober Hinweis, die Varianten eines Agenten teilen denselben Basislauf).

| Agent | Attribut | Mittel A − B (pp) | A stärker | p |
|---|---|---|---|---|
| byactiontype | dmg | -0,28 | 0 von 6 | 0,031 |
| byactiontype | hp | -0,15 | 2 von 6 | 0,688 |
| byactiontype | ability | +0,00 | 2 von 6 | 0,688 |
| byactiontype | alle | -0,14 | 4 von 18 | 0,031 |
| uniform | dmg | -0,24 | 1 von 6 | 0,219 |
| uniform | hp | -0,32 | 0 von 6 | 0,031 |
| uniform | ability | -0,08 | 0 von 6 | 0,031 |
| uniform | alle | -0,22 | 1 von 18 | 0,000 |
| aggressive | dmg | -0,96 | 0 von 6 | 0,031 |
| aggressive | hp | +0,23 | 6 von 6 | 0,031 |
| aggressive | ability | +0,19 | 5 von 6 | 0,219 |
| aggressive | alle | -0,18 | 11 von 18 | 0,481 |
| defensive | dmg | -0,37 | 0 von 6 | 0,031 |
| defensive | hp | +0,15 | 6 von 6 | 0,031 |
| defensive | ability | -0,39 | 1 von 6 | 0,219 |
| defensive | alle | -0,20 | 7 von 18 | 0,481 |

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

