using TacticsCore.Abilities;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Zweiter Referenzagent (Bachelorarbeit-Erweiterung), der ebenfalls
/// ausschließlich über die vom MatchController übergebene, seed-gesteuerte
/// <see cref="Random"/>-Instanz entscheidet - keine eigene Zufallsquelle,
/// kein eigener Zustand (siehe <see cref="IAgent"/>).
///
/// AUSWAHLREGEL (namensgebend, bewusst ANDERS als <see cref="RandomAgentByActionType"/>,
/// siehe dort für die ausführliche Begründung des Unterschieds): statt
/// zuerst einen Aktionstyp und danach ein Ziel INNERHALB dieses Typs zu
/// wählen, werden ALLE aktuell legalen EINZELZÜGE - jede Bewegung, jeder
/// Angriff, jeder Fähigkeitseinsatz (Ziel UND Modus jeweils einzeln
/// aufgeführt) - zu EINER GEMEINSAMEN Liste zusammengeführt, aus der
/// GLEICHVERTEILT gezogen wird. Ein Zug mit zehn möglichen Zielen ist damit
/// zehnmal so wahrscheinlich wie einer mit einem einzigen Ziel - unabhängig
/// davon, ob beide demselben oder unterschiedlichen Aktionstypen angehören.
/// Ist die Liste leer, wird der Zug beendet.
///
/// REIHENFOLGE DER ZUSAMMENGEFÜHRTEN LISTE (Determinismus, Anforderung 11):
/// IMMER Bewegungen, dann Angriffe, dann Fähigkeitseinsätze - dieselbe
/// Reihenfolge bei JEDEM Aufruf, nie von einer Wörterbuch- oder
/// Mengen-Iteration abhängig (<see cref="Unit.GetLegalMoves"/>,
/// <see cref="Unit.GetLegalAttacks"/> und <see cref="AbilitySystem.GetLegalAbilityUses"/>
/// liefern ihrerseits bereits jeweils stabil sortierte Listen - das
/// simple Aneinanderhängen dreier stabiler Listen ist selbst wieder
/// stabil). Ohne diese feste Reihenfolge wäre der an
/// <c>random.Next(combined.Count)</c> übergebene Index zwar weiterhin
/// deterministisch INNERHALB eines Laufs, würde aber bei identischem Seed
/// nicht mehr zuverlässig auf denselben KONKRETEN Zug zeigen, sobald sich
/// z.B. die interne Reihenfolge einer zugrunde liegenden Datenstruktur
/// ändert - genau das würde die geforderte Reproduzierbarkeit
/// unterlaufen, ohne dass sich am Seed selbst etwas geändert hätte.
///
/// Trägt (wie <see cref="RandomAgentByActionType"/>) KEINEN eigenen
/// Zustand und legt KEINE eigene Zufallsquelle an - die Aufteilung in
/// Bewegungs- und Aktionsbudget (<see cref="TurnBudget"/>) bleibt exakt
/// dieselbe, nur die Auswahlregel INNERHALB eines Aufrufs unterscheidet
/// sich.
/// </summary>
public sealed class RandomAgentUniform : IAgent
{
    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        // Bewusst als EINE Liste aus drei fest geordneten Anteilen statt
        // dreier Reihen mit gewichteter Typenwahl (siehe Klassendokumentation
        // für die Begründung, warum die Reihenfolge deterministisch fest
        // sein muss). Kapazität grob geschätzt, um unnötige Nachvergrößerungen
        // zu vermeiden - reine Optimierung, keine Verhaltensänderung.
        var legalTurns = new List<TurnAction>();

        if (budget.CanMove)
        {
            foreach (var move in unit.GetLegalMoves(state))
            {
                legalTurns.Add(new TurnAction.Move(move));
            }
        }

        if (budget.CanAct)
        {
            foreach (var attackTarget in unit.GetLegalAttacks(state))
            {
                legalTurns.Add(new TurnAction.Attack(attackTarget));
            }

            if (unit is Hero hero)
            {
                foreach (var abilityUse in AbilitySystem.GetLegalAbilityUses(hero, state))
                {
                    legalTurns.Add(new TurnAction.UseAbility(abilityUse.Target, abilityUse.Mode));
                }
            }
        }

        if (legalTurns.Count == 0)
        {
            return TurnAction.EndTurn.Instance;
        }

        return legalTurns[random.Next(legalTurns.Count)];
    }
}
