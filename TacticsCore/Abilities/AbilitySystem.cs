using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Abilities;

/// <summary>
/// Generischer Interpreter für Heldenfähigkeiten: ermittelt legale
/// Ziel-/Moduskombinationen und wendet die in einer
/// <see cref="AbilityDefinition"/> hinterlegten <see cref="EffectTemplate"/>e
/// an. Es gibt bewusst EINEN Codepfad für ALLE Fähigkeiten - welcher Held
/// welche Wirkung hat, ergibt sich ausschließlich aus den Daten der
/// jeweiligen Definition, nicht aus einer Verzweigung nach Helden-Id
/// (Anforderung 8: "ohne für jeden Fall Sonderlogik zu schreiben").
/// </summary>
public static class AbilitySystem
{
    /// <summary>
    /// Liefert alle aktuell legalen Fähigkeitseinsätze (Ziel + ggf. Modus)
    /// für <paramref name="hero"/>, stabil sortiert nach (q, r, Team-Bezug,
    /// UnitId) - bei Feld-Zielen wird dafür die Feldkoordinate selbst als
    /// (q, r) verwendet und Guid.Empty als Platzhalter für die (in dem Fall
    /// nicht vorhandene) UnitId. Leere Liste, wenn die Fähigkeit passiv ist,
    /// der Held ein Klon ist, oder eine Abklingzeit läuft.
    ///
    /// TEAMNEUTRALE SORTIERUNG (Bachelorarbeit-Korrektur "Symmetrie der
    /// Spiegelpaarungen"): Stehen auf DEMSELBEN Feld mehrere Optionen, kommen
    /// Optionen mit Ziel im EIGENEN Team (der Anwender selbst eingeschlossen)
    /// vor Optionen mit gegnerischem Ziel, erst danach entscheidet die UnitId.
    /// Ohne diese Zwischenstufe hing die Reihenfolge davon ab, welches Team die
    /// kleineren Ids trägt (Team A 1-3, Team B 101-103): der Zufallsagent wählt
    /// per Index, ein Teamtausch (siehe GameSetup.WithSwappedTeams) ließ ihn
    /// bei gleichem Zufallswert eine ANDERE Option treffen. Praktisch trat das
    /// nur beim Hugger auf - seine Fähigkeit bietet über
    /// <see cref="AbilityTargetType.AnyUnit"/> eigene UND gegnerische Einheiten
    /// in derselben Liste an, und nach dem Greifen stehen "Lösen" (Schlüssel =
    /// eigene Id des Huggers) und "Greifen" (Schlüssel = Id des Ziels) auf
    /// demselben Feld. Innerhalb EINES Teams bleibt die Id-Reihenfolge
    /// erhalten (ein Teamtausch vertauscht die Id-Bereiche geschlossen), für
    /// alle anderen Fähigkeiten (nur eigene ODER nur gegnerische Ziele) ist der
    /// neue Schlüssel konstant und ändert nichts.
    ///
    /// KLON-ERZEUGENDE FÄHIGKEITEN sind bewusst NICHT blockiert, solange
    /// bereits ein Klon existiert (anders als in einer früheren Fassung):
    /// ein erneuter Einsatz ist immer legal und lässt den alten Klon
    /// verschwinden (siehe <see cref="ApplyToUnit"/> und Illusionist) - "je
    /// Anwender nur ein Klon gleichzeitig" gilt weiterhin, nur die
    /// Durchsetzung ist jetzt "ersetzen" statt "ablehnen".
    /// </summary>
    public static IReadOnlyList<AbilityUseOption> GetLegalAbilityUses(Hero hero, GameState state)
    {
        var ability = hero.Definition.Ability;

        if (ability.IsPassive || hero.IsClone || hero.IsDefeated)
        {
            return Array.Empty<AbilityUseOption>();
        }

        if (hero.AbilityCooldownRemaining is > 0)
        {
            return Array.Empty<AbilityUseOption>();
        }

        var targets = GetLegalTargets(hero, ability, state);

        var options = new List<AbilityUseOption>();
        if (ability.Modes is null)
        {
            foreach (var target in targets)
            {
                options.Add(new AbilityUseOption(target, null));
            }
        }
        else
        {
            // Modi, die das gewählte Ziel ohnehin ignorieren (siehe
            // AbilityMode.IgnoresChosenTarget, z.B. Hugger "Lösen"), werden
            // GENAU EINMAL angeboten - mit dem Anwender selbst als rein
            // technischem Platzhalter-Ziel -, statt einmal je Zielkandidat
            // sinnlos vervielfacht zu werden.
            foreach (var mode in ability.Modes.Where(m => m.IgnoresChosenTarget && IsModeLegal(m, hero, state)))
            {
                options.Add(new AbilityUseOption(new AbilityTarget.UnitTarget(hero), mode));
            }

            foreach (var target in targets)
            {
                foreach (var mode in ability.Modes.Where(m => !m.IgnoresChosenTarget && IsModeLegal(m, hero, state)))
                {
                    options.Add(new AbilityUseOption(target, mode));
                }
            }
        }

        return options
            .OrderBy(o => SortKey(o.Target).Q)
            .ThenBy(o => SortKey(o.Target).R)
            .ThenBy(o => o.Target is AbilityTarget.UnitTarget ut && ut.Unit.Team != hero.Team ? 1 : 0)
            .ThenBy(o => SortKey(o.Target).UnitId)
            .ThenBy(o => o.Mode?.Id ?? string.Empty, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Versucht, die Fähigkeit von <paramref name="hero"/> auf
    /// <paramref name="target"/> (ggf. mit <paramref name="mode"/>)
    /// einzusetzen. Validiert die Wahl selbst NEU anhand des aktuellen
    /// Zustands (nicht per Objekt-Identität mit einer zuvor zurückgegebenen
    /// Liste - dieselbe Vorgehensweise wie CombatSystem.CanAttack), damit
    /// ein Agent, der sein Ziel aus einer zuvor abgerufenen Liste gewählt
    /// hat, auch bei einer frisch neu berechneten Liste zuverlässig als
    /// gültig erkannt wird. Ein ungültiger Versuch ändert nichts.
    /// </summary>
    public static bool TryUseAbility(Hero hero, AbilityTarget target, GameState state, AbilityMode? mode = null, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        if (!CanUseAbility(hero, target, state, mode))
        {
            return false;
        }

        var ability = hero.Definition.Ability;
        var templates = mode?.Effects ?? ability.Effects;

        foreach (var template in templates)
        {
            ApplyTemplate(state, hero, target, ability, template, replayEntries, roundNumber);
        }

        if (ability.CooldownRounds is int cooldown)
        {
            hero.SetAbilityCooldown(cooldown);
        }

        return true;
    }

    /// <summary>
    /// Ob ein Modus gerade wählbar ist: seine eigene Bedingung
    /// (<see cref="AbilityMode.IsLegal"/>) UND - Behebung der Regellücke
    /// "angehefteter letzter Held", siehe <see cref="EffectSystem.ReleaseAttachmentsOfLastHeroes"/> -
    /// kein Modus, der den Anwender anheftet, solange er der letzte lebende Held
    /// seines Teams ist (sonst wäre er sofort wieder unangreifbar).
    /// </summary>
    private static bool IsModeLegal(AbilityMode mode, Hero hero, GameState state) =>
        (mode.IsLegal is null || mode.IsLegal(hero, state))
        && !(mode.Effects.Any(e => e.CausesSourceToFollowTarget) && EffectSystem.IsLastHeroOfTeam(hero, state));

    private static bool CanUseAbility(Hero hero, AbilityTarget target, GameState state, AbilityMode? mode)
    {
        var ability = hero.Definition.Ability;
        if (ability.IsPassive || hero.IsClone || hero.IsDefeated)
        {
            return false;
        }

        if (hero.AbilityCooldownRemaining is > 0)
        {
            return false;
        }

        if (ability.Modes is null)
        {
            if (mode is not null)
            {
                return false; // Modus angegeben, obwohl die Fähigkeit keine Moduswahl kennt
            }
        }
        else if (mode is null || !ability.Modes.Any(m => m.Id == mode.Id && IsModeLegal(m, hero, state)))
        {
            return false;
        }

        if (mode?.IgnoresChosenTarget == true)
        {
            // Das übergebene Ziel ist bei einem solchen Modus nur ein
            // technischer Platzhalter (siehe AbilityMode.IgnoresChosenTarget)
            // - die sonst übliche Ziel-Gültigkeitsprüfung entfällt bewusst.
            return true;
        }

        return target switch
        {
            AbilityTarget.UnitTarget u => IsValidUnitTarget(hero, ability, u.Unit, state),
            AbilityTarget.FieldTarget f => IsValidFieldTarget(hero, ability, f.Field, state),
            _ => false,
        };
    }

    private static bool IsValidUnitTarget(Hero hero, AbilityDefinition ability, Unit target, GameState state)
    {
        if (target.IsDefeated)
        {
            return false;
        }

        if (HexCoordinate.Distance(hero.Position, target.Position) > ability.Range)
        {
            return false;
        }

        // Generische Unangreifbarkeit gegenüber Gegnern (siehe
        // EffectTemplate.PreventsBeingTargetedByEnemies, z.B. Hugger während
        // der Anheftung) - team-relativ, ein Verbündeter des Ziels
        // (einschließlich des Ziels selbst, siehe AbilityTargetType.Self/Ally)
        // ist davon nie betroffen.
        if (EffectiveStats.PreventsBeingTargetedBy(target, hero.Team, state))
        {
            return false;
        }

        return ability.TargetType switch
        {
            AbilityTargetType.Self => target.Id == hero.Id,
            // "Verbündet" schließt den Anwender selbst mit ein - siehe
            // AbilityTargetType.Ally für die Begründung (Elfe: Selbstheilung).
            AbilityTargetType.Ally => target.Team == hero.Team && !target.IsClone,
            AbilityTargetType.Enemy => target.Team != hero.Team,
            AbilityTargetType.AnyUnit => target.Id != hero.Id && !target.IsClone,
            _ => false,
        };
    }

    private static bool IsValidFieldTarget(Hero hero, AbilityDefinition ability, HexCoordinate field, GameState state)
    {
        if (!state.Board.Contains(field))
        {
            return false;
        }

        if (HexCoordinate.Distance(hero.Position, field) > ability.Range)
        {
            return false;
        }

        return ability.TargetType is AbilityTargetType.Field or AbilityTargetType.AllUnitsOnField or AbilityTargetType.AllUnitsInRadius;
    }

    private static IReadOnlyList<AbilityTarget> GetLegalTargets(Hero hero, AbilityDefinition ability, GameState state)
    {
        switch (ability.TargetType)
        {
            case AbilityTargetType.Self:
                return new AbilityTarget[] { new AbilityTarget.UnitTarget(hero) };

            case AbilityTargetType.Ally:
                // Schließt den Anwender selbst mit ein - siehe
                // AbilityTargetType.Ally (Elfe: Selbstheilung).
                return FieldsInRange(hero, ability.Range, state)
                    .SelectMany(state.GetUnitsAt)
                    .Where(u => !u.IsDefeated && u.Team == hero.Team && !u.IsClone)
                    .Select(u => (AbilityTarget)new AbilityTarget.UnitTarget(u))
                    .ToList();

            case AbilityTargetType.Enemy:
                return FieldsInRange(hero, ability.Range, state)
                    .SelectMany(state.GetUnitsAt)
                    .Where(u => !u.IsDefeated && u.Team != hero.Team)
                    .Where(u => !EffectiveStats.PreventsBeingTargetedBy(u, hero.Team, state))
                    .Select(u => (AbilityTarget)new AbilityTarget.UnitTarget(u))
                    .ToList();

            case AbilityTargetType.AnyUnit:
                return FieldsInRange(hero, ability.Range, state)
                    .SelectMany(state.GetUnitsAt)
                    .Where(u => !u.IsDefeated && u.Id != hero.Id && !u.IsClone)
                    .Where(u => !EffectiveStats.PreventsBeingTargetedBy(u, hero.Team, state))
                    .Select(u => (AbilityTarget)new AbilityTarget.UnitTarget(u))
                    .ToList();

            case AbilityTargetType.Field:
            case AbilityTargetType.AllUnitsOnField:
            case AbilityTargetType.AllUnitsInRadius:
                return FieldsInRange(hero, ability.Range, state)
                    .Select(c => (AbilityTarget)new AbilityTarget.FieldTarget(c))
                    .ToList();

            default:
                return Array.Empty<AbilityTarget>();
        }
    }

    private static IEnumerable<HexCoordinate> FieldsInRange(Hero hero, int range, GameState state) =>
        state.Board.Fields
            .Select(f => f.Coordinate)
            .Where(c => HexCoordinate.Distance(hero.Position, c) <= range);

    private static (int Q, int R, Guid UnitId) SortKey(AbilityTarget target) => target switch
    {
        AbilityTarget.UnitTarget u => (u.Unit.Position.Q, u.Unit.Position.R, u.Unit.Id),
        AbilityTarget.FieldTarget f => (f.Field.Q, f.Field.R, Guid.Empty),
        _ => (0, 0, Guid.Empty),
    };

    /// <summary>
    /// Wendet ein einzelnes Template an. <see cref="EffectTemplate.AppliesToCaster"/>
    /// leitet die Wirkung generisch auf den ANWENDER selbst um, unabhängig
    /// vom eigentlich gewählten Ziel - für Fähigkeiten, die NEBEN dem
    /// gewählten Ziel zusätzlich sich selbst betreffen (siehe Hugger). Bei
    /// einem Einheiten-Ziel ist der Anwender selbst der Bezugspunkt für einen
    /// evtl. konfigurierten Distanzabfall (siehe ApplyToUnit); bei einem
    /// Feld-Ziel übernimmt ApplyToFieldTarget stattdessen das GEWÄHLTE
    /// ZENTRUMSFELD als Bezugspunkt (siehe dortige Begründung).
    /// </summary>
    private static void ApplyTemplate(GameState state, Hero source, AbilityTarget target, AbilityDefinition ability, EffectTemplate template, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        if (template.AppliesToCaster)
        {
            ApplyToUnit(state, source, source, source.Position, template, replayEntries, roundNumber);
            return;
        }

        switch (target)
        {
            case AbilityTarget.UnitTarget u:
                ApplyToUnit(state, source, u.Unit, source.Position, template, replayEntries, roundNumber);
                break;

            case AbilityTarget.FieldTarget f:
                ApplyToFieldTarget(state, source, f.Field, ability, template, replayEntries, roundNumber);
                break;
        }
    }

    /// <summary>
    /// Wendet ein Feld-Ziel-Template an. Bachelorarbeit-Korrektur: ein evtl.
    /// konfigurierter Distanzabfall (<see cref="EffectTemplate.ScaleWithDistance"/>)
    /// misst die Distanz hier bewusst vom GEWÄHLTEN ZENTRUMSFELD
    /// <paramref name="field"/> aus, NICHT von der Position des Anwenders
    /// (<paramref name="source"/>.Position) - siehe die entsprechenden
    /// <c>distanceOrigin</c>-Aufrufe unten. Vor dieser Korrektur berechnete
    /// <see cref="ApplyToUnit"/> die Distanz IMMER ab dem Anwender; bei den
    /// aktuell neun Helden blieb das folgenlos, weil jede
    /// <see cref="AbilityTargetType.AllUnitsInRadius"/>-Fähigkeit Reichweite
    /// 0 hat (das Zentrum ist dadurch zwangsläufig immer das eigene Feld des
    /// Anwenders, beides fällt zusammen) - bei einer künftigen
    /// Flächenfähigkeit mit Reichweite &gt; 0 UND Distanzabfall hätte sich das
    /// aber sichtbar und falsch ausgewirkt (ein Verbündeter GENAU auf dem
    /// gewählten Zentrumsfeld wäre fälschlich mit der Distanz Anwender→Feld
    /// statt mit Distanz 0 skaliert worden). Siehe AbilitySystemTests für
    /// den Regressionstest mit Reichweite &gt; 0.
    /// </summary>
    private static void ApplyToFieldTarget(GameState state, Hero source, HexCoordinate field, AbilityDefinition ability, EffectTemplate template, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        // Bei AllUnitsInRadius werden zusätzlich alle Felder im
        // konfigurierten Radius um das gewählte Zentrum erfasst; in allen
        // anderen Fällen (Field, AllUnitsOnField) betrifft der Effekt genau
        // das gewählte Feld.
        IReadOnlyList<HexCoordinate> affectedFields = ability.TargetType == AbilityTargetType.AllUnitsInRadius
            ? state.Board.Fields
                .Select(f => f.Coordinate)
                .Where(c => HexCoordinate.Distance(field, c) <= (ability.Radius ?? 0))
                .ToList()
            : new[] { field };

        // Feld-verankerte Fähigkeiten des Effekts (Schadensreduktion,
        // Feldsperre, Angriffs-Umleitung) - wirken auf das/die Feld(er)
        // selbst, unabhängig davon, wer aktuell dort steht oder später
        // dort hinzieht, solange der Effekt aktiv ist. Ein Teamfilter ergibt
        // hier keinen Sinn (ein Feld selbst hat kein Team) - AreaFilter
        // betrifft ausschließlich die untenstehende Einheiten-Wirkung.
        if (template.IncomingDamageMultiplier is not null || template.LocksFieldForEnemies || template.RedirectsAttacksToSource)
        {
            foreach (var affectedField in affectedFields)
            {
                EffectSystem.Apply(state, ActiveEffect.OnField(source, affectedField, template), replayEntries, roundNumber);
            }
        }

        // Wirkungen, die konkrete EINHEITEN auf dem/den Feld(ern) treffen
        // (Sofortschaden/-heilung, DoT, Buff/Debuff) - stabil sortiert nach
        // (q, r, UnitId) AUS SICHT DER EIGENEN BLÜTE des Anwenders angewendet
        // (Bachelorarbeit, Korrektur des Seitenvorteils: für ein Team, dessen
        // eigene Blüte B ist, wird (q, r) vorher über Board.MirrorBetweenBlossoms
        // gespiegelt). Die Reihenfolge bestimmt die Reihenfolge der Instanzen in
        // der Effektliste - und davon hängt ab, welche Instanz eines Mehrfach-Buffs
        // beim Ablauf als "Gegenpart" mit entfernt wird (siehe
        // EffectSystem.RemoveEffectAndLinkedCounterpart, z.B. Kampflied). Mit
        // absolutem (q, r) war das zwischen den Seiten nicht symmetrisch. AreaFilter (siehe AreaTargetFilter)
        // schränkt dabei generisch ein, welche Team-Zugehörigkeit relativ
        // zum Anwender betroffen ist (Standard: alle, wie bisher).
        if (template.InstantDamage is not null || template.InstantHeal is not null || template.DamagePerTick is not null || template.StatModifiers is not null || template.BonusDamageForNextAttack is not null)
        {
            var affectedUnits = affectedFields
                .SelectMany(state.GetUnitsAt)
                .Where(u => !u.IsDefeated)
                .Where(u => ability.AreaFilter switch
                {
                    AreaTargetFilter.AlliesOnly => u.Team == source.Team,
                    AreaTargetFilter.EnemiesOnly => u.Team != source.Team,
                    _ => true,
                })
                // Generische Unangreifbarkeit gegenüber Gegnern gilt auch
                // für Flächenwirkungen (siehe EffectTemplate.PreventsBeingTargetedByEnemies) -
                // betrifft ausschließlich Einheiten aus einem ANDEREN Team
                // als die Quelle, ein verbündeter Flächen-Buff bleibt also
                // unberührt.
                .Where(u => !EffectiveStats.PreventsBeingTargetedBy(u, source.Team, state))
                .Select(u => (Einheit: u, Sicht: state.HomeBlossomOf(source.Team) == TacticsCore.Board.FieldType.BlossomB
                    ? TacticsCore.Board.Board.MirrorBetweenBlossoms(u.Position) : u.Position))
                .OrderBy(e => e.Sicht.Q).ThenBy(e => e.Sicht.R).ThenBy(e => e.Einheit.Id)
                .Select(e => e.Einheit)
                .ToList();

            foreach (var unit in affectedUnits)
            {
                // distanceOrigin = field (das gewählte Zentrum), NICHT
                // source.Position - siehe Klassendoku dieser Methode oben.
                ApplyToUnit(state, source, unit, field, template, replayEntries, roundNumber);
            }
        }
    }

    /// <summary>
    /// Wendet <paramref name="template"/> auf <paramref name="target"/> an.
    /// <paramref name="distanceOrigin"/> ist der Bezugspunkt für einen evtl.
    /// konfigurierten Distanzabfall - bei einem direkten Einheiten-Ziel die
    /// Position des Anwenders, bei einer Flächenwirkung das gewählte
    /// Zentrumsfeld (siehe die beiden Aufrufer, ApplyTemplate bzw.
    /// ApplyToFieldTarget, für die genaue Begründung).
    /// </summary>
    private static void ApplyToUnit(GameState state, Hero source, Unit target, HexCoordinate distanceOrigin, EffectTemplate template, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        // Aktives Beenden eines eigenen, noch laufenden Effekts (siehe
        // Bogenschütze: "Aufladung abbrechen") - generisch über den Namen,
        // kein weiterer Effekt wird in diesem Fall angewendet. Bislang
        // konnte eine Fähigkeit nur Effekte hinzufügen, nie proaktiv einen
        // eigenen entfernen; das ist der einzige Zweck dieses Feldes.
        if (template.CancelEffectName is string effectNameToCancel)
        {
            var effectToCancel = state.GetEffectsOnUnit(source.Id).FirstOrDefault(e => e.Name == effectNameToCancel);
            if (effectToCancel is not null)
            {
                EffectSystem.RemoveEffectAndLinkedCounterpart(state, effectToCancel, replayEntries, roundNumber);
            }

            return;
        }

        int distance = HexCoordinate.Distance(distanceOrigin, target.Position);

        if (template.IsInstant)
        {
            if (template.InstantDamage is int damage)
            {
                CombatSystem.ApplyDamageAndHandleDefeat(state, source, target, ScaleByDistance(damage, distance, template));
            }

            if (template.InstantHeal is int heal)
            {
                target.Heal(ScaleByDistance(heal, distance, template));
            }

            if (template.SpawnsClone && target.Id == source.Id && !target.IsClone)
            {
                // Ein bereits vorhandener Klon verschwindet beim erneuten
                // Einsatz (siehe Illusionist), statt den Einsatz abzulehnen -
                // "je Anwender nur ein Klon gleichzeitig" gilt weiterhin,
                // nur die Durchsetzung ist "ersetzen" statt "verbieten".
                var existingClone = state.Units.FirstOrDefault(u => u.IsClone && u.CreatorId == source.Id);
                if (existingClone is not null)
                {
                    existingClone.ApplyDamage(existingClone.MaxHitPoints);
                    state.RemoveUnit(existingClone);
                    EffectSystem.RemoveEffectsRelatedTo(state, existingClone.Id);
                    replayEntries?.Add(ReplayEntry.ForCloneDespawned(roundNumber, existingClone.Id, existingClone.Team, source.Id));
                }

                state.AddUnit(Unit.CreateClone(source, source.Definition.DisplayName));
            }

            return;
        }

        // Team-relative Effektstärke (siehe EffectTemplate.StatModifiersForEnemy
        // und Hugger): bei einem gegnerischen tatsächlichen Ziel gilt - falls
        // gesetzt - das gegnerische Modifikator-Set statt des normalen.
        var effectiveTemplate = template.StatModifiersForEnemy is not null && target.Team != source.Team
            ? template with { StatModifiers = template.StatModifiersForEnemy }
            : template;

        // Entfernungsabhängige Skalierung (siehe ScaleByDistance) gilt
        // GENERISCH nicht nur für Sofortschaden/-heilung, sondern auch für
        // dauerhafte Stat-Modifikatoren (Anforderung: Musiker-Lied) - jedes
        // der vier Delta-Felder wird unabhängig über dieselbe Formel
        // skaliert. Ein Delta von 0 bleibt dabei bewusst 0 (nicht negativ) -
        // ein nicht genutztes Feld (z.B. MovementRangeDelta bei einem reinen
        // Schadensbonus) soll durch die Entfernung nicht erst zu einer
        // eigenen, ungewollten Wirkung werden.
        if (effectiveTemplate.ScaleWithDistance && effectiveTemplate.StatModifiers is { } baseModifiers)
        {
            var scaledModifiers = new StatModifierSet(
                ScaleNonZeroByDistance(baseModifiers.BaseAttackDamageDelta, distance, effectiveTemplate),
                ScaleNonZeroByDistance(baseModifiers.MovementRangeDelta, distance, effectiveTemplate),
                ScaleNonZeroByDistance(baseModifiers.AttackRangeDelta, distance, effectiveTemplate),
                ScaleNonZeroByDistance(baseModifiers.SpeedDelta, distance, effectiveTemplate),
                ScaleMultiplierByDistance(baseModifiers.AttackDamageMultiplier, distance, effectiveTemplate));
            effectiveTemplate = effectiveTemplate with { StatModifiers = scaledModifiers };
        }

        // Nicht-instantane (dauerhafte) Effekte werden als ActiveEffect
        // angelegt - inklusive Stapelregel (siehe EffectSystem.Apply).
        EffectSystem.Apply(state, ActiveEffect.OnUnit(source, target, effectiveTemplate), replayEntries, roundNumber);
    }

    private static int ScaleNonZeroByDistance(int value, int distance, EffectTemplate template) =>
        value == 0 ? 0 : ScaleByDistance(value, distance, template);

    /// <summary>
    /// Entfernungsskalierung eines prozentualen Schadensmodifikators (Designentscheidung
    /// "Kampflied prozentual"): skaliert wird die Abweichung von 1 in PROZENTPUNKTEN
    /// über dieselbe Formel wie die Deltas (<see cref="ScaleByDistance"/>) - beim
    /// Kampflied 1,4 (+40) mit Abfall 20 je Schritt hinter dem Plateau: Distanz 2 ->
    /// +20 -> 1,2. Innerhalb des Plateaus bleibt der Multiplikator exakt unverändert.
    /// </summary>
    private static double ScaleMultiplierByDistance(double multiplier, int distance, EffectTemplate template)
    {
        if (multiplier == 1.0)
        {
            return 1.0;
        }

        int percentPoints = (int)Math.Round((multiplier - 1.0) * 100);
        int scaled = ScaleByDistance(percentPoints, distance, template);
        return scaled == percentPoints ? multiplier : 1.0 + scaled / 100.0;
    }

    /// <summary>
    /// Lineare Entfernungsskalierung MIT optionalem Plateau (Anforderung 8):
    /// bis einschließlich <see cref="EffectTemplate.PlateauRadius"/>
    /// Distanzschritte bleibt die Wirkung UNVERÄNDERT auf voller Stärke, erst
    /// darüber hinaus sinkt sie je weiterem Schritt um
    /// <see cref="EffectTemplate.DistanceFalloffPerStep"/>, nach unten
    /// begrenzt durch <see cref="EffectTemplate.MinimumAfterDistanceScaling"/>.
    /// Bei <c>PlateauRadius: 0</c> (Standardwert) entspricht das exakt der
    /// bisherigen Formel ohne Plateau (Abfall setzt sofort ab Distanz 1 ein).
    /// Bewusst eine einfache, vollständig deterministische Formel ohne
    /// Zufall - "Distanz 1 stärker als Distanz 2" folgt direkt daraus.
    /// </summary>
    private static int ScaleByDistance(int baseAmount, int distance, EffectTemplate template)
    {
        if (!template.ScaleWithDistance)
        {
            return baseAmount;
        }

        int distanceBeyondPlateau = Math.Max(0, distance - template.PlateauRadius);
        int scaled = baseAmount - (template.DistanceFalloffPerStep ?? 0) * distanceBeyondPlateau;
        return template.MinimumAfterDistanceScaling is int minimum ? Math.Max(minimum, scaled) : scaled;
    }
}
