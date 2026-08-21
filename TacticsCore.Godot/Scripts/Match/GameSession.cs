using System;
using System.Threading;
using TacticsCore;
using TacticsCore.Agents;
using TacticsCore.Match;
using TacticsCore.Units;
using TacticsCoreGodot.Shared;

namespace TacticsCoreGodot.Match;

/// <summary>
/// Führt EINE Partie auf einem eigenen Hintergrundthread aus und meldet
/// jedes Ereignis sowie das Endergebnis über die vom Aufrufer bereit-
/// gestellte Marshalling-Funktion zurück auf den Hauptthread.
///
/// DAS BLOCKIERUNGSPROBLEM (siehe Punkt 2 der Aufgabenstellung): Diese
/// Klasse selbst kennt KEINEN Godot-Typ - sie bekommt lediglich einen
/// <c>runOnMainThread</c>-Delegaten übergeben (in der Praxis
/// <c>Node.CallDeferred</c>). <see cref="MatchController"/> und
/// <see cref="IAgent"/> bleiben dadurch UNVERÄNDERT und vollständig
/// synchron - die gesamte Partie inklusive aller <see cref="IAgent.DecideAction"/>-
/// Aufrufe läuft komplett auf dem Hintergrundthread. Ein menschlicher Agent
/// (siehe <see cref="GodotHumanInputSource"/>) blockiert dort so lange, bis
/// die Oberfläche (auf dem Hauptthread) eine Auswahl zurückmeldet - der
/// Godot-Hauptthread selbst wartet nie, er bleibt für Rendering und
/// Eingabe die ganze Zeit frei.
/// </summary>
public sealed class GameSession
{
    private readonly GameState _state;
    private readonly IAgent _teamAAgent;
    private readonly IAgent _teamBAgent;
    private readonly int _seed;
    private readonly Team? _startingTeamOverride;
    private readonly Action<Action> _runOnMainThread;

    /// <summary>
    /// Wird für JEDES von der Partie gemeldete Ereignis genau einmal auf dem
    /// HAUPTTHREAD ausgelöst - inklusive einer bereits eingefrorenen
    /// Momentaufnahme des Spielfelds unmittelbar NACH diesem Ereignis (siehe
    /// <see cref="BoardRenderModel"/>: der eigentliche <see cref="GameState"/>
    /// mutiert auf dem Hintergrundthread ja sofort weiter).
    /// </summary>
    public event Action<ReplayEntry, BoardRenderModel>? EventOccurred;

    /// <summary>Wird EINMAL auf dem Hauptthread ausgelöst, sobald die Partie beendet ist.</summary>
    public event Action<MatchResult>? MatchFinished;

    public GameSession(GameState state, IAgent teamAAgent, IAgent teamBAgent, int seed, Action<Action> runOnMainThread, Team? startingTeamOverride = null)
    {
        _state = state;
        _teamAAgent = teamAAgent;
        _teamBAgent = teamBAgent;
        _seed = seed;
        _startingTeamOverride = startingTeamOverride;
        _runOnMainThread = runOnMainThread;
    }

    /// <summary>Startet die Partie auf einem eigenen Hintergrundthread und kehrt sofort zurück.</summary>
    public void Start()
    {
        var thread = new Thread(RunOnBackgroundThread)
        {
            IsBackground = true,
            Name = "TacticsCore-Match",
        };
        thread.Start();
    }

    private void RunOnBackgroundThread()
    {
        // recordReplay:true zusätzlich zu onEvent, damit MatchResult.Replay
        // am Ende vollständig ist (z.B. um die live gespielte Partie danach
        // speichern zu können). WICHTIG dabei zu wissen: EffectSystem und
        // AbilitySystem protokollieren "EffectApplied"/"EffectTicked"/
        // "EffectExpired" INTERN direkt in diese Liste, nicht über
        // MatchController.onEvent - live erscheinen im Ereignisprotokoll
        // daher nur die von MatchController SELBST gemeldeten Ereignisse
        // (Bewegung, Angriff, Fähigkeitseinsatz, Zug-/Rundenwechsel,
        // Niederlage, Klon-Ereignisse, Partieende); das Spielfeld selbst
        // zeigt Effekte trotzdem immer korrekt (es liest GameState direkt).
        // Der Replay-Bildschirm (siehe ReplayTimeline) gleicht das für eine
        // GESPEICHERTE Partie im Nachhinein vollständig ab.
        var controller = new MatchController(_state, _teamAAgent, _teamBAgent, _seed, recordReplay: true, onEvent: OnReplayEntry);
        var result = controller.Run(_startingTeamOverride);
        _runOnMainThread(() => MatchFinished?.Invoke(result));
    }

    /// <summary>
    /// Wird SYNCHRON vom MatchController auf dem Hintergrundthread
    /// aufgerufen - deshalb wird das Render-Modell HIER, noch auf diesem
    /// Thread, aus dem aktuellen (unveränderten) Zustand eingefroren, bevor
    /// an den Hauptthread weitergereicht wird.
    /// </summary>
    private void OnReplayEntry(ReplayEntry entry)
    {
        var snapshot = BoardRenderModel.FromGameState(_state);
        _runOnMainThread(() => EventOccurred?.Invoke(entry, snapshot));
    }
}
