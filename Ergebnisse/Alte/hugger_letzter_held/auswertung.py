#!/usr/bin/env python3
"""Vorher/Nachher-Vergleich zweier Basisläufe (beide Zufallsagenten, Seed 2026).
Aufruf: python auswertung.py <vorher-ordner> <nachher-ordner> [held-fuer-kontrolle]
Beide Ordner relativ zu Ergebnisse/. Kontrolle: Paarungen OHNE den genannten Helden
(Standard: hugger) müssen bitgenau gleich bleiben.
Rauschreferenz: Seed 2027 vs 2026 aus ../spiegel_symmetrie."""
import csv, math, os, sys

ERG = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
VOR = os.path.join(ERG, sys.argv[1])
NACH = os.path.join(ERG, sys.argv[2])
HELD = sys.argv[3] if len(sys.argv) > 3 else "hugger"
RAUSCH = os.path.join(ERG, "spiegel_symmetrie")

def lade(path, keys):
    with open(path, encoding="utf-8-sig") as f:
        return {tuple(r[k] for k in keys): r for r in csv.DictReader(f, delimiter=";")}

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def rms(xs): return math.sqrt(sum(x * x for x in xs) / len(xs))

def gesamt(p, filt=lambda k: True):
    rows = [r for k, r in p.items() if filt(k)]
    n = sum(int(r["Partien"]) for r in rows)
    return (sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in rows) / n,
            sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in rows) / n)

out, zeilen = [f"Vorher: {sys.argv[1]}  Nachher: {sys.argv[2]}  Kontrolle: Paarungen ohne '{HELD}'"], []
for agent in ("byactiontype", "uniform"):
    hv = lade(os.path.join(VOR, f"basislauf_{agent}_helden.csv"), ["Held"])
    hn = lade(os.path.join(NACH, f"basislauf_{agent}_helden.csv"), ["Held"])
    h6 = lade(os.path.join(RAUSCH, f"basislauf_{agent}_seed2026_helden.csv"), ["Held"])
    h7 = lade(os.path.join(RAUSCH, f"basislauf_{agent}_seed2027_helden.csv"), ["Held"])
    pv = lade(os.path.join(VOR, f"basislauf_{agent}_paarungen.csv"), ["TeamA", "TeamB"])
    pn = lade(os.path.join(NACH, f"basislauf_{agent}_paarungen.csv"), ["TeamA", "TeamB"])

    out.append(f"\n=== Agent: {agent} ===")
    felder = ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden")
    def mit(k): return HELD in k[0].split("+") or HELD in k[1].split("+")
    ohne = [k for k in pv if not mit(k)]
    abw = sum(1 for k in ohne if any(pv[k][f] != pn[k][f] for f in felder))
    veraendert = sum(1 for k in pv if mit(k) and any(pv[k][f] != pn[k][f] for f in felder))
    out.append(f"Kontrolle: Paarungen ohne {HELD} {len(ohne)}, davon abweichend {abw}; Paarungen mit {HELD} veraendert: {veraendert} von {len(pv) - len(ohne)}")
    (rv, lv), (rn, ln) = gesamt(pv), gesamt(pn)
    out.append(f"Partiedauer gesamt: {fmt(rv, 3)} -> {fmt(rn, 3)} Runden; Rundenlimit {fmt(lv * 100)} % -> {fmt(ln * 100)} %")
    (rvh, lvh), (rnh, lnh) = gesamt(pv, mit), gesamt(pn, mit)
    out.append(f"Partiedauer mit {HELD}: {fmt(rvh, 3)} -> {fmt(rnh, 3)} Runden; Rundenlimit {fmt(lvh * 100)} % -> {fmt(lnh * 100)} %")

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
    tanks = ("illusionist", "schildtraeger", "wand")
    for label, h in (("vorher", hv), ("nachher", hn)):
        order = sorted(tanks, key=lambda t: -fl(h[(t,)]["Siegquote"]))
        out.append(f"Tank-Rangfolge {label}: " + " > ".join(f"{t} {fmt(fl(h[(t,)]['Siegquote']) * 100)}" for t in order))

text = "\n".join(out)
print(text)
name = f"auswertung_{os.path.basename(VOR)}_zu_{os.path.basename(NACH)}"
with open(os.path.join(NACH, name + ".txt"), "w", encoding="utf-8") as f:
    f.write(text)
with open(os.path.join(NACH, name + "_helden.csv"), "w", newline="", encoding="utf-8-sig") as f:
    w = csv.DictWriter(f, fieldnames=list(zeilen[0].keys()), delimiter=";")
    w.writeheader(); w.writerows(zeilen)
