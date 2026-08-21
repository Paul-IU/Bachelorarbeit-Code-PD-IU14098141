using TacticsCore.Heroes;

namespace TacticsCore.Setup;

/// <summary>
/// Erzeugt alle möglichen Team-Kompositionen aus einem Heldenkatalog
/// (Anforderung 2): jede 3-elementige Teilmenge der Katalog-Helden genau
/// einmal - bei neun Helden also C(9,3) = 84 Kompositionen. Reine
/// Kombinatorik OHNE Berücksichtigung der Reihenfolge (die Zuordnung zu
/// Startfeldern kommt erst bei <see cref="AusgangslagenGenerator"/> dazu).
/// </summary>
public static class CompositionGenerator
{
    /// <summary>
    /// Liefert alle 3er-Teilmengen der Helden aus <paramref name="catalog"/>,
    /// stabil sortiert: zunächst nach dem Schlüssel der Komposition
    /// (<see cref="TeamComposition.Key"/>, also alphabetisch nach den
    /// enthaltenen Helden-Ids). <see cref="HeroCatalog.All"/> liefert die
    /// Helden bereits stabil nach Id sortiert - die Kombinationsbildung
    /// selbst (klassischer "Kombinationen ohne Wiederholung"-Algorithmus
    /// über Indizes) ist rein deterministisch, ohne jeden Zufall.
    /// </summary>
    public static IReadOnlyList<TeamComposition> GenerateAllCompositions(HeroCatalog catalog)
    {
        var heroes = catalog.All;
        var compositions = new List<TeamComposition>();

        for (int i = 0; i < heroes.Count; i++)
        {
            for (int j = i + 1; j < heroes.Count; j++)
            {
                for (int k = j + 1; k < heroes.Count; k++)
                {
                    compositions.Add(new TeamComposition(new[] { heroes[i], heroes[j], heroes[k] }));
                }
            }
        }

        return compositions
            .OrderBy(c => c.Key, StringComparer.Ordinal)
            .ToList();
    }
}
