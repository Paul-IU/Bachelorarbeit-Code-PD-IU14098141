using TacticsCore.Agents;

namespace TacticsCore.Batch;

/// <summary>
/// Legt fest, WIE die beiden Seiten einer Partie entscheiden - als FABRIK
/// (<see cref="Func{TResult}"/>) statt als fertige <see cref="IAgent"/>-
/// Instanz, damit im parallelen Batch-Lauf (Anforderung 11) JEDE Partie
/// ihre EIGENE Agenten-Instanz bekommt. Das ist bei
/// <see cref="RandomAgentByActionType"/>/<see cref="RandomAgentUniform"/>
/// zwar unnötig (beide tragen keinen eigenen Zustand), macht die
/// Thread-Sicherheit aber unabhängig von dieser Eigenschaft eines konkreten
/// Agenten - ein künftiger, zustandsbehafteter Agent (z.B. mit eigenem
/// Cache) wäre sonst eine tückische, unter Last selten auftretende
/// Race-Condition-Quelle.
/// </summary>
/// <param name="CreateTeamAAgent">Fabrik für Team A.</param>
/// <param name="CreateTeamBAgent">Fabrik für Team B.</param>
/// <param name="Name">
/// Sprechender, stabiler Bezeichner DIESER Konfiguration (Bachelorarbeit-
/// Erweiterung: "der verwendete Agent soll in den Ergebnisdateien vermerkt
/// werden") - siehe <see cref="AgentNames"/> für die gültigen Werte. Reine
/// Metadaten, beeinflusst kein Simulationsverhalten.
/// </param>
public sealed record AgentConfiguration(Func<IAgent> CreateTeamAAgent, Func<IAgent> CreateTeamBAgent, string Name)
{
    /// <summary>Beide Seiten nutzen je eine frische <see cref="RandomAgentByActionType"/>-Instanz - der Standardfall für die meisten Simulationsläufe (siehe auch <see cref="UniformVsUniform"/> für die Vergleichsvariante).</summary>
    public static AgentConfiguration RandomVsRandom() => new(() => new RandomAgentByActionType(), () => new RandomAgentByActionType(), AgentNames.ByActionType);

    /// <summary>Beide Seiten nutzen je eine frische <see cref="RandomAgentUniform"/>-Instanz - Vergleichslauf zur (nicht gleichverteilten) <see cref="RandomVsRandom"/>-Auswahlregel.</summary>
    public static AgentConfiguration UniformVsUniform() => new(() => new RandomAgentUniform(), () => new RandomAgentUniform(), AgentNames.Uniform);

    /// <summary>Beide Seiten nutzen den heuristischen Agenten mit aggressiver Gewichtung (offensiv : defensiv = 2 : 1, siehe <see cref="HeuristicAgentSettings"/>).</summary>
    public static AgentConfiguration AggressiveVsAggressive() =>
        new(() => new HeuristicAgent(HeuristicWeights.AggressiveProfile), () => new HeuristicAgent(HeuristicWeights.AggressiveProfile), AgentNames.Aggressive);

    /// <summary>Beide Seiten nutzen den heuristischen Agenten mit defensiver Gewichtung (offensiv : defensiv = 1 : 2, siehe <see cref="HeuristicAgentSettings"/>).</summary>
    public static AgentConfiguration DefensiveVsDefensive() =>
        new(() => new HeuristicAgent(HeuristicWeights.DefensiveProfile), () => new HeuristicAgent(HeuristicWeights.DefensiveProfile), AgentNames.Defensive);

    /// <summary>Beide Seiten nutzen den vorausschauenden Agenten (neutrale Gewichtung 1:1, zwei Suchebenen, siehe <see cref="LookaheadHeuristicAgent"/>).</summary>
    public static AgentConfiguration LookaheadVsLookahead() =>
        new(() => new LookaheadHeuristicAgent(), () => new LookaheadHeuristicAgent(), AgentNames.Lookahead);

    /// <summary>Beide Seiten nutzen den Schwellen-Agenten mit aggressiver Gewichtung (w_D : w_H = 2 : 1, siehe <see cref="ThresholdHeuristicSettings"/>).</summary>
    public static AgentConfiguration ThresholdAggressiveVsThresholdAggressive() =>
        new(() => new ThresholdHeuristicAgent(ThresholdWeights.AggressiveProfile), () => new ThresholdHeuristicAgent(ThresholdWeights.AggressiveProfile), AgentNames.ThresholdAggressive);

