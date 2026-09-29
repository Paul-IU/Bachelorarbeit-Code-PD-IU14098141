using System.Threading;

namespace TacticsCoreGodot.Match;

/// <summary>
/// Taktgeber für Agenten-Züge in der Live-Ansicht: nach jedem Zug eines
/// Agenten wartet der Hintergrundthread der Partie (siehe <see cref="GameSession"/>)
/// die eingestellte Verzögerung ab und bleibt stehen, solange pausiert ist.
///
/// Beeinflusst ausschließlich, WANN der <see cref="TacticsCore.Match.MatchController"/>
/// weiterrechnet, nie WAS er rechnet - keine Zufallsquelle, keine Uhrzeit im
/// Spielablauf. Der Verlauf ist mit und ohne Taktgeber identisch.
/// Alle Mitglieder sind threadsicher: Einstellungen kommen vom Hauptthread,
/// gewartet wird auf dem Hintergrundthread.
/// </summary>
public sealed class TurnPacer
{
    private readonly ManualResetEventSlim _running = new(initialState: true);
    private readonly CancellationTokenSource _cancel = new();
    private int _delayMilliseconds;

    public TurnPacer(int delayMilliseconds) => _delayMilliseconds = delayMilliseconds;

    /// <summary>Wartezeit nach jedem Agenten-Zug in Millisekunden (0 = ohne Verzögerung).</summary>
    public int DelayMilliseconds
    {
        get => Volatile.Read(ref _delayMilliseconds);
        set => Volatile.Write(ref _delayMilliseconds, Math.Max(0, value));
    }

    public bool IsPaused => !_running.IsSet;

    public void Pause() => _running.Reset();

    public void Resume() => _running.Set();

    /// <summary>Beendet jedes Warten dauerhaft (z.B. beim Verlassen der Partie) - die Partie rechnet danach ohne Pause zu Ende.</summary>
    public void Cancel()
    {
        _cancel.Cancel();
        _running.Set();
    }

    /// <summary>Vom Hintergrundthread nach einem Agenten-Zug aufzurufen.</summary>
    public void WaitAfterAgentTurn()
    {
        var token = _cancel.Token;
        if (token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            if (DelayMilliseconds > 0)
            {
                token.WaitHandle.WaitOne(DelayMilliseconds);
            }

            _running.Wait(token);
        }
        catch (OperationCanceledException)
        {
            // Partie wurde verlassen - ohne weitere Wartezeit fortsetzen.
        }
    }
}
