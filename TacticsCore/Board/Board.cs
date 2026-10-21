using TacticsCore.Hex;

namespace TacticsCore.Board;

/// <summary>
/// Das Spielfeld: exakt 16 Hexfelder, als feste Daten definiert (nicht
/// prozedural erzeugt). Das erlaubt es, die Geometrie später einfach
/// anzupassen, indem nur diese Liste geändert wird.
///
/// ---------------------------------------------------------------------
/// GEOMETRIE
/// ---------------------------------------------------------------------
/// Das Feld besteht aus drei Spalten (Achse q), die jeweils entlang r
/// aufsteigen:
///
///   Spalte q = -1 ("links"):  5 Felder, r = 1..5
///   Spalte q =  0 ("Mitte"):  6 Felder, r = 0..5
///   Spalte q = +1 ("rechts"): 5 Felder, r = 0..4
///
/// Bei flat-top-Hexfeldern gilt für die Umrechnung axialer Koordinaten in
/// eine vertikale Bildschirmposition näherungsweise y = r + q/2 (siehe
/// Rendering/AsciiRenderer.cs). Damit ergibt sich:
///
///   - Spalte q=-1: y = r - 0.5  → Reihen bei y = 0.5, 1.5, 2.5, 3.5, 4.5
///   - Spalte q=0:  y = r        → Reihen bei y = 0, 1, 2, 3, 4, 5
///   - Spalte q=+1: y = r + 0.5  → Reihen bei y = 0.5, 1.5, 2.5, 3.5, 4.5
///
/// Die äußeren Spalten (q=-1 und q=+1) liegen also auf gleicher Höhe
/// zueinander, während die mittlere Spalte genau um eine halbe Zeile
/// versetzt dazwischenliegt - das ist der in der Aufgabenstellung
/// beschriebene vertikale Versatz der Mittelspalte.
///
/// WARUM q=-1 BEI r=1 UND NICHT BEI r=0 BEGINNT: Das ist keine
/// willkürliche Wahl, sondern zwingend, wenn beide Außenspalten
/// zeilengleich zueinander liegen sollen. Aus y = r + q/2 folgt: ein
/// Schritt von q=+1 zu q=-1 (Δq = -2) verschiebt bei GLEICHBLEIBENDEM y
/// den nötigen r-Wert um genau +1 (Δr = -Δq/2 = 1). Startet die rechte
/// Spalte bei r=0, liegt ihr zeilengleiches Pendant links also
/// zwangsläufig bei r=1. Würde man stattdessen beide Spalten bei r=0
/// beginnen lassen, läge die linke Spalte eine ganze Zeile höher als die
/// rechte - aus der symmetrischen Blüte würde ein geschertes
/// Parallelogramm mit einem einsamen, unverbundenen Feld oben links
/// ((-1,0)) und unten rechts ((1,4) läge dann außerhalb der 5 Felder).
///
/// Die beiden Blüten sind bezüglich der Mitte des Feldes (y = 2.5,
/// zwischen den Feldern (0,2) und (0,3)) symmetrisch. Die Verbindungs-
/// felder (-1,3) und (1,2) liegen exakt auf dieser Mittellinie (y = 2.5)
/// und verbinden dadurch die Außenspalten der beiden Blüten. In der
/// Mittelspalte gibt es dagegen keinen separaten Connector, weil (0,2)
/// und (0,3) dort bereits direkt benachbart sind.
///
/// ---------------------------------------------------------------------
/// STARTFELDER
/// ---------------------------------------------------------------------
/// Startfelder sind je Blüte die drei Felder, die am weitesten von der
/// Feldmitte entfernt liegen (euklidische Distanz der oben beschriebenen
/// (q, y)-Position zum Mittelpunkt (q=0, y=2.5) des gesamten Spielfelds).
/// Das sind pro Blüte die drei Felder der "äußersten Reihe" (dem eigenen
/// Rand zugewandt, vom Gegner am weitesten entfernt):
///
///   Blüte A: (0,0), (-1,1), (1,0)
///   Blüte B: (0,5), (-1,5), (1,4)
///
/// Die übrigen vier Felder jeder Blüte liegen näher an der Mitte und sind
/// keine Startfelder.
/// ---------------------------------------------------------------------
/// </summary>
public sealed class Board
{
    /// <summary>
    /// Rohdaten der 16 Spielfelder. Dies ist die einzige Stelle, an der
    /// die Geometrie des Spielfelds definiert wird.
    /// </summary>
    private static readonly IReadOnlyList<Field> FieldDefinitions = new[]
    {
        // ---- Blüte A (Team 1) ----
        new Field(new HexCoordinate(0, 0), FieldType.BlossomA, isStartField: true),
        new Field(new HexCoordinate(0, 1), FieldType.BlossomA, isStartField: false),
        new Field(new HexCoordinate(0, 2), FieldType.BlossomA, isStartField: false),
        new Field(new HexCoordinate(-1, 1), FieldType.BlossomA, isStartField: true),
        new Field(new HexCoordinate(-1, 2), FieldType.BlossomA, isStartField: false),
        new Field(new HexCoordinate(1, 0), FieldType.BlossomA, isStartField: true),
        new Field(new HexCoordinate(1, 1), FieldType.BlossomA, isStartField: false),

        // ---- Verbindungsfelder ----
        new Field(new HexCoordinate(-1, 3), FieldType.Connector, isStartField: false),
        new Field(new HexCoordinate(1, 2), FieldType.Connector, isStartField: false),

        // ---- Blüte B (Team 2) ----
        new Field(new HexCoordinate(0, 3), FieldType.BlossomB, isStartField: false),
        new Field(new HexCoordinate(0, 4), FieldType.BlossomB, isStartField: false),
        new Field(new HexCoordinate(0, 5), FieldType.BlossomB, isStartField: true),
        new Field(new HexCoordinate(-1, 4), FieldType.BlossomB, isStartField: false),
        new Field(new HexCoordinate(-1, 5), FieldType.BlossomB, isStartField: true),
        new Field(new HexCoordinate(1, 3), FieldType.BlossomB, isStartField: false),
        new Field(new HexCoordinate(1, 4), FieldType.BlossomB, isStartField: true),
    };

