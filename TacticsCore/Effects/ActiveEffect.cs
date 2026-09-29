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
    public HexCoordinate? TargetField { get; private set; }

    /// <summary>
    /// Restdauer in EIGENEN ZÜGEN DES ANWENDERS (<see cref="SourceUnitId"/>):
    /// sie sinkt zu Beginn jedes Zugs des Anwenders um 1 (siehe
    /// <see cref="EffectSystem.TickDurationsOfSource"/>), unabhängig davon, wie
    /// viele Einheiten der Effekt betrifft und ob er an eine Einheit oder ein
    /// Feld gebunden ist. Dauer 1 = wirkt bis zum Beginn des nächsten eigenen
    /// Zugs des Anwenders. null = unbegrenzt (endet nur über Tod von
    /// Quelle/Ziel oder aktives Beenden).
    /// </summary>
    public int? RemainingDuration { get; private set; }

    public StackingRule Stacking { get; }

    public StatModifierSet? StatModifiers { get; }
    public int? DamagePerTick { get; private set; }

    /// <summary>Siehe <see cref="EffectTemplate.DamagePerTickReductionPerTick"/>.</summary>
    public int? DamagePerTickReductionPerTick { get; }
    public double? IncomingDamageMultiplier { get; }
    public bool PreventsMovement { get; }
    public Team? FieldLockAgainstTeam { get; }
    public bool RedirectsAttacksToSource { get; }

    /// <summary>Siehe <see cref="EffectTemplate.ReflectsFractionOfRedirectedDamage"/>.</summary>
    public double? ReflectsFractionOfRedirectedDamage { get; }
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
    /// die Quelle selbst ist. Zusätzlich zieht bereits <see cref="Effects.EffectSystem.Apply"/>
    /// die Quelle SOFORT beim Anlegen dieses Effekts auf das aktuelle Feld
    /// des Ziels (nicht erst bei dessen nächster Bewegung).
    /// </summary>
    public bool CausesSourceToFollowTarget { get; }

    /// <summary>Siehe <see cref="EffectTemplate.PreventsAttacking"/>.</summary>
    public bool PreventsAttacking { get; }

    /// <summary>Siehe <see cref="EffectTemplate.PreventsBeingTargetedByEnemies"/>.</summary>
    public bool PreventsBeingTargetedByEnemies { get; }

    /// <summary>Siehe <see cref="EffectTemplate.FieldFollowsSource"/>: nur bei feld-verankerten Effekten; das Feld wandert mit dem Anwender.</summary>
    public bool FieldFollowsSource { get; }

    /// <summary>Siehe <see cref="EffectTemplate.TargetingProtectionSuspendedByAttackRounds"/>.</summary>
    public int? TargetingProtectionSuspendedByAttackRounds { get; }

    /// <summary>Siehe <see cref="EffectTemplate.SuspendsTargetingProtection"/>.</summary>
    public bool SuspendsTargetingProtection { get; }

    private ActiveEffect(
        string name, Guid sourceUnitId, EffectAnchorType anchorType, Guid? targetUnitId, HexCoordinate? targetField,
        int? duration, StackingRule stacking, StatModifierSet? statModifiers, int? damagePerTick,
        int? damagePerTickReductionPerTick,
        double? incomingDamageMultiplier, bool preventsMovement, Team? fieldLockAgainstTeam,
        bool redirectsAttacksToSource, double? reflectsFractionOfRedirectedDamage,
        int? bonusDamageForNextAttack, int? maximumAccumulatedBonusDamage,
        bool causesSourceToFollowTarget, bool preventsAttacking, bool preventsBeingTargetedByEnemies,
        bool fieldFollowsSource = false, int? targetingProtectionSuspendedByAttackRounds = null, bool suspendsTargetingProtection = false)
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
        DamagePerTickReductionPerTick = damagePerTickReductionPerTick;
        IncomingDamageMultiplier = incomingDamageMultiplier;
        PreventsMovement = preventsMovement;
        FieldLockAgainstTeam = fieldLockAgainstTeam;
        RedirectsAttacksToSource = redirectsAttacksToSource;
        ReflectsFractionOfRedirectedDamage = reflectsFractionOfRedirectedDamage;
        BonusDamageForNextAttack = bonusDamageForNextAttack;
        MaximumAccumulatedBonusDamage = maximumAccumulatedBonusDamage;
        CausesSourceToFollowTarget = causesSourceToFollowTarget;
        PreventsAttacking = preventsAttacking;
        PreventsBeingTargetedByEnemies = preventsBeingTargetedByEnemies;
        FieldFollowsSource = fieldFollowsSource;
        TargetingProtectionSuspendedByAttackRounds = targetingProtectionSuspendedByAttackRounds;
        SuspendsTargetingProtection = suspendsTargetingProtection;
    }

    /// <summary>Erzeugt einen an eine Einheit gebundenen Effekt (Buff/Debuff, DoT, Bewegungssperre, Anheftung, Angriffsbonus, ...).</summary>
    public static ActiveEffect OnUnit(Unit source, Unit target, EffectTemplate template) => new(
        template.Name, source.Id, EffectAnchorType.Unit, target.Id, null,
        template.Duration, template.Stacking, template.StatModifiers, template.DamagePerTick,
        template.DamagePerTickReductionPerTick,
        template.IncomingDamageMultiplier, template.PreventsMovement, fieldLockAgainstTeam: null,
        redirectsAttacksToSource: false, reflectsFractionOfRedirectedDamage: null,
        template.BonusDamageForNextAttack, template.MaximumAccumulatedBonusDamage,
        causesSourceToFollowTarget: template.CausesSourceToFollowTarget && target.Id != source.Id,
        preventsAttacking: template.PreventsAttacking,
        preventsBeingTargetedByEnemies: template.PreventsBeingTargetedByEnemies,
        targetingProtectionSuspendedByAttackRounds: template.TargetingProtectionSuspendedByAttackRounds,
        suspendsTargetingProtection: template.SuspendsTargetingProtection);

    /// <summary>Erzeugt einen an ein Feld gebundenen Effekt (Flächen-Schadensreduktion, Feldsperre, Angriffs-Umleitung).</summary>
    public static ActiveEffect OnField(Unit source, HexCoordinate field, EffectTemplate template) => new(
        template.Name, source.Id, EffectAnchorType.Field, null, field,
        template.Duration, template.Stacking, template.StatModifiers, template.DamagePerTick,
        template.DamagePerTickReductionPerTick,
        template.IncomingDamageMultiplier, preventsMovement: false,
        fieldLockAgainstTeam: template.LocksFieldForEnemies ? OppositeTeamOf(source.Team) : null,
        redirectsAttacksToSource: template.RedirectsAttacksToSource,
        reflectsFractionOfRedirectedDamage: template.ReflectsFractionOfRedirectedDamage,
        bonusDamageForNextAttack: template.BonusDamageForNextAttack,
        maximumAccumulatedBonusDamage: template.MaximumAccumulatedBonusDamage,
        causesSourceToFollowTarget: false,
        preventsAttacking: false,
        preventsBeingTargetedByEnemies: false,
        fieldFollowsSource: template.FieldFollowsSource);

    private static Team OppositeTeamOf(Team team) => team == Team.TeamA ? Team.TeamB : Team.TeamA;

    /// <summary>Unabhängige Kopie mit identischem Zustand - ausschließlich für <see cref="TacticsCore.GameState.CloneForSimulation"/>.</summary>
    internal ActiveEffect CopyForSimulation() => new(
        Name, SourceUnitId, AnchorType, TargetUnitId, TargetField,
        RemainingDuration, Stacking, StatModifiers, DamagePerTick,
        DamagePerTickReductionPerTick,
        IncomingDamageMultiplier, PreventsMovement, FieldLockAgainstTeam,
        RedirectsAttacksToSource, ReflectsFractionOfRedirectedDamage,
        BonusDamageForNextAttack, MaximumAccumulatedBonusDamage,
        CausesSourceToFollowTarget, PreventsAttacking, PreventsBeingTargetedByEnemies, FieldFollowsSource,
        TargetingProtectionSuspendedByAttackRounds, SuspendsTargetingProtection);

    /// <summary>Ausschließlich für <see cref="TacticsCore.GameState.MoveUnit"/> gedacht (siehe <see cref="FieldFollowsSource"/>).</summary>
    internal void SetTargetField(HexCoordinate field) => TargetField = field;

    /// <summary>Ausschließlich für <see cref="EffectSystem"/> gedacht (Ticken/Ablaufen/Auffrischen der Dauer).</summary>
    internal void SetRemainingDuration(int? value) => RemainingDuration = value;

    /// <summary>
    /// Ausschließlich für <see cref="EffectSystem"/> gedacht: setzt den
    /// aktuellen Tick-Schaden, wenn dieser gemäß
    /// <see cref="DamagePerTickReductionPerTick"/> nach jedem Tick abklingt
    /// (siehe Elementarmagier-Feuer) bzw. bei einem Auffrischen der Dauer
    /// wieder auf den vollen Ausgangswert zurückgesetzt wird.
    /// </summary>
    internal void SetDamagePerTick(int? value) => DamagePerTick = value;

    /// <summary>
    /// Ausschließlich für <see cref="EffectSystem"/> gedacht: setzt den
    /// aktuell angesammelten Bonus eines aufladenden Effekts
    /// (<see cref="StackingRule.Accumulate"/>, siehe Bogenschütze).
    /// </summary>
    internal void SetBonusDamageForNextAttack(int? value) => BonusDamageForNextAttack = value;
}
