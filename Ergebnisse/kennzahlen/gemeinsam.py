"""Gemeinsame Hilfsfunktionen der Kennzahl-Auswertung (Bachelorarbeit).

Findet alle Läufe im Ergebnisordner und lädt Paarungs-, Helden- und Partiedateien.
Ein Lauf ist ein Tupel (agent, art, prozent, seed, präfix):
  - Basislauf:  art = "basis", prozent = 0, seed = 2026..2030
  - Variante:   art = "dmg" | "hp" | "ability", prozent = z.B. -2.5, seed = 2026
Der Ordner "Test" (Elfen-Heilung) und "Alte" (frühere Katalogstände) gehören nicht dazu.
"""
import csv
import glob
import gzip
import os
import re

import numpy as np

KENNZAHLEN = os.path.dirname(os.path.abspath(__file__))
ERGEBNISSE = os.path.dirname(KENNZAHLEN)
AGENTEN = ("byactiontype", "uniform", "aggressive", "defensive")
VERAENDERTER_HELD = "schildtraeger"

# Grundwerte des Basiskatalogs (TacticsCore/Heroes/DefaultHeroes.cs, Stand 09.10.2026).
# Hugger: BaseAttackDamage = HuggerGrundschaden = 60.
GRUNDWERTE = {  # Held: (MaxHitPoints, BaseAttackDamage)
    "schildtraeger": (850, 70),
    "elfe": (450, 80),
    "berserker": (400, 80),
    "wand": (1000, 70),
    "illusionist": (750, 60),
    "musiker": (480, 90),
    "hugger": (500, 60),
    "elementarmagier": (320, 100),
    "bogenschuetze": (280, 120),
}


def zahl(s):
    """Zahl im deutschen Format ("0,6807") lesen."""
    return float(s.replace(",", "."))


def lade_csv(pfad):
    with open(pfad, encoding="utf-8-sig") as f:
        return list(csv.DictReader(f, delimiter=";"))


def finde_laeufe():
    """Alle Basis- und Variantenläufe, für die eine Paarungsdatei vorliegt."""
    laeufe = []
    for a in AGENTEN:
        p = os.path.join(ERGEBNISSE, "basislauf_2026_09_26_partien", f"basislauf_{a}")
        if os.path.exists(p + "_paarungen.csv"):
            laeufe.append((a, "basis", 0.0, 2026, p))
    for ordner in sorted(glob.glob(os.path.join(ERGEBNISSE, "seed_*"))):
        seed = int(ordner.rsplit("_", 1)[1])
        for a in AGENTEN:
            p = os.path.join(ordner, f"basislauf_{a}_seed{seed}")
            if os.path.exists(p + "_paarungen.csv"):
                laeufe.append((a, "basis", 0.0, seed, p))
    muster = re.compile(r"schildtraeger_(dmg|hp|ability)_(-?[\d.]+)_(\w+)_paarungen\.csv$")
    for pfad in sorted(glob.glob(os.path.join(ERGEBNISSE, "*", "*", "schildtraeger", "*_paarungen.csv"))):
        m = muster.search(os.path.basename(pfad))
        if m and m.group(3) in AGENTEN:
            laeufe.append((m.group(3), m.group(1), float(m.group(2)), 2026, pfad[: -len("_paarungen.csv")]))
    return laeufe


def lauf_name(lauf):
    a, art, p, seed, _ = lauf
    if art == "basis":
        return f"basis_{a}_seed{seed}"
    return f"{VERAENDERTER_HELD}_{art}_{p:g}_{a}"


def basis_von(lauf, laeufe):
    """Der Basislauf (Seed 2026) desselben Agenten."""
    return next(l for l in laeufe if l[0] == lauf[0] and l[1] == "basis" and l[3] == 2026)


def kompositionen(paarungen):
    """Alle 84 Kompositionsschlüssel, alphabetisch."""
    return sorted({r["TeamA"] for r in paarungen} | {r["TeamB"] for r in paarungen})


def lade_partien(praefix):
    """Partiedatei als Numpy-Arrays: Paarung, Ausgangslage, TeamA-/TeamB-Kompositionsindex, Sieger (0 = A, 1 = B, 2 = unentschieden)."""
    with gzip.open(praefix + "_partien.csv.gz", "rt", encoding="utf-8") as f:
        zeilen = list(csv.DictReader(f, delimiter=";"))
    return zeilen


def spearman(x, y):
    """Spearman-Rangkorrelation mit Durchschnittsrängen bei Gleichstand."""
    def raenge(v):
        v = np.asarray(v, dtype=float)
        ordnung = v.argsort(kind="mergesort")
        r = np.empty(len(v))
        r[ordnung] = np.arange(1, len(v) + 1)
        for wert in np.unique(v):  # Gleichstände: Mittelwert der Ränge
            maske = v == wert
            r[maske] = r[maske].mean()
        return r
    rx, ry = raenge(x), raenge(y)
    return float(np.corrcoef(rx, ry)[0, 1])


def fmt(x, nd=2, vz=False):
    """Zahl im deutschen Format für die Ausgabetabellen."""
    s = f"{x:+.{nd}f}" if vz else f"{x:.{nd}f}"
    return s.replace(".", ",")


# ---- Siegquoten (einheitlich, Bachelorarbeit 13.10.2026): Unentschieden zählen als halber Sieg.
UNENTSCHIEDEN_GEWICHT = 0.5


def helden_quote(paarungen, gewicht=UNENTSCHIEDEN_GEWICHT):
    """Bereinigte Siegquote je Held in %: nur Paarungen mit dem Helden auf GENAU EINER Seite (wie die Heldendatei),
    Siege der eigenen Seite + gewicht * Unentschieden, geteilt durch Partien. gewicht = 0 reproduziert exakt
    die Spalte 'Siegquote' der Heldendatei (Unentschieden zählen dort nicht als Sieg)."""
    siege, spiele = {}, {}
    for r in paarungen:
        a, b = set(r["TeamA"].split("+")), set(r["TeamB"].split("+"))
        p, d = int(r["Partien"]), int(r["Unentschieden"])
        for held in a ^ b:
            eigene = int(r["SiegeTeamA"]) if held in a else int(r["SiegeTeamB"])
            siege[held] = siege.get(held, 0.0) + eigene + gewicht * d
            spiele[held] = spiele.get(held, 0) + p
    return {h: 100.0 * siege[h] / spiele[h] for h in siege}


def komp_quote(paarungen, gewicht=UNENTSCHIEDEN_GEWICHT):
    """Siegquote je Komposition in % ohne Spiegelpaarung; Unentschieden mit gewicht."""
    siege, spiele = {}, {}
    for r in paarungen:
        a, b = r["TeamA"], r["TeamB"]
        if a == b:
            continue
        p, d = int(r["Partien"]), int(r["Unentschieden"])
        for k, eigene in ((a, int(r["SiegeTeamA"])), (b, int(r["SiegeTeamB"]))):
            siege[k] = siege.get(k, 0.0) + eigene + gewicht * d
            spiele[k] = spiele.get(k, 0) + p
    return {k: 100.0 * siege[k] / spiele[k] for k in siege}


def vorzeichen_schwelle(n, alpha=0.05):
    """Kleinstes k mit P(X >= k) <= alpha für X ~ Binomial(n, 1/2) (einseitiger Vorzeichentest); None, wenn keins existiert (n < 5)."""
    from math import comb
    for k in range(n + 1):
        if sum(comb(n, i) for i in range(k, n + 1)) / 2 ** n <= alpha:
            return k
    return None
