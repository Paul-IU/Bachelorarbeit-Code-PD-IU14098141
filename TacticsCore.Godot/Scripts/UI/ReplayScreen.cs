using Godot;
using TacticsCore.Heroes;
using TacticsCore.Persistence;
using TacticsCoreGodot.Animation;
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
///
/// LAYOUT (überarbeitet): dieselbe Drei-Spalten-Struktur wie MatchScreen
/// (siehe dortige Dokumentation) - links Status/Protokoll, Mitte Spielfeld,
/// rechts fest breit. Anstelle der Aktions-Schaltflächen stehen rechts hier
/// die Datei-Auswahl und die Wiedergabesteuerung (vor/zurück/abspielen/
/// Geschwindigkeit), wie gewünscht "an die Stelle der Aktionen".
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
    private EventAnimationPlayer _animator = null!;
    private SpinBox _animationSpeedSpinBox = null!;
    private CheckBox _skipCheckBox = null!;

    private ReplayData? _replay;
    private int _currentIndex = -1;

    /// <summary>
    /// Wachsen bei einem einzelnen animierten Vorwärtsschritt inkrementell
    /// mit (siehe GoToIndex) - bei jedem Sprung/Rückwärtsschritt komplett
    /// neu aufgebaut, damit sie unabhängig vom Navigationsverlauf immer den
    /// zum aktuellen Index passenden Stand zeigen.
    /// </summary>
    private EventDescriber _describer = new();
    private TurnOrderTracker _turnOrderTracker = new();

    /// <summary>Der Benutzer möchte zum Hauptmenü zurück.</summary>
    public event Action? BackRequested;

    public override void _Ready()
    {
        // Siehe MatchScreen für die Begründung: SetAnchorsAndOffsetsPreset()
        // statt SetAnchorsPreset(), sonst bleiben Offsets bei einem frisch
        // erzeugten (0x0 großen) Control auf riesigen negativen Werten
        // stehen statt auf 0 - das Control würde trotz korrekter Anker
        // 0x0 groß bleiben.
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); // diese Steuerung selbst füllt ihren Elternknoten (MainController) aus

        var outerMargin = new MarginContainer();
        outerMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        outerMargin.AddThemeConstantOverride("margin_left", 8);
        outerMargin.AddThemeConstantOverride("margin_top", 8);
        outerMargin.AddThemeConstantOverride("margin_right", 8);
        outerMargin.AddThemeConstantOverride("margin_bottom", 8);

        var root = new HBoxContainer();
        root.AddThemeConstantOverride("separation", 8);

        // --- Links, feste Breite: Statusanzeige + Ereignisprotokoll (identisch zu MatchScreen) ---
        var leftColumn = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(240, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        leftColumn.AddThemeConstantOverride("separation", 6);

        var backButton = new Button { Text = "< Zurück zum Menü" };
        backButton.Pressed += () => BackRequested?.Invoke();
        leftColumn.AddChild(backButton);

        _hud = new Hud();
        leftColumn.AddChild(_hud);

        leftColumn.AddChild(new Label { Text = "Ereignisprotokoll:" });
        _log = new EventLogPanel { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        leftColumn.AddChild(_log);

        root.AddChild(leftColumn);

        // --- Mitte, dehnbar: Spielfeld (identisch zu MatchScreen) ---
        var boardPanel = new PanelContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        _board = new HexBoardView { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        boardPanel.AddChild(_board);
        root.AddChild(boardPanel);

        // Animationsschicht (Vorgabe Teil B) - dieselbe Klasse wie in
        // MatchScreen, damit Live-Spiel UND Replay identisch animieren.
        _animator = new EventAnimationPlayer { Board = _board };
        AddChild(_animator);

        // --- Rechts, feste Breite: Datei-Auswahl + Wiedergabesteuerung (an der Stelle, an der bei MatchScreen die Aktionen stehen) ---
        var rightPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(320, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        var rightMargin = new MarginContainer();
        rightMargin.AddThemeConstantOverride("margin_left", 6);
        rightMargin.AddThemeConstantOverride("margin_top", 6);
        rightMargin.AddThemeConstantOverride("margin_right", 6);
        rightMargin.AddThemeConstantOverride("margin_bottom", 6);
        var rightBox = new VBoxContainer();
        rightBox.AddThemeConstantOverride("separation", 6);

        rightBox.AddChild(new Label { Text = "Replay-Datei:", AutowrapMode = TextServer.AutowrapMode.WordSmart });
        _pathField = new LineEdit
        {
            PlaceholderText = "Pfad zu einer gespeicherten Partie (.json)",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        rightBox.AddChild(_pathField);

        var fileButtonsRow = new HBoxContainer();
        var browseButton = new Button { Text = "Durchsuchen...", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        browseButton.Pressed += () => _fileDialog.Popup();
        fileButtonsRow.AddChild(browseButton);
        var loadButton = new Button { Text = "Laden", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        loadButton.Pressed += OnLoadPressed;
        fileButtonsRow.AddChild(loadButton);
        rightBox.AddChild(fileButtonsRow);

        _statusLabel = new Label { Text = "Keine Partie geladen.", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        rightBox.AddChild(_statusLabel);

        rightBox.AddChild(new HSeparator());
        rightBox.AddChild(new Label { Text = "Wiedergabe:" });

        var stepRow = new HBoxContainer();
        var stepBackButton = new Button { Text = "<< Zurück", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        stepBackButton.Pressed += () => Step(-1);
        stepRow.AddChild(stepBackButton);
        var stepForwardButton = new Button { Text = "Vor >>", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        stepForwardButton.Pressed += () => Step(1);
        stepRow.AddChild(stepForwardButton);
        rightBox.AddChild(stepRow);

        _playPauseButton = new Button { Text = "Abspielen", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _playPauseButton.Pressed += TogglePlay;
        rightBox.AddChild(_playPauseButton);

        rightBox.AddChild(new Label { Text = "Schrittfolge (Schritte/s):" });
        _speedSpinBox = new SpinBox { MinValue = 0.5, MaxValue = 20, Step = 0.5, Value = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _speedSpinBox.ValueChanged += _ => UpdateTimerInterval();
        rightBox.AddChild(_speedSpinBox);

        // Animationstempo (Vorgabe Teil B: "einstellbare Geschwindigkeit,
        // inklusive Option zum vollständigen Überspringen" - "im Replay-
        // Modus... schnell durchspulen können"). Getrennt von der
        // Schrittfolge oben: die Schrittfolge begrenzt, wie oft "Abspielen"
        // überhaupt den nächsten Schritt anstößt, das Animationstempo, wie
        // schnell EIN Schritt dabei visuell abläuft - "Abspielen" wartet
        // ohnehin auf das Ende der laufenden Animation (siehe OnPlayTick),
        // daher bestimmt bei hoher Schrittfolge faktisch das Animationstempo
        // (bzw. "Überspringen") das tatsächliche Durchspul-Tempo.
        rightBox.AddChild(new Label { Text = "Animationstempo:" });
        _animationSpeedSpinBox = new SpinBox { MinValue = 0.25, MaxValue = 8, Step = 0.25, Value = 1, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _animationSpeedSpinBox.ValueChanged += value => _animator.SpeedMultiplier = (float)value;
        rightBox.AddChild(_animationSpeedSpinBox);

        _skipCheckBox = new CheckBox { Text = "Animationen überspringen" };
        _skipCheckBox.Toggled += pressed => _animator.SkipAnimations = pressed;
        rightBox.AddChild(_skipCheckBox);

        rightBox.AddChild(new Label { Text = "Position:" });
        _positionSlider = new HSlider { MinValue = 0, MaxValue = 0, Step = 1, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        // Ein Schieberegler-Sprung ist kein "einzelner Vorwärtsschritt" -
        // immer ohne Animation (Vorgabe: siehe GoToIndex).
        _positionSlider.ValueChanged += value => GoToIndex((int)value, animate: false);
        rightBox.AddChild(_positionSlider);

        rightMargin.AddChild(rightBox);
        rightPanel.AddChild(rightMargin);
        root.AddChild(rightPanel);

        outerMargin.AddChild(root);
        AddChild(outerMargin);

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
            GoToIndex(-1, animate: false); // vor dem ersten Ereignis - zeigt die Ausgangsstellung
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Fehler beim Laden: {ex.Message}";
            _replay = null;
        }
    }

    /// <summary>Ein einzelner Schritt animiert (Vorgabe), egal ob per Schaltfläche oder Abspielen ausgelöst - Rückwärts läuft ohne Animation direkt auf den Zielzustand (siehe GoToIndex).</summary>
    private void Step(int direction) => GoToIndex(_currentIndex + direction, animate: direction > 0);

    private void OnPlayTick()
    {
        if (_replay is null || _currentIndex >= _replay.Frames.Count - 1)
        {
            StopPlaying();
            return;
        }

        // Läuft die Animation des vorherigen Schritts noch, wird dieser
        // Tick übersprungen - der nächste Timer-Tick versucht es erneut.
        // Verhindert, dass sich bei kurzem Timer-Intervall Schritte
        // schneller anhäufen, als sie sichtbar abgespielt werden können
        // (siehe EventAnimationPlayer.Enqueue - würde sonst einfach
        // aufstauen statt zu pacen).
        if (_animator.IsBusy)
        {
            return;
        }

        Step(1);
    }

    /// <summary>
    /// Springt zu einem beliebigen Zeitpunkt der Zeitleiste. Zwei Pfade:
    ///   - EIN EINZELNER SCHRITT VORWÄRTS (<paramref name="animate"/> wahr
    ///     UND direkt auf den aktuellen Index folgend): Protokoll-Zeile und
    ///     Reihenfolge-Anzeige werden nur INKREMENTELL ergänzt, das
    ///     Spielfeld läuft über den Animator (Vorgabe Teil B: "Bewegung
    ///     entlang des gemeldeten Pfades" usw.) - genau wie im Live-Spiel.
    ///   - JEDER ANDERE SPRUNG (rückwärts, Schieberegler, initiales Laden):
    ///     Protokoll UND Reihenfolge-Anzeige werden komplett neu aus den
    ///     Einträgen bis zum Zielindex aufgebaut (frischer EventDescriber/
    ///     TurnOrderTracker), das Spielfeld springt SOFORT ohne Animation
    ///     (Vorgabe: "Beim schrittweisen Zurückgehen darf ohne Animation
    ///     direkt auf den Zustand gesprungen werden" - dieselbe Behandlung
    ///     gilt konsequent auch für Schieberegler-Sprünge, aus demselben
    ///     Grund: mehrere übersprungene Ereignisse lassen sich nicht
    ///     sinnvoll nacheinander animieren, ohne "schnelles Durchspulen"
    ///     zu behindern).
    /// </summary>
    private void GoToIndex(int index, bool animate)
    {
        if (_replay is null)
        {
            return;
        }

        index = Mathf.Clamp(index, -1, _replay.Frames.Count - 1);
        bool isSingleAnimatedForwardStep = animate && index == _currentIndex + 1 && index >= 0;

        if (isSingleAnimatedForwardStep)
        {
            var frame = _replay.Frames[index];
            var before = _currentIndex == -1 ? _replay.InitialModel : _replay.Frames[_currentIndex].Model;
            if (_describer.Describe(frame.Event, frame.Model) is { } line)
            {
                _log.AppendLine(line);
            }

            _turnOrderTracker.Register(frame.Event, before, frame.Model);
            _hud.UpdateFrom(frame.Model);
            _hud.UpdateTurnOrder(_turnOrderTracker.BuildDisplayOrder(frame.Model));
            _positionSlider.SetValueNoSignal(index);

            _currentIndex = index;
            _animator.Enqueue(frame.Event, before, frame.Model);
        }
        else
        {
            _currentIndex = index;

            _log.Clear();
            _describer = new EventDescriber();
            _turnOrderTracker = new TurnOrderTracker();
            for (int i = 0; i <= index; i++)
            {
                var frameBefore = i == 0 ? _replay.InitialModel : _replay.Frames[i - 1].Model;
                if (_describer.Describe(_replay.Frames[i].Event, _replay.Frames[i].Model) is { } rebuildLine)
                {
                    _log.AppendLine(rebuildLine);
                }

                _turnOrderTracker.Register(_replay.Frames[i].Event, frameBefore, _replay.Frames[i].Model);
            }

            var currentModel = index == -1 ? _replay.InitialModel : _replay.Frames[index].Model;
            _animator.ShowInstantly(currentModel); // leert zusätzlich eine evtl. noch laufende Animation eines vorherigen Vorwärtsschritts
            _hud.UpdateFrom(currentModel);
            _hud.UpdateTurnOrder(_turnOrderTracker.BuildDisplayOrder(currentModel));
            _positionSlider.SetValueNoSignal(index);
        }

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
                GoToIndex(-1, animate: false); // am Ende erneut "Abspielen" beginnt von vorn
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
