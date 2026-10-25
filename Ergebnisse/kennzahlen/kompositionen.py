#!/usr/bin/env python3
"""Auswertung der Kompositionen für Kapitel 8.3 (Bachelorarbeit).

Aufruf:  python kompositionen.py
Ausgabe: kompositionen_ergebnisse.md (Teil A bis D), kompositionen_delta.csv (Langformat)

Nur aus vorhandenen Dateien. Siegquote einer Komposition ohne Spiegelpaarung, Unentschieden als halber Sieg
(gemeinsam.komp_quote). Basis = Basislauf Seed 2026 des jeweiligen Agenten. Die Zusatzvariante dmg +20 % bleibt
außen vor; ausgewertet werden die 72 Varianten (3 Attribute × 6 Stufen × 4 Agenten).

A  Basisläufe: Mittelwert, SD (n−1), min, max der 84 Kompositionssiegquoten; fünf stärkste und schwächste;
   Spearman der 84 Kompositionen zwischen allen Agentenpaaren.
B  Je Variante: Änderung (Variante − Basis, pp) der 28 Kompositionen mit und der 56 ohne Schildträger. "In Richtung"
   = Änderung × Vorzeichen der Manipulation. Vollständig getrennt, wenn keine Komposition ohne Schildträger stärker
   in Richtung verschoben ist als die schwächste mit Schildträger.
C  Je Variante: am stärksten und am schwächsten reagierende Komposition mit Schildträger (nach Änderung in Richtung)
   und Spearman zwischen Basissiegquote und Änderung in Richtung über die 28; Median und Anzahl negativer Werte.
D  Siegquote des Schildträgers getrennt nach Seite: Blüte A = er gehört zu Team A (Team A steht in allen 72
   Ausgangslagen auf Blüte A, AusgangslagenGenerator), Blüte B = Team B; nur Paarungen mit ihm auf genau einer Seite,
   Unentschieden als halber Sieg. Δ = Variante − Basis je Seite.
"""
import csv
import os
from statistics import median

import numpy as np

from gemeinsam import AGENTEN, KENNZAHLEN, UNENTSCHIEDEN_GEWICHT, VERAENDERTER_HELD, finde_laeufe, fmt, komp_quote, lade_csv, spearman

ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)


def stufe_name(art, p):
    return f"{art} {fmt(p, 1, True).replace(',0', '')} %"


def st_nach_seite(paarungen, held=VERAENDERTER_HELD, gewicht=UNENTSCHIEDEN_GEWICHT):
    """Bereinigte Siegquote des Helden in %, getrennt nach Seite: {'A': ..., 'B': ...}."""
    siege, spiele = {"A": 0.0, "B": 0.0}, {"A": 0, "B": 0}
    for r in paarungen:
        in_a, in_b = held in r["TeamA"].split("+"), held in r["TeamB"].split("+")
        if in_a == in_b:
            continue
        seite = "A" if in_a else "B"
        eigene = int(r["SiegeTeamA"]) if in_a else int(r["SiegeTeamB"])
        siege[seite] += eigene + gewicht * int(r["Unentschieden"])
        spiele[seite] += int(r["Partien"])
    return {s: 100.0 * siege[s] / spiele[s] for s in siege}, spiele


def vorzeichen_p(k, n):
    """Zweiseitiger Vorzeichentest: P(|X − n/2| >= |k − n/2|) für X ~ Binomial(n, 1/2)."""
    from math import comb
    if n == 0:
        return float("nan")
    extrem = max(k, n - k)
    return min(1.0, 2 * sum(comb(n, i) for i in range(extrem, n + 1)) / 2 ** n)


