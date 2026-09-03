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
    public void Heilung_ElfeKannSichSelbstAlsZielWaehlenUndHeilen()
    {
        // Bachelorarbeit-Regel (siehe AbilityTargetType.Ally): "verbündet"
        // schließt den Anwender selbst mit ein - ein Held ist sich selbst
        // gegenüber nicht feindlich. Betrifft aktuell konkret die Elfe.
        var state = NewState();
        var elfe = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Elfe);
        state.AddUnit(elfe);
        elfe.ApplyDamage(15);

        Assert.Contains(
            AbilitySystem.GetLegalAbilityUses(elfe, state),
            o => o.Target is AbilityTarget.UnitTarget u && u.Unit.Id == elfe.Id);

        bool used = AbilitySystem.TryUseAbility(elfe, new AbilityTarget.UnitTarget(elfe), state);

        Assert.True(used);
        Assert.Equal(elfe.MaxHitPoints, elfe.CurrentHitPoints); // Distanz 0 -> volle Heilung (20) gedeckelt bei MaxHitPoints (45-15+20=50 > 45)
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

        Assert.Equal(3, first.Count); // ally1, ally2 UND die Elfe selbst (siehe AbilityTargetType.Ally)
        Assert.Equal(
            first.Select(o => ((AbilityTarget.UnitTarget)o.Target).Unit.Id),
            second.Select(o => ((AbilityTarget.UnitTarget)o.Target).Unit.Id));
    }

    [Fact]
    public void DreiGegnerAufEinemFeld_GetLegalAbilityUsesLiefertDreiGetrennteEinzelziele()
    {
        // Ergänzt CombatSystemTests.DreiGegnerAufEinemFeld_GetValidAttackTargetsLiefertDreiGetrennteEinzelziele
        // (Bachelorarbeit-Verifikation Punkt C.7) um den Fähigkeiten-Pfad:
        // derselbe ".SelectMany(state.GetUnitsAt)"-Mechanismus wie beim
        // Grundangriff, hier für AbilitySystem.GetLegalTargets bestätigt -
        // stehen mehrere Einheiten auf demselben Feld, erscheint auch hier
        // JEDE als eigenes, wählbares Ziel statt nur eines je Feld.
        var state = NewState();
        var caster = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Elementarmagier);
        var sharedField = new HexCoordinate(0, 1);
        var enemy1 = new TestUnit(Id(2), Team.TeamB, sharedField, speed: 10);
        var enemy2 = new TestUnit(Id(3), Team.TeamB, sharedField, speed: 10);
        var enemy3 = new TestUnit(Id(4), Team.TeamB, sharedField, speed: 10);
        state.AddUnit(caster);
        state.AddUnit(enemy1);
        state.AddUnit(enemy2);
        state.AddUnit(enemy3);

        var options = AbilitySystem.GetLegalAbilityUses(caster, state);
        var targetIds = options.Select(o => ((AbilityTarget.UnitTarget)o.Target).Unit.Id).Distinct().ToList();

        // Elementarschlag hat 2 wählbare Modi (Feuer/Erde) x 3 Ziele = 6
        // Optionen insgesamt, aber genau DREI unterschiedliche Ziele.
        Assert.Equal(3, targetIds.Count);
        Assert.Contains(enemy1.Id, targetIds);
        Assert.Contains(enemy2.Id, targetIds);
        Assert.Contains(enemy3.Id, targetIds);
    }

    [Fact]
    public void FlaechenfaehigkeitMitReichweite_DistanzabfallMisstAbGewaehltemZentrumNichtAbAnwender()
    {
        // Bachelorarbeit-Korrektur: vor dieser Korrektur maß ApplyToUnit die
        // Distanz für ScaleWithDistance IMMER ab der Position des Anwenders -
        // auch bei einer Flächenfähigkeit mit eigener, vom Anwender
        // verschiedener Zielfeld-Wahl (Reichweite > 0). Bei allen neun
        // Standardhelden blieb das folgenlos (siehe DefaultHeroes: jede
        // AllUnitsInRadius-Fähigkeit hat Reichweite 0, das Zentrum IST dort
        // immer das eigene Feld). Dieser Test nutzt bewusst eine testeigene
        // Fähigkeit MIT Reichweite > 0, um genau den zuvor unerreichbaren
        // Fall zu prüfen: ein Verbündeter GENAU auf dem gewählten
        // Zentrumsfeld (Distanz 0 vom Zentrum) muss die VOLLE Wirkung
        // erhalten, unabhängig davon, wie weit dieses Zentrum vom Anwender
        // entfernt gewählt wurde.
        var testAbility = new AbilityDefinition(
            Id: "test-flaechenheilung",
            DisplayName: "Test-Flächenheilung",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsInRadius,
            Range: 3,
            Radius: 1,
            AreaFilter: AreaTargetFilter.AlliesOnly,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Testheilung",
                    IsInstant: true,
                    InstantHeal: 20,
                    ScaleWithDistance: true,
                    DistanceFalloffPerStep: 5,
                    MinimumAfterDistanceScaling: 0),
            });
        var casterDefinition = DefaultHeroes.Elfe with { Ability = testAbility };

        var state = NewState();
        var caster = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), casterDefinition);
        var centerField = new HexCoordinate(0, 3); // Distanz 3 vom Anwender - liegt innerhalb der Reichweite 3
        var allyOnCenter = new TestUnit(Id(2), Team.TeamA, centerField, speed: 10); // Distanz 0 vom ZENTRUM, aber Distanz 3 vom ANWENDER
        allyOnCenter.ApplyDamage(20);
        state.AddUnit(caster);
        state.AddUnit(allyOnCenter);

        bool used = AbilitySystem.TryUseAbility(caster, new AbilityTarget.FieldTarget(centerField), state);

        Assert.True(used);
        // Bei (fälschlicher) Distanzmessung ab dem Anwender wäre die Distanz
        // 3, die Heilung also nur 20 - 5*3 = 5 - korrekt ab dem gewählten
        // Zentrumsfeld gemessen ist die Distanz 0, die Heilung also voll 20.
        Assert.Equal(allyOnCenter.MaxHitPoints, allyOnCenter.CurrentHitPoints);
    }
}
