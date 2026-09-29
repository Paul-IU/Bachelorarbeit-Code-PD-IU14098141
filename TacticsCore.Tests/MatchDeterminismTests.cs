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
/// Prüft die für die spätere Massen-Simulation zentrale Anforderung:
/// gleicher Seed + gleiche Ausgangslage ergeben exakt denselben
/// Partieverlauf, unterschiedliche Seeds ergeben (mit an Sicherheit
/// grenzender Wahrscheinlichkeit) unterschiedliche Verläufe. Verwendet
/// bewusst <see cref="RandomAgentByActionType"/> auf beiden Seiten, da dort die
/// einzige nennenswerte Zufallsquelle innerhalb eines Zugs entsteht.
/// </summary>
public class MatchDeterminismTests
{
    /// <summary>
    /// Baut dieselbe 3-vs-3-Ausgangsaufstellung wie das Konsolenprogramm,
    /// aber mit FESTEN statt zufällig erzeugten UnitIds
    /// (<c>Guid.NewGuid()</c>). Das ist hier bewusst so und kein Widerspruch
    /// zur "nur Seed"-Regel: UnitIds sind Teil der Ausgangslage, nicht der
    /// eigentlichen Simulationszufälligkeit (siehe Kommentar in
    /// TurnOrder) - für einen Reproduzierbarkeits-VERGLEICH zweier Partien
    /// müssen daher zwei Aufstellungen mit identischen UnitIds erzeugt
    /// werden, sonst würde allein die - für den Test irrelevante -
    /// ID-Vergabe künstlich für Abweichungen sorgen.
    /// </summary>
    private static GameState BuildFixedLineup(GameBoard board)
    {
        var state = new GameState(board);
        int counter = 1;

        foreach (var startField in board.GetStartFields(FieldType.BlossomA))
        {
            state.AddUnit(new Unit(FixedId(counter++), Team.TeamA, startField.Coordinate));
        }

        foreach (var startField in board.GetStartFields(FieldType.BlossomB))
        {
            state.AddUnit(new Unit(FixedId(counter++), Team.TeamB, startField.Coordinate));
        }

        return state;
    }

    private static Guid FixedId(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void GleicherSeedUndGleicheAusgangslage_ErgibtExaktDenselbenPartieverlaufUndDasselbeErgebnis()
    {
        var board = new GameBoard();

        var result1 = new MatchController(BuildFixedLineup(board), new RandomAgentByActionType(), new RandomAgentByActionType(), seed: 12345, recordReplay: true).Run();
        var result2 = new MatchController(BuildFixedLineup(board), new RandomAgentByActionType(), new RandomAgentByActionType(), seed: 12345, recordReplay: true).Run();

        Assert.Equal(result1.Winner, result2.Winner);
        Assert.Equal(result1.EndReason, result2.EndReason);
        Assert.Equal(result1.RoundsPlayed, result2.RoundsPlayed);
        Assert.Equal(result1.StartingTeam, result2.StartingTeam);
        Assert.Equal(result1.TeamAResult, result2.TeamAResult);
        Assert.Equal(result1.TeamBResult, result2.TeamBResult);

        // Der stärkste Beleg für "exakt derselbe Verlauf", nicht nur
        // dasselbe Endergebnis: das vollständige Replay-Protokoll ist
        // Eintrag für Eintrag identisch.
        Assert.NotNull(result1.Replay);
        Assert.NotNull(result2.Replay);
        Assert.Equal(result1.Replay!.Entries, result2.Replay!.Entries);
    }

    [Fact]
    public void GleicherSeedUndGleicheAusgangslage_MehrfacheAusfuehrung_BleibtStetsIdentisch()
    {
        // Derselbe Vergleich wie oben, aber dreifach statt zweifach - schließt
        // aus, dass zwei Läufe "zufällig" übereinstimmen.
        var board = new GameBoard();

        var replays = Enumerable.Range(0, 3)
            .Select(_ => new MatchController(BuildFixedLineup(board), new RandomAgentByActionType(), new RandomAgentByActionType(), seed: 777, recordReplay: true).Run().Replay!.Entries)
            .ToList();

        Assert.Equal(replays[0], replays[1]);
        Assert.Equal(replays[0], replays[2]);
    }

    [Fact]
    public void UnterschiedlicheSeeds_ErgebenUnterschiedlichePartieverlaeufe()
    {
        var board = new GameBoard();
        var results = new List<MatchResult>();

        for (int seed = 0; seed < 20; seed++)
        {
            var controller = new MatchController(BuildFixedLineup(board), new RandomAgentByActionType(), new RandomAgentByActionType(), seed, recordReplay: true);
            results.Add(controller.Run());
        }

        // Über 20 verschiedene Seeds dürfen nicht alle Partieverläufe
        // identisch sein - sonst würde der Seed die Simulation faktisch
        // nicht beeinflussen (Implementierungsfehler, z.B. eine
        // versehentlich statische/globale Zufallsquelle).
        int distinctEntryCounts = results.Select(r => r.Replay!.Entries.Count).Distinct().Count();
        int distinctRoundsPlayed = results.Select(r => r.RoundsPlayed).Distinct().Count();
        int distinctStartingTeams = results.Select(r => r.StartingTeam).Distinct().Count();

        Assert.True(distinctEntryCounts > 1 || distinctRoundsPlayed > 1,
            "Erwartet, dass unterschiedliche Seeds zu unterschiedlich langen/verlaufenden Partien führen.");
        Assert.True(distinctStartingTeams > 1,
            "Erwartet, dass unterschiedliche Seeds nicht immer dasselbe Startteam auswürfeln.");
    }
}
