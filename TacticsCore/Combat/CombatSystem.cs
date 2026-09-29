using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore.Combat;

/// <summary>
/// Eigene, von <see cref="Unit"/> getrennte Kampflogik: Angriffsmuster,
/// Zielermittlung und die Ausführung eines Grundangriffs inklusive
/// Sichtlinien-Umleitung, Schadensreduktion, Niederlage und Entfernen vom
/// Spielfeld.
///
/// ZENTRALE SCHADENS-/NIEDERLAGE-BEHANDLUNG: Jeglicher Schaden in
/// TacticsCore - ob aus einem Grundangriff, einem Fähigkeitseffekt oder
/// einem Schaden-über-Zeit-Tick - läuft durch genau EINE der beiden
/// <see cref="ApplyDamageAndHandleDefeat(GameState, Unit, int)"/>-
/// Überladungen. Das stellt sicher, dass Schadensreduktion, Niederlage,
/// Entfernen aus dem GameState, das Aufräumen zugehöriger Effekte
/// (<see cref="EffectSystem.RemoveEffectsRelatedTo"/>) und das
/// Verschwinden eines evtl. vorhandenen Klons (Anforderung 9) IMMER
/// konsistent ablaufen, unabhängig von der Schadensquelle - es gibt keine
/// zweite, abweichende Stelle im Code, an der eine Einheit "sterben" kann.
/// </summary>
public static class CombatSystem
{
    /// <summary>
    /// Liefert die Felder, die vom Angriffsmuster einer Einheit abgedeckt
    /// werden: alle existierenden Spielfelder mit Hex-Distanz
    /// &lt;= der EFFEKTIVEN Angriffsreichweite (siehe
    /// <see cref="EffectiveStats.AttackRange"/> - berücksichtigt die
    /// individuelle Reichweite der Einheit bzw. eines Helden sowie aktive
    /// Reichweiten-Modifikatoren). Beim Standard-Radius 1 sind das das
    /// eigene Feld PLUS alle sechs direkt angrenzenden Felder.
    ///
    /// Bewusst über die reine Hex-Distanz und NICHT über eine Breitensuche
    /// bestimmt: Anders als bei der Bewegung geht es beim Angriff nicht um
    /// erreichbare Wege, sondern rein geometrisch darum, wie nah ein Feld
    /// am Angreifer liegt. Bewusst wiederverwendbar für spätere
    /// Flächenfähigkeiten (siehe <see cref="GetValidAttackTargets"/>).
    /// </summary>
    public static IReadOnlyList<HexCoordinate> GetAttackPatternFields(Unit attacker, GameState state) =>
        state.Board.Fields
            .Select(f => f.Coordinate)
            .Where(c => HexCoordinate.Distance(attacker.Position, c) <= EffectiveStats.AttackRange(attacker, state))
            .ToList();

    /// <summary>
    /// Liefert alle aktuell gültigen Angriffsziele - einzelne Einheiten,
    /// nicht Felder - für einen Grundangriff von <paramref name="attacker"/>.
    /// Ein Ziel ist gültig, wenn es auf einem Feld des Angriffsmusters
    /// steht, nicht besiegt ist und (sofern
    /// <see cref="CombatRules.RequireEnemyTarget"/> aktiv ist) einem
    /// anderen Team angehört als der Angreifer. Ein Klon kann selbst NICHT
    /// angreifen (Anforderung 9) - für ihn liefert diese Methode immer eine
    /// leere Liste. Stabil sortiert nach (q, r, UnitId).
    ///
    /// WICHTIG - Angriffe werden durch andere Einheiten NICHT blockiert: es
    /// gibt keine Sichtlinien-Prüfung für die ZIEL-GÜLTIGKEIT. Eine
    /// Sichtlinien-Blockade (Anforderung 8) ändert stattdessen erst beim
    /// tatsächlichen Ausführen (<see cref="TryAttack"/>), WER den Schaden
    /// abbekommt (Umleitung), nicht OB ein Ziel wählbar ist.
    /// </summary>
    public static IReadOnlyList<Unit> GetValidAttackTargets(Unit attacker, GameState state)
    {
        if (attacker.IsDefeated || attacker.IsClone)
        {
            return Array.Empty<Unit>();
        }

        // Generische Angriffssperre (siehe EffectTemplate.PreventsAttacking,
        // z.B. Hugger während der Anheftung: "kann selbst nicht angreifen").
        if (EffectiveStats.IsAttackingPrevented(attacker, state))
        {
            return Array.Empty<Unit>();
        }

        return GetAttackPatternFields(attacker, state)
            .SelectMany(state.GetUnitsAt)
            .Where(u => !u.IsDefeated)
            .Where(u => !CombatRules.RequireEnemyTarget || u.Team != attacker.Team)
            // Generische Unangreifbarkeit gegenüber Gegnern (siehe
            // EffectTemplate.PreventsBeingTargetedByEnemies, z.B. Hugger
            // während der Anheftung: "kein gültiges Angriffsziel für
            // Gegner") - team-relativ, betrifft daher nie ein Ziel aus dem
            // eigenen Team.
            .Where(u => !EffectiveStats.PreventsBeingTargetedBy(u, attacker.Team, state))
            .OrderBy(u => u.Position.Q)
            .ThenBy(u => u.Position.R)
            .ThenBy(u => u.Id)
            .ToList();
    }

