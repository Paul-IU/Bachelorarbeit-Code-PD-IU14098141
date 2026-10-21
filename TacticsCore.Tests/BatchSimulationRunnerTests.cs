using System.Globalization;
using TacticsCore.Abilities;
using TacticsCore.Batch;
using TacticsCore.Board;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Match;
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
    /// Option "Seiten vertauschen" (Messung des Seitenvorteils): ohne Option
    /// bleibt der Lauf bitgenau der bisherige; mit Option werden dieselben
    /// Partien (gleiche Anzahl, gleiche Seeds, gleiches beginnendes Team) auf
    /// gespiegelten Startfeldern gespielt.
    /// </summary>
    [Fact]
    public void SeitenVertauschen_OhneOptionUnveraendert_MitOptionGleicheSeedsUndStartteams()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioA) };
        var agents = AgentConfiguration.FromName(AgentNames.Aggressive);

        var standard = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 2026, maxSetupsPerPairing: 12, recordMatches: true);
        var normal = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 2026, maxSetupsPerPairing: 12, recordMatches: true, swapSides: false);
        var vertauscht = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 2026, maxSetupsPerPairing: 12, recordMatches: true, swapSides: true);

        Assert.Equal(standard.Matches!, normal.Matches!);
        Assert.Equal(standard.Matches!.Select(m => (m.SetupIndex, m.Seed, m.StartingTeam)),
                     vertauscht.Matches!.Select(m => (m.SetupIndex, m.Seed, m.StartingTeam)));
    }

    /// <summary>
    /// Dieselbe Prüfung wie <see cref="VollstaendigerLaufUeberSpiegelpaarungMitAntithetischerPaarbildung_ErgibtGenau50ProzentSiegquote"/>,
    /// hier mit <see cref="AgentConfiguration.UniformVsUniform"/> statt
    /// <see cref="AgentConfiguration.RandomVsRandom"/> - die exakte 50%-
    /// Symmetrie einer Spiegelpaarung mit antithetischer Paarbildung folgt
    /// rein aus der Teamvertauschung (siehe GameSetup.WithSwappedTeams) und
    /// ist unabhängig von der konkreten Auswahlregel des Agenten, muss also
    /// für BEIDE Agenten gleichermaßen gelten.
    /// </summary>
    [Fact]
    public void VollstaendigerLaufUeberSpiegelpaarungMitAntithetischerPaarbildungUndUniformAgent_ErgibtGenau50ProzentSiegquote()
    {
        var board = new GameBoard();
        var result = BatchSimulationRunner.Run(
            board,
            new[] { (TrioA, TrioA) },
            AgentConfiguration.UniformVsUniform(),
            baseSeed: 12345,
            maxSetupsPerPairing: 6,
            useAntitheticPairing: true);

        var pairing = Assert.Single(result.PairingResults);
        Assert.Equal(pairing.TeamAWins, pairing.TeamBWins);
        Assert.Equal(0.5, pairing.TeamAWinRate, precision: 10);
    }

    /// <summary>
    /// Symmetrietest über ALLE 84 Kompositionen des Standardkatalogs, ALLE 72
    /// Ausgangslagen je Spiegelpaarung und beide Agenten: bei antithetischer
    /// Paarbildung (jede Ausgangslage wird mit demselben Seed ein zweites Mal
    /// mit vertauschten Team-Etiketten gespielt, siehe
    /// <see cref="GameSetup.WithSwappedTeams"/>) müssen beide Teams EXAKT gleich
    /// viele Partien gewinnen.
    ///
    /// WAS DIESER TEST BEWEIST - und was nicht: die vertauschte Partie steht
    /// physisch identisch da (dieselben Einheiten auf denselben Feldern, nur
    /// die Etiketten "Team A/B" und die Ids sind getauscht). Gleiche Siege
    /// bedeuten deshalb, dass das Regelwerk samt Agenten NAMENSUNABHÄNGIG ist:
    /// nichts im Spielablauf hängt von Team-Etikett oder Id-Bereich ab (er hat
    /// z.B. den Hugger-Fehler in der Fähigkeiten-Sortierung aufgedeckt, siehe
    /// <see cref="TacticsCore.Abilities.AbilitySystem.GetLegalAbilityUses"/>).
    /// Er sagt dagegen NICHTS darüber, ob eine Komposition auf Blüte A gegen
    /// sich selbst auf Blüte B im Mittel 50% erreicht - das ist eine andere,
    /// nur statistisch prüfbare Frage (siehe README in
    /// Ergebnisse/spiegel_symmetrie).
    /// Geprüft werden bewusst die SIEGE, nicht die Siegquote: bei einem
    /// Unentschieden am Rundenlimit ist die Quote trotz exakt gleicher Siege
    /// kleiner als 0,5.
    /// </summary>
    [Theory]
    [InlineData(AgentNames.ByActionType)]
    [InlineData(AgentNames.Uniform)]
    public void AlleSpiegelpaarungenMitAntithetischerPaarbildung_ErgebenExaktGleicheSiegzahlen(string agentName)
    {
        var compositions = CompositionGenerator.GenerateAllCompositions(DefaultHeroes.CreateDefaultCatalog());
        var mirrors = compositions.Select(c => (c, c)).ToList();
        Assert.Equal(84, mirrors.Count); // C(9,3): alle Dreier-Teilmengen der neun Helden

        var result = BatchSimulationRunner.Run(
            new GameBoard(),
            mirrors,
            AgentConfiguration.FromName(agentName),
            baseSeed: 2026,
            maxSetupsPerPairing: null, // alle 72 Ausgangslagen
            useAntitheticPairing: true,
            degreeOfParallelism: Environment.ProcessorCount);

        var asymmetrisch = result.PairingResults
            .Where(p => p.TeamAWins != p.TeamBWins)
            .Select(p => $"{p.TeamA.Key}: A={p.TeamAWins} B={p.TeamBWins} Remis={p.Draws}")
            .ToList();
        Assert.True(asymmetrisch.Count == 0, "Asymmetrische Spiegelpaarungen:\n" + string.Join("\n", asymmetrisch));
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

        // HeroActionTotals ist ein Dictionary - die von PairingResult (als
        // record) automatisch erzeugte Gleichheit vergleicht ein
        // Dictionary-Feld NICHT inhaltlich, sondern per Referenz (Dictionary
        // überschreibt Equals nicht). Für den Vergleich der ÜBRIGEN
        // (skalaren) Felder wird das Feld hier bewusst auf eine gemeinsame,
        // leere Instanz normalisiert - der eigentliche Inhaltsvergleich
        // erfolgt separat weiter unten, dort GENAU über diese Werte-Gleichheit.
        var sharedEmptyActionTotals = new Dictionary<string, HeroActionCounts>();
        Assert.Equal(
            singleThreaded.PairingResults.Select(r => r with { HeroActionTotals = sharedEmptyActionTotals }),
            multiThreaded.PairingResults.Select(r => r with { HeroActionTotals = sharedEmptyActionTotals }));

        for (int i = 0; i < singleThreaded.PairingResults.Count; i++)
        {
            // Direkter Vergleich zweier IEnumerable<KeyValuePair<...>> -
            // xUnit vergleicht diese (im Gegensatz zu einem Dictionary-Feld
            // INNERHALB eines Records) elementweise; KeyValuePair<string,
            // HeroActionCounts> selbst hat als Struct korrekte Werte-Gleichheit.
            Assert.Equal(
                singleThreaded.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                multiThreaded.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal));
        }
    }

    /// <summary>
    /// Dieselbe Prüfung wie <see cref="EinThreadUndMehrereThreads_LiefernIdentischeErgebnisse"/>,
    /// hier mit <see cref="AgentConfiguration.UniformVsUniform"/> - die
    /// Reproduzierbarkeit unter Parallelisierung (Anforderung 11) hängt
    /// ausschließlich an der seed-Ableitung und der strikt sequentiellen
    /// Nachbearbeitung in BatchSimulationRunner (siehe dortige
    /// Klassendokumentation), NICHT am konkreten Agenten - muss also für
    /// BEIDE Agenten gleichermaßen gelten.
    /// </summary>
    [Fact]
    public void EinThreadUndMehrereThreadsMitUniformAgent_LiefernIdentischeErgebnisse()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };

        var singleThreaded = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.UniformVsUniform(), baseSeed: 99, maxSetupsPerPairing: 24, degreeOfParallelism: 1);
        var multiThreaded = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.UniformVsUniform(), baseSeed: 99, maxSetupsPerPairing: 24, degreeOfParallelism: 8);

        var sharedEmptyActionTotals = new Dictionary<string, HeroActionCounts>();
        Assert.Equal(
            singleThreaded.PairingResults.Select(r => r with { HeroActionTotals = sharedEmptyActionTotals }),
            multiThreaded.PairingResults.Select(r => r with { HeroActionTotals = sharedEmptyActionTotals }));

        for (int i = 0; i < singleThreaded.PairingResults.Count; i++)
        {
            Assert.Equal(
                singleThreaded.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                multiThreaded.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal));
        }
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
            CsvExporter.ExportPairingResults(result.PairingResults, pairingsCsv, AgentNames.ByActionType);
            CsvExporter.ExportCompositionWinRates(result.PairingResults, compositionsCsv, AgentNames.ByActionType);

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

    /// <summary>
    /// Regressionstest für einen echten Bug: Bei einer Spiegelpaarung
    /// (TeamA und TeamB derselben Komposition) trafen beide Accumulate-
    /// Aufrufe in ExportCompositionWinRates denselben Dictionary-Eintrag -
    /// die Partien dieser einen Zeile wurden dadurch DOPPELT statt einmal
    /// gezählt (144 statt 72 bei 72 Partien je Paarung), jede Komposition
    /// erschien so um genau ihre eigenen Spiegelpartien zu hoch.
    /// </summary>
    [Fact]
    public void ExportCompositionWinRates_ZaehltSpiegelpaarungNurEinmalNichtDoppelt()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioA) };
        var result = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 3, maxSetupsPerPairing: 12);
        var pairing = Assert.Single(result.PairingResults);

        string csv = Path.Combine(Path.GetTempPath(), $"tacticscore-mirror-comp-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportCompositionWinRates(result.PairingResults, csv, AgentNames.ByActionType);
            var fields = File.ReadAllLines(csv)[1].Split(';');

            Assert.Equal(pairing.TotalMatches.ToString(), fields[1]); // NICHT das Doppelte
            Assert.Equal((pairing.TeamAWins + pairing.TeamBWins).ToString(), fields[2]);
        }
        finally
        {
            File.Delete(csv);
        }
    }

    /// <summary>
    /// Spaltenreihenfolge von <see cref="CsvExporter.ExportHeroWinRates"/> als
    /// benannte Indizes - hält die übrigen Tests robust gegen Verschiebungen
    /// innerhalb der beiden Spaltengruppen (bereinigt/verzerrt).
    /// </summary>
    private static class HeroCsv
    {
        public const int Partien = 2, Siege = 3, Siegquote = 4;
        public const int RundenAmZug = 5, Bewegungen = 6, Angriffe = 7, Faehigkeitseinsaetze = 8, VerfuegbareZuegeFuerFaehigkeit = 9;
        public const int DurchschnittPartiedauer = 10, DurchschnittRundenAmZug = 11;
        public const int DurchschnittBewegungenProZug = 12, DurchschnittAngriffeProZug = 13, DurchschnittFaehigkeitseinsaetzeProZug = 14, DurchschnittFaehigkeitseinsaetzeProVerfuegbaremZug = 15;
        public const int PartienBeideSeiten = 16;
        public const int PartienGesamtVerzerrt = 17, SiegeGesamtVerzerrt = 18, SiegquoteGesamtVerzerrt = 19;
        public const int Agent = 20;
    }

    /// <summary>
    /// Bachelorarbeit-Korrektur: eine REINE Spiegelpaarung (TeamA und TeamB
    /// sind dieselbe Komposition) hat für jeden ihrer drei Helden gar keine
    /// "einseitigen" Partien - er steht in JEDER Partie dieser Zeile auf
    /// BEIDEN Seiten. Die bereinigte Kennzahl ("Partien"/"Siege"/"Siegquote")
    /// muss dafür also 0 bleiben, während die komplette Partienzahl korrekt
    /// (nicht doppelt) unter "PartienBeideSeiten" bzw. den "*Verzerrt"-Spalten
    /// erscheint.
    /// </summary>
    [Fact]
    public void ExportHeroWinRates_SpiegelpaarungLandetVollstaendigInDerBeidseitigenBilanz()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioA) };
        var result = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 3, maxSetupsPerPairing: 12);
        var pairing = Assert.Single(result.PairingResults);

        string csv = Path.Combine(Path.GetTempPath(), $"tacticscore-mirror-hero-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportHeroWinRates(result.PairingResults, csv, AgentNames.ByActionType);
            var lines = File.ReadAllLines(csv).Skip(1).ToList();

            Assert.Equal(3, lines.Count); // je einer für Schildträger, Elfe, Berserker (TrioA)
            foreach (var line in lines)
            {
                var fields = line.Split(';');
                Assert.Equal("0", fields[HeroCsv.Partien]); // keine einzige einseitige Partie
                Assert.Equal("0", fields[HeroCsv.Siege]);
                Assert.Equal(pairing.TotalMatches.ToString(), fields[HeroCsv.PartienBeideSeiten]); // NICHT das Doppelte
                Assert.Equal(pairing.TotalMatches.ToString(), fields[HeroCsv.PartienGesamtVerzerrt]);
                Assert.Equal((pairing.TeamAWins + pairing.TeamBWins).ToString(), fields[HeroCsv.SiegeGesamtVerzerrt]);
            }
        }
        finally
        {
            File.Delete(csv);
        }
    }

    /// <summary>
    /// Bachelorarbeit-Korrektur, echter Bug - Erweiterung der obigen
    /// Spiegelpaarungs-Korrektur: TrioA und TrioC sind VERSCHIEDENE
    /// Kompositionen (TeamA.Key != TeamB.Key), teilen sich aber den
    /// Schildträger - er tritt in dieser Paarung faktisch gegen sich selbst
    /// an, landet also (wie ein echter Spiegel) VOLLSTÄNDIG in der
    /// beidseitigen Bilanz, während die drei übrigen (nicht geteilten)
    /// Helden ganz normal einseitig gezählt werden.
    /// </summary>
    [Fact]
    public void ExportHeroWinRates_GeteilterHeldAusUnterschiedlichenKompositionenLandetVollstaendigInDerBeidseitigenBilanz()
    {
        var board = new GameBoard();
        var trioC = new TeamComposition(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Illusionist, DefaultHeroes.Bogenschuetze });
        var pairings = new[] { (TrioA, trioC) };
        var result = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 3, maxSetupsPerPairing: 12);
        var pairing = Assert.Single(result.PairingResults);

        string csv = Path.Combine(Path.GetTempPath(), $"tacticscore-shared-hero-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportHeroWinRates(result.PairingResults, csv, AgentNames.ByActionType);
            var byId = File.ReadAllLines(csv).Skip(1).ToDictionary(l => l.Split(';')[0], l => l.Split(';'));

            // Schildträger: geteilt zwischen TrioA und trioC -> vollständig beidseitig.
            var schildtraeger = byId["schildtraeger"];
            Assert.Equal("0", schildtraeger[HeroCsv.Partien]);
            Assert.Equal(pairing.TotalMatches.ToString(), schildtraeger[HeroCsv.PartienBeideSeiten]);
            Assert.Equal(pairing.TotalMatches.ToString(), schildtraeger[HeroCsv.PartienGesamtVerzerrt]);
            Assert.Equal((pairing.TeamAWins + pairing.TeamBWins).ToString(), schildtraeger[HeroCsv.SiegeGesamtVerzerrt]);

            // Elfe: nur in TrioA (TeamA) -> ganz normal einseitig, mit TeamAWins.
            var elfe = byId["elfe"];
            Assert.Equal(pairing.TotalMatches.ToString(), elfe[HeroCsv.Partien]);
            Assert.Equal(pairing.TeamAWins.ToString(), elfe[HeroCsv.Siege]);
            Assert.Equal("0", elfe[HeroCsv.PartienBeideSeiten]);

            // Illusionist: nur in trioC (TeamB) -> ganz normal einseitig, mit TeamBWins.
            var illusionist = byId["illusionist"];
            Assert.Equal(pairing.TotalMatches.ToString(), illusionist[HeroCsv.Partien]);
            Assert.Equal(pairing.TeamBWins.ToString(), illusionist[HeroCsv.Siege]);
            Assert.Equal("0", illusionist[HeroCsv.PartienBeideSeiten]);
        }
        finally
        {
            File.Delete(csv);
        }
    }

    /// <summary>
    /// Bachelorarbeit-Erweiterung: die Helden-CSV enthält zusätzlich Runden-
    /// am-Zug, Bewegungen/Angriffe/Fähigkeitseinsätze (absolut UND je Zug),
    /// die Verfügbarkeit der Fähigkeit sowie Partiedauer vs. Runden-am-Zug
    /// getrennt (siehe CsvExporter.ExportHeroWinRates). Prüft zusätzlich die
    /// strukturelle Obergrenze aus Problem 2: pro Zug höchstens eine
    /// Bewegung und höchstens eine Aktion (Angriff ODER Fähigkeit) - in Summe
    /// über alle einseitigen Partien kann "Bewegungen" bzw. "Angriffe +
    /// Faehigkeitseinsaetze" die Anzahl "RundenAmZug" daher NIEMALS übersteigen.
    /// </summary>
    [Fact]
    public void ExportHeroWinRates_EnthaeltKonsistenteAktionszahlenJeZugUndUeberschreitetNieDieRundenAmZug()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB) };
        var result = BatchSimulationRunner.Run(board, pairings, AgentConfiguration.RandomVsRandom(), baseSeed: 3, maxSetupsPerPairing: 24);

        string csv = Path.Combine(Path.GetTempPath(), $"tacticscore-hero-actions-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportHeroWinRates(result.PairingResults, csv, AgentNames.ByActionType);
            var lines = File.ReadAllLines(csv);

            Assert.Equal(
                "Held;Rolle;Partien;Siege;Siegquote;" +
                "RundenAmZug;Bewegungen;Angriffe;Faehigkeitseinsaetze;VerfuegbareZuegeFuerFaehigkeit;" +
                "DurchschnittPartiedauer;DurchschnittRundenAmZug;" +
                "DurchschnittBewegungenProZug;DurchschnittAngriffeProZug;DurchschnittFaehigkeitseinsaetzeProZug;DurchschnittFaehigkeitseinsaetzeProVerfuegbaremZug;" +
                "PartienBeideSeiten;PartienGesamtVerzerrt;SiegeGesamtVerzerrt;SiegquoteGesamtVerzerrt;Agent;" +
                "PartienRundenlimit;AnteilRundenlimit;RundenlimitMehrHelden;RundenlimitMehrRestHp;RundenlimitUnentschieden",
                lines[0]);

            foreach (var line in lines.Skip(1))
            {
                var fields = line.Split(';');
                Assert.Equal(26, fields.Length);
                Assert.Equal(AgentNames.ByActionType, fields[HeroCsv.Agent]);

                int rundenAmZug = int.Parse(fields[HeroCsv.RundenAmZug]);
                int bewegungen = int.Parse(fields[HeroCsv.Bewegungen]);
                int angriffe = int.Parse(fields[HeroCsv.Angriffe]);
                int faehigkeitseinsaetze = int.Parse(fields[HeroCsv.Faehigkeitseinsaetze]);
                int verfuegbareZuege = int.Parse(fields[HeroCsv.VerfuegbareZuegeFuerFaehigkeit]);

                // Problem 2: höchstens eine Bewegung, höchstens eine Aktion je Zug.
                Assert.True(bewegungen <= rundenAmZug);
                Assert.True(angriffe + faehigkeitseinsaetze <= rundenAmZug);
                Assert.True(verfuegbareZuege <= rundenAmZug);
                Assert.True(faehigkeitseinsaetze <= verfuegbareZuege); // kann nur genutzt werden, wenn verfügbar

                Assert.True(double.Parse(fields[HeroCsv.DurchschnittPartiedauer], CultureInfo.GetCultureInfo("de-DE")) > 0);
                Assert.True(double.Parse(fields[HeroCsv.DurchschnittRundenAmZug], CultureInfo.GetCultureInfo("de-DE")) > 0);
                Assert.True(double.Parse(fields[HeroCsv.DurchschnittBewegungenProZug], CultureInfo.GetCultureInfo("de-DE")) is >= 0 and <= 1);
                double proZugSumme = double.Parse(fields[HeroCsv.DurchschnittAngriffeProZug], CultureInfo.GetCultureInfo("de-DE"))
                    + double.Parse(fields[HeroCsv.DurchschnittFaehigkeitseinsaetzeProZug], CultureInfo.GetCultureInfo("de-DE"));
                Assert.True(proZugSumme is >= 0 and <= 1.0001); // Rundungstoleranz
            }
        }
        finally
        {
            File.Delete(csv);
        }
    }

    /// <summary>
    /// Bachelorarbeit-Korrektur (Problem 1.4 der Auswertungskorrektur): bei
    /// einem Katalog aus NEUN VÖLLIG IDENTISCHEN Helden (gleiche Werte, nur
    /// unterschiedliche Ids und Geschwindigkeiten - <see cref="HeroCatalog"/>
    /// verlangt paarweise verschiedene Geschwindigkeiten, siehe DefaultHeroes)
    /// darf KEIN Held einen strukturellen Vor- oder Nachteil haben: die
    /// bereinigte Siegquote muss bei jedem Helden exakt 50% liegen, die
    /// verzerrte Rohquote exakt bei der kombinatorischen Erwartung für 9
    /// Helden/Teamgröße 3 (siehe <see cref="HeroWinRateExpectations"/>).
    /// Nutzt <c>useAntitheticPairing</c> (siehe BatchSimulationRunner), damit
    /// die Erwartung nicht nur im statistischen Mittel, sondern EXAKT
    /// erreicht wird, ohne eine riesige Partienzahl zu benötigen.
    /// </summary>
    [Fact]
    public void ExportHeroWinRates_BeiVoelligIdentischenHeldenTrifftDieNeutraleErwartungExakt()
    {
        // Neutrale Erwartung unter Berücksichtigung der Unentschieden (siehe unten).
        static bool Einseitig(PairingResult r, string id) => r.TeamA.Heroes.Any(h => h.Id == id) ^ r.TeamB.Heroes.Any(h => h.Id == id);
        static bool Beidseitig(PairingResult r, string id) => r.TeamA.Heroes.Any(h => h.Id == id) && r.TeamB.Heroes.Any(h => h.Id == id);
        static double NeutralCleaned(IEnumerable<PairingResult> rs, string id)
        {
            var one = rs.Where(r => Einseitig(r, id)).ToList();
            return one.Sum(r => (r.TotalMatches - r.Draws) / 2.0) / one.Sum(r => r.TotalMatches);
        }
        static double NeutralSkewed(IEnumerable<PairingResult> rs, string id)
        {
            var all = rs.Where(r => Einseitig(r, id) || Beidseitig(r, id)).ToList();
            double siege = all.Sum(r => Beidseitig(r, id) ? r.TotalMatches - r.Draws : (r.TotalMatches - r.Draws) / 2.0);
            return siege / all.Sum(r => r.TotalMatches);
        }

        var noOpAbility = new AbilityDefinition(
            Id: "noop", DisplayName: "Nichts", IsPassive: false,
            TargetType: AbilityTargetType.Self, Range: 0, Effects: Array.Empty<EffectTemplate>());
        var baseHero = new HeroDefinition(
            Id: "h0", DisplayName: "H0", Role: HeroRole.Dmg,
            MaxHitPoints: 50, BaseAttackDamage: 10, Speed: 1,
            MovementRange: 2, AttackRange: 1, Ability: noOpAbility);
        var heroes = Enumerable.Range(0, 9)
            .Select(i => baseHero with { Id = $"h{i}", DisplayName = $"H{i}", Speed = i + 1 })
            .ToArray();
        var catalog = new HeroCatalog(heroes);
        var compositions = CompositionGenerator.GenerateAllCompositions(catalog);

        // Jede Komposition gegen jede ANDERE genau einmal, plus gegen sich
        // selbst (Spiegelpaarung) - dieselbe Konstruktion wie
        // Program.GenerateAllUniquePairingsIncludingMirrors, hier lokal
        // nachgebildet, da TacticsCore.Demo für Tests nicht referenziert wird.
        var pairings = new List<(TeamComposition, TeamComposition)>();
        for (int i = 0; i < compositions.Count; i++)
        {
            for (int j = i; j < compositions.Count; j++)
            {
                pairings.Add((compositions[i], compositions[j]));
            }
        }

        var board = new GameBoard();
        var result = BatchSimulationRunner.Run(
            board, pairings, AgentConfiguration.RandomVsRandom(),
            baseSeed: 2026, maxSetupsPerPairing: 1, useAntitheticPairing: true, degreeOfParallelism: Environment.ProcessorCount);

        string csv = Path.Combine(Path.GetTempPath(), $"tacticscore-identical-heroes-{Guid.NewGuid():N}.csv");
        try
        {
            CsvExporter.ExportHeroWinRates(result.PairingResults, csv, AgentNames.ByActionType);
            var lines = File.ReadAllLines(csv).Skip(1).ToList();

            Assert.Equal(9, lines.Count);

            // UNENTSCHIEDEN (seit Rundenlimit 50 kommen sie hier vor): bei identischen
            // Helden und antithetischer Paarbildung gewinnt in einer EINSEITIGEN Paarung
            // jede Seite genau die Hälfte der ENTSCHIEDENEN Partien, (M − U) / 2; in einer
            // BEIDSEITIGEN zählt jede entschiedene Partie als Sieg des Helden, M − U.
            // Die exakte neutrale Erwartung je Held folgt daher aus den tatsächlichen
            // Unentschieden U jeder Paarung. Ohne Unentschieden ergibt sie genau die
            // geschlossene Formel aus HeroWinRateExpectations (60,28 % bzw. 50 %).
            int totalDraws = result.PairingResults.Sum(r => r.Draws);
            if (totalDraws == 0)
            {
                var (raw, cleaned) = HeroWinRateExpectations.NeutralExpectation(catalog.All.Count, compositions[0].Heroes.Count);
                Assert.Equal(raw, NeutralSkewed(result.PairingResults, "h0"), 9);
                Assert.Equal(cleaned, NeutralCleaned(result.PairingResults, "h0"), 9);
            }

            foreach (var line in lines)
            {
                var fields = line.Split(';');
                string heroId = fields[0];
                double expectedCleaned = NeutralCleaned(result.PairingResults, heroId);
                double expectedRaw = NeutralSkewed(result.PairingResults, heroId);
                double cleanedWinRate = double.Parse(fields[HeroCsv.Siegquote], CultureInfo.GetCultureInfo("de-DE"));
                double skewedWinRate = double.Parse(fields[HeroCsv.SiegquoteGesamtVerzerrt], CultureInfo.GetCultureInfo("de-DE"));

                // Präzision 4 statt mehr: die CSV selbst rundet jeden Wert
                // bereits auf 4 Nachkommastellen (siehe CsvExporter.Num) -
                // die zugrunde liegende Simulation trifft die Erwartung
                // dank antithetischer Paarbildung tatsächlich EXAKT, das
                // lässt sich über den gerundeten CSV-Text hinaus aber nicht
                // mehr nachweisen.
                Assert.Equal(expectedCleaned, cleanedWinRate, precision: 4);
                Assert.Equal(expectedRaw, skewedWinRate, precision: 4);
            }
        }
        finally
        {
            File.Delete(csv);
        }
    }
}
