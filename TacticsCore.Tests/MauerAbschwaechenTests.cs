using TacticsCore.Abilities;
using TacticsCore.Agents;
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
/// Designentscheidung "Mauer abschwächen" (siehe <see cref="DefaultHeroes.Wand"/>):
/// Selbstreduktion 25 % statt 50 %, Abklingzeit von einer Runde. Der Schildwall
/// bleibt unverändert.
/// </summary>
public class MauerAbschwaechenTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    private static (GameState State, Hero Wand) WandMitMauer()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        state.AddUnit(wand);
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        return (state, wand);
    }

    // --- Reduktionswert -------------------------------------------------

    [Fact]
    public void Mauer_ReduziertDenSchadenAnDerWandUm25Prozent()
    {
        var (state, wand) = WandMitMauer();

        Assert.Equal(0.75, DefaultHeroes.WandMauerSchadensmultiplikator);
        Assert.Equal(0.75, EffectiveStats.IncomingDamageMultiplier(wand, state));

        CombatSystem.ApplyDamageAndHandleDefeat(state, wand, 100);
        Assert.Equal(wand.MaxHitPoints - 75, wand.CurrentHitPoints);
    }

    /// <summary>
    /// 0,75 erzeugt bei ungeraden Vielfachen von 10 echte Halbwerte, die das
    /// CombatSystem unverändert per Bankers Rounding auflöst (auf die gerade Zahl).
    /// </summary>
    [Theory]
    [InlineData(70, 52)]  // 52,5 -> 52
    [InlineData(90, 68)]  // 67,5 -> 68
    [InlineData(120, 90)] // exakt
    public void Mauer_HalbwerteWerdenPerBankersRoundingAufgeloest(int schaden, int erwartet)
    {
        var (state, wand) = WandMitMauer();

        CombatSystem.ApplyDamageAndHandleDefeat(state, wand, schaden);

        Assert.Equal(wand.MaxHitPoints - erwartet, wand.CurrentHitPoints);
    }

    [Fact]
    public void Schildwall_BleibtBei50Prozent()
    {
        var state = new GameState(new GameBoard());
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
        state.AddUnit(tank);
        Assert.True(AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state));

        Assert.Equal(0.5, EffectiveStats.IncomingDamageMultiplier(tank, state));
        Assert.Equal(2, DefaultHeroes.Schildtraeger.Ability.CooldownRounds); // unverändert
    }

    // --- Abklingzeit ----------------------------------------------------

    [Fact]
    public void Mauer_IstImNaechstenEigenenZugGesperrtUndImUebernaechstenWiederVerfuegbar()
    {
        var (state, wand) = WandMitMauer();
        Assert.Equal(DefaultHeroes.WandMauerAbklingzeit, wand.AbilityCooldownRemaining);

        wand.TickAbilityCooldown(); // Beginn des nächsten eigenen Zugs
        Assert.Empty(AbilitySystem.GetLegalAbilityUses(wand, state));
        Assert.False(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));

        wand.TickAbilityCooldown(); // Beginn des übernächsten eigenen Zugs
        Assert.NotEmpty(AbilitySystem.GetLegalAbilityUses(wand, state));
    }

    /// <summary>
    /// Durch echte Züge: eine Wand, die die Mauer bei jeder Gelegenheit wirkt,
    /// kommt nur in jeder zweiten Runde dazu - dazwischen liegt jeweils eine
    /// Runde, in der keine Mauer aktiv ist (vorher: Einsatz in jeder Runde).
    /// </summary>
    [Fact]
    public void WandMitDauereinsatz_WirktDieMauerNurJedeZweiteRunde_DazwischenEineUngeschuetzteRunde()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Wand);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10);
        state.AddUnit(wand);
        state.AddUnit(gegner);

        var result = new MatchController(state, new ImmerMauerAgent(), new ScriptedAgent(), seed: 1, recordReplay: true).Run(Team.TeamA);

        var einsaetze = result.Replay!.Entries
            .Where(e => e.ActionType == ReplayActionType.UseAbility && e.RoundNumber <= 6)
            .Select(e => e.RoundNumber)
            .ToList();
        Assert.Equal(new[] { 1, 3, 5 }, einsaetze);

        // Runde 2: die Mauer aus Runde 1 läuft zu Beginn des Wand-Zugs ab, eine neue entsteht nicht.
        var runde2 = result.Replay.Entries.Where(e => e.RoundNumber == 2).ToList();
        Assert.Contains(runde2, e => e.ActionType == ReplayActionType.EffectExpired && e.EffectName == "Mauer");
        Assert.DoesNotContain(runde2, e => e.ActionType == ReplayActionType.EffectApplied && e.EffectName == "Mauer");
    }

    /// <summary>Wirkt die Fähigkeit, sobald sie legal ist; sonst Zugende.</summary>
    private sealed class ImmerMauerAgent : IAgent
    {
        public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
        {
            if (budget.CanAct && unit is Hero hero && AbilitySystem.GetLegalAbilityUses(hero, state).FirstOrDefault() is { } option)
            {
                return new TurnAction.UseAbility(option.Target, option.Mode);
            }

            return TurnAction.EndTurn.Instance;
        }
    }
}
