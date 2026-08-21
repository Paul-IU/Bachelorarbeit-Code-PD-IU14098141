using System;
using Godot;
using TacticsCore.Hex;

namespace TacticsCoreGodot.Shared;

/// <summary>
/// Umrechnung zwischen den axialen Spiellogik-Koordinaten
/// (<see cref="HexCoordinate"/>) und Bildschirmkoordinaten.
///
/// ORIENTIERUNG GEPRÜFT, NICHT GERATEN: TacticsCore.Hex.HexCoordinate
/// dokumentiert ausdrücklich "flat-top"-Hexfelder ("q wächst nach rechts",
/// "die sechs Nachbarrichtungen für flat-top-Hexfelder") - siehe auch
/// Board.Board ("Bei flat-top-Hexfeldern gilt ... y = r + q/2"). Die
/// Formeln hier sind daher bewusst die flat-top-Variante:
///   x = größe * 3/2 * q
///   y = größe * sqrt(3) * (r + q/2)
/// </summary>
public static class BoardGeometry
{
    /// <summary>Radius eines Hexfelds (Mitte zu Ecke) in Pixeln - rein optisch, hat mit der Spiellogik nichts zu tun.</summary>
    public const float HexSize = 90f;

    public static Vector2 HexToPixel(HexCoordinate coordinate, float size = HexSize) => new(
        size * 1.5f * coordinate.Q,
        size * Mathf.Sqrt(3f) * (coordinate.R + coordinate.Q / 2f));

    /// <summary>
    /// Rückrichtung: Mausposition (bereits in den lokalen Koordinatenraum
    /// des Spielfelds umgerechnet) auf das nächstgelegene Hexfeld abbilden.
    /// Löst zunächst nach gebrochenen axialen Koordinaten auf (algebraische
    /// Umkehrung von <see cref="HexToPixel"/>) und rundet anschließend über
    /// die Würfelkoordinaten-Darstellung auf ein tatsächliches Hexfeld -
    /// Standardverfahren für Hex-Raster (siehe z.B.
    /// redblobgames.com/grids/hexagons/#pixel-to-hex). Bewusst unabhängig
    /// von TacticsCore.Hex.HexLine neu geschrieben: das ist reine
    /// Bildschirm-Eingabe-Geometrie, keine Spielregel, die dadurch
    /// dupliziert würde.
    /// </summary>
    public static HexCoordinate PixelToHex(Vector2 localPixel, float size = HexSize)
    {
        double q = (2.0 / 3.0) * localPixel.X / size;
        double r = (-1.0 / 3.0) * localPixel.X / size + (Math.Sqrt(3.0) / 3.0) * localPixel.Y / size;
        return RoundToHex(q, r);
    }

    private static HexCoordinate RoundToHex(double qFrac, double rFrac)
    {
        double x = qFrac;
        double z = rFrac;
        double y = -x - z;

        int rx = (int)Math.Round(x);
        int ry = (int)Math.Round(y);
        int rz = (int)Math.Round(z);

        double xDiff = Math.Abs(rx - x);
        double yDiff = Math.Abs(ry - y);
        double zDiff = Math.Abs(rz - z);

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

    /// <summary>Die sechs Eckpunkte eines flat-top-Hexfelds um seinen Mittelpunkt, für das Zeichnen als Polygon.</summary>
    public static Vector2[] HexCorners(Vector2 center, float size = HexSize)
    {
        var corners = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            // Flat-top: die erste Ecke liegt bei 0°, danach je 60° weiter.
            double angleDeg = 60.0 * i;
            double angleRad = Math.PI / 180.0 * angleDeg;
            corners[i] = center + new Vector2(size * (float)Math.Cos(angleRad), size * (float)Math.Sin(angleRad));
        }
        return corners;
    }
}
