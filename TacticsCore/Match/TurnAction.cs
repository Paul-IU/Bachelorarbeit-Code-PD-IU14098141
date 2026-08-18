using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Eine von einem Agenten für seine Einheit gewählte Teilaktion innerhalb
/// ihres aktuellen Zugs. Bewusst als geschlossene Klassenhierarchie
/// modelliert (privater Basiskonstruktor - nur die drei hier definierten
/// Fälle sind möglich), weil C# keine echten Discriminated Unions kennt,
/// eine erschöpfende switch-Auswertung im <see cref="MatchController"/>
/// aber trotzdem gewünscht ist.
///
/// ERWEITERBARKEIT: Eine spätere Fähigkeit als Alternative zum Grundangriff
/// wird hier einfach ein weiterer Fall (z.B. <c>TurnAction.UseAbility</c>),
/// ohne dass sich an <see cref="TurnBudget"/> oder der Zugschleife im
/// MatchController grundsätzlich etwas ändern muss - Move und Attack/
/// UseAbility bleiben unabhängige Budget-Slots.
/// </summary>
public abstract class TurnAction
{
    private TurnAction()
    {
    }

    /// <summary>Bewegung auf <see cref="Target"/> (siehe <see cref="Unit.TryMove"/>). Verbraucht den Bewegungs-Slot.</summary>
    public sealed class Move : TurnAction
    {
        public HexCoordinate Target { get; }
        public Move(HexCoordinate target) => Target = target;
    }

    /// <summary>Grundangriff auf <see cref="Target"/> (siehe <see cref="Unit.TryAttack"/>). Verbraucht den Angriffs-Slot.</summary>
    public sealed class Attack : TurnAction
    {
        public Unit Target { get; }
        public Attack(Unit target) => Target = target;
    }

    /// <summary>
    /// Beendet den Zug aktiv, unabhängig davon, ob noch Budget übrig ist -
    /// das ist das "Passen" aus Anforderung 3 ("auf beides darf verzichtet
    /// werden, auch komplettes Passen ist legal").
    /// </summary>
    public sealed class EndTurn : TurnAction
    {
        /// <summary>EndTurn trägt keine Nutzdaten - eine gemeinsame Instanz genügt.</summary>
        public static readonly EndTurn Instance = new();
        private EndTurn()
        {
        }
    }
}

/// <summary>
/// Zeigt einem Agenten an, welche Aktionskomponenten für den aktuellen Zug
/// noch verfügbar sind. Siehe Anforderung 3: genau eine Bewegung UND genau
/// ein Angriff pro Zug, in freier Reihenfolge.
/// </summary>
public readonly record struct TurnBudget(bool CanMove, bool CanAttack)
{
    /// <summary>Ob überhaupt noch irgendeine Aktion möglich ist (sonst ist der Zug ohnehin zwangsläufig zu Ende).</summary>
    public bool HasAnyBudget => CanMove || CanAttack;
}
