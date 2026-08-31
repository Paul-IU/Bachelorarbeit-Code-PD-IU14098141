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

        ValidateDistinctSpeedValues();
    }

    /// <summary>
    /// Erzwingt, dass alle Helden dieses Katalogs PAARWEISE VERSCHIEDENE
    /// Geschwindigkeitswerte tragen (Bachelorarbeit-Anforderung). Die
    /// Geschwindigkeit bestimmt die Zugreihenfolge innerhalb eines Teams
    /// (siehe <see cref="Match.TurnOrder"/>) - bei Gleichstand entscheidet
    /// dort ersatzweise die UnitId, wodurch die Reihenfolge von der (freien)
    /// Aufstellungsreihenfolge abhinge statt allein von den Heldenwerten.
    /// Läuft bei JEDER Katalogerzeugung, nicht nur beim Standardkatalog -
    /// damit fällt ein Gleichstand auch in einer per <c>with</c> oder
    /// <see cref="HeroCatalogVariants"/> abgewandelten Balancing-Variante
    /// sofort beim Erzeugen auf, statt erst später als schwer nachvollzieh-
    /// bare Verzerrung der Zugreihenfolge in einer Simulation aufzufallen.
    /// </summary>
    private void ValidateDistinctSpeedValues()
    {
        var duplicateGroup = _byId.Values
            .GroupBy(h => h.Speed)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateGroup is not null)
        {
            var names = string.Join(", ", duplicateGroup.Select(h => $"{h.DisplayName} ({h.Id})"));
            throw new ArgumentException(
                $"Ungültiger Heldenkatalog: mehrere Helden teilen sich die Geschwindigkeit {duplicateGroup.Key} - {names}. " +
                "Geschwindigkeitswerte müssen im gesamten Katalog paarweise verschieden sein, da sie die Zugreihenfolge bestimmen.");
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
