using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Effects;

/// <summary>
/// Eine aktive Effekt-Instanz im Spielzustand. Effekte sind bewusst
/// DATENGETRIEBEN und additiv modelliert: statt für jede Wirkungsart eine
/// eigene Effekt-Unterklasse mit eigenem Verhalten zu schreiben, trägt
/// jeder Effekt eine Kombination unabhängiger, optionaler "Fähigkeiten".
/// Die zentralen Systeme (MovementSystem, CombatSystem, EffectiveStats)
/// fragen jeweils GENERISCH über alle aktiven Effekte ab, ob/wie stark eine
/// bestimmte Fähigkeit vorhanden ist - es gibt keine Sonderfall-Verzweigung
/// pro konkretem Effekt oder Helden (siehe Anforderung 8 und
/// <see cref="EffectTemplate"/> für die Zuordnung der Wirkungsarten).
///
/// "ANHEFTUNG AN EINE EINHEIT" braucht dabei KEIN eigenes Feld: ein
/// unit-verankerter Effekt (<see cref="EffectAnchorType.Unit"/>) "klebt"
/// bereits qua Definition an der Zieleinheit - es gibt keinen separaten
/// Ortsbezug, er bewegt sich also automatisch mit ihr mit und endet
/// automatisch, wenn sie besiegt wird (siehe
/// <see cref="TacticsCore.Combat.CombatSystem"/>, das nach jeder Niederlage
/// <c>EffectSystem.RemoveEffectsRelatedTo</c> aufruft). "Aktives Beenden"
/// ist als darüber hinausgehende Fähigkeit vorbereitet: eine künftige
/// Fähigkeit kann jederzeit direkt <c>GameState.RemoveEffect</c> aufrufen.
///
/// Bewusst OHNE eigene, zufällig erzeugte Kennung (kein
/// <c>Guid.NewGuid()</c>): das würde eine nicht über den Seed gesteuerte
/// Zufallsquelle in die Simulation einschleusen, sobald Effekte (z.B. im
/// Replay-Protokoll) anhand ihrer Identität referenziert würden.
/// Gleichartigkeit für die Stapelregel wird stattdessen rein inhaltlich
/// festgestellt (Name + Anker + Ziel, siehe <see cref="EffectSystem.Apply"/>).
/// </summary>
public sealed class ActiveEffect
{
    public string Name { get; }
    public Guid SourceUnitId { get; }
    public EffectAnchorType AnchorType { get; }
    public Guid? TargetUnitId { get; }
    public HexCoordinate? TargetField { get; }

    /// <summary>Restdauer in "eigenen Zügen"/Runden der betroffenen Einheit bzw. Feld-Runden; null = unbegrenzt (endet nur über Tod von Quelle/Ziel oder aktives Beenden).</summary>
    public int? RemainingDuration { get; private set; }

    public StackingRule Stacking { get; }

    public StatModifierSet? StatModifiers { get; }
    public int? DamagePerTick { get; }
    public double? IncomingDamageMultiplier { get; }
    public bool PreventsMovement { get; }
    public Team? FieldLockAgainstTeam { get; }
    public bool RedirectsAttacksToSource { get; }
    public int? BonusDamageForNextAttack { get; private set; }

    /// <summary>
    /// Obergrenze für <see cref="BonusDamageForNextAttack"/>, wenn dieser
    /// Effekt mit <see cref="StackingRule.Accumulate"/> angewendet wird
    /// (siehe Bogenschütze) - sonst null/ungenutzt.
    /// </summary>
    public int? MaximumAccumulatedBonusDamage { get; }

    /// <summary>
    /// Wenn gesetzt: zieht <see cref="TargetUnitId"/> um, wird
    /// <see cref="SourceUnitId"/> automatisch auf dasselbe Feld mitgezogen
    /// (siehe <see cref="TacticsCore.GameState.MoveUnit"/> und Hugger).
    /// Ergibt nur bei einem unit-verankerten Effekt Sinn, dessen Ziel NICHT
    /// die Quelle selbst ist.
    /// </summary>
    public bool CausesSourceToFollowTarget { get; }

