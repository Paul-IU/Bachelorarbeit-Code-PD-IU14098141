using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Führt eine vollständige Partie durch: nimmt eine bereits aufgestellte
/// <see cref="GameState"/> entgegen, wickelt Runde für Runde die Züge aller
/// lebenden Helden ab (siehe <see cref="TurnOrder"/> für die Zugreihenfolge
/// und Anforderung 6 aus Teil B für das Aktionsbudget), erkennt das
/// Partieende und liefert das Ergebnis als <see cref="MatchResult"/>.
///
/// Die Partiesteuerung trifft KEINE Annahme über die konkrete
/// Entscheidungslogik der beiden Seiten - sie kennt nur die
/// <see cref="IAgent"/>-Schnittstelle.
///
/// ZUFALL: Die einzige direkt hier verortete Zufallsentscheidung ist das
/// Startteam (ein einzelner Wurf aus der seed-gesteuerten
/// <see cref="Random"/>-Instanz, siehe <see cref="Run"/>) - AUSSER, der
/// Aufrufer legt es über <c>startingTeamOverride</c> explizit fest (siehe
/// dort; für die systematische Batch-Simulation über feste Ausgangslagen,
/// Teil A der Aufgabenstellung, ist das beginnende Team ja bereits Teil der
/// Ausgangslage selbst, nicht mehr zufällig zu bestimmen). Dieselbe Random-
/// Instanz wird anschließend an jede <see cref="IAgent.DecideAction"/>-
/// Anfrage weitergereicht - es gibt also genau EINEN Zufallsstrom pro
/// Partie, vollständig durch <c>seed</c> bestimmt. Kein globaler oder
/// statischer Zufallsgenerator, keine Zeitabhängigkeit.
///
/// EREIGNISMELDUNG (Anforderung 16-19 aus dem Schnittstellen-Teil): Sowohl
/// das Aufzeichnen ins Replay-Protokoll (<c>recordReplay</c>) als auch das
/// LIVE-Melden an einen Zuhörer (<c>onEvent</c>) laufen über GENAU DIESELBE
/// Ereignis-Grundlage - <see cref="ReplayEntry"/> - statt über zwei getrennte
/// Mechanismen (Anforderung 17). Beide sind unabhängig voneinander
/// zuschaltbar: <c>recordReplay=false, onEvent=null</c> erzeugt nachweisbar
/// KEINEN einzigen <see cref="ReplayEntry"/> (siehe die private Methode
/// "ShouldEmit" weiter unten - an jeder Aufrufstelle wird das teure
/// Erzeugen des Eintrags selbst übersprungen, nicht nur dessen
/// Weiterverarbeitung), damit auch bei
/// hunderttausenden Partien im Batch-Lauf kein Mehraufwand entsteht
/// (Anforderung 19). Die Spiellogik selbst weiß nichts über eine etwaige
/// Oberfläche - <c>onEvent</c> ist ein bloßer Delegat, dessen Existenz oder
/// Verhalten den Spielablauf nicht beeinflusst.
/// </summary>
public sealed class MatchController
{
    private readonly GameState _state;
    private readonly IAgent _teamAAgent;
    private readonly IAgent _teamBAgent;
    private readonly int _seed;
    private readonly bool _recordReplay;
    private readonly Action<ReplayEntry>? _onEvent;

    /// <summary>
    /// Referenzen auf ALLE zu Partiebeginn angetretenen Einheiten (beider
    /// Teams), unabhängig davon, ob sie später besiegt und aus
    /// <see cref="GameState.Units"/> entfernt werden. Wird für die
    /// Endauswertung (HP-Summen über das gesamte Team) gebraucht, da eine
    /// besiegte Unit zwar aus der GameState-Buchführung verschwindet, ihre
    /// Eigenschaften (MaxHitPoints, CurrentHitPoints=0) als Objekt aber
    /// weiterhin lesbar bleiben. Enthält NIE Klone, da diese erst während
    /// der Partie entstehen, nach diesem Konstruktor-Aufruf.
    /// </summary>
    private readonly IReadOnlyList<Unit> _initialUnits;

    private readonly IReadOnlyList<UnitSetup> _initialSetup;

