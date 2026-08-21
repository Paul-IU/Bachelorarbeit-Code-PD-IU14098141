using TacticsCore.Heroes;
using TacticsCore.Hex;

namespace TacticsCoreGodot.Setup;

/// <summary>Die Aufstellung EINES Teams: welcher Held auf welchem Startfeld steht, und ob ein Mensch oder der Zufallsagent dieses Team steuert.</summary>
public sealed record TeamSetupChoice(IReadOnlyList<(HeroDefinition Hero, HexCoordinate Field)> Assignments, bool IsHuman);

/// <summary>Vollständige, vom SetupScreen zusammengestellte Konfiguration für eine neue, live gespielte Partie (Vorgabe Punkt 6).</summary>
public sealed record MatchSetupRequest(TeamSetupChoice TeamA, TeamSetupChoice TeamB, int Seed);
