using TacticsCore.Hex;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore.Tests.TestSupport;

/// <summary>
/// Minimale Test-Unterklasse, die den geschützten Unit-Konstruktor mit
/// abweichenden Werten öffentlich zugänglich macht - insbesondere um in
/// Tests Einheiten mit unterschiedlichem Speed zu erzeugen (die einzige
/// öffentliche <c>Unit(...)</c>-Überladung nutzt für alle Einheiten
/// denselben <see cref="UnitDefaults.Speed"/>). Validiert nebenbei, dass
/// die für künftige Helden-Unterklassen vorgesehene
/// Konstruktor-Erweiterung tatsächlich funktioniert.
/// </summary>
public sealed class TestUnit : Unit
{
    public TestUnit(Guid id, Team team, HexCoordinate position, int speed)
        : base(id, team, position, UnitDefaults.MovementRange, UnitDefaults.MaxHitPoints, UnitDefaults.BaseAttackDamage, speed)
    {
    }
}
