# Werte für die Bachelorarbeit nach der Seitenkorrektur

Erzeugt von `Ergebnisse/kennzahlen/werte_neu.py`. Je Wert **neu (alt)**: neu = Neuerhebung mit seitensymmetrischen Regeln (alle 88 Läufe), alt = Stand vor der Seitenkorrektur (Archiv). Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung, Basisläufe Seed 2026, ohne dmg +20 %. **‡ = deutliche Änderung** (Prozentwerte ≥ 1 pp, Runden ≥ 1, Rangkorrelation ≥ 0,10, Rang oder Aussage geändert).

## 6.1 Partien

| Agent | mittlere Partiedauer (Runden) | am Rundenlimit | davon a) mehr Helden | b) mehr LP-Anteil | c) unentschieden | Unentschieden gesamt |
|---|---|---|---|---|---|---|
| byactiontype | 26,92 (26,92) | 10,68 % (10,67 %) | 5,23 % (5,22 %) | 5,43 % (5,43 %) | 0,02 % (0,02 %) | 0,02 % (0,02 %) |
| uniform | 29,48 (29,47) | 15,42 % (15,41 %) | 8,00 % (8,00 %) | 7,39 % (7,39 %) | 0,02 % (0,02 %) | 0,02 % (0,02 %) |
| aggressive | 10,61 (11,34) | 0,04 % (0,64 %) | 0,03 % (0,49 %) | 0,01 % (0,15 %) | 0,00 % (0,01 %) | 0,00 % (0,01 %) |
| defensive | 26,20 (22,62) ‡ | 27,54 % (19,00 %) ‡ | 9,75 % (6,24 %) ‡ | 13,26 % (9,40 %) ‡ | 4,54 % (3,35 %) ‡ | 4,54 % (3,35 %) ‡ |

Anteile an allen 257.040 Partien; die drei Auflösungsstufen ergeben zusammen den Anteil am Rundenlimit.

## 7.1 Schildträger und Aufladen

| Agent | Siegquote Schildträger (Basis) |
|---|---|
| byactiontype | 47,23 % (47,23 %) |
| uniform | 51,29 % (51,25 %) |
| aggressive | 56,56 % (55,40 %) ‡ |
| defensive | 44,51 % (44,65 %) |

**Aufladen des Bogenschützen beim aggressiven Agenten:** 0,0053 Einsätze je Partie mit Bogenschütze (0,0042); in 0,064 % seiner Züge (0,075 %). Der Anteil der Partien mit mindestens einem Aufladen steht nicht in den Ergebnisdateien (die Heldendatei zählt nur Einsätze); er ließe sich nur durch Nachspielen ermitteln. Bei weniger als 0,01 Einsätzen je Partie liegt er höchstens bei diesem Wert, also unter 1 %.

## 7.3 Verifikation

### Etiketten-Prüfung (Basislauf Seed 2026, jede Partie zusätzlich mit vertauschten Etiketten)

| Agent | neu | alt |
|---|---|---|
| byactiontype | 257.040 geprüft, 0 Abweichungen | 257.040 geprüft, 0 Abweichungen |
| uniform | 257.040 geprüft, 0 Abweichungen | 257.040 geprüft, 0 Abweichungen |
| aggressive | 257.040 geprüft, 0 Abweichungen | 257.040 geprüft, 0 Abweichungen |
| defensive | 257.040 geprüft, 0 Abweichungen | 257.040 geprüft, 0 Abweichungen |

### Spiegelpartien: Anteil von Team A an den entschiedenen Partien (84 × 72)

| Agent | normal (Team A auf Blüte A) | vertauscht (Team A auf Blüte B) |
|---|---|---|
| byactiontype | 50,12 % (50,19 %) | 49,80 % (49,69 %) |
| uniform | 48,83 % (48,88 %) | 50,31 % (50,29 %) |
| aggressive | 52,96 % (60,37 %) ‡ | 52,96 % (38,85 %) ‡ |
| defensive | 52,10 % (57,53 %) ‡ | 52,10 % (40,70 %) ‡ |

### Basislauf und vertauschter Basislauf, Partie für Partie (heuristische Agenten)

Verglichen: Sieger, Endgrund, Auflösung, Runden, Überlebende und Rest-LP je Team (Etiketten bleiben beim Seitentausch erhalten).

| Agent | abweichende Partien neu | alt |
|---|---|---|
| aggressive | **0 von 257.040** (alle exakt gespiegelt) | 253.271 von 257.040 |
| defensive | **0 von 257.040** (alle exakt gespiegelt) | 244.270 von 257.040 |