    /// <summary>Prüft, ob <paramref name="target"/> aktuell ein gültiges Angriffsziel für <paramref name="attacker"/> ist.</summary>
    public static bool CanAttack(Unit attacker, Unit target, GameState state) =>
        GetValidAttackTargets(attacker, state).Contains(target);

    /// <summary>
    /// Führt einen Grundangriff von <paramref name="attacker"/> auf genau
    /// EIN gewähltes Ziel aus. Stehen mehrere Einheiten auf dem Zielfeld,
    /// entscheidet allein <paramref name="target"/>, welche davon
    /// getroffen wird. Ein ungültiger Angriff ändert weder HP noch Position
    /// irgendeiner Einheit.
    ///
    /// Ablauf des tatsächlichen Treffers:
    ///   1. Grundschaden = effektiver Angriffsschaden des Angreifers
    ///      (<see cref="EffectiveStats.AttackDamage"/> - berücksichtigt
    ///      Buffs/Debuffs und passive Fähigkeiten wie den Berserker).
    ///   2. Ein evtl. vorhandener Einmal-Bonus für den nächsten Angriff
    ///      ("aufladender Eigenzustand", Anforderung 8) wird addiert und
    ///      verbraucht (entfernt) - unabhängig vom finalen Ziel.
    ///   3. Sichtlinien-Umleitung (Anforderung 8): verläuft die Linie vom
    ///      Angreifer zum gewählten Ziel durch ein Feld mit aktiver
    ///      Blockade-Wirkung, trifft stattdessen die Einheit, die diese
    ///      Blockade gesetzt hat (das NÄCHSTGELEGENE blockierende Feld auf
    ///      der Linie entscheidet - deterministisch, da <see cref="HexLine"/>
    ///      eine feste, reproduzierbare Feldfolge liefert).
    ///   4. Schadensreduktion des TATSÄCHLICHEN Empfängers (z.B.
    ///      Schildträger-Effekt) wird angewendet.
    ///   5. Zentrale Schadens-/Niederlage-Behandlung (siehe Klassendoku).
    /// </summary>
    public static bool TryAttack(Unit attacker, Unit target, GameState state) =>
        TryAttackDetailed(attacker, target, state, out _, out _);

    /// <summary>
    /// Wie <see cref="TryAttack"/>, liefert zusätzlich über die
    /// <c>out</c>-Parameter, WER den Schaden tatsächlich abbekommen hat
    /// (bei einer Sichtlinien-Umleitung nicht zwangsläufig
    /// <paramref name="target"/>) und WIE VIEL Schaden es nach allen
    /// Modifikatoren (Angreifer-Bonus, Zielreduktion) tatsächlich war.
    /// Wird vom <see cref="Match.MatchController"/> für ein akkurates
    /// Replay-Protokoll genutzt - <see cref="TryAttack"/> selbst bleibt der
    /// einfache, für die meisten Aufrufer (auch <see cref="Unit.TryAttack"/>)
    /// ausreichende Fall.
    /// </summary>
    public static bool TryAttackDetailed(Unit attacker, Unit target, GameState state, out Unit actualTarget, out int damageDealt)
    {
        if (!CanAttack(attacker, target, state))
        {
            actualTarget = target;
            damageDealt = 0;
            return false;
        }

        int damage = EffectiveStats.AttackDamage(attacker, state);

        if (EffectiveStats.FindNextAttackBonus(attacker, state) is { } bonusEffect)
        {
            damage += bonusEffect.BonusDamageForNextAttack!.Value;
            state.RemoveEffect(bonusEffect);
        }

        (actualTarget, damageDealt) = ApplyDamageAndHandleDefeat(state, attacker, target, damage);
        ExposeAttackerIfProtected(state, attacker);
        return true;
    }

