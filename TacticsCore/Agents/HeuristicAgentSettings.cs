namespace TacticsCore.Agents;

/// <summary>
/// ZENTRALE STELLSCHRAUBEN der heuristischen Agenten (siehe <see cref="HeuristicAgent"/>
/// und <see cref="TurnEvaluator"/>). Alle Werte stehen hier und nirgends sonst -
/// eine Anpassung ist eine Änderung an genau dieser Datei, keine Suche im Code.
/// Alle heuristischen Agenten verwenden dieselbe Bewertungsfunktion; aggressiver
/// und defensiver Agent unterscheiden sich AUSSCHLIESSLICH in den beiden Gewichten,
/// der vorausschauende zusätzlich in der Suchtiefe.
/// </summary>
public static class HeuristicAgentSettings
{
    /// <summary>Aggressiver Agent: offensiver Beitrag doppelt so stark gewichtet wie der defensive (2:1).</summary>
    public const double AggressiveOffensiveWeight = 2.0;

    /// <summary>Siehe <see cref="AggressiveOffensiveWeight"/>.</summary>
    public const double AggressiveDefensiveWeight = 1.0;

    /// <summary>Defensiver Agent: defensiver Beitrag doppelt so stark gewichtet wie der offensive (1:2).</summary>
    public const double DefensiveOffensiveWeight = 1.0;

    /// <summary>Siehe <see cref="DefensiveOffensiveWeight"/>.</summary>
    public const double DefensiveDefensiveWeight = 2.0;

    /// <summary>
    /// Vorausschauender Agent (<see cref="LookaheadHeuristicAgent"/>): NEUTRALE
    /// Gewichtung 1:1. Begründung: der vorausschauende Agent soll sich von den
    /// einstufigen Agenten allein durch die Suchtiefe unterscheiden, nicht durch
    /// eine Ausrichtung. 1:1 liegt genau zwischen aggressiv (2:1) und defensiv
    /// (1:2) und bevorzugt keinen der beiden Beiträge; Unterschiede im Spiel lassen
    /// sich so der Vorausschau zuschreiben statt einer Gewichtung.
    /// </summary>
    public const double NeutralOffensiveWeight = 1.0;

    /// <summary>Siehe <see cref="NeutralOffensiveWeight"/>.</summary>
    public const double NeutralDefensiveWeight = 1.0;

    /// <summary>
    /// BESCHNEIDUNG des vorausschauenden Agenten: nur die besten K eigenen
    /// Zugfolgen nach einstufiger Bewertung erhalten die zweite Ebene (simulierte
    /// Antwort der nächsten gegnerischen Einheit). Begründung und Messung siehe
    /// <see cref="LookaheadHeuristicAgent"/>, Abschnitt "Beschneidung".
    /// </summary>
    public const int LookaheadPruningWidth = 5;

    // (Entfallen mit Überarbeitung 9: PlanningHorizonRounds = 2 - Planungshorizont für
    // Wirkungen ohne feste Dauer. Nach Überarbeitung 8 las ihn nur noch der Schaden
    // über Zeit ohne feste Dauer, der im Katalog nicht vorkommt; ein wirkungsloser,
    // nicht begründbarer Wert. Ersetzt durch die Regel "ein Tick", siehe TurnEvaluator.)

    /// <summary>
    /// Bonus für eine besiegte gegnerische Einheit, in Runden ihres
    /// Angriffsschadens: eine besiegte Einheit richtet in dieser Zahl von Runden
    /// keinen Schaden mehr an. Umgekehrt zählt der Verlust einer eigenen Einheit
    /// mit demselben Wert negativ.
    /// </summary>
    public const int KillBonusRounds = 2;

    // (Entfallen mit Überarbeitung 6: DamagePreventedWeight = 1,0 - Gewicht der
    // abgefangenen Bedrohung. Schutz steckt seitdem in der geschützt gerechneten
    // Bedrohung selbst und hat damit implizit dasselbe Gewicht wie die Bedrohung.)

    /// <summary>
    /// GRUNDGEWICHT der eigenen ANGRIFFSMÖGLICHKEIT (Überarbeitung 2 und 3, siehe
    /// <see cref="TurnEvaluator"/>): Faktor auf die Änderung der Summe der eigenen
    /// Schadenswerte gegenüber erreichbaren Gegnern. Wird VOR der Profil-
    /// Gewichtung (2:1 bzw. 1:2) angewandt und gilt für beide Agenten gleich.
    ///
    /// Überarbeitung 2 setzte 1,0 (genaues Spiegelbild der Bedrohung). Rückt eine
    /// Einheit heran, steigen Angriffsmöglichkeit und Bedrohung dann um denselben
    /// Betrag X; beim defensiven Profil ergibt das 1·X − 2·X = −X - Heranrücken
    /// war IMMER schlechter als Stehenbleiben (0), unabhängig von der Lage. Das
    /// war eine Eigenschaft der Formel, kein defensives Verhalten (83 % der
    /// Partien am Rundenlimit). Überarbeitung 3 setzt 2,0: beim defensiven Profil
    /// ist symmetrisches Heranrücken dann neutral (1·2X − 2·X = 0), beim
    /// aggressiven klar positiv (2·2X − X = 3X); es entscheiden die übrigen Terme.
    /// Begründung der Asymmetrie: wer angreifen kann, entscheidet selbst, ob er es
    /// tut; wer bedroht ist, hat darüber keine Kontrolle.
    /// </summary>
    public const double AttackOpportunityWeight = 2.0;
}

/// <summary>Gewichtung von offensivem und defensivem Beitrag (siehe <see cref="TurnEvaluator"/>).</summary>
public sealed record HeuristicWeights(double Offensive, double Defensive)
{
    public static HeuristicWeights AggressiveProfile { get; } =
        new(HeuristicAgentSettings.AggressiveOffensiveWeight, HeuristicAgentSettings.AggressiveDefensiveWeight);

    public static HeuristicWeights DefensiveProfile { get; } =
        new(HeuristicAgentSettings.DefensiveOffensiveWeight, HeuristicAgentSettings.DefensiveDefensiveWeight);

    /// <summary>Neutrale Gewichtung 1:1 des vorausschauenden Agenten (siehe <see cref="HeuristicAgentSettings.NeutralOffensiveWeight"/>).</summary>
    public static HeuristicWeights NeutralProfile { get; } =
        new(HeuristicAgentSettings.NeutralOffensiveWeight, HeuristicAgentSettings.NeutralDefensiveWeight);
}
