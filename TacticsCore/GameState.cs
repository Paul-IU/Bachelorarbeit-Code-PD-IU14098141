using TacticsCore.Board;
using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore;

/// <summary>
/// Hält den vollständigen, veränderlichen Zustand einer Partie: das
/// Spielfeld (unveränderlich) und die aktuell aktiven Einheiten
/// (veränderlich über Bewegung, Schaden und Niederlage). Enthält bewusst
/// keinerlei Zufalls- oder Zeitabhängigkeit - der Zustand entwickelt sich
/// ausschließlich durch explizite, deterministische Aufrufe, was für die
/// spätere Massen-Simulation zwingend ist.
///
/// GameState ist die einzige Stelle, die weiß, welche Einheiten auf
/// welchem Feld stehen (die Belegung), und die einzige Stelle, an der sich
/// die aktive Einheitenliste ändert (Platzieren via <see cref="AddUnit"/>,
/// Bewegen via <see cref="MoveUnit"/>, Entfernen nach Niederlage via
/// <see cref="RemoveUnit"/>). Ein Field selbst trägt keinen "besetzt"-
/// Zustand - so bleibt die reine Geometrie (Board/Field) von der sich
/// ändernden Partie-Situation sauber getrennt.
///
/// Wichtig: ein Feld kann von BELIEBIG VIELEN Einheiten gleichzeitig belegt
/// werden, auch von Einheiten unterschiedlicher Teams - siehe
/// <see cref="OccupancyRules"/> für die (aktuell inaktiven) Stellschrauben,
/// über die sich das später ändern lässt.
/// </summary>
public sealed class GameState
{
    private readonly List<Unit> _units = new();
    private readonly Dictionary<HexCoordinate, List<Unit>> _unitsByCoordinate = new();
    private readonly List<ActiveEffect> _activeEffects = new();

    public GameState(Board.Board board)
    {
        Board = board;
    }

    public Board.Board Board { get; }

    /// <summary>Alle aktuell aktiven (nicht besiegten) Einheiten der Partie (schreibgeschützte Sicht).</summary>
    public IReadOnlyList<Unit> Units => _units;

    /// <summary>
    /// Platziert eine neue Einheit auf dem Spielfeld. Die Position muss
    /// Teil des Spielfelds sein, sonst wird nichts verändert. Ein bereits
    /// belegtes Zielfeld ist dagegen grundsätzlich KEIN Hinderungsgrund -
    /// mehrere Einheiten (auch gegnerische) dürfen dasselbe Feld teilen,
    /// solange kein Stapel-Limit gesetzt ist (siehe
    /// <see cref="OccupancyRules.MaxUnitsPerField"/>).
    /// </summary>
    /// <returns>true, wenn die Einheit platziert wurde; sonst false.</returns>
    public bool AddUnit(Unit unit)
    {
        if (!Board.Contains(unit.Position))
        {
            return false;
        }

        // Schutz gegen Programmierfehler: dieselbe Unit-Instanz darf nicht
        // zweimal registriert werden, sonst würde die Belegungs-Buchführung
        // beim nächsten Move dieser Einheit inkonsistent (doppelter Eintrag,
        // von dem nur einer entfernt wird).
        if (_units.Contains(unit))
        {
            return false;
        }

        if (!HasCapacity(unit, unit.Position))
        {
            return false;
        }

        _units.Add(unit);
        AddToIndex(unit.Position, unit);
        return true;
    }

    /// <summary>Prüft, ob auf <paramref name="coordinate"/> aktuell mindestens eine Einheit steht.</summary>
    public bool IsOccupied(HexCoordinate coordinate) =>
        _unitsByCoordinate.TryGetValue(coordinate, out var units) && units.Count > 0;

    /// <summary>
    /// Liefert alle Einheiten, die aktuell auf <paramref name="coordinate"/>
    /// stehen (kann mehr als eine sein, auch aus unterschiedlichen Teams).
    /// Leere Liste, wenn das Feld frei ist.
    /// </summary>
    public IReadOnlyList<Unit> GetUnitsAt(HexCoordinate coordinate) =>
        _unitsByCoordinate.TryGetValue(coordinate, out var units) ? units : Array.Empty<Unit>();

