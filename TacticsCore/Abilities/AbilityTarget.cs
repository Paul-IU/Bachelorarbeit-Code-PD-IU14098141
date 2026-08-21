using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Abilities;

/// <summary>
/// Das von einem Agenten für einen Fähigkeitseinsatz gewählte Ziel.
/// Bewusst als geschlossene Klassenhierarchie modelliert (wie
/// <see cref="Match.TurnAction"/>), weil C# keine echten Discriminated
/// Unions kennt: entweder eine konkrete Einheit (Self/Ally/Enemy) oder ein
/// Feld (Field/AllUnitsOnField/AllUnitsInRadius) - nie beides.
/// </summary>
public abstract class AbilityTarget
{
    private AbilityTarget()
    {
    }

    public sealed class UnitTarget : AbilityTarget
    {
        public Unit Unit { get; }
        public UnitTarget(Unit unit) => Unit = unit;
    }

    public sealed class FieldTarget : AbilityTarget
    {
        public HexCoordinate Field { get; }
        public FieldTarget(HexCoordinate field) => Field = field;
    }
}
