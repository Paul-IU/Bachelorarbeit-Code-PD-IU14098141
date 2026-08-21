using TacticsCore.Abilities;
using TacticsCore.Effects;

namespace TacticsCore.Heroes;

/// <summary>
/// Alle neun Standardhelden (je drei Tank, Support, Dmg) als Datensätze
/// über <see cref="HeroDefinition"/> - reine Daten, kein Held bringt
/// eigenen Code mit. Jede Fähigkeit ist ausschließlich über bestehende
/// <see cref="EffectTemplate"/>-Felder bzw. den <see cref="AbilityMode"/>-
/// Mechanismus beschrieben; wo eine Mechanik im System noch fehlte, wurde
/// sie GENERISCH ergänzt (siehe Kommentare bei den jeweils betroffenen
/// Feldern in <see cref="EffectTemplate"/>, <see cref="StackingRule"/>,
/// <see cref="AbilityMode"/> und <see cref="AreaTargetFilter"/>) - nirgends
/// eine Held-spezifische Sonderfallprüfung im Interpreter selbst.
///
/// ATTRIBUTWERTE SIND VORLÄUFIG: HP, Schaden, Geschwindigkeit sowie
/// Bewegungs-/Angriffsreichweite aller neun Helden sind erste, plausible
/// Platzhalter entlang der vorgegebenen Leitlinie (Tank: überdurchschnitt-
/// liche HP/unterdurchschnittlicher Schaden; Dmg: umgekehrt; Support:
/// mittlere Werte; Fernkämpfer haben geringere HP als Nahkämpfer derselben
/// Rolle) und werden ERST SPÄTER über Simulationen (siehe die
/// Plausibilitätsprüfungen im Konsolenprogramm) ausbalanciert. Alle
/// Geschwindigkeitswerte sind über ALLE NEUN Helden hinweg paarweise
/// verschieden (5, 6, 9, 10, 11, 12, 13, 14, 15) - unabhängig davon, welche
/// drei Helden je Team antreten, kann es daher nie einen Speed-Gleichstand
/// innerhalb eines Teams geben.
/// </summary>
public static class DefaultHeroes
{
    /// <summary>
    /// Tank: aktiv, gibt allen Einheiten auf seinem EIGENEN Feld
    /// (inklusive sich selbst) für eine Runde 50% Schadensreduktion. Das
    /// "eigene Feld" ergibt sich daraus, dass die Fähigkeit
    /// <see cref="AbilityTargetType.AllUnitsOnField"/> mit Reichweite 0
    /// nutzt - das einzige Feld in Reichweite 0 ist das eigene, es gibt
    /// also gar keine echte Zielwahl (ein "Ein-Klick"-Schild).
    /// </summary>
    public static readonly HeroDefinition Schildtraeger = new(
        Id: "schildtraeger",
        DisplayName: "Schildträger",
        Role: HeroRole.Tank,
        MaxHitPoints: 80,
        BaseAttackDamage: 8,
        Speed: 6,
        MovementRange: 1,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "schildwall",
            DisplayName: "Schildwall",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsOnField,
            Range: 0,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Schildwall",
                    IsInstant: false,
                    Duration: 1,
                    Stacking: StackingRule.RefreshDuration,
                    IncomingDamageMultiplier: 0.5),
            }));

    /// <summary>
    /// Support: aktiv, heilt einen Verbündeten - Wirkung sinkt mit
    /// Entfernung (20 HP auf Distanz 0, 15 auf Distanz 1, 10 ab Distanz 2 -
    /// siehe <see cref="EffectTemplate.MinimumAfterDistanceScaling"/>).
    /// </summary>
    public static readonly HeroDefinition Elfe = new(
        Id: "elfe",
        DisplayName: "Elfe",
        Role: HeroRole.Support,
        MaxHitPoints: 35,
        BaseAttackDamage: 6,
        Speed: 12,
        MovementRange: 2,
        AttackRange: 2,
        Ability: new AbilityDefinition(
            Id: "heilung",
            DisplayName: "Heilung",
            IsPassive: false,
            TargetType: AbilityTargetType.Ally,
            Range: 2,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Heilung",
                    IsInstant: true,
                    InstantHeal: 20,
                    ScaleWithDistance: true,
                    DistanceFalloffPerStep: 5,
                    MinimumAfterDistanceScaling: 10),
            }));

    /// <summary>
    /// Dmg: passiv, Schaden steigt bei sinkenden eigenen HP - linear von
    /// +0 bei voller HP bis +20 bei (theoretisch) 0 HP. Wird nicht als
    /// gespeicherter Effekt geführt, sondern bei jeder Abfrage aus den
    /// AKTUELLEN HP live neu berechnet (siehe <see cref="Hero.GetPassiveStatContribution"/>),
    /// da sich die HP jederzeit ändern.
    /// </summary>
    public static readonly HeroDefinition Berserker = new(
        Id: "berserker",
        DisplayName: "Berserker",
        Role: HeroRole.Dmg,
        MaxHitPoints: 45,
        BaseAttackDamage: 12,
        Speed: 11,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "blutrausch",
            DisplayName: "Blutrausch",
            IsPassive: true,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            Effects: Array.Empty<EffectTemplate>(),
            PassiveStatModifier: (unit, _) =>
            {
                double missingFraction = 1.0 - (double)unit.CurrentHitPoints / unit.MaxHitPoints;
                int bonus = (int)Math.Round(missingFraction * 20);
                return new StatModifierSet(BaseAttackDamageDelta: bonus);
            }));

    /// <summary>
    /// Anzahl Runden, die die Wand nach Einsatz ihrer Fähigkeit
    /// bewegungsunfähig ist (Standard laut Aufgabenstellung: 1). Bewusst als
    /// benannte Konstante statt zweimal wiederholter Literalwert, da
    /// sowohl die Feldsperre/Umleitung als auch die Bewegungsunfähigkeit
    /// exakt dieselbe Dauer tragen müssen (siehe unten und
    /// <see cref="Effects.EffectSystem.RemoveEffectAndLinkedCounterpart"/>,
    /// das die beiden über den gemeinsamen Namen "Mauer" gekoppelt hält).
    /// </summary>
    private const int WandBewegungsunfaehigkeitsdauer = 1;

    /// <summary>
    /// Tank: aktiv, sperrt das eigene Feld für Gegner
    /// (<see cref="EffectTemplate.LocksFieldForEnemies"/>) und leitet
    /// Angriffe, deren Sichtlinie durch dieses Feld verläuft - AUCH auf ein
    /// Ziel, das sich das Feld mit der Wand teilt - auf die Wand selbst um
    /// (<see cref="EffectTemplate.RedirectsAttacksToSource"/>, siehe die
    /// entsprechend erweiterte <see cref="Combat.CombatSystem"/>-Umleitung).
    /// Danach ist die Wand für <see cref="WandBewegungsunfaehigkeitsdauer"/>
    /// Runden bewegungsunfähig; die Feldsperre/Umleitung endet exakt dann,
    /// wenn diese Bewegungsunfähigkeit endet (gleicher Effektname "Mauer" auf
    /// zwei getrennten Effekt-Instanzen - eine feld-, eine unit-verankert -,
    /// deren Lebensdauer über <see cref="Effects.EffectSystem.RemoveEffectAndLinkedCounterpart"/>
    /// gekoppelt ist, siehe dort für die Begründung, warum ein bloß gleicher
    /// Anfangswert allein nicht gereicht hätte) oder wenn die Wand besiegt
    /// wird (bereits durch die bestehende Tod-Aufräumung abgedeckt, da beide
    /// Instanzen dieselbe Quelle tragen).
    /// </summary>
    public static readonly HeroDefinition Wand = new(
        Id: "wand",
        DisplayName: "Wand",
        Role: HeroRole.Tank,
        MaxHitPoints: 85,
        BaseAttackDamage: 7,
        Speed: 5,
        MovementRange: 1,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "mauer",
            DisplayName: "Mauer",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsOnField,
            Range: 0,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Mauer",
                    IsInstant: false,
                    Duration: WandBewegungsunfaehigkeitsdauer,
                    Stacking: StackingRule.RefreshDuration,
                    LocksFieldForEnemies: true,
                    RedirectsAttacksToSource: true),
                new EffectTemplate(
                    Name: "Mauer",
                    IsInstant: false,
                    Duration: WandBewegungsunfaehigkeitsdauer,
                    Stacking: StackingRule.RefreshDuration,
                    PreventsMovement: true,
                    AppliesToCaster: true),
            }));

    /// <summary>
    /// Tank: aktiv, erzeugt einen Klon (siehe <see cref="Units.Unit.CreateClone"/>
    /// und <see cref="Abilities.AbilitySystem"/>). Ein erneuter Einsatz
    /// lässt einen bereits vorhandenen Klon verschwinden und erzeugt einen
    /// neuen - das ist inzwischen das generische Verhalten JEDER
    /// klon-erzeugenden Fähigkeit (siehe <see cref="Abilities.AbilitySystem"/>),
    /// nicht ein Illusionist-Sonderfall.
    /// </summary>
    public static readonly HeroDefinition Illusionist = new(
        Id: "illusionist",
        DisplayName: "Illusionist",
        Role: HeroRole.Tank,
        MaxHitPoints: 75,
        BaseAttackDamage: 6,
        Speed: 9,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "trugbild",
            DisplayName: "Trugbild",
            IsPassive: false,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            Effects: new[]
            {
                new EffectTemplate(Name: "Trugbild", IsInstant: true, SpawnsClone: true),
            }));

    /// <summary>
    /// Support: aktiv, wählbarer Modus, wirkt auf alle VERBÜNDETEN im
    /// Umkreis um sich selbst (<see cref="AbilityTargetType.AllUnitsInRadius"/>
    /// mit Reichweite 0 - das Zentrum ist immer das eigene Feld, es gibt also
    /// nur die Moduswahl, keine echte Zielwahl - plus
    /// <see cref="AreaTargetFilter.AlliesOnly"/>, damit anders als beim
    /// Schildträger explizit NUR Verbündete betroffen sind). Modus A gibt
    /// einen Schadensbonus, Modus B einen Bewegungsreichweiten-Bonus, beide
    /// für dieselbe Dauer.
    /// </summary>
    public static readonly HeroDefinition Musiker = new(
        Id: "musiker",
        DisplayName: "Musiker",
        Role: HeroRole.Support,
        MaxHitPoints: 48,
        BaseAttackDamage: 8,
        Speed: 13,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "lied",
            DisplayName: "Lied",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsInRadius,
            Range: 0,
            Radius: 2,
            AreaFilter: AreaTargetFilter.AlliesOnly,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "kampflied",
                    DisplayName: "Kampflied",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Kampflied",
                            IsInstant: false,
                            Duration: 2,
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(BaseAttackDamageDelta: 3)),
                    }),
                new AbilityMode(
                    Id: "marschlied",
                    DisplayName: "Marschlied",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Marschlied",
                            IsInstant: false,
                            Duration: 2,
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(MovementRangeDelta: 1)),
                    }),
            }));

    /// <summary>
    /// Support: aktiv, heftet sich an eine beliebige Einheit in Reichweite
    /// (<see cref="AbilityTargetType.AnyUnit"/> - erst das TATSÄCHLICHE Ziel
    /// entscheidet über Bonus oder Abzug, siehe
    /// <see cref="EffectTemplate.StatModifiersForEnemy"/>). Die Anheftung
    /// zieht Hugger auch RÄUMLICH mit dem Ziel mit
    /// (<see cref="EffectTemplate.CausesSourceToFollowTarget"/>: bewegt sich
    /// das Ziel, zieht Hugger automatisch auf dasselbe Feld mit, ohne dafür
    /// eine eigene Bewegung zu verbrauchen) - dafür ist Hugger, solange die
    /// Anheftung aktiv ist, selbst bewegungsunfähig
    /// (<see cref="EffectTemplate.PreventsMovement"/>) und kann in dieser
    /// Zeit nur noch die Wirkung der Anheftung selbst nutzen. Zusätzlich
    /// wirkt dieselbe Fähigkeit über <see cref="EffectTemplate.AppliesToCaster"/>
    /// NEBEN dem gewählten Ziel auch auf Hugger selbst (reduzierter
    /// eingehender Schaden, solange die Anheftung läuft). Beide Effekt-
    /// Instanzen tragen denselben Namen "Umklammerung" und sind damit über
    /// <see cref="Effects.EffectSystem.RemoveEffectAndLinkedCounterpart"/>
    /// gekoppelt (endet gemeinsam, wenn das Ziel besiegt wird). Die
    /// Anheftung selbst nutzt <see cref="StackingRule.ReplaceForSameSource"/>,
    /// damit ein erneuter Einsatz auf ein ANDERES Ziel die alte Anheftung
    /// (auf dem alten Ziel) tatsächlich beendet, statt sie unangetastet
    /// bestehen zu lassen.
    /// </summary>
    public static readonly HeroDefinition Hugger = new(
        Id: "hugger",
        DisplayName: "Hugger",
        Role: HeroRole.Support,
        MaxHitPoints: 46,
        BaseAttackDamage: 8,
        Speed: 14,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "umklammerung",
            DisplayName: "Umklammerung",
            IsPassive: false,
            TargetType: AbilityTargetType.AnyUnit,
            Range: 2,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Umklammerung",
                    IsInstant: false,
                    Duration: null,
                    Stacking: StackingRule.ReplaceForSameSource,
                    StatModifiers: new StatModifierSet(BaseAttackDamageDelta: 4),
                    StatModifiersForEnemy: new StatModifierSet(BaseAttackDamageDelta: -4),
                    CausesSourceToFollowTarget: true),
                new EffectTemplate(
                    Name: "Umklammerung",
                    IsInstant: false,
                    Duration: null,
                    Stacking: StackingRule.Replace,
                    AppliesToCaster: true,
                    IncomingDamageMultiplier: 0.5,
                    PreventsMovement: true),
            }));

    /// <summary>
    /// Dmg, Fernkämpfer: aktiv, wählbarer Modus auf einen Gegner in
    /// Reichweite. Feuer = Sofortschaden + Schaden über mehrere Runden; Erde
    /// = Sofortschaden + Abzug auf die Bewegungsreichweite des Ziels. Beide
    /// Modi sind vollständig über bereits bestehende Wirkungsarten
    /// abbildbar (Sofortschaden + DoT bzw. Sofortschaden + Stat-Debuff) -
    /// keine Erweiterung des Fähigkeitensystems nötig.
    /// </summary>
    public static readonly HeroDefinition Elementarmagier = new(
        Id: "elementarmagier",
        DisplayName: "Elementarmagier",
        Role: HeroRole.Dmg,
        MaxHitPoints: 32,
        BaseAttackDamage: 13,
        Speed: 10,
        MovementRange: 2,
        AttackRange: 2,
        Ability: new AbilityDefinition(
            Id: "elementarschlag",
            DisplayName: "Elementarschlag",
            IsPassive: false,
            TargetType: AbilityTargetType.Enemy,
            Range: 2,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "feuer",
                    DisplayName: "Feuer",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "Feuerball", IsInstant: true, InstantDamage: 10),
                        new EffectTemplate(
                            Name: "Verbrennung",
                            IsInstant: false,
                            Duration: 3,
                            Stacking: StackingRule.RefreshDuration,
                            DamagePerTick: 4),
                    }),
                new AbilityMode(
                    Id: "erde",
                    DisplayName: "Erde",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "Erdschlag", IsInstant: true, InstantDamage: 8),
                        new EffectTemplate(
                            Name: "Verlangsamung",
                            IsInstant: false,
                            Duration: 2,
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(MovementRangeDelta: -1)),
                    }),
            }));

    /// <summary>
    /// Anzahl HP, um die sich der Schadensbonus des Bogenschützen je
    /// Ladungsrunde erhöht, und die dabei geltende Obergrenze - siehe
    /// <see cref="StackingRule.Accumulate"/>.
    /// </summary>
    private const int BogenschuetzeLadungProRunde = 5;
    private const int BogenschuetzeMaximaleLadung = 20;

    /// <summary>
    /// Dmg, Fernkämpfer: aktiv, wählbarer Modus. "Laden" erhöht mit jeder
    /// Runde, in der er erneut gewählt wird, den Schadensbonus des nächsten
    /// Angriffs um <see cref="BogenschuetzeLadungProRunde"/>, gedeckelt bei
    /// <see cref="BogenschuetzeMaximaleLadung"/> (<see cref="StackingRule.Accumulate"/>)
    /// und macht bewegungsunfähig, solange die Ladung hält
    /// (<see cref="EffectTemplate.PreventsMovement"/> - auf demselben Effekt
    /// wie der Bonus selbst, daher automatisch synchron: ein Angriff
    /// verbraucht beides gemeinsam, siehe <see cref="Combat.CombatSystem.TryAttackDetailed"/>).
    /// "Abbrechen" ist nur wählbar, solange tatsächlich aufgeladen wird
    /// (<see cref="AbilityMode.IsLegal"/>) und löscht die Ladung ohne
    /// weitere Wirkung (<see cref="EffectTemplate.CancelEffectName"/>).
    /// </summary>
    public static readonly HeroDefinition Bogenschuetze = new(
        Id: "bogenschuetze",
        DisplayName: "Bogenschütze",
        Role: HeroRole.Dmg,
        MaxHitPoints: 28,
        BaseAttackDamage: 10,
        Speed: 15,
        MovementRange: 2,
        AttackRange: 3,
        Ability: new AbilityDefinition(
            Id: "aufladen",
            DisplayName: "Aufladen",
            IsPassive: false,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "laden",
                    DisplayName: "Laden",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Aufladen",
                            IsInstant: false,
                            Duration: null,
                            Stacking: StackingRule.Accumulate,
                            PreventsMovement: true,
                            BonusDamageForNextAttack: BogenschuetzeLadungProRunde,
                            MaximumAccumulatedBonusDamage: BogenschuetzeMaximaleLadung),
                    }),
                new AbilityMode(
                    Id: "abbrechen",
                    DisplayName: "Abbrechen",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "AufladenAbbrechen", IsInstant: true, CancelEffectName: "Aufladen"),
                    },
                    IsLegal: (unit, state) => state.GetEffectsOnUnit(unit.Id).Any(e => e.Name == "Aufladen")),
            }));

    /// <summary>Baut einen neuen Standardkatalog mit allen neun Helden.</summary>
    public static HeroCatalog CreateDefaultCatalog() => new(new[]
    {
        Schildtraeger, Elfe, Berserker,
        Wand, Illusionist, Musiker, Hugger,
        Elementarmagier, Bogenschuetze,
    });
}
