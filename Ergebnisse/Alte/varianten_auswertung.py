#!/usr/bin/env python3
"""Auswertung der Varianten-Läufe gegen den Basislauf desselben Agenten.

Aufruf:  python varianten_auswertung.py <agent> <art>/<art_prozent>/<held> [...]
Beispiel: python varianten_auswertung.py uniform dmg/dmg_10/schildtraeger hp/hp_10/schildtraeger

Je Variante: bereinigte Siegquote je Held mit Differenz zum Basislauf (Prozentpunkte),
Verschiebung der Kompositionen mit dem veränderten Helden (Mittel und Größtwert),
Partiedauer, Rundenlimit-Anteil, Rangverschiebungen, Laufzeit (aus dem Laufprotokoll).
Einordnung am Seed-Rauschen des Agenten (RAUSCHEN unten: RMS über neun Helden und größte
Einzelabweichung zweier Läufe mit verschiedenem Seed).
Ergebnis zusätzlich in <ordner>/<variante>_<agent>_auswertung.txt."""
import csv, gzip, os, re, sys

# Ergebnisordner: der eigene oder ein darüberliegender, der den Basislauf enthält
# (das Skript darf dadurch z.B. in einem Unterordner wie "Alte" liegen).
ERG = os.path.dirname(os.path.abspath(__file__))
while not os.path.isdir(os.path.join(ERG, "basislauf_2026_09_26_partien")) and os.path.dirname(ERG) != ERG:
    ERG = os.path.dirname(ERG)
BASIS = os.path.join(ERG, "basislauf_2026_09_26_partien")
# Seed-Rauschen je Agent in Prozentpunkten: (RMS über neun Helden, größte Einzelabweichung)
RAUSCHEN = {"uniform": (0.14, 0.32)}

def lade(path):
    with open(path, encoding="utf-8-sig") as f:
        return list(csv.DictReader(f, delimiter=";"))

def fl(s): return float(s.replace(",", "."))
def fmt(x, nd=2, vz=False): return (f"{x:+.{nd}f}" if vz else f"{x:.{nd}f}").replace(".", ",")
def raenge(w):
    o = sorted(w, key=lambda k: -w[k]); return {k: i + 1 for i, k in enumerate(o)}

def lauf(prefix):
    h = {r["Held"]: r for r in lade(prefix + "_helden.csv")}
    k = {r["Komposition"]: fl(r["Siegquote"]) * 100 for r in lade(prefix + "_kompositionen.csv")}
    p = lade(prefix + "_paarungen.csv")
    n = sum(int(r["Partien"]) for r in p)
    dauer = sum(int(r["Partien"]) * fl(r["DurchschnittRunden"]) for r in p) / n
    limit = sum(int(r["Partien"]) * fl(r["AnteilRundenlimit"]) for r in p) / n * 100
    return h, k, dauer, limit

def einordnung(d, rms, gross):
    if rms != rms:  # NaN: fuer diesen Agenten ist kein Seed-Rauschen gemessen
        return "(kein Rauschwert)"
    a = abs(d)
    return "deutlich ueber Rauschen" if a > 2 * gross else "ueber Rauschen" if a > gross else "Grenzbereich" if a > rms else "im Rauschen"

agent, varianten = sys.argv[1], sys.argv[2:]
rms, gross = RAUSCHEN.get(agent, (float("nan"), float("nan")))
bh, bk, bdauer, blimit = lauf(os.path.join(BASIS, f"basislauf_{agent}"))
bq = {k: fl(r["Siegquote"]) * 100 for k, r in bh.items()}
brang = raenge(bq)

for pfad in varianten:
    art, artp, held = pfad.split("/")
    name = f"{held}_{artp}"
    ordner = os.path.join(ERG, art, artp, held)
    prefix = os.path.join(ordner, f"{name}_{agent}")
    vh, vk, vdauer, vlimit = lauf(prefix)
    vq = {k: fl(r["Siegquote"]) * 100 for k, r in vh.items()}
    vrang = raenge(vq)
    out = [f"=== {name} ({agent}) ==="]
    log = open(prefix + "_lauf.log", encoding="utf-8", errors="replace").read()
    m = re.search(r"Gesamtlaufzeit: ([\d,]+) s", log)
    if m:
        s = float(m.group(1).replace(",", ".")); out.append(f"Laufzeit {int(s // 60)} min {int(s % 60)} s")
    out.append(f"Partiedauer {fmt(vdauer)} Runden (Basis {fmt(bdauer)}, {fmt(vdauer - bdauer, 2, True)}), "
               f"Rundenlimit {fmt(vlimit)} % (Basis {fmt(blimit)} %, {fmt(vlimit - blimit, 2, True)} pp)")
    out.append(f"Rauschen {agent}: RMS {fmt(rms)} pp, groesste Einzelabweichung {fmt(gross)} pp")
    out.append(f"{'Held':16}{'Basis':>8}{'Variante':>10}{'Δ pp':>8}{'Rang':>8}   Einordnung")
    for hid in sorted(vq, key=lambda x: -vq[x]):
        d = vq[hid] - bq[hid]
        rang = f"{brang[hid]}→{vrang[hid]}" if brang[hid] != vrang[hid] else f"{vrang[hid]}"
        out.append(f"{hid:16}{fmt(bq[hid]):>8}{fmt(vq[hid]):>10}{fmt(d, 2, True):>8}{rang:>8}   {einordnung(d, rms, gross)}{'  <- veraendert' if hid == held else ''}")
    andere = [vq[h] - bq[h] for h in vq if h != held]
    out.append(f"Uebrige acht Helden: RMS der Differenz {fmt((sum(x * x for x in andere) / len(andere)) ** 0.5)} pp, "
               f"groesste {fmt(max(andere, key=abs), 2, True)} pp")
    wechsel = [f"{h} {brang[h]}→{vrang[h]}" for h in sorted(vq, key=lambda x: vrang[x]) if brang[h] != vrang[h]]
    out.append("Rangverschiebungen: " + (", ".join(wechsel) if wechsel else "keine"))
    mit = {k: vk[k] - bk[k] for k in vk if held in k.split("+")}
    ohne = {k: vk[k] - bk[k] for k in vk if held not in k.split("+")}
    grk = max(mit, key=lambda k: abs(mit[k]))
    out.append(f"Kompositionen mit {held} ({len(mit)}): Mittel {fmt(sum(mit.values()) / len(mit), 2, True)} pp, "
               f"Mittel |Δ| {fmt(sum(abs(x) for x in mit.values()) / len(mit))} pp, Groesstwert {fmt(mit[grk], 2, True)} pp ({grk}), "
               f"kleinster {fmt(min(mit.values()), 2, True)} pp")
    out.append(f"Kompositionen ohne {held} ({len(ohne)}): Mittel {fmt(sum(ohne.values()) / len(ohne), 2, True)} pp, "
               f"Mittel |Δ| {fmt(sum(abs(x) for x in ohne.values()) / len(ohne))} pp")
    text = "\n".join(out)
    print(text + "\n")
    with open(prefix + "_auswertung.txt", "w", encoding="utf-8") as f:
        f.write(text + "\n")
