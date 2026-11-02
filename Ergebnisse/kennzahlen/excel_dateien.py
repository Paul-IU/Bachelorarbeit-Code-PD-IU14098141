#!/usr/bin/env python3
"""Excel-Fassungen zweier Auswertungen (Bachelorarbeit).

Aufruf:  python excel_dateien.py   (nach platzhalter_kapitel8.py, das abb_8-3-2_daten.csv schreibt)
Ausgabe: heldensiegquoten.xlsx, abb_8-3-2_daten.xlsx
Benötigt: openpyxl

heldensiegquoten.xlsx: bereinigte Siegquote der neun Helden je Agent im Basislauf Seed 2026 (Unentschieden als
halber Sieg, wie heldensiegquoten.csv aus werte_neu.py, hier ungerundet), sortiert nach dem Mittelwert über die vier
Agenten; die Spalte Mittelwert ist eine Excel-Formel.

abb_8-3-2_daten.xlsx: die Werte von abb_8-3-2_daten.csv als Zahlen, dazu die Spalten delta_mit / delta_ohne
(delta_pp, getrennt nach Komposition mit / ohne Schildträger, als Formel) und ein Blatt mit kleinstem und größtem
delta_pp je Feld (ebenfalls Formeln).

Die Formeln berechnet Excel beim Öffnen (die Datei ist entsprechend markiert).
"""
import csv
import os

from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, Side
from openpyxl.workbook.properties import CalcProperties

from gemeinsam import AGENTEN, KENNZAHLEN, finde_laeufe, helden_quote, lade_csv

SCHRIFT = Font(name="Arial", size=10)
FETT = Font(name="Arial", size=10, bold=True)
EINGABE = Font(name="Arial", size=10, color="0000FF")   # eingetragene Werte blau, Formeln schwarz
HINWEIS = Font(name="Arial", size=9, italic=True)
RAND = Border(bottom=Side(style="thin"))


def kopfzeile(blatt, spalten):
    for j, k in enumerate(spalten, 1):
        c = blatt.cell(1, j, k)
        c.font, c.border, c.alignment = FETT, RAND, Alignment(horizontal="center")


def heldensiegquoten():
    laeufe = finde_laeufe()
    q = {a: helden_quote(lade_csv(next(l for l in laeufe if l[0] == a and l[1] == "basis" and l[3] == 2026)[4] + "_paarungen.csv"))
         for a in AGENTEN}
    helden = sorted(q[AGENTEN[0]], key=lambda h: -sum(q[a][h] for a in AGENTEN) / len(AGENTEN))
    wb = Workbook()
    ws = wb.active
    ws.title = "Heldensiegquoten"
    kopfzeile(ws, ["Held"] + list(AGENTEN) + ["Mittelwert"])
    for i, h in enumerate(helden, 2):
        ws.cell(i, 1, h).font = SCHRIFT
        for j, a in enumerate(AGENTEN, 2):
            c = ws.cell(i, j, q[a][h] / 100)
            c.font, c.number_format = EINGABE, "0.00%"
        c = ws.cell(i, len(AGENTEN) + 2, f"=AVERAGE(B{i}:E{i})")
        c.font, c.number_format = SCHRIFT, "0.00%"
    ws.cell(len(helden) + 3, 1, "Quelle: Basisläufe Seed 2026 (Ergebnisse/basislauf_2026_09_26_partien). Bereinigte Siegquote: nur "
            "Partien mit dem Helden auf genau einer Seite, Unentschieden als halber Sieg. Nach dem Mittelwert absteigend.").font = HINWEIS
    ws.column_dimensions["A"].width = 18
    for col in "BCDEF":
        ws.column_dimensions[col].width = 14
    ws.freeze_panes = "B2"
    wb.calculation = CalcProperties(fullCalcOnLoad=True)
    wb.save(os.path.join(KENNZAHLEN, "heldensiegquoten.xlsx"))


def abbildung_b():
    zeilen = list(csv.DictReader(open(os.path.join(KENNZAHLEN, "abb_8-3-2_daten.csv"), encoding="utf-8"), delimiter=";"))
    wb = Workbook()
    ws = wb.active
    ws.title = "Daten"
    kopfzeile(ws, ["feld", "agent", "variante", "rang", "komposition", "mit_schildtraeger", "siegquote_basis", "delta_pp",
                   "delta_mit", "delta_ohne"])
    for i, r in enumerate(zeilen, 2):
        werte = [r["feld"], r["agent"], r["variante"], int(r["rang"]), r["komposition"], int(r["mit_schildtraeger"]),
                 float(r["siegquote_basis"]), float(r["delta_pp"])]
        for j, v in enumerate(werte, 1):
            c = ws.cell(i, j, v)
            c.font = EINGABE if j >= 4 else SCHRIFT
            if j in (7, 8):
                c.number_format = "0.000000"
        for j, bedingung in ((9, 1), (10, 0)):
            c = ws.cell(i, j, f'=IF(F{i}={bedingung},H{i},"")')
            c.font, c.number_format = SCHRIFT, "0.000000"
    for col, breite in zip("ABCDEFGHIJ", (26, 12, 12, 6, 36, 17, 15, 12, 12, 12)):
        ws.column_dimensions[col].width = breite
    ws.freeze_panes = "A2"

    s = wb.create_sheet("Spannweite")
    kopfzeile(s, ["feld", "Zeilen in 'Daten'", "kleinster delta_pp", "größter delta_pp"])
    felder = []   # je Feld die zusammenhängenden Zeilen im Blatt "Daten"
    for i, r in enumerate(zeilen, 2):
        if not felder or felder[-1][0] != r["feld"]:
            felder.append([r["feld"], i, i])
        felder[-1][2] = i
    for z, (feld, von, bis) in enumerate(felder, 2):
        s.cell(z, 1, feld).font = SCHRIFT
        s.cell(z, 2, f"{von}–{bis}").font = SCHRIFT
        for j, funktion in ((3, "MIN"), (4, "MAX")):
            c = s.cell(z, j, f"={funktion}(Daten!H{von}:H{bis})")
            c.font, c.number_format = SCHRIFT, "0.00"
    s.cell(len(felder) + 3, 1, "Quelle: abb_8-3-2_daten.csv (platzhalter_kapitel8.py); delta_pp = Variante − Basis in Prozentpunkten.").font = HINWEIS
    for col, breite in zip("ABCD", (28, 16, 18, 18)):
        s.column_dimensions[col].width = breite
    wb.calculation = CalcProperties(fullCalcOnLoad=True)
    wb.save(os.path.join(KENNZAHLEN, "abb_8-3-2_daten.xlsx"))


def main():
    heldensiegquoten()
    abbildung_b()
    print("heldensiegquoten.xlsx und abb_8-3-2_daten.xlsx geschrieben")


if __name__ == "__main__":
    main()
