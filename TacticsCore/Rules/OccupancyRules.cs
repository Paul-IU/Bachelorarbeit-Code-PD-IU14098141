namespace TacticsCore.Rules;

/// <summary>
/// Belegungsregeln für Bewegung und Stapeln von Einheiten auf einem Feld.
/// Bewusst als eigene, benannte Optionen an dieser einen Stelle definiert
/// (statt als verstreute Magic Values in Bewegungs- oder Zustandslogik),
/// damit sie sich für spätere Balancing-Experimente - insbesondere
/// blockierende Fähigkeiten - gezielt umstellen lassen.
/// </summary>
public static class OccupancyRules
{
    /// <summary>
    /// Ob belegte Felder die Bewegung anderer Einheiten blockieren.
    ///
    /// Aktuelle Grundregel: false. Einheiten - verbündet wie gegnerisch -
    /// blockieren weder das Durchqueren noch das Betreten eines Feldes;
    /// jede Einheit darf durch jede andere hindurchziehen. Blockieren soll
    /// in dieser Phase des Projekts ausschließlich über zukünftige
    /// Fähigkeiten möglich sein, nicht als generelle Bewegungsregel.
    ///
    /// Ist der Schalter aktiv, behandelt <see cref="Movement.MovementSystem"/>
    /// belegte Felder als vollständig unpassierbar - weder durchquerbar
    /// noch als Ziel wählbar (siehe dortige Dokumentation). Das ist eine
    /// bewusst einfache Startinterpretation; feinere Regeln (z.B. "nur
    /// Gegner blockieren" oder "blockiert nur das Durchqueren, nicht das
    /// Betreten") sollen erst entstehen, wenn die erste tatsächlich
    /// blockierende Fähigkeit gebaut wird.
    /// </summary>
    public const bool UnitsBlockMovement = false;

    /// <summary>
    /// Maximale Anzahl Einheiten, die gleichzeitig ein Feld belegen dürfen.
    /// null = unbegrenzt (aktuelle Grundregel - theoretisch könnten so alle
    /// Einheiten beider Teams auf demselben Feld stehen). Wird von
    /// <see cref="TacticsCore.GameState"/> bei Platzierung
    /// (<c>AddUnit</c>) und Bewegung (<c>MoveUnit</c>) geprüft.
    ///
    /// Hinweis: Die Erreichbarkeitsberechnung (BFS in
    /// <see cref="Movement.MovementSystem"/>) berücksichtigt dieses Limit
    /// bewusst NICHT - sie prüft ausdrücklich nur die Spielfeldgrenzen,
    /// keine Einheiten. Ein Feld kann also als erreichbar ausgewiesen
    /// werden, obwohl ein hier gesetztes Stapel-Limit die tatsächliche
    /// Ausführung der Bewegung im Einzelfall noch verhindern könnte. Bei
    /// der aktuellen Grundregel (kein Limit) ist das irrelevant; sollte
    /// aber bedacht werden, falls dieser Wert später aktiv genutzt wird.
    ///
    /// (Als <c>static readonly</c> statt <c>const</c> deklariert, da
    /// <c>int?</c> in C# kein zulässiger Konstantentyp ist.)
    /// </summary>
    public static readonly int? MaxUnitsPerField = null;
}
