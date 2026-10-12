# Struktur der Ergebnisdateien

Stand: 02.10.2026. Alle Läufe: 3.570 Paarungen (84 Kompositionen, jede gegen jede einschließlich
Spiegelpaarung) × 72 Ausgangslagen = 257.040 Partien, Rundenlimit 50, Basis-Seed 2026 (sofern nicht
anders angegeben). CSV-Format: Trennzeichen `;`, Dezimalkomma, UTF-8.

## Ablagestruktur

```
Ergebnisse/
├── basislauf_2026_09_26_partien/        Basisläufe aller vier Agenten (Basiskatalog)
├── seed_2027/                           Basisläufe der Zufallsagenten mit Seed 2027 (Seed-Rauschen)
├── dmg/                                 Variante Grundschaden
│   └── dmg_<prozent>/<held>/            z.B. dmg/dmg_-2.5/schildtraeger/
├── hp/                                  Variante maximale Lebenspunkte
│   └── hp_<prozent>/<held>/
├── ability/                             Variante Fähigkeitsstärke
│   └── ability_<prozent>/<held>/
├── Test/                                Einzelne Testvarianten (z.B. Heilung der Elfe 80/50/20)
└── Alte/                                Läufe früherer Katalogstände (nicht mit dem aktuellen Stand vergleichbar)
```

`<prozent>`: Änderung in Prozent mit Vorzeichen, Dezimalpunkt (10, 5, 2.5, -2.5, -5, -10).

## Benennungsschema

| Lauf | Schema | Beispiel |
|---|---|---|
| Basislauf | `basislauf_<agent>_<dateiart>` | `basislauf_uniform_paarungen.csv` |
| Basislauf Seed 2027 | `basislauf_<agent>_seed2027_<dateiart>` | `basislauf_uniform_seed2027_helden.csv` |
| Variante | `<held>_<art>_<prozent>_<agent>_<dateiart>` | `schildtraeger_dmg_-2.5_aggressive_partien.csv.gz` |

`<agent>`: `byactiontype`, `uniform`, `aggressive`, `defensive`.

`<dateiart>`:

- `paarungen.csv`
- `kompositionen.csv`
- `helden.csv`
- `partien.csv.gz` (gzip-komprimiert)

Daneben liegen Hilfsdateien ohne Ergebnisdaten:

- `…_paarungen.fortschritt.txt`: Fortschrittsanzeige.
- `…_lauf.log`: Konsolenprotokoll.
- `…_auswertung.txt`: Auswertung einer Variante gegen den Basislauf.

## Paarungsdatei (`…_paarungen.csv`) – eine Zeile je Paarung (3.570 Zeilen)

| Spalte | Bedeutung |
|---|---|
| TeamA, TeamB | Kompositionsschlüssel (Helden-Ids, alphabetisch, mit `+` verbunden) |
| Partien | Anzahl Partien der Paarung (72) |
| SiegeTeamA, SiegeTeamB, Unentschieden | Ausgang der Partien |
| SiegquoteTeamA, SiegquoteTeamB | Siege / Partien |
| DurchschnittRunden | mittlere Rundenzahl |
| MaxHpTeamA, MaxHpTeamB | Summe der maximalen Lebenspunkte der drei Helden |
| DurchschnittRestHpTeamA, DurchschnittRestHpTeamB | mittlere Rest-Lebenspunkte der Helden am Partieende |
| DurchschnittRestHpAnteilTeamA, DurchschnittRestHpAnteilTeamB | dieselben als Anteil an MaxHp |
| AnteilRundenlimit | Anteil der Partien, die am Rundenlimit endeten |
| Agent | verwendeter Agent |

## Kompositionsdatei (`…_kompositionen.csv`) – eine Zeile je Komposition (84 Zeilen)

| Spalte | Bedeutung |
|---|---|
| Komposition | Kompositionsschlüssel |
| Partien | Partien der Komposition über alle Paarungen (Spiegelpaarung nur einmal gezählt) |
| Siege | gewonnene Partien |
| Siegquote | Siege / Partien |
| Agent | verwendeter Agent |

