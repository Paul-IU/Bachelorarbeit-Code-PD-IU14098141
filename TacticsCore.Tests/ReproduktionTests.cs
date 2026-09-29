using TacticsCore.Batch;
using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Belegt, dass eine einzeln nachgespielte Partie (Godot-Oberfläche,
/// Konsolenmodi "play-one"/"batch-match") exakt denselben Verlauf nimmt wie
/// dieselbe Partie im Batch-Lauf. Der Aufbau hier ist Schritt für Schritt der
/// der Godot-Vorbereitung (MatchScreen.StartMatch): Helden in der Reihenfolge
/// der nach (q, r) sortierten Startfelder, Aufbau über
/// <see cref="AusgangslagenGenerator.BuildGameState"/>, Agenten über
/// <see cref="AgentConfiguration.FromName"/>, fest vorgegebenes Startteam, Seed.
/// </summary>
public class ReproduktionTests
{
    private static readonly HeroCatalog Catalog = DefaultHeroes.CreateDefaultCatalog();

    private static MatchResult SpieleWieGodot(IReadOnlyList<HeroDefinition> heldenA, IReadOnlyList<HeroDefinition> heldenB, Team startteam, int seed, string agentA, string agentB)
    {
        var board = new GameBoard();
        var felderA = board.GetStartFields(FieldType.BlossomA).OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R).Select(f => f.Coordinate).ToList();
        var felderB = board.GetStartFields(FieldType.BlossomB).OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R).Select(f => f.Coordinate).ToList();
        var setup = new GameSetup(heldenA.Zip(felderA).ToList(), heldenB.Zip(felderB).ToList(), startteam);

        var state = AusgangslagenGenerator.BuildGameState(setup, board);
        return new MatchController(state,
            AgentConfiguration.FromName(agentA).CreateTeamAAgent(),
            AgentConfiguration.FromName(agentB).CreateTeamBAgent(),
            seed, recordReplay: true).Run(startteam);
    }

    [Theory]
    [InlineData(AgentNames.ByActionType)]
    [InlineData(AgentNames.Uniform)]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void EinzelnNachgespieltePartien_EntsprechenDemBatchLauf(string agent)
    {
        var teamA = new TeamComposition(new[] { Catalog.Get("berserker"), Catalog.Get("hugger"), Catalog.Get("wand") });
        var teamB = new TeamComposition(new[] { Catalog.Get("elfe"), Catalog.Get("illusionist"), Catalog.Get("musiker") });
        const int baseSeed = 2026, setups = 4;

        var batch = BatchSimulationRunner.Run(new GameBoard(), new[] { (teamA, teamB) }, AgentConfiguration.FromName(agent),
            baseSeed, maxSetupsPerPairing: setups).PairingResults.Single();

        var alleSetups = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, new GameBoard());
        var einzeln = Enumerable.Range(0, setups).Select(i =>
        {
            var s = alleSetups[i];
            return SpieleWieGodot(s.TeamAUnits.Select(u => u.Hero).ToList(), s.TeamBUnits.Select(u => u.Hero).ToList(),
                s.StartingTeam, BatchSimulationRunner.DeriveSeed(baseSeed, pairingIndex: 0, setupIndex: i), agent, agent);
        }).ToList();

        Assert.Equal(batch.TeamAWins, einzeln.Count(r => r.Winner == MatchWinner.TeamA));
        Assert.Equal(batch.TeamBWins, einzeln.Count(r => r.Winner == MatchWinner.TeamB));
        Assert.Equal(batch.AverageRoundsPlayed, einzeln.Average(r => r.RoundsPlayed), precision: 12);
        Assert.Equal(batch.AverageTeamARemainingHitPointsFraction, einzeln.Average(r => r.TeamAResult.RemainingHitPointsFraction), precision: 12);
        Assert.Equal(batch.AverageTeamBRemainingHitPointsFraction, einzeln.Average(r => r.TeamBResult.RemainingHitPointsFraction), precision: 12);
    }

    [Fact]
    public void Fingerabdruck_IstBeiGleicherPartieGleichUndBeiAnderemSeedVerschieden()
    {
        var a = new[] { Catalog.Get("berserker"), Catalog.Get("elfe"), Catalog.Get("wand") };
        var b = new[] { Catalog.Get("bogenschuetze"), Catalog.Get("hugger"), Catalog.Get("musiker") };

        string Lauf(int seed) => ReplayFingerprint.Compute(
            SpieleWieGodot(a, b, Team.TeamB, seed, AgentNames.Uniform, AgentNames.ByActionType).Replay!.Entries);

        Assert.Equal(Lauf(42), Lauf(42));
        Assert.NotEqual(Lauf(42), Lauf(43));
        Assert.Matches("^[0-9a-f]{16}$", Lauf(42));
    }
}
