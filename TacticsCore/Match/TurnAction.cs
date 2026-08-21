using TacticsCore.Abilities;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Eine von einem Agenten für seine Einheit gewählte Teilaktion innerhalb
/// ihres aktuellen Zugs. Bewusst als geschlossene Klassenhierarchie
/// modelliert (privater Basiskonstruktor - nur die hier definierten Fälle
/// sind möglich), weil C# keine echten Discriminated Unions kennt, eine
/// erschöpfende switch-Auswertung im <see cref="MatchController"/> aber
/// trotzdem gewünscht ist.
///
/// AKTIONSBUDGET (Anforderung 6 aus Teil B): pro Zug genau eine Bewegung
/// PLUS genau eine Aktion, wobei die Aktion entweder ein Grundangriff
/// (<see cref="Attack"/>) ODER eine Heldenfähigkeit
/// (<see cref="UseAbility"/>) ist - beide verbrauchen denselben
/// "Aktions"-Budget-Slot (siehe <see cref="TurnBudget.CanAct"/>). Das ist
/// der Grund, warum das frühere <c>TurnBudget.CanAttack</c> zu
/// <c>CanAct</c> wurde: die Rundenverwaltung selbst musste dafür NICHT
/// umgebaut werden, nur dieser eine Slot heißt jetzt allgemeiner und der
/// switch in MatchController bekam einen weiteren Fall.
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

    /// <summary>Grundangriff auf <see cref="Target"/> (siehe <see cref="Unit.TryAttack"/>). Verbraucht den Aktions-Slot.</summary>
    public sealed class Attack : TurnAction
    {
        public Unit Target { get; }
        public Attack(Unit target) => Target = target;
    }

    /// <summary>Einsatz der Heldenfähigkeit auf <see cref="Target"/> (ggf. mit <see cref="Mode"/>). Verbraucht denselben Aktions-Slot wie <see cref="Attack"/>.</summary>
    public sealed class UseAbility : TurnAction
    {
        public AbilityTarget Target { get; }
        public AbilityMode? Mode { get; }
        public UseAbility(AbilityTarget target, AbilityMode? mode = null)
        {
            Target = target;
            Mode = mode;
        }
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
/// noch verfügbar sind: genau eine Bewegung UND genau eine Aktion
/// (Grundangriff ODER Heldenfähigkeit), in freier Reihenfolge.
/// </summary>
public readonly record struct TurnBudget(bool CanMove, bool CanAct)
{
    /// <summary>Ob überhaupt noch irgendeine Aktion möglich ist (sonst ist der Zug ohnehin zwangsläufig zu Ende).</summary>
    public bool HasAnyBudget => CanMove || CanAct;
}
