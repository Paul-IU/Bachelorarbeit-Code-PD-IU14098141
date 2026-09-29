# Designentscheidung: Jede Wand hat ihre eigene Mauer

**Regel** (`EffectSystem.FindMatching`, generisch): bei Effekten mit
`FieldFollowsSource` gehört der Anwender zur Stapel-Identität (Name + Anker + Ziel
+ **Anwender**). Stehen zwei gegnerische Wände auf demselben Feld und wirken beide,
bestehen zwei Feld-Mauern nebeneinander. Vorher frischte die zweite Wand die Mauer
der ersten auf und stärkte damit die Fähigkeit ihres Gegners. Alle übrigen Effekte
sind unverändert, insbesondere der Schildwall (zwei Schildträger ergeben weiterhin
eine Instanz, also 0,5 statt 0,25 Aufnahme).

Verhalten mit beiden Mauern (Tests in `MauerNurGegnerTests`, Abschnitt 4):
- Ein Angriff auf eine Einheit auf dem Feld geht zur Wand des **angegriffenen** Teams.
  Die Mauer der eigenen Wand des Angreifers leitet nicht um, weil die Umleitung nur für
  Gegner gilt. Die Vergeltung trifft den ursprünglichen Angreifer.
- Die Vergeltung selbst wird nie umgeleitet, deshalb gibt es keine Kette zwischen den Wänden.
- Zieht eine Wand weg, nimmt sie nur ihre eigene Mauer mit. Die andere bleibt.
- Beide Sperren wirken: Das Feld ist für Außenstehende beider Teams gesperrt.
  Einheiten auf dem Feld dürfen es verlassen.
- Test `ZweiSchildtraegerAufEinemFeld_SchildwallBleibtEineInstanz`: Der Schildwall
  stapelt sich nicht.

Vorher = `../mauer_nur_gegner`, Seed 2026, beide Zufallsagenten. Details stehen in
`auswertung.txt` und `helden_vergleich.csv`.

## Ergebnis

| | byActionType | uniform |
|---|---|---|
| Paarungen ohne Wand auf beiden Seiten | 3.164, alle bitgleich | 3.164, alle bitgleich |
| Paarungen Wand gegen Wand verändert | 398 von 406 | 392 von 406 |
| Siegquote Wand | 45,92 → 45,92 % (±0,00) | 50,79 → 50,79 % (±0,00) |
| Rang der Wand | 7 von 9 | 5 von 9 |
| RMS-Änderung über 9 Helden (Seed-Rauschen) | 0,01 pp (0,18) | 0,01 pp (0,14) |
| RMS-Änderung der 28 Wand-Kompositionen (Seed-Rauschen) | 0,11 pp (0,94) | 0,08 pp (0,85) |
| Kompositionen mit Wand, Spanne | −0,24 … +0,23 pp | −0,19 … +0,22 pp |

**Die Wand wird nicht zum stärksten Helden.** Ihre Siegquote kann sich durch diese
Änderung aus einem strukturellen Grund nicht ändern. Jede Komposition hat drei
verschiedene Helden, also betrifft die Änderung nur Partien, in denen **beide**
Teams eine Wand haben. In jeder solchen Partie gewinnt genau eine Wand-Komposition
und eine verliert. Die Helden-Siegquote der Wand bleibt deshalb exakt gleich. Die
Änderung verschiebt nur, **welche** Wand-Kompositionen gegeneinander gewinnen, und
das liegt bei etwa einem Neuntel des Seed-Rauschens.

Stärkste Helden bleiben:
- byActionType: Elfe 59,73 % und Elementarmagier 59,47 %
- uniform: Elfe 61,57 % und Elementarmagier 58,20 %

Etiketten-Prüfung und Reproduzierbarkeit: siehe `etiketten_pruefung.log`
(`--verify-name-independence`, zweiter Lauf bitgleich zum ersten). Zusätzlich ist der
Test `AlleSpiegelpaarungenMitAntithetischerPaarbildung_ErgebenExaktGleicheSiegzahlen`
grün, ebenso die übrige Suite (299 Tests, ohne „Langlaufend“).

Befehl: `dotnet run --project TacticsCore.Demo -c Release -- full-baseline <p> <k> <h> 2026 <agent> --verify-name-independence`
