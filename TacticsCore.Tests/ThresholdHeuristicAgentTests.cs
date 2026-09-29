using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests des Schwellen-Agenten (<see cref="ThresholdHeuristicAgent"/>) und seiner
/// Bewertungsfunktion (<see cref="ThresholdTurnEvaluator"/>). Die Tests des
/// bisherigen Agenten (<see cref="HeuristicAgentTests"/>) bleiben unverändert.
/// </summary>
public class ThresholdHeuristicAgentTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly TurnBudget VolleBudget = new(CanMove: true, CanAct: true);
    private static readonly TeamComposition TrioA = new(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
    private static readonly TeamComposition TrioB = new(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

    // -----------------------------------------------------------------
    // Bedrohung ohne Mehrfachzählung
    // -----------------------------------------------------------------

    [Fact]
    public void TeamBedrohung_ZaehltEinenGegnerNurEinmal_AuchWennErMehrereEigeneEinheitenErreicht()
    {
        var state = NewState();
        state.AddUnit(new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10));
        state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10));
        state.AddUnit(new TestUnit(Id(3), Team.TeamA, new HexCoordinate(1, 1), speed: 10));
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 3), speed: 10)); // erreicht alle drei (Reichweite 3)

        Assert.Equal(100, ThresholdTurnEvaluator.TeamThreat(state, Team.TeamA)); // nicht 3 × 100

        state.AddUnit(new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 4), speed: 10)); // zweiter Gegner
        Assert.Equal(200, ThresholdTurnEvaluator.TeamThreat(state, Team.TeamA));
    }

    // -----------------------------------------------------------------
    // Gefahrenschwelle: Heranrücken darunter, Zurückweichen darüber
    // -----------------------------------------------------------------

    /// <summary>
    /// Unterhalb der Schwelle (100 drohender Schaden gegen 500 Lebenspunkte =
    /// 0,2 &lt; 0,5) ist die Bedrohung neutral; der Distanzgewinn macht
    /// Heranrücken für BEIDE Ausrichtungen positiv.
    /// </summary>
    [Fact]
    public void UnterhalbDerSchwelle_WirdHeranrueckenPositivBewertet()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(eigene);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10));

        var heran = ThresholdHeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(0, 2)))!;

        Assert.Equal(2, heran.DistanceGain);
        Assert.Equal(0, heran.Retreat);
        Assert.True(heran.Weighted(ThresholdWeights.DefensiveProfile) > 0);
        Assert.True(heran.Weighted(ThresholdWeights.AggressiveProfile) > 0);

        var zug = Assert.IsType<TurnAction.Move>(new ThresholdHeuristicAgent(ThresholdWeights.DefensiveProfile).DecideAction(eigene, state, VolleBudget, new Random(1)));
        Assert.Equal(3, HexCoordinate.Distance(zug.Target, new HexCoordinate(0, 5)));
    }

    /// <summary>
    /// Oberhalb der Schwelle (zwei Gegner mit Reichweite 2 + 2 = 4, 200 drohender
    /// Schaden gegen 150 Lebenspunkte; Schwelle 75, Überschuss 125) lohnt
    /// Zurückweichen außer Reichweite: R = +125, obwohl der Distanzgewinn negativ
    /// ist. Ein eigener Angriff ist in diesem Zug nicht möglich (Distanz 4, eigene
    /// Reichweite 2 + 1 = 3) - sonst konkurriert er mit dem Rückzug (siehe
    /// <see cref="OberhalbDerSchwelle_KonkurriertEinMoeglicherAngriffMitDemRueckzug"/>).
    /// </summary>
    [Fact]
    public void OberhalbDerSchwelle_WirdZurueckweichenPositivBewertet()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(eigene);
        eigene.ApplyDamage(eigene.MaxHitPoints - 150);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10, attackRange: 2));
        state.AddUnit(new TestUnit(Id(102), Team.TeamB, new HexCoordinate(-1, 5), speed: 10, attackRange: 2));

        var zurueck = ThresholdHeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(0, 0)))!;

        Assert.Equal(125, zurueck.Retreat);
        Assert.True(zurueck.DistanceGain < 0);
        Assert.True(zurueck.Weighted(ThresholdWeights.DefensiveProfile) > 0);

        var zug = Assert.IsType<TurnAction.Move>(new ThresholdHeuristicAgent(ThresholdWeights.DefensiveProfile).DecideAction(eigene, state, VolleBudget, new Random(1)));
        Assert.True(HexCoordinate.Distance(zug.Target, new HexCoordinate(0, 5)) > 4); // außerhalb der Reichweite beider Gegner
        Assert.True(HexCoordinate.Distance(zug.Target, new HexCoordinate(-1, 5)) > 4);
    }

    /// <summary>
    /// BEFUND (kein Soll-Verhalten, sondern Eigenschaft der Formel): kann die
    /// gefährdete Einheit nach einer Bewegung angreifen, konkurriert der Angriff
    /// mit dem Rückzug. Bleibt die Einheit ohnehin in Reichweite aller Gegner,
    /// ändert Heranrücken den Überschuss nicht (R = 0) - hier gewinnt beim
    /// defensiven Profil der Angriff (1,0 Schaden + 0,2 Distanz) knapp gegen den
    /// Rückzug (1,25 − 0,1).
    /// </summary>
    [Fact]
    public void OberhalbDerSchwelle_KonkurriertEinMoeglicherAngriffMitDemRueckzug()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(eigene);
        eigene.ApplyDamage(eigene.MaxHitPoints - 150);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10));
        state.AddUnit(new TestUnit(Id(102), Team.TeamB, new HexCoordinate(-1, 5), speed: 10));

        var angriff = ThresholdHeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(-1, 4)), new TurnAction.Attack(state.Units.Single(u => u.Id == Id(102))))!;
        var rueckzug = ThresholdHeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(1, 1)))!;

        Assert.Equal(0, angriff.Retreat);
        Assert.Equal(125, rueckzug.Retreat);
        Assert.True(angriff.Weighted(ThresholdWeights.DefensiveProfile) > rueckzug.Weighted(ThresholdWeights.DefensiveProfile));
    }

    /// <summary>
    /// Ein angehefteter Hugger ist nicht angreifbar und zählt deshalb nicht als
    /// "nächster Gegner" - Heranrücken bezieht sich auf den nächsten angreifbaren.
    /// </summary>
    [Fact]
    public void Distanzgewinn_IgnoriertEinenNichtAngreifbarenGegner()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var umklammerter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var hugger = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 2), DefaultHeroes.Hugger);
        state.AddUnit(eigene);
        state.AddUnit(umklammerter);
        state.AddUnit(hugger);
        state.AddUnit(new TestUnit(Id(102), Team.TeamB, new HexCoordinate(0, 5), speed: 10));
        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(umklammerter), state, greifen)); // Hugger zieht auf (0,1)

        var heran = ThresholdHeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(0, 2)))!;

        Assert.Equal(2, heran.DistanceGain); // Distanz zum angreifbaren Gegner auf (0,5): 5 -> 3
    }

    // -----------------------------------------------------------------
    // Schutz nur gegen tatsächlich bestehende Bedrohung
    // -----------------------------------------------------------------

    [Fact]
    public void Schildwall_WirdNurMitGegnerInReichweitePositivBewertet()
    {
        ThresholdTurnScore Bewerte(int gegnerZeile)
        {
            var state = NewState();
            var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
            state.AddUnit(tank);
            state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10));
            state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, gegnerZeile), speed: 10));
            return ThresholdHeuristicAgent.EvaluateSequence(tank, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(tank.Position)))!;
        }

        var ohneGegner = Bewerte(5); // Distanz 4 > Reichweite 3
        Assert.Equal(0, ohneGegner.ProtectionGain);
        Assert.False(ohneGegner.Weighted(ThresholdWeights.DefensiveProfile) > 0);
        Assert.False(ohneGegner.Weighted(ThresholdWeights.AggressiveProfile) > 0);

        var mitGegner = Bewerte(3); // erreicht beide geschützten Einheiten: 100 × (1 − 0,5), EINMAL
        Assert.Equal(50, mitGegner.ProtectionGain);
        Assert.True(mitGegner.Weighted(ThresholdWeights.DefensiveProfile) > 0);
        Assert.True(mitGegner.Weighted(ThresholdWeights.AggressiveProfile) > 0);
    }

    /// <summary>
    /// Der Gegner wählt das ungeschützte Ziel: erreicht er neben den geschützten
    /// Einheiten auch eine ungeschützte, fängt der Schildwall nichts ab.
    /// </summary>
    [Fact]
    public void Schildwall_IstNichtsWertWennDerGegnerEinUngeschuetztesZielErreicht()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
        state.AddUnit(tank);
        state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 2), speed: 10)); // nicht auf dem Schildwall-Feld
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 3), speed: 10)); // erreicht beide

        var score = ThresholdHeuristicAgent.EvaluateSequence(tank, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(tank.Position)))!;

        Assert.Equal(0, score.ProtectionGain);
    }

    // -----------------------------------------------------------------
    // Weitere Terme
    // -----------------------------------------------------------------

    [Fact]
    public void Angriff_ZaehltSchadenUndBesiegteGegner()
    {
        var state = NewState();
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        state.AddUnit(gegner);
        gegner.ApplyDamage(gegner.MaxHitPoints - 50);

        var score = ThresholdHeuristicAgent.EvaluateSequence(angreifer, state, new TurnAction.Attack(gegner))!;

        Assert.Equal(50, score.Damage);
        Assert.Equal(1, score.Defeated);
        Assert.Equal(0, score.DistanceGain); // der besiegte Gegner zählt bei der Distanz nicht
        Assert.Equal(0.5 * ThresholdHeuristicSettings.DefensiveDamageWeight + ThresholdHeuristicSettings.DefeatWeight, score.Weighted(ThresholdWeights.DefensiveProfile), 9);
    }

    [Fact]
    public void Agent_NutztDieRandomInstanzNicht()
    {
        var state = NewState();
        var eigene = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Berserker);
        state.AddUnit(eigene);
        state.AddUnit(new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 3), DefaultHeroes.Elfe));
        var agent = new ThresholdHeuristicAgent(ThresholdWeights.AggressiveProfile);

        var random = new Random(5);
        var erste = agent.DecideAction(eigene, state, VolleBudget, random);

        Assert.Equal(new Random(5).Next(), random.Next());
        Assert.Equal(erste.ToString(), agent.DecideAction(eigene, state, VolleBudget, new Random(999)).ToString());
    }

    /// <summary>
    /// Die Schwellen-Agenten sind vorerst deaktiviert (<see cref="AgentNames.ThresholdAgentsEnabled"/>):
    /// die Tests hier erzeugen sie deshalb über die weiterhin vorhandenen
    /// Fabrikmethoden statt über <see cref="AgentConfiguration.FromName"/>.
    /// </summary>
    private static AgentConfiguration Konfiguration(string agentName) => agentName switch
    {
        AgentNames.ThresholdAggressive => AgentConfiguration.ThresholdAggressiveVsThresholdAggressive(),
        AgentNames.ThresholdDefensive => AgentConfiguration.ThresholdDefensiveVsThresholdDefensive(),
        _ => throw new ArgumentException(agentName),
    };

    [Theory]
    [InlineData(AgentNames.ThresholdAggressive)]
    [InlineData(AgentNames.ThresholdDefensive)]
    public void Fabrikmethoden_ErzeugenDieSchwellenAgenten(string name)
    {
        var config = Konfiguration(name);
        Assert.IsType<ThresholdHeuristicAgent>(config.CreateTeamAAgent());
        Assert.IsType<ThresholdHeuristicAgent>(config.CreateTeamBAgent());
        Assert.Equal(name, config.Name);
    }

    /// <summary>Solange deaktiviert: weder in der Auswahlliste (Godot, CLI) noch über FromName wählbar.</summary>
    [Theory]
    [InlineData("threshold-aggressive")]
    [InlineData("THRESHOLD-DEFENSIVE")]
    public void Deaktiviert_NichtAuswaehlbar(string name)
    {
        Assert.False(AgentNames.ThresholdAgentsEnabled);
        Assert.DoesNotContain(name.ToLowerInvariant(), AgentNames.All);
        Assert.Throws<ArgumentException>(() => AgentConfiguration.FromName(name));
        Assert.Equal(new[] { AgentNames.ByActionType, AgentNames.Uniform, AgentNames.Aggressive, AgentNames.Defensive, AgentNames.Lookahead }, AgentNames.All);
    }

    // -----------------------------------------------------------------
    // Determinismus, Threads, Etiketten-Unabhängigkeit
    // -----------------------------------------------------------------

    [Theory]
    [InlineData(AgentNames.ThresholdAggressive)]
    [InlineData(AgentNames.ThresholdDefensive)]
    public void GleicherSeed_UndEinOderMehrereThreads_LiefernIdentischeErgebnisse(string agentName)
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };
        var agents = Konfiguration(agentName);

        var einThread = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 1);
        var nochmal = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 1);
        var mehrere = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 8);

        var leer = new Dictionary<string, HeroActionCounts>();
        foreach (var anderer in new[] { nochmal, mehrere })
        {
            Assert.Equal(
                einThread.PairingResults.Select(r => r with { HeroActionTotals = leer }),
                anderer.PairingResults.Select(r => r with { HeroActionTotals = leer }));
            for (int i = 0; i < einThread.PairingResults.Count; i++)
            {
                Assert.Equal(
                    einThread.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                    anderer.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal));
            }
        }
    }

    /// <summary>Alle 84 Spiegelpaarungen mit 2 Ausgangslagen je Paarung (normaler Testlauf).</summary>
    [Theory]
    [InlineData(AgentNames.ThresholdAggressive)]
    [InlineData(AgentNames.ThresholdDefensive)]
    public void AlleSpiegelpaarungen_Stichprobe_SindEtikettenunabhaengig(string agentName) =>
        PruefeSpiegelpaarungen(agentName, setups: 2);

    /// <summary>Alle 84 Spiegelpaarungen × alle 72 Ausgangslagen - langlaufend.</summary>
    [Theory]
    [Trait("Kategorie", "Langlaufend")]
    [InlineData(AgentNames.ThresholdAggressive)]
    [InlineData(AgentNames.ThresholdDefensive)]
    public void AlleSpiegelpaarungenAlle72Ausgangslagen_SindEtikettenunabhaengig(string agentName) =>
        PruefeSpiegelpaarungen(agentName, setups: null);

    private static void PruefeSpiegelpaarungen(string agentName, int? setups)
    {
        var mirrors = CompositionGenerator.GenerateAllCompositions(DefaultHeroes.CreateDefaultCatalog()).Select(c => (c, c)).ToList();
        Assert.Equal(84, mirrors.Count);

        var result = BatchSimulationRunner.Run(
            new GameBoard(), mirrors, Konfiguration(agentName), baseSeed: 2026,
            maxSetupsPerPairing: setups, degreeOfParallelism: Environment.ProcessorCount, verifyNameIndependence: true);

        var report = result.NameIndependence!;
        Assert.Equal(84 * (setups ?? 72), report.CheckedMatches);
        Assert.True(report.Violations.Count == 0, string.Join("\n", report.Violations.Take(5).Select(v => v.Describe())));
    }
}
