using TacticsCore.Heroes;

namespace TacticsCore.Setup;

/// <summary>
/// Eine Team-Komposition: genau drei VERSCHIEDENE Helden aus einem Katalog
/// (Anforderung 1) - eine reine Auswahl, noch OHNE Zuordnung zu konkreten
/// Startfeldern (das leistet erst <see cref="GameSetup"/> bzw.
/// <see cref="AusgangslagenGenerator"/>). Zwei Kompositionen mit denselben
/// drei Helden in unterschiedlicher Reihenfolge gelten als GLEICH - dafür
/// wird intern kanonisch nach Helden-Id sortiert gespeichert, damit
/// Gleichheit/Duplikaterkennung (siehe <see cref="CompositionGenerator"/>)
/// nicht von der zufälligen Aufzählungsreihenfolge einer Menge abhängt.
/// </summary>
public sealed class TeamComposition : IEquatable<TeamComposition>
{
    /// <summary>Die drei Helden dieser Komposition, stabil nach Id sortiert.</summary>
    public IReadOnlyList<HeroDefinition> Heroes { get; }

    public TeamComposition(IReadOnlyList<HeroDefinition> heroes)
    {
        if (heroes.Count != 3)
        {
            throw new ArgumentException($"Eine Team-Komposition besteht aus genau drei Helden, nicht {heroes.Count}.", nameof(heroes));
        }

        if (heroes.Select(h => h.Id).Distinct().Count() != 3)
        {
            throw new ArgumentException("Derselbe Held darf nicht zweimal in derselben Komposition vorkommen.", nameof(heroes));
        }

        Heroes = heroes.OrderBy(h => h.Id, StringComparer.Ordinal).ToList();
    }

    /// <summary>Kurzbezeichnung für Ausgabe/CSV, z.B. "berserker+elfe+schildtraeger".</summary>
    public string Key => string.Join("+", Heroes.Select(h => h.Id));

    public bool Equals(TeamComposition? other) => other is not null && Key == other.Key;
    public override bool Equals(object? obj) => Equals(obj as TeamComposition);
    public override int GetHashCode() => Key.GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Key;
}