def main():
    laeufe = finde_laeufe()
    basis = {a: next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026) for a in AGENTEN}
    bpaar = {a: lade_csv(basis[a][4] + "_paarungen.csv") for a in AGENTEN}
    bq = {a: komp_quote(bpaar[a]) for a in AGENTEN}
    komps = sorted(bq[AGENTEN[0]])
    mit = [k for k in komps if VERAENDERTER_HELD in k.split("+")]
    ohne = [k for k in komps if k not in mit]
    varianten = {(l[0], l[1], l[2]): l for l in laeufe if l[1] != "basis" and not (l[1] == "dmg" and l[2] == 20.0)}
    vpaar = {key: lade_csv(v[4] + "_paarungen.csv") for key, v in varianten.items()}
    vq = {key: komp_quote(p) for key, p in vpaar.items()}

    out = ["# Kompositionen", "",
           "Erzeugt von `Ergebnisse/kennzahlen/kompositionen.py`. Siegquote einer Komposition ohne Spiegelpaarung, "
           "Unentschieden als halber Sieg; Basis = Basislauf Seed 2026 des Agenten; 72 Varianten (ohne dmg +20 %). "
           "Rohdaten aller Änderungen: `kompositionen_delta.csv`.", ""]

    # ---------------------------------------------------------------- A
    out += ["## Teil A – Kompositionen in den Basisläufen", "", "### A1 Lage und Streuung der 84 Kompositionssiegquoten", "",
            "| Agent | Mittelwert | SD | kleinster Wert | größter Wert |", "|---|---|---|---|---|"]
    for a in AGENTEN:
        v = np.array([bq[a][k] for k in komps])
        out.append(f"| {a} | {fmt(v.mean(), 2)} % | {fmt(v.std(ddof=1), 2)} pp | {fmt(v.min(), 2)} % | {fmt(v.max(), 2)} % |")
    out += ["", "| Agent | Rang | fünf stärkste | Siegquote | fünf schwächste | Siegquote |", "|---|---|---|---|---|---|"]
    for a in AGENTEN:
        rang = sorted(komps, key=lambda k: -bq[a][k])
        for i in range(5):
            s, w = rang[i], rang[-1 - i]
            out.append(f"| {a if i == 0 else ''} | {i + 1} | {s} | {fmt(bq[a][s], 2)} % | {w} | {fmt(bq[a][w], 2)} % |")
    out += ["", "### A2 Spearman der 84 Kompositionen zwischen den Agenten", "", "| Paar | Spearman |", "|---|---|"]
    for i, x in enumerate(AGENTEN):
        for y in AGENTEN[i + 1:]:
            out.append(f"| {x} / {y} | {fmt(spearman([bq[x][k] for k in komps], [bq[y][k] for k in komps]), 3)} |")
    out.append("")

    # ---------------------------------------------------------------- B und C (gemeinsame Daten)
    aenderung = {key: {k: vq[key][k] - bq[key[0]][k] for k in komps} for key in varianten}
    b_zeilen = ["| Agent | Variante | mit ST: Mittel | mit ST: min | mit ST: max | ohne ST: Mittel | ohne ST: min | ohne ST: max | getrennt | Überlappung |",
                "|---|---|---|---|---|---|---|---|---|---|"]
    c_zeilen = ["| Agent | Variante | am stärksten | Änderung | am schwächsten | Änderung | Spearman Basis / Reaktion |",
                "|---|---|---|---|---|---|---|"]
    getrennt_je = {a: 0 for a in AGENTEN}
    rho = {}
    for a in AGENTEN:
        for art in ARTEN:
            for p in STUFEN:
                d = aenderung[(a, art, p)]
                r = 1 if p > 0 else -1
                dm, do = np.array([d[k] for k in mit]), np.array([d[k] for k in ohne])
                schwaechste_mit = (dm * r).min()
                ueber = int(np.sum(do * r > schwaechste_mit))
                getrennt_je[a] += ueber == 0
                b_zeilen.append(f"| {a} | {stufe_name(art, p)} | {fmt(dm.mean(), 2, True)} | {fmt(dm.min(), 2, True)} | {fmt(dm.max(), 2, True)} | "
                                f"{fmt(do.mean(), 2, True)} | {fmt(do.min(), 2, True)} | {fmt(do.max(), 2, True)} | "
                                f"{'ja' if ueber == 0 else 'nein'} | {ueber if ueber else '–'} |")
                reaktion = {k: d[k] * r for k in mit}
                rang = sorted(mit, key=lambda k: -reaktion[k])
                rho[(a, art, p)] = spearman([bq[a][k] for k in mit], [reaktion[k] for k in mit])
                c_zeilen.append(f"| {a} | {stufe_name(art, p)} | {rang[0]} | {fmt(d[rang[0]], 2, True)} | {rang[-1]} | {fmt(d[rang[-1]], 2, True)} | "
                                f"{fmt(rho[(a, art, p)], 3)} |")

    out += ["## Teil B – Trennung betroffen / nicht betroffen", "",
            "Änderung = Variante − Basis in pp. Getrennt = keine Komposition ohne Schildträger ist stärker in Richtung der "
            "Manipulation verschoben als die schwächste mit Schildträger. Überlappung = Anzahl der Kompositionen ohne "
            "Schildträger (von 56), die stärker in Richtung verschoben sind als die schwächste mit Schildträger.", ""] + b_zeilen
    out += ["", "Vollständig getrennt: " + ", ".join(f"{a} {getrennt_je[a]}/18" for a in AGENTEN) +
            f"; zusammen {sum(getrennt_je.values())} von 72.", ""]

    out += ["## Teil C – Unterschiedlich starke Reaktion", "",
            "Reaktion = Änderung in Richtung der Manipulation; die Spalten „Änderung“ zeigen die Änderung mit Vorzeichen (pp). "
            "Spearman = Rangkorrelation zwischen Basissiegquote und Reaktion über die 28 Kompositionen mit Schildträger; "
            "negativ heißt: schwache Kompositionen reagieren stärker.", ""] + c_zeilen
    out += ["", "### Zusammenfassung: Spearman Basis / Reaktion", "",
            "| Agent | Attribut | Median | negativ | Spannweite |", "|---|---|---|---|---|"]
    for a in AGENTEN:
        for art in ARTEN:
            w = [rho[(a, art, p)] for p in STUFEN]
            out.append(f"| {a} | {art} | {fmt(median(w), 3)} | {sum(x < 0 for x in w)} von 6 | {fmt(min(w), 3)} … {fmt(max(w), 3)} |")
    gesamt_neg = sum(x < 0 for x in rho.values())
    out += ["", f"Negativ in {gesamt_neg} von 72 Varianten.", ""]

    # ---------------------------------------------------------------- D
    seite_b = {a: st_nach_seite(bpaar[a]) for a in AGENTEN}
    out += ["## Teil D – Seitenvorteil, erster Schritt", "",
            "### D1 Siegquote des Schildträgers nach Seite", "",
            "Blüte A = Schildträger in Team A (Team A steht in allen 72 Ausgangslagen auf Blüte A), Blüte B = in Team B. "
            "Nur Paarungen mit ihm auf genau einer Seite, Unentschieden als halber Sieg. Weil Team A immer die alphabetisch "
            "frühere Komposition ist, unterscheiden sich die beiden Teilmengen auch in Mitspielern und Gegnern; der Vergleich "
            "trennt die Seite also nicht sauber von der Zusammensetzung.", "",
            "| Agent | Partien auf A | Partien auf B | Basis auf A | Basis auf B | Differenz A − B |", "|---|---|---|---|---|---|"]
    for a in AGENTEN:
        q, n = seite_b[a]
        out.append(f"| {a} | {n['A']:_} | {n['B']:_} | {fmt(q['A'], 2)} % | {fmt(q['B'], 2)} % | {fmt(q['A'] - q['B'], 2, True)} pp |".replace("_", "."))
    out += ["", "Änderung je Seite (Variante − Basis, pp); „in Richtung“ = Änderung × Vorzeichen der Manipulation:", "",
            "| Agent | Variante | Δ auf A | Δ auf B | A − B (in Richtung) |", "|---|---|---|---|---|"]
    zusammenfassung = []
    for a in AGENTEN:
        diffs = {}
        for art in ARTEN:
            for p in STUFEN:
                q, _ = st_nach_seite(vpaar[(a, art, p)])
                da, db = q["A"] - seite_b[a][0]["A"], q["B"] - seite_b[a][0]["B"]
                r = 1 if p > 0 else -1
                diffs[(art, p)] = (da * r - db * r)
                out.append(f"| {a} | {stufe_name(art, p)} | {fmt(da, 2, True)} | {fmt(db, 2, True)} | {fmt(diffs[(art, p)], 2, True)} |")
        for art in ARTEN + ("alle",):
            w = [v for (ar, _), v in diffs.items() if art in ("alle", ar)]
            k = sum(x > 0 for x in w)
            zusammenfassung.append((a, art, np.mean(w), k, len(w), vorzeichen_p(k, len(w))))
    out += ["", "**Zusammenfassung:** A − B in Richtung der Manipulation (positiv = auf Blüte A reagiert der Schildträger stärker). "
            "p = zweiseitiger Vorzeichentest über die Varianten (nur ein grober Hinweis, die Varianten eines Agenten teilen denselben Basislauf).", "",
            "| Agent | Attribut | Mittel A − B (pp) | A stärker | p |", "|---|---|---|---|---|"]
    for a, art, m, k, n, pw in zusammenfassung:
        out.append(f"| {a} | {art} | {fmt(m, 2, True)} | {k} von {n} | {fmt(pw, 3)} |")
    out.append("")
    out += D2_TEXT
    with open(os.path.join(KENNZAHLEN, "kompositionen_ergebnisse.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")

    # ---------------------------------------------------------------- CSV
    with open(os.path.join(KENNZAHLEN, "kompositionen_delta.csv"), "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter=";")
        w.writerow(["agent", "variante", "komposition", "mit_schildtraeger", "siegquote_basis", "siegquote_variante", "delta_pp"])
        for a in AGENTEN:
            for art in ARTEN:
                for p in STUFEN:
                    key = (a, art, p)
                    for k in komps:
                        w.writerow([a, f"{art}_{p:+g}", k, int(k in mit), f"{bq[a][k]:.6f}", f"{vq[key][k]:.6f}", f"{aenderung[key][k]:.6f}"])
    print("\n".join(out))


D2_TEXT = [
    "### D2 Code: Symmetrie des Spielfelds und Gleichstandsregel", "",
    "*Historisch: Analyse vom 20.10.2026, vor der Seitenkorrektur. Die hier beschriebene Ursache ist seit dem 23.10.2026 behoben "
    "(CandidateKey, Sichtlinie und Flächenwirkungen aus Sicht der eigenen Blüte, Commit 9bf7b7f); Basislauf und vertauschter "
    "Basislauf stimmen jetzt in allen 257.040 Partien gespiegelt überein. Teil D1 oben ist mit den neuen Läufen gerechnet.*", "",
    "**Spielfeld (`TacticsCore/Board/Board.cs`, Feldliste Z. 75–97).** Blüte A: (0,0) (0,1) (0,2) (−1,1) (−1,2) (1,0) (1,1); "
    "Verbindungsfelder (−1,3) (1,2); Blüte B: (0,3) (0,4) (0,5) (−1,4) (−1,5) (1,3) (1,4). Startfelder A: (0,0) (−1,1) (1,0), "
    "B: (0,5) (−1,5) (1,4). Das Feld ist auf zwei Arten symmetrisch zwischen A und B:",
    "- Punktspiegelung (Drehung um 180°) (q, r) → (−q, 5 − r): bildet jedes Feld von A auf ein Feld von B ab, Startfeld auf "
    "Startfeld, und die beiden Verbindungsfelder aufeinander ((−1,3) ↔ (1,2)).",
    "- Achsenspiegelung an der Mittellinie (q, r) → (q, 5 − r − q): ebenfalls A ↔ B, Startfeld ↔ Startfeld; die "
    "Verbindungsfelder bleiben fest.",
    "Die 72 Ausgangslagen (`TacticsCore/Setup/AusgangslagenGenerator.cs`, Z. 31–55) belegen die drei Startfelder je Blüte "
    "mit allen 6 Permutationen; das Startteam wechselt. Die Aufstellungsmenge ist damit unter der Spiegelung ebenfalls "
    "symmetrisch. **Die Geometrie bevorzugt keine Seite.**", "",
    "**Gleichstandsregel der heuristischen Agenten (`TacticsCore/Agents/HeuristicAgent.cs`).**",
    "- `DecideAction` (Z. 47–88) durchläuft die Kandidaten in fester Reihenfolge und übernimmt einen Kandidaten nur bei "
    "`score > bestScore` (Z. 81). Bei gleicher Bewertung gewinnt also der **erste** Kandidat der Reihenfolge.",
    "- Die Reihenfolge kommt aus `Candidates` (Z. 119–156, `list.OrderBy(c => c.Item2)`) mit dem Schlüssel `CandidateKey` "
    "(Z. 198–220): erst Art (0 Bewegung, 1 Angriff, 2 Fähigkeit), dann **absolute Koordinate Q aufsteigend, dann R "
    "aufsteigend** des Zielfelds bzw. der Zieleinheit, dann eigenes vor gegnerischem Team, Held vor Klon, Id, Modus.",
    "- Die absolute Koordinate ist nicht seitenneutral. Kleines R liegt bei Blüte A (Startreihe r = 0/1), großes bei B. "
    "Bei gleicher Bewertung wählt eine Einheit von **Team B** daher bevorzugt das Feld mit kleinem R, also **in Richtung "
    "Gegner**, eine Einheit von **Team A** dagegen ebenfalls kleines R, also **zur eigenen Startreihe zurück**. Dazu "
    "kommt eine Links-rechts-Vorliebe (kleines Q). Unter der Spiegelung (q, r) → (−q, 5 − r) wird die Vorliebe "
    "„kleines Q, kleines R“ von Team B für Team A zu „großes Q, großes R“, die beiden Seiten folgen also nicht derselben Regel.",
    "- Gleichstände sind bei diesen Agenten häufig, weil `TurnEvaluator` den Zustand nach dem Zug bewertet und viele "
    "Bewegungen dieselbe Bewertung ergeben (insbesondere ohne Kontakt zum Gegner und bei defensiven Gewichten).", "",
    "Andere Sortierungen sind seitenneutral oder nicht entscheidungsrelevant: `TurnOrder.RankBySpeedDescending` "
    "(`TacticsCore/Match/TurnOrder.cs`, Z. 137–138) sortiert bei gleichem Speed nach UnitId; die Zuordnung der Ids "
    "ist durch die Etiketten-Prüfung abgedeckt (0 Abweichungen). `MatchController` (Z. 124–129) sortiert nach "
    "Startposition nur für das Protokoll. `AbilitySystem.GetLegalAbilityUses` ordnet nach Feld, aber die Agenten sortieren "
    "die Kandidaten anschließend ohnehin neu.", "",
    "**Wahrscheinlichste Ursache:** die Gleichstandsauflösung über absolute Koordinaten in `HeuristicAgent.CandidateKey` in "
    "Verbindung mit `score > bestScore`. Sie wirkt bei den Zufallsagenten nicht, weil die ohne Bewertung zufällig wählen, und "
    "die Etiketten-Prüfung erfasst sie nicht, weil sie Etiketten bei gleichen Positionen tauscht, nicht die Seiten. "
    "Belegen ließe es sich nur mit Läufen, also außerhalb dieses Auftrags: etwa ein Lauf, in dem Team A auf Blüte B "
    "steht, oder ein Schlüssel relativ zur eigenen Blüte (r für Team B gespiegelt).", "",
]


if __name__ == "__main__":
    main()
