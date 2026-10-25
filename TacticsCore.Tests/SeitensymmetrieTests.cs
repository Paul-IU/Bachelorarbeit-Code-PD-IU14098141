using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Seitensymmetrische Gleichstandsregel der heuristischen Agenten (Bachelorarbeit,
/// Korrektur des Seitenvorteils): Die Zielfelder werden aus Sicht der eigenen Blüte
/// sortiert (<see cref="HeuristicAgent"/>, CandidateKey). Eine Partie mit vertauschten
/// Seiten (<see cref="AusgangslagenGenerator.GenerateAllSetups"/>, swapSides) muss
/// dadurch Zug für Zug spiegelbildlich verlaufen und exakt dasselbe Ergebnis haben.
/// </summary>
public class SeitensymmetrieTests
{
    private static readonly IReadOnlyList<TeamComposition> Kompositionen =
        CompositionGenerator.GenerateAllCompositions(DefaultHeroes.CreateDefaultCatalog());

    [Fact]
    public void GameState_EigeneBluete_NormalUndVertauscht()
    {
        var board = new GameBoard();
        var normal = AusgangslagenGenerator.BuildGameState(
            AusgangslagenGenerator.GenerateAllSetups(Kompositionen[0], Kompositionen[40], board)[0], board);
        var vertauscht = AusgangslagenGenerator.BuildGameState(
            AusgangslagenGenerator.GenerateAllSetups(Kompositionen[0], Kompositionen[40], board, swapSides: true)[0], board);

        Assert.False(normal.SidesSwapped);
        Assert.Equal(FieldType.BlossomA, normal.HomeBlossomOf(Team.TeamA));
        Assert.Equal(FieldType.BlossomB, normal.HomeBlossomOf(Team.TeamB));
        Assert.True(vertauscht.SidesSwapped);
        Assert.Equal(FieldType.BlossomB, vertauscht.HomeBlossomOf(Team.TeamA));
        Assert.Equal(FieldType.BlossomA, vertauscht.HomeBlossomOf(Team.TeamB));
        Assert.True(vertauscht.CloneForSimulation().SidesSwapped); // die Simulationskopie des Agenten kennt die Seiten
        Assert.False(new GameState(board).SidesSwapped);           // ohne Ausgangslage: Team A auf Blüte A
    }

    /// <summary>
    /// Sichtlinie (<see cref="Hex.HexLine.Between"/>): für JEDES Feldpaar des Spielfelds liefert die Linie
    /// zwischen den gespiegelten Feldern exakt die gespiegelten Felder der ursprünglichen Linie; Start und Ziel
    /// sind immer die Endpunkte, die Länge ist Distanz + 1.
    /// </summary>
    [Fact]
    public void Sichtlinie_IstFuerAlleFeldpaareSpiegelsymmetrisch()
    {
        var felder = new GameBoard().Fields.Select(f => f.Coordinate).ToList();
        foreach (var a in felder)
        {
            foreach (var b in felder)
            {
                var linie = Hex.HexLine.Between(a, b);
                var gespiegelt = Hex.HexLine.Between(GameBoard.MirrorBetweenBlossoms(a), GameBoard.MirrorBetweenBlossoms(b));

                Assert.Equal(linie.Select(GameBoard.MirrorBetweenBlossoms), gespiegelt);
                Assert.Equal(a, linie[0]);
                Assert.Equal(b, linie[^1]);
                Assert.Equal(Hex.HexCoordinate.Distance(a, b) + 1, linie.Count);
                Assert.All(linie.Zip(linie.Skip(1)), p => Assert.Equal(1, Hex.HexCoordinate.Distance(p.First, p.Second))); // lückenlos
            }
        }
    }

    /// <summary>
    /// Erster Entscheidungsschritt jeder Einheit in der Ausgangsaufstellung: mit
    /// vertauschten Seiten muss der Agent das gespiegelte Zielfeld wählen.
    /// </summary>
    [Theory]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void ErsteEntscheidung_IstMitVertauschtenSeitenGespiegelt(string agentName)
    {
        var board = new GameBoard();
        var agents = AgentConfiguration.FromName(agentName);
        foreach (var (a, b) in new[] { (0, 83), (5, 61), (12, 12), (30, 77) })
        {
            var normalSetups = AusgangslagenGenerator.GenerateAllSetups(Kompositionen[a], Kompositionen[b], board);
            var vertauschtSetups = AusgangslagenGenerator.GenerateAllSetups(Kompositionen[a], Kompositionen[b], board, swapSides: true);
            for (int i = 0; i < normalSetups.Count; i += 7)
            {
                var normal = AusgangslagenGenerator.BuildGameState(normalSetups[i], board);
                var vertauscht = AusgangslagenGenerator.BuildGameState(vertauschtSetups[i], board);
                foreach (var einheit in normal.Units)
                {
                    var gegenstueck = vertauscht.Units.Single(u => u.Id == einheit.Id);
                    var agent = einheit.Team == Team.TeamA ? agents.CreateTeamAAgent() : agents.CreateTeamBAgent();
                    var budget = new TurnBudget(CanMove: true, CanAct: true);

                    var zugNormal = agent.DecideAction(einheit, normal, budget, new Random(0));
                    var zugVertauscht = agent.DecideAction(gegenstueck, vertauscht, budget, new Random(0));

                    Assert.Equal(Beschreibe(zugNormal, gespiegelt: true), Beschreibe(zugVertauscht, gespiegelt: false));
                }
            }
        }
    }

    /// <summary>
    /// Prüfung vor den Läufen: 1.008 Partien des aggressiven Agenten (14 über den
    /// Katalog verteilte Paarungen × 72 Ausgangslagen) normal und mit vertauschten
    /// Seiten. Jede Partie muss exakt dasselbe Ergebnis haben - Sieger, Endgrund,
    /// Runden, Überlebende und Rest-Lebenspunkte je Team (die Etiketten bleiben beim
    /// Seitentausch erhalten, gespiegelt sind nur die Felder).
    /// </summary>
    [Fact]
    public void Aggressiv_1008Partien_NormalUndVertauscht_SindIdentisch() =>
        PruefeIdentisch(AgentNames.Aggressive, paarungen: 14);

    /// <summary>Dieselbe Prüfung für den defensiven Agenten an denselben 1.008 Partien.</summary>
    [Fact]
    public void Defensiv_1008Partien_NormalUndVertauscht_SindIdentisch() =>
        PruefeIdentisch(AgentNames.Defensive, paarungen: 14);

    /// <summary>Schwellen-Agent (mitkorrigiert, nicht Teil der Erhebung): 216 Partien je Ausrichtung.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Schwellenagent_216Partien_NormalUndVertauscht_SindIdentisch(bool aggressiv) =>
        PruefeIdentisch(aggressiv ? AgentConfiguration.ThresholdAggressiveVsThresholdAggressive() : AgentConfiguration.ThresholdDefensiveVsThresholdDefensive(),
            paarungen: 3);

    /// <summary>
    /// Teamreihenfolge getauscht: dieselbe Paarung, aber Komposition Y ist Team A und X Team B, sonst alles gleich.
    /// Zu jeder Ausgangslage gibt es genau ein Gegenstück (Felder gespiegelt, dieselbe Komposition beginnt).
    /// Behandelt die Simulation beide Teams gleich - Etiketten, Seiten UND Id-Reihenfolge -, geht jede Partie
    /// gespiegelt gleich aus. Vor der Korrektur der Id-Vergabe (Ids in absoluter Startfeld-Reihenfolge) wichen
    /// 967 (aggressive) bzw. 866 (defensive) dieser 1.008 Partien ab.
    /// </summary>
    [Theory]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void TeamreihenfolgeGetauscht_1008Partien_SindGespiegeltIdentisch(string agentName)
    {
        var board = new GameBoard();
        var agents = AgentConfiguration.FromName(agentName);
        var paare = new List<(GameSetup Normal, GameSetup Getauscht, string Name)>();
        for (int k = 0; k < 14; k++)
        {
            var x = Kompositionen[k * 6 % 84];
            var y = Kompositionen[(k * 37 + 11) % 84];
            var getauscht = AusgangslagenGenerator.GenerateAllSetups(y, x, board);
            foreach (var s in AusgangslagenGenerator.GenerateAllSetups(x, y, board))
            {
                var yErwartet = s.TeamBUnits.Select(u => (u.Hero.Id, GameBoard.MirrorBetweenBlossoms(u.Position))).ToHashSet();
                var xErwartet = s.TeamAUnits.Select(u => (u.Hero.Id, GameBoard.MirrorBetweenBlossoms(u.Position))).ToHashSet();
                var start = s.StartingTeam == Team.TeamA ? Team.TeamB : Team.TeamA;
                var t = getauscht.Single(g => g.StartingTeam == start
                    && g.TeamAUnits.Select(u => (u.Hero.Id, u.Position)).ToHashSet().SetEquals(yErwartet)
                    && g.TeamBUnits.Select(u => (u.Hero.Id, u.Position)).ToHashSet().SetEquals(xErwartet));
                paare.Add((s, t, $"{x.Key} gegen {y.Key}"));
            }
        }

        var abweichungen = new System.Collections.Concurrent.ConcurrentBag<string>();
        Parallel.ForEach(paare, p =>
        {
            string normal = NachKomposition(Spiele(p.Normal, board, agents), xIstA: true);
            string getauscht = NachKomposition(Spiele(p.Getauscht, board, agents), xIstA: false);
            if (normal != getauscht)
            {
                abweichungen.Add($"{p.Name}: normal {normal}, getauscht {getauscht}");
            }
        });

        Assert.Equal(1008, paare.Count);
        Assert.True(abweichungen.IsEmpty, $"{abweichungen.Count} von 1008 Partien weichen ab:\n" + string.Join("\n", abweichungen.Take(20)));
    }

    private static MatchResult Spiele(GameSetup setup, GameBoard board, AgentConfiguration agents) =>
        new MatchController(AusgangslagenGenerator.BuildGameState(setup, board), agents.CreateTeamAAgent(), agents.CreateTeamBAgent(), seed: 0)
            .Run(setup.StartingTeam);

    /// <summary>Ergebnis aus Sicht der Kompositionen X und Y statt der Teametiketten.</summary>
    private static string NachKomposition(MatchResult r, bool xIstA)
    {
        var (x, y) = xIstA ? (r.TeamAResult, r.TeamBResult) : (r.TeamBResult, r.TeamAResult);
        string sieger = r.Winner switch
        {
            MatchWinner.TeamA => xIstA ? "X" : "Y",
            MatchWinner.TeamB => xIstA ? "Y" : "X",
            _ => "unentschieden",
        };
        return $"{sieger}/{r.EndReason}/R{r.RoundsPlayed}/Ü{x.SurvivingUnitCount}:{y.SurvivingUnitCount}/HP{x.TotalCurrentHitPoints}:{y.TotalCurrentHitPoints}";
    }

    private static void PruefeIdentisch(string agentName, int paarungen) =>
        PruefeIdentisch(AgentConfiguration.FromName(agentName), paarungen);

    private static void PruefeIdentisch(AgentConfiguration agents, int paarungen)
    {
        var board = new GameBoard();
        var auswahl = Enumerable.Range(0, paarungen)
            .Select(k => (Kompositionen[k * 6 % 84], Kompositionen[(k * 37 + 11) % 84]))
            .ToList();

        var normal = BatchSimulationRunner.Run(board, auswahl, agents, baseSeed: 2026,
            degreeOfParallelism: Environment.ProcessorCount, recordMatches: true);
        var vertauscht = BatchSimulationRunner.Run(board, auswahl, agents, baseSeed: 2026,
            degreeOfParallelism: Environment.ProcessorCount, recordMatches: true, swapSides: true);

        var abweichungen = normal.Matches!.Zip(vertauscht.Matches!)
            .Where(p => Ergebnis(p.First) != Ergebnis(p.Second))
            .Select(p => $"Paarung {p.First.PairingIndex} ({auswahl[p.First.PairingIndex].Item1.Key} gegen {auswahl[p.First.PairingIndex].Item2.Key}), " +
                         $"Ausgangslage {p.First.SetupIndex}: normal {Ergebnis(p.First)}, vertauscht {Ergebnis(p.Second)}")
            .ToList();

        Assert.Equal(paarungen * 72, normal.Matches!.Count);
        Assert.True(abweichungen.Count == 0,
            $"{abweichungen.Count} von {normal.Matches!.Count} Partien weichen ab:\n" + string.Join("\n", abweichungen.Take(20)));
    }

    private static string Ergebnis(MatchRecord m) =>
        $"{m.Winner}/{m.EndReason}/{m.Resolution}/R{m.RoundsPlayed}/Ü{m.TeamASurvivingHeroes}:{m.TeamBSurvivingHeroes}/HP{m.TeamARemainingHitPoints}:{m.TeamBRemainingHitPoints}";

    private static string Beschreibe(TurnAction zug, bool gespiegelt)
    {
        string Feld(Hex.HexCoordinate c) => (gespiegelt ? GameBoard.MirrorBetweenBlossoms(c) : c).ToString();
        return zug switch
        {
            TurnAction.Move m => $"Bewegung {Feld(m.Target)}",
            TurnAction.Attack a => $"Angriff {a.Target.Id}",
            TurnAction.UseAbility u => u.Target switch
            {
                AbilityTarget.UnitTarget t => $"Fähigkeit {u.Mode?.Id} auf {t.Unit.Id}",
                AbilityTarget.FieldTarget f => $"Fähigkeit {u.Mode?.Id} auf Feld {Feld(f.Field)}",
                _ => $"Fähigkeit {u.Mode?.Id}",
            },
            _ => zug.GetType().Name,
        };
    }
}
