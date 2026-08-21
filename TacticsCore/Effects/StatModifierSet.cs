namespace TacticsCore.Effects;

/// <summary>
/// Additive Änderungen an den vier für Bewegung/Kampf/Zugreihenfolge
/// relevanten Attributwerten einer Einheit. Mehrere gleichzeitig aktive
/// Modifikatoren (aus Buffs, Debuffs und passiven Fähigkeiten) werden
/// einfach aufsummiert (siehe <see cref="EffectiveStats"/>) - es gibt
/// bewusst keine prozentualen Modifikatoren hier, um Reihenfolge-Fragen
/// ("erst addieren oder erst multiplizieren?") von vornherein zu vermeiden;
/// prozentuale Wirkungen (wie die Schildträger-Schadensreduktion) werden
/// stattdessen als eigener, expliziter Multiplikator auf eingehenden
/// Schaden geführt (siehe <see cref="ActiveEffect.IncomingDamageMultiplier"/>),
/// nicht über diesen Typ.
/// </summary>
public sealed record StatModifierSet(
    int BaseAttackDamageDelta = 0,
    int MovementRangeDelta = 0,
    int AttackRangeDelta = 0,
    int SpeedDelta = 0)
{
    public static readonly StatModifierSet None = new();

    public static StatModifierSet operator +(StatModifierSet a, StatModifierSet b) => new(
        a.BaseAttackDamageDelta + b.BaseAttackDamageDelta,
        a.MovementRangeDelta + b.MovementRangeDelta,
        a.AttackRangeDelta + b.AttackRangeDelta,
        a.SpeedDelta + b.SpeedDelta);
}
