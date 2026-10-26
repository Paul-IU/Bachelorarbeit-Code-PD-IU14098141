# Werte für die Bachelorarbeit, Stand f0286a5

Erzeugt von `Ergebnisse/kennzahlen/werte_neu.py`. Je Wert **neu (alt)**: neu = Neuerhebung mit seitensymmetrischen Regeln und Id-Vergabe aus Sicht der eigenen Blüte (alle 88 Läufe, f0286a5), alt = Stand e6953bd (Archiv_vor_Idkorrektur). Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung, Basisläufe Seed 2026, ohne dmg +20 %. **‡ = deutliche Änderung** (Prozentwerte ≥ 1 pp, Runden ≥ 1, Rangkorrelation ≥ 0,10, Rang oder Aussage geändert).

## 6.1 Partien

| Agent | mittlere Partiedauer (Runden) | am Rundenlimit | davon a) mehr Helden | b) mehr LP-Anteil | c) unentschieden | Unentschieden gesamt |
|---|---|---|---|---|---|---|
| byactiontype | 26,92 (26,92) | 10,67 % (10,68 %) | 5,22 % (5,23 %) | 5,42 % (5,43 %) | 0,02 % (0,02 %) | 0,02 % (0,02 %) |
| uniform | 29,48 (29,48) | 15,45 % (15,42 %) | 8,03 % (8,00 %) | 7,39 % (7,39 %) | 0,02 % (0,02 %) | 0,02 % (0,02 %) |
| aggressive | 10,47 (10,61) | 0,03 % (0,04 %) | 0,02 % (0,03 %) | 0,01 % (0,01 %) | 0,00 % (0,00 %) | 0,00 % (0,00 %) |
| defensive | 26,16 (26,20) | 27,40 % (27,54 %) | 9,74 % (9,75 %) | 13,16 % (13,26 %) | 4,50 % (4,54 %) | 4,50 % (4,54 %) |

Anteile an allen 257.040 Partien; die drei Auflösungsstufen ergeben zusammen den Anteil am Rundenlimit.

## 7.1 Schildträger und Aufladen

| Agent | Siegquote Schildträger (Basis) |
|---|---|
| byactiontype | 47,31 % (47,23 %) |
| uniform | 51,21 % (51,29 %) |
| aggressive | 58,77 % (56,56 %) ‡ |
| defensive | 44,77 % (44,51 %) |

**Aufladen des Bogenschützen beim aggressiven Agenten:** 0,0053 Einsätze je Partie mit Bogenschütze (0,0053); in 0,063 % seiner Züge (0,064 %). Der Anteil der Partien mit mindestens einem Aufladen steht nicht in den Ergebnisdateien (die Heldendatei zählt nur Einsätze); er ließe sich nur durch Nachspielen ermitteln. Bei weniger als 0,01 Einsätzen je Partie liegt er höchstens bei diesem Wert, also unter 1 %.

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
| byactiontype | 49,90 % (50,12 %) | 50,36 % (49,80 %) |
| uniform | 49,70 % (48,83 %) | 49,90 % (50,31 %) |
| aggressive | 50,00 % (52,96 %) ‡ | 50,00 % (52,96 %) ‡ |
| defensive | 50,00 % (52,10 %) ‡ | 50,00 % (52,10 %) ‡ |

### Basislauf und vertauschter Basislauf, Partie für Partie (heuristische Agenten)

Verglichen: Sieger, Endgrund, Auflösung, Runden, Überlebende und Rest-LP je Team (Etiketten bleiben beim Seitentausch erhalten).

| Agent | abweichende Partien neu | alt |
|---|---|---|
| aggressive | **0 von 257.040** (alle exakt gespiegelt) | 0 von 257.040 |
| defensive | **0 von 257.040** (alle exakt gespiegelt) | 0 von 257.040 |

### Siegquote des beginnenden Teams (entschiedene Partien, alle 257.040)

| Agent | beginnendes Team |
|---|---|
| byactiontype | 50,02 % (49,98 %) |
| uniform | 50,11 % (50,23 %) |
| aggressive | 50,48 % (50,60 %) |
| defensive | 50,32 % (50,32 %) |

