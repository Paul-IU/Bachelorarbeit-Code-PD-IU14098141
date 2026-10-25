# Kompositionen

Erzeugt von `Ergebnisse/kennzahlen/kompositionen.py`. Siegquote einer Komposition ohne Spiegelpaarung, Unentschieden als halber Sieg; Basis = Basislauf Seed 2026 des Agenten; 72 Varianten (ohne dmg +20 %). Rohdaten aller Änderungen: `kompositionen_delta.csv`.

## Teil A – Kompositionen in den Basisläufen

### A1 Lage und Streuung der 84 Kompositionssiegquoten

| Agent | Mittelwert | SD | kleinster Wert | größter Wert |
|---|---|---|---|---|
| byactiontype | 50,00 % | 14,67 pp | 14,19 % | 76,10 % |
| uniform | 50,00 % | 16,70 pp | 10,34 % | 76,56 % |
| aggressive | 50,00 % | 16,60 pp | 18,19 % | 91,42 % |
| defensive | 50,00 % | 17,97 pp | 11,83 % | 86,55 % |

| Agent | Rang | fünf stärkste | Siegquote | fünf schwächste | Siegquote |
|---|---|---|---|---|---|
| byactiontype | 1 | elementarmagier+elfe+illusionist | 76,10 % | hugger+musiker+schildtraeger | 14,19 % |
|  | 2 | berserker+elementarmagier+elfe | 75,64 % | hugger+musiker+wand | 19,01 % |
|  | 3 | berserker+elementarmagier+illusionist | 74,11 % | hugger+illusionist+musiker | 19,41 % |
|  | 4 | berserker+elfe+illusionist | 73,23 % | bogenschuetze+hugger+musiker | 20,48 % |
|  | 5 | elementarmagier+illusionist+wand | 71,85 % | bogenschuetze+hugger+schildtraeger | 25,03 % |
| uniform | 1 | bogenschuetze+elementarmagier+elfe | 76,56 % | hugger+illusionist+musiker | 10,34 % |
|  | 2 | berserker+elementarmagier+elfe | 76,24 % | hugger+musiker+schildtraeger | 11,30 % |
|  | 3 | elementarmagier+elfe+illusionist | 75,18 % | hugger+musiker+wand | 14,07 % |
|  | 4 | elementarmagier+elfe+wand | 73,85 % | berserker+hugger+musiker | 18,26 % |
|  | 5 | berserker+bogenschuetze+elfe | 72,59 % | bogenschuetze+hugger+musiker | 18,51 % |
| aggressive | 1 | berserker+schildtraeger+wand | 91,42 % | elfe+hugger+musiker | 18,19 % |
|  | 2 | berserker+illusionist+wand | 83,72 % | elementarmagier+hugger+musiker | 20,20 % |
|  | 3 | berserker+bogenschuetze+wand | 83,08 % | hugger+illusionist+musiker | 22,62 % |
|  | 4 | berserker+elfe+wand | 78,65 % | elementarmagier+elfe+hugger | 24,06 % |
|  | 5 | berserker+illusionist+schildtraeger | 78,45 % | hugger+musiker+schildtraeger | 24,83 % |
| defensive | 1 | berserker+bogenschuetze+elfe | 86,55 % | hugger+illusionist+musiker | 11,83 % |
|  | 2 | bogenschuetze+elfe+musiker | 85,68 % | hugger+illusionist+schildtraeger | 13,44 % |
|  | 3 | bogenschuetze+elementarmagier+elfe | 85,42 % | hugger+illusionist+wand | 14,83 % |
|  | 4 | bogenschuetze+elfe+illusionist | 81,95 % | hugger+musiker+schildtraeger | 17,57 % |
|  | 5 | bogenschuetze+elfe+wand | 79,48 % | elementarmagier+hugger+musiker | 17,60 % |

### A2 Spearman der 84 Kompositionen zwischen den Agenten

| Paar | Spearman |
|---|---|
| byactiontype / uniform | 0,965 |
| byactiontype / aggressive | 0,461 |
| byactiontype / defensive | 0,562 |
| uniform / aggressive | 0,510 |
| uniform / defensive | 0,625 |
| aggressive / defensive | 0,536 |

## Teil B – Trennung betroffen / nicht betroffen

Änderung = Variante − Basis in pp. Getrennt = keine Komposition ohne Schildträger ist stärker in Richtung der Manipulation verschoben als die schwächste mit Schildträger. Überlappung = Anzahl der Kompositionen ohne Schildträger (von 56), die stärker in Richtung verschoben sind als die schwächste mit Schildträger.

