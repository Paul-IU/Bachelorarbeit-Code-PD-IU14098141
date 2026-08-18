using TacticsCore.Board;
using TacticsCore.Combat;
using TacticsCore.Hex;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class CombatSystemTests
{
    private static GameState NewState() => new(new GameBoard());

    [Fact]
    public void TryAttack_ReduziertHpDesZielsUmGenau10()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var target = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        state.AddUnit(attacker);
        state.AddUnit(target);

        bool result = attacker.TryAttack(target, state);

        Assert.True(result);
        Assert.Equal(40, target.CurrentHitPoints);
    }

    [Fact]
    public void FuenfAngriffe_BesiegenEineEinheitMit50Hp_DanachIstSieEntfernt()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var target = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        state.AddUnit(attacker);
        state.AddUnit(target);

        for (int i = 0; i < 4; i++)
        {
            Assert.True(attacker.TryAttack(target, state));
            Assert.False(target.IsDefeated);
        }

        Assert.True(attacker.TryAttack(target, state)); // 5. Treffer: 50 - 5*10 = 0

        Assert.True(target.IsDefeated);
        Assert.Equal(0, target.CurrentHitPoints);
        Assert.DoesNotContain(target, state.Units);
        Assert.DoesNotContain(target, state.GetUnitsAt(target.Position));
    }

    [Fact]
    public void ApplyDamage_KlemmtDieHpBei0Fest_FallenNichtInsNegative()
    {
        // Direkter Test des Clamping-Invariants über den internal-Zugriff
        // (InternalsVisibleTo) - mit dem fixen Grundschaden von 10 auf 50 HP
        // lässt sich ein Overkill-Treffer über CombatSystem allein nicht
        // erzeugen (5*10 trifft exakt auf 0), daher hier isoliert geprüft.
        var target = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));

        target.ApplyDamage(9999);

        Assert.Equal(0, target.CurrentHitPoints);
        Assert.True(target.IsDefeated);
    }

    [Fact]
    public void GegnerAufDemselbenFeld_IstEinGueltigesAngriffsziel()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var enemy = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 0)); // dasselbe Feld
        state.AddUnit(attacker);
        state.AddUnit(enemy);

        bool result = attacker.TryAttack(enemy, state);

        Assert.True(result);
        Assert.Equal(40, enemy.CurrentHitPoints);
    }

    [Fact]
    public void AngriffAufDistanz2_SchlaegtFehl()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var farEnemy = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 2)); // Distanz 2
        state.AddUnit(attacker);
        state.AddUnit(farEnemy);
        Assert.Equal(2, HexCoordinate.Distance(attacker.Position, farEnemy.Position));

        bool result = attacker.TryAttack(farEnemy, state);

        Assert.False(result);
        Assert.Equal(farEnemy.MaxHitPoints, farEnemy.CurrentHitPoints);
    }

    [Fact]
    public void AngriffAufEinenVerbuendeten_SchlaegtFehl()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var ally = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 1));
        state.AddUnit(attacker);
        state.AddUnit(ally);

        bool result = attacker.TryAttack(ally, state);

        Assert.False(result);
        Assert.Equal(ally.MaxHitPoints, ally.CurrentHitPoints);
    }

    [Fact]
    public void AngriffAufEinLeeresFeld_SchlaegtFehl()
    {
        // "Leeres Feld" heißt hier: das übergebene Ziel steht laut GameState
        // gar nicht auf dem Feld, weil es nie über AddUnit registriert
        // wurde. GetValidAttackTargets kennt es dann nicht - der Angriff
        // schlägt fehl, obwohl die Koordinate selbst im Angriffsmuster läge.
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(attacker);
        var neverPlaced = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));

        bool result = attacker.TryAttack(neverPlaced, state);

        Assert.False(result);
    }

    [Fact]
    public void EineBesiegteEinheit_IstKeinGueltigesZielMehr()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var target = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        state.AddUnit(attacker);
        state.AddUnit(target);

        for (int i = 0; i < 5; i++)
        {
            attacker.TryAttack(target, state);
        }
        Assert.True(target.IsDefeated);

        bool secondAttempt = attacker.TryAttack(target, state);

        Assert.False(secondAttempt);
    }

    [Fact]
    public void EinFehlgeschlagenerAngriff_AendertWederHpNochPosition()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var ally = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 1)); // ungültiges Ziel (Verbündeter)
        state.AddUnit(attacker);
        state.AddUnit(ally);

        int hpBefore = ally.CurrentHitPoints;
        var positionBefore = ally.Position;

        Assert.False(attacker.TryAttack(ally, state));

        Assert.Equal(hpBefore, ally.CurrentHitPoints);
        Assert.Equal(positionBefore, ally.Position);
    }

    [Fact]
    public void ZweiGegnerAufEinemFeld_NurDerGewaehlteWirdGetroffen()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var enemy1 = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        var enemy2 = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1)); // gleiches Feld
        state.AddUnit(attacker);
        state.AddUnit(enemy1);
        state.AddUnit(enemy2);

        bool result = attacker.TryAttack(enemy1, state);

        Assert.True(result);
        Assert.Equal(40, enemy1.CurrentHitPoints);
        Assert.Equal(enemy2.MaxHitPoints, enemy2.CurrentHitPoints);
    }

    [Fact]
    public void GetAttackPatternFields_EnthaeltEigenesFeldUndAlleSechsNachbarn()
    {
        var state = NewState();
        // (0,1) ist ein Innenfeld mit vollen 6 Nachbarn.
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 1));
        state.AddUnit(attacker);

        var fields = CombatSystem.GetAttackPatternFields(attacker, state);

        Assert.Equal(7, fields.Count); // eigenes Feld + 6 Nachbarn
        Assert.Contains(new HexCoordinate(0, 1), fields);
    }
}
