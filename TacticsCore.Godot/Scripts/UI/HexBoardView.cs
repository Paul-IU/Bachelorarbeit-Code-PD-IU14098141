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
/// LAYOUT (überarbeitet): Diese Steuerung bekommt vom umgebenden
/// PanelContainer die volle verfügbare Fläche der mittleren Spalte
/// zugewiesen (kein ScrollContainer mehr darum). Die Hexfeldgröße ist daher
/// KEINE feste Konstante mehr, sondern wird bei jeder Größenänderung dieser
/// Steuerung (Resized-Signal) aus der tatsächlichen Ausdehnung aller 16
/// Felder UND der aktuell verfügbaren Fläche neu berechnet (siehe
/// RecomputeLayout): das komplette Spielfeld bleibt so bei jeder
/// Fenstergröße ohne Scrollen sichtbar und wirkt zentriert.
///
/// Bewusst als reine, programmatisch gezeichnete <see cref="Control"/> ohne
/// eigene Kindknoten in der Szene umgesetzt (kein <c>UnitToken</c>-Szenen-
/// Prefab): Feld- und Einheitenanzahl ändern sich laufend, eine
/// handgepflegte Szenen-Hierarchie dafür wäre nur eine zweite,
/// fehleranfällige Quelle der Wahrheit neben diesem Skript.
/// </summary>
public partial class HexBoardView : Control
{
    // -----------------------------------------------------------------
    // RANDKONSTANTEN (Vorgabe: "Randanteil deutlich reduziert, als benannte
    // Konstante zum Nachjustieren"). Zwei getrennte Anteile:
    //   - HexBoundsPadding: Platz UM die reine Feld-Ausdehnung herum (in
    //     Einheits-Hexgröße, also z.B. 1.0 = eine Hexfeld-Breite), damit die
    //     Ecken der äußersten Hexfelder sowie die HP-/Namens-/Effekttext-
    //     Beschriftung der äußersten Einheiten nicht abgeschnitten wirken.
    //     Ein flat-top-Hexfeld reicht in X um genau 1x Hexgröße über seinen
    //     Mittelpunkt hinaus, in Y nur um sin(60°)*Hexgröße ≈ 0.87x - X
    //     bleibt daher bei 1.0 (geometrisch notwendig), Y wurde von vormals
    //     1.7 auf 0.95 gesenkt (knapp über dem geometrischen Minimum von
    //     ≈0.87, mit kleinem Rest-Puffer für Beschriftungen).
    //   - ZoomMarginFactor: zusätzlicher, rein optischer Rand zum Rahmen des
    //     umgebenden Panels (1.0 = kein Rand). Von vormals 0.92 auf 0.99
    //     gesenkt.
    // Zusammen ergibt das rechnerisch rund 23% mehr Hexfeldgröße bei
    // typischen Fenstergrößen (Höhe ist praktisch immer die begrenzende
    // Dimension, siehe RecomputeLayout) - nahe am gewünschten Richtwert von
    // 25%, mit bewusst kleinem Sicherheitspuffer statt exakt am rechnerischen
    // Minimum. Beide Werte hier bei Bedarf weiter anpassen.
    // -----------------------------------------------------------------
    private const float HexBoundsPaddingX = 1.0f;
    private const float HexBoundsPaddingY = 0.95f;
    private const float ZoomMarginFactor = 0.99f;

    private readonly GameBoard _board = new();

    /// <summary>Zentrale Zuordnung Feldtyp/Heldenkennung -> Textur (siehe VisualAssetCatalog) - fehlt eine Datei, liefert sie null und hier wird wie bisher mit Formen/Farben gezeichnet.</summary>
    private readonly VisualAssetCatalog _assets = VisualAssetCatalog.LoadCatalog();

    /// <summary>Feldmittelpunkte bei EINHEITS-Hexgröße (size=1) - die Basis, aus der RecomputeLayout die tatsächliche Anzeige ableitet.</summary>
    private readonly Dictionary<HexCoordinate, Vector2> _unitCenters = new();
    private Vector2 _unitBoundsMin;
    private Vector2 _unitBoundsMax;

    /// <summary>Aktuell berechnete Hexfeldgröße bzw. Bildschirm-Ursprung - beides wird bei jeder Größenänderung neu bestimmt, siehe RecomputeLayout.</summary>
    private float _hexSize = 1f;
    private Vector2 _origin;

