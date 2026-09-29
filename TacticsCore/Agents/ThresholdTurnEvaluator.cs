using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Ergebnis einer Zugbewertung des Schwellen-Agenten, aufgeschlüsselt. Die
/// Lebenspunkt-Größen stehen in ROHEN Lebenspunkten; die Umrechnung in Treffer
/// (<see cref="ThresholdHeuristicSettings.HitPointUnit"/>) geschieht erst in
/// <see cref="Weighted"/>.
/// </summary>
/// <param name="Damage">D: unmittelbar verursachter Schaden (verlorene Lebenspunkte gegnerischer Einheiten, auch durch Fähigkeiten).</param>
/// <param name="HitPoints">H: Änderung der eigenen Lebenspunkte (Heilung positiv, erlittener Schaden und Verluste negativ, neue Klone positiv).</param>
/// <param name="ProtectionGain">Zunahme des abgefangenen Anteils der tatsächlich bestehenden Bedrohung (Schildwall, Mauer); zählt mit w_H wie erhaltene Lebenspunkte.</param>
/// <param name="DistanceGain">P: um wie viele Felder die handelnde Einheit dem nächsten Gegner näher kommt.</param>
/// <param name="Defeated">K: unmittelbar besiegte gegnerische Helden.</param>
/// <param name="Retreat">R: Verringerung des Schadens, der die Gefahrenschwelle übersteigt (über alle eigenen Helden).</param>
public sealed record ThresholdTurnScore(
    double Damage,
    double HitPoints,
    double ProtectionGain,
    double DistanceGain,
    int Defeated,
    double Retreat)
{
    /// <summary>U = w_D·D + w_H·(H + Schutz) + w_P·P + w_K·K + w_R·R, Lebenspunkt-Größen in Treffern.</summary>
    public double Weighted(ThresholdWeights weights)
    {
        double unit = ThresholdHeuristicSettings.HitPointUnit;
        return weights.Damage * Damage / unit
             + weights.HitPoints * (HitPoints + ProtectionGain) / unit
             + ThresholdHeuristicSettings.DistanceGainWeight * DistanceGain
             + ThresholdHeuristicSettings.DefeatWeight * Defeated
             + ThresholdHeuristicSettings.RetreatWeight * Retreat / unit;
    }
}