### Siegquote des beginnenden Teams (entschiedene Partien, alle 257.040)

| Agent | beginnendes Team |
|---|---|
| byactiontype | 49,98 % (49,99 %) |
| uniform | 50,23 % (50,24 %) |
| aggressive | 50,60 % (48,04 %) ‡ |
| defensive | 50,32 % (48,13 %) ‡ |

### Streuung über die fünf Seeds 2026–2030 (SD, n − 1, pp)

| Agent | Helden: mittlere SD | Helden: größte SD | Kompositionen: mittlere SD | Kompositionen: größte SD |
|---|---|---|---|---|
| byactiontype | 0,143 (0,143) | 0,177 (0,179) | 0,567 (0,569) | 1,069 (1,078) |
| uniform | 0,105 (0,111) | 0,185 (0,184) | 0,519 (0,521) | 1,122 (1,086) |

### Zufallsagenten vor und nach der Korrektur (gleiche Seeds)

| Agent | Partien mit anderem Sieger | mit anderem Ergebnis | größte Änderung einer Heldensiegquote |
|---|---|---|---|
| byactiontype | 690 (0,27 %) | 4.385 (1,71 %) | 0,038 pp |
| uniform | 883 (0,34 %) | 4.900 (1,91 %) | 0,041 pp |

## 8.3.1 Siegquoten und Rangfolgen

### Helden (Siegquote in %, Rang)

| Held | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| bogenschuetze | 47,70 (47,69), Rang 6 | 53,39 (53,43), Rang 5 | 76,28 (54,67) ‡, Rang 1 (4) ‡ | 73,20 (62,85) ‡, Rang 2 (3) ‡ |
| elfe | 60,31 (60,29), Rang 2 | 61,78 (61,81), Rang 2 | 39,41 (44,27) ‡, Rang 8 (6) ‡ | 79,92 (71,31) ‡, Rang 1 |
| wand | 52,95 (52,96), Rang 5 | 55,22 (55,19), Rang 3 | 68,70 (63,35) ‡, Rang 2 | 50,95 (49,50) ‡, Rang 3 (4) ‡ |
| elementarmagier | 63,30 (63,31), Rang 1 | 62,65 (62,66), Rang 1 | 42,87 (42,15), Rang 6 (7) ‡ | 44,68 (47,95) ‡, Rang 5 |
| berserker | 56,87 (56,88), Rang 3 | 54,72 (54,75), Rang 4 | 49,85 (68,07) ‡, Rang 5 (1) ‡ | 47,63 (63,10) ‡, Rang 4 (2) ‡ |
| illusionist | 55,95 (55,96), Rang 4 | 52,90 (52,90), Rang 6 | 50,00 (51,19) ‡, Rang 4 (5) ‡ | 41,31 (39,42) ‡, Rang 7 (8) ‡ |
| schildtraeger | 47,23 (47,23), Rang 7 | 51,29 (51,25), Rang 7 | 56,56 (55,40) ‡, Rang 3 | 44,51 (44,65), Rang 6 |
| musiker | 37,43 (37,45), Rang 8 | 35,02 (34,99), Rang 8 | 40,63 (41,47), Rang 7 (8) ‡ | 37,80 (41,35) ‡, Rang 8 (7) ‡ |
| hugger | 28,27 (28,23), Rang 9 | 23,03 (23,04), Rang 9 | 25,69 (29,41) ‡, Rang 9 | 30,00 (29,88), Rang 9 |

### Spearman zwischen den Agenten

| Paar | Helden | Kompositionen |
|---|---|---|
| byactiontype / uniform | 0,917 (0,917) | 0,965 (0,965) |
| byactiontype / aggressive | 0,033 (0,283) ‡ | 0,265 (0,461) ‡ |
| byactiontype / defensive | 0,600 (0,617) | 0,454 (0,562) ‡ |
| uniform / aggressive | 0,167 (0,333) ‡ | 0,396 (0,510) ‡ |
| uniform / defensive | 0,767 (0,733) | 0,563 (0,625) |
| aggressive / defensive | 0,400 (0,550) ‡ | 0,445 (0,536) |

### Die 84 Kompositionen je Agent

