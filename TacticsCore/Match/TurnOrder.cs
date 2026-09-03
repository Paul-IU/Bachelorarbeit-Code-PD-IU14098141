using TacticsCore.Effects;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Berechnet die verschränkte Zugreihenfolge einer Runde aus den aktuell
/// lebenden Einheiten beider Teams. Reine, zustandslose Funktion ohne
/// Bezug zu GameState oder MatchController - leicht isoliert testbar.
///
/// ZUGREIHENFOLGE (Anforderung 2): Innerhalb jedes Teams werden die
/// lebenden HELDEN (siehe Klon-Sonderregel unten) nach der EFFEKTIVEN
/// Geschwindigkeit (siehe <see cref="EffectiveStats.Speed"/> - Basiswert plus
/// Summe aller zu diesem Zeitpunkt aktiven Speed-Modifikatoren aus Buffs/
/// Debuffs/passiven Fähigkeiten) absteigend in Ränge einsortiert (Rang 1 =
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
/// TIE-BREAK BEI GLEICHEM (EFFEKTIVEM) SPEED: Es entscheidet die UnitId
/// (aufsteigend) - das ist an dieser Stelle KEINE kosmetische Nebenent-
/// scheidung: Solange (wie aktuell bei den Standardfiguren) alle Einheiten
/// denselben effektiven Speed-Wert tragen, bestimmt dieser Tie-Break die
/// GESAMTE Rangfolge innerhalb eines Teams. Für die Determinismus-
/// Anforderung bedeutet das: die UnitIds sind Teil der "Ausgangslage", nicht
/// nur ein technisches Implementierungsdetail. Ein Reproduzierbarkeits-Test
/// muss daher dieselben Unit-Objekte (bzw. dieselben, fest vergebenen
/// UnitIds) verwenden, nicht bloß denselben Seed bei strukturell gleicher,
/// aber frisch ausgewürfelter ID-Vergabe.
///
/// SPEED-MODIFIKATOREN (Bachelorarbeit-Korrektur): Bis zu dieser Korrektur
/// las diese Klasse direkt <c>unit.Speed</c> (den unveränderlichen Basiswert)
/// statt der effektiven Geschwindigkeit - ein Speed-Buff oder -Debuff hätte
/// sich also NICHT auf die Rangfolge ausgewirkt, obwohl er auf jeden anderen
/// effektiven Wert (Schaden, Reichweite, Bewegung) bereits generisch wirkte.
/// Behoben, indem <see cref="ComputeRoundOrder"/> zusätzlich den
/// <see cref="TacticsCore.GameState"/> entgegennimmt und darüber
/// <see cref="EffectiveStats.Speed"/> statt des rohen Feldes abfragt. WICHTIG
/// für die Reproduzierbarkeit: das ändert NUR, WELCHER Wert gelesen wird,
/// nicht WANN er gelesen wird - <see cref="ComputeRoundOrder"/> wird nach wie
/// vor exakt einmal, zu Beginn jeder Runde, aufgerufen (siehe
/// <see cref="MatchController.Run"/>); die daraus entstehende Reihenfolge ist
/// weiterhin für die gesamte Runde fest und ändert sich nicht mehr, selbst
/// wenn ein während der laufenden Runde neu gewirkter Speed-Buff die
/// effektive Geschwindigkeit einer bereits eingeordneten Einheit verändert -
/// das wirkt sich erst auf die NÄCHSTE Neuberechnung (die nächste Runde) aus.
/// <see cref="EffectiveStats.Speed"/> selbst ist eine reine, deterministische
/// Funktion des aktuellen (ebenfalls deterministisch erreichten)
/// Spielzustands - die Erweiterung führt also keine neue Zufalls- oder
/// Zeitabhängigkeit ein und gefährdet die Reproduzierbarkeit nicht.
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
    /// bestimmt." <paramref name="state"/> wird ausschließlich für die
    /// effektive Geschwindigkeit gebraucht (siehe <see cref="EffectiveStats.Speed"/>
    /// und die Klassendokumentation, Abschnitt "Speed-Modifikatoren") - die
    /// übergebenen <paramref name="units"/> müssen dafür nicht zwingend
    /// bereits in <paramref name="state"/> platziert sein, ein fehlender
    /// Eintrag ergibt schlicht keine aktiven Effekte (effektiv = Basiswert).
    /// </summary>
    public static IReadOnlyList<Unit> ComputeRoundOrder(IEnumerable<Unit> units, Team startingTeam, GameState state)
    {
        var living = units.Where(u => !u.IsDefeated).ToList();

        var heroes = living.Where(u => !u.IsClone).ToList();
        var clonesByCreator = living
            .Where(u => u.IsClone && u.CreatorId.HasValue)
            .ToLookup(u => u.CreatorId!.Value);

        var teamARanked = RankBySpeedDescending(heroes.Where(u => u.Team == Team.TeamA), state);
        var teamBRanked = RankBySpeedDescending(heroes.Where(u => u.Team == Team.TeamB), state);

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

    /// <summary>Sortiert die Einheiten EINES Teams nach effektivem Speed absteigend, bei Gleichstand nach UnitId aufsteigend (siehe Klassendokumentation).</summary>
    private static IReadOnlyList<Unit> RankBySpeedDescending(IEnumerable<Unit> teamUnits, GameState state) =>
        teamUnits.OrderByDescending(u => EffectiveStats.Speed(u, state)).ThenBy(u => u.Id).ToList();
}