    private BoardRenderModel? _model;
    private HashSet<HexCoordinate> _reachableFields = new();
    private HashSet<HexCoordinate> _targetFields = new();
    private HexCoordinate? _selectedUnitField;

    // -----------------------------------------------------------------
    // ANIMATIONS-OVERLAY (siehe Scripts/Animation/EventAnimationPlayer):
    // rein visuelle Zusatzzustände, die zusätzlich zum bzw. anstelle des
    // eigentlichen BoardRenderModel gezeichnet werden. HexBoardView kennt
    // dabei WEDER Spiellogik NOCH Timing - sie bietet nur diese
    // Ansatzpunkte zum Setzen/Löschen an; WANN/WIE LANGE etwas sichtbar
    // ist, entscheidet ausschließlich EventAnimationPlayer. Ohne aktive
    // Animation (alle Dictionaries/Listen leer) verhält sich das Zeichnen
    // exakt wie zuvor.
    // -----------------------------------------------------------------
    private readonly Dictionary<Guid, Vector2> _unitPositionOverrides = new(); // Einheits-Koordinaten, wie _unitCenters
    private readonly Dictionary<Guid, float> _unitAlphaOverrides = new();
    private readonly Dictionary<Guid, UnitInfo> _fadingGhosts = new(); // besiegte/verschwundene Einheiten, die noch ausblenden (im Modell bereits nicht mehr enthalten)
    private readonly List<FloatingLabel> _floatingLabels = new();
    private readonly List<ProjectileVisual> _projectiles = new();
    private readonly Dictionary<HexCoordinate, FieldFlash> _fieldFlashes = new();

    /// <summary>Ein kurzzeitig über einem Feld eingeblendeter Text (Schadenszahl, Heilzahl, Effekt-Tick, ...) - Position/Alpha werden vom Animator per Tween live verändert.</summary>
    public sealed class FloatingLabel
    {
        public required Vector2 UnitSpacePosition;
        public required string Text;
        public required Color Color;
        public float YOffset;
        public float Alpha = 1f;
    }

    /// <summary>Ein einzelnes fliegendes Geschoss (Fernkampf/Fähigkeit mit Ziel) - Fortschritt 0..1 wird vom Animator per Tween live verändert.</summary>
    public sealed class ProjectileVisual
    {
        public required Vector2 FromUnitSpace;
        public required Vector2 ToUnitSpace;
        public Texture2D? Texture;
        public required Color FallbackColor;
        public float Progress;
    }

    /// <summary>
    /// Kurzes Aufleuchten eines Feldes (Schild/Schadensreduktion/Fähigkeits-
    /// Einsatz ohne eigenes Zielgeschoss, z.B. Musiker-Lied) - Intensität
    /// 0..1 wird vom Animator per Tween live verändert. Ist eine Fähigkeits-
    /// spezifische Grafik hinterlegt (siehe AnimationAssetCatalog), wird sie
    /// statt der reinen Farbfläche gezeichnet.
    /// </summary>
    public sealed class FieldFlash
    {
        public required Color Color;
        public Texture2D? Texture;
        public float Intensity;
    }

    /// <summary>Der Benutzer hat auf ein tatsächlich existierendes Feld geklickt.</summary>
    public event Action<HexCoordinate>? FieldClicked;

    /// <summary>Ob das Spielfeld aktuell Mausklicks annimmt (siehe EventAnimationPlayer: während einer Animation gesperrt).</summary>
    public bool InteractionEnabled { get; set; } = true;

    /// <summary>Die aktuell berechnete Hexfeldgröße in Pixeln - für den Animator, um Bewegungsdistanzen (z.B. aufsteigende Zahlen) proportional zur aktuellen Zoomstufe zu bemessen.</summary>
    public float CurrentHexSize => _hexSize;

    /// <summary>Entfernt JEDE aktive visuelle Überlagerung (siehe EventAnimationPlayer.ShowInstantly) - für einen Sprung ohne Animation, der keine "eingefrorene" Animation vom vorherigen Zustand übrig lassen darf.</summary>
    public void ClearAllAnimationOverlays()
    {
        _unitPositionOverrides.Clear();
        _unitAlphaOverrides.Clear();
        _fadingGhosts.Clear();
        _floatingLabels.Clear();
        _projectiles.Clear();
        _fieldFlashes.Clear();
        QueueRedraw();
    }

