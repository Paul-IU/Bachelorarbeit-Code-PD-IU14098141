using TacticsCore.Board;
using TacticsCore.Hex;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class BoardTests
{
    [Fact]
    public void Board_EnthaeltGenau16Felder()
    {
        var board = new GameBoard();

        Assert.Equal(16, board.Fields.Count);
    }

    [Theory]
    // Blüte A
    [InlineData(0, 0, FieldType.BlossomA)]
    [InlineData(0, 1, FieldType.BlossomA)]
    [InlineData(0, 2, FieldType.BlossomA)]
    [InlineData(-1, 1, FieldType.BlossomA)]
    [InlineData(-1, 2, FieldType.BlossomA)]
    [InlineData(1, 0, FieldType.BlossomA)]
    [InlineData(1, 1, FieldType.BlossomA)]
    // Verbindungsfelder
    [InlineData(-1, 3, FieldType.Connector)]
    [InlineData(1, 2, FieldType.Connector)]
    // Blüte B
    [InlineData(0, 3, FieldType.BlossomB)]
    [InlineData(0, 4, FieldType.BlossomB)]
    [InlineData(0, 5, FieldType.BlossomB)]
    [InlineData(-1, 4, FieldType.BlossomB)]
    [InlineData(-1, 5, FieldType.BlossomB)]
    [InlineData(1, 3, FieldType.BlossomB)]
    [InlineData(1, 4, FieldType.BlossomB)]
    public void Board_OrdnetJedemFeldDenRichtigenTypZu(int q, int r, FieldType expectedType)
    {
        var board = new GameBoard();
        var field = board.GetField(new HexCoordinate(q, r));

        Assert.NotNull(field);
        Assert.Equal(expectedType, field!.Type);
    }

    [Theory]
    [InlineData(2, 0)]   // rechts außerhalb
    [InlineData(-2, 3)]  // links außerhalb
    [InlineData(0, 6)]   // unterhalb der Blüte B
    [InlineData(0, -1)]  // oberhalb der Blüte A
    [InlineData(-1, 0)]  // in Spalte q=-1 existierende Reihe, aber r zu klein
    [InlineData(1, 5)]   // in Spalte q=1 existierende Reihe, aber r zu groß
    public void Contains_IstFalseFuerFelderAusserhalbDer16(int q, int r)
    {
        var board = new GameBoard();

        Assert.False(board.Contains(new HexCoordinate(q, r)));
        Assert.Null(board.GetField(new HexCoordinate(q, r)));
    }

    [Fact]
    public void Contains_IstTrueFuerAlleDefiniertenFelder()
    {
        var board = new GameBoard();

        foreach (var field in board.Fields)
        {
            Assert.True(board.Contains(field.Coordinate));
        }
    }

    [Fact]
    public void Startfelder_BlueteA_SindDieDreiVonDerFeldmitteEntferntesten()
    {
        var board = new GameBoard();

        var startFields = board.GetStartFields(FieldType.BlossomA)
            .Select(f => f.Coordinate)
            .ToHashSet();

        Assert.Equal(3, startFields.Count);
        Assert.Contains(new HexCoordinate(0, 0), startFields);
        Assert.Contains(new HexCoordinate(-1, 1), startFields);
        Assert.Contains(new HexCoordinate(1, 0), startFields);
    }

    [Fact]
    public void Startfelder_BlueteB_SindDieDreiVonDerFeldmitteEntferntesten()
    {
        var board = new GameBoard();

        var startFields = board.GetStartFields(FieldType.BlossomB)
            .Select(f => f.Coordinate)
            .ToHashSet();

        Assert.Equal(3, startFields.Count);
        Assert.Contains(new HexCoordinate(0, 5), startFields);
        Assert.Contains(new HexCoordinate(-1, 5), startFields);
        Assert.Contains(new HexCoordinate(1, 4), startFields);
    }

    [Fact]
    public void Startfelder_VerbindungsfelderSindNieStartfelder()
    {
        var board = new GameBoard();

        var connectors = board.Fields.Where(f => f.Type == FieldType.Connector);

        Assert.All(connectors, c => Assert.False(c.IsStartField));
    }

    [Fact]
    public void GetNeighbors_LiefertNurTatsaechlichExistierendeFelder()
    {
        var board = new GameBoard();

        // (0,0) liegt in der Ecke der Blüte A: von den 6 geometrischen
        // Nachbarn existieren nur 3 tatsächlich auf dem 16-Felder-Board.
        var neighbors = board.GetNeighbors(new HexCoordinate(0, 0)).ToList();

        Assert.All(neighbors, n => Assert.True(board.Contains(n.Coordinate)));
        var neighborCoords = neighbors.Select(n => n.Coordinate).ToHashSet();
        Assert.Equal(3, neighborCoords.Count);
        Assert.Contains(new HexCoordinate(0, 1), neighborCoords);
        Assert.Contains(new HexCoordinate(-1, 1), neighborCoords);
        Assert.Contains(new HexCoordinate(1, 0), neighborCoords);
    }
}
