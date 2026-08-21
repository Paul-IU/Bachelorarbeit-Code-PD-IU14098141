using System.Globalization;
using System.Text;

namespace TacticsCore.Batch;

/// <summary>
/// Exportiert Simulationsergebnisse als CSV (Anforderung 9) für die
/// spätere statistische Auswertung außerhalb von TacticsCore (z.B. in R,
/// Python/pandas oder einer Tabellenkalkulation). Zahlen werden bewusst mit
/// <see cref="CultureInfo.InvariantCulture"/> formatiert (Punkt statt
/// Komma als Dezimaltrennzeichen) - unabhängig von der Systemsprache des
/// Rechners, auf dem die Simulation läuft, damit die Datei überall
/// gleichermaßen einlesbar ist.
/// </summary>
public static class CsvExporter
{
    private const char Separator = ';'; // ';' statt ',' - unproblematisch, falls Dezimalwerte in einer Locale mit Komma-Trennzeichen betrachtet werden, und in Excel (DE) der übliche CSV-Trenner

    /// <summary>Eine Zeile je Paarung, mit allen in <see cref="PairingResult"/> gesammelten Kennzahlen.</summary>
    public static void ExportPairingResults(IEnumerable<PairingResult> results, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Separator, "TeamA", "TeamB", "Partien", "SiegeTeamA", "SiegeTeamB", "Unentschieden",
            "SiegquoteTeamA", "SiegquoteTeamB", "DurchschnittRunden", "DurchschnittRestHpTeamA", "DurchschnittRestHpTeamB", "AnteilRundenlimit"));

        foreach (var r in results)
        {
            sb.AppendLine(string.Join(Separator,
                Csv(r.TeamA.Key), Csv(r.TeamB.Key), r.TotalMatches, r.TeamAWins, r.TeamBWins, r.Draws,
                Num(r.TeamAWinRate), Num(r.TeamBWinRate), Num(r.AverageRoundsPlayed),
                Num(r.AverageTeamARemainingHitPointsFraction), Num(r.AverageTeamBRemainingHitPointsFraction), Num(r.RoundLimitShare)));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Eine Zeile JE KOMPOSITION mit der über ALLE ihre Paarungen
    /// aggregierten Siegquote (Anforderung 9: "separater Export je
    /// Komposition mit der aggregierten Siegquote") - eine Komposition kann
    /// in verschiedenen Paarungen sowohl als Team A als auch als Team B
    /// aufgetreten sein, beides fließt hier zusammen ein.
    /// </summary>
    public static void ExportCompositionWinRates(IEnumerable<PairingResult> results, string filePath)
    {
        var byComposition = new Dictionary<string, (Setup.TeamComposition Composition, int Wins, int Matches)>();

        foreach (var r in results)
        {
            Accumulate(r.TeamA, r.TeamAWins, r.TotalMatches);
            Accumulate(r.TeamB, r.TeamBWins, r.TotalMatches);

            void Accumulate(Setup.TeamComposition composition, int wins, int matches)
            {
                (Setup.TeamComposition Composition, int Wins, int Matches) current = byComposition.TryGetValue(composition.Key, out var existing)
                    ? existing
                    : (composition, 0, 0);
                byComposition[composition.Key] = (composition, current.Wins + wins, current.Matches + matches);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Separator, "Komposition", "Partien", "Siege", "Siegquote"));

        // Stabil nach dem Komposition-Schlüssel sortiert statt in
        // Dictionary-Durchlaufreihenfolge - Letztere ist für die konkrete
        // .NET-Implementierung zwar in der Praxis meist insertion-order,
        // aber nicht offiziell garantiert.
        foreach (var entry in byComposition.Values.OrderBy(v => v.Composition.Key, StringComparer.Ordinal))
        {
            double winRate = entry.Matches == 0 ? 0.0 : (double)entry.Wins / entry.Matches;
            sb.AppendLine(string.Join(Separator, Csv(entry.Composition.Key), entry.Matches, entry.Wins, Num(winRate)));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    private static string Num(double value) => value.ToString("F4", CultureInfo.InvariantCulture);

    /// <summary>Minimales CSV-Escaping (Anführungszeichen verdoppeln + umschließen, falls der Wert den Trenner oder Anführungszeichen enthält) - die hier genutzten Werte (Helden-Ids) enthalten davon zwar nichts, robust ist es trotzdem.</summary>
    private static string Csv(string value) =>
        value.Contains(Separator) || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
