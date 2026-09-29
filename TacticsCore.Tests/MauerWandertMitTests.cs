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
/// Designentscheidung "Die Mauer wandert mit" (siehe <see cref="DefaultHeroes.Wand"/>
/// und <see cref="EffectTemplate.FieldFollowsSource"/>): Sperre, Umleitung und
/// Vergeltung beziehen sich auf das jeweils aktuelle Feld der Wand.
/// </summary>
public class MauerWandertMitTests
{
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly HexCoordinate AltesFeld = new(0, 1);
    private static readonly HexCoordinate NeuesFeld = new(0, 0);
    private static readonly HexCoordinate NachbarBeiderFelder = new(1, 0); // Distanz 1 zu beiden Feldern

    private static (GameState State, Hero Wand) WandMitMauer()
    {
        var state = new GameState(new GameBoard());
        var wand = new Hero(Id(1), Team.TeamA, AltesFeld, DefaultHeroes.Wand);
        state.AddUnit(wand);
        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        return (state, wand);
    }

    [Fact]
    public void Feldeffekt_WandertMitDerWand()
    {
        var (state, wand) = WandMitMauer();
        Assert.True(wand.TryMove(NeuesFeld, state));

        Assert.Empty(state.GetEffectsOnField(AltesFeld));
        var mauer = Assert.Single(state.GetEffectsOnField(NeuesFeld));
        Assert.True(mauer.RedirectsAttacksToSource);
        Assert.Contains(state.GetLockedFields(), f => f.Field == NeuesFeld && f.LockedAgainst == Team.TeamB);
        Assert.DoesNotContain(state.GetLockedFields(), f => f.Field == AltesFeld);
    }

    [Fact]
    public void Feldsperre_SperrtDasNeueFeldUndGibtDasAlteFrei()
    {
        var (state, wand) = WandMitMauer();
        var gegner = new TestUnit(Id(101), Team.TeamB, NachbarBeiderFelder, speed: 10);
        state.AddUnit(gegner);
        Assert.DoesNotContain(AltesFeld, gegner.GetLegalMoves(state));

        Assert.True(wand.TryMove(NeuesFeld, state));

        var ziele = gegner.GetLegalMoves(state);
        Assert.Contains(AltesFeld, ziele);
        Assert.DoesNotContain(NeuesFeld, ziele);
    }

    [Fact]
    public void UmleitungUndVergeltung_GeltenNurAufDemAktuellenFeldDerWand()
    {
        var (state, wand) = WandMitMauer();
        var verbuendeterAlt = new TestUnit(Id(2), Team.TeamA, AltesFeld, speed: 10);
        var verbuendeterNeu = new TestUnit(Id(3), Team.TeamA, NeuesFeld, speed: 10);
        var angreifer = new TestUnit(Id(101), Team.TeamB, NachbarBeiderFelder, speed: 10);
        state.AddUnit(verbuendeterAlt);
        state.AddUnit(verbuendeterNeu);
        state.AddUnit(angreifer);
        Assert.True(wand.TryMove(NeuesFeld, state));

        // Altes Feld: keine Umleitung, keine Vergeltung mehr.
        Assert.True(CombatSystem.TryAttackDetailed(angreifer, verbuendeterAlt, state, out var ziel1, out _));
        Assert.Same(verbuendeterAlt, ziel1);
        Assert.Equal(angreifer.MaxHitPoints, angreifer.CurrentHitPoints);

        // Neues Feld: Umleitung auf die Wand, Vergeltung an den Angreifer.
        Assert.True(CombatSystem.TryAttackDetailed(angreifer, verbuendeterNeu, state, out var ziel2, out int schaden));
        Assert.Same(wand, ziel2);
        Assert.Equal((int)Math.Round(angreifer.BaseAttackDamage * DefaultHeroes.WandMauerSchadensmultiplikator), schaden); // Selbstreduktion
        Assert.Equal(verbuendeterNeu.MaxHitPoints, verbuendeterNeu.CurrentHitPoints);
        Assert.Equal(angreifer.MaxHitPoints - angreifer.BaseAttackDamage / 2, angreifer.CurrentHitPoints);
    }

    [Fact]
    public void Kopplung_BeideInstanzenEndenGemeinsam_AuchNachDemMitwandern()
    {
        var (state, wand) = WandMitMauer();
        Assert.True(wand.TryMove(NeuesFeld, state));
        Assert.Equal(2, state.ActiveEffects.Count(e => e.Name == "Mauer"));

        EffectSystem.TickUnitEffects(state, wand); // Dauer 1: endet im nächsten Zug der Wand
        Assert.Empty(state.ActiveEffects);

        var (state2, wand2) = WandMitMauer();
        Assert.True(wand2.TryMove(NeuesFeld, state2));
        CombatSystem.ApplyDamageAndHandleDefeat(state2, wand2, 99999);
        Assert.Empty(state2.ActiveEffects); // Tod der Wand beendet beide Instanzen
    }

    [Fact]
    public void WandZiehtAufFeldMitGegner_GegnerBleibtUndDarfGehen_VerbuendeteAngriffeTreffenDenGegner()
    {
        var (state, wand) = WandMitMauer();
        var gegner = new TestUnit(Id(101), Team.TeamB, NeuesFeld, speed: 10);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, NachbarBeiderFelder, speed: 10);
        state.AddUnit(gegner);
        state.AddUnit(verbuendeter);

        Assert.True(wand.TryMove(NeuesFeld, state)); // die eigene Sperre hindert die Wand nicht

        Assert.Equal(NeuesFeld, gegner.Position); // nicht verdrängt
        Assert.NotEmpty(gegner.GetLegalMoves(state).Where(f => f != NeuesFeld)); // kann das Feld verlassen

        // Umleitung nur für Gegner (Designentscheidung): der Angriff des Verbündeten
        // trifft den Gegner, weder die eigene Wand noch eine Vergeltung den Verbündeten.
        // Vor dieser Entscheidung traf er die eigene Wand und erhielt die Vergeltung.
        Assert.True(CombatSystem.TryAttackDetailed(verbuendeter, gegner, state, out var ziel, out _));
        Assert.Same(gegner, ziel);
        Assert.Equal(gegner.MaxHitPoints - verbuendeter.BaseAttackDamage, gegner.CurrentHitPoints);
        Assert.Equal(wand.MaxHitPoints, wand.CurrentHitPoints);
        Assert.Equal(verbuendeter.MaxHitPoints, verbuendeter.CurrentHitPoints);
    }

    [Fact]
    public void Simulationskopie_MitwandernBleibtAufDieKopieBeschraenkt()
    {
        var (state, wand) = WandMitMauer();
        var kopie = state.CloneForSimulation();
        var wandKopie = kopie.Units.Single(u => u.Id == wand.Id);

        Assert.True(wandKopie.TryMove(NeuesFeld, kopie));

        Assert.Single(kopie.GetEffectsOnField(NeuesFeld));
        Assert.Single(state.GetEffectsOnField(AltesFeld)); // Original unverändert
        Assert.Empty(state.GetEffectsOnField(NeuesFeld));
    }
}