/// <summary>
/// ZWEITE, SCHLANKERE BEWERTUNGSFUNKTION (Bachelorarbeit) - Grundlage des
/// <see cref="ThresholdHeuristicAgent"/>. Die bisherige Funktion
/// (<see cref="TurnEvaluator"/>) bleibt unverändert bestehen; beide werden als
/// Befund zur Modellabhängigkeit gegenübergestellt.
///
///   U = w_D·D + w_H·H + w_P·P + w_K·K + w_R·R
///
/// Bewertet wird wie bisher der Zustand NACH dem Zug im Vergleich zu davor,
/// ohne Folgezüge und ohne gegnerische Antworten; Passen (keine Änderung)
/// ergibt 0.
///
/// UNTERSCHIEDE ZUR BISHERIGEN FUNKTION:
///   - Keine Summe über Paare aus eigener Einheit und Gegner: jeder Gegner zählt
///     in der Teambedrohung genau EINMAL (<see cref="TeamThreat"/>) - die
///     Mehrfachzählung war die Hauptursache der Passivität des bisherigen
///     defensiven Agenten.
///   - Die Bedrohung bremst nicht linear, sondern erst oberhalb einer
///     GEFAHRENSCHWELLE (<see cref="ThresholdHeuristicSettings.DangerThreshold"/>,
///     relativ zu den Lebenspunkten der Einheit). Darunter ist sie neutral, und
///     der Distanzgewinn P entscheidet - Heranrücken ist dort immer ein kleiner
///     Vorteil. Darüber wird Zurückweichen belohnt (R).
///   - Statt Angriffsmöglichkeit, Besiegt-Bonus aus Schadenswerten und
///     angekündigten Wirkungen nur noch fünf unmittelbar sichtbare Größen.
///     Angekündigte Wirkungen (Schaden über Zeit, Buffs, Schwächungen, Aufladen)
///     bewertet diese Funktion NICHT - bewusste Vereinfachung, siehe unten.
///
/// DIE TERME:
///   - D: verlorene Lebenspunkte aller gegnerischen Einheiten in diesem Zug
///     (Angriff und Fähigkeitsschaden).
///   - H: Änderung der Lebenspunkte aller eigenen Einheiten (Heilung,
///     Vergeltung, Verluste; ein neuer Klon zählt mit seinen Lebenspunkten).
///     Da die Lebenspunkte vor dem Zug für alle Zugfolgen gleich sind, ergibt die
///     Änderung dieselbe Rangfolge wie die verbleibenden Lebenspunkte selbst,
///     mit Passen = 0 als Bezugspunkt. Zu H zählt außerdem der SCHUTZ: der
///     Anteil der tatsächlich bestehenden Bedrohung, den Schadensreduktion und
///     Umleitung abfangen - je Gegner einmal, gegen das am wenigsten geschützte
///     eigene Ziel, das er erreicht (ein Gegner greift einmal an und wählt das
///     ungeschützte Ziel). Ohne Gegner in Reichweite ist Schutz 0 wert.
///   - P: Verringerung der Hex-Distanz der handelnden Einheit zum nächsten
///     ANGREIFBAREN gegnerischen Helden. (Erste Fassung: zum nächsten Helden
///     überhaupt - ein angehefteter, nicht angreifbarer Hugger auf dem Feld einer
///     eigenen Einheit war dann oft der nächste Gegner, und die Einheiten rückten
///     nicht mehr vor: in der Stichprobe Grund für 68 % der Passzüge des
///     aggressiven Schwellen-Agenten.)
///   - K: Zahl der in diesem Zug besiegten gegnerischen Helden.
///   - R: für jeden eigenen Helden der ÜBERSCHUSS E = max(0, T − s·HP), T =
///     Summe der Schadenswerte der Gegner, die ihn erreichen (jeder Gegner
///     einmal), s = Gefahrenschwelle, HP = seine Lebenspunkte. R = Summe der
///     Überschüsse vorher − nachher: positiv, wenn der Zug die Gefahr über der
///     Schwelle senkt (Zurückweichen, Heilen, Besiegen eines Gegners), negativ,
///     wenn er sie erhöht. Unterhalb der Schwelle ist R immer 0. T ist
///     bewusst OHNE Schadensreduktion gerechnet - R bewertet die Lage, Schutz
///     ist bereits in H enthalten.
///
/// Einheiten, die in DIESEM Zug besiegt werden, zählen bei Schutz, Distanz und
/// Überschuss in keinem der beiden Zustände (sonst erschiene ihr Wegfall als
/// Schutz- oder Distanzverlust bzw. als Gefahrenrückgang); ihr Verlust steckt in
/// D, K bzw. H.
///
/// BEWUSSTE VEREINFACHUNGEN UND GRENZEN:
///   - Angekündigte Wirkungen zählen nicht: ein Buff (Kampflied), eine Schwächung
///     oder Aufladen ist nur so viel wert, wie er im selben Zug bewirkt - also
///     meist nichts. Solche Fähigkeiten setzt der Agent nur ein, wenn nichts
///     anderes positiv ist, und dann gar nicht (Passen gewinnt bei 0).
///   - Reichweite wie bisher: Hex-Distanz ≤ Bewegungs- + Angriffsreichweite; gesperrte
///     Felder und Wege werden nicht geprüft.
///   - Die Wahl des Gegners (welches Ziel er angreift) wird für den Schutz als
///     "das am wenigsten geschützte" angenommen, für R als "jeder Gegner kann
///     jede erreichbare Einheit angreifen" - das ist für jede einzelne Einheit
///     der ungünstigste Fall, für das Team keine Mehrfachzählung, weil R je
///     Einheit an deren eigener Schwelle gemessen wird.
/// </summary>
public static class ThresholdTurnEvaluator
{
    /// <summary>Bewertet den Übergang <paramref name="before"/> → <paramref name="after"/> aus Sicht von <paramref name="team"/>; <paramref name="actorId"/> ist die handelnde Einheit (für P).</summary>
    public static ThresholdTurnScore Evaluate(GameState before, GameState after, Team team, Guid actorId)
    {
        var aliveAfter = after.Units.Where(u => !u.IsDefeated).Select(u => u.Id).ToHashSet();
        var defeatedThisTurn = before.Units.Where(u => !u.IsDefeated && !aliveAfter.Contains(u.Id)).Select(u => u.Id).ToHashSet();

        double damage = 0, hitPoints = 0;
        int defeated = 0;
        var hpAfter = after.Units.Where(u => !u.IsDefeated).ToDictionary(u => u.Id, u => u.CurrentHitPoints);
        foreach (var unit in before.Units.Where(u => !u.IsDefeated))
        {
            int lost = unit.CurrentHitPoints - hpAfter.GetValueOrDefault(unit.Id, 0);
            if (unit.Team == team)
            {
                hitPoints -= lost;
            }
            else
            {
                damage += lost;
                if (!unit.IsClone && defeatedThisTurn.Contains(unit.Id))
                {
                    defeated++;
                }
            }
        }

        var existedBefore = before.Units.Where(u => !u.IsDefeated).Select(u => u.Id).ToHashSet();
        foreach (var unit in after.Units.Where(u => !u.IsDefeated && u.Team == team && !existedBefore.Contains(u.Id)))
        {
            hitPoints += unit.CurrentHitPoints; // neu entstandene eigene Einheit (Klon)
        }

        return new ThresholdTurnScore(
            Damage: damage,
            HitPoints: hitPoints,
            ProtectionGain: PreventedThreat(after, team, defeatedThisTurn) - PreventedThreat(before, team, defeatedThisTurn),
            DistanceGain: DistanceToNearestEnemy(before, actorId, defeatedThisTurn) - DistanceToNearestEnemy(after, actorId, defeatedThisTurn),
            Defeated: defeated,
            Retreat: ExcessDanger(before, team, defeatedThisTurn) - ExcessDanger(after, team, defeatedThisTurn));
    }

