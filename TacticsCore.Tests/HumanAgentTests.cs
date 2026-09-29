using TacticsCore.Agents;
using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Fake-Eingabequelle für Tests: wählt deterministisch den Zug an einem
/// festen Index (oder den letzten, falls der Index außerhalb liegt) - kein
/// echtes Konsolen-/UI-Verhalten, nur zum Prüfen, dass
/// <see cref="HumanAgent"/> die Auswahl korrekt an die Eingabequelle
/// delegiert.
/// </summary>
public sealed class FakeHumanInputSource : IHumanInputSource
{
    private readonly int _index;
    public IReadOnlyList<TurnAction>? LastOfferedActions { get; private set; }

    public FakeHumanInputSource(int index) => _index = index;

    public TurnAction ChooseAction(Unit unit, GameState state, IReadOnlyList<TurnAction> legalActions)
    {
        LastOfferedActions = legalActions;
        return legalActions[Math.Min(_index, legalActions.Count - 1)];
    }
}

public class HumanAgentTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void HumanAgent_GibtEinenZugAusDerListeDerLegalenZuegeZurueck()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(unit);
        state.AddUnit(enemy);

        var input = new FakeHumanInputSource(index: 0);
        var agent = new HumanAgent(input);

        var action = agent.DecideAction(unit, state, new TurnBudget(CanMove: true, CanAct: true), new Random(1));

        Assert.NotNull(input.LastOfferedActions);
        Assert.Contains(action, input.LastOfferedActions!);
        Assert.Contains(input.LastOfferedActions!, a => a is TurnAction.EndTurn); // Passen ist immer eine Option
    }

    [Fact]
    public void HumanAgent_BietetOhneVerbleibendesBudgetNurNochPassenAn()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(unit);

        var input = new FakeHumanInputSource(index: 0);
        var agent = new HumanAgent(input);

        var action = agent.DecideAction(unit, state, new TurnBudget(CanMove: false, CanAct: false), new Random(1));

        Assert.Single(input.LastOfferedActions!);
        Assert.IsType<TurnAction.EndTurn>(action);
    }

    [Fact]
    public void HumanAgent_KannEineVollstaendigePartieGegenEinenZufallsagentenSpielen()
    {
        // Wählt an jeder Stelle den ERSTEN legalen Zug - keine "Intelligenz",
        // nur ein Nachweis, dass HumanAgent sich nahtlos in den
        // MatchController einfügt wie jeder andere IAgent auch.
        var state = NewState();
        foreach (var field in state.Board.GetStartFields(FieldType.BlossomA))
        {
            state.AddUnit(new Unit(Guid.NewGuid(), Team.TeamA, field.Coordinate));
        }
        foreach (var field in state.Board.GetStartFields(FieldType.BlossomB))
        {
            state.AddUnit(new Unit(Guid.NewGuid(), Team.TeamB, field.Coordinate));
        }

        var humanAgent = new HumanAgent(new FakeHumanInputSource(index: 0));
        var controller = new MatchController(state, humanAgent, new TacticsCore.Agents.RandomAgentByActionType(), seed: 1);

        var result = controller.Run();

        Assert.True(result.RoundsPlayed > 0);
    }
}
