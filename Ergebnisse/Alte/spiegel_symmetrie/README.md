# Symmetrie der Spiegelpaarungen: Befund, Behebung, Klärung

Zwei Punkte standen der Symmetrie-Verifikation entgegen. Beide sind hier
untersucht und dokumentiert.

| Punkt | Befund | Stand |
|---|---|---|
| 1. Hugger-Spiegelpaarungen nicht exakt symmetrisch | Fehler in der Sortierung der Fähigkeitsoptionen | **behoben**, Test über alle 84 Kompositionen × 72 Ausgangslagen × 2 Agenten grün |
| 2. Spiegelzeilen des Basislaufs nicht exakt 50 % | kein Fehler, sondern eine andere Eigenschaft als im Test | **geklärt**; Lösungsvorschlag (c) wartet auf Entscheidung |

## Punkt 1: Sortierung in `AbilitySystem.GetLegalAbilityUses`

**Ursache.** Die Optionen wurden nach (Feld, `UnitId`) sortiert. Ein Teamtausch
(`GameSetup.WithSwappedTeams`) vertauscht die Id-Bereiche (Team A 1-3, Team B
101-103). Stehen mehrere Optionen auf demselben Feld, kippt ihre Reihenfolge -
der Zufallsagent wählt per Index und trifft bei gleichem Zufallswert eine
andere Option. Nur beim Hugger tritt das auf: seine Fähigkeit bietet über
`AnyUnit` eigene und gegnerische Einheiten in einer Liste an, und nach dem
Greifen liegen "Lösen" (Schlüssel = eigene Id) und "Greifen" (Schlüssel = Id
des Ziels) auf demselben Feld.

**Belege.** Protokollvergleich Original gegen Teamtausch (mit Id-Zuordnung):
Kontrolle Berserker+Bogenschütze+Elfe 0 von 72 abweichend, Berserker+
Bogenschütze+Hugger 69-72 von 72; in allen 40 ausgewerteten ersten
Abweichungen (je Komposition und Agent) war es eine Umklammerung-Entscheidung,
nie eine Bewegung oder ein Angriff. Mit der Korrektur waren im Versuch alle 84
Spiegelpaarungen über alle 72 Ausgangslagen exakt symmetrisch (vorher 27 bzw.
25 von 84 asymmetrisch, alle mit Hugger).

**Behebung.** Sortierschlüssel (Feld, eigene vor gegnerischen Einheiten, `UnitId`).
Innerhalb eines Teams bleibt die Id-Reihenfolge erhalten, für alle anderen
Fähigkeiten (nur eigene *oder* nur gegnerische Ziele) ist der neue Schlüssel
konstant. Belegt: die 1.596 Paarungen ohne Hugger liefern nach der Korrektur
in beiden Agenten **exakt dieselben Werte wie vorher** (0 Abweichungen), nur
Läufe mit Hugger ändern sich.

**Tests.**
- `AlleSpiegelpaarungenMitAntithetischerPaarbildung_ErgebenExaktGleicheSiegzahlen`
  (früher `...OhneHugger...`): alle 84 Kompositionen, alle 72 Ausgangslagen,
  beide Agenten.
- `GetLegalAbilityUses_HuggerAngeheftetAufDemFeldDesZiels_ReihenfolgeIstUnabhaengigVonDenTeamIds`:
  direkter Regressionstest der Reihenfolge; schlägt ohne die Korrektur fehl
  (erwartet `loesen, greifen`, tatsächlich `greifen, loesen`).
- Determinismus bei gleichem Seed, Reproduzierbarkeit bei 1 und mehreren
  Threads: bestehende Tests unverändert grün (257 Tests). Zusätzlich sind die
  1.596 Nicht-Hugger-Paarungen zweier unabhängiger Läufe (12 Threads) bitgenau
  gleich.

## Punkt 2: Test und Basislauf prüfen verschiedene Eigenschaften

### a) Worin sie sich unterscheiden

