using TacticsCore.Abilities;
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

/// <summary>
/// Tests für die Ereignismeldung (Teil D): erwartete Ereignisfolge für eine
/// bekannte, vollständig gescriptete Partie, Unabhängigkeit von Live-Meldung
/// (<c>onEvent</c>) und Aufzeichnung (<c>recordReplay</c>), sowie die
/// vollständige Auslesbarkeit des Spielzustands während einer Partie.
/// </summary>
public class GameEventTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void BekannterZug_LiefertDieErwarteteEreignisfolge()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 20);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        agent.Script(attacker, new TurnAction.Attack(enemy), new TurnAction.Move(new HexCoordinate(0, 0)));

        var events = new List<ReplayEntry>();
        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1, onEvent: events.Add);

        controller.ExecuteTurn(attacker, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Equal(
            new[]
            {
                ReplayActionType.TurnStarted,
                ReplayActionType.Attack,
                ReplayActionType.Move,
                ReplayActionType.TurnEnded,
            },
            events.Select(e => e.ActionType));
    }

    [Fact]
    public void BekanntePartie_LiefertBisZumEndeDieErwarteteRahmung()
    {
        // Zwei Standardfiguren, eine schlägt die andere in genau fünf
        // Treffern (50 HP / 10 Schaden) - vollständig deterministisch
        // gescriptet, keine Zufallsentscheidung im Spiel.
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 20);
        var victim = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(victim);

        // Je Zug werden ZWEI Teilaktionen abgefragt (Bewegung + Aktion) -
        // ScriptedAgent.Script ERSETZT die Warteschlange bei jedem Aufruf,
        // statt sie zu erweitern, daher hier EIN Aufruf mit der kompletten,
        // ineinander verschachtelten Folge "Angriff, Zugende" mal fünf.
        var agentA = new ScriptedAgent();
        var script = new List<TurnAction>();
        for (int i = 0; i < 5; i++)
        {
            script.Add(new TurnAction.Attack(victim));
            script.Add(TurnAction.EndTurn.Instance);
        }
        agentA.Script(attacker, script.ToArray());

        var events = new List<ReplayEntry>();
        var controller = new MatchController(state, agentA, new ScriptedAgent(), seed: 1, onEvent: events.Add);
        var result = controller.Run();

        Assert.Equal(MatchWinner.TeamA, result.Winner);
        Assert.Equal(ReplayActionType.RoundStarted, events[0].ActionType); // Partie beginnt mit einem Rundenbeginn
        Assert.Equal(ReplayActionType.MatchEnded, events[^1].ActionType); // ... und endet mit dem Partieende als letztem Ereignis
        Assert.Equal(MatchWinner.TeamA, events[^1].Winner);
        Assert.Equal(MatchEndReason.Elimination, events[^1].EndReason);

        // Jedes TurnStarted hat ein passendes TurnEnded - keine Zug-Klammer bleibt offen.
        Assert.Equal(
            events.Count(e => e.ActionType == ReplayActionType.TurnStarted),
            events.Count(e => e.ActionType == ReplayActionType.TurnEnded));
    }

    [Fact]
    public void OnEvent_FeuertAuchOhneAktivierteAufzeichnung()
    {
        var state = NewState();
        var unit = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(unit);

        var agent = new ScriptedAgent();
        agent.Script(unit, TurnAction.EndTurn.Instance);

        var liveEvents = new List<ReplayEntry>();
        // recordReplay bleibt auf false - dennoch soll onEvent feuern.
        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1, recordReplay: false, onEvent: liveEvents.Add);

        var result = controller.ExecuteTurn(unit, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.Null(result);
        Assert.NotEmpty(liveEvents);
        Assert.Contains(liveEvents, e => e.ActionType == ReplayActionType.TurnStarted);
        Assert.Contains(liveEvents, e => e.ActionType == ReplayActionType.TurnEnded);
    }

    [Fact]
    public void UnitDefeated_WirdBeiEinemToedlichenAngriffGemeldet()
    {
        var state = NewState();
        var attacker = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 20);
        var victim = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        state.AddUnit(attacker);
        state.AddUnit(victim);
        victim.ApplyDamage(victim.MaxHitPoints - 1); // ein weiterer Treffer genügt

        var agent = new ScriptedAgent();
        agent.Script(attacker, new TurnAction.Attack(victim));

        var events = new List<ReplayEntry>();
        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1, onEvent: events.Add);
        controller.ExecuteTurn(attacker, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        var defeatedEntry = Assert.Single(events, e => e.ActionType == ReplayActionType.UnitDefeated);
        Assert.Equal(victim.Id, defeatedEntry.ActingUnitId);
        Assert.Equal(Team.TeamB, defeatedEntry.ActingTeam);
        // Kommt NACH dem auslösenden Angriff, nicht davor.
        Assert.True(events.FindIndex(e => e.ActionType == ReplayActionType.UnitDefeated) > events.FindIndex(e => e.ActionType == ReplayActionType.Attack));
    }

    [Fact]
    public void UnitDefeated_WirdAuchBeiSchadenUeberZeitGemeldetUndDerEigeneKlonFaelltMit()
    {
        var state = NewState();
        var caster = new TacticsCore.Heroes.Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), TacticsCore.Heroes.DefaultHeroes.Illusionist);
        state.AddUnit(caster);

        var poisonTemplate = new TacticsCore.Effects.EffectTemplate(Name: "Testgift", IsInstant: false, Duration: 3, DamagePerTick: caster.MaxHitPoints);
        TacticsCore.Effects.EffectSystem.Apply(state, TacticsCore.Effects.ActiveEffect.OnUnit(caster, caster, poisonTemplate));

        AbilitySystem.TryUseAbility(caster, new AbilityTarget.UnitTarget(caster), state); // erzeugt einen Klon (Illusionist-Fähigkeit)
        var clone = state.Units.Single(u => u.IsClone);

        var events = new List<ReplayEntry>();
        var agent = new ScriptedAgent();
        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1, onEvent: events.Add);

        // Der Effekt-Tick zu Zugbeginn (VOR jeder Agenten-Entscheidung)
        // besiegt den Caster - sein Klon fällt automatisch mit.
        controller.ExecuteTurn(caster, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);

        Assert.True(caster.IsDefeated);
        Assert.True(clone.IsDefeated);
        Assert.Contains(events, e => e.ActionType == ReplayActionType.UnitDefeated && e.ActingUnitId == caster.Id);
        Assert.Contains(events, e => e.ActionType == ReplayActionType.UnitDefeated && e.ActingUnitId == clone.Id);
        Assert.Contains(events, e => e.ActionType == ReplayActionType.CloneDespawned && e.ActingUnitId == clone.Id);
    }

    [Fact]
    public void GameState_IstWaehrendDerPartieVollstaendigAuslesbar()
    {
        var state = NewState();
        var wand = new TacticsCore.Heroes.Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), TacticsCore.Heroes.DefaultHeroes.Wand);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(wand);
        state.AddUnit(enemy);

        var agent = new ScriptedAgent();
        agent.Script(wand, new TurnAction.UseAbility(new TacticsCore.Abilities.AbilityTarget.FieldTarget(wand.Position)));

        var controller = new MatchController(state, agent, new ScriptedAgent(), seed: 1);
        controller.ExecuteTurn(wand, agent, new Random(1), null, roundNumber: 5, startingTeam: Team.TeamA);

        // Alle Einheiten mit Position/HP/Effekten sind über GameState.Units
        // bzw. GetEffectsOnUnit direkt lesbar - hier speziell die durch die
        // Wand-Fähigkeit gesetzte Bewegungsunfähigkeit.
        Assert.NotEmpty(state.GetEffectsOnUnit(wand.Id));

        // Alle gesperrten Felder sind auslesbar.
        Assert.Contains(state.GetLockedFields(), f => f.Field == wand.Position && f.LockedAgainst == Team.TeamB);

        // Aktuelle Runde bleibt nach Zugende auf dem zuletzt gesetzten Wert stehen.
        Assert.Equal(5, state.CurrentRoundNumber);

        // Zwischen zwei Zügen ist niemand mehr "am Zug".
        Assert.Null(state.CurrentActingUnitId);
    }
}
