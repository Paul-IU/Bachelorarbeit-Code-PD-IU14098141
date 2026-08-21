using System.Text.Json;
using System.Text.Json.Serialization;
using TacticsCore.Hex;

namespace TacticsCore.Persistence;

/// <summary>
/// Expliziter JSON-Konverter für <see cref="HexCoordinate"/>: System.Text.Json
/// kann den vorhandenen Konstruktor <c>HexCoordinate(int q, int r)</c> NICHT
/// automatisch verwenden, weil die Parameternamen ("q", "r") sich in der
/// Groß-/Kleinschreibung von den serialisierten Property-Namen ("Q", "R")
/// unterscheiden - die konstruktorbasierte Zuordnung ist (anders als der
/// reine Eigenschaftsabgleich) NICHT von
/// <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/> betroffen.
/// Ohne diesen Konverter würden beim Laden alle Koordinaten stillschweigend
/// zu (0,0) - ein Test hat das aufgedeckt (siehe SavedMatchIOTests). Statt
/// mich auf dieses Detailverhalten zu verlassen, wird hier explizit und
/// unmissverständlich als kompaktes Feld-Array "[q, r]" (de-)serialisiert.
/// </summary>
internal sealed class HexCoordinateJsonConverter : JsonConverter<HexCoordinate>
{
    public override HexCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Beim Eintritt steht der Reader auf dem StartArray-Token ("[").
        // Ein Read() je Schritt bewegt ihn zum nächsten Token weiter - nach
        // dem letzten Read() steht er auf EndArray ("]"), wie von
        // System.Text.Json für das Ende eines Read()-Aufrufs erwartet.
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Erwartetes Format für HexCoordinate: [q, r].");
        }

        reader.Read();
        int q = reader.GetInt32();
        reader.Read();
        int r = reader.GetInt32();
        reader.Read();
        return new HexCoordinate(q, r);
    }

    public override void Write(Utf8JsonWriter writer, HexCoordinate value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Q);
        writer.WriteNumberValue(value.R);
        writer.WriteEndArray();
    }
}

/// <summary>
/// (De-)Serialisiert einen <see cref="SavedMatch"/> als lesbares JSON.
/// Nutzt ausschließlich <c>System.Text.Json</c> (Teil von .NET, keine neue
/// Abhängigkeit) - alle beteiligten Typen (Records, Enums, der einfache
/// <see cref="Hex.HexCoordinate"/>-Werttyp) sind bewusst simple, unveränd-
/// liche Datenträger ohne zyklische Referenzen, daher genügt die
/// Standard-Reflektion ohne eigene Konverter, bis auf Enums als Text statt
/// Zahl (robuster gegen künftige Umsortierung der Enum-Werte).
/// </summary>
public static class SavedMatchIO
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(), new HexCoordinateJsonConverter() },
    };

    public static void Save(SavedMatch match, string filePath) =>
        File.WriteAllText(filePath, JsonSerializer.Serialize(match, Options));

    public static SavedMatch Load(string filePath)
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<SavedMatch>(json, Options)
            ?? throw new InvalidDataException($"Datei '{filePath}' enthält keine gültige gespeicherte Partie.");
    }
}
