using Godot;
using TacticsCore.Match;
using TacticsCore.Units;
using TacticsCoreGodot.Shared;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Schlichte Abschlussansicht am Ende einer live gespielten Partie (dient dem
/// Beobachten, nicht der Auswertung): Sieger und Grund (Auslöschung oder
/// Rundenlimit samt entscheidender Auflösungsstufe), Rundenzahl und je Held
/// verursachter/erlittener Schaden, Besiegte und ob er überlebt hat. Alle
/// Werte stammen aus TacticsCore (<see cref="MatchResult.CombatStatisticsByUnitId"/>,
/// <see cref="MatchResult.RoundLimitResolution"/>) - hier wird nichts selbst
/// berechnet, nur angezeigt.
/// </summary>
public partial class MatchSummaryPanel : PanelContainer
{
    private Label _headline = null!;
    private Label _details = null!;
    private GridContainer _table = null!;

    /// <summary>Der Benutzer möchte zum Hauptmenü zurück.</summary>
    public event Action? BackRequested;

    public override void _Ready()
    {
        // Mittig über dem Spielfeld, Größe nach Inhalt.
        SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;

        var margin = new MarginContainer();
        foreach (var side in new[] { "margin_left", "margin_top", "margin_right", "margin_bottom" })
        {
            margin.AddThemeConstantOverride(side, 16);
        }

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);

        _headline = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _headline.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(_headline);

        _details = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        vbox.AddChild(_details);

        _table = new GridContainer { Columns = 6 };
        _table.AddThemeConstantOverride("h_separation", 18);
        vbox.AddChild(_table);

        var backButton = new Button { Text = "Zurück zum Hauptmenü", SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        backButton.Pressed += () => BackRequested?.Invoke();
        vbox.AddChild(backButton);

        margin.AddChild(vbox);
        AddChild(margin);
    }

    /// <summary>
    /// Füllt die Ansicht. <paramref name="initialModel"/> liefert Namen und Team
    /// aller angetretenen Helden, <paramref name="finalModel"/> die am Ende noch
    /// lebenden Einheiten.
    /// </summary>
    public void Present(MatchResult result, BoardRenderModel initialModel, BoardRenderModel finalModel)
    {
        _headline.Text = result.Winner switch
        {
            MatchWinner.TeamA => "Team A hat gewonnen",
            MatchWinner.TeamB => "Team B hat gewonnen",
            _ => "Unentschieden",
        };

        string reason = result.EndReason == MatchEndReason.Elimination
            ? "Auslöschung"
            : "Rundenlimit - " + result.RoundLimitResolution switch
            {
                RoundLimitResolution.SurvivingHeroes => $"entschieden durch lebende Helden ({result.TeamAResult.SurvivingUnitCount} : {result.TeamBResult.SurvivingUnitCount})",
                RoundLimitResolution.HitPointFraction => $"entschieden durch HP-Anteil ({result.TeamAResult.RemainingHitPointsFraction:P1} : {result.TeamBResult.RemainingHitPointsFraction:P1})",
                _ => "Gleichstand bei lebenden Helden und HP-Anteil",
            };
        _details.Text = $"Grund: {reason}\nRunden: {result.RoundsPlayed}";

        foreach (var child in _table.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var header in new[] { "Team", "Held", "Verursacht", "Erlitten", "Besiegte", "Überlebt" })
        {
            var label = new Label { Text = header };
            label.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.7f));
            _table.AddChild(label);
        }

        var alive = finalModel.Units.Select(u => u.Id).ToHashSet();
        var statistics = result.CombatStatisticsByUnitId ?? new Dictionary<Guid, UnitCombatStatistics>();
        foreach (var hero in initialModel.Units.Where(u => !u.IsClone).OrderBy(u => u.Team).ThenBy(u => u.Id))
        {
            var s = statistics.GetValueOrDefault(hero.Id, UnitCombatStatistics.Empty);
            AddCell(hero.Team == Team.TeamA ? "A" : "B");
            AddCell(hero.DisplayName);
            AddCell(s.DamageDealt.ToString(), HorizontalAlignment.Right);
            AddCell(s.DamageTaken.ToString(), HorizontalAlignment.Right);
            AddCell(s.Defeats.ToString(), HorizontalAlignment.Right);
            AddCell(alive.Contains(hero.Id) ? "ja" : "nein");
        }

        Visible = true;
    }

    private void AddCell(string text, HorizontalAlignment alignment = HorizontalAlignment.Left) =>
        _table.AddChild(new Label { Text = text, HorizontalAlignment = alignment });
}
