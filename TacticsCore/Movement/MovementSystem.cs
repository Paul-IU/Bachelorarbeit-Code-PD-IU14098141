using TacticsCore.Hex;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore.Movement;

/// <summary>
/// Ein einzelnes, über die Erreichbarkeitsberechnung gefundenes Zielfeld,
/// zusammen mit der dafür minimal benötigten Schrittzahl.
/// </summary>
public readonly record struct ReachableField(HexCoordinate Coordinate, int Steps);

/// <summary>
/// Eigene, von <see cref="Unit"/> getrennte Bewegungslogik: Erreichbarkeits-
/// berechnung per Breitensuche (BFS), Pfadrekonstruktion und Zugausführung.
///
/// Es gibt bewusst nur DIESE eine Bewegungslogik - eine frühere Ein-Feld-
/// Bewegung ist hier kein separater Codepfad mehr, sondern ergibt sich
/// automatisch: ein direktes Nachbarfeld ist immer mit Schrittzahl 1
/// erreichbar, solange die Reichweite mindestens 1 beträgt.
/// </summary>
public static class MovementSystem
{
    /// <summary>Ergebnis einer Breitensuche: Distanzen und Vorgänger für die Pfadrekonstruktion.</summary>
    private sealed class BfsResult
    {
        public required Dictionary<HexCoordinate, int> Distances { get; init; }
        public required Dictionary<HexCoordinate, HexCoordinate> Predecessors { get; init; }
    }

