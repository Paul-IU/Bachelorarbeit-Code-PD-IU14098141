#!/usr/bin/env python3
"""Zusammenstellung für Kapitel 8.3 der Bachelorarbeit.

Aufruf:  python kapitel8.py   (nach kennzahlen.py, teilmengen.py, erkennung.py)
Ausgabe: ergebnisse_kapitel8.md

Nur aus vorhandenen Dateien. Aktueller Stand: Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung,
ohne die Zusatzvariante dmg +20 %. Werte, die sich gegenüber den früheren Berichten (vor dem 13.10.2026:
Unentschieden der Helden nicht als Sieg; Simulationsumfang nur über das Vorzeichen) verändert haben, sind mit
"‡" markiert; der frühere Wert steht in Klammern. Verglichen wird auf der angezeigten Stellenzahl.

Steigung: Kleinste-Quadrate-Gerade mit Achsenabschnitt durch die Stufen und den Basispunkt (0 %, 0 pp);
positiv = 0, 2,5, 5, 10; negativ = −10, −5, −2,5, 0; gesamt = alle sieben Punkte.
Verhältnis: |Δ(−p)| / |Δ(+p)|; > 1 heißt, die Abschwächung wirkt stärker als die Verstärkung.
"""
import os
import re

import numpy as np

from gemeinsam import AGENTEN, KENNZAHLEN, VERAENDERTER_HELD, finde_laeufe, fmt, helden_quote, lade_csv, lauf_name

ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)
ALT, NEU = 0.0, 0.5
MARKE = "‡"


def stufe_name(art, p):
    return f"{art} {fmt(p, 1, True).replace(',0', '')} %"


def markiert(neu, alt):
    """Neuer Wert, bei Abweichung zum alten mit Marke und altem Wert in Klammern."""
    return neu if neu == alt else f"{neu} {MARKE} ({alt})"


def tabellenzeilen(datei, muster):
    pfad = os.path.join(KENNZAHLEN, datei)
    return [[z.strip() for z in zeile.strip().strip("|").split("|")]
            for zeile in open(pfad, encoding="utf-8") if re.match(muster, zeile)]


def gerade(x, y):
    x, y = np.asarray(x, float), np.asarray(y, float)
    b, a = np.polyfit(x, y, 1)
    rest = y - (a + b * x)
    gesamt = np.sum((y - y.mean()) ** 2)
    return b, (1 - np.sum(rest ** 2) / gesamt) if gesamt > 0 else float("nan")


