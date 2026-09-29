#!/usr/bin/env python3
"""Aggressiver HeuristicAgent (vollständiger Basislauf) im Vergleich zu beiden Zufallsagenten.
Zufallsagenten: ../hugger_letzter_held (gleicher Regelstand; die Bewertungsfunktion betrifft sie nicht)."""
import csv, os

HIER = os.path.dirname(os.path.abspath(__file__))
ERG = os.path.dirname(HIER)
QUELLEN = {
    "byactiontype": os.path.join(ERG, "hugger_letzter_held", "basislauf_byactiontype"),
    "uniform": os.path.join(ERG, "hugger_letzter_held", "basislauf_uniform"),
    "aggressive": os.path.join(HIER, "basislauf_aggressive"),
}
PARTIEN_GESAMT = 257040

def lade(path):
    with open(path, encoding="utf-8-sig") as f:
        return list(csv.DictReader(f, delimiter=";"))

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")

def raenge(werte):
    """Rang 1 = höchster Wert; Gleichstände erhalten den mittleren Rang."""
    ordnung = sorted(werte, key=lambda k: -werte[k])
    r, i = {}, 0
    while i < len(ordnung):
        j = i
        while j + 1 < len(ordnung) and werte[ordnung[j + 1]] == werte[ordnung[i]]:
            j += 1
        for k in ordnung[i:j + 1]:
            r[k] = (i + j) / 2 + 1
        i = j + 1
    return r

def spearman(a, b):
    ra, rb = raenge(a), raenge(b)
    n = len(ra)
    ma, mb = sum(ra.values()) / n, sum(rb.values()) / n
    kov = sum((ra[k] - ma) * (rb[k] - mb) for k in ra)
    va = sum((ra[k] - ma) ** 2 for k in ra) ** 0.5
    vb = sum((rb[k] - mb) ** 2 for k in rb) ** 0.5
    return kov / (va * vb)

def kendall(a, b):
    keys = sorted(a)
    konk = disk = 0
    for i in range(len(keys)):
        for j in range(i + 1, len(keys)):
            s = (a[keys[i]] - a[keys[j]]) * (b[keys[i]] - b[keys[j]])
            konk += s > 0; disk += s < 0
    return (konk - disk) / (len(keys) * (len(keys) - 1) / 2)

out = []
quoten, helden = {}, {}
for agent, prefix in QUELLEN.items():
    h = {r["Held"]: r for r in lade(prefix + "_helden.csv")}
    p = lade(prefix + "_paarungen.csv")
    helden[agent] = h
    quoten[agent] = {k: fl(v["Siegquote"]) for k, v in h.items()}
    n = sum(int(r["Partien"]) for r in p)
    runden = sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p) / n
    limit = sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p) / n
    out.append(f"{agent}: Partiedauer {fmt(runden)} Runden, Rundenlimit {fmt(limit * 100)} %")

out.append("")
out.append("Bereinigte Siegquote je Held (nur Partien, in denen der Held genau EINER Seite angehoert) und Rang:")
out.append(f"{'Held':16}" + "".join(f"{a:>22}" for a in QUELLEN))
ra = {a: raenge(quoten[a]) for a in QUELLEN}
for held in sorted(quoten["aggressive"], key=lambda k: -quoten["aggressive"][k]):
    out.append(f"{held:16}" + "".join(f"{fmt(quoten[a][held] * 100):>15} % ({int(ra[a][held])})" for a in QUELLEN))

out.append("")
for a, b in (("byactiontype", "uniform"), ("byactiontype", "aggressive"), ("uniform", "aggressive")):
    out.append(f"Rangkorrelation {a} / {b}: Spearman rho = {fmt(spearman(quoten[a], quoten[b]), 3)}, Kendall tau = {fmt(kendall(quoten[a], quoten[b]), 3)}")

out.append("")
tanks = ("illusionist", "schildtraeger", "wand")
for a in QUELLEN:
    order = sorted(tanks, key=lambda t: -quoten[a][t])
    out.append(f"Tank-Rangfolge {a}: " + " > ".join(f"{t} {fmt(quoten[a][t] * 100)}" for t in order))

out.append("")
h = helden["aggressive"]
tz = sum(int(r["RundenAmZug"]) for r in h.values())
out.append("Aggressiv, je Heldenzug (alle Helden): Bewegungen {}, Angriffe {}, Faehigkeiten {}".format(
    fmt(sum(int(r["Bewegungen"]) for r in h.values()) / tz, 3),
    fmt(sum(int(r["Angriffe"]) for r in h.values()) / tz, 3),
    fmt(sum(int(r["Faehigkeitseinsaetze"]) for r in h.values()) / tz, 3)))
for held, name in (("wand", "Mauer"), ("schildtraeger", "Schildwall"), ("musiker", "Lied (Kampflied + Marschlied)"), ("elfe", "Heilung")):
    r = h[held]
    out.append(f"  {name}: {fmt(int(r['Faehigkeitseinsaetze']) / PARTIEN_GESAMT, 3)} je Partie (alle Partien), "
               f"{fmt(int(r['Faehigkeitseinsaetze']) / int(r['PartienGesamtVerzerrt']), 3)} je Partie mit {held}")

text = "\n".join(out)
print(text)
with open(os.path.join(HIER, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