| Agent | Mittelwert | SD | Minimum | Maximum | Partien ganz ohne Schaden |
|---|---|---|---|---|---|
| byactiontype | 50,00 % (50,00 %) | 14,66 pp (14,67 pp) | 14,17 % (14,19 %) | 76,17 % (76,10 %) | 0,00 % (0,00 %) |
| uniform | 50,00 % (50,00 %) | 16,69 pp (16,70 pp) | 10,36 % (10,34 %) | 76,56 % (76,56 %) | 0,00 % (0,00 %) |
| aggressive | 50,00 % (50,00 %) | 21,36 pp (16,60 pp) ‡ | 9,96 % (18,19 %) ‡ | 90,56 % (91,42 %) | 0,00 % (0,00 %) |
| defensive | 50,00 % (50,00 %) | 21,76 pp (17,97 pp) ‡ | 10,36 % (11,83 %) ‡ | 94,60 % (86,55 %) ‡ | 4,32 % (3,16 %) ‡ |

Partie ganz ohne Schaden: beide Teams beenden die Partie mit den vollen Lebenspunkten ihrer Komposition.

## 8.3.2 Vollständig getrennte Varianten (von 18 je Agent)

| Agent | getrennt |
|---|---|
| byactiontype | 12 (12) |
| uniform | 14 (13) |
| aggressive | 10 (10) |
| defensive | 6 (5) |

Zusammen: 42 (40) von 72.

## 8.3.3 Spearman zwischen Basissiegquote und Reaktion (28 Kompositionen mit Schildträger)

| Agent | Schaden: Median | Lebenspunkte: Median | Fähigkeit: Median | negativ (von 18) |
|---|---|---|---|---|
| byactiontype | -0,479 (-0,490) | -0,182 (-0,175) | -0,136 (-0,131) | 15 (17) |
| uniform | -0,440 (-0,449) | -0,419 (-0,431) | -0,258 (-0,259) | 18 (18) |
| aggressive | -0,568 (-0,368) ‡ | -0,615 (-0,196) ‡ | -0,087 (-0,427) ‡ | 16 (18) |
| defensive | -0,046 (-0,319) ‡ | -0,452 (-0,546) | -0,011 (-0,044) | 13 (15) |

Negativ in 62 (68) von 72 Varianten.

## 8.3.6 Bradley-Terry und Grundwertsumme

| Agent | mittlere Abweichung BT-Modell | Spearman GWS / Helden | Spearman GWS / Kompositionen | größte Abweichungen (Rang GWS → Rang Siegquote) |
|---|---|---|---|---|
| byactiontype | 0,0605 (0,0605) | -0,117 (-0,117) | 0,212 (0,213) | berserker 8→3 (+5), elfe 7→2 (+5), bogenschuetze 2→6 (-4) (berserker 8→3 (+5), elfe 7→2 (+5), bogenschuetze 2→6 (-4)) |
| uniform | 0,0577 (0,0577) | 0,150 (0,150) | 0,379 (0,382) | elfe 7→2 (+5), berserker 8→4 (+4), elementarmagier 5→1 (+4) (elfe 7→2 (+5), berserker 8→4 (+4), elementarmagier 5→1 (+4)) |
| aggressive | 0,0992 (0,1108) | 0,783 (0,367) ‡ | 0,799 (0,494) ‡ | berserker 8→5 (+3), musiker 4→7 (-3), illusionist 6→4 (+2) (berserker 8→1 (+7), musiker 4→8 (-4), bogenschuetze 2→4 (-2)) |
| defensive | 0,1022 (0,1167) | 0,317 (0,133) ‡ | 0,290 (0,164) ‡ | elfe 7→1 (+6), berserker 8→4 (+4), musiker 4→8 (-4) (berserker 8→2 (+6), elfe 7→1 (+6), musiker 4→7 (-3)) |

Mittlere Abweichung: |vorhergesagt − gemessen| über alle 3.486 Paarungen ohne Spiegel (Anteile, nicht %).

## Was sich an den Aussagen der Arbeit ändert

**Zufallsagenten: nichts.** Rund 2 % der Partien gehen im Detail anders aus (0,3 % im Sieger), keine Heldensiegquote ändert sich um mehr als 0,04 pp, Rangfolgen, Seed-Streuung, Δ Schildträger (höchstens 0,02 pp), Erkennung und Simulationsumfang bleiben praktisch gleich.

**Heuristische Agenten: deutliche Änderungen, weil der Seitenvorteil wegfällt.**