    /// <summary>
    /// Designentscheidung "Hugger-Überarbeitung", generisch umgesetzt: trägt der
    /// Angreifer eine Unangreifbarkeit, die ein eigener Angriff aufhebt
    /// (<see cref="EffectTemplate.TargetingProtectionSuspendedByAttackRounds"/>),
    /// erhält er einen Markereffekt "Exponiert" mit dieser Dauer. Die Dauer zählt
    /// zu Beginn seiner eigenen Züge herunter (siehe EffectSystem.TickDurationsOfSource):
    /// Dauer 1 = angreifbar bis zum Beginn seines nächsten eigenen Zugs, also genau
    /// eine Runde. Ein erneuter Angriff frischt den Marker auf. Der Schutzeffekt
    /// selbst bleibt bestehen. Kein Replay-Eintrag für das Anlegen (das Kampfsystem
    /// führt kein Protokoll); das Ablaufen protokolliert der Dauer-Tick wie üblich.
    /// </summary>
    private static void ExposeAttackerIfProtected(GameState state, Unit attacker)
    {
        if (attacker.IsDefeated)
        {
            return;
        }

        var rounds = state.GetEffectsOnUnit(attacker.Id)
            .Where(e => e.PreventsBeingTargetedByEnemies && e.TargetingProtectionSuspendedByAttackRounds is not null)
            .Select(e => e.TargetingProtectionSuspendedByAttackRounds!.Value)
            .DefaultIfEmpty(0)
            .Max();
        if (rounds > 0)
        {
            EffectSystem.Apply(state, ActiveEffect.OnUnit(attacker, attacker, new EffectTemplate(
                Name: ExposedEffectName, IsInstant: false, Duration: rounds, Stacking: StackingRule.RefreshDuration, SuspendsTargetingProtection: true)));
        }
    }

    /// <summary>Name des Markereffekts - bewusst NICHT der Name des Schutzeffekts, sonst würde ihn die Kopplung gleichnamiger Instanzen (EffectSystem.RemoveEffectAndLinkedCounterpart) mitentfernen.</summary>
    public const string ExposedEffectName = "Exponiert";

    /// <summary>
    /// Zentrale Schadens-/Niederlage-Behandlung OHNE Sichtlinien-Umleitung -
    /// für Fälle ohne eindeutigen, positionsbehafteten Verursacher (z.B.
    /// ein Schaden-über-Zeit-Tick, dessen Quelle ggf. weit entfernt oder
    /// nicht mehr relevant ist). Wendet weiterhin die Schadensreduktion des
    /// Ziels an und löst bei Niederlage dieselbe Aufräum-Kaskade aus wie
    /// die andere Überladung (siehe Klassendokumentation).
    /// </summary>
    /// <param name="damageSource">Nur für die Kampfbilanz (<see cref="GameState.CombatStatistics"/>): Verursacher, z.B. Anwender eines Schadens über Zeit oder die vergeltende Wand; ohne Einfluss auf den Schaden.</param>
    public static void ApplyDamageAndHandleDefeat(GameState state, Unit target, int amount, Unit? damageSource = null)
    {
        int reduced = ApplyIncomingReduction(target, amount, state);
        int hitPointsBefore = target.CurrentHitPoints;
        target.ApplyDamage(reduced);
        state.RecordDamage(damageSource, target, hitPointsBefore - target.CurrentHitPoints);
        if (target.IsDefeated)
        {
            HandleDefeat(state, target);
        }
    }

