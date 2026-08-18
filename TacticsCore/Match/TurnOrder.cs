using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Berechnet die verschränkte Zugreihenfolge einer Runde aus den aktuell
/// lebenden Einheiten beider Teams. Reine, zustandslose Funktion ohne
/// Bezug zu GameState oder MatchController - leicht isoliert testbar.
///
/// ZUGREIHENFOLGE (Anforderung 2): Innerhalb jedes Teams werden die
/// lebenden Einheiten nach Speed absteigend in Ränge einsortiert (Rang 1 =
/// schnellste Einheit). Gehandelt wird dann verschränkt über beide Teams
/// hinweg, beginnend mit dem per Zufall bestimmten Startteam:
///
///   Start-Team Rang 1, anderes Team Rang 1,
///   Start-Team Rang 2, anderes Team Rang 2,
///   Start-Team Rang 3, anderes Team Rang 3, ...
///
/// Hat ein Team auf einem Rang keine lebende Einheit mehr (ungleiche
/// Heldenzahl), wird dieser Slot ausgelassen statt übersprungen zu werden
/// im Sinne von "verschoben" - das zahlenmäßig unterlegene Team bekommt
/// dadurch schlicht weniger Aktionen in dieser Runde, das zahlenmäßig
/// überlegene Team entsprechend mehr. Das ist laut Aufgabenstellung
/// beabsichtigt (kein Ausgleichsmechanismus).
///
/// TIE-BREAK BEI GLEICHEM SPEED: Es entscheidet die UnitId (aufsteigend) -
/// das ist an dieser Stelle KEINE kosmetische Nebenentscheidung: Solange
/// (wie aktuell) alle Einheiten denselben Speed-Wert tragen, bestimmt
/// dieser Tie-Break die GESAMTE Rangfolge innerhalb eines Teams. Für die
/// Determinismus-Anforderung ("gleicher Seed + gleiche Ausgangslage =
/// exakt gleicher Partieverlauf") bedeutet das: die UnitIds sind Teil der
/// "Ausgangslage", nicht nur ein technisches Implementierungsdetail. Ein
/// Reproduzierbarkeits-Test muss daher dieselben Unit-Objekte (bzw.
/// dieselben, fest vergebenen UnitIds) verwenden, nicht bloß denselben
/// Seed bei strukturell gleicher, aber frisch ausgewürfelter ID-Vergabe
/// (siehe z.B. <c>Guid.NewGuid()</c> in einer Demo/einem Testaufbau).
/// </summary>
public static class TurnOrder
{
    /// <summary>
    /// Berechnet die Zugreihenfolge für eine einzelne Runde. Besiegte
    /// Einheiten in <paramref name="units"/> werden defensiv herausgefiltert
    /// (die Methode verlässt sich also nicht darauf, dass der Aufrufer
    /// bereits nur lebende Einheiten übergibt) - siehe Anforderung 2: "Die
    /// Rangfolge wird zu Beginn JEDER Runde neu aus den lebenden Helden
    /// bestimmt."
    /// </summary>
    public static IReadOnlyList<Unit> ComputeRoundOrder(IEnumerable<Unit> units, Team startingTeam)
    {
        var living = units.Where(u => !u.IsDefeated).ToList();

        var teamARanked = RankBySpeedDescending(living.Where(u => u.Team == Team.TeamA));
        var teamBRanked = RankBySpeedDescending(living.Where(u => u.Team == Team.TeamB));

        var (firstTeamRanked, secondTeamRanked) = startingTeam == Team.TeamA
            ? (teamARanked, teamBRanked)
            : (teamBRanked, teamARanked);

        var order = new List<Unit>(teamARanked.Count + teamBRanked.Count);
        int rankCount = Math.Max(firstTeamRanked.Count, secondTeamRanked.Count);
        for (int rank = 0; rank < rankCount; rank++)
        {
            if (rank < firstTeamRanked.Count)
            {
                order.Add(firstTeamRanked[rank]);
            }

            if (rank < secondTeamRanked.Count)
            {
                order.Add(secondTeamRanked[rank]);
            }
        }

        return order;
    }

    /// <summary>Sortiert die Einheiten EINES Teams nach Speed absteigend, bei Gleichstand nach UnitId aufsteigend (siehe Klassendokumentation).</summary>
    private static IReadOnlyList<Unit> RankBySpeedDescending(IEnumerable<Unit> teamUnits) =>
        teamUnits.OrderByDescending(u => u.Speed).ThenBy(u => u.Id).ToList();
}
