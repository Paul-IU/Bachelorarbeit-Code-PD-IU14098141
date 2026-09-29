#!/usr/bin/env python3
"""Umklammerung-Bonus prozentual (Schritt 1) und Kampflied prozentual (Schritt 2), getrennt gemessen.
Vorher: Zufallsagenten ../hugger_letzter_held, aggressiv ../heuristik_aggressiv.
Schritt 1: ../umklammerung_bonus_prozentual (Kampflied noch absolut). Schritt 2: dieser Ordner.
Seed-Rauschen (nur Zufallsagenten): Seed 2027 vs 2026 aus ../spiegel_symmetrie."""
import csv, math, os

HIER = os.path.dirname(os.path.abspath(__file__))
ERG = os.path.dirname(HIER)
STAENDE = {
    "vorher": {"byactiontype": "hugger_letzter_held", "uniform": "hugger_letzter_held", "aggressive": "heuristik_aggressiv"},
    "schritt1": {a: "umklammerung_bonus_prozentual" for a in ("byactiontype", "uniform", "aggressive")},
    "schritt2": {a: "kampflied_prozentual" for a in ("byactiontype", "uniform", "aggressive")},
}
AGENTEN = ("byactiontype", "uniform", "aggressive")

def lade(path, keys):
    with open(path, encoding="utf-8-sig") as f:
        return {tuple(r[k] for k in keys): r for r in csv.DictReader(f, delimiter=";")}

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def rms(xs): return math.sqrt(sum(x * x for x in xs) / len(xs))

def pfad(stand, agent, art): return os.path.join(ERG, STAENDE[stand][agent], f"basislauf_{agent}_{art}.csv")
def helden(stand, agent): return {k[0]: fl(r["Siegquote"]) * 100 for k, r in lade(pfad(stand, agent, "helden"), ["Held"]).items()}
def paarungen(stand, agent): return lade(pfad(stand, agent, "paarungen"), ["TeamA", "TeamB"])

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

def dauer(p):
    n = sum(int(r["Partien"]) for r in p.values())
    return (sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p.values()) / n,
            sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p.values()) / n)

def kontrolle(p0, p1, held):
    felder = ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden")
    ohne = [k for k in p0 if held not in k[0].split("+") and held not in k[1].split("+")]
    return len(ohne), sum(1 for k in ohne if any(p0[k][f] != p1[k][f] for f in felder))

out = []
for agent in AGENTEN:
    h = {s: helden(s, agent) for s in STAENDE}
    p = {s: paarungen(s, agent) for s in STAENDE}
    out.append(f"\n=== {agent} ===")
    n1, a1 = kontrolle(p["vorher"], p["schritt1"], "hugger")
    n2, a2 = kontrolle(p["schritt1"], p["schritt2"], "musiker")
    out.append(f"Kontrolle Schritt 1: Paarungen ohne Hugger {n1}, abweichend {a1}; Schritt 2: Paarungen ohne Musiker {n2}, abweichend {a2}")
    for s in STAENDE:
        r, l = dauer(p[s]); out.append(f"{s:9}: Partiedauer {fmt(r)} Runden, Rundenlimit {fmt(l * 100)} %")
    rausch = None
    if agent != "aggressive":
        r6 = lade(os.path.join(ERG, "spiegel_symmetrie", f"basislauf_{agent}_seed2026_helden.csv"), ["Held"])
        r7 = lade(os.path.join(ERG, "spiegel_symmetrie", f"basislauf_{agent}_seed2027_helden.csv"), ["Held"])
        rausch = {k[0]: (fl(r7[k]["Siegquote"]) - fl(r6[k]["Siegquote"])) * 100 for k in r6}
    out.append(f"{'Held':16}{'vorher':>9}{'Schritt1':>10}{'Δ1':>8}{'Schritt2':>10}{'Δ2':>8}{'Δ gesamt':>10}" + (f"{'Rauschen':>10}" if rausch else ""))
    for held in sorted(h["schritt2"], key=lambda k: -h["schritt2"][k]):
        v, s1, s2 = h["vorher"][held], h["schritt1"][held], h["schritt2"][held]
        out.append(f"{held:16}{fmt(v):>9}{fmt(s1):>10}{fmt(s1 - v):>8}{fmt(s2):>10}{fmt(s2 - s1):>8}{fmt(s2 - v):>10}" + (f"{fmt(rausch[held]):>10}" if rausch else ""))
    d1 = [h["schritt1"][k] - h["vorher"][k] for k in h["vorher"]]
    d2 = [h["schritt2"][k] - h["schritt1"][k] for k in h["vorher"]]
    out.append(f"RMS Schritt 1 {fmt(rms(d1))} pp, Schritt 2 {fmt(rms(d2))} pp" + (f", Seed-Rauschen {fmt(rms(list(rausch.values())))} pp" if rausch else ""))
    tanks = ("illusionist", "schildtraeger", "wand")
    out.append("Tank-Rangfolge Schritt 2: " + " > ".join(f"{t} {fmt(h['schritt2'][t])}" for t in sorted(tanks, key=lambda t: -h["schritt2"][t])))

out.append("\n=== Rangfolgen und Rangkorrelationen ===")
for s in STAENDE:
    q = {a: helden(s, a) for a in AGENTEN}
    out.append(f"\n{s}:")
    for a in AGENTEN:
        out.append(f"  {a:13}: " + ", ".join(f"{k} {fmt(q[a][k], 1)}" for k in sorted(q[a], key=lambda k: -q[a][k])))
    for x, y in (("byactiontype", "uniform"), ("byactiontype", "aggressive"), ("uniform", "aggressive")):
        out.append(f"  {x} / {y}: Spearman {fmt(spearman(q[x], q[y]), 3)}, Kendall {fmt(kendall(q[x], q[y]), 3)}")

text = "\n".join(out)
print(text)
with open(os.path.join(HIER, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
