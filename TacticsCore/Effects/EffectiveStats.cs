using TacticsCore.Units;

namespace TacticsCore.Effects;

/// <summary>
/// Berechnet die EFFEKTIVEN Werte einer Einheit: Basiswert (aus Unit bzw.
/// der Helden-Definition) plus die Summe aller aktiven, sie betreffenden
/// Stat-Modifikatoren (Buffs/Debuffs) und - bei passiven Helden-
/// Fähigkeiten - deren live berechneter Beitrag (siehe
/// <see cref="IHasPassiveStatContribution"/>).
///
/// Das ist die EINZIGE Stelle, an der Bewegungs- und Kampfsysteme nach den
/// tatsächlich geltenden Werten einer Einheit fragen sollen - sobald
/// Effekte im Spiel sind, darf nirgendwo sonst direkt auf
/// <c>unit.MovementRange</c>, <c>unit.BaseAttackDamage</c> o.ä.
/// zugegriffen werden.
///
/// EINSCHRÄNKUNG (bewusst dokumentiert statt stillschweigend): Die
/// Zugreihenfolge (<see cref="TacticsCore.Match.TurnOrder"/>) liest aktuell
/// noch direkt <c>unit.Speed</c> statt <see cref="Speed"/> hier - ein
/// Speed-Buff/-Debuff würde sich also (noch) nicht auf die Rangfolge
/// auswirken. Keiner der drei Beispielhelden nutzt einen Speed-Modifikator
/// und keine Anforderung verlangt das aktuell testbar; die Erweiterung
/// (TurnOrder müsste dafür zusätzlich den GameState kennen) ist bewusst für
/// einen späteren Schritt zurückgestellt, um hier keinen breiten,
/// ungetesteten Umbau vorzunehmen.
/// </summary>
public static class EffectiveStats
{
    public static int MovementRange(Unit unit, GameState state) =>
        Math.Max(0, unit.MovementRange + SumModifiers(unit, state).MovementRangeDelta);

    public static int AttackDamage(Unit unit, GameState state) =>
        Math.Max(0, unit.BaseAttackDamage + SumModifiers(unit, state).BaseAttackDamageDelta);

    public static int AttackRange(Unit unit, GameState state) =>
        Math.Max(0, unit.AttackRange + SumModifiers(unit, state).AttackRangeDelta);

    /// <summary>Siehe Klassendokumentation - aktuell von TurnOrder noch nicht konsultiert.</summary>
    public static int Speed(Unit unit, GameState state) =>
        Math.Max(0, unit.Speed + SumModifiers(unit, state).SpeedDelta);

    /// <summary>Ob die Einheit aktuell durch einen Effekt bewegungsunfähig ist (siehe <see cref="ActiveEffect.PreventsMovement"/>).</summary>
    public static bool IsMovementPrevented(Unit unit, GameState state) =>
        state.GetEffectsOnUnit(unit.Id).Any(e => e.PreventsMovement);

    /// <summary>
    /// Ein evtl. vorhandener, einmalig zu verbrauchender Bonusschaden für
    /// den NÄCHSTEN Angriff dieser Einheit ("aufladender Eigenzustand",
    /// Anforderung 8). Wird von CombatSystem abgefragt und bei Verwendung
    /// entfernt (Verbrauch).
    /// </summary>
    public static ActiveEffect? FindNextAttackBonus(Unit unit, GameState state) =>
        state.GetEffectsOnUnit(unit.Id).FirstOrDefault(e => e.BonusDamageForNextAttack.HasValue);

    /// <summary>
    /// Multiplikator auf eingehenden Schaden für <paramref name="target"/>
    /// (z.B. 0.5 für die Schildträger-Schadensreduktion). Berücksichtigt
    /// sowohl feld- als auch unit-verankerte Reduktionen; mehrere aktive
    /// Reduktionen multiplizieren sich (bewusst einfache, deterministische
    /// Kombinationsregel).
    /// </summary>
    public static double IncomingDamageMultiplier(Unit target, GameState state)
    {
        double multiplier = 1.0;

        foreach (var effect in state.GetEffectsOnField(target.Position))
        {
            if (effect.IncomingDamageMultiplier is double fieldMultiplier)
            {
                multiplier *= fieldMultiplier;
            }
        }

        foreach (var effect in state.GetEffectsOnUnit(target.Id))
        {
            if (effect.IncomingDamageMultiplier is double unitMultiplier)
            {
                multiplier *= unitMultiplier;
            }
        }

        return multiplier;
    }

    private static StatModifierSet SumModifiers(Unit unit, GameState state)
    {
        var total = StatModifierSet.None;

        foreach (var effect in state.GetEffectsOnUnit(unit.Id))
        {
            if (effect.StatModifiers is { } modifiers)
            {
                total += modifiers;
            }
        }

        if (unit is IHasPassiveStatContribution passiveSource)
        {
            total += passiveSource.GetPassiveStatContribution(state);
        }

        return total;
    }
}
