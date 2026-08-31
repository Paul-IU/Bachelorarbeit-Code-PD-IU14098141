using Godot;
using TacticsCore.Match;
using TacticsCore.Units;
using TacticsCoreGodot.Shared;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Zeigt Runde, aktuell handelnde Einheit, ihr verbleibendes Aktionsbudget
/// sowie die Zugreihenfolge der Runde als Icon-Leiste (Vorgabe Punkt 4:
/// "wer gerade am Zug ist, welche Runde läuft, welche Aktionen ... noch
/// bleiben", sowie "Reihenfolge der Helden ... als Icons mit Ausgrauung von
/// Helden, die an der Reihe waren").
/// </summary>
public partial class Hud : PanelContainer
{
    private Label _roundLabel = null!;
    private Label _actorLabel = null!;
    private Label _budgetLabel = null!;
    private HFlowContainer _turnOrderRow = null!;

    public override void _Ready()
    {
        SizeFlagsHorizontal = SizeFlags.ExpandFill;

        var vbox = new VBoxContainer();
        _roundLabel = new Label { Text = "Runde: -", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _actorLabel = new Label { Text = "Am Zug: -", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        _budgetLabel = new Label { Text = "", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        vbox.AddChild(_roundLabel);
        vbox.AddChild(_actorLabel);
        vbox.AddChild(_budgetLabel);

        vbox.AddChild(new HSeparator());
        vbox.AddChild(new Label { Text = "Reihenfolge dieser Runde:", AutowrapMode = TextServer.AutowrapMode.WordSmart });
        _turnOrderRow = new HFlowContainer();
        vbox.AddChild(_turnOrderRow);

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

    /// <summary>
    /// Zeichnet die Reihenfolge-Icon-Leiste neu: ein kleines, teamfarbenes
    /// Kästchen je Eintrag der zu Rundenbeginn festgelegten (und seither
    /// unveränderten) Reihenfolge - siehe <see cref="TurnOrderTracker"/>.
    /// Der Anzeige-Zustand je Eintrag (<see cref="TurnOrderStatus"/>) macht
    /// Zustandsänderungen INNERHALB der Runde sichtbar, ohne die Reihenfolge
    /// selbst umzusortieren: bereits gezogene Einheiten abgeblendet, die
    /// aktuell handelnde golden umrandet hervorgehoben, während der Runde
    /// besiegte an ihrer Position durchgestrichen stehen gelassen statt
    /// entfernt. Die Teamfarbe (Hintergrund) bleibt in allen vier Zuständen
    /// erkennbar (Vorgabe: "Teamzugehörigkeit soll weiterhin über die Farbe
    /// erkennbar bleiben").
    /// </summary>
    public void UpdateTurnOrder(IReadOnlyList<TurnOrderSlot> order)
    {
        foreach (var child in _turnOrderRow.GetChildren())
        {
            child.QueueFree();
        }

        var shortLabels = BuildShortLabels(order);

        foreach (var slot in order)
        {
            var unit = slot.Unit;
            bool isDefeated = slot.Status == TurnOrderStatus.Defeated;
            bool isActing = slot.Status == TurnOrderStatus.Acting;
            bool hasActed = slot.Status == TurnOrderStatus.Acted;

            string fullName = (unit.IsClone ? "~" : "") + unit.DisplayName;
            var icon = new PanelContainer
            {
                CustomMinimumSize = new Vector2(34, 34),
                TooltipText = isDefeated ? $"{fullName} (besiegt)" : fullName, // Vorgabe 4: eindeutige Kennzeichnung per Tooltip mit vollem Namen
                Modulate = isDefeated ? new Color(1f, 1f, 1f, 0.3f) : hasActed ? new Color(1f, 1f, 1f, 0.35f) : Colors.White,
            };
            var style = new StyleBoxFlat
            {
                BgColor = unit.Team == Team.TeamA ? TeamAColor : TeamBColor,
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4,
                CornerRadiusBottomRight = 4,
                BorderWidthTop = isActing ? 3 : 0,
                BorderWidthBottom = isActing ? 3 : 0,
                BorderWidthLeft = isActing ? 3 : 0,
                BorderWidthRight = isActing ? 3 : 0,
                BorderColor = ActingHighlightColor,
            };
            icon.AddThemeStyleboxOverride("panel", style);

            // RichTextLabel statt Label, damit eine besiegte Einheit über
            // BBCode [s]...[/s] echt durchgestrichen dargestellt werden kann
            // (Vorgabe 3: "durchgestrichen oder stark abgeblendet" - hier
            // beides kombiniert).
            var label = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
                MouseFilter = MouseFilterEnum.Ignore,
            };
            label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            string shortLabel = shortLabels[unit.Id];
            label.Text = isDefeated ? $"[center][s]{shortLabel}[/s][/center]" : $"[center]{shortLabel}[/center]";
            icon.AddChild(label);

            _turnOrderRow.AddChild(icon);
        }
    }

    private static readonly Color TeamAColor = new(0.2f, 0.5f, 1f);
    private static readonly Color TeamBColor = new(1f, 0.42f, 0.32f);
    private static readonly Color ActingHighlightColor = new(1f, 0.85f, 0.2f); // Gold - hebt die aktuell handelnde Einheit hervor

    /// <summary>
    /// Kürzt Heldennamen auf das kürzestmögliche eindeutige Präfix (mind. 2
    /// Zeichen) innerhalb der AKTUELL angezeigten Menge - löst Mehrdeutig-
    /// keiten wie "Elfe"/"Elementarmagier" (beide "El" bei fester
    /// 2-Zeichen-Kürzung) generisch, ohne feste Sonderfälle für einzelne
    /// Heldennamen im Code. Ein Klon erhält zusätzlich das Präfix "~" vor
    /// der (für seinen Erzeuger-Namen berechneten) Abkürzung - dieselbe
    /// Konvention wie beim vollen Namen im Tooltip.
    /// </summary>
    private static Dictionary<Guid, string> BuildShortLabels(IReadOnlyList<TurnOrderSlot> order)
    {
        var distinctNames = order.Select(s => s.Unit.DisplayName).Distinct().ToList();
        var abbreviationByName = distinctNames.ToDictionary(name => name, name => ShortestUniquePrefix(name, distinctNames));

        var result = new Dictionary<Guid, string>();
        foreach (var slot in order)
        {
            string abbreviation = abbreviationByName[slot.Unit.DisplayName];
            result[slot.Unit.Id] = slot.Unit.IsClone ? "~" + abbreviation : abbreviation;
        }

        return result;
    }

    private static string ShortestUniquePrefix(string name, IReadOnlyList<string> allNames)
    {
        for (int len = 2; len < name.Length; len++)
        {
            string candidate = name[..len];
            if (!allNames.Any(other => other != name && other.StartsWith(candidate, StringComparison.Ordinal)))
            {
                return candidate;
            }
        }

        return name; // Präfix bis zur vollen Länge ausgeschöpft und immer noch nicht eindeutig (z.B. zwei identische Namen) - dann bleibt nur der ganze Name
    }

    public void ShowMatchEnded(MatchWinner winner, MatchEndReason reason, int roundsPlayed)
    {
        _actorLabel.Text = $"Partie beendet - Sieger: {winner} ({reason}, {roundsPlayed} Runden)";
        _budgetLabel.Text = "";
    }
}
