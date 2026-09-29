# Designentscheidung: Mauer abschwächen

Anlass war eine Beobachtung in Godot: Die Wand wirkt ihre Mauer in fast jedem Zug, die Elfe
heilt den Rest weg. Geändert wurde nur die Mauer, der Schildwall bleibt gleich (Dokumentation in
`DefaultHeroes.Wand`):
- **Selbstreduktion 50 % → 25 %** (`WandMauerSchadensmultiplikator` 0,5 → 0,75). Die Vergeltung
  bleibt bei der Hälfte des ursprünglichen Schadens.
- **Abklingzeit eine Runde** (`WandMauerAbklingzeit` = 2). Die Abklingzeit sinkt schon zu Beginn
  des ersten eigenen Zugs nach dem Einsatz. Der Wert 1 hätte daher keine Wirkung. Mit 2 liegt
  zwischen zwei Mauern immer eine ungeschützte Runde (gleiche Regel wie beim Schildwall).
- **Rundung**: 0,75 erzeugt bei ungeraden Vielfachen von 10 echte Halbwerte. Die bisherige
  Rundung (Bankers Rounding) löst sie auf: 70 → 52, 90 → 68.

Tests: `MauerAbschwaechenTests`. 10 bestehende Tests wurden auf 75 % umgestellt.
Vorher = `../mauer_eigene_identitaet`, Seed 2026. Details stehen in `auswertung.txt`.

## Zufallsagenten (vollständiger Basislauf)

| | byActionType | uniform |
|---|---|---|
| Paarungen ohne Wand (1.596) | alle bitgleich | alle bitgleich |
| Etiketten-Prüfung | 0 Abweichungen | 0 Abweichungen |
| **Siegquote Wand** | 45,92 → **51,11 %** (+5,19 pp) | 50,79 → **53,12 %** (+2,33 pp) |
| RMS-Änderung über 9 Helden (Seed-Rauschen) | 1,92 pp (0,18) | 1,01 pp (0,14) |
| Tank-Rangfolge vorher | Illusionist > Schildträger > Wand | Illusionist > Wand > Schildträger |
| Tank-Rangfolge nachher | Illusionist > **Wand > Schildträger** | **Wand > Illusionist** > Schildträger |
| Mauer je Zug der Wand | 0,78 → 0,44 | 0,77 → 0,43 |
| Angriffe je Zug der Wand | 0,22 → 0,32 | 0,23 → 0,31 |
| Partiedauer gesamt (Runden) | 35,8 → 31,8 | 41,0 → 36,8 |
| Rundenlimit gesamt | 4,12 → 2,68 % | 6,60 → 4,36 % |
| Partiedauer mit Wand (Runden) | 41,9 → 34,6 | 47,0 → 39,3 |
| Rundenlimit mit Wand | 5,17 → 2,56 % | 8,09 → 4,05 % |

**Die Wand wird bei den Zufallsagenten stärker, nicht schwächer.** Ein Zufallsagent setzt die
Mauer auch ohne Gegner in der Nähe ein. Dieser Einsatz verbraucht die Aktion, ohne etwas zu
bewirken. Die Abklingzeit verhindert das in jedem zweiten Zug, und die Wand greift stattdessen
an (+0,09 bis +0,10 Angriffe je Zug). Dieser Gewinn überwiegt den Verlust an Schutz. Der Befund
ist also modellabhängig: Die Maßnahme richtet sich gegen einen gezielt eingesetzten Schutz, und
den gibt es bei Zufallsagenten nicht.

## Heuristische Agenten (Stichprobe 20 × 12, wie in `../heuristik_ueberarbeitung`)

| | aggressive vorher → nachher | defensive vorher → nachher |
|---|---|---|
| Mauer je Partie | 5,80 → 1,69 | 53,22 → 21,04 |
| Heilung je Partie | 2,29 → 0,75 | 23,46 → 34,92 |
| Rundenlimit | 5,8 → 7,5 % | 83,3 → 79,6 % |
| Runden im Schnitt | 17,4 → 17,5 | 86,1 → 83,1 |

Siegquoten je Held gibt es für die heuristischen Agenten erst nach einem vollständigen Lauf.
