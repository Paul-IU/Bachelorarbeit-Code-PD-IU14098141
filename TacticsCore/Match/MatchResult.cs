using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>Ergebnis einer Partie: welches Team gewonnen hat, oder Unentschieden.</summary>
public enum MatchWinner
{
    TeamA,
    TeamB,

    /// <summary>
    /// Eigenständiges Ergebnis, kein "Sieg" eines Teams - siehe
    /// Anforderung 4: "Unentschieden muss als eigenständiges Ergebnis
    /// existieren, nicht als Sieg eines Teams verkleidet."
    /// </summary>
    Draw,
}

/// <summary>Wodurch die Partie beendet wurde.</summary>
public enum MatchEndReason
{
    /// <summary>Ein Team wurde vollständig besiegt (Prüfung erfolgte sofort nach dem entscheidenden Angriff).</summary>
    Elimination,

    /// <summary>Das Rundenlimit (<see cref="MatchRules.RoundLimit"/>) wurde erreicht, ohne dass ein Team ausgelöscht wurde.</summary>
    RoundLimit,
}

/// <summary>Entscheidende Stufe der Auflösung beim Rundenlimit (siehe <see cref="MatchController.DetermineRoundLimitResolution"/>).</summary>
public enum RoundLimitResolution
{
    /// <summary>a) Mehr lebende Helden.</summary>
    SurvivingHeroes,

    /// <summary>b) Bei Gleichstand: höherer prozentualer Gesamt-HP-Anteil.</summary>
    HitPointFraction,

    /// <summary>c) Erneuter Gleichstand: Unentschieden.</summary>
    Draw,
}

/// <summary>
/// Momentaufnahme, welche Einheit (Id, Team) zu Beginn der Partie auf
/// welchem Feld stand. Unabhängig vom veränderlichen GameState festgehalten
/// (Einheiten können im Partieverlauf besiegt, entfernt oder verschoben
/// werden), damit die Ausgangsaufstellung im Ergebnis und im Replay-
/// Protokoll auch nach Partieende noch verfügbar ist.
/// </summary>
public sealed record UnitSetup(Guid UnitId, Team Team, HexCoordinate StartPosition);

/// <summary>
/// Zusammenfassung des Endzustands eines einzelnen Teams: wie viele
/// Einheiten haben überlebt, absolute und prozentuale Rest-HP über das
/// gesamte Team (bezogen auf ALLE zu Partiebeginn angetretenen Einheiten,
/// nicht nur die Überlebenden - eine besiegte Einheit trägt 0 aktuelle HP
/// bei, zählt aber weiterhin zu den maximalen HP des Teams).
/// </summary>
public sealed record TeamResult(
    Team Team,
    int SurvivingUnitCount,
    int TotalCurrentHitPoints,
    int TotalMaxHitPoints,
    double RemainingHitPointsFraction);

