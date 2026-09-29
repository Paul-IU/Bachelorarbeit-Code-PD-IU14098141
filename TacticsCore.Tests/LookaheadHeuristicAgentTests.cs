using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie die darin enthaltene Klasse.
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests des vorausschauenden Agenten (<see cref="LookaheadHeuristicAgent"/>):
/// Suchtiefe genau zwei Ebenen, Bestimmung der nächsten gegnerischen Einheit,
/// Determinismus und Etiketten-Unabhängigkeit.
/// </summary>
public class LookaheadHeuristicAgentTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly HeroCatalog Catalog = DefaultHeroes.CreateDefaultCatalog();
    private static readonly TeamComposition TrioA = new(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
    private static readonly TeamComposition TrioB = new(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

    /// <summary>Eigener Berserker gegen Elfe und Bogenschütze; laufende Runde: Berserker, Elfe, Bogenschütze.</summary>
    private static (GameState State, Hero Berserker, Hero Elfe, Hero Bogen) Aufbau()
    {
        var state = new GameState(new GameBoard());
        var berserker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Berserker);
        var elfe = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 4), DefaultHeroes.Elfe);
        var bogen = new Hero(Id(102), Team.TeamB, new HexCoordinate(1, 4), DefaultHeroes.Bogenschuetze);
        state.AddUnit(berserker);
        state.AddUnit(elfe);
        state.AddUnit(bogen);
        state.SetRoundOrder(new[] { berserker.Id, elfe.Id, bogen.Id }, Team.TeamA);
        return (state, berserker, elfe, bogen);
    }

    /// <summary>Anzahl der Zugfolgen (ohne die leere), die der Agent auf Ebene 1 bewertet - dieselbe Aufzählung wie im Agenten.</summary>
    private static int AnzahlZugfolgen(Unit unit, GameState state)
    {
        int n = 0;
        foreach (var (first, _) in HeuristicAgent.Candidates(unit, state, canMove: true, canAct: true))
        {
            var afterFirst = state.CloneForSimulation();
            if (!HeuristicAgent.Apply(first, unit.Id, afterFirst)) continue;
            n++;
            var actor = afterFirst.Units.FirstOrDefault(u => u.Id == unit.Id);
            if (actor is null || actor.IsDefeated) continue;
            foreach (var (second, _) in HeuristicAgent.Candidates(actor, afterFirst, first is not TurnAction.Move, first is TurnAction.Move))
            {
                if (HeuristicAgent.Apply(second, unit.Id, afterFirst.CloneForSimulation())) n++;
            }
        }
        return n;
    }

    // -----------------------------------------------------------------
    // Suchtiefe
    // -----------------------------------------------------------------

    /// <summary>
    /// Die Suche endet nach GENAU ZWEI Ebenen: Ebene 1 bewertet jede eigene Zugfolge
    /// genau einmal, Ebene 2 simuliert für jede der K besten genau EINEN Gegnerzug -
    /// den der nächsten gegnerischen Einheit (Elfe), nie den einer weiteren Einheit
    /// (Bogenschütze, eigene Einheiten). Würde der simulierte Gegner seinerseits
    /// vorausschauen, entstünden weitere Gegnerzüge bzw. eine Ebene 3.
    /// </summary>
    [Fact]
    public void Suche_EndetNachGenauZweiEbenen()
    {
        var (state, berserker, elfe, _) = Aufbau();
        const int k = 3;
        var ereignisse = new List<LookaheadProbeEvent>();
        var agent = new LookaheadHeuristicAgent(HeuristicWeights.NeutralProfile, k, ereignisse.Add);

        agent.DecideAction(berserker, state, new TurnBudget(CanMove: true, CanAct: true), new Random(1));

        Assert.All(ereignisse, e => Assert.InRange(e.Level, 1, 2));
        var ebene1 = ereignisse.Where(e => e.Level == 1).ToList();
        var ebene2 = ereignisse.Where(e => e.Level == 2).ToList();

        Assert.All(ebene1, e => Assert.Equal(berserker.Id, e.UnitId));
        Assert.Equal(AnzahlZugfolgen(berserker, state), ebene1.Count(e => !e.IsOpponentTurnStart));
        Assert.DoesNotContain(ebene1, e => e.IsOpponentTurnStart);

        Assert.All(ebene2, e => Assert.Equal(elfe.Id, e.UnitId)); // nur die nächste Einheit
        Assert.Equal(k, ebene2.Count(e => e.IsOpponentTurnStart)); // je beschnittener Zugfolge genau ein Gegnerzug
        Assert.True(ebene2.Count(e => !e.IsOpponentTurnStart) > 0);

        // Reihenfolge: alle eigenen Zugfolgen zuerst, danach nur noch Ebene 2 - keine Verschachtelung.
        int letzteEbene1 = ereignisse.FindLastIndex(e => e.Level == 1);
        int ersteEbene2 = ereignisse.FindIndex(e => e.Level == 2);
        Assert.True(letzteEbene1 < ersteEbene2);
    }

    /// <summary>Ohne bekannte Zugreihenfolge (kein Startteam) entfällt die zweite Ebene für alle Zugfolgen.</summary>
    [Fact]
    public void OhneZugreihenfolge_EntscheidetEinstufig()
    {
        var state = new GameState(new GameBoard());
        var berserker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Berserker);
        state.AddUnit(berserker);
        state.AddUnit(new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 4), DefaultHeroes.Elfe));
        var ereignisse = new List<LookaheadProbeEvent>();

        new LookaheadHeuristicAgent(HeuristicWeights.NeutralProfile, 5, ereignisse.Add)
            .DecideAction(berserker, state, new TurnBudget(CanMove: true, CanAct: true), new Random(1));

        Assert.NotEmpty(ereignisse);
        Assert.All(ereignisse, e => Assert.Equal(1, e.Level));
    }

    // -----------------------------------------------------------------
    // Nächste Einheit
    // -----------------------------------------------------------------

    [Fact]
    public void NaechsteEinheit_IstDieNaechsteGegnerischeDerLaufendenRunde()
    {
        var (state, berserker, elfe, _) = Aufbau();
        Assert.Equal(elfe.Id, LookaheadHeuristicAgent.NextEnemyUnit(berserker, state, state)!.Id);
    }

    /// <summary>Eigene Einheiten dazwischen werden übersprungen, ohne ihren Zug zu simulieren.</summary>
    [Fact]
    public void NaechsteEinheit_UeberspringtEigeneEinheiten()
    {
        var (state, berserker, elfe, bogen) = Aufbau();
        var eigene = new Hero(Id(2), Team.TeamA, new HexCoordinate(-1, 1), DefaultHeroes.Musiker);
        state.AddUnit(eigene);
        state.SetRoundOrder(new[] { berserker.Id, eigene.Id, bogen.Id, elfe.Id }, Team.TeamA);

        Assert.Equal(bogen.Id, LookaheadHeuristicAgent.NextEnemyUnit(berserker, state, state)!.Id);
    }

    /// <summary>Eine von der eigenen Zugfolge besiegte Einheit handelt nicht mehr und wird übersprungen.</summary>
    [Fact]
    public void NaechsteEinheit_UeberspringtBesiegteEinheiten()
    {
        var (state, berserker, elfe, bogen) = Aufbau();
        var danach = state.CloneForSimulation();
        var elfeDanach = danach.Units.Single(u => u.Id == elfe.Id);
        elfeDanach.ApplyDamage(elfeDanach.CurrentHitPoints);

        Assert.Equal(bogen.Id, LookaheadHeuristicAgent.NextEnemyUnit(berserker, state, danach)!.Id);
    }

    /// <summary>Handelt der Akteur als letzter der Runde, ist es die erste gegnerische Einheit der nächsten Runde.</summary>
    [Fact]
    public void NaechsteEinheit_AmRundenende_ErsteGegnerischeDerNaechstenRunde()
    {
        var (state, berserker, elfe, bogen) = Aufbau();
        state.SetRoundOrder(new[] { elfe.Id, bogen.Id, berserker.Id }, Team.TeamB);

        var erwartet = TurnOrder.ComputeRoundOrder(state.Units, Team.TeamB, state).First(u => u.Team == Team.TeamB);
        Assert.Equal(erwartet.Id, LookaheadHeuristicAgent.NextEnemyUnit(berserker, state, state)!.Id);
    }

    // -----------------------------------------------------------------
    // Vorausschau ändert die Wahl: Angriff auf den Berserker
    // -----------------------------------------------------------------

    /// <summary>
    /// Lage aus der Stichprobe (Seed 2026, Runde 5): alle drei Helden auf einem Feld;
    /// der gegnerische Elementarmagier (80 HP) hat nur noch seine Aktion. Als Nächster
    /// handelt der Berserker von Team A (300 HP) zu Beginn der nächsten Runde.
    /// </summary>
    private static (GameState State, Hero Magier, Hero BerserkerA) BerserkerLage()
    {
        var state = new GameState(new GameBoard());
        var feld = new HexCoordinate(-1, 1);
        var berserkerA = new Hero(Id(3), Team.TeamA, feld, DefaultHeroes.Berserker);
        var berserkerB = new Hero(Id(101), Team.TeamB, feld, DefaultHeroes.Berserker);
        var magier = new Hero(Id(103), Team.TeamB, feld, DefaultHeroes.Elementarmagier);
        state.AddUnit(berserkerA);
        state.AddUnit(berserkerB);
        state.AddUnit(magier);
        berserkerA.ApplyDamage(100);
        berserkerB.ApplyDamage(120);
        magier.ApplyDamage(240);
        state.SetRoundOrder(new[] { berserkerA.Id, magier.Id, berserkerB.Id }, Team.TeamA);
        return (state, magier, berserkerA);
    }

    /// <summary>
    /// Der vorausschauende Agent wählt einen Angriff auf den Berserker, den der
    /// einstufige Agent mit derselben (neutralen) Gewichtung verwirft - er zieht
    /// dort den Feuerschlag vor. Gemessene Bewertungen (neutral 1:1):
    ///   einstufig: Feuer +90 (30 Sofortschaden + 60 angekündigte Verbrennung),
    ///              Angriff +60 (100 Schaden − 40 Bedrohung: die Berserker-Passive
    ///              steigt mit dem Treffer um eine Stufe).
    ///   nach der Antwort (der Berserker besiegt den Magier mit 80 HP in jedem Fall):
    ///              Angriff −420, Feuer −460. Mit dem Magier enden seine Effekte
    ///              (EffectSystem.RemoveEffectsRelatedTo) - von der Verbrennung bleibt
    ///              nur der eine Tick zu Beginn des Berserker-Zugs, und der hebt die
    ///              Passive ebenfalls um eine Stufe (−40). Der angekündigte Wert, der
    ///              das Feuer einstufig vorn sah, zahlt sich nie aus.
    /// Die Vorausschau erkennt also, dass eine verzögerte Wirkung verfällt, wenn der
    /// Anwender die Antwort nicht überlebt.
    /// </summary>
    [Fact]
    public void Vorausschau_WaehltAngriffAufDenBerserker_DenEinEinstufigerAgentVerwirft()
    {
        var (state, magier, berserkerA) = BerserkerLage();
        var budget = new TurnBudget(CanMove: false, CanAct: true);

        var einstufig = new HeuristicAgent(HeuristicWeights.NeutralProfile).DecideAction(magier, state, budget, new Random(1));
        var vorausschauend = new LookaheadHeuristicAgent().DecideAction(magier, state, budget, new Random(1));

        Assert.IsNotType<TurnAction.Attack>(einstufig);
        var angriff = Assert.IsType<TurnAction.Attack>(vorausschauend);
        Assert.Equal(berserkerA.Id, angriff.Target.Id);
    }

    // -----------------------------------------------------------------
    // Determinismus, Threads, Etiketten-Unabhängigkeit, Auswahl
    // -----------------------------------------------------------------

    [Fact]
    public void Agent_NutztDieRandomInstanzNicht()
    {
        var (state, berserker, _, _) = Aufbau();
        var budget = new TurnBudget(CanMove: true, CanAct: true);
        var agent = new LookaheadHeuristicAgent();

        var random = new Random(5);
        var erste = agent.DecideAction(berserker, state, budget, random);

        Assert.Equal(new Random(5).Next(), random.Next()); // Zufallsstrom unberührt
        Assert.Equal(Describe(erste), Describe(agent.DecideAction(berserker, state, budget, new Random(999))));
    }

    private static string Describe(TurnAction action) => action switch
    {
        TurnAction.Move m => $"Move {m.Target}",
        TurnAction.Attack a => $"Attack {a.Target.Id}",
        TurnAction.UseAbility u => $"Ability {u.Target} {u.Mode?.Id}",
        _ => action.GetType().Name,
    };

    [Fact]
    public void GleicherSeed_UndEinOderMehrereThreads_LiefernIdentischeErgebnisse()
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };
        var agents = AgentConfiguration.FromName(AgentNames.Lookahead);

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

    /// <summary>
    /// Etiketten-Unabhängigkeit über alle 84 Spiegelpaarungen mit der bestehenden
    /// Prüfung (<see cref="NameIndependenceCheck"/>), 2 Ausgangslagen je Paarung.
    /// </summary>
    [Fact]
    public void AlleSpiegelpaarungen_Stichprobe_SindEtikettenunabhaengig()
    {
        var mirrors = CompositionGenerator.GenerateAllCompositions(Catalog).Select(c => (c, c)).ToList();
        Assert.Equal(84, mirrors.Count);

        var result = BatchSimulationRunner.Run(
            new GameBoard(), mirrors, AgentConfiguration.FromName(AgentNames.Lookahead), baseSeed: 2026,
            maxSetupsPerPairing: 2, degreeOfParallelism: Environment.ProcessorCount, verifyNameIndependence: true);

        var report = result.NameIndependence!;
        Assert.Equal(84 * 2, report.CheckedMatches);
        Assert.True(report.Violations.Count == 0, string.Join("\n", report.Violations.Take(5).Select(v => v.Describe())));
    }

    [Theory]
    [InlineData("lookahead")]
    [InlineData("LOOKAHEAD")]
    public void FromName_KenntDenVorausschauendenAgenten(string name)
    {
        var config = AgentConfiguration.FromName(name);
        Assert.IsType<LookaheadHeuristicAgent>(config.CreateTeamAAgent());
        Assert.IsType<LookaheadHeuristicAgent>(config.CreateTeamBAgent());
        Assert.Equal(AgentNames.Lookahead, config.Name);
        Assert.Contains(AgentNames.Lookahead, AgentNames.All);
    }
}
