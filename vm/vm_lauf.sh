#!/usr/bin/env bash
# =======================================================================
# TacticsCore auf einer frischen Ubuntu-VM (22.04 / 24.04, x86-64): .NET 8
# installieren, Repository klonen, bauen, Tests ausführen, die Stichprobe
# Partie für Partie mit der lokalen Referenz vergleichen und erst dann die
# Läufe aus einer Laufliste nacheinander abarbeiten. Jeder Lauf wird am Ende
# als .tar.gz gepackt (Pfade relativ zum Repository - im lokalen Repository
# entpackt landen die Dateien an derselben Stelle).
#
# Aufruf:
#   bash vm_lauf.sh <laufliste.txt> [--repo URL] [--branch NAME] [--threads N]
#                   [--ohne-tests] [--arbeitsordner DIR]
#
#   --repo           Standard https://github.com/Paul-IU/Code.git
#                    (privates Repository: https://<token>@github.com/Paul-IU/Code.git)
#   --branch         Standard main
#   --threads        parallele Partien je Lauf, Standard: Anzahl der vCPUs (nproc)
#   --ohne-tests     Testsuite überspringen (nur, wenn sie für diesen Stand schon lief)
#   --arbeitsordner  Standard ~/tacticscore; Pakete in <arbeitsordner>/pakete
#
# Laufliste: eine Zeile je Lauf, "#" leitet Kommentare ein:
#   <agent> <basis|schildtraeger_<dmg|hp|ability>_<prozent>> [seed] [--swap-sides] [--verify-name-independence]
# Beispiele:
#   defensive basis
#   aggressive schildtraeger_dmg_-2.5
#   aggressive basis 2026 --swap-sides
#
# Ablage im Repository (wie lokal):
#   basis              Ergebnisse/basislauf_2026_09_26_partien/basislauf_<agent>_*
#   basis, Seed != 2026 Ergebnisse/seed_<seed>/basislauf_<agent>_seed<seed>_*
#   basis --swap-sides Ergebnisse/seiten_vertauscht/basislauf_<agent>_vertauscht_*
#   Variante           Ergebnisse/<art>/<art>_<p>/schildtraeger/schildtraeger_<art>_<p>_<agent>_*
# Zu jedem Lauf zusätzlich <präfix>_lauf.log und <präfix>_maschine.txt
# (Maschinentyp, vCPUs, Threads, Start, Ende, Dauer, Git-Stand) - laufuebersicht.py
# übernimmt daraus Laufzeit und Maschinentyp.
# =======================================================================
set -euo pipefail

LISTE=""
REPO="https://github.com/Paul-IU/Code.git"
BRANCH="main"
THREADS="$(nproc)"
TESTS=1
ARBEIT="$HOME/tacticscore"
while [ $# -gt 0 ]; do
    case "$1" in
        --repo) REPO="$2"; shift 2 ;;
        --branch) BRANCH="$2"; shift 2 ;;
        --threads) THREADS="$2"; shift 2 ;;
        --ohne-tests) TESTS=0; shift ;;
        --arbeitsordner) ARBEIT="$2"; shift 2 ;;
        -*) echo "Unbekannte Option: $1"; exit 2 ;;
        *) LISTE="$(readlink -f "$1")"; shift ;;
    esac
done
[ -n "$LISTE" ] && [ -f "$LISTE" ] || { echo "Laufliste fehlt (Aufruf: bash vm_lauf.sh <laufliste.txt> ...)"; exit 2; }

REPO_DIR="$ARBEIT/Code"
PAKETE="$ARBEIT/pakete"
STATUS="$ARBEIT/status.txt"
mkdir -p "$ARBEIT" "$PAKETE"
log() { echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "$STATUS"; }
SUDO=""; [ "$(id -u)" -ne 0 ] && SUDO="sudo"   # als root (z. B. Hetzner) ohne sudo

