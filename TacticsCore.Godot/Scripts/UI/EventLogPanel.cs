using Godot;

namespace TacticsCoreGodot.UI;

/// <summary>Scrollbares Ereignisprotokoll (Vorgabe Punkt 7: "Ereignisprotokoll als scrollbare Textausgabe").</summary>
public partial class EventLogPanel : PanelContainer
{
    private RichTextLabel _label = null!;

    public override void _Ready()
    {
        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };

        _label = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            FitContent = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(280, 0),
        };

        scroll.AddChild(_label);
        AddChild(scroll);
    }

    public void AppendLine(string bbcodeText) => _label.AppendText(bbcodeText + "\n");

    public void Clear() => _label.Clear();
}
