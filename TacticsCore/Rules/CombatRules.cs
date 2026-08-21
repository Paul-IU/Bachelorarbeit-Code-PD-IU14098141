namespace TacticsCore.Rules;

/// <summary>
/// Regeln für den Grundangriff. Als eigene, benannte Optionen definiert,
/// damit sie sich für Balancing-Experimente an dieser einen Stelle
/// umstellen lassen, ohne die Kampflogik selbst anfassen zu müssen.
/// </summary>
public static class CombatRules
{
    /// <summary>
    /// Ob nur gegnerische Einheiten gültige Angriffsziele sind (true,
    /// aktuelle Grundregel) oder auch verbündete Einheiten getroffen
    /// werden könnten (false - z.B. für spätere Heil- oder
    /// Freund-Feind-Fähigkeiten).
    /// </summary>
    public const bool RequireEnemyTarget = true;
}
