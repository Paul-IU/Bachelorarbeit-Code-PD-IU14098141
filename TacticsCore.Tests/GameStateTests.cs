using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests für die reine Zustandsverwaltung von GameState (Platzieren,
/// Belegungs-Buchführung) - unabhängig von Bewegungs- oder Kampfregeln,
/// die in MovementSystemTests bzw. CombatSystemTests geprüft werden.
/// </summary>
public class GameStateTests
{
    private static GameState NewState() => new(new GameBoard());

    [Fact]
    public void AddUnit_AufFeldAusserhalbDesSpielfelds_SchlaegtFehl()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(5, 5));

        Assert.False(state.AddUnit(unit));
        Assert.Empty(state.Units);
    }

    [Fact]
    public void AddUnit_DieselbeEinheitZweimal_SchlaegtBeimZweitenMalFehl()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));

        Assert.True(state.AddUnit(unit));
        Assert.False(state.AddUnit(unit));
        Assert.Single(state.Units);
    }

    [Fact]
    public void AddUnit_MehrereEinheitenAufGleichemFeld_IstErlaubtOhneStapellimit()
    {
        // Belegungsregel: kein Stapellimit in der Grundregel (siehe
        // OccupancyRules.MaxUnitsPerField == null).
        var state = NewState();
        var first = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var second = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 0));

        Assert.True(state.AddUnit(first));
        Assert.True(state.AddUnit(second));
        Assert.Equal(2, state.GetUnitsAt(new HexCoordinate(0, 0)).Count);
    }

    [Fact]
    public void GetUnitsAt_AufLeeremFeld_LiefertLeereListe()
    {
        var state = NewState();

        Assert.Empty(state.GetUnitsAt(new HexCoordinate(0, 0)));
        Assert.False(state.IsOccupied(new HexCoordinate(0, 0)));
    }
}
