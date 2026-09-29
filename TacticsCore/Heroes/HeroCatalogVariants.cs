using System.Globalization;
using TacticsCore.Abilities;

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

    /// <summary>
    /// FÄHIGKEITSSTÄRKE (Bachelorarbeit, Varianten-Läufe): verändert die STÄRKE der
    /// Fähigkeit des Helden um <paramref name="percentageChange"/> Prozent - alles
    /// andere (Wirkungsdauer, Abklingzeit, Reichweite, Zielart) bleibt gleich. Was
    /// "Stärke" ist, hängt von der Fähigkeit ab und ist deshalb je Held festgelegt;
    /// bisher nur für den Schildträger (siehe <see cref="ScaleShieldWall"/>). Für
    /// andere Helden wirft die Methode, statt stillschweigend etwas zu raten.
    /// </summary>
    public static HeroCatalog WithAbilityStrengthChange(HeroCatalog baseCatalog, string heroId, double percentageChange)
    {
        var target = baseCatalog.Get(heroId);
        var changed = heroId switch
        {
            "schildtraeger" => target with { Ability = ScaleShieldWall(target.Ability, percentageChange) },
            _ => throw new ArgumentException(
                $"Für '{heroId}' ist noch nicht festgelegt, was die Fähigkeitsstärke ist - bitte vor dem Lauf definieren.", nameof(heroId)),
        };

        var heroes = baseCatalog.All.Select(h => h.Id == heroId ? changed : h).ToList();
        return new HeroCatalog(heroes);
    }

    /// <summary>
    /// SCHILDWALL: Stärke = die SCHADENSREDUKTION r = 1 − m (m = Multiplikator des
    /// eingehenden Schadens, Basis 0,5 → r = 50 %). "+10 %" skaliert die Reduktion
    /// relativ, genau wie Schaden und Lebenspunkte relativ skaliert werden:
    /// r' = r · 1,1 = 55 %, also m' = 0,45.
    ///
    /// Beim Schildwall führt die zweite vertretbare Lesart - "der verbleibende
    /// Schaden sinkt um 10 %", m' = m · 0,9 - zum SELBEN Wert 0,45, weil die Basis
    /// genau 50 % beträgt. Allgemein unterscheiden sich beide (bei einer Reduktion von
    /// 25 % etwa 27,5 % gegenüber 32,5 %); für die Wand ist das vor einer Variante zu
    /// entscheiden.
    ///
    /// RUNDUNG DES MULTIPLIKATORS auf 4 Nachkommastellen: 1 − 1,1 · 0,5 ergibt in
    /// Gleitkomma 0,44999999999999996. Unbehandelt würde ein Treffer von 70 zu
    /// 31,4999… und damit auf 31 statt 32 gerundet - ein Artefakt der Zahlendarstellung,
    /// keine Eigenschaft der Variante.
    /// </summary>
    private static AbilityDefinition ScaleShieldWall(AbilityDefinition ability, double percentageChange)
    {
        var effects = ability.Effects.Select(e =>
        {
            if (e.IncomingDamageMultiplier is not double m)
            {
                return e;
            }

            double reduction = (1.0 - m) * (1.0 + percentageChange / 100.0);
            return e with { IncomingDamageMultiplier = Math.Round(1.0 - reduction, 4) };
        }).ToList();

        return ability with { Effects = effects };
    }

    /// <summary>
    /// Katalogvariante über ihren Namen (für die Batch-Konfiguration, siehe
    /// Demo "full-baseline --variant"): "&lt;heldId&gt;_&lt;dmg|hp|ability&gt;_&lt;prozent&gt;",
    /// z.B. "schildtraeger_dmg_10" (Grundschaden +10 %), "schildtraeger_hp_10"
    /// (maximale Lebenspunkte +10 %), "schildtraeger_ability_10" (Fähigkeitsstärke
    /// +10 %). Negative Prozentwerte mit Minus ("wand_hp_-10"). Derselbe Name ergibt
    /// immer denselben Katalog - ein Lauf lässt sich für jeden Agenten ohne
    /// Codeänderung wiederholen.
    /// </summary>
    public static HeroCatalog FromName(HeroCatalog baseCatalog, string variantName)
    {
        var parts = variantName.Trim().ToLowerInvariant().Split('_');
        if (parts.Length == 5 && parts[1] == "heilung"
            && int.TryParse(parts[2], out int heal0) && int.TryParse(parts[3], out int heal1) && int.TryParse(parts[4], out int heal2))
        {
            return WithHealingByDistance(baseCatalog, parts[0], heal0, heal1, heal2);
        }

        if (parts.Length != 3 || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double percent))
        {
            throw new ArgumentException($"Variantenname '{variantName}' hat nicht die Form <heldId>_<dmg|hp|ability>_<prozent>.", nameof(variantName));
        }

        return parts[1] switch
        {
            "dmg" => WithAttributePercentageChange(baseCatalog, parts[0], HeroAttribute.BaseAttackDamage, percent),
            "hp" => WithAttributePercentageChange(baseCatalog, parts[0], HeroAttribute.MaxHitPoints, percent),
            "ability" => WithAbilityStrengthChange(baseCatalog, parts[0], percent),
            _ => throw new ArgumentException($"Unbekannte Variantenart '{parts[1]}' - gültig sind dmg, hp, ability.", nameof(variantName)),
        };
    }

    /// <summary>
    /// TESTVARIANTE HEILUNG (Name "&lt;heldId&gt;_heilung_&lt;d0&gt;_&lt;d1&gt;_&lt;d2&gt;", z.B.
    /// "elfe_heilung_80_50_20"): legt die Heilmenge je Entfernung ABSOLUT fest - Distanz
    /// 0, 1 und 2 (die Reichweite der Heilung). Umgesetzt über die bestehenden Felder:
    /// Grundwert = d0, Abfall je Feld = d0 − d1, Mindestwert = d2. Ergibt diese
    /// Rechnung die gewünschten Werte nicht exakt (etwa weil d2 über d0 − 2·(d0 − d1)
    /// liegt, der Mindestwert also schon auf Distanz 1 griffe), wirft die Methode.
    /// Basis Elfe: 120 / 70 / 20.
    /// </summary>
    public static HeroCatalog WithHealingByDistance(HeroCatalog baseCatalog, string heroId, int heal0, int heal1, int heal2)
    {
        var target = baseCatalog.Get(heroId);
        var healEffects = target.Ability.Effects.Where(e => e.InstantHeal is not null).ToList();
        if (healEffects.Count != 1 || !healEffects[0].ScaleWithDistance)
        {
            throw new ArgumentException($"'{heroId}' hat keine einzelne, entfernungsabhängige Sofortheilung.", nameof(heroId));
        }

        int falloff = heal0 - heal1;
        int At(int distance) => Math.Max(heal2, heal0 - falloff * Math.Max(0, distance - healEffects[0].PlateauRadius));
        if (healEffects[0].PlateauRadius != 0 || At(0) != heal0 || At(1) != heal1 || At(2) != heal2)
        {
            throw new ArgumentException($"Heilung {heal0}/{heal1}/{heal2} lässt sich mit Grundwert, Abfall und Mindestwert nicht exakt abbilden.");
        }

        var effects = target.Ability.Effects.Select(e => e.InstantHeal is null ? e
            : e with { InstantHeal = heal0, DistanceFalloffPerStep = falloff, MinimumAfterDistanceScaling = heal2 }).ToList();
        var changed = target with { Ability = target.Ability with { Effects = effects } };
        return new HeroCatalog(baseCatalog.All.Select(h => h.Id == heroId ? changed : h).ToList());
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
