using Godot;
using TacticsCore.Board;

namespace TacticsCoreGodot.Shared;

/// <summary>
/// Zentrale Zuordnung: welche Texturdatei gehört zu welchem Feldtyp bzw.
/// welcher Heldenkennung (siehe Ordner Art/ und Art/ART_README.md für die
/// erwartete Ordnerstruktur/Maße/Ausrichtung). Ersetzt keine Spiellogik -
/// reine Anzeige-Konfiguration.
///
/// Bewusst als Godot-<see cref="Resource"/> umgesetzt: im Editor als eigene
/// Ressource anlegbar/editierbar, ohne C#-Code zu ändern. Die Pfade sind
/// bewusst einfache STRINGS statt direkter Texture2D-Exports: würde eine
/// fehlende Bilddatei direkt als Texture2D-Feld referenziert, könnte das den
/// Ladevorgang der GESAMTEN Ressource scheitern lassen. Als String bleibt
/// eine (noch) nicht vorhandene Datei folgenlos - LoadTextureOrNull() prüft
/// jeden Pfad einzeln und liefert bei Fehlen schlicht null, worauf
/// HexBoardView automatisch auf Formen/Farben zurückfällt. So lassen sich
/// Grafiken nach und nach ergänzen, ohne dass die Anwendung je abstürzt
/// oder ein leeres Feld zeigt.
///
/// EIGENE GRAFIKEN OHNE CODEÄNDERUNG:
///   - Standardfall: Bilddateien einfach unter den unten vorgegebenen Pfaden
///     ablegen (siehe Art/ART_README.md) - diese Klasse muss dafür nicht
///     angefasst werden.
///   - Abweichende Pfade/Struktur: im Godot-Editor über "Neue Ressource..."
///     eine VisualAssetCatalog-Ressource anlegen, Felder im Inspector
///     befüllen und unter res://Art/VisualAssetCatalog.tres speichern -
///     <see cref="LoadCatalog"/> lädt diese Datei automatisch, falls
///     vorhanden, und benutzt sonst die Vorgabewerte hier im Code.
/// </summary>
[GlobalClass]
public partial class VisualAssetCatalog : Resource
{
    /// <summary>Optionale .tres-Übersteuerung mit abweichenden Pfaden - siehe Klassendokumentation.</summary>
    public const string OverrideResourcePath = "res://Art/VisualAssetCatalog.tres";

    [Export] public string FieldBlossomATexture { get; set; } = "res://Art/Fields/blossom_a.png";
    [Export] public string FieldBlossomBTexture { get; set; } = "res://Art/Fields/blossom_b.png";
    [Export] public string FieldConnectorTexture { get; set; } = "res://Art/Fields/connector.png";

    /// <summary>Zusätzliche Grafik über einem Startfeld (Vorgabe: "Startfelder ... über eigene Texturen oder eine Überlagerung"). Leer/fehlend -> Rückfallebene ist der bisherige gelbe Ring.</summary>
    [Export] public string StartFieldOverlayTexture { get; set; } = "res://Art/Fields/start_overlay.png";

    /// <summary>Zusätzliche Grafik über einem gesperrten Feld. Leer/fehlend -> Rückfallebene ist die bisherige rote Flächenüberlagerung.</summary>
    [Export] public string LockedFieldOverlayTexture { get; set; } = "res://Art/Fields/locked_overlay.png";

    /// <summary>Heldenkennung (HeroDefinition.Id, siehe TacticsCore.Heroes.DefaultHeroes) -> Texturpfad. Im Godot-Inspector als Dictionary editierbar.</summary>
    [Export]
    public Godot.Collections.Dictionary<string, string> HeroTexturePaths { get; set; } = new()
    {
        ["schildtraeger"] = "res://Art/Heroes/schildtraeger.png",
        ["elfe"] = "res://Art/Heroes/elfe.png",
        ["berserker"] = "res://Art/Heroes/berserker.png",
        ["wand"] = "res://Art/Heroes/wand.png",
        ["illusionist"] = "res://Art/Heroes/illusionist.png",
        ["musiker"] = "res://Art/Heroes/musiker.png",
        ["hugger"] = "res://Art/Heroes/hugger.png",
        ["elementarmagier"] = "res://Art/Heroes/elementarmagier.png",
        ["bogenschuetze"] = "res://Art/Heroes/bogenschuetze.png",
    };

    // Geladene Texturen je Pfad zwischenspeichern - _Draw() läuft potenziell
    // sehr häufig, wiederholtes ResourceLoader.Exists()/GD.Load() pro Bild
    // und Frame wäre unnötig teuer. Gemeinsame Implementierung mit
    // AnimationAssetCatalog, siehe TextureCache.
    private readonly TextureCache _textureCache = new();

    /// <summary>Lädt die .tres-Übersteuerung, falls vorhanden, sonst die im Code hinterlegten Vorgabewerte.</summary>
    public static VisualAssetCatalog LoadCatalog()
    {
        if (ResourceLoader.Exists(OverrideResourcePath))
        {
            var loaded = GD.Load<VisualAssetCatalog>(OverrideResourcePath);
            if (loaded is not null)
            {
                return loaded;
            }
        }

        return new VisualAssetCatalog();
    }

    /// <summary>
    /// Lädt die Textur unter <paramref name="path"/>, oder liefert null,
    /// wenn der Pfad leer ist, keine Datei dort existiert, oder das Laden
    /// aus einem anderen Grund fehlschlägt. DIES ist die zentrale
    /// Rückfallstelle: jeder Aufrufer bekommt bei null einfach nichts und
    /// muss selbst mit Formen/Farben weiterzeichnen - nie eine Ausnahme.
    /// </summary>
    public Texture2D? LoadTextureOrNull(string? path) => _textureCache.LoadOrNull(path);

    public Texture2D? GetFieldTexture(FieldType type) => LoadTextureOrNull(type switch
    {
        FieldType.BlossomA => FieldBlossomATexture,
        FieldType.BlossomB => FieldBlossomBTexture,
        FieldType.Connector => FieldConnectorTexture,
        _ => null,
    });

    public Texture2D? GetStartFieldOverlay() => LoadTextureOrNull(StartFieldOverlayTexture);

    public Texture2D? GetLockedFieldOverlay() => LoadTextureOrNull(LockedFieldOverlayTexture);

    public Texture2D? GetHeroTexture(string? heroId)
    {
        if (heroId is null || !HeroTexturePaths.TryGetValue(heroId, out var path))
        {
            return null;
        }

        return LoadTextureOrNull(path);
    }
}
