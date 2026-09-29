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
/// Designentscheidung "Hugger-Überarbeitung" (siehe <see cref="DefaultHeroes.Hugger"/>):
/// Grundschaden 60, Angriff während der Anheftung mit halbem Schaden, und die
/// Unangreifbarkeit ist an das Angreifen gekoppelt (ein Angriff hebt sie genau
/// eine Runde lang auf).
/// </summary>
public class HuggerUeberarbeitungTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly AbilityMode Greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
    private static readonly AbilityMode Loesen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "loesen");

    private static bool IstAngeheftet(Unit hugger, GameState state) =>
        state.ActiveEffects.Any(e => e.Name == "Umklammerung" && e.SourceUnitId == hugger.Id);

    /// <summary>Team A: Hugger + Verbündeter; Team B: zwei Gegner, der Hugger umklammert den ersten (zieht auf sein Feld).</summary>
    private static (GameState State, Hero Hugger, Unit Verbuendeter, Unit Gegner) AufGegnerAngeheftet()
    {
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        foreach (var u in new Unit[] { hugger, verbuendeter, gegner, new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 5), speed: 10) }) state.AddUnit(u);
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(gegner), state, Greifen));
        Assert.Equal(gegner.Position, hugger.Position);
        return (state, hugger, verbuendeter, gegner);
    }

    // --- 1. Grundschaden ------------------------------------------------

    [Fact]
    public void Grundschaden_Ist60()
    {
        Assert.Equal(60, DefaultHeroes.Hugger.BaseAttackDamage);
    }

    // --- 2. Angreifen während der Anheftung ------------------------------

    /// <summary>Auf einem Gegner angeheftet: er kann GENAU DIESEN Gegner angreifen (gleiches Feld), mit halbem Schaden.</summary>
    [Fact]
    public void AufGegnerAngeheftet_GreiftDiesenGegnerMitHalbemSchadenAn()
    {
        var (state, hugger, _, gegner) = AufGegnerAngeheftet();

        Assert.False(EffectiveStats.IsAttackingPrevented(hugger, state));
        Assert.Equal(30, EffectiveStats.AttackDamage(hugger, state));
        Assert.Contains(gegner, hugger.GetLegalAttacks(state));

        Assert.True(CombatSystem.TryAttackDetailed(hugger, gegner, state, out var ziel, out int schaden));
        Assert.Same(gegner, ziel);
        Assert.Equal(30, schaden);
        Assert.True(IstAngeheftet(hugger, state)); // die Anheftung bleibt bestehen
    }

    [Fact]
    public void AufVerbuendetemAngeheftet_GreiftMitHalbemSchadenAn()
    {
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(1, 0), speed: 10); // neben dem Verbündeten
        foreach (var u in new Unit[] { hugger, verbuendeter, gegner, new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 5), speed: 10) }) state.AddUnit(u);
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(verbuendeter), state, Greifen));
        Assert.Equal(verbuendeter.Position, hugger.Position);

        Assert.True(CombatSystem.TryAttackDetailed(hugger, gegner, state, out _, out int schaden));
        Assert.Equal(30, schaden);
        Assert.Equal(gegner.MaxHitPoints - 30, gegner.CurrentHitPoints);
    }

    [Fact]
    public void NichtAngeheftet_GreiftMitVollemSchadenAn()
    {
        var (state, hugger, _, gegner) = AufGegnerAngeheftet();
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(hugger), state, Loesen));

        Assert.Equal(60, EffectiveStats.AttackDamage(hugger, state));
        Assert.True(CombatSystem.TryAttackDetailed(hugger, gegner, state, out _, out int schaden));
        Assert.Equal(60, schaden);
    }

    // --- 3. Unangreifbarkeit an das Angreifen gekoppelt ------------------

    [Fact]
    public void OhneAngriff_BleibtErUnangreifbar()
    {
        var (state, hugger, verbuendeter, gegner) = AufGegnerAngeheftet();

        Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        Assert.DoesNotContain(hugger, gegner.GetLegalAttacks(state));

        // Züge vergehen, ohne dass er angreift: er bleibt unangreifbar.
        foreach (var einheit in new[] { gegner, verbuendeter, (Unit)hugger, gegner })
        {
            EffectSystem.TickUnitEffects(state, einheit);
            Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        }
    }

    /// <summary>
    /// Nach einem Angriff angreifbar - über die Züge aller anderen Einheiten hinweg
    /// bis zum Beginn seines NÄCHSTEN eigenen Zugs (genau eine Runde); dann wieder
    /// unangreifbar. <see cref="EffectSystem.TickUnitEffects"/> ist genau der Aufruf,
    /// den der MatchController zu Beginn jedes Zugs ausführt.
    /// </summary>
    [Fact]
    public void NachAngriff_AngreifbarBisZumBeginnSeinesNaechstenEigenenZugs()
    {
        var (state, hugger, verbuendeter, gegner) = AufGegnerAngeheftet();

        Assert.True(CombatSystem.TryAttack(hugger, gegner, state));
        Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        Assert.Contains(hugger, gegner.GetLegalAttacks(state));

        // Züge der übrigen Einheiten (Rest dieser Runde, Beginn der nächsten): weiter angreifbar.
        foreach (var andere in new[] { gegner, verbuendeter, gegner })
        {
            EffectSystem.TickUnitEffects(state, andere);
            Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        }

        // Beginn seines nächsten eigenen Zugs: wieder unangreifbar, Anheftung besteht weiter.
        EffectSystem.TickUnitEffects(state, hugger);
        Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        Assert.True(IstAngeheftet(hugger, state));
        Assert.DoesNotContain(state.ActiveEffects, e => e.Name == CombatSystem.ExposedEffectName);
    }

    [Fact]
    public void ErneuterAngriff_VerlaengertDieAngreifbarkeitUmEineWeitereRunde()
    {
        var (state, hugger, _, gegner) = AufGegnerAngeheftet();
        Assert.True(CombatSystem.TryAttack(hugger, gegner, state));

        EffectSystem.TickUnitEffects(state, hugger); // nächster eigener Zug: Marker läuft ab ...
        Assert.True(CombatSystem.TryAttack(hugger, gegner, state)); // ... und er greift erneut an
        Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));

        EffectSystem.TickUnitEffects(state, gegner);
        Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        EffectSystem.TickUnitEffects(state, hugger);
        Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
    }

    /// <summary>
    /// Angriff in einem Zug, Lösen im nächsten: zu Beginn des nächsten Zugs ist er
    /// kurz wieder unangreifbar, nach dem Lösen ist er eine gewöhnliche, jederzeit
    /// angreifbare Einheit (keine Anheftung, kein Marker mehr).
    /// </summary>
    [Fact]
    public void AngriffUndImNaechstenZugLoesen_DanachGewoehnlichAngreifbar()
    {
        var (state, hugger, _, gegner) = AufGegnerAngeheftet();
        Assert.True(CombatSystem.TryAttack(hugger, gegner, state));

        EffectSystem.TickUnitEffects(state, hugger);
        Assert.True(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));

        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(hugger), state, Loesen));
        Assert.False(IstAngeheftet(hugger, state));
        Assert.False(EffectiveStats.PreventsBeingTargetedBy(hugger, Team.TeamB, state));
        Assert.Contains(hugger, gegner.GetLegalAttacks(state));
    }

    /// <summary>Unabhängig davon: die Regel "angehefteter letzter Held" gilt weiter (Lösen bei Tod des letzten Verbündeten).</summary>
    [Fact]
    public void RegelLetzterHeld_GiltWeiterhin()
    {
        var (state, hugger, verbuendeter, gegner) = AufGegnerAngeheftet();

        CombatSystem.ApplyDamageAndHandleDefeat(state, verbuendeter, 99999);

        Assert.False(IstAngeheftet(hugger, state));
        Assert.DoesNotContain(AbilitySystem.GetLegalAbilityUses(hugger, state), o => o.Mode?.Id == "greifen");
        Assert.Contains(hugger, gegner.GetLegalAttacks(state));
    }
}
