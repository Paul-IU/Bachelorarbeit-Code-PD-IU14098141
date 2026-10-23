#!/usr/bin/env python3
"""Vergleich alt/neu für die Umstellung vom 13.10.2026 (Bachelorarbeit).

Alt: Unentschieden zählen bei den Helden nicht als Sieg (Spalte 'Siegquote' der Heldendatei);
     Simulationsumfang nur über das Vorzeichen der Schildträger-Siegquote.
Neu: Unentschieden zählen überall als halber Sieg; Simulationsumfang mit demselben Kriterium (a) UND (b)
     wie der vollständige Lauf.

Aufruf:  python vergleich_unentschieden.py   (nach kennzahlen.py, teilmengen.py, erkennung.py)
Liest die Sicherungen *_vor_unentschieden.md für die alten Erkennungs- und Teilmengenergebnisse.
Ausgabe: vergleich_unentschieden.md
"""
import os
import re

import numpy as np

from gemeinsam import AGENTEN, KENNZAHLEN, VERAENDERTER_HELD, fmt, finde_laeufe, helden_quote, komp_quote, lade_csv, lauf_name, spearman
from kennzahlen import kendall

ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)
ALT, NEU = 0.0, 0.5


def lies_tabelle(datei, muster):
    """Zeilen einer Markdown-Datei, die auf muster passen, als Liste von Zellenlisten."""
    pfad = os.path.join(KENNZAHLEN, datei)
    return [[z.strip() for z in zeile.strip().strip("|").split("|")]
            for zeile in open(pfad, encoding="utf-8") if re.match(muster, zeile)]


def lies_ab(datei):
    """Teilmengen-Übersicht: Lauf -> letzte Spalte ('ab')."""
    ab = {}
    for z in lies_tabelle(datei, r"\| schildtraeger_\S+ \|"):
        ab.setdefault(z[0], "nie" if z[-1] == "nicht erreicht" else z[-1])
    return ab


def lies_erkennung(datei):
    """Einzelwerte: (agent, variante) -> (Δ, in Richtung, dagegen, erfüllt)."""
    return {(z[0], z[1]): (z[2], z[3], z[4], z[7]) for z in lies_tabelle(datei, r"\| (" + "|".join(AGENTEN) + r") \| ")}


