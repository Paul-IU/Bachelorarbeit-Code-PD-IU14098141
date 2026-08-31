namespace TacticsCore.Match;

/// <summary>
/// Werttyp-Wrapper um eine Zugreihenfolge (Liste von Unit-Ids, siehe
/// <see cref="ReplayEntry.RoundOrder"/>) MIT struktureller Gleichheit - aus
/// demselben Grund wie <see cref="HexPath"/> bei <see cref="ReplayEntry.MovePath"/>
/// (siehe dortige Begründung): Da <see cref="ReplayEntry"/> ein
/// <c>record</c> ist, würde dessen automatisch erzeugte Gleichheitsprüfung
/// eine rohe <see cref="IReadOnlyList{T}"/> (z.B. eine gewöhnliche
/// <c>List&lt;Guid&gt;</c>) nur per Objektreferenz vergleichen, nicht nach
/// Inhalt. Zwei inhaltlich identische, aber in zwei unabhängigen
/// Partie-Durchläufen separat berechnete Reihenfolgen wären dadurch zwei
/// verschiedene Listenobjekte und würden fälschlich als "ungleich" gelten -
/// die Determinismus-Prüfung ("gleicher Seed -&gt; exakt gleiches
/// Replay-Protokoll") wäre damit unbrauchbar. Dieser kleine, eigens
/// gleichheits-überschriebene Wrapper löst genau dieses eine Problem.
/// </summary>
public sealed class UnitOrder : IEquatable<UnitOrder>
{
    public IReadOnlyList<Guid> UnitIds { get; }

    public UnitOrder(IReadOnlyList<Guid> unitIds) => UnitIds = unitIds;

    public bool Equals(UnitOrder? other) => other is not null && UnitIds.SequenceEqual(other.UnitIds);
    public override bool Equals(object? obj) => Equals(obj as UnitOrder);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var id in UnitIds)
        {
            hash.Add(id);
        }

        return hash.ToHashCode();
    }
}
