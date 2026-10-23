#!/usr/bin/env python3
"""Platzhalterwerte und Abbildung B für Kapitel 8.3 (Bachelorarbeit).

Aufruf:  python platzhalter_kapitel8.py
Ausgabe: platzhalter_kapitel8.md, abb_8-3-2_daten.csv, Abb_8-3-2_Kompositionen.png (300 dpi) und .svg

Nur aus vorhandenen Dateien; Werte wie in kompositionen_ergebnisse.md: Siegquote einer Komposition ohne
Spiegelpaarung, Unentschieden als halber Sieg, Basis = Basislauf Seed 2026, Änderung = Variante − Basis (pp).
Reaktion = Änderung in Richtung der Manipulation (bei Minus-Varianten der Rückgang), wie in Teil C.
"""
import csv
import os

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.patches import Patch
from matplotlib.ticker import FuncFormatter

from gemeinsam import KENNZAHLEN, VERAENDERTER_HELD, finde_laeufe, fmt, komp_quote, lade_csv, spearman

MIT, OHNE = "#0072B2", "#999999"
ZOLL = 1 / 2.54


def lade(laeufe, agent, art=None, p=None):
    if art is None:
        l = next(l for l in laeufe if l[0] == agent and l[1] == "basis" and l[3] == 2026)
    else:
        l = next(l for l in laeufe if l[0] == agent and l[1] == art and l[2] == p)
    return komp_quote(lade_csv(l[4] + "_paarungen.csv"))


