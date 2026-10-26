#!/usr/bin/env python3
"""Werte für die Bachelorarbeit, Stand f0286a5 (Seiten- und Id-Korrektur), geordnet nach Kapiteln, neu (alt).

Aufruf:  python werte_neu.py
Ausgabe: werte_neu.md, heldensiegquoten.csv

Neu = Läufe der Neuerhebung (Oktober 2026, seitensymmetrische Regeln) in Ergebnisse/.
Alt = Läufe des Stands e6953bd (Seitenkorrektur, Ids noch in absoluter Startfeld-Reihenfolge) in
Ergebnisse/Archiv_vor_Idkorrektur/ (gleiche relative Pfade).
Beide mit denselben Definitionen: Unentschieden als halber Sieg, Kompositionen ohne Spiegelpaarung, Basisläufe
Seed 2026, ohne die Zusatzvariante dmg +20 %. Jede Kennzahl wird mit derselben Funktion auf beiden Ständen
berechnet. "‡" markiert deutliche Änderungen (Schwellen siehe DEUTLICH).
"""
import csv
import gzip
import os
import re
from statistics import median

import numpy as np

from gemeinsam import AGENTEN, ERGEBNISSE, GRUNDWERTE, KENNZAHLEN, VERAENDERTER_HELD, fmt, helden_quote, komp_quote, lade_csv, spearman
from kennzahlen import bradley_terry, bt_guete, matrix

NEU = ERGEBNISSE
ALT = os.path.join(ERGEBNISSE, "Archiv_vor_Idkorrektur")   # Stand e6953bd
ARTEN = ("dmg", "hp", "ability")
STUFEN = (10.0, 5.0, 2.5, -2.5, -5.0, -10.0)
ZUFALL = ("byactiontype", "uniform")
MARKE = "‡"
# deutliche Änderung: Prozentwerte ab 1 pp, Rundenzahlen ab 1 Runde, Rangkorrelationen ab 0,10, Rang geändert
DEUTLICH = {"pp": 1.0, "runden": 1.0, "rho": 0.10}
HELDEN = sorted(GRUNDWERTE)
ERGEBNIS_SPALTEN = ("Sieger", "Endgrund", "Aufloesung", "Runden", "UeberlebendeTeamA", "UeberlebendeTeamB", "RestHpTeamA", "RestHpTeamB")


# ---------------------------------------------------------------- Pfade und Laden
def basis(wurzel, a, seed=2026):
    if seed == 2026:
        return os.path.join(wurzel, "basislauf_2026_09_26_partien", f"basislauf_{a}")
    return os.path.join(wurzel, f"seed_{seed}", f"basislauf_{a}_seed{seed}")


def vertauscht(wurzel, a):
    return os.path.join(wurzel, "seiten_vertauscht", f"basislauf_{a}_vertauscht")


def variante(wurzel, a, art, p):
    return os.path.join(wurzel, art, f"{art}_{p:g}", "schildtraeger", f"schildtraeger_{art}_{p:g}_{a}")


_cache = {}


def paar(praefix):
    if praefix not in _cache:
        _cache[praefix] = lade_csv(praefix + "_paarungen.csv")
    return _cache[praefix]


def partien(praefix):
    key = praefix + "#partien"
    if key not in _cache:
        with gzip.open(praefix + "_partien.csv.gz", "rt", encoding="utf-8-sig", newline="") as f:
            _cache[key] = list(csv.DictReader(f, delimiter=";"))
    return _cache[key]


def etiketten(wurzel, a):
    """(geprüfte Partien, Abweichungen) aus dem Protokoll des Laufs mit Etiketten-Prüfung."""
    kandidaten = [basis(wurzel, a) + "_lauf.log",
                  os.path.join(wurzel, "basislauf_2026_09_26_partien", f"lauf_{a}.log"),
                  os.path.join(wurzel, "etiketten_pruefung_heuristik", f"lauf_{a}.log")]
    for k in kandidaten:
        if os.path.exists(k):
            m = re.search(r"Etiketten-Pr.fung: (\d+) Partien gepr.ft, (\d+) Abweichung", open(k, encoding="utf-8", errors="replace").read())
            if m:
                return int(m.group(1)), int(m.group(2))
    return None


