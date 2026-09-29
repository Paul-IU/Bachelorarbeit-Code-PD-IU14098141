# Zweite Bewertungsfunktion: Schwellen-Agent

Neue Klassen, die bisherigen Agenten bleiben unverändert:
- `ThresholdTurnEvaluator`: die Bewertungsfunktion.
- `ThresholdHeuristicAgent`: der Agent.
- `ThresholdHeuristicSettings` und `ThresholdWeights`: Gewichte und Profile.
- Auswahl über die Kennworte `threshold-aggressive` und `threshold-defensive`, im Batch
  wie in Godot.

Die Namen folgen der bestehenden Konvention (`HeuristicAgent`, `TurnEvaluator`,
`HeuristicAgentSettings`). „Threshold“ benennt das, was den Ansatz ausmacht: die
Gefahrenschwelle.

    U = w_D·D + w_H·(H + Schutz) + w_P·P + w_K·K + w_R·R

| Größe | Bedeutung | Gewicht |
|---|---|---|
| D | verursachter Schaden (auch durch Fähigkeiten) | aggressiv 2, defensiv 1 |
| H | Änderung der eigenen Lebenspunkte (Heilung, Verluste) | aggressiv 1, defensiv 2 |
| Schutz | abgefangener Anteil der tatsächlich bestehenden Bedrohung (je Gegner einmal, gegen das am wenigsten geschützte erreichbare Ziel) | mit w_H |
| P | Felder, um die die handelnde Einheit dem nächsten **angreifbaren** Gegner näher kommt | w_P = 0,1 |
| K | besiegte gegnerische Helden | w_K = 5 |
| R | Verringerung des Überschusses über die Gefahrenschwelle, max(0, T − 0,5·HP) je Held | w_R = 1 |

- **Einheit:** D, H, Schutz und R werden in Treffern zu 100 Lebenspunkten gerechnet
  (`HitPointUnit`). Sonst wäre w_K = 5 neben Schaden in rohen Lebenspunkten bedeutungslos.
- **Gefahrenschwelle 0,5:** Darunter übersteht eine Einheit einen gebündelten Angriff
  aller Gegner, die sie erreichen, mit mehr als der Hälfte ihrer Lebenspunkte. Beispiel:
  Der Bogenschütze (280 LP) ist ab zwei Gegnern gefährdet, die Wand (1000 LP) bei voller
  Gesundheit praktisch nie.
- **Begründung von w_P und w_R:** siehe `ThresholdHeuristicSettings`.
- **Grenzen:** Angekündigte Wirkungen (Buffs, Schwächungen, Schaden über Zeit,
  Aufladen) zählen nicht. Deshalb setzen die Schwellen-Agenten das Kampflied nie ein.
  Gesperrte Felder werden wie bisher nicht berücksichtigt.

**Korrektur während der Stichprobe:** P bezog sich zuerst auf den nächsten Gegner
überhaupt. Ein angehefteter Hugger (nicht angreifbar) war dann oft der nächste Gegner.
Jetzt zählt der nächste angreifbare Gegner (Test `Distanzgewinn_IgnoriertEinenNichtAngreifbarenGegner`).
Wirkung: aggressiv 36,2 → 34,2 % Rundenlimit, defensiv 50,8 → 51,2 %.

## Stichprobe: vier Agenten

20 Paarungen × 12 Ausgangslagen, Seed 2026.

| | alt aggressiv | alt defensiv | **neu aggressiv** | **neu defensiv** |
|---|---|---|---|---|
| Rundenlimit | 0,0 % | 47,5 % | **34,2 %** | **51,2 %** |
| davon Patt mit angeheftetem Hugger¹ | – | 41 von 114 | **71 von 82** | **79 von 123** |
| Rundenlimit ohne dieses Patt | 0,0 % | 30,4 % | **4,6 %** | **18,3 %** |
| Runden im Schnitt | 10,3 | 57,3 | 42,2 | 57,9 |
| je Heldenzug: Bewegungen / Angriffe / Fähigkeiten | 0,90 / 0,67 / 0,20 | 0,65 / 0,18 / 0,31 | 0,35 / 0,26 / 0,07 | 0,52 / 0,29 / 0,22 |
| Mauer je Partie | 1,50 | 13,9 | 0,17 | 9,0 |
| Schildwall je Partie | 1,37 | 7,5 | 0,23 | 4,4 |
| Kampflied je Partie | 2,25 | 13,9 | 0 | 0 |
| Heilung je Partie | 0,58 | 24,9 | 10,0 | 39,8 |
| Mauer/Schildwall mit Gegner in Reichweite | 100 % | 100 % | 100 % | 100 % |
| Heldenzüge über der Gefahrenschwelle | 45,6 % | 22,0 % | 15,9 % | 22,5 % |
| … davon Rückzug / Angriff / weder noch | 9 / 76 / 15 % | 24 / 34 / 42 % | 10 / 65 / 25 % | **4** / 48 / 47 % |
| geschätzter Vollauf | ≈ 2,4 h | ≈ 11 h | ≈ 6 h | ≈ 17 h |

¹ Auf einer Seite lebt nur noch ein Hugger, der an einer Einheit angeheftet ist. Er ist
nicht angreifbar, darf nicht angreifen und löst sich nicht. Bis zum Rundenlimit geschieht
nichts mehr. Das ist eine Lücke in den Spielregeln, keine Eigenschaft einer
Bewertungsfunktion. Warum der alte aggressive Agent nie in dieses Patt gerät, ist
nicht untersucht. Vermutlich löst sich sein Hugger (0,14-mal „Lösen“ je Partie), weil die
Angriffsmöglichkeit das belohnt.

„Rückzug“ heißt hier: eine Bewegung, die den Überschuss über die Schwelle senkt.
