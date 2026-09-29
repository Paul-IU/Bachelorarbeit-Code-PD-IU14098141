using System.Globalization;
using System.Text;

namespace TacticsCore.Match;

/// <summary>
/// Kurzer, reproduzierbarer Fingerabdruck eines vollständigen Partieverlaufs
/// (alle Einträge des Replay-Protokolls). Zwei Partien mit identischem Verlauf
/// haben denselben Fingerabdruck - unabhängig davon, ob sie in der Godot-
/// Oberfläche, im Konsolenprogramm oder im Batch-Lauf gespielt wurden.
///
/// Bewusst NICHT über <c>string.GetHashCode()</c> gebildet: .NET randomisiert
/// dessen Ergebnis je Prozess, der Wert wäre zwischen zwei Programmläufen nicht
/// vergleichbar. Stattdessen FNV-1a (64 Bit) über eine feste Textdarstellung
/// jedes Eintrags.
/// </summary>
public static class ReplayFingerprint
{
    public static string Compute(IReadOnlyList<ReplayEntry> entries)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        ulong hash = offset;
        foreach (var entry in entries)
        {
            foreach (byte b in Encoding.UTF8.GetBytes(Canonical(entry)))
            {
                hash ^= b;
                hash *= prime;
            }
        }

        return hash.ToString("x16", CultureInfo.InvariantCulture);
    }

    private static string Canonical(ReplayEntry e)
    {
        var sb = new StringBuilder();
        sb.Append(e.RoundNumber).Append('|').Append(e.ActingUnitId).Append('|').Append(e.ActingTeam).Append('|').Append(e.ActionType)
          .Append('|').Append(e.MoveSource).Append('|').Append(e.MoveTarget).Append('|').Append(e.AttackTargetUnitId)
          .Append('|').Append(e.DamageDealt).Append('|').Append(e.AbilityId).Append('|').Append(e.ModeId)
          .Append('|').Append(e.EffectName).Append('|').Append(e.RelatedUnitId).Append('|').Append(e.EffectField)
          .Append('|').Append(e.Winner).Append('|').Append(e.EndReason).Append('|');
        if (e.MovePath is { } path)
        {
            sb.Append(string.Join(";", path.Fields));
        }

        sb.Append('|');
        if (e.RoundOrder is { } order)
        {
            sb.Append(string.Join(";", order.UnitIds));
        }

        return sb.Append('\n').ToString();
    }
}
