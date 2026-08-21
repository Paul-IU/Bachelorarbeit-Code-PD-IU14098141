using TacticsCore.Hex;

namespace TacticsCore.Match;

/// <summary>
/// Werttyp-Wrapper um eine Feldfolge (den vollständigen Bewegungspfad in
/// <see cref="ReplayEntry.MovePath"/>) MIT struktureller Gleichheit.
///
/// Notwendig, weil <see cref="ReplayEntry"/> ein <c>record</c> ist: dessen
/// automatisch erzeugte Gleichheitsprüfung vergleicht Referenztyp-Felder wie
/// eine rohe <see cref="IReadOnlyList{T}"/> (z.B. eine gewöhnliche
/// <c>List&lt;HexCoordinate&gt;</c>) NUR per Objektreferenz, nicht nach
/// Inhalt. Zwei inhaltlich identische, aber in zwei unabhängigen
/// Partie-Durchläufen separat berechnete Pfade wären dadurch zwei
/// verschiedene Listenobjekte und würden fälschlich als "ungleich" gelten -
/// die Determinismus-Prüfung ("gleicher Seed -&gt; exakt gleiches
/// Replay-Protokoll") wäre damit unbrauchbar. Dieser kleine, eigens
/// gleichheits-überschriebene Wrapper löst genau dieses eine Problem, ohne
/// dass <see cref="ReplayEntry"/> selbst seine gesamte automatisch erzeugte
/// Gleichheitsprüfung von Hand nachbauen müsste.
/// </summary>
public sealed class HexPath : IEquatable<HexPath>
{
    public IReadOnlyList<HexCoordinate> Fields { get; }

    public HexPath(IReadOnlyList<HexCoordinate> fields) => Fields = fields;

    public bool Equals(HexPath? other) => other is not null && Fields.SequenceEqual(other.Fields);
    public override bool Equals(object? obj) => Equals(obj as HexPath);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var field in Fields)
        {
            hash.Add(field);
        }

        return hash.ToHashCode();
    }
}
