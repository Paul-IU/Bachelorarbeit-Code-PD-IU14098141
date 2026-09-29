#!/usr/bin/env python3
"""Vorher/Nachher der Designentscheidung 'Mauer abschwächen' (beide Zufallsagenten).
Vorher = ../mauer_eigene_identitaet (Seed 2026). Rauschreferenz: Seed 2027 vs 2026 aus ../spiegel_symmetrie."""
import csv, math, os

HIER = os.path.dirname(os.path.abspath(__file__))
ERG = os.path.dirname(HIER)
VOR = os.path.join(ERG, "mauer_eigene_identitaet")
RAUSCH = os.path.join(ERG, "spiegel_symmetrie")

def lade(path, keys):
    with open(path, encoding="utf-8-sig") as f:
        return {tuple(r[k] for k in keys): r for r in csv.DictReader(f, delimiter=";")}

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def rms(xs): return math.sqrt(sum(x * x for x in xs) / len(xs))
def mean(xs): return sum(xs) / len(xs)

def gesamt(p):
    n = sum(int(r["Partien"]) for r in p.values())
    return (sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p.values()) / n,
            sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p.values()) / n)

def dauer_mit(p, held):
    rows = [r for (a, b), r in p.items() if held in a.split("+") or held in b.split("+")]
    n = sum(int(r["Partien"]) for r in rows)
    return (sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in rows) / n,
            sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in rows) / n)

out, zeilen = [], []
for agent in ("byactiontype", "uniform"):
    hv = lade(os.path.join(VOR, f"basislauf_{agent}_helden.csv"), ["Held"])
    hn = lade(os.path.join(HIER, f"basislauf_{agent}_helden.csv"), ["Held"])
    h6 = lade(os.path.join(RAUSCH, f"basislauf_{agent}_seed2026_helden.csv"), ["Held"])
    h7 = lade(os.path.join(RAUSCH, f"basislauf_{agent}_seed2027_helden.csv"), ["Held"])
    pv = lade(os.path.join(VOR, f"basislauf_{agent}_paarungen.csv"), ["TeamA", "TeamB"])
    pn = lade(os.path.join(HIER, f"basislauf_{agent}_paarungen.csv"), ["TeamA", "TeamB"])
    kv = lade(os.path.join(VOR, f"basislauf_{agent}_kompositionen.csv"), ["Komposition"])
    kn = lade(os.path.join(HIER, f"basislauf_{agent}_kompositionen.csv"), ["Komposition"])

    out.append(f"\n=== Agent: {agent} ===")
    # Kontrolle: Paarungen OHNE Wand müssen bitgenau gleich bleiben (die Änderung betrifft nur die Mauer).
    felder = ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden")
    ohne_wand = [k for k in pv if "wand" not in k[0].split("+") and "wand" not in k[1].split("+")]
    abw = sum(1 for k in ohne_wand if any(pv[k][f] != pn[k][f] for f in felder))
    out.append(f"Kontrolle: Paarungen ohne Wand {len(ohne_wand)}, davon abweichend {abw}")
    (rv, lv), (rn, ln) = gesamt(pv), gesamt(pn)
    out.append(f"Partiedauer gesamt: {fmt(rv, 3)} -> {fmt(rn, 3)} Runden; Rundenlimit {fmt(lv * 100)} % -> {fmt(ln * 100)} %")
    (rvw, lvw), (rnw, lnw) = dauer_mit(pv, "wand"), dauer_mit(pn, "wand")
    out.append(f"Partiedauer mit Wand: {fmt(rvw, 3)} -> {fmt(rnw, 3)} Runden; Rundenlimit {fmt(lvw * 100)} % -> {fmt(lnw * 100)} %")

    out.append("")
    out.append(f"{'Held':16}{'vorher':>8}{'nachher':>9}{'Δ pp':>8}{'Seed-Rauschen':>15}")
    dk, ds = [], []
    for (h,), r in sorted(hv.items(), key=lambda kv_: -fl(kv_[1]["Siegquote"])):
        v, n = fl(r["Siegquote"]) * 100, fl(hn[(h,)]["Siegquote"]) * 100
        s = fl(h7[(h,)]["Siegquote"]) * 100 - fl(h6[(h,)]["Siegquote"]) * 100
        dk.append(n - v); ds.append(s)
        out.append(f"{h:16}{fmt(v):>8}{fmt(n):>9}{fmt(n - v):>8}{fmt(s):>15}")
        zeilen.append({"Agent": agent, "Held": h, "Siegquote_vorher": fmt(v), "Siegquote_nachher": fmt(n), "Differenz_pp": fmt(n - v), "Seedrauschen_pp": fmt(s)})
    out.append(f"RMS ueber 9 Helden: Aenderung {fmt(rms(dk))} pp, Seed-Rauschen {fmt(rms(ds))} pp")
    rang = sorted(hn, key=lambda k_: -fl(hn[k_]["Siegquote"]))
    out.append("Rangfolge nachher: " + ", ".join(f"{k_[0]} {fmt(fl(hn[k_]['Siegquote']) * 100)}" for k_ in rang))
    out.append(f"Wand Rang nachher: {rang.index(('wand',)) + 1} von {len(rang)}")
    tanks = ("illusionist", "schildtraeger", "wand")
    for label, h in (("vorher", hv), ("nachher", hn)):
        order = sorted(tanks, key=lambda t: -fl(h[(t,)]["Siegquote"]))
        out.append(f"Tank-Rangfolge {label}: " + " > ".join(f"{t} {fmt(fl(h[(t,)]['Siegquote']) * 100)}" for t in order))

    w0, w1 = hv[("wand",)], hn[("wand",)]
    out.append("Wand je Zug (vorher -> nachher): Bewegungen {} -> {}, Angriffe {} -> {}, Faehigkeit {} -> {}; Partiedauer {} -> {}".format(
        w0["DurchschnittBewegungenProZug"], w1["DurchschnittBewegungenProZug"],
        w0["DurchschnittAngriffeProZug"], w1["DurchschnittAngriffeProZug"],
        w0["DurchschnittFaehigkeitseinsaetzeProZug"], w1["DurchschnittFaehigkeitseinsaetzeProZug"],
        w0["DurchschnittPartiedauer"], w1["DurchschnittPartiedauer"]))

    d_wand = [fl(kn[k]["Siegquote"]) * 100 - fl(r["Siegquote"]) * 100 for k, r in kv.items() if "wand" in k[0].split("+")]
    out.append(f"Kompositionen mit Wand (28): mittlere Aenderung {fmt(mean(d_wand))} pp, min {fmt(min(d_wand))}, max {fmt(max(d_wand))}")
    diffs = sorted(((fl(kn[k]["Siegquote"]) * 100 - fl(r["Siegquote"]) * 100, k[0]) for k, r in kv.items()))
    out.append("Groesste Rueckgaenge: " + "; ".join(f"{k} {fmt(d)}" for d, k in diffs[:3]))
    out.append("Groesste Zuwaechse:   " + "; ".join(f"{k} {fmt(d)}" for d, k in diffs[::-1][:3]))

text = "\n".join(out)
print(text)
with open(os.path.join(HIER, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
with open(os.path.join(HIER, "helden_vergleich.csv"), "w", newline="", encoding="utf-8-sig") as f:
    w = csv.DictWriter(f, fieldnames=list(zeilen[0].keys()), delimiter=";")
    w.writeheader(); w.writerows(zeilen)
