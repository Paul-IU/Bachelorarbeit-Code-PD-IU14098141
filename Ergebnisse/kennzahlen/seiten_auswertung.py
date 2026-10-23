#!/usr/bin/env python3
"""Seitenvorteil: normale gegen vertauschte Basisläufe (Bachelorarbeit).

Aufruf:  python seiten_auswertung.py
Ausgabe: seiten_ergebnisse.md

Normal = bestehender Basislauf Seed 2026 (Team A auf Blüte A). Vertauscht = Basislauf mit Option --swap-sides
(Team A auf Blüte B, sonst alles gleich; Ergebnisse/seiten_vertauscht). Seitenbereinigt = Mittelwert aus beiden;
da beide Läufe je Paarung dieselbe Partienzahl haben, ist das gleich der gepoolten Siegquote. Unentschieden als halber
Sieg, Kompositionen ohne Spiegelpaarung. Top-D (Bradley-Terry) und Top-B wie in kennzahlen.py; seitenbereinigt aus
den zusammengelegten Zählungen beider Läufe (W = Mittel der beiden Matrizen). Agenten ohne vertauschten Lauf werden
übersprungen.
"""
import os

import numpy as np

from gemeinsam import AGENTEN, ERGEBNISSE, GRUNDWERTE, KENNZAHLEN, fmt, helden_quote, komp_quote, kompositionen, lade_csv, spearman
from kennzahlen import G_WERTE, bradley_terry, matrix, nicht_dominiert, top_d

NORMAL = os.path.join(ERGEBNISSE, "basislauf_2026_09_26_partien", "basislauf_{a}_paarungen.csv")
VERTAUSCHT = os.path.join(ERGEBNISSE, "seiten_vertauscht", "basislauf_{a}_vertauscht_paarungen.csv")


def spiegel_team_a(paar):
    """Anteil von Team A an den entschiedenen Partien der 84 Spiegelzeilen und Unentschieden."""
    sp = [r for r in paar if r["TeamA"] == r["TeamB"]]
    sa, sb = sum(int(r["SiegeTeamA"]) for r in sp), sum(int(r["SiegeTeamB"]) for r in sp)
    return 100.0 * sa / (sa + sb), sum(int(r["Unentschieden"]) for r in sp)


def rang(werte):
    return sorted(werte, key=lambda k: -werte[k])


