namespace TacticsCore.Heroes;

/// <summary>
/// Die veränderbaren numerischen Attribute einer <see cref="HeroDefinition"/> -
/// als eigenes Enum, damit <see cref="HeroCatalogVariants"/> ein bestimmtes
/// Attribut generisch (per Auswahl statt per Held-Sonderfall) ansprechen
/// kann, ohne dass der Aufrufer selbst Getter/Setter-Delegaten schreiben
/// müsste.
/// </summary>
public enum HeroAttribute
{
    MaxHitPoints,
    BaseAttackDamage,
    Speed,
    MovementRange,
    AttackRange,
}