### Streuung über die fünf Seeds 2026–2030 (SD, n − 1, pp)

| Agent | Helden: mittlere SD | Helden: größte SD | Kompositionen: mittlere SD | Kompositionen: größte SD |
|---|---|---|---|---|
| byactiontype | 0,143 (0,143) | 0,286 (0,177) | 0,570 (0,567) | 1,082 (1,069) |
| uniform | 0,141 (0,105) | 0,193 (0,185) | 0,555 (0,519) | 1,011 (1,122) |

### Zufallsagenten vor und nach der Korrektur (gleiche Seeds)

| Agent | Partien mit anderem Sieger | mit anderem Ergebnis | größte Änderung einer Heldensiegquote |
|---|---|---|---|
| byactiontype | 48.178 (18,74 %) | 193.964 (75,46 %) | 0,364 pp |
| uniform | 48.381 (18,82 %) | 202.082 (78,62 %) | 0,228 pp |

## 8.3.1 Siegquoten und Rangfolgen

### Helden (Siegquote in %, Rang)

| Held | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| bogenschuetze | 47,95 (47,70), Rang 6 | 53,50 (53,39), Rang 5 | 75,34 (76,28), Rang 1 | 73,09 (73,20), Rang 2 |
| elfe | 60,42 (60,31), Rang 2 | 61,81 (61,78), Rang 2 | 38,33 (39,41) ‡, Rang 8 | 79,86 (79,92), Rang 1 |
| wand | 52,91 (52,95), Rang 5 | 55,26 (55,22), Rang 3 | 70,73 (68,70) ‡, Rang 2 | 51,65 (50,95), Rang 3 |
| elementarmagier | 63,10 (63,30), Rang 1 | 62,50 (62,65), Rang 1 | 42,03 (42,87), Rang 6 | 44,64 (44,68), Rang 6 (5) ‡ |
| berserker | 56,86 (56,87), Rang 3 | 54,59 (54,72), Rang 4 | 47,26 (49,85) ‡, Rang 5 | 47,15 (47,63), Rang 4 |
| schildtraeger | 47,31 (47,23), Rang 7 | 51,21 (51,29), Rang 7 | 58,77 (56,56) ‡, Rang 3 | 44,77 (44,51), Rang 5 (6) ‡ |
| illusionist | 56,10 (55,95), Rang 4 | 53,13 (52,90), Rang 6 | 50,48 (50,00), Rang 4 | 41,35 (41,31), Rang 7 |
| musiker | 37,45 (37,43), Rang 8 | 35,13 (35,02), Rang 8 | 41,18 (40,63), Rang 7 | 37,65 (37,80), Rang 8 |
| hugger | 27,90 (28,27), Rang 9 | 22,87 (23,03), Rang 9 | 25,88 (25,69), Rang 9 | 29,85 (30,00), Rang 9 |

### Spearman zwischen den Agenten

| Paar | Helden | Kompositionen |
|---|---|---|
| byactiontype / uniform | 0,917 (0,917) | 0,966 (0,965) |
| byactiontype / aggressive | 0,033 (0,033) | 0,255 (0,265) |
| byactiontype / defensive | 0,500 (0,600) | 0,463 (0,454) |
| uniform / aggressive | 0,167 (0,167) | 0,391 (0,396) |
| uniform / defensive | 0,667 (0,767) | 0,571 (0,563) |
| aggressive / defensive | 0,450 (0,400) | 0,411 (0,445) |

### Die 84 Kompositionen je Agent

| Agent | Mittelwert | SD | Minimum | Maximum | Partien ganz ohne Schaden |
|---|---|---|---|---|---|
| byactiontype | 50,00 % (50,00 %) | 14,74 pp (14,66 pp) | 13,91 % (14,17 %) | 76,32 % (76,17 %) | 0,00 % (0,00 %) |
| uniform | 50,00 % (50,00 %) | 16,73 pp (16,69 pp) | 10,07 % (10,36 %) | 76,32 % (76,56 %) | 0,00 % (0,00 %) |
| aggressive | 50,00 % (50,00 %) | 21,46 pp (21,36 pp) | 10,74 % (9,96 %) | 90,26 % (90,56 %) | 0,00 % (0,00 %) |
| defensive | 50,00 % (50,00 %) | 21,83 pp (21,76 pp) | 10,07 % (10,36 %) | 94,88 % (94,60 %) | 4,26 % (4,32 %) |