| Agent | Variante | mit ST: Mittel | mit ST: min | mit ST: max | ohne ST: Mittel | ohne ST: min | ohne ST: max | getrennt | Überlappung |
|---|---|---|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | +1,91 | +0,78 | +3,45 | -0,95 | -1,47 | -0,47 | ja | – |
| byactiontype | dmg +5 % | +0,99 | +0,40 | +1,72 | -0,49 | -0,99 | -0,03 | ja | – |
| byactiontype | dmg +2,5 % | +0,43 | +0,13 | +0,89 | -0,22 | -0,52 | +0,07 | ja | – |
| byactiontype | dmg -2,5 % | -1,07 | -1,94 | -0,29 | +0,54 | +0,22 | +1,17 | ja | – |
| byactiontype | dmg -5 % | -1,71 | -2,86 | -0,95 | +0,86 | +0,38 | +1,49 | ja | – |
| byactiontype | dmg -10 % | -2,77 | -4,80 | -1,58 | +1,39 | +0,79 | +2,09 | ja | – |
| byactiontype | hp +10 % | +1,77 | +1,10 | +2,79 | -0,88 | -1,51 | -0,37 | ja | – |
| byactiontype | hp +5 % | +0,91 | +0,38 | +1,44 | -0,45 | -0,87 | -0,17 | ja | – |
| byactiontype | hp +2,5 % | +0,48 | +0,18 | +0,94 | -0,24 | -0,42 | -0,07 | ja | – |
| byactiontype | hp -2,5 % | -0,48 | -0,82 | -0,25 | +0,24 | +0,05 | +0,45 | ja | – |
| byactiontype | hp -5 % | -0,98 | -1,62 | -0,52 | +0,49 | +0,15 | +0,84 | ja | – |
| byactiontype | hp -10 % | -2,00 | -3,51 | -1,29 | +1,00 | +0,47 | +1,54 | ja | – |
| byactiontype | ability +10 % | +0,21 | -0,10 | +0,48 | -0,10 | -0,30 | +0,03 | nein | 28 |
| byactiontype | ability +5 % | +0,12 | -0,13 | +0,35 | -0,06 | -0,27 | +0,07 | nein | 50 |
| byactiontype | ability +2,5 % | +0,07 | -0,12 | +0,28 | -0,04 | -0,25 | +0,07 | nein | 52 |
| byactiontype | ability -2,5 % | -0,03 | -0,17 | +0,05 | +0,02 | -0,03 | +0,10 | nein | 49 |
| byactiontype | ability -5 % | -0,06 | -0,27 | +0,08 | +0,03 | -0,08 | +0,17 | nein | 49 |
| byactiontype | ability -10 % | -0,16 | -0,50 | +0,12 | +0,08 | -0,05 | +0,27 | nein | 40 |
| uniform | dmg +10 % | +2,02 | +0,90 | +3,41 | -1,01 | -1,77 | -0,59 | ja | – |
| uniform | dmg +5 % | +1,08 | +0,45 | +1,76 | -0,54 | -0,97 | -0,08 | ja | – |
| uniform | dmg +2,5 % | +0,47 | -0,12 | +0,79 | -0,23 | -0,47 | +0,03 | nein | 11 |
| uniform | dmg -2,5 % | -1,16 | -2,04 | -0,38 | +0,58 | +0,25 | +1,15 | ja | – |
| uniform | dmg -5 % | -1,82 | -2,93 | -0,87 | +0,91 | +0,44 | +1,41 | ja | – |
| uniform | dmg -10 % | -2,83 | -4,27 | -1,57 | +1,42 | +0,79 | +2,14 | ja | – |
| uniform | hp +10 % | +1,84 | +1,07 | +3,13 | -0,92 | -1,61 | -0,25 | ja | – |
| uniform | hp +5 % | +0,94 | +0,54 | +1,61 | -0,47 | -0,94 | -0,15 | ja | – |
| uniform | hp +2,5 % | +0,49 | +0,15 | +0,92 | -0,25 | -0,52 | -0,05 | ja | – |
| uniform | hp -2,5 % | -0,46 | -0,77 | -0,13 | +0,23 | +0,02 | +0,45 | ja | – |
| uniform | hp -5 % | -1,00 | -1,67 | -0,40 | +0,50 | +0,20 | +0,92 | ja | – |
| uniform | hp -10 % | -2,15 | -3,53 | -1,12 | +1,07 | +0,47 | +1,69 | ja | – |
| uniform | ability +10 % | +0,38 | +0,08 | +0,62 | -0,19 | -0,47 | -0,03 | ja | – |
| uniform | ability +5 % | +0,22 | +0,02 | +0,44 | -0,11 | -0,28 | +0,03 | nein | 1 |
| uniform | ability +2,5 % | +0,13 | -0,05 | +0,28 | -0,06 | -0,27 | +0,10 | nein | 22 |
| uniform | ability -2,5 % | -0,06 | -0,27 | +0,07 | +0,03 | -0,02 | +0,12 | nein | 45 |
| uniform | ability -5 % | -0,16 | -0,52 | -0,02 | +0,08 | -0,03 | +0,28 | nein | 1 |
| uniform | ability -10 % | -0,32 | -0,64 | -0,05 | +0,16 | +0,02 | +0,37 | ja | – |
| aggressive | dmg +10 % | +1,88 | -0,02 | +8,10 | -0,94 | -2,23 | -0,25 | ja | – |
| aggressive | dmg +5 % | +0,81 | +0,03 | +1,99 | -0,41 | -1,27 | +0,20 | nein | 2 |
| aggressive | dmg +2,5 % | +0,35 | -0,35 | +1,39 | -0,17 | -0,42 | +0,02 | nein | 54 |
| aggressive | dmg -2,5 % | -1,51 | -5,96 | -0,33 | +0,75 | +0,32 | +1,66 | ja | – |
| aggressive | dmg -5 % | -2,30 | -7,60 | -0,17 | +1,15 | +0,60 | +1,91 | ja | – |
| aggressive | dmg -10 % | -3,46 | -8,20 | -0,60 | +1,73 | +0,72 | +3,63 | ja | – |
| aggressive | hp +10 % | +2,87 | +1,14 | +4,90 | -1,43 | -2,39 | -0,65 | ja | – |
| aggressive | hp +5 % | +1,38 | +0,54 | +2,54 | -0,69 | -1,34 | -0,18 | ja | – |
| aggressive | hp +2,5 % | +0,75 | +0,27 | +1,26 | -0,37 | -0,92 | +0,12 | ja | – |
| aggressive | hp -2,5 % | -0,90 | -2,01 | -0,17 | +0,45 | +0,08 | +1,04 | ja | – |
| aggressive | hp -5 % | -1,55 | -3,03 | -0,44 | +0,78 | +0,37 | +1,31 | ja | – |
| aggressive | hp -10 % | -3,19 | -6,34 | -0,97 | +1,59 | +0,99 | +2,44 | ja | – |
| aggressive | ability +10 % | -0,18 | -0,97 | +0,95 | +0,09 | -0,33 | +1,96 | nein | 56 |
| aggressive | ability +5 % | -0,05 | -0,52 | +1,20 | +0,02 | -0,23 | +0,87 | nein | 56 |
| aggressive | ability +2,5 % | +0,00 | -0,37 | +0,49 | -0,00 | -0,20 | +1,02 | nein | 56 |
| aggressive | ability -2,5 % | -0,03 | -0,18 | +0,10 | +0,02 | -0,15 | +0,18 | nein | 49 |
| aggressive | ability -5 % | -0,04 | -0,30 | +0,33 | +0,02 | -0,90 | +0,32 | nein | 56 |
| aggressive | ability -10 % | +0,04 | -0,49 | +1,00 | -0,02 | -1,71 | +0,74 | nein | 56 |
| defensive | dmg +10 % | +1,65 | -0,69 | +4,80 | -0,82 | -2,89 | +0,21 | nein | 25 |
| defensive | dmg +5 % | +0,78 | -1,09 | +2,69 | -0,39 | -1,74 | +0,43 | nein | 52 |
| defensive | dmg +2,5 % | +0,33 | -0,47 | +1,31 | -0,17 | -1,26 | +0,36 | nein | 50 |
| defensive | dmg -2,5 % | -1,07 | -3,64 | +0,17 | +0,54 | -0,22 | +1,31 | nein | 5 |
| defensive | dmg -5 % | -1,54 | -4,43 | +0,30 | +0,77 | -0,08 | +1,86 | nein | 7 |
| defensive | dmg -10 % | -2,40 | -6,03 | +0,00 | +1,20 | +0,23 | +3,00 | ja | – |
| defensive | hp +10 % | +1,38 | -0,10 | +3,03 | -0,69 | -1,36 | -0,13 | ja | – |
| defensive | hp +5 % | +0,77 | +0,00 | +1,66 | -0,39 | -0,79 | -0,07 | ja | – |
| defensive | hp +2,5 % | +0,44 | -0,03 | +0,95 | -0,22 | -0,44 | -0,02 | nein | 2 |
| defensive | hp -2,5 % | -0,32 | -0,79 | +0,08 | +0,16 | -0,02 | +0,30 | nein | 9 |
| defensive | hp -5 % | -0,75 | -1,47 | +0,02 | +0,37 | +0,03 | +0,84 | ja | – |
| defensive | hp -10 % | -1,62 | -3,13 | +0,00 | +0,81 | +0,15 | +1,27 | ja | – |
| defensive | ability +10 % | +0,04 | -1,05 | +1,08 | -0,02 | -0,95 | +0,62 | nein | 56 |
| defensive | ability +5 % | -0,14 | -1,24 | +0,64 | +0,07 | -0,85 | +0,92 | nein | 56 |
| defensive | ability +2,5 % | -0,19 | -1,12 | +0,49 | +0,09 | -0,66 | +0,94 | nein | 56 |
| defensive | ability -2,5 % | -0,09 | -0,59 | +0,33 | +0,05 | -0,49 | +0,33 | nein | 55 |
| defensive | ability -5 % | -0,18 | -0,79 | +0,41 | +0,09 | -0,74 | +0,47 | nein | 54 |
| defensive | ability -10 % | -0,45 | -1,85 | +0,57 | +0,22 | -0,61 | +0,95 | nein | 51 |

