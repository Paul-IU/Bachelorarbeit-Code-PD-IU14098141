using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Entscheidungslogik für eine Seite der Partie. Der
/// <see cref="MatchController"/> kennt ausschließlich diese Schnittstelle
/// und trifft keinerlei Annahme über die konkrete Strategie dahinter -
/// aktuell existieren <see cref="RandomAgentByActionType"/> und
/// <see cref="RandomAgentUniform"/> (zwei Varianten mit unterschiedlicher
/// Auswahlregel, siehe dort), spätere regelbasierte oder gelernte Agenten
/// treten hier an dieselbe Stelle, ohne dass sich an
/// der Rundenverwaltung etwas ändern müsste.
///
/// Ein Agent trägt bewusst KEINEN eigenen Zufallszustand (kein Random-Feld,
/// keine statische Zustandshaltung) - jede benötigte Zufallsentscheidung
/// bekommt die für die gesamte Partie zuständige, seed-gesteuerte
/// <see cref="Random"/>-Instanz explizit vom MatchController übergeben.
/// Das hält Implementierungen zustandslos und über mehrere Partien hinweg
/// gefahrlos wiederverwendbar, ohne dass an dieser Stelle eine "versteckte"
/// Zufallsquelle entstehen könnte.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Wählt die nächste Teilaktion für <paramref name="unit"/> innerhalb
    /// ihres aktuellen Zugs. Wird vom MatchController wiederholt
    /// aufgerufen, bis eine <see cref="TurnAction.EndTurn"/>-Aktion
    /// zurückkommt oder das per <paramref name="budget"/> beschriebene
    /// Aktionsbudget vollständig verbraucht ist. <paramref name="state"/>
    /// spiegelt dabei bereits alle innerhalb dieses Zugs zuvor
    /// ausgeführten Teilaktionen wider (z.B. die neue Position nach einer
    /// bereits erfolgten Bewegung).
    /// </summary>
    TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random);
}