    public override void _Ready()
    {
        foreach (var field in _board.Fields)
        {
            _unitCenters[field.Coordinate] = BoardGeometry.HexToPixel(field.Coordinate, 1f);
        }

        // Ausdehnung bei Einheitsgröße 1 inkl. Rand für die Hex-Ecken selbst
        // und Beschriftungen unter/über den Einheiten (siehe
        // HexBoundsPaddingX/Y oben). Diese Werte sind unabhängig von der
        // späteren Bildschirmgröße - erst RecomputeLayout skaliert sie auf
        // den tatsächlich verfügbaren Platz.
        float minX = _unitCenters.Values.Min(p => p.X) - HexBoundsPaddingX;
        float maxX = _unitCenters.Values.Max(p => p.X) + HexBoundsPaddingX;
        float minY = _unitCenters.Values.Min(p => p.Y) - HexBoundsPaddingY;
        float maxY = _unitCenters.Values.Max(p => p.Y) + HexBoundsPaddingY;
        _unitBoundsMin = new Vector2(minX, minY);
        _unitBoundsMax = new Vector2(maxX, maxY);

        // Nur ein kleiner technischer Mindestboden, damit die Steuerung nie
        // auf 0x0 zusammenfällt - die eigentliche Größe bestimmt allein der
        // umgebende Container (ExpandFill in der mittleren Spalte).
        CustomMinimumSize = new Vector2(200, 200);
        MouseFilter = MouseFilterEnum.Pass;

        Resized += RecomputeLayout;
        RecomputeLayout();
    }

    /// <summary>
    /// Berechnet Zoomfaktor (_hexSize) und Mittelpunkt-Verschiebung (_origin)
    /// neu, ausgehend von den minimalen/maximalen Bildschirmkoordinaten
    /// aller Felder (bei Einheitsgröße) und der aktuell tatsächlich
    /// verfügbaren Fläche dieser Steuerung. Wird bei jeder Größenänderung
    /// (Resized-Signal) erneut aufgerufen - keine feste Pixelgröße irgendwo
    /// fest verdrahtet.
    /// </summary>
    private void RecomputeLayout()
    {
        var unitExtent = _unitBoundsMax - _unitBoundsMin;
        if (unitExtent.X <= 0 || unitExtent.Y <= 0 || Size.X <= 0 || Size.Y <= 0)
        {
            return;
        }

        float scale = Mathf.Min(Size.X / unitExtent.X, Size.Y / unitExtent.Y) * ZoomMarginFactor;
        _hexSize = Mathf.Max(scale, 4f); // nie auf (praktisch) 0 schrumpfen, falls das Panel extrem klein wird

        var unitCenter = (_unitBoundsMin + _unitBoundsMax) / 2f;
        // So verschieben, dass der Mittelpunkt der (skalierten) Feld-
        // Ausdehnung genau auf den Mittelpunkt der Steuerung fällt - das
        // Spielfeld erscheint dadurch unabhängig von seiner axialen
        // Koordinatenverteilung immer zentriert.
        _origin = Size / 2f - unitCenter * _hexSize;

        QueueRedraw();
    }

    /// <summary>Bildschirmposition eines Feldmittelpunkts bei der aktuell berechneten Größe/Zentrierung.</summary>
    private Vector2 FieldCenter(HexCoordinate coordinate) => _unitCenters[coordinate] * _hexSize + _origin;

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

    // -----------------------------------------------------------------
    // Animations-Overlay: Mutatoren für EventAnimationPlayer. Jede Methode
    // löst selbst QueueRedraw() aus - der Aufrufer muss sich darum nicht
    // kümmern.
    // -----------------------------------------------------------------

    public void SetUnitPositionOverride(Guid unitId, Vector2 unitSpacePosition)
    {
        _unitPositionOverrides[unitId] = unitSpacePosition;
        QueueRedraw();
    }

    public void ClearUnitPositionOverride(Guid unitId)
    {
        _unitPositionOverrides.Remove(unitId);
        QueueRedraw();
    }

    public void SetUnitAlphaOverride(Guid unitId, float alpha)
    {
        _unitAlphaOverrides[unitId] = alpha;
        QueueRedraw();
    }

    public void ClearUnitAlphaOverride(Guid unitId)
    {
        _unitAlphaOverrides.Remove(unitId);
        QueueRedraw();
    }

    /// <summary>Zeigt eine bereits aus dem Modell entfernte Einheit (besiegt/Klon verschwunden) weiter an, solange sie noch ausblendet - siehe RemoveFadingGhost.</summary>
    public void SetFadingGhost(UnitInfo lastKnownInfo, float alpha)
    {
        _fadingGhosts[lastKnownInfo.Id] = lastKnownInfo;
        _unitAlphaOverrides[lastKnownInfo.Id] = alpha;
        QueueRedraw();
    }

