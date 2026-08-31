using Godot;
using TacticsCoreGodot.Shared;

namespace TacticsCoreGodot.Animation;

/// <summary>
/// Zentrale Zuordnung: welche Geschossgrafik gehört zu welcher Fähigkeit
/// bzw. zum Fernkampf-Grundangriff, welche Farbe zu welcher Wirkungsart
/// (Schaden/Heilung/Buff/Debuff/Schild/Effekt-Tick). Genau dasselbe Muster
/// wie <see cref="VisualAssetCatalog"/> (siehe dort für die ausführliche
/// Begründung): Godot-<see cref="Resource"/>, Pfade als Strings statt
/// direkter Texture2D-Exports (eine fehlende Datei darf die gesamte
/// Ressource nicht am Laden hindern), Rückfall auf eine generische,
/// eingefärbte Form, wenn keine Grafik hinterlegt ist - Animationen dürfen
/// nie ausfallen, nur "schlichter" aussehen.
///
/// EIGENE GRAFIKEN OHNE CODEÄNDERUNG: siehe Art/ART_README.md, Abschnitt
/// "Animationsgrafiken" - genau wie bei Feld-/Heldengrafiken entweder
/// Dateien unter den vorgegebenen Pfaden ablegen, oder eine
/// Art/AnimationAssetCatalog.tres mit abweichenden Pfaden anlegen.
/// </summary>
[GlobalClass]
public partial class AnimationAssetCatalog : Resource
{
    public const string OverrideResourcePath = "res://Art/AnimationAssetCatalog.tres";

    /// <summary>Geschossgrafik für den Grundangriff auf Distanz (kein Nahkampf) - Rückfall, wenn keine fähigkeitsspezifische Grafik greift.</summary>
    [Export] public string RangedAttackProjectileTexture { get; set; } = "res://Art/Projectiles/arrow.png";

    /// <summary>Fähigkeitskennung (AbilityDefinition.Id) -> Geschossgrafik. Im Godot-Inspector als Dictionary editierbar.</summary>
    [Export]
    public Godot.Collections.Dictionary<string, string> AbilityProjectileTexturePaths { get; set; } = new()
    {
        ["elementarschlag"] = "res://Art/Projectiles/fireball.png",
        ["heilung"] = "res://Art/Projectiles/heal_orb.png",
        ["umklammerung"] = "res://Art/Projectiles/grab_orb.png",
        ["lied"] = "res://Art/Projectiles/note.png",
    };

    /// <summary>
    /// Fähigkeitskennungen, bei denen sich der ANWENDER sichtbar zum Ziel
    /// bewegt, statt ein Geschoss zu verschicken (z.B. Hugger: "heftet sich
    /// an, wirkt nicht aus der Ferne") - siehe
    /// <see cref="EventAnimationPlayer.PlayUnitTargetedAbilityAsync"/>.
    /// Generisch nutzbar: jede künftige Fähigkeit mit ähnlicher Wirkung
    /// (Anwender begibt sich physisch zum Ziel) trägt hier einfach ihre Id
    /// ein, ohne dass die Animationsschicht selbst geändert werden müsste.
    /// </summary>
    [Export] public Godot.Collections.Array<string> AbilitiesUsingCasterMovement { get; set; } = new() { "umklammerung" };

    /// <summary>
    /// Fähigkeitskennung -&gt; Symbol/Text, das über JEDER betroffenen Einheit
    /// aufsteigt und ausblendet (Vorgabe: Musiker-Lied - "über jedem
    /// betroffenen der Noten-Effekt"). Generisch für jede flächenwirkende
    /// Fähigkeit nutzbar, nicht nur für Lied - fehlt ein Eintrag, entfällt
    /// dieser Zusatzeffekt einfach (der Feld-Flash bleibt davon unberührt).
    /// </summary>
    [Export]
    public Godot.Collections.Dictionary<string, string> AbilityFloatingSymbols { get; set; } = new()
    {
        ["lied"] = "♪",
    };

    // Farben für generische Wirkungsarten - sowohl als Rückfallebene für
    // Geschosse ohne eigene Grafik als auch für Farbüberlagerungen/
    // Schadenszahlen (siehe EventAnimationPlayer).
    [Export] public Color DamageColor { get; set; } = new(0.85f, 0.15f, 0.15f);
    [Export] public Color HealColor { get; set; } = new(0.25f, 0.85f, 0.35f);
    [Export] public Color BuffColor { get; set; } = new(0.3f, 0.55f, 0.95f);
    [Export] public Color DebuffColor { get; set; } = new(0.65f, 0.25f, 0.75f);
    [Export] public Color ShieldColor { get; set; } = new(0.95f, 0.85f, 0.25f);
    [Export] public Color EffectTickColor { get; set; } = new(0.9f, 0.6f, 0.15f);

    private readonly TextureCache _textureCache = new();

    public static AnimationAssetCatalog LoadCatalog()
    {
        if (ResourceLoader.Exists(OverrideResourcePath))
        {
            var loaded = GD.Load<AnimationAssetCatalog>(OverrideResourcePath);
            if (loaded is not null)
            {
                return loaded;
            }
        }

        return new AnimationAssetCatalog();
    }

    /// <summary>Geschossgrafik für eine Fähigkeit, oder die allgemeine Fernkampf-Grafik, wenn keine spezifische hinterlegt ist. Null, wenn auch diese fehlt (Aufrufer zeichnet dann die farbige Rückfallebene).</summary>
    public Texture2D? GetAbilityProjectileTexture(string abilityId)
    {
        if (AbilityProjectileTexturePaths.TryGetValue(abilityId, out var specificPath))
        {
            var specific = _textureCache.LoadOrNull(specificPath);
            if (specific is not null)
            {
                return specific;
            }
        }

        return _textureCache.LoadOrNull(RangedAttackProjectileTexture);
    }

    public Texture2D? GetRangedAttackProjectileTexture() => _textureCache.LoadOrNull(RangedAttackProjectileTexture);

    /// <summary>
    /// NUR die eigens für diese Fähigkeit hinterlegte Grafik, ohne Rückfall
    /// auf die allgemeine Fernkampf-Grafik (anders als
    /// <see cref="GetAbilityProjectileTexture"/>) - für Stellen, an denen
    /// ein generischer Pfeil fehl am Platz wäre, z.B. das Aufleuchten eines
    /// Feldes bei einer Selbst-/Flächenwirkung ohne Geschossflug (siehe
    /// EventAnimationPlayer.PlayFieldTargetedAbilityAsync: Musiker-Lied
    /// bekommt so korrekt sein Noten-Symbol, Schildträger/Wand ohne eigene
    /// Grafik bleiben bei der reinen Farbfläche statt fälschlich einen Pfeil
    /// zu zeigen).
    /// </summary>
    public Texture2D? GetSpecificAbilityProjectileTexture(string abilityId) =>
        AbilityProjectileTexturePaths.TryGetValue(abilityId, out var path) ? _textureCache.LoadOrNull(path) : null;

    public bool UsesCasterMovement(string abilityId) => AbilitiesUsingCasterMovement.Contains(abilityId);

    public string? GetFloatingSymbol(string abilityId) =>
        AbilityFloatingSymbols.TryGetValue(abilityId, out var symbol) ? symbol : null;
}
