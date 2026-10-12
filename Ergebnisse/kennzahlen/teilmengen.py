#!/usr/bin/env python3
"""Schritt 6 der Kennzahlen: Simulationsumfang über Teilmengen von Partien.

Aufruf:  python teilmengen.py
Ausgabe: teilmengen_ergebnisse.md

Für jede Variante mit Partiedatei, gepaart gegen den Basislauf (Seed 2026) desselben Agenten:
  - Stichproben von 1.000, 10.000, 50.000 und 100.000 Partien, je 100 Wiederholungen,
    ohne Zurücklegen aus den 257.040 Partien.
  - Basislauf und Variante verwenden DIESELBEN Partien (gleiche Zeilen = gleiche Paarung
    und Ausgangslage; die Ausrichtung beider Dateien wird geprüft).
  - SEEDS der Stichprobenziehung: numpy.random.default_rng([2026, Größe, Wiederholung]),
    Wiederholung = 0..99. Dieselben Stichproben gelten für alle Varianten und Agenten.

Je Stichprobe:
  - Änderung der bereinigten Siegquote des Schildträgers (nur Partien mit ihm auf genau
    einer Seite; Unentschieden zählen nicht als Sieg), Variante minus Basis, in pp.
  - Anteil der 28 Kompositionen mit Schildträger, deren Siegquote (ohne Spiegelpaarung) sich
    in Richtung der Manipulation verschiebt (Plus-Variante: steigt; Minus-Variante: sinkt).
    Keine Änderung oder keine Partie der Komposition in der Stichprobe zählt als "nicht verschoben".
"""
import csv
import gzip
import os

import numpy as np

from gemeinsam import KENNZAHLEN, VERAENDERTER_HELD, basis_von, finde_laeufe, fmt, lauf_name

GROESSEN = (1000, 10000, 50000, 100000)
WIEDERHOLUNGEN = 100


def lade(praefix, komp_index):
    """Partiedatei als Arrays: (Paarung, Ausgangslage), Kompositionsindex A/B, Sieger (0 = A, 1 = B, 2 = Draw)."""
    paarung, lage, ka, kb, sieger = [], [], [], [], []
    code = {"TeamA": 0, "TeamB": 1, "Draw": 2}
    with gzip.open(praefix + "_partien.csv.gz", "rt", encoding="utf-8") as f:
        for r in csv.DictReader(f, delimiter=";"):
            paarung.append(int(r["Paarung"]))
            lage.append(int(r["Ausgangslage"]))
            ka.append(komp_index.setdefault(r["TeamA"], len(komp_index)))
            kb.append(komp_index.setdefault(r["TeamB"], len(komp_index)))
            sieger.append(code[r["Sieger"]])
    return dict(paarung=np.array(paarung), lage=np.array(lage), ka=np.array(ka), kb=np.array(kb), sieger=np.array(sieger))


def kennwerte(d, idx, st_a, st_b, st_komps, n_komps):
    """Bereinigte Siegquote des Schildträgers und Siegquote je Komposition (ohne Spiegel) für die Partien idx."""
    s = d["sieger"][idx]
    a, b = st_a[idx], st_b[idx]
    einseitig = a ^ b
    siege_st = (a & (s == 0)) | (b & (s == 1))
    st_quote = 100.0 * siege_st[einseitig].sum() / max(1, einseitig.sum())

    ka, kb = d["ka"][idx], d["kb"][idx]
    kein_spiegel = ka != kb
    spiele = np.bincount(ka[kein_spiegel], minlength=n_komps) + np.bincount(kb[kein_spiegel], minlength=n_komps)
    siege = (np.bincount(ka[kein_spiegel], weights=(s[kein_spiegel] == 0), minlength=n_komps) +
             np.bincount(kb[kein_spiegel], weights=(s[kein_spiegel] == 1), minlength=n_komps))
    with np.errstate(invalid="ignore", divide="ignore"):
        quote = np.where(spiele > 0, siege / spiele, np.nan)
    return st_quote, quote[st_komps]