    public void RemoveFadingGhost(Guid unitId)
    {
        _fadingGhosts.Remove(unitId);
        _unitAlphaOverrides.Remove(unitId);
        QueueRedraw();
    }

    public FloatingLabel AddFloatingLabel(HexCoordinate field, string text, Color color)
    {
        var label = new FloatingLabel { UnitSpacePosition = _unitCenters[field], Text = text, Color = color };
        _floatingLabels.Add(label);
        QueueRedraw();
        return label;
    }

    public void RemoveFloatingLabel(FloatingLabel label)
    {
        _floatingLabels.Remove(label);
        QueueRedraw();
    }

    public ProjectileVisual AddProjectile(HexCoordinate from, HexCoordinate to, Texture2D? texture, Color fallbackColor)
    {
        var projectile = new ProjectileVisual { FromUnitSpace = _unitCenters[from], ToUnitSpace = _unitCenters[to], Texture = texture, FallbackColor = fallbackColor };
        _projectiles.Add(projectile);
        QueueRedraw();
        return projectile;
    }

    public void RemoveProjectile(ProjectileVisual projectile)
    {
        _projectiles.Remove(projectile);
        QueueRedraw();
    }

    public FieldFlash SetFieldFlash(HexCoordinate field, Color color, Texture2D? texture = null)
    {
        var flash = new FieldFlash { Color = color, Texture = texture };
        _fieldFlashes[field] = flash;
        QueueRedraw();
        return flash;
    }

    public void ClearFieldFlash(HexCoordinate field)
    {
        _fieldFlashes.Remove(field);
        QueueRedraw();
    }

    /// <summary>Erzwingt eine Neuzeichnung nach einer Tween-Zwischenänderung (Position/Alpha/Progress/Intensity einer bereits zurückgegebenen Instanz) - die Objekte selbst lösen das nicht automatisch aus.</summary>
    public void RequestRedraw() => QueueRedraw();

