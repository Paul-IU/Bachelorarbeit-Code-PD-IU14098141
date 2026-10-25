# Trefferschwellen des Schildträgers

Erzeugt von `Ergebnisse/kennzahlen/trefferschwellen.py`. Treffer = ⌈Lebenspunkte / Schaden⌉ aus vollen Lebenspunkten, ohne Modifikatoren. Schadenswerte gerundet wie im Katalog (70 × 0,95 = 66,5 → 66; 70 × 1,05 = 73,5 → 74).

## 1 Treffer je Held und Schadenswert

| Held | LP | 63 (−10 %) | 66 (−5 %) | 68 (−2,5 %) | 70 (Basis) | 72 (+2,5 %) | 74 (+5 %) | 77 (+10 %) |
|---|---|---|---|---|---|---|---|---|
| wand | 1000 | **16** | **16** | 15 | 15 | **14** | **14** | **13** |
| schildtraeger | 850 | **14** | 13 | 13 | 13 | **12** | **12** | **12** |
| illusionist | 750 | **12** | **12** | **12** | 11 | 11 | 11 | **10** |
| hugger | 500 | 8 | 8 | 8 | 8 | **7** | **7** | **7** |
| musiker | 480 | **8** | **8** | **8** | 7 | 7 | 7 | 7 |
| elfe | 450 | **8** | 7 | 7 | 7 | 7 | 7 | **6** |
| berserker | 400 | **7** | **7** | 6 | 6 | 6 | 6 | 6 |
| elementarmagier | 320 | **6** | 5 | 5 | 5 | 5 | 5 | 5 |
| bogenschuetze | 280 | **5** | **5** | **5** | 4 | 4 | 4 | 4 |

Fett = andere Trefferzahl als bei 70.

## 2 Änderungen gegenüber 70

| Schaden | Stufe | Helden mit anderer Trefferzahl |
|---|---|---|
| 63 | −10 % | wand (15 → 16), schildtraeger (13 → 14), illusionist (11 → 12), musiker (7 → 8), elfe (7 → 8), berserker (6 → 7), elementarmagier (5 → 6), bogenschuetze (4 → 5) |
| 66 | −5 % | wand (15 → 16), illusionist (11 → 12), musiker (7 → 8), berserker (6 → 7), bogenschuetze (4 → 5) |
| 68 | −2,5 % | illusionist (11 → 12), musiker (7 → 8), bogenschuetze (4 → 5) |
| 72 | +2,5 % | wand (15 → 14), schildtraeger (13 → 12), hugger (8 → 7) |
| 74 | +5 % | wand (15 → 14), schildtraeger (13 → 12), hugger (8 → 7) |
| 77 | +10 % | wand (15 → 13), schildtraeger (13 → 12), illusionist (11 → 10), hugger (8 → 7), elfe (7 → 6) |

## 3 Gewichtet nach Gegnerbegegnungen in den Basisläufen

Die Gewichte sind bei allen vier Agenten identisch (jede Paarung hat 72 Partien), deshalb gilt die Tabelle für alle Agenten.

| Held | Anteil an den Gegnerbegegnungen (einseitig) | Anteil (alle Paarungen mit Schildträger) |
|---|---|---|
| wand | 12,50 % | 11,14 % |
| schildtraeger | 0,00 % | 10,84 % |
| illusionist | 12,50 % | 11,14 % |
| hugger | 12,50 % | 11,14 % |
| musiker | 12,50 % | 11,14 % |
| elfe | 12,50 % | 11,14 % |
| berserker | 12,50 % | 11,14 % |
| elementarmagier | 12,50 % | 11,14 % |
| bogenschuetze | 12,50 % | 11,14 % |

Anteil der Gegnerbegegnungen, bei denen sich die Trefferzahl gegenüber 70 ändert:

| Stufe | Abschwächung (einseitig) | Verstärkung (einseitig) | Abschwächung (alle) | Verstärkung (alle) |
|---|---|---|---|---|
| ±2,5 % | 37,5 % | 25,0 % | 33,4 % | 33,1 % |
| ±5 % | 62,5 % | 25,0 % | 55,7 % | 33,1 % |
| ±10 % | 87,5 % | 50,0 % | 88,9 % | 55,4 % |

Einseitig = nur Paarungen mit dem Schildträger auf genau einer Seite (Grundlage der bereinigten Siegquote); alle = zusätzlich Paarungen mit Schildträger auf beiden Seiten (ohne Spiegel), dort zählt der gegnerische Schildträger als Gegner.

Mittlere relative Änderung der Trefferzahl über die acht Gegner (einseitig, gleich gewichtet; |Treffer neu − Treffer 70| / Treffer 70):

| Stufe | Abschwächung | Verstärkung | Verhältnis |
|---|---|---|---|
| ±2,5 % | 6,05 % | 2,40 % | 2,52 |
| ±5 % | 8,96 % | 2,40 % | 3,74 |
| ±10 % | 13,25 % | 6,15 % | 2,15 |

## 4 Einschätzung

Beobachtetes Verhältnis |Δ(−p)| / |Δ(+p)| der Schildträger-Siegquote bei Schaden (ergebnisse_kapitel8.md, Abschnitt 2): ±2,5 % 2,48–4,32, ±5 % 1,68–2,83, ±10 % 1,40–1,85 (byactiontype, uniform, aggressive, defensive).

- **Die Asymmetrie erklären die Schwellen gut.** Auf jeder Stufe ändert die Abschwächung die Trefferzahl gegen mehr Gegner als die Verstärkung (37,5 gegen 25 %, 62,5 gegen 25 %, 87,5 gegen 50 % der Begegnungen). Vor allem trifft sie andere Helden: Die Abschwächung kostet einen zusätzlichen Treffer gegen die Helden mit wenig Lebenspunkten (Bogenschütze 4 → 5, Berserker 6 → 7, Musiker 7 → 8), also relativ viel (+14 bis +25 %). Die Verstärkung spart dagegen zuerst Treffer gegen Wand und Hugger (15 → 14, 8 → 7), relativ wenig (−7 bis −13 %). Gemessen an der mittleren relativen Änderung der Trefferzahl wirkt die Abschwächung 2,5- bis 3,7-mal so stark, in derselben Größenordnung wie beobachtet.
- **Das Abnehmen mit wachsender Stufe erklären sie nur teilweise.** Von ±5 auf ±10 % sinkt das Schwellenverhältnis (3,74 → 2,15), weil bei +10 % auch Illusionist und Elfe eine Schwelle überschreiten; das passt zur Beobachtung. Nach dem Schwellenmodell müsste ±2,5 % aber schwächer asymmetrisch sein als ±5 % (2,52 gegen 3,74), beobachtet ist es umgekehrt. Bei ±2,5 % sind die Δ klein (0,5–0,7 pp bei Verstärkung) und das Verhältnis entsprechend unsicher; dass es dort am größten ist, ist mit den Schwellen allein nicht erklärt.
- **Grenzen der Rechnung.** Gerechnet ist ein Duell aus vollen Lebenspunkten ohne Modifikatoren. In der Partie greifen meist mehrere Helden dasselbe Ziel an, die Schwelle liegt dann beim gemeinsamen Schaden; Heilung (Elfe), Schadensminderung (Schildwall, Mauer), Kampflied und Teil-Lebenspunkte verschieben die Schwellen. Die Trefferschwellen sind deshalb ein plausibler Mechanismus für die Richtung der Asymmetrie, kein quantitatives Modell.

