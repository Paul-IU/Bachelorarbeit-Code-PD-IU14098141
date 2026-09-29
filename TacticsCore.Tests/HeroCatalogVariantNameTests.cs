using TacticsCore.Heroes;

namespace TacticsCore.Tests;

/// <summary>
/// Katalogvarianten über ihren Namen (<see cref="HeroCatalogVariants.FromName"/>) und
/// die Fähigkeitsstärke des Schildträgers: genau EINE Änderung, der Basiskatalog
/// bleibt unverändert.
/// </summary>
public class HeroCatalogVariantNameTests
{
    private static HeroCatalog Basis() => DefaultHeroes.CreateDefaultCatalog();

    private static void NurSchildtraegerGeaendert(HeroCatalog variante)
    {
        var basis = Basis();
        Assert.Equal(basis.All.Select(h => h.Id), variante.All.Select(h => h.Id));
        foreach (var held in basis.All.Where(h => h.Id != "schildtraeger"))
        {
            Assert.Same(held.Ability, variante.Get(held.Id).Ability);
            Assert.Equal(held, variante.Get(held.Id));
        }
    }

    [Fact]
    public void Grundschaden_Plus10Prozent()
    {
        var v = HeroCatalogVariants.FromName(Basis(), "schildtraeger_dmg_10");
        var s = v.Get("schildtraeger");

        Assert.Equal(77, s.BaseAttackDamage); // 70 · 1,1
        Assert.Equal(DefaultHeroes.Schildtraeger with { BaseAttackDamage = 77 }, s);
        NurSchildtraegerGeaendert(v);
    }

    [Fact]
    public void Lebenspunkte_Plus10Prozent()
    {
        var v = HeroCatalogVariants.FromName(Basis(), "schildtraeger_hp_10");
        var s = v.Get("schildtraeger");

        Assert.Equal(935, s.MaxHitPoints); // 850 · 1,1
        Assert.Equal(DefaultHeroes.Schildtraeger with { MaxHitPoints = 935 }, s);
        NurSchildtraegerGeaendert(v);
    }

    /// <summary>
    /// Schildwall: Reduktion 50 % → 55 %, Multiplikator exakt 0,45 (nicht
    /// 0,44999999999999996, siehe HeroCatalogVariants.ScaleShieldWall). Dauer,
    /// Abklingzeit, Reichweite und Zielart unverändert.
    /// </summary>
    [Fact]
    public void Faehigkeitsstaerke_Plus10Prozent_SchildwallReduktion55Prozent()
    {
        var v = HeroCatalogVariants.FromName(Basis(), "schildtraeger_ability_10");
        var s = v.Get("schildtraeger");
        var basis = DefaultHeroes.Schildtraeger;

        var effekt = Assert.Single(s.Ability.Effects);
        Assert.Equal(0.45, effekt.IncomingDamageMultiplier);
        Assert.Equal(basis.Ability.Effects[0] with { IncomingDamageMultiplier = 0.45 }, effekt);
        Assert.Equal(basis.Ability.CooldownRounds, s.Ability.CooldownRounds);
        Assert.Equal(basis.Ability.Range, s.Ability.Range);
        Assert.Equal(basis.Ability.TargetType, s.Ability.TargetType);
        Assert.Equal(basis with { Ability = s.Ability }, s);
        Assert.Equal(32, (int)Math.Round(70 * effekt.IncomingDamageMultiplier!.Value)); // 31,5 → 32, kein Gleitkomma-Artefakt
        NurSchildtraegerGeaendert(v);

        Assert.Equal(0.5, DefaultHeroes.Schildtraeger.Ability.Effects[0].IncomingDamageMultiplier); // Basiskatalog unverändert
    }

    /// <summary>
    /// +5 %: 70 · 1,05 = 73,5 und 850 · 1,05 = 892,5 sind exakte Halbwerte (auch in
    /// Gleitkomma). Math.Round rundet sie zur geraden Zahl: 74 bzw. 892. Schildwall:
    /// Reduktion 52,5 %, Multiplikator 0,475.
    /// </summary>
    [Fact]
    public void Plus5Prozent_WerteUndRundung()
    {
        Assert.Equal(74, HeroCatalogVariants.FromName(Basis(), "schildtraeger_dmg_5").Get("schildtraeger").BaseAttackDamage);
        Assert.Equal(892, HeroCatalogVariants.FromName(Basis(), "schildtraeger_hp_5").Get("schildtraeger").MaxHitPoints);
        Assert.Equal(0.475, HeroCatalogVariants.FromName(Basis(), "schildtraeger_ability_5").Get("schildtraeger").Ability.Effects[0].IncomingDamageMultiplier);
    }

