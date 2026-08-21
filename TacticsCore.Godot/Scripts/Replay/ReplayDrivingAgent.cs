using System;
using System.Collections.Generic;
using System.Linq;
using TacticsCore;
using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCoreGodot.Replay;

/// <summary>
/// Spielt eine AUFGEZEICHNETE Partie erneut durch, indem exakt die im
/// Replay-Protokoll festgehaltenen Entscheidungen wieder zurückgegeben
/// werden, statt den Spielzustand von Hand nachzubauen - der
/// <see cref="MatchController"/> läuft dadurch ein zweites Mal, diesmal
/// aber vollständig deterministisch aus dem Protokoll gesteuert, und
/// erzeugt so byte-genau dieselbe Ereignisfolge samt Zwischenzuständen
/// (siehe <see cref="ReplayTimeline"/>). Dadurch dupliziert die Oberfläche
/// KEINE Spielregel für das Abspielen - sie nutzt exakt dieselbe Simulation
/// wie beim Live-Spiel.
///
/// ZUORDNUNG: <see cref="ReplayEntry"/> trägt bereits seine eigene
/// Rundennummer - Einträge werden daher direkt nach (Runde, handelnde
/// Einheit) gruppiert, ohne die TurnStarted/TurnEnded-Rahmung eigens
/// nachverfolgen zu müssen. Innerhalb einer Gruppe bleibt die
/// protokollierte Reihenfolge erhalten (FIFO) - bei höchstens zwei
/// Teilaktionen je Zug (eine Bewegung, eine Aktion) genügt das, um exakt
/// dieselbe Reihenfolge wie im Original zu reproduzieren.
/// </summary>
public sealed class ReplayDrivingAgent : IAgent
{
    private readonly Dictionary<(int Round, Guid UnitId), Queue<ReplayEntry>> _actionsByRoundAndUnit;

    private ReplayDrivingAgent(Dictionary<(int, Guid), Queue<ReplayEntry>> actionsByRoundAndUnit)
    {
        _actionsByRoundAndUnit = actionsByRoundAndUnit;
    }

    public static ReplayDrivingAgent Build(ReplayLog replay)
    {
        var buckets = new Dictionary<(int, Guid), Queue<ReplayEntry>>();

        foreach (var entry in replay.Entries)
        {
            if (entry.ActionType is not (ReplayActionType.Move or ReplayActionType.Attack or ReplayActionType.UseAbility))
            {
                continue; // alles andere (Effekte, Klon-/Zug-/Rundenereignisse, Partieende) ergibt sich automatisch wieder aus dem erneuten Ausführen derselben Entscheidungen
            }

            var key = (entry.RoundNumber, entry.ActingUnitId);
            if (!buckets.TryGetValue(key, out var queue))
            {
                queue = new Queue<ReplayEntry>();
                buckets[key] = queue;
            }

            queue.Enqueue(entry);
        }

        return new ReplayDrivingAgent(buckets);
    }

    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        var key = (state.CurrentRoundNumber, unit.Id);
        if (!_actionsByRoundAndUnit.TryGetValue(key, out var queue) || queue.Count == 0)
        {
            return TurnAction.EndTurn.Instance;
        }

        var entry = queue.Dequeue();
        return entry.ActionType switch
        {
            ReplayActionType.Move => new TurnAction.Move(entry.MoveTarget!.Value),
            ReplayActionType.Attack => new TurnAction.Attack(ResolveUnit(state, entry.AttackTargetUnitId!.Value)),
            ReplayActionType.UseAbility => new TurnAction.UseAbility(ResolveAbilityTarget(state, entry), ResolveMode((Hero)unit, entry.ModeId)),
            _ => TurnAction.EndTurn.Instance, // unerreichbar, siehe Filter in Build()
        };
    }

    private static Unit ResolveUnit(GameState state, Guid id) =>
        state.Units.First(u => u.Id == id);

    private static AbilityTarget ResolveAbilityTarget(GameState state, ReplayEntry entry) =>
        entry.EffectField is { } field
            ? new AbilityTarget.FieldTarget(field)
            : new AbilityTarget.UnitTarget(ResolveUnit(state, entry.AttackTargetUnitId!.Value));

    private static AbilityMode? ResolveMode(Hero hero, string? modeId) =>
        modeId is null ? null : hero.Definition.Ability.Modes?.SingleOrDefault(m => m.Id == modeId);
}
