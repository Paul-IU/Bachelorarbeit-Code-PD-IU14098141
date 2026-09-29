namespace TacticsCore.Match;

/// <summary>
/// Kampfbilanz einer Einheit über eine Partie (für die Abschlussansicht in
/// Godot; reine Buchführung, beeinflusst keine Regel).
///
/// ZÄHLWEISE:
///   - Schaden = tatsächlich verlorene Lebenspunkte (nach Reduktion, höchstens
///     bis 0) - Überschussschaden auf eine fast besiegte Einheit zählt nicht.
///   - Verursacht zählt jede Schadensquelle: Grundangriff, Fähigkeit,
///     Schaden über Zeit (Quelle = Anwender des Effekts) und Vergeltung
///     (Quelle = die Wand, die den umgeleiteten Treffer abfing).
///   - Schaden und Besiegte eines Klons zählen für seinen Erzeuger; Schaden AM
///     Klon zählt nicht als erlittener Schaden des Erzeugers.
///   - Besiegte: nur gegnerische HELDEN (kein Klon, der beim Tod seines
///     Erzeugers ohnehin verschwindet).
/// </summary>
public sealed record UnitCombatStatistics(int DamageDealt, int DamageTaken, int Defeats)
{
    public static UnitCombatStatistics Empty { get; } = new(0, 0, 0);
}
