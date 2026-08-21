using TacticsCore.Abilities;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>Art der im Replay protokollierten Aktion bzw. des protokollierten Ereignisses.</summary>
public enum ReplayActionType
{
    Move,
    Attack,
    UseAbility,
    EffectApplied,
    EffectExpired,
    CloneSpawned,
    CloneDespawned,

    /// <summary>Eine Einheit (Held, Standardfigur oder Klon) wurde besiegt (HP auf 0) und aus dem Spiel entfernt.</summary>
    UnitDefeated,

    /// <summary>
    /// Ein aktiver, dauerhafter Effekt wird zu seinem regulären Tick-Zeitpunkt
    /// verarbeitet (siehe EffectSystem.TickUnitEffects/TickFieldEffects) -
    /// unabhängig davon, ob er dabei Schaden verursacht und/oder in
    /// DEMSELBEN Tick abläuft (dann folgt zusätzlich ein EffectExpired-
    /// Eintrag). Für eine spätere Oberfläche z.B. relevant, um einen
    /// wiederkehrenden Effekt (Gift, Feldsperre, ...) sichtbar zu "pulsen".
    /// </summary>
    EffectTicked,

    /// <summary>Der Zug einer Einheit beginnt (noch vor dem eigenen Effekt-Tick).</summary>
    TurnStarted,

    /// <summary>Der Zug einer Einheit endet (Budget verbraucht, aktiv gepasst, oder die Einheit war bereits besiegt).</summary>
    TurnEnded,

    /// <summary>Eine neue Runde beginnt (vor der Neuberechnung der Zugreihenfolge).</summary>
    RoundStarted,

    /// <summary>Die Partie ist beendet - siehe <see cref="ReplayEntry.Winner"/>/<see cref="ReplayEntry.EndReason"/>.</summary>
    MatchEnded,
}

