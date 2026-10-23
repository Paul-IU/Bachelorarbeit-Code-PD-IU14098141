#!/usr/bin/env python3
"""Plausibilitätsprüfung der Option "Seiten vertauschen" an einem Zufallsagenten (Bachelorarbeit).

Aufruf:  python seiten_pruefung.py [agent]      (Standard: byactiontype)
Ausgabe: Konsole

Bei den Zufallsagenten gibt es keinen Seitenvorteil zu erwarten. Mit vertauschten Seiten dürfen sich die Siegquoten
daher nur im Rahmen der Seed-Streuung ändern. Verglichen wird der vertauschte Lauf mit dem normalen Basislauf
(Seed 2026). Maßstab ist die SD über die fünf Seeds 2026–2030 (n−1): Die Differenz zweier unabhängiger Läufe hat die
Streuung √2 · SD, z = Differenz / (√2 · SD). Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung.
Zum Vergleich dieselben Kennwerte für die Seed-Paare 2026/2027 … 2026/2030.
"""
import os
import sys

import numpy as np

from gemeinsam import ERGEBNISSE, finde_laeufe, fmt, helden_quote, komp_quote, lade_csv


def kennwerte(hq_a, hq_b, kq_a, kq_b, sd_h, sd_k):
    zh = np.array([(hq_a[h] - hq_b[h]) / (np.sqrt(2) * sd_h[h]) for h in sorted(sd_h)])
    zk = np.array([(kq_a[k] - kq_b[k]) / (np.sqrt(2) * sd_k[k]) for k in sorted(sd_k)])
    dh = np.array([hq_a[h] - hq_b[h] for h in sorted(sd_h)])
    dk = np.array([kq_a[k] - kq_b[k] for k in sorted(sd_k)])
    return dict(h_max=np.abs(dh).max(), h_rms=np.sqrt(np.mean(dh ** 2)), h_zmax=np.abs(zh).max(), h_chi=np.sum(zh ** 2),
                k_max=np.abs(dk).max(), k_rms=np.sqrt(np.mean(dk ** 2)), k_z196=int(np.sum(np.abs(zk) > 1.96)), k_chi=np.sum(zk ** 2),
                k_mittel=dk.mean())


def main():
    agent = sys.argv[1] if len(sys.argv) > 1 else "byactiontype"
    laeufe = finde_laeufe()
    seeds = sorted((l for l in laeufe if l[0] == agent and l[1] == "basis"), key=lambda l: l[3])
    paar = {l[3]: lade_csv(l[4] + "_paarungen.csv") for l in seeds}
    hq = {s: helden_quote(p) for s, p in paar.items()}
    kq = {s: komp_quote(p) for s, p in paar.items()}
    sd_h = {h: np.std([hq[s][h] for s in hq], ddof=1) for h in hq[2026]}
    sd_k = {k: np.std([kq[s][k] for s in kq], ddof=1) for k in kq[2026]}

    vt = lade_csv(os.path.join(ERGEBNISSE, "seiten_vertauscht", f"basislauf_{agent}_vertauscht_paarungen.csv"))
    hv, kv = helden_quote(vt), komp_quote(vt)

    zeilen = [("vertauscht − normal", kennwerte(hv, hq[2026], kv, kq[2026], sd_h, sd_k))]
    zeilen += [(f"Seed {s} − 2026", kennwerte(hq[s], hq[2026], kq[s], kq[2026], sd_h, sd_k)) for s in sorted(hq) if s != 2026]
    print(f"Agent {agent}: SD über {len(seeds)} Seeds, Helden im Mittel {fmt(np.mean(list(sd_h.values())), 3)} pp, "
          f"Kompositionen {fmt(np.mean(list(sd_k.values())), 3)} pp")
    print("| Vergleich | Helden max |Δ| | Helden RMS | Helden max |z| | Helden Σz² (Erw. 9) | Komp. max |Δ| | Komp. RMS | Komp. |z|>1,96 (Erw. 4,2) | Komp. Σz² (Erw. 84) |")
    print("|---|---|---|---|---|---|---|---|---|")
    for name, w in zeilen:
        print(f"| {name} | {fmt(w['h_max'], 3)} | {fmt(w['h_rms'], 3)} | {fmt(w['h_zmax'], 2)} | {fmt(w['h_chi'], 1)} | "
              f"{fmt(w['k_max'], 2)} | {fmt(w['k_rms'], 3)} | {w['k_z196']} | {fmt(w['k_chi'], 1)} |")
    print()
    print("| Held | normal | vertauscht | Δ | z |")
    print("|---|---|---|---|---|")
    for h in sorted(hv, key=lambda h: -hq[2026][h]):
        d = hv[h] - hq[2026][h]
        print(f"| {h} | {fmt(hq[2026][h], 2)} | {fmt(hv[h], 2)} | {fmt(d, 2, True)} | {fmt(d / (np.sqrt(2) * sd_h[h]), 2, True)} |")


if __name__ == "__main__":
    main()
