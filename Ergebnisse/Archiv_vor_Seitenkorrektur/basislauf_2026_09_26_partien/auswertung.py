#!/usr/bin/env python3
"""Basisläufe mit Export je Partie (Rundenlimit 50, Seed 2026).
Wertet aus, was vorliegt: je Agent Siegquoten, Partiedauer, Aktionen, Fähigkeiten,
Rundenlimit je Held samt Auflösungsstufen. Prüft zusätzlich:
  - Partiedatei gegen Paarungsdatei (Summen je Paarung),
  - Übereinstimmung mit dem Lauf ohne Partieexport (../basislauf_2026_09_26):
    Paarungs- und Kompositionsdatei bytegleich, Heldendatei in den bisherigen 21 Spalten gleich.
Liegen mindestens zwei Agenten vor, zusätzlich Rangfolgen und Rangkorrelationen."""
import csv, gzip, io, os
from collections import defaultdict

HIER = os.path.dirname(os.path.abspath(__file__))
VORHER = os.path.join(os.path.dirname(HIER), "basislauf_2026_09_26")
AGENTEN = ("byactiontype", "uniform", "aggressive", "defensive")
FAEHIGKEIT = {"wand": "Mauer", "schildtraeger": "Schildwall", "musiker": "Lied", "elfe": "Heilung",
              "elementarmagier": "Elementarschlag", "hugger": "Umklammerung",
              "illusionist": "Trugbild", "bogenschuetze": "Aufladen", "berserker": "Blutrausch (passiv)"}

def lade(path):
    with open(path, encoding="utf-8-sig") as f:
        return list(csv.DictReader(f, delimiter=";"))

def lade_gz(path):
    with gzip.open(path, "rt", encoding="utf-8") as f:
        return list(csv.DictReader(f, delimiter=";"))

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def raenge(w):
    o = sorted(w, key=lambda k: -w[k]); return {k: i + 1 for i, k in enumerate(o)}
def spearman(a, b):
    ra, rb = raenge(a), raenge(b); n = len(ra)
    return 1 - 6 * sum((ra[k] - rb[k]) ** 2 for k in ra) / (n * (n * n - 1))
def kendall(a, b):
    ks = sorted(a); c = d = 0
    for i in range(len(ks)):
        for j in range(i + 1, len(ks)):
            s = (a[ks[i]] - a[ks[j]]) * (b[ks[i]] - b[ks[j]]); c += s > 0; d += s < 0
    return (c - d) / (len(ks) * (len(ks) - 1) / 2)

def pruefe_partien(paarungen, partien):
    """Summen der Partiedatei je Paarung gegen die Paarungsdatei. Liefert Liste der Abweichungen."""
    je = defaultdict(list)
    for z in partien: je[int(z["Paarung"])].append(z)
    fehler = []
    if len(partien) != sum(int(r["Partien"]) for r in paarungen):
        fehler.append(f"Zeilenzahl {len(partien)} passt nicht zur Summe der Partien")
    for i, r in enumerate(paarungen):
        z = je.get(i, [])
        n = len(z)
        pruef = {
            "Partien": (int(r["Partien"]), n),
            "SiegeTeamA": (int(r["SiegeTeamA"]), sum(x["Sieger"] == "TeamA" for x in z)),
            "SiegeTeamB": (int(r["SiegeTeamB"]), sum(x["Sieger"] == "TeamB" for x in z)),
            "Unentschieden": (int(r["Unentschieden"]), sum(x["Sieger"] == "Draw" for x in z)),
        }
        for k, (soll, ist) in pruef.items():
            if soll != ist: fehler.append(f"Paarung {i} {k}: Datei {soll}, Partien {ist}")
        if z:
            if r["TeamA"] != z[0]["TeamA"] or r["TeamB"] != z[0]["TeamB"]:
                fehler.append(f"Paarung {i}: Kompositionen weichen ab")
            # Mittelwerte: Paarungsdatei auf 4 Nachkommastellen gerundet
            for k, ist in (("DurchschnittRunden", sum(int(x["Runden"]) for x in z) / n),
                           ("AnteilRundenlimit", sum(x["Endgrund"] == "RoundLimit" for x in z) / n),
                           ("DurchschnittRestHpTeamA", sum(int(x["RestHpTeamA"]) for x in z) / n),
                           ("DurchschnittRestHpTeamB", sum(int(x["RestHpTeamB"]) for x in z) / n)):
                if abs(fl(r[k]) - ist) > 0.00006:
                    fehler.append(f"Paarung {i} {k}: Datei {r[k]}, Partien {ist:.6f}")
    return fehler

