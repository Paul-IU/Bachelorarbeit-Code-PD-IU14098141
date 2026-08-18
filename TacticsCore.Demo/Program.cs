// Demo-/Verifikationsprogramm für die Rundenverwaltung: führt eine
// vollständige, per Zufallsagenten gespielte 3-vs-3-Partie durch und gibt
// Ausgangsstellung, Rundenverlauf (aus dem Replay-Protokoll) und
// Endergebnis aus. Anschließend eine Batch-Simulation über viele Partien
// mit unterschiedlichen Seeds, als Plausibilitätsprüfung der
// Siegverteilung (siehe Aufgabenstellung: bei identischen Standardfiguren
// auf beiden Seiten sollte diese annähernd ausgeglichen sein). Enthält
// keine eigene Spiellogik, sondern nutzt ausschließlich die öffentliche
// API von TacticsCore.

using TacticsCore;
using TacticsCore.Agents;
using TacticsCore.Board;
using TacticsCore.Match;
using TacticsCore.Rendering;
using TacticsCore.Units;

RunSingleNarratedMatch();
Console.WriteLine();
Console.WriteLine(new string('=', 76));
Console.WriteLine();
RunBatchSimulation(matchCount: 1000000);

// =======================================================================
// Teil 1: eine einzelne, vollständig nacherzählte Partie.
// =======================================================================
static void RunSingleNarratedMatch()
{
    Console.WriteLine("=== TacticsCore - Einzelpartie mit Rundenverlauf (Zufallsagenten) ===");
    Console.WriteLine();

    const int seed = 42; // fest gewählt, damit die Ausgabe bei jedem Lauf des Demos identisch ist
    var (state, _) = BuildStandardLineup();

    // Statt anonymer GUID-Kürzel bekommt jede Einheit einen kurzen, gut
    // unterscheidbaren Namen: Team A = A/B/C, Team B = X/Y/Z. Rein eine
    // Darstellungsfrage des Demos - TacticsCore selbst kennt keine Namen.
    var names = BuildDisplayNames(state);

    Console.WriteLine("Ausgangsstellung:");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));

    var controller = new MatchController(state, new RandomAgent(), new RandomAgent(), seed, recordReplay: true);
    var result = controller.Run();

    Console.WriteLine($"Seed: {seed}   Startteam: {result.StartingTeam}");
    Console.WriteLine();

    // Der komplette Rundenverlauf lässt sich allein aus dem Replay-
    // Protokoll rekonstruieren - die Partie selbst wurde dafür kein
    // zweites Mal ausgeführt.
    foreach (var roundGroup in result.Replay!.Entries.GroupBy(e => e.RoundNumber).OrderBy(g => g.Key))
    {
        Console.WriteLine($"Runde {roundGroup.Key}:");
        foreach (var entry in roundGroup)
        {
            string actorName = Name(names, entry.ActingUnitId);
            if (entry.ActionType == ReplayActionType.Move)
            {
                Console.WriteLine($"  [{entry.ActingTeam}] {actorName} zieht von {entry.MoveSource} nach {entry.MoveTarget}");
            }
            else
            {
                string targetName = Name(names, entry.AttackTargetUnitId!.Value);
                Console.WriteLine($"  [{entry.ActingTeam}] {actorName} greift {targetName} an ({entry.DamageDealt} Schaden)");
            }
        }
    }

    Console.WriteLine();
    Console.WriteLine("--- Endergebnis ---");
    Console.WriteLine($"Sieger: {result.Winner}   Ende durch: {result.EndReason}   Runden gespielt: {result.RoundsPlayed}");
    PrintTeamResult(result.TeamAResult);
    PrintTeamResult(result.TeamBResult);
    Console.WriteLine();

    Console.WriteLine("Endstellung:");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));
}

static void PrintTeamResult(TeamResult team)
{
    Console.WriteLine($"  {team.Team}: {team.SurvivingUnitCount} überlebende Einheit(en), " +
        $"{team.TotalCurrentHitPoints}/{team.TotalMaxHitPoints} HP ({team.RemainingHitPointsFraction:P1})");
}

