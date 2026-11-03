# Struktur der Ergebnisdateien

Stand der Erhebung: Code **f0286a5** (seitensymmetrische Regeln und Id-Vergabe), Ergebnisse aus Commit **41d01a2**.
88 Läufe, jeder mit 3.570 Paarungen (84 Kompositionen, jede gegen jede einschließlich Spiegelpaarung) × 72
Ausgangslagen = 257.040 Partien, Rundenlimit 50. CSV-Format: Trennzeichen `;`, Dezimalkomma, UTF-8; die
Partiedatei enthält nur Ganzzahlen.

## Ablage

```
Ergebnisse/
├── basislauf_2026_09_26_partien/   Basisläufe Seed 2026 der vier Agenten (mit Etiketten-Prüfung)
├── seed_2027/ … seed_2030/         weitere Seeds der Zufallsagenten
├── seiten_vertauscht/              Basisläufe mit vertauschten Seiten (Team A startet auf Blüte B)
├── dmg/<dmg_prozent>/<held>/       Variante Schaden
├── hp/<hp_prozent>/<held>/         Variante Lebenspunkte
├── ability/<ability_prozent>/<held>/  Variante Fähigkeitsstärke
├── kennzahlen/                     Auswertungsskripte und Auswertungen
├── Archiv_vor_Seitenkorrektur/     erste vollständige Erhebung
├── Archiv_vor_Idkorrektur/         zweite vollständige Erhebung
├── Alte/                           Läufe aus der Entwicklung
└── Test/                           Testvariante zur Heilung der Elfe
```

## Benennungsschema

Jede Datei eines Laufs heißt **Laufname + Agent + Dateiart**.

| Lauf | Schema | Beispiel |
|---|---|---|
| Basislauf (Seed 2026) | `basislauf_<agent>_<dateiart>` | `basislauf_defensive_helden.csv` |
| weiterer Seed | `basislauf_<agent>_seed<seed>_<dateiart>` | `basislauf_uniform_seed2028_paarungen.csv` |
| vertauschte Seiten | `basislauf_<agent>_vertauscht_<dateiart>` | `basislauf_aggressive_vertauscht_partien.csv.gz` |
| Variante | `<held>_<attribut>_<prozent>_<agent>_<dateiart>` | `schildtraeger_dmg_-2.5_aggressive_partien.csv.gz` |

- `<agent>`: `byactiontype`, `uniform`, `aggressive`, `defensive`
- `<attribut>`: `dmg` (Schaden), `hp` (Lebenspunkte), `ability` (Fähigkeitsstärke)
- `<prozent>`: Änderung mit Vorzeichen und Dezimalpunkt (10, 5, 2.5, -2.5, -5, -10)

| Dateiart | Inhalt |
|---|---|
| `helden.csv`, `kompositionen.csv`, `paarungen.csv`, `partien.csv.gz` | Ergebnisdaten |
| `paarungen.fortschritt.txt` | Fortschrittsanzeige |
| `lauf.log` | Konsolenprotokoll |
| `lauf.err.log` | Fehlerprotokoll (nur lokale Läufe, leer) |
| `maschine.txt` | Rechner und Laufzeit (nur Serverläufe von aggressive und defensive) |
| `auswertung.txt` | Kurzvergleich mit dem Basislauf (nur Varianten der heuristischen Agenten) |

## Heldendatei (`…_helden.csv`) – eine Zeile je Held (9 Zeilen)

Außer `PartienBeideSeiten` und den Spalten `…GesamtVerzerrt` zählen nur Partien, in denen der Held auf **genau
einer** Seite steht. Unentschieden zählen hier nicht als halber Sieg; das geschieht erst in der Auswertung.

| Spalte | Bedeutung |
|---|---|
| Held, Rolle | Held; Rolle Tank, Dmg oder Support |
| Partien, Siege, Siegquote | bereinigte Siegquote |
| RundenAmZug | eigene Züge, zu deren Beginn der Held lebte |
| Bewegungen, Angriffe, Faehigkeitseinsaetze | ausgeführte Aktionen (Summe), dazu dieselben Werte je Zug |
| VerfuegbareZuegeFuerFaehigkeit | Züge, in denen die Fähigkeit nicht abklang |
| PartienBeideSeiten | Partien mit dem Helden auf beiden Seiten |
| …GesamtVerzerrt | rohe Werte einschließlich dieser Partien (Erwartung bei Gleichgewicht 60,28 %) |
| PartienRundenlimit, AnteilRundenlimit | Partien, die am Rundenlimit endeten |
| RundenlimitMehrHelden, …MehrRestHp, …Unentschieden | wie diese entschieden wurden (Stufe a, b, c) |
| Agent | verwendeter Agent |

