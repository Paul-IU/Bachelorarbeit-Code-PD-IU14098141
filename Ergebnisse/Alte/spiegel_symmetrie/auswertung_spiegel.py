#!/usr/bin/env python3
"""Auswertung nach der Sortier-Korrektur (Hugger): Vorher/Nachher je Held, Rauschreferenz
(Seed 2027) und Test auf systematische Verzerrung. Beide Agenten."""
import csv, math, os, sys

# Projektrelativ: dieses Skript liegt in Ergebnisse/spiegel_symmetrie
NEU = os.path.dirname(os.path.abspath(__file__))              # Stand nach der Sortier-Korrektur (Seeds 2026 und 2027)
VOR = os.path.join(os.path.dirname(NEU), "effektdauer_korrektur")  # Stand davor (Seed 2026)

def pfad_vor(agent, art): return os.path.join(VOR, f"basislauf_{agent}_{art}.csv")
def pfad_neu(agent, art, seed): return os.path.join(NEU, f"basislauf_{agent}_seed{seed}_{art}.csv")

def lade(path, keys):
    rows = {}
    with open(path, encoding="utf-8-sig") as f:
        for r in csv.DictReader(f, delimiter=";"):
            rows[tuple(r[k] for k in keys)] = r
    return rows

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2): return f"{x:.{nd}f}".replace(".", ",")
def mean(xs): return sum(xs) / len(xs)
def sd(xs):
    m = mean(xs); return math.sqrt(sum((x - m) ** 2 for x in xs) / (len(xs) - 1))
def rms(xs): return math.sqrt(sum(x * x for x in xs) / len(xs))

HUGGER = "hugger"
out = []
csv_rows = []

def p(s=""):
    out.append(s)