Vollständig getrennt: byactiontype 12/18, uniform 13/18, aggressive 10/18, defensive 5/18; zusammen 40 von 72.

## Teil C – Unterschiedlich starke Reaktion

Reaktion = Änderung in Richtung der Manipulation; die Spalten „Änderung“ zeigen die Änderung mit Vorzeichen (pp). Spearman = Rangkorrelation zwischen Basissiegquote und Reaktion über die 28 Kompositionen mit Schildträger; negativ heißt: schwache Kompositionen reagieren stärker.

| Agent | Variante | am stärksten | Änderung | am schwächsten | Änderung | Spearman Basis / Reaktion |
|---|---|---|---|---|---|---|
| byactiontype | dmg +10 % | musiker+schildtraeger+wand | +3,45 | berserker+elfe+schildtraeger | +0,78 | -0,733 |
| byactiontype | dmg +5 % | musiker+schildtraeger+wand | +1,72 | illusionist+schildtraeger+wand | +0,40 | -0,730 |
| byactiontype | dmg +2,5 % | musiker+schildtraeger+wand | +0,89 | bogenschuetze+illusionist+schildtraeger | +0,13 | -0,739 |
| byactiontype | dmg -2,5 % | hugger+illusionist+schildtraeger | -1,94 | berserker+elfe+schildtraeger | -0,29 | 0,026 |
| byactiontype | dmg -5 % | hugger+illusionist+schildtraeger | -2,86 | bogenschuetze+musiker+schildtraeger | -0,95 | -0,058 |
| byactiontype | dmg -10 % | hugger+illusionist+schildtraeger | -4,80 | berserker+elfe+schildtraeger | -1,58 | -0,250 |
| byactiontype | hp +10 % | musiker+schildtraeger+wand | +2,79 | berserker+illusionist+schildtraeger | +1,10 | -0,239 |
| byactiontype | hp +5 % | hugger+illusionist+schildtraeger | +1,44 | berserker+illusionist+schildtraeger | +0,38 | -0,191 |
| byactiontype | hp +2,5 % | musiker+schildtraeger+wand | +0,94 | elementarmagier+hugger+schildtraeger | +0,18 | -0,064 |
| byactiontype | hp -2,5 % | hugger+illusionist+schildtraeger | -0,82 | berserker+elfe+schildtraeger | -0,25 | -0,334 |
| byactiontype | hp -5 % | hugger+schildtraeger+wand | -1,62 | elementarmagier+elfe+schildtraeger | -0,52 | -0,159 |
| byactiontype | hp -10 % | hugger+schildtraeger+wand | -3,51 | elfe+musiker+schildtraeger | -1,29 | -0,128 |
| byactiontype | ability +10 % | berserker+elfe+schildtraeger | +0,48 | illusionist+musiker+schildtraeger | -0,10 | -0,074 |
| byactiontype | ability +5 % | berserker+elfe+schildtraeger | +0,35 | illusionist+musiker+schildtraeger | -0,13 | -0,085 |
| byactiontype | ability +2,5 % | berserker+elfe+schildtraeger | +0,28 | illusionist+musiker+schildtraeger | -0,12 | -0,198 |
| byactiontype | ability -2,5 % | hugger+illusionist+schildtraeger | -0,17 | bogenschuetze+musiker+schildtraeger | +0,05 | -0,177 |
| byactiontype | ability -5 % | hugger+illusionist+schildtraeger | -0,27 | bogenschuetze+musiker+schildtraeger | +0,08 | -0,178 |
| byactiontype | ability -10 % | hugger+illusionist+schildtraeger | -0,50 | bogenschuetze+musiker+schildtraeger | +0,12 | -0,017 |
| uniform | dmg +10 % | musiker+schildtraeger+wand | +3,41 | berserker+schildtraeger+wand | +0,90 | -0,532 |
| uniform | dmg +5 % | musiker+schildtraeger+wand | +1,76 | bogenschuetze+elfe+schildtraeger | +0,45 | -0,543 |
| uniform | dmg +2,5 % | hugger+illusionist+schildtraeger | +0,79 | berserker+schildtraeger+wand | -0,12 | -0,460 |
| uniform | dmg -2,5 % | illusionist+schildtraeger+wand | -2,04 | berserker+bogenschuetze+schildtraeger | -0,38 | -0,158 |
| uniform | dmg -5 % | berserker+schildtraeger+wand | -2,93 | berserker+bogenschuetze+schildtraeger | -0,87 | -0,257 |
| uniform | dmg -10 % | illusionist+musiker+schildtraeger | -4,27 | elementarmagier+elfe+schildtraeger | -1,57 | -0,437 |
| uniform | hp +10 % | hugger+schildtraeger+wand | +3,13 | berserker+elementarmagier+schildtraeger | +1,07 | -0,400 |
| uniform | hp +5 % | hugger+schildtraeger+wand | +1,61 | elementarmagier+elfe+schildtraeger | +0,54 | -0,241 |
| uniform | hp +2,5 % | musiker+schildtraeger+wand | +0,92 | berserker+elementarmagier+schildtraeger | +0,15 | -0,255 |
| uniform | hp -2,5 % | hugger+schildtraeger+wand | -0,77 | elfe+schildtraeger+wand | -0,13 | -0,574 |
| uniform | hp -5 % | musiker+schildtraeger+wand | -1,67 | elementarmagier+elfe+schildtraeger | -0,40 | -0,547 |
| uniform | hp -10 % | musiker+schildtraeger+wand | -3,53 | elementarmagier+elfe+schildtraeger | -1,12 | -0,462 |
| uniform | ability +10 % | musiker+schildtraeger+wand | +0,62 | bogenschuetze+elfe+schildtraeger | +0,08 | -0,220 |
| uniform | ability +5 % | berserker+hugger+schildtraeger | +0,44 | bogenschuetze+schildtraeger+wand | +0,02 | -0,299 |
| uniform | ability +2,5 % | elementarmagier+hugger+schildtraeger | +0,28 | berserker+illusionist+schildtraeger | -0,05 | -0,456 |
| uniform | ability -2,5 % | hugger+schildtraeger+wand | -0,27 | elementarmagier+hugger+schildtraeger | +0,07 | -0,071 |
| uniform | ability -5 % | hugger+schildtraeger+wand | -0,52 | bogenschuetze+elementarmagier+schildtraeger | -0,02 | -0,101 |
| uniform | ability -10 % | hugger+schildtraeger+wand | -0,64 | elfe+schildtraeger+wand | -0,05 | -0,315 |
| aggressive | dmg +10 % | elementarmagier+musiker+schildtraeger | +8,10 | berserker+elfe+schildtraeger | -0,02 | -0,439 |
| aggressive | dmg +5 % | hugger+musiker+schildtraeger | +1,99 | berserker+illusionist+schildtraeger | +0,03 | -0,376 |
| aggressive | dmg +2,5 % | elementarmagier+musiker+schildtraeger | +1,39 | berserker+elfe+schildtraeger | -0,35 | -0,517 |
| aggressive | dmg -2,5 % | elfe+musiker+schildtraeger | -5,96 | berserker+schildtraeger+wand | -0,33 | -0,300 |
| aggressive | dmg -5 % | elfe+musiker+schildtraeger | -7,60 | berserker+schildtraeger+wand | -0,17 | -0,359 |
| aggressive | dmg -10 % | elfe+musiker+schildtraeger | -8,20 | berserker+schildtraeger+wand | -0,60 | -0,254 |
| aggressive | hp +10 % | berserker+hugger+schildtraeger | +4,90 | berserker+schildtraeger+wand | +1,14 | -0,298 |
| aggressive | hp +5 % | berserker+elementarmagier+schildtraeger | +2,54 | berserker+illusionist+schildtraeger | +0,54 | -0,238 |
| aggressive | hp +2,5 % | berserker+musiker+schildtraeger | +1,26 | elementarmagier+illusionist+schildtraeger | +0,27 | -0,256 |
| aggressive | hp -2,5 % | berserker+elementarmagier+schildtraeger | -2,01 | berserker+schildtraeger+wand | -0,17 | -0,049 |
| aggressive | hp -5 % | berserker+elementarmagier+schildtraeger | -3,03 | berserker+schildtraeger+wand | -0,44 | -0,036 |
| aggressive | hp -10 % | berserker+elementarmagier+schildtraeger | -6,34 | berserker+schildtraeger+wand | -0,97 | -0,153 |
| aggressive | ability +10 % | bogenschuetze+elfe+schildtraeger | +0,95 | elfe+schildtraeger+wand | -0,97 | -0,574 |
| aggressive | ability +5 % | bogenschuetze+elfe+schildtraeger | +1,20 | bogenschuetze+schildtraeger+wand | -0,52 | -0,503 |
| aggressive | ability +2,5 % | bogenschuetze+musiker+schildtraeger | +0,49 | berserker+elfe+schildtraeger | -0,37 | -0,438 |
| aggressive | ability -2,5 % | elfe+hugger+schildtraeger | -0,18 | bogenschuetze+elementarmagier+schildtraeger | +0,10 | -0,180 |
| aggressive | ability -5 % | elfe+hugger+schildtraeger | -0,30 | berserker+elementarmagier+schildtraeger | +0,33 | -0,415 |
| aggressive | ability -10 % | elfe+musiker+schildtraeger | -0,49 | illusionist+musiker+schildtraeger | +1,00 | -0,363 |
| defensive | dmg +10 % | elementarmagier+musiker+schildtraeger | +4,80 | bogenschuetze+illusionist+schildtraeger | -0,69 | -0,383 |
| defensive | dmg +5 % | musiker+schildtraeger+wand | +2,69 | bogenschuetze+illusionist+schildtraeger | -1,09 | -0,119 |
| defensive | dmg +2,5 % | musiker+schildtraeger+wand | +1,31 | elfe+hugger+schildtraeger | -0,47 | 0,057 |
| defensive | dmg -2,5 % | elfe+musiker+schildtraeger | -3,64 | elfe+hugger+schildtraeger | +0,17 | -0,255 |
| defensive | dmg -5 % | elfe+musiker+schildtraeger | -4,43 | bogenschuetze+elfe+schildtraeger | +0,30 | -0,449 |
| defensive | dmg -10 % | musiker+schildtraeger+wand | -6,03 | bogenschuetze+elfe+schildtraeger | +0,00 | -0,492 |
| defensive | hp +10 % | hugger+musiker+schildtraeger | +3,03 | bogenschuetze+elfe+schildtraeger | -0,10 | -0,685 |
| defensive | hp +5 % | hugger+musiker+schildtraeger | +1,66 | bogenschuetze+elfe+schildtraeger | +0,00 | -0,580 |
| defensive | hp +2,5 % | hugger+musiker+schildtraeger | +0,95 | elfe+illusionist+schildtraeger | -0,03 | -0,484 |
| defensive | hp -2,5 % | elementarmagier+musiker+schildtraeger | -0,79 | berserker+illusionist+schildtraeger | +0,08 | -0,520 |
| defensive | hp -5 % | elementarmagier+musiker+schildtraeger | -1,47 | elementarmagier+elfe+schildtraeger | +0,02 | -0,552 |
| defensive | hp -10 % | berserker+musiker+schildtraeger | -3,13 | bogenschuetze+elfe+schildtraeger | +0,00 | -0,540 |
| defensive | ability +10 % | elfe+musiker+schildtraeger | +1,08 | berserker+hugger+schildtraeger | -1,05 | 0,364 |
| defensive | ability +5 % | elfe+musiker+schildtraeger | +0,64 | berserker+hugger+schildtraeger | -1,24 | 0,193 |
| defensive | ability +2,5 % | hugger+musiker+schildtraeger | +0,49 | berserker+hugger+schildtraeger | -1,12 | -0,042 |
| defensive | ability -2,5 % | hugger+musiker+schildtraeger | -0,59 | berserker+bogenschuetze+schildtraeger | +0,33 | -0,176 |
| defensive | ability -5 % | elfe+hugger+schildtraeger | -0,79 | berserker+elementarmagier+schildtraeger | +0,41 | -0,151 |
| defensive | ability -10 % | elfe+hugger+schildtraeger | -1,85 | berserker+bogenschuetze+schildtraeger | +0,57 | -0,046 |

