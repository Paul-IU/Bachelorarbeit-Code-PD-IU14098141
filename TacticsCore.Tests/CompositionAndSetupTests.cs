using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests für Teil A (Team-Aufstellung): Kompositionsgenerator und
/// Ausgangslagen-Aufzählung.
/// </summary>
public class CompositionAndSetupTests
{
    [Fact]
    public void CompositionGenerator_LiefertBeiNeunHeldenGenau84KompositionenOhneDublettenUndOhneDoppelteHelden()
    {
        var catalog = DefaultHeroes.CreateDefaultCatalog();

        var compositions = CompositionGenerator.GenerateAllCompositions(catalog);

        Assert.Equal(84, compositions.Count);
        Assert.Equal(compositions.Count, compositions.Select(c => c.Key).Distinct().Count()); // keine Dubletten
        Assert.All(compositions, c => Assert.Equal(3, c.Heroes.Select(h => h.Id).Distinct().Count())); // nie derselbe Held doppelt
    }

    [Fact]
    public void CompositionGenerator_IstBeiWiederholtenAufrufenImmerIdentischSortiert()
    {
        var catalog = DefaultHeroes.CreateDefaultCatalog();

        var first = CompositionGenerator.GenerateAllCompositions(catalog);
        var second = CompositionGenerator.GenerateAllCompositions(catalog);

        Assert.Equal(first.Select(c => c.Key), second.Select(c => c.Key));
    }

    [Fact]
    public void TeamComposition_WirftBeiDoppeltemHeldenImSelbenTeam()
    {
        Assert.Throws<ArgumentException>(() => new TeamComposition(new[] { DefaultHeroes.Wand, DefaultHeroes.Wand, DefaultHeroes.Hugger }));
    }

    [Fact]
    public void AusgangslagenGenerator_LiefertJePaarungGenau72VerschiedeneAusgangslagen()
    {
        var board = new GameBoard();
        var teamA = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
        var teamB = new TeamComposition(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

        var setups = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board);

        Assert.Equal(72, setups.Count);

        // "Alle verschieden": über die vollständige (Held, Feld)-Zuordnung
        // beider Teams plus beginnendes Team als Vergleichsschlüssel.
        var keys = setups.Select(SetupKey).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void AusgangslagenGenerator_JedeAusgangslageVerwendetGenauDieDreiHeldenDerJeweiligenKomposition()
    {
        var board = new GameBoard();
        var teamA = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
        var teamB = new TeamComposition(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

        var setups = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board);

        Assert.All(setups, setup =>
        {
            Assert.Equal(teamA.Heroes.Select(h => h.Id).ToHashSet(), setup.TeamAUnits.Select(u => u.Hero.Id).ToHashSet());
            Assert.Equal(teamB.Heroes.Select(h => h.Id).ToHashSet(), setup.TeamBUnits.Select(u => u.Hero.Id).ToHashSet());
        });
    }

    [Fact]
    public void AusgangslagenGenerator_BuildGameState_ErzeugtSpielbareAufstellungMitDreiHeldenJeTeam()
    {
        var board = new GameBoard();
        var teamA = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
        var teamB = new TeamComposition(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });
        var setup = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board)[0];

        var state = AusgangslagenGenerator.BuildGameState(setup, board);

        Assert.Equal(3, state.Units.Count(u => u.Team == Team.TeamA));
        Assert.Equal(3, state.Units.Count(u => u.Team == Team.TeamB));
        Assert.All(state.Units, u => Assert.IsType<Hero>(u));
        Assert.Equal(state.Units.Select(u => u.Id).Distinct().Count(), state.Units.Count); // eindeutige Ids
    }

    [Fact]
    public void GameSetup_WithSwappedTeams_TauschtNurDieTeamZugehoerigkeitNichtDiePositionen()
    {
        var board = new GameBoard();
        var teamA = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
        var teamB = new TeamComposition(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });
        var setup = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board)[0];

        var swapped = setup.WithSwappedTeams();

        Assert.Equal(setup.TeamAUnits, swapped.TeamBUnits);
        Assert.Equal(setup.TeamBUnits, swapped.TeamAUnits);
        Assert.NotEqual(setup.StartingTeam, swapped.StartingTeam);
    }

    private static string SetupKey(GameSetup setup) =>
        string.Join("|", setup.TeamAUnits.Select(u => $"{u.Hero.Id}@{u.Position}")) + ">" +
        string.Join("|", setup.TeamBUnits.Select(u => $"{u.Hero.Id}@{u.Position}")) + ">" +
        setup.StartingTeam;
}
