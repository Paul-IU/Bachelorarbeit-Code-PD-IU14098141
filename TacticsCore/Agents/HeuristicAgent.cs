using TacticsCore.Abilities;
using TacticsCore.Combat;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// HEURISTISCHER AGENT (Bachelorarbeit): wählt den Zug, dessen Ergebnis die
/// generische Bewertungsfunktion <see cref="TurnEvaluator"/> am höchsten
/// bewertet. Der aggressive und der defensive Agent sind DIESELBE Klasse und
/// unterscheiden sich ausschließlich in den Gewichten (<see cref="HeuristicWeights"/>,
/// Werte in <see cref="HeuristicAgentSettings"/>) - kein heldenspezifisches
/// Tuning, keine Sonderregel für eine Fähigkeit.
///
/// ABLAUF: Ein Zug besteht aus höchstens einer Bewegung und höchstens einer
/// Aktion (Angriff oder Fähigkeit) in freier Reihenfolge. Aufgezählt werden
/// alle Zugfolgen, die das verbleibende Budget erlaubt: [], [B], [A], [B, A],
/// [A, B]. Jede wird auf einer Kopie des Spielzustands ausgeführt
/// (<see cref="GameState.CloneForSimulation"/>) und der Zustand DANACH mit dem
/// Zustand davor verglichen. Es werden keine Folgezüge und keine gegnerischen
/// Antworten simuliert.
///
/// ZUSTANDSLOS: <see cref="MatchController"/> fragt pro Teilaktion einzeln. Der
/// Agent plant bei jeder Anfrage den gesamten RESTLICHEN Zug neu und gibt nur
/// dessen ersten Schritt zurück. Weil die Simulation deterministisch ist, führt
/// die zweite Anfrage genau die zuvor geplante Fortsetzung aus - ein
/// gespeicherter Plan ist nicht nötig.
///
/// DETERMINISMUS UND ETIKETTEN-UNABHÄNGIGKEIT: die übergebene Random-Instanz
/// wird NICHT benutzt. Gleich bewertete Zugfolgen entscheidet die erste in
/// einer festen Reihenfolge (<see cref="CandidateKey"/>): Art des Schritts,
/// dann Zielfeld (q, r), dann eigene vor gegnerischen Einheiten, Helden vor
/// Klonen, dann Id (bei Klonen die ihres Erzeugers) - dieselbe Idee wie in
/// <see cref="AbilitySystem.GetLegalAbilityUses"/>. Innerhalb eines Teams bleibt
/// die Id-Reihenfolge bei einem Teamtausch erhalten, die Reihenfolge hängt
/// also nicht davon ab, welches Team die kleineren Ids trägt.
/// </summary>
public sealed class HeuristicAgent : IAgent
{
    private readonly HeuristicWeights _weights;

    public HeuristicAgent(HeuristicWeights weights) => _weights = weights;

    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        TurnAction best = TurnAction.EndTurn.Instance;
        double bestScore = 0.0; // leere Zugfolge (passen) bewertet den unveränderten Zustand mit 0

        // Ausgangszustand einmal je Entscheidung auswerten (siehe TurnEvaluator.DecisionContext).
        var context = new TurnEvaluator.DecisionContext(state, unit.Team);

        foreach (var (first, _) in Candidates(unit, state, budget.CanMove, budget.CanAct))
        {
            var afterFirst = state.CloneForSimulation();
            if (!Apply(first, unit.Id, afterFirst))
            {
                continue;
            }

            double score = Score(context, afterFirst);

            var actorAfterFirst = afterFirst.Units.FirstOrDefault(u => u.Id == unit.Id);
            if (actorAfterFirst is not null && !actorAfterFirst.IsDefeated)
            {
                bool firstWasMove = first is TurnAction.Move;
                bool canMove = budget.CanMove && !firstWasMove;
                bool canAct = budget.CanAct && firstWasMove;
                foreach (var (second, _) in Candidates(actorAfterFirst, afterFirst, canMove, canAct))
                {
                    var afterSecond = afterFirst.CloneForSimulation();
                    if (Apply(second, unit.Id, afterSecond))
                    {
                        score = Math.Max(score, Score(context, afterSecond));
                    }
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = first;
            }
        }

        return best;
    }

    private double Score(TurnEvaluator.DecisionContext context, GameState after) =>
        context.Evaluate(after).Weighted(_weights);

    /// <summary>
    /// Bewertet eine Zugfolge vollständig (für Tests und Auswertung): führt die
    /// Schritte der Reihe nach auf einer Kopie aus und liefert die Bewertung
    /// oder null, wenn ein Schritt nicht ausführbar ist. Die Schritte beziehen
    /// sich auf Einheiten des übergebenen Zustands.
    /// </summary>
    public static TurnScore? EvaluateSequence(Unit actor, GameState state, params TurnAction[] steps)
    {
        var copy = state.CloneForSimulation();
        foreach (var step in steps)
        {
            if (!Apply(step, actor.Id, copy))
            {
                return null;
            }
        }

        return TurnEvaluator.Evaluate(state, copy, actor.Team);
    }