### Zusammenfassung: Spearman Basis / Reaktion

| Agent | Attribut | Median | negativ | Spannweite |
|---|---|---|---|---|
| byactiontype | dmg | -0,490 | 5 von 6 | -0,739 … 0,026 |
| byactiontype | hp | -0,175 | 6 von 6 | -0,334 … -0,064 |
| byactiontype | ability | -0,131 | 6 von 6 | -0,198 … -0,017 |
| uniform | dmg | -0,449 | 6 von 6 | -0,543 … -0,158 |
| uniform | hp | -0,431 | 6 von 6 | -0,574 … -0,241 |
| uniform | ability | -0,259 | 6 von 6 | -0,456 … -0,071 |
| aggressive | dmg | -0,368 | 6 von 6 | -0,517 … -0,254 |
| aggressive | hp | -0,196 | 6 von 6 | -0,298 … -0,036 |
| aggressive | ability | -0,427 | 6 von 6 | -0,574 … -0,180 |
| defensive | dmg | -0,319 | 5 von 6 | -0,492 … 0,057 |
| defensive | hp | -0,546 | 6 von 6 | -0,685 … -0,484 |
| defensive | ability | -0,044 | 4 von 6 | -0,176 … 0,364 |

Negativ in 68 von 72 Varianten.

## Teil D – Seitenvorteil, erster Schritt