Partie ganz ohne Schaden: beide Teams beenden die Partie mit den vollen Lebenspunkten ihrer Komposition.

## 8.3.2 Vollständig getrennte Varianten (von 18 je Agent)

| Agent | getrennt |
|---|---|
| byactiontype | 12 (12) |
| uniform | 13 (14) |
| aggressive | 9 (10) |
| defensive | 5 (6) |

Zusammen: 39 (42) von 72.

## 8.3.3 Spearman zwischen Basissiegquote und Reaktion (28 Kompositionen mit Schildträger)

| Agent | Schaden: Median | Lebenspunkte: Median | Fähigkeit: Median | negativ (von 18) |
|---|---|---|---|---|
| byactiontype | -0,541 (-0,479) | -0,140 (-0,182) | -0,167 (-0,136) | 15 (15) |
| uniform | -0,536 (-0,440) | -0,379 (-0,419) | -0,080 (-0,258) ‡ | 18 (18) |
| aggressive | -0,494 (-0,568) | -0,651 (-0,615) | 0,061 (-0,087) ‡ | 15 (16) |
| defensive | -0,005 (-0,046) | -0,369 (-0,452) | 0,064 (-0,011) | 10 (13) |

Negativ in 58 (62) von 72 Varianten.

## 8.3.6 Bradley-Terry und Grundwertsumme

| Agent | mittlere Abweichung BT-Modell | Spearman GWS / Helden | Spearman GWS / Kompositionen | größte Abweichungen (Rang GWS → Rang Siegquote) |
|---|---|---|---|---|
| byactiontype | 0,0613 (0,0605) | -0,117 (-0,117) | 0,217 (0,212) | berserker 8→3 (+5), elfe 7→2 (+5), bogenschuetze 2→6 (-4) (berserker 8→3 (+5), elfe 7→2 (+5), bogenschuetze 2→6 (-4)) |
| uniform | 0,0580 (0,0577) | 0,150 (0,150) | 0,383 (0,379) | elfe 7→2 (+5), berserker 8→4 (+4), elementarmagier 5→1 (+4) (elfe 7→2 (+5), berserker 8→4 (+4), elementarmagier 5→1 (+4)) |
| aggressive | 0,0975 (0,0992) | 0,783 (0,783) | 0,837 (0,799) | berserker 8→5 (+3), musiker 4→7 (-3), illusionist 6→4 (+2) (berserker 8→5 (+3), musiker 4→7 (-3), illusionist 6→4 (+2)) |
| defensive | 0,1027 (0,1022) | 0,350 (0,317) | 0,302 (0,290) | elfe 7→1 (+6), berserker 8→4 (+4), musiker 4→8 (-4) (elfe 7→1 (+6), berserker 8→4 (+4), musiker 4→8 (-4)) |

Mittlere Abweichung: |vorhergesagt − gemessen| über alle 3.486 Paarungen ohne Spiegel (Anteile, nicht %).

## Was sich gegenüber dem Stand e6953bd ändert

Die Id-Korrektur ändert die Reihenfolge, in der Gleichstände zwischen Einheiten desselben Teams auf einem Feld aufgelöst werden. Weil ein früh anders aufgelöster Gleichstand den ganzen Verlauf verändert, gehen viele Partien anders aus - bei den Zufallsagenten verschiebt sich zudem die Reihenfolge ihrer Auswahllisten, die Zufallsziehungen treffen dadurch andere Züge. Die Läufe sind damit statistisch wie ein neuer Seed zu lesen.

