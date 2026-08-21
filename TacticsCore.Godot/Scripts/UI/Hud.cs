using Godot;
using TacticsCore.Match;
using TacticsCoreGodot.Shared;

namespace TacticsCoreGodot.UI;

/// <summary>Zeigt Runde, aktuell handelnde Einheit und ihr verbleibendes Aktionsbudget (Vorgabe Punkt 4: "wer gerade am Zug ist, welche Runde läuft, welche Aktionen ... noch bleiben").</summary>
public partial class Hud : PanelContainer
{
    private Label _roundLabel = null!;
    private Label _actorLabel = null!;
    private Label _budgetLabel = null!;

    public override void _Ready()
    {
        var vbox = new VBoxContainer();
        _roundLabel = new Label { Text = "Runde: -" };
        _actorLabel = new Label { Text = "Am Zug: -" };
        _budgetLabel = new Label { Text = "" };
        vbox.AddChild(_roundLabel);
        vbox.AddChild(_actorLabel);
        vbox.AddChild(_budgetLabel);
        AddChild(vbox);
    }

    public void UpdateFrom(BoardRenderModel model)
    {
        _roundLabel.Text = $"Runde: {model.RoundNumber}";

        var acting = model.Units.FirstOrDefault(u => u.Id == model.ActingUnitId);
        _actorLabel.Text = acting is not null ? $"Am Zug: {acting.DisplayName} ({acting.Team})" : "Am Zug: -";

        var remaining = new List<string>();
        if (model.ActingCanMove)
        {
            remaining.Add("Bewegung");
        }

        if (model.ActingCanAct)
        {
            remaining.Add("Aktion");
        }

        _budgetLabel.Text = remaining.Count > 0 ? "Noch verfügbar: " + string.Join(" + ", remaining) : "Kein Budget mehr übrig";
    }

    public void ShowMatchEnded(MatchWinner winner, MatchEndReason reason, int roundsPlayed)
    {
        _actorLabel.Text = $"Partie beendet - Sieger: {winner} ({reason}, {roundsPlayed} Runden)";
        _budgetLabel.Text = "";
    }
}
