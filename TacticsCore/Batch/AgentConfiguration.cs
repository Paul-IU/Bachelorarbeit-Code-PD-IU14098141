using TacticsCore.Agents;

namespace TacticsCore.Batch;

/// <summary>
/// Legt fest, WIE die beiden Seiten einer Partie entscheiden - als FABRIK
/// (<see cref="Func{TResult}"/>) statt als fertige <see cref="IAgent"/>-
/// Instanz, damit im parallelen Batch-Lauf (Anforderung 11) JEDE Partie
/// ihre EIGENE Agenten-Instanz bekommt. Das ist bei <see cref="RandomAgent"/>
/// zwar unnötig (er trägt keinen eigenen Zustand), macht die
/// Thread-Sicherheit aber unabhängig von dieser Eigenschaft eines konkreten
/// Agenten - ein künftiger, zustandsbehafteter Agent (z.B. mit eigenem
/// Cache) wäre sonst eine tückische, unter Last selten auftretende
/// Race-Condition-Quelle.
/// </summary>
public sealed record AgentConfiguration(Func<IAgent> CreateTeamAAgent, Func<IAgent> CreateTeamBAgent)
{
    /// <summary>Beide Seiten nutzen je eine frische <see cref="RandomAgent"/>-Instanz - der Standardfall für die meisten Simulationsläufe.</summary>
    public static AgentConfiguration RandomVsRandom() => new(() => new RandomAgent(), () => new RandomAgent());
}
