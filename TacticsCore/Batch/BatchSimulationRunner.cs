using System.Diagnostics;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;

namespace TacticsCore.Batch;

/// <summary>
/// Spielt eine Menge von Paarungen (Anforderung 5) über ALLE (oder eine
/// begrenzte Auswahl, siehe <paramref name="maxSetupsPerPairing"/> unten)
/// ihrer Ausgangslagen durch und aggregiert die Ergebnisse je Paarung.
///
/// ---------------------------------------------------------------------
/// REPRODUZIERBARKEIT UNTER PARALLELISIERUNG (Anforderung 11) - zentrale
/// Voraussetzung dieser Untersuchung, daher hier ausführlich begründet:
/// ---------------------------------------------------------------------
/// Das Ergebnis eines Laufs muss bei gleichem Eingang (Katalog, Paarungen,
/// Agentenkonfiguration, Basis-Seed) UNABHÄNGIG von der Anzahl genutzter
/// Threads exakt identisch sein. Das wird durch drei getrennte Maßnahmen
/// sichergestellt:
///
///   1. JEDE einzelne Partie bekommt ihren Seed rein ARITHMETISCH aus
///      (Basis-Seed, Paarungsindex, Ausgangslagen-Index) hergeleitet (siehe
///      <see cref="DeriveSeed"/>) - eine reine Formel ohne jede
///      Nebenläufigkeits- oder Ausführungsreihenfolge-Abhängigkeit. KEIN
///      <c>HashCode.Combine</c> und KEIN <c>string.GetHashCode()</c>: .NET
///      randomisiert deren konkreten Rückgabewert standardmäßig pro
///      Prozess (Sicherheitsmaßnahme gegen Hash-Flooding) - ein darauf
///      aufgebauter Seed wäre also selbst bei IDENTISCHEM Eingang zwischen
///      zwei Programmläufen nicht reproduzierbar, unabhängig von jeder
///      Parallelisierungsfrage.
///
///   2. Jede Partie schreibt ihr Ergebnis in einen ihr FEST zugeordneten
///      Index eines vorab in voller Größe angelegten Arrays
///      (<c>outcomes[setupIndex]</c>). Nebenläufig ausgeführte Partien
///      konkurrieren dadurch nie um denselben Speicherplatz - jeder Thread
///      schreibt exklusiv "seinen" Index, unabhängig davon, WANN er im
///      Vergleich zu den anderen fertig wird.
///
///   3. Die AGGREGATION (Summenbildung für die Mittelwerte, Auszählen der
///      Siege) geschieht ausschließlich NACH Abschluss ALLER Partien einer
///      Paarung, und zwar STRIKT SEQUENTIELL in Index-Reihenfolge (siehe
///      <see cref="Aggregate"/>). Das ist kein Stildetail: Gleitkomma-
///      Addition ist nicht assoziativ (<c>(a+b)+c</c> kann sich von
///      <c>a+(b+c)</c> im letzten Bit unterscheiden) - würde stattdessen
///      z.B. direkt beim Eintreffen jedes Partie-Ergebnisses ein gemeinsamer
///      Summen-Akkumulator aktualisiert, hinge das Ergebnis von der
///      (nichtdeterministischen) Reihenfolge ab, in der die Threads fertig
///      werden. Die feste Index-Reihenfolge der sequentiellen
///      Nachbearbeitung macht das Gesamtergebnis dagegen komplett
///      unabhängig von <paramref name="degreeOfParallelism"/> - probiert in
///      <c>BatchSimulationRunnerTests</c> explizit mit 1 und mit mehreren
///      Threads gegeneinander.
///
/// Die eigentliche Parallelisierung selbst (<see cref="System.Threading.Tasks.Parallel.For"/>)
/// ist damit "embarrassingly parallel": jede Partie ist von jeder anderen
/// vollständig unabhängig, es gibt keinen geteilten veränderlichen Zustand
/// außer dem rein additiven Fortschritts-Zähler (<see cref="Interlocked.Increment(ref int)"/>),
/// der selbst nirgends in die Ergebnis-Berechnung einfließt, sondern
/// ausschließlich der Fortschrittsanzeige dient.
/// </summary>
public static class BatchSimulationRunner
{
    private readonly record struct MatchOutcome(MatchWinner Winner, MatchEndReason EndReason, int RoundsPlayed, double TeamARemainingHpFraction, double TeamBRemainingHpFraction);