    /// <summary>
    /// Führt die eigentliche Bewegung aus: aktualisiert sowohl die
    /// Belegungs-Buchführung als auch die Position der Unit. Wird
    /// ausschließlich von <see cref="Movement.MovementSystem.TryMove"/>
    /// nach erfolgreicher Reichweitenprüfung aufgerufen (daher internal) -
    /// so gibt es genau eine Stelle im Code, an der sich eine Position
    /// ändert. Schlägt nur fehl, wenn ein (aktuell inaktives) Stapel-Limit
    /// das Zielfeld blockiert; in diesem Fall bleibt der Zustand
    /// unverändert.
    ///
    /// MITZIEHEN (siehe <see cref="Effects.EffectTemplate.CausesSourceToFollowTarget"/>,
    /// z.B. Hugger): weil dies die EINZIGE Stelle ist, an der sich eine
    /// Position tatsächlich ändert, wird hier - nach der eigentlichen
    /// Bewegung - generisch geprüft, ob ein aktiver Effekt DIESE Einheit
    /// als Ziel hat und seine Quelle mitziehen soll; falls ja, wird die
    /// Quelle rekursiv über dieselbe Methode auf dasselbe Feld bewegt (ohne
    /// eigenes Bewegungsbudget zu verbrauchen und ohne die sonst für eine
    /// Bewegung geltenden Prüfungen wie Reichweite oder Feldsperren - das
    /// ist keine eigene Zugentscheidung der mitgezogenen Einheit).
    /// </summary>
    internal bool MoveUnit(Unit unit, HexCoordinate target)
    {
        if (!HasCapacity(unit, target))
        {
            return false;
        }

        RemoveFromIndex(unit.Position, unit);
        unit.SetPosition(target);
        AddToIndex(target, unit);

        foreach (var effect in _activeEffects.Where(e => e.CausesSourceToFollowTarget && e.TargetUnitId == unit.Id).ToList())
        {
            var follower = _units.FirstOrDefault(u => u.Id == effect.SourceUnitId);
            if (follower is not null && follower.Position != target)
            {
                MoveUnit(follower, target);
            }
        }

        return true;
    }

    /// <summary>
    /// Entfernt eine Einheit endgültig aus der Partie - aktuell der einzige
    /// Auslöser: Niederlage durch einen Angriff (siehe
    /// <see cref="Combat.CombatSystem.TryAttack"/>). Danach ist die Einheit
    /// weder in <see cref="Units"/> noch in der Belegungs-Buchführung
    /// enthalten und kann nicht mehr Ziel einer Bewegung oder eines
    /// weiteren Angriffs sein. Wird ausschließlich vom CombatSystem
    /// aufgerufen (daher internal).
    /// </summary>
    internal void RemoveUnit(Unit unit)
    {
        RemoveFromIndex(unit.Position, unit);
        _units.Remove(unit);
    }

    /// <summary>
    /// Prüft das optionale Stapel-Limit aus
    /// <see cref="OccupancyRules.MaxUnitsPerField"/>. <paramref name="unit"/>
    /// selbst zählt dabei nicht als Belegung mit - so bleibt "auf dem
    /// eigenen Feld stehen bleiben" auch bei einem später gesetzten, engen
    /// Limit immer erlaubt. Ist kein Limit gesetzt (null, aktuelle
    /// Grundregel), liefert die Prüfung immer true.
    /// </summary>
    private bool HasCapacity(Unit unit, HexCoordinate target)
    {
        if (OccupancyRules.MaxUnitsPerField is not int max)
        {
            return true;
        }

        int occupantsExcludingSelf = GetUnitsAt(target).Count(u => u != unit);
        return occupantsExcludingSelf < max;
    }

    /// <summary>
    /// Alle aktuell aktiven Effekte der Partie (Buffs, Debuffs,
    /// Feldsperren, Schadensreduktionen, ...) - Teil des Spielzustands
    /// (Anforderung 10). Die eigentlichen Regeln (Anwenden, Ticken,
    /// Ablaufen, Stapelverhalten) liegen bewusst getrennt in
    /// <see cref="Effects.EffectSystem"/>, nicht hier - GameState verwaltet
    /// nur die reine Buchführung, analog zu Units/Belegung.
    /// </summary>
    public IReadOnlyList<ActiveEffect> ActiveEffects => _activeEffects;

