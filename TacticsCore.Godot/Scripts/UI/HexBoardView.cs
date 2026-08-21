using Godot;
using TacticsCore.Board;
using TacticsCore.Hex;
using TacticsCore.Units;
using TacticsCoreGodot.Shared;
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCoreGodot.UI;

/// <summary>
/// Zeichnet das Spielfeld (16 Hexfelder, siehe <see cref="TacticsCore.Board.Board"/>)
/// und die aktuell darauf stehenden Einheiten, und bildet Mausklicks auf
/// Hexfelder ab (Vorgabe Punkt 3/5). Kennt selbst KEINE Spielregel - sie
/// bekommt ausschließlich ein bereits fertiges <see cref="BoardRenderModel"/>
/// zum Anzeigen sowie (von außen gesetzte) Mengen hervorzuhebender Felder
/// übergeben; welche Felder das sein sollen, entscheidet immer der
/// Bildschirm, der TacticsCore nach den legalen Zügen fragt (siehe
/// MatchScreen/ReplayScreen), nie diese Klasse selbst.
///
/// Bewusst als reine, programmatisch gezeichnete <see cref="Control"/> ohne
/// eigene Kindknoten in der Szene umgesetzt (kein <c>UnitToken</c>-Szenen-
/// Prefab): Feld- und Einheitenanzahl ändern sich laufend, eine
/// handgepflegte Szenen-Hierarchie dafür wäre nur eine zweite,
/// fehleranfällige Quelle der Wahrheit neben diesem Skript.
/// </summary>
public partial class HexBoardView : Control
{
    private readonly GameBoard _board = new();
    private readonly Dictionary<HexCoordinate, Vector2> _fieldCenters = new();
    private Vector2 _origin;

    private BoardRenderModel? _model;
    private HashSet<HexCoordinate> _reachableFields = new();
    private HashSet<HexCoordinate> _targetFields = new();
    private HexCoordinate? _selectedUnitField;

    /// <summary>Der Benutzer hat auf ein tatsächlich existierendes Feld geklickt.</summary>
    public event Action<HexCoordinate>? FieldClicked;

    public override void _Ready()
    {
        foreach (var field in _board.Fields)
        {
            _fieldCenters[field.Coordinate] = BoardGeometry.HexToPixel(field.Coordinate);
        }

        // Ursprung so verschieben, dass alle Felder (inklusive ihrer
        // Ausdehnung) innerhalb des Kontrollrechtecks sichtbar sind, egal
        // wie die axialen Koordinaten (auch negativ) verteilt sind.
        float minX = _fieldCenters.Values.Min(p => p.X) - BoardGeometry.HexSize;
        float maxX = _fieldCenters.Values.Max(p => p.X) + BoardGeometry.HexSize;
        float minY = _fieldCenters.Values.Min(p => p.Y) - BoardGeometry.HexSize * 1.8f; // zusätzlicher Rand unten für HP-/Effekttext
        float maxY = _fieldCenters.Values.Max(p => p.Y) + BoardGeometry.HexSize * 1.8f;

        _origin = new Vector2(-minX, -minY);
        CustomMinimumSize = new Vector2(maxX - minX, maxY - minY);
        MouseFilter = MouseFilterEnum.Pass;
    }

    /// <summary>Vom Aufrufer nach jedem Ereignis mit dem aktuellen Zustand aufzurufen (siehe GameSession/ReplayTimeline).</summary>
    public void SetRenderModel(BoardRenderModel model)
    {
        _model = model;
        QueueRedraw();
    }

    /// <summary>Legt fest, welche Felder als "erreichbar" bzw. "gültiges Ziel" hervorgehoben werden, und welche Einheit gerade ausgewählt ist.</summary>
    public void SetHighlights(IEnumerable<HexCoordinate> reachable, IEnumerable<HexCoordinate> targets, HexCoordinate? selectedUnitField)
    {
        _reachableFields = reachable.ToHashSet();
        _targetFields = targets.ToHashSet();
        _selectedUnitField = selectedUnitField;
        QueueRedraw();
    }

