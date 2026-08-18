namespace TacticsCore.Hex;

/// <summary>
/// Axiale Koordinate (q, r) für ein Hexfeld in "flat-top"-Ausrichtung
/// (die Sechsecke haben eine flache Kante oben/unten, nicht eine Spitze).
///
/// Achsen-Konvention:
///   - q wächst nach rechts (Spalte).
///   - r wächst schräg nach unten-links innerhalb einer Spalte.
///   - Die dritte, implizite Würfelkoordinate ist s = -q - r.
///
/// Der Typ ist unveränderlich (immutable) und wertbasiert (readonly struct
/// mit Equals/GetHashCode), damit Instanzen direkt als Dictionary-Key oder
/// HashSet-Element verwendet werden können - das ist für ein Spielfeld,
/// das Felder über ihre Koordinate nachschlägt, essenziell.
/// </summary>
public readonly struct HexCoordinate : IEquatable<HexCoordinate>
{
    public int Q { get; }
    public int R { get; }

    public HexCoordinate(int q, int r)
    {
        Q = q;
        R = r;
    }

    /// <summary>
    /// Die sechs Nachbarrichtungen für flat-top-Hexfelder in axialen Koordinaten,
    /// im Uhrzeigersinn beginnend bei "rechts". Diese sechs Offsets sind die
    /// einzige Stelle, an der die Nachbarschaftsgeometrie definiert ist -
    /// alle anderen Nachbarschaftsabfragen im Projekt bauen darauf auf.
    /// </summary>
    public static readonly IReadOnlyList<HexCoordinate> NeighborDirections = new[]
    {
        new HexCoordinate(+1, 0),  // rechts
        new HexCoordinate(+1, -1), // rechts-oben
        new HexCoordinate(0, -1),  // links-oben
        new HexCoordinate(-1, 0),  // links
        new HexCoordinate(-1, +1), // links-unten
        new HexCoordinate(0, +1),  // rechts-unten
    };

    /// <summary>Addiert zwei axiale Koordinaten (komponentenweise).</summary>
    public HexCoordinate Add(HexCoordinate other) => new(Q + other.Q, R + other.R);

    /// <summary>
    /// Liefert die sechs Koordinaten, die geometrisch an dieses Feld angrenzen.
    /// Ob dort tatsächlich ein Feld des Spielfelds existiert, prüft diese
    /// Methode bewusst NICHT - das ist Aufgabe von <see cref="Board.Board"/>.
    /// </summary>
    public IEnumerable<HexCoordinate> GetNeighbors()
    {
        foreach (var direction in NeighborDirections)
        {
            yield return Add(direction);
        }
    }

    /// <summary>Prüft, ob zwei Koordinaten direkte Nachbarn sind (Distanz genau 1).</summary>
    public static bool AreNeighbors(HexCoordinate a, HexCoordinate b) => Distance(a, b) == 1;

    /// <summary>Prüft, ob <paramref name="other"/> ein direkter Nachbar dieser Koordinate ist.</summary>
    public bool IsNeighborOf(HexCoordinate other) => AreNeighbors(this, other);

    /// <summary>
    /// Hex-Distanz zwischen zwei axialen Koordinaten (Anzahl der mindestens
    /// nötigen Schritte über Nachbarfelder). Standardformel über die
    /// Würfelkoordinaten-Darstellung: dist = (|dq| + |dr| + |dq+dr|) / 2.
    /// </summary>
    public static int Distance(HexCoordinate a, HexCoordinate b)
    {
        int dq = a.Q - b.Q;
        int dr = a.R - b.R;
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
    }

    /// <summary>Hex-Distanz von dieser Koordinate zu <paramref name="other"/>.</summary>
    public int DistanceTo(HexCoordinate other) => Distance(this, other);

    public bool Equals(HexCoordinate other) => Q == other.Q && R == other.R;

    public override bool Equals(object? obj) => obj is HexCoordinate other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Q, R);

    public static bool operator ==(HexCoordinate left, HexCoordinate right) => left.Equals(right);

    public static bool operator !=(HexCoordinate left, HexCoordinate right) => !left.Equals(right);

    public override string ToString() => $"({Q},{R})";
}
