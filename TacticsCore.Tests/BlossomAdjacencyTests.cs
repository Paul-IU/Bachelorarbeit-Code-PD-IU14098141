using TacticsCore.Board;
using TacticsCore.Hex;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Dokumentiert explizit, an welchen Stellen die beiden Blüten und die
/// Verbindungsfelder aneinandergrenzen. Das sind die einzigen Übergänge,
/// über die Einheiten von einer Spielfeldhälfte in die andere gelangen
/// können - deshalb hier bewusst als eigener, lesbarer Test festgehalten
/// und nicht nur implizit über generische Nachbarschaftstests geprüft.
/// </summary>
public class BlossomAdjacencyTests
{
    private static readonly GameBoard Board = new();

    /// <summary>
    /// Für jedes Feld von Blüte A: die Menge seiner Nachbarn, die NICHT
    /// zu Blüte A gehören (also Verbindungsfeld oder Blüte B). Das ist
    /// die vollständige "Grenze" von Blüte A zum Rest des Spielfelds.
    /// </summary>
    public static IEnumerable<object[]> BlossomAFieldsWithExpectedCrossBorderNeighbors()
    {
        // Feld                  -> erwartete Nachbarn außerhalb von Blüte A
        yield return new object[] { new HexCoordinate(0, 0), new HexCoordinate[] { } };
        yield return new object[] { new HexCoordinate(0, 1), new HexCoordinate[] { } };
        yield return new object[]
        {
            new HexCoordinate(0, 2),
            new[]
            {
                new HexCoordinate(-1, 3), // Verbindungsfeld links
                new HexCoordinate(1, 2),  // Verbindungsfeld rechts
                new HexCoordinate(0, 3),  // direkter Übergang zu Blüte B in der Mittelspalte
            },
        };
        yield return new object[] { new HexCoordinate(-1, 1), new HexCoordinate[] { } };
        yield return new object[]
        {
            new HexCoordinate(-1, 2),
            new[] { new HexCoordinate(-1, 3) }, // Verbindungsfeld links
        };
        yield return new object[] { new HexCoordinate(1, 0), new HexCoordinate[] { } };
        yield return new object[]
        {
            new HexCoordinate(1, 1),
            new[] { new HexCoordinate(1, 2) }, // Verbindungsfeld rechts
        };
    }

    [Theory]
    [MemberData(nameof(BlossomAFieldsWithExpectedCrossBorderNeighbors))]
    public void BlueteA_GrenztNurAnDenDokumentiertenStellenAnConnectorOderBlueteB(
        HexCoordinate field, HexCoordinate[] expectedCrossBorderNeighbors)
    {
        var actual = Board.GetNeighbors(field)
            .Where(n => n.Type != FieldType.BlossomA)
            .Select(n => n.Coordinate)
            .ToHashSet();

        Assert.Equal(expectedCrossBorderNeighbors.ToHashSet(), actual);
    }

    /// <summary>Spiegelbildlich zu Blüte A: die Grenze von Blüte B zum Rest des Spielfelds.</summary>
    public static IEnumerable<object[]> BlossomBFieldsWithExpectedCrossBorderNeighbors()
    {
        yield return new object[]
        {
            new HexCoordinate(0, 3),
            new[]
            {
                new HexCoordinate(-1, 3), // Verbindungsfeld links (selbst kein BlossomB, hier zählt nur "nicht B")
                new HexCoordinate(1, 2),  // Verbindungsfeld rechts
                new HexCoordinate(0, 2),  // direkter Übergang zu Blüte A in der Mittelspalte
            },
        };
        yield return new object[] { new HexCoordinate(0, 4), new HexCoordinate[] { } };
        yield return new object[] { new HexCoordinate(0, 5), new HexCoordinate[] { } };
        yield return new object[]
        {
            new HexCoordinate(-1, 4),
            new[] { new HexCoordinate(-1, 3) },
        };
        yield return new object[] { new HexCoordinate(-1, 5), new HexCoordinate[] { } };
        yield return new object[]
        {
            new HexCoordinate(1, 3),
            new[] { new HexCoordinate(1, 2) },
        };
        yield return new object[] { new HexCoordinate(1, 4), new HexCoordinate[] { } };
    }

    [Theory]
    [MemberData(nameof(BlossomBFieldsWithExpectedCrossBorderNeighbors))]
    public void BlueteB_GrenztNurAnDenDokumentiertenStellenAnConnectorOderBlueteA(
        HexCoordinate field, HexCoordinate[] expectedCrossBorderNeighbors)
    {
        var actual = Board.GetNeighbors(field)
            .Where(n => n.Type != FieldType.BlossomB)
            .Select(n => n.Coordinate)
            .ToHashSet();

        Assert.Equal(expectedCrossBorderNeighbors.ToHashSet(), actual);
    }

    [Fact]
    public void VerbindungsfeldLinks_BrueckeZwischenBlueteAUndBlueteBAmAeusserenRand()
    {
        var neighbors = Board.GetNeighbors(new HexCoordinate(-1, 3)).ToList();

        var aSide = neighbors.Where(n => n.Type == FieldType.BlossomA).Select(n => n.Coordinate).ToHashSet();
        var bSide = neighbors.Where(n => n.Type == FieldType.BlossomB).Select(n => n.Coordinate).ToHashSet();

        Assert.Equal(new HashSet<HexCoordinate> { new(0, 2), new(-1, 2) }, aSide);
        Assert.Equal(new HashSet<HexCoordinate> { new(0, 3), new(-1, 4) }, bSide);
    }

    [Fact]
    public void VerbindungsfeldRechts_BrueckeZwischenBlueteAUndBlueteBAmAeusserenRand()
    {
        var neighbors = Board.GetNeighbors(new HexCoordinate(1, 2)).ToList();

        var aSide = neighbors.Where(n => n.Type == FieldType.BlossomA).Select(n => n.Coordinate).ToHashSet();
        var bSide = neighbors.Where(n => n.Type == FieldType.BlossomB).Select(n => n.Coordinate).ToHashSet();

        Assert.Equal(new HashSet<HexCoordinate> { new(1, 1), new(0, 2) }, aSide);
        Assert.Equal(new HashSet<HexCoordinate> { new(0, 3), new(1, 3) }, bSide);
    }

    [Fact]
    public void DieBeidenVerbindungsfelder_SindSelbstKeineDirektenNachbarn()
    {
        // Die beiden Connector-Felder liegen sich in der Mitte gegenüber,
        // sind aber selbst zwei Schritte voneinander entfernt - die einzige
        // direkte Verbindung zwischen den Außenspalten läuft jeweils über
        // die angrenzenden Blütenfelder, nicht über die Connector direkt.
        var left = new HexCoordinate(-1, 3);
        var right = new HexCoordinate(1, 2);

        Assert.False(left.IsNeighborOf(right));
        Assert.Equal(2, HexCoordinate.Distance(left, right));
    }

    [Fact]
    public void Mittelspalte_HatKeinEigenesVerbindungsfeld_LetztesFeldVonAUndErstesFeldVonBSindDirekteNachbarn()
    {
        var lastFieldOfA = new HexCoordinate(0, 2);
        var firstFieldOfB = new HexCoordinate(0, 3);

        Assert.True(lastFieldOfA.IsNeighborOf(firstFieldOfB));
    }
}
