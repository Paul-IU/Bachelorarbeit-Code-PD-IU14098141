namespace TacticsCore.Effects;

/// <summary>
/// Wird von Einheiten implementiert, deren Fähigkeit eine PASSIVE, live
/// aus einem aktuellen Wert berechnete Stat-Wirkung hat (Anforderung 8:
/// "Schaden steigt bei sinkenden HP" o.ä.) - im Gegensatz zu einem
/// gespeicherten <see cref="ActiveEffect"/> mit fester Dauer wird dieser
/// Beitrag bei JEDER Abfrage neu berechnet, da sich der zugrunde liegende
/// Wert (z.B. aktuelle HP) jederzeit ändern kann.
///
/// <see cref="EffectiveStats"/> fragt diese Schnittstelle generisch ab,
/// ohne den konkreten Einheitentyp (z.B. Hero) kennen zu müssen - das hält
/// den Namespace TacticsCore.Effects frei von einer Abhängigkeit auf
/// TacticsCore.Heroes (Abhängigkeitsrichtung: Heroes -&gt; Effects, nie
/// umgekehrt).
/// </summary>
public interface IHasPassiveStatContribution
{
    StatModifierSet GetPassiveStatContribution(GameState state);
}