# ---------------------------------------------------------------- 1. .NET 8, git, python3
log "Installation: .NET 8 SDK, git, python3"
export DEBIAN_FRONTEND=noninteractive
$SUDO apt-get update -qq
$SUDO apt-get install -y -qq git python3 curl ca-certificates libicu-dev >/dev/null
if ! command -v dotnet >/dev/null 2>&1 || ! dotnet --list-sdks | grep -q '^8\.'; then
    if ! $SUDO apt-get install -y -qq dotnet-sdk-8.0 >/dev/null 2>&1; then
        log "dotnet-sdk-8.0 nicht im Paketarchiv - Installation über dotnet-install.sh"
        curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
        bash /tmp/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
        export PATH="$HOME/.dotnet:$PATH"
    fi
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
log ".NET: $(dotnet --version)"

# ---------------------------------------------------------------- 2. Repository
if [ -d "$REPO_DIR/.git" ]; then
    log "Repository aktualisieren ($BRANCH)"
    git -C "$REPO_DIR" fetch -q "$REPO" "+refs/heads/$BRANCH:refs/remotes/origin/$BRANCH"
    git -C "$REPO_DIR" checkout -q "$BRANCH"
    git -C "$REPO_DIR" reset -q --hard "origin/$BRANCH"
else
    log "Repository klonen ($BRANCH)"
    git clone -q --branch "$BRANCH" "$REPO" "$REPO_DIR"
fi
# Zugangsdaten (Token) nicht in .git/config stehen lassen: Remote-URL ohne Benutzerteil speichern
git -C "$REPO_DIR" remote set-url origin "$(echo "$REPO" | sed -E 's#https://[^@/]*@#https://#')"
cd "$REPO_DIR"
COMMIT="$(git rev-parse --short HEAD)"
log "Stand: $COMMIT"

# ---------------------------------------------------------------- 3. Bauen, 4. Tests
log "Bauen (Release)"
dotnet build TacticsCore.Demo -c Release --nologo -v q
dotnet build TacticsCore.Tests -c Release --nologo -v q
DLL="TacticsCore.Demo/bin/Release/net8.0/TacticsCore.Demo.dll"
if [ "$TESTS" = 1 ]; then
    log "Testsuite (dauert lange; Protokoll: $ARBEIT/tests.log)"
    if ! dotnet test TacticsCore.Tests -c Release --no-build --nologo > "$ARBEIT/tests.log" 2>&1; then
        log "ABBRUCH: Tests nicht grün"; tail -20 "$ARBEIT/tests.log"; exit 1
    fi
    log "Tests: $(grep -E 'Passed!|Failed!' "$ARBEIT/tests.log" | tail -1)"
fi

# ---------------------------------------------------------------- 5. Stichprobe gegen die lokale Referenz
REFERENZ="vm/referenz/stichprobe_aggressive_partien.csv.gz"
[ -f "$REFERENZ" ] || { log "ABBRUCH: Referenz $REFERENZ fehlt im Repository"; exit 1; }
SP="$ARBEIT/stichprobe"
mkdir -p "$SP"
log "Stichprobe: 1.008 Partien aggressive (--sample)"
dotnet "$DLL" full-baseline "$SP/stichprobe_aggressive_paarungen.csv" "$SP/stichprobe_aggressive_kompositionen.csv" \
    "$SP/stichprobe_aggressive_helden.csv" 2026 aggressive --sample --threads "$THREADS" > "$SP/stichprobe.log" 2>&1
if ! python3 vm/vergleiche_partien.py "$REFERENZ" "$SP/stichprobe_aggressive_partien.csv.gz" | tee -a "$STATUS"; then
    log "ABBRUCH: Stichprobe weicht von der lokalen Referenz ab - keine Läufe gestartet"; exit 1
fi

