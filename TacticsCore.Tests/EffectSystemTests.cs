using TacticsCore.Abilities;
using TacticsCore.Board;
using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Movement;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class EffectSystemTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    // -----------------------------------------------------------------
    // Schadensreduktion (Schildträger-Mechanik)
    // -----------------------------------------------------------------

    [Fact]
    public void Schadensreduktion_SenktEingehendenSchadenKorrektUndLaeuftAb()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger);
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(attacker);

        Assert.True(AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state));

        int hpBeforeReducedHit = tank.CurrentHitPoints;
        attacker.TryAttack(tank, state); // 10 Grundschaden, mit 50% Reduktion nur 5
        Assert.Equal(hpBeforeReducedHit - 5, tank.CurrentHitPoints);

        // Dauer 1 - läuft mit dem nächsten Feld-Tick (Rundenbeginn) ab.
        EffectSystem.TickFieldEffects(state);
        Assert.Empty(state.GetEffectsOnField(tank.Position));

        int hpBeforeFullHit = tank.CurrentHitPoints;
        attacker.TryAttack(tank, state); // keine Reduktion mehr - voller Schaden
        Assert.Equal(hpBeforeFullHit - 10, tank.CurrentHitPoints);
    }

    [Fact]
    public void Schadensreduktion_WirktAufAlleEinheitenDesselbenFeldes()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10); // teilt sich das Feld mit dem Tank
        var attacker = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(ally);
        state.AddUnit(attacker);

        AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state);

        int allyHpBefore = ally.CurrentHitPoints;
        attacker.TryAttack(ally, state);

        Assert.Equal(allyHpBefore - 5, ally.CurrentHitPoints); // ebenfalls 50% Reduktion, obwohl nicht selbst Anwender
    }

    // -----------------------------------------------------------------
    // Lebenszyklus: Ende bei Tod von Quelle/Ziel, Tick-Zeitpunkt, Stapelregel
    // -----------------------------------------------------------------

    [Fact]
    public void EinEffekt_EndetWennSeineQuelleBesiegtWird()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger);
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(attacker);

        AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state);
        Assert.NotEmpty(state.GetEffectsOnField(tank.Position));

        CombatSystem.ApplyDamageAndHandleDefeat(state, tank, 9999); // Quelle (Tank) besiegen, deutlich überkillt trotz Reduktion

        Assert.True(tank.IsDefeated);
        Assert.Empty(state.GetEffectsOnField(new HexCoordinate(0, 0)));
    }

    [Fact]
    public void EinEffekt_EndetWennSeinZielBesiegtWird()
    {
        var state = NewState();
        var source = new TestUnit(Id(1), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        var target = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(source);
        state.AddUnit(target);

        var template = new EffectTemplate(Name: "Schwächung", IsInstant: false, Duration: null, StatModifiers: new StatModifierSet(BaseAttackDamageDelta: -5));
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template));
        Assert.NotEmpty(state.GetEffectsOnUnit(target.Id));

        CombatSystem.ApplyDamageAndHandleDefeat(state, target, 9999); // Ziel besiegen

        Assert.True(target.IsDefeated);
        Assert.Empty(state.GetEffectsOnUnit(target.Id));
    }

    [Fact]
    public void Effekte_TickenGenauEinmalProAufruf()
    {
        var state = NewState();
        var source = new TestUnit(Id(1), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        var target = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(source);
        state.AddUnit(target);

        var template = new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 3, DamagePerTick: 4);
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template));

        int hpBefore = target.CurrentHitPoints;
        EffectSystem.TickUnitEffects(state, target);
        Assert.Equal(hpBefore - 4, target.CurrentHitPoints); // genau ein Tick

        EffectSystem.TickUnitEffects(state, target);
        Assert.Equal(hpBefore - 8, target.CurrentHitPoints); // zweiter, unabhängiger Aufruf

        var effect = Assert.Single(state.GetEffectsOnUnit(target.Id));
        Assert.Equal(1, effect.RemainingDuration); // Dauer 3, zweimal getickt -> noch 1 übrig
    }

    [Fact]
    public void StackingRule_Stack_LegtZusaetzlicheUnabhaengigeInstanzAn()
    {
        var (state, source, target) = NewSourceAndTarget();
        var template = new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 2, DamagePerTick: 3, Stacking: StackingRule.Stack);

        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template));
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template));

        Assert.Equal(2, state.GetEffectsOnUnit(target.Id).Count());
    }

    [Fact]
    public void StackingRule_Replace_ErsetztDenBestehendenEffektVollstaendig()
    {
        var (state, source, target) = NewSourceAndTarget();
        var weak = new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 5, DamagePerTick: 1, Stacking: StackingRule.Replace);
        var strong = new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 2, DamagePerTick: 9, Stacking: StackingRule.Replace);

        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, weak));
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, strong));

        var effect = Assert.Single(state.GetEffectsOnUnit(target.Id));
        Assert.Equal(9, effect.DamagePerTick);
        Assert.Equal(2, effect.RemainingDuration);
    }

    [Fact]
    public void StackingRule_RefreshDuration_BehaeltDenBestehendenEffektUndSetztNurDieRestdauerZurueck()
    {
        var (state, source, target) = NewSourceAndTarget();
        var template = new EffectTemplate(Name: "Schwächung", IsInstant: false, Duration: 3, StatModifiers: new StatModifierSet(BaseAttackDamageDelta: -2), Stacking: StackingRule.RefreshDuration);

        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template));
        EffectSystem.TickUnitEffects(state, target); // Restdauer auf 2
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, template)); // erneut angewendet

        var effect = Assert.Single(state.GetEffectsOnUnit(target.Id));
        Assert.Equal(3, effect.RemainingDuration); // aufgefrischt statt aufsummiert oder verdoppelt
    }

    private static (GameState State, Unit Source, Unit Target) NewSourceAndTarget()
    {
        var state = NewState();
        var source = new TestUnit(Id(1), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        var target = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(source);
        state.AddUnit(target);
        return (state, source, target);
    }

    // -----------------------------------------------------------------
    // Feldsperre, Sichtlinien-Blockade, Bewegungsunfähigkeit
    // -----------------------------------------------------------------

    [Fact]
    public void EinGesperrtesFeld_VerhindertDasDurchquerenDurchGegnerNichtDurchVerbuendete()
    {
        var state = NewState();
        var locker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(-1, 1), speed: 10);
        state.AddUnit(locker);

        var template = new EffectTemplate(Name: "Bannkreis", IsInstant: false, Duration: null, LocksFieldForEnemies: true);
        EffectSystem.Apply(state, ActiveEffect.OnField(locker, new HexCoordinate(0, 1), template));

        var enemyMover = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        var allyMover = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(enemyMover);
        state.AddUnit(allyMover);

        var enemyReachable = MovementSystem.GetReachableFields(enemyMover, state).Select(rf => rf.Coordinate).ToHashSet();
        var allyReachable = MovementSystem.GetReachableFields(allyMover, state).Select(rf => rf.Coordinate).ToHashSet();

        Assert.DoesNotContain(new HexCoordinate(0, 1), enemyReachable);
        Assert.Contains(new HexCoordinate(0, 1), allyReachable);
    }

    [Fact]
    public void AngriffMitBlockierterSichtlinie_TrifftDenBlockierendenStattDesEigentlichenZiels()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10, attackRange: 2);
        var blocker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10); // liegt genau zwischen Angreifer und Ziel
        var realTarget = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(blocker);
        state.AddUnit(realTarget);

        var template = new EffectTemplate(Name: "Deckung", IsInstant: false, Duration: null, RedirectsAttacksToSource: true);
        EffectSystem.Apply(state, ActiveEffect.OnField(blocker, blocker.Position, template));

        bool result = attacker.TryAttack(realTarget, state);

        Assert.True(result);
        Assert.Equal(realTarget.MaxHitPoints, realTarget.CurrentHitPoints); // unberührt
        Assert.Equal(blocker.MaxHitPoints - attacker.BaseAttackDamage, blocker.CurrentHitPoints); // stattdessen getroffen
    }

    [Fact]
    public void EinBewegungsunfaehigerHeld_HatKeineLegalenBewegungszuege()
    {
        var state = NewState();
        var hero = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(hero);

        var template = new EffectTemplate(Name: "Erstarrung", IsInstant: false, Duration: 2, PreventsMovement: true);
        EffectSystem.Apply(state, ActiveEffect.OnUnit(hero, hero, template));

        Assert.Empty(hero.GetLegalMoves(state));
        Assert.Empty(hero.GetReachableFields(state));
        Assert.False(hero.TryMove(hero.Position, state)); // selbst "Stehenbleiben" schlägt fehl
    }
}