### D1 Siegquote des Schildträgers nach Seite

Blüte A = Schildträger in Team A (Team A steht in allen 72 Ausgangslagen auf Blüte A), Blüte B = in Team B. Nur Paarungen mit ihm auf genau einer Seite, Unentschieden als halber Sieg. Weil Team A immer die alphabetisch frühere Komposition ist, unterscheiden sich die beiden Teilmengen auch in Mitspielern und Gegnern; der Vergleich trennt die Seite also nicht sauber von der Zusammensetzung.

| Agent | Partien auf A | Partien auf B | Basis auf A | Basis auf B | Differenz A − B |
|---|---|---|---|---|---|
| byactiontype | 37.800 | 75.096 | 53,34 % | 44,15 % | +9,20 pp |
| uniform | 37.800 | 75.096 | 57,89 % | 47,90 % | +10,00 pp |
| aggressive | 37.800 | 75.096 | 75,98 % | 45,05 % | +30,93 pp |
| defensive | 37.800 | 75.096 | 64,75 % | 34,53 % | +30,22 pp |

Änderung je Seite (Variante − Basis, pp); „in Richtung“ = Änderung × Vorzeichen der Manipulation:

| Agent | Variante | Δ auf A | Δ auf B | A − B (in Richtung) |
|---|---|---|---|---|
| byactiontype | dmg +10 % | +2,70 | +2,89 | -0,19 |
| byactiontype | dmg +5 % | +1,37 | +1,51 | -0,14 |
| byactiontype | dmg +2,5 % | +0,56 | +0,68 | -0,11 |
| byactiontype | dmg -2,5 % | -1,33 | -1,72 | -0,39 |
| byactiontype | dmg -5 % | -2,28 | -2,67 | -0,39 |
| byactiontype | dmg -10 % | -3,84 | -4,24 | -0,40 |
| byactiontype | hp +10 % | +2,31 | +2,77 | -0,46 |
| byactiontype | hp +5 % | +1,21 | +1,42 | -0,20 |
| byactiontype | hp +2,5 % | +0,60 | +0,76 | -0,16 |
| byactiontype | hp -2,5 % | -0,75 | -0,69 | +0,06 |
| byactiontype | hp -5 % | -1,48 | -1,45 | +0,03 |
| byactiontype | hp -10 % | -2,82 | -3,04 | -0,23 |
| byactiontype | ability +10 % | +0,29 | +0,31 | -0,02 |
| byactiontype | ability +5 % | +0,19 | +0,16 | +0,03 |
| byactiontype | ability +2,5 % | +0,13 | +0,10 | +0,04 |
| byactiontype | ability -2,5 % | -0,03 | -0,05 | -0,02 |
| byactiontype | ability -5 % | -0,07 | -0,10 | -0,03 |
| byactiontype | ability -10 % | -0,22 | -0,24 | -0,02 |
| uniform | dmg +10 % | +2,73 | +3,13 | -0,40 |
| uniform | dmg +5 % | +1,55 | +1,62 | -0,08 |
| uniform | dmg +2,5 % | +0,68 | +0,70 | -0,02 |
| uniform | dmg -2,5 % | -1,49 | -1,83 | -0,33 |
| uniform | dmg -5 % | -2,48 | -2,80 | -0,32 |
| uniform | dmg -10 % | -3,90 | -4,35 | -0,45 |
| uniform | hp +10 % | +2,31 | +2,93 | -0,62 |
| uniform | hp +5 % | +1,22 | +1,48 | -0,26 |
| uniform | hp +2,5 % | +0,63 | +0,78 | -0,15 |
| uniform | hp -2,5 % | -0,61 | -0,73 | -0,11 |
| uniform | hp -5 % | -1,33 | -1,56 | -0,23 |
| uniform | hp -10 % | -2,81 | -3,37 | -0,57 |
| uniform | ability +10 % | +0,45 | +0,61 | -0,17 |
| uniform | ability +5 % | +0,26 | +0,36 | -0,10 |
| uniform | ability +2,5 % | +0,14 | +0,22 | -0,08 |
| uniform | ability -2,5 % | -0,08 | -0,09 | -0,01 |
| uniform | ability -5 % | -0,21 | -0,25 | -0,03 |
| uniform | ability -10 % | -0,44 | -0,50 | -0,07 |
| aggressive | dmg +10 % | +2,17 | +3,09 | -0,91 |
| aggressive | dmg +5 % | +1,17 | +1,22 | -0,05 |
| aggressive | dmg +2,5 % | +0,50 | +0,53 | -0,03 |
| aggressive | dmg -2,5 % | -1,70 | -2,50 | -0,80 |
| aggressive | dmg -5 % | -2,71 | -3,75 | -1,03 |
| aggressive | dmg -10 % | -4,50 | -5,45 | -0,96 |
| aggressive | hp +10 % | +4,26 | +4,24 | +0,02 |
| aggressive | hp +5 % | +2,06 | +2,05 | +0,01 |
| aggressive | hp +2,5 % | +1,12 | +1,09 | +0,03 |
| aggressive | hp -2,5 % | -1,41 | -1,29 | +0,12 |
| aggressive | hp -5 % | -2,50 | -2,20 | +0,30 |
| aggressive | hp -10 % | -5,08 | -4,55 | +0,53 |
| aggressive | ability +10 % | -0,37 | -0,21 | -0,16 |
| aggressive | ability +5 % | -0,13 | -0,03 | -0,10 |
| aggressive | ability +2,5 % | -0,03 | +0,03 | -0,06 |
| aggressive | ability -2,5 % | -0,10 | -0,02 | +0,07 |
| aggressive | ability -5 % | -0,11 | -0,05 | +0,06 |
| aggressive | ability -10 % | -0,15 | +0,17 | +0,33 |
| defensive | dmg +10 % | +2,82 | +2,26 | +0,57 |
| defensive | dmg +5 % | +1,60 | +0,93 | +0,67 |
| defensive | dmg +2,5 % | +0,89 | +0,29 | +0,60 |
| defensive | dmg -2,5 % | -1,64 | -1,56 | +0,08 |
| defensive | dmg -5 % | -2,34 | -2,26 | +0,08 |
| defensive | dmg -10 % | -3,65 | -3,50 | +0,15 |
| defensive | hp +10 % | +2,08 | +2,04 | +0,04 |
| defensive | hp +5 % | +1,26 | +1,09 | +0,17 |
| defensive | hp +2,5 % | +0,68 | +0,64 | +0,05 |
| defensive | hp -2,5 % | -0,53 | -0,45 | +0,08 |
| defensive | hp -5 % | -1,23 | -1,05 | +0,18 |
| defensive | hp -10 % | -2,48 | -2,35 | +0,13 |
| defensive | ability +10 % | -0,13 | +0,16 | -0,29 |
| defensive | ability +5 % | -0,31 | -0,16 | -0,15 |
| defensive | ability +2,5 % | -0,30 | -0,27 | -0,04 |
| defensive | ability -2,5 % | +0,05 | -0,23 | -0,29 |
| defensive | ability -5 % | +0,13 | -0,47 | -0,60 |
| defensive | ability -10 % | -0,37 | -0,81 | -0,44 |