def main():
    laeufe = finde_laeufe()
    faelle = {  # Schlüssel: (agent, art, stufe)
        ("uniform", "dmg", 5.0): "uniform, Schaden +5 %",
        ("aggressive", "ability", 5.0): "aggressiv, Fähigkeit +5 %",
        ("aggressive", "dmg", -5.0): "aggressiv, Schaden −5 %",
    }
    basis = {a: lade(laeufe, a) for a in {k[0] for k in faelle}}
    aend = {}
    for key in faelle:
        v = lade(laeufe, *key)
        aend[key] = {k: v[k] - basis[key[0]][k] for k in v}
    komps = sorted(basis["uniform"])
    mit = [k for k in komps if VERAENDERTER_HELD in k.split("+")]
    ohne = [k for k in komps if k not in mit]

    def zahl(x, nd=2, vz=True):
        return fmt(x, nd, vz).replace("-", "−")

    out = ["# Platzhalter Kapitel 8.3", "",
           "Erzeugt von `Ergebnisse/kennzahlen/platzhalter_kapitel8.py`. Siegquote einer Komposition ohne Spiegelpaarung, "
           "Unentschieden als halber Sieg, Basis = Basislauf Seed 2026; Änderung = Variante − Basis in Prozentpunkten.", "",
           "## 1 Für Abbildung B und Kapitel 8.3.2", "",
           "| Variante | mittlere Änderung mit Schildträger (28) | mittlere Änderung ohne Schildträger (56) | vollständig getrennt | Überlappung |",
           "|---|---|---|---|---|"]
    for key in (("uniform", "dmg", 5.0), ("aggressive", "ability", 5.0)):
        d, r = aend[key], 1 if key[2] > 0 else -1
        dm, do = np.array([d[k] for k in mit]), np.array([d[k] for k in ohne])
        ueber = int(np.sum(do * r > (dm * r).min()))
        out.append(f"| {faelle[key]} | {zahl(dm.mean())} pp | {zahl(do.mean())} pp | {'ja' if ueber == 0 else 'nein'} | "
                   f"{'–' if ueber == 0 else f'{ueber} von 56 Kompositionen ohne Schildträger liegen über der schwächsten mit Schildträger'} |")
    out += ["", "Überlappung = Kompositionen ohne Schildträger, die stärker in Richtung der Manipulation verschoben sind als die "
            "schwächste Komposition mit Schildträger.", "",
            "## 2 Für Kapitel 8.3.3", "",
            "| Variante | | Komposition | Siegquote Basis | Änderung |", "|---|---|---|---|---|"]
    rhos = []
    for key in (("uniform", "dmg", 5.0), ("aggressive", "dmg", -5.0)):
        d, r = aend[key], 1 if key[2] > 0 else -1
        rang = sorted(mit, key=lambda k: -d[k] * r)
        for label, k in (("am stärksten", rang[0]), ("am schwächsten", rang[-1])):
            out.append(f"| {faelle[key]} | {label} | {k} | {zahl(basis[key[0]][k], 2, False)} % | {zahl(d[k])} pp |")
        rhos.append((faelle[key], spearman([basis[key[0]][k] for k in mit], [d[k] * r for k in mit])))
    out += ["", "| Variante | Spearman Basissiegquote / Reaktion (28 Kompositionen mit Schildträger) |", "|---|---|"]
    out += [f"| {n} | {zahl(rho, 3, False)} |" for n, rho in rhos]
    out += ["", "Reaktion = Änderung in Richtung der Manipulation (bei Schaden −5 % also der Rückgang); „am stärksten“ und "
            "„am schwächsten“ beziehen sich darauf, die Spalte „Änderung“ zeigt die Änderung mit Vorzeichen. Negativer "
            "Spearman: schwache Kompositionen reagieren stärker.", "",
            "## 3 Abbildung B", "",
            "`Abb_8-3-2_Kompositionen.png` (300 dpi) und `.svg`; Werte in `abb_8-3-2_daten.csv`.", ""]
    with open(os.path.join(KENNZAHLEN, "platzhalter_kapitel8.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))

    # ---- Daten der Abbildung
    felder = [("uniform", "dmg", 5.0), ("aggressive", "ability", 5.0)]
    with open(os.path.join(KENNZAHLEN, "abb_8-3-2_daten.csv"), "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter=";")
        w.writerow(["feld", "agent", "variante", "rang", "komposition", "mit_schildtraeger", "siegquote_basis", "delta_pp"])
        for key in felder:
            for i, k in enumerate(sorted(komps, key=lambda k: -aend[key][k]), 1):
                w.writerow([faelle[key], key[0], f"{key[1]}_{key[2]:+g}", i, k, int(k in mit),
                            f"{basis[key[0]][k]:.6f}", f"{aend[key][k]:.6f}"])

    # ---- Abbildung
    plt.rcParams.update({"font.family": "Arial", "font.size": 9, "axes.unicode_minus": True,
                         "svg.fonttype": "none"})
    fig, achsen = plt.subplots(1, 2, figsize=(17 * ZOLL, 7.5 * ZOLL), sharey=True)
    for ax, key in zip(achsen, felder):
        reihe = sorted(komps, key=lambda k: -aend[key][k])
        werte = [aend[key][k] for k in reihe]
        farben = [MIT if k in mit else OHNE for k in reihe]
        ax.bar(range(len(reihe)), werte, width=0.8, color=farben, linewidth=0)
        ax.axhline(0, color="#333333", linewidth=0.8)
        ax.set_title(faelle[key], fontsize=9)
        ax.set_xticks([])
        ax.set_xlim(-1, len(reihe))
        for s in ("top", "right", "bottom"):
            ax.spines[s].set_visible(False)
        ax.grid(axis="y", color="#e0e0e0", linewidth=0.5)
        ax.set_axisbelow(True)
    achsen[0].set_ylabel("Änderung der Siegquote (pp)")
    achsen[0].yaxis.set_major_formatter(FuncFormatter(lambda y, _: f"{y:.1f}".replace(".", ",").replace("-", "−")))
    fig.legend(handles=[Patch(color=MIT, label="mit Schildträger"), Patch(color=OHNE, label="ohne Schildträger")],
               loc="lower center", ncol=2, frameon=False, fontsize=9)
    fig.tight_layout(rect=(0, 0.08, 1, 1))
    for endung, kw in (("png", {"dpi": 300}), ("svg", {})):
        fig.savefig(os.path.join(KENNZAHLEN, f"Abb_8-3-2_Kompositionen.{endung}"), **kw)


if __name__ == "__main__":
    main()