**Zufallsagenten: inhaltlich nichts.** 75 % (byactiontype) bzw. 79 % (uniform) der Partien gehen im Detail anders aus, 19 % mit anderem Sieger. Die Heldensiegquoten ändern sich aber nur im Rahmen der Seed-Streuung: über alle fünf Seeds RMS 0,12 bzw. 0,13 pp (zwei unabhängige Seeds unterscheiden sich um etwa 0,16-0,20 pp), größte Einzeländerung 0,36 pp; die Rangfolgen sind unverändert, Spearman byactiontype/uniform 0,917 (Helden) bzw. 0,966 (Kompositionen). Δ Schildträger der 36 Varianten ändert sich um höchstens 0,21 pp (RMS 0,07), alle Vorzeichen bleiben. Erkennung und Simulationsumfang der Zufallsagenten bleiben bis auf byactiontype Fähigkeit +10/+5 % (jetzt 50.000 bzw. 100.000 statt 100.000 bzw. nie) gleich.

**Heuristische Agenten: Restasymmetrie beseitigt, Aussagen bleiben, Einzelwerte verschieben sich.**

- **Spiegelpartien (7.3):** Team A gewinnt jetzt exakt 50,00 % (vorher 52,96 % bzw. 52,10 %); Basislauf und vertauschter Basislauf stimmen weiter in allen 257.040 Partien gespiegelt überein, Etiketten-Prüfung 0 Abweichungen.
- **Rangfolgen (8.3.1):** Die Spitzen bleiben (aggressive Bogenschütze vor Wand und Schildträger, defensive Elfe vor Bogenschütze und Wand). Beim aggressiven Agenten steigt der Schildträger auf 58,77 % (56,56) und die Wand auf 70,73 % (68,70), der Berserker fällt auf 47,26 % (49,85) - wie für einen Held erwartet, der immer in Team A stand. Beim defensiven Agenten tauschen Schildträger und Elementarmagier die Plätze 5 und 6.
- **Agentenabhängigkeit (8.3.1): unverändert stark.** Zufallsagenten gegen aggressive Spearman 0,03 / 0,17 (gleich), gegen defensive 0,50 / 0,67 (vorher 0,60 / 0,77), aggressive gegen defensive 0,45 (0,40).
- **Grundwertsumme (8.3.6):** aggressive folgt weiter weitgehend den Grundwerten (Helden 0,78, Kompositionen 0,84), defensive wenig (0,35 / 0,30); Bradley-Terry-Abweichung praktisch gleich.
- **Erkennung (8.3.2): 62 statt 64 von 72.** Defensive Schaden +2,5 % wird knapp nicht mehr erkannt (18 von 27 veränderten Kompositionen in Richtung, Schwelle 19) und defensive Fähigkeit +5 % ebenfalls nicht mehr; erkannt bleibt die Fähigkeit bei defensive ±10 % (+10 % genau an der Schwelle: 19 von 28) und −5 % (19 von 28). Für aggressive bleibt die Fähigkeit auf keiner Stufe erkennbar. Die beiden Wechsel liegen an der Schwelle des Vorzeichentests und zeigen, wie empfindlich knappe Fälle auf die Gleichstandsauflösung reagieren.
- **Simulationsumfang (8.3.2):** aggressive Schaden +5 % braucht jetzt 50.000 statt 10.000 Partien, Lebenspunkte +10 % 10.000 statt 1.000; defensive Fähigkeit +10 % wird auch mit 100.000 Partien nicht mehr sicher erkannt.
- **Schaden und Lebenspunkte (8.3):** Unverändert: Schaden asymmetrisch (Verhältnis > 1 bei allen Agenten, bei aggressive ±2,5 % jetzt 3,9 statt 5,3), Lebenspunkte nahezu linear und symmetrisch (R² ≥ 0,996), beim aggressiven Agenten am stärksten (0,54 pp je %).
- **Trennung und Reaktion (8.3.2, 8.3.3):** vollständig getrennt 39 statt 42 von 72; Spearman Basis/Reaktion negativ in 58 statt 62 von 72 Varianten - die Aussage „schwache Kompositionen reagieren stärker“ gilt weiter für Schaden und Lebenspunkte, für die Fähigkeit nicht mehr eindeutig (Median nahe 0 bei uniform, aggressive, defensive).

