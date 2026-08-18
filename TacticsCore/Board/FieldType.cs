namespace TacticsCore.Board;

/// <summary>
/// Typ eines Spielfelds. "Blüte" bezeichnet die jeweilige Heimatzone eines
/// Teams (Team 1 = BlossomA, Team 2 = BlossomB), "Connector" die schmalen
/// Verbindungsfelder zwischen den beiden Blüten.
/// </summary>
public enum FieldType
{
    BlossomA,
    BlossomB,
    Connector,
}
