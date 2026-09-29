using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>Fortschrittsanzeige für Batch-Läufe (<see cref="BatchProgressReporter"/>).</summary>
public class BatchProgressReporterTests
{
    [Fact]
    public void Format_EnthaeltPartienProzentPaarungenZeitUndRestzeit()
    {
        var reporter = new BatchProgressReporter(progressFilePath: null, verifyNameIndependence: false);

        // Nach 1 h ist ein Viertel geschafft -> Restzeit 3 h.
        string text = reporter.Format(new BatchProgress(892, 3570, 64260, 257040, TimeSpan.FromHours(1)), finished: false);

        Assert.Contains("Partien 64.260 / 257.040", text);
        Assert.Contains("25,0", text);
        Assert.Contains("Paarungen 892 / 3.570", text);
        Assert.Contains("vergangen 1:00:00", text);
        Assert.Contains("Rest ca. 3:00:00", text);
    }

    [Fact]
    public void Format_WeistBeiEtikettenPruefungDiePruefpartienAus()
    {
        var reporter = new BatchProgressReporter(progressFilePath: null, verifyNameIndependence: true);
        string text = reporter.Format(new BatchProgress(0, 3570, 100, 257040, TimeSpan.FromSeconds(10)), finished: false);
        Assert.Contains("Prüfpartien", text);
    }

    [Fact]
    public void Datei_WirdGeschriebenUndBeimAbschlussAlsAbgeschlossenMarkiert()
    {
        string pfad = Path.Combine(Path.GetTempPath(), $"fortschritt_test_{Guid.NewGuid():N}.txt");
        try
        {
            var reporter = new BatchProgressReporter(pfad, verifyNameIndependence: false, interval: TimeSpan.Zero);
            reporter.Report(new BatchProgress(0, 10, 1, 100, TimeSpan.FromSeconds(1)));
            Assert.Contains("Partien 1 / 100", File.ReadAllText(pfad));

            reporter.Finish(new BatchProgress(10, 10, 100, 100, TimeSpan.FromSeconds(50)));
            string inhalt = File.ReadAllText(pfad);
            Assert.Contains("Partien 100 / 100", inhalt);
            Assert.Contains("ABGESCHLOSSEN", inhalt);
        }
        finally
        {
            File.Delete(pfad);
        }
    }

    /// <summary>Die Anzeige hat keinen Einfluss auf die Ergebnisse - mit und ohne Reporter bitgleich, auch mehrfädig.</summary>
    [Fact]
    public void Ergebnisse_SindMitUndOhneAnzeigeIdentisch()
    {
        var comps = CompositionGenerator.GenerateAllCompositions(DefaultHeroes.CreateDefaultCatalog());
        var pairings = new[] { (comps[0], comps[5]), (comps[3], comps[3]), (comps[10], comps[40]) };
        var agents = AgentConfiguration.RandomVsRandom();
        var reporter = new BatchProgressReporter(progressFilePath: null, verifyNameIndependence: false, interval: TimeSpan.Zero);

        var ohne = BatchSimulationRunner.Run(new GameBoard(), pairings, agents, baseSeed: 7, maxSetupsPerPairing: 12, degreeOfParallelism: 8);
        var mit = BatchSimulationRunner.Run(new GameBoard(), pairings, agents, baseSeed: 7, maxSetupsPerPairing: 12, degreeOfParallelism: 8, onProgress: reporter.Report);

        var leer = new Dictionary<string, HeroActionCounts>();
        Assert.Equal(ohne.PairingResults.Select(r => r with { HeroActionTotals = leer }), mit.PairingResults.Select(r => r with { HeroActionTotals = leer }));
        for (int i = 0; i < ohne.PairingResults.Count; i++)
        {
            Assert.Equal(ohne.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                         mit.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal));
        }
    }
}
