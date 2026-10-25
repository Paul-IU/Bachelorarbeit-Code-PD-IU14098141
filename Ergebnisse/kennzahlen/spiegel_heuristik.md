# Spiegelpaarungen der heuristischen Agenten

Erzeugt von `Ergebnisse/kennzahlen/spiegel_heuristik.py`. Bestehende Basisläufe Seed 2026, 84 Spiegelzeilen × 72 Ausgangslagen = 6.048 Partien je Agent. Keine z-Werte (deterministische Agenten). Hinweis, wenn ein Anteil mehr als 5 pp von 50 % abweicht.

| | aggressive | defensive |
|---|---|---|
| Spiegelzeilen / Partien | 84 / 6048 | 84 / 6048 |
| Unentschieden | 8 (0,13 %), in 1 Zeilen | 1230 (20,34 %), in 68 Zeilen |
| Team A an den entschiedenen | 3199 / 6040 = **52,96 %** | 2510 / 4818 = **52,10 %** |
| Team-A-Anteil je Zeile | 38,89 bis 73,61 % | 30,65 bis 71,21 % |
| Zeilen mit Team A > 50 / = 50 / < 50 % | 53 / 10 / 21 | 49 / 3 / 32 |
| Beginnendes Team, Spiegelpartien (entschiedene) | 3211 / 6040 = **53,16 %** | 2454 / 4818 = **50,93 %** |
| Beginnendes Team je Zeile | 19,44 bis 84,72 % | 14,29 bis 90,48 % |
| Beginnendes Team, alle 257.040 Partien (entschiedene, Vergleich) | 50,60 % | 50,32 % |

## Hinweise

- aggressive, extremste Spiegelzeilen (Team A): bogenschuetze+elfe+illusionist 38,9 %, bogenschuetze+elfe+hugger 40,3 %, berserker+bogenschuetze+elfe 41,7 %, berserker+schildtraeger+wand 66,7 %, illusionist+schildtraeger+wand 72,2 %, hugger+schildtraeger+wand 73,6 %
- defensive, extremste Spiegelzeilen (Team A): illusionist+musiker+schildtraeger 30,6 %, berserker+musiker+schildtraeger 36,6 %, berserker+elfe+schildtraeger 37,8 %, berserker+elfe+hugger 69,1 %, berserker+elfe+illusionist 70,6 %, berserker+hugger+musiker 71,2 %

## Wie oft steht ein Held auf Blüte A?

Team A steht in allen 72 Ausgangslagen auf Blüte A, und Team A ist in jeder Paarung die alphabetisch frühere Komposition. Anteil der einseitigen Paarungen (Held auf genau einer Seite), in denen der Held auf Blüte A steht:

| Held | auf Blüte A |
|---|---|
| berserker | 1568 / 1568 = 100,0 % |
| bogenschuetze | 1127 / 1568 = 71,9 % |
| elementarmagier | 866 / 1568 = 55,2 % |
| elfe | 715 / 1568 = 45,6 % |
| hugger | 628 / 1568 = 40,1 % |
| illusionist | 577 / 1568 = 36,8 % |
| musiker | 546 / 1568 = 34,8 % |
| schildtraeger | 525 / 1568 = 33,5 % |
| wand | 504 / 1568 = 32,1 % |

