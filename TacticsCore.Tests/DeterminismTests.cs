using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Movement;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Prüft explizit die für die spätere Massen-Simulation zwingende
/// Determinismus-Anforderung: alle zurückgegebenen Listen (Bewegungsziele,
/// Angriffsziele, erreichbare Felder) müssen bei identischem Spielzustand
/// bei jedem Aufruf exakt gleich sortiert sein. Da mehrere Einheiten auf
/// demselben Feld stehen können, reicht eine Sortierung nach (q, r) für
/// Einheiten nicht aus - dort wird zusätzlich nach UnitId sortiert.
/// </summary>
public class DeterminismTests
{
    private static GameState NewState() => new(new GameBoard());

    [Fact]
    public void GetLegalMoves_IstBeiWiederholtenAufrufenImmerIdentischSortiert()
    {
        var state = NewState();
        // Verbindungsfeld: liefert mit 12 erreichbaren Feldern die größte
        // und damit aussagekräftigste Liste für diesen Test.
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(-1, 3));
        state.AddUnit(unit);

        var first = unit.GetLegalMoves(state);
        var second = unit.GetLegalMoves(state);

        Assert.Equal(first, second);

        // Explizit nach (q, r) sortiert.
        var expectedOrder = first.OrderBy(c => c.Q).ThenBy(c => c.R).ToList();
        Assert.Equal(expectedOrder, first);
    }

    [Fact]
    public void GetReachableFields_IstBeiWiederholtenAufrufenImmerIdentischSortiert()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 1));
        state.AddUnit(unit);

        var first = MovementSystem.GetReachableFields(unit, state);
        var second = MovementSystem.GetReachableFields(unit, state);

        Assert.Equal(first, second);

        var expectedOrder = first.OrderBy(rf => rf.Coordinate.Q).ThenBy(rf => rf.Coordinate.R).ToList();
        Assert.Equal(expectedOrder, first);
    }

    [Fact]
    public void GetLegalAttacks_IstBeiWiederholtenAufrufenImmerIdentischSortiert()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var enemy1 = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        var enemy2 = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(-1, 1));
        state.AddUnit(attacker);
        state.AddUnit(enemy1);
        state.AddUnit(enemy2);

        var first = attacker.GetLegalAttacks(state);
        var second = attacker.GetLegalAttacks(state);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ZweiEinheitenAufDemselbenFeld_WerdenReproduzierbarNachUnitIdGeordnet()
    {
        var state = NewState();
        var attacker = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));

        // Bewusst feste, kleine GUIDs statt Guid.NewGuid(): Guid.CompareTo
        // vergleicht seine Felder in fester Reihenfolge, beginnend mit dem
        // ersten int-Feld - hier unterscheidet sich nur dieses erste Feld
        // (1 vs. 2), die erwartete Reihenfolge ist damit unzweideutig und
        // für den Test nachvollziehbar, statt sich auf zufällige GUIDs zu
        // verlassen.
        var smallerIdUnit = new Unit(new Guid(1, 0, 0, new byte[8]), Team.TeamB, new HexCoordinate(0, 1));
        var largerIdUnit = new Unit(new Guid(2, 0, 0, new byte[8]), Team.TeamB, new HexCoordinate(0, 1));
        Assert.True(smallerIdUnit.Id.CompareTo(largerIdUnit.Id) < 0);

        // Bewusst in "falscher" Reihenfolge registriert, um zu zeigen, dass
        // die Sortierung nicht von der Registrierungsreihenfolge abhängt,
        // sondern ausschließlich von (q, r, UnitId).
        state.AddUnit(attacker);
        state.AddUnit(largerIdUnit);
        state.AddUnit(smallerIdUnit);

        var targets = attacker.GetLegalAttacks(state);

        Assert.Equal(new[] { smallerIdUnit, largerIdUnit }, targets);
    }
}
