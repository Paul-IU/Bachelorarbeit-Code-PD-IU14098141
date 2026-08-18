using TacticsCore.Agents;
using TacticsCore.Units;

namespace TacticsCore.Match;

/// <summary>
/// Führt eine vollständige Partie durch: nimmt eine bereits aufgestellte
/// <see cref="GameState"/> entgegen, wickelt Runde für Runde die Züge aller
/// lebenden Einheiten ab (siehe <see cref="TurnOrder"/> für die
/// Zugreihenfolge und Anforderung 3 für das Aktionsbudget), erkennt das
/// Partieende und liefert das Ergebnis als <see cref="MatchResult"/>.
///
/// Die Partiesteuerung trifft KEINE Annahme über die konkrete
/// Entscheidungslogik der beiden Seiten - sie kennt nur die
/// <see cref="IAgent"/>-Schnittstelle. Aktuell wird dafür ausschließlich
/// <see cref="RandomAgent"/> eingesetzt, aber das ist reine Aufrufer-Sache.
///
/// ZUFALL: Die einzige direkt hier verortete Zufallsentscheidung ist das
/// Startteam (ein einzelner Wurf aus der seed-gesteuerten
/// <see cref="Random"/>-Instanz, siehe <see cref="Run"/>). Dieselbe Random-
/// Instanz wird anschließend an jede <see cref="IAgent.DecideAction"/>-
/// Anfrage weitergereicht - es gibt also genau EINEN Zufallsstrom pro
/// Partie, vollständig durch <c>seed</c> bestimmt. Kein globaler oder
/// statischer Zufallsgenerator, keine Zeitabhängigkeit.
/// </summary>
public sealed class MatchController
{
    private readonly GameState _state;
    private readonly IAgent _teamAAgent;
    private readonly IAgent _teamBAgent;
    private readonly int _seed;
    private readonly bool _recordReplay;

    /// <summary>
    /// Referenzen auf ALLE zu Partiebeginn angetretenen Einheiten (beider
    /// Teams), unabhängig davon, ob sie später besiegt und aus
    /// <see cref="GameState.Units"/> entfernt werden. Wird für die
    /// Endauswertung (Anforderung 5: HP-Summen über das gesamte Team)
    /// gebraucht, da eine besiegte Unit zwar aus der GameState-Buchführung
    /// verschwindet, ihre Eigenschaften (MaxHitPoints, CurrentHitPoints=0)
    /// als Objekt aber weiterhin lesbar bleiben.
    /// </summary>
    private readonly IReadOnlyList<Unit> _initialUnits;

    private readonly IReadOnlyList<UnitSetup> _initialSetup;

    /// <summary>
    /// Erstellt eine neue Partiesteuerung für eine bereits vollständig
    /// aufgestellte Partie (Spielfeld und alle Einheiten sind bereits über
    /// <paramref name="state"/> platziert - die Erzeugung der Einheiten
    /// selbst, inklusive Vergabe ihrer <see cref="Unit.Id"/>, ist bewusst
    /// Sache des Aufrufers und nicht des MatchControllers, siehe Hinweis zu
    /// UnitIds in <see cref="TurnOrder"/>).
    /// </summary>
    /// <param name="state">Die aufgestellte Partie (Board + Einheiten beider Teams).</param>
    /// <param name="teamAAgent">Entscheidungslogik für Team A.</param>
    /// <param name="teamBAgent">Entscheidungslogik für Team B.</param>
    /// <param name="seed">Seed für sämtliche Zufallsentscheidungen dieser Partie.</param>
    /// <param name="recordReplay">
    /// Ob ein vollständiges Replay-Protokoll aufgezeichnet werden soll
    /// (Anforderung 6). Bei false entsteht keinerlei zusätzliche Liste
    /// oder Zuteilung dafür.
    /// </param>
    public MatchController(GameState state, IAgent teamAAgent, IAgent teamBAgent, int seed, bool recordReplay = false)
    {
        _state = state;
        _teamAAgent = teamAAgent;
        _teamBAgent = teamBAgent;
        _seed = seed;
        _recordReplay = recordReplay;

        // Momentaufnahme der Ausgangsaufstellung, bevor sich irgendetwas
        // bewegt oder irgendjemand Schaden nimmt.
        _initialUnits = state.Units.ToList();
        _initialSetup = _initialUnits
            .Select(u => new UnitSetup(u.Id, u.Team, u.Position))
            .OrderBy(s => s.StartPosition.Q)
            .ThenBy(s => s.StartPosition.R)
            .ThenBy(s => s.UnitId)
            .ToList();
    }

