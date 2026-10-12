#!/usr/bin/env python3
"""Kennzahlen der Bachelorarbeit, Schritte 1, 2, 3, 4 und 5 (Schritt 6: teilmengen.py).

Aufruf:  python kennzahlen.py
Ausgabe: kennzahlen_ergebnisse.md (alle Tabellen) und matrizen/<lauf>.csv (84x84-Matrizen W).

Festlegungen (mit der Autorin/dem Autor abgestimmt am 09.10.2026):
  - Siegquote einer Komposition OHNE Spiegelpaarung (nur Partien gegen andere
    Kompositionen, 5.976 je Komposition); Unentschieden zählen hier nicht als Sieg.
  - Standardabweichung über die fünf Seeds mit n-1.
  - Abweichung von der Erwartung (Schritt 5) als Rangdifferenz.
  - Die Seed-Basisläufe 2027-2030 werden in den Schritten 2-4 mit ausgewertet.
"""
import os

import numpy as np

from gemeinsam import (AGENTEN, GRUNDWERTE, KENNZAHLEN, VERAENDERTER_HELD, basis_von, finde_laeufe, fmt,
                       kompositionen, lade_csv, lauf_name, spearman, zahl)

G_WERTE = (0.01, 0.02, 0.04, 0.08)
SEEDS = (2026, 2027, 2028, 2029, 2030)


# ---------------------------------------------------------------- Schritt 2
def matrix(paarungen, komps):
    """W(c1, c2) = (Siege c1 + 0,5 * Unentschieden) / Partien; W(c2, c1) = 1 - W(c1, c2); Diagonale 0,5.
    Daneben die Rohzählungen für Bradley-Terry (ohne Spiegelpaarung)."""
    idx = {k: i for i, k in enumerate(komps)}
    n = len(komps)
    W = np.full((n, n), 0.5)
    siege = np.zeros((n, n))   # siege[i, j] = Siege von i gegen j (Unentschieden je halb)
    spiele = np.zeros((n, n))  # Partien zwischen i und j
    for r in paarungen:
        i, j = idx[r["TeamA"]], idx[r["TeamB"]]
        if i == j:
            continue  # Spiegelpaarung: Diagonale per Definition 0,5
        sa, sb, d, p = int(r["SiegeTeamA"]), int(r["SiegeTeamB"]), int(r["Unentschieden"]), int(r["Partien"])
        W[i, j] = (sa + 0.5 * d) / p
        W[j, i] = (sb + 0.5 * d) / p
        siege[i, j] += sa + 0.5 * d
        siege[j, i] += sb + 0.5 * d
        spiele[i, j] += p
        spiele[j, i] += p
    return W, siege, spiele


# ---------------------------------------------------------------- Schritt 3
def bradley_terry(siege, spiele, toleranz=1e-12, max_iter=100000):
    """Maximum-Likelihood-Schätzung der Stärken R über den MM-Algorithmus (Hunter 2004):
    R_i <- W_i / sum_j n_ij / (R_i + R_j). Normiert auf geometrisches Mittel 1."""
    R = np.ones(len(siege))
    W_i = siege.sum(axis=1)
    for _ in range(max_iter):
        nenner = (spiele / (R[:, None] + R[None, :])).sum(axis=1)
        neu = W_i / nenner
        neu /= np.exp(np.log(neu).mean())
        if np.max(np.abs(neu - R) / R) < toleranz:
            return neu
        R = neu
    raise RuntimeError("Bradley-Terry konvergiert nicht")


def top_d(R):
    top = int(np.argmax(R))
    p = R / (R + R[top])  # Siegwahrscheinlichkeit gegen c_top nach dem Modell
    return top, {g: int(np.sum(p + g >= 0.5)) for g in G_WERTE}


def bt_guete(R, W):
    """Abweichung zwischen vorhergesagter und gemessener Siegwahrscheinlichkeit über alle 3.486 Paare (ohne Diagonale)."""
    P = R[:, None] / (R[:, None] + R[None, :])
    i, j = np.triu_indices(len(R), k=1)
    d = np.abs(P[i, j] - W[i, j])
    return float(d.mean()), float(np.sqrt((d ** 2).mean())), float(d.max())


# ---------------------------------------------------------------- Schritt 4
def nicht_dominiert(W):
    """c1 dominiert c2, wenn für JEDE Komposition c (einschließlich c1 und c2) W(c1, c) > W(c2, c)."""
    n = len(W)
    dominiert = np.zeros(n, dtype=bool)
    for j in range(n):
        dominiert[j] = np.any(np.all(W > W[j][None, :], axis=1))
    return [i for i in range(n) if not dominiert[i]]


