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
/// ATTRIBUTWERTE SIND AUSDRÜCKLICH VORLÄUFIG: HP, Schaden, Geschwindigkeit
/// sowie Bewegungs-/Angriffsreichweite aller neun Helden sind erste,
/// plausible Platzhalter und werden ERST SPÄTER über Simulationsläufe
/// (siehe die Plausibilitätsprüfungen im Konsolenprogramm) ausbalanciert -
/// die beiden folgenden Regeln sind dabei aber bewusst bereits jetzt FEST
/// vorgegeben, nicht selbst Gegenstand der späteren Feinabstimmung:
///
/// 1. GESCHWINDIGKEIT PAARWEISE VERSCHIEDEN: alle neun Werte sind über den
///    GESAMTEN Katalog hinweg eindeutig, nach Rolle gestaffelt (Tank 4-6,
///    Support 10-12, Dmg 13-15) - <see cref="HeroCatalog"/> erzwingt das bei
///    JEDER Katalogerzeugung zur Laufzeit (auch bei abgewandelten
///    Balancing-Varianten). Grund: Geschwindigkeit bestimmt die
///    Zugreihenfolge innerhalb eines Teams (siehe <see cref="Match.TurnOrder"/>);
///    bei einem Gleichstand entschiede stattdessen die UnitId, wodurch die
///    Reihenfolge von der (an sich beliebigen) Aufstellungsreihenfolge
///    abhinge statt allein von den Heldenwerten.
///
/// 2. ROLLEN-KORRIDORE FÜR HP UND SCHADEN: damit sich die drei Rollen auch
///    MECHANISCH (nicht nur nominell über <see cref="HeroRole"/>, die laut
///    Anforderung ja explizit KEIN System beeinflussen darf) klar
///    unterscheiden - Grundlage für die spätere Untersuchung, ob sich die
///    Rolle systematisch auf die Balance auswirkt:
///
///      Tank:    HP 75-90,  Grundschaden 5-7    (siehe TankMinHp/TankMaxHp/...)
///      Support: HP 45-55,  Grundschaden 8-10   (siehe SupportMinHp/...)
///      Dmg:     HP 28-45,  Grundschaden 12-15  (siehe DmgMinHp/...)
///
///    Zusätzlich gilt weiterhin: ein Fernkämpfer hat WENIGER HP als ein
///    Nahkämpfer DERSELBEN Rolle (Elfe &lt; Musiker/Hugger; Elementarmagier
///    und Bogenschütze &lt; Berserker). Diese beiden Regeln sind bewusst nur
///    als Test abgesichert (siehe HeroCatalogTests/DefaultHeroesTests),
///    nicht als Konstruktor-Zwang wie die Geschwindigkeit: ein abgewandelter
///    Katalog (z.B. eine gezielte "+50% Schaden"-Testvariante) darf einen
///    Korridor bewusst verlassen, ohne dass die Katalogerzeugung selbst
///    dafür fehlschlägt.
/// </summary>
public static class DefaultHeroes
{
    // ---- Rollen-Korridore (siehe Klassendokumentation Punkt 2) ----
    public const int TankMinHp = 75, TankMaxHp = 90, TankMinDamage = 5, TankMaxDamage = 7;
    public const int SupportMinHp = 45, SupportMaxHp = 55, SupportMinDamage = 8, SupportMaxDamage = 10;
    public const int DmgMinHp = 28, DmgMaxHp = 45, DmgMinDamage = 12, DmgMaxDamage = 15;

