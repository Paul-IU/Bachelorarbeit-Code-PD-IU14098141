using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// VORAUSSCHAUENDER HEURISTISCHER AGENT (Bachelorarbeit): derselbe Ansatz wie
/// <see cref="HeuristicAgent"/> - dieselbe Bewertungsfunktion
/// (<see cref="TurnEvaluator"/>), dieselben Zugfolgen, dieselbe stabile
/// Sortierung -, aber mit ZWEI Suchebenen statt einer. Er bewertet eine eigene
/// Zugfolge nicht nach dem Zustand direkt danach, sondern nach dem Zustand,
/// nachdem die nächste gegnerische Einheit darauf geantwortet hat.
///
/// GEWICHTUNG: neutral 1:1 (<see cref="HeuristicWeights.NeutralProfile"/>). Der
/// Agent soll sich von den einstufigen Agenten allein durch die Suchtiefe
/// unterscheiden; eine eigene Ausrichtung (2:1 oder 1:2) würde diesen Unterschied
/// mit einem zweiten vermischen.
///
/// SUCHTIEFE - GENAU ZWEI EBENEN:
///   Ebene 1: alle eigenen Zugfolgen, die das Budget erlaubt ([], [B], [A], [B, A],
///            [A, B]), werden auf einer Kopie ausgeführt und einstufig bewertet -
///            genau wie beim <see cref="HeuristicAgent"/>.
///   Ebene 2: für die besten K davon (siehe "Beschneidung") wird zusätzlich der Zug
///            der NÄCHSTEN GEGNERISCHEN Einheit simuliert (siehe "Nächste Einheit"):
///            erst ihr Zugbeginn (Effekt-Ticks, Abklingzeit - wie in
///            MatchController.ExecuteTurn), dann ihre Zugfolge. Bewertet wird der
///            Zustand danach, und zwar gegen den Zustand VOR der eigenen Entscheidung
///            - die Bewertung enthält also den eigenen Zug und die Antwort darauf.
///   Gewählt wird die Zugfolge mit der besten Bewertung auf Ebene 2; zurückgegeben
///   wird, wie beim <see cref="HeuristicAgent"/>, nur ihr erster Schritt.
///
/// GEGNERMODELL (Entscheidung "Modell B"): die simulierte Einheit wählt die
/// Antwort, die für den AGENTEN am schlechtesten ist - sie minimiert die eigene
/// Bewertung des Agenten (dieselbe Bewertungsfunktion, dieselben Gewichte, mit
/// vertauschtem Vorzeichen). Das ist die klassische Annahme einer Zweiebenen-Suche
/// (Minimax über eine Antwort): der Agent verlässt sich nicht darauf, dass der Gegner
/// ihm entgegenkommt.
///
/// VERWORFEN: MODELL A - der Gegner bewertet aus der Sicht SEINES Teams, wie ein
/// gewöhnlicher heuristischer Agent (<see cref="TurnEvaluator.DecisionContext"/> mit
/// dem gegnerischen Team). Wegen der Asymmetrie der Bewertung (Angriffsmöglichkeit
/// Gewicht 2,0, Bedrohung 1,0, siehe <see cref="HeuristicAgentSettings.AttackOpportunityWeight"/>)
/// läuft ein solcher Gegner bereitwillig heran: aus Sicht des Agenten bringt das
/// +2X Angriffsmöglichkeit gegen −X Bedrohung. Beispiel aus der Einzelanalyse:
/// Passen +40 (der Gegner rückt heran und greift an: Angriffsmöglichkeit +160,
/// Lebenspunkte −120), eigener Angriff −80 (Vergeltung voll). Der Agent lernte so,
/// dass Abwarten belohnt wird - kein realistisches Gegnermodell, sondern eine
/// Artefaktschleife der Gewichtung. Vormessung (12 Partien): 41,7 % Rundenlimit,
/// 24,9 Runden im Schnitt, 0,08 Angriffe je Heldenzug (einstufig: 0 %, 6,8 Runden,
/// 0,83 Angriffe).
///
/// KEINE REKURSION: die simulierte Einheit entscheidet EINSTUFIG, ohne eigene
/// Vorausschau - sonst würde sie ihrerseits vorausschauen, deren Gegner wieder, und
/// die Suche liefe unbegrenzt weiter. Strukturell gesichert: die Gegnersuche
/// (<see cref="WorstReplyScore"/>) ruft ausschließlich die einstufige Aufzählung auf,
/// nie die zweite Ebene. Bekannte, akzeptierte Nebenwirkung: der simulierte Gegner
/// spielt schwächer als der Agent selbst.
///
/// NÄCHSTE EINHEIT: die zweite Ebene simuliert die nächste GEGNERISCHE Einheit in
/// der Zugreihenfolge - die nächste in der laufenden Runde
/// (<see cref="GameState.CurrentRoundOrder"/>), sonst die erste gegnerische der
/// nächsten Runde (<see cref="TurnOrder.ComputeRoundOrder"/> auf dem Zustand nach
/// der eigenen Zugfolge, mit <see cref="GameState.StartingTeam"/>). Einheiten, die
/// die eigene Zugfolge besiegt, werden dabei übersprungen.
///   Ist die als nächste handelnde Einheit eine EIGENE (der eigene Klon direkt nach
///   seinem Erzeuger, oder ein eigener Held, wenn der Gegner auf diesem Rang keine
///   Einheit mehr hat), wird sie übersprungen, ohne ihren Zug zu simulieren; die
///   zweite Ebene simuliert trotzdem die nächste gegnerische. Begründung:
///   a) Vergleichbarkeit - die nächste Einheit hängt von der Zugfolge ab (ein
///      erzeugter Klon, ein besiegter Gegner). Entfiele die zweite Ebene nur für
///      manche Zugfolgen, würden einstufige und zweistufige Bewertungen
///      miteinander verglichen; die zweistufige enthält den gegnerischen Schaden,
///      die einstufige nicht - Zugfolgen ohne Gegnerantwort wären systematisch
///      bevorzugt (etwa das Trugbild, weil danach der eigene Klon handelt).
///   b) Den Zug einer eigenen Einheit zu simulieren wäre eine dritte, eigene
///      Suchebene mit eigenem Entscheidungsmodell - genau das soll die
///      Zwei-Ebenen-Grenze ausschließen.
///   Folge: die Züge der dazwischen handelnden eigenen Einheiten fehlen in der
///   Bewertung; das trifft alle Zugfolgen gleich. Ist KEINE gegnerische Einheit mehr
///   übrig (die Zugfolge beendet die Partie), zählt die einstufige Bewertung - der
///   Endzustand ist dann endgültig.
///   Fehlt die Zugreihenfolge (Agent außerhalb des MatchControllers, kein Startteam
///   bekannt), entfällt die zweite Ebene für ALLE Zugfolgen gleichermaßen; der Agent
///   entscheidet dann einstufig mit neutraler Gewichtung.
///   Vereinfachung: stirbt die simulierte Einheit an ihrem eigenen Schaden über Zeit
///   zu Zugbeginn, zählt ihr Zug als leer; die dann tatsächlich folgende Einheit
///   wird nicht weiter gesucht.
///
/// BESCHNEIDUNG: nur die besten K eigenen Zugfolgen nach einstufiger Bewertung
/// erhalten die zweite Ebene (K = <see cref="HeuristicAgentSettings.LookaheadPruningWidth"/>).
/// Die übrigen scheiden aus. Die leere Zugfolge (passen, Bewertung 0) nimmt wie jede
/// andere am Wettbewerb um die K Plätze teil, ohne Sonderstellung.
///   Messung (Ergebnisse/lookahead_stichprobe, 240 Partien, Seed 2026): gegenüber
///   K = 5 ändern sich bei K = 10 nur 1,2 % der Entscheidungen bei +57 % Rechenzeit
///   je Entscheidung, bei K = 3 nur 1,5 % bei −36 %. Die Entscheidung hängt also
///   fast immer an den wenigen einstufig besten Zugfolgen; eine breitere zweite
///   Ebene kostet viel und ändert kaum etwas. (Endgültiger Wert von K: siehe
///   Entscheidung nach der Laufzeitmessung.)
///
/// ZUSTANDSLOS UND PLANUNGSTREU: wie beim <see cref="HeuristicAgent"/> plant der
/// Agent bei jeder Anfrage den RESTLICHEN Zug neu. Die Endzustände der verbliebenen
/// Zugfolgen und damit ihre Bewertungen auf Ebene 2 sind dieselben wie bei der
/// ersten Anfrage, die zweite Anfrage führt also die geplante Fortsetzung aus.
///
/// DETERMINISMUS UND ETIKETTEN-UNABHÄNGIGKEIT: die übergebene Random-Instanz wird
/// nicht benutzt. Die Aufzählung folgt der festen Reihenfolge des
/// <see cref="HeuristicAgent"/> (dessen CandidateKey): zuerst die leere
/// Zugfolge, dann je erstem Schritt die Folge aus ihm allein und danach seine
/// Fortsetzungen. Die Rangliste der Ebene 1 ist stabil (gleiche Bewertung: frühere
/// in dieser Reihenfolge zuerst). Auf Ebene 2 gewinnt bei Gleichstand die in der
/// Rangliste frühere Zugfolge (strikter Vergleich), also die einstufig bessere.
/// Die Gegnersimulation verwendet dieselbe Regel wie der <see cref="HeuristicAgent"/>
/// (Passen gewinnt Gleichstände, sonst die erste in der festen Reihenfolge).
/// </summary>
public sealed class LookaheadHeuristicAgent : IAgent
{
    private readonly HeuristicWeights _weights;
    private readonly int _pruningWidth;
    private readonly Action<LookaheadProbeEvent>? _probe;

