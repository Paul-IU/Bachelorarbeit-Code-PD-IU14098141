using TacticsCore.Abilities;
using TacticsCore.Board;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests für <see cref="ReplayActionType.RoundOrderDetermined"/> - das neue
/// Ereignis, über das <see cref="MatchController"/> die zu Rundenbeginn von
/// <see cref="TurnOrder.ComputeRoundOrder"/> bestimmte Zugreihenfolge nach
/// außen meldet (Grundlage für die "Reihenfolge dieser Runde"-Anzeige in der
/// Godot-Oberfläche, siehe TacticsCoreGodot.Shared.TurnOrderTracker). Prüft
/// zweierlei: dass die gemeldeten Daten inhaltlich exakt der intern
/// berechneten Reihenfolge entsprechen (nicht bloß irgendeine plausible
/// Annäherung), und dass sie über eine ganze Partie hinweg - insbesondere in
/// Runden mit einer Besiegung oder einer Klon-Erzeugung - niemals der
/// tatsächlich abgearbeiteten Zugreihenfolge widerspricht.
/// </summary>
public class RoundOrderReportingTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    private static readonly AbilityDefinition CloneAbility = new(
        Id: "testklon",
        DisplayName: "Testklon",
        IsPassive: false,
        TargetType: AbilityTargetType.Self,
        Range: 0,
        Effects: new[] { new EffectTemplate(Name: "Klon", IsInstant: true, SpawnsClone: true) });

    private static Hero NewCloneCapableHero(Guid id, Team team, HexCoordinate position, int speed) =>
        new(id, team, position, DefaultHeroes.Berserker with { Speed = speed, Ability = CloneAbility });

    [Fact]
    public void RoundOrderDetermined_EntsprichtGenauDerVonTurnOrderBerechnetenReihenfolge()
    {
        var state = NewState();
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 20);
        var b1 = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(5, 0), speed: 15);
        var b2 = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(5, 1), speed: 5);
        state.AddUnit(a1);
        state.AddUnit(a2);
        state.AddUnit(b1);
        state.AddUnit(b2);

        // Unabhängig (VOR dem Lauf) berechnet - "unabhängig" nur insofern, als
        // es ein zweiter Aufruf ist, nicht weil es einen zweiten Algorithmus
        // gäbe (den soll die Oberfläche ja gerade NICHT selbst nachbauen).
        var expected = TurnOrder.ComputeRoundOrder(state.Units, Team.TeamA).Select(u => u.Id).ToList();

        var controller = new MatchController(state, new ScriptedAgent(), new ScriptedAgent(), seed: 1, recordReplay: true);
        var result = controller.Run(startingTeamOverride: Team.TeamA);

        var firstRoundOrder = result.Replay!.Entries.First(e => e.ActionType == ReplayActionType.RoundOrderDetermined);
        Assert.Equal(1, firstRoundOrder.RoundNumber);
        Assert.Equal(expected, firstRoundOrder.RoundOrder!.UnitIds);
    }

    [Fact]
    public void RoundOrderDetermined_KommtInJederRundeGenauEinmalVorDemErstenZugDieserRunde()
    {
        var state = NewState();
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var b1 = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(5, 0), speed: 10);
        state.AddUnit(a1);
        state.AddUnit(b1);

        var controller = new MatchController(state, new ScriptedAgent(), new ScriptedAgent(), seed: 1, recordReplay: true);
        var result = controller.Run(startingTeamOverride: Team.TeamA);

        var entries = result.Replay!.Entries.ToList();
        var roundNumbers = entries.Where(e => e.ActionType == ReplayActionType.RoundStarted).Select(e => e.RoundNumber).Distinct();

        foreach (var round in roundNumbers)
        {
            int roundStartedIndex = entries.FindIndex(e => e.RoundNumber == round && e.ActionType == ReplayActionType.RoundStarted);
            int determinedIndex = entries.FindIndex(e => e.RoundNumber == round && e.ActionType == ReplayActionType.RoundOrderDetermined);
            int firstTurnStartedIndex = entries.FindIndex(e => e.RoundNumber == round && e.ActionType == ReplayActionType.TurnStarted);

            Assert.True(determinedIndex >= 0, $"Runde {round}: kein RoundOrderDetermined-Eintrag gefunden");
            Assert.Equal(1, entries.Count(e => e.RoundNumber == round && e.ActionType == ReplayActionType.RoundOrderDetermined));
            Assert.True(determinedIndex > roundStartedIndex);
            if (firstTurnStartedIndex >= 0)
            {
                Assert.True(determinedIndex < firstTurnStartedIndex);
            }
        }
    }

    /// <summary>
    /// Der eigentliche Vertrauens-Test für die Oberfläche: Baut absichtlich
    /// eine Runde, in der (a) eine Einheit VOR ihrem eigenen Zug besiegt wird
    /// (sie darf in der gemeldeten Reihenfolge fehlen, aber nicht an falscher
    /// Stelle auftauchen) und (b) ein Klon währenddessen neu entsteht (er
    /// darf in der zu Rundenbeginn gemeldeten Reihenfolge fehlen - er wird ja
    /// erst per CloneSpawned dynamisch eingefügt, siehe TurnOrder.cs) - und
    /// prüft über die GESAMTE Partie hinweg, dass die gemeldete Reihenfolge
    /// der tatsächlich abgearbeiteten niemals widerspricht.
    /// </summary>
    [Fact]
    public void RoundOrderDetermined_WiderersprichtNieDerTatsaechlichenZugreihenfolge_AuchBeiBesiegungUndKlonerzeugung()
    {
        var state = NewState();
        var creator = NewCloneCapableHero(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var victim = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(5, 0), speed: 5);
        var attacker = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(5, 1), speed: 20);
        victim.ApplyDamage(45); // ein einzelner Grundangriff (10 Schaden) reicht danach zur Besiegung
        state.AddUnit(creator);
        state.AddUnit(victim);
        state.AddUnit(attacker);

        // Rang 1: creator (30) vs. attacker (20); Rang 2: victim (5) vs. -
        // (Team B hat keine zweite Einheit). attacker handelt VOR victim und
        // besiegt sie - ihr eigener, eigentlich noch ausstehender Zug entfällt.
        var agentA = new ScriptedAgent();
        agentA.Script(creator, new TurnAction.UseAbility(new AbilityTarget.UnitTarget(creator)));
        var agentB = new ScriptedAgent();
        agentB.Script(attacker, new TurnAction.Attack(victim));

        var controller = new MatchController(state, agentA, agentB, seed: 1, recordReplay: true);
        var result = controller.Run(startingTeamOverride: Team.TeamA);

        AssertReportedOrderNeverContradictsActualPlay(result.Replay!.Entries);
    }

    private static void AssertReportedOrderNeverContradictsActualPlay(IReadOnlyList<ReplayEntry> entries)
    {
        foreach (var round in entries.Select(e => e.RoundNumber).Distinct())
        {
            var roundEntries = entries.Where(e => e.RoundNumber == round).ToList();
            var roundOrderEntry = roundEntries.FirstOrDefault(e => e.ActionType == ReplayActionType.RoundOrderDetermined);
            if (roundOrderEntry is null)
            {
                continue;
            }

            var reportedOrder = roundOrderEntry.RoundOrder!.UnitIds;
            var reportedSet = reportedOrder.ToHashSet();

            var actualTurnStartOrder = roundEntries
                .Where(e => e.ActionType == ReplayActionType.TurnStarted)
                .Select(e => e.ActingUnitId)
                .Where(id => reportedSet.Contains(id)) // ein mitten in der Runde erzeugter Klon ist absichtlich nicht in reportedOrder enthalten
                .ToList();

            // Von der gemeldeten Reihenfolge dürfen einzelne Einheiten fehlen
            // (vorzeitig besiegt, nie an der Reihe gewesen), aber KEINE darf
            // gegenüber einer anderen gemeldeten Einheit vertauscht auftreten.
            var reportedRestrictedToActuallyActed = reportedOrder.Where(id => actualTurnStartOrder.Contains(id)).ToList();
            Assert.Equal(reportedRestrictedToActuallyActed, actualTurnStartOrder);
        }
    }
}
