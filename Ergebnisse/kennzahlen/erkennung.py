#!/usr/bin/env python3
"""Erkennungskriterium für alle Variantenläufe (Bachelorarbeit).

Aufruf:  python erkennung.py   (nach teilmengen.py, dessen Übersicht hier gelesen wird)
Ausgabe: erkennung_ergebnisse.md

KRITERIUM VOLLSTÄNDIGER LAUF (gegen den Basislauf Seed 2026 desselben Agenten):
  (a) Die bereinigte Siegquote des Schildträgers (nur Partien mit ihm auf genau einer Seite,
      seit 13.10.2026 Unentschieden als halber Sieg, aus der Paarungsdatei) verschiebt sich in Richtung der Manipulation, UND
  (b) mindestens k der Kompositionen mit Schildträger bewegen sich in dieselbe Richtung.
      Siegquote der Kompositionen ohne Spiegelpaarung, Unentschieden als halber Sieg.
      Kompositionen ohne jede Änderung zählen nicht; k ist die Schwelle des einseitigen
      Vorzeichentests mit alpha = 0,05 für n = Anzahl der veränderten Kompositionen
      (kleinstes k mit P(X >= k | n, 1/2) <= 0,05; für n = 28 ist k = 19).

KRITERIUM SIMULATIONSUMFANG: kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000, bei der dasselbe
Kriterium (a) UND (b) in >= 95 % der 100 Wiederholungen erfüllt ist (aus teilmengen_ergebnisse.md).
"""
import os
import re

from gemeinsam import (AGENTEN, KENNZAHLEN, VERAENDERTER_HELD, basis_von, finde_laeufe, fmt, helden_quote,
                       komp_quote, lade_csv, lauf_name, vorzeichen_schwelle)

ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)


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
        b_paar = lade_csv(basis[4] + "_paarungen.csv")
        b_st, b_k = helden_quote(b_paar)[VERAENDERTER_HELD], komp_quote(b_paar)
        for art in ARTEN:
            for p in STUFEN:
                v = varianten.get((a, art, p))
                name = f"{art} {fmt(p, 1, True).replace(',0', '')} %"
                if v is None:
                    matrix[(art, p, a)] = "fehlt"
                    zeilen.append(f"| {a} | {name} | – | – | – | – | – | Lauf fehlt | – |")
                    continue
                richtung = 1 if p > 0 else -1
                v_paar = lade_csv(v[4] + "_paarungen.csv")
                d_st = helden_quote(v_paar)[VERAENDERTER_HELD] - b_st
                v_k = komp_quote(v_paar)
                mit = [k for k in v_k if VERAENDERTER_HELD in k.split("+")]
                diffs = [v_k[k] - b_k[k] for k in mit]
                rein = sum(1 for d in diffs if d * richtung > 0)
                gegen = sum(1 for d in diffs if d * richtung < 0)
                gleich = len(diffs) - rein - gegen
                k = vorzeichen_schwelle(rein + gegen)
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
           "Unentschieden zählen überall als halber Sieg (seit 13.10.2026).", "",
           "**Kriterium vollständiger Lauf:** (a) bereinigte Siegquote des Schildträgers verschiebt sich in Richtung der "
           "Manipulation UND (b) mindestens k der veränderten Kompositionen mit Schildträger bewegen sich in dieselbe Richtung "
           "(Siegquote ohne Spiegelpaarung, Unentschieden als halber Sieg). Unveränderte Kompositionen zählen nicht; "
           "k = Schwelle des einseitigen Vorzeichentests, α = 0,05, für n = Anzahl veränderter Kompositionen (n = 28 → k = 19).", "",
           "**Kriterium Simulationsumfang:** kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der dasselbe "
           "Kriterium (a) UND (b) in ≥ 95 % der 100 Wiederholungen erfüllt ist (aus `teilmengen_ergebnisse.md`).", "",
           "## Übersicht", ""] + uebersicht + ["", "## Einzelwerte", ""] + zeilen
    with open(os.path.join(KENNZAHLEN, "erkennung_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
