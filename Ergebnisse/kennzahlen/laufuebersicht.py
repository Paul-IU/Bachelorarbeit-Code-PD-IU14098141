#!/usr/bin/env python3
"""Übersicht über alle Simulationsläufe (Bachelorarbeit). Liest nur, rechnet nichts neu.

Aufruf:  python laufuebersicht.py
Ausgabe: laufuebersicht.csv (Semikolon, nach Startzeit sortiert)

QUELLEN
  1. Fortschrittsdateien (*.fortschritt.txt): letzte Zeile "Stand" = Ende bzw. Abbruchzeitpunkt,
     "vergangen" = Dauer -> Start = Stand - vergangen. Eine Fortschrittsdatei hält nur den LETZTEN
     Versuch eines Laufs fest (ein Neustart überschreibt sie).
  2. Laufprotokolle (*.log, konsole_*.txt): "Gesamtlaufzeit" als Dauer; das Ende kommt dann aus dem
     Änderungszeitpunkt der Paarungsdatei (Zeitquelle "Protokoll + Dateizeitstempel").
  3. Nur Dateizeitstempel: Ende = Änderungszeitpunkt der Paarungsdatei, Start und Dauer unbekannt.
     ACHTUNG: Dateien in "Alte/" wurden verschoben bzw. kopiert; ihr Zeitstempel kann der Kopierzeitpunkt sein.
  4. Ketten- und Wächterprotokolle (Statusdateien im Scratchpad bzw. deren im Gesprächsprotokoll
     festgehaltene Ausgaben): für abgebrochene Versuche und Fehlstarts, deren Fortschrittsdatei später
     überschrieben wurde. Diese Läufe stehen unten in PROTOKOLL_LAEUFE, jeweils mit Beleg.
Zeitzone: Ortszeit Europe/Berlin, im Untersuchungszeitraum Sommerzeit (+02:00).
"""
import csv
import glob
import os
import re
from datetime import datetime, timedelta

from gemeinsam import ERGEBNISSE, KENNZAHLEN

TZ = "+02:00"
AGENTEN = ("byactiontype", "uniform", "aggressive", "defensive")


def iso(t):
    """Ortszeit mit dem jeweils gültigen Versatz (Sommerzeit +02:00, ab 25.10.2026 Winterzeit +01:00)."""
    return t.astimezone().isoformat(timespec="seconds") if t else ""


def lies_fortschritt(pfad):
    text = open(pfad, encoding="utf-8-sig").read()
    m = re.search(r"Partien ([\d.]+) / ([\d.]+).*?vergangen (\d+):(\d\d):(\d\d)", text)
    s = re.search(r"Stand: (\d{4}-\d\d-\d\d \d\d:\d\d:\d\d)", text)
    if not (m and s):
        return None
    stand = datetime.strptime(s.group(1), "%Y-%m-%d %H:%M:%S")
    dauer = timedelta(hours=int(m.group(3)), minutes=int(m.group(4)), seconds=int(m.group(5)))
    return dict(erreicht=int(m.group(1).replace(".", "")), geplant=int(m.group(2).replace(".", "")),
                ende=stand, start=stand - dauer, fertig="ABGESCHLOSSEN" in text)


def lies_maschine(pfad):
    """<präfix>_maschine.txt eines Laufs auf einer Cloud-VM (vm/vm_lauf.sh): Schlüssel: Wert je Zeile."""
    werte = {}
    for zeile in open(pfad, encoding="utf-8"):
        if ":" in zeile:
            k, v = zeile.split(":", 1)
            werte[k.strip()] = v.strip()
    return werte


def ortszeit(s):
    """ISO-Zeit mit beliebiger Zeitzone (VM meist UTC) in die lokale Ortszeit ohne Zeitzone."""
    return datetime.fromisoformat(s).astimezone().replace(tzinfo=None, microsecond=0)


def gesamtlaufzeiten(ordner):
    """Gesamtlaufzeiten aus allen Protokollen eines Ordners: {Dateiname der Paarungs-CSV: Sekunden} und Liste in Reihenfolge."""
    zuordnung, reihe = {}, []
    for log in sorted(glob.glob(os.path.join(ordner, "*.log")) + glob.glob(os.path.join(ordner, "konsole_*.txt"))):
        if log.endswith(".err.log") or os.path.basename(log) == "etiketten_pruefung.log":
            continue  # etiketten_pruefung.log gehört zu eigenen Prüfläufen ohne Ergebnisdateien (siehe PROTOKOLL_LAEUFE)
        text = open(log, encoding="utf-8", errors="replace").read().replace("\r", "\n")
        letzte = None
        for zeile in text.splitlines():
            m = re.search(r"Gesamtlaufzeit: ([\d,]+) s", zeile)
            if m:
                letzte = float(m.group(1).replace(",", "."))
                reihe.append((log, letzte))
            m = re.search(r"Paarungs-Ergebnisse gespeichert:\s+(\S+_paarungen\.csv)", zeile)
            if m and letzte is not None:
                zuordnung[os.path.basename(m.group(1).replace("\\", "/"))] = (letzte, os.path.basename(log))
    return zuordnung, reihe