def main():
    agenten = [a for a in AGENTEN if os.path.exists(VERTAUSCHT.format(a=a))]
    paar = {a: {"normal": lade_csv(NORMAL.format(a=a)), "vertauscht": lade_csv(VERTAUSCHT.format(a=a))} for a in agenten}
    komps = kompositionen(paar[agenten[0]]["normal"])
    hq, kq, mat = {}, {}, {}
    for a in agenten:
        for fall in ("normal", "vertauscht"):
            hq[(a, fall)] = helden_quote(paar[a][fall])
            kq[(a, fall)] = komp_quote(paar[a][fall])
            mat[(a, fall)] = matrix(paar[a][fall], komps)
        hq[(a, "bereinigt")] = {h: (hq[(a, "normal")][h] + hq[(a, "vertauscht")][h]) / 2 for h in hq[(a, "normal")]}
        kq[(a, "bereinigt")] = {k: (kq[(a, "normal")][k] + kq[(a, "vertauscht")][k]) / 2 for k in komps}
        (wn, sn, pn), (wv, sv, pv) = mat[(a, "normal")], mat[(a, "vertauscht")]
        mat[(a, "bereinigt")] = ((wn + wv) / 2, sn + sv, pn + pv)
    helden = sorted(hq[(agenten[0], "normal")])

    out = ["# Seitenvorteil: normale und vertauschte Basisläufe", "",
           "Erzeugt von `Ergebnisse/kennzahlen/seiten_auswertung.py`. Normal = Team A auf Blüte A (bestehender Basislauf "
           "Seed 2026), vertauscht = Team A auf Blüte B (Option `--swap-sides`, sonst identisch), seitenbereinigt = Mittel "
           "aus beiden. Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung.", "",
           f"Ausgewertete Agenten: {', '.join(agenten)}" + ("" if len(agenten) == len(AGENTEN) else
           f" (noch ohne vertauschten Lauf: {', '.join(a for a in AGENTEN if a not in agenten)})"), ""]

    # ---- Spiegelzeilen als direkter Nachweis
    out += ["## Spiegelzeilen (84 × 72 Partien)", "",
            "In den Spiegelzeilen spielt dieselbe Komposition gegen sich selbst; ein Abstand von 50 % ist ein Seitenvorteil. "
            "Normal steht Team A auf Blüte A, vertauscht auf Blüte B.", "",
            "| Agent | Team A normal | Team A vertauscht | Blüte A gewinnt (Mittel) | Unentschieden normal / vertauscht |", "|---|---|---|---|---|"]
    for a in agenten:
        n, un = spiegel_team_a(paar[a]["normal"])
        v, uv = spiegel_team_a(paar[a]["vertauscht"])
        out.append(f"| {a} | {fmt(n, 2)} % | {fmt(v, 2)} % | {fmt((n + 100 - v) / 2, 2)} % | {un} / {uv} |")
    out.append("")

    # ---- Helden
    out += ["## Helden: bereinigte Siegquote (%)", ""]
    for a in agenten:
        out += [f"### {a}", "", "| Held | normal | vertauscht | seitenbereinigt | vertauscht − normal | Rang normal | Rang bereinigt |",
                "|---|---|---|---|---|---|---|"]
        rn, rb = rang(hq[(a, "normal")]), rang(hq[(a, "bereinigt")])
        for h in rb:
            out.append(f"| {h} | {fmt(hq[(a, 'normal')][h], 2)} | {fmt(hq[(a, 'vertauscht')][h], 2)} | {fmt(hq[(a, 'bereinigt')][h], 2)} | "
                       f"{fmt(hq[(a, 'vertauscht')][h] - hq[(a, 'normal')][h], 2, True)} | {rn.index(h) + 1} | {rb.index(h) + 1} |")
        out += ["", f"Spearman normal / bereinigt: {fmt(spearman([hq[(a, 'normal')][h] for h in helden], [hq[(a, 'bereinigt')][h] for h in helden]), 3)}; "
                    f"normal / vertauscht: {fmt(spearman([hq[(a, 'normal')][h] for h in helden], [hq[(a, 'vertauscht')][h] for h in helden]), 3)}", ""]

    def korrelationen(werte, schluessel, titel):
        zeilen = [f"| Paar | Spearman normal | Spearman seitenbereinigt | Δ |", "|---|---|---|---|"]
        for i, x in enumerate(agenten):
            for y in agenten[i + 1:]:
                n = spearman([werte[(x, "normal")][k] for k in schluessel], [werte[(y, "normal")][k] for k in schluessel])
                b = spearman([werte[(x, "bereinigt")][k] for k in schluessel], [werte[(y, "bereinigt")][k] for k in schluessel])
                zeilen.append(f"| {x} / {y} | {fmt(n, 3)} | {fmt(b, 3)} | {fmt(b - n, 3, True)} |")
        return [titel, ""] + zeilen + [""]

    if len(agenten) > 1:
        out += korrelationen(hq, helden, "### Spearman der neun Helden zwischen den Agenten")

    # ---- Kompositionen
    out += ["## Kompositionen", "", "| Agent | Spearman normal / bereinigt | größte Änderung durch die Bereinigung | fünf stärkste (bereinigt) |",
            "|---|---|---|---|"]
    for a in agenten:
        d = {k: kq[(a, "bereinigt")][k] - kq[(a, "normal")][k] for k in komps}
        groesste = max(komps, key=lambda k: abs(d[k]))
        out.append(f"| {a} | {fmt(spearman([kq[(a, 'normal')][k] for k in komps], [kq[(a, 'bereinigt')][k] for k in komps]), 3)} | "
                   f"{groesste} {fmt(d[groesste], 2, True)} pp | " +
                   ", ".join(f"{k} {fmt(kq[(a, 'bereinigt')][k], 2)}" for k in rang(kq[(a, 'bereinigt')])[:5]) + " |")
    out.append("")
    if len(agenten) > 1:
        out += korrelationen(kq, komps, "### Spearman der 84 Kompositionen zwischen den Agenten")

    # ---- Top-D und Top-B
    out += ["## Top-D und Top-B", "", "| Agent | Fall | c_top | " + " | ".join(f"Top-D {fmt(g, 2)}" for g in G_WERTE) + " | Top-B |",
            "|---|---|---|" + "---|" * len(G_WERTE) + "---|"]
    for a in agenten:
        for fall in ("normal", "vertauscht", "bereinigt"):
            W, siege, spiele = mat[(a, fall)]
            R = bradley_terry(siege, spiele)
            top, td = top_d(R)
            out.append(f"| {a} | {fall} | {komps[top]} | " + " | ".join(str(td[g]) for g in G_WERTE) + f" | {len(nicht_dominiert(W))} |")
    out.append("")

    # ---- Lage und Streuung der 84 Kompositionen
    out += ["## Kompositionen: Lage und Streuung (seitenbereinigt)", "",
            "SD mit n − 1; in Klammern der Wert des normalen Laufs.", "",
            "| Agent | Mittelwert | SD | kleinster Wert | größter Wert |", "|---|---|---|---|---|"]
    for a in agenten:
        b = np.array([kq[(a, "bereinigt")][k] for k in komps])
        n = np.array([kq[(a, "normal")][k] for k in komps])
        out.append(f"| {a} | {fmt(b.mean(), 2)} % ({fmt(n.mean(), 2)}) | {fmt(b.std(ddof=1), 2)} pp ({fmt(n.std(ddof=1), 2)}) | "
                   f"{fmt(b.min(), 2)} % ({fmt(n.min(), 2)}) | {fmt(b.max(), 2)} % ({fmt(n.max(), 2)}) |")
    out.append("")

    # ---- Grundwertsumme (wie kennzahlen.py, Schritt 5)
    hp = np.array([GRUNDWERTE[h][0] for h in helden], dtype=float)
    dm = np.array([GRUNDWERTE[h][1] for h in helden], dtype=float)
    gws = dict(zip(helden, (hp - hp.mean()) / hp.std() + (dm - dm.mean()) / dm.std()))
    komp_gws = {k: sum(gws[h] for h in k.split("+")) for k in komps}
    out += ["## Grundwertsumme und Siegquote", "",
            "Grundwertsumme wie in kennzahlen.py (Schritt 5): z(Lebenspunkte) + z(Grundschaden) über die neun Helden, "
            "für Kompositionen die Summe ihrer drei Helden.", "",
            "| Agent | Helden normal | Helden seitenbereinigt | Kompositionen normal | Kompositionen seitenbereinigt |",
            "|---|---|---|---|---|"]
    for a in agenten:
        werte = []
        for schluessel, quelle, g in ((helden, hq, gws), (komps, kq, komp_gws)):
            for fall in ("normal", "bereinigt"):
                werte.append(spearman([g[k] for k in schluessel], [quelle[(a, fall)][k] for k in schluessel]))
        out.append(f"| {a} | {fmt(werte[0], 3)} | {fmt(werte[1], 3)} | {fmt(werte[2], 3)} | {fmt(werte[3], 3)} |")
    out.append("")

    with open(os.path.join(KENNZAHLEN, "seiten_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
