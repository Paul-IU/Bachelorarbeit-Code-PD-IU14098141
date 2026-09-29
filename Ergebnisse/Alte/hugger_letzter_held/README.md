# Behebung einer Regellücke: angehefteter letzter Held

**Lücke:** Ein angehefteter Hugger ist für Gegner nicht angreifbar, kann selbst nicht
angreifen und löst sich nur aktiv. War er die letzte lebende Einheit seines Teams, stand
die Partie bis zum Rundenlimit still.

**Regel** (`EffectSystem.ReleaseAttachmentsOfLastHeroes`, generisch über
`CausesSourceToFollowTarget`, keine Sonderregel für den Hugger):
1. Ist ein angehefteter Held der letzte lebende Held seines Teams, löst sich die
   Anheftung sofort, samt gekoppelter Instanz (Unangreifbarkeit, Angriffs- und
   Bewegungssperre, Schwächung des Ziels).
2. Als letzter Held kann er keine anheftende Fähigkeit einsetzen
   (`AbilitySystem.IsModeLegal`). Sonst entstünde das Patt im nächsten Zug neu.

**Die drei Fragen:**
- **Zeitpunkt:** Geprüft wird nach jeder Niederlage (`CombatSystem.HandleDefeat`),
  also im selben Moment, in dem die vorletzte Einheit fällt. Er ist sofort angreifbar.
- **Klon:** Er zählt nicht als weitere Einheit, wie bei der Siegbedingung. Er kann auch
  nie übrig bleiben, weil er mit seinem Erzeuger verschwindet. Stirbt der Illusionist,
  löst sich der Hugger im selben Moment.
- **Symmetrie:** Jedes Team wird für sich geprüft; die Reihenfolge ist ohne Einfluss.
  Dass beide Teams nur aus einem angehefteten Helden bestehen, ist nicht möglich. Eine
  Anheftung endet mit dem Tod ihres Ziels, und ein angehefteter Held ist kein gültiges
  Ziel, kann also selbst nicht angeheftet werden.

Die Anheftung im normalen Spielverlauf ist unverändert.

**Tests:** `HuggerLetzterHeldTests` (6 Tests). Zwei bestehende Testaufbauten bekamen
einen Verbündeten für den Hugger, weil er als einziger Held seines Teams nicht mehr
greifen darf: `AbilitySystemTests` und `UmklammerungProzentualTests`.

## Basislauf (Zufallsagenten)

Vorher = `../umklammerung_prozentual`: aktueller Stand mit abgeschalteter Regel.
Details stehen in `auswertung_umklammerung_prozentual_zu_hugger_letzter_held.txt`.

| | byActionType | uniform |
|---|---|---|
| Paarungen ohne Hugger (1.596) | alle bitgleich | alle bitgleich |
| Etiketten-Prüfung | 0 Abweichungen | 0 Abweichungen |
| **Siegquote Hugger** | 31,28 → **37,80 %** (+6,52 pp) | 29,23 → **35,52 %** (+6,29 pp) |
| RMS-Änderung über 9 Helden (Seed-Rauschen) | 2,36 pp (0,18) | 2,28 pp (0,14) |
| Partiedauer mit Hugger (Runden) | 33,0 → 28,6 | 39,1 → 33,1 |
| Rundenlimit mit Hugger | 1,75 → 1,42 % | 3,48 → 2,65 % |
| Tank-Rangfolge | unverändert: Illusionist > Wand > Schildträger | unverändert: Wand > Illusionist > Schildträger |

Für die Zufallsagenten war das Patt selten; sie lösen den Hugger zufällig auch aktiv.
Der Hugger gewinnt trotzdem deutlich, vermutlich wie bei der Mauer-Abklingzeit: Als
letzter Held verschwendet er keine Züge mehr mit erneutem Greifen, sondern greift an.

## Stichprobe (20 Paarungen × 12 Ausgangslagen, Seed 2026)

