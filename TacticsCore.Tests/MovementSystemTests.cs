using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Movement;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class MovementSystemTests
{
    private static GameState NewState() => new(new GameBoard());

    // ---------------------------------------------------------------
    // Reichweite 2 auf leerem Feld, von unterschiedlichen Startpositionen
    // aus. Die erwarteten Feldanzahlen wurden von Hand über die
    // tatsächliche Nachbarschaftsstruktur der 16 Felder nachgerechnet
    // (nicht über die reine Hex-Distanzformel, da das Spielfeld - anders
    // als ein unendliches Hexraster - Lücken/Ränder hat):
    //
    //   (0,1)  "Feldmitte":       Innenfeld von Blüte A mit vollen 6
    //                             Nachbarn -> 1 (Start) + 6 (Distanz 1)
    //                             + 3 (Distanz 2) = 10 Felder.
    //   (0,0)  "Randfeld":        Eckfeld mit nur 3 Nachbarn -> 1 + 3 + 3
    //                             = 7 Felder.
    //   (-1,3) "Verbindungsfeld": Knotenpunkt mit 4 Nachbarn, der in 2
    //                             Schritten beide Blüten UND die andere
    //                             Verbindungsseite erreicht -> 1 + 4 + 7
    //                             = 12 Felder (die höchste Reichweiten-
    //                             Abdeckung aller 16 Felder - der
    //                             Verbindungsbereich ist also auch für
    //                             die Mobilität der zentrale Punkt des
    //                             Spielfelds, nicht nur geometrisch).
    // ---------------------------------------------------------------
    [Theory]
    [InlineData(0, 1, 10)]
    [InlineData(0, 0, 7)]
    [InlineData(-1, 3, 12)]
    public void GetReachableFields_LiefertVonVerschiedenenStartpositionenDieErwarteteAnzahl(int q, int r, int expectedCount)
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(q, r));
        state.AddUnit(unit);

        var reachable = MovementSystem.GetReachableFields(unit, state);

        Assert.Equal(expectedCount, reachable.Count);
        // Das Startfeld selbst gilt als erreichbar mit 0 Schritten.
        Assert.Contains(reachable, rf => rf.Coordinate == unit.Position && rf.Steps == 0);
    }

    [Fact]
    public void GetReachableFields_SchrittzahlEntsprichtDerTatsaechlichenPfadlaenge()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(unit);

        var reachable = MovementSystem.GetReachableFields(unit, state);

        var oneStep = Assert.Single(reachable, rf => rf.Coordinate == new HexCoordinate(0, 1));
        Assert.Equal(1, oneStep.Steps);

        var twoSteps = Assert.Single(reachable, rf => rf.Coordinate == new HexCoordinate(0, 2));
        Assert.Equal(2, twoSteps.Steps);
    }

    [Fact]
    public void GegnerAufDemWeg_VerhindertDasErreichenDesDahinterliegendenFeldesNicht()
    {
        // Belegungsregel: Einheiten blockieren die Bewegung grundsätzlich
        // nicht (OccupancyRules.UnitsBlockMovement == false) - weder
        // verbündete noch gegnerische. (0,1) liegt direkt auf dem Weg von
        // (0,0) nach (0,2).
        var state = NewState();
        var mover = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var blocker = new Unit(Guid.NewGuid(), Team.TeamB, new HexCoordinate(0, 1));
        state.AddUnit(mover);
        state.AddUnit(blocker);

        var reachable = MovementSystem.GetReachableFields(mover, state);

        Assert.Contains(reachable, rf => rf.Coordinate == new HexCoordinate(0, 2) && rf.Steps == 2);
        // Die Belegung ändert nichts an der insgesamt erreichbaren Feldanzahl.
        Assert.Equal(7, reachable.Count);
    }

    [Theory]
    [InlineData(true)]  // Verbündete dürfen das Zielfeld belegen
    [InlineData(false)] // Gegnerische Einheiten ebenso
    public void TryMove_AufBereitsBesetztesFeld_IstGueltig_StapelnErlaubt(bool occupantIsAlly)
    {
        var state = NewState();
        var mover = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        var occupant = new Unit(Guid.NewGuid(), occupantIsAlly ? Team.TeamA : Team.TeamB, new HexCoordinate(0, 1));
        state.AddUnit(mover);
        state.AddUnit(occupant);

        bool result = mover.TryMove(new HexCoordinate(0, 1), state);

        Assert.True(result);
        Assert.Equal(new HexCoordinate(0, 1), mover.Position);
        var unitsOnTarget = state.GetUnitsAt(new HexCoordinate(0, 1));
        Assert.Equal(2, unitsOnTarget.Count);
        Assert.Contains(mover, unitsOnTarget);
        Assert.Contains(occupant, unitsOnTarget);
    }

    [Fact]
    public void TryMove_UeberDenSpielfeldrandHinaus_SchlaegtFehlUndAendertNichts()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(unit);
        var outside = new HexCoordinate(2, 0);
        Assert.False(state.Board.Contains(outside));

        bool result = unit.TryMove(outside, state);

        Assert.False(result);
        Assert.Equal(new HexCoordinate(0, 0), unit.Position);
        Assert.False(state.IsOccupied(outside));
    }

    [Fact]
    public void Stehenbleiben_IstEinLegalerZug()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(unit);

        bool result = unit.TryMove(unit.Position, state);

        Assert.True(result);
        Assert.Equal(new HexCoordinate(0, 0), unit.Position);
        Assert.Contains(unit.GetLegalMoves(state), c => c == new HexCoordinate(0, 0));
    }

    [Fact]
    public void GetShortestPath_LiefertPfadInklusiveStartUndZielMitKorrekterLaenge()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(unit);

        var path = MovementSystem.GetShortestPath(unit, new HexCoordinate(0, 2), state);

        Assert.Equal(new[] { new HexCoordinate(0, 0), new HexCoordinate(0, 1), new HexCoordinate(0, 2) }, path);
    }

    [Fact]
    public void GetShortestPath_ZuUnerreichbaremZiel_LiefertLeereListe()
    {
        var state = NewState();
        var unit = new Unit(Guid.NewGuid(), Team.TeamA, new HexCoordinate(0, 0));
        state.AddUnit(unit);

        // (0,3) liegt in Distanz 3 - außerhalb der Reichweite 2.
        var path = MovementSystem.GetShortestPath(unit, new HexCoordinate(0, 3), state);

        Assert.Empty(path);
    }
}