    /// <summary>
    /// Wie <see cref="ApplyDamageAndHandleDefeat(GameState, Unit, int)"/>,
    /// zusätzlich mit Sichtlinien-Umleitung ausgehend von
    /// <paramref name="source"/> (siehe Anforderung 8). Liefert den
    /// tatsächlichen Empfänger (nach Umleitung) und den tatsächlich
    /// angewendeten Schaden (nach Reduktion) zurück - wichtig, damit
    /// Aufrufer wie <see cref="TryAttackDetailed"/> ein akkurates
    /// Replay-Protokoll führen können, statt den rohen, ggf. durch
    /// Modifikatoren längst überholten Ausgangswert zu loggen.
    ///
    /// VERGELTUNGSSCHADEN (Bachelorarbeit-Korrektur, Wand-Rework, siehe
    /// <see cref="Effects.EffectTemplate.ReflectsFractionOfRedirectedDamage"/>):
    /// wurde der Angriff tatsächlich umgeleitet UND trägt der dafür
    /// verantwortliche Feld-Effekt einen Vergeltungs-Anteil, erhält
    /// <paramref name="source"/> im Anschluss einen Bruchteil des
    /// URSPRÜNGLICHEN, noch UNREDUZIERTEN Schadens zurück - bewusst NICHT
    /// des bereits um die eigene Schadensreduktion gekürzten Werts, damit
    /// sich beide Wirkungen der Wand (Selbstreduktion UND Vergeltung)
    /// UNABHÄNGIG voneinander addieren: bei 12 eingehendem Schaden und
    /// jeweils 50% nimmt sie 6 (Selbstreduktion) UND gibt 6 zurück
    /// (Vergeltung auf Basis der ursprünglichen 12) - nicht nur 3 (50% von
    /// bereits reduzierten 6), was ihre Vergeltung sonst unbeabsichtigt an
    /// die eigene Zähigkeit gekoppelt hätte. Läuft über dieselbe zentrale
    /// <see cref="ApplyDamageAndHandleDefeat(GameState, Unit, int)"/>-
    /// Überladung wie jeder andere nicht-Angriffs-Schaden (kein Zwei-Wege-
    /// Redirect möglich, kein Sonderfall für die Aufräum-Kaskade nötig).
    /// </summary>
    public static (Unit ActualTarget, int DamageApplied) ApplyDamageAndHandleDefeat(GameState state, Unit source, Unit target, int amount)
    {
        var (actualTarget, redirectEffect) = ResolveRedirect(state, source, target);
        int reduced = ApplyIncomingReduction(actualTarget, amount, state);
        int hitPointsBefore = actualTarget.CurrentHitPoints;
        actualTarget.ApplyDamage(reduced);
        state.RecordDamage(source, actualTarget, hitPointsBefore - actualTarget.CurrentHitPoints);
        if (actualTarget.IsDefeated)
        {
            HandleDefeat(state, actualTarget);
        }

        if (redirectEffect?.ReflectsFractionOfRedirectedDamage is double fraction && source.Id != actualTarget.Id && !source.IsDefeated)
        {
            int retaliation = (int)Math.Round(amount * fraction);
            if (retaliation > 0)
            {
                ApplyDamageAndHandleDefeat(state, source, retaliation, damageSource: actualTarget);
            }
        }

        return (actualTarget, reduced);
    }

