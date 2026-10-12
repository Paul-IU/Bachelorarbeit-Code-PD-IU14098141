#!/usr/bin/env python3
"""Erkennungskriterium für alle Variantenläufe (Bachelorarbeit).

Aufruf:  python erkennung.py   (nach teilmengen.py, dessen Übersicht hier gelesen wird)
Ausgabe: erkennung_ergebnisse.md

KRITERIUM VOLLSTÄNDIGER LAUF (gegen den Basislauf Seed 2026 desselben Agenten):
  (a) Die bereinigte Siegquote des Schildträgers (Heldendatei: nur Partien mit ihm auf genau
      einer Seite) verschiebt sich in Richtung der Manipulation, UND
  (b) mindestens k der Kompositionen mit Schildträger bewegen sich in dieselbe Richtung.
      Siegquote der Kompositionen ohne Spiegelpaarung, Unentschieden als halber Sieg.
      Kompositionen ohne jede Änderung zählen nicht; k ist die Schwelle des einseitigen
      Vorzeichentests mit alpha = 0,05 für n = Anzahl der veränderten Kompositionen
      (kleinstes k mit P(X >= k | n, 1/2) <= 0,05; für n = 28 ist k = 19).

KRITERIUM SIMULATIONSUMFANG: kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000, bei der
der Schildträger in >= 95 % der 100 Wiederholungen richtig verschoben ist (aus teilmengen_ergebnisse.md).
"""
import os
import re
from math import comb

from gemeinsam import (AGENTEN, KENNZAHLEN, VERAENDERTER_HELD, basis_von, finde_laeufe, fmt, kompositionen,
                       lade_csv, lauf_name, zahl)

ALPHA = 0.05
ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)


def schwelle(n):
    """Kleinstes k mit P(X >= k) <= alpha für X ~ Binomial(n, 1/2); None, wenn kein k das erreicht."""
    for k in range(n + 1):
        if sum(comb(n, i) for i in range(k, n + 1)) / 2 ** n <= ALPHA:
            return k
    return None


def komp_quote(paarungen):
    """Siegquote je Komposition ohne Spiegelpaarung, Unentschieden als halber Sieg (in %)."""
    komps = kompositionen(paarungen)
    siege = dict.fromkeys(komps, 0.0)
    spiele = dict.fromkeys(komps, 0)
    for r in paarungen:
        a, b = r["TeamA"], r["TeamB"]
        if a == b:
            continue
        p, d = int(r["Partien"]), int(r["Unentschieden"])
        siege[a] += int(r["SiegeTeamA"]) + 0.5 * d
        siege[b] += int(r["SiegeTeamB"]) + 0.5 * d
        spiele[a] += p
        spiele[b] += p
    return {k: 100.0 * siege[k] / spiele[k] for k in komps}


def st_quote(praefix):
    return next(zahl(r["Siegquote"]) * 100 for r in lade_csv(praefix + "_helden.csv") if r["Held"] == VERAENDERTER_HELD)


def lies_teilmengen():
    """Übersicht aus teilmengen_ergebnisse.md: Lauf -> kleinste Stichprobe ('ab'-Spalte)."""
    pfad = os.path.join(KENNZAHLEN, "teilmengen_ergebnisse.md")
    ab = {}
    if os.path.exists(pfad):
        for zeile in open(pfad, encoding="utf-8"):
            m = re.match(r"\| (schildtraeger_\S+) \| .* \| ([^|]+) \|\s*$", zeile)
            if m:
                wert = m.group(2).strip()
                ab.setdefault(m.group(1), "nie" if wert == "nicht erreicht" else wert)
    return ab


def main():
    laeufe = finde_laeufe()
    varianten = {(l[0], l[1], l[2]): l for l in laeufe if l[1] != "basis"}
    ab = lies_teilmengen()

    zeilen = ["| Agent | Variante | Δ Schildträger (pp) | in Richtung | dagegen | unverändert | Schwelle | erfüllt | kleinste Stichprobe |",
              "|---|---|---|---|---|---|---|---|---|"]
    matrix = {}
    for a in AGENTEN:
        basis = basis_von(next(iter(v for k, v in varianten.items() if k[0] == a)), laeufe)
        b_st, b_k = st_quote(basis[4]), komp_quote(lade_csv(basis[4] + "_paarungen.csv"))
        for art in ARTEN:
            for p in STUFEN:
                v = varianten.get((a, art, p))
                name = f"{art} {fmt(p, 1, True).replace(',0', '')} %"
                if v is None:
                    matrix[(art, p, a)] = "fehlt"
                    zeilen.append(f"| {a} | {name} | – | – | – | – | – | Lauf fehlt | – |")
                    continue
                richtung = 1 if p > 0 else -1
                d_st = st_quote(v[4]) - b_st
                v_k = komp_quote(lade_csv(v[4] + "_paarungen.csv"))
                mit = [k for k in v_k if VERAENDERTER_HELD in k.split("+")]
                diffs = [v_k[k] - b_k[k] for k in mit]
                rein = sum(1 for d in diffs if d * richtung > 0)
                gegen = sum(1 for d in diffs if d * richtung < 0)
                gleich = len(diffs) - rein - gegen
                k = schwelle(rein + gegen)
                erfuellt = d_st * richtung > 0 and k is not None and rein >= k
                matrix[(art, p, a)] = "erkannt" if erfuellt else "nicht erkannt"
                zeilen.append(f"| {a} | {name} | {fmt(d_st, 2, True)} | {rein} | {gegen} | {gleich} | "
                              f"{k if k is not None else '–'} von {rein + gegen} | {'ja' if erfuellt else 'nein'} | "
                              f"{ab.get(lauf_name(v), 'nicht berechnet')} |")

    uebersicht = ["| Variante | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    for art in ARTEN:
        for p in STUFEN:
            name = f"{art} {fmt(p, 1, True).replace(',0', '')} %"
            uebersicht.append(f"| {name} | " + " | ".join(matrix[(art, p, a)] for a in AGENTEN) + " |")

    out = ["# Erkennung der Ungleichgewichte", "",
           "Erzeugt von `Ergebnisse/kennzahlen/erkennung.py`. Je Variantenlauf gegen den Basislauf (Seed 2026) desselben Agenten.", "",
           "**Kriterium vollständiger Lauf:** (a) bereinigte Siegquote des Schildträgers verschiebt sich in Richtung der "
           "Manipulation UND (b) mindestens k der veränderten Kompositionen mit Schildträger bewegen sich in dieselbe Richtung "
           "(Siegquote ohne Spiegelpaarung, Unentschieden als halber Sieg). Unveränderte Kompositionen zählen nicht; "
           "k = Schwelle des einseitigen Vorzeichentests, α = 0,05, für n = Anzahl veränderter Kompositionen (n = 28 → k = 19).", "",
           "**Kriterium Simulationsumfang:** kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der der "
           "Schildträger in ≥ 95 % der 100 Wiederholungen in Richtung der Manipulation verschoben ist (aus `teilmengen_ergebnisse.md`).", "",
           "## Übersicht", ""] + uebersicht + ["", "## Einzelwerte", ""] + zeilen
    with open(os.path.join(KENNZAHLEN, "erkennung_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
