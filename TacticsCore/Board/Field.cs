using TacticsCore.Hex;

namespace TacticsCore.Board;

/// <summary>
/// Ein einzelnes Feld des Spielfelds. Felder sind unveränderlich - ihre
/// Koordinate, ihr Typ und ob sie ein Startfeld sind, stehen mit der
/// Erzeugung fest und ändern sich über die Laufzeit einer Simulation nie.
/// (Ob ein Feld aktuell durch eine Einheit besetzt ist, ist kein Zustand
/// des Feldes selbst, sondern wird von <see cref="TacticsCore.GameState"/>
/// verwaltet - siehe Kommentar dort.)
/// </summary>
public sealed class Field
{
    public HexCoordinate Coordinate { get; }
    public FieldType Type { get; }

    /// <summary>
    /// Markiert eines der drei Felder je Blüte, auf denen die Einheiten
    /// eines Teams die Partie beginnen. Siehe <see cref="Board"/> für die
    /// Herleitung, welche Felder das sind.
    /// </summary>
    public bool IsStartField { get; }

    public Field(HexCoordinate coordinate, FieldType type, bool isStartField)
    {
        Coordinate = coordinate;
        Type = type;
        IsStartField = isStartField;
    }

    public override string ToString() => $"{Coordinate} [{Type}{(IsStartField ? ", Start" : "")}]";
}