# ---------------------------------------------------------------- Formatierung
def zelle(neu, alt, nd=2, art="pp", vz=False, einheit=""):
    """'neu (alt)', mit Marke bei deutlicher Änderung."""
    if alt is None:
        return f"{fmt(neu, nd, vz)}{einheit} (–)"
    deutlich = abs(neu - alt) >= DEUTLICH[art]
    return f"{fmt(neu, nd, vz)}{einheit} ({fmt(alt, nd, vz)}{einheit})" + (f" {MARKE}" if deutlich else "")


def text_zelle(neu, alt):
    return neu if neu == alt else f"{neu} ({alt}) {MARKE}"


# ---------------------------------------------------------------- Kennzahlen je Stand
def partie_kennzahlen(praefix, maxhp_komp):
    z = partien(praefix)
    n = len(z)
    runden = np.mean([int(r["Runden"]) for r in z])
    limit = sum(r["Endgrund"] == "RoundLimit" for r in z)
    stufe = {k: sum(r["Aufloesung"] == k for r in z) for k in ("SurvivingHeroes", "HitPointFraction", "Draw")}
    entschieden = [r for r in z if r["Sieger"] in ("TeamA", "TeamB")]
    start = sum(r["Sieger"] == r["Startteam"] for r in entschieden)
    ohne_schaden = sum(int(r["RestHpTeamA"]) >= maxhp_komp[r["TeamA"]] and int(r["RestHpTeamB"]) >= maxhp_komp[r["TeamB"]] for r in z)
    return dict(n=n, runden=runden, limit=100 * limit / n, stufe={k: 100 * v / n for k, v in stufe.items()},
                remis=100 * stufe["Draw"] / n, start=100 * start / len(entschieden), ohne_schaden=100 * ohne_schaden / n)


def spiegel_team_a(praefix):
    sp = [r for r in paar(praefix) if r["TeamA"] == r["TeamB"]]
    sa, sb = sum(int(r["SiegeTeamA"]) for r in sp), sum(int(r["SiegeTeamB"]) for r in sp)
    return 100 * sa / (sa + sb)


def gespiegelt_gleich(p1, p2):
    a, b = partien(p1), partien(p2)
    assert len(a) == len(b)
    return sum(any(x[k] != y[k] for k in ERGEBNIS_SPALTEN) for x, y in zip(a, b)), len(a)


def seed_sd(wurzel, a):
    seeds = [basis(wurzel, a, s) for s in (2026, 2027, 2028, 2029, 2030)]
    hq = [helden_quote(paar(s)) for s in seeds]
    kq = [komp_quote(paar(s)) for s in seeds]
    sd_h = [np.std([q[h] for q in hq], ddof=1) for h in HELDEN]
    sd_k = [np.std([q[k] for q in kq], ddof=1) for k in kq[0]]
    return np.mean(sd_h), max(sd_h), np.mean(sd_k), max(sd_k)


def grundwertsumme():
    hp = np.array([GRUNDWERTE[h][0] for h in HELDEN], float)
    dm = np.array([GRUNDWERTE[h][1] for h in HELDEN], float)
    return dict(zip(HELDEN, (hp - hp.mean()) / hp.std() + (dm - dm.mean()) / dm.std()))


def trennung_und_reaktion(wurzel, a):
    """Anzahl vollständig getrennter Varianten und Spearman Basis/Reaktion je Variante (wie kompositionen.py)."""
    bq = komp_quote(paar(basis(wurzel, a)))
    mit = [k for k in bq if VERAENDERTER_HELD in k.split("+")]
    ohne = [k for k in bq if k not in mit]
    getrennt, rho = 0, {}
    for art in ARTEN:
        for p in STUFEN:
            vq = komp_quote(paar(variante(wurzel, a, art, p)))
            r = 1 if p > 0 else -1
            dm = np.array([(vq[k] - bq[k]) * r for k in mit])
            do = np.array([(vq[k] - bq[k]) * r for k in ohne])
            getrennt += int(np.sum(do > dm.min()) == 0)
            rho[(art, p)] = spearman([bq[k] for k in mit], list(dm))
    return getrennt, rho