    /// <summary>
    /// Erstellt eine neue Partiesteuerung für eine bereits vollständig
    /// aufgestellte Partie (Spielfeld und alle Einheiten sind bereits über
    /// <paramref name="state"/> platziert - die Erzeugung der Einheiten
    /// selbst, inklusive Vergabe ihrer <see cref="Unit.Id"/>, ist bewusst
    /// Sache des Aufrufers und nicht des MatchControllers, siehe Hinweis zu
    /// UnitIds in <see cref="TurnOrder"/>).
    /// </summary>
    /// <param name="state">Die aufgestellte Partie (Board + Einheiten beider Teams).</param>
    /// <param name="teamAAgent">Entscheidungslogik für Team A.</param>
    /// <param name="teamBAgent">Entscheidungslogik für Team B.</param>
    /// <param name="seed">Seed für sämtliche Zufallsentscheidungen dieser Partie.</param>
    /// <param name="recordReplay">
    /// Ob ein vollständiges Replay-Protokoll aufgezeichnet werden soll.
    /// Bei false entsteht keinerlei zusätzliche Liste oder Zuteilung dafür.
    /// </param>
    /// <param name="onEvent">
    /// Optionaler Live-Empfänger für jedes einzelne Ereignis, in der
    /// Reihenfolge seines Auftretens - siehe Klassendokumentation. Unabhängig
    /// von <paramref name="recordReplay"/> nutzbar (auch ohne vollständige
    /// Protokollierung, z.B. für eine Oberfläche, die nur "live" mitspielen,
    /// aber keinen kompletten Log im Speicher halten will).
    /// </param>
    public MatchController(GameState state, IAgent teamAAgent, IAgent teamBAgent, int seed, bool recordReplay = false, Action<ReplayEntry>? onEvent = null)
    {
        _state = state;
        _teamAAgent = teamAAgent;
        _teamBAgent = teamBAgent;
        _seed = seed;
        _recordReplay = recordReplay;
        _onEvent = onEvent;

        // Momentaufnahme der Ausgangsaufstellung, bevor sich irgendetwas
        // bewegt oder irgendjemand Schaden nimmt.
        _initialUnits = state.Units.ToList();
        _initialSetup = _initialUnits
            .Select(u => new UnitSetup(u.Id, u.Team, u.Position))
            .OrderBy(s => s.StartPosition.Q)
            .ThenBy(s => s.StartPosition.R)
            .ThenBy(s => s.UnitId)
            .ToList();
    }

    /// <summary>
    /// Ob an dieser Stelle überhaupt ein <see cref="ReplayEntry"/> erzeugt
    /// werden muss (Anforderung 19: kein Mehraufwand bei deaktivierter
    /// Protokollierung). Bewusst anhand des LOKAL übergebenen
    /// <paramref name="replayEntries"/>-Parameters entschieden, nicht anhand
    /// von <c>_recordReplay</c>: <see cref="ExecuteTurn"/> ist absichtlich
    /// <c>internal</c> und wird von Tests auch DIREKT (unter Umgehung von
    /// <see cref="Run"/>) mit einer eigenen, unabhängigen Sammel-Liste
    /// aufgerufen - <c>_recordReplay</c> spiegelt in diesem Fall gar nicht
    /// wider, ob der Aufrufer protokollieren möchte.
    /// </summary>
    private bool ShouldEmit(List<ReplayEntry>? replayEntries) => replayEntries is not null || _onEvent is not null;

    private void Emit(List<ReplayEntry>? sink, ReplayEntry entry)
    {
        sink?.Add(entry);
        _onEvent?.Invoke(entry);
    }

