using TacticsCore.Units;

namespace TacticsCore.Effects;

/// <summary>
/// Berechnet die EFFEKTIVEN Werte einer Einheit: Basiswert (aus Unit bzw.
/// der Helden-Definition) plus die Summe aller aktiven, sie betreffenden
/// Stat-Modifikatoren (Buffs/Debuffs) und - bei passiven Helden-
/// Fähigkeiten - deren live berechneter Beitrag (siehe
/// <see cref="IHasPassiveStatContribution"/>).
///
/// Das ist die EINZIGE Stelle, an der Bewegungs-, Kampf- UND
/// Zugreihenfolge-Systeme nach den tatsächlich geltenden Werten einer
/// Einheit fragen sollen - sobald Effekte im Spiel sind, darf nirgendwo
/// sonst direkt auf <c>unit.MovementRange</c>, <c>unit.BaseAttackDamage</c>,
/// <c>unit.Speed</c> o.ä. zugegriffen werden (siehe auch
/// <see cref="TacticsCore.Match.TurnOrder"/>, das seit der Bachelorarbeit-
/// Korrektur ebenfalls <see cref="Speed"/> statt des rohen Feldes für die
/// Zugreihenfolge nutzt).
/// </summary>
public static class EffectiveStats
{
    public static int MovementRange(Unit unit, GameState state) =>
        Math.Max(0, unit.MovementRange + SumModifiers(unit, state).MovementRangeDelta);

    public static int AttackDamage(Unit unit, GameState state) =>
        Math.Max(0, unit.BaseAttackDamage + SumModifiers(unit, state).BaseAttackDamageDelta);

    public static int AttackRange(Unit unit, GameState state) =>
        Math.Max(0, unit.AttackRange + SumModifiers(unit, state).AttackRangeDelta);

    /// <summary>Siehe Klassendokumentation - wird seit der Bachelorarbeit-Korrektur auch von <see cref="TacticsCore.Match.TurnOrder"/> für die Zugreihenfolge konsultiert.</summary>
    public static int Speed(Unit unit, GameState state) =>
        Math.Max(0, unit.Speed + SumModifiers(unit, state).SpeedDelta);

    /// <summary>Ob die Einheit aktuell durch einen Effekt bewegungsunfähig ist (siehe <see cref="ActiveEffect.PreventsMovement"/>).</summary>
    public static bool IsMovementPrevented(Unit unit, GameState state) =>
        state.GetEffectsOnUnit(unit.Id).Any(e => e.PreventsMovement);

    /// <summary>Ob die Einheit aktuell durch einen Effekt am Angreifen gehindert ist (siehe <see cref="ActiveEffect.PreventsAttacking"/>, z.B. Hugger während der Anheftung).</summary>
    public static bool IsAttackingPrevented(Unit unit, GameState state) =>
        state.GetEffectsOnUnit(unit.Id).Any(e => e.PreventsAttacking);

    /// <summary>
    /// Ob <paramref name="target"/> für einen Anwender aus
    /// <paramref name="chooserTeam"/> aktuell als Ziel (Angriff ODER
    /// Fähigkeit) gesperrt ist (siehe <see cref="ActiveEffect.PreventsBeingTargetedByEnemies"/>,
    /// z.B. Hugger während der Anheftung). Bewusst TEAM-RELATIV: die Sperre
    /// gilt ausdrücklich nur gegenüber GEGNERN - ist <paramref name="target"/>
    /// im selben Team wie der Anwender, liefert diese Methode immer false,
    /// unabhängig von einem aktiven Effekt (Verbündete, z.B. eine heilende
    /// Elfe, sollen die Einheit weiterhin wählen können).
    /// </summary>
    public static bool PreventsBeingTargetedBy(Unit target, Team chooserTeam, GameState state) =>
        target.Team != chooserTeam && state.GetEffectsOnUnit(target.Id).Any(e => e.PreventsBeingTargetedByEnemies);

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
