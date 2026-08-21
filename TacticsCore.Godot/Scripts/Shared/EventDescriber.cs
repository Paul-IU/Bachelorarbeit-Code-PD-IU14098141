using TacticsCore.Match;

namespace TacticsCoreGodot.Shared;

/// <summary>
/// Übersetzt ein einzelnes <see cref="ReplayEntry"/> (zusammen mit dem
/// Spielzustand UNMITTELBAR danach) in eine lesbare BBCode-Zeile für das
/// Ereignisprotokoll (Vorgabe Punkt 7) - von Live-Spiel (MatchScreen) UND
/// Replay (ReplayScreen) gleichermaßen genutzt, damit dieselbe Partie in
/// beiden Modi identisch beschrieben wird.
///
/// Hält eine wachsende Id-&gt;Name-Zuordnung: eine gerade besiegte oder
/// verschwundene Einheit ist im NACH dem Ereignis aufgenommenen
/// <see cref="BoardRenderModel"/> bereits nicht mehr enthalten - ihr Name
/// muss deshalb aus einem FRÜHEREN Ereignis bekannt sein. Jede Instanz
/// gehört zu GENAU EINER Partie/Zeitleiste, nie global geteilt.
/// </summary>
public sealed class EventDescriber
{
    private readonly Dictionary<Guid, string> _names = new();

    public string Describe(ReplayEntry entry, BoardRenderModel modelAfter)
    {
        foreach (var unit in modelAfter.Units)
        {
            _names[unit.Id] = (unit.IsClone ? "~" : "") + unit.DisplayName;
        }

        string actor = NameOf(entry.ActingUnitId);

        return entry.ActionType switch
        {
            ReplayActionType.Move =>
                $"[{entry.ActingTeam}] {actor} zieht von {entry.MoveSource} nach {entry.MoveTarget}",

            ReplayActionType.Attack =>
                $"[{entry.ActingTeam}] {actor} greift {NameOf(entry.AttackTargetUnitId)} an ({entry.DamageDealt} Schaden)",

            ReplayActionType.UseAbility =>
                $"[{entry.ActingTeam}] {actor} setzt Fähigkeit '{entry.AbilityId}' ein" +
                (entry.AttackTargetUnitId is Guid targetId ? $" (Ziel: {NameOf(targetId)})" : "") +
                (entry.EffectField is not null ? $" (Zielfeld: {entry.EffectField})" : "") +
                (entry.ModeId is not null ? $" [Modus: {entry.ModeId}]" : ""),

            ReplayActionType.EffectApplied =>
                $"    → Effekt '{entry.EffectName}' angewendet" +
                (entry.RelatedUnitId is Guid onUnit ? $" auf {NameOf(onUnit)}" : "") +
                (entry.EffectField is not null ? $" auf Feld {entry.EffectField}" : ""),

            ReplayActionType.EffectTicked =>
                $"    → Effekt '{entry.EffectName}' tickt",

            ReplayActionType.EffectExpired =>
                $"    → Effekt '{entry.EffectName}' ist abgelaufen",

            ReplayActionType.UnitDefeated =>
                $"[color=orangered][{entry.ActingTeam}] {actor} ist besiegt![/color]",

            ReplayActionType.CloneSpawned =>
                $"[{entry.ActingTeam}] Klon von {NameOf(entry.RelatedUnitId)} erscheint",

            ReplayActionType.CloneDespawned =>
                $"[{entry.ActingTeam}] Klon von {NameOf(entry.RelatedUnitId)} verschwindet",

            ReplayActionType.TurnStarted =>
                $"[i]— Zug von {actor} beginnt —[/i]",

            ReplayActionType.TurnEnded =>
                $"[i]— Zug von {actor} endet —[/i]",

            ReplayActionType.RoundStarted =>
                $"[b]== Runde {entry.RoundNumber} ==[/b]",

            ReplayActionType.MatchEnded =>
                $"[b]Partie beendet - Sieger: {entry.Winner} ({entry.EndReason}, Runde {entry.RoundNumber})[/b]",

            _ => entry.ActionType.ToString(),
        };
    }

    private string NameOf(Guid? id) => id is Guid value ? NameOf(value) : "?";

    private string NameOf(Guid id) => _names.TryGetValue(id, out var name) ? name : id.ToString("N")[..8];
}