    /// <summary>
    /// Teambedrohung OHNE Mehrfachzählung: Summe der effektiven Schadenswerte
    /// aller gegnerischen Helden, die MINDESTENS EINEN eigenen Helden im nächsten
    /// Zug erreichen können - jeder Gegner genau einmal.
    /// </summary>
    public static double TeamThreat(GameState state, Team team) =>
        EnemyThreats(state, team, new HashSet<Guid>()).Sum(t => t.Damage);

    /// <summary>
    /// Je Gegner, der mindestens einen eigenen Helden erreicht: sein Schaden und
    /// der größte Schadensmultiplikator unter den erreichbaren eigenen Helden
    /// (das am wenigsten geschützte Ziel).
    /// </summary>
    private static IEnumerable<(double Damage, double WorstMultiplier)> EnemyThreats(GameState state, Team team, IReadOnlySet<Guid> excluded)
    {
        var own = OwnHeroes(state, team, excluded).ToList();
        foreach (var enemy in Attackers(state, team, excluded))
        {
            double? worst = null;
            foreach (var target in own)
            {
                if (CanReach(enemy, target, state))
                {
                    worst = Math.Max(worst ?? double.MinValue, EffectiveMultiplier(target, state));
                }
            }

            if (worst is double m)
            {
                yield return (EffectiveStats.AttackDamage(enemy, state), m);
            }
        }
    }

    /// <summary>Abgefangener Anteil der Teambedrohung: Σ Schaden × (1 − Multiplikator des ungeschütztesten erreichbaren Ziels).</summary>
    private static double PreventedThreat(GameState state, Team team, IReadOnlySet<Guid> excluded) =>
        EnemyThreats(state, team, excluded).Sum(t => t.Damage * (1.0 - t.WorstMultiplier));

