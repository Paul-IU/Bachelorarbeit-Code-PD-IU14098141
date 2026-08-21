using TacticsCore.Combat;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Effects;

/// <summary>
/// Eigene, von <see cref="ActiveEffect"/> (reine Daten) getrennte
/// Effektverwaltung: Anwenden inklusive Stapelregel, Ticken, Ablaufen und
/// das Aufräumen beim Tod von Quelle oder Ziel.
///
/// ZEITPUNKTE (Anforderung 10 - "klar definierter Zeitpunkt für Ticken und
/// Ablaufen"):
///   - Unit-verankerte Effekte (<see cref="TickUnitEffects"/>) ticken
///     GENAU EINMAL, zu Beginn JEDES Zugs der betroffenen Einheit (siehe
///     MatchController.ExecuteTurn) - noch bevor der Agent nach der ersten
///     Teilaktion gefragt wird. Die Dauer zählt damit in EIGENEN Zügen der
///     Einheit, nicht in Kalenderrunden: wird ein Effekt auf eine Einheit
///     angewendet, bevor diese in der laufenden Runde schon an der Reihe
///     war, tickt er noch in derselben Runde; kam die Einheit in dieser
///     Runde schon dran, tickt er erst wieder in der nächsten.
///   - Feld-verankerte Effekte (<see cref="TickFieldEffects"/>) haben
///     keinen eigenen Zug, an dem sie ticken könnten - ihre Restdauer wird
///     daher einmal zu Beginn JEDER Runde verringert (siehe
///     MatchController.Run).
///
/// Reihenfolge innerhalb eines Tick-Aufrufs: Effekte werden in der
/// Reihenfolge abgearbeitet, in der sie ursprünglich angewendet wurden
/// (stabile Liste, keine Neusortierung) - bei mehreren Schaden-über-Zeit-
/// Stapeln auf derselben Einheit ist das reproduzierbar die
/// Anwendungsreihenfolge.
///
/// REPLAY (Anforderung 13): <paramref name="replayEntries"/>/<paramref name="roundNumber"/>
/// sind überall optional (Default null/0) - ohne sie verhält sich die
/// Effektverwaltung exakt wie zuvor, ohne jeden Mehraufwand. Nur wenn
/// <see cref="Match.MatchController"/> mit aktivierter Aufzeichnung läuft,
/// wird tatsächlich protokolliert.
/// </summary>
public static class EffectSystem
{
    /// <summary>
    /// Wendet einen fertig instanziierten Effekt gemäß seiner Stapelregel
    /// an (Anforderung 10). "Gleichartig" heißt: gleicher Name UND gleiches
    /// Ziel (Unit- oder Feld-Anker) wie ein bereits aktiver Effekt.
    /// </summary>
    public static void Apply(GameState state, ActiveEffect effect, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        var existing = FindMatching(state, effect);
        if (existing is null)
        {
            state.AddEffect(effect);
            LogApplied(effect, replayEntries, roundNumber);
            return;
        }

        switch (effect.Stacking)
        {
            case StackingRule.Replace:
            case StackingRule.ReplaceForSameSource:
                state.RemoveEffect(existing);
                state.AddEffect(effect);
                LogApplied(effect, replayEntries, roundNumber);
                break;

            case StackingRule.RefreshDuration:
                // Keine neue Instanz - nur die Restdauer der bereits
                // bestehenden wird zurückgesetzt, daher kein zusätzlicher
                // "Angewendet"-Eintrag (die erste Anwendung ist bereits
                // protokolliert).
                existing.SetRemainingDuration(effect.RemainingDuration);
                break;

            case StackingRule.Accumulate:
                // Kein Ersetzen/Stapeln - der bestehende Bonus wird um den
                // neuen Wert erhöht, gedeckelt durch die konfigurierte
                // Obergrenze (siehe StackingRule.Accumulate und Bogenschütze).
                int current = existing.BonusDamageForNextAttack ?? 0;
                int increment = effect.BonusDamageForNextAttack ?? 0;
                int cap = effect.MaximumAccumulatedBonusDamage ?? int.MaxValue;
                existing.SetBonusDamageForNextAttack(Math.Min(cap, current + increment));
                existing.SetRemainingDuration(effect.RemainingDuration);
                LogApplied(effect, replayEntries, roundNumber);
                break;

            case StackingRule.Stack:
            default:
                state.AddEffect(effect);
                LogApplied(effect, replayEntries, roundNumber);
                break;
        }
    }

    private static void LogApplied(ActiveEffect effect, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        replayEntries?.Add(ReplayEntry.ForEffectApplied(roundNumber, effect.SourceUnitId, effect.Name, effect.TargetUnitId, effect.TargetField));
    }

    /// <summary>
    /// Sucht eine bereits aktive, "gleichartige" Effekt-Instanz. Für die
    /// meisten Regeln bedeutet das: gleicher Name, gleicher Anker UND
    /// gleiches Ziel. <see cref="StackingRule.ReplaceForSameSource"/> ist der
    /// bewusste Sonderfall (siehe dortige Dokumentation): dort zählt allein
    /// Name + Quelle, unabhängig vom (ggf. neuen) Ziel.
    /// </summary>
    private static ActiveEffect? FindMatching(GameState state, ActiveEffect effect)
    {
        if (effect.Stacking == StackingRule.ReplaceForSameSource)
        {
            return state.ActiveEffects.FirstOrDefault(e =>
                e.Name == effect.Name &&
                e.SourceUnitId == effect.SourceUnitId);
        }

        return state.ActiveEffects.FirstOrDefault(e =>
            e.Name == effect.Name &&
            e.AnchorType == effect.AnchorType &&
            e.TargetUnitId == effect.TargetUnitId &&
            e.TargetField == effect.TargetField);
    }

