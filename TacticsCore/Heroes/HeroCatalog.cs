namespace TacticsCore.Heroes;

/// <summary>
/// Sammlung von <see cref="HeroDefinition"/>en, per Id nachschlagbar.
/// Bewusst eine ganz gewöhnliche, zur Laufzeit frei konstruierbare Klasse -
/// KEIN Singleton, keine statische Registrierung - damit ein Aufrufer
/// (Test, Konsolenprogramm, spätere Massen-Simulation) jederzeit einen
/// eigenen, ggf. veränderten Katalog übergeben kann (Anforderung 3:
/// "muss möglich sein, zur Laufzeit einen veränderten Heldenkatalog zu
/// übergeben, ohne den Code anzupassen").
/// </summary>
public sealed class HeroCatalog
{
    private readonly Dictionary<string, HeroDefinition> _byId;

    public HeroCatalog(IEnumerable<HeroDefinition> heroes)
    {
        _byId = new Dictionary<string, HeroDefinition>();
        foreach (var hero in heroes)
        {
            if (!_byId.TryAdd(hero.Id, hero))
            {
                throw new ArgumentException($"Doppelte Helden-Id im Katalog: '{hero.Id}'.", nameof(heroes));
            }
        }
    }

    public HeroDefinition Get(string id) =>
        _byId.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Kein Held mit Id '{id}' im Katalog.");

    public bool TryGet(string id, out HeroDefinition definition) => _byId.TryGetValue(id, out definition!);

    /// <summary>Alle Helden des Katalogs, stabil nach Id sortiert.</summary>
    public IReadOnlyList<HeroDefinition> All => _byId.Values.OrderBy(d => d.Id, StringComparer.Ordinal).ToList();
}