    /// <summary>
    /// Führt die Partie bis zum Ende aus (Auslöschung eines Teams oder
    /// Rundenlimit) und liefert das Ergebnis.
    /// </summary>
    /// <param name="startingTeamOverride">
    /// Legt das beginnende Team FEST fest, statt es per Zufall zu bestimmen
    /// (siehe Klassendokumentation) - für die systematische Batch-Simulation
    /// über vorgegebene Ausgangslagen (Anforderung 3/4 aus Teil A), bei denen
    /// das beginnende Team bereits Teil der Ausgangslage ist. Null (Standard)
    /// erhält das bisherige Verhalten (zufällig über den Seed).
    /// </param>
    public MatchResult Run(Team? startingTeamOverride = null)
    {
        var random = new Random(_seed);

        // Einziger Zufallswurf außerhalb der Agenten-Entscheidungen: welches
        // Team eröffnet. random.Next(2) liefert 0 oder 1, beide mit
        // gleicher Wahrscheinlichkeit. Wird übersprungen, wenn der Aufrufer
        // das beginnende Team bereits vorgibt - WICHTIG: der Aufruf entfällt
        // dann vollständig (nicht nur das Ergebnis wird verworfen), damit der
        // nachfolgende Zufallsstrom für die Agenten-Entscheidungen exakt
        // gleich bleibt, ob man nun mit oder ohne Override startet.
        var startingTeam = startingTeamOverride ?? (random.Next(2) == 0 ? Team.TeamA : Team.TeamB);

        var replayEntries = _recordReplay ? new List<ReplayEntry>() : null;

        int roundNumber = 0;
        while (true)
        {
            roundNumber++;
            _state.SetCurrentRound(roundNumber);

            if (ShouldEmit(replayEntries))
            {
                Emit(replayEntries, ReplayEntry.ForRoundStarted(roundNumber));
            }

            // Feld-verankerte Effekte (Feldsperren, Flächen-Schadens-
            // reduktion) haben keinen eigenen Zug, an dem sie ticken
            // könnten - ihre Restdauer wird daher hier, einmal zu Beginn
            // JEDER Runde, verringert (siehe EffectSystem.TickFieldEffects).
            TickFieldEffectsWithEvents(replayEntries, roundNumber);

            // Rangfolge wird zu Beginn JEDER Runde neu aus den aktuell
            // lebenden Helden bestimmt (Anforderung 2). Als veränderliche
            // Liste (nicht foreach über die feste Rückgabe), damit ein
            // WÄHREND der Runde neu erzeugter Klon unmittelbar nach seinem
            // Erzeuger eingefügt werden kann, statt erst in der nächsten
            // Runde an die Reihe zu kommen (Anforderung 9: "handelt in der
            // Zugreihenfolge unmittelbar nach seinem Erzeuger").
            var roundOrder = new List<Unit>(TurnOrder.ComputeRoundOrder(_state.Units, startingTeam, _state));

            // Nach außen melden, WELCHE Reihenfolge soeben bestimmt wurde
            // (siehe ReplayActionType.RoundOrderDetermined) - eine
            // Oberfläche soll diese Berechnung niemals selbst wiederholen
            // müssen, sondern exakt diesen bereits ermittelten Wert
            // übernehmen können. Rein informativ, verändert _state nicht.
            if (ShouldEmit(replayEntries))
            {
                Emit(replayEntries, ReplayEntry.ForRoundOrderDetermined(roundNumber, roundOrder.Select(u => u.Id).ToList()));
            }

            for (int i = 0; i < roundOrder.Count; i++)
            {
                var unit = roundOrder[i];

                // Zwischen Berechnung der Rangfolge (bzw. dem Zeitpunkt der
                // dynamischen Einfügung unten) und dem eigenen Zug dieser
                // Einheit kann sie durch die Handlung eines anderen, früher
                // handelnden Helden bereits besiegt worden sein - ihr Zug
                // entfällt dann ersatzlos (Anforderung 2).
                if (unit.IsDefeated)
                {
                    continue;
                }

                var cloneBeforeTurn = FindLivingCloneOf(unit.Id);

                var agent = unit.Team == Team.TeamA ? _teamAAgent : _teamBAgent;
                var earlyResult = ExecuteTurn(unit, agent, random, replayEntries, roundNumber, startingTeam);
                if (earlyResult is not null)
                {
                    return earlyResult;
                }

                var cloneAfterTurn = FindLivingCloneOf(unit.Id);
                if (cloneAfterTurn is not null && cloneAfterTurn != cloneBeforeTurn)
                {
                    // In diesem Zug wurde ein neuer Klon erzeugt - er
                    // handelt UNMITTELBAR im Anschluss, noch in dieser
                    // Runde, unabhängig von der zu Rundenbeginn berechneten
                    // Rangfolge (Anforderung 9).
                    roundOrder.Insert(i + 1, cloneAfterTurn);

                    if (ShouldEmit(replayEntries))
                    {
                        Emit(replayEntries, ReplayEntry.ForCloneSpawned(roundNumber, unit, cloneAfterTurn));
                    }
                }
            }

            if (roundNumber >= MatchRules.RoundLimit)
            {
                return BuildResult(MatchEndReason.RoundLimit, roundNumber, startingTeam, replayEntries);
            }
        }
    }