    /// <summary>
    /// Prüft, ob die Sichtlinie von <paramref name="source"/> zu
    /// <paramref name="target"/> durch ein Feld mit aktiver
    /// Angriffs-Umleitung verläuft, und liefert in diesem Fall die Einheit,
    /// die den Angriff stattdessen abbekommt.
    ///
    /// Geprüft werden alle Felder der Sichtlinie AUSSER dem Feld, auf dem
    /// der ANGREIFER selbst steht - das ZIELFELD zählt dabei IMMER mit
    /// (siehe <see cref="HexLine.Between"/>, nicht <see cref="HexLine.StrictlyBetween"/>):
    /// die Wand-Fähigkeit verlangt ausdrücklich, dass die Umleitung auch für
    /// Ziele gilt, die auf DEMSELBEN Feld wie die Wand stehen (z.B. ein
    /// Verbündeter der Wand, der sich das Feld mit ihr teilt). Stehen
    /// Angreifer UND Ziel beide auf dem blockierten Feld (Distanz 0), wird
    /// genau dieses eine Feld geprüft - hier gibt es kein "Dazwischen", aber
    /// das Zielfeld selbst ist ja ohnehin das fragliche Feld. Zielt der
    /// Angriff direkt auf die blockierende Einheit selbst, hat eine
    /// "Umleitung auf sich selbst" keine Wirkung (wird unten übersprungen).
    ///
    /// NUR GEGNER (Bachelorarbeit-Designentscheidung): umgeleitet werden nur
    /// Angriffe, deren Angreifer einem ANDEREN Team angehört als die Einheit,
    /// die die Umleitung gesetzt hat. Vorher wurde das Team nicht geprüft:
    /// Verbündete der Wand, die einen Gegner auf ihrem Feld angriffen oder durch
    /// ihr Feld schossen, trafen die eigene Wand und erhielten die Vergeltung
    /// (Stichprobe: in rund einem Viertel der Partien mit Wand), und die Wand
    /// traf sich bei einem Angriff auf einen Gegner auf ihrem eigenen Feld
    /// selbst. Die Teamprüfung behebt beide Fälle.
    ///
    /// Liefert neben dem tatsächlichen Ziel auch den dafür verantwortlichen
    /// Feld-Effekt zurück (<c>null</c>, wenn keine Umleitung stattfand) -
    /// der Aufrufer braucht ihn, um einen ggf. konfigurierten
    /// Vergeltungsschaden auszuwerten (siehe
    /// <see cref="Effects.EffectTemplate.ReflectsFractionOfRedirectedDamage"/>).
    /// </summary>
    private static (Unit ActualTarget, ActiveEffect? RedirectEffect) ResolveRedirect(GameState state, Unit source, Unit target)
    {
        var fieldsToCheck = source.Position == target.Position
            ? new[] { target.Position }
            : HexLine.Between(source.Position, target.Position).Skip(1).ToArray();

        foreach (var field in fieldsToCheck)
        {
            foreach (var blocker in state.GetEffectsOnField(field))
            {
                if (!blocker.RedirectsAttacksToSource)
                {
                    continue;
                }

                // NUR ANGRIFFE VON GEGNERN werden umgeleitet (Designentscheidung,
                // siehe Klassendokumentation von DefaultHeroes.Wand): Angriffe von
                // Verbündeten des Umleitungsziels - und von ihm selbst - gehen an ihr
                // eigentliches Ziel und lösen keine Vergeltung aus.
                var redirectUnit = state.Units.FirstOrDefault(u => u.Id == blocker.SourceUnitId);
                if (redirectUnit is not null && !redirectUnit.IsDefeated && redirectUnit.Id != target.Id && redirectUnit.Team != source.Team)
                {
                    return (redirectUnit, blocker); // nächstgelegenes blockierendes Feld auf der Linie gewinnt
                }
            }
        }

        return (target, null);
    }

    private static int ApplyIncomingReduction(Unit target, int amount, GameState state)
    {
        double multiplier = EffectiveStats.IncomingDamageMultiplier(target, state);
        return Math.Max(0, (int)Math.Round(amount * multiplier));
    }

    /// <summary>
    /// Vollständige Aufräum-Kaskade nach einer Niederlage: Entfernen aus
    /// dem GameState, Aufräumen aller mit dieser Einheit verbundenen
    /// Effekte (Anforderung 10: "Effekte enden, wenn Quelle oder Ziel
    /// besiegt wird"), und - falls die besiegte Einheit selbst Erzeuger
    /// eines Klons war - dessen Verschwinden (Anforderung 9: "Der Klon
    /// verschwindet, wenn sein Erzeuger besiegt wird"). Der Klon wird dabei
    /// selbst als besiegt markiert (HP auf 0, nicht nur stillschweigend aus
    /// der Buchführung entfernt) - so bleibt die Invariante "jede aus
    /// GameState.Units entfernte Einheit ist IsDefeated" überall erhalten,
    /// insbesondere für die "vor dem eigenen Zug besiegt"-Prüfung im
    /// MatchController, die sonst eine bereits entfernte, aber technisch
    /// nicht besiegte Einheit fälschlich noch handeln lassen würde.
    /// </summary>
    private static void HandleDefeat(GameState state, Unit unit)
    {
        state.RemoveUnit(unit);
        EffectSystem.RemoveEffectsRelatedTo(state, unit.Id);
        DespawnCloneOfCreator(state, unit);
        EffectSystem.ReleaseAttachmentsOfLastHeroes(state); // Behebung der Regellücke "angehefteter letzter Held"
    }

    private static void DespawnCloneOfCreator(GameState state, Unit creator)
    {
        var clone = state.Units.FirstOrDefault(u => u.IsClone && u.CreatorId == creator.Id);
        if (clone is null)
        {
            return;
        }

        clone.ApplyDamage(clone.MaxHitPoints);
        state.RemoveUnit(clone);
        EffectSystem.RemoveEffectsRelatedTo(state, clone.Id);
    }
}
