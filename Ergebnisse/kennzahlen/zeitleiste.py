#!/usr/bin/env python3
"""Zeitleiste der Simulationsläufe (Bachelorarbeit) aus laufuebersicht.csv. Rechnet keine Läufe neu.

Aufruf:  python zeitleiste.py
Ausgabe: zeitleiste.png (300 dpi) und zeitleiste.svg; Bericht auf der Konsole.

Darstellung:
  - je Tag eine Spalte, senkrecht die Uhrzeit (0-24 Uhr); Läufe über Mitternacht auf beide Tage verteilt
  - jeder Tag wird minutenweise gerastert; je Minute zählt, welche Läufe aktiv sind
  - mehrere Läufe DESSELBEN Agenten: Helligkeit (L im Lab-Farbraum) je zusätzlichem Lauf -20 %, höchstens bis 40 %
  - verschiedene Agenten: danach Mittelwert der (abgedunkelten) Agentenfarben im Lab-Farbraum, jeder Agent einmal
  - abgebrochene Läufe schraffiert, Läufe mit geschätzten Zeiten (Zeitquelle nicht Fortschrittsdatei) gestrichelt umrandet
  - Läufe ohne bekannten Start oder ohne bekanntes Ende werden nicht gezeichnet (Bericht)
Zeitzone wie in der CSV (+02:00); die Zeiten werden als Ortszeit gelesen.
"""
import csv
import os
from collections import defaultdict
from datetime import datetime, timedelta

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.patches import Patch, Rectangle

HIER = os.path.dirname(os.path.abspath(__file__))

# ---- Parameter
NULL_UHR_UNTEN = True        # False: 0 Uhr oben, 24 Uhr unten
SPALTENBREITE = 0.8          # Anteil des Tagesabstands
HILFSLINIEN_STUNDEN = 2

# Okabe-Ito-Palette (farbenblindtauglich; geprüft mit dem Palettenvalidator der dataviz-Vorgaben)
FARBEN = {
    "byactiontype": "#56B4E9",    # Himmelblau
    "uniform": "#009E73",         # Blaugrün
    "aggressiv": "#D55E00",       # Zinnoberrot
    "defensiv": "#0072B2",        # Blau
    "vorausschauend": "#CC79A7",  # Rötliches Lila
    "Prüf-/Testlauf, neutral": "#999999",
}
GRUPPEN = list(FARBEN)


def gruppe(z):
    """Farbgruppe eines Laufs."""
    if z["art"] in ("Prüflauf", "Testlauf") or z["agent"] == "neutral":
        return "Prüf-/Testlauf, neutral"
    if z["agent"].startswith("lookahead"):
        return "vorausschauend"
    return {"aggressive": "aggressiv", "defensive": "defensiv"}.get(z["agent"], z["agent"])


# ---- sRGB <-> CIELAB (D65)
def _lin(c):
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def _gam(c):
    return np.where(c <= 0.0031308, 12.92 * c, 1.055 * np.clip(c, 0, None) ** (1 / 2.4) - 0.055)


M = np.array([[0.4124564, 0.3575761, 0.1804375], [0.2126729, 0.7151522, 0.0721750], [0.0193339, 0.1191920, 0.9503041]])
WEISS = np.array([0.95047, 1.0, 1.08883])


def rgb_lab(rgb):
    xyz = M @ _lin(np.asarray(rgb, float)) / WEISS
    f = np.where(xyz > (6 / 29) ** 3, np.cbrt(xyz), xyz / (3 * (6 / 29) ** 2) + 4 / 29)
    return np.array([116 * f[1] - 16, 500 * (f[0] - f[1]), 200 * (f[1] - f[2])])


def lab_rgb(lab):
    fy = (lab[0] + 16) / 116
    f = np.array([fy + lab[1] / 500, fy, fy - lab[2] / 200])
    xyz = np.where(f > 6 / 29, f ** 3, 3 * (6 / 29) ** 2 * (f - 4 / 29)) * WEISS
    return np.clip(_gam(np.linalg.inv(M) @ xyz), 0, 1)


def hex_rgb(h):
    return np.array([int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)])


LAB = {g: rgb_lab(hex_rgb(FARBEN[g])) for g in GRUPPEN}


