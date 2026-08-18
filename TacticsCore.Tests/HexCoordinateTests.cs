using TacticsCore.Hex;

namespace TacticsCore.Tests;

public class HexCoordinateTests
{
    [Fact]
    public void GetNeighbors_LiefertGenauDieSechsDefiniertenRichtungen()
    {
        var origin = new HexCoordinate(0, 0);

        var neighbors = origin.GetNeighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Contains(new HexCoordinate(1, 0), neighbors);
        Assert.Contains(new HexCoordinate(1, -1), neighbors);
        Assert.Contains(new HexCoordinate(0, -1), neighbors);
        Assert.Contains(new HexCoordinate(-1, 0), neighbors);
        Assert.Contains(new HexCoordinate(-1, 1), neighbors);
        Assert.Contains(new HexCoordinate(0, 1), neighbors);
    }

    [Theory]
    [InlineData(0, 0, 1, 0)]
    [InlineData(0, 0, 1, -1)]
    [InlineData(0, 0, 0, -1)]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, -1, 1)]
    [InlineData(0, 0, 0, 1)]
    public void AreNeighbors_ErkenntDirekteNachbarnInAllenSechsRichtungen(int q1, int r1, int q2, int r2)
    {
        var a = new HexCoordinate(q1, r1);
        var b = new HexCoordinate(q2, r2);

        Assert.True(HexCoordinate.AreNeighbors(a, b));
        Assert.True(a.IsNeighborOf(b));
        // Nachbarschaft ist symmetrisch.
        Assert.True(b.IsNeighborOf(a));
    }

    [Fact]
    public void AreNeighbors_IstFalseFuerNichtBenachbarteFelder()
    {
        var a = new HexCoordinate(0, 0);
        var b = new HexCoordinate(0, 2); // Distanz 2, kein Nachbar
        var c = new HexCoordinate(0, 0); // identisches Feld, kein Nachbar seiner selbst

        Assert.False(HexCoordinate.AreNeighbors(a, b));
        Assert.False(HexCoordinate.AreNeighbors(a, c));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]   // gleiches Feld
    [InlineData(0, 0, 1, 0, 1)]   // direkter Nachbar
    [InlineData(0, 0, 0, 2, 2)]   // zwei Schritte gerade
    [InlineData(0, 0, -1, 3, 3)]  // Blüte-A-Rand bis zum Verbindungsfeld
    [InlineData(-1, 1, 1, 0, 2)]  // Distanz über zwei Spalten hinweg
    [InlineData(0, 0, 0, 5, 5)]   // von einem Ende des Spielfelds zum anderen
    public void Distance_BerechnetDieKorrekteHexDistanz(int q1, int r1, int q2, int r2, int expected)
    {
        var a = new HexCoordinate(q1, r1);
        var b = new HexCoordinate(q2, r2);

        Assert.Equal(expected, HexCoordinate.Distance(a, b));
        // Distanz ist symmetrisch.
        Assert.Equal(expected, HexCoordinate.Distance(b, a));
        Assert.Equal(expected, a.DistanceTo(b));
    }

    [Fact]
    public void Equals_UndGetHashCode_ErlaubenVerwendungAlsDictionaryKey()
    {
        var key = new HexCoordinate(-1, 3);
        var dict = new Dictionary<HexCoordinate, string>
        {
            [key] = "Verbindungsfeld links",
        };

        // Eine neu erzeugte, aber wertgleiche Koordinate muss denselben Eintrag finden.
        var lookupKey = new HexCoordinate(-1, 3);

        Assert.True(dict.ContainsKey(lookupKey));
        Assert.Equal("Verbindungsfeld links", dict[lookupKey]);
        Assert.Equal(key, lookupKey);
        Assert.Equal(key.GetHashCode(), lookupKey.GetHashCode());
        Assert.True(key == lookupKey);
        Assert.False(key != lookupKey);
    }

    [Fact]
    public void Equals_UnterscheidetVerschiedeneKoordinaten()
    {
        var a = new HexCoordinate(0, 0);
        var b = new HexCoordinate(0, 1);

        Assert.NotEqual(a, b);
        Assert.True(a != b);
        Assert.False(a == b);
    }
}
