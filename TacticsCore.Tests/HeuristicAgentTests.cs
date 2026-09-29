using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Tests.TestSupport;
using TacticsCore.Units;

// Alias nötig, weil der Namespace TacticsCore.Board selbst "Board" heißt wie
// die darin enthaltene Klasse - ohne Alias wäre der bloße Bezeichner "Board"
// als Typname mehrdeutig (Namespace vs. Klasse).
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCore.Tests;

/// <summary>
/// Tests der heuristischen Agenten (<see cref="HeuristicAgent"/>) und ihrer
/// gemeinsamen Bewertungsfunktion (<see cref="TurnEvaluator"/>).
/// </summary>
public class HeuristicAgentTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);
    private static readonly HeroCatalog Catalog = DefaultHeroes.CreateDefaultCatalog();
    private static readonly TeamComposition TrioA = new(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });
    private static readonly TeamComposition TrioB = new(new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Musiker });

    // -----------------------------------------------------------------
    // Bewertungsfunktion
    // -----------------------------------------------------------------

    /// <summary>
    /// Überarbeitung 1 (Schutz als Flussgröße): der Schildwall ist nur so viel
    /// wert, wie er von der AKTUELL bestehenden Bedrohung abfängt. Im ersten
    /// Entwurf zählte er als Zugewinn an effektiven Lebenspunkten (+850 allein,
    /// +1850 mit zwei Verbündeten) - auch ohne jeden Gegner in Reichweite.
    /// </summary>
    [Fact]
    public void Schildwall_WirdNurPositivBewertetWennEinGegnerInReichweiteSteht()
    {
        TurnScore Bewerte(bool gegnerInReichweite)
        {
            var state = NewState();
            var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
            state.AddUnit(tank);
            state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10));
            // Gegner: Reichweite 2 + 1 = 3; Distanz 2 erreicht das Feld, Distanz 4 nicht.
            state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, gegnerInReichweite ? 3 : 5), speed: 10));
            return HeuristicAgent.EvaluateSequence(tank, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(tank.Position)))!;
        }

        var ohneGegner = Bewerte(gegnerInReichweite: false);
        Assert.Equal(0, ohneGegner.DamagePreventedGain);
        Assert.False(ohneGegner.Weighted(HeuristicWeights.AggressiveProfile) > 0);
        Assert.False(ohneGegner.Weighted(HeuristicWeights.DefensiveProfile) > 0);

        // Überarbeitung 6: der Gegner greift EINMAL an - 100 × 0,5 = 50 abgefangen
        // (vorher je Paar gezählt: 2 × 100 × 0,5 = 100).
        var mitGegner = Bewerte(gegnerInReichweite: true);
        Assert.Equal(50, mitGegner.DamagePreventedGain);
        Assert.Equal(50, mitGegner.ThreatReduction); // der Schutz steckt in der Bedrohungsverringerung
        Assert.True(mitGegner.Weighted(HeuristicWeights.AggressiveProfile) > 0);
        Assert.True(mitGegner.Weighted(HeuristicWeights.DefensiveProfile) > 0);
    }

    /// <summary>
    /// Umleitung verschiebt den Schaden nur: der Verbündete auf dem Feld der Wand
    /// gilt als so geschützt wie die Wand selbst (Selbstreduktion 25 %). Weil der
    /// Schutz jetzt ein Fluss ist, hängt er nicht mehr von den Lebenspunkten der
    /// Wand ab (im ersten Entwurf: max(effektive HP von Wand und Verbündetem)).
    /// </summary>
    [Fact]
    public void Mauer_FaengtFuerWandUndVerbuendetenJeDenReduktionsanteilDerBedrohungAb()
    {
        TurnScore Bewerte(int gegnerZeile)
        {
            var state = NewState();
            var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
            state.AddUnit(wand);
            state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10)); // teilt das Feld
            state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, gegnerZeile), speed: 10));
            return HeuristicAgent.EvaluateSequence(wand, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(wand.Position)))!;
        }

        // Überarbeitung 6: ein Gegner, einmal gezählt, gegen das ungeschützteste Ziel -
        // Wand (0,75) und umgeleiteter Verbündeter (0,75): 100 × 0,25 = 25
        // (vorher je Einheit: 25 + 25 = 50).
        Assert.Equal(100 * 0.25, Bewerte(gegnerZeile: 3).DamagePreventedGain);
        Assert.Equal(0, Bewerte(gegnerZeile: 5).DamagePreventedGain);
    }

    /// <summary>
    /// Überarbeitung 7: ein Gegner, dem die Mauer den Weg versperrt, zählt für das
    /// abgeschnittene Ziel nicht mehr als bedrohlich. Aufbau: Verbündeter auf (0,0),
    /// Wand auf (0,1), gegnerischer Schildträger (Bewegung 1, Reichweite 1,
    /// Schaden 70) auf (0,2). Der Verbündete ist nur über (0,1) erreichbar - sperrt
    /// die Mauer dieses Feld, bleibt dem Gegner nur die Wand (Multiplikator 0,75):
    /// Bedrohung 70 -> 52,5. Mit der reinen Hex-Distanz (Überarbeitung 6) galt der
    /// Verbündete weiter als erreichbar, die Mauer war 0 wert.
    /// </summary>
    [Fact]
    public void GegnerDemDieMauerDenWegVersperrt_ZaehltFuerDasAbgeschnitteneZielNicht()
    {
        var state = NewState();
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        state.AddUnit(wand);
        state.AddUnit(new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10));
        state.AddUnit(new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 2), DefaultHeroes.Schildtraeger));

        var score = HeuristicAgent.EvaluateSequence(wand, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(wand.Position)))!;

        Assert.Equal(70 - 70 * 0.75, score.ThreatReduction, 9);
        Assert.Equal(70 - 70 * 0.75, score.DamagePreventedGain, 9);
    }

    /// <summary>
    /// Überarbeitung 6: ein Gegner, der mehrere eigene Einheiten erreicht, geht nur
    /// EINMAL in die Bedrohung ein. Rückt eine weitere Einheit in seine Reichweite,
    /// ändert sich die Bedrohung nicht (er gewinnt nur ein weiteres mögliches Ziel);
    /// verlässt eine von mehreren bedrohten Einheiten seine Reichweite, ebenso wenig.
    /// </summary>
    [Fact]
    public void Bedrohung_ZaehltEinenGegnerNurEinmal()
    {
        var state = NewState();
        var a = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 2), speed: 10);
        var b = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(1, 1), speed: 10);
        var c = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 0), speed: 10); // außer Reichweite (Distanz 4)
        state.AddUnit(a);
        state.AddUnit(b);
        state.AddUnit(c);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 4), speed: 10)); // Reichweite 3, erreicht a und b

        var hinein = HeuristicAgent.EvaluateSequence(c, state, new TurnAction.Move(new HexCoordinate(0, 1)))!; // jetzt ebenfalls in Reichweite
        Assert.Equal(0, hinein.ThreatReduction); // vorher: −100 (dritte Paarung)

        var heraus = HeuristicAgent.EvaluateSequence(a, state, new TurnAction.Move(new HexCoordinate(0, 0)))!; // b bleibt bedroht
        Assert.Equal(0, heraus.ThreatReduction); // vorher: +100
    }

    /// <summary>
    /// Überarbeitung 3 (Grundgewicht der Angriffsmöglichkeit): symmetrisches
    /// Heranrücken (Angriffsmöglichkeit und Bedrohung steigen um denselben
    /// Betrag) ist beim defensiven Agenten nicht mehr schlechter als
    /// Stehenbleiben, sondern neutral (1 × 200 − 2 × 100 = 0; mit dem Spiegelbild
    /// 1:1 waren es −100). Der aggressive Agent bewertet denselben Zug klar
    /// positiv und rückt heran; der defensive rückt bei Gleichstand nicht heran,
    /// weil ein Zug die leere Zugfolge (0) übertreffen muss.
    /// </summary>
    [Fact]
    public void SymmetrischesHeranruecken_IstBeimDefensivenAgentenNeutral_BeimAggressivenPositiv()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10); // Reichweite 2 + 1 = 3
        state.AddUnit(eigene);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10)); // ebenfalls Reichweite 3

        var heran = HeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(0, 2)))!;

        Assert.Equal(0, heran.Weighted(HeuristicWeights.DefensiveProfile));
        Assert.True(heran.Weighted(HeuristicWeights.AggressiveProfile) > heran.Weighted(HeuristicWeights.DefensiveProfile));

        var budget = new TurnBudget(CanMove: true, CanAct: true);
        Assert.IsType<TurnAction.Move>(new HeuristicAgent(HeuristicWeights.AggressiveProfile).DecideAction(eigene, state, budget, new Random(1)));
        Assert.IsType<TurnAction.EndTurn>(new HeuristicAgent(HeuristicWeights.DefensiveProfile).DecideAction(eigene, state, budget, new Random(1)));
    }

    /// <summary>
    /// Heranrücken bleibt beim defensiven Agenten lageabhängig: kann eine Einheit
    /// mit größerer Reichweite einen Gegner für den nächsten Zug erreichen, ohne
    /// selbst in dessen Reichweite zu geraten, rückt sie heran und bleibt dabei
    /// außerhalb der gegnerischen Reichweite (+200 Angriffsmöglichkeit, keine
    /// Bedrohung; mit Grundgewicht 1,0 war dieser Zug ebenfalls schon positiv).
    /// </summary>
    [Fact]
    public void DefensiverAgent_RuecktHeranWennErDabeiNichtBedrohtWird()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10, attackRange: 2); // Reichweite 2 + 2 = 4
        state.AddUnit(eigene);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10)); // Reichweite 3, Distanz 5

        var zug = Assert.IsType<TurnAction.Move>(new HeuristicAgent(HeuristicWeights.DefensiveProfile)
            .DecideAction(eigene, state, new TurnBudget(CanMove: true, CanAct: true), new Random(1)));

        Assert.Equal(4, HexCoordinate.Distance(zug.Target, new HexCoordinate(0, 5))); // erreichbar, aber selbst unbedroht
    }

    /// <summary>
    /// Überarbeitung 2 (Angriffsmöglichkeit): Heranrücken, das einen Gegner für
    /// den nächsten Zug in Reichweite bringt, ist beim aggressiven Agenten mehr
    /// wert als Stehenbleiben (0) - obwohl es die eigene Bedrohung ebenso erhöht.
    /// </summary>
    [Fact]
    public void Heranruecken_InReichweiteEinesGegners_WirdVomAggressivenAgentenBesserBewertetAlsStehenbleiben()
    {
        var state = NewState();
        var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10); // Reichweite 2 + 1 = 3
        state.AddUnit(eigene);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 5), speed: 10)); // Distanz 5: kein Angriff in diesem Zug

        var heran = HeuristicAgent.EvaluateSequence(eigene, state, new TurnAction.Move(new HexCoordinate(0, 2)))!;

        Assert.Equal(HeuristicAgentSettings.AttackOpportunityWeight * 100, heran.AttackOpportunityGain); // Grundgewicht 2,0 -> 200
        Assert.Equal(-100, heran.ThreatReduction);
        Assert.True(heran.Weighted(HeuristicWeights.AggressiveProfile) > 0); // 2 × 200 − 100

        var zug = Assert.IsType<TurnAction.Move>(new HeuristicAgent(HeuristicWeights.AggressiveProfile)
            .DecideAction(eigene, state, new TurnBudget(CanMove: true, CanAct: true), new Random(1)));
        Assert.True(HexCoordinate.Distance(zug.Target, new HexCoordinate(0, 5)) <= 3);
    }

    [Fact]
    public void Angriff_ZaehltSchadenUndBesiegtBonus()
    {
        var state = NewState();
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        state.AddUnit(gegner);
        gegner.ApplyDamage(gegner.MaxHitPoints - 50); // ein Treffer besiegt ihn

        var score = HeuristicAgent.EvaluateSequence(angreifer, state, new TurnAction.Attack(gegner))!;

        Assert.Equal(50, score.DamageDealt); // Schaden bis 0, nicht der volle Angriffswert
        Assert.Equal(gegner.BaseAttackDamage * HeuristicAgentSettings.KillBonusRounds, score.KillBonus);
        Assert.Equal(gegner.BaseAttackDamage, score.ThreatReduction); // der besiegte Gegner bedroht niemanden mehr
        Assert.Equal(0, score.AttackOpportunityGain); // sein Wegfall ist kein Verlust an Angriffsmöglichkeit
    }

    /// <summary>
    /// Aufbau für Überarbeitung 4: eigener Angreifer neben einem angeschlagenen
    /// Gegner (50 HP, ein Treffer besiegt ihn); optional umklammert ihn der eigene
    /// Hugger (seit "Umklammerung prozentual" 100 × 0,67 = 67, vorher 100 − 100 = 0)
    /// oder ein Buff hebt seinen Schaden.
    /// </summary>
    private static (GameState State, Unit Angreifer, Unit Gegner) BesiegbarerGegner(bool umklammert = false, int schadensbuff = 0)
    {
        var state = NewState();
        var angreifer = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(angreifer);
        state.AddUnit(gegner);
        gegner.ApplyDamage(gegner.MaxHitPoints - 50);

        if (umklammert)
        {
            var hugger = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
            state.AddUnit(hugger);
            var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");
            Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(gegner), state, greifen));
            Assert.Equal(67, Effects.EffectiveStats.AttackDamage(gegner, state)); // 100 × 0,67 (vorher 100 − 100 = 0)
        }

        if (schadensbuff != 0)
        {
            Effects.EffectSystem.Apply(state, Effects.ActiveEffect.OnUnit(gegner, gegner,
                new Effects.EffectTemplate(Name: "Buff", IsInstant: false, Duration: 2, StatModifiers: new Effects.StatModifierSet(BaseAttackDamageDelta: schadensbuff))));
            Assert.Equal(gegner.BaseAttackDamage + schadensbuff, Effects.EffectiveStats.AttackDamage(gegner, state));
        }

        return (state, angreifer, gegner);
    }

    /// <summary>
    /// Überarbeitung 4: ein Angriff, der einen vom eigenen Hugger umklammerten
    /// Gegner besiegt, ist für BEIDE Profile positiv. Vorher: Besiegt-Bonus 0
    /// (effektiver Schaden 0) und die wegfallende Schwächung als Verlust
    /// (−100 × 2 Runden, defensiv doppelt) - der Angriff war negativ.
    /// </summary>
    [Fact]
    public void AngriffDerEinenUmklammertenGegnerBesiegt_WirdPositivBewertet()
    {
        var (state, angreifer, gegner) = BesiegbarerGegner(umklammert: true);

        var score = HeuristicAgent.EvaluateSequence(angreifer, state, new TurnAction.Attack(gegner))!;

        Assert.Equal(50, score.DamageDealt);
        Assert.Equal(gegner.BaseAttackDamage * HeuristicAgentSettings.KillBonusRounds, score.KillBonus);
        Assert.Equal(0, score.FutureDefense); // die Schwächung des besiegten Gegners wird nicht als Verlust gebucht
        Assert.True(score.Weighted(HeuristicWeights.DefensiveProfile) > 0);
        Assert.True(score.Weighted(HeuristicWeights.AggressiveProfile) > 0);
    }

    /// <summary>
    /// Überarbeitung 4, a: der Besiegt-Bonus ist der ungeschwächte Grundschaden ×
    /// KillBonusRounds - gleich, ob der Gegner gerade geschwächt, gebufft oder
    /// unverändert ist.
    /// </summary>
    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 60)]
    public void BesiegtBonus_HaengtNichtVomAktuellenEffektivenSchadenAb(bool umklammert, int schadensbuff)
    {
        var (state, angreifer, gegner) = BesiegbarerGegner(umklammert, schadensbuff);

        var score = HeuristicAgent.EvaluateSequence(angreifer, state, new TurnAction.Attack(gegner))!;

        Assert.Equal(gegner.BaseAttackDamage * HeuristicAgentSettings.KillBonusRounds, score.KillBonus);
    }

    /// <summary>
    /// Überarbeitung 8: die Umklammerung verändert Schadenswerte (eigene Halbierung,
    /// Schwächung des Gegners) - das geht nur noch EINMAL ein, über Angriffsmöglichkeit
    /// und Bedrohung, nicht zusätzlich als angekündigte Wirkung.
    /// Aufbau: Hugger (60) + Verbündeter (100) gegen einen Gegner (100, Reichweite 3),
    /// der beide erreicht; der Hugger heftet sich an den Gegner.
    /// </summary>
    [Fact]
    public void Umklammerung_SchadensmodifikatorenGehenNurEinmalEin()
    {
        var state = NewState();
        var hugger = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 0), speed: 10);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        foreach (var u in new Unit[] { hugger, verbuendeter, gegner }) state.AddUnit(u);
        var greifen = DefaultHeroes.Hugger.Ability.Modes!.Single(m => m.Id == "greifen");

        var score = HeuristicAgent.EvaluateSequence(hugger, state, new TurnAction.UseAbility(new AbilityTarget.UnitTarget(gegner), greifen))!;

        // Keine angekündigte Buchung mehr (vorher: −60 Halbierung, +66 Schwächung über 2 Runden).
        Assert.Equal(0, score.FutureOffense);
        Assert.Equal(0, score.FutureDefense);
        // Halbierung genau einmal: Angriffsmöglichkeit des Huggers 60 -> 30, mit Grundgewicht 2.
        Assert.Equal(HeuristicAgentSettings.AttackOpportunityWeight * (30 - 60), score.AttackOpportunityGain);
        // Schwächung genau einmal: Bedrohung 100 -> 67 (der Hugger ist unangreifbar, der Verbündete bleibt erreichbar).
        Assert.Equal(100 - 67, score.ThreatReduction);
    }

    /// <summary>
    /// Überarbeitung 8: ein Kampflied zählt nur, wenn ein gebuffter Verbündeter im
    /// nächsten Zug einen Gegner erreicht - dann genau einmal über die
    /// Angriffsmöglichkeit; ohne erreichbaren Gegner ist es nichts wert.
    /// </summary>
    [Theory]
    [InlineData(5, 0)]   // Gegner für niemanden erreichbar
    [InlineData(4, 40)]  // nur der Verbündete (100) erreicht ihn: +40 %
    public void Kampflied_ZaehltNurUeberDieAngriffsmoeglichkeit(int gegnerZeile, int erwarteteZunahme)
    {
        var state = NewState();
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Musiker);
        var verbuendeter = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // Reichweite 3
        state.AddUnit(musiker);
        state.AddUnit(verbuendeter);
        state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, gegnerZeile), speed: 10));
        var kampflied = DefaultHeroes.Musiker.Ability.Modes!.Single(m => m.Id == "kampflied");

        var score = HeuristicAgent.EvaluateSequence(musiker, state, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(musiker.Position), kampflied))!;

        Assert.Equal(0, score.FutureOffense); // vorher zusätzlich als angekündigter Buff gebucht
        Assert.Equal(HeuristicAgentSettings.AttackOpportunityWeight * erwarteteZunahme, score.AttackOpportunityGain);
        if (erwarteteZunahme == 0)
        {
            Assert.False(score.Weighted(HeuristicWeights.AggressiveProfile) > 0);
            Assert.False(score.Weighted(HeuristicWeights.DefensiveProfile) > 0);
        }
    }

    // -----------------------------------------------------------------
    // Zugwahl
    // -----------------------------------------------------------------

    [Fact]
    public void DefensiverUndAggressiverAgent_TreffenBeiIdentischerAusgangslageUnterschiedlicheZugwahl()
    {
        // Um anzugreifen, muss die Einheit in die Reichweite des Gegners ziehen:
        // +100 zugefügter Schaden, aber +100 Bedrohung. Aggressiv (2:1) lohnt das,
        // defensiv (1:2) nicht.
        (GameState State, Unit Eigene) Aufbau()
        {
            var state = NewState();
            var eigene = new TestUnit(Id(1), Team.TeamA, new HexCoordinate(0, 0), speed: 10, attackRange: 2);
            state.AddUnit(eigene);
            state.AddUnit(new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 4), speed: 10)); // Reichweite 2 + 1 = 3
            return (state, eigene);
        }

        var budget = new TurnBudget(CanMove: true, CanAct: true);
        var (sA, uA) = Aufbau();
        var (sD, uD) = Aufbau();
        var aggressiv = new HeuristicAgent(HeuristicWeights.AggressiveProfile).DecideAction(uA, sA, budget, new Random(1));
        var defensiv = new HeuristicAgent(HeuristicWeights.DefensiveProfile).DecideAction(uD, sD, budget, new Random(1));

        var zug = Assert.IsType<TurnAction.Move>(aggressiv);
        Assert.True(HexCoordinate.Distance(zug.Target, new HexCoordinate(0, 4)) <= 2); // in Angriffsreichweite
        Assert.IsType<TurnAction.EndTurn>(defensiv); // bleibt außer Reichweite
    }

    [Fact]
    public void Agent_NutztDieRandomInstanzNicht()
    {
        var state = NewState();
        var eigene = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Berserker);
        state.AddUnit(eigene);
        state.AddUnit(new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 3), DefaultHeroes.Elfe));
        var budget = new TurnBudget(CanMove: true, CanAct: true);
        var agent = new HeuristicAgent(HeuristicWeights.AggressiveProfile);

        var random = new Random(5);
        var erste = agent.DecideAction(eigene, state, budget, random);
        int naechsterWert = random.Next();

        Assert.Equal(new Random(5).Next(), naechsterWert); // Zufallsstrom unberührt
        Assert.Equal(Describe(erste), Describe(agent.DecideAction(eigene, state, budget, new Random(999))));
    }

    private static string Describe(TurnAction action) => action switch
    {
        TurnAction.Move m => $"Move {m.Target}",
        TurnAction.Attack a => $"Attack {a.Target.Id}",
        TurnAction.UseAbility u => $"Ability {u.Target} {u.Mode?.Id}",
        _ => action.GetType().Name,
    };

    // -----------------------------------------------------------------
    // Determinismus, Threads, Etiketten-Unabhängigkeit
    // -----------------------------------------------------------------

    [Theory]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void GleicherSeed_UndEinOderMehrereThreads_LiefernIdentischeErgebnisse(string agentName)
    {
        var board = new GameBoard();
        var pairings = new[] { (TrioA, TrioB), (TrioA, TrioA) };
        var agents = AgentConfiguration.FromName(agentName);

        var einThread = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 1);
        var nochmal = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 1);
        var mehrere = BatchSimulationRunner.Run(board, pairings, agents, baseSeed: 99, maxSetupsPerPairing: 6, degreeOfParallelism: 8);

        var leer = new Dictionary<string, HeroActionCounts>();
        foreach (var anderer in new[] { nochmal, mehrere })
        {
            Assert.Equal(
                einThread.PairingResults.Select(r => r with { HeroActionTotals = leer }),
                anderer.PairingResults.Select(r => r with { HeroActionTotals = leer }));
            for (int i = 0; i < einThread.PairingResults.Count; i++)
            {
                Assert.Equal(
                    einThread.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                    anderer.PairingResults[i].HeroActionTotals.OrderBy(kv => kv.Key, StringComparer.Ordinal));
            }
        }
    }

    /// <summary>
    /// Etiketten-Unabhängigkeit mit der bestehenden Prüfung
    /// (<see cref="NameIndependenceCheck"/>) über alle 84 Spiegelpaarungen - im
    /// normalen Testlauf mit 2 Ausgangslagen je Paarung, damit die Suite schnell
    /// bleibt. Die vollständige Prüfung über alle 72 Ausgangslagen steht in
    /// <see cref="AlleSpiegelpaarungenAlle72Ausgangslagen_SindEtikettenunabhaengig"/>.
    /// </summary>
    [Theory]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void AlleSpiegelpaarungen_Stichprobe_SindEtikettenunabhaengig(string agentName) =>
        PruefeSpiegelpaarungen(agentName, setups: 2);

    /// <summary>
    /// Vollständige Prüfung: alle 84 Spiegelpaarungen × alle 72 Ausgangslagen.
    /// Läuft je Agent etwa 40 Minuten (gemessen: 1 h 16 min für beide) und ist deshalb als "Langlaufend"
    /// markiert - aus dem normalen Lauf ausschließen mit
    /// <c>dotnet test --filter "Kategorie!=Langlaufend"</c>, gezielt ausführen mit
    /// <c>--filter "Kategorie=Langlaufend"</c>.
    /// </summary>
    [Theory]
    [Trait("Kategorie", "Langlaufend")]
    [InlineData(AgentNames.Aggressive)]
    [InlineData(AgentNames.Defensive)]
    public void AlleSpiegelpaarungenAlle72Ausgangslagen_SindEtikettenunabhaengig(string agentName) =>
        PruefeSpiegelpaarungen(agentName, setups: null);

    private static void PruefeSpiegelpaarungen(string agentName, int? setups)
    {
        var mirrors = CompositionGenerator.GenerateAllCompositions(Catalog).Select(c => (c, c)).ToList();
        Assert.Equal(84, mirrors.Count);

        var result = BatchSimulationRunner.Run(
            new GameBoard(), mirrors, AgentConfiguration.FromName(agentName), baseSeed: 2026,
            maxSetupsPerPairing: setups, degreeOfParallelism: Environment.ProcessorCount, verifyNameIndependence: true);

        var report = result.NameIndependence!;
        Assert.Equal(84 * (setups ?? 72), report.CheckedMatches);
        Assert.True(report.Violations.Count == 0, string.Join("\n", report.Violations.Take(5).Select(v => v.Describe())));
    }

    [Theory]
    [InlineData("aggressive")]
    [InlineData("DEFENSIVE")]
    public void FromName_KenntDieHeuristischenAgenten(string name)
    {
        var config = AgentConfiguration.FromName(name);
        Assert.IsType<HeuristicAgent>(config.CreateTeamAAgent());
        Assert.IsType<HeuristicAgent>(config.CreateTeamBAgent());
        Assert.Equal(name.ToLowerInvariant(), config.Name);
    }

    [Fact]
    public void CloneForSimulation_IstUnabhaengigVomOriginal()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
        var gegner = new TestUnit(Id(101), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(gegner);
        AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state);

        var kopie = state.CloneForSimulation();
        var tankKopie = (Hero)kopie.Units.Single(u => u.Id == tank.Id);
        var gegnerKopie = kopie.Units.Single(u => u.Id == gegner.Id);
        Assert.Equal(tank.AbilityCooldownRemaining, tankKopie.AbilityCooldownRemaining);
        Assert.Equal(state.ActiveEffects.Count, kopie.ActiveEffects.Count);

        Assert.True(Combat.CombatSystem.TryAttack(tankKopie, gegnerKopie, kopie));
        Assert.True(tankKopie.TryMove(new HexCoordinate(0, 0), kopie));

        Assert.Equal(gegner.MaxHitPoints, gegner.CurrentHitPoints); // Original unverändert
        Assert.Equal(new HexCoordinate(0, 1), tank.Position);
        Assert.Contains(tank, state.GetUnitsAt(new HexCoordinate(0, 1)));
        Assert.NotEmpty(state.GetEffectsOnField(new HexCoordinate(0, 1)));
    }
}
