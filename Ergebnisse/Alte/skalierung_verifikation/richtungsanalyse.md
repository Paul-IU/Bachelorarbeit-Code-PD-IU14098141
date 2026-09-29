# Richtungsanalyse der Rundungs-Abweichung (Schildträger/Wand)

Prüft die Vermutung: *"Die alte Rundung fiel systematisch zugunsten des
Angreifers aus, die alte Schadensreduktion war also schwächer als
beabsichtigt - die neuen (skalierten) Werte sind korrekter."*

**Ergebnis vorweg: die Vermutung ist NICHT einheitlich bestätigt.** Es gibt
einen realen, messbaren Effekt in Richtung "Reduktion wirkt nach der
Skalierung tendenziell stärker" - aber er ist kein sauberer
Angreifer/Verteidiger-Bias, sondern das Ergebnis der zufälligen
Häufigkeitsverteilung der konkreten ungeraden Werte in diesem Katalog, und
er zeigt sich nicht dort, wo die Vermutung ihn erwartet (einseitig
geschützte Seite gewinnt mehr).

## 1. Rechnerischer Nachweis auf Einheitenebene

Für jeden Grundschadenswert `X` im Katalog: `altes Ergebnis = Math.Round(X·0,5)`
(Bankers Rounding) gegen `neues Ergebnis ÷10 = X·5÷10 = X/2` (exakt, da `X·10`
immer gerade ist). Differenz = neu − alt, auf dieselbe (alte) Skala gebracht.

| Grundschadenswert | Alt (X) | X mod 4 | Altes Ergebnis | Neues Ergebnis (÷10) | Differenz | Richtung |
|---|---:|---:|---:|---:|---:|---|
| Schildträger: Grundschaden | 7 | 3 | 4 | 3,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Wand: Grundschaden | 7 | 3 | 4 | 3,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Elementarmagier: Erdschlag (Instant) | 7 | 3 | 4 | 3,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Elementarmagier: Feuerball (Instant) | 3 | 3 | 2 | 1,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Elementarmagier: Verbrennung Tick 1 | 3 | 3 | 2 | 1,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Elementarmagier: Verbrennung Tick 2 | 2 | 2 | 1 | 1,0 | 0 | gerade, keine Rundung |
| Elementarmagier: Verbrennung Tick 3 | 1 | 1 | 0 | 0,5 | **+0,5** | **alte Reduktion stärker (Verteidiger begünstigt)** |
| Musiker: Grundschaden (ohne Buff) | 9 | 1 | 4 | 4,5 | **+0,5** | **alte Reduktion stärker (Verteidiger begünstigt)** |
| Musiker: Grundschaden (mit eigenem Kampflied +6) | 15 | 3 | 8 | 7,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Bogenschütze: ohne Ladung | 12 | 0 | 6 | 6,0 | 0 | gerade |
| Bogenschütze: +1 Ladung (12+5) | 17 | 1 | 8 | 8,5 | **+0,5** | **alte Reduktion stärker (Verteidiger begünstigt)** |
| Bogenschütze: +2 Ladungen (12+10) | 22 | 2 | 11 | 11,0 | 0 | gerade |
| Bogenschütze: +3 Ladungen (12+15) | 27 | 3 | 14 | 13,5 | −0,5 | alte Reduktion schwächer (Angreifer begünstigt) |
| Bogenschütze: +4/Max (12+20) | 32 | 0 | 16 | 16,0 | 0 | gerade |
| Elfe / Berserker / Illusionist / Hugger / Elementarmagier (Grundwerte) | 8/8/6/10/10 | 0/0/2/2/2 | – | – | 0 | alle gerade, keine Rundung |

**Befund:** die Richtung hängt von `X mod 4` ab, nicht vom Rollenbegriff
"Angreifer". Werte ≡3 (mod 4) - hier 7, 3, 15, 27 - wurden vorher zugunsten
des Angreifers gerundet (stützt die Vermutung). Werte ≡1 (mod 4) - hier 1, 9,
17 - wurden vorher zugunsten des Verteidigers gerundet (widerspricht der
Vermutung). Beide Gruppen kommen im Katalog vor. Die Vermutung "systematisch
zugunsten des Angreifers" ist auf Einheitenebene **nicht** haltbar; es ist
ein gemischtes Bild.

## 2. Auswertung der Vergleichsdatei (524 Ergebniswechsel)

Die 524 Paarungen mit geändertem Sieger zerfallen in zwei sehr
unterschiedliche Gruppen, denen ein reiner Angreifer/Verteidiger-Test nicht
gerecht wird:

**a) Genau eine Seite hat Schildträger/Wand (klarer Test der Vermutung):**
nur 76 der 524 Fälle. Davon gewinnt die geschützte Seite in 37 Fällen nach
der Skalierung MEHR Partien, in 39 Fällen WENIGER. **37 zu 39 - das ist
Rauschen, keine systematische Verzerrung.** Die Vermutung wird hier NICHT
gestützt.

