#!/usr/bin/env python3
"""Vergleicht zwei Partiedateien (*_partien.csv.gz) Partie für Partie (Bachelorarbeit, Plattformvergleich).

Aufruf:  python3 vergleiche_partien.py <referenz_partien.csv.gz> <neu_partien.csv.gz>
Ergebnis: Exit-Code 0, wenn alle Partien in allen Spalten identisch sind, sonst 1 (mit den ersten Abweichungen).

Zeilenenden werden nicht verglichen: .NET schreibt unter Windows CRLF, unter Linux LF - der Inhalt ist entscheidend.
Verglichen werden alle Spalten (Paarung, Teams, Ausgangslage, Seed, Startteam, Sieger, Endgrund, Auflösung, Runden,
Überlebende, Rest-Lebenspunkte, Agent).
"""
import csv
import gzip
import sys


def lies(pfad):
    with gzip.open(pfad, "rt", encoding="utf-8-sig", newline="") as f:
        return [tuple(z) for z in csv.reader(f, delimiter=";")]


def main():
    if len(sys.argv) != 3:
        print(__doc__)
        return 2
    ref, neu = lies(sys.argv[1]), lies(sys.argv[2])
    if ref[0] != neu[0]:
        print(f"Kopfzeilen verschieden:\n  {ref[0]}\n  {neu[0]}")
        return 1
    kopf = ref[0]
    abweichungen = []
    for i, (a, b) in enumerate(zip(ref[1:], neu[1:]), 1):
        if a != b:
            spalten = [f"{k}: {x} / {y}" for k, x, y in zip(kopf, a, b) if x != y]
            abweichungen.append(f"Zeile {i} (Paarung {a[0]}, Ausgangslage {a[3]}): " + "; ".join(spalten))
    if len(ref) != len(neu):
        abweichungen.append(f"Zeilenzahl verschieden: {len(ref) - 1} / {len(neu) - 1}")
    if abweichungen:
        print(f"NICHT IDENTISCH: {len(abweichungen)} Abweichung(en) bei {len(ref) - 1} Partien")
        for z in abweichungen[:20]:
            print("  " + z)
        return 1
    print(f"IDENTISCH: alle {len(ref) - 1} Partien stimmen in allen {len(kopf)} Spalten überein.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
