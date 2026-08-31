using Godot;
using TacticsCore;
using TacticsCore.Agents;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;
using TacticsCoreGodot.Animation;
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
///
/// LAYOUT (überarbeitet): drei Spalten in einem einzigen HBoxContainer, der
/// den gesamten Bildschirm ausfüllt - links/rechts FESTE Breite
/// (CustomMinimumSize ohne horizontales ExpandFill), Mitte (Spielfeld)
/// ExpandFill und bekommt dadurch automatisch den gesamten verbleibenden
/// Platz. Ein HBoxContainer legt seine Kinder grundsätzlich nebeneinander
/// an - eine Überlappung der drei Bereiche ist dadurch strukturell
/// ausgeschlossen, unabhängig von der Fenstergröße. Kein ScrollContainer
/// mehr um das Spielfeld (siehe HexBoardView: berechnet seine Hexgröße
/// selbst aus dem ihm zugewiesenen Platz).
/// </summary>
public partial class MatchScreen : Control
{
    private HexBoardView _board = null!;
    private Hud _hud = null!;
    private ActionPanel _actionPanel = null!;
    private EventLogPanel _log = null!;
    private EventAnimationPlayer _animator = null!;
    private SpinBox _speedSpinBox = null!;
    private CheckBox _skipCheckBox = null!;

    private GodotHumanInputSource? _pendingHumanInput;
    private HexCoordinate? _currentActingField;
    private EventDescriber _describer = new();
    private TurnOrderTracker _turnOrderTracker = new();

    /// <summary>
    /// Der Anzeigezustand UNMITTELBAR VOR dem jeweils nächsten Ereignis -
    /// wird dem Animator zusammen mit dem Zustand danach übergeben (siehe
    /// EventAnimationPlayer), z.B. damit eine besiegte Einheit für die
    /// Ausblend-Animation noch an ihrer letzten Position bekannt ist,
    /// obwohl das NACHHER-Modell sie bereits nicht mehr enthält.
    /// </summary>
    private BoardRenderModel? _lastModel;

    /// <summary>Der Benutzer möchte zum Hauptmenü zurück (z.B. nach Partieende).</summary>
    public event Action? BackRequested;

    public override void _Ready()
    {
        // WICHTIG: SetAnchorsPreset() allein reicht hier NICHT - bei einem
        // frisch erzeugten Control (Größe (0,0)) berechnet es die Offsets so,
        // dass das AKTUELLE (leere) Rechteck erhalten bleibt, statt sie auf 0
        // zu setzen (Godot versucht, die sichtbare Position beim Ankerwechsel
        // nicht springen zu lassen). Ergebnis: riesige negative Offsets, das
        // Control bleibt 0x0 groß, obwohl die Anker korrekt (0,0,1,1) sind -
        // exakt der beobachtete "alles oben links winzig"-Fehler.
        // SetAnchorsAndOffsetsPreset() setzt Anker UND Offsets fest auf die
        // Preset-Werte (bei FullRect: Offsets = 0), unabhängig vom aktuellen
        // Rechteck - das ist hier der richtige Aufruf.
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); // diese Steuerung selbst füllt ihren Elternknoten (MainController) aus

        var outerMargin = new MarginContainer();
        outerMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        outerMargin.AddThemeConstantOverride("margin_left", 8);
        outerMargin.AddThemeConstantOverride("margin_top", 8);
        outerMargin.AddThemeConstantOverride("margin_right", 8);
        outerMargin.AddThemeConstantOverride("margin_bottom", 8);

        var root = new HBoxContainer();
        root.AddThemeConstantOverride("separation", 8);

        // --- Links, feste Breite: Statusanzeige + Ereignisprotokoll ---
        var leftColumn = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(240, 0), // feste Breite (Vorgabe: 200-250px) - bewusst KEIN SizeFlagsHorizontal.ExpandFill
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        leftColumn.AddThemeConstantOverride("separation", 6);

        var backButton = new Button { Text = "< Zurück zum Menü" };
        backButton.Pressed += () => BackRequested?.Invoke();
        leftColumn.AddChild(backButton);

