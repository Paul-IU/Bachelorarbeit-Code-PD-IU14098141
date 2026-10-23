#!/usr/bin/env python3
"""Spiegelpaarungen der heuristischen Agenten (Bachelorarbeit).

Aufruf:  python spiegel_heuristik.py
Ausgabe: spiegel_heuristik.md

Wertet die 84 Spiegelzeilen (TeamA == TeamB, je 72 Partien) der bestehenden Basisläufe (Seed 2026)
für aggressive und defensive aus, wie zuvor bei den Zufallsagenten: Anteil von Team A an den
entschiedenen Partien, Spannweite je Zeile, Unentschieden. Getrennt dazu der Anteil der Siege des
beginnenden Teams (aus der Partiedatei, Spalte Startteam). Die Agenten sind deterministisch, deshalb
keine z-Werte: Jede Partie ist durch Ausgangslage und Seed festgelegt, eine Binomialstreuung gibt es nicht.
"""
import csv
import gzip
import os

from gemeinsam import ERGEBNISSE, KENNZAHLEN, fmt, lade_csv

AGENTEN = ("aggressive", "defensive")
ORDNER = os.path.join(ERGEBNISSE, "basislauf_2026_09_26_partien")
AUFFAELLIG = 5.0  # pp Abstand zu 50 %, ab dem ein Hinweis ausgegeben wird


def prozent(z, n):
    return 100.0 * z / n if n else float("nan")


