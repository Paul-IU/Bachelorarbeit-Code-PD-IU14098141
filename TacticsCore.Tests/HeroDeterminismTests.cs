using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Board;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class HeroDeterminismTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    /// <summary>
    /// Aufstellung mit allen drei Beispielhelden auf Team A (der Berserker
    /// erhält für diesen Test zusätzlich eine klon-erzeugende Fähigkeit,
    /// damit auch der Klon-Mechanismus unter der Determinismus-Prüfung
    /// steht) gegen drei Standardfiguren auf Team B. Feste statt zufällig
    /// erzeugte UnitIds (siehe Hinweis in TurnOrder) - für den Vergleich
    /// zweier Partien müssen beide dieselbe Ausgangslage haben.
    /// </summary>
    private static GameState BuildMixedLineup(GameBoard board)
    {
        var state = new GameState(board);

        var cloneAbility = new AbilityDefinition(
            Id: "testklon", DisplayName: "Testklon", IsPassive: false,
            TargetType: AbilityTargetType.Self, Range: 0,
            Effects: new[] { new EffectTemplate(Name: "Klon", IsInstant: true, SpawnsClone: true) });

        var heroDefinitions = new[]
        {
            DefaultHeroes.Schildtraeger,
            DefaultHeroes.Elfe,
            DefaultHeroes.Berserker with { Ability = cloneAbility },
        };

        var heroStartFields = board.GetStartFields(FieldType.BlossomA).ToList();
        for (int i = 0; i < heroDefinitions.Length; i++)
        {
            state.AddUnit(new Hero(Id(i + 1), Team.TeamA, heroStartFields[i].Coordinate, heroDefinitions[i]));
        }

        int counter = 100;
        foreach (var startField in board.GetStartFields(FieldType.BlossomB))
        {
            state.AddUnit(new Unit(Id(counter++), Team.TeamB, startField.Coordinate));
        }

        return state;
    }

    [Fact]
    public void GleicherSeedMitFaehigkeitenEffektenUndKlonen_ErgibtExaktDenselbenPartieverlauf()
    {
        var board = new GameBoard();

        var result1 = new MatchController(BuildMixedLineup(board), new RandomAgent(), new RandomAgent(), seed: 2024, recordReplay: true).Run();
        var result2 = new MatchController(BuildMixedLineup(board), new RandomAgent(), new RandomAgent(), seed: 2024, recordReplay: true).Run();

        Assert.Equal(result1.Winner, result2.Winner);
        Assert.Equal(result1.EndReason, result2.EndReason);
        Assert.Equal(result1.RoundsPlayed, result2.RoundsPlayed);
        Assert.Equal(result1.TeamAResult, result2.TeamAResult);
        Assert.Equal(result1.TeamBResult, result2.TeamBResult);
        Assert.Equal(result1.Replay!.Entries, result2.Replay!.Entries);
    }

    /// <summary>
    /// Aufstellung mit ALLEN NEUN Helden auf Team A (Stapeln auf denselben
    /// Startfeldern ist ausdrücklich erlaubt, siehe GameState) gegen drei
    /// Standardfiguren auf Team B - deckt damit übergreifend jede neu
    /// hinzugekommene Mechanik (Feldsperre/Umleitung, Klon-Ersetzen,
    /// Flächen-Teamfilter, teamrelative Effektstärke, gekoppelte Effekte,
    /// Moduswahl, aufladender/abbrechbarer Zustand) in EINER Partie ab.
    /// </summary>
    private static GameState BuildAllNineHeroesLineup(GameBoard board)
    {
        var state = new GameState(board);

        var heroDefinitions = new[]
        {
            DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker,
            DefaultHeroes.Wand, DefaultHeroes.Illusionist, DefaultHeroes.Musiker,
            DefaultHeroes.Hugger, DefaultHeroes.Elementarmagier, DefaultHeroes.Bogenschuetze,
        };

        var heroStartFields = board.GetStartFields(FieldType.BlossomA).ToList();
        for (int i = 0; i < heroDefinitions.Length; i++)
        {
            // Mehr Helden als Startfelder - Stapeln auf denselben Feldern
            // (reihum) ist ausdrücklich erlaubt.
            var position = heroStartFields[i % heroStartFields.Count].Coordinate;
            state.AddUnit(new Hero(Id(i + 1), Team.TeamA, position, heroDefinitions[i]));
        }

        int counter = 100;
        foreach (var startField in board.GetStartFields(FieldType.BlossomB))
        {
            state.AddUnit(new Unit(Id(counter++), Team.TeamB, startField.Coordinate));
        }

        return state;
    }

    [Fact]
    public void GleicherSeedMitAllenNeunHelden_ErgibtExaktDenselbenPartieverlauf()
    {
        var board = new GameBoard();

        var result1 = new MatchController(BuildAllNineHeroesLineup(board), new RandomAgent(), new RandomAgent(), seed: 4711, recordReplay: true).Run();
        var result2 = new MatchController(BuildAllNineHeroesLineup(board), new RandomAgent(), new RandomAgent(), seed: 4711, recordReplay: true).Run();

        Assert.Equal(result1.Winner, result2.Winner);
        Assert.Equal(result1.EndReason, result2.EndReason);
        Assert.Equal(result1.RoundsPlayed, result2.RoundsPlayed);
        Assert.Equal(result1.TeamAResult, result2.TeamAResult);
        Assert.Equal(result1.TeamBResult, result2.TeamBResult);
        Assert.Equal(result1.Replay!.Entries, result2.Replay!.Entries);
    }

    [Fact]
    public void EinVeraenderterHeldenkatalog_WirktSichNachweisbarAufDenEffektivenAngriffsschadenAus()
    {
        // "20% mehr Schaden" - rein durch eine abgewandelte Definition
        // (record-`with`), ohne jeglichen TacticsCore-Code anzufassen
        // (Anforderung 3).
        var boostedBerserker = DefaultHeroes.Berserker with { BaseAttackDamage = (int)Math.Round(DefaultHeroes.Berserker.BaseAttackDamage * 1.2) };
        Assert.True(boostedBerserker.BaseAttackDamage > DefaultHeroes.Berserker.BaseAttackDamage);

        var state = new GameState(new GameBoard());
        var normalHero = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Berserker);
        var boostedHero = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), boostedBerserker);
        state.AddUnit(normalHero);
        state.AddUnit(boostedHero);

        Assert.True(EffectiveStats.AttackDamage(boostedHero, state) > EffectiveStats.AttackDamage(normalHero, state));
    }
}