**b) Beide Seiten haben Schildträger/Wand:** 448 der 524 Fälle (davon 23
echte Spiegelpaarungen ohne zuordenbare Richtung, 281 mit unterschiedlichen
Wand+Wand- bzw. Schildträger+Schildträger-Kompositionen, und 144 direkte
Wand-gegen-Schildträger-Kreuzvergleiche). Bei den 144 direkten
Wand-gegen-Schildträger-Vergleichen zeigt sich ein deutliches Ungleichgewicht:
**Wand gewinnt in 99 Fällen nach der Skalierung mehr Partien, Schildträger
nur in 45.** Das ist real (99:45, nicht 50:50) - aber es ist kein
Angreifer/Verteidiger-Effekt, sondern eine Verschiebung ZWISCHEN Wand und
Schildträger: Wands Fähigkeit löst pro umgeleitetem Treffer ZWEI
rundungsabhängige Berechnungen aus (Selbstreduktion UND Vergeltungsschaden,
beide auf Basis desselben Rohschadens), Schildträgers Fähigkeit nur EINE
(reine Reduktion, keine Vergeltung). Wand ist von der Rundungsänderung also
strukturell stärker betroffen als Schildträger - das erklärt die
Verschiebung, nicht eine generelle Angreifer-Begünstigung.

**Rundenzahl-Analyse (1.468 Fälle mit gleichem Sieger, anderer Rundenzahl):**
in 1.128 Fällen (76,8 %) sind die Partien nach der Skalierung LÄNGER, in nur
340 Fällen (23,2 %) kürzer. Summe aller Rundendifferenzen: **+188,5 Runden**
netto über alle betroffenen Paarungen. **Das stützt die Vermutung** - die
Schadensreduktion wirkt im Mittel tatsächlich stärker/länger anhaltend nach
der Skalierung. Erklärung: im aktuellen Katalog kommen die "Angreifer
begünstigenden" Werte (7 - gleich dreifach als Schildträger-, Wand- und
Erdschlag-Grundschaden -, sowie 3 bei Feuerball) häufiger und ungepaart vor,
während die "Verteidiger begünstigenden" Werte (1 und 9) seltener sind bzw.
sich teils selbst aufheben (Verbrennungs-Tick 1 und Tick 3 kompensieren sich
gegenseitig je Anwendung). Der Netto-Effekt ist damit eine Eigenschaft der
KONKRETEN Zahlenverteilung dieses Katalogs, keine mathematische
Notwendigkeit der Rundungsregel selbst.

## 3. Bereinigte Siegquoten je Held (vorher/nachher, beide Agenten)

Siehe `helden_siegquoten_vergleich.csv`. Kernaussage: **die Rangfolge der
drei Tanks ist in BEIDEN Agenten unverändert** (Illusionist > Schildträger >
Wand, vorher wie nachher), alle Verschiebungen liegen unter 0,1
Prozentpunkten:

| Agent | Illusionist | Schildträger | Wand |
|---|---|---|---|
| byActionType vorher | 58,14 % | 45,59 % | 37,86 % |
| byActionType nachher | 58,10 % (−0,04) | 45,51 % (−0,08) | 37,93 % (**+0,07**) |
| Uniform vorher | 53,91 % | 47,49 % | 42,20 % |
| Uniform nachher | 53,91 % (0,00) | 47,44 % (−0,05) | 42,25 % (**+0,05**) |

Konsistent über beide Agenten: Wand gewinnt minimal hinzu, Schildträger
verliert minimal - exakt in der Größenordnung und Richtung, die der
Wand-gegen-Schildträger-Befund aus Schritt 2 erwarten lässt. Die absolute
Größenordnung (<0,1 pp) ist für die Tank-Rangfolge selbst nicht
entscheidungsrelevant.

## Fazit für die Bachelorarbeit

Die Vermutung "alte Rundung systematisch zugunsten des Angreifers, neue
Werte also korrekter" ist **nur teilweise und nicht aus dem behaupteten
Grund** belegbar:

- Auf Einheitenebene ist die Rundungsrichtung gemischt (abhängig von
  `X mod 4`), nicht einheitlich angreiferfreundlich.
- Der klare Test (eine Seite geschützt, andere nicht) zeigt KEINE
  systematische Verzerrung (37:39).
- Der tatsächlich gefundene systematische Effekt (längere Partien, Wand vs.
  Schildträger) ist eine Konsequenz der KONKRETEN Werteverteilung dieses
  Katalogs (mehr "angreiferfreundliche" Fälle als "verteidigerfreundliche")
  und eines strukturellen Unterschieds zwischen Wands Zwei-Rundungs- und
  Schildträgers Ein-Rundungs-Mechanik - nicht einer generellen Eigenschaft
  von Bankers Rounding gegenüber Schadensreduktion.
- Für die Tank-Rangfolge (Illusionist > Schildträger > Wand) ist der Effekt
  in jedem Fall zu klein (<0,1 pp, beide Agenten), um die für die
  Untersuchung getroffene Heldenauswahl zu beeinflussen.
