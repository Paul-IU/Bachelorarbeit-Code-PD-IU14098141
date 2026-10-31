#!/usr/bin/env python3
"""Grundwertsumme der neun Helden mit Rängen (Bachelorarbeit, 8.3.6).

Aufruf:  python grundwertsumme.py
Ausgabe: grundwertsumme.csv (Semikolon, Dezimalkomma)

Berechnung wie in kennzahlen.py (Schritt 5) und werte_neu.py (8.3.6): z-Werte über die neun Helden mit der
Populations-SD, Grundwertsumme = z(Lebenspunkte) + z(Grundschaden). Rang 1 = höchste Grundwertsumme bzw. höchste
bereinigte Siegquote im Basislauf Seed 2026 (Unentschieden als halber Sieg). Sortiert nach der Grundwertsumme.
"""
import csv
import os

import numpy as np

from gemeinsam import AGENTEN, GRUNDWERTE, KENNZAHLEN, finde_laeufe, fmt, helden_quote, lade_csv


def main():
    helden = list(GRUNDWERTE)
    hp = np.array([GRUNDWERTE[h][0] for h in helden], dtype=float)
    dm = np.array([GRUNDWERTE[h][1] for h in helden], dtype=float)
    # Standardabweichung bewusst mit n im Nenner (np.std, ddof=0), nicht n − 1: Die neun Helden sind die vollständige
    # Grundgesamtheit des Katalogs, keine Stichprobe (Formel (5) der Arbeit, Tabelle in 8.3.6: Wand 1,28, Hugger −1,39).
    z_hp = dict(zip(helden, (hp - hp.mean()) / hp.std(ddof=0)))
    z_dm = dict(zip(helden, (dm - dm.mean()) / dm.std(ddof=0)))
    gws = {h: z_hp[h] + z_dm[h] for h in helden}
    reihe = sorted(helden, key=lambda h: -gws[h])
    rang_gws = {h: i + 1 for i, h in enumerate(reihe)}

    laeufe = finde_laeufe()
    rang_quote = {}
    for a in AGENTEN:
        basis = next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026)
        q = helden_quote(lade_csv(basis[4] + "_paarungen.csv"))
        rang_quote[a] = {h: i + 1 for i, h in enumerate(sorted(helden, key=lambda h: -q[h]))}

    with open(os.path.join(KENNZAHLEN, "grundwertsumme.csv"), "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f, delimiter=";")
        w.writerow(["Held", "Lebenspunkte", "Schaden", "Lebenspunkte_standardisiert", "Schaden_standardisiert", "Grundwertsumme",
                    "Rang_Grundwertsumme"] + [f"Rang_Siegquote_{a}" for a in AGENTEN])
        for h in reihe:
            w.writerow([h, GRUNDWERTE[h][0], GRUNDWERTE[h][1], fmt(z_hp[h], 3), fmt(z_dm[h], 3), fmt(gws[h], 3), rang_gws[h]]
                       + [rang_quote[a][h] for a in AGENTEN])
    print(open(os.path.join(KENNZAHLEN, "grundwertsumme.csv"), encoding="utf-8-sig").read())


if __name__ == "__main__":
    main()
