using Godot;
using TacticsCore;
using TacticsCore.Abilities;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Bietet dem Menschen seine aktuell legalen Züge an (Vorgabe Punkt 5):
/// gruppiert über Schaltflächen (Bewegen / Angreifen / je Fähigkeit-Modus /
/// Passen) mit anschließender Zielauswahl per Klick auf das Spielfeld,
/// UND zusätzlich als schlichte, vollständige Liste zum direkten Anklicken
/// ("Fallback ... damit jede Aktion erreichbar ist").
///
/// Wichtig: diese Klasse ENTSCHEIDET nichts über Legalität - sie bekommt
/// die bereits fertige Liste legaler Züge von außen (letztlich von
/// <see cref="HumanAgent"/>/<see cref="AbilitySystem"/>) und übersetzt sie
/// nur in Schaltflächen und Text.
/// </summary>
public partial class ActionPanel : PanelContainer
{
    private VBoxContainer _buttonsBox = null!;
    private VBoxContainer _fallbackListBox = null!;
    private Label _selectedUnitLabel = null!;
    private PopupMenu _targetChoicePopup = null!;

    /// <summary>Die TurnActions der aktuell per Schaltfläche gewählten Kategorie (z.B. alle Bewegungsziele) - oder null, solange keine Kategorie gewählt ist.</summary>
    private List<TurnAction>? _pendingGroup;

    /// <summary>
    /// Die Kandidaten des zuletzt geöffneten Auswahl-Popups (siehe
    /// <see cref="TryResolveBoardClick"/>) - Index entspricht der über
    /// <see cref="PopupMenu.IdPressed"/> gemeldeten Id.
    /// </summary>
    private List<TurnAction> _ambiguousChoices = new();

    private bool _interactionEnabled = true;

    /// <summary>
    /// Ob das Panel aktuell Eingaben annimmt - während einer Animation
    /// (siehe EventAnimationPlayer) gesperrt (Vorgabe Teil B: "während eine
    /// Animation läuft, sind keine Eingaben möglich"). Sperrt sowohl neu
    /// erzeugte als auch bereits vorhandene Schaltflächen (siehe AddButton)
    /// UND Klicks auf das Spielfeld (siehe TryResolveBoardClick).
    /// </summary>
    public bool InteractionEnabled
    {
        get => _interactionEnabled;
        set
        {
            _interactionEnabled = value;
            SetButtonsDisabled(this, !value);
            Modulate = value ? Colors.White : new Color(1f, 1f, 1f, 0.55f); // sichtbares Feedback, dass gerade nicht interagiert werden kann
        }
    }

