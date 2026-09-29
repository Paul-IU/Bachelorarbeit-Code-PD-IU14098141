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
/// AUSWAHLREGEL (namensgebend, siehe auch <see cref="RandomAgentUniform"/>
/// für die zweite, bewusst ANDERS gewichtete Variante): pro Aufruf wird
/// ZUERST zufällig zwischen den noch verfügbaren AKTIONSTYPEN entschieden -
/// Bewegung (falls <see cref="TurnBudget.CanMove"/>) sowie, für den
/// Aktions-Slot (<see cref="TurnBudget.CanAct"/>), Angriff UND/ODER
/// Fähigkeitseinsatz (bei Helden mit gerade nutzbarer aktiver Fähigkeit) -
/// sofern es dafür überhaupt etwas Sinnvolles zu wählen gibt (mindestens
/// ein gültiges Ziel) - und ERST DANACH, gleichverteilt, eines der jeweils
/// legalen Ziele DIESES Typs. Stehen keine Optionen mehr zur Verfügung,
/// wird der Zug beendet.
///
/// BACHELORARBEIT-ERWEITERUNG - UNBEABSICHTIGTE VERZERRUNG DIESER REGEL
/// (Grund für die Einführung von <see cref="RandomAgentUniform"/> als
/// Vergleichsagent): weil die Wahl zweistufig ist (erst Typ, dann Ziel
/// INNERHALB des Typs), hängt die Wahrscheinlichkeit eines EINZELNEN
/// Zuges nicht nur von der Gesamtzahl aller legalen Züge ab, sondern auch
/// davon, wie sie sich auf die drei Typen verteilen. Zwei konkrete Folgen:
///   - Ein Held mit dauerhaft verfügbarer Fähigkeit (keine Abklingzeit,
///     z.B. Elfe) setzt sie im Aktions-Slot mit ungefähr 50% Wahrscheinlichkeit
///     ein (ein Typ von zweien: Angriff ODER Fähigkeit), UNABHÄNGIG davon,
///     ob es dafür ein oder zehn legale Ziele gibt.
///   - Ein Held mit Abklingzeit greift in der Zwischenzeit (Fähigkeit nicht
///     verfügbar, kein dritter Typ im Angebot) automatisch häufiger an, weil
///     der Angriffs-Typ dann öfter der einzig verbleibende ist.
/// <see cref="RandomAgentUniform"/> wählt stattdessen gleichverteilt über
/// ALLE einzelnen legalen Züge hinweg - ein Zug mit zehn Zielen ist dort
/// zehnmal so wahrscheinlich wie einer mit einem Ziel, unabhängig von der
/// Aufteilung auf Typen. Beide Regeln sind gültige, aber UNTERSCHIEDLICHE
/// Annahmen über "zufälliges Verhalten" - welche für eine Balancing-Aussage
/// geeigneter ist, hängt von der Fragestellung ab (siehe
/// BatchSimulationRunnerTests für den direkten Vergleich beider Agenten).
///
/// Wichtig: dieser Agent musste für die Einführung von Fähigkeiten NICHT
/// strukturell geändert werden (Anforderung 12: "der bestehende
/// Zufallsagent muss ohne Anpassung damit umgehen können") - die einzige
/// Änderung ist ein weiterer, gleichrangiger Eintrag in derselben
/// "verfügbare Aktionstypen"-Liste wie Bewegung und Angriff schon vorher.
/// </summary>
public sealed class RandomAgentByActionType : IAgent
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
