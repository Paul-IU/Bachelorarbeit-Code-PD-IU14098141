#!/usr/bin/env python3
"""Trefferschwellen des Schildträgers (Bachelorarbeit).

Aufruf:  python trefferschwellen.py
Ausgabe: trefferschwellen.md

Treffer = ceil(MaxHitPoints / Schaden), ohne Modifikatoren (keine Schildwall-, Mauer-, Lied- oder andere Effekte).
Schadenswerte der Varianten: 70 × (1 + p), gerundet wie im Katalog (HeroCatalogVariants.Scale, Math.Round mit
Rundung zur geraden Zahl): −10 % 63, −5 % 66 (66,5), −2,5 % 68 (68,25), 70, +2,5 % 72 (71,75), +5 % 74 (73,5), +10 % 77.

Gewichtung: Gegnerbegegnungen des Schildträgers in den Basisläufen (Seed 2026) = je Partie jeder gegnerische Held
einmal, über die Paarungsdatei (Spalte Partien).
  einseitig: nur Paarungen mit dem Schildträger auf genau einer Seite (wie die bereinigte Siegquote);
  alle:      zusätzlich Paarungen mit Schildträger auf beiden Seiten, ohne Spiegelpaarung (dann ist der gegnerische
             Schildträger auch ein Gegner).
"""
import os
from math import ceil

from gemeinsam import AGENTEN, GRUNDWERTE, KENNZAHLEN, VERAENDERTER_HELD, finde_laeufe, fmt, lade_csv

SCHADEN = {-10.0: 63, -5.0: 66, -2.5: 68, 0.0: 70, 2.5: 72, 5.0: 74, 10.0: 77}


def begegnungen(paarungen, nur_einseitig):
    zaehl = {h: 0 for h in GRUNDWERTE}
    for r in paarungen:
        a, b = r["TeamA"].split("+"), r["TeamB"].split("+")
        if r["TeamA"] == r["TeamB"]:
            continue
        in_a, in_b = VERAENDERTER_HELD in a, VERAENDERTER_HELD in b
        if nur_einseitig and in_a == in_b:
            continue
        p = int(r["Partien"])
        if in_a:
            for h in b:
                zaehl[h] += p
        if in_b:
            for h in a:
                zaehl[h] += p
    return zaehl