# Ergebnis der Etiketten-Prüfung je Agent (aus lauf_<agent>.log in etiketten_pruefung_heuristik).
ETIKETTEN_HEURISTIK = {
    "aggressive": "257.040 Partien geprüft, 0 Abweichungen; alle vier Ergebnisdateien bytegleich zu basislauf_2026_09_26_partien",
    "defensive": "257.040 Partien geprüft, 0 Abweichungen; alle vier Ergebnisdateien bytegleich zu basislauf_2026_09_26_partien",
}


def klassifiziere(rel, datei):
    """art, agent, held, attribut, stufe, seed, bemerkung aus Ordner und Dateiname."""
    teile0 = rel.replace("\\", "/").split("/")
    if teile0[0] == "Archiv_vor_Seitenkorrektur":  # vm/archiviere_heuristik.py: alte heuristische Läufe
        art, agent, held, attribut, stufe, seed, bem = klassifiziere("/".join(teile0[1:]), datei)
        return art, agent, held, attribut, stufe, seed, bem + ["archiviert: heuristischer Agent vor der Seitenkorrektur"]
    agent = next((a for a in AGENTEN if f"_{a}" in datei), "")
    bem = []
    if not agent:
        agent = "byactiontype"
        bem.append("Agent nicht im Dateinamen, Standardagent byactiontype angenommen")
    seed = re.search(r"seed(\d{4})", datei)
    seed = seed.group(1) if seed else "2026"
    held = attribut = stufe = ""
    teile = rel.replace("\\", "/").split("/")
    m = re.match(r"schildtraeger_(dmg|hp|ability)_(-?[\d.]+)_", datei)
    if teile[0] in ("dmg", "hp", "ability") and m:
        art, held, attribut, stufe = "Variante", "schildtraeger", m.group(1), m.group(2)
        if stufe == "20":
            bem.append("Stufe +20 % nachträglich gestrichen (nicht Teil des Variantenplans)")
    elif teile[0].startswith("seed_"):
        art = "Seed-Lauf"
    elif teile[0] == "basislauf_2026_09_26_partien":
        art = "Basislauf"
        bem.append("aktueller Basislauf mit Partieexport")
    elif teile[0] == "etiketten_pruefung_heuristik":
        art = "Prüflauf"
        bem.append("Wiederholung des aktuellen Basislaufs mit Etiketten-Prüfung über alle 257.040 Partien "
                   "(gleicher Seed und Katalog); " + ETIKETTEN_HEURISTIK.get(agent, "Ergebnis der Prüfung noch nicht eingetragen"))
    elif teile[0] == "seiten_vertauscht":
        art = "Seitentausch-Lauf"
        bem.append("Basislauf mit vertauschten Seiten (--swap-sides: Team A auf Blüte B), sonst wie der aktuelle Basislauf")
    elif teile[0] == "Test":
        art, held, attribut, stufe = "Testlauf", "elfe", "heilung", "80/50/20"
        bem.append("Testvariante Elfen-Heilung 80/50/20 (Basis 120/70/20)")
    elif teile[0] == "Alte":
        unter = teile[1] if len(teile) > 2 else ""
        if unter in ("etiketten_pruefung", "spiegel_symmetrie"):
            art = "Prüflauf"
        else:
            art = "Basislauf"
        bem.append(f"älterer Katalog-/Codestand ({unter or 'Alte'})" if unter != "basislauf_2026_09_26"
                   else "Basislauf vom 26.09. ohne Partieexport, durch basislauf_2026_09_26_partien ersetzt")
        if "_mit_" in datei:
            bem.append("mit Etiketten-Prüfung")
        if "_ohne_" in datei:
            bem.append("ohne Etiketten-Prüfung")
        if "vor_korrektur" in rel:
            bem.append("Stand vor der Effektdauer-Korrektur")
    else:
        art = "unbekannt"
    return art, agent, held, attribut, stufe, seed, bem


