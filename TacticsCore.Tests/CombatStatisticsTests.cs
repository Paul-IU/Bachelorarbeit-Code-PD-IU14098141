using TacticsCore.Abilities;
using TacticsCore.Combat;
using TacticsCore.Effects;
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
/// Kampfbilanz je Einheit (<see cref="GameState.CombatStatistics"/>) für die
/// Abschlussansicht in Godot - Zählweise siehe <see cref="UnitCombatStatistics"/>.
/// </summary>
public class CombatStatisticsTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void Angriff_ZaehltNurTatsaechlichVerloreneLebenspunkteUndDenBesiegten()
    {
        var state = new GameState(new GameBoard());
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        state.AddUnit(gegner);

        Assert.True(CombatSystem.TryAttack(angreifer, gegner, state)); // 100 Schaden
        gegner.ApplyDamage(gegner.CurrentHitPoints - 30);               // außerhalb des Kampfs, zählt nicht
        Assert.True(CombatSystem.TryAttack(angreifer, gegner, state)); // nur noch 30 HP übrig

        Assert.Equal(new UnitCombatStatistics(DamageDealt: 130, DamageTaken: 0, Defeats: 1), state.CombatStatistics[angreifer.Id]);
        Assert.Equal(130, state.CombatStatistics[gegner.Id].DamageTaken);
    }

    [Fact]
    public void Vergeltung_WirdDerWandGutgeschrieben_SchadenUeberZeitDemAnwender()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        foreach (var u in new Unit[] { wand, verbuendeter, gegner }) state.AddUnit(u);
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));

        Assert.True(CombatSystem.TryAttack(gegner, verbuendeter, state)); // umgeleitet: Wand nimmt 75, Vergeltung 50

        Assert.Equal(75, state.CombatStatistics[gegner.Id].DamageDealt);
        Assert.Equal(75, state.CombatStatistics[wand.Id].DamageTaken);
        Assert.Equal(50, state.CombatStatistics[wand.Id].DamageDealt);
        Assert.Equal(50, state.CombatStatistics[gegner.Id].DamageTaken);
        Assert.False(state.CombatStatistics.ContainsKey(verbuendeter.Id));

        EffectSystem.Apply(state, ActiveEffect.OnUnit(wand, gegner, new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 2, DamagePerTick: 40)));
        EffectSystem.TickDamageOverTime(state, gegner);
        Assert.Equal(50 + 40, state.CombatStatistics[wand.Id].DamageDealt);
    }

    [Fact]
    public void Simulationskopie_BeginntMitLeererBilanz()
    {
        var state = new GameState(new GameBoard());
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        state.AddUnit(gegner);
        Assert.True(CombatSystem.TryAttack(angreifer, gegner, state));

        Assert.Empty(state.CloneForSimulation().CombatStatistics);
    }
}
