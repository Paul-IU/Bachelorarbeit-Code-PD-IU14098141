using TacticsCore.Abilities;
using TacticsCore.Board;
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

public class AbilitySystemTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void EinHeld_KannProZugEntwederAngreifenOderDieFaehigkeitNutzen()
    {
        var state = NewState();
        var hero = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(hero);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        // Beide Aktionen zielen auf denselben "Aktions"-Budget-Slot - der
        // zweite Versuch darf nicht mehr wirken.
        agent.Script(hero,
            new TurnAction.Attack(enemy),
            new TurnAction.UseAbility(new AbilityTarget.FieldTarget(hero.Position)));

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        controller.ExecuteTurn(hero, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Equal(enemy.MaxHitPoints - hero.BaseAttackDamage, enemy.CurrentHitPoints); // Angriff wirkte
        Assert.Empty(state.GetEffectsOnField(hero.Position)); // Fähigkeit wurde NICHT zusätzlich angewendet
    }

    [Fact]
    public void Heilung_UeberschreitetDieMaximalenHpNicht()
    {
        var state = NewState();
        var elfe = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Elfe);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(elfe);
        state.AddUnit(ally);
        ally.ApplyDamage(5); // knapp unter Maximum - 20 Heilung würde weit darüber hinausschießen

        bool used = AbilitySystem.TryUseAbility(elfe, new AbilityTarget.UnitTarget(ally), state);

        Assert.True(used);
        Assert.Equal(ally.MaxHitPoints, ally.CurrentHitPoints);
    }

    [Fact]
    public void DistanzabhaengigeWirkung_IstAufDistanz1StaerkerAlsAufDistanz2()
    {
        var state = NewState();
        var elfe = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Elfe);
        var allyClose = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // Distanz 1
        var allyFar = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 2), speed: 10); // Distanz 2
        state.AddUnit(elfe);
        state.AddUnit(allyClose);
        state.AddUnit(allyFar);
        // Auf 1 HP setzen statt auf 0: bei 0 HP gilt die Einheit bereits als
        // besiegt und wäre für die Heilung gar kein gültiges Ziel mehr -
        // das würde diesen Test verfälschen, nicht die Distanzskalierung
        // selbst prüfen.
        allyClose.ApplyDamage(allyClose.MaxHitPoints - 1);
        allyFar.ApplyDamage(allyFar.MaxHitPoints - 1);

        Assert.True(AbilitySystem.TryUseAbility(elfe, new AbilityTarget.UnitTarget(allyClose), state));
        Assert.True(AbilitySystem.TryUseAbility(elfe, new AbilityTarget.UnitTarget(allyFar), state));

        Assert.True(allyClose.CurrentHitPoints > allyFar.CurrentHitPoints);
        Assert.Equal(1 + 15, allyClose.CurrentHitPoints); // 1 HP + (20 - 5*1)
        Assert.Equal(1 + 10, allyFar.CurrentHitPoints);   // 1 HP + (20 - 5*2, durch Minimum 10 gestützt)
    }

    [Fact]
    public void PassiveFaehigkeit_SchadenDesBerserkersSteigtBeiSinkendenHp()
    {
        var state = NewState();
        var berserker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Berserker);
        state.AddUnit(berserker);

        int damageAtFullHp = EffectiveStats.AttackDamage(berserker, state);
        Assert.Equal(berserker.BaseAttackDamage, damageAtFullHp); // volle HP -> kein Bonus

        berserker.ApplyDamage(berserker.MaxHitPoints / 2);
        int damageAtHalfHp = EffectiveStats.AttackDamage(berserker, state);

        Assert.True(damageAtHalfHp > damageAtFullHp);
    }

    [Fact]
    public void FaehigkeitMitWaehlbaremModus_ModusIstTeilDerOptionUndBestimmtDieWirkung()
    {
        var fireMode = new AbilityMode("feuer", "Feuer", new[] { new EffectTemplate(Name: "Feuerschaden", IsInstant: true, InstantDamage: 8) });
        var iceMode = new AbilityMode("eis", "Eis", new[] { new EffectTemplate(Name: "Eisschaden", IsInstant: true, InstantDamage: 3) });
        var ability = new AbilityDefinition(
            Id: "elementarangriff", DisplayName: "Elementarangriff", IsPassive: false,
            TargetType: AbilityTargetType.Enemy, Range: 1, Effects: Array.Empty<EffectTemplate>(),
            Modes: new[] { fireMode, iceMode });
        var definition = DefaultHeroes.Berserker with { Ability = ability };

        var state = NewState();
        var caster = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), definition);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(caster);
        state.AddUnit(enemy);

        var options = AbilitySystem.GetLegalAbilityUses(caster, state);
        Assert.Equal(2, options.Count); // ein Ziel, zwei Modi - die Wahl ist Teil des Zuges

        bool used = AbilitySystem.TryUseAbility(caster, new AbilityTarget.UnitTarget(enemy), state, fireMode);

        Assert.True(used);
        Assert.Equal(enemy.MaxHitPoints - 8, enemy.CurrentHitPoints);
    }

    [Fact]
    public void GetLegalAbilityUses_IstBeiWiederholtenAufrufenImmerIdentischSortiert()
    {
        var state = NewState();
        var elfe = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Elfe);
        var ally1 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var ally2 = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(-1, 1), speed: 10);
        state.AddUnit(elfe);
        state.AddUnit(ally1);
        state.AddUnit(ally2);

        var first = AbilitySystem.GetLegalAbilityUses(elfe, state);
        var second = AbilitySystem.GetLegalAbilityUses(elfe, state);

        Assert.Equal(2, first.Count);
        Assert.Equal(
            first.Select(o => ((AbilityTarget.UnitTarget)o.Target).Unit.Id),
            second.Select(o => ((AbilityTarget.UnitTarget)o.Target).Unit.Id));
    }
}