    /// <param name="board">Das (immer gleiche) Spielfeld.</param>
    /// <param name="pairings">Die zu simulierenden Paarungen (Team-A- gegen Team-B-Komposition).</param>
    /// <param name="agents">Entscheidungslogik beider Seiten (siehe <see cref="AgentConfiguration"/>).</param>
    /// <param name="baseSeed">Basis-Seed, aus dem JEDE Einzelpartie deterministisch ihren eigenen Seed ableitet (siehe Klassendokumentation).</param>
    /// <param name="maxSetupsPerPairing">Optionale Begrenzung der je Paarung tatsächlich gespielten Ausgangslagen (von 72) - für schnelle Stichproben/Testläufe. Null = alle.</param>
    /// <param name="useAntitheticPairing">
    /// Spielt zu JEDER Ausgangslage zusätzlich, mit demselben Seed, die
    /// teamvertauschte Variante (<see cref="GameSetup.WithSwappedTeams"/>) -
    /// eine klassische Varianzreduktions-Technik ("antithetische Paare") aus
    /// der Monte-Carlo-Simulation. Bei identischer Komposition auf beiden
    /// Seiten (Spiegelpaarung) ergibt sich dadurch nachweisbar eine EXAKTE
    /// 50%-Siegquote (siehe <see cref="GameSetup.WithSwappedTeams"/> für die
    /// Begründung, warum das exakt statt nur näherungsweise gilt).
    /// </param>
    /// <param name="degreeOfParallelism">Maximale Anzahl gleichzeitig simulierter Partien; 1 = sequentiell.</param>
    /// <param name="onProgress">Optionaler Fortschritts-Empfänger (Anforderung 10) - wird aus BELIEBIGEM Thread aufgerufen, muss also selbst thread-sicher sein.</param>
    public static SimulationRunResult Run(
        TacticsCore.Board.Board board,
        IReadOnlyList<(TeamComposition TeamA, TeamComposition TeamB)> pairings,
        AgentConfiguration agents,
        int baseSeed,
        int? maxSetupsPerPairing = null,
        bool useAntitheticPairing = false,
        int degreeOfParallelism = 1,
        Action<BatchProgress>? onProgress = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // Ausgangslagen je Paarung vorab bestimmen (rein deterministisch,
        // siehe AusgangslagenGenerator) - auch, um die Gesamtzahl der
        // Partien für die Fortschrittsanzeige im Voraus zu kennen.
        var setupsPerPairing = pairings
            .Select(p =>
            {
                var setups = AusgangslagenGenerator.GenerateAllSetups(p.TeamA, p.TeamB, board);
                return maxSetupsPerPairing is int max && setups.Count > max ? setups.Take(max).ToList() : setups;
            })
            .ToList();

        int totalMatches = setupsPerPairing.Sum(s => s.Count) * (useAntitheticPairing ? 2 : 1);
        int completedMatches = 0;

        var pairingResults = new PairingResult[pairings.Count];

        for (int pairingIndex = 0; pairingIndex < pairings.Count; pairingIndex++)
        {
            var setups = setupsPerPairing[pairingIndex];
            int gamesPerSetup = useAntitheticPairing ? 2 : 1;
            var outcomes = new MatchOutcome[setups.Count * gamesPerSetup];

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
            int capturedPairingIndex = pairingIndex;

            Parallel.For(0, setups.Count, parallelOptions, setupIndex =>
            {
                int seed = DeriveSeed(baseSeed, capturedPairingIndex, setupIndex);
                var setup = setups[setupIndex];

                outcomes[setupIndex * gamesPerSetup] = PlayOne(setup, board, agents, seed);
                ReportProgress();

                if (useAntitheticPairing)
                {
                    outcomes[setupIndex * gamesPerSetup + 1] = PlayOne(setup.WithSwappedTeams(), board, agents, seed);
                    ReportProgress();
                }
            });

            pairingResults[pairingIndex] = Aggregate(pairings[pairingIndex].TeamA, pairings[pairingIndex].TeamB, outcomes);

            void ReportProgress()
            {
                if (onProgress is null)
                {
                    return;
                }

                int completedNow = Interlocked.Increment(ref completedMatches);
                onProgress(new BatchProgress(capturedPairingIndex, pairings.Count, completedNow, totalMatches, stopwatch.Elapsed));
            }
        }

        return new SimulationRunResult(pairingResults, stopwatch.Elapsed);
    }