def dateilaeufe():
    zeilen = []
    for pfad in sorted(glob.glob(os.path.join(ERGEBNISSE, "**", "*_paarungen.csv"), recursive=True)):
        rel = os.path.relpath(pfad, ERGEBNISSE)
        if rel.startswith("kennzahlen"):
            continue
        datei = os.path.basename(pfad)
        ordner = os.path.dirname(pfad)
        art, agent, held, attribut, stufe, seed, bem = klassifiziere(rel, datei)
        mtime = datetime.fromtimestamp(os.path.getmtime(pfad)).replace(microsecond=0)
        with open(pfad, encoding="utf-8-sig") as f:
            partien = sum(int(r["Partien"]) for r in csv.DictReader(f, delimiter=";"))
        fort = pfad[: -len(".csv")] + ".fortschritt.txt"
        start = ende = None
        quelle = ""
        maschine = "lokal (Windows, 12 Threads)"
        mpfad = pfad[: -len("_paarungen.csv")] + "_maschine.txt"
        fp = lies_fortschritt(fort) if os.path.exists(fort) else None
        if os.path.exists(mpfad):
            m = lies_maschine(mpfad)
            start, ende, quelle = ortszeit(m["Start"]), ortszeit(m["Ende"]), "Maschinendatei (Cloud-VM)"
            typ = m.get("Maschine", "unbekannt")
            if "404" in typ:  # vm_lauf.sh hat die Antwort des Hetzner-Metadatendienstes als AWS gedeutet; Zuordnung über die vCPUs
                typ = {"16": "Hetzner CCX43", "8": "Hetzner CCX33"}.get(m.get("vCPUs", ""), "Hetzner (Typ unbekannt)")
            maschine = f"{typ}, {m.get('vCPUs', '?')} vCPUs ({m.get('CPU', '?')}), {m.get('Threads', '?')} Threads"
            bem.append(f"Cloud-VM, Git {m.get('Git', '?')}, {m.get('Betriebssystem', '')}")
        elif fp and fp["fertig"]:
            start, ende, quelle = fp["start"], fp["ende"], "Fortschrittsdatei"
        else:
            zuo, reihe = gesamtlaufzeiten(ordner)
            dauer = zuo.get(datei)
            if dauer is None and reihe:
                # Protokoll ohne Dateinamen: Zuordnung über die Reihenfolge (Paarungsdateien nach Änderungszeit)
                csvs = sorted(glob.glob(os.path.join(ordner, "*_paarungen.csv")), key=os.path.getmtime)
                if len(csvs) == len(reihe):
                    dauer = (reihe[csvs.index(pfad)][1], os.path.basename(reihe[csvs.index(pfad)][0]))
                    bem.append("Protokollzeile über die Reihenfolge zugeordnet")
            ende = mtime
            if dauer:
                start = mtime - timedelta(seconds=dauer[0])
                quelle = f"Protokoll ({dauer[1]}) + Dateizeitstempel"
            else:
                quelle = "Dateizeitstempel"
                bem.append("Start und Dauer unbekannt (kein Protokoll); Ende = Änderungszeit der Paarungsdatei")
        zeilen.append(dict(art=art, agent=agent, held=held, attribut=attribut, stufe=stufe, seed=seed,
                           start=start, ende=ende, status="abgeschlossen", geplant=partien, erreicht=partien,
                           zeitquelle=quelle, bemerkung="; ".join(bem), datei=rel, maschine=maschine))
    return zeilen


def t(s):
    return datetime.strptime(s, "%Y-%m-%d %H:%M:%S")


