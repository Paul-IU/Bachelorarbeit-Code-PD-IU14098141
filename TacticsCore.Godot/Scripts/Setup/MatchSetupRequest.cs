using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCoreGodot.Setup;

/// <summary>
/// Wer ein Team steuert: <see cref="Human"/> oder eines der Kennworte aus
/// <see cref="AgentNames"/> - bewusst dieselben Bezeichnungen wie im
/// Batch-Lauf, damit sich Beobachtungen in Godot den Batch-Ergebnissen
/// zuordnen lassen. Die Agenten selbst entstehen über
/// <see cref="AgentConfiguration.FromName"/>, also exakt dieselben Klassen wie im Batch.
/// </summary>
public static class TeamControllers
{
    public const string Human = "mensch";

    /// <summary>Alle wählbaren Steuerungen in Anzeigereihenfolge.</summary>
    public static IReadOnlyList<string> All { get; } = new[] { Human }.Concat(AgentNames.All).ToList();

    /// <summary>Anzeigetext: das Kennwort plus kurze Erläuterung.</summary>
    public static string Describe(string controller) => controller switch
    {
        Human => "mensch (Eingabe über die Oberfläche)",
        AgentNames.ByActionType => $"{AgentNames.ByActionType} (Zufall, erst Aktionstyp)",
        AgentNames.Uniform => $"{AgentNames.Uniform} (Zufall, gleichverteilt über alle Züge)",
        AgentNames.Aggressive => $"{AgentNames.Aggressive} (heuristisch, offensiv 2:1)",
        AgentNames.Defensive => $"{AgentNames.Defensive} (heuristisch, defensiv 1:2)",
        AgentNames.Lookahead => $"{AgentNames.Lookahead} (heuristisch, neutral 1:1, zwei Suchebenen)",
        AgentNames.ThresholdAggressive => $"{AgentNames.ThresholdAggressive} (Schwellen-Agent, Schaden : Lebenspunkte 2:1)",
        AgentNames.ThresholdDefensive => $"{AgentNames.ThresholdDefensive} (Schwellen-Agent, Schaden : Lebenspunkte 1:2)",
        _ => controller,
    };
}

/// <summary>Die Aufstellung EINES Teams: welcher Held auf welchem Startfeld steht (in der Reihenfolge der Startfelder) und wer das Team steuert (siehe <see cref="TeamControllers"/>).</summary>
public sealed record TeamSetupChoice(IReadOnlyList<(HeroDefinition Hero, HexCoordinate Field)> Assignments, string Controller)
{
    public bool IsHuman => Controller == TeamControllers.Human;
}

/// <summary>
/// Vollständige, vom SetupScreen zusammengestellte Konfiguration für eine neue, live gespielte Partie (Vorgabe Punkt 6).
/// </summary>
/// <param name="StartingTeam">Beginnendes Team; null = wie bisher über den Seed gezogen. Zum Nachspielen einer Batch-Partie fest vorgeben (dort ist es Teil der Ausgangslage).</param>
public sealed record MatchSetupRequest(TeamSetupChoice TeamA, TeamSetupChoice TeamB, int Seed, Team? StartingTeam);
