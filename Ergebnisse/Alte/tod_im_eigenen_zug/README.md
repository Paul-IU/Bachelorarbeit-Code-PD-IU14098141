# Korrektur: Zug endet bei Tod im eigenen Zug

**Fehler:** Starb eine Einheit in ihrem eigenen Zug durch die Vergeltung der
Mauer (auf einen Angriff oder Fähigkeitsschaden), fragte `MatchController`
ihren Agenten weiter nach der noch offenen Bewegung. Die Bewegung selbst
scheiterte (`MovementSystem.TryMove` lehnt besiegte Einheiten ab), der
Zufallsagent verbrauchte dabei aber Zufallswerte und verschob den Rest der
Partie. In Godot wurden einem Menschen für die tote Einheit noch Bewegungen
angeboten. Tod durch Effekt-Tick war bereits korrekt behandelt.

**Behebung:** `MatchController.ExecuteTurn` beendet den Zug sofort, wenn die
Einheit nach Angriff oder Fähigkeit besiegt ist (Tests: `TodImEigenenZugTests`).

**Häufigkeit vor der Korrektur** (alle 257.040 Partien, Seed 2026): 6.206
Partien (2,41 %) bei byActionType, 3.439 (1,34 %) bei uniform. Heuristische
Agenten waren im Ergebnis nicht betroffen (sie nutzen keine Zufallszahlen und
eine tote Einheit kann den Zustand nicht verändern).

**Wirkung** (Seed 2026, vorher = `../spiegel_symmetrie`): 1.692 bzw. 1.272
Paarungen ändern sich, weil sich der Zufallsstrom verschiebt. Die bereinigten
Siegquoten ändern sich um höchstens 0,05 Prozentpunkte (RMS 0,02 / 0,01 pp),
also deutlich weniger als das Seed-Rauschen (RMS 0,18 / 0,14 pp). Partiedauer
und Rundenlimit-Anteil bleiben praktisch gleich. Details: `auswertung.txt`.