    /// <summary>+2,5 %: 71,75 → 72, 871,25 → 871 (keine Halbwerte), Schildwall-Multiplikator 0,4875.</summary>
    [Fact]
    public void Plus2Komma5Prozent_Werte()
    {
        Assert.Equal(72, HeroCatalogVariants.FromName(Basis(), "schildtraeger_dmg_2.5").Get("schildtraeger").BaseAttackDamage);
        Assert.Equal(871, HeroCatalogVariants.FromName(Basis(), "schildtraeger_hp_2.5").Get("schildtraeger").MaxHitPoints);
        Assert.Equal(0.4875, HeroCatalogVariants.FromName(Basis(), "schildtraeger_ability_2.5").Get("schildtraeger").Ability.Effects[0].IncomingDamageMultiplier);
    }

    /// <summary>Testvariante Heilung der Elfe 80 / 50 / 20 (Basis 120 / 70 / 20); sonst unverändert.</summary>
    [Fact]
    public void ElfeHeilung_80_50_20()
    {
        var v = HeroCatalogVariants.FromName(Basis(), "elfe_heilung_80_50_20");
        var basis = DefaultHeroes.Elfe;
        var effekt = Assert.Single(v.Get("elfe").Ability.Effects);

        Assert.Equal(basis.Ability.Effects[0] with { InstantHeal = 80, DistanceFalloffPerStep = 30, MinimumAfterDistanceScaling = 20 }, effekt);
        Assert.Equal(basis with { Ability = v.Get("elfe").Ability }, v.Get("elfe"));
        foreach (var held in Basis().All.Where(h => h.Id != "elfe"))
        {
            Assert.Equal(held, v.Get(held.Id));
        }

        // Tatsächliche Heilmenge je Entfernung über das Spielsystem: 80 / 50 / 20.
        foreach (var (distanz, erwartet) in new[] { (0, 80), (1, 50), (2, 20) })
        {
            var state = new GameState(new TacticsCore.Board.Board());
            var elfe = new Hero(new Guid(1, 0, 0, new byte[8]), Units.Team.TeamA, new Hex.HexCoordinate(0, 0), v.Get("elfe"));
            var ziel = new Hero(new Guid(2, 0, 0, new byte[8]), Units.Team.TeamA, new Hex.HexCoordinate(0, distanz), DefaultHeroes.Schildtraeger);
            state.AddUnit(elfe);
            state.AddUnit(ziel);
            ziel.ApplyDamage(300);
            Assert.True(Abilities.AbilitySystem.TryUseAbility(elfe, new Abilities.AbilityTarget.UnitTarget(ziel), state));
            Assert.Equal(erwartet, ziel.CurrentHitPoints - (ziel.MaxHitPoints - 300));
        }

        Assert.Equal(120, DefaultHeroes.Elfe.Ability.Effects[0].InstantHeal); // Basiskatalog unverändert
    }

    /// <summary>
    /// Negative Stufen: −2,5 % (68 / 829 / 0,5125), −5 % (66,5 → 66 und 807,5 → 808,
    /// Halbwerte zur geraden Zahl / 0,525), −10 % (63 / 765 / 0,55).
    /// </summary>
    [Theory]
    [InlineData("-2.5", 68, 829, 0.5125)]
    [InlineData("-5", 66, 808, 0.525)]
    [InlineData("-10", 63, 765, 0.55)]
    public void NegativeStufen_Werte(string prozent, int schaden, int lebenspunkte, double multiplikator)
    {
        Assert.Equal(schaden, HeroCatalogVariants.FromName(Basis(), $"schildtraeger_dmg_{prozent}").Get("schildtraeger").BaseAttackDamage);
        Assert.Equal(lebenspunkte, HeroCatalogVariants.FromName(Basis(), $"schildtraeger_hp_{prozent}").Get("schildtraeger").MaxHitPoints);
        Assert.Equal(multiplikator, HeroCatalogVariants.FromName(Basis(), $"schildtraeger_ability_{prozent}").Get("schildtraeger").Ability.Effects[0].IncomingDamageMultiplier);
    }

    [Theory]
    [InlineData("schildtraeger_dmg")]
    [InlineData("schildtraeger_speed_10")]
    [InlineData("niemand_dmg_10")]
    [InlineData("wand_ability_10")] // Fähigkeitsstärke der Wand noch nicht festgelegt
    public void UngueltigeOderNichtFestgelegteVarianten_Werfen(string name) =>
        Assert.ThrowsAny<Exception>(() => HeroCatalogVariants.FromName(Basis(), name));
}
