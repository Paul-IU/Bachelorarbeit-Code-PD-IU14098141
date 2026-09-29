using TacticsCore.Abilities;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// <see cref="IAgent"/>-Implementierung, die selbst KEINE Entscheidung
/// trifft, sondern sie an eine austauschbare <see cref="IHumanInputSource"/>
/// delegiert (Anforderung 12/13). Baut dafür die vollständige Liste aller
/// aktuell legalen Züge (Bewegung, Angriff, Fähigkeitseinsatz, Passen) auf -
/// stabil sortiert wie überall sonst im System - und übergibt sie der
/// Eingabequelle zur Auswahl.
///
/// Bewusst genauso zustandslos wie <see cref="RandomAgentByActionType"/>
/// bzw. <see cref="RandomAgentUniform"/> (siehe dortige Begründung) - der
/// einzige "Zustand" ist die vom Aufrufer übergebene
/// Eingabequelle, die HumanAgent selbst nicht verändert.
/// </summary>
public sealed class HumanAgent : IAgent
{
    private readonly IHumanInputSource _inputSource;

    public HumanAgent(IHumanInputSource inputSource) => _inputSource = inputSource;

    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        var legalActions = BuildLegalActions(unit, state, budget);
        return _inputSource.ChooseAction(unit, state, legalActions);
    }

    /// <summary>
    /// Reihenfolge: Bewegungen (stabil nach (q, r), siehe
    /// <see cref="Unit.GetLegalMoves"/>), dann Angriffe (stabil nach
    /// (q, r, UnitId), siehe <see cref="Unit.GetLegalAttacks"/>), dann
    /// Fähigkeitseinsätze (stabil sortiert, siehe
    /// <see cref="AbilitySystem.GetLegalAbilityUses"/>), zuletzt das aktive
    /// Passen. Insgesamt also selbst wieder eine stabile, reproduzierbare
    /// Reihenfolge - wichtig für eine nummerierte Konsolen-/Oberflächen-
    /// Anzeige, die sich zwischen zwei Abfragen desselben Zustands nicht
    /// ändern darf.
    /// </summary>
    private static List<TurnAction> BuildLegalActions(Unit unit, GameState state, TurnBudget budget)
    {
        var actions = new List<TurnAction>();

        if (budget.CanMove)
        {
            foreach (var target in unit.GetLegalMoves(state))
            {
                actions.Add(new TurnAction.Move(target));
            }
        }

        if (budget.CanAct)
        {
            foreach (var target in unit.GetLegalAttacks(state))
            {
                actions.Add(new TurnAction.Attack(target));
            }

            if (unit is Hero hero)
            {
                foreach (var option in AbilitySystem.GetLegalAbilityUses(hero, state))
                {
                    actions.Add(new TurnAction.UseAbility(option.Target, option.Mode));
                }
            }
        }

        actions.Add(TurnAction.EndTurn.Instance);
        return actions;
    }
}