def main():
    laeufe = finde_laeufe()
    varianten = [l for l in laeufe if l[1] != "basis" and os.path.exists(l[4] + "_partien.csv.gz")]
    n = 257040
    stichproben = {(g, w): np.random.default_rng([2026, g, w]).choice(n, size=g, replace=False)
                   for g in GROESSEN for w in range(WIEDERHOLUNGEN)}

    out = ["# Simulationsumfang über Teilmengen (Schritt 6)", "",
           "Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: "
           "`numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.", "",
           "ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp). "
           "Richtung = Anteil der Wiederholungen, in denen ΔST das Vorzeichen der Manipulation hat. "
           "Kompositionen = Anteil der 28 Kompositionen mit Schildträger, die sich in Richtung der Manipulation verschieben.", ""]
    kurz = ["## Übersicht: kleinste Stichprobe, ab der der Schildträger in ≥ 95 % der Wiederholungen richtig verschoben ist", "",
            "| Variante | 1.000 | 10.000 | 50.000 | 100.000 | ab |", "|---|---|---|---|---|---|"]
    detail = ["## Einzelwerte je Variante", ""]

    cache = {}
    for v in varianten:
        basis = basis_von(v, laeufe)
        if basis not in cache:  # Basislauf je Agent nur einmal laden; der Kompositionsindex gilt dann für alle seine Varianten
            komp_index = {}
            cache[basis] = (lade(basis[4], komp_index), komp_index)
        b, komp_index = cache[basis]
        d = lade(v[4], komp_index)
        if not (np.array_equal(d["paarung"], b["paarung"]) and np.array_equal(d["lage"], b["lage"])):
            raise RuntimeError(f"{lauf_name(v)}: Partien nicht gleich ausgerichtet wie der Basislauf")

        namen = sorted(komp_index, key=komp_index.get)
        n_komps = len(namen)
        mit_st = np.array([VERAENDERTER_HELD in k.split("+") for k in namen])
        st_komps = np.where(mit_st)[0]
        st_a, st_b = mit_st[b["ka"]], mit_st[b["kb"]]
        richtung = 1.0 if v[2] > 0 else -1.0

        alle = np.arange(n)
        sb, kb_ = kennwerte(b, alle, st_a, st_b, st_komps, n_komps)
        sv, kv = kennwerte(d, alle, st_a, st_b, st_komps, n_komps)
        voll_delta = sv - sb
        voll_anteil = np.mean(np.sign(kv - kb_) == richtung)

        zeilen = [f"### {lauf_name(v)}", "",
                  f"Alle 257.040 Partien: ΔST {fmt(voll_delta, 2, True)} pp, Kompositionen in Richtung {fmt(100 * voll_anteil, 1)} %.", "",
                  "| Stichprobe | ΔST Mittel | ΔST min … max | Richtung | Kompositionen Mittel | Kompositionen min … max |",
                  "|---|---|---|---|---|---|"]
        kurz_werte = []
        for g in GROESSEN:
            deltas, anteile = [], []
            for w in range(WIEDERHOLUNGEN):
                idx = stichproben[(g, w)]
                sb_, kb_s = kennwerte(b, idx, st_a, st_b, st_komps, n_komps)
                sv_, kv_s = kennwerte(d, idx, st_a, st_b, st_komps, n_komps)
                deltas.append(sv_ - sb_)
                anteile.append(np.mean(np.sign(kv_s - kb_s) == richtung))
            deltas, anteile = np.array(deltas), 100 * np.array(anteile)
            richtig = 100 * np.mean(np.sign(deltas) == richtung)
            kurz_werte.append(richtig)
            zeilen.append(f"| {g:_} | {fmt(deltas.mean(), 2, True)} | {fmt(deltas.min(), 2, True)} … {fmt(deltas.max(), 2, True)} | "
                          f"{fmt(richtig, 0)} % | {fmt(anteile.mean(), 1)} % | {fmt(anteile.min(), 1)} … {fmt(anteile.max(), 1)} % |".replace("_", "."))
        ab = next((f"{g:_}".replace("_", ".") for g, r in zip(GROESSEN, kurz_werte) if r >= 95), "nicht erreicht")
        kurz.append(f"| {lauf_name(v)} | " + " | ".join(f"{fmt(r, 0)} %" for r in kurz_werte) + f" | {ab} |")
        detail += zeilen + [""]
        print(lauf_name(v), [round(r) for r in kurz_werte], flush=True)

    with open(os.path.join(KENNZAHLEN, "teilmengen_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out + kurz + [""] + detail) + "\n")


if __name__ == "__main__":
    main()
