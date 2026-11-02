# TacticsCore – Code und Ergebnisse zur Bachelorarbeit

TacticsCore ist der Prototyp der Bachelorarbeit „Automatisiertes Playtesting zur Balance-Bewertung in rundenbasierten
1-gegen-1-Spielen mit Teamzusammenstellung“ (Paul Decker, IU Internationale Hochschule, 2026). Das rundenbasierte
Hex-Taktikspiel dient dazu, Ungleichgewichte im Heldenkatalog durch Massensimulation zu untersuchen. Dieses Repository umfasst die Spiellogik, die Agenten (zwei Zufallsagenten und zwei
heuristische Agenten), die Batch-Simulation, die Testsuite, eine Godot-Oberfläche sowie alle Simulationsergebnisse und
Auswertungsskripte.

Ausgewerteter Stand: **f0286a5** (seitensymmetrische Regeln und Id-Vergabe), Ergebnisse aus Commit **41d01a2**.

## Aufbau

| Ordner | Inhalt |
|---|---|
| `TacticsCore/` | Spiellogik als Bibliothek: Spielfeld, Regeln, Helden und Fähigkeiten, Agenten, Batch-Simulation und Export der Ergebnisse. |
| `TacticsCore.Demo/` | Konsolenprogramm, mit dem die Simulationsläufe gestartet werden (Befehl `full-baseline`). |
| `TacticsCore.Tests/` | Automatisierte Testsuite (xUnit, 449 Tests). |
| `TacticsCore.Godot/` | Grafische Oberfläche in Godot zum Spielen und Ansehen von Partien. |
| `vm/` | Skript und Lauflisten für die Läufe auf Cloud-Servern, Referenz-Stichprobe, Partievergleich, Archivierungsskript (siehe `vm/README.md`). |
| `Ergebnisse/` | Alle Simulationsergebnisse und die Auswertung. |

Wichtige Unterordner von `Ergebnisse/`:

- `basislauf_2026_09_26_partien/` – Basisläufe Seed 2026 der vier Agenten
- `seed_2027/` … `seed_2030/` – weitere Seeds der Zufallsagenten
- `seiten_vertauscht/` – Basisläufe mit vertauschten Seiten
- `dmg/`, `hp/`, `ability/` – die 72 Varianten des Schildträgers (Schaden, Lebenspunkte, Fähigkeitsstärke je ±2,5/±5/±10 %)
- `kennzahlen/` – Auswertungsskripte und Auswertungen, u. a. `ergebnisse_kapitel8.md`, `werte_neu.md`, `laufuebersicht.csv`
- `Archiv_vor_Seitenkorrektur/`, `Archiv_vor_Idkorrektur/` – frühere Stände der Erhebung
- `Alte/`, `Test/` – frühere Entwicklungsstände und die Testvariante zur Elfen-Heilung

Jeder Lauf schreibt eine Paarungs-, Kompositions- und Heldendatei (CSV), eine Partiedatei (`*_partien.csv.gz`, eine
Zeile je Partie) und eine Fortschrittsdatei. Die vollständige Liste aller Läufe mit Laufzeit und Maschine steht in
`Ergebnisse/kennzahlen/laufuebersicht.csv`.

## Voraussetzungen

| Komponente | Version |
|---|---|
| .NET-SDK | 8 oder neuer (Zielplattform aller Projekte: `net8.0`; verwendet: 10.0.204 unter Windows, 8.0.132 unter Ubuntu 24.04) |
| Python | 3.12 mit numpy (2.2.5), matplotlib (3.11.2) und openpyxl (3.1.5) |
| Godot | 4.7, C#-Variante – nur für die Oberfläche, nicht für Simulation und Auswertung |

## Befehle

Alle Befehle im obersten Ordner des Repositorys.

**Bauen**
```
dotnet build TacticsCore.Demo -c Release
```

**Testsuite** (lokal ca. 3 h)
```
dotnet test TacticsCore.Tests -c Release
```

**Ein Lauf** – Beispiel: defensiver Agent, Schaden des Schildträgers +5 %, Seed 2026, Ausgabe in den Ordner `neu/`:
```
dotnet TacticsCore.Demo/bin/Release/net8.0/TacticsCore.Demo.dll full-baseline neu/defensive_dmg5_paarungen.csv neu/defensive_dmg5_kompositionen.csv neu/defensive_dmg5_helden.csv 2026 defensive --variant schildtraeger_dmg_5
```
Partie- und Fortschrittsdatei entstehen automatisch daneben. Agenten: `byactiontype`, `uniform`, `aggressive`,
`defensive`. Varianten: `schildtraeger_<dmg|hp|ability>_<prozent>`, ohne `--variant` der Basiskatalog. Weitere Optionen:
`--threads <n>` (Standard 12, ändert die Ergebnisse nicht), `--swap-sides` (Team A startet auf Blüte B),
`--verify-name-independence` (Etiketten-Prüfung, etwa doppelte Laufzeit).

**Alle Auswertungen neu berechnen** – in dieser Reihenfolge, weil spätere Skripte die Ausgaben früherer lesen (zusammen
ca. 7 min):
```
cd Ergebnisse/kennzahlen
python kennzahlen.py
python teilmengen.py
python erkennung.py
python kompositionen.py
python kapitel8.py
python platzhalter_kapitel8.py
python excel_dateien.py
python spiegel_heuristik.py
python seiten_auswertung.py
python grundwertsumme.py
python werte_neu.py
python laufuebersicht.py
python zeitleiste.py
```
`kapitel8.py` und `werte_neu.py` vergleichen zusätzlich mit dem Stand e6953bd und brauchen dafür
`Ergebnisse/Archiv_vor_Idkorrektur/` und `Ergebnisse/kennzahlen/vor_idkorrektur/`.

## Laufzeiten (12 Threads, Windows-Rechner der Arbeit)

| Agent | Lauf | Dauer |
|---|---|---|
| byactiontype, uniform | Variante, Seed, vertauschte Seiten | ca. 5–6 min |
| byactiontype, uniform | Basis mit Etiketten-Prüfung | ca. 10–11 min |
| aggressive | Variante, vertauschte Seiten | ca. 1,9 h |
| aggressive | Basis mit Etiketten-Prüfung | ca. 3,9 h |
| defensive | Variante | ca. 4 h |
| defensive | Basis mit Etiketten-Prüfung | ca. 8 h |

Die Läufe der heuristischen Agenten auf Stand f0286a5 liefen auf drei Hetzner-CCX33 (8 vCPU, 8 Threads): aggressive ca.
0,6 h, defensive ca. 1,4–1,7 h je Lauf.

## Die 88 Läufe der Erhebung (Stand f0286a5)

| Laufart | byactiontype | uniform | aggressive | defensive |
|---|---|---|---|---|
| Basislauf Seed 2026 mit Etiketten-Prüfung | 1 | 1 | 1 | 1 |
| weitere Seeds 2027–2030 | 4 | 4 | – | – |
| Basislauf mit vertauschten Seiten | 1 | 1 | 1 | 1 |
| Varianten | 18 | 18 | 18 | 18 |
| **Summe** | **24** | **24** | **20** | **20** |
| gerechnet auf | lokal | lokal | 3 × Hetzner CCX33 | 3 × Hetzner CCX33 |

Jeder Lauf umfasst 3.570 Paarungen × 72 Ausgangslagen = 257.040 Partien.
