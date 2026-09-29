using Godot;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCoreGodot.Setup;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Aufstellung EINES Teams: je Startfeld ein Held (Vorgabe Punkt 6: "je drei
/// Helden aus dem Katalog wählen und den Startfeldern zuordnen"), plus die
/// Wahl, wer dieses Team steuert (Mensch oder einer der Agenten des
/// Batch-Laufs, siehe <see cref="TeamControllers"/>).
/// </summary>
public partial class TeamSetupPanel : VBoxContainer
{
    private readonly List<OptionButton> _heroDropdowns = new();
    private OptionButton _controllerDropdown = null!;
    private Label _errorLabel = null!;
    private IReadOnlyList<HeroDefinition> _catalogHeroes = Array.Empty<HeroDefinition>();
    private IReadOnlyList<HexCoordinate> _fields = Array.Empty<HexCoordinate>();

    /// <summary>
    /// Zufallsquelle NUR für die Zufallsknöpfe der Aufstellung. Sie füllt lediglich
    /// die Auswahllisten vor - die Auswahl ist danach sichtbar und wird wie eine
    /// Handauswahl übernommen. Auf den Partieverlauf hat sie keinen Einfluss; der
    /// läuft weiterhin ausschließlich über den Seed (siehe SetupScreen) und bleibt
    /// damit reproduzierbar.
    /// </summary>
    private static readonly Random SetupRandom = new();

    public string TeamLabel { get; set; } = "Team";

    public override void _Ready()
    {
        var title = new Label { Text = TeamLabel };
        title.AddThemeFontSizeOverride("font_size", 20);
        AddChild(title);
    }

    /// <summary>Muss unmittelbar nach dem Hinzufügen zum Baum aufgerufen werden (baut die Dropdown-Zeilen je Startfeld auf).</summary>
    public void Configure(IReadOnlyList<HeroDefinition> catalogHeroes, IReadOnlyList<HexCoordinate> fields)
    {
        _catalogHeroes = catalogHeroes;
        _fields = fields;

        for (int i = 0; i < fields.Count; i++)
        {
            var row = new HBoxContainer();
            row.AddChild(new Label { Text = $"Feld {fields[i]}:", CustomMinimumSize = new Vector2(90, 0) });

            var dropdown = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            foreach (var hero in catalogHeroes)
            {
                dropdown.AddItem($"{hero.DisplayName} ({hero.Role})");
            }
            // Unterschiedliche Vorauswahl je Zeile, damit nicht von Anfang
            // an versehentlich derselbe Held dreimal gewählt ist.
            dropdown.Selected = i % Math.Max(1, catalogHeroes.Count);

            row.AddChild(dropdown);
            AddChild(row);
            _heroDropdowns.Add(dropdown);
        }

        var randomHeroesButton = new Button { Text = "Zufällige Helden" };
        randomHeroesButton.Pressed += RandomizeHeroes;
        AddChild(randomHeroesButton);

        var controllerRow = new HBoxContainer();
        controllerRow.AddChild(new Label { Text = "Gesteuert von:", CustomMinimumSize = new Vector2(90, 0) });
        _controllerDropdown = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        foreach (var controller in TeamControllers.All)
        {
            _controllerDropdown.AddItem(TeamControllers.Describe(controller));
        }
        _controllerDropdown.Selected = 0; // Mensch - bisheriges Standardverhalten
        controllerRow.AddChild(_controllerDropdown);
        var randomAgentButton = new Button { Text = "Zufälliger Agent" };
        randomAgentButton.Pressed += RandomizeAgent;
        controllerRow.AddChild(randomAgentButton);
        AddChild(controllerRow);

        _errorLabel = new Label { Modulate = new Color(1f, 0.4f, 0.4f) };
        AddChild(_errorLabel);
    }

    /// <summary>Wählt je Startfeld einen Helden zufällig - alle verschieden (wie bei der Handauswahl gefordert).</summary>
    private void RandomizeHeroes()
    {
        var indices = Enumerable.Range(0, _catalogHeroes.Count).OrderBy(_ => SetupRandom.Next()).ToList();
        for (int i = 0; i < _heroDropdowns.Count && i < indices.Count; i++)
        {
            _heroDropdowns[i].Selected = indices[i];
        }

        _errorLabel.Text = "";
    }

    /// <summary>Wählt einen der Agenten zufällig - nie die menschliche Steuerung.</summary>
    private void RandomizeAgent()
    {
        var agentIndices = Enumerable.Range(0, TeamControllers.All.Count)
            .Where(i => TeamControllers.All[i] != TeamControllers.Human)
            .ToList();
        _controllerDropdown.Selected = agentIndices[SetupRandom.Next(agentIndices.Count)];
    }

    /// <summary>
    /// Prüft, dass innerhalb des Teams kein Held doppelt gewählt wurde
    /// (Anforderung 1 aus Teil A der Simulationsaufgabe: "genau drei
    /// VERSCHIEDENE Helden"), und liefert bei Erfolg die fertige Auswahl.
    /// </summary>
    public bool TryBuildChoice(out TeamSetupChoice choice)
    {
        var selectedHeroes = _heroDropdowns.Select(d => _catalogHeroes[d.Selected]).ToList();
        if (selectedHeroes.Select(h => h.Id).Distinct().Count() != selectedHeroes.Count)
        {
            _errorLabel.Text = "Bitte drei VERSCHIEDENE Helden wählen.";
            choice = null!;
            return false;
        }

        _errorLabel.Text = "";
        var assignments = _fields.Zip(selectedHeroes, (field, hero) => (hero, field)).ToList();
        choice = new TeamSetupChoice(assignments, TeamControllers.All[_controllerDropdown.Selected]);
        return true;
    }
}