# ---------------------------------------------------------------- Siegquoten
def komp_siegquote_ohne_spiegel(paarungen, komps):
    """Siegquote je Komposition nur gegen andere Kompositionen (ohne Spiegelpaarung)."""
    siege = dict.fromkeys(komps, 0)
    spiele = dict.fromkeys(komps, 0)
    for r in paarungen:
        a, b = r["TeamA"], r["TeamB"]
        if a == b:
            continue
        p = int(r["Partien"])
        siege[a] += int(r["SiegeTeamA"])
        siege[b] += int(r["SiegeTeamB"])
        spiele[a] += p
        spiele[b] += p
    return {k: 100.0 * siege[k] / spiele[k] for k in komps}


def helden_siegquote(praefix):
    return {r["Held"]: zahl(r["Siegquote"]) * 100 for r in lade_csv(praefix + "_helden.csv")}


# ---------------------------------------------------------------- Hauptteil
def main():
    laeufe = finde_laeufe()
    out = ["# Kennzahlen der Bachelorarbeit", "",
           "Erzeugt von `Ergebnisse/kennzahlen/kennzahlen.py`. Prozentwerte in Prozentpunkten (pp).", ""]
    os.makedirs(os.path.join(KENNZAHLEN, "matrizen"), exist_ok=True)

    # ---- Schritt 1a: Partiedateien
    out += ["## Schritt 1a – Partiedateien je Lauf", "", "| Lauf | Partiedatei |", "|---|---|"]
    for l in laeufe:
        out.append(f"| {lauf_name(l)} | {'vorhanden' if os.path.exists(l[4] + '_partien.csv.gz') else 'FEHLT'} |")
    out.append("")

    # ---- Ergebnisse je Lauf (Schritte 2-4)
    erg = {}
    for l in laeufe:
        paar = lade_csv(l[4] + "_paarungen.csv")
        komps = kompositionen(paar)
        W, siege, spiele = matrix(paar, komps)
        np.savetxt(os.path.join(KENNZAHLEN, "matrizen", lauf_name(l) + ".csv"), W, delimiter=";", fmt="%.6f",
                   header=";".join(komps), comments="")
        R = bradley_terry(siege, spiele)
        top, td = top_d(R)
        erg[l] = dict(komps=komps, W=W, R=R, top=komps[top], topd=td, guete=bt_guete(R, W),
                      topb=[komps[i] for i in nicht_dominiert(W)], paar=paar)

    # ---- Schritt 1d: Streuung über fünf Seeds
    out += ["## Schritt 1d – Standardabweichung über fünf Seeds (2026–2030, n−1)", ""]
    for a in ("byactiontype", "uniform"):
        seed_laeufe = [l for l in laeufe if l[0] == a and l[1] == "basis"]
        seeds = sorted(l[3] for l in seed_laeufe)
        if seeds != list(SEEDS):
            out += [f"**{a}:** nur Seeds {seeds} vorhanden – Auswertung übersprungen.", ""]
            continue
        hq = [helden_siegquote(l[4]) for l in sorted(seed_laeufe, key=lambda l: l[3])]
        kq = [komp_siegquote_ohne_spiegel(erg[l]["paar"], erg[l]["komps"]) for l in sorted(seed_laeufe, key=lambda l: l[3])]
        out += [f"### {a}", "", "| Held | " + " | ".join(str(s) for s in SEEDS) + " | Mittel | SD |",
                "|---|" + "---|" * (len(SEEDS) + 2)]
        for h in sorted(hq[0], key=lambda h: -hq[0][h]):
            v = np.array([q[h] for q in hq])
            out.append(f"| {h} | " + " | ".join(fmt(x) for x in v) + f" | {fmt(v.mean())} | {fmt(v.std(ddof=1), 3)} |")
        sd_h = np.array([np.std([q[h] for q in hq], ddof=1) for h in hq[0]])
        sd_k = {k: np.std([q[k] for q in kq], ddof=1) for k in kq[0]}
        sk = np.array(list(sd_k.values()))
        out += ["", f"Helden: mittlere SD {fmt(sd_h.mean(), 3)} pp, größte {fmt(sd_h.max(), 3)} pp.",
                f"Kompositionen (ohne Spiegelpaarung, n = 84): mittlere SD {fmt(sk.mean(), 3)} pp, Median {fmt(np.median(sk), 3)}, "
                f"größte {fmt(sk.max(), 3)} pp ({max(sd_k, key=sd_k.get)}), kleinste {fmt(sk.min(), 3)} pp.", ""]
        with open(os.path.join(KENNZAHLEN, f"seed_sd_kompositionen_{a}.csv"), "w", encoding="utf-8") as f:
            f.write("Komposition;" + ";".join(f"Seed{s}" for s in SEEDS) + ";Mittel;SD\n")
            for k in sorted(sd_k):
                v = [q[k] for q in kq]
                f.write(f"{k};" + ";".join(fmt(x, 4) for x in v) + f";{fmt(np.mean(v), 4)};{fmt(sd_k[k], 4)}\n")
        out += [f"Alle 84 Werte: `seed_sd_kompositionen_{a}.csv`.", ""]

    # ---- Schritt 3: Top-D
    out += ["## Schritt 3 – Top-D (Bradley-Terry, Lin et al.)", "",
            "Top-D(G) = Anzahl Kompositionen c mit R(c)/(R(c)+R(c_top)) + G ≥ 0,5 (c_top zählt mit). "
            "Güte: Abweichung |vorhergesagt − gemessen| über alle 3.486 Paarungen (ohne Spiegel).", "",
            "| Lauf | c_top | Top-D 0,01 | 0,02 | 0,04 | 0,08 | Δ zur Basis (0,01/0,02/0,04/0,08) | MAE | RMS | max |",
            "|---|---|---|---|---|---|---|---|---|---|"]
    for l in laeufe:
        e = erg[l]
        delta = ""
        if l[1] != "basis":
            b = erg[basis_von(l, laeufe)]
            delta = "/".join(f"{e['topd'][g] - b['topd'][g]:+d}" for g in G_WERTE)
        mae, rms, mx = e["guete"]
        out.append(f"| {lauf_name(l)} | {e['top']} | " + " | ".join(str(e["topd"][g]) for g in G_WERTE) +
                   f" | {delta} | {fmt(mae, 4)} | {fmt(rms, 4)} | {fmt(mx, 4)} |")
    out.append("")

    # ---- Schritt 4: Top-B
    out += ["## Schritt 4 – Top-B (nicht dominierte Kompositionen, Proposition 5.8)", "",
            "| Lauf | Top-B | Δ zur Basis | nicht dominierte Kompositionen |", "|---|---|---|---|"]
    for l in laeufe:
        e = erg[l]
        delta = "" if l[1] == "basis" else f"{len(e['topb']) - len(erg[basis_von(l, laeufe)]['topb']):+d}"
        out.append(f"| {lauf_name(l)} | {len(e['topb'])} | {delta} | {', '.join(e['topb'])} |")
    out.append("")

    # ---- Schritt 5: Grundwertsumme
    helden = list(GRUNDWERTE)
    hp = np.array([GRUNDWERTE[h][0] for h in helden], dtype=float)
    dm = np.array([GRUNDWERTE[h][1] for h in helden], dtype=float)
    gws = dict(zip(helden, (hp - hp.mean()) / hp.std() + (dm - dm.mean()) / dm.std()))
    out += ["## Schritt 5 – Grundwertsumme (nur Basisläufe Seed 2026)", "",
            "z-Werte über die neun Helden (Populations-SD; die Wahl n oder n−1 ändert keine Ränge).", "",
            "| Held | Lebenspunkte | Grundschaden | Grundwertsumme |", "|---|---|---|---|"]
    for h in sorted(helden, key=lambda h: -gws[h]):
        out.append(f"| {h} | {GRUNDWERTE[h][0]} | {GRUNDWERTE[h][1]} | {fmt(gws[h], 3, True)} |")
    out += ["", "| Agent | Spearman Helden (n = 9) | Spearman Kompositionen (n = 84) | größte Abweichungen (Rang GWS → Rang Siegquote) |",
            "|---|---|---|---|"]
    for a in AGENTEN:
        l = next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026)
        hq = helden_siegquote(l[4])
        kq = komp_siegquote_ohne_spiegel(erg[l]["paar"], erg[l]["komps"])
        rho_h = spearman([gws[h] for h in helden], [hq[h] for h in helden])
        komp_gws = {k: sum(gws[h] for h in k.split("+")) for k in kq}
        rho_k = spearman([komp_gws[k] for k in kq], [kq[k] for k in kq])
        rang_g = {h: i + 1 for i, h in enumerate(sorted(helden, key=lambda h: -gws[h]))}
        rang_s = {h: i + 1 for i, h in enumerate(sorted(helden, key=lambda h: -hq[h]))}
        abw = sorted(helden, key=lambda h: -abs(rang_g[h] - rang_s[h]))[:3]
        txt = ", ".join(f"{h} {rang_g[h]}→{rang_s[h]} ({rang_g[h] - rang_s[h]:+d})" for h in abw)
        out.append(f"| {a} | {fmt(rho_h, 3)} | {fmt(rho_k, 3)} | {txt} |")
    out += ["", "Rangdifferenz positiv = Held steht nach Siegquote besser, als seine Grundwertsumme erwarten lässt.", ""]

    with open(os.path.join(KENNZAHLEN, "kennzahlen_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
