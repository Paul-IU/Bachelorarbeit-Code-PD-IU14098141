using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TacticsCore;
using TacticsCore.Agents;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCoreGodot.Match;

/// <summary>
/// Verbindet den UNVERÄNDERTEN <see cref="HumanAgent"/> mit der Godot-
/// Oberfläche. <see cref="ChooseAction"/> wird von <see cref="HumanAgent"/>
/// synchron auf dem HINTERGRUNDTHREAD aufgerufen (siehe
/// <see cref="GameSession"/>) und blockiert dort so lange, bis
/// <see cref="SubmitChoice"/> von einem UI-Klick auf dem Hauptthread
/// aufgerufen wurde - über die vom Aufgabensteller vorgeschlagene
/// <see cref="TaskCompletionSource{TResult}"/>-Brücke. Kümmert sich dabei
/// selbst um das Marshalling der "hier sind die legalen Züge"-Meldung auf
/// den Hauptthread, damit ein Aufrufer das nicht vergessen kann.
/// </summary>
public sealed class GodotHumanInputSource : IHumanInputSource
{
    private readonly Action<Action> _runOnMainThread;
    private readonly Action<Unit, GameState, IReadOnlyList<TurnAction>> _presentChoices;
    private TaskCompletionSource<TurnAction>? _pendingChoice;

    public GodotHumanInputSource(Action<Action> runOnMainThread, Action<Unit, GameState, IReadOnlyList<TurnAction>> presentChoices)
    {
        _runOnMainThread = runOnMainThread;
        _presentChoices = presentChoices;
    }

    public TurnAction ChooseAction(Unit unit, GameState state, IReadOnlyList<TurnAction> legalActions)
    {
        var tcs = new TaskCompletionSource<TurnAction>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingChoice = tcs;

        _runOnMainThread(() => _presentChoices(unit, state, legalActions));

        // Blockiert bewusst den HINTERGRUNDTHREAD (auf dem MatchController
        // diesen Aufruf ohnehin schon synchron tätigt) - der Godot-
        // Hauptthread selbst läuft währenddessen völlig normal weiter
        // (Rendering, Eingabeverarbeitung), bis SubmitChoice aufgerufen wird.
        return tcs.Task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Von einem UI-Klick-Handler auf dem HAUPTTHREAD aufzurufen, sobald der
    /// Mensch eine Wahl getroffen hat. Ein bereits abgeschlossener/erneuter
    /// Aufruf (z.B. Doppelklick) wird über TrySetResult stillschweigend
    /// ignoriert, statt eine Ausnahme zu werfen.
    /// </summary>
    public void SubmitChoice(TurnAction action) => _pendingChoice?.TrySetResult(action);
}