    public void ClearHighlights() => SetHighlights(Array.Empty<HexCoordinate>(), Array.Empty<HexCoordinate>(), null);

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mouseButton)
        {
            var local = mouseButton.Position - _origin;
            var hex = BoardGeometry.PixelToHex(local);
            if (_board.Contains(hex))
            {
                FieldClicked?.Invoke(hex);
            }
        }
    }

    public override void _Draw()
    {
        foreach (var field in _board.Fields)
        {
            DrawField(field);
        }

        if (_model is null)
        {
            return;
        }

        foreach (var lockedField in _model.LockedFields)
        {
            DrawFieldOverlay(lockedField.Field, new Color(0.55f, 0.08f, 0.08f, 0.4f));
        }

        foreach (var stack in _model.Units.GroupBy(u => u.Position))
        {
            DrawUnitStack(stack.Key, stack.ToList());
        }
    }

    private void DrawField(Field field)
    {
        var center = _fieldCenters[field.Coordinate] + _origin;
        var corners = BoardGeometry.HexCorners(center);
        var closedCorners = corners.Append(corners[0]).ToArray();

        Color fill = field.Type switch
        {
            FieldType.BlossomA => new Color(0.27f, 0.45f, 0.78f),
            FieldType.BlossomB => new Color(0.78f, 0.33f, 0.30f),
            _ => new Color(0.55f, 0.55f, 0.58f), // Verbindungsfeld
        };

        DrawColoredPolygon(corners, fill);
        DrawPolyline(closedCorners, new Color(0, 0, 0, 0.6f), 2f);

        if (field.IsStartField)
        {
            DrawArc(center, BoardGeometry.HexSize * 0.55f, 0, Mathf.Tau, 24, new Color(1f, 0.85f, 0.2f), 3f);
        }

        if (_reachableFields.Contains(field.Coordinate))
        {
            DrawColoredPolygon(corners, new Color(0.3f, 0.9f, 0.3f, 0.35f));
        }

        if (_targetFields.Contains(field.Coordinate))
        {
            DrawColoredPolygon(corners, new Color(1f, 0.7f, 0.1f, 0.45f));
        }

        if (_selectedUnitField is { } selected && selected.Equals(field.Coordinate))
        {
            DrawPolyline(closedCorners, Colors.White, 4f);
        }

        string label = $"{field.Coordinate.Q},{field.Coordinate.R}";
        DrawString(ThemeDB.FallbackFont, center + new Vector2(-16, BoardGeometry.HexSize * 0.9f), label, HorizontalAlignment.Center, 32, 12, new Color(0, 0, 0, 0.55f));
    }

    private void DrawFieldOverlay(HexCoordinate coordinate, Color color)
    {
        if (!_fieldCenters.TryGetValue(coordinate, out var fieldCenter))
        {
            return;
        }

        var corners = BoardGeometry.HexCorners(fieldCenter + _origin, BoardGeometry.HexSize * 0.9f);
        DrawColoredPolygon(corners, color);
    }

    private void DrawUnitStack(HexCoordinate field, IReadOnlyList<UnitInfo> units)
    {
        if (!_fieldCenters.TryGetValue(field, out var fieldCenter))
        {
            return;
        }

        var center = fieldCenter + _origin;
        const float radius = 15f;

        // Mehrere Einheiten auf demselben Feld sind laut Regeln erlaubt
        // (Vorgabe Punkt 4: "müssen alle erkennbar sein") - kreisförmig um
        // die Feldmitte versetzt statt übereinandergelegt.
        for (int i = 0; i < units.Count; i++)
        {
            Vector2 offset = units.Count == 1
                ? Vector2.Zero
                : Vector2.FromAngle(Mathf.Tau * i / units.Count) * (BoardGeometry.HexSize * 0.42f);
            DrawUnit(center + offset, units[i], radius);
        }
    }

    private void DrawUnit(Vector2 position, UnitInfo unit, float radius)
    {
        Color teamColor = unit.Team == Team.TeamA ? new Color(0.2f, 0.5f, 1f) : new Color(1f, 0.42f, 0.32f);
        if (unit.IsClone)
        {
            teamColor = teamColor.Lightened(0.5f);
        }

        DrawCircle(position, radius, teamColor);
        DrawArc(position, radius, 0, Mathf.Tau, 24, unit.IsClone ? Colors.White : new Color(0, 0, 0, 0.7f), unit.IsClone ? 2f : 1.5f);

        // Bewegungsunfähigkeit generisch sichtbar machen (Vorgabe Punkt 4) -
        // unabhängig davon, WELCHER Effekt sie auslöst (Wand-Feldsperre,
        // Bogenschütze-Aufladen, ...): ein zusätzlicher magentafarbener Ring.
        if (unit.IsImmobilized)
        {
            DrawArc(position, radius + 4, 0, Mathf.Tau, 16, new Color(0.85f, 0.1f, 0.85f), 2.5f);
        }

        float barWidth = radius * 2.4f;
        const float barHeight = 5f;
        var barTopLeft = position + new Vector2(-barWidth / 2, radius + 4);
        DrawRect(new Rect2(barTopLeft, new Vector2(barWidth, barHeight)), new Color(0, 0, 0, 0.6f));
        float fraction = unit.MaxHp <= 0 ? 0f : Mathf.Clamp((float)unit.CurrentHp / unit.MaxHp, 0f, 1f);
        Color hpColor = fraction > 0.5f ? new Color(0.3f, 0.85f, 0.3f) : fraction > 0.25f ? new Color(0.9f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
        DrawRect(new Rect2(barTopLeft, new Vector2(barWidth * fraction, barHeight)), hpColor);

        string nameLabel = (unit.IsClone ? "~" : "") + unit.DisplayName;
        DrawString(ThemeDB.FallbackFont, position + new Vector2(-40, -radius - 8), nameLabel, HorizontalAlignment.Center, 80, 11, Colors.White);
        DrawString(ThemeDB.FallbackFont, position + new Vector2(-40, radius + 20), $"{unit.CurrentHp}/{unit.MaxHp}", HorizontalAlignment.Center, 80, 10, Colors.White);

        if (unit.Effects.Count > 0)
        {
            string effectsLabel = string.Join(" ", unit.Effects.Select(e => e.ShortLabel));
            DrawString(ThemeDB.FallbackFont, position + new Vector2(-60, radius + 32), effectsLabel, HorizontalAlignment.Center, 120, 9, new Color(1f, 1f, 0.55f));
        }
    }
}