    /// <summary>Beide Seiten nutzen den Schwellen-Agenten mit defensiver Gewichtung (w_D : w_H = 1 : 2, siehe <see cref="ThresholdHeuristicSettings"/>).</summary>
    public static AgentConfiguration ThresholdDefensiveVsThresholdDefensive() =>
        new(() => new ThresholdHeuristicAgent(ThresholdWeights.DefensiveProfile), () => new ThresholdHeuristicAgent(ThresholdWeights.DefensiveProfile), AgentNames.ThresholdDefensive);

    /// <summary>
    /// Wählt anhand eines sprechenden Kennworts (case-insensitiv, siehe
    /// <see cref="AgentNames"/>) - für die CLI-/Konfigurationsdatei-
    /// gesteuerte Agentenauswahl (Bachelorarbeit-Erweiterung: "ohne dass
    /// Code geändert werden muss").
    /// </summary>
    public static AgentConfiguration FromName(string name) => name.Trim().ToLowerInvariant() switch
    {
        AgentNames.ByActionType => RandomVsRandom(),
        AgentNames.Uniform => UniformVsUniform(),
        AgentNames.Aggressive => AggressiveVsAggressive(),
        AgentNames.Defensive => DefensiveVsDefensive(),
        AgentNames.Lookahead => LookaheadVsLookahead(),
        AgentNames.ThresholdAggressive when AgentNames.ThresholdAgentsEnabled => ThresholdAggressiveVsThresholdAggressive(),
        AgentNames.ThresholdDefensive when AgentNames.ThresholdAgentsEnabled => ThresholdDefensiveVsThresholdDefensive(),
        _ => throw new ArgumentException($"Unbekannter Agentenname '{name}' - gültig sind {string.Join(", ", AgentNames.All.Select(n => $"'{n}'"))}.", nameof(name)),
    };
}

/// <summary>
/// Zentrale, benannte Kennworte für die Agentenauswahl (siehe
/// <see cref="AgentConfiguration.FromName"/>) - EIN Ort statt an mehreren
/// Stellen wiederholter String-Literale (CLI-Hilfetext, CSV-Spalte,
/// Vergleichs-Switch).
/// </summary>
public static class AgentNames
{
    public const string ByActionType = "byactiontype";
    public const string Uniform = "uniform";
    public const string Aggressive = "aggressive";
    public const string Defensive = "defensive";

    /// <summary>Vorausschauender Agent (<see cref="LookaheadHeuristicAgent"/>): neutrale Gewichtung, zwei Suchebenen.</summary>
    public const string Lookahead = "lookahead";

    /// <summary>Schwellen-Agent (zweiter heuristischer Ansatz, <see cref="ThresholdHeuristicAgent"/>), aggressiv.</summary>
    public const string ThresholdAggressive = "threshold-aggressive";

    /// <summary>Schwellen-Agent, defensiv.</summary>
    public const string ThresholdDefensive = "threshold-defensive";

    /// <summary>
    /// SCHWELLEN-AGENTEN VORERST DEAKTIVIERT (Bachelorarbeit): die
    /// Vergleichsmessung ergab, dass der bisherige <see cref="HeuristicAgent"/>
    /// besser spielt. Code, Klassen und Tests des <see cref="ThresholdHeuristicAgent"/>
    /// bleiben vollständig erhalten (Vergleich in der Arbeit); die Fabrikmethoden
    /// <see cref="AgentConfiguration.ThresholdAggressiveVsThresholdAggressive"/> und
    /// <see cref="AgentConfiguration.ThresholdDefensiveVsThresholdDefensive"/> sind
    /// weiterhin direkt aufrufbar. Deaktiviert ist nur die AUSWAHL: die Kennworte
    /// fehlen in <see cref="All"/> (Godot-Oberfläche, CLI-Hilfe) und
    /// <see cref="AgentConfiguration.FromName"/> lehnt sie ab.
    ///
    /// REAKTIVIEREN: diesen Schalter auf true setzen - sonst nichts. Dann stehen
    /// beide Ausrichtungen wieder in Batch-Konfiguration und Godot zur Wahl.
    /// </summary>
    public const bool ThresholdAgentsEnabled = false;

    /// <summary>Alle aktuell wählbaren Kennworte, in der Reihenfolge der Dokumentation (siehe <see cref="ThresholdAgentsEnabled"/>).</summary>
    public static IReadOnlyList<string> All { get; } = ThresholdAgentsEnabled
        ? new[] { ByActionType, Uniform, Aggressive, Defensive, Lookahead, ThresholdAggressive, ThresholdDefensive }
        : new[] { ByActionType, Uniform, Aggressive, Defensive, Lookahead };
}
