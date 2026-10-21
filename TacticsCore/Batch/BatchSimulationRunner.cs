using System.Diagnostics;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

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
    private readonly record struct MatchOutcome(
        MatchWinner Winner,
        MatchEndReason EndReason,
        int RoundsPlayed,
        double TeamARemainingHpFraction,
        double TeamBRemainingHpFraction,
        IReadOnlyDictionary<string, HeroActionCounts> HeroActionCounts,
        RoundLimitResolution? Resolution,
        int Seed,
        Team StartingTeam,
        int TeamASurvivingHeroes,
        int TeamBSurvivingHeroes,
        int TeamARemainingHitPoints,
        int TeamBRemainingHitPoints);

    /// <param name="board">Das (immer gleiche) Spielfeld.</param>
    /// <param name="pairings">Die zu simulierenden Paarungen (Team-A- gegen Team-B-Komposition).</param>
    /// <param name="agents">Entscheidungslogik beider Seiten (siehe <see cref="AgentConfiguration"/>).</param>
    /// <param name="baseSeed">Basis-Seed, aus dem JEDE Einzelpartie deterministisch ihren eigenen Seed ableitet (siehe Klassendokumentation).</param>
    /// <param name="maxSetupsPerPairing">Optionale Begrenzung der je Paarung tatsächlich gespielten Ausgangslagen (von 72) - für schnelle Stichproben/Testläufe. Null = alle.</param>
    /// <param name="useAntitheticPairing">
    /// Spielt zu JEDER Ausgangslage zusätzlich, mit demselben Seed, die
    /// teamvertauschte Variante (<see cref="GameSetup.WithSwappedTeams"/>).
    /// ACHTUNG: das ist KEINE Varianzreduktion im Sinne der Monte-Carlo-
    /// Simulation, sondern eine Namensunabhängigkeits-Prüfung: die vertauschte
    /// Partie ist physisch eine Dublette der ursprünglichen. Bei identischer
    /// Komposition auf beiden Seiten (Spiegelpaarung) gewinnen beide Etiketten
    /// dadurch exakt gleich oft (siehe <see cref="GameSetup.WithSwappedTeams"/>
    /// für die Begründung und die Grenzen dieser Aussage); bei VERSCHIEDENEN
    /// Kompositionen ist das Ergebnis dagegen immer exakt 50:50 und damit
    /// ohne Aussagekraft - dort NICHT für die Auswertung verwenden. Der
    /// vollständige Basislauf (full-baseline) nutzt sie bewusst nicht.
    /// </param>
    /// <param name="degreeOfParallelism">Maximale Anzahl gleichzeitig simulierter Partien; 1 = sequentiell.</param>
    /// <param name="onProgress">Optionaler Fortschritts-Empfänger (Anforderung 10) - wird aus BELIEBIGEM Thread aufgerufen, muss also selbst thread-sicher sein.</param>
    /// <param name="verifyNameIndependence">
    /// Optionale Etiketten-Prüfung (Standard aus, siehe <see cref="NameIndependenceCheck"/>):
    /// jede Partie wird zusätzlich mit vertauschten Team-Etiketten gespielt und
    /// ihr Ergebnis (Sieger, Endgrund, Rundenzahl, Rest-Lebenspunkte) auf
    /// Spiegelbildlichkeit geprüft. Die Prüfpartien fließen NICHT in die
    /// Ergebnisse ein - <see cref="SimulationRunResult.PairingResults"/> ist mit
    /// und ohne Option bitgenau gleich. Kostet etwa die doppelte Laufzeit.
    /// </param>
    /// <param name="recordMatches">
    /// Zusätzlich jede Partie einzeln zurückgeben (<see cref="SimulationRunResult.Matches"/>,
    /// Export siehe <see cref="CsvExporter.ExportMatches"/>). Die Partien stehen ohnehin
    /// je Paarung in einem Array mit festem Platz je Ausgangslage (siehe Punkt 2 der
    /// Klassendokumentation); sie werden nach Abschluss der Paarung in dieser
    /// Reihenfolge übernommen - die Zeilenfolge hängt damit nicht von der
    /// Threadanzahl ab. Beeinflusst die Aggregate nicht.
    /// </param>
    /// <param name="swapSides">
    /// Seiten vertauschen (Standard aus): Team A startet auf Blüte B, Team B auf
    /// Blüte A, über die Spiegelsymmetrie des Spielfelds (siehe
    /// <see cref="AusgangslagenGenerator.GenerateAllSetups"/>). Kompositionen,
    /// Ausgangslagen-Reihenfolge, beginnendes Team und Seeds bleiben gleich.
    /// </param>
    public static SimulationRunResult Run(
        TacticsCore.Board.Board board,
        IReadOnlyList<(TeamComposition TeamA, TeamComposition TeamB)> pairings,
        AgentConfiguration agents,
        int baseSeed,
        int? maxSetupsPerPairing = null,
        bool useAntitheticPairing = false,
        int degreeOfParallelism = 1,
        Action<BatchProgress>? onProgress = null,
        bool verifyNameIndependence = false,
        bool recordMatches = false,
        bool swapSides = false)
    {
        var stopwatch = Stopwatch.StartNew();

        // Ausgangslagen je Paarung vorab bestimmen (rein deterministisch,
        // siehe AusgangslagenGenerator) - auch, um die Gesamtzahl der
        // Partien für die Fortschrittsanzeige im Voraus zu kennen.
        var setupsPerPairing = pairings
            .Select(p =>
            {
                var setups = AusgangslagenGenerator.GenerateAllSetups(p.TeamA, p.TeamB, board, swapSides);
                return maxSetupsPerPairing is int max && setups.Count > max ? setups.Take(max).ToList() : setups;
            })
            .ToList();

        int totalMatches = setupsPerPairing.Sum(s => s.Count) * (useAntitheticPairing ? 2 : 1);
        int completedMatches = 0;

        var pairingResults = new PairingResult[pairings.Count];
        var matches = recordMatches ? new List<MatchRecord>(totalMatches) : null;
        var violations = verifyNameIndependence ? new List<NameIndependenceViolation>() : null;
        int checkedMatches = 0;

        for (int pairingIndex = 0; pairingIndex < pairings.Count; pairingIndex++)
        {
            var setups = setupsPerPairing[pairingIndex];
            int gamesPerSetup = useAntitheticPairing ? 2 : 1;
            var outcomes = new MatchOutcome[setups.Count * gamesPerSetup];
            // Je Ausgangslage ein fester Platz (wie outcomes) - Abweichungen
            // erscheinen dadurch unabhängig von der Threadanzahl in derselben
            // Reihenfolge.
            var setupViolations = verifyNameIndependence ? new NameIndependenceViolation?[setups.Count] : null;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
            int capturedPairingIndex = pairingIndex;

            Parallel.For(0, setups.Count, parallelOptions, setupIndex =>
            {
                int seed = DeriveSeed(baseSeed, capturedPairingIndex, setupIndex);
                var setup = setups[setupIndex];

                var original = PlayOne(setup, board, agents, seed, recordReplay: false);
                outcomes[setupIndex * gamesPerSetup] = ToOutcome(original);

                if (setupViolations is not null)
                {
                    setupViolations[setupIndex] = CheckNameIndependence(
                        pairings[capturedPairingIndex], capturedPairingIndex, setupIndex, setup, board, agents, seed, original);
                }

                ReportProgress();

                if (useAntitheticPairing)
                {
                    outcomes[setupIndex * gamesPerSetup + 1] = ToOutcome(PlayOne(setup.WithSwappedTeams(), board, agents, seed, recordReplay: false));
                    ReportProgress();
                }
            });

            if (setupViolations is not null)
            {
                checkedMatches += setups.Count;
                violations!.AddRange(setupViolations.Where(v => v is not null)!);
            }

            pairingResults[pairingIndex] = Aggregate(pairings[pairingIndex].TeamA, pairings[pairingIndex].TeamB, outcomes);

            if (matches is not null)
            {
                // Sequentiell in Index-Reihenfolge, NACH Abschluss aller Partien der Paarung.
                for (int i = 0; i < outcomes.Length; i++)
                {
                    var o = outcomes[i];
                    matches.Add(new MatchRecord(
                        pairingIndex, i / gamesPerSetup, TeamsSwapped: i % gamesPerSetup == 1, o.Seed, o.StartingTeam,
                        o.Winner, o.EndReason, o.Resolution, o.RoundsPlayed,
                        o.TeamASurvivingHeroes, o.TeamBSurvivingHeroes, o.TeamARemainingHitPoints, o.TeamBRemainingHitPoints));
                }
            }

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

        var nameIndependence = violations is null ? null : new NameIndependenceReport(checkedMatches, violations);
        return new SimulationRunResult(pairingResults, stopwatch.Elapsed, nameIndependence, matches);
    }

    /// <summary>
    /// Spielt dieselbe Ausgangslage mit vertauschten Etiketten und demselben
    /// Seed und vergleicht das Endergebnis. Nur bei einer Abweichung werden
    /// beide Partien mit Protokoll wiederholt (deterministisch, also mit
    /// identischem Verlauf), um den ersten nicht spiegelbildlichen Zug zu finden.
    /// </summary>
    private static NameIndependenceViolation? CheckNameIndependence(
        (TeamComposition TeamA, TeamComposition TeamB) pairing, int pairingIndex, int setupIndex,
        GameSetup setup, TacticsCore.Board.Board board, AgentConfiguration agents, int seed, MatchResult original)
    {
        var swappedSetup = setup.WithSwappedTeams();
        var swapped = PlayOne(swappedSetup, board, agents, seed, recordReplay: false);
        string? difference = NameIndependenceCheck.CompareOutcomes(original, swapped);
        if (difference is null)
        {
            return null;
        }

        var originalReplay = PlayOne(setup, board, agents, seed, recordReplay: true).Replay!.Entries;
        var swappedReplay = PlayOne(swappedSetup, board, agents, seed, recordReplay: true).Replay!.Entries;
        return new NameIndependenceViolation(
            pairing.TeamA, pairing.TeamB, pairingIndex, setupIndex, setup, seed, difference,
            NameIndependenceCheck.FindFirstDivergence(originalReplay, swappedReplay));
    }

    /// <summary>
    /// Rein arithmetische, von Ausführungsreihenfolge/Threadanzahl
    /// UNABHÄNGIGE Ableitung eines Partie-Seeds - siehe Klassendokumentation,
    /// Punkt 1. Die konkreten Faktoren (große, ungerade Primzahlen) dienen
    /// nur dazu, dass unterschiedliche (Paarung, Ausgangslage)-Kombinationen
    /// nicht zufällig denselben Seed erzeugen - keine kryptographische
    /// Anforderung, nur Kollisionsvermeidung für die Praxis.
    /// </summary>
    /// <summary>Wiederverwendete leere Instanz statt einer Neuzuteilung je Partie, falls (theoretisch) kein <see cref="MatchResult.HeroActionCountsByHeroId"/> vorhanden ist - in der Praxis liefert <see cref="MatchController"/> immer eine (ggf. leere) Zuordnung.</summary>
    private static readonly IReadOnlyDictionary<string, HeroActionCounts> EmptyHeroActionCounts = new Dictionary<string, HeroActionCounts>();

    /// <remarks>
    /// Öffentlich, damit eine einzelne Partie eines Batch-Laufs gezielt
    /// nachgespielt werden kann (Konsolenmodus "batch-match", Godot-Oberfläche):
    /// Paarungsindex = Zeile der Paarungs-CSV (0-basiert, ohne Kopfzeile),
    /// Ausgangslagen-Index = Position in <see cref="AusgangslagenGenerator.GenerateAllSetups"/>.
    /// </remarks>
    public static int DeriveSeed(int baseSeed, int pairingIndex, int setupIndex) =>
        unchecked(baseSeed * 1_000_003 + pairingIndex * 10_007 + setupIndex);

    private static MatchResult PlayOne(GameSetup setup, TacticsCore.Board.Board board, AgentConfiguration agents, int seed, bool recordReplay)
    {
        var state = AusgangslagenGenerator.BuildGameState(setup, board);
        var controller = new MatchController(state, agents.CreateTeamAAgent(), agents.CreateTeamBAgent(), seed, recordReplay);
        return controller.Run(setup.StartingTeam);
    }

    private static MatchOutcome ToOutcome(MatchResult result) =>
        new(
            result.Winner,
            result.EndReason,
            result.RoundsPlayed,
            result.TeamAResult.RemainingHitPointsFraction,
            result.TeamBResult.RemainingHitPointsFraction,
            result.HeroActionCountsByHeroId ?? EmptyHeroActionCounts,
            result.RoundLimitResolution,
            result.Seed,
            result.StartingTeam,
            result.TeamAResult.SurvivingUnitCount,
            result.TeamBResult.SurvivingUnitCount,
            result.TeamAResult.TotalCurrentHitPoints,
            result.TeamBResult.TotalCurrentHitPoints);

    /// <summary>Siehe Klassendokumentation, Punkt 3: rein sequentielle Nachbearbeitung in fester Index-Reihenfolge.</summary>
    private static PairingResult Aggregate(TeamComposition teamA, TeamComposition teamB, MatchOutcome[] outcomes)
    {
        int teamAWins = 0, teamBWins = 0, draws = 0, roundLimitEndings = 0, bySurvivingHeroes = 0, byHitPointFraction = 0;
        long totalRounds = 0;
        double totalTeamAFraction = 0.0, totalTeamBFraction = 0.0;
        var heroActionTotals = new Dictionary<string, HeroActionCounts>();

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

            if (outcome.Resolution == RoundLimitResolution.SurvivingHeroes)
            {
                bySurvivingHeroes++;
            }
            else if (outcome.Resolution == RoundLimitResolution.HitPointFraction)
            {
                byHitPointFraction++;
            }

            totalRounds += outcome.RoundsPlayed;
            totalTeamAFraction += outcome.TeamARemainingHpFraction;
            totalTeamBFraction += outcome.TeamBRemainingHpFraction;

            foreach (var (heroId, counts) in outcome.HeroActionCounts)
            {
                heroActionTotals.TryGetValue(heroId, out var existing);
                heroActionTotals[heroId] = existing + counts;
            }
        }

        int n = outcomes.Length;
        return new PairingResult(
            teamA, teamB, teamAWins, teamBWins, draws, n,
            AverageRoundsPlayed: n == 0 ? 0.0 : (double)totalRounds / n,
            AverageTeamARemainingHitPointsFraction: n == 0 ? 0.0 : totalTeamAFraction / n,
            AverageTeamBRemainingHitPointsFraction: n == 0 ? 0.0 : totalTeamBFraction / n,
            RoundLimitShare: n == 0 ? 0.0 : (double)roundLimitEndings / n,
            HeroActionTotals: heroActionTotals,
            RoundLimitSurvivingHeroes: bySurvivingHeroes,
            RoundLimitHitPointFraction: byHitPointFraction);
    }
}