# Läufe, die nur aus Ketten-/Wächterprotokollen hervorgehen (Fortschrittsdatei überschrieben oder fehlend).
PROTOKOLL_LAEUFE = [
    dict(art="Basislauf", agent="byactiontype", seed="2026", start=t("2026-09-26 14:53:39"), ende=t("2026-09-26 14:53:57"),
         status="abgebrochen", geplant=257040, erreicht=21715, zeitquelle="Fortschrittsdatei",
         bemerkung="auf Wunsch gestoppt (Alte/basislauf_paarungen.fortschritt.txt); Agent nicht im Dateinamen, byactiontype angenommen"),
    dict(art="Variante", agent="defensive", held="schildtraeger", attribut="dmg", stufe="-2.5", seed="2026",
         start=t("2026-09-30 16:24:17"), ende=t("2026-09-30 16:54:16"), status="abgebrochen", geplant=257040, erreicht=50545,
         zeitquelle="Protokoll", bemerkung="1. Versuch; beendet mit dem Hintergrundjob (Zeitlimit des Werkzeugs)"),
    dict(art="Variante", agent="defensive", held="schildtraeger", attribut="dmg", stufe="-2.5", seed="2026",
         start=t("2026-09-30 16:54:39"), ende=t("2026-09-30 19:12:09"), status="abgebrochen", geplant=257040, erreicht=174651,
         zeitquelle="Protokoll", bemerkung="2. Versuch; Absturz UnauthorizedAccessException in BatchProgressReporter (Fortschrittsdatei geöffnet), danach behoben"),
    dict(art="Variante", agent="aggressive", held="schildtraeger", attribut="hp", stufe="-5", seed="2026",
         start=t("2026-10-01 11:18:29"), ende=t("2026-10-01 12:47:09"), status="abgebrochen", geplant=257040, erreicht=246872,
         zeitquelle="Protokoll", bemerkung="1. Versuch; mit dem Neustart der Claude-Sitzung beendet (Ende = letzter Fortschrittsstand)"),
    dict(art="Variante", agent="defensive", held="schildtraeger", attribut="ability", stufe="-5", seed="2026",
         start=t("2026-10-01 20:11:33"), ende=t("2026-10-01 20:47:04"), status="abgebrochen", geplant=257040, erreicht=61555,
         zeitquelle="Protokoll", bemerkung="1. Versuch; Windows-Abmeldung/Standby 20:47 (Systemprotokoll)"),
    dict(art="Variante", agent="defensive", held="schildtraeger", attribut="hp", stufe="2.5", seed="2026",
         start=t("2026-10-03 00:41:56"), ende=t("2026-10-03 00:48:09"), status="abgebrochen", geplant=257040, erreicht=9810,
         zeitquelle="Protokoll", bemerkung="1. Versuch; vermutlich Abmeldung/Energiesparmodus (Ende = letzter Fortschrittsstand)"),
    dict(art="Variante", agent="aggressive", held="schildtraeger", attribut="hp", stufe="-2.5", seed="2026",
         start=t("2026-10-03 21:00:20"), ende=t("2026-10-03 21:00:20"), status="abgebrochen", geplant=257040, erreicht=0,
         zeitquelle="Protokoll", bemerkung="Fehlstart: Kettenskript übergab 'schildtraeger_h_p' (PowerShell-Listenfehler); keine Partie gespielt, sofort neu gestartet"),
    dict(art="Variante", agent="aggressive", held="schildtraeger", attribut="dmg", stufe="5", seed="2026",
         start=t("2026-10-24 04:40:48"), ende=t("2026-10-24 04:41:30"), status="abgebrochen", geplant=257040, erreicht=2811,
         zeitquelle="Fortschrittsdatei", bemerkung="1. Versuch nach der Seitenkorrektur; Windows-Abmeldung und Standby 04:41 (Systemprotokoll, Ereignis 187/42), Kette um 12:15 neu gestartet"),
    # Alte/mauer_eigene_identitaet/etiketten_pruefung.log (Änderungszeit 24.09. 23:08:xx): zwei Prüfläufe mit
    # Etiketten-Prüfung ohne gespeicherte Ergebnisdateien; Dauer aus dem Protokoll, Ende des zweiten = Änderungszeit,
    # Ende des ersten = Start des zweiten (nacheinander gestartet angenommen).
    dict(art="Prüflauf", agent="byactiontype", seed="2026", start=None, ende=None, status="abgeschlossen",
         geplant=257040, erreicht=257040, zeitquelle="Protokoll + Dateizeitstempel", platzhalter="mauer_1",
         bemerkung="Etiketten-Prüfung (0 Abweichungen), älterer Katalogstand (mauer_eigene_identitaet); Start/Ende geschätzt: "
                   "Ende = Start des Folgelaufs, nacheinander gestartet angenommen"),
    dict(art="Prüflauf", agent="uniform", seed="2026", start=None, ende=None, status="abgeschlossen",
         geplant=257040, erreicht=257040, zeitquelle="Protokoll + Dateizeitstempel", platzhalter="mauer_2",
         bemerkung="Etiketten-Prüfung (0 Abweichungen), älterer Katalogstand (mauer_eigene_identitaet); Ende = Änderungszeit des Protokolls"),
    dict(art="Prüflauf", agent="aggressive+defensive", seed="2026", start=t("2026-09-24 18:26:33"), ende=t("2026-09-24 19:42:33"),
         status="abgeschlossen", geplant=2 * 2 * 84 * 72, erreicht=2 * 2 * 84 * 72, zeitquelle="Protokoll",
         bemerkung="Test AlleSpiegelpaarungenAlle72Ausgangslagen (Etiketten-Unabhängigkeit), 84 Spiegelpaarungen x 72 Lagen je Agent, "
                   "jede Partie doppelt; Dauer 1 h 16 min laut Testausgabe, Ende = Zeitpunkt der Ausgabe; älterer Katalogstand"),
]


