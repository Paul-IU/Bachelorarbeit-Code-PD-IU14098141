using System.Globalization;
using System.IO.Compression;
using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie die darin enthaltene Klasse.
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests des Exports je Partie (<see cref="MatchRecord"/>, <see cref="CsvExporter.ExportMatches"/>)
/// und der Rundenlimit-Spalten der Heldendatei: die Einzelpartien müssen sich exakt
/// zu den bestehenden Aggregaten aufsummieren, Reihenfolge und Inhalt dürfen nicht von
/// der Threadanzahl abhängen.
/// </summary>
public class MatchExportTests
{
    private static readonly HeroCatalog Catalog = DefaultHeroes.CreateDefaultCatalog();
    private static readonly IReadOnlyList<TeamComposition> Compositions = CompositionGenerator.GenerateAllCompositions(Catalog);

    /// <summary>Gemischte Paarungen: Spiegelpaarung, geteilter Held, völlig verschiedene Kompositionen.</summary>
    private static readonly (TeamComposition TeamA, TeamComposition TeamB)[] Pairings =
    {
        (Compositions[0], Compositions[0]),
        (Compositions[0], Compositions[1]),
        (Compositions[5], Compositions[60]),
        (Compositions[20], Compositions[83]),
        (Compositions[40], Compositions[40]),
        (Compositions[10], Compositions[70]),
    };

    private sealed record Zeile(
        int Paarung, string TeamA, string TeamB, int Ausgangslage, int Vertauscht, int Seed, string Startteam,
        string Sieger, string Endgrund, string Aufloesung, int Runden,
        int UeberlebendeA, int UeberlebendeB, int RestHpA, int RestHpB, string Agent);

    private static List<Zeile> Lies(string path, out string kopf)
    {
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        kopf = reader.ReadLine()!;
        var zeilen = new List<Zeile>();
        while (reader.ReadLine() is { } line)
        {
            var f = line.Split(';');
            Assert.Equal(16, f.Length);
            zeilen.Add(new Zeile(int.Parse(f[0]), f[1], f[2], int.Parse(f[3]), int.Parse(f[4]), int.Parse(f[5]), f[6],
                f[7], f[8], f[9], int.Parse(f[10]), int.Parse(f[11]), int.Parse(f[12]), int.Parse(f[13]), int.Parse(f[14]), f[15]));
        }
        return zeilen;
    }

    private static string Temp(string suffix) => Path.Combine(Path.GetTempPath(), $"tacticscore-{Guid.NewGuid():N}{suffix}");