/// <summary>
/// Eine einzelne, tatsächlich AUSGEFÜHRTE (also gültige) Aktion oder ein
/// daraus folgendes Ereignis im Replay-Protokoll. Nur zustandsverändernde
/// Vorgänge werden protokolliert - ein explizites Passen
/// (<see cref="TurnAction.EndTurn"/>) oder ein fehlgeschlagener Versuch
/// hinterlassen keinen Eintrag, weil sie den Spielzustand nicht verändern
/// und für die Zug-für-Zug-Rekonstruktion des Spielfelds nichts beitragen.
///
/// Reiner Datensatz mit ausschließlich einfachen Feldern (Guid, Enum, int,
/// string, HexCoordinate als Value-Type aus zwei ints) - problemlos mit
/// System.Text.Json serialisierbar, ohne dass TacticsCore selbst eine
/// JSON-Abhängigkeit bräuchte. Nicht jeder Eintragstyp nutzt alle Felder -
/// welche relevant sind, ergibt sich aus <see cref="ActionType"/> (siehe
/// die einzelnen <c>For...</c>-Fabrikmethoden).
/// </summary>
public sealed record ReplayEntry(
    int RoundNumber,
    Guid ActingUnitId,
    Team ActingTeam,
    ReplayActionType ActionType,
    HexCoordinate? MoveSource,
    HexCoordinate? MoveTarget,
    Guid? AttackTargetUnitId,
    int? DamageDealt,
    string? AbilityId = null,
    string? ModeId = null,
    string? EffectName = null,
    Guid? RelatedUnitId = null,
    HexCoordinate? EffectField = null,
    HexPath? MovePath = null,
    MatchWinner? Winner = null,
    MatchEndReason? EndReason = null)
{
    /// <summary>
    /// <paramref name="source"/> ist die Position der Einheit UNMITTELBAR
    /// VOR der Bewegung - sie muss vom Aufrufer festgehalten werden, bevor
    /// <see cref="Unit.TryMove"/> ausgeführt wird, da <see cref="Unit.Position"/>
    /// danach bereits das Ziel ist. <paramref name="path"/> ist der
    /// vollständige, tatsächlich zurückgelegte Weg inklusive Start- und
    /// Zielfeld (Anforderung 16: "Einheit bewegt sich (mit Pfad)") - optional,
    /// damit bestehende Aufrufer ohne Pfad-Kenntnis weiter funktionieren. Als
    /// <see cref="HexPath"/> (nicht als rohe Liste) gekapselt, damit zwei
    /// inhaltlich gleiche Pfade aus zwei unabhängigen Partie-Durchläufen auch
    /// als GLEICH erkannt werden (siehe dortige Begründung).
    /// </summary>
    public static ReplayEntry ForMove(int roundNumber, Unit actor, HexCoordinate source, HexCoordinate target, IReadOnlyList<HexCoordinate>? path = null) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.Move, MoveSource: source, MoveTarget: target, AttackTargetUnitId: null, DamageDealt: null, MovePath: path is null ? null : new HexPath(path));

    public static ReplayEntry ForAttack(int roundNumber, Unit actor, Unit target, int damageDealt) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.Attack, MoveSource: null, MoveTarget: null, AttackTargetUnitId: target.Id, DamageDealt: damageDealt);

    public static ReplayEntry ForAbility(int roundNumber, Unit actor, AbilityTarget target, AbilityMode? mode, string abilityId)
    {
        Guid? targetUnitId = target is AbilityTarget.UnitTarget u ? u.Unit.Id : null;
        HexCoordinate? targetField = target is AbilityTarget.FieldTarget f ? f.Field : null;
        return new(roundNumber, actor.Id, actor.Team, ReplayActionType.UseAbility, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: targetUnitId, DamageDealt: null, AbilityId: abilityId, ModeId: mode?.Id, EffectField: targetField);
    }

    /// <summary>
    /// <see cref="ReplayEntry.ActingTeam"/> ist hier bewusst der Default-Wert
    /// (nicht aussagekräftig): <see cref="Effects.ActiveEffect"/> speichert
    /// nur die Quell-UNIT-Id, nicht deren Team - für die Zug-für-Zug-
    /// Rekonstruktion genügt die Id (das Team der Quelle lässt sich bei
    /// Bedarf über die Ausgangsaufstellung nachschlagen).
    /// </summary>
    public static ReplayEntry ForEffectApplied(int roundNumber, Guid sourceUnitId, string effectName, Guid? targetUnitId, HexCoordinate? targetField) =>
        new(roundNumber, sourceUnitId, default, ReplayActionType.EffectApplied, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, EffectName: effectName, RelatedUnitId: targetUnitId, EffectField: targetField);

    public static ReplayEntry ForEffectExpired(int roundNumber, string effectName, Guid? targetUnitId, HexCoordinate? targetField) =>
        new(roundNumber, Guid.Empty, default, ReplayActionType.EffectExpired, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, EffectName: effectName, RelatedUnitId: targetUnitId, EffectField: targetField);

    public static ReplayEntry ForCloneSpawned(int roundNumber, Unit creator, Unit clone) =>
        new(roundNumber, clone.Id, clone.Team, ReplayActionType.CloneSpawned, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, RelatedUnitId: creator.Id);

    public static ReplayEntry ForCloneDespawned(int roundNumber, Guid cloneId, Team team, Guid creatorId) =>
        new(roundNumber, cloneId, team, ReplayActionType.CloneDespawned, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, RelatedUnitId: creatorId);

    /// <summary>
    /// Anforderung 16: "Einheit besiegt" als eigenständiges Ereignis - bisher
    /// nur indirekt aus einem Attack-/UseAbility-/EffectTicked-Eintrag mit
    /// entsprechendem Schaden ableitbar. Wird für JEDE besiegte Einheit
    /// gemeldet (Held, Standardfigur, Klon) - bei einem Klon zusätzlich zum
    /// (semantisch eigenständigen) CloneDespawned-Ereignis.
    /// </summary>
    public static ReplayEntry ForUnitDefeated(int roundNumber, Guid unitId, Team team) =>
        new(roundNumber, unitId, team, ReplayActionType.UnitDefeated, MoveSource: null, MoveTarget: null, AttackTargetUnitId: null, DamageDealt: null);

    /// <summary>Siehe <see cref="ReplayActionType.EffectTicked"/>.</summary>
    public static ReplayEntry ForEffectTicked(int roundNumber, string effectName, Guid? targetUnitId, HexCoordinate? targetField) =>
        new(roundNumber, Guid.Empty, default, ReplayActionType.EffectTicked, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, EffectName: effectName, RelatedUnitId: targetUnitId, EffectField: targetField);

    public static ReplayEntry ForTurnStarted(int roundNumber, Unit actor) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.TurnStarted, MoveSource: null, MoveTarget: null, AttackTargetUnitId: null, DamageDealt: null);

    public static ReplayEntry ForTurnEnded(int roundNumber, Unit actor) =>
        new(roundNumber, actor.Id, actor.Team, ReplayActionType.TurnEnded, MoveSource: null, MoveTarget: null, AttackTargetUnitId: null, DamageDealt: null);

    /// <summary>Kein handelndes Team - <see cref="ReplayEntry.ActingTeam"/> bleibt der bedeutungslose Default-Wert (siehe <see cref="ForEffectApplied"/>).</summary>
    public static ReplayEntry ForRoundStarted(int roundNumber) =>
        new(roundNumber, Guid.Empty, default, ReplayActionType.RoundStarted, MoveSource: null, MoveTarget: null, AttackTargetUnitId: null, DamageDealt: null);

    public static ReplayEntry ForMatchEnded(int roundNumber, MatchWinner winner, MatchEndReason endReason) =>
        new(roundNumber, Guid.Empty, default, ReplayActionType.MatchEnded, MoveSource: null, MoveTarget: null,
            AttackTargetUnitId: null, DamageDealt: null, Winner: winner, EndReason: endReason);
}

/// <summary>
/// Vollständiges, optionales Protokoll einer Partie (Anforderung 6 aus Teil
/// B): Ausgangszustand (Seed, Startteam, Ausgangsaufstellung) plus die
/// geordnete Liste aller tatsächlich ausgeführten Aktionen und daraus
/// folgenden Ereignisse (Fähigkeitseinsatz, Effekt-Anwendung/-Ablauf,
/// Klon-Erzeugung/-Verschwinden). Reicht aus, um die Partie später Zug für
/// Zug nachzuvollziehen, ohne die Agenten-/Zufallslogik erneut ausführen
/// zu müssen.
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
