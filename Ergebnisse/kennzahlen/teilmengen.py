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

Je Stichprobe (seit 13.10.2026 dasselbe Kriterium wie im vollständigen Lauf, siehe erkennung.py;
Unentschieden zählen überall als halber Sieg):
  (a) Die bereinigte Siegquote des Schildträgers (nur Partien mit ihm auf genau einer Seite) verschiebt
      sich in Richtung der Manipulation (Plus-Variante: steigt; Minus-Variante: sinkt).
  (b) Vorzeichentest über die 28 Kompositionen mit Schildträger (Siegquote ohne Spiegelpaarung):
      Kompositionen ohne Änderung - oder ohne Partie in der Stichprobe - zählen nicht; mindestens k der
      übrigen n bewegen sich in Richtung der Manipulation, k = Schwelle des einseitigen Tests, alpha = 0,05.
      Für n < 5 gibt es keine solche Schwelle: der Test kann nicht anschlagen (eigens gezählt).
  Eine Wiederholung gilt als ERKANNT, wenn (a) und (b) erfüllt sind. Gesucht ist die kleinste Stichprobe,
  bei der das in >= 95 % der Wiederholungen zutrifft.
"""
import csv
import gzip
import os

import numpy as np

from gemeinsam import KENNZAHLEN, UNENTSCHIEDEN_GEWICHT, VERAENDERTER_HELD, basis_von, finde_laeufe, fmt, lauf_name, vorzeichen_schwelle

GROESSEN = (1000, 10000, 50000, 100000)
WIEDERHOLUNGEN = 100
SCHWELLE = {n: vorzeichen_schwelle(n) for n in range(29)}


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
    siege_st = ((a & (s == 0)) | (b & (s == 1))) + UNENTSCHIEDEN_GEWICHT * (s == 2)
    st_quote = 100.0 * siege_st[einseitig].sum() / max(1, einseitig.sum())

    ka, kb = d["ka"][idx], d["kb"][idx]
    kein_spiegel = ka != kb
    spiele = np.bincount(ka[kein_spiegel], minlength=n_komps) + np.bincount(kb[kein_spiegel], minlength=n_komps)
    remis = UNENTSCHIEDEN_GEWICHT * (s[kein_spiegel] == 2)
    siege = (np.bincount(ka[kein_spiegel], weights=(s[kein_spiegel] == 0) + remis, minlength=n_komps) +
             np.bincount(kb[kein_spiegel], weights=(s[kein_spiegel] == 1) + remis, minlength=n_komps))
    with np.errstate(invalid="ignore", divide="ignore"):
        quote = np.where(spiele > 0, siege / spiele, np.nan)
    return st_quote, quote[st_komps]


def kriterium(delta_st, k_basis, k_variante, richtung):
    """(erkannt, Test unmöglich, in Richtung, dagegen) für eine Stichprobe bzw. den vollständigen Lauf."""
    diff = k_variante - k_basis
    veraendert = ~np.isnan(diff) & (diff != 0)
    rein = int(np.sum(diff[veraendert] * richtung > 0))
    gegen = int(np.sum(diff[veraendert] * richtung < 0))
    k = SCHWELLE[rein + gegen]
    erkannt = bool(delta_st * richtung > 0 and k is not None and rein >= k)
    return erkannt, k is None, rein, gegen


def main():
    laeufe = finde_laeufe()
    varianten = [l for l in laeufe if l[1] != "basis" and os.path.exists(l[4] + "_partien.csv.gz")]
    n = 257040
    stichproben = {(g, w): np.random.default_rng([2026, g, w]).choice(n, size=g, replace=False)
                   for g in GROESSEN for w in range(WIEDERHOLUNGEN)}

    out = ["# Simulationsumfang über Teilmengen (Schritt 6)", "",
           "Erzeugt von `Ergebnisse/kennzahlen/teilmengen.py`. Seeds der Stichproben: "
           "`numpy.random.default_rng([2026, Größe, Wiederholung])`, Wiederholung 0–99, ohne Zurücklegen.", "",
           "Unentschieden zählen als halber Sieg. Erkannt = (a) Siegquote des Schildträgers in Richtung der Manipulation UND "
           "(b) einseitiger Vorzeichentest (α = 0,05) über die veränderten Kompositionen mit Schildträger. "
           "ΔST = Änderung der bereinigten Siegquote des Schildträgers (Variante − Basis, pp); nur (a) = Anteil der Wiederholungen "
           "mit richtigem Vorzeichen von ΔST; Test unmöglich = Wiederholungen mit weniger als 5 veränderten Kompositionen.", ""]
    kurz = ["## Übersicht: Anteil erkannter Wiederholungen und kleinste Stichprobe mit ≥ 95 %", "",
            "In Klammern: Wiederholungen, in denen der Vorzeichentest mangels veränderter Kompositionen nicht anschlagen konnte (wenn > 0).", "",
            "| Variante | voller Lauf | 1.000 | 10.000 | 50.000 | 100.000 | ab |", "|---|---|---|---|---|---|---|"]
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
        voll_erkannt, _, voll_rein, voll_gegen = kriterium(voll_delta, kb_, kv, richtung)

        zeilen = [f"### {lauf_name(v)}", "",
                  f"Alle 257.040 Partien: ΔST {fmt(voll_delta, 2, True)} pp, Kompositionen in Richtung / dagegen {voll_rein} / {voll_gegen}, "
                  f"{'erkannt' if voll_erkannt else 'nicht erkannt'}.", "",
                  "| Stichprobe | erkannt | Test unmöglich | nur (a) | ΔST Mittel | ΔST min … max | Komp. in Richtung Mittel (min … max) |",
                  "|---|---|---|---|---|---|---|"]
        kurz_werte, unmoeglich_werte = [], []
        for g in GROESSEN:
            deltas, reins, erk, unm = [], [], 0, 0
            for w in range(WIEDERHOLUNGEN):
                idx = stichproben[(g, w)]
                sb_, kb_s = kennwerte(b, idx, st_a, st_b, st_komps, n_komps)
                sv_, kv_s = kennwerte(d, idx, st_a, st_b, st_komps, n_komps)
                e, u, rein, _ = kriterium(sv_ - sb_, kb_s, kv_s, richtung)
                deltas.append(sv_ - sb_)
                reins.append(rein)
                erk += e
                unm += u
            deltas, reins = np.array(deltas), np.array(reins)
            anteil = 100 * erk / WIEDERHOLUNGEN
            nur_a = 100 * np.mean(np.sign(deltas) == richtung)
            kurz_werte.append(anteil)
            unmoeglich_werte.append(unm)
            zeilen.append(f"| {g:_} | {fmt(anteil, 0)} % | {unm} | {fmt(nur_a, 0)} % | {fmt(deltas.mean(), 2, True)} | "
                          f"{fmt(deltas.min(), 2, True)} … {fmt(deltas.max(), 2, True)} | {fmt(reins.mean(), 1)} ({reins.min()} … {reins.max()}) |".replace("_", "."))
        ab = next((f"{g:_}".replace("_", ".") for g, r in zip(GROESSEN, kurz_werte) if r >= 95), "nie")
        kurz.append(f"| {lauf_name(v)} | {'erkannt' if voll_erkannt else 'nicht erkannt'} | " +
                    " | ".join(f"{fmt(r, 0)} %" + (f" ({u})" if u else "") for r, u in zip(kurz_werte, unmoeglich_werte)) + f" | {ab} |")
        detail += zeilen + [""]
        print(lauf_name(v), [round(r) for r in kurz_werte], flush=True)

    with open(os.path.join(KENNZAHLEN, "teilmengen_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out + kurz + [""] + detail) + "\n")


if __name__ == "__main__":
    main()