    /// <summary>Summe der Überschüsse über die Gefahrenschwelle, max(0, T − s·HP), über alle eigenen Helden.</summary>
    private static double ExcessDanger(GameState state, Team team, IReadOnlySet<Guid> excluded)
    {
        var enemies = Attackers(state, team, new HashSet<Guid>()).ToList(); // Gegner-Tod senkt die Gefahr echt - nicht ausgenommen
        double total = 0;
        foreach (var unit in OwnHeroes(state, team, excluded))
        {
            double threat = enemies.Where(e => CanReach(e, unit, state)).Sum(e => (double)EffectiveStats.AttackDamage(e, state));
            total += Math.Max(0, threat - ThresholdHeuristicSettings.DangerThreshold * unit.CurrentHitPoints);
        }

        return total;
    }

    private static double DistanceToNearestEnemy(GameState state, Guid actorId, IReadOnlySet<Guid> excluded)
    {
        var actor = state.Units.FirstOrDefault(u => u.Id == actorId && !u.IsDefeated);
        if (actor is null)
        {
            return 0;
        }

        // Nur ANGREIFBARE Gegner: ein angehefteter Hugger (nicht angreifbar, steht
        // auf dem Feld seines Ziels) war sonst oft "der nächste Gegner" - die
        // Einheiten rückten dann nicht mehr auf die übrigen Gegner vor.
        var distances = state.Units
            .Where(u => !u.IsDefeated && !u.IsClone && u.Team != actor.Team && !excluded.Contains(u.Id)
                        && !EffectiveStats.PreventsBeingTargetedBy(u, actor.Team, state))
            .Select(u => HexCoordinate.Distance(actor.Position, u.Position))
            .ToList();
        return distances.Count == 0 ? 0 : distances.Min();
    }

    private static IEnumerable<Unit> OwnHeroes(GameState state, Team team, IReadOnlySet<Guid> excluded) =>
        state.Units.Where(u => !u.IsDefeated && !u.IsClone && u.Team == team && !excluded.Contains(u.Id));

    /// <summary>Gegnerische Helden, die angreifen können (keine Klone, nicht am Angreifen gehindert).</summary>
    private static IEnumerable<Unit> Attackers(GameState state, Team team, IReadOnlySet<Guid> excluded) =>
        state.Units.Where(u => !u.IsDefeated && !u.IsClone && u.Team != team && !excluded.Contains(u.Id) && !EffectiveStats.IsAttackingPrevented(u, state));

    /// <summary>Reichweitenregel wie im bisherigen Agenten: Hex-Distanz ≤ Bewegungs- (0 wenn bewegungsunfähig) + Angriffsreichweite.</summary>
    private static bool CanReach(Unit attacker, Unit target, GameState state)
    {
        if (EffectiveStats.PreventsBeingTargetedBy(target, attacker.Team, state))
        {
            return false;
        }

        int reach = (EffectiveStats.IsMovementPrevented(attacker, state) ? 0 : EffectiveStats.MovementRange(attacker, state))
                    + EffectiveStats.AttackRange(attacker, state);
        return HexCoordinate.Distance(attacker.Position, target.Position) <= reach;
    }

    /// <summary>
    /// Anteil eines Treffers, der beim Team ankommt: eigener Multiplikator, bei
    /// sicherer Umleitung (Umleitungsziel steht auf dem Feld) der kleinere von
    /// eigenem und dem des Umleitungsziels - wie im bisherigen Agenten.
    /// </summary>
    private static double EffectiveMultiplier(Unit unit, GameState state)
    {
        double m = Math.Max(0.0, EffectiveStats.IncomingDamageMultiplier(unit, state));
        foreach (var effect in state.GetEffectsOnField(unit.Position))
        {
            if (!effect.RedirectsAttacksToSource || effect.SourceUnitId == unit.Id)
            {
                continue;
            }

            var absorber = state.Units.FirstOrDefault(u => u.Id == effect.SourceUnitId);
            if (absorber is not null && !absorber.IsDefeated && absorber.Team == unit.Team)
            {
                m = Math.Min(m, Math.Max(0.0, EffectiveStats.IncomingDamageMultiplier(absorber, state)));
            }
        }

        return m;
    }
}
