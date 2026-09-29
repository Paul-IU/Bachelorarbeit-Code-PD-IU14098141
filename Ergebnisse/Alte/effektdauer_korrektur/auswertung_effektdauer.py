#!/usr/bin/env python3
"""Vergleich der vollstaendigen Basislaeufe VOR und NACH der Korrektur
'Wirkungsdauer haengt am Anwender' - beide Agenten."""
import csv
import os
import sys

# Projektrelativ, Aufruf aus beliebigem Verzeichnis moeglich. "vor_korrektur" enthaelt den
# Stand NACH der x10-Skalierung und VOR der Korrektur, dieses Verzeichnis den Stand danach.
REPO = os.path.dirname(os.path.abspath(__file__))
VOR = {a: os.path.join(REPO, "vor_korrektur") for a in ("byactiontype", "uniform")}
NACH = {a: REPO for a in ("byactiontype", "uniform")}

def pfad(basis, agent, art, vor):
    return os.path.join(basis, f"basislauf_{agent}_{art}.csv")

def lade(path, keys):
    rows = {}
    with open(path, encoding="utf-8-sig") as f:
        for row in csv.DictReader(f, delimiter=";"):
            rows[tuple(row[k] for k in keys)] = row
    return rows

def fl(s):
    return float(s.replace(",", "."))

def fmt(x, nd=2):
    return f"{x:.{nd}f}".replace(".", ",")

DAUER_HELDEN = {"schildtraeger", "wand", "musiker", "elementarmagier"}
ROLLEN_TANK = ("illusionist", "schildtraeger", "wand")

def held_metriken(paarungen):
    """Je Held: gewichteter Mittelwert von DurchschnittRunden und AnteilRundenlimit
    ueber alle Paarungen, in denen der Held auf mindestens einer Seite steht."""
    acc = {}
    for (a, b), r in paarungen.items():
        helden = set(a.split("+")) | set(b.split("+"))
        n = int(r["Partien"])
        for h in helden:
            s = acc.setdefault(h, [0, 0.0, 0.0])
            s[0] += n
            s[1] += n * fl(r["DurchschnittRunden"])
            s[2] += n * fl(r["AnteilRundenlimit"])
    return {h: (s[1] / s[0], s[2] / s[0]) for h, s in acc.items()}

def gesamt(paarungen):
    n = sum(int(r["Partien"]) for r in paarungen.values())
    runden = sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in paarungen.values()) / n
    limit = sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in paarungen.values()) / n
    return n, runden, limit

alle_zeilen = []
gesamt_zeilen = []
report = []