def stand(wurzel):
    maxhp_komp = {}
    w = {}
    for a in AGENTEN:
        b = basis(wurzel, a)
        if not maxhp_komp:
            for r in paar(b):
                for k in (r["TeamA"], r["TeamB"]):
                    maxhp_komp[k] = sum(GRUNDWERTE[h][0] for h in k.split("+"))
        hq, kq = helden_quote(paar(b)), komp_quote(paar(b))
        komps = sorted(kq)
        W, siege, spiele = matrix(paar(b), komps)
        mae = bt_guete(bradley_terry(siege, spiele), W)[0]
        h_bogen = next(r for r in lade_csv(b + "_helden.csv") if r["Held"] == "bogenschuetze")
        getrennt, rho = trennung_und_reaktion(wurzel, a)
        w[a] = dict(pk=partie_kennzahlen(b, maxhp_komp), st=hq[VERAENDERTER_HELD], hq=hq, kq=kq,
                    etikett=etiketten(wurzel, a), spiegel_n=spiegel_team_a(b),
                    spiegel_v=spiegel_team_a(vertauscht(wurzel, a)) if os.path.exists(vertauscht(wurzel, a) + "_paarungen.csv") else None,
                    mae=mae, getrennt=getrennt, rho=rho,
                    bogen_je_partie=int(h_bogen["Faehigkeitseinsaetze"]) / int(h_bogen["Partien"]),
                    bogen_je_zug=100 * int(h_bogen["Faehigkeitseinsaetze"]) / int(h_bogen["RundenAmZug"]))
        if a in ("aggressive", "defensive") and os.path.exists(vertauscht(wurzel, a) + "_partien.csv.gz"):
            w[a]["gespiegelt"] = gespiegelt_gleich(b, vertauscht(wurzel, a))
        if a in ZUFALL:
            w[a]["seed_sd"] = seed_sd(wurzel, a)
    return w


