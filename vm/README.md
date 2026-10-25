# Läufe auf Cloud-Servern (Ubuntu, x86-64)

Nur der Batch-Lauf (`TacticsCore.Demo full-baseline`) läuft auf dem Server, Godot wird nicht gebraucht.

## Aufteilung der Neuerhebung (nach der Korrektur der Id-Reihenfolge)

- **lokal:** die 48 Läufe der Zufallsagenten (eigene Kette, Ergebnisse direkt in `Ergebnisse/`)
- **drei Hetzner-CCX33** (je 8 vCPU, `--threads 8`): die 40 Läufe von aggressive und defensive, so verteilt, dass alle
  drei etwa gleichzeitig fertig werden (Schätzung aus den Laufzeiten auf dem CCX33: defensive ca. 2,0 h, aggressive ca. 0,7 h
  je Lauf, mit Etiketten-Prüfung doppelt):
  - `laufliste_server1.txt`: ca. 19,4 h (Basisläufe mit Etiketten-Prüfung, defensive vertauscht, defensive Schaden)
  - `laufliste_server2.txt`: ca. 19,0 h (defensive Lebenspunkte, aggressive vertauscht, Schaden, Lebenspunkte +10/−10/+5)
  - `laufliste_server3.txt`: ca. 18,3 h (defensive Fähigkeit, aggressive Lebenspunkte −5/±2,5, Fähigkeit)
- Die Lauflisten des vorigen Stands (e6953bd) liegen in `lauflisten_e6953bd/`.

Format der Laufliste, je Zeile ein Lauf, `#` leitet Kommentare ein:
`<agent> <basis|schildtraeger_<dmg|hp|ability>_<prozent>> [seed] [--swap-sides] [--verify-name-independence]`

## Was `vm_lauf.sh` macht

1. Es installiert .NET 8, git und python3, klont das Repository (der Token wird danach aus `.git/config` entfernt) und baut im Release-Modus.
2. Es führt die Testsuite aus und bricht ab, wenn sie nicht grün ist. `--ohne-tests` überspringt diesen Schritt.
3. Es rechnet die Stichprobe (1.008 Partien aggressive, `--sample`) und vergleicht sie Partie für Partie mit
   `vm/referenz/stichprobe_aggressive_partien.csv.gz`. **Nur wenn alle Partien identisch sind, startet es die Läufe.**
4. Es arbeitet die Laufliste ab. Je Lauf entstehen die Ergebnisdateien wie lokal, dazu `<präfix>_lauf.log` und `<präfix>_maschine.txt`
   (Maschinentyp, vCPUs, Threads, Start, Ende, Dauer, Git-Stand) sowie ein Paket `~/tacticscore/pakete/<präfix>.tar.gz`.

Die Referenz muss zum Code-Stand passen. Nach jeder Änderung an Regeln oder Agenten wird sie neu erzeugt:
```
dotnet TacticsCore.Demo/bin/Release/net8.0/TacticsCore.Demo.dll full-baseline ref_paarungen.csv ref_kompositionen.csv ref_helden.csv 2026 aggressive --sample
copy ref_partien.csv.gz vm\referenz\stichprobe_aggressive_partien.csv.gz
```

## Hetzner (Ubuntu 24.04, root, SSH-Schlüssel), Befehle in Windows PowerShell

Skript und Laufliste hochladen und verbinden:
```
cd C:\Users\20020\Desktop\BA\Code
scp vm\vm_lauf.sh vm\laufliste_server1.txt root@<IP_SERVER1>:
ssh root@<IP_SERVER1>
```

Auf dem Server:
```
sed -i 's/\r$//' vm_lauf.sh laufliste_*.txt
tmux new -s laeufe
read -rs GH_TOKEN
bash vm_lauf.sh laufliste_server1.txt --branch seiten-vertauschen --threads 8 --repo "https://x-access-token:${GH_TOKEN}@github.com/Paul-IU/Code.git"; unset GH_TOKEN
```
- `sed` entfernt mögliche Windows-Zeilenenden.
- Nach `read -rs GH_TOKEN` den Token einfügen und Enter drücken. Er wird nicht angezeigt und landet nicht im Verlauf; dort steht nur `${GH_TOKEN}`.
- Mit `Strg+B`, dann `D` trennst du dich von tmux. Der Lauf geht weiter, auch wenn die SSH-Verbindung abbricht.
- Für Server 2 und 3 gilt dasselbe mit `laufliste_server2.txt` bzw. `laufliste_server3.txt`.

Fortschritt von Windows aus:
```
ssh root@<IP> "tail -n 5 /root/tacticscore/status.txt"
ssh root@<IP> 'cat $(find /root/tacticscore/Code/Ergebnisse -name "*fortschritt.txt" -mmin -2 | head -1)'
ssh -t root@<IP> tmux attach -t laeufe
```
Die erste Zeile zeigt den Gesamtstand, die zweite den laufenden Lauf, die dritte die Sitzung (wieder trennen mit `Strg+B`, `D`).

Ergebnisse herunterladen und im Repository entpacken:
```
mkdir C:\Users\20020\Desktop\BA\pakete -Force
scp "root@<IP>:/root/tacticscore/pakete/*.tar.gz" C:\Users\20020\Desktop\BA\pakete\
cd C:\Users\20020\Desktop\BA\Code
Get-ChildItem C:\Users\20020\Desktop\BA\pakete\*.tar.gz | ForEach-Object { tar -xzf $_.FullName }
python Ergebnisse\kennzahlen\laufuebersicht.py
```
Voraussetzung: Die alten Ergebnisse sind archiviert (`python vm\archiviere_alte_ergebnisse.py --ziel Archiv_vor_Idkorrektur --ausfuehren`; das passiert vor
dem Start der lokalen Kette). Danach die Server in der Hetzner-Konsole **löschen**, denn sie kosten auch im ausgeschalteten Zustand.

## Threads

`--threads <n>` legt fest, wie viele Partien parallel laufen. Lokal ist der Standard 12, `vm_lauf.sh` nimmt ohne Angabe die
Zahl der vCPUs. Die Ergebnisse hängen nicht davon ab, denn jede Partie hat ihren eigenen Seed und festen Ergebnisplatz.