def abgedunkelt(g, n):
    """Lab-Farbe der Gruppe bei n gleichzeitigen Läufen: L * max(0,4; 1 - 0,2 * (n - 1))."""
    lab = LAB[g].copy()
    lab[0] *= max(0.4, 1 - 0.2 * (n - 1))
    return lab


def mischfarbe(zaehlung):
    """{Gruppe: Anzahl aktiver Läufe} -> RGB: erst je Gruppe abdunkeln, dann Lab-Mittelwert."""
    labs = [abgedunkelt(g, n) for g, n in zaehlung.items() if n > 0]
    return lab_rgb(np.mean(labs, axis=0))


def lies(pfad):
    def zeit(s):
        return datetime.fromisoformat(s).replace(tzinfo=None) if s else None
    with open(pfad, encoding="utf-8-sig") as f:
        return [dict(r, start_dt=zeit(r["start"]), ende_dt=zeit(r["ende"])) for r in csv.DictReader(f, delimiter=";")]


def segmente(z):
    """(Tag, Startminute, Endminute) je Kalendertag, halboffen; mindestens eine Minute, damit Fehlstarts sichtbar bleiben."""
    a, e = z["start_dt"], z["ende_dt"]
    if e <= a:
        e = a + timedelta(minutes=1)
    teile = []
    while a < e:
        mitternacht = datetime(a.year, a.month, a.day) + timedelta(days=1)
        stop = min(e, mitternacht)
        m0 = a.hour * 60 + a.minute
        # halboffen [m0, m1): Ende abgerundet, damit die Übergabeminute zweier Kettenläufe nicht doppelt zählt
        m1 = 1440 if stop == mitternacht else stop.hour * 60 + stop.minute
        teile.append((a.date(), m0, max(m1, m0 + 1)))
        a = stop
    return teile


