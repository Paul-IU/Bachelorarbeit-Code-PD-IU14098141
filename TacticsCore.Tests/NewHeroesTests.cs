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
/// Tests für die sechs zusätzlichen Helden (Wand, Illusionist, Musiker,
/// Hugger, Elementarmagier, Bogenschütze) - je Held die in der
/// Aufgabenstellung geforderten Fälle. Wo eine Mechanik bereits generisch
/// an anderer Stelle geprüft ist (z.B. das grundsätzliche Klon-Verhalten in
/// <see cref="CloneTests"/>), wird sie hier nur noch einmal mit der
/// TATSÄCHLICHEN Heldendefinition nachvollzogen, statt den gesamten
/// Mechanismus erneut von Grund auf zu beweisen.
/// </summary>
public class NewHeroesTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    private static Hero NewHero(Guid id, Team team, HexCoordinate position, HeroDefinition definition) =>
        new(id, team, position, definition);

    // -----------------------------------------------------------------
    // WAND
    // -----------------------------------------------------------------

    [Fact]
    public void Wand_SperrtIhrEigenesFeldFuerGegnerischeDurchquerung()
    {
        var state = NewState();
        var wand = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Wand);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 4), speed: 10); // Distanz 2 zu (0,2) über (0,3)
        state.AddUnit(wand);
        state.AddUnit(enemy);

        Assert.Contains(new HexCoordinate(0, 2), enemy.GetLegalMoves(state)); // vorher erreichbar

        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));

        Assert.DoesNotContain(new HexCoordinate(0, 2), enemy.GetLegalMoves(state)); // jetzt gesperrt
    }

    [Fact]
    public void Wand_SichtlinienUmleitungGiltAuchFuerZielAufDemselbenFeldWieDieWand()
    {
        var state = NewState();
        var wand = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        var allySharingField = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // teilt sich das Feld mit der Wand
        var enemyAttacker = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10, attackRange: 2); // Distanz 2, Sichtlinie über (0,2)
        state.AddUnit(wand);
        state.AddUnit(allySharingField);
        state.AddUnit(enemyAttacker);

        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));

        bool attacked = CombatSystem.TryAttackDetailed(enemyAttacker, allySharingField, state, out var actualTarget, out _);

        Assert.True(attacked);
        Assert.Same(wand, actualTarget); // die Wand fängt den Angriff ab, nicht der eigentliche Verbündete
        Assert.Equal(allySharingField.MaxHitPoints, allySharingField.CurrentHitPoints); // Verbündeter unberührt
        Assert.True(wand.CurrentHitPoints < wand.MaxHitPoints);
    }

    [Fact]
    public void Wand_IstWaehrendDerSperreSelbstBewegungsunfaehig()
    {
        var state = NewState();
        var wand = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        state.AddUnit(wand);

        Assert.NotEmpty(wand.GetLegalMoves(state));
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        Assert.Empty(wand.GetLegalMoves(state)); // keine legalen Bewegungszüge, nicht einmal "stehen bleiben"
    }

    [Fact]
    public void Wand_SperreEndetKorrektWennDieBewegungsunfaehigkeitEndet()
    {
        var state = NewState();
        var wand = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Wand);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 4), speed: 10);
        state.AddUnit(wand);
        state.AddUnit(enemy);

        AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state);
        Assert.Empty(wand.GetLegalMoves(state));
        Assert.DoesNotContain(new HexCoordinate(0, 2), enemy.GetLegalMoves(state));

        // Feld-verankerte Effekte ticken einmal je Runde (siehe EffectSystem) -
        // bei Dauer 1 reicht ein einziger Tick, um BEIDE gekoppelten
        // Instanzen (Feldsperre + Bewegungsunfähigkeit) zu beenden.
        EffectSystem.TickFieldEffects(state);

        Assert.Empty(state.GetEffectsOnField(wand.Position));
        Assert.Empty(state.GetEffectsOnUnit(wand.Id));
        Assert.NotEmpty(wand.GetLegalMoves(state)); // Wand kann sich wieder bewegen
        Assert.Contains(new HexCoordinate(0, 2), enemy.GetLegalMoves(state)); // Feld wieder passierbar
    }

    // -----------------------------------------------------------------
    // ILLUSIONIST
    // -----------------------------------------------------------------

    [Fact]
    public void Illusionist_KlonAbsorbiertGenauEinenTrefferUndVerschwindetDanach()
    {
        var state = NewState();
        var illusionist = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(illusionist);
        state.AddUnit(attacker);
        AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state);
        var clone = state.Units.Single(u => u.IsClone);

        Assert.True(attacker.TryAttack(clone, state));

        Assert.True(clone.IsDefeated);
        Assert.DoesNotContain(clone, state.Units);
    }

    [Fact]
    public void Illusionist_ErneuterEinsatzErsetztDenBestehendenKlon()
    {
        var state = NewState();
        var illusionist = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        state.AddUnit(illusionist);

        AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state);
        var firstClone = state.Units.Single(u => u.IsClone);

        Assert.True(AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state));

        Assert.Single(state.Units, u => u.IsClone);
        Assert.True(firstClone.IsDefeated);
        Assert.DoesNotContain(firstClone, state.Units);
    }

    [Fact]
    public void Illusionist_KlonKannSichNurBewegenNichtAngreifenOderFaehigkeitNutzen()
    {
        var state = NewState();
        var illusionist = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(illusionist);
        state.AddUnit(enemy);
        AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state);
        var clone = state.Units.Single(u => u.IsClone);

        Assert.NotEmpty(clone.GetLegalMoves(state));
        Assert.Empty(clone.GetLegalAttacks(state));
        Assert.False(clone.TryAttack(enemy, state));
        Assert.IsNotType<Hero>(clone); // strukturell gar nicht fähigkeitsfähig
    }

    [Fact]
    public void Illusionist_TeamMitNurNochLebendemKlon_GiltAlsBesiegtSobaldDerHeldFaellt()
    {
        var state = NewState();
        var illusionist = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Illusionist);
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(illusionist);
        state.AddUnit(attacker);

        AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state);
        Assert.Single(state.Units, u => u.IsClone);

        illusionist.ApplyDamage(illusionist.MaxHitPoints - attacker.BaseAttackDamage); // ein weiterer Treffer genügt

        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        agentB.Script(attacker, new TurnAction.Attack(illusionist));

        var controller = new MatchController(state, agentA, agentB, seed: 1);
        var result = controller.Run();

        Assert.Equal(MatchWinner.TeamB, result.Winner);
        Assert.DoesNotContain(state.Units, u => u.Team == Team.TeamA); // weder Held noch Klon übrig
    }

    [Fact]
    public void Illusionist_KlonVerschwindetWennDerErzeugerBesiegtWird()
    {
        var state = NewState();
        var illusionist = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        var attacker = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(illusionist);
        state.AddUnit(attacker);
        AbilitySystem.TryUseAbility(illusionist, new AbilityTarget.UnitTarget(illusionist), state);
        var clone = state.Units.Single(u => u.IsClone);

        CombatSystem.ApplyDamageAndHandleDefeat(state, attacker, illusionist, illusionist.MaxHitPoints);

        Assert.True(clone.IsDefeated);
        Assert.DoesNotContain(clone, state.Units);
    }

    // -----------------------------------------------------------------
    // MUSIKER
    // -----------------------------------------------------------------

    [Fact]
    public void Musiker_BeideModiSindEigenstaendigeLegaleZuege()
    {
        var state = NewState();
        var musiker = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Musiker);
        state.AddUnit(musiker);

        var options = AbilitySystem.GetLegalAbilityUses(musiker, state);

        Assert.Equal(2, options.Count); // ein (implizites) Ziel, zwei Modi
        Assert.Contains(options, o => o.Mode?.Id == "kampflied");
        Assert.Contains(options, o => o.Mode?.Id == "marschlied");
    }

    [Fact]
    public void Musiker_BuffWirktNurAufVerbuendeteImUmkreisUndLaeuftAb()
    {
        var state = NewState();
        var musiker = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Musiker);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // Distanz 1
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(1, 2), speed: 10); // Distanz 1, aber Gegner
        state.AddUnit(musiker);
        state.AddUnit(ally);
        state.AddUnit(enemy);

        var kampflied = musiker.Definition.Ability.Modes!.Single(m => m.Id == "kampflied");
        bool used = AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied);

        Assert.True(used);
        Assert.True(EffectiveStats.AttackDamage(ally, state) > ally.BaseAttackDamage); // Verbündeter profitiert
        Assert.Equal(enemy.BaseAttackDamage, EffectiveStats.AttackDamage(enemy, state)); // Gegner NICHT betroffen

        // Dauer 2 - zwei Ticks am eigenen Zug des Verbündeten beenden den Buff.
        EffectSystem.TickUnitEffects(state, ally);
        EffectSystem.TickUnitEffects(state, ally);
        Assert.Empty(state.GetEffectsOnUnit(ally.Id));
        Assert.Equal(ally.BaseAttackDamage, EffectiveStats.AttackDamage(ally, state));
    }

    // -----------------------------------------------------------------
    // HUGGER
    // -----------------------------------------------------------------

    [Fact]
    public void Hugger_AnheftungBewegtSichMitDemZielMitUndZiehtHuggerRaeumlichMit()
    {
        var state = NewState();
        var hugger = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(ally);

        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ally), state));
        Assert.True(ally.TryMove(new HexCoordinate(-1, 2), state));

        Assert.NotEmpty(state.GetEffectsOnUnit(ally.Id)); // Effekt ist an die EINHEIT gebunden, nicht an das Feld
        // Hugger "klebt" an der Einheit - zieht sie um, wird er räumlich
        // mitgezogen, ohne selbst eine Bewegung zu verbrauchen.
        Assert.Equal(ally.Position, hugger.Position);
    }

    [Fact]
    public void Hugger_IstSolangeDieAnheftungAktivIstSelbstBewegungsunfaehig()
    {
        var state = NewState();
        var hugger = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(ally);

        Assert.NotEmpty(hugger.GetLegalMoves(state));
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ally), state));
        Assert.Empty(hugger.GetLegalMoves(state));
    }

    [Fact]
    public void Hugger_VerbuendeterErhaeltBonusGegnerErhaeltAbzugUndNurEineAnheftungGleichzeitig()
    {
        var state = NewState();
        var hugger = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(1, 1), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(ally);
        state.AddUnit(enemy);

        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ally), state));
        Assert.True(EffectiveStats.AttackDamage(ally, state) > ally.BaseAttackDamage);

        // Erneuter Einsatz auf ein ANDERES (gegnerisches) Ziel ersetzt die
        // Anheftung - "nur eine Anheftung gleichzeitig".
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(enemy), state));

        Assert.Equal(ally.BaseAttackDamage, EffectiveStats.AttackDamage(ally, state)); // Bonus beim alten Ziel weg
        Assert.True(EffectiveStats.AttackDamage(enemy, state) < enemy.BaseAttackDamage); // Abzug beim neuen Ziel
        Assert.Single(state.ActiveEffects, e => e.TargetUnitId == enemy.Id); // genau eine Anheftung
    }

    [Fact]
    public void Hugger_NimmtSelbstNurReduziertenAnteilDesEingehendenSchadensSolangeAngeheftetIst()
    {
        var state = NewState();
        var hugger = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(ally);
        AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ally), state);

        CombatSystem.ApplyDamageAndHandleDefeat(state, hugger, 20);

        Assert.Equal(hugger.MaxHitPoints - 10, hugger.CurrentHitPoints); // 50% von 20 = 10
    }

    [Fact]
    public void Hugger_AnheftungEndetWennDasZielBesiegtWird()
    {
        var state = NewState();
        var hugger = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var attacker = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(1, 1), speed: 10);
        state.AddUnit(hugger);
        state.AddUnit(ally);
        state.AddUnit(attacker);
        AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ally), state);
        Assert.NotEmpty(state.GetEffectsOnUnit(hugger.Id)); // eigene Schadensreduktion aktiv

        CombatSystem.ApplyDamageAndHandleDefeat(state, attacker, ally, ally.MaxHitPoints);

        Assert.True(ally.IsDefeated);
        Assert.Empty(state.GetEffectsOnUnit(hugger.Id)); // Anheftung UND eigene Schadensreduktion enden gemeinsam
        Assert.Empty(state.ActiveEffects);
    }

    // -----------------------------------------------------------------
    // ELEMENTARMAGIER
    // -----------------------------------------------------------------

    [Fact]
    public void Elementarmagier_FeuerVerursachtSofortschadenPlusSchadenUeberZeit()
    {
        var state = NewState();
        var mage = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(mage);
        state.AddUnit(enemy);
        var feuer = mage.Definition.Ability.Modes!.Single(m => m.Id == "feuer");

        Assert.True(AbilitySystem.TryUseAbility(mage, new AbilityTarget.UnitTarget(enemy), state, feuer));

        Assert.Equal(enemy.MaxHitPoints - 10, enemy.CurrentHitPoints); // Sofortschaden
        var dot = Assert.Single(state.GetEffectsOnUnit(enemy.Id));
        Assert.Equal(4, dot.DamagePerTick);

        EffectSystem.TickUnitEffects(state, enemy);
        Assert.Equal(enemy.MaxHitPoints - 10 - 4, enemy.CurrentHitPoints); // DoT-Tick wirkt zusätzlich
    }

    [Fact]
    public void Elementarmagier_ErdeReduziertDieBewegungsreichweiteDesZiels()
    {
        var state = NewState();
        var mage = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(mage);
        state.AddUnit(enemy);
        var erde = mage.Definition.Ability.Modes!.Single(m => m.Id == "erde");
        int rangeBefore = EffectiveStats.MovementRange(enemy, state);

        Assert.True(AbilitySystem.TryUseAbility(mage, new AbilityTarget.UnitTarget(enemy), state, erde));

        Assert.Equal(enemy.MaxHitPoints - 8, enemy.CurrentHitPoints);
        Assert.Equal(rangeBefore - 1, EffectiveStats.MovementRange(enemy, state));
    }

    [Fact]
    public void Elementarmagier_BeideModiSindWaehlbar()
    {
        var state = NewState();
        var mage = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(mage);
        state.AddUnit(enemy);

        var options = AbilitySystem.GetLegalAbilityUses(mage, state);

        Assert.Equal(2, options.Count); // ein Ziel, zwei Modi
        Assert.Contains(options, o => o.Mode?.Id == "feuer");
        Assert.Contains(options, o => o.Mode?.Id == "erde");
    }

    // -----------------------------------------------------------------
    // BOGENSCHÜTZE
    // -----------------------------------------------------------------

    private static AbilityMode LadenMode(Hero bogenschuetze) => bogenschuetze.Definition.Ability.Modes!.Single(m => m.Id == "laden");
    private static AbilityMode AbbrechenMode(Hero bogenschuetze) => bogenschuetze.Definition.Ability.Modes!.Single(m => m.Id == "abbrechen");

    [Fact]
    public void Bogenschuetze_SchadenSteigtProLaderundeBisZumKonfiguriertenMaximum()
    {
        var state = NewState();
        var archer = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Bogenschuetze);
        state.AddUnit(archer);
        var laden = LadenMode(archer);

        AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, laden);
        Assert.Equal(5, EffectiveStats.FindNextAttackBonus(archer, state)!.BonusDamageForNextAttack);

        AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, laden);
        Assert.Equal(10, EffectiveStats.FindNextAttackBonus(archer, state)!.BonusDamageForNextAttack);

        for (int i = 0; i < 5; i++)
        {
            AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, laden);
        }

        Assert.Equal(20, EffectiveStats.FindNextAttackBonus(archer, state)!.BonusDamageForNextAttack); // gedeckelt
    }

    [Fact]
    public void Bogenschuetze_IstWaehrendDesLadensBewegungsunfaehig()
    {
        var state = NewState();
        var archer = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Bogenschuetze);
        state.AddUnit(archer);

        Assert.NotEmpty(archer.GetLegalMoves(state));
        AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, LadenMode(archer));
        Assert.Empty(archer.GetLegalMoves(state));
    }

    [Fact]
    public void Bogenschuetze_AbbrechenIstNurWaehlbarWaehrendDesLadensUndLoeschtDieLadungOhneWirkung()
    {
        var state = NewState();
        var archer = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Bogenschuetze);
        state.AddUnit(archer);

        Assert.DoesNotContain(AbilitySystem.GetLegalAbilityUses(archer, state), o => o.Mode?.Id == "abbrechen");

        AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, LadenMode(archer));
        Assert.Contains(AbilitySystem.GetLegalAbilityUses(archer, state), o => o.Mode?.Id == "abbrechen");

        Assert.True(AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, AbbrechenMode(archer)));

        Assert.Null(EffectiveStats.FindNextAttackBonus(archer, state));
        Assert.NotEmpty(archer.GetLegalMoves(state)); // Bewegungsunfähigkeit endet mit der Ladung
    }

    [Fact]
    public void Bogenschuetze_LadungWirdBeimAngriffVerbrauchtUndErhoehtDenSchaden()
    {
        var state = NewState();
        var archer = NewHero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Bogenschuetze);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 3), speed: 10); // Distanz 2, innerhalb Reichweite 3
        state.AddUnit(archer);
        state.AddUnit(enemy);
        AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, LadenMode(archer));

        bool attacked = CombatSystem.TryAttackDetailed(archer, enemy, state, out _, out int damageDealt);

        Assert.True(attacked);
        Assert.Equal(archer.BaseAttackDamage + 5, damageDealt);
        Assert.Null(EffectiveStats.FindNextAttackBonus(archer, state)); // Ladung verbraucht
        Assert.NotEmpty(archer.GetLegalMoves(state)); // ... und damit auch die Bewegungsunfähigkeit vorbei
    }
}