    private ActiveEffect(
        string name, Guid sourceUnitId, EffectAnchorType anchorType, Guid? targetUnitId, HexCoordinate? targetField,
        int? duration, StackingRule stacking, StatModifierSet? statModifiers, int? damagePerTick,
        double? incomingDamageMultiplier, bool preventsMovement, Team? fieldLockAgainstTeam,
        bool redirectsAttacksToSource, int? bonusDamageForNextAttack, int? maximumAccumulatedBonusDamage,
        bool causesSourceToFollowTarget)
    {
        Name = name;
        SourceUnitId = sourceUnitId;
        AnchorType = anchorType;
        TargetUnitId = targetUnitId;
        TargetField = targetField;
        RemainingDuration = duration;
        Stacking = stacking;
        StatModifiers = statModifiers;
        DamagePerTick = damagePerTick;
        IncomingDamageMultiplier = incomingDamageMultiplier;
        PreventsMovement = preventsMovement;
        FieldLockAgainstTeam = fieldLockAgainstTeam;
        RedirectsAttacksToSource = redirectsAttacksToSource;
        BonusDamageForNextAttack = bonusDamageForNextAttack;
        MaximumAccumulatedBonusDamage = maximumAccumulatedBonusDamage;
        CausesSourceToFollowTarget = causesSourceToFollowTarget;
    }

    /// <summary>Erzeugt einen an eine Einheit gebundenen Effekt (Buff/Debuff, DoT, Bewegungssperre, Anheftung, Angriffsbonus, ...).</summary>
    public static ActiveEffect OnUnit(Unit source, Unit target, EffectTemplate template) => new(
        template.Name, source.Id, EffectAnchorType.Unit, target.Id, null,
        template.Duration, template.Stacking, template.StatModifiers, template.DamagePerTick,
        template.IncomingDamageMultiplier, template.PreventsMovement, fieldLockAgainstTeam: null,
        redirectsAttacksToSource: false, template.BonusDamageForNextAttack, template.MaximumAccumulatedBonusDamage,
        causesSourceToFollowTarget: template.CausesSourceToFollowTarget && target.Id != source.Id);

    /// <summary>Erzeugt einen an ein Feld gebundenen Effekt (Flächen-Schadensreduktion, Feldsperre, Angriffs-Umleitung).</summary>
    public static ActiveEffect OnField(Unit source, HexCoordinate field, EffectTemplate template) => new(
        template.Name, source.Id, EffectAnchorType.Field, null, field,
        template.Duration, template.Stacking, template.StatModifiers, template.DamagePerTick,
        template.IncomingDamageMultiplier, preventsMovement: false,
        fieldLockAgainstTeam: template.LocksFieldForEnemies ? OppositeTeamOf(source.Team) : null,
        redirectsAttacksToSource: template.RedirectsAttacksToSource,
        bonusDamageForNextAttack: template.BonusDamageForNextAttack,
        maximumAccumulatedBonusDamage: template.MaximumAccumulatedBonusDamage,
        causesSourceToFollowTarget: false);

    private static Team OppositeTeamOf(Team team) => team == Team.TeamA ? Team.TeamB : Team.TeamA;

    /// <summary>Ausschließlich für <see cref="EffectSystem"/> gedacht (Ticken/Ablaufen/Auffrischen der Dauer).</summary>
    internal void SetRemainingDuration(int? value) => RemainingDuration = value;

    /// <summary>
    /// Ausschließlich für <see cref="EffectSystem"/> gedacht: setzt den
    /// aktuell angesammelten Bonus eines aufladenden Effekts
    /// (<see cref="StackingRule.Accumulate"/>, siehe Bogenschütze).
    /// </summary>
    internal void SetBonusDamageForNextAttack(int? value) => BonusDamageForNextAttack = value;
}