def main():
    laeufe = finde_laeufe()
    basis = {a: next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026) for a in AGENTEN}
    varianten = {(l[0], l[1], l[2]): l for l in laeufe if l[1] != "basis" and not (l[1] == "dmg" and l[2] == 20.0)}
    st = {}
    for l in list(basis.values()) + list(varianten.values()):
        paar = lade_csv(l[4] + "_paarungen.csv")
        st[l] = {w: helden_quote(paar, w)[VERAENDERTER_HELD] for w in (ALT, NEU)}
    delta = {(a, art, p, w): st[varianten[(a, art, p)]][w] - st[basis[a]][w]
             for a in AGENTEN for art in ARTEN for p in STUFEN for w in (ALT, NEU)}

    out = ["# Ergebnisse für Kapitel 8.3", "",
           "Erzeugt von `Ergebnisse/kennzahlen/kapitel8.py`. Aktueller Stand: Unentschieden als halber Sieg, Kompositionen ohne "
           "Spiegelpaarung, ohne die Zusatzvariante dmg +20 %. Basis = Basislauf Seed 2026 des jeweiligen Agenten.", "",
           f"**{MARKE} = gegenüber früheren Berichten verändert**, früherer Wert in Klammern (vor dem 13.10.2026: Unentschieden "
           "der Helden nicht als Sieg; Simulationsumfang nur über das Vorzeichen der Schildträger-Siegquote). "
           "Verglichen wird auf der angezeigten Stellenzahl.", ""]

    # ---- 1 Δ Schildträger
    out += ["## 1 Δ der bereinigten Siegquote des Schildträgers (pp)", "",
            "Basis (Seed 2026): " + ", ".join(f"{a} {markiert(fmt(st[basis[a]][NEU], 2), fmt(st[basis[a]][ALT], 2))} %" for a in AGENTEN), "",
            "| Variante | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    for art in ARTEN:
        for p in STUFEN:
            out.append(f"| {stufe_name(art, p)} | " + " | ".join(
                markiert(fmt(delta[(a, art, p, NEU)], 2, True), fmt(delta[(a, art, p, ALT)], 2, True)) for a in AGENTEN) + " |")
    out.append("")

    # ---- 2 Verhältnis
    out += ["## 2 Verhältnis Abschwächung zu Verstärkung, |Δ(−p)| / |Δ(+p)|", "",
            "| Attribut | Agent | ±2,5 % | ±5 % | ±10 % |", "|---|---|---|---|---|"]
    for art in ("dmg", "hp"):
        for a in AGENTEN:
            zellen = []
            for p in (2.5, 5.0, 10.0):
                v = {w: abs(delta[(a, art, -p, w)]) / abs(delta[(a, art, p, w)]) for w in (ALT, NEU)}
                zellen.append(markiert(fmt(v[NEU], 2), fmt(v[ALT], 2)))
            out.append(f"| {art} | {a} | " + " | ".join(zellen) + " |")
    # Fähigkeitsstärke: Verhältnis nur bei den Zufallsagenten; bei den heuristischen Agenten ist die Wirkung nicht
    # monoton (siehe Abschnitt 1), dort nur, ob Δ(+p) und Δ(−p) dasselbe Vorzeichen haben.
    for a in AGENTEN:
        zellen = []
        for p in (2.5, 5.0, 10.0):
            if a in ("byactiontype", "uniform"):
                v = {w: abs(delta[(a, "ability", -p, w)]) / abs(delta[(a, "ability", p, w)]) for w in (ALT, NEU)}
                zellen.append(markiert(fmt(v[NEU], 2), fmt(v[ALT], 2)))
            else:
                text = {}
                for w in (ALT, NEU):
                    plus, minus = delta[(a, "ability", p, w)], delta[(a, "ability", -p, w)]
                    if np.sign(plus) == np.sign(minus):
                        text[w] = f"gleiches Vorzeichen ({'beide +' if plus > 0 else 'beide −'})"
                    else:
                        text[w] = "entgegengesetzt, " + ("beide in Richtung der Manipulation" if plus > 0 else "beide gegen die Manipulation")
                    text[w] += f": {fmt(plus, 2, True)} / {fmt(minus, 2, True)}"
                zellen.append(markiert(text[NEU], text[ALT]))
        out.append(f"| ability | {a} | " + " | ".join(zellen) + " |")
    out += ["", "Fähigkeitsstärke: Verhältnis nur für byactiontype und uniform. Für aggressive und defensive steht statt eines "
            "Werts, ob Δ(+p) und Δ(−p) dasselbe Vorzeichen haben, dahinter Δ(+p) / Δ(−p) in pp. Bei gleichem Vorzeichen "
            "wirkt eine der beiden Stufen gegen ihre Richtung; bei entgegengesetztem Vorzeichen ist zusätzlich angegeben, ob "
            "beide in oder beide gegen die Richtung der Manipulation liegen. Ein Verhältnis wäre dort nicht sinnvoll. "
            "Alle Verhältnisse aus den ungerundeten Werten."]
    out.append("")

    # ---- 3 Steigung
    out += ["## 3 Steigung (pp je % Manipulation) und R²", "",
            "Kleinste-Quadrate-Gerade mit Achsenabschnitt über die Stufen und den Basispunkt (0 %, 0 pp): positiv = 0/2,5/5/10, "
            "negativ = −10/−5/−2,5/0, gesamt = alle sieben Punkte.", "",
            "| Attribut | Agent | positiv: Steigung | R² | negativ: Steigung | R² | gesamt: Steigung | R² |",
            "|---|---|---|---|---|---|---|---|"]
    gruppen = {"positiv": (0.0, 2.5, 5.0, 10.0), "negativ": (-10.0, -5.0, -2.5, 0.0),
               "gesamt": (-10.0, -5.0, -2.5, 0.0, 2.5, 5.0, 10.0)}
    for art in ARTEN:
        for a in AGENTEN:
            zellen = []
            for g, xs in gruppen.items():
                res = {w: gerade(xs, [0.0 if x == 0 else delta[(a, art, x, w)] for x in xs]) for w in (ALT, NEU)}
                zellen.append(markiert(fmt(res[NEU][0], 3, True), fmt(res[ALT][0], 3, True)))
                zellen.append(markiert(fmt(res[NEU][1], 3), fmt(res[ALT][1], 3)))
            out.append(f"| {art} | {a} | " + " | ".join(zellen) + " |")
    out.append("")

    # ---- 4 Erkennung und Simulationsumfang
    def erkennung(datei):
        return {(z[0], z[1]): z for z in tabellenzeilen(datei, r"\| (" + "|".join(AGENTEN) + r") \| ")}

    def ab(datei):
        return {z[0]: ("nie" if z[-1] in ("nicht erreicht", "nie") else z[-1])
                for z in tabellenzeilen(datei, r"\| schildtraeger_\S+ \|")}
    e_neu, e_alt = erkennung("erkennung_ergebnisse.md"), erkennung("erkennung_ergebnisse_vor_unentschieden.md")
    ab_neu, ab_alt = ab("teilmengen_ergebnisse.md"), ab("teilmengen_ergebnisse_vor_unentschieden.md")
    teil = {z[0]: z for z in tabellenzeilen("teilmengen_ergebnisse.md", r"\| schildtraeger_\S+ \|")}
    out += ["## 4 Erkennung", "",
            "**Kriterium:** (a) Siegquote des Schildträgers in Richtung der Manipulation UND (b) einseitiger Vorzeichentest "
            "(α = 0,05) über die veränderten Kompositionen mit Schildträger; unveränderte Kompositionen zählen nicht.", "",
            "### Erkennungsmatrix (vollständiger Lauf, 257.040 Partien)", "",
            "| Variante | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    for art in ARTEN:
        for p in STUFEN:
            n = stufe_name(art, p)
            zellen = []
            for a in AGENTEN:
                z_neu, z_alt = e_neu[(a, n)], e_alt[(a, n)]
                txt = "erkannt" if z_neu[7] == "ja" else "nicht erkannt"
                zellen.append(f"{txt} ({z_neu[3]}/{z_neu[3 + 1]})" + (f" {MARKE}" if z_neu[7] != z_alt[7] else ""))
            out.append(f"| {n} | " + " | ".join(zellen) + " |")
    anzahl = sum(z[7] == "ja" for z in e_neu.values() if not z[1].startswith("dmg +20"))
    out += ["", f"In Klammern: Kompositionen in Richtung / dagegen. Erkannt: {anzahl} von 72.", "",
            "### Simulationsumfang", "",
            "Kleinste Stichprobe aus 1.000 / 10.000 / 50.000 / 100.000 Partien, bei der das Kriterium in ≥ 95 % der 100 "
            "Wiederholungen erfüllt ist; nie = auch bei 100.000 nicht.", "",
            "| Variante | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    for art in ARTEN:
        for p in STUFEN:
            zellen = [markiert(ab_neu[lauf_name(varianten[(a, art, p)])], ab_alt[lauf_name(varianten[(a, art, p)])]) for a in AGENTEN]
            out.append(f"| {stufe_name(art, p)} | " + " | ".join(zellen) + " |")
    out += ["", "Anteil erkannter Wiederholungen je Stichprobengröße (in Klammern: Wiederholungen, in denen der Vorzeichentest "
            "mangels veränderter Kompositionen nicht anschlagen konnte):", "",
            "| Agent | Variante | 1.000 | 10.000 | 50.000 | 100.000 |", "|---|---|---|---|---|---|"]
    for a in AGENTEN:
        for art in ARTEN:
            for p in STUFEN:
                z = teil[lauf_name(varianten[(a, art, p)])]
                out.append(f"| {a} | {stufe_name(art, p)} | " + " | ".join(z[2:6]) + " |")
    out.append("")

    # ---- 5 Top-D und Top-B
    topd = {z[0]: z for z in tabellenzeilen("kennzahlen_ergebnisse.md", r"\| (basis_|schildtraeger_)\S+ \| [a-z+]+ \| \d")}
    topb = {z[0]: z[1] for z in tabellenzeilen("kennzahlen_ergebnisse.md", r"\| (basis_|schildtraeger_)\S+ \| \d+ \|")}
    out += ["## 5 Top-D und Top-B", "",
            "Top-D(G) nach Bradley-Terry (Lin et al.), Top-B = Anzahl nicht dominierter Kompositionen (Proposition 5.8). "
            "Unentschieden zählten hier schon bisher als halber Sieg; alle Werte sind unverändert.", "",
            "| Lauf | c_top | Top-D 0,01 | 0,02 | 0,04 | 0,08 | Top-B |", "|---|---|---|---|---|---|---|"]
    seeds = {a: sorted((l for l in laeufe if l[0] == a and l[1] == "basis"), key=lambda l: l[3]) for a in AGENTEN}
    namen = [lauf_name(l) for a in AGENTEN for l in seeds[a]] + \
            [lauf_name(varianten[(a, art, p)]) for a in AGENTEN for art in ARTEN for p in STUFEN]
    for n in namen:
        z = topd[n]
        out.append(f"| {n} | {z[1]} | {z[2]} | {z[3]} | {z[4]} | {z[5]} | {topb[n]} |")
    out += ["", "Spannweite über die fünf Seeds (2026–2030), min … max:", "",
            "| Agent | Top-D 0,01 | 0,02 | 0,04 | 0,08 | Top-B |", "|---|---|---|---|---|---|"]
    for a in AGENTEN:
        if len(seeds[a]) < 5:
            continue
        ns = [lauf_name(l) for l in seeds[a]]
        spalten = [[int(topd[n][i]) for n in ns] for i in range(2, 6)] + [[int(topb[n]) for n in ns]]
        out.append(f"| {a} | " + " | ".join(f"{min(s)} … {max(s)}" for s in spalten) + " |")
    out.append("")

    # ---- 6 Schildwall
    out += ["## 6 Schildwall (Basisläufe Seed 2026)", "",
            "Aus der Heldendatei, nur einseitige Partien; der Schildwall ist die einzige Fähigkeit des Schildträgers (Abklingzeit 2).", "",
            "| Agent | Züge des Schildträgers | mit Schildwall-Einsatz | Schildwall verfügbar | Einsatz je verfügbarem Zug |",
            "|---|---|---|---|---|"]
    for a in AGENTEN:
        h = next(r for r in lade_csv(basis[a][4] + "_helden.csv") if r["Held"] == VERAENDERTER_HELD)
        zuege, eins, verf = int(h["RundenAmZug"]), int(h["Faehigkeitseinsaetze"]), int(h["VerfuegbareZuegeFuerFaehigkeit"])
        out.append(f"| {a} | {zuege:_} | {fmt(100 * eins / zuege, 2)} % | {fmt(100 * verf / zuege, 2)} % | {fmt(100 * eins / verf, 2)} % |".replace("_", "."))
    out.append("")

    with open(os.path.join(KENNZAHLEN, "ergebnisse_kapitel8.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


if __name__ == "__main__":
    main()
