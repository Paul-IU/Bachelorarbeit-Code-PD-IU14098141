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
/// Ablaufen"; Bachelorarbeit-Korrektur "Wirkungsdauer hängt am Anwender"):
/// Es gibt genau ZWEI getrennte Vorgänge, beide zu Beginn eines Zugs (siehe
/// MatchController.ExecuteTurn), noch bevor der Agent nach der ersten
/// Teilaktion gefragt wird - aber an jeweils einer ANDEREN Einheit:
///   - SCHADENS-TICK (<see cref="TickDamageOverTime"/>): beim Zugbeginn der
///     ZIELEINHEIT - sie erleidet den Schaden über Zeit (z.B. Verbrennung)
///     und der Tick-Schaden klingt ggf. ab.
///   - DAUER-TICK (<see cref="TickDurationsOfSource"/>): beim Zugbeginn des
///     ANWENDERS (<see cref="ActiveEffect.SourceUnitId"/>) - die Restdauer
///     JEDES seiner Effekte mit begrenzter Dauer sinkt um 1, bei 0 endet der
///     Effekt. Das gilt unabhängig vom Anker (unit- UND feld-verankert) und
///     davon, wie viele Einheiten der Effekt betrifft: die Dauer hängt an
///     genau EINER Stelle, dem Anwender, nicht an den Betroffenen. Ein
///     Schildwall (Dauer 1), gesetzt in Runde N, schützt damit ALLE Einheiten
///     auf seinem Feld bis zum Beginn des NÄCHSTEN eigenen Zugs seines
///     Anwenders - also genau eine volle Runde, egal an welcher Position der
///     Reihenfolge der Anwender steht.
/// <see cref="TickUnitEffects"/> ruft beide Vorgänge in dieser Reihenfolge auf
/// (erst der Schaden an der tickenden Einheit als ZIEL, dann die Dauer ihrer
/// eigenen Effekte als ANWENDER) - das ist exakt die Reihenfolge "erst Schaden,
/// dann Ablauf" des früheren, nur zielbezogenen Ticks.
///
/// FRÜHERES VERHALTEN (nur zur Nachvollziehbarkeit der Korrektur): unit-
/// verankerte Effekte tickten samt Dauer beim Zugbeginn ihres ZIELS, feld-
/// verankerte einmal zu Beginn JEDER RUNDE. Ein Schildwall/eine Mauer lief
/// deshalb schon beim nächsten Rundenbeginn ab - also VOR den Zügen aller
/// Einheiten, die in der Reihenfolge vor dem Anwender standen. Bei einem
/// Anwender an spätem Rang schützte der Effekt praktisch nur den Rest der
/// eigenen Runde.
///
/// TOD DES ANWENDERS: da die Dauer nur an den Zügen des Anwenders zählt,
/// müsste ein Effekt eines gefallenen Anwenders sonst ewig bestehen bleiben.
/// <see cref="RemoveEffectsRelatedTo"/> (aufgerufen nach jeder Niederlage)
/// beendet deshalb - wie schon vor der Korrektur - ALLE Effekte, deren Quelle
/// ODER Ziel die besiegte Einheit ist, sofort und ohne Dauer-Tick. Diese
/// Regel ist mit der Korrektur nicht mehr nur eine Designentscheidung,
/// sondern notwendige Voraussetzung.
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
            AddEffectAndSnapIfNeeded(state, effect);
            LogApplied(effect, replayEntries, roundNumber);
            return;
        }

        switch (effect.Stacking)
        {
            case StackingRule.Replace:
            case StackingRule.ReplaceForSameSource:
                state.RemoveEffect(existing);
                AddEffectAndSnapIfNeeded(state, effect);
                LogApplied(effect, replayEntries, roundNumber);
                break;

            case StackingRule.RefreshDuration:
                // Keine neue Instanz - nur die Restdauer der bereits
                // bestehenden wird zurückgesetzt, daher kein zusätzlicher
                // "Angewendet"-Eintrag (die erste Anwendung ist bereits
                // protokolliert). Zusätzlich wird ein ggf. bereits
                // abgeklungener Tick-Schaden (siehe DamagePerTickReductionPerTick)
                // auf den vollen Ausgangswert des NEU angewendeten Templates
                // zurückgesetzt - ein erneuter Einsatz (z.B. Elementarmagier-
                // Feuer auf ein bereits brennendes Ziel) "zündet" die
                // Abklingfolge also wieder von vorn an, statt sich über
                // mehrere Einsätze hinweg unbemerkt aufzusummieren oder in
                // einem bereits abgeklungenen Zustand zu verharren.
                existing.SetRemainingDuration(effect.RemainingDuration);
                existing.SetDamagePerTick(effect.DamagePerTick);
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
                AddEffectAndSnapIfNeeded(state, effect);
                LogApplied(effect, replayEntries, roundNumber);
                break;
        }
    }

    /// <summary>
    /// Legt den Effekt an und zieht - falls er die Quelle räumlich an ihr
    /// Ziel bindet (<see cref="ActiveEffect.CausesSourceToFollowTarget"/>,
    /// siehe Hugger) - die Quelle SOFORT auf das aktuelle Feld des Ziels,
    /// statt erst auf dessen NÄCHSTE Bewegung zu warten (Bachelorarbeit-
    /// Anforderung: "die Anheftung soll den Hugger sofort auf das Feld des
    /// Ziels wechseln lassen"). Nur an DIESER einen Stelle nötig: das
    /// spätere Mitziehen bei jeder weiteren Bewegung übernimmt bereits
    /// generisch <see cref="TacticsCore.GameState.MoveUnit"/>.
    /// </summary>
    private static void AddEffectAndSnapIfNeeded(GameState state, ActiveEffect effect)
    {
        state.AddEffect(effect);

        if (!effect.CausesSourceToFollowTarget || effect.AnchorType != EffectAnchorType.Unit || effect.TargetUnitId is not Guid targetId)
        {
            return;
        }

        var source = state.Units.FirstOrDefault(u => u.Id == effect.SourceUnitId);
        var target = state.Units.FirstOrDefault(u => u.Id == targetId);
        if (source is not null && target is not null && source.Position != target.Position)
        {
            state.MoveUnit(source, target.Position);
        }
    }

    private static void LogApplied(ActiveEffect effect, List<ReplayEntry>? replayEntries, int roundNumber)
    {
        replayEntries?.Add(ReplayEntry.ForEffectApplied(roundNumber, effect.SourceUnitId, effect.Name, effect.TargetUnitId, effect.TargetField));
    }

    /// <summary>
    /// Sucht eine bereits aktive, "gleichartige" Effekt-Instanz. Für die
    /// meisten Regeln bedeutet das: gleicher Name, gleicher Anker UND
    /// gleiches Ziel - bei Effekten mit <see cref="ActiveEffect.FieldFollowsSource"/>
    /// zusätzlich derselbe Anwender. <see cref="StackingRule.ReplaceForSameSource"/> ist der
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

        // Effekte, deren Feld ihrem Anwender folgt (EffectTemplate.FieldFollowsSource,
        // z.B. Mauer der Wand), gehören inhaltlich zu genau EINEM Anwender - bei
        // ihnen ist der Anwender Teil der Identität. Zwei Wände auf demselben Feld
        // erzeugen dadurch je ihre eigene Mauer, statt dass die zweite die Mauer der
        // ersten auffrischt (Designentscheidung, siehe DefaultHeroes.Wand). Für alle
        // übrigen Effekte bleibt es bei "gleicher Name, gleiches Ziel" - z.B. stapelt
        // sich der Schildwall zweier Schildträger bewusst NICHT zu 0,25 Aufnahme.
        return state.ActiveEffects.FirstOrDefault(e =>
            e.Name == effect.Name &&
            e.AnchorType == effect.AnchorType &&
            e.TargetUnitId == effect.TargetUnitId &&
            e.TargetField == effect.TargetField &&
            (!(e.FieldFollowsSource || effect.FieldFollowsSource) || e.SourceUnitId == effect.SourceUnitId));
    }

    /// <summary>
    /// Alles, was zu Beginn des Zugs von <paramref name="unit"/> an Effekten
    /// passiert: erst der Schadens-Tick auf <paramref name="unit"/> als ZIEL
    /// (<see cref="TickDamageOverTime"/>), dann der Dauer-Tick ihrer eigenen
    /// Effekte als ANWENDER (<see cref="TickDurationsOfSource"/>) - siehe
    /// Klassendokumentation für den genauen Zeitpunkt und die Begründung.
    /// </summary>
    public static void TickUnitEffects(GameState state, Unit unit, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        TickDamageOverTime(state, unit, replayEntries, roundNumber);
        TickDurationsOfSource(state, unit, replayEntries, roundNumber);
    }

    /// <summary>
    /// SCHADENS-TICK: wendet den Schaden über Zeit aller Effekte an, die auf
    /// <paramref name="target"/> liegen (Zugbeginn des ZIELS, siehe
    /// Klassendokumentation). Berührt die Restdauer bewusst NICHT - die zählt
    /// beim Anwender (<see cref="TickDurationsOfSource"/>). Wo der Schaden
    /// entsteht, hat sich dadurch nicht geändert: weiterhin beim Ziel.
    /// </summary>
    public static void TickDamageOverTime(GameState state, Unit target, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        // Kopie der Liste: während des Tickens (Schaden -> ggf. Niederlage
        // -> Aufräumung) können Einträge aus der Originalliste entfernt
        // werden, was eine laufende Enumeration ungültig machen würde.
        var effects = state.GetEffectsOnUnit(target.Id).Where(e => e.DamagePerTick.HasValue).ToList();

        foreach (var effect in effects)
        {
            if (target.IsDefeated)
            {
                break; // z.B. durch den Tick-Schaden eines vorherigen Effekts bereits besiegt
            }

            replayEntries?.Add(ReplayEntry.ForEffectTicked(roundNumber, effect.Name, effect.TargetUnitId, effect.TargetField));

            int tickDamage = effect.DamagePerTick!.Value;
            CombatSystem.ApplyDamageAndHandleDefeat(state, target, tickDamage,
                damageSource: state.Units.FirstOrDefault(u => u.Id == effect.SourceUnitId)); // nur für die Kampfbilanz

            // Abklingender Schaden über Zeit (Bachelorarbeit-Korrektur,
            // Elementarmagier-Feuer, siehe EffectTemplate.DamagePerTickReductionPerTick):
            // NACH dem soeben zugefügten Tick um den konfigurierten Betrag
            // absenken, nach unten bei 0 begrenzt (nie negativer Schaden).
            if (effect.DamagePerTickReductionPerTick is int reduction)
            {
                effect.SetDamagePerTick(Math.Max(0, tickDamage - reduction));
            }
        }
    }

    /// <summary>
    /// DAUER-TICK: verringert die Restdauer JEDES Effekts mit begrenzter
    /// Dauer, dessen Anwender <paramref name="source"/> ist (Zugbeginn des
    /// ANWENDERS, siehe Klassendokumentation), und entfernt ihn bei 0 -
    /// unabhängig vom Anker (Einheit/Feld) und davon, auf wie viele Einheiten
    /// er wirkt. Effekte ohne Dauer (<see cref="ActiveEffect.RemainingDuration"/>
    /// = null) bleiben unberührt.
    ///
    /// Zwei Effekt-Instanzen desselben Namens und Anwenders (z.B. die
    /// gekoppelte Feldsperre + Selbstreduktion der Wand) enden über
    /// <see cref="RemoveEffectAndLinkedCounterpart"/> gemeinsam - sobald die
    /// erste abläuft, ist die zweite bereits entfernt und wird hier
    /// übersprungen (statt ein zweites Mal getickt zu werden).
    /// </summary>
    public static void TickDurationsOfSource(GameState state, Unit source, List<ReplayEntry>? replayEntries = null, int roundNumber = 0)
    {
        var effects = state.ActiveEffects
            .Where(e => e.SourceUnitId == source.Id && e.RemainingDuration.HasValue)
            .ToList();

        foreach (var effect in effects)
        {
            if (!state.ActiveEffects.Contains(effect))
            {
                continue; // bereits als gekoppelter Gegenpart eines soeben abgelaufenen Effekts entfernt
            }

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
    /// BEHEBUNG EINER REGELLÜCKE (Bachelorarbeit, Befund): Ein angehefteter
    /// Held (Quelle eines Effekts mit <see cref="ActiveEffect.CausesSourceToFollowTarget"/>,
    /// derzeit nur Huggers Umklammerung) ist für Gegner nicht angreifbar, kann
    /// selbst nicht angreifen und löst sich nur aktiv. War er die LETZTE lebende
    /// Einheit seines Teams, konnte die Partie deshalb nicht mehr enden - sie
    /// stand bis zum Rundenlimit still (in der Stichprobe z.B. 71 von 82
    /// Rundenlimit-Partien des aggressiven Schwellen-Agenten).
    ///
    /// REGEL: Ist ein angehefteter Held der letzte lebende Held seines Teams,
    /// löst sich die Anheftung sofort (samt gekoppelter Instanz, siehe
    /// <see cref="RemoveEffectAndLinkedCounterpart"/>) - danach ist er angreifbar
    /// und kann angreifen. Ergänzend kann der letzte Held keine anheftende
    /// Fähigkeit mehr einsetzen (siehe AbilitySystem), sonst entstünde das Patt
    /// im nächsten Zug neu.
    ///
    /// ZEITPUNKT: aufgerufen nach JEDER Niederlage (CombatSystem.HandleDefeat),
    /// also im selben Moment, in dem die vorletzte Einheit fällt - nicht erst zu
    /// Beginn seines nächsten Zugs, sonst bliebe er bis dahin unangreifbar.
    ///
    /// KLONE zählen nicht als weitere Einheit - wie bei der Siegbedingung.
    /// Ein Klon kann ohnehin nie die letzte weitere Einheit sein: er verschwindet
    /// mit seinem Erzeuger (CombatSystem.DespawnCloneOfCreator).
    ///
    /// SYMMETRIE: geprüft wird jedes Team für sich, die Reihenfolge ist ohne
    /// Einfluss (jedes Lösen betrifft nur die Effekte seines eigenen Helden).
    /// Dass BEIDE Teams nur noch aus einem angehefteten Helden bestehen, ist
    /// ohnehin nicht möglich: eine Anheftung endet mit dem Tod ihres Ziels, der
    /// letzte angeheftete Held hängt also stets an einer lebenden gegnerischen
    /// Einheit - und ein angehefteter Held kann selbst nicht angeheftet werden,
    /// weil er für Gegner kein gültiges Ziel ist.
    ///
    /// NACHTRAG (Hugger-Überarbeitung): angeheftet darf der Hugger inzwischen
    /// angreifen und wird dadurch eine Runde lang angreifbar. Die Regel bleibt
    /// trotzdem nötig: ein angehefteter letzter Held, der NICHT angreift, wäre sonst
    /// weiterhin dauerhaft unangreifbar.
    ///
    /// Die Anheftung im normalen Spielverlauf bleibt unverändert. Ein Replay-
    /// Eintrag entsteht (wie beim übrigen Tod-Aufräumen) nicht.
    /// </summary>
    public static void ReleaseAttachmentsOfLastHeroes(GameState state)
    {
        foreach (var team in state.Units.Where(u => !u.IsDefeated).Select(u => u.Team).Distinct().ToList())
        {
            var heroes = state.Units.Where(u => !u.IsDefeated && !u.IsClone && u.Team == team).ToList();
            if (heroes.Count != 1)
            {
                continue;
            }

            foreach (var attachment in state.ActiveEffects
                .Where(e => e.CausesSourceToFollowTarget && e.SourceUnitId == heroes[0].Id)
                .ToList())
            {
                RemoveEffectAndLinkedCounterpart(state, attachment, replayEntries: null, roundNumber: 0);
            }
        }
    }

    /// <summary>Ob <paramref name="unit"/> der letzte lebende Held (ohne Klone) seines Teams ist.</summary>
    public static bool IsLastHeroOfTeam(Units.Unit unit, GameState state) =>
        !unit.IsClone && !state.Units.Any(u => u.Id != unit.Id && !u.IsDefeated && !u.IsClone && u.Team == unit.Team);

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
    /// Seit die Dauer beider Instanzen am selben Anwender hängt (siehe
    /// Klassendokumentation), zählen sie im selben Dauer-Tick herunter und
    /// laufen ohnehin gleichzeitig ab - die Kopplung bleibt trotzdem
    /// erforderlich, weil Zieltod, aktives Beenden (<see cref="ActiveEffect"/>
    /// per CancelEffectName) oder ein einseitiges Auffrischen nur EINE der
    /// beiden Instanzen direkt treffen und die andere sonst bestehen bliebe.
    /// Diese EINE zentrale Stelle (aufgerufen sowohl bei natürlichem Ablauf
    /// als auch beim Tod-Aufräumen) sorgt generisch dafür, dass beide
    /// Instanzen immer gemeinsam enden - unabhängig davon, WARUM die erste
    /// entfernt wurde, und ohne Held-spezifische Sonderprüfung an mehreren
    /// Stellen im Code.
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