/// <summary>Weist Team A die Bezeichnungen A/B/C und Team B die Bezeichnungen X/Y/Z zu (stabil nach Startposition sortiert).</summary>
static Dictionary<Guid, string> BuildDisplayNames(GameState state)
{
    var names = new Dictionary<Guid, string>();
    AssignLabels(state.Units.Where(u => u.Team == Team.TeamA), new[] { "A", "B", "C" }, names);
    AssignLabels(state.Units.Where(u => u.Team == Team.TeamB), new[] { "X", "Y", "Z" }, names);
    return names;
}

static void AssignLabels(IEnumerable<Unit> teamUnits, string[] labels, Dictionary<Guid, string> target)
{
    var ordered = teamUnits.OrderBy(u => u.Position.Q).ThenBy(u => u.Position.R).ThenBy(u => u.Id).ToList();
    for (int i = 0; i < ordered.Count; i++)
    {
        // Fallback für den (aktuell nicht vorkommenden) Fall von mehr als
        // 3 Einheiten pro Team, damit die Zuordnung nie eine Einheit ausließe.
        target[ordered[i].Id] = i < labels.Length ? labels[i] : $"{labels[0]}{i + 1}";
    }
}

static string Name(Dictionary<Guid, string> names, Guid id) =>
    names.TryGetValue(id, out var name) ? name : id.ToString("N")[..8];

// =======================================================================
// Teil 2: Batch-Simulation über viele Partien - Plausibilitätsprüfung.
// =======================================================================
static void RunBatchSimulation(int matchCount)
{
    Console.WriteLine($"=== TacticsCore - Batch-Simulation ({matchCount} Partien) ===");
    Console.WriteLine();

    int teamAWins = 0;
    int teamBWins = 0;
    int draws = 0;
    int roundLimitEndings = 0;
    long totalRoundsPlayed = 0;

    // Jede Partie bekommt ihren eigenen Seed (hier schlicht der laufende
    // Index) - kein Replay-Protokoll nötig, das würde bei 1000 Partien nur
    // unnötig Speicher belegen (siehe Performance-Anforderung).
    for (int seed = 0; seed < matchCount; seed++)
    {
        var (state, _) = BuildStandardLineup();
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

        if (result.EndReason == MatchEndReason.RoundLimit)
        {
            roundLimitEndings++;
        }

        totalRoundsPlayed += result.RoundsPlayed;
    }

    double teamAWinRate = (double)teamAWins / matchCount;
    double teamBWinRate = (double)teamBWins / matchCount;
    double drawRate = (double)draws / matchCount;
    double averageRounds = (double)totalRoundsPlayed / matchCount;
    double roundLimitShare = (double)roundLimitEndings / matchCount;

    Console.WriteLine($"Team A Siege:          {teamAWins,5}  ({teamAWinRate:P1})");
    Console.WriteLine($"Team B Siege:          {teamBWins,5}  ({teamBWinRate:P1})");
    Console.WriteLine($"Unentschieden:         {draws,5}  ({drawRate:P1})");
    Console.WriteLine();
    Console.WriteLine($"Durchschnittliche Rundenzahl:      {averageRounds:F1}");
    Console.WriteLine($"Anteil Rundenlimit-Enden:          {roundLimitShare:P1}");
    Console.WriteLine();
    Console.WriteLine("Plausibilitätshinweis: Bei identischen Standardfiguren und Zufallsagenten auf");
    Console.WriteLine("beiden Seiten sollte die Siegverteilung annähernd ausgeglichen sein. Eine");
    Console.WriteLine("deutliche Abweichung würde auf einen strukturellen Vorteil des Startteams");
    Console.WriteLine("oder auf einen Implementierungsfehler hindeuten.");
}

// =======================================================================
// Gemeinsamer Aufbau: 3 Einheiten pro Team auf den jeweiligen Startfeldern.
// =======================================================================
static (GameState State, Board Board) BuildStandardLineup()
{
    var board = new Board();
    var state = new GameState(board);

    foreach (var startField in board.GetStartFields(FieldType.BlossomA))
    {
        state.AddUnit(new Unit(Guid.NewGuid(), Team.TeamA, startField.Coordinate));
    }

    foreach (var startField in board.GetStartFields(FieldType.BlossomB))
    {
        state.AddUnit(new Unit(Guid.NewGuid(), Team.TeamB, startField.Coordinate));
    }

    return (state, board);
}