    /// <summary>Alle aktiven, an <paramref name="unitId"/> gebundenen Effekte.</summary>
    public IEnumerable<ActiveEffect> GetEffectsOnUnit(Guid unitId) =>
        _activeEffects.Where(e => e.AnchorType == EffectAnchorType.Unit && e.TargetUnitId == unitId);

    /// <summary>Alle aktiven, an <paramref name="field"/> gebundenen Effekte.</summary>
    public IEnumerable<ActiveEffect> GetEffectsOnField(HexCoordinate field) =>
        _activeEffects.Where(e => e.AnchorType == EffectAnchorType.Field && e.TargetField == field);

    /// <summary>Ausschließlich für <see cref="Effects.EffectSystem"/> gedacht.</summary>
    internal void AddEffect(ActiveEffect effect) => _activeEffects.Add(effect);

    /// <summary>Ausschließlich für <see cref="Effects.EffectSystem"/> gedacht.</summary>
    internal void RemoveEffect(ActiveEffect effect) => _activeEffects.Remove(effect);

    /// <summary>Alle Felder, die aktuell für mindestens ein Team gesperrt sind, mit dem gesperrten Team (Anforderung 18: "alle gesperrten Felder" müssen auslesbar sein).</summary>
    public IEnumerable<(HexCoordinate Field, Team LockedAgainst)> GetLockedFields() =>
        _activeEffects
            .Where(e => e.FieldLockAgainstTeam.HasValue)
            .Select(e => (e.TargetField!.Value, e.FieldLockAgainstTeam!.Value))
            .Distinct();

    // -----------------------------------------------------------------
    // ZUSÄTZLICH AUSLESBARER LAUFZEIT-KONTEXT (Anforderung 18 aus dem
    // Schnittstellen-Teil): welche Runde gerade läuft, wer aktuell am Zug
    // ist und welches Aktionsbudget dieser Einheit noch verbleibt. Reine
    // Buchführung, ausschließlich vom MatchController gepflegt (daher
    // "internal set"-artige Methoden) - für eine spätere Oberfläche
    // gedacht, die den GESAMTEN Spielzustand ohne Rückgriff auf den
    // MatchController selbst ablesen können soll, ohne dass GameState
    // dadurch irgendeine Rundenlogik selbst übernimmt.
    // -----------------------------------------------------------------

    /// <summary>Die aktuell laufende Runde (0, solange noch keine Runde begonnen hat).</summary>
    public int CurrentRoundNumber { get; private set; }

    /// <summary>Die Einheit, die gerade am Zug ist; null zwischen zwei Zügen bzw. vor/nach der Partie.</summary>
    public Guid? CurrentActingUnitId { get; private set; }

    /// <summary>Das noch verbleibende Aktionsbudget der aktuell handelnden Einheit (siehe <see cref="CurrentActingUnitId"/>).</summary>
    public TurnBudget CurrentTurnBudget { get; private set; }

    /// <summary>Ausschließlich für <see cref="Match.MatchController"/> gedacht.</summary>
    internal void SetCurrentRound(int roundNumber) => CurrentRoundNumber = roundNumber;

    /// <summary>Ausschließlich für <see cref="Match.MatchController"/> gedacht.</summary>
    internal void SetCurrentActor(Guid? unitId, TurnBudget budget)
    {
        CurrentActingUnitId = unitId;
        CurrentTurnBudget = budget;
    }

    private void AddToIndex(HexCoordinate coordinate, Unit unit)
    {
        if (!_unitsByCoordinate.TryGetValue(coordinate, out var units))
        {
            units = new List<Unit>();
            _unitsByCoordinate[coordinate] = units;
        }

        units.Add(unit);
    }

    private void RemoveFromIndex(HexCoordinate coordinate, Unit unit)
    {
        if (!_unitsByCoordinate.TryGetValue(coordinate, out var units))
        {
            return;
        }

        units.Remove(unit);
        if (units.Count == 0)
        {
            _unitsByCoordinate.Remove(coordinate);
        }
    }
}
