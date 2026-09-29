#!/usr/bin/env python3
"""Basisläufe nach allen Katalog- und Bewertungsänderungen (Rundenlimit 50).
Wertet aus, was vorliegt: je Agent Siegquoten, Partiedauer, Aktionen, Fähigkeiten.
Liegen alle vier Agenten vor, zusätzlich Rangfolgen und Rangkorrelationen.
Vergleich: letzter vollständiger Stand ../hugger_letzter_held (Zufallsagenten) bzw.
../heuristik_aggressiv (aggressiv) - ACHTUNG: dort noch Rundenlimit 100 und ohne die
seitherigen Katalogänderungen, also kein sauberer Einzelvergleich."""
import csv, os

HIER = os.path.dirname(os.path.abspath(__file__))
ERG = os.path.dirname(HIER)
AGENTEN = ("byactiontype", "uniform", "aggressive", "defensive")
VORHER = {"byactiontype": ("hugger_letzter_held", "byactiontype"), "uniform": ("hugger_letzter_held", "uniform"),
          "aggressive": ("heuristik_aggressiv", "aggressive")}
PARTIEN = 257040
FAEHIGKEIT = {"wand": "Mauer", "schildtraeger": "Schildwall", "musiker": "Lied (Kampf- + Marschlied)", "elfe": "Heilung",
              "elementarmagier": "Elementarschlag (Feuer + Erde)", "hugger": "Umklammerung (Greifen + Loesen)",
              "illusionist": "Trugbild", "bogenschuetze": "Aufladen (Laden + Abbrechen)", "berserker": "Blutrausch (passiv)"}

def lade(path):
    with open(path, encoding="utf-8-sig") as f:
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

vorhanden = [a for a in AGENTEN if os.path.exists(os.path.join(HIER, f"basislauf_{a}_helden.csv"))]
out, quoten = [], {}
for a in vorhanden:
    h = {r["Held"]: r for r in lade(os.path.join(HIER, f"basislauf_{a}_helden.csv"))}
    p = lade(os.path.join(HIER, f"basislauf_{a}_paarungen.csv"))
    quoten[a] = {k: fl(r["Siegquote"]) * 100 for k, r in h.items()}
    n = sum(int(r["Partien"]) for r in p)
    runden = sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p) / n
    limit = sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p) / n
    tz = sum(int(r["RundenAmZug"]) for r in h.values())
    out.append(f"\n=== {a} ===")
    out.append(f"Partiedauer {fmt(runden)} Runden, Rundenlimit {fmt(limit * 100)} %")
    out.append("je Heldenzug: Bewegungen {}, Angriffe {}, Faehigkeiten {}".format(
        fmt(sum(int(r["Bewegungen"]) for r in h.values()) / tz, 3), fmt(sum(int(r["Angriffe"]) for r in h.values()) / tz, 3),
        fmt(sum(int(r["Faehigkeitseinsaetze"]) for r in h.values()) / tz, 3)))
    vor = None
    if a in VORHER:
        ordner, name = VORHER[a]
        # Vergleichsordner liegt entweder direkt unter Ergebnisse oder im Archivordner der vollständigen Läufe.
        kandidaten = [os.path.join(ERG, ordner), os.path.join(ERG, "AAAAAA Vollständige Läufe", ordner)]
        pfad = next((os.path.join(k, f"basislauf_{name}_helden.csv") for k in kandidaten if os.path.exists(os.path.join(k, f"basislauf_{name}_helden.csv"))), None)
        if pfad:
            vor = {r["Held"]: fl(r["Siegquote"]) * 100 for r in lade(pfad)}
    out.append(f"{'Held':16}{'Siegquote':>10}{'Rang':>6}" + (f"{'vorher':>9}{'Δ pp':>8}" if vor else "") + "   Faehigkeit je Partie (alle / mit Held)")
    rg = raenge(quoten[a])
    for held in sorted(quoten[a], key=lambda k: -quoten[a][k]):
        r = h[held]
        use = int(r["Faehigkeitseinsaetze"])
        zeile = f"{held:16}{fmt(quoten[a][held]):>10}{rg[held]:>6}"
        if vor: zeile += f"{fmt(vor[held]):>9}{fmt(quoten[a][held] - vor[held]):>8}"
        zeile += f"   {FAEHIGKEIT[held]}: {fmt(use / PARTIEN, 3)} / {fmt(use / int(r['PartienGesamtVerzerrt']), 3)}"
        out.append(zeile)
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
