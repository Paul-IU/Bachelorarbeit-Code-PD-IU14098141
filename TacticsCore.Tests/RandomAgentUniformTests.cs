using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Board;
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
/// Gezielte Tests für <see cref="RandomAgentUniform"/> - die Determinismus-,
/// Parallelisierungs- und Spiegelpaarungs-Eigenschaften sind bereits generisch
/// (unabhängig vom konkreten Agenten) in <see cref="HeroDeterminismTests"/>
/// bzw. <see cref="BatchSimulationRunnerTests"/> mit BEIDEN Agenten
/// abgesichert - hier ausschließlich die für diesen Agenten NAMENSGEBENDE
/// Auswahlregel selbst: gleichverteilt über ALLE Einzelzüge, nicht zuerst
/// über den Aktionstyp (siehe Klassendokumentation von RandomAgentUniform
/// für den Unterschied zu RandomAgentByActionType).
/// </summary>
public class RandomAgentUniformTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    [Fact]
    public void DecideAction_OhneJedeOptionBeendetDenZug()
    {
        var state = new GameState(new GameBoard());
        var hero = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        state.AddUnit(hero);
        // Kein Bewegungs-, kein Aktions-Budget mehr - keine Teilaktion kann
        // legal sein, unabhängig davon, was auf dem Feld steht.
        var budget = new TurnBudget(CanMove: false, CanAct: false);

        var action = new RandomAgentUniform().DecideAction(hero, state, budget, new Random(1));

        Assert.IsType<TurnAction.EndTurn>(action);
    }

    /// <summary>
    /// Bachelorarbeit-Anforderung: bei einer konstruierten Situation mit
    /// mehreren legalen Bewegungen, zwei legalen Angriffen und genau einem
    /// legalen Fähigkeitseinsatz muss über viele Durchläufe hinweg JEDE
    /// Kategorie in etwa proportional zu ihrer tatsächlichen Anzahl an
    /// Optionen gewählt werden - eine Bewegung mit vielen Zielen also
    /// entsprechend häufiger als ein Angriff mit nur zwei oder eine
    /// Fähigkeit mit nur einem einzigen Ziel. Bewusst NICHT auf eine exakt
    /// erwartete Bewegungsanzahl (z.B. "8") verdrahtet - die tatsächliche
    /// Anzahl hängt von der konkreten Hex-Geometrie an dieser Stelle des
    /// Spielfelds ab und wird stattdessen zur Laufzeit über dieselbe API
    /// ermittelt, die auch der Agent selbst nutzt (<see cref="Unit.GetLegalMoves"/>).
    /// Angriffe (genau zwei angrenzende Gegner) und Fähigkeitseinsatz (der
    /// Illusionist hat genau ein Ziel - sich selbst, keine Modi) sind dagegen
    /// bewusst exakt konstruiert.
    /// </summary>
    [Fact]
    public void DecideAction_WaehltJedenEinzelzugProportionalZurAnzahlSeinerOptionen()
    {
        var state = new GameState(new GameBoard());
        var illusionist = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Illusionist);
        // Zwei Gegner auf direkt angrenzenden Feldern (Hex-Distanz 1) -
        // Illusionist hat AttackRange 1, das ergibt exakt zwei legale
        // Angriffsziele, unabhängig von der übrigen Feldgeometrie.
        var enemyA = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 1), speed: 10);
        var enemyB = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(1, 0), speed: 10);
        state.AddUnit(illusionist);
        state.AddUnit(enemyA);
        state.AddUnit(enemyB);

        var budget = new TurnBudget(CanMove: true, CanAct: true);
        int expectedMoves = illusionist.GetLegalMoves(state).Count;
        int expectedAttacks = illusionist.GetLegalAttacks(state).Count;
        int expectedAbilityUses = AbilitySystem.GetLegalAbilityUses(illusionist, state).Count;

        Assert.True(expectedMoves > 0); // Vorbedingung: es gibt überhaupt etwas zu vergleichen
        Assert.Equal(2, expectedAttacks);
        Assert.Equal(1, expectedAbilityUses); // Trugbild: Self-Ziel, keine Modi -> immer genau eine Option

        int totalOptions = expectedMoves + expectedAttacks + expectedAbilityUses;
        double expectedMoveShare = (double)expectedMoves / totalOptions;
        double expectedAttackShare = (double)expectedAttacks / totalOptions;
        double expectedAbilityShare = (double)expectedAbilityUses / totalOptions;

        // Zustand wird in der Schleife bewusst NICHT verändert (keine der
        // zurückgegebenen TurnAction-Werte wird tatsächlich ausgeführt) -
        // die Optionenzahl bleibt über alle Durchläufe hinweg konstant,
        // damit die erwarteten Anteile über den gesamten Testlauf gültig
        // bleiben.
        var agent = new RandomAgentUniform();
        var random = new Random(12345);
        int moveCount = 0, attackCount = 0, abilityCount = 0;
        const int trials = 20_000;
        for (int i = 0; i < trials; i++)
        {
            var action = agent.DecideAction(illusionist, state, budget, random);
            switch (action)
            {
                case TurnAction.Move: moveCount++; break;
                case TurnAction.Attack: attackCount++; break;
                case TurnAction.UseAbility: abilityCount++; break;
                default: Assert.Fail($"Unerwartete Aktion: {action}"); break;
            }
        }

        // Toleranz 2 Prozentpunkte - bei 20.000 Durchläufen und den hier
        // vorliegenden Anteilen (keine Randwahrscheinlichkeit nahe 0 oder 1)
        // liegt die statistische Schwankungsbreite (Binomialverteilung) weit
        // darunter; ein deutlich größerer Ausschlag wäre ein echtes Indiz für
        // einen Fehler in der Gewichtung, kein normales Rauschen.
        const double tolerance = 0.02;
        Assert.True(Math.Abs((double)moveCount / trials - expectedMoveShare) < tolerance,
            $"Bewegungsanteil: erwartet ~{expectedMoveShare:P1}, beobachtet {(double)moveCount / trials:P1}");
        Assert.True(Math.Abs((double)attackCount / trials - expectedAttackShare) < tolerance,
            $"Angriffsanteil: erwartet ~{expectedAttackShare:P1}, beobachtet {(double)attackCount / trials:P1}");
        Assert.True(Math.Abs((double)abilityCount / trials - expectedAbilityShare) < tolerance,
            $"Fähigkeitsanteil: erwartet ~{expectedAbilityShare:P1}, beobachtet {(double)abilityCount / trials:P1}");
    }
}
