using TacticsCore.Abilities;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Designentscheidung "Echte Pausen" (siehe <see cref="DefaultHeroes.Elementarmagier"/>
/// und <see cref="DefaultHeroes.Musiker"/>): zwischen zwei Wirkungen liegen
/// Abklingzeit − Dauer Runden. Die Tests spielen Runden in BEIDEN Reihenfolgen
/// (Ziel vor bzw. nach dem Anwender) durch und rufen zu jedem Zugbeginn dieselben
/// Tick-Funktionen auf wie der MatchController (EffectSystem.TickUnitEffects,
/// Hero.TickAbilityCooldown). Der Anwender wirkt, sobald die Fähigkeit legal ist.
/// </summary>
public class EchtePausenTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    /// <summary>
    /// Spielt <paramref name="runden"/> Runden; liefert je Runde, ob der Anwender
    /// gewirkt hat und ob der Effekt <paramref name="effektName"/> zu Beginn des
    /// Zugs des Ziels (nach dessen Tick) auf ihm lag.
    /// </summary>
    private static (List<int> Einsaetze, List<bool> ZielBetroffen) Spiele(
        GameState state, Hero anwender, Unit ziel, bool zielZuerst, int runden, Func<bool> wirke, string effektName)
    {
        var einsaetze = new List<int>();
        var betroffen = new List<bool>();
        for (int runde = 1; runde <= runden; runde++)
        {
            foreach (var einheit in zielZuerst ? new Unit[] { ziel, anwender } : new Unit[] { anwender, ziel })
            {
                EffectSystem.TickUnitEffects(state, einheit);
                if (einheit is Hero h) h.TickAbilityCooldown();

                if (einheit == anwender && wirke()) einsaetze.Add(runde);
                if (einheit == ziel) betroffen.Add(state.GetEffectsOnUnit(ziel.Id).Any(e => e.Name == effektName));
            }
        }

        return (einsaetze, betroffen);
    }

    // --- Erde: 1 Runde verlangsamt, 2 Runden Pause --------------------------

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Erde_EineRundeVerlangsamt_DannZweiRundenPause(bool zielZuerst)
    {
        var state = new GameState(new GameBoard());
        var magier = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var ziel = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(magier);
        state.AddUnit(ziel);
        var erde = DefaultHeroes.Elementarmagier.Ability.Modes!.Single(m => m.Id == "erde"); // das Ziel (500 HP) überlebt drei Erdschläge zu 70

        var (einsaetze, verlangsamt) = Spiele(state, magier, ziel, zielZuerst, runden: 9,
            () => AbilitySystem.TryUseAbility(magier, new AbilityTarget.UnitTarget(ziel), state, erde), "Verlangsamung");

        Assert.Equal(new[] { 1, 4, 7 }, einsaetze); // Abklingzeit 3: zwei eigene Züge gesperrt
        // Das Ziel ist in genau einem eigenen Zug je Einsatz verlangsamt, danach zwei Züge frei.
        var erwartet = zielZuerst
            ? new[] { false, true, false, false, true, false, false, true, false }
            : new[] { true, false, false, true, false, false, true, false, false };
        Assert.Equal(erwartet, verlangsamt);
    }

    /// <summary>Feuer und Erde teilen sich die Abklingzeit: nach Erde ist auch Feuer zwei eigene Züge gesperrt.</summary>
    [Fact]
    public void Erde_SperrtAuchFeuer()
    {
        var state = new GameState(new GameBoard());
        var magier = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var ziel = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(magier);
        state.AddUnit(ziel);
        var erde = DefaultHeroes.Elementarmagier.Ability.Modes!.Single(m => m.Id == "erde");
        var feuer = DefaultHeroes.Elementarmagier.Ability.Modes!.Single(m => m.Id == "feuer");
        Assert.True(AbilitySystem.TryUseAbility(magier, new AbilityTarget.UnitTarget(ziel), state, erde));

        for (int zug = 1; zug <= 2; zug++)
        {
            magier.TickAbilityCooldown();
            Assert.False(AbilitySystem.TryUseAbility(magier, new AbilityTarget.UnitTarget(ziel), state, feuer));
        }

        magier.TickAbilityCooldown();
        Assert.True(AbilitySystem.TryUseAbility(magier, new AbilityTarget.UnitTarget(ziel), state, feuer));
    }

    /// <summary>
    /// BEFUND: die Verbrennung (Dauer 3) bleibt mit Abklingzeit 3 lückenlos - Pause
    /// 3 − 3 = 0: in keinem Zug des Ziels fehlt sie, wenn der Magier stets Feuer wirkt.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Verbrennung_BleibtMitAbklingzeit3Lueckenlos(bool zielZuerst)
    {
        var state = new GameState(new GameBoard());
        var magier = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var ziel = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 3), DefaultHeroes.Wand); // 1000 HP, überlebt
        state.AddUnit(magier);
        state.AddUnit(ziel);
        var feuer = DefaultHeroes.Elementarmagier.Ability.Modes!.Single(m => m.Id == "feuer");

        var (_, brennt) = Spiele(state, magier, ziel, zielZuerst, runden: 9,
            () => AbilitySystem.TryUseAbility(magier, new AbilityTarget.UnitTarget(ziel), state, feuer), "Verbrennung");

        Assert.All(brennt.Skip(zielZuerst ? 1 : 0), b => Assert.True(b));
    }

    // --- Lieder: 1 Runde Buff, 1 Runde ohne ----------------------------------

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Kampflied_EineRundeBuff_DannEineRundeOhne(bool zielZuerst)
    {
        var state = new GameState(new GameBoard());
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Musiker);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(musiker);
        state.AddUnit(verbuendeter);
        var kampflied = DefaultHeroes.Musiker.Ability.Modes!.Single(m => m.Id == "kampflied");

        var (einsaetze, gebufft) = Spiele(state, musiker, verbuendeter, zielZuerst, runden: 6,
            () => AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied), "Kampflied");

        Assert.Equal(new[] { 1, 3, 5 }, einsaetze); // Abklingzeit 2: ein eigener Zug gesperrt
        var erwartet = zielZuerst
            ? new[] { false, true, false, true, false, true }
            : new[] { true, false, true, false, true, false };
        Assert.Equal(erwartet, gebufft);
    }

    /// <summary>
    /// Wechselt der Musiker zwischen den Liedern, sperrt die gemeinsame Abklingzeit
    /// auch das andere Lied: nie beide gleichzeitig, und dazwischen immer ein Zug ohne Buff.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BeideLieder_NieGleichzeitig_UndImmerEineRundeOhneBuff(bool zielZuerst)
    {
        var state = new GameState(new GameBoard());
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Musiker);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(musiker);
        state.AddUnit(verbuendeter);
        var modi = DefaultHeroes.Musiker.Ability.Modes!;
        int naechster = 0;
        var beide = new List<bool>();
        var irgendeins = new List<bool>();

        for (int runde = 1; runde <= 8; runde++)
        {
            foreach (var einheit in zielZuerst ? new Unit[] { verbuendeter, musiker } : new Unit[] { musiker, verbuendeter })
            {
                EffectSystem.TickUnitEffects(state, einheit);
                if (einheit is Hero h) h.TickAbilityCooldown();
                if (einheit == musiker)
                {
                    // abwechselnd Kampflied und Marschlied, sobald die Fähigkeit frei ist
                    if (AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, modi[naechster % 2])) naechster++;
                    else Assert.All(modi, m => Assert.False(AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, m)));
                }
                if (einheit == verbuendeter)
                {
                    var namen = state.GetEffectsOnUnit(verbuendeter.Id).Select(e => e.Name).ToHashSet();
                    beide.Add(namen.Contains("Kampflied") && namen.Contains("Marschlied"));
                    irgendeins.Add(namen.Contains("Kampflied") || namen.Contains("Marschlied"));
                }
            }
        }

        Assert.All(beide, b => Assert.False(b));
        for (int i = 1; i < irgendeins.Count; i++)
        {
            Assert.False(irgendeins[i] && irgendeins[i - 1]); // nie zwei Züge in Folge mit Buff
        }
    }
}