    /// <summary>
    /// Führt die eigentliche Breitensuche ab <paramref name="start"/> bis
    /// zur maximalen Schrittzahl <paramref name="range"/> aus. Das
    /// Startfeld selbst ist immer mit 0 Schritten enthalten.
    ///
    /// BLOCKIERREGEL (siehe <see cref="OccupancyRules.UnitsBlockMovement"/>):
    /// In der aktuellen Grundregel blockieren Einheiten die Bewegung nicht
    /// - die Suche berücksichtigt daher ausschließlich, ob ein Feld Teil
    /// des 16-Felder-Spielfelds ist, nicht ob es belegt ist. Der Schalter
    /// existiert bereits jetzt als Vorbereitung für später blockierende
    /// Fähigkeiten.
    ///
    /// DETERMINISMUS / PFADAUSWAHL: Die Warteschlange arbeitet strikt FIFO
    /// (<see cref="Queue{T}"/>), und die Nachbarn jedes Feldes werden immer
    /// in der festen Reihenfolge von <see cref="HexCoordinate.NeighborDirections"/>
    /// untersucht. Ein bereits entdecktes Feld wird nie erneut verarbeitet
    /// (siehe die <c>Distances.ContainsKey</c>-Prüfung unten) - der
    /// Vorgänger jedes Feldes ist also exakt der erste Knoten, der es auf
    /// diese Weise erreicht. Da sowohl die Warteschlangen-Reihenfolge als
    /// auch die Nachbar-Reihenfolge bei identischem Ausgangszustand jedes
    /// Mal exakt gleich ablaufen, liefert die Suche bei jedem Aufruf
    /// denselben Baum und damit denselben kürzesten Pfad zu jedem Feld -
    /// unabhängig von z.B. der internen Reihenfolge eines Dictionary, auf
    /// die an keiner Stelle für die Kontrolllogik zurückgegriffen wird.
    /// </summary>
    private static BfsResult RunBreadthFirstSearch(HexCoordinate start, int range, GameState state)
    {
        var distances = new Dictionary<HexCoordinate, int> { [start] = 0 };
        var predecessors = new Dictionary<HexCoordinate, HexCoordinate>();
        var queue = new Queue<HexCoordinate>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentSteps = distances[current];
            if (currentSteps >= range)
            {
                continue; // Reichweite an dieser Stelle ausgeschöpft - keine weiteren Schritte von hier
            }

            foreach (var neighbor in current.GetNeighbors())
            {
                if (!state.Board.Contains(neighbor))
                {
                    continue; // existiert nicht auf dem 16-Felder-Spielfeld
                }

                if (OccupancyRules.UnitsBlockMovement && state.IsOccupied(neighbor))
                {
                    continue; // siehe Blockierregel-Kommentar oben
                }

                if (distances.ContainsKey(neighbor))
                {
                    continue; // bereits mit kürzerer oder gleich kurzer Distanz gefunden
                }

                distances[neighbor] = currentSteps + 1;
                predecessors[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        return new BfsResult { Distances = distances, Predecessors = predecessors };
    }

    /// <summary>
    /// Liefert alle innerhalb der Bewegungsreichweite von <paramref name="unit"/>
    /// erreichbaren Felder mit der jeweils benötigten Schrittzahl. Das
    /// eigene Ausgangsfeld ist immer mit 0 Schritten enthalten - Stehen-
    /// bleiben ist ein legaler Zug. Stabil sortiert nach (q, r).
    /// </summary>
    public static IReadOnlyList<ReachableField> GetReachableFields(Unit unit, GameState state)
    {
        var result = RunBreadthFirstSearch(unit.Position, unit.MovementRange, state);

        return result.Distances
            .Select(kvp => new ReachableField(kvp.Key, kvp.Value))
            .OrderBy(rf => rf.Coordinate.Q)
            .ThenBy(rf => rf.Coordinate.R)
            .ToList();
    }

    /// <summary>
    /// Liefert nur die Zielkoordinaten aus <see cref="GetReachableFields"/>
    /// (ohne Schrittzahl) - das sind exakt die legalen Bewegungsziele einer
    /// Einheit, stabil sortiert nach (q, r). Grundlage für die späteren
    /// Entscheidungsagenten.
    /// </summary>
    public static IReadOnlyList<HexCoordinate> GetLegalMoveTargets(Unit unit, GameState state) =>
        GetReachableFields(unit, state).Select(rf => rf.Coordinate).ToList();

    /// <summary>
    /// Liefert den kürzesten Pfad von der aktuellen Position der Einheit zu
    /// <paramref name="target"/>, inklusive Start- und Zielfeld. Leere
    /// Liste, wenn das Ziel außerhalb der Reichweite liegt. Die Auswahlregel
    /// bei mehreren gleich langen Pfaden ist bei
    /// <see cref="RunBreadthFirstSearch"/> dokumentiert.
    /// </summary>
    public static IReadOnlyList<HexCoordinate> GetShortestPath(Unit unit, HexCoordinate target, GameState state)
    {
        var result = RunBreadthFirstSearch(unit.Position, unit.MovementRange, state);
        if (!result.Distances.ContainsKey(target))
        {
            return Array.Empty<HexCoordinate>();
        }

        var path = new List<HexCoordinate> { target };
        var current = target;
        while (current != unit.Position)
        {
            current = result.Predecessors[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Versucht, <paramref name="unit"/> auf <paramref name="target"/> zu
    /// bewegen. Gültig ist jedes Feld, das laut Erreichbarkeitsberechnung
    /// innerhalb der Bewegungsreichweite liegt (inklusive des eigenen
    /// Feldes - Stehenbleiben). Eine ungültige Bewegung ändert den
    /// Spielzustand nicht (keine Teilausführung).
    /// </summary>
    public static bool TryMove(Unit unit, HexCoordinate target, GameState state)
    {
        if (unit.IsDefeated)
        {
            return false; // besiegte Einheiten können nicht mehr handeln
        }

        var result = RunBreadthFirstSearch(unit.Position, unit.MovementRange, state);
        if (!result.Distances.ContainsKey(target))
        {
            return false;
        }

        // Ab hier ist die Bewegung als Ganzes gültig - state.MoveUnit prüft
        // intern nur noch das (aktuell inaktive) Stapel-Limit und führt die
        // Änderung atomar aus (siehe GameState).
        return state.MoveUnit(unit, target);
    }
}
