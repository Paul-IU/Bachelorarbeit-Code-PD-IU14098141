namespace TacticsCore.Setup;

/// <summary>
/// Kleiner, generischer Hilfsbaustein: alle Permutationen einer Liste, in
/// EINER festen, reproduzierbaren Reihenfolge (lexikographisch über die
/// Positionen der Eingabeliste, nicht über deren Inhalt - die Eingabe wird
/// nicht sortiert, nur permutiert). Wird für die Zuordnung von je drei
/// Helden zu drei Startfeldern gebraucht (3! = 6 Permutationen je Team,
/// siehe <see cref="AusgangslagenGenerator"/>), bewusst generisch statt
/// hartkodiert für genau drei Elemente gehalten, falls sich die Teamgröße
/// später ändert.
/// </summary>
public static class Permutations
{
    /// <summary>
    /// Liefert alle n! Permutationen von <paramref name="items"/>, stets in
    /// derselben Reihenfolge bei gleicher Eingabe (rein indexbasierter,
    /// deterministischer Algorithmus - keine Nebenläufigkeit, kein Zufall).
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<T>> GenerateAll<T>(IReadOnlyList<T> items)
    {
        var result = new List<IReadOnlyList<T>>();
        var indices = Enumerable.Range(0, items.Count).ToArray();
        Permute(indices, 0, result, items);
        return result;
    }

    private static void Permute<T>(int[] indices, int start, List<IReadOnlyList<T>> result, IReadOnlyList<T> items)
    {
        if (start == indices.Length)
        {
            result.Add(indices.Select(i => items[i]).ToList());
            return;
        }

        for (int i = start; i < indices.Length; i++)
        {
            (indices[start], indices[i]) = (indices[i], indices[start]);
            Permute(indices, start + 1, result, items);
            (indices[start], indices[i]) = (indices[i], indices[start]); // zurücktauschen, damit die nächste äußere Iteration von der ursprünglichen Reihenfolge ausgeht
        }
    }
}