        // Animationsgeschwindigkeit/Überspringen (Vorgabe Teil B: "eine
        // einstellbare Geschwindigkeit, inklusive einer Option zum
        // vollständigen Überspringen").
        var animationRow = new HBoxContainer();
        animationRow.AddChild(new Label { Text = "Tempo:" });
        _speedSpinBox = new SpinBox { MinValue = 0.25, MaxValue = 8, Step = 0.25, Value = 1, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _speedSpinBox.ValueChanged += value => _animator.SpeedMultiplier = (float)value;
        animationRow.AddChild(_speedSpinBox);
        leftColumn.AddChild(animationRow);

        _skipCheckBox = new CheckBox { Text = "Animationen überspringen" };
        _skipCheckBox.Toggled += pressed => _animator.SkipAnimations = pressed;
        leftColumn.AddChild(_skipCheckBox);

        _hud = new Hud();
        leftColumn.AddChild(_hud);

        leftColumn.AddChild(new Label { Text = "Ereignisprotokoll:" });
        _log = new EventLogPanel
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill, // füllt den kompletten restlichen vertikalen Platz der linken Spalte
        };
        leftColumn.AddChild(_log);

        root.AddChild(leftColumn);

        // --- Mitte, dehnbar: Spielfeld - bekommt den gesamten verbleibenden Platz ---
        var boardPanel = new PanelContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        _board = new HexBoardView { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        boardPanel.AddChild(_board);
        root.AddChild(boardPanel);

        // --- Rechts, feste Breite: Aktionen (ActionPanel ist selbst schon ein PanelContainer) ---
        _actionPanel = new ActionPanel
        {
            CustomMinimumSize = new Vector2(320, 0), // feste Breite (Vorgabe: 300-350px) - bewusst KEIN SizeFlagsHorizontal.ExpandFill
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        root.AddChild(_actionPanel);

        outerMargin.AddChild(root);
        AddChild(outerMargin);

        // Animationsschicht (Vorgabe Teil B) - reine Anzeige-Erweiterung,
        // sitzt zwischen GameSession-Ereignissen und HexBoardView, verändert
        // aber nichts an der Spiellogik selbst (siehe EventAnimationPlayer).
        _animator = new EventAnimationPlayer { Board = _board };
        AddChild(_animator);
        _animator.BusyChanged += busy => _actionPanel.InteractionEnabled = !busy;

        _actionPanel.ActionChosen += OnActionChosen;
        _actionPanel.HighlightsChanged += (reachable, targets) => _board.SetHighlights(reachable, targets, _currentActingField);
        _board.FieldClicked += OnBoardFieldClicked;
    }

    /// <summary>Baut die Ausgangslage aus <paramref name="request"/> auf und startet die Partie.</summary>
    public void StartMatch(MatchSetupRequest request)
    {
        _log.Clear();
        _describer = new EventDescriber();
        _turnOrderTracker = new TurnOrderTracker();
        _actionPanel.Clear();
        _currentActingField = null;

        // Etwaige noch laufende Animation einer vorherigen Partie beenden,
        // bevor die neue beginnt - sonst könnte eine "eingefrorene"
        // Überlagerung (Position/Alpha) von der alten Partie übrig bleiben.
        _animator.Reset();

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

        var initialModel = BoardRenderModel.FromGameState(state);
        _lastModel = initialModel;
        _board.SetRenderModel(initialModel);
        _hud.UpdateFrom(initialModel);
        _hud.UpdateTurnOrder(_turnOrderTracker.BuildDisplayOrder(initialModel));

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
        // Löst bei mehreren Zielen auf demselben Feld (Vorgabe Punkt C.6)
        // ein Auswahl-Popup aus und meldet die endgültige Wahl über das
        // bereits bestehende ActionChosen-Ereignis (OnActionChosen) -
        // synchron bei eindeutigem Treffer, sonst erst nach der Auswahl.
        _actionPanel.TryResolveBoardClick(hex);
    }

    private void OnSessionEvent(ReplayEntry entry, BoardRenderModel model)
    {
        // Status (Runde/Held/Reihenfolge) und Ereignisprotokoll bleiben
        // bewusst LIVE, ohne auf die Animation zu warten - nur die
        // Spielfeld-DARSTELLUNG (Bewegung, Angriff, ...) wird zeitlich
        // gestreckt über den Animator abgespielt (Vorgabe Teil B). Das
        // Protokoll dient dem Nachvollziehen unabhängig vom Lauftempo der
        // Animation, genau wie Runde/Held/Budget-Anzeige zuvor auch.
        _turnOrderTracker.Register(entry, _lastModel, model);
        _hud.UpdateFrom(model);
        _hud.UpdateTurnOrder(_turnOrderTracker.BuildDisplayOrder(model));
        if (_describer.Describe(entry, model) is { } line)
        {
            _log.AppendLine(line);
        }

        _animator.Enqueue(entry, _lastModel, model);
        _lastModel = model;
    }

    private void OnMatchFinished(MatchResult result)
    {
        _hud.ShowMatchEnded(result.Winner, result.EndReason, result.RoundsPlayed);
        _actionPanel.Clear();
        _board.ClearHighlights();
    }
}
