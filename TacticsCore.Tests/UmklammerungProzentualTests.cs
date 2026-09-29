using TacticsCore.Abilities;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Designentscheidung "Umklammerung prozentual" (siehe <see cref="DefaultHeroes.Hugger"/>):
/// das umklammerte gegnerische Ziel verliert 33 % seines Angriffsschadens,
/// unabhängig vom Helden - vorher absolut −100, was die meisten auf 0 setzte.
/// </summary>
public class UmklammerungProzentualTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    public static IEnumerable<object[]> AlleHelden() =>
        DefaultHeroes.CreateDefaultCatalog().All.Select(h => new object[] { h.Id });

    [Theory]
    [MemberData(nameof(AlleHelden))]
    public void Umklammerung_ReduziertBeiJedemHeldenDenselbenAnteil(string heldId)
    {
        var definition = DefaultHeroes.CreateDefaultCatalog().Get(heldId);
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var ziel = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 2), definition);
        state.AddUnit(hugger);
        state.AddUnit(ziel);
        // Verbündeter: als EINZIGER Held seines Teams dürfte der Hugger nicht greifen (Regellücke "angehefteter letzter Held").
        state.AddUnit(new TestSupport.TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10));
        int vorher = EffectiveStats.AttackDamage(ziel, state);

        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(ziel), state, greifen));

        int nachher = EffectiveStats.AttackDamage(ziel, state);
        Assert.Equal((int)Math.Round(vorher * DefaultHeroes.HuggerUmklammerungSchadensmultiplikator), nachher);
        Assert.InRange((double)nachher / vorher, 0.66, 0.68); // derselbe Anteil (bis auf Rundung), nie 0
    }

    public static IEnumerable<object[]> HeldenOhneHugger() =>
        DefaultHeroes.CreateDefaultCatalog().All.Where(h => h.Id != "hugger").Select(h => new object[] { h.Id });

    /// <summary>
    /// Designentscheidung "Umklammerung prozentual, Bonus": ein verbündetes Ziel
    /// erhält +33 % (vorher absolut +100) - bei jedem Helden derselbe Anteil.
    /// </summary>
    [Theory]
    [MemberData(nameof(HeldenOhneHugger))]
    public void Umklammerung_BonusFuerVerbuendetesZiel_IstBeiJedemHeldenDerselbeAnteil(string heldId)
    {
        var definition = DefaultHeroes.CreateDefaultCatalog().Get(heldId);
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 2), definition);
        state.AddUnit(hugger);
        state.AddUnit(verbuendeter);
        int vorher = EffectiveStats.AttackDamage(verbuendeter, state);

        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(verbuendeter), state, greifen));

        int nachher = EffectiveStats.AttackDamage(verbuendeter, state);
        Assert.Equal((int)Math.Round(vorher * DefaultHeroes.HuggerUmklammerungBonusmultiplikator), nachher);
        Assert.InRange((double)nachher / vorher, 1.32, 1.34);
    }

    /// <summary>Die Beispiele aus der Vorgabe: Illusionist 160 -> 80, Bogenschütze 220 -> 160.</summary>
    [Theory]
    [InlineData("illusionist", 80)]
    [InlineData("bogenschuetze", 160)]
    public void Umklammerung_BonusBeispiele(string heldId, int erwartet)
    {
        var state = new GameState(new GameBoard());
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.CreateDefaultCatalog().Get(heldId));
        state.AddUnit(hugger);
        state.AddUnit(verbuendeter);
        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(verbuendeter), state, greifen));

        Assert.Equal(erwartet, EffectiveStats.AttackDamage(verbuendeter, state));
    }

    /// <summary>
    /// Designentscheidung "Kampflied prozentual": +40 % auf Distanz 0 und 1, +20 % auf
    /// Distanz 2 - bei jedem Helden derselbe Anteil (vorher absolut +60 / +40).
    /// </summary>
    [Theory]
    [MemberData(nameof(AlleHelden))]
    public void Kampflied_IstBeiJedemHeldenDerselbeAnteil(string heldId)
    {
        var definition = DefaultHeroes.CreateDefaultCatalog().Get(heldId);
        var state = new GameState(new GameBoard());
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Musiker);
        var nah = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), definition);  // Distanz 1
        var fern = new Hero(Id(3), Team.TeamA, new HexCoordinate(0, 0), definition); // Distanz 2
        foreach (var u in new Unit[] { musiker, nah, fern }) state.AddUnit(u);
        int basis = EffectiveStats.AttackDamage(nah, state);

        var kampflied = DefaultHeroes.Musiker.Ability.Modes!.Single(m => m.Id == "kampflied");
        Assert.True(AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied));

        Assert.Equal((int)Math.Round(basis * 1.4), EffectiveStats.AttackDamage(nah, state));
        Assert.Equal((int)Math.Round(basis * 1.2), EffectiveStats.AttackDamage(fern, state));
    }

    /// <summary>
    /// Verrechnungsregel: mehrere prozentuale Modifikatoren werden multipliziert -
    /// das Ergebnis ist unabhängig von der Reihenfolge, in der sie entstehen.
    /// Held unter Kampflied, vom eigenen Hugger umklammert (+33 %) und vom
    /// gegnerischen Hugger umklammert (−33 %): 100 × 1,4 × 1,33 × 0,67 = 124,75 -> 125.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MehrereProzentualeModifikatoren_WerdenMultipliziert_UnabhaengigVonDerReihenfolge(int reihenfolge)
    {
        var state = new GameState(new GameBoard());
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Musiker);
        var eigenerHugger = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var held = new TestSupport.TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // 100 Grundschaden
        var gegnerHugger = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 2), DefaultHeroes.Hugger);
        var gegnerVerbuendeter = new TestSupport.TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 5), speed: 10);
        foreach (var u in new Unit[] { musiker, eigenerHugger, held, gegnerHugger, gegnerVerbuendeter }) state.AddUnit(u);

        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
        var kampflied = DefaultHeroes.Musiker.Ability.Modes!.Single(m => m.Id == "kampflied");
        var schritte = new Action[]
        {
            () => Assert.True(AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied)),
            () => Assert.True(AbilitySystem.TryUseAbility(eigenerHugger, new AbilityTarget.UnitTarget(held), state, greifen)),
            () => Assert.True(AbilitySystem.TryUseAbility(gegnerHugger, new AbilityTarget.UnitTarget(held), state, greifen)),
        };
        foreach (int i in Enumerable.Range(0, 3)) schritte[(i + reihenfolge) % 3]();

        Assert.Equal(125, EffectiveStats.AttackDamage(held, state));
    }

    /// <summary>Reihenfolge: erst alle Deltas addieren, dann den Multiplikator anwenden.</summary>
    [Fact]
    public void ProzentualerModifikator_WirktNachDenAdditivenDeltas()
    {
        var state = new GameState(new GameBoard());
        var einheit = new TestSupport.TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // 100 Grundschaden
        state.AddUnit(einheit);
        EffectSystem.Apply(state, ActiveEffect.OnUnit(einheit, einheit, new EffectTemplate(Name: "Buff", IsInstant: false, Duration: 2, StatModifiers: new StatModifierSet(BaseAttackDamageDelta: 50))));
        EffectSystem.Apply(state, ActiveEffect.OnUnit(einheit, einheit, new EffectTemplate(Name: "Schwaechung", IsInstant: false, Duration: 2, StatModifiers: new StatModifierSet(AttackDamageMultiplier: 0.5))));

        Assert.Equal(75, EffectiveStats.AttackDamage(einheit, state)); // (100 + 50) × 0,5
    }
}
