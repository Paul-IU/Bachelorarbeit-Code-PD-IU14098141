namespace TacticsCore.Heroes;

/// <summary>
/// Erzeugt aus einem Basiskatalog abgeleitete Varianten, indem GENAU EIN
/// Attribut EINES bestimmten Helden um einen Prozentsatz verändert wird
/// (Anforderung 6/7 aus Teil B) - die zentrale Grundlage für "was passiert
/// mit den Simulationsergebnissen, wenn Held X 20% mehr Schaden bekommt?".
/// Der Basiskatalog selbst bleibt unverändert (<see cref="HeroDefinition"/>
/// ist ein record, "with" erzeugt immer eine neue Instanz) - beide Kataloge
/// lassen sich also parallel für einen Vergleichslauf verwenden.
/// </summary>
public static class HeroCatalogVariants
{
    /// <summary>
    /// Liefert einen neuen Katalog, in dem der Held mit Id
    /// <paramref name="heroId"/> bei <paramref name="attribute"/> um
    /// <paramref name="percentageChange"/> Prozent verändert ist (z.B. 20.0
    /// für "20% mehr", -15.0 für "15% weniger"), gerundet auf den nächsten
    /// ganzzahligen Attributwert. ALLE anderen Helden und ALLE anderen
    /// Attribute dieses Helden bleiben exakt unverändert.
    /// </summary>
    public static HeroCatalog WithAttributePercentageChange(HeroCatalog baseCatalog, string heroId, HeroAttribute attribute, double percentageChange)
    {
        var target = baseCatalog.Get(heroId); // wirft, falls die Id nicht existiert - bewusst, ein stiller Tippfehler wäre sonst kaum zu bemerken
        var changed = ApplyChange(target, attribute, percentageChange);

        var heroes = baseCatalog.All.Select(h => h.Id == heroId ? changed : h).ToList();
        return new HeroCatalog(heroes);
    }

    private static HeroDefinition ApplyChange(HeroDefinition hero, HeroAttribute attribute, double percentageChange)
    {
        int Scale(int value) => (int)Math.Round(value * (1.0 + percentageChange / 100.0));

        return attribute switch
        {
            HeroAttribute.MaxHitPoints => hero with { MaxHitPoints = Scale(hero.MaxHitPoints) },
            HeroAttribute.BaseAttackDamage => hero with { BaseAttackDamage = Scale(hero.BaseAttackDamage) },
            HeroAttribute.Speed => hero with { Speed = Scale(hero.Speed) },
            HeroAttribute.MovementRange => hero with { MovementRange = Scale(hero.MovementRange) },
            HeroAttribute.AttackRange => hero with { AttackRange = Scale(hero.AttackRange) },
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unbekanntes Heldenattribut."),
        };
    }
}
