namespace TacticsCore.Agents;

/// <summary>
/// ZENTRALE STELLSCHRAUBEN des Schwellen-Agenten (<see cref="ThresholdHeuristicAgent"/>,
/// Bewertung <see cref="ThresholdTurnEvaluator"/>). Beide Ausrichtungen verwenden
/// dieselbe Funktion und unterscheiden sich AUSSCHLIESSLICH in w_D und w_H;
/// w_K, w_P und w_R sind für beide gleich. Unabhängig von
/// <see cref="HeuristicAgentSettings"/> (bisheriger Agent, unverändert).
/// </summary>
public static class ThresholdHeuristicSettings
{
    /// <summary>Aggressiv: Schaden doppelt so stark wie eigene Lebenspunkte (w_D = 2, w_H = 1).</summary>
    public const double AggressiveDamageWeight = 2.0;

    /// <summary>Siehe <see cref="AggressiveDamageWeight"/>.</summary>
    public const double AggressiveHitPointWeight = 1.0;

    /// <summary>Defensiv: eigene Lebenspunkte doppelt so stark wie Schaden (w_D = 1, w_H = 2).</summary>
    public const double DefensiveDamageWeight = 1.0;

    /// <summary>Siehe <see cref="DefensiveDamageWeight"/>.</summary>
    public const double DefensiveHitPointWeight = 2.0;

    /// <summary>
    /// BEZUGSGRÖSSE der Lebenspunkt-Terme (D, H, R): 100 Lebenspunkte = 1
    /// Einheit, also etwa ein Treffer (Grundschaden 60 bis 120). Erst dadurch
    /// sind die dimensionslosen Gewichte w_K (je besiegtem Gegner) und w_P (je
    /// Feld) mit Schaden und Lebenspunkten vergleichbar - in rohen Lebenspunkten
    /// wäre w_K = 5 neben einem Treffer von 100 bedeutungslos.
    /// </summary>
    public const double HitPointUnit = 100.0;

    /// <summary>w_K: Wert je unmittelbar besiegtem gegnerischen Helden, in Treffern (Vorgabe: 5).</summary>
    public const double DefeatWeight = 5.0;

    /// <summary>
    /// w_P: Wert je Feld, um das die handelnde Einheit dem nächsten Gegner
    /// näher kommt: 0,1 = zehn Lebenspunkte, ein Zehntel eines Treffers.
    /// Begründung: Heranrücken soll jeden Gleichstand gegenüber Stehenbleiben
    /// entscheiden (unterhalb der Gefahrenschwelle ist sonst alles neutral),
    /// aber nie einen echten Schaden, eine Heilung oder einen Schutz aufwiegen -
    /// auch drei Felder (0,3) bleiben unter einem halben Treffer.
    /// </summary>
    public const double DistanceGainWeight = 0.1;

    /// <summary>
    /// w_R: Wert des Rückzugs, also der Verringerung des Schadens, der die
    /// Gefahrenschwelle übersteigt (in Treffern). 1 = eine vermiedene
    /// Überschreitung zählt wie derselbe Betrag an Lebenspunkten beim
    /// aggressiven Profil (w_H = 1) und halb so viel wie beim defensiven
    /// (w_H = 2). So weicht der defensive Agent eher zurück, aber auch der
    /// aggressive nimmt eine starke Überschreitung nicht für einen kleinen
    /// Treffer in Kauf.
    /// </summary>
    public const double RetreatWeight = 1.0;

    /// <summary>
    /// GEFAHRENSCHWELLE als Verhältnis drohender Schaden / verbleibende
    /// Lebenspunkte der Einheit: 0,5. Begründung: darunter übersteht die Einheit
    /// selbst einen gebündelten Angriff aller Gegner, die sie erreichen, mit mehr
    /// als der Hälfte ihrer Lebenspunkte - kein Grund zum Zurückweichen.
    /// Darüber bringt EINE Runde gebündelter Angriffe sie in Todesnähe. Beispiele:
    /// Bogenschütze (280 HP) - ein Gegner mit 100 Schaden 0,36 (neutral), zwei
    /// 0,71 (Rückzug lohnt); Wand (1000 HP) erst ab 500 drohendem Schaden, also
    /// praktisch nie bei voller Gesundheit. Mit sinkenden Lebenspunkten sinkt die
    /// Schwelle absolut mit - eine angeschlagene Einheit weicht früher.
    /// </summary>
    public const double DangerThreshold = 0.5;
}

/// <summary>Gewichtung von Schaden (w_D) und eigenen Lebenspunkten (w_H) des Schwellen-Agenten.</summary>
public sealed record ThresholdWeights(double Damage, double HitPoints)
{
    public static ThresholdWeights AggressiveProfile { get; } =
        new(ThresholdHeuristicSettings.AggressiveDamageWeight, ThresholdHeuristicSettings.AggressiveHitPointWeight);

    public static ThresholdWeights DefensiveProfile { get; } =
        new(ThresholdHeuristicSettings.DefensiveDamageWeight, ThresholdHeuristicSettings.DefensiveHitPointWeight);
}
