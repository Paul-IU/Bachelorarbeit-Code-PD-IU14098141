# Designentscheidungen: Umleitung nur für Gegner, Selbstreduktion generell

1. **Umleitung nur für Gegner** (`CombatSystem.ResolveRedirect`): Angriffe von
   Verbündeten der Wand - auch durch ihr Feld hindurch - treffen ihr eigentliches
   Ziel und lösen keine Vergeltung aus. Behebt zugleich, dass die Wand sich bei
   einem Angriff auf einen Gegner auf ihrem eigenen Feld selbst traf.
2. **Selbstreduktion generell** (nur Dokumentation): solange die Mauer aktiv ist,
   nimmt die Wand jeden Schaden halb - direkt, umgeleitet und über Zeit. Die
   Vergeltung bleibt an umgeleitete Treffer gebunden.

Tests: `MauerNurGegnerTests`, `MauerWandertMitTests`. Vorher = `../mauer_wandert_mit`
(Seed 2026). Details: `auswertung.txt`, `helden_vergleich.csv`.

| | byActionType | uniform |
|---|---|---|
| Eigenbeschuss (Stichprobe, Partien mit Wand) | 28,16 % → **0 %** | 24,42 % → **0 %** |
| Siegquote Wand | 41,83 → 45,92 % (**+4,09 pp**) | 47,36 → 50,79 % (**+3,43 pp**) |
| Kompositionen mit Wand | im Mittel +2,73 pp (−0,97 … +9,57) | im Mittel +2,29 pp (−0,89 … +7,45) |
| Seed-Rauschen (RMS über 9 Helden) | 0,18 pp | 0,14 pp |

Gewinner neben der Wand: Bogenschütze (+0,92 / +0,85) und Elementarmagier
(+0,52 / +0,54) - die Fernkämpfer, die vorher durch das Feld der eigenen Wand
schossen. Alle übrigen Helden verlieren 0,35 bis 1,19 pp. Paarungen ohne Wand
sind bitgenau unverändert.

## Offen: zwei Wände auf einem Feld

Wirken zwei gegnerische Wände auf demselben Feld ihre Mauer, frischt die zweite
die Feld-Mauer der ersten auf, statt eine eigene zu erzeugen (Stapelregel: gleicher
Name, gleiches Feld). Die zweite Wand erhält nur ihre Selbstreduktion, ihre
Verbündeten auf dem Feld bleiben ungeschützt. Häufigkeit (alle 29.232 Partien
Wand gegen Wand, Seed 2026): in 27,9 % (byActionType) bzw. 18,5 % (uniform)
dieser Partien mindestens einmal, das sind 3,2 % bzw. 2,1 % aller Basislauf-Partien.
Test: `Beobachtung_MauerAufFeldMitGegnerischerMauer_FrischtDieGegnerischeAuf`.

**Inzwischen behoben**: siehe `../mauer_eigene_identitaet` (der Anwender gehört zur
Identität). Der Beobachtungstest wurde durch Verhaltenstests ersetzt.