| | Symmetrietest | Basislauf (`full-baseline`) |
|---|---|---|
| Paarbildung | `useAntitheticPairing: true`: jede Ausgangslage **zusätzlich** mit denselben Seed und **vertauschten Etiketten** | aus: jede Ausgangslage **einmal**, Seed je Ausgangslage aus (Basis-Seed, Paarung, Ausgangslage) |
| Partien je Spiegelzeile | 144 | 72 |
| Was das Ergebnis aussagt | Namensunabhängigkeit des Regelwerks | Abschneiden von Blüte A gegen Blüte B (Zufallsstichprobe) |

Die vertauschte Partie steht **physisch identisch** da (dieselben Einheiten
auf denselben Feldern, dieselbe beginnende Seite), nur Etiketten und Ids sind
getauscht. Sie ist eine **Dublette**. Gleiche Siege bei Team A und B heißen
daher: das Regelwerk hängt nicht vom Etikett ab (so wurde der Hugger-Fehler
gefunden). Es heißt *nicht*, dass die Aufstellung auf Blüte A gegen dieselbe
Komposition auf Blüte B im Mittel 50 % erreicht.

Folge (gemessen): bei einer **Nicht**-Spiegelpaarung liefert die antithetische
Paarbildung immer exakt 50:50 (Schildträger+Elfe+Berserker gegen Wand+
Bogenschütze+Musiker: ohne 58:14, mit 72:72) - sie hebt jede Aussage auf. Die
frühere Doku nannte sie eine Varianzreduktion für beliebige Paarungen; das war
falsch und ist in `GameSetup.WithSwappedTeams` und
`BatchSimulationRunner.Run` korrigiert.

### b) Warum die Spiegelzeilen des Basislaufs nicht exakt 50 % ergeben

Drei unabhängige Gründe, keiner ist ein Fehler:

1. **Zufall, nicht Determinismus, bestimmt das Ergebnis.** Regeln und Seeds sind
   reproduzierbar, aber die Agenten würfeln: jede der 72 Partien ist eine
   Zufallsziehung. Das Aufzählen aller Ausgangslagen entfernt nur die Varianz der
   *Aufstellung*, nicht die der Agentenentscheidungen. Bei etwa 70 entschiedenen
   Partien ist ein exaktes 36:36 selbst bei perfekt fairem Spiel nur in ca. 9 %
   der Zeilen zu erwarten (gemessen 5-10 von 84 Zeilen, siehe unten).
2. **Keine Ausgangslage hat ihr Gegenstück in der Menge** (geprüft: 0 von 72
   eigenes Gegenstück, 0 von 72 Gegenstück in der Menge). Team A steht immer auf
   den Feldern von Blüte A, Team B auf Blüte B; der Teamtausch (Einheiten bleiben
   stehen) erzeugt eine Aufstellung, die im Basislauf nie vorkommt.
3. **Auch ein Gegenstück in der Menge würde nicht genügen**, denn die Seed-
   Ableitung (`baseSeed·1.000.003 + Paarung·10.007 + Ausgangslage`) gibt jeder
   Ausgangslage einen eigenen Seed. Exakte Kompensation braucht denselben Seed
   für beide Partien; nur die antithetische Paarbildung liefert das - als
   Dublette, siehe a).

Eine geometrische Spiegelung (Blüte A auf Blüte B abbilden) ist keine exakte
Symmetrie der Simulation, weil z.B. Ziel- und Zuglisten nach (q, r) sortiert
werden und eine Spiegelung diese Reihenfolge ändert.

**Statistische Prüfung der Spiegelzeilen** (84 Zeilen, 6.048 Partien je Lauf):

| Agent | Seed | Team A / B / Remis | Anteil A an entschiedenen | z | χ² (Erwartung 84 ± 13) | Zeilen exakt 50:50 |
|---|---|---|---:|---:|---:|---:|
| byActionType | 2026 | 3054 / 2939 / 55 | 50,96 % | +1,49 | 101,9 | 7 |
| byActionType | 2027 | 2984 / 2993 / 71 | 49,92 % | −0,12 | 89,2 | 5 |
| Uniform | 2026 | 2968 / 2984 / 96 | 49,87 % | −0,21 | 91,5 | 10 |
| Uniform | 2027 | 2930 / 3019 / 99 | 49,25 % | −1,15 | 101,1 | 8 |

