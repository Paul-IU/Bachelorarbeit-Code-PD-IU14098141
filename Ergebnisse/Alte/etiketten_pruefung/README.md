# Etiketten-Prüfung (optional): Laufzeitmessung

Vollständiger Basislauf (3.570 Paarungen × 72 Ausgangslagen = 257.040 Partien,
Seed 2026, 12 Threads), je einmal ohne und mit `--verify-name-independence`,
nacheinander auf derselben Maschine.

| Agent | ohne Option | mit Option | Faktor | Ergebnis der Prüfung |
|---|---:|---:|---:|---|
| byActionType | 352,7 s (5,9 min) | 677,7 s (11,3 min) | 1,92 | 257.040 geprüft, 0 Abweichungen |
| Uniform | 387,5 s (6,5 min) | 804,9 s (13,4 min) | 2,08 | 257.040 geprüft, 0 Abweichungen |

- Die CSV-Dateien mit und ohne Option sind bitgenau gleich und auch gleich zum
  früheren Lauf in `../spiegel_symmetrie` (Seed 2026).
- Der Mehraufwand ist die zweite Partie je Ausgangslage. Nur bei einer
  Abweichung werden beide Partien zusätzlich mit Protokoll wiederholt.

Aufruf:

```
dotnet run --project TacticsCore.Demo -c Release -- full-baseline <paarungenCsv> <kompositionenCsv> <heldenCsv> 2026 byactiontype --verify-name-independence
```

Konsolenausgaben: `konsole_<agent>_<ohne|mit>.txt`.
