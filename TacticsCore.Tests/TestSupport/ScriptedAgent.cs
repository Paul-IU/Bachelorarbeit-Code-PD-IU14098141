using TacticsCore.Agents;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Tests.TestSupport;

/// <summary>
/// Test-Doppelgänger für <see cref="IAgent"/>: liefert für eine per
/// <see cref="Script"/> vorprogrammierte Einheit eine feste Folge von
/// <see cref="TurnAction"/>en (FIFO) zurück, unabhängig von Zufall oder
/// Spielzustand - damit lassen sich Zugreihenfolge, Aktionsbudget und
/// Passen deterministisch und ohne Rückgriff auf Zufall testen. Ist die
/// Folge einer Einheit leer (oder wurde sie nie gescriptet), liefert der
/// Agent <see cref="TurnAction.EndTurn"/> - ein Test muss also nicht für
/// jede unbeteiligte Einheit explizit ein abschließendes Passen angeben.
///
/// Zählt zusätzlich pro Einheit die Anzahl der tatsächlichen Aufrufe
/// (<see cref="CallCount"/>) - Grundlage z.B. für den Test "eine vor ihrem
/// Zug besiegte Einheit wird nie gefragt".
/// </summary>
public sealed class ScriptedAgent : IAgent
{
    private readonly Dictionary<Guid, Queue<TurnAction>> _scripts = new();
    private readonly Dictionary<Guid, int> _callCounts = new();

    public void Script(Unit unit, params TurnAction[] actions) => _scripts[unit.Id] = new Queue<TurnAction>(actions);

    public int CallCount(Unit unit) => _callCounts.TryGetValue(unit.Id, out var count) ? count : 0;

    public TurnAction DecideAction(Unit unit, GameState state, TurnBudget budget, Random random)
    {
        _callCounts[unit.Id] = CallCount(unit) + 1;

        if (_scripts.TryGetValue(unit.Id, out var queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return TurnAction.EndTurn.Instance;
    }
}