    public override void _GuiInput(InputEvent @event)
    {
        if (!InteractionEnabled)
        {
            return;
        }

        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } mouseButton)
        {
            // Rückrichtung der Zeichentransformation: erst die Zentrierung
            // entfernen, dann durch die aktuelle Hexgröße teilen - damit
            // liefert PixelToHex wieder axiale Koordinaten bei Einheitsgröße,
            // exakt wie sie in _unitCenters hinterlegt sind.
            var unitSpacePixel = (mouseButton.Position - _origin) / _hexSize;
            var hex = BoardGeometry.PixelToHex(unitSpacePixel, 1f);
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

        // Feld-Aufleuchten (Schild/Schadensreduktion bzw. Fähigkeits-Einsatz
        // ohne eigenes Zielgeschoss, Punkt 3/5 der Animationsvorgabe) -
        // unabhängig vom Modell, rein Overlay. Mit fähigkeitsspezifischer
        // Grafik (siehe FieldFlash.Texture/AnimationAssetCatalog) wird diese
        // statt der reinen Farbfläche gezeichnet - fehlt sie, bleibt es bei
        // der Farbfläche als Rückfallebene (nie ein Ausfall der Animation).
        foreach (var (field, flash) in _fieldFlashes)
        {
            if (!_unitCenters.ContainsKey(field))
            {
                continue;
            }

            var center = FieldCenter(field);
            if (flash.Texture is not null)
            {
                float side = _hexSize * 1.1f;
                var rect = new Rect2(center - new Vector2(side, side) / 2f, new Vector2(side, side));
                DrawTextureRect(flash.Texture, rect, false, new Color(1, 1, 1, flash.Intensity));
            }
            else
            {
                var corners = BoardGeometry.HexCorners(center, _hexSize * 0.95f);
                DrawColoredPolygon(corners, new Color(flash.Color, flash.Color.A * flash.Intensity));
            }
        }

        if (_model is not null)
        {
            foreach (var lockedField in _model.LockedFields)
            {
                DrawFieldOverlay(lockedField.Field, new Color(0.55f, 0.08f, 0.08f, 0.4f));
            }

            DrawUnits();
        }

        // Ausblendende "Geister" (besiegte/verschwundene Einheiten, siehe
        // SetFadingGhost) werden zusätzlich gezeichnet, obwohl sie im
        // AKTUELLEN Modell bereits nicht mehr enthalten sind. War der
        // Geist selbst angeheftet (z.B. Hugger stirbt, während er noch am
        // Ziel hängt), blendet er weiterhin als kleines Abzeichen an dessen
        // aktueller Position aus, statt kurz in voller Größe aufzutauchen.
        foreach (var ghost in _fadingGhosts.Values)
        {
            if (ghost.AttachedToUnitId is Guid hostId)
            {
                var host = _model?.Units.FirstOrDefault(u => u.Id == hostId);
                if (host is not null && _unitCenters.ContainsKey(host.Position))
                {
                    DrawAttachedBadge(FieldCenter(host.Position), _hexSize * 0.32f, ghost);
                    continue;
                }
            }

            if (_unitCenters.ContainsKey(ghost.Position))
            {
                DrawUnit(FieldCenter(ghost.Position), ghost, _hexSize * 0.32f);
            }
        }

        foreach (var projectile in _projectiles)
        {
            DrawProjectile(projectile);
        }

        foreach (var label in _floatingLabels)
        {
            DrawFloatingLabel(label);
        }
    }

    /// <summary>
    /// Zeichnet alle Einheiten des aktuellen Modells.
    ///
    /// ANGEHEFTETE EINHEITEN (Bachelorarbeit-Vorgabe, generisch über
    /// <see cref="UnitInfo.AttachedToUnitId"/> - siehe BoardRenderModel für
    /// die Herleitung, NICHT auf Hugger festgeschrieben): eine angeheftete
    /// Einheit zählt beim normalen Feld-Stapel NICHT mehr mit, sondern wird
    /// erst NACHDEM ihr Ziel gezeichnet wurde als kleines Abzeichen genau an
    /// dessen tatsächlicher (ggf. gerade animierter) Bildschirmposition
    /// ergänzt - dadurch folgt sie einem sich bewegenden Ziel automatisch,
    /// ohne eigene Nachverfolgung.
    ///
    /// Einheiten mit einer aktiven Positions-Override (siehe
    /// SetUnitPositionOverride, z.B. mitten in einer Bewegungsanimation)
    /// werden EINZELN an ihrer überschriebenen Position gezeichnet statt im
    /// Stapel ihres (bereits erreichten) Zielfelds - sie gehören dort
    /// visuell noch nicht dazu.
    /// </summary>
    private void DrawUnits()
    {
        var attachedByTarget = _model!.Units
            .Where(u => u.AttachedToUnitId is not null)
            .ToLookup(u => u.AttachedToUnitId!.Value);
        bool hasAttachments = attachedByTarget.Count > 0;

        var animatedUnitIds = _unitPositionOverrides.Count == 0 ? null : new HashSet<Guid>(_unitPositionOverrides.Keys);

        IEnumerable<UnitInfo> drawableNormally = _model.Units;
        if (hasAttachments)
        {
            drawableNormally = drawableNormally.Where(u => u.AttachedToUnitId is null);
        }

        if (animatedUnitIds is not null)
        {
            drawableNormally = drawableNormally.Where(u => !animatedUnitIds.Contains(u.Id));
        }

        foreach (var stack in drawableNormally.GroupBy(u => u.Position))
        {
            DrawUnitStack(stack.Key, stack.ToList(), attachedByTarget);
        }

        if (animatedUnitIds is null)
        {
            return;
        }

        foreach (var unitId in animatedUnitIds)
        {
            var info = _model.Units.FirstOrDefault(u => u.Id == unitId);
            if (info is null || info.AttachedToUnitId is not null)
            {
                continue; // eine angeheftete Einheit wird nie einzeln animiert bewegt - siehe Klassendoku
            }

            var position = _unitPositionOverrides[unitId] * _hexSize + _origin;
            DrawUnit(position, info, _hexSize * 0.32f);
            DrawAttachedFollowers(position, unitId, _hexSize * 0.32f, attachedByTarget);
        }
    }

    /// <summary>Zeichnet für jede an <paramref name="hostId"/> angeheftete Einheit ein kleines Abzeichen versetzt oben rechts von <paramref name="hostScreenPosition"/> (Vorgabe: "kleiner, versetzt, darf das Ziel nicht verdecken").</summary>
    private void DrawAttachedFollowers(Vector2 hostScreenPosition, Guid hostId, float hostRadius, ILookup<Guid, UnitInfo> attachedByTarget)
    {
        if (!attachedByTarget.Contains(hostId))
        {
            return;
        }

        foreach (var follower in attachedByTarget[hostId])
        {
            DrawAttachedBadge(hostScreenPosition, hostRadius, follower);
        }
    }

    /// <summary>
    /// Verkleinerte Darstellung (etwa halbe Größe, Vorgabe Punkt 5) einer
    /// angehefteten Einheit - schlicht dieselbe DrawUnit-Zeichnung bei
    /// halbem Radius, nur an einer versetzten Position statt im normalen
    /// Feld-Stapel. Der Versatz (oben rechts, außerhalb des Wirt-Kreises)
    /// verdeckt das Ziel nicht; der HP-Balken bleibt Teil von DrawUnit und
    /// damit ablesbar (Vorgabe: "Verbündete können ihn heilen").
    /// </summary>
    private void DrawAttachedBadge(Vector2 hostScreenPosition, float hostRadius, UnitInfo attachedUnit)
    {
        float badgeRadius = hostRadius * 0.5f;
        var offsetDirection = new Vector2(1f, -1f).Normalized(); // oben rechts
        var badgePosition = hostScreenPosition + offsetDirection * (hostRadius + badgeRadius * 0.6f);
        DrawUnit(badgePosition, attachedUnit, badgeRadius);
    }

    private void DrawProjectile(ProjectileVisual projectile)
    {
        var from = projectile.FromUnitSpace * _hexSize + _origin;
        var to = projectile.ToUnitSpace * _hexSize + _origin;
        var position = from.Lerp(to, Mathf.Clamp(projectile.Progress, 0f, 1f));

        if (projectile.Texture is not null)
        {
            float side = _hexSize * 0.5f;
            var rect = new Rect2(position - new Vector2(side, side) / 2f, new Vector2(side, side));
            DrawTextureRect(projectile.Texture, rect, false);
        }
        else
        {
            // Rückfallebene: einfache eingefärbte Form (Vorgabe: "fehlt eine
            // Grafik, generischer Platzhalter statt Ausfall der Animation").
            DrawCircle(position, _hexSize * 0.16f, projectile.FallbackColor);
        }
    }

    private void DrawFloatingLabel(FloatingLabel label)
    {
        var basePosition = label.UnitSpacePosition * _hexSize + _origin + new Vector2(0, -_hexSize * 0.9f + label.YOffset);
        int fontSize = (int)Mathf.Clamp(_hexSize * 0.22f, 10, 26);
        float width = Mathf.Max(_hexSize * 2f, 80f);
        DrawString(ThemeDB.FallbackFont, basePosition - new Vector2(width / 2, 0), label.Text, HorizontalAlignment.Center, width, fontSize, new Color(label.Color, label.Alpha));
    }

    private void DrawField(Field field)
    {
        var center = FieldCenter(field.Coordinate);
        var corners = BoardGeometry.HexCorners(center, _hexSize);
        var closedCorners = corners.Append(corners[0]).ToArray();

        // Eigene Grafik je Feldtyp, falls hinterlegt (siehe
        // VisualAssetCatalog) - sonst unverändert die bisherige Fläche in
        // Teamfarbe/Grundfarbe als Rückfallebene.
        var fieldTexture = _assets.GetFieldTexture(field.Type);
        if (fieldTexture is not null)
        {
            DrawHexTexture(fieldTexture, center);
        }
        else
        {
            Color fill = field.Type switch
            {
                FieldType.BlossomA => new Color(0.27f, 0.45f, 0.78f),
                FieldType.BlossomB => new Color(0.78f, 0.33f, 0.30f),
                _ => new Color(0.55f, 0.55f, 0.58f), // Verbindungsfeld
            };
            DrawColoredPolygon(corners, fill);
        }

        DrawPolyline(closedCorners, new Color(0, 0, 0, 0.6f), 2f);

        if (field.IsStartField)
        {
            var startOverlay = _assets.GetStartFieldOverlay();
            if (startOverlay is not null)
            {
                DrawHexTexture(startOverlay, center);
            }
            else
            {
                DrawArc(center, _hexSize * 0.55f, 0, Mathf.Tau, 24, new Color(1f, 0.85f, 0.2f), 3f);
            }
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

        int coordFontSize = (int)Mathf.Clamp(_hexSize * 0.16f, 9, 20);
        string label = $"{field.Coordinate.Q},{field.Coordinate.R}";
        DrawString(ThemeDB.FallbackFont, center + new Vector2(-_hexSize * 0.3f, _hexSize * 0.9f), label, HorizontalAlignment.Center, _hexSize * 0.7f, coordFontSize, new Color(0, 0, 0, 0.55f));
    }

    private void DrawFieldOverlay(HexCoordinate coordinate, Color fallbackColor)
    {
        if (!_unitCenters.ContainsKey(coordinate))
        {
            return;
        }

        var center = FieldCenter(coordinate);
        var lockedOverlay = _assets.GetLockedFieldOverlay();
        if (lockedOverlay is not null)
        {
            DrawHexTexture(lockedOverlay, center);
        }
        else
        {
            var corners = BoardGeometry.HexCorners(center, _hexSize * 0.9f);
            DrawColoredPolygon(corners, fallbackColor);
        }
    }

    /// <summary>
    /// Zeichnet eine Textur zentriert über einem Hexfeld, in der Bounding-
    /// Box eines flat-top-Hexfelds bei der aktuellen Hexgröße (Breite =
    /// 2*_hexSize, Höhe = sqrt(3)*_hexSize - siehe BoardGeometry/
    /// Art/ART_README.md für die genaue Ausrichtung). Skaliert dadurch
    /// automatisch mit dem berechneten Zoomfaktor mit (Vorgabe Punkt 1) -
    /// eigene Positions-/Klick-Berechnung bleibt davon unberührt.
    /// </summary>
    private void DrawHexTexture(Texture2D texture, Vector2 center)
    {
        var size = new Vector2(_hexSize * 2f, _hexSize * Mathf.Sqrt(3f));
        DrawTextureRect(texture, new Rect2(center - size / 2f, size), false);
    }

    private void DrawUnitStack(HexCoordinate field, IReadOnlyList<UnitInfo> units, ILookup<Guid, UnitInfo> attachedByTarget)
    {
        if (!_unitCenters.ContainsKey(field))
        {
            return;
        }

        var center = FieldCenter(field);
        float radius = _hexSize * 0.32f;

        // Mehrere Einheiten auf demselben Feld sind laut Regeln erlaubt
        // (Vorgabe Punkt 4: "müssen alle erkennbar sein") - kreisförmig um
        // die Feldmitte versetzt statt übereinandergelegt.
        for (int i = 0; i < units.Count; i++)
        {
            Vector2 offset = units.Count == 1
                ? Vector2.Zero
                : Vector2.FromAngle(Mathf.Tau * i / units.Count) * (_hexSize * 0.42f);
            var position = center + offset;
            DrawUnit(position, units[i], radius);
            DrawAttachedFollowers(position, units[i].Id, radius, attachedByTarget);
        }
    }

    private void DrawUnit(Vector2 position, UnitInfo unit, float radius)
    {
        Color teamColor = unit.Team == Team.TeamA ? new Color(0.2f, 0.5f, 1f) : new Color(1f, 0.42f, 0.32f);
        if (unit.IsClone)
        {
            teamColor = teamColor.Lightened(0.5f);
        }

        // Klone sichtbar transparenter als echte Helden (Vorgabe Punkt 2:
        // "zum Beispiel durch Transparenz") - gilt für Formdarstellung UND
        // eigene Grafik gleichermaßen.
        float portraitAlpha = unit.IsClone ? 0.55f : 1f;
        var heroTexture = _assets.GetHeroTexture(unit.HeroId);

        // Zusätzliche Alpha-Überschreibung des Animators (Ein-/Ausblenden,
        // siehe SetUnitAlphaOverride/SetFadingGhost) - multipliziert sich
        // mit der Klon-Transparenz, statt sie zu ersetzen, damit ein
        // ausblendender Klon weiterhin als solcher erkennbar bleibt.
        float animatedAlpha = _unitAlphaOverrides.TryGetValue(unit.Id, out var overrideAlpha) ? overrideAlpha : 1f;
        // Lokaler Helfer: wendet BEIDE Alpha-Faktoren auf eine Farbe an - so
        // muss unten nicht jede einzelne DrawXxx-Stelle von Hand multiplizieren.
        Color WithAlpha(Color c, float baseAlpha = 1f) => new(c, c.A * baseAlpha * animatedAlpha);

        // visualRadius: bei eigener Grafik etwas größer als die reine
        // Portrait-Fläche, damit ein teamfarbener Rand ("Umrandung", Vorgabe
        // Punkt 2) sichtbar bleibt, an dem sich die Teamzugehörigkeit auch
        // mit fremder Grafik weiterhin auf den ersten Blick ablesen lässt.
        // HP-Balken/Name/Effekte richten sich nach diesem Wert, damit sie
        // nicht mit der (ggf. größeren) Grafik überlappen.
        float visualRadius = heroTexture is not null ? radius * 1.15f : radius;

        if (heroTexture is not null)
        {
            DrawCircle(position, visualRadius, WithAlpha(teamColor, portraitAlpha));
            float textureSide = radius * 2f;
            var rect = new Rect2(position - new Vector2(textureSide, textureSide) / 2f, new Vector2(textureSide, textureSide));
            DrawTextureRect(heroTexture, rect, false, WithAlpha(Colors.White, portraitAlpha));
        }
        else
        {
            // Rückfallebene: einfarbiger Kreis in Teamfarbe (unverändert zur bisherigen Darstellung, Vorgabe Punkt 3).
            DrawCircle(position, visualRadius, WithAlpha(teamColor, portraitAlpha));
        }

        DrawArc(position, visualRadius, 0, Mathf.Tau, 24, unit.IsClone ? WithAlpha(Colors.White) : WithAlpha(new Color(0, 0, 0, 0.7f)), unit.IsClone ? 2f : 1.5f);

        // Bewegungsunfähigkeit generisch sichtbar machen (Vorgabe Punkt 4) -
        // unabhängig davon, WELCHER Effekt sie auslöst (Wand-Feldsperre,
        // Bogenschütze-Aufladen, ...): ein zusätzlicher magentafarbener Ring.
        if (unit.IsImmobilized)
        {
            DrawArc(position, visualRadius + Mathf.Max(3f, radius * 0.25f), 0, Mathf.Tau, 16, WithAlpha(new Color(0.85f, 0.1f, 0.85f)), 2.5f);
        }

        float barWidth = radius * 2.4f;
        float barHeight = Mathf.Max(3f, radius * 0.3f);
        var barTopLeft = position + new Vector2(-barWidth / 2, visualRadius + barHeight * 0.8f);
        DrawRect(new Rect2(barTopLeft, new Vector2(barWidth, barHeight)), WithAlpha(new Color(0, 0, 0, 0.6f)));
        float fraction = unit.MaxHp <= 0 ? 0f : Mathf.Clamp((float)unit.CurrentHp / unit.MaxHp, 0f, 1f);
        Color hpColor = fraction > 0.5f ? new Color(0.3f, 0.85f, 0.3f) : fraction > 0.25f ? new Color(0.9f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
        DrawRect(new Rect2(barTopLeft, new Vector2(barWidth * fraction, barHeight)), WithAlpha(hpColor));

        int nameFontSize = (int)Mathf.Clamp(radius * 0.75f, 9, 20);
        int hpFontSize = (int)Mathf.Clamp(radius * 0.68f, 8, 18);
        int effectFontSize = (int)Mathf.Clamp(radius * 0.6f, 7, 16);
        float labelWidth = Mathf.Max(barWidth * 2.5f, 90f);

        // Verbleibende Fähigkeits-Abklingzeit (Punkt E) direkt am Namen
        // sichtbar machen, statt sie nur im Aktionspanel indirekt (fehlende
        // Fähigkeit in der Zugliste) erkennbar zu lassen.
        string cooldownSuffix = unit.AbilityCooldownRemaining is int cd ? $" (Abklingzeit: {cd})" : "";
        string nameLabel = (unit.IsClone ? "~" : "") + unit.DisplayName + cooldownSuffix;
        DrawString(ThemeDB.FallbackFont, position + new Vector2(-labelWidth / 2, -visualRadius - barHeight), nameLabel, HorizontalAlignment.Center, labelWidth, nameFontSize, WithAlpha(Colors.White));
        DrawString(ThemeDB.FallbackFont, position + new Vector2(-labelWidth / 2, barTopLeft.Y - position.Y + barHeight + hpFontSize + 4), $"{unit.CurrentHp}/{unit.MaxHp}", HorizontalAlignment.Center, labelWidth, hpFontSize, WithAlpha(Colors.White));

        if (unit.Effects.Count > 0)
        {
            string effectsLabel = string.Join(" ", unit.Effects.Select(e => e.ShortLabel));
            DrawString(ThemeDB.FallbackFont, position + new Vector2(-labelWidth / 2, barTopLeft.Y - position.Y + barHeight + hpFontSize + effectFontSize + 8), effectsLabel, HorizontalAlignment.Center, labelWidth, effectFontSize, WithAlpha(new Color(1f, 1f, 0.55f)));
        }
    }
}
