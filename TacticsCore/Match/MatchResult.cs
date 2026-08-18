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
/// Vollständiges Ergebnis einer Partie - die Grundlage für die spätere
/// statistische Auswertung über hunderttausende Partien hinweg. Enthält
/// bewusst auch die Rahmendaten (Seed, Startteam, Ausgangsaufstellung), die
/// zur Reproduktion einer auffälligen Partie nötig sind, ohne dass dafür
/// zwingend das (optionale) volle Replay-Protokoll mitgeführt werden muss.
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
    ReplayLog? Replay);
