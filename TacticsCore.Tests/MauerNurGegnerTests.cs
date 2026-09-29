using TacticsCore.Abilities;
using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Designentscheidungen zur Mauer der Wand (siehe <see cref="DefaultHeroes.Wand"/>):
/// (1) umgeleitet werden nur Angriffe von GEGNERN der Wand, (2) die
/// Selbstreduktion (25 %, siehe DefaultHeroes.WandMauerSchadensmultiplikator) wirkt auf
/// JEDEN Schaden an der Wand, (3) Vergeltung nur bei
/// umgeleiteten Treffern, (4) jede Wand hat ihre eigene Mauer (der Anwender
/// gehört bei <see cref="EffectTemplate.FieldFollowsSource"/> zur Identität).
/// </summary>
public class MauerNurGegnerTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    /// <summary>Schaden an der Wand bei aktiver Mauer (Selbstreduktion, gerundet wie im CombatSystem).</summary>
    private static int MitMauer(int schaden) => (int)Math.Round(schaden * DefaultHeroes.WandMauerSchadensmultiplikator);

    /// <summary>Wand (Team A) mit aktiver Mauer auf (0,1).</summary>
    private static (GameState State, Hero Wand) WandMitMauer()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        state.AddUnit(wand);
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        return (state, wand);
    }

    // --- (1) Umleitung nur für Gegner ---------------------------------

    [Fact]
    public void VerbuendeterSchiesstDurchDasFeldDerWand_TrifftSeinZielUndErhaeltKeineVergeltung()
    {
        var (state, wand) = WandMitMauer();
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10, attackRange: 2);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(verbuendeter);
        state.AddUnit(gegner);
        Assert.Contains(new HexCoordinate(0, 1), HexLine.Between(verbuendeter.Position, gegner.Position)); // Sichtlinie kreuzt das Feld der Wand

        Assert.True(CombatSystem.TryAttackDetailed(verbuendeter, gegner, state, out var ziel, out int schaden));

        Assert.Same(gegner, ziel);
        Assert.Equal(verbuendeter.BaseAttackDamage, schaden);
        Assert.Equal(wand.MaxHitPoints, wand.CurrentHitPoints);
        Assert.Equal(verbuendeter.MaxHitPoints, verbuendeter.CurrentHitPoints); // keine Vergeltung
    }

    [Fact]
    public void GegnerSchiesstDurchDasFeldDerWand_WirdUmgeleitetUndErhaeltVergeltung()
    {
        var (state, wand) = WandMitMauer();
        var verbuendeterDerWand = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10, attackRange: 2);
        state.AddUnit(verbuendeterDerWand);
        state.AddUnit(gegner);

        Assert.True(CombatSystem.TryAttackDetailed(gegner, verbuendeterDerWand, state, out var ziel, out int schaden));

        Assert.Same(wand, ziel);
        Assert.Equal(MitMauer(gegner.BaseAttackDamage), schaden); // Selbstreduktion
        Assert.Equal(verbuendeterDerWand.MaxHitPoints, verbuendeterDerWand.CurrentHitPoints);
        Assert.Equal(gegner.MaxHitPoints - gegner.BaseAttackDamage / 2, gegner.CurrentHitPoints); // Vergeltung: Hälfte des URSPRÜNGLICHEN Schadens
    }

    [Fact]
    public void WandGreiftGegnerAufIhremEigenenFeldAn_TrifftDenGegnerNichtSichSelbst()
    {
        var (state, wand) = WandMitMauer();
        var gegner = new TestUnit(Id(101), Team.TeamB, wand.Position, speed: 10);
        state.AddUnit(gegner);

        Assert.True(CombatSystem.TryAttackDetailed(wand, gegner, state, out var ziel, out _));

        Assert.Same(gegner, ziel);
        Assert.Equal(wand.MaxHitPoints, wand.CurrentHitPoints);
        Assert.Equal(gegner.MaxHitPoints - wand.BaseAttackDamage, gegner.CurrentHitPoints);
    }

    // --- (4) Jede Wand hat ihre eigene Mauer (Anwender gehört zur Identität) ---

    private static readonly HexCoordinate GemeinsamesFeld = new(0, 1);

    /// <summary>
    /// Zwei gegnerische Wände samt je einem Verbündeten auf demselben Feld,
    /// beide wirken ihre Mauer. Vor der Designentscheidung frischte die zweite
    /// Wand die Mauer der ersten auf, statt eine eigene zu erzeugen.
    /// </summary>
    private static (GameState State, Hero WandA, Hero WandB, Unit VerbuendeterA, Unit VerbuendeterB) ZweiWaendeMitMauer()
    {
        var state = new GameState(new GameBoard());
        var wandA = new Hero(Id(1), Team.TeamA, GemeinsamesFeld, DefaultHeroes.Wand);
        var wandB = new Hero(Id(101), Team.TeamB, GemeinsamesFeld, DefaultHeroes.Wand);
        var verbuendeterA = new TestUnit(Id(2), Team.TeamA, GemeinsamesFeld, speed: 10);
        var verbuendeterB = new TestUnit(Id(102), Team.TeamB, GemeinsamesFeld, speed: 10);
        foreach (var u in new Unit[] { wandA, wandB, verbuendeterA, verbuendeterB }) state.AddUnit(u);

        Assert.True(AbilitySystem.TryUseAbility(wandA, new AbilityTarget.FieldTarget(GemeinsamesFeld), state));
        Assert.True(AbilitySystem.TryUseAbility(wandB, new AbilityTarget.FieldTarget(GemeinsamesFeld), state));
        return (state, wandA, wandB, verbuendeterA, verbuendeterB);
    }

    [Fact]
    public void ZweiWaendeAufEinemFeld_BeideMauernBestehenNebeneinander()
    {
        var (state, wandA, wandB, _, _) = ZweiWaendeMitMauer();

        var feldMauern = state.GetEffectsOnField(GemeinsamesFeld).ToList();
        Assert.Equal(2, feldMauern.Count);
        Assert.Contains(feldMauern, e => e.SourceUnitId == wandA.Id && e.FieldLockAgainstTeam == Team.TeamB);
        Assert.Contains(feldMauern, e => e.SourceUnitId == wandB.Id && e.FieldLockAgainstTeam == Team.TeamA);
        Assert.Contains(state.GetEffectsOnUnit(wandA.Id), e => e.IncomingDamageMultiplier == DefaultHeroes.WandMauerSchadensmultiplikator);
        Assert.Contains(state.GetEffectsOnUnit(wandB.Id), e => e.IncomingDamageMultiplier == DefaultHeroes.WandMauerSchadensmultiplikator);
    }

    [Fact]
    public void ZweiWaendeAufEinemFeld_AngriffGehtZurWandDesAngegriffenenTeams_VergeltungAnDenAngreifer()
    {
        var (state, wandA, wandB, verbuendeterA, verbuendeterB) = ZweiWaendeMitMauer();
        var angreiferA = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var angreiferB = new TestUnit(Id(103), Team.TeamB, new HexCoordinate(0, 0), speed: 10);
        state.AddUnit(angreiferA);
        state.AddUnit(angreiferB);

        // Gegner von A greift A's Verbündeten an: Umleitung zu Wand A (nicht zur
        // eigenen Wand B, deren Mauer Angriffe ihres Teams nicht umleitet).
        Assert.True(CombatSystem.TryAttackDetailed(angreiferB, verbuendeterA, state, out var zielVonB, out int schadenVonB));
        Assert.Same(wandA, zielVonB);
        Assert.Equal(MitMauer(angreiferB.BaseAttackDamage), schadenVonB); // Selbstreduktion von Wand A
        Assert.Equal(verbuendeterA.MaxHitPoints, verbuendeterA.CurrentHitPoints);
        Assert.Equal(angreiferB.MaxHitPoints - angreiferB.BaseAttackDamage / 2, angreiferB.CurrentHitPoints); // Vergeltung
        Assert.Equal(wandB.MaxHitPoints, wandB.CurrentHitPoints);

        // Spiegelbildlich: Gegner von B greift B's Verbündeten an -> Wand B.
        Assert.True(CombatSystem.TryAttackDetailed(angreiferA, verbuendeterB, state, out var zielVonA, out int schadenVonA));
        Assert.Same(wandB, zielVonA);
        Assert.Equal(MitMauer(angreiferA.BaseAttackDamage), schadenVonA);
        Assert.Equal(verbuendeterB.MaxHitPoints, verbuendeterB.CurrentHitPoints);
        Assert.Equal(angreiferA.MaxHitPoints - angreiferA.BaseAttackDamage / 2, angreiferA.CurrentHitPoints);

        // Die Vergeltung selbst wird nie umgeleitet - Wand A hat nur den ersten Treffer erhalten.
        Assert.Equal(wandA.MaxHitPoints - MitMauer(angreiferB.BaseAttackDamage), wandA.CurrentHitPoints);
    }

    [Fact]
    public void ZweiWaendeAufEinemFeld_WegziehendeWandNimmtNurIhreEigeneMauerMit()
    {
        var (state, wandA, wandB, _, _) = ZweiWaendeMitMauer();
        var neuesFeld = new HexCoordinate(1, 1);

        Assert.True(wandA.TryMove(neuesFeld, state)); // Einheiten auf dem Feld dürfen es verlassen

        var verbliebene = Assert.Single(state.GetEffectsOnField(GemeinsamesFeld));
        Assert.Equal(wandB.Id, verbliebene.SourceUnitId);
        var mitgenommene = Assert.Single(state.GetEffectsOnField(neuesFeld));
        Assert.Equal(wandA.Id, mitgenommene.SourceUnitId);
    }

    [Fact]
    public void ZweiWaendeAufEinemFeld_BeideSperrenWirken_EinheitenAufDemFeldDuerfenGehen()
    {
        var (state, wandA, wandB, verbuendeterA, verbuendeterB) = ZweiWaendeMitMauer();
        var aussenA = new TestUnit(Id(4), Team.TeamA, new HexCoordinate(1, 0), speed: 10);
        var aussenB = new TestUnit(Id(104), Team.TeamB, new HexCoordinate(1, 1), speed: 10);
        state.AddUnit(aussenA);
        state.AddUnit(aussenB);

        // Außenstehende BEIDER Teams: gesperrt (Team A durch die Mauer von B und umgekehrt).
        Assert.DoesNotContain(GemeinsamesFeld, aussenA.GetLegalMoves(state));
        Assert.DoesNotContain(GemeinsamesFeld, aussenB.GetLegalMoves(state));

        // Wer bereits auf dem Feld steht, kann es verlassen.
        foreach (var einheit in new Unit[] { wandA, wandB, verbuendeterA, verbuendeterB })
            Assert.NotEmpty(einheit.GetLegalMoves(state).Where(f => f != GemeinsamesFeld));
    }

    /// <summary>
    /// Die Regel gilt nur für Effekte mit <see cref="EffectTemplate.FieldFollowsSource"/>:
    /// der Schildwall zweier Schildträger stapelt sich weiterhin NICHT (sonst 0,25 Aufnahme).
    /// </summary>
    [Fact]
    public void ZweiSchildtraegerAufEinemFeld_SchildwallBleibtEineInstanz()
    {
        var state = new GameState(new GameBoard());
        var schild1 = new Hero(Id(1), Team.TeamA, GemeinsamesFeld, DefaultHeroes.Schildtraeger);
        var schild2 = new Hero(Id(2), Team.TeamA, GemeinsamesFeld, DefaultHeroes.Schildtraeger);
        var verbuendeter = new TestUnit(Id(3), Team.TeamA, GemeinsamesFeld, speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        foreach (var u in new Unit[] { schild1, schild2, verbuendeter, gegner }) state.AddUnit(u);

        Assert.True(AbilitySystem.TryUseAbility(schild1, new AbilityTarget.FieldTarget(GemeinsamesFeld), state));
        int vorher = state.ActiveEffects.Count(e => e.Name == "Schildwall");
        Assert.True(AbilitySystem.TryUseAbility(schild2, new AbilityTarget.FieldTarget(GemeinsamesFeld), state));

        Assert.Equal(vorher, state.ActiveEffects.Count(e => e.Name == "Schildwall"));
        Assert.True(CombatSystem.TryAttackDetailed(gegner, verbuendeter, state, out _, out int schaden));
        Assert.Equal(gegner.BaseAttackDamage / 2, schaden);
    }

    // --- (2) Selbstreduktion generell, (3) Vergeltung nur bei Umleitung ---

    [Fact]
    public void DirekterAngriffAufDieWand_ReduzierterSchadenAberKeineVergeltung()
    {
        var (state, wand) = WandMitMauer();
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(gegner);

        Assert.True(CombatSystem.TryAttackDetailed(gegner, wand, state, out var ziel, out int schaden));

        Assert.Same(wand, ziel);
        Assert.Equal(MitMauer(gegner.BaseAttackDamage), schaden);
        Assert.Equal(gegner.MaxHitPoints, gegner.CurrentHitPoints);
    }

    [Fact]
    public void SchadenUeberZeitAnDerWand_WirdEbenfallsReduziert()
    {
        var (state, wand) = WandMitMauer();
        var quelle = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 4), speed: 10);
        state.AddUnit(quelle);
        EffectSystem.Apply(state, ActiveEffect.OnUnit(quelle, wand, new EffectTemplate(Name: "Gift", IsInstant: false, Duration: 3, DamagePerTick: 100)));

        EffectSystem.TickDamageOverTime(state, wand);

        Assert.Equal(wand.MaxHitPoints - 75, wand.CurrentHitPoints);
        Assert.Equal(quelle.MaxHitPoints, quelle.CurrentHitPoints); // Schaden über Zeit wird nicht umgeleitet, keine Vergeltung
    }
}
