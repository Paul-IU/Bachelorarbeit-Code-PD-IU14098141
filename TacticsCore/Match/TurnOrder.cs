using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Berechnet die verschränkte Zugreihenfolge einer Runde aus den aktuell
/// lebenden Einheiten beider Teams. Reine, zustandslose Funktion ohne
/// Bezug zu GameState oder MatchController - leicht isoliert testbar.
///
/// ZUGREIHENFOLGE (Anforderung 2): Innerhalb jedes Teams werden die
/// lebenden HELDEN (siehe Klon-Sonderregel unten) nach Speed absteigend in
/// Ränge einsortiert (Rang 1 = schnellste Einheit). Gehandelt wird dann
/// verschränkt über beide Teams hinweg, beginnend mit dem per Zufall
/// bestimmten Startteam:
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
/// (wie aktuell bei den Standardfiguren) alle Einheiten denselben Speed-Wert
/// tragen, bestimmt dieser Tie-Break die GESAMTE Rangfolge innerhalb eines
/// Teams. Für die Determinismus-Anforderung bedeutet das: die UnitIds sind
/// Teil der "Ausgangslage", nicht nur ein technisches Implementierungs-
/// detail. Ein Reproduzierbarkeits-Test muss daher dieselben Unit-Objekte
/// (bzw. dieselben, fest vergebenen UnitIds) verwenden, nicht bloß denselben
/// Seed bei strukturell gleicher, aber frisch ausgewürfelter ID-Vergabe.
///
/// KLON-SONDERREGEL (Anforderung 9): Ein Klon zählt NICHT als Held und wird
/// daher NICHT nach eigenem Speed in die Rangfolge einsortiert - er handelt
/// stattdessen immer UNMITTELBAR NACH seinem Erzeuger, unabhängig von
/// dessen Rang. Existiert der Klon bereits VOR Rundenbeginn, geschieht das
/// hier (Einfügen direkt hinter dem Erzeuger-Rang); entsteht der Klon erst
/// WÄHREND der laufenden Runde (im Zug seines Erzeugers), fügt ihn
/// <see cref="MatchController"/> zusätzlich dynamisch in die bereits
/// laufende Reihenfolge ein (siehe dortige Dokumentation) - beide Fälle
/// zusammen stellen sicher, dass ein Klon in jedem Fall unmittelbar nach
/// seinem Erzeuger drankommt.
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

        var heroes = living.Where(u => !u.IsClone).ToList();
        var clonesByCreator = living
            .Where(u => u.IsClone && u.CreatorId.HasValue)
            .ToLookup(u => u.CreatorId!.Value);

        var teamARanked = RankBySpeedDescending(heroes.Where(u => u.Team == Team.TeamA));
        var teamBRanked = RankBySpeedDescending(heroes.Where(u => u.Team == Team.TeamB));

        var (firstTeamRanked, secondTeamRanked) = startingTeam == Team.TeamA
            ? (teamARanked, teamBRanked)
            : (teamBRanked, teamARanked);

        var order = new List<Unit>(heroes.Count + living.Count - heroes.Count);
        int rankCount = Math.Max(firstTeamRanked.Count, secondTeamRanked.Count);
        for (int rank = 0; rank < rankCount; rank++)
        {
            if (rank < firstTeamRanked.Count)
            {
                AddHeroWithClones(firstTeamRanked[rank]);
            }

            if (rank < secondTeamRanked.Count)
            {
                AddHeroWithClones(secondTeamRanked[rank]);
            }
        }

        return order;

        void AddHeroWithClones(Unit hero)
        {
            order.Add(hero);

            // Je Erzeuger darf ohnehin nur ein Klon gleichzeitig existieren
            // (Anforderung 9) - die Sortierung nach UnitId ist daher nur
            // eine defensive Absicherung für den Fall, dass diese Invariante
            // später aufgeweicht würde.
            foreach (var clone in clonesByCreator[hero.Id].OrderBy(c => c.Id))
            {
                order.Add(clone);
            }
        }
    }

    /// <summary>Sortiert die Einheiten EINES Teams nach Speed absteigend, bei Gleichstand nach UnitId aufsteigend (siehe Klassendokumentation).</summary>
    private static IReadOnlyList<Unit> RankBySpeedDescending(IEnumerable<Unit> teamUnits) =>
        teamUnits.OrderByDescending(u => u.Speed).ThenBy(u => u.Id).ToList();
}
