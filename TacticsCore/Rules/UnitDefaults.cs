namespace TacticsCore.Rules;

/// <summary>
/// Zentrale, benannte Grundwerte für die aktuell einzige (identische)
/// Standardfigur. Alle drei Werte sind ausschließlich an dieser einen
/// Stelle definiert, statt im Code verstreut - wenn später Helden-
/// Unterklassen mit abweichenden Werten hinzukommen, ändert sich nur
/// diese Datei bzw. die jeweilige Unterklasse reicht eigene Werte über
/// die geschützte Konstruktor-Überladung von
/// <see cref="TacticsCore.Units.Unit"/> durch.
/// </summary>
public static class UnitDefaults
{
    /// <summary>Bewegungsreichweite in Feldern pro Zug.</summary>
    public const int MovementRange = 2;

    /// <summary>Maximale Lebenspunkte.</summary>
    public const int MaxHitPoints = 50;

    /// <summary>Schaden eines Grundangriffs.</summary>
    public const int BaseAttackDamage = 10;

    /// <summary>
    /// Geschwindigkeit - bestimmt die Rangfolge innerhalb eines Teams in
    /// der Zugreihenfolge (siehe <see cref="TacticsCore.Match.TurnOrder"/>),
    /// absteigend sortiert. Der konkrete Wert ist momentan willkürlich, da
    /// ihn ausnahmslos alle Einheiten teilen - er wird erst bedeutsam,
    /// sobald künftige Helden-Unterklassen abweichende Werte mitbringen.
    /// Solange alle Speed-Werte gleich sind, entscheidet bei JEDEM
    /// Rangfolge-Vergleich die UnitId (siehe TurnOrder) - Unit-IDs sind
    /// also, anders als man vermuten könnte, bereits jetzt
    /// simulationsrelevant, nicht nur ein technisches Detail.
    /// </summary>
    public const int Speed = 10;
}