- **Seitenvorteil behoben (7.3).** Basislauf und vertauschter Basislauf stimmen in allen 257.040 Partien gespiegelt überein (vorher wichen 98,5 % bzw. 95,0 % ab); der Team-A-Anteil der Spiegelpartien ist normal und vertauscht identisch, das beginnende Team gewinnt rund 50 %. Es bleibt ein kleiner Abstand von 50 % (aggressive 52,96 %, defensive 52,10 %), der nicht von der Seite abhängt (in beiden Ausrichtungen gleich). Wahrscheinliche Ursache: Die Unit-Ids werden je Team in der absoluten Reihenfolge der Startfelder vergeben, und Gleichstände zwischen Einheiten auf demselben Feld entscheidet die Id. Nicht untersucht; für die Arbeit als Restasymmetrie zwischen den Team-Rollen zu nennen.
- **Agentenabhängigkeit (8.3.1): bleibt und wird stärker.** Zufallsagenten untereinander unverändert (Helden 0,917, Kompositionen 0,965). Zwischen Zufallsagenten und aggressive fällt Spearman der Helden auf 0,03 bzw. 0,17 (vorher 0,28 bzw. 0,33), zwischen aggressive und defensive auf 0,40 (0,55); bei den Kompositionen ähnlich. Die Rangfolgen der heuristischen Agenten ändern sich stark: aggressive Bogenschütze 76,3 % auf Rang 1 (vorher Rang 4), Wand Rang 2, Berserker fällt von Rang 1 auf Rang 5, Elfe auf Rang 8; defensive Elfe Rang 1, Bogenschütze Rang 2, Berserker von Rang 2 auf Rang 4. Der frühere Spitzenplatz des Berserkers war ein Seiteneffekt (er stand immer auf Blüte A).
- **Grundwertsumme (8.3.6):** Beim aggressiven Agenten folgt die Rangfolge jetzt weitgehend den Grundwerten (Spearman Helden 0,78 statt 0,37, Kompositionen 0,80 statt 0,49), beim defensiven etwas mehr als vorher (0,32 / 0,29 statt 0,13 / 0,16). Die Aussage „Grundwerte erklären die Stärke kaum“ gilt damit nur noch für die Zufallsagenten.
- **Spreizung der Kompositionen (8.3.1):** Bei beiden heuristischen Agenten größer als vorher (SD 21,4 / 21,8 pp statt 16,6 / 18,0), die Spannweite reicht jetzt von etwa 10 % bis 91 % bzw. 95 %.
- **Erkennung der Fähigkeit (8.3.2): ändert sich für defensive.** Erkannt jetzt bei ±10 %, +5 % und −5 % (vorher nur −5 und −10 %); die Wirkung ist größer und in beide Richtungen gerichtet (+10 %: +0,95 pp, −10 %: −1,08 pp, vorher +0,06 / −0,66). Für aggressive bleibt die Fähigkeit auf keiner Stufe erkennbar. Insgesamt 64 statt 62 von 72 Varianten erkannt.
- **Simulationsumfang (8.3.2):** Für die Zufallsagenten unverändert. Beim defensiven Agenten brauchen Schaden ±5 % und +10 % jetzt 50.000 statt 10.000 Partien, weil die Wirkung des Schadens dort kleiner ist (+10 %: +1,66 statt +2,44 pp). Aggressive Schaden +2,5 % wird auch bei 100.000 Partien nicht mehr in 95 % der Wiederholungen erkannt. Die Fähigkeit bei defensive ±10 % ist jetzt ab 50.000 bzw. 100.000 Partien erkennbar.
- **Schaden und Lebenspunkte (8.3):** Die Asymmetrie beim Schaden bleibt bei allen Agenten (Verhältnis > 1), bei aggressive bei ±2,5 % noch ausgeprägter (5,3), bei defensive schwächer (2,2 statt 3,2). Lebenspunkte bleiben nahezu linear und symmetrisch; beim aggressiven Agenten wirken sie noch stärker (±10 %: +5,21 / −5,73 pp statt +4,25 / −4,73).
- **Reaktion schwacher Kompositionen (8.3.3):** Bleibt überwiegend negativ (62 statt 68 von 72 Varianten). Beim aggressiven Agenten deutlich stärker (Median Schaden −0,57, Lebenspunkte −0,62), beim defensiven Agenten für den Schaden nicht mehr (Median −0,05).
- **Partien (6.1):** Der defensive Agent spielt länger (26,2 statt 22,6 Runden), mehr Partien enden am Rundenlimit (27,5 % statt 19,0 %) und unentschieden (4,5 % statt 3,4 %); der aggressive Agent kommt fast nie ans Limit (0,04 %).
- **Bradley-Terry (8.3.6):** Das Modell passt bei den heuristischen Agenten etwas besser (mittlere Abweichung 0,099 / 0,102 statt 0,111 / 0,117), bleibt aber deutlich schlechter als bei den Zufallsagenten (0,06).

