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
/// Aktionstypen (Bewegung/Angriff, siehe <see cref="TurnBudget"/>)
/// entschieden, sofern es dafür überhaupt etwas Sinnvolles zu wählen gibt
/// (bei Angriff: mindestens ein gültiges Ziel vorhanden), und dann
/// gleichverteilt eines der jeweils legalen Ziele gewählt. Stehen keine
/// Optionen mehr zur Verfügung, wird der Zug beendet.
///
/// Dient in diesem Schritt als einzige konkrete Agentenimplementierung -
/// insbesondere für die Plausibilitätsprüfung: bei identischen
/// Standardfiguren auf beiden Seiten sollte die Siegverteilung über viele
/// Partien mit diesem Agenten auf beiden Seiten annähernd ausgeglichen
/// sein (siehe Konsolenprogramm und PlausibilityTests).
/// </summary>
public sealed class RandomAgent : IAgent
{
    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        // Für jeden aktuell noch verfügbaren und sinnvollen Aktionstyp die
        // jeweiligen Ziel-Optionen einmal abfragen. "Sinnvoll" heißt hier:
        // Bewegung ist immer eine Option (das eigene Feld - Stehenbleiben -
        // ist stets Teil der legalen Ziele), Angriff nur, wenn es
        // mindestens ein gültiges Ziel gibt.
        IReadOnlyList<HexCoordinate>? legalMoves = null;
        IReadOnlyList<Unit>? legalAttacks = null;

        var availableActionTypes = new List<int>(2); // 0 = Bewegung, 1 = Angriff

        if (budget.CanMove)
        {
            legalMoves = unit.GetLegalMoves(state);
            if (legalMoves.Count > 0)
            {
                availableActionTypes.Add(0);
            }
        }

        if (budget.CanAttack)
        {
            legalAttacks = unit.GetLegalAttacks(state);
            if (legalAttacks.Count > 0)
            {
                availableActionTypes.Add(1);
            }
        }

        if (availableActionTypes.Count == 0)
        {
            return TurnAction.EndTurn.Instance;
        }

        int chosenActionType = availableActionTypes[random.Next(availableActionTypes.Count)];

        if (chosenActionType == 0)
        {
            var target = legalMoves![random.Next(legalMoves.Count)];
            return new TurnAction.Move(target);
        }

        var attackTarget = legalAttacks![random.Next(legalAttacks.Count)];
        return new TurnAction.Attack(attackTarget);
    }
}