    private static void SetButtonsDisabled(Node node, bool disabled)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Button button)
            {
                button.Disabled = disabled;
            }

            SetButtonsDisabled(child, disabled);
        }
    }

    /// <summary>Der Benutzer hat sich endgültig für genau diesen Zug entschieden.</summary>
    public event Action<TurnAction>? ActionChosen;

    /// <summary>Welche Felder gerade als "erreichbar" bzw. "gültiges Ziel" hervorgehoben werden sollen (siehe HexBoardView.SetHighlights).</summary>
    public event Action<IEnumerable<HexCoordinate>, IEnumerable<HexCoordinate>>? HighlightsChanged;

    public override void _Ready()
    {
        var root = new VBoxContainer();

        _selectedUnitLabel = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
        root.AddChild(_selectedUnitLabel);

        root.AddChild(new Label { Text = "Aktionen:" });
        _buttonsBox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        root.AddChild(_buttonsBox);

        root.AddChild(new HSeparator());
        root.AddChild(new Label { Text = "Alle legalen Züge (Fallback):" });
        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 150), // nur ein Mindestboden - wächst über ExpandFill mit dem verfügbaren Platz
        };
        _fallbackListBox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(_fallbackListBox);
        root.AddChild(scroll);

        AddChild(root);

        // Auswahl-Popup für mehrdeutige Feldklicks (Vorgabe Punkt C.6:
        // "alle gültigen Ziele auf dem angeklickten Feld einzeln zur Auswahl
        // anbieten") - siehe TryResolveBoardClick.
        _targetChoicePopup = new PopupMenu();
        _targetChoicePopup.IdPressed += OnTargetChoicePicked;
        AddChild(_targetChoicePopup);
    }

    /// <summary>Vom Aufrufer (MatchScreen) aufzurufen, sobald der Mensch am Zug ist und seine legalen Züge feststehen.</summary>
    public void Present(Unit unit, IReadOnlyList<TurnAction> legalActions)
    {
        _selectedUnitLabel.Text = $"{DescribeUnit(unit)} wählt eine Aktion:";
        _pendingGroup = null;

        BuildGroupedButtons(unit, legalActions);
        BuildFallbackList(legalActions);
        HighlightsChanged?.Invoke(Array.Empty<HexCoordinate>(), Array.Empty<HexCoordinate>());
    }

    /// <summary>Blendet die Auswahl aus (z.B. während ein anderer/kein Mensch am Zug ist).</summary>
    public void Clear()
    {
        _selectedUnitLabel.Text = "";
        _pendingGroup = null;
        ClearChildren(_buttonsBox);
        ClearChildren(_fallbackListBox);
        HighlightsChanged?.Invoke(Array.Empty<HexCoordinate>(), Array.Empty<HexCoordinate>());
    }

    /// <summary>
    /// Von MatchScreen aufgerufen, wenn auf dem Spielfeld geklickt wurde -
    /// ordnet den Klick der aktuell per Schaltfläche gewählten Kategorie zu.
    /// Die Kernbibliothek liefert legale Ziele bereits EINHEITENWEISE, nicht
    /// feldweise (siehe CombatSystem.GetValidAttackTargets/AbilitySystem.GetLegalTargets,
    /// verifiziert in CombatSystemTests) - stehen daher (durch erlaubtes
    /// Stapeln) MEHRERE Ziele auf demselben Feld, öffnet diese Methode ein
    /// kleines Auswahl-Popup mit JEDEM davon (Vorgabe Punkt C.6), statt
    /// automatisch das erste zu wählen. Bei genau einem Treffer wird direkt
    /// gewählt, bei keinem Treffer passiert nichts.
    /// </summary>
    public void TryResolveBoardClick(HexCoordinate clicked)
    {
        if (_pendingGroup is null || !_interactionEnabled)
        {
            return;
        }

        // Reihenfolge bleibt exakt die der Kernbibliothek (stabil sortiert
        // nach q, r, UnitId - siehe CombatSystem/AbilitySystem) - hier nur
        // gefiltert, nicht neu sortiert (Vorgabe Punkt C.8).
        var matches = _pendingGroup.Where(action => TargetFieldOf(action) == clicked).ToList();

        switch (matches.Count)
        {
            case 0:
                return;
            case 1:
                ActionChosen?.Invoke(matches[0]);
                return;
            default:
                ShowTargetChoicePopup(matches);
                return;
        }
    }

    /// <summary>Zeigt für mehrere Ziele auf demselben Feld eine einfache Auswahlliste (Vorgabe Punkt C.6).</summary>
    private void ShowTargetChoicePopup(List<TurnAction> matches)
    {
        _ambiguousChoices = matches;
        _targetChoicePopup.Clear();
        for (int i = 0; i < matches.Count; i++)
        {
            _targetChoicePopup.AddItem(DescribeAction(matches[i]), i);
        }

        // Zentriert statt an der (hier nicht zuverlässig bekannten)
        // Klickposition geöffnet - robust unabhängig von Bildschirm-/
        // Skalierungsdetails, die sich visuell nicht überprüfen lassen.
        _targetChoicePopup.PopupCentered(new Vector2I(320, Math.Min(matches.Count, 8) * 28 + 16));
    }

    private void OnTargetChoicePicked(long id)
    {
        if (id >= 0 && id < _ambiguousChoices.Count)
        {
            ActionChosen?.Invoke(_ambiguousChoices[(int)id]);
        }

        _ambiguousChoices.Clear();
    }

    private void BuildGroupedButtons(Unit unit, IReadOnlyList<TurnAction> legalActions)
    {
        ClearChildren(_buttonsBox);

        var moves = legalActions.OfType<TurnAction.Move>().Cast<TurnAction>().ToList();
        if (moves.Count > 0)
        {
            AddButton(_buttonsBox, $"Bewegen ({moves.Count} Felder)", () => SelectGroup(moves));
        }

        var attacks = legalActions.OfType<TurnAction.Attack>().Cast<TurnAction>().ToList();
        if (attacks.Count > 0)
        {
            AddButton(_buttonsBox, $"Angreifen ({attacks.Count} Ziele)", () => SelectGroup(attacks));
        }

        // Fähigkeiten je Modus gruppiert - bei mehreren Modi (Musiker,
        // Elementarmagier, Bogenschütze) erscheint jeder Modus als eigene
        // Schaltfläche, wie gefordert ("bei mehreren Modi Modusauswahl
        // anbieten").
        string abilityName = unit is Hero hero ? hero.Definition.Ability.DisplayName : "Fähigkeit";
        var abilityGroups = legalActions.OfType<TurnAction.UseAbility>()
            .GroupBy(a => a.Mode?.DisplayName ?? abilityName);
        foreach (var group in abilityGroups)
        {
            var groupActions = group.Cast<TurnAction>().ToList();
            AddButton(_buttonsBox, $"{group.Key} ({groupActions.Count} Ziele)", () => SelectGroup(groupActions));
        }

        if (legalActions.Any(a => a is TurnAction.EndTurn))
        {
            AddButton(_buttonsBox, "Passen / Zug beenden", () => ActionChosen?.Invoke(TurnAction.EndTurn.Instance));
        }
    }

    private void SelectGroup(List<TurnAction> group)
    {
        _pendingGroup = group;
        var fields = group.Select(TargetFieldOf).Where(f => f.HasValue).Select(f => f!.Value).ToList();
        // Bewegungsziele werden als "erreichbar" (grün), alle anderen
        // Zielarten als "gültiges Ziel der gewählten Aktion" (orange)
        // hervorgehoben (Vorgabe Punkt 5).
        bool isMove = group.Count > 0 && group[0] is TurnAction.Move;
        HighlightsChanged?.Invoke(isMove ? fields : Array.Empty<HexCoordinate>(), isMove ? Array.Empty<HexCoordinate>() : fields);
    }

    private void BuildFallbackList(IReadOnlyList<TurnAction> legalActions)
    {
        ClearChildren(_fallbackListBox);
        foreach (var action in legalActions)
        {
            AddButton(_fallbackListBox, DescribeAction(action), () => ActionChosen?.Invoke(action));
        }
    }

    private static HexCoordinate? TargetFieldOf(TurnAction action) => action switch
    {
        TurnAction.Move move => move.Target,
        TurnAction.Attack attack => attack.Target.Position,
        TurnAction.UseAbility ability => ability.Target switch
        {
            AbilityTarget.UnitTarget u => u.Unit.Position,
            AbilityTarget.FieldTarget f => f.Field,
            _ => null,
        },
        _ => null,
    };

    private void AddButton(VBoxContainer parent, string text, Action onPressed)
    {
        var button = new Button
        {
            Text = text,
            ClipText = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 32),
            Disabled = !_interactionEnabled, // neu erzeugte Schaltflächen übernehmen den aktuellen Sperrzustand (siehe InteractionEnabled)
        };
        button.Pressed += onPressed;
        parent.AddChild(button);
    }

    private static void ClearChildren(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            child.QueueFree();
        }
    }

    private static string DescribeUnit(Unit unit) =>
        (unit.IsClone ? "~" : "") + (unit.DisplayNameOverride ?? (unit is Hero hero ? hero.Definition.DisplayName : "Standardfigur"));

    /// <summary>Vollständige, eindeutige Beschreibung EINES Zuges - für die Fallback-Liste (Vorgabe Punkt 5).</summary>
    public static string DescribeAction(TurnAction action) => action switch
    {
        TurnAction.Move move => $"Bewegen nach {move.Target}",
        TurnAction.Attack attack => $"Angreifen: {DescribeUnit(attack.Target)} ({attack.Target.CurrentHitPoints}/{attack.Target.MaxHitPoints} HP auf {attack.Target.Position})",
        TurnAction.UseAbility ability => DescribeAbility(ability),
        TurnAction.EndTurn => "Passen / Zug beenden",
        _ => action.GetType().Name,
    };

    private static string DescribeAbility(TurnAction.UseAbility ability)
    {
        string targetPart = ability.Target switch
        {
            AbilityTarget.UnitTarget u => $"Ziel: {DescribeUnit(u.Unit)} auf {u.Unit.Position}",
            AbilityTarget.FieldTarget f => $"Zielfeld: {f.Field}",
            _ => "",
        };
        string modePart = ability.Mode is not null ? $" [Modus: {ability.Mode.DisplayName}]" : "";
        return $"Fähigkeit ({targetPart}){modePart}";
    }
}
