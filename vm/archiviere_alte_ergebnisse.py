#!/usr/bin/env python3
"""Archiviert alle Ergebnisse vor der Seitenkorrektur (Bachelorarbeit), für alle vier Agenten.

Aufruf:  python vm/archiviere_alte_ergebnisse.py              (nur anzeigen, was verschoben würde)
         python vm/archiviere_alte_ergebnisse.py --ausfuehren (verschieben)

Mit der Seitenkorrektur (Gleichstandsregel der heuristischen Agenten, Sichtlinie, Reihenfolge der
Flächenwirkungen) werden alle vier Agenten neu erhoben. Das Skript verschiebt alle Dateien aus den Ablageorten,
in die auch die Neuerhebung schreibt, nach Ergebnisse/Archiv_vor_Seitenkorrektur/<gleicher relativer Pfad>:
Basisläufe, Seed-Läufe 2027-2030, alle Varianten (auch die Zusatzvariante dmg +20 %), Läufe mit vertauschten
Seiten und die Etiketten-Prüfung der heuristischen Agenten. Nichts wird gelöscht oder überschrieben: liegt am
Ziel schon eine Datei, bricht das Skript ab. Vor dem Entpacken der neuen Läufe ausführen.
"""
import glob
import os
import shutil
import sys

CODE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ERGEBNISSE = os.path.join(CODE, "Ergebnisse")
ARCHIV = os.path.join(ERGEBNISSE, "Archiv_vor_Seitenkorrektur")
MUSTER = [os.path.join("basislauf_2026_09_26_partien", "*"), os.path.join("seed_*", "*")]
MUSTER += [os.path.join(art, "*", "schildtraeger", "*") for art in ("dmg", "hp", "ability")]
MUSTER += [os.path.join("seiten_vertauscht", "*"), os.path.join("etiketten_pruefung_heuristik", "*")]


def main():
    ausfuehren = "--ausfuehren" in sys.argv
    dateien = sorted({p for m in MUSTER for p in glob.glob(os.path.join(ERGEBNISSE, m)) if os.path.isfile(p)})
    ziele = [(p, os.path.join(ARCHIV, os.path.relpath(p, ERGEBNISSE))) for p in dateien]
    belegt = [z for _, z in ziele if os.path.exists(z)]
    if belegt:
        print(f"ABBRUCH: {len(belegt)} Zieldatei(en) existieren schon, z. B. {belegt[0]}")
        return 1
    for quelle, ziel in ziele:
        print(f"{'verschiebe' if ausfuehren else 'würde verschieben'}: {os.path.relpath(quelle, CODE)}")
        if ausfuehren:
            os.makedirs(os.path.dirname(ziel), exist_ok=True)
            shutil.move(quelle, ziel)
    print(f"{len(ziele)} Dateien {'verschoben' if ausfuehren else 'betroffen (Probelauf, nichts verschoben)'}.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