    /// <summary>Führt die Partie bis zum Ende aus (Auslöschung eines Teams oder Rundenlimit) und liefert das Ergebnis.</summary>
    public MatchResult Run()
    {
        var random = new Random(_seed);

        // Einziger Zufallswurf außerhalb der Agenten-Entscheidungen: welches
        // Team eröffnet (Anforderung 2). random.Next(2) liefert 0 oder 1,
        // beide mit gleicher Wahrscheinlichkeit.
        var startingTeam = random.Next(2) == 0 ? Team.TeamA : Team.TeamB;

        var replayEntries = _recordReplay ? new List<ReplayEntry>() : null;

        int roundNumber = 0;
        while (true)
        {
            roundNumber++;

            // Rangfolge wird zu Beginn JEDER Runde neu aus den aktuell
            // lebenden Einheiten bestimmt (Anforderung 2).
            var roundOrder = TurnOrder.ComputeRoundOrder(_state.Units, startingTeam);

            foreach (var unit in roundOrder)
            {
                // Zwischen Berechnung der Rangfolge und dem eigenen Zug
                // dieser Einheit kann sie durch den Angriff eines anderen,
                // früher handelnden Helden bereits besiegt worden sein -
                // ihr Zug entfällt dann ersatzlos (Anforderung 2).
                if (unit.IsDefeated)
                {
                    continue;
                }

                var agent = unit.Team == Team.TeamA ? _teamAAgent : _teamBAgent;
                var earlyResult = ExecuteTurn(unit, agent, random, replayEntries, roundNumber, startingTeam);
                if (earlyResult is not null)
                {
                    return earlyResult;
                }
            }

            if (roundNumber >= MatchRules.RoundLimit)
            {
                return BuildResult(MatchEndReason.RoundLimit, roundNumber, startingTeam, replayEntries);
            }
        }
    }

    /// <summary>
    /// Wickelt den vollständigen Zug einer einzelnen Einheit ab: fragt den
    /// zuständigen Agenten wiederholt nach der nächsten Teilaktion, bis das
    /// Aktionsbudget (Anforderung 3: genau eine Bewegung, genau ein
    /// Angriff, freie Reihenfolge) verbraucht ist oder der Agent aktiv
    /// passt. Prüft nach JEDEM erfolgreichen Angriff sofort die
    /// Siegbedingung (Anforderung 4) - endet die Partie dadurch, wird das
    /// fertige <see cref="MatchResult"/> zurückgegeben und der Zug (auch
    /// ein eventuell noch offenes Bewegungsbudget derselben Einheit) sofort
    /// abgebrochen.
    /// </summary>
    /// <remarks>
    /// Als <c>internal</c> statt <c>private</c> deklariert, damit Tests
    /// (über <c>InternalsVisibleTo</c>) das Aktionsbudget, die freie
    /// Reihenfolge und das Passen für EINEN Zug gezielt und ohne
    /// vollständigen <see cref="Run"/>-Durchlauf (der ansonsten bis zu 100
    /// Runden laufen würde) direkt prüfen können.
    /// </remarks>
    /// <returns>Das Partieergebnis, falls die Partie während dieses Zugs endet; sonst null.</returns>
    internal MatchResult? ExecuteTurn(Unit unit, IAgent agent, Random random, List<ReplayEntry>? replayEntries, int roundNumber, Team startingTeam)
    {
        bool hasMoved = false;
        bool hasAttacked = false;

        // Schleife läuft, solange mindestens eine Budget-Komponente noch
        // offen ist - EndTurn (aktives Passen) verlässt sie jederzeit
        // vorzeitig, siehe return-Anweisung im switch unten.
        while (!hasMoved || !hasAttacked)
        {
            var budget = new TurnBudget(CanMove: !hasMoved, CanAttack: !hasAttacked);
            var action = agent.DecideAction(unit, _state, budget, random);

            switch (action)
            {
                case TurnAction.Move move when !hasMoved:
                    // Position VOR der Bewegung festhalten - unit.Position
                    // zeigt nach TryMove bereits auf das Ziel, siehe
                    // ReplayEntry.ForMove.
                    var sourcePosition = unit.Position;

                    // Ein BewegungsVERSUCH verbraucht den Bewegungs-Slot
                    // unabhängig vom Erfolg - das schützt vor einer
                    // Endlosschleife durch einen Agenten, der wiederholt
                    // ein (aus welchem Grund auch immer) ungültiges Ziel
                    // liefert, und passt zur Lesart "der Versuch ist der
                    // Zugbestandteil, nicht zwingend sein Erfolg". Ein
                    // wohlverhaltener Agent, der Ziele stets aus
                    // unit.GetLegalMoves(...) wählt, erlebt das ohnehin nie.
                    bool moved = unit.TryMove(move.Target, _state);
                    if (moved && replayEntries is not null)
                    {
                        replayEntries.Add(ReplayEntry.ForMove(roundNumber, unit, sourcePosition, move.Target));
                    }
                    hasMoved = true;
                    break;

                case TurnAction.Attack attack when !hasAttacked:
                    bool attacked = unit.TryAttack(attack.Target, _state);
                    if (attacked)
                    {
                        if (replayEntries is not null)
                        {
                            replayEntries.Add(ReplayEntry.ForAttack(roundNumber, unit, attack.Target, unit.BaseAttackDamage));
                        }

                        // Anforderung 4: Prüfung nach JEDEM einzelnen
                        // Angriff, nicht erst am Rundenende - sofort hier,
                        // noch bevor die Einheit ggf. ihr Bewegungsbudget
                        // nutzt.
                        if (IsTeamEliminated(Team.TeamA) || IsTeamEliminated(Team.TeamB))
                        {
                            return BuildResult(MatchEndReason.Elimination, roundNumber, startingTeam, replayEntries);
                        }
                    }
                    hasAttacked = true;
                    break;

                case TurnAction.EndTurn:
                    return null;

                default:
                    // Der Agent hat eine laut Budget nicht verfügbare
                    // Aktion gewählt (z.B. erneut Move trotz hasMoved).
                    // Als Fehlverhalten des Agenten gewertet - Zug wird
                    // sicherheitshalber beendet, damit ein fehlerhafter
                    // Agent keine Endlosschleife auslösen kann.
                    return null;
            }
        }

        return null;
    }