def main():
    out = ["# Spiegelpaarungen der heuristischen Agenten", "",
           "Erzeugt von `Ergebnisse/kennzahlen/spiegel_heuristik.py`. Bestehende Basisläufe Seed 2026, "
           "84 Spiegelzeilen × 72 Ausgangslagen = 6.048 Partien je Agent. Keine z-Werte (deterministische Agenten). "
           f"Hinweis, wenn ein Anteil mehr als {fmt(AUFFAELLIG, 0)} pp von 50 % abweicht.", ""]
    tab = ["| | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    werte = {}
    hinweise = []
    for a in AGENTEN:
        praefix = os.path.join(ORDNER, f"basislauf_{a}")
        spiegel = [r for r in lade_csv(praefix + "_paarungen.csv") if r["TeamA"] == r["TeamB"]]
        sa = sum(int(r["SiegeTeamA"]) for r in spiegel)
        sb = sum(int(r["SiegeTeamB"]) for r in spiegel)
        un = sum(int(r["Unentschieden"]) for r in spiegel)
        zeilen = [(prozent(int(r["SiegeTeamA"]), int(r["SiegeTeamA"]) + int(r["SiegeTeamB"])), r["TeamA"]) for r in spiegel]
        zeilen.sort()
        zeilen_mit_remis = sum(1 for r in spiegel if int(r["Unentschieden"]) > 0)

        # Beginnendes Team: Spiegelpartien und zum Vergleich alle Partien
        st = {"spiegel": [0, 0, 0], "alle": [0, 0, 0]}  # Siege Startteam, Siege anderes Team, Unentschieden
        st_zeile = {}
        with gzip.open(praefix + "_partien.csv.gz", "rt", encoding="utf-8") as f:
            for r in csv.DictReader(f, delimiter=";"):
                gruppen = ("alle", "spiegel") if r["TeamA"] == r["TeamB"] else ("alle",)
                if r["Sieger"] not in ("TeamA", "TeamB"):
                    i = 2
                else:
                    i = 0 if r["Sieger"] == r["Startteam"] else 1
                for g in gruppen:
                    st[g][i] += 1
                if r["TeamA"] == r["TeamB"] and i < 2:
                    z = st_zeile.setdefault(r["TeamA"], [0, 0])
                    z[i] += 1
        st_anteile = sorted((prozent(z[0], z[0] + z[1]), k) for k, z in st_zeile.items())
        w = {
            "Spiegelzeilen / Partien": f"{len(spiegel)} / {sa + sb + un}",
            "Unentschieden": f"{un} ({fmt(prozent(un, sa + sb + un), 2)} %), in {zeilen_mit_remis} Zeilen",
            "Team A an den entschiedenen": f"{sa} / {sa + sb} = **{fmt(prozent(sa, sa + sb), 2)} %**",
            "Team-A-Anteil je Zeile": f"{fmt(zeilen[0][0], 2)} bis {fmt(zeilen[-1][0], 2)} %",
            "Zeilen mit Team A > 50 / = 50 / < 50 %": f"{sum(z > 50 for z, _ in zeilen)} / {sum(z == 50 for z, _ in zeilen)} / {sum(z < 50 for z, _ in zeilen)}",
            "Beginnendes Team, Spiegelpartien (entschiedene)": f"{st['spiegel'][0]} / {st['spiegel'][0] + st['spiegel'][1]} = **{fmt(prozent(st['spiegel'][0], st['spiegel'][0] + st['spiegel'][1]), 2)} %**",
            "Beginnendes Team je Zeile": f"{fmt(st_anteile[0][0], 2)} bis {fmt(st_anteile[-1][0], 2)} %",
            "Beginnendes Team, alle 257.040 Partien (entschiedene, Vergleich)": f"{fmt(prozent(st['alle'][0], st['alle'][0] + st['alle'][1]), 2)} %",
        }
        werte[a] = w
        for name, anteil in (("Team A an den entschiedenen Spiegelpartien", prozent(sa, sa + sb)),
                             ("Siege des beginnenden Teams in den Spiegelpartien", prozent(st["spiegel"][0], st["spiegel"][0] + st["spiegel"][1])),
                             ("Siege des beginnenden Teams über alle Partien", prozent(st["alle"][0], st["alle"][0] + st["alle"][1]))):
            if abs(anteil - 50) > AUFFAELLIG:
                hinweise.append(f"- **{a}:** {name} {fmt(anteil, 2)} % – deutlich abweichend von 50 %.")
        extrem = [f"{k} {fmt(z, 1)} %" for z, k in zeilen[:3]] + [f"{k} {fmt(z, 1)} %" for z, k in zeilen[-3:]]
        hinweise.append(f"- {a}, extremste Spiegelzeilen (Team A): " + ", ".join(extrem))
    for schluessel in werte[AGENTEN[0]]:
        tab.append(f"| {schluessel} | " + " | ".join(werte[a][schluessel] for a in AGENTEN) + " |")
    out += tab + ["", "## Hinweise", ""] + hinweise + [""]

    # Team A steht immer auf Blüte A (AusgangslagenGenerator), und Team A ist in jeder Paarung die alphabetisch
    # frühere Komposition. Ein Vorteil der Blüte A trifft die Helden daher unterschiedlich stark.
    paar = lade_csv(os.path.join(ORDNER, f"basislauf_{AGENTEN[0]}_paarungen.csv"))
    helden = sorted({h for r in paar for h in r["TeamA"].split("+")})
    out += ["## Wie oft steht ein Held auf Blüte A?", "",
            "Team A steht in allen 72 Ausgangslagen auf Blüte A, und Team A ist in jeder Paarung die alphabetisch frühere "
            "Komposition. Anteil der einseitigen Paarungen (Held auf genau einer Seite), in denen der Held auf Blüte A steht:", "",
            "| Held | auf Blüte A |", "|---|---|"]
    for h in helden:
        a = b = 0
        for r in paar:
            in_a, in_b = h in r["TeamA"].split("+"), h in r["TeamB"].split("+")
            if in_a != in_b:
                a += in_a
                b += in_b
        out.append(f"| {h} | {a} / {a + b} = {fmt(prozent(a, a + b), 1)} % |")
    out.append("")
    with open(os.path.join(KENNZAHLEN, "spiegel_heuristik.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