for agent in ("byactiontype", "uniform"):
    hv = lade(pfad_vor(agent, "helden"), ["Held"])
    h6 = lade(pfad_neu(agent, "helden", 2026), ["Held"])
    h7 = lade(pfad_neu(agent, "helden", 2027), ["Held"])
    pv = lade(pfad_vor(agent, "paarungen"), ["TeamA", "TeamB"])
    p6 = lade(pfad_neu(agent, "paarungen", 2026), ["TeamA", "TeamB"])
    p7 = lade(pfad_neu(agent, "paarungen", 2027), ["TeamA", "TeamB"])
    kv = lade(pfad_vor(agent, "kompositionen"), ["Komposition"])
    k6 = lade(pfad_neu(agent, "kompositionen", 2026), ["Komposition"])
    k7 = lade(pfad_neu(agent, "kompositionen", 2027), ["Komposition"])

    p(f"\n=== Agent: {agent} ===")

    # 1) Pairings ohne Hugger: bit-identisch?
    felder = ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden", "AnteilRundenlimit",
              "DurchschnittRestHpAnteilTeamA", "DurchschnittRestHpAnteilTeamB")
    ohne = [k for k in pv if HUGGER not in k[0].split("+") and HUGGER not in k[1].split("+")]
    mit = [k for k in pv if k not in set(ohne)]
    abw_ohne = [k for k in ohne if any(pv[k][f] != p6[k][f] for f in felder)]
    abw_mit = [k for k in mit if any(pv[k][f] != p6[k][f] for f in ("SiegeTeamA", "SiegeTeamB", "Unentschieden"))]
    p(f"Paarungen ohne Hugger: {len(ohne)}, davon abweichend (Seed 2026 vorher vs nachher): {len(abw_ohne)}")
    p(f"Paarungen mit Hugger:  {len(mit)}, davon mit geaenderter Sieganzahl: {len(abw_mit)}")

    # 2) Heldenquoten vorher / nachher / Seed 2027 (Rauschreferenz)
    p("\nBereinigte Siegquote je Held (Prozent)")
    p(f"{'Held':16}{'vorher':>8}{'nachher':>9}{'Δ Korrektur':>13}{'Seed 2027':>11}{'Δ Seed-Rauschen':>17}")
    dk, ds = [], []
    for (h,), r in sorted(hv.items(), key=lambda kv_: -fl(kv_[1]["Siegquote"])):
        v = fl(r["Siegquote"]) * 100
        n6 = fl(h6[(h,)]["Siegquote"]) * 100
        n7 = fl(h7[(h,)]["Siegquote"]) * 100
        dk.append(n6 - v); ds.append(n7 - n6)
        p(f"{h:16}{fmt(v):>8}{fmt(n6):>9}{fmt(n6 - v):>13}{fmt(n7):>11}{fmt(n7 - n6):>17}")
        csv_rows.append({"Agent": agent, "Held": h, "Rolle": r["Rolle"],
                         "Siegquote_vorher": fmt(v), "Siegquote_nachher_Seed2026": fmt(n6),
                         "Differenz_Korrektur_pp": fmt(n6 - v),
                         "Siegquote_Seed2027": fmt(n7), "Differenz_Seedrauschen_pp": fmt(n7 - n6)})
    p(f"Effektgroesse (RMS ueber 9 Helden): Korrektur {fmt(rms(dk))} pp, reines Seed-Rauschen {fmt(rms(ds))} pp")
    hug_k = fl(h6[(HUGGER,)]["Siegquote"]) * 100 - fl(hv[(HUGGER,)]["Siegquote"]) * 100
    p(f"Hugger: Aenderung durch die Korrektur {fmt(hug_k)} pp; groesste Seed-Abweichung eines Helden {fmt(max(abs(x) for x in ds))} pp")

    # 3) Kompositionen mit / ohne Hugger
    def komp_delta(a, b, hug):
        return [fl(b[k]["Siegquote"]) * 100 - fl(a[k]["Siegquote"]) * 100 for k in a if (HUGGER in k[0].split("+")) == hug]
    for hug, name in ((True, "mit Hugger (28)"), (False, "ohne Hugger (56)")):
        d_fix = komp_delta(kv, k6, hug)
        d_seed = komp_delta(k6, k7, hug)
        se = sd(d_fix) / math.sqrt(len(d_fix))
        p(f"Kompositionen {name}: mittlere Aenderung durch Korrektur {fmt(mean(d_fix), 3)} pp (SE {fmt(se, 3)}; t = {fmt(mean(d_fix) / se, 2)}), "
          f"RMS {fmt(rms(d_fix))} pp; RMS reines Seed-Rauschen {fmt(rms(d_seed))} pp")

    # 4) Direkter Verzerrungstest auf Paarungsebene: Siege der Hugger-Seite, nur Paarungen mit Hugger auf GENAU einer Seite
    def hugger_seite_delta(a, b):
        ds_ = []
        for (ta, tb), r in a.items():
            ha, hb = HUGGER in ta.split("+"), HUGGER in tb.split("+")
            if ha == hb:
                continue
            f = "SiegeTeamA" if ha else "SiegeTeamB"
            ds_.append(int(b[(ta, tb)][f]) - int(r[f]))
        return ds_
    d_fix = hugger_seite_delta(pv, p6)
    d_seed = hugger_seite_delta(p6, p7)
    for name, d in (("Korrektur (gleicher Seed)", d_fix), ("Seed-Wechsel 2026->2027 (Referenz)", d_seed)):
        se = sd(d) / math.sqrt(len(d))
        p(f"Siege der Hugger-Seite, {len(d)} Paarungen, {name}: Mittel {fmt(mean(d), 3)} Siege je 72 Partien (SE {fmt(se, 3)}; t = {fmt(mean(d) / se, 2)}), RMS {fmt(rms(d))}")

    # 5) Spiegelzeilen (nachher, beide Seeds)
    for label, pp in (("Seed 2026", p6), ("Seed 2027", p7)):
        rows = [r for (ta, tb), r in pp.items() if ta == tb]
        A = sum(int(r["SiegeTeamA"]) for r in rows); B = sum(int(r["SiegeTeamB"]) for r in rows)
        D = sum(int(r["Unentschieden"]) for r in rows); N = sum(int(r["Partien"]) for r in rows)
        z = (A - B) / math.sqrt(A + B)
        chi = sum(((int(r["SiegeTeamA"]) - int(r["SiegeTeamB"])) ** 2) / (int(r["SiegeTeamA"]) + int(r["SiegeTeamB"])) for r in rows)
        ties = sum(1 for r in rows if r["SiegeTeamA"] == r["SiegeTeamB"])
        p(f"Spiegelzeilen {label}: {len(rows)} Zeilen, {N} Partien: Team A {A} ({fmt(A / N * 100)}%), Team B {B} ({fmt(B / N * 100)}%), Remis {D}; "
          f"Anteil Team A an entschiedenen {fmt(A / (A + B) * 100)}%, z = {fmt(z, 2)}; Chi-Quadrat {fmt(chi, 1)} bei {len(rows)} Zeilen; exakt 50:50 in {ties} Zeilen")

text = "\n".join(out)
print(text)
with open(os.path.join(NEU, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(text)
with open(os.path.join(NEU, "helden_vergleich.csv"), "w", newline="", encoding="utf-8-sig") as f:
    w = csv.DictWriter(f, fieldnames=list(csv_rows[0].keys()), delimiter=";")
    w.writeheader(); w.writerows(csv_rows)