# ---------------------------------------------------------------- Ausgabe
def main():
    neu, alt = stand(NEU), stand(ALT)
    gws = grundwertsumme()
    out = ["# Werte für die Bachelorarbeit, Stand f0286a5", "",
           "Erzeugt von `Ergebnisse/kennzahlen/werte_neu.py`. Je Wert **neu (alt)**: neu = Neuerhebung mit seitensymmetrischen "
           "Regeln und Id-Vergabe aus Sicht der eigenen Blüte (alle 88 Läufe, f0286a5), alt = Stand e6953bd (Archiv_vor_Idkorrektur). Unentschieden als halber Sieg, Kompositionen "
           "ohne Spiegelpaarung, Basisläufe Seed 2026, ohne dmg +20 %. "
           f"**{MARKE} = deutliche Änderung** (Prozentwerte ≥ 1 pp, Runden ≥ 1, Rangkorrelation ≥ 0,10, Rang oder Aussage geändert).", ""]

    # ---- 6.1
    out += ["## 6.1 Partien", "",
            "| Agent | mittlere Partiedauer (Runden) | am Rundenlimit | davon a) mehr Helden | b) mehr LP-Anteil | c) unentschieden | Unentschieden gesamt |",
            "|---|---|---|---|---|---|---|"]
    for a in AGENTEN:
        n, o = neu[a]["pk"], alt[a]["pk"]
        out.append(f"| {a} | {zelle(n['runden'], o['runden'], 2, 'runden')} | {zelle(n['limit'], o['limit'], 2, einheit=' %')} | "
                   f"{zelle(n['stufe']['SurvivingHeroes'], o['stufe']['SurvivingHeroes'], 2, einheit=' %')} | "
                   f"{zelle(n['stufe']['HitPointFraction'], o['stufe']['HitPointFraction'], 2, einheit=' %')} | "
                   f"{zelle(n['stufe']['Draw'], o['stufe']['Draw'], 2, einheit=' %')} | {zelle(n['remis'], o['remis'], 2, einheit=' %')} |")
    out += ["", "Anteile an allen 257.040 Partien; die drei Auflösungsstufen ergeben zusammen den Anteil am Rundenlimit.", ""]

    # ---- 7.1
    out += ["## 7.1 Schildträger und Aufladen", "",
            "| Agent | Siegquote Schildträger (Basis) |", "|---|---|"]
    out += [f"| {a} | {zelle(neu[a]['st'], alt[a]['st'], 2, einheit=' %')} |" for a in AGENTEN]
    n, o = neu["aggressive"], alt["aggressive"]
    out += ["", f"**Aufladen des Bogenschützen beim aggressiven Agenten:** {fmt(n['bogen_je_partie'], 4)} Einsätze je Partie mit "
                f"Bogenschütze ({fmt(o['bogen_je_partie'], 4)}); in {fmt(n['bogen_je_zug'], 3)} % seiner Züge ({fmt(o['bogen_je_zug'], 3)} %). "
                "Der Anteil der Partien mit mindestens einem Aufladen steht nicht in den Ergebnisdateien (die Heldendatei zählt nur "
                "Einsätze); er ließe sich nur durch Nachspielen ermitteln. Bei weniger als 0,01 Einsätzen je Partie liegt er "
                "höchstens bei diesem Wert, also unter 1 %.", ""]

    # ---- 7.3
    out += ["## 7.3 Verifikation", "", "### Etiketten-Prüfung (Basislauf Seed 2026, jede Partie zusätzlich mit vertauschten Etiketten)", "",
            "| Agent | neu | alt |", "|---|---|---|"]
    for a in AGENTEN:
        f = lambda e: f"{e[0]:_} geprüft, {e[1]} Abweichungen".replace("_", ".") if e else "–"
        out.append(f"| {a} | {f(neu[a]['etikett'])} | {f(alt[a]['etikett'])} |")
    out += ["", "### Spiegelpartien: Anteil von Team A an den entschiedenen Partien (84 × 72)", "",
            "| Agent | normal (Team A auf Blüte A) | vertauscht (Team A auf Blüte B) |", "|---|---|---|"]
    for a in AGENTEN:
        out.append(f"| {a} | {zelle(neu[a]['spiegel_n'], alt[a]['spiegel_n'], 2, einheit=' %')} | "
                   f"{zelle(neu[a]['spiegel_v'], alt[a]['spiegel_v'], 2, einheit=' %') if neu[a]['spiegel_v'] is not None else '–'} |")
    out += ["", "### Basislauf und vertauschter Basislauf, Partie für Partie (heuristische Agenten)", "",
            "Verglichen: Sieger, Endgrund, Auflösung, Runden, Überlebende und Rest-LP je Team (Etiketten bleiben beim Seitentausch erhalten).", "",
            "| Agent | abweichende Partien neu | alt |", "|---|---|---|"]
    for a in ("aggressive", "defensive"):
        dn, nn = neu[a]["gespiegelt"]
        da, na = alt[a]["gespiegelt"]
        out.append(f"| {a} | **{dn:_} von {nn:_}** ({'alle exakt gespiegelt' if dn == 0 else 'NICHT alle gespiegelt'}) | {da:_} von {na:_} |".replace("_", "."))
    out += ["", "### Siegquote des beginnenden Teams (entschiedene Partien, alle 257.040)", "", "| Agent | beginnendes Team |", "|---|---|"]
    out += [f"| {a} | {zelle(neu[a]['pk']['start'], alt[a]['pk']['start'], 2, einheit=' %')} |" for a in AGENTEN]
    out += ["", "### Streuung über die fünf Seeds 2026–2030 (SD, n − 1, pp)", "",
            "| Agent | Helden: mittlere SD | Helden: größte SD | Kompositionen: mittlere SD | Kompositionen: größte SD |", "|---|---|---|---|---|"]
    for a in ZUFALL:
        sn, so = neu[a]["seed_sd"], alt[a]["seed_sd"]
        out.append(f"| {a} | " + " | ".join(zelle(x, y, 3) for x, y in zip(sn, so)) + " |")
    out += ["", "### Zufallsagenten vor und nach der Korrektur (gleiche Seeds)", "",
            "| Agent | Partien mit anderem Sieger | mit anderem Ergebnis | größte Änderung einer Heldensiegquote |", "|---|---|---|---|"]
    for a in ZUFALL:
        pn, pa = partien(basis(NEU, a)), partien(basis(ALT, a))
        sieger = sum(x["Sieger"] != y["Sieger"] for x, y in zip(pn, pa))
        anders = sum(any(x[k] != y[k] for k in ERGEBNIS_SPALTEN) for x, y in zip(pn, pa))
        dmax = max(abs(neu[a]["hq"][h] - alt[a]["hq"][h]) for h in HELDEN)
        out.append(f"| {a} | {sieger:_} ({fmt(100 * sieger / len(pn), 2)} %) | {anders:_} ({fmt(100 * anders / len(pn), 2)} %) | {fmt(dmax, 3)} pp |".replace("_", "."))
    out.append("")

    # ---- 8.3.1
    out += ["## 8.3.1 Siegquoten und Rangfolgen", "", "### Helden (Siegquote in %, Rang)", "",
            "| Held | " + " | ".join(AGENTEN) + " |", "|---|" + "---|" * len(AGENTEN)]
    rang = {(s, a): {h: i + 1 for i, h in enumerate(sorted(HELDEN, key=lambda h: -w[a]["hq"][h]))} for s, w in (("n", neu), ("a", alt)) for a in AGENTEN}
    mittel = {h: np.mean([neu[a]["hq"][h] for a in AGENTEN]) for h in HELDEN}
    for h in sorted(HELDEN, key=lambda h: -mittel[h]):
        zellen = []
        for a in AGENTEN:
            rn, ra = rang[("n", a)][h], rang[("a", a)][h]
            zellen.append(f"{zelle(neu[a]['hq'][h], alt[a]['hq'][h], 2)}, Rang {rn}" + (f" ({ra}) {MARKE}" if rn != ra else ""))
        out.append(f"| {h} | " + " | ".join(zellen) + " |")
    out += ["", "### Spearman zwischen den Agenten", "", "| Paar | Helden | Kompositionen |", "|---|---|---|"]
    for i, x in enumerate(AGENTEN):
        for y in AGENTEN[i + 1:]:
            rh = [spearman([w[x]["hq"][h] for h in HELDEN], [w[y]["hq"][h] for h in HELDEN]) for w in (neu, alt)]
            ks = sorted(neu[x]["kq"])
            rk = [spearman([w[x]["kq"][k] for k in ks], [w[y]["kq"][k] for k in ks]) for w in (neu, alt)]
            out.append(f"| {x} / {y} | {zelle(rh[0], rh[1], 3, 'rho')} | {zelle(rk[0], rk[1], 3, 'rho')} |")
    out += ["", "### Die 84 Kompositionen je Agent", "", "| Agent | Mittelwert | SD | Minimum | Maximum | Partien ganz ohne Schaden |", "|---|---|---|---|---|---|"]
    for a in AGENTEN:
        vn, va = np.array(list(neu[a]["kq"].values())), np.array(list(alt[a]["kq"].values()))
        out.append(f"| {a} | {zelle(vn.mean(), va.mean(), 2, einheit=' %')} | {zelle(vn.std(ddof=1), va.std(ddof=1), 2, einheit=' pp')} | "
                   f"{zelle(vn.min(), va.min(), 2, einheit=' %')} | {zelle(vn.max(), va.max(), 2, einheit=' %')} | "
                   f"{zelle(neu[a]['pk']['ohne_schaden'], alt[a]['pk']['ohne_schaden'], 2, einheit=' %')} |")
    out += ["", "Partie ganz ohne Schaden: beide Teams beenden die Partie mit den vollen Lebenspunkten ihrer Komposition.", ""]

    # ---- 8.3.2
    out += ["## 8.3.2 Vollständig getrennte Varianten (von 18 je Agent)", "", "| Agent | getrennt |", "|---|---|"]
    out += [f"| {a} | {neu[a]['getrennt']} ({alt[a]['getrennt']})" + (f" {MARKE}" if abs(neu[a]['getrennt'] - alt[a]['getrennt']) >= 2 else "") + " |" for a in AGENTEN]
    out += ["", f"Zusammen: {sum(neu[a]['getrennt'] for a in AGENTEN)} ({sum(alt[a]['getrennt'] for a in AGENTEN)}) von 72.", ""]

    # ---- 8.3.3
    out += ["## 8.3.3 Spearman zwischen Basissiegquote und Reaktion (28 Kompositionen mit Schildträger)", "",
            "| Agent | Schaden: Median | Lebenspunkte: Median | Fähigkeit: Median | negativ (von 18) |", "|---|---|---|---|---|"]
    for a in AGENTEN:
        zellen = [zelle(median([w[a]["rho"][(art, p)] for p in STUFEN]) if True else 0,
                        median([alt[a]["rho"][(art, p)] for p in STUFEN]), 3, "rho") for art in ARTEN for w in (neu,)]
        neg_n = sum(v < 0 for v in neu[a]["rho"].values())
        neg_a = sum(v < 0 for v in alt[a]["rho"].values())
        out.append(f"| {a} | " + " | ".join(zellen) + f" | {neg_n} ({neg_a}) |")
    gn = sum(v < 0 for a in AGENTEN for v in neu[a]["rho"].values())
    ga = sum(v < 0 for a in AGENTEN for v in alt[a]["rho"].values())
    out += ["", f"Negativ in {gn} ({ga}) von 72 Varianten.", ""]

    # ---- 8.3.6
    out += ["## 8.3.6 Bradley-Terry und Grundwertsumme", "",
            "| Agent | mittlere Abweichung BT-Modell | Spearman GWS / Helden | Spearman GWS / Kompositionen | größte Abweichungen (Rang GWS → Rang Siegquote) |",
            "|---|---|---|---|---|"]
    rang_g = {h: i + 1 for i, h in enumerate(sorted(HELDEN, key=lambda h: -gws[h]))}
    for a in AGENTEN:
        def gws_werte(w):
            ks = sorted(w[a]["kq"])
            kg = {k: sum(gws[h] for h in k.split("+")) for k in ks}
            rh = spearman([gws[h] for h in HELDEN], [w[a]["hq"][h] for h in HELDEN])
            rk = spearman([kg[k] for k in ks], [w[a]["kq"][k] for k in ks])
            rs = {h: i + 1 for i, h in enumerate(sorted(HELDEN, key=lambda h: -w[a]["hq"][h]))}
            top = sorted(HELDEN, key=lambda h: (-abs(rang_g[h] - rs[h]), h))[:3]
            return rh, rk, ", ".join(f"{h} {rang_g[h]}→{rs[h]} ({rang_g[h] - rs[h]:+d})" for h in top)
        (rhn, rkn, tn), (rha, rka, ta) = gws_werte(neu), gws_werte(alt)
        out.append(f"| {a} | {zelle(neu[a]['mae'], alt[a]['mae'], 4, 'rho')} | {zelle(rhn, rha, 3, 'rho')} | {zelle(rkn, rka, 3, 'rho')} | "
                   f"{tn} ({ta}) |")
    out += ["", "Mittlere Abweichung: |vorhergesagt − gemessen| über alle 3.486 Paarungen ohne Spiegel (Anteile, nicht %).", ""]

    out += AUSSAGEN
    with open(os.path.join(KENNZAHLEN, "werte_neu.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(out) + "\n")
    print("\n".join(out))

    # ---- heldensiegquoten.csv
    with open(os.path.join(KENNZAHLEN, "heldensiegquoten.csv"), "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter=";")
        w.writerow(["Held"] + list(AGENTEN) + ["Mittelwert"])
        for h in sorted(HELDEN, key=lambda h: -mittel[h]):
            w.writerow([h] + [fmt(neu[a]["hq"][h], 2) for a in AGENTEN] + [fmt(mittel[h], 2)])


# Bewertung (nach Sichtung der Werte vom 26.10.2026 geschrieben; Vergleich f0286a5 gegen e6953bd)
AUSSAGEN = [
    "## Was sich gegenüber dem Stand e6953bd ändert", "",
    "Die Id-Korrektur ändert die Reihenfolge, in der Gleichstände zwischen Einheiten desselben Teams auf einem Feld "
    "aufgelöst werden. Weil ein früh anders aufgelöster Gleichstand den ganzen Verlauf verändert, gehen viele Partien "
    "anders aus - bei den Zufallsagenten verschiebt sich zudem die Reihenfolge ihrer Auswahllisten, die Zufallsziehungen "
    "treffen dadurch andere Züge. Die Läufe sind damit statistisch wie ein neuer Seed zu lesen.", "",
    "**Zufallsagenten: inhaltlich nichts.** 75 % (byactiontype) bzw. 79 % (uniform) der Partien gehen im Detail anders "
    "aus, 19 % mit anderem Sieger. Die Heldensiegquoten ändern sich aber nur im Rahmen der Seed-Streuung: über alle fünf "
    "Seeds RMS 0,12 bzw. 0,13 pp (zwei unabhängige Seeds unterscheiden sich um etwa 0,16-0,20 pp), größte Einzeländerung "
    "0,36 pp; die Rangfolgen sind unverändert, Spearman byactiontype/uniform 0,917 (Helden) bzw. 0,966 (Kompositionen). "
    "Δ Schildträger der 36 Varianten ändert sich um höchstens 0,21 pp (RMS 0,07), alle Vorzeichen bleiben. Erkennung und "
    "Simulationsumfang der Zufallsagenten bleiben bis auf byactiontype Fähigkeit +10/+5 % (jetzt 50.000 bzw. 100.000 statt "
    "100.000 bzw. nie) gleich.", "",
    "**Heuristische Agenten: Restasymmetrie beseitigt, Aussagen bleiben, Einzelwerte verschieben sich.**", "",
    "- **Spiegelpartien (7.3):** Team A gewinnt jetzt exakt 50,00 % (vorher 52,96 % bzw. 52,10 %); Basislauf und vertauschter "
    "Basislauf stimmen weiter in allen 257.040 Partien gespiegelt überein, Etiketten-Prüfung 0 Abweichungen.",
    "- **Rangfolgen (8.3.1):** Die Spitzen bleiben (aggressive Bogenschütze vor Wand und Schildträger, defensive Elfe vor "
    "Bogenschütze und Wand). Beim aggressiven Agenten steigt der Schildträger auf 58,77 % (56,56) und die Wand auf 70,73 % "
    "(68,70), der Berserker fällt auf 47,26 % (49,85) - wie für einen Held erwartet, der immer in Team A stand. Beim "
    "defensiven Agenten tauschen Schildträger und Elementarmagier die Plätze 5 und 6.",
    "- **Agentenabhängigkeit (8.3.1): unverändert stark.** Zufallsagenten gegen aggressive Spearman 0,03 / 0,17 (gleich), "
    "gegen defensive 0,50 / 0,67 (vorher 0,60 / 0,77), aggressive gegen defensive 0,45 (0,40).",
    "- **Grundwertsumme (8.3.6):** aggressive folgt weiter weitgehend den Grundwerten (Helden 0,78, Kompositionen 0,84), "
    "defensive wenig (0,35 / 0,30); Bradley-Terry-Abweichung praktisch gleich.",
    "- **Erkennung (8.3.2): 62 statt 64 von 72.** Defensive Schaden +2,5 % wird knapp nicht mehr erkannt (18 von 27 "
    "veränderten Kompositionen in Richtung, Schwelle 19) und defensive Fähigkeit +5 % ebenfalls nicht mehr; erkannt bleibt "
    "die Fähigkeit bei defensive ±10 % (+10 % genau an der Schwelle: 19 von 28) und −5 % (19 von 28). Für aggressive bleibt die "
    "Fähigkeit auf keiner Stufe erkennbar. Die beiden Wechsel liegen an der Schwelle des Vorzeichentests und zeigen, wie "
    "empfindlich knappe Fälle auf die Gleichstandsauflösung reagieren.",
    "- **Simulationsumfang (8.3.2):** aggressive Schaden +5 % braucht jetzt 50.000 statt 10.000 Partien, Lebenspunkte +10 % "
    "10.000 statt 1.000; defensive Fähigkeit +10 % wird auch mit 100.000 Partien nicht mehr sicher erkannt.",
    "- **Schaden und Lebenspunkte (8.3):** Unverändert: Schaden asymmetrisch (Verhältnis > 1 bei allen Agenten, bei aggressive "
    "±2,5 % jetzt 3,9 statt 5,3), Lebenspunkte nahezu linear und symmetrisch (R² ≥ 0,996), beim aggressiven Agenten am "
    "stärksten (0,54 pp je %).",
    "- **Trennung und Reaktion (8.3.2, 8.3.3):** vollständig getrennt 39 statt 42 von 72; Spearman Basis/Reaktion negativ in "
    "58 statt 62 von 72 Varianten - die Aussage „schwache Kompositionen reagieren stärker“ gilt weiter für Schaden und "
    "Lebenspunkte, für die Fähigkeit nicht mehr eindeutig (Median nahe 0 bei uniform, aggressive, defensive).",
    "",
]


if __name__ == "__main__":
    main()