    private void TickFieldEffectsWithEvents(List<ReplayEntry>? replayEntries, int roundNumber)
    {
        // EffectSystem selbst erzeugt weiterhin nur dann ReplayEntry-Objekte
        // (auch für den "EffectTicked"-Zwischenschritt), wenn tatsächlich
        // protokolliert werden soll - hier genügt daher dieselbe Weiche wie
        // überall sonst, es wird keine eigene Zuteilung vorgenommen.
        if (!ShouldEmit(replayEntries))
        {
            EffectSystem.TickFieldEffects(_state);
            return;
        }

        var sink = new List<ReplayEntry>();
        EffectSystem.TickFieldEffects(_state, sink, roundNumber);
        foreach (var entry in sink)
        {
            Emit(replayEntries, entry);
        }
    }

    private Unit? FindLivingCloneOf(Guid creatorId) =>
        _state.Units.FirstOrDefault(u => u.IsClone && u.CreatorId == creatorId);

    /// <summary>
    /// Momentaufnahme ALLER aktuell im Spiel befindlichen Einheiten (Id,
    /// Team, Klon-Kennzeichen, ggf. Erzeuger) - Grundlage für
    /// <see cref="LogUnitChanges"/>. Werden diese Angaben hier statt erst
    /// nach der Aktion festgehalten, bleiben sie auch dann bekannt, wenn die
    /// betroffene Einheit durch die Aktion aus <see cref="GameState.Units"/>
    /// entfernt wurde (eine besiegte Einheit ist danach schlicht nicht mehr
    /// dort auffindbar).
    /// </summary>
    private List<(Guid Id, Team Team, bool IsClone, Guid CreatorId)> SnapshotUnits() =>
        _state.Units.Select(u => (u.Id, u.Team, u.IsClone, u.CreatorId ?? Guid.Empty)).ToList();

    /// <summary>
    /// Protokolliert JEDE seit <paramref name="before"/> aus dem Spiel
    /// verschwundene Einheit generisch über einen Vorher/Nachher-Vergleich,
    /// statt für jeden möglichen Auslöser (direkter Treffer, kaskadierendes
    /// Verschwinden eines Klons beim Tod seines Erzeugers, Schaden über Zeit,
    /// Fähigkeitsschaden, ...) eine eigene Sonderprüfung zu brauchen - EIN
    /// Mechanismus deckt "Einheit besiegt" (Anforderung 16) UND "Klon
    /// verschwindet" (Anforderung 9/13) gleichermaßen ab. Ein Klon erhält
    /// dabei BEIDE Ereignisse (UnitDefeated UND CloneDespawned) - semantisch
    /// zwei unterschiedliche, für eine Oberfläche je eigenständig nützliche
    /// Aussagen ("eine Einheit ist gestorben" vs. "dieser Klon speziell ist
    /// verschwunden").
    /// </summary>
    private void LogUnitChanges(List<(Guid Id, Team Team, bool IsClone, Guid CreatorId)> before, int roundNumber, List<ReplayEntry>? replayEntries)
    {
        if (before.Count == 0)
        {
            return;
        }

        var stillAliveIds = _state.Units.Select(u => u.Id).ToHashSet();
        foreach (var snapshot in before)
        {
            if (stillAliveIds.Contains(snapshot.Id))
            {
                continue;
            }

            Emit(replayEntries, ReplayEntry.ForUnitDefeated(roundNumber, snapshot.Id, snapshot.Team));
            if (snapshot.IsClone)
            {
                Emit(replayEntries, ReplayEntry.ForCloneDespawned(roundNumber, snapshot.Id, snapshot.Team, snapshot.CreatorId));
            }
        }
    }

