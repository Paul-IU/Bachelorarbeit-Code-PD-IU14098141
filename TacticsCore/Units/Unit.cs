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
    /// Reichweite des Grundangriffs in Hex-Distanz (0 = nur das eigene
    /// Feld, 1 = zusätzlich alle sechs Nachbarn, ...). Siehe
    /// <see cref="UnitDefaults.AttackRange"/> und
    /// <see cref="Combat.CombatSystem.GetAttackPatternFields"/>.
    /// </summary>
    public int AttackRange { get; }

    /// <summary>
    /// Geschwindigkeit - bestimmt die Rangfolge innerhalb des eigenen Teams
    /// in der Zugreihenfolge (absteigend). Siehe <see cref="UnitDefaults.Speed"/>
    /// und <see cref="Match.TurnOrder"/>.
    /// </summary>
    public int Speed { get; }

    /// <summary>Aktuelle Lebenspunkte, nie negativ (siehe <see cref="ApplyDamage"/>) und nie über <see cref="MaxHitPoints"/> (siehe <see cref="Heal"/>).</summary>
    public int CurrentHitPoints { get; private set; }

    /// <summary>Ob die Einheit besiegt ist (HP auf 0 gefallen). Besiegte Einheiten handeln nicht mehr und sind kein gültiges Ziel.</summary>
    public bool IsDefeated => CurrentHitPoints <= 0;

    /// <summary>
    /// Ob diese Einheit eine temporäre, von einer Fähigkeit erzeugte
    /// Einheit ("Klon") ist - siehe <see cref="CreateClone"/> und
    /// Anforderung 9. Ein Klon zählt NICHT als Held: weder für die
    /// Siegbedingung noch für die Heldenanzahl-Wertung beim Rundenlimit
    /// (siehe <see cref="Match.MatchController"/>), kann nicht angreifen
    /// und keine Fähigkeit nutzen (siehe <see cref="Combat.CombatSystem"/>
    /// und <see cref="Abilities.AbilitySystem"/>) und handelt in der
    /// Zugreihenfolge unmittelbar nach seinem Erzeuger statt nach eigenem
    /// Speed (siehe <see cref="Match.TurnOrder"/> und
    /// <see cref="Match.MatchController"/>).
    /// </summary>
    public bool IsClone { get; private set; }

    /// <summary>Bei einem Klon (<see cref="IsClone"/>) die Id des erzeugenden Helden; sonst null.</summary>
    public Guid? CreatorId { get; private set; }

    public Unit(Guid id, Team team, HexCoordinate startPosition)
        : this(id, team, startPosition, UnitDefaults.MovementRange, UnitDefaults.MaxHitPoints, UnitDefaults.BaseAttackDamage, UnitDefaults.Speed, UnitDefaults.AttackRange)
    {
    }

    /// <summary>
    /// Erweiterte Konstruktor-Überladung für spätere Helden-Unterklassen mit
    /// abweichender Reichweite, HP, Grundschaden, Speed oder Angriffs-
    /// reichweite. Bewusst als normale, im Konstruktor gesetzte
    /// Eigenschaften umgesetzt statt als überschreibbare virtuelle Member:
    /// ein virtueller Aufruf im Basiskonstruktor würde bei Unterklassen,
    /// deren Override auf noch nicht initialisierten eigenen Feldern
    /// basiert, zu falschen Werten führen (bekannte C#-Falle). Werte werden
    /// stattdessen explizit über den Konstruktor durchgereicht - eine
    /// Hero-Unterklasse ruft einfach
    /// <c>base(id, team, position, def.MovementRange, def.MaxHitPoints, def.BaseAttackDamage, def.Speed, def.AttackRange)</c> auf.
    /// </summary>
    protected Unit(Guid id, Team team, HexCoordinate startPosition, int movementRange, int maxHitPoints, int baseAttackDamage, int speed, int attackRange)
    {
        Id = id;
        Team = team;
        Position = startPosition;
        MovementRange = movementRange;
        MaxHitPoints = maxHitPoints;
        BaseAttackDamage = baseAttackDamage;
        Speed = speed;
        AttackRange = attackRange;
        CurrentHitPoints = maxHitPoints;
    }

    /// <summary>
    /// Erzeugt eine temporäre Einheit ("Klon") für <paramref name="creator"/>
    /// (Anforderung 9): startet auf dessen aktuellem Feld, im selben Team,
    /// mit dessen Bewegungsreichweite, aber genau 1 HP - der Klon
    /// "absorbiert genau eine Schadensinstanz vollständig und verschwindet
    /// danach", was sich mit MaxHitPoints=1 automatisch aus der
    /// bestehenden HP-/Niederlage-Logik ergibt (jeder Treffer, gleich
    /// welcher Schadenshöhe, bringt die HP auf 0 - kein Sonderfall in
    /// CombatSystem nötig). Grundschaden und Angriffsreichweite sind 0, da
    /// der Klon ohnehin nicht angreifen darf (siehe
    /// <see cref="Combat.CombatSystem"/>).
    ///
    /// Die Id wird bewusst NICHT über <c>Guid.NewGuid()</c> erzeugt, sondern
    /// deterministisch aus der Erzeuger-Id abgeleitet: <c>Guid.NewGuid()</c>
    /// wäre eine nicht über den Seed gesteuerte Zufallsquelle innerhalb der
    /// laufenden Simulation (der Klon entsteht MITTEN in einer Partie, nicht
    /// nur bei der Aufstellung) und würde z.B. die (q,r,UnitId)-Sortierung
    /// von Zielen auf einem geteilten Feld unvorhersehbar verschieben. Da je
    /// Erzeuger ohnehin nur ein Klon gleichzeitig existieren darf, ist eine
    /// feste 1:1-Ableitung eindeutig und ausreichend.
    /// </summary>
    public static Unit CreateClone(Unit creator)
    {
        var clone = new Unit(DeriveCloneId(creator.Id), creator.Team, creator.Position, creator.MovementRange, maxHitPoints: 1, baseAttackDamage: 0, speed: 0, attackRange: 0);
        clone.IsClone = true;
        clone.CreatorId = creator.Id;
        return clone;
    }

    private static Guid DeriveCloneId(Guid creatorId)
    {
        var bytes = creatorId.ToByteArray();
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= 0xA5; // fester Bitmuster-Salt, nur zur Unterscheidung von der Erzeuger-Id
        }

        return new Guid(bytes);
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

    /// <summary>
    /// Erhöht die aktuellen HP um <paramref name="amount"/>, nie über
    /// <see cref="MaxHitPoints"/> hinaus. Ausschließlich für
    /// <see cref="Abilities.AbilitySystem"/> gedacht (daher internal) -
    /// Heilung entsteht nur durch eine validierte Fähigkeit.
    /// </summary>
    internal void Heal(int amount) => CurrentHitPoints = Math.Min(MaxHitPoints, CurrentHitPoints + amount);

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
