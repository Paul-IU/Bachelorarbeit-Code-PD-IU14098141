using System.Text;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

namespace TacticsCore.Batch;

/// <summary>
/// Ergebnis der optionalen Etiketten-Prüfung eines Laufs (siehe
/// <see cref="BatchSimulationRunner.Run"/>, Option <c>verifyNameIndependence</c>).
/// </summary>
/// <param name="CheckedMatches">Anzahl geprüfter Partien (jede einmal original, einmal mit vertauschten Etiketten gespielt).</param>
/// <param name="Violations">Jede Abweichung einzeln, in fester Reihenfolge (Paarung, Ausgangslage) - unabhängig von der Threadanzahl.</param>
public sealed record NameIndependenceReport(int CheckedMatches, IReadOnlyList<NameIndependenceViolation> Violations);

/// <summary>
/// Eine einzelne Partie, deren Ergebnis vom Team-Etikett abhing.
/// </summary>
/// <param name="OutcomeDifference">Welche der verglichenen Kenngrößen abwich (Sieger, Endgrund, Rundenzahl, Rest-Lebenspunkte).</param>
/// <param name="FirstDivergence">Der erste Eintrag im Ereignisprotokoll, der nicht spiegelbildlich ist; null, falls beide Protokolle spiegelbildlich sind und nur das Endergebnis abweicht.</param>
public sealed record NameIndependenceViolation(
    TeamComposition TeamA,
    TeamComposition TeamB,
    int PairingIndex,
    int SetupIndex,
    GameSetup Setup,
    int Seed,
    string OutcomeDifference,
    ReplayDivergence? FirstDivergence)
{
    /// <summary>Mehrzeilige, deutschsprachige Beschreibung für Konsole/Bericht.</summary>
    public string Describe()
    {
        var names = NameIndependenceCheck.BuildNameLookup(Setup);
        var sb = new StringBuilder();
        sb.AppendLine($"Paarung {PairingIndex}: {TeamA.Key} gegen {TeamB.Key}, Ausgangslage {SetupIndex}, Seed {Seed}");
        sb.AppendLine($"  Aufstellung Team A: {string.Join(", ", Setup.TeamAUnits.Select(u => $"{u.Hero.DisplayName} {u.Position}"))}");
        sb.AppendLine($"  Aufstellung Team B: {string.Join(", ", Setup.TeamBUnits.Select(u => $"{u.Hero.DisplayName} {u.Position}"))}");
        sb.AppendLine($"  Beginnt: {Setup.StartingTeam}");
        sb.AppendLine($"  Abweichung im Ergebnis: {OutcomeDifference}");
        if (FirstDivergence is { } d)
        {
            sb.AppendLine($"  Erster abweichender Eintrag (Nr. {d.EntryIndex} im Protokoll):");
            sb.AppendLine($"    Original:                 {NameIndependenceCheck.Format(d.Original, names, swapped: false)}");
            sb.AppendLine($"    Mit vertauschten Etiketten: {NameIndependenceCheck.Format(d.Swapped, names, swapped: true)}");
        }
        else
        {
            sb.AppendLine("  Ereignisprotokolle sind spiegelbildlich - nur das Endergebnis weicht ab.");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Erster nicht spiegelbildlicher Eintrag zweier Ereignisprotokolle. Einer
/// der beiden Einträge ist null, wenn ein Protokoll an dieser Stelle bereits
/// zu Ende ist.
/// </summary>
public sealed record ReplayDivergence(int EntryIndex, ReplayEntry? Original, ReplayEntry? Swapped);

/// <summary>
/// ETIKETTEN-UNABHÄNGIGKEIT (Bachelorarbeit-Verifikation, optional): eine
/// Partie wird ein zweites Mal gespielt, physisch identisch (dieselben
/// Einheiten auf denselben Feldern, derselbe Seed, dieselbe beginnende Seite),
/// nur mit vertauschten Team-Etiketten und Ids (<see cref="GameSetup.WithSwappedTeams"/>).
/// Hängt nichts im Spielablauf vom Etikett ab, muss das Ergebnis exakt
/// spiegelbildlich sein. Geprüft werden Sieger, Endgrund, Rundenzahl und die
/// verbleibenden Lebenspunkte je Seite; bei einer Abweichung wird die Partie
/// mit Protokoll wiederholt und der erste nicht spiegelbildliche Eintrag
/// ermittelt (nur dann - der Normalfall kostet daher nur die zweite Partie).
///
/// VORAUSSETZUNGEN: (1) Unit-Ids nach dem Schema von
/// <see cref="AusgangslagenGenerator.BuildGameState"/> (Team A 1-3, Team B
/// 101-103; Klon-Ids davon abgeleitet, siehe <see cref="Unit.CreateClone"/>).
/// (2) Beide Seiten nutzen dieselbe Agentenart - eine Konfiguration mit
/// verschiedenen Agenten je Seite ist per Definition etikettenabhängig (der
/// Agent hängt am Etikett) und würde jede Partie als Abweichung melden.
/// </summary>
public static class NameIndependenceCheck
{
    /// <summary>
    /// Vergleicht die Endergebnisse; liefert null, wenn sie spiegelbildlich
    /// sind, sonst eine Beschreibung aller abweichenden Kenngrößen.
    /// </summary>
    public static string? CompareOutcomes(MatchResult original, MatchResult swapped)
    {
        var diffs = new List<string>();
        if (Mirror(original.Winner) != swapped.Winner)
        {
            diffs.Add($"Sieger {original.Winner} -> gespiegelt erwartet {Mirror(original.Winner)}, tatsächlich {swapped.Winner}");
        }

        if (original.EndReason != swapped.EndReason)
        {
            diffs.Add($"Endgrund {original.EndReason} vs {swapped.EndReason}");
        }

        if (original.RoundsPlayed != swapped.RoundsPlayed)
        {
            diffs.Add($"Rundenzahl {original.RoundsPlayed} vs {swapped.RoundsPlayed}");
        }

        if (original.TeamAResult.TotalCurrentHitPoints != swapped.TeamBResult.TotalCurrentHitPoints ||
            original.TeamBResult.TotalCurrentHitPoints != swapped.TeamAResult.TotalCurrentHitPoints)
        {
            diffs.Add($"Rest-Lebenspunkte (A/B) {original.TeamAResult.TotalCurrentHitPoints}/{original.TeamBResult.TotalCurrentHitPoints} " +
                      $"vs gespiegelt {swapped.TeamBResult.TotalCurrentHitPoints}/{swapped.TeamAResult.TotalCurrentHitPoints}");
        }

        return diffs.Count == 0 ? null : string.Join("; ", diffs);
    }

    /// <summary>
    /// Sucht den ersten Eintrag, der nach Umrechnung der Ids und Etiketten
    /// nicht mit dem Protokoll der vertauschten Partie übereinstimmt; null,
    /// wenn beide Protokolle vollständig spiegelbildlich sind.
    /// </summary>
    public static ReplayDivergence? FindFirstDivergence(IReadOnlyList<ReplayEntry> original, IReadOnlyList<ReplayEntry> swapped)
    {
        int common = Math.Min(original.Count, swapped.Count);
        for (int i = 0; i < common; i++)
        {
            if (!IsMirrorOf(original[i], swapped[i]))
            {
                return new ReplayDivergence(i, original[i], swapped[i]);
            }
        }

        return original.Count == swapped.Count
            ? null
            : new ReplayDivergence(common, original.ElementAtOrDefault(common), swapped.ElementAtOrDefault(common));
    }

    private static bool IsMirrorOf(ReplayEntry original, ReplayEntry swapped)
    {
        var mapped = original with
        {
            ActingUnitId = SwapId(original.ActingUnitId),
            AttackTargetUnitId = original.AttackTargetUnitId is Guid t ? SwapId(t) : null,
            RelatedUnitId = original.RelatedUnitId is Guid r ? SwapId(r) : null,
            RoundOrder = original.RoundOrder is { } order ? new UnitOrder(order.UnitIds.Select(SwapId).ToList()) : null,
            Winner = original.Winner is MatchWinner w ? Mirror(w) : null,
        };

        // Manche Einträge tragen als ActingTeam nur den bedeutungslosen
        // Standardwert (siehe ReplayEntry.ForEffectApplied) - dort darf das
        // Etikett NICHT mitgetauscht werden, bei allen anderen schon.
        return mapped with { ActingTeam = Mirror(original.ActingTeam) } == swapped
            || mapped == swapped;
    }

    private static MatchWinner Mirror(MatchWinner winner) => winner switch
    {
        MatchWinner.TeamA => MatchWinner.TeamB,
        MatchWinner.TeamB => MatchWinner.TeamA,
        _ => winner,
    };

    private static Team Mirror(Team team) => team == Team.TeamA ? Team.TeamB : Team.TeamA;

    /// <summary>
    /// Übersetzt eine Id der Originalpartie in die Id DERSELBEN physischen
    /// Einheit in der vertauschten Partie: 1-3 &lt;-&gt; 101-103, Klon-Ids
    /// entsprechend ihres Erzeugers. Guid.Empty und unbekannte Ids bleiben
    /// unverändert.
    /// </summary>
    internal static Guid SwapId(Guid id)
    {
        if (TrySwapBaseId(id, out var swapped))
        {
            return swapped;
        }

        var creator = XorCloneSalt(id);
        return TrySwapBaseId(creator, out var swappedCreator) ? XorCloneSalt(swappedCreator) : id;
    }

    private static bool TrySwapBaseId(Guid id, out Guid swapped)
    {
        var bytes = id.ToByteArray();
        int n = BitConverter.ToInt32(bytes, 0);
        bool isBaseId = bytes.Skip(4).All(b => b == 0) && n is (>= 1 and <= 99) or (>= 101 and <= 199);
        swapped = isBaseId ? new Guid(n <= 99 ? n + 100 : n - 100, 0, 0, new byte[8]) : id;
        return isBaseId;
    }

    private static Guid XorCloneSalt(Guid id) => new(id.ToByteArray().Select(b => (byte)(b ^ 0xA5)).ToArray());

    /// <summary>Anzeigenamen aller Einheiten der ORIGINALpartie (Held + Team), inklusive möglicher Klone.</summary>
    internal static IReadOnlyDictionary<Guid, string> BuildNameLookup(GameSetup setup)
    {
        // Ids in derselben Reihenfolge wie AusgangslagenGenerator.BuildGameState (aus Sicht der eigenen Blüte)
        var (teamA, teamB) = AusgangslagenGenerator.IdReihenfolge(setup, new TacticsCore.Board.Board());
        var names = new Dictionary<Guid, string>();
        void Add(IReadOnlyList<(Heroes.HeroDefinition Hero, Hex.HexCoordinate Position)> units, int firstId, string team)
        {
            int id = firstId;
            foreach (var (hero, _) in units)
            {
                var guid = new Guid(id++, 0, 0, new byte[8]);
                names[guid] = $"{hero.DisplayName} ({team})";
                names[XorCloneSalt(guid)] = $"Klon von {hero.DisplayName} ({team})";
            }
        }

        Add(teamA, 1, "A");
        Add(teamB, 101, "B");
        return names;
    }

    /// <summary>
    /// Lesbare Kurzform eines Eintrags. Bei <paramref name="swapped"/> werden
    /// die Ids zuerst auf die physische Einheit der Originalpartie
    /// zurückgerechnet, damit beide Zeilen dieselben Namen tragen.
    /// </summary>
    internal static string Format(ReplayEntry? entry, IReadOnlyDictionary<Guid, string> originalNames, bool swapped)
    {
        if (entry is null)
        {
            return "(Protokoll bereits zu Ende)";
        }

        string Name(Guid? id)
        {
            if (id is not Guid g || g == Guid.Empty)
            {
                return "-";
            }

            var original = swapped ? SwapId(g) : g;
            return originalNames.TryGetValue(original, out var name) ? name : g.ToString();
        }

        var parts = new List<string> { $"Runde {entry.RoundNumber}", entry.ActionType.ToString() };
        if (entry.ActingUnitId != Guid.Empty) parts.Add($"von {Name(entry.ActingUnitId)}");
        if (entry.AttackTargetUnitId is not null) parts.Add($"Ziel {Name(entry.AttackTargetUnitId)}");
        if (entry.RelatedUnitId is not null) parts.Add($"betrifft {Name(entry.RelatedUnitId)}");
        if (entry.MoveSource is not null || entry.MoveTarget is not null) parts.Add($"{entry.MoveSource} -> {entry.MoveTarget}");
        if (entry.DamageDealt is int dmg) parts.Add($"Schaden {dmg}");
        if (entry.AbilityId is not null) parts.Add($"Fähigkeit {entry.AbilityId}{(entry.ModeId is null ? "" : "/" + entry.ModeId)}");
        if (entry.EffectName is not null) parts.Add($"Effekt {entry.EffectName}");
        if (entry.EffectField is not null) parts.Add($"Feld {entry.EffectField}");
        if (entry.RoundOrder is { } order) parts.Add($"Reihenfolge {string.Join(", ", order.UnitIds.Select(u => Name(u)))}");
        if (entry.Winner is not null) parts.Add($"Sieger {entry.Winner} ({entry.EndReason})");
        return string.Join(" | ", parts);
    }
}
