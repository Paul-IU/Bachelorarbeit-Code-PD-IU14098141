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

    /// <summary>
    /// Maximale Lebenspunkte. Bachelorarbeit-Korrektur ("Faktor-10-Skalierung",
    /// siehe Klassendokumentation von <see cref="Heroes.DefaultHeroes"/>):
    /// 50 -&gt; 500, wie alle Kampfwerte um den Faktor 10 angehoben, damit auch
    /// kleine Prozentänderungen (siehe <see cref="Heroes.HeroCatalogVariants"/>)
    /// bei ganzzahliger Rundung sichtbar bleiben.
    /// </summary>
    public const int MaxHitPoints = 500;

    /// <summary>Schaden eines Grundangriffs. Bachelorarbeit-Korrektur: ebenfalls um den Faktor 10 skaliert (10 -&gt; 100), siehe MaxHitPoints.</summary>
    public const int BaseAttackDamage = 100;

    /// <summary>
    /// Reichweite des Grundangriffs in Hex-Distanz. 1 bedeutet: das eigene
    /// Feld (Distanz 0) PLUS alle sechs direkt angrenzenden Felder - das
    /// eigene Feld ist bewusst Teil des Musters, weil dort (siehe
    /// <see cref="OccupancyRules"/>) auch gegnerische Einheiten stehen
    /// können. Löst den früheren, für alle Einheiten global fixen
    /// <c>CombatRules.AttackPatternRadius</c> ab, jetzt da Helden (Fernkampf)
    /// individuelle Angriffsreichweiten haben können.
    /// </summary>
    public const int AttackRange = 1;

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