    /// <summary>Siehe Klassendokumentation für den genauen Zeitpunkt.</summary>
    public static void TickUnitEffects(GameState state, Unit unit, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        // Kopie der Liste: während des Tickens (Schaden -> ggf. Niederlage
        // -> Aufräumung) können Einträge aus der Originalliste entfernt
        // werden, was eine laufende Enumeration ungültig machen würde.
        var effects = state.GetEffectsOnUnit(unit.Id).ToList();

        foreach (var effect in effects)
        {
            if (unit.IsDefeated)
            {
                break; // z.B. durch den Tick-Schaden eines vorherigen Effekts bereits besiegt
            }

            replayEntries?.Add(ReplayEntry.ForEffectTicked(roundNumber, effect.Name, effect.TargetUnitId, effect.TargetField));

            if (effect.DamagePerTick is int tickDamage)
            {
                CombatSystem.ApplyDamageAndHandleDefeat(state, unit, tickDamage);
            }

            ExpireIfDone(state, effect, replayEntries, roundNumber);
        }
    }

    /// <summary>Siehe Klassendokumentation für den genauen Zeitpunkt.</summary>
    public static void TickFieldEffects(GameState state, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        foreach (var effect in state.ActiveEffects.Where(e => e.AnchorType == EffectAnchorType.Field).ToList())
        {
            replayEntries?.Add(ReplayEntry.ForEffectTicked(roundNumber, effect.Name, effect.TargetUnitId, effect.TargetField));
            ExpireIfDone(state, effect, replayEntries, roundNumber);
        }
    }

    private static void ExpireIfDone(GameState state, ActiveEffect effect, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        if (effect.RemainingDuration is not int remaining)
        {
            return; // unbegrenzte Dauer - endet nur durch Tod von Quelle/Ziel oder aktives Beenden
        }

        int updated = remaining - 1;
        if (updated <= 0)
        {
            RemoveEffectAndLinkedCounterpart(state, effect, replayEntries, roundNumber);
            replayEntries?.Add(ReplayEntry.ForEffectExpired(roundNumber, effect.Name, effect.TargetUnitId, effect.TargetField));
        }
        else
        {
            effect.SetRemainingDuration(updated);
        }
    }

    /// <summary>
    /// Entfernt alle Effekte, deren Quelle ODER Ziel die soeben entfernte
    /// Einheit ist (Anforderung 10: "Effekte enden, wenn Quelle oder Ziel
    /// besiegt wird"). Wird zentral von <see cref="CombatSystem"/> nach
    /// JEDER erfolgreichen Niederlage aufgerufen - unabhängig davon, ob der
    /// Schaden aus einem Grundangriff, einem Fähigkeitseffekt oder einem
    /// DoT-Tick stammt (alle laufen durch dieselbe Stelle, siehe
    /// CombatSystem.ApplyDamageAndHandleDefeat).
    /// </summary>
    public static void RemoveEffectsRelatedTo(GameState state, Guid unitId)
    {
        foreach (var effect in state.ActiveEffects
            .Where(e => e.SourceUnitId == unitId || e.TargetUnitId == unitId)
            .ToList())
        {
            RemoveEffectAndLinkedCounterpart(state, effect, replayEntries: null, roundNumber: 0);
        }
    }

    /// <summary>
    /// Entfernt <paramref name="effect"/> und - falls vorhanden - seinen
    /// GEKOPPELTEN GEGENPART: eine andere aktive Instanz mit demselben Namen
    /// und derselben Quelle. Manche Fähigkeiten legen bewusst ZWEI separate
    /// Effekt-Instanzen mit identischem Namen an, deren Lebensdauer laut
    /// Aufgabenstellung strikt aneinander gekoppelt sein muss:
    ///   - Wand: eine feld-verankerte Feldsperre/Umleitung + eine
    ///     unit-verankerte Bewegungsunfähigkeit ("die Sperre endet, wenn die
    ///     Bewegungsunfähigkeit endet").
    ///   - Hugger: die Anheftung auf dem Ziel + die eigene Schadensreduktion
    ///     auf dem Hugger selbst ("solange die Anheftung aktiv ist").
    /// Da unit- und feld-verankerte Effekte an UNTERSCHIEDLICHEN Zeitpunkten
    /// ticken (siehe Klassendokumentation) und Zieltod nur EINE der beiden
    /// Instanzen direkt trifft, würde ein bloß gleicher Anfangs-Dauer-Wert
    /// sonst leicht auseinanderlaufen bzw. die andere Instanz gar nicht erst
    /// mit aufräumen. Diese EINE zentrale Stelle (aufgerufen sowohl bei
    /// natürlichem Ablauf als auch beim Tod-Aufräumen) sorgt generisch dafür,
    /// dass beide Instanzen immer gemeinsam enden - unabhängig davon, WARUM
    /// die erste entfernt wurde, und ohne Held-spezifische Sonderprüfung an
    /// mehreren Stellen im Code.
    /// </summary>
    public static void RemoveEffectAndLinkedCounterpart(GameState state, ActiveEffect effect, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        state.RemoveEffect(effect);

        var counterpart = state.ActiveEffects.FirstOrDefault(e =>
            e.Name == effect.Name && e.SourceUnitId == effect.SourceUnitId);

        if (counterpart is not null)
        {
            state.RemoveEffect(counterpart);
            replayEntries?.Add(ReplayEntry.ForEffectExpired(roundNumber, counterpart.Name, counterpart.TargetUnitId, counterpart.TargetField));
        }
    }
}
