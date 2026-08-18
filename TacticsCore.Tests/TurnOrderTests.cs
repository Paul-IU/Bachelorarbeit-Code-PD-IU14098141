using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

namespace TacticsCore.Tests;

/// <summary>
/// Isolierte Tests für die reine Rangfolge-Berechnung - ohne GameState oder
/// MatchController, da <see cref="TurnOrder.ComputeRoundOrder"/> nur eine
/// Liste von Einheiten und das Startteam braucht.
/// </summary>
public class TurnOrderTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void ComputeRoundOrder_3v3_VerschraenktBeginnendMitDemStartteam()
    {
        // Bewusst eindeutig abgestufte Speed-Werte, damit dieser Test rein
        // die Verschränkung prüft, unabhängig vom (separat getesteten)
        // Speed-Tie-Break.
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 20);
        var a3 = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var b1 = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(0, 3), speed: 30);
        var b2 = new TestUnit(Id(5), Team.TeamB, new HexCoordinate(0, 4), speed: 20);
        var b3 = new TestUnit(Id(6), Team.TeamB, new HexCoordinate(0, 5), speed: 10);
        var units = new Unit[] { a1, a2, a3, b1, b2, b3 };

        var orderStartingA = TurnOrder.ComputeRoundOrder(units, Team.TeamA);
        Assert.Equal(new Unit[] { a1, b1, a2, b2, a3, b3 }, orderStartingA);

        var orderStartingB = TurnOrder.ComputeRoundOrder(units, Team.TeamB);
        Assert.Equal(new Unit[] { b1, a1, b2, a2, b3, a3 }, orderStartingB);
    }

    [Fact]
    public void ComputeRoundOrder_InnerhalbEinesTeams_SortiertNachSpeedAbsteigend()
    {
        // IDs bewusst GEGENLÄUFIG zur erwarteten Speed-Reihenfolge vergeben:
        // wäre die Implementierung fälschlich (nur) nach UnitId sortiert,
        // würde dieser Test die exakt umgekehrte Reihenfolge liefern und
        // damit fehlschlagen.
        var slow = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 5);
        var medium = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var fast = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 2), speed: 20);
        var onlyB = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(0, 3), speed: 10);

        var order = TurnOrder.ComputeRoundOrder(new Unit[] { slow, medium, fast, onlyB }, Team.TeamA);
        var teamAOrder = order.Where(u => u.Team == Team.TeamA).ToList();

        Assert.Equal(new Unit[] { fast, medium, slow }, teamAOrder);
    }

    [Fact]
    public void ComputeRoundOrder_BeiSpeedGleichstand_EntscheidetDieUnitIdAufsteigend()
    {
        var higherId = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var lowerId = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);

        var order = TurnOrder.ComputeRoundOrder(new Unit[] { higherId, lowerId }, Team.TeamA);

        Assert.Equal(new Unit[] { lowerId, higherId }, order);
    }

    [Fact]
    public void ComputeRoundOrder_UngleicheHeldenzahl_FehlendeRaengeWerdenAusgelassen()
    {
        // Team A hat 3 lebende Helden, Team B nur 1 - auf den Rängen 2 und 3
        // gibt es für Team B niemanden, diese Slots entfallen ersatzlos.
        // Team A bekommt dadurch in dieser Runde 3 statt nur 2 Aktionen.
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 20);
        var a3 = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var b1 = new TestUnit(Id(4), Team.TeamB, new HexCoordinate(0, 3), speed: 10);

        var order = TurnOrder.ComputeRoundOrder(new Unit[] { a1, a2, a3, b1 }, Team.TeamA);

        Assert.Equal(new Unit[] { a1, b1, a2, a3 }, order);
    }

    [Fact]
    public void ComputeRoundOrder_FiltertBesiegteEinheitenDefensivHeraus()
    {
        var a1 = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var a2 = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 20);
        var b1 = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        a1.ApplyDamage(9999);
        Assert.True(a1.IsDefeated);

        var order = TurnOrder.ComputeRoundOrder(new Unit[] { a1, a2, b1 }, Team.TeamA);

        Assert.DoesNotContain(a1, order);
        Assert.Equal(new Unit[] { a2, b1 }, order);
    }

    [Fact]
    public void NachTodDesSchnellstenHelden_RuecktDerNaechstschnellereInDerFolgeberechnungAufRang1Nach()
    {
        var fastest = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 30);
        var second = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 20);
        var b1 = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        var units = new Unit[] { fastest, second, b1 };

        var roundOneOrder = TurnOrder.ComputeRoundOrder(units, Team.TeamA);
        Assert.Equal(fastest, roundOneOrder[0]);

        // fastest fällt zwischen den Runden (z.B. durch einen Angriff im
        // Verlauf von Runde 1).
        fastest.ApplyDamage(9999);

        var roundTwoOrder = TurnOrder.ComputeRoundOrder(units, Team.TeamA);
        Assert.Equal(second, roundTwoOrder[0]);
        Assert.DoesNotContain(fastest, roundTwoOrder);
    }
}