**Zusammenfassung:** A − B in Richtung der Manipulation (positiv = auf Blüte A reagiert der Schildträger stärker). p = zweiseitiger Vorzeichentest über die Varianten (nur ein grober Hinweis, die Varianten eines Agenten teilen denselben Basislauf).

| Agent | Attribut | Mittel A − B (pp) | A stärker | p |
|---|---|---|---|---|
| byactiontype | dmg | -0,27 | 0 von 6 | 0,031 |
| byactiontype | hp | -0,16 | 2 von 6 | 0,688 |
| byactiontype | ability | -0,00 | 2 von 6 | 0,688 |
| byactiontype | alle | -0,14 | 4 von 18 | 0,031 |
| uniform | dmg | -0,26 | 0 von 6 | 0,031 |
| uniform | hp | -0,32 | 0 von 6 | 0,031 |
| uniform | ability | -0,08 | 0 von 6 | 0,031 |
| uniform | alle | -0,22 | 0 von 18 | 0,000 |
| aggressive | dmg | -0,63 | 0 von 6 | 0,031 |
| aggressive | hp | +0,17 | 6 von 6 | 0,031 |
| aggressive | ability | +0,02 | 3 von 6 | 1,000 |
| aggressive | alle | -0,15 | 9 von 18 | 1,000 |
| defensive | dmg | +0,36 | 6 von 6 | 0,031 |
| defensive | hp | +0,11 | 6 von 6 | 0,031 |
| defensive | ability | -0,30 | 0 von 6 | 0,031 |
| defensive | alle | +0,05 | 12 von 18 | 0,238 |

### D2 Code: Symmetrie des Spielfelds und Gleichstandsregel

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

