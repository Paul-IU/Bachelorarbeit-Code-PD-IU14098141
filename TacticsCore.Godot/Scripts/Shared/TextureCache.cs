using Godot;

namespace TacticsCoreGodot.Shared;

/// <summary>
/// Gemeinsame Lade-/Zwischenspeicherlogik für optionale Texturen, genutzt
/// sowohl von <see cref="VisualAssetCatalog"/> (Feld-/Heldengrafiken) als
/// auch von der Animationsschicht (Geschossgrafiken) - EINE zentrale
/// Stelle für "fehlende Datei -&gt; null statt Absturz", statt sie mehrfach
/// zu duplizieren. Siehe VisualAssetCatalog für die ausführliche Begründung
/// dieses Verhaltens.
/// </summary>
public sealed class TextureCache
{
    private readonly Dictionary<string, Texture2D?> _cache = new();

    public Texture2D? LoadOrNull(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (_cache.TryGetValue(path, out var cached))
        {
            return cached;
        }

        Texture2D? texture = null;
        try
        {
            if (ResourceLoader.Exists(path))
            {
                texture = GD.Load<Texture2D>(path);
            }
        }
        catch (Exception)
        {
            texture = null;
        }

        _cache[path] = texture;
        return texture;
    }
}