In allen vier Läufen ist |z| < 2 (mit 50 % vereinbar) und die Streuung der
Zeilen entspricht reiner Binomialstreuung. Es gibt also keinen Hinweis auf einen
Vor- oder Nachteil von Blüte A oder der beginnenden Seite. Nachweisbar wäre eine
Abweichung ab etwa 1,3 Prozentpunkten (2 Standardfehler bei 6.048 Partien).

### c) Vorschlag (noch nicht umgesetzt)

Die Bedingung "exakt 50 %" lässt sich im Basislauf nicht sinnvoll herstellen
(siehe b). Stattdessen zwei getrennte, jeweils ehrlich formulierbare
Eigenschaften, beide **im ausgewerteten Lauf selbst** geprüft:

1. **Exakt: Namensunabhängigkeit jeder Partie.** Neue Option
   `verifyNameIndependence` in `BatchSimulationRunner.Run` (im `full-baseline`
   standardmäßig an): jede der 257.040 Partien wird zusätzlich mit vertauschten
   Etiketten gespielt und verglichen (Sieger gespiegelt, Rundenzahl, Endgrund,
   Rest-HP gespiegelt). Verletzungen werden gezählt und im Lauf ausgegeben. In
   die Ergebnisse gehen weiterhin nur die Originalpartien ein - die CSV-Zahlen
   bleiben also identisch. Der Test ruft **denselben** Prüfcode auf.
   Aussage: "für alle 257.040 Partien ist das Ergebnis vom Team-Etikett
   unabhängig". Das ist stärker als der heutige Test (alle Paarungen statt nur
   der Spiegel). Kosten: etwa doppelte Laufzeit (≈ 6 → 12 Minuten je Agent).
2. **Statistisch: Fairness der Spiegelzeilen.** Am Ende des Laufs wird
   automatisch die Auswertung aus b) ausgegeben und gespeichert (Team A/B/Remis,
   Anteil, z-Wert, χ²). Aussage: "die 84 Spiegelzeilen (6.048 Partien) liegen bei
   50,96 % bzw. 50,57 % (z = 1,49 / 0,88), also mit 50 % vereinbar".

Verworfen: (i) antithetische Paarbildung im Basislauf, auch nur für
Spiegelzeilen - erzeugt exakt 50 %, aber als Tautologie, und macht die Zeile
inhaltsleer; (ii) physische Spiegelung der Aufstellung - keine exakte Symmetrie
der Simulation. Optional zusätzlich: `useAntitheticPairing` bei verschiedenen
Kompositionen mit einer Ausnahme ablehnen, damit die Fußangel (immer 50:50)
nicht versehentlich in eine Auswertung gerät.

## Ergebnisse nach der Korrektur

Bereinigte Siegquote je Held (Prozent). "Δ Korrektur" = nachher − vorher
(gleicher Seed 2026); "Δ Seed" = Seed 2027 − Seed 2026 (nachher), die Größe des
reinen Zufallsrauschens.

**byActionType**

| Held | vorher | nachher | Δ Korrektur | Seed 2027 | Δ Seed |
|---|---:|---:|---:|---:|---:|
| Elfe | 60,70 | 60,66 | −0,04 | 60,53 | −0,13 |
| Elementarmagier | 58,98 | 58,91 | −0,07 | 58,77 | −0,14 |
| Illusionist | 57,87 | 57,90 | +0,03 | 57,79 | −0,11 |
| Berserker | 56,30 | 56,15 | −0,15 | 56,29 | +0,14 |
| Schildträger | 47,62 | 47,56 | −0,06 | 47,54 | −0,02 |
| Bogenschütze | 45,13 | 45,13 | 0,00 | 45,33 | +0,20 |
| Wand | 42,64 | 42,75 | +0,11 | 43,07 | +0,32 |
| Musiker | 40,38 | 40,40 | +0,02 | 40,14 | −0,26 |
| **Hugger** | 40,37 | 40,53 | **+0,16** | 40,54 | +0,01 |

**Uniform**

