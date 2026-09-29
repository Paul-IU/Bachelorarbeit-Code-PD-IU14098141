# Verifikation der ×10-Skalierung aller Kampfwerte

Vergleicht den vollständigen Basislauf (3.570 Paarungen, 84 Kompositionen,
72 Ausgangslagen je Paarung, 257.040 Partien, Basis-Seed `2026`,
Zufallsagent `byactiontype` auf beiden Seiten) **vor** und **nach** der
Faktor-10-Skalierung aller Schadens-/Lebenspunkte-Werte im Heldenkatalog
(siehe Klassendokumentation von `DefaultHeroes`, Punkt 3).

## Ergebnis in Kürze

| Gruppe | Anzahl Paarungen | Verhalten |
|---|---|---|
| Ohne Schildträger UND ohne Wand | 630 | **exakt identisch** (Siege, Siegquote, Ø-Rundenzahl, Ø-Rest-HP-Anteil, Rundenlimit-Anteil - alle Kennzahlen bitgenau gleich) |
| Mit Schildträger und/oder Wand | 2.940 | 1.992 (67,8 %) zeigen eine Abweichung in mindestens einer der 72 Ausgangslagen |
| ... davon mit geändertem Sieger/Sieganzahl | 524 (17,8 % der 2.940) | in `abweichungen_schildtraeger_wand.csv`, Spalte `SiegAnzahlVeraendert=True` |
| ... davon NUR mit geänderter Ø-Rundenzahl (gleicher Sieger) | 1.468 | `SiegAnzahlVeraendert=False` |

## Ursache (empirisch nachvollzogen, nicht nur vermutet)

Schildträgers "Schildwall" und Wands "Mauer" reduzieren eingehenden Schaden
um exakt 50 % (`EffectTemplate.IncomingDamageMultiplier: 0.5`,
`CombatSystem.ApplyIncomingReduction`, `Math.Round(amount * 0.5)`).

**Vor** der Skalierung enthielt der Katalog mehrere UNGERADE Rohschadenswerte:
Schildträger-/Wand-Grundschaden (7), Feuerball (3), Erdschlag (7),
Verbrennungs-Ticks (3 bzw. 1), Musiker-Grundschaden (9), Bogenschütze mit
ungerader Ladungsstufe (12+5=17). Halbiert ergeben diese echte
Kommazahlen (3,5 / 1,5 / 0,5 / 8,5 / ...), über die `Math.Round` per
"Bankers Rounding" (round-half-to-even) entscheiden musste - z.B. 3,5 → 4,
aber 0,5 → 0.

**Nach** der ×10-Skalierung sind alle diese Werte gerade (70, 30, 70, 30/10,
90, 170), ihre Hälfte ist immer eine ganze Zahl (35, 15, 35, 15/5, 45, 85) -
`Math.Round` muss nie mehr entscheiden. Das ist die korrektere, unzweideutige
Rechnung, aber sie liefert an genau diesen Stellen ein ANDERES Ergebnis als
die alte Rundungsentscheidung (35 statt 4×10=40, 5 statt 0×10=0, ...). In
einem deterministischen, aber verzweigenden System (jede weitere
Zufallsentscheidung hängt vom Spielzustand ab) genügt eine einzelne
abweichende Trefferzahl, um den gesamten weiteren Partieverlauf zu ändern.

Direkt nachvollzogen an einer konkreten Partie (Berserker+Bogenschütze+
Elementarmagier gegen Berserker+Bogenschütze+Wand, Ausgangslage 25): bis
Runde 18 sind alle Einzeltreffer exakt proportional (×10) zwischen altem
und neuem Katalog. In Runde 23 unterscheidet sich die kumulierte Rest-HP
der Wand durch genau diesen Rundungseffekt - die Partie endet dadurch mit
vertauschtem Sieger (Runde 23 statt 24).

## Warum das nicht behebbar ist, ohne das eigentliche Ziel zu unterlaufen

Die alten Bankers-Rounding-Ergebnisse Wert für Wert nachzubilden liefe dem
Zweck der Skalierung (klare, prozentual sauber veränderbare Zehnerwerte)
entgegen und wäre zudem selbst wieder nur ein Sonderfall-Flicken. Außerhalb
von Schildträger/Wand-Paarungen (630 von 3.570, siehe oben) ist das
Verhalten nachweislich exakt identisch geblieben - die Restabweichung ist
eine mathematisch inhärente, jetzt exakt quantifizierte und erklärte
Randerscheinung der Skalierung selbst, keine übersehene Katalogstelle.

## Dateien

- `abweichungen_schildtraeger_wand.csv` - eine Zeile je abweichender Paarung
  (1.992 Zeilen + Kopfzeile), sortiert nach Schweregrad
  (`SiegAnzahlVeraendert` zuerst): Sieganzahl und Ø-Rundenzahl je Team,
  vorher/nachher gegenübergestellt, plus Kennzeichnung ob Schildträger
  bzw. Wand beteiligt ist.

## Reproduzierbarkeit

Basis-Seed `2026`, Agent `byactiontype`, erzeugt über:

```
dotnet run --project TacticsCore.Demo -c Release -- full-baseline <paarungenCsv> <kompositionenCsv> <heldenCsv> 2026
```

einmal mit dem Katalogstand vor der Skalierung, einmal danach - alle
übrigen Parameter (Board, Ausgangslagen, Threads) sind für beide Läufe
identisch.