def main():
    helden = sorted(GRUNDWERTE, key=lambda h: -GRUNDWERTE[h][0])
    treffer = {(h, s): ceil(GRUNDWERTE[h][0] / s) for h in helden for s in SCHADEN.values()}
    laeufe = finde_laeufe()
    paar = {a: lade_csv(next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026)[4] + "_paarungen.csv")
            for a in AGENTEN}
    gew = {(a, art): begegnungen(paar[a], art == "einseitig") for a in AGENTEN for art in ("einseitig", "alle")}
    gleich = all(gew[(a, art)] == gew[(AGENTEN[0], art)] for a in AGENTEN for art in ("einseitig", "alle"))

    def stufe(p):
        return "Basis" if p == 0 else f"{fmt(p, 1, True).replace(',0', '')} %".replace("-", "−")

    out = ["# Trefferschwellen des Schildträgers", "",
           "Erzeugt von `Ergebnisse/kennzahlen/trefferschwellen.py`. Treffer = ⌈Lebenspunkte / Schaden⌉ aus vollen "
           "Lebenspunkten, ohne Modifikatoren. Schadenswerte gerundet wie im Katalog (70 × 0,95 = 66,5 → 66; 70 × 1,05 = 73,5 → 74).", "",
           "## 1 Treffer je Held und Schadenswert", "",
           "| Held | LP | " + " | ".join(f"{s} ({stufe(p)})" for p, s in SCHADEN.items()) + " |",
           "|---|---|" + "---|" * len(SCHADEN)]
    for h in helden:
        zellen = []
        for s in SCHADEN.values():
            t = treffer[(h, s)]
            zellen.append(f"**{t}**" if t != treffer[(h, 70)] else str(t))
        out.append(f"| {h} | {GRUNDWERTE[h][0]} | " + " | ".join(zellen) + " |")
    out += ["", "Fett = andere Trefferzahl als bei 70.", "",
            "## 2 Änderungen gegenüber 70", "", "| Schaden | Stufe | Helden mit anderer Trefferzahl |", "|---|---|---|"]
    for p, s in SCHADEN.items():
        if p == 0:
            continue
        geaendert = [f"{h} ({treffer[(h, 70)]} → {treffer[(h, s)]})" for h in helden if treffer[(h, s)] != treffer[(h, 70)]]
        out.append(f"| {s} | {stufe(p)} | {', '.join(geaendert) or 'keine'} |")

    out += ["", "## 3 Gewichtet nach Gegnerbegegnungen in den Basisläufen", ""]
    w = gew[(AGENTEN[0], "einseitig")]
    summe = sum(w.values())
    out += [("Die Gewichte sind bei allen vier Agenten identisch (jede Paarung hat 72 Partien), deshalb gilt die Tabelle für alle Agenten."
             if gleich else "Achtung: Die Gewichte unterscheiden sich zwischen den Agenten; Tabelle für " + AGENTEN[0] + "."), "",
            "| Held | Anteil an den Gegnerbegegnungen (einseitig) | Anteil (alle Paarungen mit Schildträger) |", "|---|---|---|"]
    wa = gew[(AGENTEN[0], "alle")]
    summe_a = sum(wa.values())
    for h in helden:
        out.append(f"| {h} | {fmt(100 * w[h] / summe, 2)} % | {fmt(100 * wa[h] / summe_a, 2)} % |")
    out += ["", "Anteil der Gegnerbegegnungen, bei denen sich die Trefferzahl gegenüber 70 ändert:", "",
            "| Stufe | Abschwächung (einseitig) | Verstärkung (einseitig) | Abschwächung (alle) | Verstärkung (alle) |",
            "|---|---|---|---|---|"]
    for p in (2.5, 5.0, 10.0):
        zellen = []
        for gewichte, s_ in ((w, summe), (wa, summe_a)):
            for q in (-p, p):
                anteil = sum(gewichte[h] for h in helden if treffer[(h, SCHADEN[q])] != treffer[(h, 70)]) / s_
                zellen.append(f"{fmt(100 * anteil, 1)} %")
        out.append(f"| ±{fmt(p, 1).replace(',0', '')} % | {zellen[0]} | {zellen[1]} | {zellen[2]} | {zellen[3]} |")
    out += ["", "Einseitig = nur Paarungen mit dem Schildträger auf genau einer Seite (Grundlage der bereinigten Siegquote); "
            "alle = zusätzlich Paarungen mit Schildträger auf beiden Seiten (ohne Spiegel), dort zählt der gegnerische "
            "Schildträger als Gegner.", ""]
    out += ["Mittlere relative Änderung der Trefferzahl über die acht Gegner (einseitig, gleich gewichtet; |Treffer neu − Treffer 70| / Treffer 70):", "",
            "| Stufe | Abschwächung | Verstärkung | Verhältnis |", "|---|---|---|---|"]
    gegner = [h for h in helden if w[h] > 0]
    for p in (2.5, 5.0, 10.0):
        rel = [sum(w[h] * abs(treffer[(h, SCHADEN[q])] - treffer[(h, 70)]) / treffer[(h, 70)] for h in gegner) / summe for q in (-p, p)]
        out.append(f"| ±{fmt(p, 1).replace(',0', '')} % | {fmt(100 * rel[0], 2)} % | {fmt(100 * rel[1], 2)} % | {fmt(rel[0] / rel[1], 2)} |")
    out.append("")
    out += EINSCHAETZUNG
    with open(os.path.join(KENNZAHLEN, "trefferschwellen.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))


EINSCHAETZUNG = [
    "## 4 Einschätzung", "",
    "Beobachtetes Verhältnis |Δ(−p)| / |Δ(+p)| der Schildträger-Siegquote bei Schaden (ergebnisse_kapitel8.md, Abschnitt 2): "
    "±2,5 % 2,48–4,32, ±5 % 1,68–2,83, ±10 % 1,40–1,85 (byactiontype, uniform, aggressive, defensive).", "",
    "- **Die Asymmetrie erklären die Schwellen gut.** Auf jeder Stufe ändert die Abschwächung die Trefferzahl gegen mehr "
    "Gegner als die Verstärkung (37,5 gegen 25 %, 62,5 gegen 25 %, 87,5 gegen 50 % der Begegnungen). Vor allem trifft sie "
    "andere Helden: Die Abschwächung kostet einen zusätzlichen Treffer gegen die Helden mit wenig Lebenspunkten "
    "(Bogenschütze 4 → 5, Berserker 6 → 7, Musiker 7 → 8), also relativ viel (+14 bis +25 %). Die Verstärkung spart "
    "dagegen zuerst Treffer gegen Wand und Hugger (15 → 14, 8 → 7), relativ wenig (−7 bis −13 %). Gemessen an der mittleren "
    "relativen Änderung der Trefferzahl wirkt die Abschwächung 2,5- bis 3,7-mal so stark, in derselben Größenordnung wie beobachtet.",
    "- **Das Abnehmen mit wachsender Stufe erklären sie nur teilweise.** Von ±5 auf ±10 % sinkt das Schwellenverhältnis "
    "(3,74 → 2,15), weil bei +10 % auch Illusionist und Elfe eine Schwelle überschreiten; das passt zur Beobachtung. Nach "
    "dem Schwellenmodell müsste ±2,5 % aber schwächer asymmetrisch sein als ±5 % (2,52 gegen 3,74), beobachtet ist es umgekehrt. "
    "Bei ±2,5 % sind die Δ klein (0,5–0,7 pp bei Verstärkung) und das Verhältnis entsprechend unsicher; dass es dort am "
    "größten ist, ist mit den Schwellen allein nicht erklärt.",
    "- **Grenzen der Rechnung.** Gerechnet ist ein Duell aus vollen Lebenspunkten ohne Modifikatoren. In der Partie greifen "
    "meist mehrere Helden dasselbe Ziel an, die Schwelle liegt dann beim gemeinsamen Schaden; Heilung (Elfe), "
    "Schadensminderung (Schildwall, Mauer), Kampflied und Teil-Lebenspunkte verschieben die Schwellen. Die Trefferschwellen "
    "sind deshalb ein plausibler Mechanismus für die Richtung der Asymmetrie, kein quantitatives Modell.", "",
]


if __name__ == "__main__":
    main()
