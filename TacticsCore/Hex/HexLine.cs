namespace TacticsCore.Hex;

/// <summary>
/// Berechnet die Hex-Linie zwischen zwei Feldern (die Folge von Feldern,
/// die eine gedachte Sichtlinie zwischen ihnen durchquert) - Grundlage für
/// die Sichtlinienprüfung bei Fernkampf-Angriffen (siehe
/// <see cref="TacticsCore.Combat.CombatSystem"/>: eine Fähigkeit kann ein
/// Feld so markieren, dass Angriffe, deren Sichtlinie durch dieses Feld
/// verläuft, stattdessen den Anwender treffen).
///
/// ALGORITHMUS: Standardverfahren für Hexfelder ("cube line drawing", siehe
/// z.B. redblobgames.com/grids/hexagons/#line-drawing) - beide Endpunkte
/// werden in Würfelkoordinaten (x, y, z mit x+y+z=0) umgerechnet, linear in
/// so vielen Schritten interpoliert, wie die Hex-Distanz beträgt, und jeder
/// Zwischenpunkt auf die nächstliegende gültige Hex-Koordinate gerundet
/// ("cube rounding").
///
/// DETERMINISMUS: Die Berechnung nutzt IEEE-754-double-Arithmetik
/// (Addition, Multiplikation, Rundung) - das ist auf jeder .NET-Zielplattform
/// bitgenau reproduzierbar für dieselben Eingaben, es gibt hier also keine
/// versteckte Plattform- oder Zeitabhängigkeit. Der einzige Sonderfall sind
/// exakte Kantenfälle (die gedachte Linie verläuft genau auf der Grenze
/// zwischen zwei Feldern) - dafür wird die Linie vor der Interpolation um
/// einen winzigen Betrag verschoben (siehe <c>Epsilon</c>), sodass die
/// Rundung IMMER deterministisch auflöst, statt von Gleitkomma-
/// Rundungsartefakten abzuhängen.
///
/// SPIEGELSYMMETRIE (Bachelorarbeit, Korrektur des Seitenvorteils): Früher
/// wurde nur der Startpunkt um einen FESTEN Vektor des Spielfelds
/// verschoben (ε·(+1, +2, −3) in Würfelkoordinaten). Bei einer um 180°
/// gedrehten Linie (Spielfeld-Spiegelung zwischen den Blüten, siehe
/// <see cref="Board.Board.MirrorBetweenBlossoms"/>) zeigte diese Verschiebung
/// damit relativ zur Linie in die andere Richtung, und Kantenfälle lösten
/// sich auf der einen Seite des Spielfelds anders auf als auf der anderen
/// (66 von 240 Feldpaaren). Jetzt wird die GANZE Linie relativ zu ihrer
/// eigenen Richtung d verschoben: quer zur Linie (immer zur selben Seite in
/// Blickrichtung) und zusätzlich minimal in Blickrichtung. Beide Anteile
/// drehen sich mit der Linie mit, die gespiegelte Linie liefert so exakt die
/// gespiegelten Felder (geprüft für alle Feldpaare in HexLineTests).
/// Inhaltlich gilt dieselbe Regel wie bisher: Linie von Mittelpunkt zu
/// Mittelpunkt, jeder Zwischenpunkt auf das nächste Feld gerundet; nur bei
/// exakten Kantenfällen kann ein anderes der beiden gleich nahen Felder
/// gewählt werden.
/// </summary>
public static class HexLine
{
    private const double Epsilon = 1e-6;

    /// <summary>
    /// Liefert die Felder von <paramref name="a"/> bis <paramref name="b"/>
    /// (beide Endpunkte inklusive), in dieser Reihenfolge - Anzahl der
    /// Felder ist immer <c>HexCoordinate.Distance(a, b) + 1</c>.
    /// </summary>
    public static IReadOnlyList<HexCoordinate> Between(HexCoordinate a, HexCoordinate b)
    {
        int distance = HexCoordinate.Distance(a, b);
        if (distance == 0)
        {
            return new[] { a };
        }

        // Würfelkoordinaten: x=q, z=r, y=-x-z (x+y+z=0).
        double ax = a.Q, az = a.R, ay = -ax - az;
        double bx = b.Q, bz = b.R, by = -bx - bz;

        // Verschiebung der ganzen Linie relativ zu ihrer Richtung d (siehe
        // Klassendokumentation, SPIEGELSYMMETRIE): quer = (dy − dz, dz − dx, dx − dy)
        // steht senkrecht auf d, längs = d. Beide Summen bleiben 0 (Würfelkoordinaten),
        // und beide Vektoren wechseln mit d das Vorzeichen.
        double dx = bx - ax, dy = by - ay, dz = bz - az;
        double qx = Epsilon * (dy - dz) + Epsilon * Epsilon * dx;
        double qy = Epsilon * (dz - dx) + Epsilon * Epsilon * dy;
        double qz = Epsilon * (dx - dy) + Epsilon * Epsilon * dz;
        ax += qx; ay += qy; az += qz;
        bx += qx; by += qy; bz += qz;

        var result = new List<HexCoordinate>(distance + 1);
        for (int i = 0; i <= distance; i++)
        {
            double t = (double)i / distance;
            double x = ax + (bx - ax) * t;
            double y = ay + (by - ay) * t;
            double z = az + (bz - az) * t;
            result.Add(RoundToHex(x, y, z));
        }

        return result;
    }

    /// <summary>
    /// Liefert nur die Felder STRIKT ZWISCHEN <paramref name="a"/> und
    /// <paramref name="b"/> (ohne beide Endpunkte) - das sind die Felder,
    /// die als Blockade für eine Sichtlinie zwischen ihnen überhaupt in
    /// Frage kommen. In Reihenfolge von <paramref name="a"/> nach
    /// <paramref name="b"/> - das nächstgelegene potenzielle Hindernis
    /// steht also zuerst.
    /// </summary>
    public static IReadOnlyList<HexCoordinate> StrictlyBetween(HexCoordinate a, HexCoordinate b)
    {
        var line = Between(a, b);
        return line.Count <= 2 ? Array.Empty<HexCoordinate>() : line.Skip(1).SkipLast(1).ToList();
    }

    private static HexCoordinate RoundToHex(double x, double y, double z)
    {
        int rx = (int)Math.Round(x);
        int ry = (int)Math.Round(y);
        int rz = (int)Math.Round(z);

        double xDiff = Math.Abs(rx - x);
        double yDiff = Math.Abs(ry - y);
        double zDiff = Math.Abs(rz - z);

        // Die Koordinate mit der größten Rundungsabweichung wird aus den
        // beiden anderen (bereits gerundeten) rekonstruiert, damit x+y+z=0
        // exakt erhalten bleibt.
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new HexCoordinate(rx, rz);
    }
}
