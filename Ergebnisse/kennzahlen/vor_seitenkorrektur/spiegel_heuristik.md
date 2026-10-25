# Spiegelpaarungen der heuristischen Agenten

Erzeugt von `Ergebnisse/kennzahlen/spiegel_heuristik.py`. Bestehende Basisläufe Seed 2026, 84 Spiegelzeilen × 72 Ausgangslagen = 6.048 Partien je Agent. Keine z-Werte (deterministische Agenten). Hinweis, wenn ein Anteil mehr als 5 pp von 50 % abweicht.

| | aggressive | defensive |
|---|---|---|
| Spiegelzeilen / Partien | 84 / 6048 | 84 / 6048 |
| Unentschieden | 14 (0,23 %), in 5 Zeilen | 870 (14,38 %), in 63 Zeilen |
| Team A an den entschiedenen | 3643 / 6034 = **60,37 %** | 2979 / 5178 = **57,53 %** |
| Team-A-Anteil je Zeile | 33,33 bis 80,56 % | 29,09 bis 98,46 % |
| Zeilen mit Team A > 50 / = 50 / < 50 % | 67 / 2 / 15 | 61 / 5 / 18 |
| Beginnendes Team, Spiegelpartien (entschiedene) | 3093 / 6034 = **51,26 %** | 2464 / 5178 = **47,59 %** |
| Beginnendes Team je Zeile | 23,61 bis 83,33 % | 17,24 bis 75,00 % |
| Beginnendes Team, alle 257.040 Partien (entschiedene, Vergleich) | 48,04 % | 48,13 % |

## Hinweise

- **aggressive:** Team A an den entschiedenen Spiegelpartien 60,37 % – deutlich abweichend von 50 %.
- aggressive, extremste Spiegelzeilen (Team A): bogenschuetze+elfe+illusionist 33,3 %, bogenschuetze+elfe+hugger 34,7 %, berserker+elementarmagier+illusionist 41,7 %, elfe+illusionist+wand 76,4 %, musiker+schildtraeger+wand 77,8 %, bogenschuetze+elfe+schildtraeger 80,6 %
- **defensive:** Team A an den entschiedenen Spiegelpartien 57,53 % – deutlich abweichend von 50 %.
- defensive, extremste Spiegelzeilen (Team A): berserker+illusionist+schildtraeger 29,1 %, elementarmagier+hugger+musiker 33,3 %, bogenschuetze+hugger+illusionist 34,7 %, elementarmagier+elfe+wand 90,6 %, berserker+illusionist+wand 91,8 %, bogenschuetze+elfe+wand 98,5 %

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