    /// <summary>
    /// Die aus der geschriebenen Datei zurückgelesenen Einzelpartien summieren sich je
    /// Paarung exakt zu den Aggregaten der Paarungs-CSV: Partien, Siege, Unentschieden,
    /// Rundenlimit-Anteil, Auflösungsstufen, mittlere Runden und mittlerer
    /// Rest-Lebenspunkte-Anteil. Dazu Seed, Startteam und Kopfzeile.
    /// </summary>
    [Fact]
    public void Partiedatei_SummiertSichZuDenAggregatenDerPaarungen()
    {
        var board = new GameBoard();
        var result = BatchSimulationRunner.Run(board, Pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 2026,
            maxSetupsPerPairing: 24, degreeOfParallelism: 4, recordMatches: true);
        string path = Temp(".csv.gz");
        try
        {
            CsvExporter.ExportMatches(result.Matches!, Pairings, path, AgentNames.ByActionType);
            var zeilen = Lies(path, out string kopf);

            Assert.Equal("Paarung;TeamA;TeamB;Ausgangslage;Vertauscht;Seed;Startteam;Sieger;Endgrund;Aufloesung;Runden;" +
                "UeberlebendeTeamA;UeberlebendeTeamB;RestHpTeamA;RestHpTeamB;Agent", kopf);
            Assert.Equal(Pairings.Length * 24, zeilen.Count);

            for (int p = 0; p < Pairings.Length; p++)
            {
                var agg = result.PairingResults[p];
                var eigene = zeilen.Where(z => z.Paarung == p).ToList();
                var setups = AusgangslagenGenerator.GenerateAllSetups(Pairings[p].TeamA, Pairings[p].TeamB, board);
                int maxA = Pairings[p].TeamA.Heroes.Sum(h => h.MaxHitPoints), maxB = Pairings[p].TeamB.Heroes.Sum(h => h.MaxHitPoints);

                Assert.Equal(agg.TotalMatches, eigene.Count);
                Assert.Equal(Enumerable.Range(0, 24), eigene.Select(z => z.Ausgangslage)); // feste Reihenfolge
                Assert.All(eigene, z => Assert.Equal(Pairings[p].TeamA.Key, z.TeamA));
                Assert.All(eigene, z => Assert.Equal(Pairings[p].TeamB.Key, z.TeamB));
                Assert.All(eigene, z => Assert.Equal(BatchSimulationRunner.DeriveSeed(2026, p, z.Ausgangslage), z.Seed));
                Assert.All(eigene, z => Assert.Equal(setups[z.Ausgangslage].StartingTeam.ToString(), z.Startteam));
                Assert.All(eigene, z => Assert.Equal(0, z.Vertauscht));

                Assert.Equal(agg.TeamAWins, eigene.Count(z => z.Sieger == "TeamA"));
                Assert.Equal(agg.TeamBWins, eigene.Count(z => z.Sieger == "TeamB"));
                Assert.Equal(agg.Draws, eigene.Count(z => z.Sieger == "Draw"));
                Assert.Equal(agg.RoundLimitShare, (double)eigene.Count(z => z.Endgrund == "RoundLimit") / eigene.Count, 12);
                Assert.Equal(agg.RoundLimitSurvivingHeroes, eigene.Count(z => z.Aufloesung == "SurvivingHeroes"));
                Assert.Equal(agg.RoundLimitHitPointFraction, eigene.Count(z => z.Aufloesung == "HitPointFraction"));
                Assert.Equal(agg.Draws, eigene.Count(z => z.Aufloesung == "Draw"));
                Assert.All(eigene, z => Assert.Equal(z.Endgrund == "RoundLimit", z.Aufloesung != string.Empty));
                Assert.Equal(agg.AverageRoundsPlayed, eigene.Average(z => z.Runden), 12);
                Assert.Equal(agg.AverageTeamARemainingHitPointsFraction, eigene.Average(z => (double)z.RestHpA / maxA), 12);
                Assert.Equal(agg.AverageTeamBRemainingHitPointsFraction, eigene.Average(z => (double)z.RestHpB / maxB), 12);

                // Bei Auslöschung hat genau eine Seite noch lebende Helden.
                Assert.All(eigene.Where(z => z.Endgrund == "Elimination"), z => Assert.True((z.UeberlebendeA == 0) != (z.UeberlebendeB == 0)));
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Die Rundenlimit-Spalten der Heldendatei stimmen mit der Partiedatei überein
    /// (nur einseitige Partien, wie die übrigen Heldenwerte), die bisherigen Spalten
    /// behalten ihre Position.
    /// </summary>
    [Fact]
    public void Heldendatei_RundenlimitJeHeld_PasstZurPartiedatei()
    {
        var board = new GameBoard();
        var result = BatchSimulationRunner.Run(board, Pairings, AgentConfiguration.UniformVsUniform(), baseSeed: 7,
            maxSetupsPerPairing: 24, degreeOfParallelism: 4, recordMatches: true);
        string csv = Temp(".csv");
        try
        {
            CsvExporter.ExportHeroWinRates(result.PairingResults, csv, AgentNames.Uniform);
            var lines = File.ReadAllLines(csv);
            var kopf = lines[0].Split(';');
            Assert.Equal("Agent", kopf[20]); // bisherige Spalten unverändert
            Assert.Equal(new[] { "PartienRundenlimit", "AnteilRundenlimit", "RundenlimitMehrHelden", "RundenlimitMehrRestHp", "RundenlimitUnentschieden" }, kopf[21..]);

            foreach (var line in lines.Skip(1))
            {
                var f = line.Split(';');
                string held = f[0];
                var einseitig = Enumerable.Range(0, Pairings.Length)
                    .Where(p => Pairings[p].TeamA.Heroes.Any(h => h.Id == held) != Pairings[p].TeamB.Heroes.Any(h => h.Id == held))
                    .ToHashSet();
                var partien = result.Matches!.Where(m => einseitig.Contains(m.PairingIndex)).ToList();

                Assert.Equal(partien.Count, int.Parse(f[2]));
                int limit = partien.Count(m => m.EndReason == MatchEndReason.RoundLimit);
                Assert.Equal(limit, int.Parse(f[21]));
                Assert.Equal(partien.Count == 0 ? 0.0 : (double)limit / partien.Count, double.Parse(f[22], CultureInfo.GetCultureInfo("de-DE")), 4);
                Assert.Equal(partien.Count(m => m.Resolution == RoundLimitResolution.SurvivingHeroes), int.Parse(f[23]));
                Assert.Equal(partien.Count(m => m.Resolution == RoundLimitResolution.HitPointFraction), int.Parse(f[24]));
                Assert.Equal(partien.Count(m => m.Resolution == RoundLimitResolution.Draw), int.Parse(f[25]));
            }
        }
        finally
        {
            File.Delete(csv);
        }
    }

    /// <summary>Die Aggregate sind mit und ohne Export je Partie bitgenau gleich.</summary>
    [Fact]
    public void Aggregate_SindMitUndOhnePartieexportIdentisch()
    {
        var board = new GameBoard();
        var ohne = BatchSimulationRunner.Run(board, Pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 11, maxSetupsPerPairing: 12, degreeOfParallelism: 4);
        var mit = BatchSimulationRunner.Run(board, Pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 11, maxSetupsPerPairing: 12, degreeOfParallelism: 4, recordMatches: true);

        Assert.Null(ohne.Matches);
        var leer = new Dictionary<string, HeroActionCounts>();
        Assert.Equal(ohne.PairingResults.Select(r => r with { HeroActionTotals = leer }), mit.PairingResults.Select(r => r with { HeroActionTotals = leer }));
    }

    /// <summary>
    /// Determinismus und Reproduzierbarkeit: zweimal ein Thread und einmal acht Threads
    /// liefern dieselben Partien in derselben Reihenfolge - auch als geschriebene Datei.
    /// </summary>
    [Fact]
    public void Partiedatei_IstUnabhaengigVonDerThreadanzahlUndReproduzierbar()
    {
        var board = new GameBoard();
        SimulationRunResult Lauf(int threads) => BatchSimulationRunner.Run(board, Pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 99,
            maxSetupsPerPairing: 24, degreeOfParallelism: threads, recordMatches: true);

        var laeufe = new[] { Lauf(1), Lauf(1), Lauf(8) };
        var inhalte = new List<string>();
        foreach (var lauf in laeufe)
        {
            string path = Temp(".csv.gz");
            try
            {
                CsvExporter.ExportMatches(lauf.Matches!, Pairings, path, AgentNames.ByActionType);
                using var reader = new StreamReader(new GZipStream(File.OpenRead(path), CompressionMode.Decompress));
                inhalte.Add(reader.ReadToEnd());
            }
            finally
            {
                File.Delete(path);
            }
        }

        Assert.Equal(laeufe[0].Matches, laeufe[1].Matches);
        Assert.Equal(laeufe[0].Matches, laeufe[2].Matches);
        Assert.Equal(inhalte[0], inhalte[1]);
        Assert.Equal(inhalte[0], inhalte[2]);
    }

    /// <summary>Mit teamvertauschter Zweitpartie: beide Partien je Ausgangslage, in fester Reihenfolge und gekennzeichnet.</summary>
    [Fact]
    public void Partiedatei_KennzeichnetVertauschteZweitpartien()
    {
        var result = BatchSimulationRunner.Run(new GameBoard(), Pairings[..2], AgentConfiguration.RandomVsRandom(), baseSeed: 5,
            maxSetupsPerPairing: 4, useAntitheticPairing: true, degreeOfParallelism: 4, recordMatches: true);

        var m = result.Matches!;
        Assert.Equal(2 * 4 * 2, m.Count);
        for (int i = 0; i < m.Count; i++)
        {
            Assert.Equal(i / 8, m[i].PairingIndex);
            Assert.Equal(i % 8 / 2, m[i].SetupIndex);
            Assert.Equal(i % 2 == 1, m[i].TeamsSwapped);
        }
    }

    /// <summary>
    /// Etiketten-Unabhängigkeit über alle 84 Spiegelpaarungen mit eingeschaltetem
    /// Partieexport (Zufallsagent, 2 Ausgangslagen je Paarung).
    /// </summary>
    [Fact]
    public void AlleSpiegelpaarungen_MitPartieexport_SindEtikettenunabhaengig()
    {
        var mirrors = Compositions.Select(c => (c, c)).ToList();
        Assert.Equal(84, mirrors.Count);

        var result = BatchSimulationRunner.Run(new GameBoard(), mirrors, AgentConfiguration.RandomVsRandom(), baseSeed: 2026,
            maxSetupsPerPairing: 2, degreeOfParallelism: Environment.ProcessorCount, verifyNameIndependence: true, recordMatches: true);

        Assert.Equal(84 * 2, result.Matches!.Count);
        Assert.Equal(84 * 2, result.NameIndependence!.CheckedMatches);
        Assert.True(result.NameIndependence.Violations.Count == 0, string.Join("\n", result.NameIndependence.Violations.Take(5).Select(v => v.Describe())));
    }
}
