using Godot;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Wurzelsteuerung: einfaches Hauptmenü, das zwischen Aufstellung
/// (SetupScreen -&gt; MatchScreen für eine neue, live gespielte Partie) und
/// dem Replay-Bildschirm umschaltet. Enthält selbst keine Spiellogik, nur
/// Navigation zwischen den drei Bildschirmen.
/// </summary>
public partial class MainController : Control
{
    private Control _menuScreen = null!;
    private SetupScreen _setupScreen = null!;
    private MatchScreen _matchScreen = null!;
    private ReplayScreen _replayScreen = null!;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);

        _menuScreen = BuildMenu();
        _setupScreen = new SetupScreen { Visible = false };
        _matchScreen = new MatchScreen { Visible = false };
        _replayScreen = new ReplayScreen { Visible = false };

        AddChild(_menuScreen);
        AddChild(_setupScreen);
        AddChild(_matchScreen);
        AddChild(_replayScreen);

        _setupScreen.StartRequested += request =>
        {
            ShowOnly(_matchScreen);
            _matchScreen.StartMatch(request);
        };
        _matchScreen.BackRequested += () => ShowOnly(_menuScreen);
        _replayScreen.BackRequested += () => ShowOnly(_menuScreen);

        ShowOnly(_menuScreen);
    }

    private Control BuildMenu()
    {
        var container = new CenterContainer();
        container.SetAnchorsPreset(LayoutPreset.FullRect);

        var vbox = new VBoxContainer();

        var title = new Label { Text = "TacticsCore - Godot-Oberfläche", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);

        var subtitle = new Label
        {
            Text = "Werkzeug zur visuellen Verifikation der Spielregeln",
            HorizontalAlignment = HorizontalAlignment.Center,
            Modulate = new Color(1, 1, 1, 0.7f),
        };
        vbox.AddChild(subtitle);
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 24) }); // Abstandhalter

        var newMatchButton = new Button { Text = "Neue Partie", CustomMinimumSize = new Vector2(260, 44) };
        newMatchButton.Pressed += () => ShowOnly(_setupScreen);
        vbox.AddChild(newMatchButton);

        var replayButton = new Button { Text = "Replay laden", CustomMinimumSize = new Vector2(260, 44) };
        replayButton.Pressed += () => ShowOnly(_replayScreen);
        vbox.AddChild(replayButton);

        container.AddChild(vbox);
        return container;
    }

    private void ShowOnly(Control target)
    {
        _menuScreen.Visible = target == _menuScreen;
        _setupScreen.Visible = target == _setupScreen;
        _matchScreen.Visible = target == _matchScreen;
        _replayScreen.Visible = target == _replayScreen;
    }
}