/// <summary>
/// Wie oft ein Held (über <see cref="Heroes.HeroDefinition.Id"/>
/// identifiziert) WÄHREND EINER EINZELNEN Partie erfolgreich eine Bewegung,
/// einen Grundangriff bzw. einen Fähigkeitseinsatz durchgeführt hat
/// (Bachelorarbeit-Erweiterung, Auswertungsbedarf: "wie oft bewegt sich ein
/// Held im Schnitt, wie oft greift er an, wie oft nutzt er seine
/// Fähigkeit"). Nur ERFOLGREICHE Aktionen zählen (ein misslungener
/// Bewegungsversuch verbraucht zwar den Zug-Slot, siehe
/// <see cref="MatchController"/>, zählt hier aber bewusst NICHT mit -
/// dieselbe Konvention wie beim Replay-Protokoll, das ebenfalls nur
/// erfolgreiche Aktionen aufzeichnet). Ein Klon trägt NICHT zur Bilanz
/// seines Erzeugers bei (er ist kein <see cref="Heroes.Hero"/>, siehe
/// <see cref="MatchController"/>).
///
/// <see cref="Turns"/> UND <see cref="AbilityAvailableTurns"/>
/// (Bachelorarbeit-Korrektur): ohne eine gemeinsame, GENAU SO GEZÄHLTE
/// Bezugsgröße wären Bewegungen/Angriffe/Fähigkeitseinsätze nicht sinnvoll
/// vergleichbar - weder zwischen Helden mit unterschiedlich langen Partien,
/// noch zwischen Helden mit und ohne Abklingzeit (siehe
/// <see cref="Batch.CsvExporter.ExportHeroWinRates"/> für die genaue
/// Herleitung der daraus gebildeten Kennzahlen "je Zug" bzw. "je
/// verfügbarem Zug"). <see cref="Turns"/> zählt JEDEN Zug, den der Held
/// (lebend zu Zugbeginn) tatsächlich hatte - unabhängig davon, ob er darin
/// überhaupt etwas tun konnte (siehe MatchController.ExecuteTurn: ein Zug
/// kann durch einen tödlichen Effekt-Tick vor der ersten Teilaktion bereits
/// entfallen, zählt hier aber trotzdem, da der Held zu Zugbeginn ja lebte).
/// <see cref="AbilityAvailableTurns"/> zählt NUR die Züge, in denen die
/// Fähigkeit tatsächlich NICHT auf Abklingzeit war (unabhängig davon, ob
/// der Agent sie tatsächlich genutzt hat) - bei einer PASSIVEN Fähigkeit
/// (nie als Zug wählbar) bewusst IMMER 0, nicht "immer verfügbar": eine
/// Fähigkeit, die kein Agent je aktiv einsetzen KANN, als "verfügbar"
/// auszuweisen wäre irreführend.
/// </summary>
public readonly record struct HeroActionCounts(int Moves, int Attacks, int AbilityUses, int Turns, int AbilityAvailableTurns)
{
    public static HeroActionCounts operator +(HeroActionCounts a, HeroActionCounts b) =>
        new(a.Moves + b.Moves, a.Attacks + b.Attacks, a.AbilityUses + b.AbilityUses, a.Turns + b.Turns, a.AbilityAvailableTurns + b.AbilityAvailableTurns);
}

/// <summary>
/// Vollständiges Ergebnis einer Partie - die Grundlage für die spätere
/// statistische Auswertung über hunderttausende Partien hinweg. Enthält
/// bewusst auch die Rahmendaten (Seed, Startteam, Ausgangsaufstellung), die
/// zur Reproduktion einer auffälligen Partie nötig sind, ohne dass dafür
/// zwingend das (optionale) volle Replay-Protokoll mitgeführt werden muss.
///
/// <see cref="HeroActionCountsByHeroId"/> ist über die HELDEN-ID (nicht die
/// einzelne Unit-Id) geführt: tritt DERSELBE Held in dieser einen Partie auf
/// BEIDEN Seiten an (Spiegelpaarung, oder zwei unterschiedliche
/// Kompositionen, die zufällig denselben Helden teilen), fließen die
/// Aktionen BEIDER physischen Einheiten hier bereits zusammen - exakt die
/// Aggregationsebene, die die spätere Helden-CSV (siehe
/// <see cref="Batch.CsvExporter.ExportHeroWinRates"/>) ohnehin braucht.
/// </summary>
public sealed record MatchResult(
    MatchWinner Winner,
    MatchEndReason EndReason,
    int RoundsPlayed,
    TeamResult TeamAResult,
    TeamResult TeamBResult,
    int Seed,
    IReadOnlyList<UnitSetup> InitialSetup,
    Team StartingTeam,
    ReplayLog? Replay,
    IReadOnlyDictionary<string, HeroActionCounts>? HeroActionCountsByHeroId = null,
    IReadOnlyDictionary<Guid, UnitCombatStatistics>? CombatStatisticsByUnitId = null)
{
    /// <summary>Entscheidende Auflösungsstufe, nur bei Ende durch das Rundenlimit (sonst null).</summary>
    public RoundLimitResolution? RoundLimitResolution =>
        EndReason == MatchEndReason.RoundLimit ? MatchController.DetermineRoundLimitResolution(TeamAResult, TeamBResult) : null;
}