## Kompositionsdatei (`…_kompositionen.csv`) – eine Zeile je Komposition (84 Zeilen)

| Spalte | Bedeutung |
|---|---|
| Komposition | die drei Helden, alphabetisch, mit `+` verbunden |
| Partien | 6.048 (83 Gegner × 72 Partien und einmal die Spiegelpaarung) |
| Siege, Siegquote | einschließlich Spiegelpaarung; die Auswertung lässt sie weg |
| Agent | verwendeter Agent |

## Paarungsdatei (`…_paarungen.csv`) – eine Zeile je Paarung (3.570 Zeilen)

| Spalte | Bedeutung |
|---|---|
| TeamA, TeamB | die beiden Kompositionen |
| Partien | 72 |
| SiegeTeamA, SiegeTeamB, Unentschieden | Ausgang der Partien |
| SiegquoteTeamA, SiegquoteTeamB | Siege / Partien |
| DurchschnittRunden | mittlere Rundenzahl |
| MaxHp…, DurchschnittRestHp…, DurchschnittRestHpAnteil… | Lebenspunkte zu Beginn, im Mittel am Ende und als Anteil, je Team |
| AnteilRundenlimit | Anteil der Partien am Rundenlimit |
| Agent | verwendeter Agent |

## Partiedatei (`…_partien.csv.gz`) – eine Zeile je Partie (257.040 Zeilen)

Fest sortiert nach Paarung und Ausgangslage, unabhängig von der Zahl der Threads.

| Spalte | Bedeutung |
|---|---|
| Paarung | Zeilennummer in der Paarungsdatei (ab 0, ohne Kopfzeile) |
| TeamA, TeamB | die beiden Kompositionen |
| Ausgangslage | 0–71 |
| Vertauscht | immer 0; die Prüfpartien der Etiketten-Prüfung werden nur verglichen, nicht gespeichert |
| Seed | Startwert des Zufalls dieser Partie |
| Startteam | beginnendes Team |
| Sieger | TeamA, TeamB oder Draw |
| Endgrund | Elimination (alle Helden einer Seite besiegt) oder RoundLimit |
| Aufloesung | nur bei Rundenlimit: Stufe a, b oder c |
| Runden | gespielte Runden |
| UeberlebendeTeamA/B, RestHpTeamA/B | lebende Helden und ihre Lebenspunkte am Ende (ohne Klone) |
| Agent | verwendeter Agent |

## Fortschrittsdatei (`…_paarungen.fortschritt.txt`)

Zeile 1: gespielte von geplanten Partien (bei Etiketten-Prüfung „+ ebenso viele Prüfpartien“), Prozent, Paarungen,
vergangene Zeit, geschätzte Restzeit, am Ende „ABGESCHLOSSEN“. Zeile 2: Zeit der letzten Aktualisierung.

## Maschinendatei (`…_maschine.txt`, nur Serverläufe)

| Feld | Bedeutung |
|---|---|
| Maschine | „unbekannt“, weil der Server seinen Typ nicht meldet; Zuordnung (Hetzner CCX33) in `kennzahlen/laufuebersicht.csv` |
| vCPUs, CPU, RAM, Threads | Ausstattung und Zahl der parallel gerechneten Partien |
| Betriebssystem, .NET | Ubuntu- und .NET-Version |
| Git | Code-Stand (f0286a5) |
| Start, Ende, Dauer_s | Laufzeit (Zeiten in UTC) |
| Status | z. B. „abgeschlossen“ |

Für die lokalen Läufe stehen Laufzeit und Maschine nur in `kennzahlen/laufuebersicht.csv`.

## Archive und ältere Ordner

