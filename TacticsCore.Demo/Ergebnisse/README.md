# Vollständiger Basislauf (Zufallsagent gegen Zufallsagent)

Erzeugt über:

```
dotnet run --project TacticsCore.Demo -- full-baseline Ergebnisse/basislauf_paarungen.csv Ergebnisse/basislauf_kompositionen.csv 2026
```

- Basis-Seed: `2026`
- 84 Kompositionen (alle 3er-Teilmengen der neun Standardhelden) → 3.570 eindeutige, ungeordnete Paarungen (inklusive 84 Spiegelpaarungen einer Komposition gegen sich selbst)
- Jede Paarung über alle 72 Ausgangslagen gespielt (6 Aufstellungspermutationen je Team × 2 mögliche Startteams) → **jedes Matchup genau 72 Partien**
- Insgesamt 257.040 Partien, Zufallsagent gegen Zufallsagent auf beiden Seiten
- Laufzeit auf der Erzeugungsmaschine: 207,4 s (0,81 ms/Partie, 12 Threads)

## Dateien

- `basislauf_paarungen.csv` — eine Zeile je Paarung (3.570 Zeilen + Kopfzeile): Siege je Seite, Unentschieden, Siegquoten, Ø-Rundenzahl, Ø-Rest-HP je Team, Anteil Rundenlimit-Enden.
- `basislauf_kompositionen.csv` — eine Zeile je Komposition (84 Zeilen + Kopfzeile): über ALLE ihre Paarungen aggregierte Siegquote.

## Reproduzierbarkeit

Bei gleichem Basis-Seed (`2026`) liefert der Lauf deterministisch exakt dieselben Zahlen, unabhängig von der Anzahl genutzter Threads (siehe `BatchSimulationRunner`-Klassendokumentation und `BatchSimulationRunnerTests.EinThreadUndMehrereThreads_LiefernIdentischeErgebnisse`).

## Erste Beobachtung (unkommentierte Rohdaten, keine tiefere Analyse)

Stärkste Kompositionen nach Siegquote: Berserker+Elementarmagier+Elfe (88,9 %), Berserker+Elementarmagier+Illusionist (86,5 %), Berserker+Elfe+Illusionist (85,3 %).
Schwächste Kompositionen: Hugger+Musiker+Wand (5,8 %), Hugger+Musiker+Schildträger (10,9 %), Hugger+Schildträger+Wand (11,9 %).
Auffällig: alle drei schwächsten enthalten Hugger, alle drei stärksten enthalten Berserker — ein möglicher Ausgangspunkt für die Balance-Diskussion in der Arbeit, noch nicht weiter untersucht.