def main():
    laeufe = lies(os.path.join(HIER, "laufuebersicht.csv"))
    alle_daten = [z["start_dt"] or z["ende_dt"] for z in laeufe if z["start_dt"] or z["ende_dt"]]
    tag0, tag1 = min(alle_daten).date(), max(z["ende_dt"] or z["start_dt"] for z in laeufe if z["ende_dt"] or z["start_dt"]).date()
    tage = [tag0 + timedelta(days=i) for i in range((tag1 - tag0).days + 1)]
    index = {t: i for i, t in enumerate(tage)}

    gezeichnet = [z for z in laeufe if z["start_dt"] and z["ende_dt"]]
    weggelassen = [z for z in laeufe if not (z["start_dt"] and z["ende_dt"])]

    # Minutenraster: je Tag, Minute und Gruppe die Zahl aktiver Läufe
    raster = np.zeros((len(tage), 1440, len(GRUPPEN)), dtype=int)
    rahmen = []  # (Tagesindex, m0, m1, abgebrochen, geschätzt)
    for z in gezeichnet:
        g = GRUPPEN.index(gruppe(z))
        for tag, m0, m1 in segmente(z):
            raster[index[tag], m0:m1, g] += 1
            rahmen.append((index[tag], m0, m1, z["status"] == "abgebrochen", z["zeitquelle"] != "Fortschrittsdatei"))

    plt.rcParams.update({"font.size": 10, "font.family": "DejaVu Sans"})
    fig, ax = plt.subplots(figsize=(11.69, 8.27))  # A4 quer
    for i in range(len(tage)):
        bild = np.zeros((1440, 1, 4))
        for m in range(1440):
            zaehl = {GRUPPEN[g]: raster[i, m, g] for g in range(len(GRUPPEN)) if raster[i, m, g] > 0}
            if zaehl:
                bild[m, 0, :3] = mischfarbe(zaehl)
                bild[m, 0, 3] = 1
        ax.imshow(bild, extent=(i - SPALTENBREITE / 2, i + SPALTENBREITE / 2, 0, 24), origin="lower",
                  aspect="auto", interpolation="nearest", zorder=2)

    for i, m0, m1, abgebrochen, geschaetzt in rahmen:
        x, y, h = i - SPALTENBREITE / 2, m0 / 60, (m1 - m0) / 60
        if abgebrochen:
            ax.add_patch(Rectangle((x, y), SPALTENBREITE, h, fill=False, hatch="////", edgecolor="#222222",
                                   linewidth=0, zorder=3))
        if geschaetzt:
            ax.add_patch(Rectangle((x, y), SPALTENBREITE, h, fill=False, edgecolor="#222222", linewidth=0.8,
                                   linestyle=(0, (3, 2)), zorder=4))

    ax.set_xlim(-0.6, len(tage) - 0.4)
    ax.set_ylim((0, 24) if NULL_UHR_UNTEN else (24, 0))
    ax.set_xticks(range(len(tage)), [t.strftime("%d.%m.") for t in tage], rotation=90)
    ax.set_yticks(range(0, 25, HILFSLINIEN_STUNDEN), [f"{h:02d}:00" for h in range(0, 25, HILFSLINIEN_STUNDEN)])
    ax.grid(axis="y", color="#d9d9d9", linewidth=0.6, zorder=0)
    ax.set_axisbelow(True)
    for s in ("top", "right"):
        ax.spines[s].set_visible(False)
    ax.set_xlabel("Datum (2026)")
    ax.set_ylabel("Uhrzeit (Ortszeit, UTC+02:00)")
    ax.set_title(f"Zeitleiste der Simulationsläufe ({tag0:%d.%m.} – {tag1:%d.%m.%Y}), {len(gezeichnet)} Läufe", fontsize=13)

    # Legende
    def feld(rgb, text, **kw):
        return Patch(facecolor=rgb, edgecolor="#222222" if kw else "none", label=text, **kw)
    eintraege = [feld(FARBEN[g], g) for g in GRUPPEN]
    eintraege.append(feld(mischfarbe({"byactiontype": 1, "aggressiv": 1}), "Mischung: byactiontype + aggressiv"))
    eintraege.append(feld(lab_rgb(abgedunkelt("defensiv", 2)), "defensiv, 2 Läufe parallel"))
    eintraege.append(feld(lab_rgb(abgedunkelt("defensiv", 3)), "defensiv, 3 Läufe parallel"))
    eintraege.append(Patch(facecolor="white", edgecolor="#222222", hatch="////", linewidth=0, label="abgebrochen"))
    eintraege.append(Patch(facecolor="white", edgecolor="#222222", linestyle="--", linewidth=0.8, label="Zeiten geschätzt"))
    ax.legend(handles=eintraege, loc="upper left", bbox_to_anchor=(1.01, 1), frameon=False, fontsize=9, title="Agent / Kennzeichnung")
    fig.tight_layout()
    fig.savefig(os.path.join(HIER, "zeitleiste.png"), dpi=300)
    fig.savefig(os.path.join(HIER, "zeitleiste.svg"))

    # Bericht
    print(f"Gezeichnet: {len(gezeichnet)} Läufe; weggelassen: {len(weggelassen)}")
    for z in weggelassen:
        grund = "Start unbekannt" if not z["start_dt"] else "Ende unbekannt"
        print(f"  {z['lauf_id']} {z['art']} {z['agent']} ({grund}; Ende {z['ende'] or '-'}) {z['datei']}")
    print("Tage mit gleichzeitig laufenden verschiedenen Gruppen (Minuten):")
    for i, tag in enumerate(tage):
        aktiv = raster[i] > 0
        mehrere = aktiv.sum(axis=1) >= 2
        if mehrere.any():
            kombis = defaultdict(int)
            for m in np.where(mehrere)[0]:
                kombis[" + ".join(GRUPPEN[g] for g in np.where(aktiv[m])[0])] += 1
            print(f"  {tag:%d.%m.}: " + "; ".join(f"{k} ({v} min)" for k, v in sorted(kombis.items(), key=lambda kv: -kv[1])))
    gleich = [(tage[i], GRUPPEN[g], int(raster[i, :, g].max())) for i in range(len(tage)) for g in range(len(GRUPPEN)) if raster[i, :, g].max() >= 2]
    print("Gleiche Gruppe mehrfach parallel:", "; ".join(f"{t:%d.%m.} {g} bis {n}x" for t, g, n in gleich) or "keine")


if __name__ == "__main__":
    main()
