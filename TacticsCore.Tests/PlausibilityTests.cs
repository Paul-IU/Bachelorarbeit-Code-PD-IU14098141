using TacticsCore.Agents;
using TacticsCore.Board;
using TacticsCore.Match;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Automatisiertes Gegenstück zur im Konsolenprogramm ausgegebenen
/// Plausibilitätsprüfung: bei identischen Standardfiguren auf beiden
/// Seiten und Zufallsagenten sollte die Siegverteilung über viele Partien
/// annähernd ausgeglichen sein. Eine deutliche Schieflage würde entweder
/// auf einen strukturellen Vorteil des Startteams oder auf einen
/// Implementierungsfehler in Zugreihenfolge/Kampf/Bewegung hindeuten.
///
/// Bewusst als Regressions-Netz mit großzügiger Toleranz gedacht, nicht als
/// strenger statistischer Signifikanztest - soll grobe Fehler auffangen,
/// nicht jede kleine Schwankung.
/// </summary>
public class PlausibilityTests
{
    [Fact]
    public void SiegverteilungBeiZufallsagentenAufBeidenSeiten_IstAnnaeherndAusgeglichen()
    {
        const int sampleSize = 300;
        var board = new GameBoard();

        int teamAWins = 0;
        int teamBWins = 0;
        int draws = 0;

        for (int seed = 0; seed < sampleSize; seed++)
        {
            var state = new GameState(board);
            int counter = 1;
            foreach (var startField in board.GetStartFields(FieldType.BlossomA))
            {
                state.AddUnit(new Unit(new Guid(counter++, 0, 0, new byte[8]), Team.TeamA, startField.Coordinate));
            }
            foreach (var startField in board.GetStartFields(FieldType.BlossomB))
            {
                state.AddUnit(new Unit(new Guid(counter++, 0, 0, new byte[8]), Team.TeamB, startField.Coordinate));
            }

            var result = new MatchController(state, new RandomAgent(), new RandomAgent(), seed).Run();

            switch (result.Winner)
            {
                case MatchWinner.TeamA:
                    teamAWins++;
                    break;
                case MatchWinner.TeamB:
                    teamBWins++;
                    break;
                default:
                    draws++;
                    break;
            }
        }

        double teamAWinRate = (double)teamAWins / sampleSize;

        // Großzügige Toleranz: bei symmetrischen Standardfiguren sollte die
        // Gewinnrate von Team A klar zwischen 30% und 70% liegen. Dient als
        // Regressionsschutz gegen z.B. einen unbeabsichtigten Vorteil des
        // Startteams, nicht als exakte Gleichverteilungs-Behauptung.
        Assert.True(teamAWinRate is >= 0.30 and <= 0.70,
            $"Siegrate von Team A ({teamAWinRate:P1}) über {sampleSize} Partien liegt außerhalb der erwarteten Bandbreite " +
            $"(TeamA={teamAWins}, TeamB={teamBWins}, Draws={draws}) - möglicher struktureller Vorteil des Startteams oder Implementierungsfehler.");
    }
}