    /// <summary>
    /// Tank: aktiv, gibt allen Einheiten auf seinem EIGENEN Feld
    /// (inklusive sich selbst) für eine Runde 50% Schadensreduktion. Das
    /// "eigene Feld" ergibt sich daraus, dass die Fähigkeit
    /// <see cref="AbilityTargetType.AllUnitsOnField"/> mit Reichweite 0
    /// nutzt - das einzige Feld in Reichweite 0 ist das eigene, es gibt
    /// also gar keine echte Zielwahl (ein "Ein-Klick"-Schild).
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing, siehe <see cref="AbilityDefinition.CooldownRounds"/>):
    /// 2 Runden - VORLÄUFIGER Startwert, noch nicht über Simulationen
    /// ausbalanciert. Ohne Abklingzeit ließe sich die Reduktion praktisch
    /// jede eigene Runde erneuern; 2 Runden erzwingen mindestens eine
    /// ungeschützte Zwischenrunde.
    /// </summary>
    public static readonly HeroDefinition Schildtraeger = new(
        Id: "schildtraeger",
        DisplayName: "Schildträger",
        Role: HeroRole.Tank,
        MaxHitPoints: 80, // Tank-Korridor 75-90
        BaseAttackDamage: 7, // Tank-Korridor 5-7
        Speed: 5,
        MovementRange: 1,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "schildwall",
            DisplayName: "Schildwall",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsOnField,
            Range: 0,
            CooldownRounds: 2,
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
    ///
    /// HP am unteren Rand des Support-Korridors (45-55) angesetzt: die
    /// größere Reichweite (2 statt 1 bei Musiker/Hugger) erlaubt ihr, sich
    /// aus der zweiten Reihe heraus in Sicherheit zu halten, was den
    /// niedrigeren Wert rechtfertigt - zugleich bleibt sie als einziger
    /// Fernkämpfer unter den Supports laut Vorgabe unterhalb der beiden
    /// Nahkämpfer Musiker (48) und Hugger (46).
    /// </summary>
    public static readonly HeroDefinition Elfe = new(
        Id: "elfe",
        DisplayName: "Elfe",
        Role: HeroRole.Support,
        MaxHitPoints: 45, // Support-Korridor 45-55, unterer Rand (siehe oben)
        BaseAttackDamage: 8, // Support-Korridor 8-10
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
        MaxHitPoints: 45, // Dmg-Korridor 28-45, oberer Rand (Nahkämpfer - siehe Elementarmagier/Bogenschütze für die Fernkämpfer-Werte darunter)
        BaseAttackDamage: 12, // Dmg-Korridor 12-15
        Speed: 13,
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
                //int bonus = (int)Math.Round(missingFraction * 20);
                int bonus = (int)Math.Floor(missingFraction * 3) * 5;
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
        MaxHitPoints: 85, // Tank-Korridor 75-90
        BaseAttackDamage: 7, // Tank-Korridor 5-7
        Speed: 4,
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
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing): 3 Runden - VORLÄUFIGER
    /// Startwert, noch nicht über Simulationen ausbalanciert. Am längsten
    /// von den vier mit Abklingzeit versehenen Fähigkeiten, da ein Klon
    /// (voller Schadenspuffer für einen Treffer, zusätzlicher Zug in der
    /// Reihenfolge) potenziell die stärkste Einzelwirkung hat.
    /// </summary>
    public static readonly HeroDefinition Illusionist = new(
        Id: "illusionist",
        DisplayName: "Illusionist",
        Role: HeroRole.Tank,
        MaxHitPoints: 75, // Tank-Korridor 75-90
        BaseAttackDamage: 6, // Tank-Korridor 5-7
        Speed: 6,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "trugbild",
            DisplayName: "Trugbild",
            IsPassive: false,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            CooldownRounds: 3,
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
    ///
    /// DISTANZABHÄNGIGE WIRKUNG MIT PLATEAU (<see cref="EffectTemplate.ScaleWithDistance"/>
    /// + <see cref="EffectTemplate.PlateauRadius"/>, dieselbe generische
    /// Mechanik wie bei der Elfe-Heilung, um den Plateau-Radius erweitert -
    /// siehe <see cref="Abilities.AbilitySystem.ApplyToUnit"/>, dort
    /// GENERISCH auch für dauerhafte Stat-Modifikatoren statt nur für
    /// Sofortschaden/-heilung ausgewertet):
    ///   - Kampflied: +3 Angriffsschaden auf Distanz 0 UND 1 (Plateau-Radius
    ///     1 - beide Distanzen gleich stark), +1 auf Distanz 2
    ///     (PlateauRadius: 1, DistanceFalloffPerStep: 2 - der Abfall setzt
    ///     erst NACH dem Plateau ein und beträgt dann 2 je Schritt: 3 - 2*(2-1) = 1).
    ///   - Marschlied: +1 Bewegungsreichweite auf Distanz 0 UND 1 (dasselbe
    ///     Plateau), keine Wirkung mehr auf Distanz 2 (PlateauRadius: 1,
    ///     DistanceFalloffPerStep: 1, zusätzlich MinimumAfterDistanceScaling:
    ///     0 als Sicherheitsnetz gegen einen negativen Bewegungsabzug).
    ///     Dieses "gleich stark auf Distanz 0 UND 1, danach abrupt 0"-Plateau
    ///     war die ursprüngliche Designabsicht und ist mit PlateauRadius jetzt
    ///     EXAKT abgebildet - keine Näherung mehr (siehe EffectTemplate für
    ///     die Begründung, warum die rein lineare Formel das vorher nicht
    ///     konnte und wieso ein zusätzlicher, aber optionaler Parameter statt
    ///     eines komplett neuen Mechanismus gewählt wurde).
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing): 2 Runden - VORLÄUFIGER
    /// Startwert, noch nicht über Simulationen ausbalanciert. Gilt für BEIDE
    /// Modi gemeinsam (die Abklingzeit sitzt auf der Fähigkeit als Ganzes,
    /// nicht je Modus - ein Wechsel zwischen Kampf- und Marschlied umgeht
    /// die Abklingzeit also nicht).
    /// </summary>
    public static readonly HeroDefinition Musiker = new(
        Id: "musiker",
        DisplayName: "Musiker",
        Role: HeroRole.Support,
        MaxHitPoints: 48, // Support-Korridor 45-55
        BaseAttackDamage: 8, // Support-Korridor 8-10
        Speed: 10,
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
            CooldownRounds: 2,
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
                            StatModifiers: new StatModifierSet(BaseAttackDamageDelta: 3),
                            ScaleWithDistance: true,
                            PlateauRadius: 1, // Distanz 0 UND 1 gleich stark (+3)
                            DistanceFalloffPerStep: 2), // ab Distanz 2 (= 1 Schritt hinter dem Plateau): 3 - 2*1 = 1
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
                            StatModifiers: new StatModifierSet(MovementRangeDelta: 1),
                            ScaleWithDistance: true,
                            PlateauRadius: 1, // Distanz 0 UND 1 gleich stark (+1)
                            DistanceFalloffPerStep: 1,
                            MinimumAfterDistanceScaling: 0), // Distanz 2: 1 - 1*1 = 0 - kein negativer Bewegungsabzug möglich
                    }),
            }));

    /// <summary>
    /// Support: aktiv, wählbarer Modus (Bachelorarbeit-Korrektur: Hugger
    /// braucht neben dem eigentlichen Greifen zwingend eine Möglichkeit,
    /// die Anheftung wieder aktiv zu lösen - siehe "Lösen" unten).
    ///
    /// MODUS "GREIFEN" - heftet sich an eine beliebige Einheit in Reichweite
    /// (<see cref="AbilityTargetType.AnyUnit"/> - erst das TATSÄCHLICHE Ziel
    /// entscheidet über Bonus oder Abzug, siehe
    /// <see cref="EffectTemplate.StatModifiersForEnemy"/>). Die Anheftung
    /// zieht Hugger auch RÄUMLICH mit dem Ziel mit
    /// (<see cref="EffectTemplate.CausesSourceToFollowTarget"/>: bewegt sich
    /// das Ziel, zieht Hugger automatisch auf dasselbe Feld mit, ohne dafür
    /// eine eigene Bewegung zu verbrauchen; seit der Korrektur zieht ihn
    /// bereits das ERSTMALIGE Greifen sofort auf das aktuelle Feld des
    /// Ziels, siehe <see cref="Effects.EffectSystem.Apply"/>) - dafür ist
    /// Hugger, solange die Anheftung aktiv ist, selbst bewegungsunfähig
    /// (<see cref="EffectTemplate.PreventsMovement"/>), kann selbst nicht
    /// angreifen (<see cref="EffectTemplate.PreventsAttacking"/>) und ist
    /// für Gegner kein gültiges Angriffs- oder Fähigkeitsziel mehr
    /// (<see cref="EffectTemplate.PreventsBeingTargetedByEnemies"/> - ERSETZT
    /// die vormalige pauschale Schadensreduktion: eine Einheit, die gar
    /// nicht erst zum Ziel werden kann, braucht keine zusätzliche Reduktion
    /// des dann ohnehin nie eintretenden Schadens). Beide generischen
    /// Effektwirkungen sind bewusst allgemeine <see cref="EffectTemplate"/>-
    /// Felder, keine Hugger-Sonderfallprüfung im Interpreter. Zusätzlich
    /// wirkt dieselbe Fähigkeit über <see cref="EffectTemplate.AppliesToCaster"/>
    /// NEBEN dem gewählten Ziel auch auf Hugger selbst. Beide Effekt-
    /// Instanzen (Ziel + Hugger selbst) tragen denselben Namen "Umklammerung"
    /// und sind damit über <see cref="Effects.EffectSystem.RemoveEffectAndLinkedCounterpart"/>
    /// gekoppelt (enden gemeinsam, wenn das Ziel besiegt wird oder die
    /// Anheftung gelöst/verschoben wird). Die Anheftung selbst nutzt
    /// <see cref="StackingRule.ReplaceForSameSource"/>, damit ein erneuter
    /// Einsatz auf ein ANDERES Ziel die alte Anheftung (auf dem alten Ziel)
    /// tatsächlich beendet, statt sie unangetastet bestehen zu lassen - das
    /// ist zugleich der "auf ein anderes Ziel wechseln"-Teil der Vorgabe.
    ///
    /// ANZEIGE DES BUFFS/DEBUFFS (Bachelorarbeit-Entscheidung): der Bonus/
    /// Abzug bleibt bewusst TEIL DESSELBEN "Umklammerung"-Effekts statt eine
    /// eigene, separat benannte Effekt-Instanz auf dem Ziel zu werden. Grund:
    /// <see cref="EffectTemplate.CancelEffectName"/> (siehe "Lösen" unten)
    /// kann laut <see cref="AbilitySystem.ApplyToUnit"/> AUSSCHLIESSLICH
    /// Effekte auf dem ANWENDER SELBST löschen, nie auf einem beliebigen
    /// anderen Ziel - eine zusätzliche, auf dem GEGRIFFENEN ZIEL verankerte
    /// Effekt-Instanz könnte "Lösen" also gar nicht mit aufräumen und bliebe
    /// nach dem Lösen fälschlich bestehen. Die Sichtbarkeits-Anforderung
    /// ("als eigener, sichtbarer Effekt ... in der Oberfläche angezeigt")
    /// wird stattdessen rein auf der Anzeigeseite erfüllt: die Godot-
    /// Oberfläche liest die Stat-Modifikatoren jedes Effekts aus und zeigt
    /// Betrag/Vorzeichen explizit an (siehe TacticsCoreGodot.Shared.EffectInfo),
    /// statt nur den Effektnamen ohne Wirkung darzustellen.
    ///
    /// MODUS "LÖSEN" - löscht die Anheftung aktiv, ohne ein neues Ziel zu
    /// wählen (<see cref="AbilityMode.IgnoresChosenTarget"/>, generisch über
    /// <see cref="EffectTemplate.CancelEffectName"/> + <see cref="EffectTemplate.AppliesToCaster"/>,
    /// exakt derselbe Mechanismus wie beim Bogenschützen "Abbrechen") und ist
    /// nur wählbar, solange tatsächlich angeheftet ist
    /// (<see cref="AbilityMode.IsLegal"/>). Damit sind Hugger, solange er
    /// angeheftet ist, genau die drei laut Vorgabe verbleibenden Handlungen
    /// möglich: greifen (neues Ziel bzw. dasselbe erneut), lösen, oder
    /// passen - ein direkter Grundangriff ist durch die Angriffssperre oben
    /// bereits ausgeschlossen.
    /// </summary>
    public static readonly HeroDefinition Hugger = new(
        Id: "hugger",
        DisplayName: "Hugger",
        Role: HeroRole.Support,
        MaxHitPoints: 46, // Support-Korridor 45-55
        BaseAttackDamage: 8, // Support-Korridor 8-10
        Speed: 11,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "umklammerung",
            DisplayName: "Umklammerung",
            IsPassive: false,
            TargetType: AbilityTargetType.AnyUnit,
            Range: 2,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "greifen",
                    DisplayName: "Greifen",
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
                            PreventsMovement: true,
                            PreventsAttacking: true,
                            PreventsBeingTargetedByEnemies: true),
                    }),
                new AbilityMode(
                    Id: "loesen",
                    DisplayName: "Lösen",
                    IgnoresChosenTarget: true,
                    IsLegal: (unit, state) => state.GetEffectsOnUnit(unit.Id).Any(e => e.Name == "Umklammerung"),
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "UmklammerungLoesen", IsInstant: true, AppliesToCaster: true, CancelEffectName: "Umklammerung"),
                    }),
            }));

    /// <summary>
    /// Dmg, Fernkämpfer: aktiv, wählbarer Modus auf einen Gegner in
    /// Reichweite. Feuer = Sofortschaden + Schaden über mehrere Runden; Erde
    /// = Sofortschaden + Abzug auf die Bewegungsreichweite des Ziels. Beide
    /// Modi sind vollständig über bereits bestehende Wirkungsarten
    /// abbildbar (Sofortschaden + DoT bzw. Sofortschaden + Stat-Debuff) -
    /// keine Erweiterung des Fähigkeitensystems nötig.
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing - strukturelles Problem, das
    /// diese Abklingzeit beheben soll): beide Modi liefern Sofortschaden
    /// PLUS einen anhaltenden Zusatzeffekt (DoT bzw. Bewegungsabzug) und
    /// dominieren damit den einfachen Grundangriff (nur Sofortschaden ohne
    /// Zusatzwirkung) strikt - der Elementarmagier hätte faktisch drei
    /// gleichwertig nutzbare Angriffsoptionen pro Zug, ohne dass der
    /// Grundangriff je die bessere Wahl wäre. 2 Runden Abklingzeit
    /// erzwingen, dass mindestens jede zweite Runde der (schwächere, aber
    /// abklingzeitfreie) Grundangriff die einzige Angriffsoption ist -
    /// VORLÄUFIGER Startwert, noch nicht über Simulationen ausbalanciert.
    /// </summary>
    public static readonly HeroDefinition Elementarmagier = new(
        Id: "elementarmagier",
        DisplayName: "Elementarmagier",
        Role: HeroRole.Dmg,
        MaxHitPoints: 32, // Dmg-Korridor 28-45 (Fernkämpfer - weniger als der Nahkämpfer Berserker)
        BaseAttackDamage: 13, // Dmg-Korridor 12-15
        Speed: 14,
        MovementRange: 2,
        AttackRange: 2,
        Ability: new AbilityDefinition(
            Id: "elementarschlag",
            DisplayName: "Elementarschlag",
            IsPassive: false,
            TargetType: AbilityTargetType.Enemy,
            Range: 2,
            CooldownRounds: 2,
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
        MaxHitPoints: 28, // Dmg-Korridor 28-45, unterer Rand (reinster Fernkämpfer - geringste HP im gesamten Katalog)
        BaseAttackDamage: 12, // Dmg-Korridor 12-15
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