# ---------------------------------------------------------------- Maschinentyp
maschine() {
    local typ=""
    local tok
    tok="$(curl -s -m 2 -X PUT http://169.254.169.254/latest/api/token -H 'X-aws-ec2-metadata-token-ttl-seconds: 60' || true)"
    [ -n "$tok" ] && typ="$(curl -s -m 2 -H "X-aws-ec2-metadata-token: $tok" http://169.254.169.254/latest/meta-data/instance-type || true)" && [ -n "$typ" ] && typ="AWS $typ"
    [ -z "$typ" ] && typ="$(curl -s -m 2 -H 'Metadata-Flavor: Google' http://metadata.google.internal/computeMetadata/v1/instance/machine-type 2>/dev/null | awk -F/ '{print $NF}' || true)" && [ -n "$typ" ] && typ="GCP $typ"
    [ -z "$typ" ] && typ="$(curl -s -m 2 -H 'Metadata: true' 'http://169.254.169.254/metadata/instance/compute/vmSize?api-version=2021-02-01&format=text' || true)" && [ -n "$typ" ] && typ="Azure $typ"
    [ -z "$typ" ] && typ="unbekannt"
    echo "$typ"
}
MASCHINE="$(maschine)"
CPU="$(lscpu | sed -n 's/^Model name:\s*//p' | head -1)"
RAM="$(free -g | awk '/^Mem:/{print $2}') GB"
log "Maschine: $MASCHINE, $(nproc) vCPUs ($CPU), $RAM, Threads je Lauf: $THREADS"

# ---------------------------------------------------------------- 6. Läufe
while read -r zeile || [ -n "$zeile" ]; do
    zeile="${zeile%%#*}"
    read -r -a teile <<< "$zeile"
    [ ${#teile[@]} -eq 0 ] && continue
    agent="${teile[0]}"; variante="${teile[1]:-basis}"; seed=2026; extra=()
    for t in "${teile[@]:2}"; do
        if [[ "$t" =~ ^[0-9]+$ ]]; then seed="$t"; else extra+=("$t"); fi
    done
    swap=0; for t in "${extra[@]}"; do [ "$t" = "--swap-sides" ] && swap=1; done
    if [ "$variante" = "basis" ]; then
        if [ "$swap" = 1 ]; then ordner="Ergebnisse/seiten_vertauscht"; praefix="basislauf_${agent}_vertauscht"
        elif [ "$seed" = 2026 ]; then ordner="Ergebnisse/basislauf_2026_09_26_partien"; praefix="basislauf_${agent}"
        else ordner="Ergebnisse/seed_${seed}"; praefix="basislauf_${agent}_seed${seed}"; fi
        katalog=()
    else
        art="$(echo "$variante" | cut -d_ -f2)"; p="$(echo "$variante" | cut -d_ -f3)"
        ordner="Ergebnisse/${art}/${art}_${p}/schildtraeger"; praefix="${variante}_${agent}"
        [ "$swap" = 1 ] && praefix="${praefix}_vertauscht"
        katalog=(--variant "$variante")
    fi
    mkdir -p "$ordner"
    pfad="$ordner/$praefix"
    log "START $praefix ($agent, $variante, Seed $seed ${extra[*]:-})"
    start_iso="$(date -Iseconds)"; start_s="$(date +%s)"
    status_lauf="abgeschlossen"
    if ! dotnet "$DLL" full-baseline "${pfad}_paarungen.csv" "${pfad}_kompositionen.csv" "${pfad}_helden.csv" \
            "$seed" "$agent" "${katalog[@]}" "${extra[@]}" --threads "$THREADS" > "${pfad}_lauf.log" 2>&1; then
        status_lauf="fehlgeschlagen"
    fi
    ende_iso="$(date -Iseconds)"; dauer=$(( $(date +%s) - start_s ))
    cat > "${pfad}_maschine.txt" <<EOF
Maschine: $MASCHINE
vCPUs: $(nproc)
CPU: $CPU
RAM: $RAM
Threads: $THREADS
Betriebssystem: $(. /etc/os-release && echo "$PRETTY_NAME")
.NET: $(dotnet --version)
Git: $COMMIT
Start: $start_iso
Ende: $ende_iso
Dauer_s: $dauer
Status: $status_lauf
EOF
    tar -czf "$PAKETE/${praefix}.tar.gz" "$ordner/${praefix}"_*
    log "ENDE  $praefix: $status_lauf, $((dauer / 60)) min -> $PAKETE/${praefix}.tar.gz"
done < "$LISTE"

log "FERTIG. Pakete herunterladen, z.B.: scp '<vm>:$PAKETE/*.tar.gz' . ; lokal im Repository-Ordner: for f in *.tar.gz; do tar -xzf \"\$f\"; done"
