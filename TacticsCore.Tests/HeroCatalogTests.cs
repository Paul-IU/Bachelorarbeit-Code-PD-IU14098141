using TacticsCore.Heroes;
using TacticsCore.Units;

namespace TacticsCore.Tests;

/// <summary>
/// Prüft die beiden fest vorgegebenen Katalog-Regeln aus der
/// Bachelorarbeit-Korrektur (siehe Klassendokumentation von
/// <see cref="DefaultHeroes"/>): paarweise verschiedene Geschwindigkeits-
/// werte (als Konstruktor-Zwang in <see cref="HeroCatalog"/> abgesichert)
/// sowie die Rollen-Korridore für HP/Grundschaden (nur für den
/// Standardkatalog geprüft, siehe dort für die Begründung, warum das keine
/// Konstruktor-Invariante ist).
/// </summary>
public class HeroCatalogTests
{
    private static readonly HeroDefinition[] AlleNeun =
    {
        DefaultHeroes.Schildtraeger, DefaultHeroes.Wand, DefaultHeroes.Illusionist,
        DefaultHeroes.Elfe, DefaultHeroes.Musiker, DefaultHeroes.Hugger,
        DefaultHeroes.Berserker, DefaultHeroes.Elementarmagier, DefaultHeroes.Bogenschuetze,
    };

    [Fact]
    public void Standardkatalog_AlleGeschwindigkeitswerteSindPaarweiseVerschieden()
    {
        var speeds = AlleNeun.Select(h => h.Speed).ToList();

        Assert.Equal(speeds.Count, speeds.Distinct().Count());
    }

    [Fact]
    public void HeroCatalog_LehntEinenKatalogMitDoppeltenGeschwindigkeitswertenAb()
    {
        // Zwei ansonsten unabhängige Helden mit demselben Geschwindigkeitswert -
        // unabhängig davon, ob sie aus DefaultHeroes stammen oder eine freie
        // Balancing-Variante sind, muss die Katalogerzeugung selbst das
        // sofort ablehnen (siehe HeroCatalog.ValidateDistinctSpeedValues).
        var duplicateSpeedHero = DefaultHeroes.Elfe with { Id = "elfe-zwilling", Speed = DefaultHeroes.Musiker.Speed };
        var heroesWithDuplicate = AlleNeun.Append(duplicateSpeedHero).ToList();

        var exception = Assert.Throws<ArgumentException>(() => new HeroCatalog(heroesWithDuplicate));
        Assert.Contains(DefaultHeroes.Musiker.Speed.ToString(), exception.Message);
    }

    [Fact]
    public void Standardkatalog_JederHeldLiegtInnerhalbDesKorridorsSeinerRolle()
    {
        foreach (var hero in AlleNeun)
        {
            var (minHp, maxHp, minDamage, maxDamage) = hero.Role switch
            {
                HeroRole.Tank => (DefaultHeroes.TankMinHp, DefaultHeroes.TankMaxHp, DefaultHeroes.TankMinDamage, DefaultHeroes.TankMaxDamage),
                HeroRole.Support => (DefaultHeroes.SupportMinHp, DefaultHeroes.SupportMaxHp, DefaultHeroes.SupportMinDamage, DefaultHeroes.SupportMaxDamage),
                HeroRole.Dmg => (DefaultHeroes.DmgMinHp, DefaultHeroes.DmgMaxHp, DefaultHeroes.DmgMinDamage, DefaultHeroes.DmgMaxDamage),
                _ => throw new ArgumentOutOfRangeException(),
            };

            Assert.True(hero.MaxHitPoints >= minHp && hero.MaxHitPoints <= maxHp,
                $"{hero.DisplayName} ({hero.Role}): HP {hero.MaxHitPoints} liegt außerhalb des Korridors {minHp}-{maxHp}.");
            Assert.True(hero.BaseAttackDamage >= minDamage && hero.BaseAttackDamage <= maxDamage,
                $"{hero.DisplayName} ({hero.Role}): Schaden {hero.BaseAttackDamage} liegt außerhalb des Korridors {minDamage}-{maxDamage}.");
        }
    }

    /// <summary>
    /// Zusatzregel neben den Korridoren: ein Fernkämpfer (AttackRange größer
    /// als der kleinste in dieser Rolle vorkommende Wert) hat weniger HP als
    /// jeder Nahkämpfer derselben Rolle. Tank hat aktuell keinen
    /// Fernkämpfer, die Regel greift daher nur für Support und Dmg.
    /// </summary>
    [Fact]
    public void FernkaempferHabenWenigerHpAlsNahkaempferDerselbenRolle()
    {
        AssertRangedBelowMelee(HeroRole.Support, melee: new[] { DefaultHeroes.Musiker, DefaultHeroes.Hugger }, ranged: new[] { DefaultHeroes.Elfe });
        AssertRangedBelowMelee(HeroRole.Dmg, melee: new[] { DefaultHeroes.Berserker }, ranged: new[] { DefaultHeroes.Elementarmagier, DefaultHeroes.Bogenschuetze });
    }

    private static void AssertRangedBelowMelee(HeroRole role, HeroDefinition[] melee, HeroDefinition[] ranged)
    {
        int minMeleeHp = melee.Min(h => h.MaxHitPoints);
        foreach (var rangedHero in ranged)
        {
            Assert.True(rangedHero.MaxHitPoints < minMeleeHp,
                $"{rangedHero.DisplayName} ({role}, Fernkämpfer, {rangedHero.MaxHitPoints} HP) sollte weniger HP haben als jeder Nahkämpfer derselben Rolle (mindestens {minMeleeHp} HP).");
        }
    }
}
