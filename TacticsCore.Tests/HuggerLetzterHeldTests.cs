using TacticsCore.Abilities;
using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Behebung der Regellücke "angehefteter letzter Held" (siehe
/// <see cref="EffectSystem.ReleaseAttachmentsOfLastHeroes"/>): wird ein
/// angehefteter Hugger zum letzten lebenden Helden seines Teams, löst er sich
/// sofort und ist wieder angreifbar.
/// </summary>
public class HuggerLetzterHeldTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly AbilityMode Greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");

    private static bool IstAngeheftet(Unit hugger, GameState state) =>
        state.ActiveEffects.Any(e => e.Name == "Umklammerung" && e.SourceUnitId == hugger.Id);

    /// <summary>Team A: Hugger + Verbündeter; Team B: zwei Gegner, der erste vom Hugger umklammert.</summary>
    private static (GameState State, Hero Hugger, Unit Verbuendeter, Unit Umklammerter) Aufbau()
    {
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var umklammerter = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(verbuendeter);
        state.AddUnit(umklammerter);
        state.AddUnit(new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 4), speed: 10));
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(umklammerter), state, Greifen));
        Assert.True(IstAngeheftet(hugger, state));
        return (state, hugger, verbuendeter, umklammerter);
    }

    [Fact]
    public void SolangeEinWeitererHeldLebt_BleibtDieAnheftungUnveraendert()
    {
        var (state, hugger, verbuendeter, _) = Aufbau();

        CombatSystem.ApplyDamageAndHandleDefeat(state, verbuendeter, 100); // verletzt, nicht besiegt

        Assert.True(IstAngeheftet(hugger, state));
        Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
    }

    /// <summary>
    /// Zeitpunkt: im selben Moment, in dem der vorletzte Held fällt - nicht erst
    /// zu Beginn des nächsten Hugger-Zugs. Danach ist der Hugger angreifbar, kann
    /// angreifen und bewegen, und die Schwächung des Gegners ist aufgehoben.
    /// </summary>
    [Fact]
    public void FaelltDerVorletzteHeld_LoestSichDerHuggerSofort()
    {
        var (state, hugger, verbuendeter, umklammerter) = Aufbau();
        Assert.Equal(67, EffectiveStats.AttackDamage(umklammerter, state));

        CombatSystem.ApplyDamageAndHandleDefeat(state, verbuendeter, 99999);

        Assert.False(IstAngeheftet(hugger, state));
        Assert.Empty(state.GetEffectsOnUnit(umklammerter.Id));
        Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        Assert.False(EffectiveStats.IsAttackingPrevented(hugger, state));
        Assert.False(EffectiveStats.IsMovementPrevented(hugger, state));
        Assert.Equal(umklammerter.BaseAttackDamage, EffectiveStats.AttackDamage(umklammerter, state));
        Assert.Contains(hugger, umklammerter.GetLegalAttacks(state)); // sofort angreifbar
    }

    /// <summary>Als letzter Held kann der Hugger nicht erneut greifen - sonst entstünde das Patt im nächsten Zug neu.</summary>
    [Fact]
    public void AlsLetzterHeld_KannDerHuggerNichtErneutGreifen()
    {
        var (state, hugger, verbuendeter, umklammerter) = Aufbau();
        CombatSystem.ApplyDamageAndHandleDefeat(state, verbuendeter, 99999);

        Assert.DoesNotContain(AbilitySystem.GetLegalAbilityUses(hugger, state), o => o.Mode?.Id == "greifen");
        Assert.False(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(umklammerter), state, Greifen));
        Assert.False(IstAngeheftet(hugger, state));
    }

    /// <summary>
    /// Klon: zählt wie bei der Siegbedingung nicht als weitere Einheit - und kann
    /// ohnehin nicht übrig bleiben: stirbt der Illusionist, verschwindet sein Klon
    /// mit ihm, und der Hugger ist im selben Moment der letzte Held.
    /// </summary>
    [Fact]
    public void FaelltDerIllusionist_VerschwindetSeinKlonUndDerHuggerLoestSich()
    {
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var illusionist = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        foreach (var u in new Unit[] { hugger, illusionist, gegner }) state.AddUnit(u);
        var illusionistZiel = AbilitySystem.GetLegalAbilityUses(illusionist, state).First();
        Assert.True(AbilitySystem.TryUseAbility(illusionist, illusionistZiel.Target, state, illusionistZiel.Mode));
        var klon = Assert.Single(state.Units, u => u.IsClone);
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(gegner), state, Greifen));

        CombatSystem.ApplyDamageAndHandleDefeat(state, illusionist, 99999);

        Assert.True(klon.IsDefeated);
        Assert.False(IstAngeheftet(hugger, state));
    }

    /// <summary>
    /// Symmetrie: beide Teams mit je Hugger + Verbündetem, jeder Hugger umklammert
    /// den gegnerischen Verbündeten. Egal, welcher Verbündete zuerst fällt - am Ende
    /// ist keiner der beiden Hugger mehr angeheftet, und das Ergebnis hängt nicht von
    /// der Reihenfolge ab.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BeideTeams_WerdenSymmetrischBehandelt(bool erstTeamA)
    {
        var state = new GameState(new GameBoard());
        var huggerA = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbA = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var huggerB = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 3), DefaultHeroes.Hugger);
        var verbB = new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        foreach (var u in new Unit[] { huggerA, verbA, huggerB, verbB }) state.AddUnit(u);
        Assert.True(AbilitySystem.TryUseAbility(huggerA, new AbilityTarget.UnitTarget(verbB), state, Greifen));
        Assert.True(AbilitySystem.TryUseAbility(huggerB, new AbilityTarget.UnitTarget(verbA), state, Greifen));

        CombatSystem.ApplyDamageAndHandleDefeat(state, erstTeamA ? verbA : verbB, 99999);

        // Der Hugger des Teams, das seinen Verbündeten verlor, ist letzter Held -> gelöst;
        // der andere verlor sein Ziel -> ebenfalls gelöst (Zieltod beendet die Anheftung).
        Assert.False(IstAngeheftet(huggerA, state));
        Assert.False(IstAngeheftet(huggerB, state));
        Assert.Empty(state.ActiveEffects);
    }
}
