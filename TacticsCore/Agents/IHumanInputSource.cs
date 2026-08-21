using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Austauschbare Eingabequelle für <see cref="HumanAgent"/> (Anforderung 13):
/// bekommt die aktuell handelnde Einheit, den Spielzustand und die Liste
/// legaler Züge, und liefert GENAU EINEN davon zurück. Die Konsole
/// (<c>ConsoleHumanInputSource</c> im Demo-Projekt) ist nur EINE mögliche
/// Umsetzung - eine spätere grafische Oberfläche implementiert dieselbe
/// Schnittstelle (z.B. "warte auf Klick auf eine der markierten Optionen"),
/// ohne dass <see cref="HumanAgent"/> selbst geändert werden müsste.
/// </summary>
public interface IHumanInputSource
{
    /// <summary>
    /// Wählt EINEN Zug aus <paramref name="legalActions"/> aus (per
    /// Objekt-Identität, nicht Neu-Konstruktion - der Rückgabewert muss ein
    /// Element DERSELBEN Liste sein). Die Liste ist niemals leer: ein
    /// aktives Passen (<see cref="TurnAction.EndTurn"/>) ist immer
    /// enthalten.
    /// </summary>
    TurnAction ChooseAction(Unit unit, GameState state, IReadOnlyList<TurnAction> legalActions);
}
