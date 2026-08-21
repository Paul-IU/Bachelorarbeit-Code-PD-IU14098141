using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

public class MatchControllerTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    // -----------------------------------------------------------------
    // Aktionsbudget pro Zug (Anforderung 3) - getestet über die intern
    // sichtbare ExecuteTurn-Methode, damit ein einzelner Zug ohne
    // vollständigen (bis zu 100 Runden langen) Run()-Durchlauf geprüft
    // werden kann.
    // -----------------------------------------------------------------

    [Fact]
    public void ExecuteTurn_BewegungVorAngriff_BeideAktionenWerdenAusgefuehrt()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10); // Distanz 2 - erst nach der Bewegung angreifbar
        state.AddUnit(unit);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        agent.Script(unit,
            new TurnAction.Move(new HexCoordinate(0, 1)),
            new TurnAction.Attack(enemy));
        var replay = new List<ReplayEntry>();

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        var result = controller.ExecuteTurn(unit, agent, new Random(1), replay, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Null(result); // Partie dadurch nicht beendet
        Assert.Equal(new HexCoordinate(0, 1), unit.Position);
        Assert.Equal(enemy.MaxHitPoints - unit.BaseAttackDamage, enemy.CurrentHitPoints);

        // Replay hält Quelle UND Ziel der Bewegung fest, nicht nur das Ziel.
        var moveEntry = Assert.Single(replay, e => e.ActionType == ReplayActionType.Move);
        Assert.Equal(new HexCoordinate(0, 0), moveEntry.MoveSource);
        Assert.Equal(new HexCoordinate(0, 1), moveEntry.MoveTarget);
    }

    [Fact]
    public void ExecuteTurn_AngriffVorBewegung_BeideAktionenWerdenAusgefuehrt()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10); // direkter Nachbar - sofort angreifbar
        state.AddUnit(unit);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        agent.Script(unit,
            new TurnAction.Attack(enemy),
            new TurnAction.Move(new HexCoordinate(-1, 1)));

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        var result = controller.ExecuteTurn(unit, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Null(result);
        Assert.Equal(enemy.MaxHitPoints - unit.BaseAttackDamage, enemy.CurrentHitPoints);
        Assert.Equal(new HexCoordinate(-1, 1), unit.Position);
    }

    [Fact]
    public void ExecuteTurn_Passen_BeendetDenZugSofortOhneAktionen()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(unit);
        var startPosition = unit.Position;

        var agent = new ScriptedAgent();
        agent.Script(unit, TurnAction.EndTurn.Instance);

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        var result = controller.ExecuteTurn(unit, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Null(result);
        Assert.Equal(startPosition, unit.Position);
        Assert.Equal(1, agent.CallCount(unit)); // nur einmal gefragt, danach sofort beendet
    }

    [Fact]
    public void ExecuteTurn_KompletesPassenOhneJeGefragtZuWerdenIstNichtNoetig_AberLeererZugAendertNichts()
    {
        // Auch ohne jede Aktion (Agent liefert beim allerersten Aufruf
        // schon EndTurn) bleibt der Zustand vollständig unverändert.
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(unit);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent(); // nichts gescriptet -> liefert sofort EndTurn

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        controller.ExecuteTurn(unit, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Equal(new HexCoordinate(0, 0), unit.Position);
        Assert.Equal(enemy.MaxHitPoints, enemy.CurrentHitPoints);
    }

    [Fact]
    public void ExecuteTurn_ZweiterBewegungsversuchImSelbenZug_WirdIgnoriertUndBeendetDenZug()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(unit);

        var agent = new ScriptedAgent();
        agent.Script(unit,
            new TurnAction.Move(new HexCoordinate(0, 1)),
            new TurnAction.Move(new HexCoordinate(-1, 1))); // Budget für Bewegung bereits verbraucht

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        controller.ExecuteTurn(unit, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        // Nur die erste Bewegung wirkte; der zweite (laut Budget ungültige)
        // Versuch hat den Zug sicherheitshalber sofort beendet, statt die
        // Einheit ein zweites Mal zu bewegen.
        Assert.Equal(new HexCoordinate(0, 1), unit.Position);
    }

    [Fact]
    public void ExecuteTurn_ProtokolliertNurTatsaechlichAusgefuehrteAktionenImReplay()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(unit);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        agent.Script(unit, new TurnAction.Attack(enemy), new TurnAction.Move(new HexCoordinate(0, 0)));
        var replay = new List<ReplayEntry>();

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        controller.ExecuteTurn(unit, agent, new Random(1), replay, roundNumber: 3, startingTeam: Team.TeamA);

        // 4 statt 2 Einträge: TurnStarted/TurnEnded rahmen den Zug zusätzlich
        // (Anforderung 16 aus dem Schnittstellen-Teil) - das ändert nichts
        // daran, dass NUR tatsächlich ausgeführte Aktionen dazwischen
        // protokolliert werden (kein Eintrag für ein aktives Passen o.ä.).
        Assert.Equal(4, replay.Count);

        Assert.Equal(ReplayActionType.TurnStarted, replay[0].ActionType);

        Assert.Equal(ReplayActionType.Attack, replay[1].ActionType);
        Assert.Equal(3, replay[1].RoundNumber);
        Assert.Equal(unit.Id, replay[1].ActingUnitId);
        Assert.Equal(Team.TeamA, replay[1].ActingTeam);
        Assert.Equal(enemy.Id, replay[1].AttackTargetUnitId);
        Assert.Equal(unit.BaseAttackDamage, replay[1].DamageDealt);
        Assert.Null(replay[1].MoveTarget);
        Assert.Null(replay[1].MoveSource);

        Assert.Equal(ReplayActionType.Move, replay[2].ActionType);
        Assert.Equal(new HexCoordinate(0, 0), replay[2].MoveSource); // Einheit stand bereits auf (0,0) - "Stehenbleiben"
        Assert.Equal(new HexCoordinate(0, 0), replay[2].MoveTarget);
        Assert.Null(replay[2].AttackTargetUnitId);

        Assert.Equal(ReplayActionType.TurnEnded, replay[3].ActionType);
    }

    // -----------------------------------------------------------------
    // Zugreihenfolge im vollständigen Partieverlauf (Anforderung 2)
    // -----------------------------------------------------------------

    [Fact]
    public void EinHeld_DerVorSeinemZugBesiegtWird_KommtNichtMehrAnDieReihe()
    {
        var state = NewState();
        var fast = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var slow = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(fast);
        state.AddUnit(slow);
        state.AddUnit(enemy);
        // Ein einziger Treffer von enemy genügt, um slow zu besiegen.
        slow.ApplyDamage(slow.MaxHitPoints - enemy.BaseAttackDamage);

        var agentA = new ScriptedAgent(); // fast und slow teilen sich diesen Agenten (beide Team A)
        var agentB = new ScriptedAgent();
        agentB.Script(enemy, new TurnAction.Attack(slow));
        // Unabhängig vom (per Seed zufällig bestimmten) Startteam steht
        // enemy immer auf Rang 1 (einziger Team-B-Held) und slow immer auf
        // Rang 2 - Rang 1 handelt in der Verschränkung stets vor Rang 2,
        // enemys Angriff auf slow passiert also so oder so vor slows Zug.
        var controller = new MatchController(state, agentA, agentB, seed: 1);

        var result = controller.Run();

        Assert.True(slow.IsDefeated);
        Assert.Equal(0, agentA.CallCount(slow)); // nie nach einer Aktion gefragt
        Assert.True(agentA.CallCount(fast) > 0); // fast kam ganz normal dran
        Assert.NotNull(result);
    }

    // -----------------------------------------------------------------
    // Siegbedingung und Partieende (Anforderung 4)
    // -----------------------------------------------------------------

    [Fact]
    public void Partie_EndetSofortWennEinTeamAusgeloeschtWird()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var lastEnemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(lastEnemy);
        lastEnemy.ApplyDamage(lastEnemy.MaxHitPoints - attacker.BaseAttackDamage); // ein Treffer genügt

        var agentA = new ScriptedAgent();
        agentA.Script(attacker, new TurnAction.Attack(lastEnemy));
        var agentB = new ScriptedAgent(); // handelt nie, falls überhaupt an der Reihe

        var controller = new MatchController(state, agentA, agentB, seed: 1);
        var result = controller.Run();

        Assert.Equal(MatchEndReason.Elimination, result.EndReason);
        Assert.Equal(MatchWinner.TeamA, result.Winner);
        Assert.Equal(1, result.RoundsPlayed);
        Assert.Equal(0, result.TeamBResult.SurvivingUnitCount);
        Assert.Equal(1, result.TeamAResult.SurvivingUnitCount);
    }

    [Fact]
    public void Rundenlimit_AufloesungUeberHeldenanzahl_TeamMitMehrLebendenHeldenGewinnt()
    {
        var state = NewState();
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var b1 = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        var b2 = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(0, 4), speed: 10);
        state.AddUnit(a1);
        state.AddUnit(a2);
        state.AddUnit(b1);
        state.AddUnit(b2);

        // Niemand handelt aktiv - die Partie läuft bis zum Rundenlimit
        // durch. b2 wird vorab (außerhalb eines regulären Angriffs)
        // besiegt, damit Team B mit weniger Überlebenden ins Limit geht.
        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        var controller = new MatchController(state, agentA, agentB, seed: 1);
        b2.ApplyDamage(b2.MaxHitPoints);
        state.RemoveUnit(b2);

        var result = controller.Run();

        Assert.Equal(MatchEndReason.RoundLimit, result.EndReason);
        Assert.Equal(MatchRules.RoundLimit, result.RoundsPlayed);
        Assert.Equal(2, result.TeamAResult.SurvivingUnitCount);
        Assert.Equal(1, result.TeamBResult.SurvivingUnitCount);
        Assert.Equal(MatchWinner.TeamA, result.Winner);
    }

    [Fact]
    public void Rundenlimit_AufloesungUeberHpAnteilBeiGleicherHeldenanzahl()
    {
        var state = NewState();
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var b1 = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        var b2 = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(0, 4), speed: 10);
        state.AddUnit(a1);
        state.AddUnit(a2);
        state.AddUnit(b1);
        state.AddUnit(b2);

        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        var controller = new MatchController(state, agentA, agentB, seed: 1);

        // Beide Teams behalten 2 Überlebende, Team B hat aber insgesamt
        // weniger Rest-HP.
        b1.ApplyDamage(30);

        var result = controller.Run();

        Assert.Equal(MatchEndReason.RoundLimit, result.EndReason);
        Assert.Equal(2, result.TeamAResult.SurvivingUnitCount);
        Assert.Equal(2, result.TeamBResult.SurvivingUnitCount);
        Assert.True(result.TeamAResult.RemainingHitPointsFraction > result.TeamBResult.RemainingHitPointsFraction);
        Assert.Equal(MatchWinner.TeamA, result.Winner);
    }

    [Fact]
    public void Rundenlimit_BeiGleichstandInHeldenanzahlUndHpAnteil_EntstehtEinUnentschieden()
    {
        var state = NewState();
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var b1 = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(a1);
        state.AddUnit(b1);

        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        var controller = new MatchController(state, agentA, agentB, seed: 1);

        var result = controller.Run();

        Assert.Equal(MatchEndReason.RoundLimit, result.EndReason);
        Assert.Equal(MatchWinner.Draw, result.Winner);
        Assert.Equal(1, result.TeamAResult.SurvivingUnitCount);
        Assert.Equal(1, result.TeamBResult.SurvivingUnitCount);
        Assert.Equal(result.TeamAResult.RemainingHitPointsFraction, result.TeamBResult.RemainingHitPointsFraction);
    }

    // -----------------------------------------------------------------
    // Replay-Protokoll (Anforderung 6)
    // -----------------------------------------------------------------

    [Fact]
    public void Run_OhneReplayAufzeichnung_LiefertKeinReplayLog()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var lastEnemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(lastEnemy);
        lastEnemy.ApplyDamage(lastEnemy.MaxHitPoints - attacker.BaseAttackDamage);

        var agentA = new ScriptedAgent();
        agentA.Script(attacker, new TurnAction.Attack(lastEnemy));
        var controller = new MatchController(state, agentA, new ScriptedAgent(), seed: 1, recordReplay: false);

        var result = controller.Run();

        Assert.Null(result.Replay);
    }

    [Fact]
    public void Run_MitReplayAufzeichnung_EnthaeltAusgangsaufstellungUndAusgefuehrteAktionen()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var lastEnemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(lastEnemy);
        lastEnemy.ApplyDamage(lastEnemy.MaxHitPoints - attacker.BaseAttackDamage);

        var agentA = new ScriptedAgent();
        agentA.Script(attacker, new TurnAction.Attack(lastEnemy));
        var controller = new MatchController(state, agentA, new ScriptedAgent(), seed: 42, recordReplay: true);

        var result = controller.Run();

        Assert.NotNull(result.Replay);
        Assert.Equal(42, result.Replay!.Seed);
        Assert.Equal(2, result.Replay.InitialSetup.Count);
        Assert.Contains(result.Replay.InitialSetup, s => s.UnitId == attacker.Id && s.StartPosition == new HexCoordinate(0, 0));
        Assert.Contains(result.Replay.Entries, e => e.ActionType == ReplayActionType.Attack && e.ActingUnitId == attacker.Id);
    }
}