def vergleiche_vorher(a):
    """Bytegleichheit bzw. Gleichheit der bisherigen Spalten mit dem Lauf ohne Partieexport."""
    zeilen = []
    for art in ("paarungen", "kompositionen"):
        neu, alt = os.path.join(HIER, f"basislauf_{a}_{art}.csv"), os.path.join(VORHER, f"basislauf_{a}_{art}.csv")
        if os.path.exists(alt):
            gleich = open(neu, "rb").read() == open(alt, "rb").read()
            zeilen.append(f"  {art}: {'bytegleich' if gleich else 'ABWEICHUNG'} zum Lauf ohne Partieexport")
    neu, alt = os.path.join(HIER, f"basislauf_{a}_helden.csv"), os.path.join(VORHER, f"basislauf_{a}_helden.csv")
    if os.path.exists(alt):
        n = [l.split(";")[:21] for l in open(neu, encoding="utf-8-sig").read().splitlines()]
        o = [l.split(";")[:21] for l in open(alt, encoding="utf-8-sig").read().splitlines()]
        zeilen.append(f"  helden (bisherige 21 Spalten): {'gleich' if n == o else 'ABWEICHUNG'} zum Lauf ohne Partieexport")
    return zeilen

vorhanden = [a for a in AGENTEN if os.path.exists(os.path.join(HIER, f"basislauf_{a}_helden.csv"))]
out, quoten = [], {}
for a in vorhanden:
    h = {r["Held"]: r for r in lade(os.path.join(HIER, f"basislauf_{a}_helden.csv"))}
    p = lade(os.path.join(HIER, f"basislauf_{a}_paarungen.csv"))
    partien = lade_gz(os.path.join(HIER, f"basislauf_{a}_partien.csv.gz"))
    quoten[a] = {k: fl(r["Siegquote"]) * 100 for k, r in h.items()}
    n = len(partien)
    runden = sum(int(z["Runden"]) for z in partien) / n
    limit = [z for z in partien if z["Endgrund"] == "RoundLimit"]
    stufen = {s: sum(z["Aufloesung"] == s for z in limit) for s in ("SurvivingHeroes", "HitPointFraction", "Draw")}
    tz = sum(int(r["RundenAmZug"]) for r in h.values())
    out.append(f"\n=== {a} ===")
    out.append(f"Partien {n}, Partiedauer {fmt(runden)} Runden, Rundenlimit {fmt(len(limit) / n * 100)} % ({len(limit)})")
    if limit:
        out.append("  Aufloesung am Rundenlimit: " + ", ".join(
            f"{name} {v} ({fmt(v / len(limit) * 100, 1)} %)" for name, v in
            (("a) mehr Helden", stufen["SurvivingHeroes"]), ("b) mehr Rest-HP", stufen["HitPointFraction"]), ("c) unentschieden", stufen["Draw"]))))
    out.append("je Heldenzug: Bewegungen {}, Angriffe {}, Faehigkeiten {}".format(
        fmt(sum(int(r["Bewegungen"]) for r in h.values()) / tz, 3), fmt(sum(int(r["Angriffe"]) for r in h.values()) / tz, 3),
        fmt(sum(int(r["Faehigkeitseinsaetze"]) for r in h.values()) / tz, 3)))
    fehler = pruefe_partien(p, partien)
    out.append(f"Pruefung Partiedatei gegen Paarungsdatei: {'keine Abweichung' if not fehler else f'{len(fehler)} ABWEICHUNGEN'}")
    out.extend("  " + f for f in fehler[:10])
    out.extend(vergleiche_vorher(a))
    out.append(f"{'Held':16}{'Siegquote':>10}{'Rang':>6}{'Limit %':>9}{'a)':>7}{'b)':>7}{'c)':>7}   Faehigkeit je Partie mit Held")
    rg = raenge(quoten[a])
    for held in sorted(quoten[a], key=lambda k: -quoten[a][k]):
        r = h[held]
        out.append(f"{held:16}{fmt(quoten[a][held]):>10}{rg[held]:>6}{fmt(fl(r['AnteilRundenlimit']) * 100, 1):>9}"
                   f"{r['RundenlimitMehrHelden']:>7}{r['RundenlimitMehrRestHp']:>7}{r['RundenlimitUnentschieden']:>7}"
                   f"   {FAEHIGKEIT[held]}: {fmt(int(r['Faehigkeitseinsaetze']) / int(r['PartienGesamtVerzerrt']), 3)}")
    tanks = ("illusionist", "schildtraeger", "wand")
    out.append("Tank-Rangfolge: " + " > ".join(f"{t} {fmt(quoten[a][t])}" for t in sorted(tanks, key=lambda t: -quoten[a][t])))

if len(vorhanden) >= 2:
    out.append("\n=== Rangfolgen und Rangkorrelationen ===")
    for a in vorhanden:
        out.append(f"  {a:13}: " + ", ".join(f"{k} {fmt(quoten[a][k], 1)}" for k in sorted(quoten[a], key=lambda k: -quoten[a][k])))
    for i, x in enumerate(vorhanden):
        for y in vorhanden[i + 1:]:
            out.append(f"  {x} / {y}: Spearman {fmt(spearman(quoten[x], quoten[y]), 3)}, Kendall {fmt(kendall(quoten[x], quoten[y]), 3)}")

text = "\n".join(out)
print(text)
with open(os.path.join(HIER, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
