# Vollständiger Basislauf: aggressiver HeuristicAgent

Stand: Bewertungsfunktion nach Überarbeitung 7 und der Laufzeit-Optimierung
`TurnEvaluator.DecisionContext`. Die Optimierung ist verhaltensneutral: Die Stichprobe
aller sechs Agenten war vor und nach der Optimierung bitgleich. Regelstand wie
`../hugger_letzter_held`. 3.570 Paarungen × 72 Ausgangslagen = 257.040 Partien, Seed 2026.

- Etiketten-Prüfung (mitlaufend): 257.040 Partien, **0 Abweichungen**.
- **Tatsächliche Laufzeit:** 10.439 s = 2 h 54 min, **mit** Etiketten-Prüfung. Die
  Prüfung spielt jede Partie ein zweites Mal; ohne sie sind es etwa 1 h 27 min.

**Laufzeit der Bewertung** (fester Messsatz, einfädig, Bestwert aus 3):

| Stand | ms je Entscheidung |
|---|---|
| vor der Optimierung | 1,41 |
| nach der Optimierung | **0,88 (−37 %)** |

**Geschätzter vollständiger Lauf des defensiven Agenten** (Stichprobe nach der
Optimierung): etwa 6,6 h ohne Etiketten-Prüfung, etwa 13 h mit. Beim aggressiven Agenten
lag die Schätzung aus der Stichprobe (99 min) nahe an der tatsächlichen Laufzeit (etwa
87 min).

## Kennzahlen

| | byActionType | uniform | aggressiv |
|---|---|---|---|
| Partiedauer (Runden) | 28,9 | 33,1 | **11,2** |
| Rundenlimit | 2,20 % | 3,47 % | **1,00 %** |

Aggressiv, je Heldenzug: Bewegungen 0,846, Angriffe 0,628, Fähigkeiten 0,156.

| Fähigkeit | je Partie (alle Partien) | je Partie mit dem Helden |
|---|---|---|
| Mauer (Wand) | 0,33 | 0,59 |
| Schildwall (Schildträger) | 0,46 | 0,83 |
| Lied (Musiker, Kampflied + Marschlied)¹ | 2,30 | 4,16 |
| Heilung (Elfe) | 0,25 | 0,45 |

¹ Die Helden-CSV trennt die Modi nicht. In der Stichprobe waren rund 90 % Kampflied.

## Rangfolge der Helden (bereinigte Siegquote)

Bereinigt heißt: nur Partien, in denen der Held genau einer Seite angehört.

| Held | byActionType | uniform | aggressiv |
|---|---|---|---|
| Hugger | 37,80 % (9) | 35,52 % (8) | **63,08 % (1)** |
| Musiker | 39,32 % (8) | 34,97 % (9) | **60,91 % (2)** |
| Wand | 51,04 % (5) | 53,31 % (3) | 58,96 % (3) |
| Berserker | 55,05 % (4) | 52,94 % (4) | 51,33 % (4) |
| Schildträger | 45,53 % (7) | 49,76 % (7) | 49,34 % (5) |
| Bogenschütze | 46,26 % (6) | 51,50 % (6) | 49,18 % (6) |
| Illusionist | 56,64 % (3) | 52,88 % (5) | 46,51 % (7) |
| Elfe | 58,50 % (2) | 60,51 % (1) | **36,29 % (8)** |
| Elementarmagier | 59,84 % (1) | 58,61 % (2) | **34,33 % (9)** |

| Rangkorrelation | Spearman ρ | Kendall τ |
|---|---|---|
| byActionType / uniform | 0,900 | 0,722 |
| byActionType / aggressiv | **−0,867** | −0,778 |
| uniform / aggressiv | **−0,700** | −0,500 |

**Befund zur Strategieabhängigkeit:** Die beiden Zufallsagenten kommen zu fast derselben
Rangfolge. Der aggressive Agent kehrt sie weitgehend um. Die beiden Supporthelden, die
unter Zufall am schwächsten sind (Hugger, Musiker), stehen vorn. Die beiden, die unter
Zufall vorn stehen (Elementarmagier, Elfe), stehen hinten.

**Tank-Rangfolge:**

| Agent | Rangfolge |
|---|---|
| byActionType | Illusionist 56,64 > Wand 51,04 > Schildträger 45,53 |
| uniform | Wand 53,31 > Illusionist 52,88 > Schildträger 49,76 |
| aggressiv | Wand 58,96 > **Schildträger 49,34** > Illusionist 46,51 |

Der Schildträger liegt nur beim aggressiven Agenten zwischen den beiden anderen Tanks.
Bei beiden Zufallsagenten ist er seit der Mauer-Abschwächung der schwächste Tank.

Seed-Rauschen: Der aggressive Agent nutzt keine Zufallszahlen. Ein Vergleich zweier Seeds
wie bei den Zufallsagenten ist daher nicht gemessen. Ob der Seed hier überhaupt eine
Wirkung hat, ist nicht geprüft.
