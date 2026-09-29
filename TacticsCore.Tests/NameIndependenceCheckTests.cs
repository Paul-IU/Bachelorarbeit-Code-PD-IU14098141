using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests der optionalen Etiketten-Prüfung (<see cref="NameIndependenceCheck"/>,
/// <see cref="BatchSimulationRunner.Run"/> mit <c>verifyNameIndependence</c>).
/// </summary>
public class NameIndependenceCheckTests
{
    private static readonly HeroCatalog Catalog = DefaultHeroes.CreateDefaultCatalog();

    private static TeamComposition Komposition(params string[] ids) => new(ids.Select(Catalog.Get).ToList());

    /// <summary>
    /// Absichtlich etikettenabhängiger Agent (für beide Seiten dieselbe Klasse):
    /// Einheiten mit dem Etikett Team A entscheiden zufällig, Team B passt
    /// immer. Nach dem Teamtausch handeln also andere physische Einheiten -
    /// genau das muss die Prüfung melden.
    /// </summary>
    private sealed class EtikettenabhaengigerAgent : IAgent
    {
        private readonly RandomAgentByActionType _inner = new();

        public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random) =>
            unit.Team == Team.TeamA ? _inner.DecideAction(unit, state, budget, random) : TurnAction.EndTurn.Instance;
    }

    private static readonly AgentConfiguration EtikettenabhaengigeKonfiguration =
        new(() => new EtikettenabhaengigerAgent(), () => new EtikettenabhaengigerAgent(), "etikettenabhaengig");

    [Theory]
    [InlineData(AgentNames.ByActionType)]
    [InlineData(AgentNames.Uniform)]
    public void MitOption_ErgebnisseSindBitgenauGleichUndDerStandardkatalogHatKeineAbweichung(string agentName)
    {
        // Spiegelpaarungen UND verschiedene Kompositionen, inklusive Hugger
        // (dort trat vor der Sortier-Korrektur die Etikettenabhängigkeit auf).
        var pairings = new[]
        {
            (Komposition("berserker", "bogenschuetze", "hugger"), Komposition("berserker", "bogenschuetze", "hugger")),
            (Komposition("elfe", "hugger", "musiker"), Komposition("elfe", "hugger", "musiker")),
            (Komposition("schildtraeger", "wand", "illusionist"), Komposition("elementarmagier", "hugger", "elfe")),
            (Komposition("berserker", "elfe", "schildtraeger"), Komposition("wand", "bogenschuetze", "musiker")),
        };
        var agents = AgentConfiguration.FromName(agentName);
        var board = new GameBoard();

        var ohne = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 2026, degreeOfParallelism: Environment.ProcessorCount);
        var mit = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 2026, degreeOfParallelism: Environment.ProcessorCount, verifyNameIndependence: true);

        Assert.Null(ohne.NameIndependence); // ohne Option wird nichts geprüft
        var report = Assert.IsType<NameIndependenceReport>(mit.NameIndependence);
        Assert.Equal(pairings.Length * 72, report.CheckedMatches);
        Assert.True(report.Violations.Count == 0, string.Join("\n", report.Violations.Select(v => v.Describe())));

        // Die Prüfpartien fließen nicht in die Ergebnisse ein.
        Assert.Equal(ohne.PairingResults.Count, mit.PairingResults.Count);
        foreach (var (a, b) in ohne.PairingResults.Zip(mit.PairingResults))
        {
            Assert.Equal(a with { HeroActionTotals = b.HeroActionTotals }, b);
            Assert.Equal(a.HeroActionTotals.OrderBy(kv => kv.Key), b.HeroActionTotals.OrderBy(kv => kv.Key));
        }
    }

    [Fact]
    public void EtikettenabhaengigerAgent_JedeAbweichungWirdEinzelnMitErstemAbweichendenZugGemeldet()
    {
        var komposition = Komposition("berserker", "bogenschuetze", "elfe");
        var result = BatchSimulationRunner.Run(
            new GameBoard(), new[] { (komposition, komposition) }, EtikettenabhaengigeKonfiguration,
            baseSeed: 1, maxSetupsPerPairing: 6, verifyNameIndependence: true);

        var report = result.NameIndependence!;
        Assert.Equal(6, report.CheckedMatches);
        Assert.NotEmpty(report.Violations);
        Assert.Equal(report.Violations.Select(v => v.SetupIndex).OrderBy(i => i), report.Violations.Select(v => v.SetupIndex)); // feste Reihenfolge

        foreach (var violation in report.Violations)
        {
            Assert.Equal(komposition, violation.TeamA);
            var divergence = Assert.IsType<ReplayDivergence>(violation.FirstDivergence);
            Assert.NotEqual(divergence.Original, divergence.Swapped);

            string text = violation.Describe();
            Assert.Contains($"Ausgangslage {violation.SetupIndex}", text);
            Assert.Contains("Erster abweichender Eintrag", text);
            Assert.Contains("Abweichung im Ergebnis", text);
        }
    }

    [Fact]
    public void Abweichungen_SindBeiEinemUndMehrerenThreadsIdentisch()
    {
        var komposition = Komposition("hugger", "musiker", "wand");
        string Lauf(int threads) => string.Join("\n", BatchSimulationRunner.Run(
                new GameBoard(), new[] { (komposition, komposition) }, EtikettenabhaengigeKonfiguration,
                baseSeed: 7, maxSetupsPerPairing: 12, degreeOfParallelism: threads, verifyNameIndependence: true)
            .NameIndependence!.Violations.Select(v => v.Describe()));

        string einThread = Lauf(1);
        Assert.NotEmpty(einThread);
        Assert.Equal(einThread, Lauf(Environment.ProcessorCount));
    }

    [Fact]
    public void SwapId_UebersetztHeldenUndKlonIdsAufDieselbePhysischeEinheit()
    {
        static Guid Id(int n) => new(n, 0, 0, new byte[8]);
        Assert.Equal(Id(101), NameIndependenceCheck.SwapId(Id(1)));
        Assert.Equal(Id(3), NameIndependenceCheck.SwapId(Id(103)));
        Assert.Equal(Guid.Empty, NameIndependenceCheck.SwapId(Guid.Empty));

        var heldA = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        var heldB = new Hero(Id(102), Team.TeamB, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        Assert.Equal(Unit.CreateClone(heldB).Id, NameIndependenceCheck.SwapId(Unit.CreateClone(heldA).Id));
    }

    [Fact]
    public void CompareOutcomes_SpiegelbildlichesErgebnisIstKeineAbweichung_UnterschiedlicheRundenzahlSchon()
    {
        var komposition = Komposition("berserker", "bogenschuetze", "elfe");
        var board = new GameBoard();
        var setup = AusgangslagenGenerator.GenerateAllSetups(komposition, komposition, board)[0];

        MatchResult Spiele(GameSetup s) =>
            new MatchController(AusgangslagenGenerator.BuildGameState(s, board), new RandomAgentByActionType(), new RandomAgentByActionType(), seed: 3).Run(s.StartingTeam);

        var original = Spiele(setup);
        var getauscht = Spiele(setup.WithSwappedTeams());

        Assert.Null(NameIndependenceCheck.CompareOutcomes(original, getauscht));
        Assert.Contains("Rundenzahl", NameIndependenceCheck.CompareOutcomes(original, getauscht with { RoundsPlayed = getauscht.RoundsPlayed + 1 }));
    }
}