| | alt aggressiv | alt defensiv | Schwelle aggressiv¹ | Schwelle defensiv¹ | byActionType | uniform |
|---|---|---|---|---|---|---|
| Rundenlimit vorher | 0,0 % | 47,5 % | 34,2 % | 51,2 % | 6,2 % | 11,2 % |
| **Rundenlimit nachher** | 0,0 % | **29,6 %** | **4,6 %** | **18,3 %** | 5,0 % | 8,8 % |
| davon Hugger-Patt vorher → nachher | – | 41 → 0 | 71 → 0 | 79 → 0 | 3 → 0 | 4 → 0 |
| Runden im Schnitt | 10,3 | 57,3 → 42,6 | 42,2 → 16,1 | 57,9 → 30,7 | 35,5 → 33,2 | 42,9 → 38,5 |
| je Heldenzug: Bew. / Angr. / Fäh. (nachher) | 0,90 / 0,67 / 0,20 | 0,79 / 0,23 / 0,35 | 0,91 / 0,69 / 0,17 | 0,86 / 0,48 / 0,35 | 0,95 / 0,31 / 0,54 | 0,96 / 0,27 / 0,58 |
| Mauer / Schildwall / Kampflied je Partie (nachher) | 1,5 / 1,4 / 2,3 | 14,0 / 7,5 / 9,1 | 0,2 / 0,2 / 0 | 9,0 / 4,5 / 0 | 10,7 / 6,4 / 2,6 | 12,8 / 6,4 / 3,5 |
| geschätzter Vollauf vorher → nachher | 2,4 h | 11 h → 10 h | 6,4 h → 4,3 h | 17 h → 15 h | 13 min | 17 min |
| **Sonderfall: automatisch gelöst** (Partien: 240) | 0 | 1 | 71 | 78 | 39 | 34 |

¹ Über die Batch-Konfiguration deaktiviert, für diesen Vergleich direkt erzeugt.

Das Patt kann auf zwei Wegen entstehen, und die Regel schließt beide:
- **Hugger bereits angeheftet, als er letzter Held wird:** Er löst sich automatisch.
  Das ist der „Sonderfall“ in der Tabelle, typisch für die Schwellen-Agenten.
- **Hugger greift erst als letzter Held (erneut):** Das ist jetzt nicht mehr erlaubt.
  Dieser Weg dominierte beim alten defensiven Agenten. Das erklärt, warum dort das Patt
  verschwindet (41 → 0), der Sonderfall aber nur einmal gezählt wird.

Zählweise des Sonderfalls: zuvor angeheftet, jetzt nicht mehr, der Held ist letzter
seines Teams, und sein bisheriges Ziel lebt noch. Andernfalls endete die Anheftung
normal durch den Tod des Ziels.

## Nebenbefund: Schritt 5 „Umklammerung prozentual“

Für diesen Schritt gab es bisher keinen Basislauf mit den Zufallsagenten.
`../umklammerung_prozentual` gegen `../mauer_abschwaechen`:

- Hugger: 39,85 → 31,28 % (byActionType) und 38,05 → 29,23 % (uniform), also **−8,6 bzw.
  −8,8 pp**. Er wurde damit zum mit Abstand schwächsten Helden.
- Paarungen ohne Hugger: bitgleich.
- Zusammen mit dieser Behebung liegt der Hugger jetzt bei 37,8 % bzw. 35,5 %, also
  weiterhin unter seinem Stand vor Schritt 5.

## Hinweis zur Messung

Zwei Fehler im Messaufbau habe ich während der Arbeit bemerkt und korrigiert:
1. Ein erster Vorher-Lauf enthielt die Regel bereits teilweise. Ein Kodierungsfehler im
   Skript ließ die Suchzeile ins Leere laufen.
2. Beim Wiederherstellen der Quelldateien blieb der alte Zeitstempel erhalten, und der
   inkrementelle Build verwendete veraltete Binärdateien.

Alle Zahlen hier stammen aus den wiederholten, vollständig neu gebauten Läufen.
