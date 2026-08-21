using TacticsCore.Abilities;

namespace TacticsCore.Heroes;

/// <summary>
/// Datengetriebene Beschreibung eines Helden (Anforderung 2 aus Teil A:
/// "als Daten definiert, nicht als je eigene Klasse mit hartkodierten
/// Werten"). Ein <see cref="Hero"/>-Objekt trägt zur Laufzeit lediglich
/// einen Verweis auf genau eine solche Definition - alle Werte kommen von
/// hier.
///
/// AUSTAUSCHBARKEIT (Anforderung 3): Da <see cref="HeroDefinition"/> ein
/// <c>record</c> ist, lässt sich zur Laufzeit jederzeit eine ABGEWANDELTE
/// Definition erzeugen, ohne TacticsCore-Code zu ändern - z.B.
/// <c>DefaultHeroes.Berserker with { BaseAttackDamage = 14 }</c> für "20%
/// mehr Schaden". Ein <see cref="HeroCatalog"/> mit einer solchen
/// abgewandelten Definition wird einfach anstelle des Standardkatalogs an
/// die Partie-Aufstellung übergeben.
/// </summary>
public sealed record HeroDefinition(
    string Id,
    string DisplayName,
    HeroRole Role,
    int MaxHitPoints,
    int BaseAttackDamage,
    int Speed,
    int MovementRange,
    int AttackRange,
    AbilityDefinition Ability);
