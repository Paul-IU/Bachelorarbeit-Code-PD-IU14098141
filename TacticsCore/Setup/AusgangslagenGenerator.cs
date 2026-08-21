using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Setup;

/// <summary>
/// Zählt alle Ausgangslagen EINER Paarung (zwei Team-Kompositionen)
/// systematisch auf (Anforderung 4) und baut daraus einen konkreten
/// <see cref="GameState"/> auf. "Systematisch" heißt hier wörtlich: es wird
/// nichts gewürfelt, sondern über drei ineinander verschachtelte,
/// deterministische Aufzählungen iteriert (Permutation Team A x Permutation
/// Team B x beginnendes Team) - bei gleicher Eingabe liefert
/// <see cref="GenerateAllSetups"/> jedes Mal exakt dieselbe Liste in
/// exakt derselben Reihenfolge.
/// </summary>
public static class AusgangslagenGenerator
{
    /// <summary>
    /// Liefert alle 72 Ausgangslagen für die Paarung (<paramref name="teamA"/>
    /// gegen <paramref name="teamB"/>): 6 Permutationen der drei Team-A-Helden
    /// auf die drei Startfelder von Blüte A, mal 6 Permutationen der drei
    /// Team-B-Helden auf die drei Startfelder von Blüte B, mal 2 mögliche
    /// beginnende Teams = 6 * 6 * 2 = 72. Die Reihenfolge ist rein durch die
    /// Verschachtelung der drei Schleifen bestimmt (Team-A-Permutation
    /// äußerste, beginnendes Team innerste Schleife) - reproduzierbar bei
    /// jedem Aufruf.
    /// </summary>
    public static IReadOnlyList<GameSetup> GenerateAllSetups(TeamComposition teamA, TeamComposition teamB, TacticsCore.Board.Board board)
    {
        var teamAFields = board.GetStartFields(FieldType.BlossomA)
            .OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R)
            .Select(f => f.Coordinate)
            .ToList();
        var teamBFields = board.GetStartFields(FieldType.BlossomB)
            .OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R)
            .Select(f => f.Coordinate)
            .ToList();

        var teamAPermutations = Permutations.GenerateAll(teamA.Heroes);
        var teamBPermutations = Permutations.GenerateAll(teamB.Heroes);

        var setups = new List<GameSetup>();
        foreach (var permA in teamAPermutations)
        {
            var teamAUnits = teamAFields.Zip(permA, (field, hero) => (hero, field)).ToList();

            foreach (var permB in teamBPermutations)
            {
                var teamBUnits = teamBFields.Zip(permB, (field, hero) => (hero, field)).ToList();

                foreach (var startingTeam in new[] { Team.TeamA, Team.TeamB })
                {
                    setups.Add(new GameSetup(teamAUnits, teamBUnits, startingTeam));
                }
            }
        }

        return setups;
    }

    /// <summary>
    /// Baut aus einer Ausgangslage einen konkreten, bespielbaren
    /// <see cref="GameState"/> auf. Die Unit-Ids werden bewusst NICHT über
    /// <c>Guid.NewGuid()</c> vergeben (das wäre eine nicht über den Seed
    /// gesteuerte Zufallsquelle und würde die geforderte exakte
    /// Reproduzierbarkeit gefährden, siehe auch die Begründung bei
    /// <see cref="Unit.CreateClone"/>), sondern rein positionsbasiert
    /// abgeleitet: Team A erhält die Ids 1-3, Team B die Ids 101-103, in der
    /// Reihenfolge der Ausgangslage. Das genügt für Eindeutigkeit INNERHALB
    /// einer Partie, mehr wird nirgends vorausgesetzt.
    /// </summary>
    public static GameState BuildGameState(GameSetup setup, TacticsCore.Board.Board board)
    {
        var state = new GameState(board);

        int idCounter = 1;
        foreach (var (hero, position) in setup.TeamAUnits)
        {
            state.AddUnit(new Hero(DeterministicId(idCounter++), Team.TeamA, position, hero));
        }

        idCounter = 101;
        foreach (var (hero, position) in setup.TeamBUnits)
        {
            state.AddUnit(new Hero(DeterministicId(idCounter++), Team.TeamB, position, hero));
        }

        return state;
    }

    private static Guid DeterministicId(int n) => new(n, 0, 0, new byte[8]);
}