**Archiv_vor_Seitenkorrektur/** – erste vollständige Erhebung (26.09.–12.10.2026), Regeln des Stands eaaf679
(bis 6ab5b39 nur Änderungen ohne Einfluss auf die Ergebnisse). `seiten_vertauscht/` entstand am 20./21.10. mit
6ab5b39, `etiketten_pruefung_heuristik/` am 13.10. Die Gleichstandsregel begünstigte damals eine Seite.

**Archiv_vor_Idkorrektur/** – zweite vollständige Erhebung (88 Läufe, 23.–25.10.2026) mit seitensymmetrischen
Regeln (9bf7b7f, in den Maschinendateien 5040f8f mit gleichem Code), Ids aber noch in absoluter
Startfeld-Reihenfolge. Eingecheckt in e6953bd; Vergleichsstand von `werte_neu.py` und `kapitel8.py`.

**Alte/** – Läufe aus der Entwicklung mit älteren Regel- und Katalogständen, je Unterordner eine
Designentscheidung oder Korrektur mit README (Mauer, Umklammerung, Wirkungsdauer, Tod im eigenen Zug, ×10-Skalierung,
Spiegelsymmetrie, Überarbeitung der Bewertungsfunktion, Schwellen-Agent, vorausschauender Agent) sowie erste
Basisläufe vom 22.–26.09. Nicht mit dem aktuellen Stand vergleichbar.

**Test/** – Testvariante Heilung der Elfe 80/50/20, nur Zufallsagenten, 28.09.2026, Regelstand der ersten
Erhebung. Nicht Teil der 88 Läufe.

**kennzahlen/vor_idkorrektur/**, **kennzahlen/vor_seitenkorrektur/** – keine Läufe, sondern die gesicherten
Auswertungen der beiden Archive.

## kennzahlen/ und die Kapitel der Arbeit

| Kapitel | Dateien |
|---|---|
| 6.1 Partien | `werte_neu.md` (6.1), `kennzahlen_ergebnisse.md` (Schritt 1a), `laufuebersicht.csv`, `zeitleiste.png` |
| 7.1 Schildträger, Aufladen | `werte_neu.md` (7.1) |
| 7.3 Verifikation | `werte_neu.md` (7.3), `seiten_ergebnisse.md`, `spiegel_heuristik.md`, `seed_sd_kompositionen_*.csv`, `vergleich_seitenkorrektur_zufall.md` |
| 8.3.1 Siegquoten, Rangfolgen | `werte_neu.md` (8.3.1), `heldensiegquoten.csv/.xlsx`, `kompositionen_ergebnisse.md` (Teil A), `kennzahlen_ergebnisse.md` (Rangfolgen) |
| 8.3.2 Erkennung | `erkennung_ergebnisse.md`, `werte_neu.md` (8.3.2), `ergebnisse_kapitel8.md` (1 und 4, Erkennungsmatrix), `platzhalter_kapitel8.md` (1), `Abb_8-3-2_Kompositionen.png/.svg`, `abb_8-3-2_daten.csv/.xlsx`, `kompositionen_delta.csv` |
| 8.3.3 Reaktion der Kompositionen | `werte_neu.md` (8.3.3), `platzhalter_kapitel8.md` (2), `kompositionen_ergebnisse.md` (Teil B/C) |
| 8.3.4 Steigung, Verhältnis | `ergebnisse_kapitel8.md` (1–3) |
| 8.3.6 Bradley-Terry, Grundwertsumme | `werte_neu.md` (8.3.6), `grundwertsumme.csv`, `kennzahlen_ergebnisse.md` (Schritte 3–5), `ergebnisse_kapitel8.md` (5) |
| 8.3.7 Simulationsumfang | `teilmengen_ergebnisse.md`, `ergebnisse_kapitel8.md` (4, Simulationsumfang), `erkennung_ergebnisse.md` (Spalte „kleinste Stichprobe“) |
| 8.3 Schildwall | `ergebnisse_kapitel8.md` (6) |
| 9.1.3 Trefferschwellen | `trefferschwellen.md` |

Ohne Kapitelwerte: `matrizen/` (84 × 84-Siegquotenmatrizen je Lauf, Grundlage für Top-D und Top-B) sowie
`vergleich_unentschieden.md` und die `*_vor_unentschieden.md` (Stand vor der Umstellung „Unentschieden als halber
Sieg“ vom 13.10.2026). `werte_neu.md` ist die zentrale Datei: nach Kapiteln geordnet, zu jedem Wert in Klammern
der Wert des Stands e6953bd.
