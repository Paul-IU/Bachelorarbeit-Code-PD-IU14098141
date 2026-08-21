using TacticsCore.Abilities;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Referenzagent, der ausschließlich über die vom MatchController
/// übergebene, seed-gesteuerte <see cref="Random"/>-Instanz entscheidet -
/// keine eigene Zufallsquelle, kein eigener Zustand (siehe
/// <see cref="IAgent"/>).
///
/// Strategie: pro Aufruf wird zufällig zwischen den noch verfügbaren
/// Aktionstypen entschieden - Bewegung (falls <see cref="TurnBudget.CanMove"/>)
/// sowie, für den Aktions-Slot (<see cref="TurnBudget.CanAct"/>), Angriff
/// UND/ODER Fähigkeitseinsatz (bei Helden mit gerade nutzbarer aktiver
/// Fähigkeit) - sofern es dafür überhaupt etwas Sinnvolles zu wählen gibt
/// (mindestens ein gültiges Ziel), und dann gleichverteilt eines der
/// jeweils legalen Ziele. Stehen keine Optionen mehr zur Verfügung, wird
/// der Zug beendet.
///
/// Wichtig: dieser Agent musste für die Einführung von Fähigkeiten NICHT
/// strukturell geändert werden (Anforderung 12: "der bestehende
/// Zufallsagent muss ohne Anpassung damit umgehen können") - die einzige
/// Änderung ist ein weiterer, gleichrangiger Eintrag in derselben
/// "verfügbare Aktionstypen"-Liste wie Bewegung und Angriff schon vorher.
/// </summary>
public sealed class RandomAgent : IAgent
{
    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        IReadOnlyList<HexCoordinate>? legalMoves = null;
        IReadOnlyList<Unit>? legalAttacks = null;
        IReadOnlyList<AbilityUseOption>? legalAbilityUses = null;

        var availableActionTypes = new List<int>(3); // 0 = Bewegung, 1 = Angriff, 2 = Fähigkeit

        if (budget.CanMove)
        {
            legalMoves = unit.GetLegalMoves(state);
            if (legalMoves.Count > 0)
            {
                availableActionTypes.Add(0);
            }
        }

        if (budget.CanAct)
        {
            legalAttacks = unit.GetLegalAttacks(state);
            if (legalAttacks.Count > 0)
            {
                availableActionTypes.Add(1);
            }

            if (unit is Hero hero)
            {
                legalAbilityUses = AbilitySystem.GetLegalAbilityUses(hero, state);
                if (legalAbilityUses.Count > 0)
                {
                    availableActionTypes.Add(2);
                }
            }
        }

        if (availableActionTypes.Count == 0)
        {
            return TurnAction.EndTurn.Instance;
        }

        int chosenActionType = availableActionTypes[random.Next(availableActionTypes.Count)];

        switch (chosenActionType)
        {
            case 0:
                var target = legalMoves![random.Next(legalMoves.Count)];
                return new TurnAction.Move(target);

            case 1:
                var attackTarget = legalAttacks![random.Next(legalAttacks.Count)];
                return new TurnAction.Attack(attackTarget);

            default:
                var abilityUse = legalAbilityUses![random.Next(legalAbilityUses.Count)];
                return new TurnAction.UseAbility(abilityUse.Target, abilityUse.Mode);
        }
    }
}