def main():
    laeufe = finde_laeufe()
    paar = {l: lade_csv(l[4] + "_paarungen.csv") for l in laeufe}
    q = {(l, w): helden_quote(paar[l], w) for l in laeufe for w in (ALT, NEU)}
    basis = {a: next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026) for a in AGENTEN}
    helden = sorted(q[(basis[AGENTEN[0]], NEU)])
    out = ["# Unentschieden als halber Sieg und einheitliches Kriterium: alt gegen neu", "",
           "Erzeugt von `Ergebnisse/kennzahlen/vergleich_unentschieden.py`. Alt = Unentschieden zählen bei den Helden nicht als Sieg "
           "(Heldendatei); neu = Unentschieden als halber Sieg (wie bisher schon bei den Kompositionen).", ""]

    # ---- 1 Basis-Siegquoten
    out += ["## 1 Basis-Siegquoten der Helden (Seed 2026, %)", ""]
    for a in AGENTEN:
        b = basis[a]
        alt_rang = sorted(helden, key=lambda h: -q[(b, ALT)][h])
        neu_rang = sorted(helden, key=lambda h: -q[(b, NEU)][h])
        out += [f"### {a}", "", "| Held | alt | neu | Δ | Rang alt | Rang neu |", "|---|---|---|---|---|---|"]
        for h in neu_rang:
            out.append(f"| {h} | {fmt(q[(b, ALT)][h])} | {fmt(q[(b, NEU)][h])} | {fmt(q[(b, NEU)][h] - q[(b, ALT)][h], 2, True)} | "
                       f"{alt_rang.index(h) + 1} | {neu_rang.index(h) + 1} |")
        remis = sum(int(r["Unentschieden"]) for r in paar[b]) / sum(int(r["Partien"]) for r in paar[b])
        out += ["", f"Unentschieden-Anteil aller Partien: {fmt(100 * remis, 2)} %; Rangfolge "
                    f"{'unverändert' if alt_rang == neu_rang else 'GEÄNDERT'}.", ""]

    # ---- 2 Rangkorrelationen
    out += ["## 2 Rangkorrelationen der Helden zwischen den Agenten (Seed 2026)", "",
            "| Paar | Spearman alt | Spearman neu | Kendall alt | Kendall neu |", "|---|---|---|---|---|"]
    for i, x in enumerate(AGENTEN):
        for y in AGENTEN[i + 1:]:
            werte = []
            for w in (ALT, NEU):
                vx, vy = [q[(basis[x], w)][h] for h in helden], [q[(basis[y], w)][h] for h in helden]
                werte.append((spearman(vx, vy), kendall(vx, vy)))
            out.append(f"| {x} / {y} | {fmt(werte[0][0], 3)} | {fmt(werte[1][0], 3)} | {fmt(werte[0][1], 3)} | {fmt(werte[1][1], 3)} |")
    out.append("")

    # ---- 3 Seed-Streuung
    out += ["## 3 Streuung über die fünf Seeds (SD, n−1, pp)", ""]
    for a in AGENTEN:
        seeds = sorted((l for l in laeufe if l[0] == a and l[1] == "basis"), key=lambda l: l[3])
        if len(seeds) < 2:
            continue
        out += [f"### {a}", "", "| Held | SD alt | SD neu |", "|---|---|---|"]
        sd = {w: {h: np.std([q[(l, w)][h] for l in seeds], ddof=1) for h in helden} for w in (ALT, NEU)}
        for h in helden:
            out.append(f"| {h} | {fmt(sd[ALT][h], 3)} | {fmt(sd[NEU][h], 3)} |")
        ksd = {}
        for w in (ALT, NEU):
            kq = [komp_quote(paar[l], w) for l in seeds]
            ksd[w] = np.array([np.std([k[x] for k in kq], ddof=1) for x in sorted(kq[0])])
        out += ["", f"Helden: mittlere SD alt {fmt(np.mean(list(sd[ALT].values())), 3)} / neu {fmt(np.mean(list(sd[NEU].values())), 3)} pp, "
                    f"größte alt {fmt(max(sd[ALT].values()), 3)} / neu {fmt(max(sd[NEU].values()), 3)} pp.",
                f"Kompositionen (n = 84, ohne Spiegel): mittlere SD alt {fmt(ksd[ALT].mean(), 3)} / neu {fmt(ksd[NEU].mean(), 3)} pp, "
                f"größte alt {fmt(ksd[ALT].max(), 3)} / neu {fmt(ksd[NEU].max(), 3)} pp.", ""]

    # ---- 4 Δ Schildträger, Erkennung, Simulationsumfang
    erk_alt, erk_neu = lies_erkennung("erkennung_ergebnisse_vor_unentschieden.md"), lies_erkennung("erkennung_ergebnisse.md")
    ab_alt, ab_neu = lies_ab("teilmengen_ergebnisse_vor_unentschieden.md"), lies_ab("teilmengen_ergebnisse.md")
    varianten = {(l[0], l[1], l[2]): l for l in laeufe if l[1] != "basis"}
    out += ["## 4 Δ Schildträger, Erkennung und Simulationsumfang je Variante", "",
            "Δ = bereinigte Siegquote des Schildträgers, Variante − Basis (pp). Erkannt = Kriterium vollständiger Lauf. "
            "ab = kleinste Stichprobe mit ≥ 95 % (alt: nur Vorzeichen von Δ; neu: (a) UND (b)).", "",
            "| Agent | Variante | Δ alt | Δ neu | erkannt alt | erkannt neu | ab alt | ab neu |", "|---|---|---|---|---|---|---|---|"]
    aenderungen = {"vorzeichen": [], "erkannt": [], "ab": []}
    for a in AGENTEN:
        for art in ARTEN:
            for p in STUFEN:
                v = varianten[(a, art, p)]
                name = f"{art} {fmt(p, 1, True).replace(',0', '')} %"
                d = {w: q[(v, w)][VERAENDERTER_HELD] - q[(basis[a], w)][VERAENDERTER_HELD] for w in (ALT, NEU)}
                e_alt, e_neu = erk_alt[(a, name)][3], erk_neu[(a, name)][3]
                x_alt, x_neu = ab_alt.get(lauf_name(v), "?"), ab_neu.get(lauf_name(v), "?")
                if np.sign(d[ALT]) != np.sign(d[NEU]):
                    aenderungen["vorzeichen"].append(f"{a} {name}")
                if e_alt != e_neu:
                    aenderungen["erkannt"].append(f"{a} {name} ({e_alt} → {e_neu})")
                if x_alt != x_neu:
                    aenderungen["ab"].append(f"{a} {name} ({x_alt} → {x_neu})")
                out.append(f"| {a} | {name} | {fmt(d[ALT], 2, True)} | {fmt(d[NEU], 2, True)} | {e_alt} | {e_neu} | {x_alt} | {x_neu} |")
    out += ["", "## 5 Änderungen auf einen Blick", "",
            f"- Vorzeichen von Δ Schildträger gewechselt: {', '.join(aenderungen['vorzeichen']) or 'keine'}",
            f"- Erkennung (vollständiger Lauf) geändert: {', '.join(aenderungen['erkannt']) or 'keine'}",
            f"- Erkannt alt / neu: {sum(e[3] == 'ja' for e in erk_alt.values())} / {sum(e[3] == 'ja' for e in erk_neu.values())} von {len(erk_neu)}",
            f"- Kleinste Stichprobe geändert ({len(aenderungen['ab'])}): {', '.join(aenderungen['ab']) or 'keine'}", ""]
    with open(os.path.join(KENNZAHLEN, "vergleich_unentschieden.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