for agent in ("byactiontype", "uniform"):
    hv = lade(pfad(VOR[agent], agent, "helden", True), ["Held"])
    hn = lade(pfad(NACH[agent], agent, "helden", False), ["Held"])
    pv = lade(pfad(VOR[agent], agent, "paarungen", True), ["TeamA", "TeamB"])
    pn = lade(pfad(NACH[agent], agent, "paarungen", False), ["TeamA", "TeamB"])
    kv = lade(pfad(VOR[agent], agent, "kompositionen", True), ["Komposition"])
    kn = lade(pfad(NACH[agent], agent, "kompositionen", False), ["Komposition"])

    mv = held_metriken(pv)
    mn = held_metriken(pn)
    nv, rv, lv = gesamt(pv)
    nn, rn, ln = gesamt(pn)

    report.append(f"\n=== Agent: {agent} ===")
    report.append(f"Partien vorher/nachher: {nv} / {nn}")
    report.append(f"Durchschnittliche Partiedauer (Runden): vorher {fmt(rv,3)}  nachher {fmt(rn,3)}  Differenz {fmt(rn-rv,3)}")
    report.append(f"Anteil Rundenlimit-Ende:               vorher {fmt(lv*100,2)}%  nachher {fmt(ln*100,2)}%  Differenz {fmt((ln-lv)*100,2)} pp")
    gesamt_zeilen.append({
        "Agent": agent, "Partien": nn,
        "Partiedauer_vorher": fmt(rv, 3), "Partiedauer_nachher": fmt(rn, 3),
        "Rundenlimit_vorher_Prozent": fmt(lv * 100, 2), "Rundenlimit_nachher_Prozent": fmt(ln * 100, 2),
    })

    report.append("")
    report.append(f"{'Held':16}{'Rolle':9}{'Siegq.vor':>10}{'nach':>8}{'Δpp':>8}{'Rang v':>8}{'n':>4}{'Dauer v':>9}{'nach':>8}{'Δ':>7}{'RLimit v':>10}{'nach':>8}{'Δpp':>7}")
    rang_v = {h: i + 1 for i, (h, _) in enumerate(sorted(hv.items(), key=lambda kv_: -fl(kv_[1]["Siegquote"])))}
    rang_n = {h: i + 1 for i, (h, _) in enumerate(sorted(hn.items(), key=lambda kv_: -fl(kv_[1]["Siegquote"])))}
    for (h,), r in sorted(hv.items(), key=lambda kv_: -fl(kv_[1]["Siegquote"])):
        sv, sn = fl(r["Siegquote"]) * 100, fl(hn[(h,)]["Siegquote"]) * 100
        dv, dn = fl(r["DurchschnittPartiedauer"]), fl(hn[(h,)]["DurchschnittPartiedauer"])
        rlv, rln = mv[h][1] * 100, mn[h][1] * 100
        report.append(f"{h:16}{r['Rolle']:9}{fmt(sv):>10}{fmt(sn):>8}{fmt(sn-sv):>8}{str(rang_v[(h,)])+'>'+str(rang_n[(h,)]):>8}{'':>4}{fmt(dv):>9}{fmt(dn):>8}{fmt(dn-dv):>7}{fmt(rlv):>10}{fmt(rln):>8}{fmt(rln-rlv):>7}")
        alle_zeilen.append({
            "Agent": agent, "Held": h, "Rolle": r["Rolle"],
            "Siegquote_vorher": fmt(sv), "Siegquote_nachher": fmt(sn), "Differenz_pp": fmt(sn - sv),
            "Rang_vorher": rang_v[(h,)], "Rang_nachher": rang_n[(h,)],
            "Partiedauer_vorher": fmt(dv), "Partiedauer_nachher": fmt(dn),
            "RundenlimitAnteil_vorher_Prozent": fmt(rlv), "RundenlimitAnteil_nachher_Prozent": fmt(rln),
        })

    # Tank-Rangfolge
    tv = sorted(ROLLEN_TANK, key=lambda h: -fl(hv[(h,)]["Siegquote"]))
    tn = sorted(ROLLEN_TANK, key=lambda h: -fl(hn[(h,)]["Siegquote"]))
    report.append(f"\nTank-Rangfolge vorher: {tv}\nTank-Rangfolge nachher: {tn}")

    # Kontrolle: Paarungen ganz ohne Held mit endlicher Wirkungsdauer -> muessen identisch sein
    ohne = [k for k in pv if not ((set(k[0].split('+')) | set(k[1].split('+'))) & DAUER_HELDEN)]
    abw_ohne = [k for k in ohne if any(pv[k][f] != pn[k][f] for f in ("SiegeTeamA", "SiegeTeamB", "Unentschieden", "DurchschnittRunden", "AnteilRundenlimit"))]
    report.append(f"Kontrolle: Paarungen ohne Schildtraeger/Wand/Musiker/Elementarmagier: {len(ohne)}, davon abweichend: {len(abw_ohne)}")

    mit = [k for k in pv if k not in set(ohne)]
    abw_mit = [k for k in mit if pv[k]["SiegeTeamA"] != pn[k]["SiegeTeamA"] or pv[k]["SiegeTeamB"] != pn[k]["SiegeTeamB"]]
    report.append(f"Paarungen mit mind. einem dieser Helden: {len(mit)}, davon mit geaenderter Sieganzahl: {len(abw_mit)}")

    # Kompositionen: groesste Veraenderungen
    diffs = []
    for (k,), r in kv.items():
        diffs.append((fl(kn[(k,)]["Siegquote"]) * 100 - fl(r["Siegquote"]) * 100, k, fl(r["Siegquote"]) * 100, fl(kn[(k,)]["Siegquote"]) * 100))
    diffs.sort()
    report.append("\nGroesste Kompositions-Rueckgaenge:")
    for d, k, v, n in diffs[:5]:
        report.append(f"  {k:48} {fmt(v)}% -> {fmt(n)}% ({fmt(d)} pp)")
    report.append("Groesste Kompositions-Zuwaechse:")
    for d, k, v, n in diffs[::-1][:5]:
        report.append(f"  {k:48} {fmt(v)}% -> {fmt(n)}% ({fmt(d)} pp)")

    # Mittlere Kompositionsaenderung je Held
    report.append("\nMittlere Aenderung der Kompositions-Siegquote je enthaltenem Held (pp):")
    for h in sorted(key[0] for key in hv.keys()):
        ds = [d for d, k, _, _ in diffs if h in k.split('+')]
        report.append(f"  {h:16} {fmt(sum(ds)/len(ds))}")

out = "\n".join(report)
print(out)

with open(os.path.join(REPO, "helden_vergleich.csv"), "w", newline="", encoding="utf-8-sig") as f:
    w = csv.DictWriter(f, fieldnames=list(alle_zeilen[0].keys()), delimiter=";")
    w.writeheader()
    w.writerows(alle_zeilen)
with open(os.path.join(REPO, "gesamt_vergleich.csv"), "w", newline="", encoding="utf-8-sig") as f:
    w = csv.DictWriter(f, fieldnames=list(gesamt_zeilen[0].keys()), delimiter=";")
    w.writeheader()
    w.writerows(gesamt_zeilen)
with open(os.path.join(REPO, "auswertung.txt"), "w", encoding="utf-8") as f:
    f.write(out)