    /// <summary>
    /// Wickelt den vollständigen Zug einer einzelnen Einheit ab: tickt zu
    /// Beginn ihre eigenen Effekte und eine evtl. laufende Fähigkeits-
    /// Abklingzeit, fragt danach den zuständigen Agenten wiederholt nach
    /// der nächsten Teilaktion, bis das Aktionsbudget (genau eine Bewegung,
    /// genau eine Aktion - Grundangriff ODER Fähigkeit - freie Reihenfolge)
    /// verbraucht ist oder der Agent aktiv passt. Prüft nach JEDEM
    /// erfolgreichen Schadensereignis sofort die Siegbedingung - endet die
    /// Partie dadurch, wird das fertige <see cref="MatchResult"/>
    /// zurückgegeben und der Zug (auch ein eventuell noch offenes
    /// Bewegungsbudget derselben Einheit) sofort abgebrochen.
    ///
    /// Meldet als Rahmen um den gesamten Zug herum je ein "TurnStarted"- und
    /// "TurnEnded"-Ereignis (Anforderung 16) - auch dann, wenn die Einheit
    /// bereits zu Zugbeginn besiegt ist und ihr Zug ersatzlos entfällt (siehe
    /// unten): Jedes TurnStarted hat also garantiert genau ein passendes
    /// TurnEnded, was einer späteren Oberfläche eine einfache, verlässliche
    /// Klammerung für Zug-Animationen erlaubt.
    /// </summary>
    /// <remarks>
    /// Als <c>internal</c> statt <c>private</c> deklariert, damit Tests
    /// (über <c>InternalsVisibleTo</c>) das Aktionsbudget, die freie
    /// Reihenfolge und das Passen für EINEN Zug gezielt und ohne
    /// vollständigen <see cref="Run"/>-Durchlauf (der ansonsten bis zu 100
    /// Runden laufen würde) direkt prüfen können.
    ///
    /// BACHELORARBEIT-BEGRÜNDUNG DES EFFEKT-ZEITPUNKTS (Punkt D der
    /// Korrektur-Anforderungen): der Tick eines dauerhaften Effekts
    /// (Schaden über Zeit wie "Verbrennung", aber auch das Ablaufen JEDER
    /// Effektdauer sowie das Verringern einer laufenden Fähigkeits-
    /// Abklingzeit) erfolgt GENAU EINMAL, GANZ ZU BEGINN des Zugs der
    /// betroffenen Einheit - siehe die Aufrufe von
    /// <see cref="Effects.EffectSystem.TickUnitEffects"/> und
    /// <see cref="Heroes.Hero.TickAbilityCooldown"/> unten, beide VOR der
    /// While-Schleife, die den Agenten nach Teilaktionen fragt. Das gilt
    /// unabhängig davon, was die Einheit in diesem Zug tut oder ob sie
    /// überhaupt etwas tut.
    ///
    /// Frühere/naive Alternative wäre gewesen, einen Effekt erst im
    /// Zusammenhang mit der ERSTEN Teilaktion der Einheit (z.B. beim ersten
    /// Bewegungs- oder Angriffsversuch) ticken zu lassen. Das wurde bewusst
    /// verworfen, aus zwei Gründen:
    ///   1. UNABHÄNGIGKEIT VOM AGENTENVERHALTEN: tickt ein Effekt erst
    ///      "irgendwann während des Zugs", hinge der genaue Zeitpunkt (und
    ///      damit z.B., ob die Einheit VOR oder NACH dem tödlichen Tick noch
    ///      handeln kann) vom Verhalten des jeweiligen Agenten ab -
    ///      passiert eine Einheit sofort (kein Bewegungs-/Angriffsversuch),
    ///      hätte der Effekt in einer aktions-gekoppelten Variante unter
    ///      Umständen NIE getickt. Ein an einen festen ZEITPUNKT im
    ///      Rundenablauf gebundenes Ticken (Zugbeginn) ist dagegen für JEDEN
    ///      Agenten (Zufall, Mensch, spätere KI) identisch und deterministisch
    ///      reproduzierbar - eine notwendige Voraussetzung für die spätere
    ///      Massen-Simulation (Batch-Läufe über tausende Partien), bei der
    ///      Agentenverhalten variiert, die Regelanwendung selbst aber nicht.
    ///   2. STERBEN VOR DEM EIGENEN ZUG MUSS MÖGLICH SEIN: eine Einheit, die
    ///      durch einen tickenden Effekt (Gift, Verbrennung, ...) genug
    ///      Schaden nimmt, um zu sterben, darf in DIESEM Zug KEINE Aktion
    ///      mehr ausführen - alles andere wäre unplausibel ("stirbt an
    ///      Vergiftung, greift danach trotzdem noch an") und würde der
    ///      besiegten Einheit faktisch eine zusätzliche, unverdiente Aktion
    ///      verschaffen. Mit Ticken vor der Aktionsschleife (siehe die
    ///      IsDefeated-Prüfung direkt nach dem Tick unten) entfällt der Zug
    ///      in diesem Fall ERSATZLOS: der Agent wird kein einziges Mal
    ///      gefragt (siehe <c>agent.CallCount</c> in den zugehörigen Tests),
    ///      und die Siegbedingung wird SOFORT geprüft - eine Partie kann
    ///      also bereits durch einen reinen Effekt-Tick enden, ohne dass in
    ///      diesem Zug überhaupt eine TurnAction ausgeführt wurde.
    /// Siehe MatchControllerTests ("EFFEKT-ZEITPUNKT"-Abschnitt) für die
    /// automatisierten Tests, die genau diese drei Eigenschaften (Tick vor
    /// jeder Aktion, Zug entfällt bei Tod, Dauer sinkt genau einmal je
    /// eigenem Zug) dauerhaft absichern.
    /// </remarks>
    /// <returns>Das Partieergebnis, falls die Partie während dieses Zugs endet; sonst null.</returns>
    internal MatchResult? ExecuteTurn(Unit unit, IAgent agent, Random random, List<ReplayEntry>? replayEntries, int roundNumber, Team startingTeam)
    {
        // Auch direkt (unter Umgehung von Run()) aufgerufen gesetzt, damit
        // GameState.CurrentRoundNumber unabhängig vom Aufrufer konsistent
        // bleibt (Anforderung 18: "aktuelle Runde muss auslesbar sein").
        _state.SetCurrentRound(roundNumber);

        if (ShouldEmit(replayEntries))
        {
            Emit(replayEntries, ReplayEntry.ForTurnStarted(roundNumber, unit));
        }

        _state.SetCurrentActor(unit.Id, new TurnBudget(CanMove: true, CanAct: true));

        // Zu Beginn des Zugs: erst eigene Effekte ticken (z.B. Schaden über
        // Zeit), dann eine evtl. laufende Fähigkeits-Abklingzeit verringern
        // (derselbe Zeitpunkt wie Effekt-Ticks, siehe EffectSystem).
        //
        // Ein Tick kann NUR die tickende Einheit selbst treffen (siehe
        // EffectSystem.TickUnitEffects) - kann er sie also besiegen, wird
        // ausschließlich IHR EIGENES Team auf Auslöschung geprüft (nicht
        // pauschal beide Teams: in einem Testaufbau ohne jede Einheit des
        // gegnerischen Teams wäre "gegnerisches Team eliminiert" sonst
        // immer trivial wahr und würde jeden Zug fälschlich sofort
        // beenden, noch bevor der Agent überhaupt gefragt wurde).
        //
        // Momentaufnahme VOR dem Tick: stirbt "unit" selbst am eigenen
        // Schaden-über-Zeit-Effekt, kann das (wie bei jeder anderen
        // Niederlage auch) einen evtl. vorhandenen eigenen Klon mit in den
        // Tod reißen (siehe CombatSystem.HandleDefeat) - ohne diesen
        // Vorher/Nachher-Vergleich bliebe genau DIESER Auslöser für
        // UnitDefeated/CloneDespawned unentdeckt.
        var beforeTick = ShouldEmit(replayEntries) ? SnapshotUnits() : null;
        EffectSystem.TickUnitEffects(_state, unit, replayEntries, roundNumber);
        if (beforeTick is not null)
        {
            LogUnitChanges(beforeTick, roundNumber, replayEntries);
        }

        if (unit is Hero heroForCooldown)
        {
            heroForCooldown.TickAbilityCooldown();
        }

        if (unit.IsDefeated)
        {
            // Team lebt noch, aber der Zug dieser (nun besiegten) Einheit
            // entfällt - TurnEnded wird trotzdem gemeldet (jedes TurnStarted
            // hat garantiert ein passendes TurnEnded), und zwar VOR einem
            // eventuellen MatchEnded (der Zug schließt ab, bevor die Partie
            // als Ganzes endet).
            bool eliminated = IsTeamEliminated(unit.Team);
            EndTurn(unit, replayEntries, roundNumber);
            return eliminated ? BuildResult(MatchEndReason.Elimination, roundNumber, startingTeam, replayEntries) : null;
        }

        bool hasMoved = false;
        bool hasActed = false;

        // Schleife läuft, solange mindestens eine Budget-Komponente noch
        // offen ist - EndTurn (aktives Passen) verlässt sie jederzeit
        // vorzeitig, siehe return-Anweisung im switch unten.
        while (!hasMoved || !hasActed)
        {
            var budget = new TurnBudget(CanMove: !hasMoved, CanAct: !hasActed);
            _state.SetCurrentActor(unit.Id, budget);
            var action = agent.DecideAction(unit, _state, budget, random);

            switch (action)
            {
                case TurnAction.Move move when !hasMoved:
                    // Position VOR der Bewegung festhalten - unit.Position
                    // zeigt nach TryMove bereits auf das Ziel, siehe
                    // ReplayEntry.ForMove. Der vollständige Pfad (Anforderung
                    // 16: "mit Pfad") wird EBENFALLS vor der Bewegung
                    // bestimmt, aus demselben Grund.
                    var sourcePosition = unit.Position;
                    var path = ShouldEmit(replayEntries) ? unit.GetShortestPathTo(move.Target, _state) : null;

                    // Ein BewegungsVERSUCH verbraucht den Bewegungs-Slot
                    // unabhängig vom Erfolg - das schützt vor einer
                    // Endlosschleife durch einen Agenten, der wiederholt
                    // ein (aus welchem Grund auch immer) ungültiges Ziel
                    // liefert, und passt zur Lesart "der Versuch ist der
                    // Zugbestandteil, nicht zwingend sein Erfolg". Ein
                    // wohlverhaltener Agent, der Ziele stets aus
                    // unit.GetLegalMoves(...) wählt, erlebt das ohnehin nie.
                    bool moved = unit.TryMove(move.Target, _state);
                    if (moved && ShouldEmit(replayEntries))
                    {
                        Emit(replayEntries, ReplayEntry.ForMove(roundNumber, unit, sourcePosition, move.Target, path));
                    }
                    hasMoved = true;
                    break;

                case TurnAction.Attack attack when !hasActed:
                    bool attackWantsLogging = ShouldEmit(replayEntries);
                    var unitsBeforeAttack = attackWantsLogging ? SnapshotUnits() : null;
                    // TryAttackDetailed statt unit.TryAttack: liefert den
                    // TATSÄCHLICHEN Empfänger (kann sich durch eine
                    // Sichtlinien-Umleitung von attack.Target unterscheiden)
                    // und den TATSÄCHLICH angewendeten Schaden (nach
                    // Angreifer-Bonus und Ziel-Reduktion) - nur damit ist das
                    // Replay-Protokoll akkurat, statt den längst durch
                    // Modifikatoren überholten Rohwert zu loggen.
                    bool attacked = CombatSystem.TryAttackDetailed(unit, attack.Target, _state, out var actualAttackTarget, out var damageDealt);
                    if (attacked)
                    {
                        if (attackWantsLogging)
                        {
                            Emit(replayEntries, ReplayEntry.ForAttack(roundNumber, unit, actualAttackTarget, damageDealt));
                            LogUnitChanges(unitsBeforeAttack!, roundNumber, replayEntries);
                        }

                        // Prüfung nach JEDEM einzelnen Schadensereignis,
                        // nicht erst am Rundenende - sofort hier, noch bevor
                        // die Einheit ggf. ihr Bewegungsbudget nutzt.
                        if (IsTeamEliminated(Team.TeamA) || IsTeamEliminated(Team.TeamB))
                        {
                            EndTurn(unit, replayEntries, roundNumber);
                            return BuildResult(MatchEndReason.Elimination, roundNumber, startingTeam, replayEntries);
                        }
                    }
                    hasActed = true;
                    break;

                case TurnAction.UseAbility useAbility when !hasActed:
                    if (unit is Hero hero)
                    {
                        bool abilityWantsLogging = ShouldEmit(replayEntries);
                        var unitsBeforeAbility = abilityWantsLogging ? SnapshotUnits() : null;
                        bool used = AbilitySystem.TryUseAbility(hero, useAbility.Target, _state, useAbility.Mode, replayEntries, roundNumber);
                        if (used && abilityWantsLogging)
                        {
                            Emit(replayEntries, ReplayEntry.ForAbility(roundNumber, hero, useAbility.Target, useAbility.Mode, hero.Definition.Ability.Id));
                            LogUnitChanges(unitsBeforeAbility!, roundNumber, replayEntries);
                        }

                        if (used && (IsTeamEliminated(Team.TeamA) || IsTeamEliminated(Team.TeamB)))
                        {
                            EndTurn(unit, replayEntries, roundNumber);
                            return BuildResult(MatchEndReason.Elimination, roundNumber, startingTeam, replayEntries);
                        }
                    }
                    hasActed = true;
                    break;

                case TurnAction.EndTurn:
                    EndTurn(unit, replayEntries, roundNumber);
                    return null;

                default:
                    // Der Agent hat eine laut Budget nicht verfügbare
                    // Aktion gewählt (z.B. erneut Move trotz hasMoved, oder
                    // UseAbility für eine Nicht-Hero-Einheit). Als
                    // Fehlverhalten des Agenten gewertet - Zug wird
                    // sicherheitshalber beendet, damit ein fehlerhafter
                    // Agent keine Endlosschleife auslösen kann.
                    EndTurn(unit, replayEntries, roundNumber);
                    return null;
            }
        }

        EndTurn(unit, replayEntries, roundNumber);
        return null;
    }