    /// <summary>
    /// Alle im Budget möglichen ersten Schritte, stabil sortiert nach
    /// <see cref="CandidateKey"/>. Die Schritte referenzieren Einheiten von
    /// <paramref name="state"/> (für die Rückgabe an den MatchController müssen
    /// es die echten Einheiten sein).
    /// </summary>
    internal static IEnumerable<(TurnAction Action, CandidateKey Key)> Candidates(Unit unit, GameState state, bool canMove, bool canAct)
    {
        var list = new List<(TurnAction, CandidateKey)>();

        if (canMove)
        {
            foreach (var target in unit.GetLegalMoves(state))
            {
                list.Add((new TurnAction.Move(target), new CandidateKey(0, target.Q, target.R, 0, 0, Guid.Empty, string.Empty)));
            }
        }

        if (canAct)
        {
            foreach (var target in unit.GetLegalAttacks(state))
            {
                list.Add((new TurnAction.Attack(target), CandidateKey.ForUnit(1, target, unit.Team, string.Empty)));
            }

            if (unit is Hero hero)
            {
                foreach (var option in AbilitySystem.GetLegalAbilityUses(hero, state))
                {
                    string mode = option.Mode?.Id ?? string.Empty;
                    var key = option.Target switch
                    {
                        AbilityTarget.UnitTarget u => CandidateKey.ForUnit(2, u.Unit, unit.Team, mode),
                        AbilityTarget.FieldTarget f => new CandidateKey(2, f.Field.Q, f.Field.R, 0, 0, Guid.Empty, mode),
                        _ => new CandidateKey(2, 0, 0, 0, 0, Guid.Empty, mode),
                    };
                    list.Add((new TurnAction.UseAbility(option.Target, option.Mode), key));
                }
            }
        }

        return list.OrderBy(c => c.Item2);
    }

    /// <summary>
    /// Führt einen Schritt auf <paramref name="simulation"/> aus. Einheiten im
    /// Schritt können aus einem anderen (aber inhaltsgleichen) Zustand stammen
    /// und werden deshalb über ihre Id auf die Einheiten der Simulation abgebildet.
    /// </summary>
    internal static bool Apply(TurnAction action, Guid actorId, GameState simulation)
    {
        var actor = simulation.Units.FirstOrDefault(u => u.Id == actorId);
        if (actor is null || actor.IsDefeated)
        {
            return false;
        }

        Unit? Find(Unit u) => simulation.Units.FirstOrDefault(x => x.Id == u.Id);

        switch (action)
        {
            case TurnAction.Move move:
                return actor.TryMove(move.Target, simulation);

            case TurnAction.Attack attack:
                return Find(attack.Target) is { } target && CombatSystem.TryAttack(actor, target, simulation);

            case TurnAction.UseAbility use when actor is Hero hero:
                AbilityTarget? mapped = use.Target switch
                {
                    AbilityTarget.UnitTarget u => Find(u.Unit) is { } t ? new AbilityTarget.UnitTarget(t) : null,
                    _ => use.Target,
                };
                return mapped is not null && AbilitySystem.TryUseAbility(hero, mapped, simulation, use.Mode);

            default:
                return false;
        }
    }

    /// <summary>
    /// Stabile, etikettenunabhängige Sortierung der Kandidaten (siehe
    /// Klassendokumentation). Relation 0 = eigenes Team, 1 = Gegner; Klon 0 =
    /// Held, 1 = Klon; Id = Id der Einheit bzw. bei Klonen die ihres Erzeugers.
    /// </summary>
    internal readonly record struct CandidateKey(int Kind, int Q, int R, int Relation, int IsCloneRank, Guid Id, string Mode) : IComparable<CandidateKey>
    {
        public static CandidateKey ForUnit(int kind, Unit target, Team actorTeam, string mode) =>
            new(kind, target.Position.Q, target.Position.R, target.Team == actorTeam ? 0 : 1, target.IsClone ? 1 : 0, target.CreatorId ?? target.Id, mode);

        public int CompareTo(CandidateKey other)
        {
            int c = Kind.CompareTo(other.Kind);
            if (c != 0) return c;
            c = Q.CompareTo(other.Q);
            if (c != 0) return c;
            c = R.CompareTo(other.R);
            if (c != 0) return c;
            c = Relation.CompareTo(other.Relation);
            if (c != 0) return c;
            c = IsCloneRank.CompareTo(other.IsCloneRank);
            if (c != 0) return c;
            c = Id.CompareTo(other.Id);
            return c != 0 ? c : string.CompareOrdinal(Mode, other.Mode);
        }
    }
}
