namespace TacticsCore.Rules;

/// <summary>
/// Regeln für den Grundangriff. Als eigene, benannte Optionen definiert,
/// damit sie sich für Balancing-Experimente an dieser einen Stelle
/// umstellen lassen, ohne die Kampflogik selbst anfassen zu müssen.
/// </summary>
public static class CombatRules
{
    /// <summary>
    /// Radius des Angriffsmusters um die eigene Position, gemessen in
    /// Hex-Distanz. 1 bedeutet: das eigene Feld (Distanz 0) PLUS alle
    /// sechs direkt angrenzenden Felder (Distanz 1) - das eigene Feld ist
    /// bewusst Teil des Musters, weil dort (siehe
    /// <see cref="OccupancyRules"/>) auch gegnerische Einheiten stehen
    /// können.
    /// </summary>
    public const int AttackPatternRadius = 1;

    /// <summary>
    /// Ob nur gegnerische Einheiten gültige Angriffsziele sind (true,
    /// aktuelle Grundregel) oder auch verbündete Einheiten getroffen
    /// werden könnten (false - z.B. für spätere Heil- oder
    /// Freund-Feind-Fähigkeiten).
    /// </summary>
    public const bool RequireEnemyTarget = true;
}