def lookahead_laeufe():
    """Stichprobe des vorausschauenden Agenten (verworfen), Alte/lookahead_stichprobe/rohdaten.txt."""
    pfad = os.path.join(ERGEBNISSE, "Alte", "lookahead_stichprobe", "rohdaten.txt")
    if not os.path.exists(pfad):
        return []
    zeilen, ende = [], None
    for zeile in open(pfad, encoding="utf-8", errors="replace"):
        m = re.match(r"--- (\d\d:\d\d:\d\d)", zeile)
        if m:
            ende = t("2026-09-26 " + m.group(1))
        m = re.match(r"(Laufzeit|Verhalten) (\S+): (\d+) Partien in ([\d,]+) s", zeile)
        if m and ende:
            name, sek = m.group(2), float(m.group(4).replace(",", "."))
            heuristisch = name in ("aggressive", "neutral")
            zeilen.append(dict(art="Testlauf" if heuristisch else "verworfen",
                               agent=name if heuristisch else f"lookahead ({name})", seed="2026",
                               start=ende - timedelta(seconds=sek), ende=ende, status="abgeschlossen",
                               geplant=int(m.group(3)), erreicht=int(m.group(3)), zeitquelle="Protokoll",
                               bemerkung=f"Stichprobe vorausschauender Agent ({m.group(1)}lauf, 20 Paarungen x 12 Lagen); "
                                         "Ende = Zeitstempel der Ausgabe, Datum aus Dateizeitstempel"))
    return zeilen


def setze_mauer_pruefung(laeufe):
    log = os.path.join(ERGEBNISSE, "Alte", "mauer_eigene_identitaet", "etiketten_pruefung.log")
    if not os.path.exists(log):
        return
    ende2 = datetime.fromtimestamp(os.path.getmtime(log)).replace(microsecond=0)
    sek = [float(x.replace(",", ".")) for x in re.findall(r"Gesamtlaufzeit: ([\d,]+) s", open(log, encoding="utf-8", errors="replace").read())]
    if len(sek) != 2:
        return
    start2 = ende2 - timedelta(seconds=sek[1])
    for z in laeufe:
        if z.get("platzhalter") == "mauer_2":
            z["start"], z["ende"] = start2, ende2
        if z.get("platzhalter") == "mauer_1":
            z["start"], z["ende"] = start2 - timedelta(seconds=sek[0]), start2


def main():
    setze_mauer_pruefung(PROTOKOLL_LAEUFE)
    alle = dateilaeufe() + [dict(held="", attribut="", stufe="", **z) if "held" not in z else z for z in PROTOKOLL_LAEUFE] + lookahead_laeufe()
    for z in alle:
        for k in ("held", "attribut", "stufe", "datei"):
            z.setdefault(k, "")
        z.setdefault("maschine", "lokal (Windows, 12 Threads)")
    alle.sort(key=lambda z: (z["start"] or z["ende"] or datetime.max))
    for i, z in enumerate(alle, 1):
        z["lauf_id"] = f"L{i:03d}"
    for z in alle:
        if z["start"] and z["ende"]:
            z["parallel_zu"] = ",".join(o["lauf_id"] for o in alle if o is not z and o["start"] and o["ende"]
                                        and o["maschine"] == z["maschine"]  # nur auf derselben Maschine parallel
                                        and o["start"] < z["ende"] and z["start"] < o["ende"]
                                        and (min(z["ende"], o["ende"]) - max(z["start"], o["start"])).total_seconds() >= 60)
        else:
            z["parallel_zu"] = ""
    felder = ["lauf_id", "art", "agent", "held", "attribut", "stufe", "seed", "start", "ende", "dauer_minuten", "status",
              "partien_geplant", "partien_erreicht", "parallel_zu", "maschine", "zeitquelle", "bemerkung", "datei"]
    with open(os.path.join(KENNZAHLEN, "laufuebersicht.csv"), "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f, delimiter=";")
        w.writerow(felder)
        for z in alle:
            dauer = (z["ende"] - z["start"]).total_seconds() / 60 if z["start"] and z["ende"] else None
            w.writerow([z["lauf_id"], z["art"], z["agent"], z["held"], z["attribut"], z["stufe"], z["seed"],
                        iso(z["start"]), iso(z["ende"]), f"{dauer:.1f}".replace(".", ",") if dauer is not None else "",
                        z["status"], z["geplant"], z["erreicht"], z["parallel_zu"], z["maschine"], z["zeitquelle"], z["bemerkung"], z["datei"]])
    print(len(alle), "Läufe")


if __name__ == "__main__":
    main()
