using TacticsCore.Combat;
using TacticsCore.Hex;
using TacticsCore.Movement;
using TacticsCore.Rules;

// GameState liegt bewusst im Wurzel-Namespace TacticsCore (siehe GameState.cs).
using TacticsCore;

namespace TacticsCore.Units;

/// <summary>
/// Basisklasse einer Spieleinheit - die "Standardfigur": Identität, Team-
/// Zugehörigkeit, Position, Bewegungsreichweite, Lebenspunkte und
/// Grundangriffsschaden. Aktuell sind alle Einheiten identisch (siehe
/// <see cref="Rules.UnitDefaults"/>); noch keine Helden-Unterklassen,
/// Rollen oder Fähigkeiten.
///
/// Eine Unit kennt weder das Spielfeld noch die anderen Einheiten selbst;
/// für Bewegung und Kampf braucht sie immer den <see cref="TacticsCore.GameState"/>.
/// Die eigentlichen Algorithmen dafür liegen bewusst NICHT in dieser Klasse,
/// sondern getrennt in <see cref="MovementSystem"/> bzw.
/// <see cref="CombatSystem"/> (siehe STIL-Vorgabe: eigene Dateien/Klassen
/// für Bewegungs- und Kampflogik). Unit selbst bietet nur schlanke
/// Delegations-Methoden als bequeme Schnittstelle
/// ("unit.TryMove(...)" statt "MovementSystem.TryMove(unit, ...)"). Das
/// hält die Unit unabhängig von jeglicher Engine- oder UI-Anbindung.
/// </summary>
public class Unit
{
    /// <summary>Eindeutige, über die gesamte Simulation stabile Kennung.</summary>
    public Guid Id { get; }

    public Team Team { get; }

    /// <summary>Aktuelle Position der Einheit auf dem Spielfeld.</summary>
    public HexCoordinate Position { get; private set; }

    /// <summary>Bewegungsreichweite in Feldern pro Zug. Siehe <see cref="UnitDefaults.MovementRange"/>.</summary>
    public int MovementRange { get; }

    /// <summary>Maximale Lebenspunkte. Siehe <see cref="UnitDefaults.MaxHitPoints"/>.</summary>
    public int MaxHitPoints { get; }

    /// <summary>Schaden eines Grundangriffs. Siehe <see cref="UnitDefaults.BaseAttackDamage"/>.</summary>
    public int BaseAttackDamage { get; }

    /// <summary>
    /// Geschwindigkeit - bestimmt die Rangfolge innerhalb des eigenen Teams
    /// in der Zugreihenfolge (absteigend). Siehe <see cref="UnitDefaults.Speed"/>
    /// und <see cref="Match.TurnOrder"/>.
    /// </summary>
    public int Speed { get; }

    /// <summary>Aktuelle Lebenspunkte, nie negativ (siehe <see cref="ApplyDamage"/>).</summary>
    public int CurrentHitPoints { get; private set; }

    /// <summary>Ob die Einheit besiegt ist (HP auf 0 gefallen). Besiegte Einheiten handeln nicht mehr und sind kein gültiges Ziel.</summary>
    public bool IsDefeated => CurrentHitPoints <= 0;

    public Unit(Guid id, Team team, HexCoordinate startPosition)
        : this(id, team, startPosition, UnitDefaults.MovementRange, UnitDefaults.MaxHitPoints, UnitDefaults.BaseAttackDamage, UnitDefaults.Speed)
    {
    }

    /// <summary>
    /// Erweiterte Konstruktor-Überladung für spätere Helden-Unterklassen mit
    /// abweichender Reichweite, HP, Grundschaden oder Speed. Bewusst als
    /// normale, im Konstruktor gesetzte Eigenschaften umgesetzt statt als
    /// überschreibbare virtuelle Member: ein virtueller Aufruf im
    /// Basiskonstruktor würde bei Unterklassen, deren Override auf noch
    /// nicht initialisierten eigenen Feldern basiert, zu falschen Werten
    /// führen (bekannte C#-Falle). Werte werden stattdessen explizit über
    /// den Konstruktor durchgereicht - eine zukünftige Hero-Klasse ruft
    /// einfach <c>base(id, team, position, heroRange, heroHp, heroDamage, heroSpeed)</c> auf.
    /// </summary>
    protected Unit(Guid id, Team team, HexCoordinate startPosition, int movementRange, int maxHitPoints, int baseAttackDamage, int speed)
    {
        Id = id;
        Team = team;
        Position = startPosition;
        MovementRange = movementRange;
        MaxHitPoints = maxHitPoints;
        BaseAttackDamage = baseAttackDamage;
        Speed = speed;
        CurrentHitPoints = maxHitPoints;
    }

    /// <summary>
    /// Setzt die Position direkt, ohne Prüfung. Ausschließlich für
    /// <see cref="TacticsCore.GameState"/> gedacht, das nach erfolgreicher
    /// Validierung einer Bewegung sowohl die Unit als auch seine eigene
    /// Belegungs-Buchführung konsistent aktualisieren muss.
    /// </summary>
    internal void SetPosition(HexCoordinate newPosition) => Position = newPosition;

    /// <summary>
    /// Reduziert die aktuellen HP um <paramref name="amount"/>, niemals
    /// unter 0. Ausschließlich für <see cref="CombatSystem"/> gedacht
    /// (daher internal) - Schaden entsteht in diesem Schritt nur durch
    /// einen validierten Angriff, nie direkt von außen.
    /// </summary>
    internal void ApplyDamage(int amount) => CurrentHitPoints = Math.Max(0, CurrentHitPoints - amount);

    /// <summary>Alle innerhalb der Bewegungsreichweite erreichbaren Felder mit Schrittzahl. Siehe <see cref="MovementSystem.GetReachableFields"/>.</summary>
    public IReadOnlyList<ReachableField> GetReachableFields(GameState state) => MovementSystem.GetReachableFields(this, state);

    /// <summary>Alle aktuell legalen Bewegungsziele, stabil sortiert nach (q, r). Grundlage für spätere Agenten.</summary>
    public IReadOnlyList<HexCoordinate> GetLegalMoves(GameState state) => MovementSystem.GetLegalMoveTargets(this, state);

    /// <summary>Kürzester Pfad zu einem Zielfeld, für Visualisierung/Debugging. Siehe <see cref="MovementSystem.GetShortestPath"/>.</summary>
    public IReadOnlyList<HexCoordinate> GetShortestPathTo(HexCoordinate target, GameState state) => MovementSystem.GetShortestPath(this, target, state);

    /// <summary>Versucht, auf <paramref name="target"/> zu ziehen. Siehe <see cref="MovementSystem.TryMove"/>.</summary>
    public bool TryMove(HexCoordinate target, GameState state) => MovementSystem.TryMove(this, target, state);

    /// <summary>Alle aktuell gültigen Angriffsziele, stabil sortiert nach (q, r, UnitId). Grundlage für spätere Agenten.</summary>
    public IReadOnlyList<Unit> GetLegalAttacks(GameState state) => CombatSystem.GetValidAttackTargets(this, state);

    /// <summary>Versucht, <paramref name="target"/> mit einem Grundangriff zu treffen. Siehe <see cref="CombatSystem.TryAttack"/>.</summary>
    public bool TryAttack(Unit target, GameState state) => CombatSystem.TryAttack(this, target, state);
}