    private void EndTurn(Unit unit, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        _state.SetCurrentActor(null, default);
        if (ShouldEmit(replayEntries))
        {
            Emit(replayEntries, ReplayEntry.ForTurnEnded(roundNumber, unit));
        }
    }

    /// <summary>
    /// Ein Team gilt als besiegt, wenn keine lebende HELDEN-Einheit mehr
    /// übrig ist - ein noch lebender Klon allein reicht NICHT (Anforderung
    /// 9: "ein Team ist besiegt, wenn alle echten Helden gefallen sind").
    /// </summary>
    private bool IsTeamEliminated(Team team) => !_state.Units.Any(u => u.Team == team && !u.IsClone);

    private MatchResult BuildResult(MatchEndReason endReason, int roundsPlayed, Team startingTeam, List<ReplayEntry>? replayEntries)
    {
        var teamAResult = BuildTeamResult(Team.TeamA);
        var teamBResult = BuildTeamResult(Team.TeamB);

        var winner = endReason == MatchEndReason.Elimination
            ? (teamAResult.SurvivingUnitCount > 0 ? MatchWinner.TeamA : MatchWinner.TeamB)
            : ResolveRoundLimitWinner(teamAResult, teamBResult);

        if (ShouldEmit(replayEntries))
        {
            Emit(replayEntries, ReplayEntry.ForMatchEnded(roundsPlayed, winner, endReason));
        }

        ReplayLog? replay = replayEntries is null
            ? null
            : new ReplayLog(_seed, startingTeam, _initialSetup, replayEntries);

        return new MatchResult(
            winner,
            endReason,
            roundsPlayed,
            teamAResult,
            teamBResult,
            _seed,
            _initialSetup,
            startingTeam,
            replay);
    }

