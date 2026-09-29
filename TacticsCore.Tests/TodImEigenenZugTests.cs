using TacticsCore.Abilities;
using TacticsCore.Heroes;
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
/// Eine Einheit, die in ihrem EIGENEN Zug besiegt wird (Vergeltung der
/// Mauer-Umleitung auf einen Angriff oder Fähigkeitsschaden), darf danach
/// nichts mehr tun: der Zug endet sofort, ihr Agent wird nicht erneut gefragt.
/// Vor der Korrektur wurde der Agent weiter nach der Bewegung gefragt - die
/// Bewegung selbst scheiterte (TryMove lehnt besiegte Einheiten ab), der
/// Zufallsagent verbrauchte aber Zufallswerte und verschob damit den Rest der Partie.
/// </summary>
public class TodImEigenenZugTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    /// <summary>Wand (Team B) mit aktiver Mauer, ein Verbündeter auf ihrem Feld; Team A hat eine zweite, weit entfernte Einheit.</summary>
    private static (GameState State, Hero Wand, TestUnit Verbuendeter) Aufbau()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        var verbuendeter = new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(wand);
        state.AddUnit(verbuendeter);
        state.AddUnit(new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 5), speed: 10)); // Team A bleibt am Leben
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        return (state, wand, verbuendeter);
    }

    [Fact]
    public void TodDurchVergeltungNachAngriff_BeendetDenZugSofort()
    {
        var (state, _, verbuendeter) = Aufbau();
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        angreifer.ApplyDamage(angreifer.MaxHitPoints - angreifer.BaseAttackDamage / 2); // Vergeltung (50 % von 100) ist tödlich

        var agent = new ScriptedAgent();
        agent.Script(angreifer, new TurnAction.Attack(verbuendeter), new TurnAction.Move(new HexCoordinate(0, 3)));
        var replay = new List<ReplayEntry>();

        var result = new MatchController(state, agent, new ScriptedAgent(), seed: 1)
            .ExecuteTurn(angreifer, agent, new Random(1), replay, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.True(angreifer.IsDefeated);
        Assert.Null(result); // beide Teams leben noch
        Assert.Equal(1, agent.CallCount(angreifer)); // nach dem Tod nicht erneut gefragt
        Assert.DoesNotContain(replay, e => e.ActionType == ReplayActionType.Move);
        Assert.Equal(ReplayActionType.TurnEnded, replay[^1].ActionType);
    }

    [Fact]
    public void TodDurchVergeltungNachFaehigkeit_BeendetDenZugSofort()
    {
        var (state, _, verbuendeter) = Aufbau();
        var magier = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Elementarmagier);
        state.AddUnit(magier);
        var erde = magier.Definition.Ability.Modes!.Single(m => m.Id == "erde"); // Sofortschaden 70 -> Vergeltung 35
        magier.ApplyDamage(magier.MaxHitPoints - 35);

        var agent = new ScriptedAgent();
        agent.Script(magier, new TurnAction.UseAbility(new AbilityTarget.UnitTarget(verbuendeter), erde), new TurnAction.Move(new HexCoordinate(0, 3)));

        var result = new MatchController(state, agent, new ScriptedAgent(), seed: 1)
            .ExecuteTurn(magier, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.True(magier.IsDefeated);
        Assert.Null(result);
        Assert.Equal(1, agent.CallCount(magier));
    }

    [Fact]
    public void UeberlebtDieEinheitDieVergeltung_DarfSieSichWeiterhinBewegen()
    {
        var (state, _, verbuendeter) = Aufbau();
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 2), speed: 10); // volle HP
        state.AddUnit(angreifer);

        var agent = new ScriptedAgent();
        agent.Script(angreifer, new TurnAction.Attack(verbuendeter), new TurnAction.Move(new HexCoordinate(0, 3)));

        new MatchController(state, agent, new ScriptedAgent(), seed: 1)
            .ExecuteTurn(angreifer, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.False(angreifer.IsDefeated);
        Assert.Equal(new HexCoordinate(0, 3), angreifer.Position);
        Assert.Equal(2, agent.CallCount(angreifer));
    }
}
