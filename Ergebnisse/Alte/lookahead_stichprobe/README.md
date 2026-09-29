# Vorausschauender Agent – Laufzeit- und Verhaltensstichprobe (2026-09-26)

Stichprobe: 20 Paarungen × 12 Ausgangslagen = 240 Partien, Seed 2026, 12 Threads
(dieselbe Auswahl wie bei früheren Stichproben). Gegnermodell B (der simulierte Gegner
wählt die für den Agenten schlechteste Antwort). Rohdaten: `rohdaten.txt`.

Laufzeitläufe ohne Zusatzzählung. Verhaltensläufe zählen zusätzlich Gelegenheiten und
Abweichungen; ihre Wandzeit ist nicht aussagekräftig.

## Laufzeit

| Agent | Stichprobe | ms je Entscheidung | Faktor zu neutral einstufig | Entscheidungen je Partie | Hochrechnung roh | korrigiert ×0,63 |
|---|---|---|---|---|---|---|
| aggressiv (2:1) | 9,7 s | 4,7 | – | 93,9 | 2,9 h | 1,8 h (tatsächlich 1 h 49 min) |
| neutral einstufig (1:1) | 28,0 s | 5,0 | 1,0 | 168,3 | 8,3 h | 5,3 h |
| Vorausschau K = 3 | 199,9 s | 34,6 | 6,9 | 187,8 | 59,5 h | 37,6 h |
| Vorausschau K = 5 | 272,6 s | 54,0 | 10,8 | 181,7 | 81,1 h | 51,2 h |
| Vorausschau K = 10 | 413,3 s | 84,6 | 16,9 | 181,1 | 123,0 h | 77,7 h |

Korrektur: für den aggressiven Agenten schätzte die Stichprobe 2,88 h, der vollständige
Lauf brauchte 1,82 h (Faktor 0,63). Unter der Annahme, dass dieser Faktor übertragbar ist.

## Partien und Aktionen

| Agent | Rundenlimit | Runden | Bewegungen / Angriffe / Fähigkeiten je Heldenzug |
|---|---|---|---|
| aggressiv | 2,1 % | 12,2 | 0,916 / 0,769 / 0,072 |
| neutral einstufig | 7,5 % | 18,1 | 0,955 / 0,646 / 0,264 |
| Vorausschau K = 3 | 13,3 % | 20,4 | 0,949 / 0,604 / 0,279 |
| Vorausschau K = 5 | 12,5 % | 19,4 | 0,940 / 0,601 / 0,283 |
| Vorausschau K = 10 | 12,1 % | 19,3 | 0,933 / 0,604 / 0,285 |

## Beschneidung

| Vergleich | abweichende Entscheidungen | ms je Entscheidung |
|---|---|---|
| K = 10 statt 5 | 1,2 % | +57 % |
| K = 3 statt 5 | 1,5 % | −36 % |

## Die beiden bekannten Schwächen

| | aggressiv | neutral einstufig | Vorausschau K = 5 |
|---|---|---|---|
| Berserker angegriffen, wenn möglich | 19,3 % (502 / 2.607) | 12,1 % (538 / 4.438) | 9,0 % (539 / 5.997) |
| Berserker-Angriffe je Partie | 2,09 | 2,24 | 2,25 |
| Aufladen gewählt, wenn möglich | 0,0 % (0 / 716) | 1,2 % (12 / 1.025) | 3,1 % (34 / 1.092) |

Vorausschau gegenüber neutral einstufig, auf denselben Zuständen (Vorausschau spielt):
5,4 % der Entscheidungen weichen ab. Davon 674-mal "einstufig griffe den Berserker an,
die Vorausschau nicht", 13-mal umgekehrt, 34-mal "nur die Vorausschau lädt auf".

"Gelegenheit" = zu Zugbeginn ist ein Angriff auf einen Berserker (direkt oder nach einer
Bewegung) bzw. Laden möglich. Die Zahl der Gelegenheiten hängt vom Spielverlauf des
jeweiligen Agenten ab.
