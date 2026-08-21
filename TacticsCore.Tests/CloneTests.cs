using TacticsCore.Abilities;
using TacticsCore.Board;
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
/// Tests für die Klon-Sonderregeln (Anforderung 9) - bewusst über eine
/// eigens für Tests gebaute, klon-erzeugende Fähigkeit geprüft, statt über
/// einen der drei offiziellen Beispielhelden (keiner davon nutzt Klone) -
/// das Klon-SYSTEM ist eine generische Engine-Fähigkeit, keine Eigenschaft
/// eines bestimmten Katalog-Helden.
/// </summary>
public class CloneTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    private static readonly AbilityDefinition CloneAbility = new(
        Id: "testklon",
        DisplayName: "Testklon",
        IsPassive: false,
        TargetType: AbilityTargetType.Self,
        Range: 0,
        Effects: new[] { new EffectTemplate(Name: "Klon", IsInstant: true, SpawnsClone: true) });

    private static Hero NewCloneCapableHero(Guid id, Team team, HexCoordinate position, int speed = 10) =>
        new(id, team, position, DefaultHeroes.Berserker with { Speed = speed, Ability = CloneAbility });

    [Fact]
    public void Klon_AbsorbiertGenauEinenTrefferUndVerschwindetDanach()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0));
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(creator);
        state.AddUnit(attacker);
        AbilitySystem.TryUseAbility(creator, new AbilityTarget.UnitTarget(creator), state);
        var clone = state.Units.Single(u => u.IsClone);

        bool result = attacker.TryAttack(clone, state);

        Assert.True(result);
        Assert.True(clone.IsDefeated);
        Assert.DoesNotContain(clone, state.Units);
    }

    /// <summary>
    /// "Nur ein Klon je Anwender gleichzeitig" wird NICHT durch Ablehnen des
    /// erneuten Einsatzes durchgesetzt, sondern dadurch, dass der alte Klon
    /// beim erneuten Einsatz verschwindet (siehe Illusionist) - ein
    /// erneuter Einsatz bleibt also jederzeit ein legaler Zug.
    /// </summary>
    [Fact]
    public void Klon_NurEinKlonJeAnwenderGleichzeitig_ErneuterEinsatzErsetztDenAltenKlon()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(creator);

        Assert.True(AbilitySystem.TryUseAbility(creator, new AbilityTarget.UnitTarget(creator), state));
        var firstClone = state.Units.Single(u => u.IsClone);

        Assert.NotEmpty(AbilitySystem.GetLegalAbilityUses(creator, state)); // erneuter Einsatz bleibt legal
        Assert.True(AbilitySystem.TryUseAbility(creator, new AbilityTarget.UnitTarget(creator), state));

        Assert.Single(state.Units, u => u.IsClone); // weiterhin nur EIN Klon gleichzeitig
        Assert.True(firstClone.IsDefeated); // der alte Klon ist verschwunden ...
        Assert.DoesNotContain(firstClone, state.Units); // ... und nicht mehr im Spielzustand
    }

    [Fact]
    public void Klon_HatNurBewegungszuege_KeineAngriffsOderFaehigkeitszuege()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0));
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(creator);
        state.AddUnit(enemy);
        AbilitySystem.TryUseAbility(creator, new AbilityTarget.UnitTarget(creator), state);
        var clone = state.Units.Single(u => u.IsClone);

        Assert.NotEmpty(clone.GetLegalMoves(state)); // kann sich bewegen
        Assert.Empty(clone.GetLegalAttacks(state)); // kann nicht angreifen
        Assert.False(clone.TryAttack(enemy, state));
        Assert.IsNotType<Hero>(clone); // strukturell gar nicht fähigkeitsfähig - kein UseAbility-Zug möglich
    }

    [Fact]
    public void Klon_HandeltDirektNachSeinemErzeugerNochInDerselbenRunde()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var slowAlly = NewCloneCapableHero(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 5);
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(creator);
        state.AddUnit(slowAlly);
        state.AddUnit(enemy);

        var agentA = new ScriptedAgent();
        agentA.Script(creator, new TurnAction.UseAbility(new AbilityTarget.UnitTarget(creator)));
        // Unit.CreateClone liefert eine deterministische, aus der Erzeuger-Id
        // abgeleitete Id (siehe Unit.cs) - dieses Objekt wird nie ins Spiel
        // gebracht, dient hier nur dazu, den ScriptedAgent unter der
        // korrekten (künftigen) Klon-Id zu scripten.
        var precomputedClone = Unit.CreateClone(creator);
        agentA.Script(precomputedClone, new TurnAction.Move(new HexCoordinate(0, 1)));
        agentA.Script(slowAlly, new TurnAction.Move(new HexCoordinate(0, 2)));
        var agentB = new ScriptedAgent();

        var controller = new MatchController(state, agentA, agentB, seed: 1, recordReplay: true);
        var result = controller.Run();

        var entries = result.Replay!.Entries.Select(e => (e.ActionType, e.ActingUnitId)).ToList();
        int abilityIndex = entries.FindIndex(e => e.ActionType == ReplayActionType.UseAbility && e.ActingUnitId == creator.Id);
        int cloneMoveIndex = entries.FindIndex(e => e.ActionType == ReplayActionType.Move && e.ActingUnitId == precomputedClone.Id);
        int slowAllyMoveIndex = entries.FindIndex(e => e.ActionType == ReplayActionType.Move && e.ActingUnitId == slowAlly.Id);

        Assert.True(abilityIndex >= 0);
        Assert.True(cloneMoveIndex >= 0);
        Assert.True(slowAllyMoveIndex >= 0);
        Assert.True(cloneMoveIndex > abilityIndex); // Klon handelt NACH seinem Erzeuger
        Assert.True(cloneMoveIndex < slowAllyMoveIndex); // ... aber VOR dem nächsten regulären Rang derselben Runde
    }

    [Fact]
    public void TeamMitNurNochEinemLebendenKlon_GiltAlsBesiegtSobaldDerLetzteHeldFaellt()
    {
        var state = NewState();
        var lastHero = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 1));
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(lastHero);
        state.AddUnit(attacker);

        AbilitySystem.TryUseAbility(lastHero, new AbilityTarget.UnitTarget(lastHero), state);
        Assert.Single(state.Units, u => u.IsClone); // Klon existiert, der Held lebt noch - Team NICHT besiegt

        lastHero.ApplyDamage(lastHero.MaxHitPoints - attacker.BaseAttackDamage); // ein weiterer Treffer genügt

        var agentA = new ScriptedAgent(); // lastHero fällt vor seinem nächsten Zug, handelt also nie mehr
        var agentB = new ScriptedAgent();
        agentB.Script(attacker, new TurnAction.Attack(lastHero));

        var controller = new MatchController(state, agentA, agentB, seed: 1);
        var result = controller.Run();

        Assert.Equal(MatchEndReason.Elimination, result.EndReason);
        Assert.Equal(MatchWinner.TeamB, result.Winner);
        Assert.DoesNotContain(state.Units, u => u.Team == Team.TeamA); // weder Held noch Klon übrig
    }

    [Fact]
    public void Klon_VerschwindetWennDerErzeugerBesiegtWird()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0));
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(creator);
        state.AddUnit(attacker);
        AbilitySystem.TryUseAbility(creator, new AbilityTarget.UnitTarget(creator), state);
        var clone = state.Units.Single(u => u.IsClone);
        Assert.False(clone.IsDefeated);

        CombatSystem.ApplyDamageAndHandleDefeat(state, attacker, creator, creator.MaxHitPoints);

        Assert.True(creator.IsDefeated);
        Assert.True(clone.IsDefeated);
        Assert.DoesNotContain(clone, state.Units);
        Assert.DoesNotContain(creator, state.Units);
    }
}
