using Godot;
using TacticsCore.Heroes;
using TacticsCore.Persistence;
using TacticsCoreGodot.Replay;
using TacticsCoreGodot.Shared;
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Lädt eine gespeicherte Partie (<see cref="SavedMatch"/>, siehe
/// TacticsCore.Persistence bzw. den "export-replay"-Modus des
/// Konsolenprogramms) und spielt sie ab - schrittweise vor/zurück sowie
/// automatisch mit einstellbarer Geschwindigkeit (Vorgabe Punkt 8, "für
/// mich der wichtigste Teil"). Nutzt <see cref="ReplayTimeline"/>, um die
/// komplette Zeitleiste EINMALIG im Voraus zu berechnen - Navigation
/// danach ist reine Listen-Indizierung, "zurück" also genauso einfach wie
/// "vor".
/// </summary>
public partial class ReplayScreen : Control
{
    private HexBoardView _board = null!;
    private Hud _hud = null!;
    private EventLogPanel _log = null!;
    private LineEdit _pathField = null!;
    private Label _statusLabel = null!;
    private HSlider _positionSlider = null!;
    private Button _playPauseButton = null!;
    private SpinBox _speedSpinBox = null!;
    private Godot.Timer _playTimer = null!;
    private FileDialog _fileDialog = null!;

    private ReplayData? _replay;
    private int _currentIndex = -1;

    /// <summary>Der Benutzer möchte zum Hauptmenü zurück.</summary>
    public event Action? BackRequested;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect); // diese Steuerung selbst füllt ihren Elternknoten (MainController) aus

        var root = new HBoxContainer { AnchorRight = 1, AnchorBottom = 1 };

        var leftColumn = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };

        var backButton = new Button { Text = "< Zurück zum Menü" };
        backButton.Pressed += () => BackRequested?.Invoke();
        leftColumn.AddChild(backButton);

        var loadRow = new HBoxContainer();
        loadRow.AddChild(new Label { Text = "Datei:" });
        _pathField = new LineEdit
        {
            PlaceholderText = "Pfad zu einer gespeicherten Partie (.json)",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        loadRow.AddChild(_pathField);
        var browseButton = new Button { Text = "Durchsuchen..." };
        browseButton.Pressed += () => _fileDialog.Popup();
        loadRow.AddChild(browseButton);
        var loadButton = new Button { Text = "Laden" };
        loadButton.Pressed += OnLoadPressed;
        loadRow.AddChild(loadButton);
        leftColumn.AddChild(loadRow);

        _statusLabel = new Label { Text = "Keine Partie geladen." };
        leftColumn.AddChild(_statusLabel);

        _hud = new Hud();
        leftColumn.AddChild(_hud);

        var boardScroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        _board = new HexBoardView();
        boardScroll.AddChild(_board);
        leftColumn.AddChild(boardScroll);

        var controlsRow = new HBoxContainer();
        var stepBackButton = new Button { Text = "<< Schritt zurück" };
        stepBackButton.Pressed += () => Step(-1);
        controlsRow.AddChild(stepBackButton);

        _playPauseButton = new Button { Text = "Abspielen" };
        _playPauseButton.Pressed += TogglePlay;
        controlsRow.AddChild(_playPauseButton);

        var stepForwardButton = new Button { Text = "Schritt vor >>" };
        stepForwardButton.Pressed += () => Step(1);
        controlsRow.AddChild(stepForwardButton);

        controlsRow.AddChild(new Label { Text = "  Geschwindigkeit (Schritte/s):" });
        _speedSpinBox = new SpinBox { MinValue = 0.5, MaxValue = 20, Step = 0.5, Value = 3 };
        _speedSpinBox.ValueChanged += _ => UpdateTimerInterval();
        controlsRow.AddChild(_speedSpinBox);
        leftColumn.AddChild(controlsRow);

        _positionSlider = new HSlider { MinValue = 0, MaxValue = 0, Step = 1, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _positionSlider.ValueChanged += value => GoToIndex((int)value);
        leftColumn.AddChild(_positionSlider);

        root.AddChild(leftColumn);

        var rightColumn = new VBoxContainer { CustomMinimumSize = new Vector2(320, 0) };
        _log = new EventLogPanel { SizeFlagsVertical = SizeFlags.ExpandFill };
        rightColumn.AddChild(_log);
        root.AddChild(rightColumn);

        AddChild(root);

        _fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Access = FileDialog.AccessEnum.Filesystem,
            Filters = new[] { "*.json ; Gespeicherte Partien" },
            Size = new Vector2I(700, 500),
        };
        _fileDialog.FileSelected += path =>
        {
            _pathField.Text = path;
            OnLoadPressed();
        };
        AddChild(_fileDialog);

        _playTimer = new Godot.Timer { OneShot = false };
        _playTimer.Timeout += OnPlayTick;
        AddChild(_playTimer);
        UpdateTimerInterval();
    }

    private void OnLoadPressed()
    {
        string path = _pathField.Text.Trim();
        if (path.Length == 0)
        {
            _statusLabel.Text = "Bitte einen Dateipfad angeben.";
            return;
        }

        try
        {
            StopPlaying();
            string resolvedPath = ProjectSettings.GlobalizePath(path);
            var savedMatch = SavedMatchIO.Load(resolvedPath);
            var catalog = DefaultHeroes.CreateDefaultCatalog();
            var board = new GameBoard();
            _replay = ReplayTimeline.Build(savedMatch, catalog, board);

            _positionSlider.MaxValue = Math.Max(0, _replay.Frames.Count - 1);
            _statusLabel.Text = $"Geladen: {_replay.Frames.Count} Ereignisse.";
            GoToIndex(-1); // vor dem ersten Ereignis - zeigt die Ausgangsstellung
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Fehler beim Laden: {ex.Message}";
            _replay = null;
        }
    }

    private void Step(int direction) => GoToIndex(_currentIndex + direction);

    private void OnPlayTick()
    {
        if (_replay is null || _currentIndex >= _replay.Frames.Count - 1)
        {
            StopPlaying();
            return;
        }

        Step(1);
    }

    /// <summary>
    /// Springt zu einem beliebigen Zeitpunkt der Zeitleiste - reine Index-
    /// Navigation über die bereits fertig berechnete Liste (siehe
    /// ReplayTimeline), daher für Vor- UND Rückwärts-Schritte gleich
    /// einfach. Das Ereignisprotokoll wird dabei bewusst komplett neu aus
    /// den Einträgen bis zum Zielindex aufgebaut (mit einem frischen
    /// EventDescriber) statt inkrementell fortgeschrieben - so zeigt es bei
    /// jedem Sprung (auch rückwärts oder über den Schieberegler) exakt den
    /// zum aktuellen Zeitpunkt passenden Verlauf, ohne eine eigene
    /// Rückwärts-Logik zu brauchen.
    /// </summary>
    private void GoToIndex(int index)
    {
        if (_replay is null)
        {
            return;
        }

        index = Mathf.Clamp(index, -1, _replay.Frames.Count - 1);
        _currentIndex = index;

        _log.Clear();
        var describer = new EventDescriber();
        for (int i = 0; i <= index; i++)
        {
            _log.AppendLine(describer.Describe(_replay.Frames[i].Event, _replay.Frames[i].Model));
        }

        var currentModel = index == -1 ? _replay.InitialModel : _replay.Frames[index].Model;
        _board.SetRenderModel(currentModel);
        _hud.UpdateFrom(currentModel);
        _positionSlider.SetValueNoSignal(index);

        if (index == _replay.Frames.Count - 1 && index >= 0)
        {
            var lastEvent = _replay.Frames[index].Event;
            if (lastEvent.ActionType == TacticsCore.Match.ReplayActionType.MatchEnded)
            {
                _hud.ShowMatchEnded(lastEvent.Winner!.Value, lastEvent.EndReason!.Value, lastEvent.RoundNumber);
            }
        }
    }

    private void TogglePlay()
    {
        if (_playTimer.IsStopped())
        {
            if (_replay is null)
            {
                return;
            }

            if (_currentIndex >= _replay.Frames.Count - 1)
            {
                GoToIndex(-1); // am Ende erneut "Abspielen" beginnt von vorn
            }

            _playPauseButton.Text = "Pause";
            _playTimer.Start();
        }
        else
        {
            StopPlaying();
        }
    }

    private void StopPlaying()
    {
        _playTimer.Stop();
        _playPauseButton.Text = "Abspielen";
    }

    private void UpdateTimerInterval() => _playTimer.WaitTime = 1.0 / Math.Max(0.1, _speedSpinBox.Value);
}