    /// <summary>
    /// Rein arithmetische, von Ausführungsreihenfolge/Threadanzahl
    /// UNABHÄNGIGE Ableitung eines Partie-Seeds - siehe Klassendokumentation,
    /// Punkt 1. Die konkreten Faktoren (große, ungerade Primzahlen) dienen
    /// nur dazu, dass unterschiedliche (Paarung, Ausgangslage)-Kombinationen
    /// nicht zufällig denselben Seed erzeugen - keine kryptographische
    /// Anforderung, nur Kollisionsvermeidung für die Praxis.
    /// </summary>
    private static int DeriveSeed(int baseSeed, int pairingIndex, int setupIndex) =>
        unchecked(baseSeed * 1_000_003 + pairingIndex * 10_007 + setupIndex);

    private static MatchOutcome PlayOne(GameSetup setup, TacticsCore.Board.Board board, AgentConfiguration agents, int seed)
    {
        var state = AusgangslagenGenerator.BuildGameState(setup, board);
        var controller = new MatchController(state, agents.CreateTeamAAgent(), agents.CreateTeamBAgent(), seed);
        var result = controller.Run(setup.StartingTeam);

        return new MatchOutcome(result.Winner, result.EndReason, result.RoundsPlayed, result.TeamAResult.RemainingHitPointsFraction, result.TeamBResult.RemainingHitPointsFraction);
    }

    /// <summary>Siehe Klassendokumentation, Punkt 3: rein sequentielle Nachbearbeitung in fester Index-Reihenfolge.</summary>
    private static PairingResult Aggregate(TeamComposition teamA, TeamComposition teamB, MatchOutcome[] outcomes)
    {
        int teamAWins = 0, teamBWins = 0, draws = 0, roundLimitEndings = 0;
        long totalRounds = 0;
        double totalTeamAFraction = 0.0, totalTeamBFraction = 0.0;

        foreach (var outcome in outcomes)
        {
            switch (outcome.Winner)
            {
                case MatchWinner.TeamA: teamAWins++; break;
                case MatchWinner.TeamB: teamBWins++; break;
                default: draws++; break;
            }

            if (outcome.EndReason == MatchEndReason.RoundLimit)
            {
                roundLimitEndings++;
            }

            totalRounds += outcome.RoundsPlayed;
            totalTeamAFraction += outcome.TeamARemainingHpFraction;
            totalTeamBFraction += outcome.TeamBRemainingHpFraction;
        }

        int n = outcomes.Length;
        return new PairingResult(
            teamA, teamB, teamAWins, teamBWins, draws, n,
            AverageRoundsPlayed: n == 0 ? 0.0 : (double)totalRounds / n,
            AverageTeamARemainingHitPointsFraction: n == 0 ? 0.0 : totalTeamAFraction / n,
            AverageTeamBRemainingHitPointsFraction: n == 0 ? 0.0 : totalTeamBFraction / n,
            RoundLimitShare: n == 0 ? 0.0 : (double)roundLimitEndings / n);
    }
}
