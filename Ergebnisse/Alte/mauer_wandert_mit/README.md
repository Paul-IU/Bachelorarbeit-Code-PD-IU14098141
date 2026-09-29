# Designentscheidung: Die Mauer wandert mit

Sperre, Umleitung und Vergeltung der Mauer beziehen sich auf das jeweils aktuelle
Feld der Wand (neues generisches Effektmerkmal `FieldFollowsSource`, ausgewertet in
`GameState.MoveUnit`). Vorher blieb der Feldeffekt beim Wegziehen auf dem alten
Feld; Angriffe auf Einheiten dort trafen die Wand auch außerhalb der Reichweite
des Angreifers und lösten Vergeltung aus. Begründung und Folgen: Klassenkommentar
von `DefaultHeroes.Wand`. Tests: `MauerWandertMitTests`.

Vorher = `../tod_im_eigenen_zug` (Seed 2026, beide Zufallsagenten). Paarungen ohne
Wand sind bitgenau unverändert (Kontrolle). Details: `auswertung.txt`,
`helden_vergleich.csv`.

| | byActionType | uniform |
|---|---|---|
| Siegquote Wand | 42,73 → 41,83 % (−0,90 pp) | 47,92 → 47,36 % (−0,56 pp) |
| Seed-Rauschen Wand | 0,32 pp | 0,15 pp |
| Kompositionen mit Wand | im Mittel −0,61 pp (−2,10 … +0,81) | im Mittel −0,38 pp (−1,44 … +0,68) |
| Partiedauer mit Wand | 41,90 → 42,33 Runden | 47,36 → 47,57 Runden |
| Eigenbeschuss (Stichprobe, Partien mit Wand) | 24,98 % → 28,16 % | 22,70 % → 24,42 % |

Eigenbeschuss = Angriff eines Verbündeten der Wand, den die Umleitung auf die
eigene Wand lenkt (Vergeltung trifft den eigenen Angreifer). Bestand schon vorher,
nicht Teil dieser Änderung.

Offener Befund (bewusst nicht geändert): die Selbstreduktion halbiert jeden
Schaden an der Wand, nicht nur umgeleitete Treffer.
