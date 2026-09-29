using Godot;
using TacticsCore.Board;
using TacticsCore.Heroes;
using TacticsCore.Units;
using TacticsCoreGodot.Setup;
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Aufstellungsbildschirm vor Partiebeginn (Vorgabe Punkt 6): beide Teams
/// zusammenstellen (Helden je Startfeld, Steuerung durch Mensch oder einen
/// der Agenten des Batch-Laufs), Seed und beginnendes Team festlegen, Partie
/// starten.
/// </summary>
public partial class SetupScreen : Control
{
    private TeamSetupPanel _teamAPanel = null!;
    private TeamSetupPanel _teamBPanel = null!;
    private SpinBox _seedSpinBox = null!;
    private OptionButton _startingTeamDropdown = null!;

    /// <summary>Der Benutzer hat eine vollständige, gültige Aufstellung bestätigt.</summary>
    public event Action<MatchSetupRequest>? StartRequested;

    public override void _Ready()
    {
        // Siehe MatchScreen für die Begründung: SetAnchorsAndOffsetsPreset()
        // statt SetAnchorsPreset(), sonst bleiben Offsets bei einem frisch
        // erzeugten (0x0 großen) Control auf riesigen negativen Werten
        // stehen statt auf 0.
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); // diese Steuerung selbst füllt ihren Elternknoten (MainController) aus

        var root = new VBoxContainer { AnchorRight = 1, AnchorBottom = 1 };

        var title = new Label { Text = "Neue Partie zusammenstellen" };
        title.AddThemeFontSizeOverride("font_size", 24);
        root.AddChild(title);

        var teamsRow = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _teamAPanel = new TeamSetupPanel { TeamLabel = "Team A", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _teamBPanel = new TeamSetupPanel { TeamLabel = "Team B", SizeFlagsHorizontal = SizeFlags.ExpandFill };
        teamsRow.AddChild(_teamAPanel);
        teamsRow.AddChild(_teamBPanel);
        root.AddChild(teamsRow);

        var catalog = DefaultHeroes.CreateDefaultCatalog();
        var board = new GameBoard();
        var teamAFields = board.GetStartFields(FieldType.BlossomA)
            .OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R)
            .Select(f => f.Coordinate).ToList();
        var teamBFields = board.GetStartFields(FieldType.BlossomB)
            .OrderBy(f => f.Coordinate.Q).ThenBy(f => f.Coordinate.R)
            .Select(f => f.Coordinate).ToList();
        _teamAPanel.Configure(catalog.All, teamAFields);
        _teamBPanel.Configure(catalog.All, teamBFields);

        // Bewusst KEIN "zufälliger Seed"-Knopf: die Oberfläche soll keine
        // eigene Zufallsquelle einführen. Der Seed ist immer vorgegeben
        // (Standard 1) und wird in der Partie angezeigt.
        var seedRow = new HBoxContainer();
        seedRow.AddChild(new Label { Text = "Seed (für reproduzierbare Partien):" });
        _seedSpinBox = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Value = 1, CustomMinimumSize = new Vector2(160, 0) };
        seedRow.AddChild(_seedSpinBox);
        root.AddChild(seedRow);

        // Im Batch-Lauf ist das beginnende Team Teil der Ausgangslage und wird
        // NICHT gezogen - zum Nachspielen einer Batch-Partie hier fest vorgeben
        // (siehe Konsolenmodus "batch-match", der den Wert ausgibt).
        var startRow = new HBoxContainer();
        startRow.AddChild(new Label { Text = "Beginnendes Team:" });
        _startingTeamDropdown = new OptionButton();
        _startingTeamDropdown.AddItem("Zufällig (aus Seed)");
        _startingTeamDropdown.AddItem("Team A");
        _startingTeamDropdown.AddItem("Team B");
        _startingTeamDropdown.Selected = 0; // bisheriges Verhalten
        startRow.AddChild(_startingTeamDropdown);
        root.AddChild(startRow);

        var startButton = new Button { Text = "Partie starten" };
        startButton.Pressed += OnStartPressed;
        root.AddChild(startButton);

        AddChild(root);
    }

    private void OnStartPressed()
    {
        bool teamAOk = _teamAPanel.TryBuildChoice(out var teamA);
        bool teamBOk = _teamBPanel.TryBuildChoice(out var teamB);
        if (!teamAOk || !teamBOk)
        {
            return; // die einzelnen Team-Panels zeigen ihre Fehlermeldung bereits selbst an
        }

        Team? startingTeam = _startingTeamDropdown.Selected switch { 1 => Team.TeamA, 2 => Team.TeamB, _ => null };
        StartRequested?.Invoke(new MatchSetupRequest(teamA, teamB, (int)_seedSpinBox.Value, startingTeam));
    }
}
