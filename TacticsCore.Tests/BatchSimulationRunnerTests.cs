using TacticsCore.Batch;
using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Setup;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class BatchSimulationRunnerTests
{
    private static readonly TeamComposition TrioA = new(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
    private static readonly TeamComposition TrioB = new(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

    /// <summary>
    /// Spiegelpaarung (identische Komposition auf beiden Seiten) plus
    /// antithetischer Paarbildung (siehe <see cref="GameSetup.WithSwappedTeams"/>)
    /// ergibt eine EXAKTE (nicht nur ungefähre) 50%-Siegquote - siehe die
    /// ausführliche Begründung dort und in <see cref="BatchSimulationRunner"/>.
    /// </summary>
    [Fact]
    public void VollstaendigerLaufUeberSpiegelpaarungMitAntithetischerPaarbildung_ErgibtGenau50ProzentSiegquote()
    {
        var board = new GameBoard();
        var result = BatchSimulationRunner.Run(
            board,
            new[] { (TrioA, TrioA) },
            AgentConfiguration.RandomVsRandom(),
            baseSeed: 12345,
            maxSetupsPerPairing: 6, // genügt für den Nachweis, hält den Test schnell
            useAntitheticPairing: true);

        var pairing = Assert.Single(result.PairingResults);
        Assert.Equal(pairing.TeamAWins, pairing.TeamBWins); // exakt gleich, nicht nur ungefähr
        Assert.Equal(0.5, pairing.TeamAWinRate, precision: 10);
    }

    /// <summary>
    /// Boosted-Team gegen SEIN EIGENES unverändertes Gegenstück (statt gegen
    /// eine dritte, ggf. bereits deutlich über- oder unterlegene Komposition)
    /// - das startet nahe 50% (siehe Spiegel-Plausibilitätsprüfung) und macht
    /// die Wirkung der Katalog-Änderung eindeutig messbar, ohne dass eine
    /// bereits gesättigte (0%/100%) Ausgangslage den Effekt verdeckt.
    /// </summary>
    [Fact]
    public void VeraenderterHeldenkatalog_FuehrtZuAnderenErgebnissenAlsDerBasiskatalog()
    {
        var board = new GameBoard();
        var baseCatalog = DefaultHeroes.CreateDefaultCatalog();
        var boostedCatalog = HeroCatalogVariants.WithAttributePercentageChange(baseCatalog, "berserker", HeroAttribute.BaseAttackDamage, 50.0);

        var baseTrio = new TeamComposition(new[] { baseCatalog.Get("schildtraeger"), baseCatalog.Get("elfe"), baseCatalog.Get("berserker") });
        var boostedTrio = new TeamComposition(new[] { boostedCatalog.Get("schildtraeger"), boostedCatalog.Get("elfe"), boostedCatalog.Get("berserker") });

        var result = BatchSimulationRunner.Run(board, new[] { (boostedTrio, baseTrio) }, AgentConfiguration.RandomVsRandom(), baseSeed: 7, maxSetupsPerPairing: 48);

        // Das geboostete Team A sollte klar über einer 50%-Basislinie liegen.
        Assert.True(result.PairingResults[0].TeamAWinRate > 0.5);
    }

    [Fact]
    public void HeroCatalogVariants_VeraendertGenauDasAngegebeneAttributDesAngegebenenHeldenUndSonstNichts()
    {
        var baseCatalog = DefaultHeroes.CreateDefaultCatalog();

        var variant = HeroCatalogVariants.WithAttributePercentageChange(baseCatalog, "berserker", HeroAttribute.BaseAttackDamage, 20.0);

        var originalBerserker = baseCatalog.Get("berserker");
        var changedBerserker = variant.Get("berserker");
        Assert.Equal((int)Math.Round(originalBerserker.BaseAttackDamage * 1.2), changedBerserker.BaseAttackDamage);
        // Sonst nichts an diesem Helden verändert:
        Assert.Equal(originalBerserker.MaxHitPoints, changedBerserker.MaxHitPoints);
        Assert.Equal(originalBerserker.Speed, changedBerserker.Speed);
        Assert.Equal(originalBerserker.MovementRange, changedBerserker.MovementRange);
        Assert.Equal(originalBerserker.AttackRange, changedBerserker.AttackRange);

        // Kein anderer Held im Katalog verändert:
        foreach (var hero in baseCatalog.All.Where(h => h.Id != "berserker"))
        {
            Assert.Equal(hero, variant.Get(hero.Id));
        }
    }

    [Fact]
    public void EinThreadUndMehrereThreads_LiefernIdentischeErgebnisse()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };

        var singleThreaded = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 99, maxSetupsPerPairing: 24, degreeOfParallelism: 1);
        var multiThreaded = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 99, maxSetupsPerPairing: 24, degreeOfParallelism: 8);

        Assert.Equal(singleThreaded.PairingResults, multiThreaded.PairingResults);
    }

    [Fact]
    public void CsvExport_EnthaeltDieErwarteteZeilenzahlUndStruktur()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };
        var result = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 3, maxSetupsPerPairing: 4);

        string pairingsCsv = Path.Combine(Path.GetTempPath(), $"tacticscore-pairings-{Guid.NewGuid():N}.csv");
        string compositionsCsv = Path.Combine(Path.GetTempPath(), $"tacticscore-compositions-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportPairingResults(result.PairingResults, pairingsCsv);
            CsvExporter.ExportCompositionWinRates(result.PairingResults, compositionsCsv);

            var pairingLines = File.ReadAllLines(pairingsCsv);
            Assert.Equal(1 + pairings.Length, pairingLines.Length); // Kopfzeile + eine Zeile je Paarung
            Assert.StartsWith("TeamA;TeamB;", pairingLines[0]);

            var compositionLines = File.ReadAllLines(compositionsCsv);
            // Zwei Paarungen mit insgesamt drei VERSCHIEDENEN Kompositionen (TrioA kommt zweimal vor, zählt aber nur einmal).
            Assert.Equal(1 + 2, compositionLines.Length);
            Assert.StartsWith("Komposition;", compositionLines[0]);
        }
        finally
        {
            File.Delete(pairingsCsv);
            File.Delete(compositionsCsv);
        }
    }
}
