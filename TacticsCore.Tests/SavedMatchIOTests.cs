using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Persistence;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class SavedMatchIOTests
{
    [Fact]
    public void SpeichernUndLaden_LiefertEineInhaltlichIdentischeGespeichertePartie()
    {
        // Eine echte Partie mit Helden (Bewegung, Angriff, Fähigkeit,
        // Effekte, ggf. Klon) erzeugen, damit möglichst viele ReplayEntry-
        // Feldkombinationen (inkl. HexPath) tatsächlich durchlaufen werden.
        var board = new GameBoard();
        var teamA = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Illusionist, DefaultHeroes.Elementarmagier });
        var teamB = new TeamComposition(new[] { DefaultHeroes.Elfe, DefaultHeroes.Hugger, DefaultHeroes.Bogenschuetze });
        var setup = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board)[0];
        var state = AusgangslagenGenerator.BuildGameState(setup, board);

        var result = new MatchController(state, new Agents.RandomAgent(), new Agents.RandomAgent(), seed: 2026, recordReplay: true)
            .Run(setup.StartingTeam);

        Assert.NotNull(result.Replay);
        Assert.NotEmpty(result.Replay!.Entries);

        // Held-Zuordnung aus der Ausgangslage rekonstruieren (Reihenfolge
        // wie von BuildGameState vergeben: Team A 1-3, Team B 101-103).
        var units = new List<SavedUnitInfo>();
        var allUnits = setup.TeamAUnits.Select((u, i) => (Id: i + 1, u.Hero, u.Position, Team: Units.Team.TeamA))
            .Concat(setup.TeamBUnits.Select((u, i) => (Id: i + 101, u.Hero, u.Position, Team: Units.Team.TeamB)));
        foreach (var (id, hero, position, team) in allUnits)
        {
            units.Add(new SavedUnitInfo(new Guid(id, 0, 0, new byte[8]), team, position, hero.Id));
        }

        var saved = new SavedMatch(units, result.Replay!);

        string path = Path.Combine(Path.GetTempPath(), $"tacticscore-savedmatch-{Guid.NewGuid():N}.json");
        try
        {
            SavedMatchIO.Save(saved, path);
            var loaded = SavedMatchIO.Load(path);

            Assert.Equal(saved.Units, loaded.Units);
            Assert.Equal(saved.Replay.Seed, loaded.Replay.Seed);
            Assert.Equal(saved.Replay.StartingTeam, loaded.Replay.StartingTeam);
            Assert.Equal(saved.Replay.InitialSetup, loaded.Replay.InitialSetup);
            Assert.Equal(saved.Replay.Entries, loaded.Replay.Entries);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Laden_EinerNichtVorhandenenDatei_WirftEineNachvollziehbareAusnahme()
    {
        string path = Path.Combine(Path.GetTempPath(), $"tacticscore-nichtvorhanden-{Guid.NewGuid():N}.json");
        Assert.ThrowsAny<IOException>(() => SavedMatchIO.Load(path));
    }
}