    private bool IsTeamEliminated(Team team) => !_state.Units.Any(u => u.Team == team);

    private MatchResult BuildResult(MatchEndReason endReason, int roundsPlayed, Team startingTeam, List<ReplayEntry>? replayEntries)
    {
        var teamAResult = BuildTeamResult(Team.TeamA);
        var teamBResult = BuildTeamResult(Team.TeamB);

        var winner = endReason == MatchEndReason.Elimination
            ? (teamAResult.SurvivingUnitCount > 0 ? MatchWinner.TeamA : MatchWinner.TeamB)
            : ResolveRoundLimitWinner(teamAResult, teamBResult);

        ReplayLog? replay = replayEntries is null
            ? null
            : new ReplayLog(_seed, startingTeam, _initialSetup, replayEntries);

        return new MatchResult(
            winner,
            endReason,
            roundsPlayed,
            teamAResult,
            teamBResult,
            _seed,
            _initialSetup,
            startingTeam,
            replay);
    }

    private TeamResult BuildTeamResult(Team team)
    {
        var teamUnits = _initialUnits.Where(u => u.Team == team).ToList();
        int surviving = teamUnits.Count(u => !u.IsDefeated);
        int currentHp = teamUnits.Sum(u => u.CurrentHitPoints);
        int maxHp = teamUnits.Sum(u => u.MaxHitPoints);
        double fraction = maxHp == 0 ? 0.0 : (double)currentHp / maxHp;
        return new TeamResult(team, surviving, currentHp, maxHp, fraction);
    }

    /// <summary>
    /// Auflösung bei Erreichen des Rundenlimits (Anforderung 4), in dieser
    /// festen Reihenfolge:
    ///   a) Das Team mit mehr lebenden Helden gewinnt.
    ///   b) Bei Gleichstand: das Team mit dem höheren prozentualen
    ///      Gesamt-HP-Anteil (aktuelle HP / maximale HP über das ganze
    ///      Team) gewinnt.
    ///   c) Bei erneutem Gleichstand: Unentschieden.
    ///
    /// Diese Kaskade ist eine bewusste Design-Entscheidung (mehr lebende
    /// Kämpfer wiegt schwerer als reine HP-Summe, weil eine tote Einheit im
    /// weiteren Verlauf gar nicht mehr beitragen könnte) und wird in der
    /// Bachelorarbeit begründet - daher hier als eigene, klar benannte
    /// Methode statt inline verstreuter Vergleichslogik.
    /// </summary>
    private static MatchWinner ResolveRoundLimitWinner(TeamResult teamA, TeamResult teamB)
    {
        if (teamA.SurvivingUnitCount != teamB.SurvivingUnitCount)
        {
            return teamA.SurvivingUnitCount > teamB.SurvivingUnitCount ? MatchWinner.TeamA : MatchWinner.TeamB;
        }

        if (teamA.RemainingHitPointsFraction != teamB.RemainingHitPointsFraction)
        {
            return teamA.RemainingHitPointsFraction > teamB.RemainingHitPointsFraction ? MatchWinner.TeamA : MatchWinner.TeamB;
        }

        return MatchWinner.Draw;
    }
}