| Held | vorher | nachher | Δ Korrektur | Seed 2027 | Δ Seed |
|---|---:|---:|---:|---:|---:|
| Elfe | 62,40 | 62,36 | −0,04 | 62,49 | +0,13 |
| Elementarmagier | 57,68 | 57,62 | −0,06 | 57,47 | −0,15 |
| Illusionist | 53,62 | 53,64 | +0,02 | 53,85 | +0,21 |
| Berserker | 53,61 | 53,68 | +0,07 | 53,52 | −0,16 |
| Schildträger | 51,29 | 51,26 | −0,03 | 51,34 | +0,08 |
| Bogenschütze | 49,57 | 49,63 | +0,06 | 49,58 | −0,05 |
| Wand | 47,90 | 47,92 | +0,02 | 47,77 | −0,15 |
| **Hugger** | 38,80 | 38,84 | **+0,04** | 38,74 | −0,10 |
| Musiker | 35,13 | 35,04 | −0,09 | 35,23 | +0,19 |

### Ist die Verzerrung systematisch? Die Erwartung wurde nicht widerlegt

Erwartung: die Korrektur ordnet nur Zufallszahlen anders den Optionen zu, hat
also keinen systematischen Einfluss. (Das gilt sogar exakt: eine zustands-
abhängige Umordnung der Optionen ändert die Verteilung der Zufallsziehung nicht.)
Gemessen:

- **Hero-Ebene:** größte Änderung durch die Korrektur 0,16 Prozentpunkte
  (Hugger, byActionType), Streuung über alle Helden (RMS) 0,09 bzw. 0,05 pp -
  kleiner als das reine Seed-Rauschen (RMS 0,18 bzw. 0,14 pp; größte einzelne
  Seed-Abweichung 0,32 pp). Der Hugger liegt in beiden Agenten leicht über dem
  Vorwert (+0,16 / +0,04), aber innerhalb des Rauschens.
- **Kompositionen mit Hugger (28):** mittlere Änderung +0,113 pp (t = 0,95) bzw.
  +0,030 pp (t = 0,25); RMS 0,63 / 0,61 pp gegenüber 1,10 / 0,89 pp Seed-Rauschen.
  Ohne Hugger (56, nur über gegnerische Hugger-Paarungen betroffen): −0,055 pp
  (t = −1,54) bzw. −0,017 pp (t = −0,44).
- **Direkter Test auf Paarungsebene** (1.568 Paarungen mit Hugger auf genau einer
  Seite, Siege dieser Seite je 72 Partien, nachher − vorher): Mittel +0,119
  (t = 1,36) bzw. +0,033 (t = 0,35). Ein systematischer Effekt zeigte sich hier
  nicht (zum Vergleich der Seed-Wechsel: +0,007 bzw. −0,078).

Kein Wert erreicht |t| = 2. Die leichte positive Tendenz beim Hugger
(byActionType, +0,16 pp / t = 1,36) ist mit Rauschen vereinbar, aber nicht mit
Sicherheit davon zu trennen; ein Effekt unter etwa 0,3 pp ist mit diesen Läufen
nicht auflösbar.

## Für die Arbeit

> Die Simulation ist namensunabhängig: vertauscht man bei gleichem Seed die
> Team-Etiketten und Ids, entsteht exakt das spiegelbildliche Ergebnis. Diese
> Eigenschaft wurde für alle 84 Kompositionen und alle 72 Ausgangslagen mit beiden
> Zufallsagenten geprüft (jeweils exakt gleiche Siegzahlen der Spiegelpaarung) und
> deckte einen Fehler in der Sortierung der Fähigkeitsoptionen auf, der nur den
> Hugger betraf. Sie ist von der Frage zu unterscheiden, ob eine Komposition
> gegen sich selbst im Mittel 50 % erreicht: da die Agenten zufällig entscheiden,
> ist das nur statistisch prüfbar. Die 84 Spiegelzeilen des Basislaufs (6.048
> Partien je Agent) liegen mit z-Werten zwischen −1,2 und +1,5 im Rahmen des
> Zufalls.

## Reproduktion

```
dotnet test -c Release --filter "FullyQualifiedName~AlleSpiegelpaarungen"
dotnet run --project TacticsCore.Demo -c Release -- full-baseline <paarungenCsv> <kompositionenCsv> <heldenCsv> 2026 byactiontype   # bzw. uniform, Seed 2027
python auswertung_spiegel.py
```