    /// <summary>Standardkonfiguration: neutrale Gewichtung, K aus <see cref="HeuristicAgentSettings"/>.</summary>
    public LookaheadHeuristicAgent()
        : this(HeuristicWeights.NeutralProfile, HeuristicAgentSettings.LookaheadPruningWidth)
    {
    }

    public LookaheadHeuristicAgent(HeuristicWeights weights, int pruningWidth)
        : this(weights, pruningWidth, null)
    {
    }

    /// <summary>
    /// Nur für Tests: <paramref name="probe"/> wird bei jeder bewerteten Zugfolge
    /// und jeder simulierten Gegnerantwort mit der Suchebene aufgerufen (siehe
    /// <see cref="LookaheadProbeEvent"/>). Beeinflusst die Entscheidung nicht.
    /// </summary>
    internal LookaheadHeuristicAgent(HeuristicWeights weights, int pruningWidth, Action<LookaheadProbeEvent>? probe)
    {
        if (pruningWidth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pruningWidth), "Mindestens eine Zugfolge muss die zweite Ebene erhalten.");
        }

        _weights = weights;
        _pruningWidth = pruningWidth;
        _probe = probe;
    }

    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        var ownContext = new TurnEvaluator.DecisionContext(state, unit.Team);

        // Ebene 1: einstufig bewerten, nur die besten K behalten.
        var ranked = EnumerateBest(unit, state, budget.CanMove, budget.CanAct, after => Score(ownContext, after), passScore: 0.0, _pruningWidth, level: 1);

        // Ebene 2: Antwort der nächsten gegnerischen Einheit. Strikter Vergleich in
        // Ranglisten-Reihenfolge - bei Gleichstand bleibt die einstufig bessere.
        TurnAction best = ranked[0].First ?? TurnAction.EndTurn.Instance;
        double bestScore = double.NegativeInfinity;
        foreach (var sequence in ranked)
        {
            double score = SecondLevel(unit, state, sequence, ownContext);
            if (score > bestScore)
            {
                bestScore = score;
                best = sequence.First ?? TurnAction.EndTurn.Instance;
            }
        }

        return best;
    }

    /// <summary>
    /// Bewertung einer eigenen Zugfolge nach der Antwort der nächsten gegnerischen
    /// Einheit. Ohne gegnerische Einheit bzw. ohne bekannte Zugreihenfolge: die
    /// einstufige Bewertung (siehe Klassendokumentation, "Nächste Einheit").
    /// </summary>
    private double SecondLevel(Unit actor, GameState state, Sequence sequence, TurnEvaluator.DecisionContext ownContext)
    {
        var responder = NextEnemyUnit(actor, state, sequence.After);
        if (responder is null)
        {
            return sequence.Score;
        }

        var simulation = sequence.After.CloneForSimulation();
        var opponent = simulation.Units.First(u => u.Id == responder.Id);
        _probe?.Invoke(new LookaheadProbeEvent(2, opponent.Id, IsOpponentTurnStart: true));

        // Zugbeginn wie in MatchController.ExecuteTurn: erst Effekt-Ticks, dann Abklingzeit.
        EffectSystem.TickUnitEffects(simulation, opponent);
        if (opponent is Hero hero)
        {
            hero.TickAbilityCooldown();
        }

        return opponent.IsDefeated
            ? Score(ownContext, simulation)
            : WorstReplyScore(opponent, simulation, ownContext);
    }

    /// <summary>
    /// EINSTUFIGE Entscheidung der simulierten gegnerischen Einheit nach Modell B
    /// (siehe Klassendokumentation): unter ihren Zugfolgen die, nach der die
    /// Bewertung des Agenten am niedrigsten ist - ohne weitere Vorausschau. Liefert
    /// diese Bewertung. Gleichstand: Passen zuerst, sonst die erste in der festen
    /// Reihenfolge (dieselbe Regel wie beim <see cref="HeuristicAgent"/>).
    /// </summary>
    private double WorstReplyScore(Unit opponent, GameState state, TurnEvaluator.DecisionContext ownContext) =>
        -EnumerateBest(opponent, state, canMove: true, canAct: true, after => -Score(ownContext, after),
            passScore: -Score(ownContext, state), capacity: 1, level: 2)[0].Score;

    /// <summary>
    /// Die nächste gegnerische Einheit nach <paramref name="actor"/> in der
    /// Zugreihenfolge, gemessen am Zustand <paramref name="after"/> nach der eigenen
    /// Zugfolge; null, wenn die Zugreihenfolge unbekannt ist oder kein Gegner mehr lebt.
    /// </summary>
    internal static Unit? NextEnemyUnit(Unit actor, GameState state, GameState after)
    {
        if (state.StartingTeam is not { } startingTeam)
        {
            return null;
        }

        var order = state.CurrentRoundOrder;
        int index = -1;
        for (int i = 0; i < order.Count; i++)
        {
            if (order[i] == actor.Id)
            {
                index = i;
                break;
            }
        }

        // Rest der laufenden Runde (nur, wenn der Akteur in ihr steht - bei einem direkt
        // über ExecuteTurn ausgeführten Zug fehlt die Reihenfolge, dann nächste Runde).
        if (index >= 0)
        {
            for (int i = index + 1; i < order.Count; i++)
            {
                var candidate = after.Units.FirstOrDefault(u => u.Id == order[i]);
                if (candidate is not null && !candidate.IsDefeated && candidate.Team != actor.Team)
                {
                    return candidate;
                }
            }
        }

        return TurnOrder.ComputeRoundOrder(after.Units, startingTeam, after).FirstOrDefault(u => u.Team != actor.Team);
    }

    /// <summary>
    /// Zählt alle Zugfolgen im Budget auf, bewertet sie einstufig und liefert die
    /// besten <paramref name="capacity"/> - absteigend nach Bewertung, bei Gleichstand
    /// in Aufzählungsreihenfolge. Die leere Zugfolge (Bewertung 0) steht in der
    /// Aufzählung zuerst und gewinnt damit Gleichstände. Wird für beide Ebenen
    /// verwendet und ruft selbst keine weitere Ebene auf.
    /// </summary>
    private List<Sequence> EnumerateBest(Unit actor, GameState state, bool canMove, bool canAct,
        Func<GameState, double> score, double passScore, int capacity, int level)
    {
        var best = new List<Sequence>(capacity + 1) { new(null, state, passScore) };

        foreach (var (first, _) in HeuristicAgent.Candidates(actor, state, canMove, canAct))
        {
            var afterFirst = state.CloneForSimulation();
            if (!HeuristicAgent.Apply(first, actor.Id, afterFirst))
            {
                continue;
            }

            _probe?.Invoke(new LookaheadProbeEvent(level, actor.Id, IsOpponentTurnStart: false));
            Offer(new Sequence(first, afterFirst, score(afterFirst)));

            var actorAfterFirst = afterFirst.Units.FirstOrDefault(u => u.Id == actor.Id);
            if (actorAfterFirst is null || actorAfterFirst.IsDefeated)
            {
                continue;
            }

            bool firstWasMove = first is TurnAction.Move;
            foreach (var (second, _) in HeuristicAgent.Candidates(actorAfterFirst, afterFirst, canMove && !firstWasMove, canAct && firstWasMove))
            {
                var afterSecond = afterFirst.CloneForSimulation();
                if (HeuristicAgent.Apply(second, actor.Id, afterSecond))
                {
                    _probe?.Invoke(new LookaheadProbeEvent(level, actor.Id, IsOpponentTurnStart: false));
                    Offer(new Sequence(first, afterSecond, score(afterSecond)));
                }
            }
        }

        return best;

        // Einfügen hinter allen mindestens gleich guten - so bleibt die Aufzählungsreihenfolge bei Gleichstand erhalten.
        void Offer(Sequence sequence)
        {
            int position = best.Count;
            while (position > 0 && best[position - 1].Score < sequence.Score)
            {
                position--;
            }

            if (position < capacity)
            {
                best.Insert(position, sequence);
                if (best.Count > capacity)
                {
                    best.RemoveAt(best.Count - 1);
                }
            }
        }
    }

    private double Score(TurnEvaluator.DecisionContext context, GameState after) => context.Evaluate(after).Weighted(_weights);

    /// <summary>Eine Zugfolge: ihr erster Schritt (null = passen), der Zustand danach und ihre einstufige Bewertung.</summary>
    private sealed record Sequence(TurnAction? First, GameState After, double Score);
}

/// <summary>
/// Beobachtung der Suche des <see cref="LookaheadHeuristicAgent"/> (nur für Tests).
/// </summary>
/// <param name="Level">Suchebene: 1 = eigene Zugfolge, 2 = Zug der simulierten gegnerischen Einheit.</param>
/// <param name="UnitId">Die Einheit, deren Zugfolge bewertet bzw. deren Zug simuliert wird.</param>
/// <param name="IsOpponentTurnStart">true = eine Gegnerantwort beginnt (einmal je Zugfolge auf Ebene 2); false = eine Zugfolge wurde bewertet.</param>
internal readonly record struct LookaheadProbeEvent(int Level, Guid UnitId, bool IsOpponentTurnStart);
