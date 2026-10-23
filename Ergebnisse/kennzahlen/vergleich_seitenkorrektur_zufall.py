#!/usr/bin/env python3
"""Zufallsagenten vor und nach der Seitenkorrektur (Bachelorarbeit, Dokumentation).

Aufruf:  python vergleich_seitenkorrektur_zufall.py <ordner_neu> [<ordner_alt>]
         ordner_alt Standard: Ergebnisse/basislauf_2026_09_26_partien, nach der Archivierung
         Ergebnisse/Archiv_vor_Seitenkorrektur/basislauf_2026_09_26_partien
Ausgabe: vergleich_seitenkorrektur_zufall.md

Die Seitenkorrektur ändert zwei Spielregeln an Kantenfällen (Sichtlinie, Reihenfolge der Flächenwirkungen). Bei
den Zufallsagenten ändern sich dadurch einzelne Partien, die Zufallsziehungen (Seeds) bleiben gleich. Verglichen
werden die Basisläufe Seed 2026 alt und neu:
  1. Partie für Partie: wie viele Partien gehen anders aus (Sieger, Endgrund, Runden, Überlebende, Rest-LP)?
  2. Bereinigte Siegquote der Helden (Unentschieden als halber Sieg): Änderung neu − alt im Verhältnis zur
     bisherigen Seed-Streuung (SD über die fünf alten Seeds 2026-2030, n−1).
"""
import csv
import gzip
import os
import sys

import numpy as np

from gemeinsam import ERGEBNISSE, KENNZAHLEN, fmt, helden_quote, lade_csv

AGENTEN = ("byactiontype", "uniform")
ERGEBNIS_SPALTEN = ("Sieger", "Endgrund", "Aufloesung", "Runden", "UeberlebendeTeamA", "UeberlebendeTeamB", "RestHpTeamA", "RestHpTeamB")


def partien(pfad):
    with gzip.open(pfad, "rt", encoding="utf-8-sig", newline="") as f:
        return list(csv.DictReader(f, delimiter=";"))


def alte_seed_laeufe(agent, ordner_alt):
    """Paarungsdateien der alten Seeds 2026-2030 (2026 aus ordner_alt, die übrigen aus seed_<s> bzw. deren Archiv)."""
    pfade = [os.path.join(ordner_alt, f"basislauf_{agent}_paarungen.csv")]
    for s in (2027, 2028, 2029, 2030):
        for wurzel in (ERGEBNISSE, os.path.join(ERGEBNISSE, "Archiv_vor_Seitenkorrektur")):
            p = os.path.join(wurzel, f"seed_{s}", f"basislauf_{agent}_seed{s}_paarungen.csv")
            if os.path.exists(p):
                pfade.append(p)
                break
    return pfade


def main():
    neu_ordner = sys.argv[1]
    alt_ordner = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ERGEBNISSE, "basislauf_2026_09_26_partien")
    out = ["# Zufallsagenten vor und nach der Seitenkorrektur", "",
           "Erzeugt von `Ergebnisse/kennzahlen/vergleich_seitenkorrektur_zufall.py`. Basisläufe Seed 2026, gleiche Seeds, "
           "nur die beiden korrigierten Regelstellen (Sichtlinie, Reihenfolge der Flächenwirkungen) unterscheiden sich.", "",
           "## Partien mit anderem Ausgang", "",
           "| Agent | Partien | anderer Sieger | anderes Ergebnis (Sieger, Endgrund, Runden, Überlebende oder Rest-LP) | davon Paarungen |",
           "|---|---|---|---|---|"]
    helden_tab = []
    for a in AGENTEN:
        alt = partien(os.path.join(alt_ordner, f"basislauf_{a}_partien.csv.gz"))
        neu = partien(os.path.join(neu_ordner, f"basislauf_{a}_partien.csv.gz"))
        assert len(alt) == len(neu) and all(x["Seed"] == y["Seed"] and x["Ausgangslage"] == y["Ausgangslage"] for x, y in zip(alt, neu))
        sieger = sum(x["Sieger"] != y["Sieger"] for x, y in zip(alt, neu))
        anders = [(x, y) for x, y in zip(alt, neu) if any(x[k] != y[k] for k in ERGEBNIS_SPALTEN)]
        paarungen = len({x["Paarung"] for x, _ in anders})
        out.append(f"| {a} | {len(alt):_} | {sieger:_} ({fmt(100 * sieger / len(alt), 2)} %) | {len(anders):_} ({fmt(100 * len(anders) / len(alt), 2)} %) | "
                   f"{paarungen:_} von 3.570 |".replace("_", "."))

        q_neu = helden_quote(lade_csv(os.path.join(neu_ordner, f"basislauf_{a}_paarungen.csv")))
        seeds = [helden_quote(lade_csv(p)) for p in alte_seed_laeufe(a, alt_ordner)]
        q_alt = seeds[0]
        sd = {h: np.std([s[h] for s in seeds], ddof=1) for h in q_alt}
        helden_tab.append((a, len(seeds), q_alt, q_neu, sd))

    out += ["", "## Helden: Änderung der Siegquote im Verhältnis zur bisherigen Seed-Streuung", "",
            "z = (neu − alt) / SD; SD = Standardabweichung über die alten Seeds 2026-2030. Die Differenz zweier "
            "unabhängiger Läufe hätte die Streuung √2 · SD; hier sind die Läufe über die Seeds gekoppelt und weichen "
            "nur an den Kantenfällen ab, die Änderung sollte also deutlich unter √2 · SD liegen.", ""]
    for a, n_seeds, q_alt, q_neu, sd in helden_tab:
        out += [f"### {a} (SD aus {n_seeds} Seeds)", "", "| Held | alt | neu | Δ (pp) | SD (pp) | z |", "|---|---|---|---|---|---|"]
        z = []
        for h in sorted(q_alt, key=lambda h: -q_alt[h]):
            d = q_neu[h] - q_alt[h]
            z.append(d / sd[h])
            out.append(f"| {h} | {fmt(q_alt[h], 2)} | {fmt(q_neu[h], 2)} | {fmt(d, 3, True)} | {fmt(sd[h], 3)} | {fmt(d / sd[h], 2, True)} |")
        innerhalb = sum(abs(x) <= 1 for x in z)
        out += ["", f"Innerhalb einer SD: {innerhalb} von 9; größtes |z| {fmt(max(abs(x) for x in z), 2)}; "
                    f"Rangfolge {'unverändert' if sorted(q_alt, key=lambda h: -q_alt[h]) == sorted(q_neu, key=lambda h: -q_neu[h]) else 'GEÄNDERT'}.", ""]
    with open(os.path.join(KENNZAHLEN, "vergleich_seitenkorrektur_zufall.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