## Heldendatei (`…_helden.csv`) – eine Zeile je Held (9 Zeilen, nach Siegquote absteigend)

Außer den drei Spalten "…GesamtVerzerrt" und "PartienBeideSeiten" beziehen sich alle Werte nur
auf Partien, in denen der Held auf **genau einer** Seite steht.

| Spalte | Bedeutung |
|---|---|
| Held, Rolle | Helden-Id, Rolle (Tank/Dmg/Support) |
| Partien, Siege, Siegquote | bereinigte Siegquote |
| RundenAmZug | eigene Züge, zu deren Beginn der Held lebte |
| Bewegungen, Angriffe, Faehigkeitseinsaetze | erfolgreiche Aktionen (Summe); Stehenbleiben zählt als Bewegung |
| VerfuegbareZuegeFuerFaehigkeit | Züge, in denen die Fähigkeit nicht auf Abklingzeit war (passive Fähigkeit: 0) |
| DurchschnittPartiedauer | mittlere Rundenzahl der Partien mit dem Helden |
| DurchschnittRundenAmZug | RundenAmZug / Partien |
| DurchschnittBewegungenProZug, DurchschnittAngriffeProZug, DurchschnittFaehigkeitseinsaetzeProZug | je eigenem Zug |
| DurchschnittFaehigkeitseinsaetzeProVerfuegbaremZug | Fähigkeitseinsätze / verfügbare Züge |
| PartienBeideSeiten | Partien mit dem Helden auf beiden Seiten (nur ausgewiesen) |
| PartienGesamtVerzerrt, SiegeGesamtVerzerrt, SiegquoteGesamtVerzerrt | einschließlich der beidseitigen Partien (neutrale Erwartung 60,28 %) |
| Agent | verwendeter Agent |
| PartienRundenlimit | Partien mit dem Helden, die am Rundenlimit endeten |
| AnteilRundenlimit | PartienRundenlimit / Partien |
| RundenlimitMehrHelden | davon entschieden über Stufe a) mehr lebende Helden |
| RundenlimitMehrRestHp | davon entschieden über Stufe b) höherer Anteil Rest-Lebenspunkte |
| RundenlimitUnentschieden | davon Stufe c) unentschieden |

Die letzten fünf Spalten stehen bewusst hinter `Agent`, damit alle älteren Spalten ihre Position behalten.

## Partiedatei (`…_partien.csv.gz`) – eine Zeile je Partie (257.040 Zeilen)

Reihenfolge: Paarung, dann Ausgangslage – unabhängig von der Threadanzahl. Ganzzahlen und englische
Aufzählungswerte.

| Spalte | Bedeutung |
|---|---|
| Paarung | Index der Paarung (= Zeile der Paarungsdatei, 0-basiert ohne Kopfzeile) |
| TeamA, TeamB | Kompositionsschlüssel |
| Ausgangslage | Index der Ausgangslage (0–71) |
| Vertauscht | 1 = teamvertauschte Zweitpartie (nur im Prüfmodus, sonst immer 0) |
| Seed | Seed der Partie |
| Startteam | beginnendes Team (TeamA, TeamB) |
| Sieger | TeamA, TeamB, Draw |
| Endgrund | Elimination (Auslöschung) oder RoundLimit |
| Aufloesung | nur bei RoundLimit: SurvivingHeroes (Stufe a), HitPointFraction (Stufe b), Draw (Stufe c) |
| Runden | gespielte Runden |
| UeberlebendeTeamA, UeberlebendeTeamB | lebende Helden am Ende (ohne Klone) |
| RestHpTeamA, RestHpTeamB | Rest-Lebenspunkte der Helden am Ende (Maximum: MaxHp in der Paarungsdatei) |
| Agent | verwendeter Agent |

Die Summen der Partiedatei stimmen mit den Aggregaten der Paarungsdatei überein. Das prüfen die
automatisierten Tests in `TacticsCore.Tests/MatchExportTests.cs` und das Auswertungsskript des Basislaufs.