    private readonly Dictionary<HexCoordinate, Field> _fieldsByCoordinate;

    public Board()
    {
        _fieldsByCoordinate = FieldDefinitions.ToDictionary(f => f.Coordinate);
    }

    /// <summary>Alle 16 Felder des Spielfelds, ohne festgelegte Reihenfolge.</summary>
    public IReadOnlyCollection<Field> Fields => _fieldsByCoordinate.Values;

    /// <summary>
    /// Prüft, ob an <paramref name="coordinate"/> überhaupt ein Feld des
    /// Spielfelds existiert. Diese Prüfung ist die Grundlage jeder
    /// Gültigkeitsprüfung für Positionen und Bewegungen im Spiel.
    /// </summary>
    public bool Contains(HexCoordinate coordinate) => _fieldsByCoordinate.ContainsKey(coordinate);

    /// <summary>Liefert das Feld an <paramref name="coordinate"/>, sofern es existiert.</summary>
    public Field? GetField(HexCoordinate coordinate) =>
        _fieldsByCoordinate.TryGetValue(coordinate, out var field) ? field : null;

    /// <summary>
    /// Liefert die tatsächlich existierenden Nachbarfelder von
    /// <paramref name="coordinate"/> (also die geometrischen Nachbarn,
    /// gefiltert auf die 16 tatsächlich vorhandenen Felder).
    /// </summary>
    public IEnumerable<Field> GetNeighbors(HexCoordinate coordinate)
    {
        foreach (var neighborCoordinate in coordinate.GetNeighbors())
        {
            var field = GetField(neighborCoordinate);
            if (field is not null)
            {
                yield return field;
            }
        }
    }

    /// <summary>
    /// Spiegelbild eines Felds in der jeweils anderen Blüte: Punktspiegelung
    /// (Drehung um 180°) um die Feldmitte, (q, r) -> (-q, 5 - r). Sie bildet
    /// das Spielfeld auf sich selbst ab, jedes Feld von Blüte A auf ein Feld
    /// von Blüte B (und umgekehrt), Startfeld auf Startfeld, und vertauscht die
    /// beiden Verbindungsfelder (-1,3) und (1,2) (siehe die Geometrie in der
    /// Klassendokumentation; geprüft in BoardTests). Als Drehung erhält sie
    /// Abstände UND den Umlaufsinn - eine Partie auf gespiegelten Feldern ist
    /// geometrisch dieselbe Partie, nur mit vertauschten Blüten.
    ///
    /// Verwendung (Bachelorarbeit, Messung des Seitenvorteils): Option
    /// "Seiten vertauschen" im <see cref="Setup.AusgangslagenGenerator"/>.
    /// </summary>
    public static HexCoordinate MirrorBetweenBlossoms(HexCoordinate coordinate) =>
        new(-coordinate.Q, 5 - coordinate.R);

    /// <summary>Alle Startfelder einer bestimmten Blüte (bzw. eines Typs).</summary>
    public IEnumerable<Field> GetStartFields(FieldType type) =>
        Fields.Where(f => f.IsStartField && f.Type == type);
}
