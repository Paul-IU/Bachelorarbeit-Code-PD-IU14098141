#!/usr/bin/env python3
"""Vorher/Nachher der Korrektur 'Zug endet bei Tod im eigenen Zug' (beide Zufallsagenten).
Vorher = ../spiegel_symmetrie (Seed 2026), Rauschreferenz = derselbe Stand mit Seed 2027."""
import csv, math, os

HIER = os.path.dirname(os.path.abspath(__file__))
VOR = os.path.join(os.path.dirname(HIER), "spiegel_symmetrie")

def lade(path, keys):
    with open(path, encoding="utf-8-sig") as f:
        return {tuple(r[k] for k in keys): r for r in csv.DictReader(f, delimiter=";")}

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def rms(xs): return math.sqrt(sum(x * x for x in xs) / len(xs))

def gesamt(p):
    n = sum(int(r["Partien"]) for r in p.values())
    return (sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p.values()) / n,
            sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p.values()) / n)

out = []
for agent in ("byactiontype", "uniform"):
    hv = lade(os.path.join(VOR, f"basislauf_{agent}_seed2026_helden.csv"), ["Held"])
    hr = lade(os.path.join(VOR, f"basislauf_{agent}_seed2027_helden.csv"), ["Held"])
    hn = lade(os.path.join(HIER, f"basislauf_{agent}_helden.csv"), ["Held"])
    pv = lade(os.path.join(VOR, f"basislauf_{agent}_seed2026_paarungen.csv"), ["TeamA", "TeamB"])
    pn = lade(os.path.join(HIER, f"basislauf_{agent}_paarungen.csv"), ["TeamA", "TeamB"])

    out.append(f"\n=== Agent: {agent} ===")
    geaendert = sum(1 for k in pv if any(pv[k][f] != pn[k][f] for f in ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden")))
    out.append(f"Paarungen mit veraendertem Ergebnis: {geaendert} von {len(pv)}")
    (rv, lv), (rn, ln) = gesamt(pv), gesamt(pn)
    out.append(f"Durchschnittliche Partiedauer: vorher {fmt(rv, 3)}  nachher {fmt(rn, 3)}  Differenz {fmt(rn - rv, 3)} Runden")
    out.append(f"Anteil Rundenlimit-Ende:       vorher {fmt(lv * 100)} %  nachher {fmt(ln * 100)} %")
    out.append("")
    out.append(f"{'Held':16}{'vorher':>8}{'nachher':>9}{'Δ Korrektur':>13}{'Δ Seed-Rauschen':>17}")
    dk, ds = [], []
    for (h,), r in sorted(hv.items(), key=lambda kv: -fl(kv[1]["Siegquote"])):
        v, n, s = (fl(x[(h,)]["Siegquote"]) * 100 for x in (hv, hn, hr))
        dk.append(n - v); ds.append(s - v)
        out.append(f"{h:16}{fmt(v):>8}{fmt(n):>9}{fmt(n - v):>13}{fmt(s - v):>17}")
    out.append(f"RMS ueber 9 Helden: Korrektur {fmt(rms(dk))} pp, Seed-Rauschen {fmt(rms(ds))} pp; groesste Aenderung durch Korrektur {fmt(max(dk, key=abs))} pp")

text = "\n".join(out)
print(text)
with open(os.path.join(HIER, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
