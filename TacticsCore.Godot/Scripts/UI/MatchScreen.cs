using Godot;
using TacticsCore;
using TacticsCore.Agents;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;
using TacticsCoreGodot.Match;
using TacticsCoreGodot.Setup;
using TacticsCoreGodot.Shared;
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Steuert eine einzelne, LIVE gespielte Partie (Vorgabe Punkt 6/7): baut
/// die Ausgangslage aus der Aufstellung, startet eine
/// <see cref="GameSession"/> auf einem Hintergrundthread (siehe dortige
/// Dokumentation zum Blockierungsproblem) und verbindet deren Ereignisse
/// mit Spielfeld-Anzeige, HUD, Aktionsauswahl und Ereignisprotokoll -
/// alles über die öffentliche API von TacticsCore, ohne eine einzige Regel
/// selbst zu berechnen.
/// </summary>
public partial class MatchScreen : Control
{
    private HexBoardView _board = null!;
    private Hud _hud = null!;
    private ActionPanel _actionPanel = null!;
    private EventLogPanel _log = null!;

    private GodotHumanInputSource? _pendingHumanInput;
    private HexCoordinate? _currentActingField;
    private EventDescriber _describer = new();

    /// <summary>Der Benutzer möchte zum Hauptmenü zurück (z.B. nach Partieende).</summary>
    public event Action? BackRequested;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect); // diese Steuerung selbst füllt ihren Elternknoten (MainController) aus

        var root = new HBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
        };

        var leftColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 3f, // bekommt den Großteil der Breite - das Spielfeld hat Priorität
        };

        var backButton = new Button { Text = "< Zurück zum Menü" };
        backButton.Pressed += () => BackRequested?.Invoke();
        leftColumn.AddChild(backButton);

        _hud = new Hud();
        leftColumn.AddChild(_hud);

        var boardScroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        _board = new HexBoardView();
        boardScroll.AddChild(_board);
        leftColumn.AddChild(boardScroll);

        root.AddChild(leftColumn);

        var rightColumn = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(420, 0), // Mindestbreite, kann bei Bedarf darüber wachsen
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 1f,
        };
        _actionPanel = new ActionPanel { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 1.2f };
        rightColumn.AddChild(_actionPanel);
        _log = new EventLogPanel { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill, SizeFlagsStretchRatio = 1f };
        rightColumn.AddChild(_log);
        root.AddChild(rightColumn);

        AddChild(root);

        _actionPanel.ActionChosen += OnActionChosen;
        _actionPanel.HighlightsChanged += (reachable, targets) => _board.SetHighlights(reachable, targets, _currentActingField);
        _board.FieldClicked += OnBoardFieldClicked;
    }

    /// <summary>Baut die Ausgangslage aus <paramref name="request"/> auf und startet die Partie.</summary>
    public void StartMatch(MatchSetupRequest request)
    {
        _log.Clear();
        _describer = new EventDescriber();
        _actionPanel.Clear();
        _currentActingField = null;

        var board = new GameBoard();
        var state = new GameState(board);

        foreach (var (hero, field) in request.TeamA.Assignments)
        {
            state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamA, field, hero));
        }
        foreach (var (hero, field) in request.TeamB.Assignments)
        {
            state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamB, field, hero));
        }

        _board.SetRenderModel(BoardRenderModel.FromGameState(state));

        var teamAAgent = BuildAgent(request.TeamA.IsHuman);
        var teamBAgent = BuildAgent(request.TeamB.IsHuman);

        var session = new GameSession(state, teamAAgent, teamBAgent, request.Seed, RunOnMainThread);
        session.EventOccurred += OnSessionEvent;
        session.MatchFinished += OnMatchFinished;
        session.Start();
    }

    private IAgent BuildAgent(bool isHuman) => isHuman ? new HumanAgent(CreateInputSource()) : new RandomAgent();

    private GodotHumanInputSource CreateInputSource()
    {
        GodotHumanInputSource? source = null;
        source = new GodotHumanInputSource(RunOnMainThread, (unit, _, legalActions) =>
        {
            _pendingHumanInput = source;
            PresentHumanChoice(unit, legalActions);
        });
        return source;
    }

    /// <summary>
    /// Führt <paramref name="action"/> "irgendwann" auf dem Hauptthread aus
    /// (siehe GameSession/GodotHumanInputSource) - der in Godot-C# übliche
    /// Weg dafür ist <see cref="GodotObject.CallDeferred"/> mit einem
    /// in ein <see cref="Callable"/> verpackten Delegaten.
    /// </summary>
    private void RunOnMainThread(Action action) => CallDeferred(nameof(InvokeCallable), Callable.From(action));

    private void InvokeCallable(Callable callable) => callable.Call();

    private void PresentHumanChoice(Unit unit, IReadOnlyList<TurnAction> legalActions)
    {
        _currentActingField = unit.Position;
        _actionPanel.Present(unit, legalActions);
    }

    private void OnActionChosen(TurnAction action)
    {
        _pendingHumanInput?.SubmitChoice(action);
        _pendingHumanInput = null;
        _actionPanel.Clear();
        _currentActingField = null;
    }

    private void OnBoardFieldClicked(HexCoordinate hex)
    {
        var action = _actionPanel.TryResolveBoardClick(hex);
        if (action is not null)
        {
            OnActionChosen(action);
        }
    }

    private void OnSessionEvent(ReplayEntry entry, BoardRenderModel model)
    {
        _board.SetRenderModel(model);
        _hud.UpdateFrom(model);
        _log.AppendLine(_describer.Describe(entry, model));
    }

    private void OnMatchFinished(MatchResult result)
    {
        _hud.ShowMatchEnded(result.Winner, result.EndReason, result.RoundsPlayed);
        _actionPanel.Clear();
        _board.ClearHighlights();
    }
}
