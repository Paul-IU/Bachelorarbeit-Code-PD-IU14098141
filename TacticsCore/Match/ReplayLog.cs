using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>Art der im Replay protokollierten Aktion.</summary>
public enum ReplayActionType
{
    Move,
    Attack,
}

/// <summary>
/// Eine einzelne, tatsächlich AUSGEFÜHRTE (also gültige) Aktion im Replay-
/// Protokoll. Nur zustandsverändernde Aktionen werden protokolliert - ein
/// explizites Passen (<see cref="TurnAction.EndTurn"/>) oder ein
/// fehlgeschlagener Versuch hinterlassen keinen Eintrag, weil sie den
/// Spielzustand nicht verändern und für die Zug-für-Zug-Rekonstruktion des
/// Spielfelds nichts beitragen.
///
/// Reiner Datensatz mit ausschließlich einfachen Feldern (Guid, Enum, int,
/// HexCoordinate ist ein Value-Type aus zwei ints) - problemlos mit
/// System.Text.Json serialisierbar, ohne dass TacticsCore selbst eine
/// JSON-Abhängigkeit bräuchte.
/// </summary>
public sealed record ReplayEntry(
    int RoundNumber,
    Guid ActingUnitId,
    Team ActingTeam,
    ReplayActionType ActionType,
    HexCoordinate? MoveSource,
    HexCoordinate? MoveTarget,
    Guid? AttackTargetUnitId,
    int? DamageDealt)
{
    /// <summary>
    /// <paramref name="source"/> ist die Position der Einheit UNMITTELBAR
    /// VOR der Bewegung - sie muss vom Aufrufer festgehalten werden, bevor
    /// <see cref="Unit.TryMove"/> ausgeführt wird, da <see cref="Unit.Position"/>
    /// danach bereits das Ziel ist. So lässt sich aus dem Replay direkt
    /// ablesen, woher eine Einheit kam, ohne das Protokoll erst von vorne
    /// "abspielen" zu müssen (siehe Klassendokumentation).
    /// </summary>
    public static ReplayEntry ForMove(int roundNumber, Unit actor, HexCoordinate source, HexCoordinate target) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.Move, MoveSource: source, MoveTarget: target, AttackTargetUnitId: null, DamageDealt: null);

    public static ReplayEntry ForAttack(int roundNumber, Unit actor, Unit target, int damageDealt) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.Attack, MoveSource: null, MoveTarget: null, AttackTargetUnitId: target.Id, DamageDealt: damageDealt);
}

/// <summary>
/// Vollständiges, optionales Protokoll einer Partie (Anforderung 6):
/// Ausgangszustand (Seed, Startteam, Ausgangsaufstellung) plus die
/// geordnete Liste aller tatsächlich ausgeführten Aktionen. Reicht aus, um
/// die Partie später Zug für Zug nachzuvollziehen (das Spielfeld zu jedem
/// Zeitpunkt zu rekonstruieren), ohne die Agenten-/Zufallslogik erneut
/// ausführen zu müssen - man muss nur noch <see cref="ReplayEntry.ActionType"/>
/// der Reihe nach "abspielen".
///
/// Wird nur erzeugt, wenn <see cref="MatchController"/> mit aktivierter
/// Aufzeichnung läuft - ist sie deaktiviert, entsteht dort keine Liste und
/// somit auch kein Zuwachs an Zuteilungen/Speicher pro Aktion (siehe
/// Performance-Anforderung für hunderttausende Partien).
/// </summary>
public sealed record ReplayLog(
    int Seed,
    Team StartingTeam,
    IReadOnlyList<UnitSetup> InitialSetup,
    IReadOnlyList<ReplayEntry> Entries);