    /// <summary>
    /// Zusammenfassung des Endzustands eines Teams - bewusst über
    /// <see cref="_initialUnits"/> (nie Klone, siehe dortige Dokumentation)
    /// statt über <see cref="GameState.Units"/> gebildet: die Heldenanzahl-
    /// und HP-Wertung beim Rundenlimit betrifft nur echte Helden
    /// (Anforderung 9), und besiegte Helden sollen weiterhin zur
    /// Gesamt-Maximal-HP des Teams zählen.
    /// </summary>
    private TeamResult BuildTeamResult(Team team)
    {
        var teamUnits = _initialUnits.Where(u => u.Team == team && !u.IsClone).ToList();
        int surviving = teamUnits.Count(u => !u.IsDefeated);
        int currentHp = teamUnits.Sum(u => u.CurrentHitPoints);
        int maxHp = teamUnits.Sum(u => u.MaxHitPoints);
        double fraction = maxHp == 0 ? 0.0 : (double)currentHp / maxHp;
        return new TeamResult(team, surviving, currentHp, maxHp, fraction);
    }

    /// <summary>
    /// Auflösung bei Erreichen des Rundenlimits, in dieser festen
    /// Reihenfolge:
    ///   a) Das Team mit mehr lebenden Helden gewinnt.
    ///   b) Bei Gleichstand: das Team mit dem höheren prozentualen
    ///      Gesamt-HP-Anteil (aktuelle HP / maximale HP über das ganze
    ///      Team) gewinnt.
    ///   c) Bei erneutem Gleichstand: Unentschieden.
    ///
    /// Diese Kaskade ist eine bewusste Design-Entscheidung (mehr lebende
    /// Kämpfer wiegt schwerer als reine HP-Summe, weil eine tote Einheit im
    /// weiteren Verlauf gar nicht mehr beitragen könnte) und wird in der
    /// Bachelorarbeit begründet - daher hier als eigene, klar benannte
    /// Methode statt inline verstreuter Vergleichslogik.
    /// </summary>
    private static MatchWinner ResolveRoundLimitWinner(TeamResult teamA, TeamResult teamB)
    {
        if (teamA.SurvivingUnitCount != teamB.SurvivingUnitCount)
        {
            return teamA.SurvivingUnitCount > teamB.SurvivingUnitCount ? MatchWinner.TeamA : MatchWinner.TeamB;
        }

        if (teamA.RemainingHitPointsFraction != teamB.RemainingHitPointsFraction)
        {
            return teamA.RemainingHitPointsFraction > teamB.RemainingHitPointsFraction ? MatchWinner.TeamA : MatchWinner.TeamB;
        }

        return MatchWinner.Draw;
    }
}
