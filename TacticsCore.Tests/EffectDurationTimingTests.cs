using TacticsCore.Abilities;
using TacticsCore.Combat;
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
/// Belegt die Korrektur "Wirkungsdauer hängt am Anwender" (siehe
/// <see cref="EffectSystem"/>): die Restdauer JEDES Effekts mit begrenzter
/// Dauer zählt zu Beginn des nächsten eigenen Zugs seines ANWENDERS herunter -
/// nicht zu Beginn einer Runde (frühere Feld-Effekte) und nicht am Zug des
/// Ziels (frühere Unit-Effekte). Der Schaden über Zeit bleibt dagegen beim
/// Zugbeginn des ZIELS. Enthält außerdem die dokumentierenden Prüfungen zur
/// Wand-Vergeltung (Abschnitt "BEOBACHTUNGEN"), die bewusst das AKTUELLE,
/// noch unveränderte Verhalten festhalten.
/// </summary>
public class EffectDurationTimingTests
{
    private static GameState NewState() => new(new GameBoard());
    private static Guid Id(int n) => new(n, 0, 0, new byte[8]);

    // -----------------------------------------------------------------
    // Zugreihenfolge des Fehlerberichts, im echten Run()-Durchlauf:
    //   Illusionist (A), Bogenschütze (B), Schildträger (A),
    //   Elementarmagier (B), Wand (A), Elfe (B)
    // -----------------------------------------------------------------

    private sealed record SechsHeldenAufbau(
        GameState State, Hero Illusionist, Hero Schildtraeger, Hero Wand,
        Hero Bogenschuetze, Hero Elementarmagier, Hero Elfe);

    private static SechsHeldenAufbau NewSechsHeldenAufbau()
    {
        var state = NewState();
        // Team A (Rang nach Speed absteigend: Illusionist 6, Schildträger 5, Wand 4)
        var illusionist = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 5), DefaultHeroes.Illusionist);
        var schildtraeger = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger);
        var wand = new Hero(Id(3), Team.TeamA, new HexCoordinate(-1, 1), DefaultHeroes.Wand);
        // Team B (Bogenschütze 15, Elementarmagier 14, Elfe 12) - Bogenschütze
        // (Reichweite 3) erreicht sowohl Schildträger (Distanz 3) als auch Wand (Distanz 3).
        var bogenschuetze = new Hero(Id(101), Team.TeamB, new HexCoordinate(0, 3), DefaultHeroes.Bogenschuetze);
        var elementarmagier = new Hero(Id(102), Team.TeamB, new HexCoordinate(0, 4), DefaultHeroes.Elementarmagier);
        var elfe = new Hero(Id(103), Team.TeamB, new HexCoordinate(1, 2), DefaultHeroes.Elfe);

        foreach (var hero in new[] { illusionist, schildtraeger, wand, bogenschuetze, elementarmagier, elfe })
        {
            state.AddUnit(hero);
        }

        return new SechsHeldenAufbau(state, illusionist, schildtraeger, wand, bogenschuetze, elementarmagier, elfe);
    }

    [Fact]
    public void Schildwall_GesetztVonEinheitAnRang3_WirktNochBeimAngriffDesBogenschuetzenAnRang2InDerFolgerunde()
    {
        var aufbau = NewSechsHeldenAufbau();
        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        agentA.Script(aufbau.Schildtraeger, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(aufbau.Schildtraeger.Position)));
        // Runde 1: passen, Runde 2: den Schildträger angreifen.
        agentB.Script(aufbau.Bogenschuetze, TurnAction.EndTurn.Instance, new TurnAction.Attack(aufbau.Schildtraeger));

        var result = new MatchController(aufbau.State, agentA, agentB, seed: 1, recordReplay: true).Run(Team.TeamA);

        var entries = result.Replay!.Entries;
        var attackIndex = entries.ToList().FindIndex(e => e.ActionType == ReplayActionType.Attack);
        var attack = entries[attackIndex];
        Assert.Equal(2, attack.RoundNumber); // Folgerunde
        Assert.Equal(aufbau.Bogenschuetze.Id, attack.ActingUnitId);
        Assert.Equal(aufbau.Bogenschuetze.BaseAttackDamage / 2, attack.DamageDealt); // halbiert: der Schildwall wirkt noch
        Assert.Equal(aufbau.Schildtraeger.MaxHitPoints - aufbau.Bogenschuetze.BaseAttackDamage / 2, aufbau.Schildtraeger.CurrentHitPoints);

        // Der Effekt läuft erst zu Beginn des eigenen Zugs des Schildträgers ab -
        // also NACH dem Angriff, noch in derselben Runde 2.
        var expiredIndex = entries.ToList().FindIndex(e => e.ActionType == ReplayActionType.EffectExpired && e.EffectName == "Schildwall");
        Assert.True(expiredIndex > attackIndex, "Schildwall darf erst nach dem Angriff des Bogenschützen ablaufen.");
        Assert.Equal(2, entries[expiredIndex].RoundNumber);
        Assert.Empty(aufbau.State.GetEffectsOnField(aufbau.Schildtraeger.Position));
    }

    [Fact]
    public void Mauer_GesetztVonEinheitAnRang5_WirktNochBeimAngriffDesBogenschuetzenAnRang2InDerFolgerunde_BeideInstanzenEndenGemeinsam()
    {
        var aufbau = NewSechsHeldenAufbau();
        var agentA = new ScriptedAgent();
        var agentB = new ScriptedAgent();
        agentA.Script(aufbau.Wand, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(aufbau.Wand.Position)));
        agentB.Script(aufbau.Bogenschuetze, TurnAction.EndTurn.Instance, new TurnAction.Attack(aufbau.Wand));

        var result = new MatchController(aufbau.State, agentA, agentB, seed: 1, recordReplay: true).Run(Team.TeamA);

        var entries = result.Replay!.Entries.ToList();
        var attackIndex = entries.FindIndex(e => e.ActionType == ReplayActionType.Attack);
        var attack = entries[attackIndex];
        Assert.Equal(2, attack.RoundNumber);
        Assert.Equal((int)Math.Round(aufbau.Bogenschuetze.BaseAttackDamage * DefaultHeroes.WandMauerSchadensmultiplikator), attack.DamageDealt); // Selbstreduktion der Mauer wirkt noch

        // Beide gekoppelten Instanzen (Feldsperre + Selbstreduktion) enden
        // gemeinsam, erst NACH dem Angriff, im Zug der Wand in Runde 2.
        var expired = entries.Select((e, i) => (Entry: e, Index: i))
            .Where(x => x.Entry.ActionType == ReplayActionType.EffectExpired && x.Entry.EffectName == "Mauer")
            .ToList();
        Assert.Equal(2, expired.Count);
        Assert.All(expired, x => Assert.True(x.Index > attackIndex));
        Assert.All(expired, x => Assert.Equal(2, x.Entry.RoundNumber));
        Assert.Empty(aufbau.State.ActiveEffects);
    }

    // -----------------------------------------------------------------
    // Eine einzige Effektinstanz, Dauer am Anwender
    // -----------------------------------------------------------------

    [Fact]
    public void Schildwall_SchuetztAlleEinheitenAufDemFeldUndEndetFuerAlleGleichzeitigBeimZugDesAnwenders()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10); // teilt sich das Feld
        var attacker = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(ally);
        state.AddUnit(attacker);

        Assert.True(AbilitySystem.TryUseAbility(tank, new AbilityTarget.FieldTarget(tank.Position), state));
        Assert.Single(state.GetEffectsOnField(tank.Position)); // EINE Instanz für alle, nicht je Einheit eine

        // Der Zug des Verbündeten (ebenfalls "betroffen") zählt die Dauer NICHT herunter.
        EffectSystem.TickUnitEffects(state, ally);
        Assert.Single(state.GetEffectsOnField(tank.Position));

        int allyHpBefore = ally.CurrentHitPoints;
        Assert.True(attacker.TryAttack(ally, state));
        Assert.Equal(allyHpBefore - attacker.BaseAttackDamage / 2, ally.CurrentHitPoints);

        // Der Zug des Anwenders beendet den Schutz für alle zugleich.
        EffectSystem.TickUnitEffects(state, tank);
        Assert.Empty(state.GetEffectsOnField(tank.Position));

        allyHpBefore = ally.CurrentHitPoints;
        int tankHpBefore = tank.CurrentHitPoints;
        Assert.True(attacker.TryAttack(ally, state));
        Assert.True(attacker.TryAttack(tank, state));
        Assert.Equal(allyHpBefore - attacker.BaseAttackDamage, ally.CurrentHitPoints); // voller Schaden
        Assert.Equal(tankHpBefore - attacker.BaseAttackDamage, tank.CurrentHitPoints);
    }

    [Fact]
    public void Kampflied_WirktAufAlleVerbuendetenUndEndetFuerAlleGleichzeitigAmNaechstenZugDesMusikers()
    {
        var state = NewState();
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Musiker);
        var allyA = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var allyB = new TestUnit(Id(3), Team.TeamA, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(musiker);
        state.AddUnit(allyA);
        state.AddUnit(allyB);
        var kampflied = musiker.Definition.Ability.Modes!.Single(m => m.Id == "kampflied");

        Assert.True(AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied));
        Assert.All(new Unit[] { allyA, allyB }, u => Assert.True(EffectiveStats.AttackDamage(u, state) > u.BaseAttackDamage));

        EffectSystem.TickUnitEffects(state, allyA); // Züge der Verbündeten zählen nicht: beide noch gebufft
        EffectSystem.TickUnitEffects(state, allyB);
        Assert.All(new Unit[] { allyA, allyB }, u => Assert.True(EffectiveStats.AttackDamage(u, state) > u.BaseAttackDamage));

        EffectSystem.TickUnitEffects(state, musiker); // nächster Zug des Musikers: Dauer 1 -> 0 (seit "Echte Pausen"), BEIDE enden im selben Tick
        Assert.All(new Unit[] { allyA, allyB }, u => Assert.Equal(u.BaseAttackDamage, EffectiveStats.AttackDamage(u, state)));
        Assert.Empty(state.ActiveEffects);
    }

    // -----------------------------------------------------------------
    // Verbrennung: der Schadens-Tick bleibt beim Ziel, nur die Restlaufzeit
    // hängt am Anwender - Gesamtschaden unabhängig von der Reihenfolge.
    // -----------------------------------------------------------------

    [Theory]
    [InlineData(true)]  // Anwender handelt in jeder Runde VOR dem Ziel
    [InlineData(false)] // Anwender handelt in jeder Runde NACH dem Ziel
    public void Verbrennung_SchadensTickBleibtBeimZiel_GesamtschadenIstUnabhaengigVonDerReihenfolge(bool anwenderZuerst)
    {
        var state = NewState();
        var mage = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(mage);
        state.AddUnit(enemy);
        var feuer = mage.Definition.Ability.Modes!.Single(m => m.Id == "feuer");

        var agent = new ScriptedAgent();
        agent.Script(mage, new TurnAction.UseAbility(new AbilityTarget.UnitTarget(enemy), feuer));
        var controller = new MatchController(state, agent, agent, seed: 1);

        for (int round = 1; round <= 6; round++)
        {
            var reihenfolge = anwenderZuerst ? new Unit[] { mage, enemy } : new Unit[] { enemy, mage };
            foreach (var unit in reihenfolge)
            {
                controller.ExecuteTurn(unit, agent, new Random(1), null, round, Team.TeamA);
            }
        }

        // Sofortschaden 30 + Ticks 30, 20, 10 (Duration 3 = drei Ticks am Ziel).
        Assert.Equal(enemy.MaxHitPoints - 30 - (30 + 20 + 10), enemy.CurrentHitPoints);
        Assert.Empty(state.GetEffectsOnUnit(enemy.Id));
    }

    // -----------------------------------------------------------------
    // Abklingzeit: hängt weiterhin an den Zügen des Anwenders, unabhängig
    // vom Ablauf des Effekts.
    // -----------------------------------------------------------------

    [Fact]
    public void Abklingzeit_ZaehltWeiterhinAnDenZuegenDesAnwendersUnabhaengigVomEffektAblauf()
    {
        var state = NewState();
        var tank = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Schildtraeger); // Dauer 1, Abklingzeit 2
        var other = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 5), speed: 10);
        state.AddUnit(tank);
        state.AddUnit(other);

        var agent = new ScriptedAgent();
        agent.Script(tank, new TurnAction.UseAbility(new AbilityTarget.FieldTarget(tank.Position)));
        var controller = new MatchController(state, agent, agent, seed: 1);

        controller.ExecuteTurn(tank, agent, new Random(1), null, roundNumber: 1, startingTeam: Team.TeamA);
        Assert.Single(state.GetEffectsOnField(tank.Position));
        Assert.Equal(2, tank.AbilityCooldownRemaining);

        controller.ExecuteTurn(tank, agent, new Random(1), null, roundNumber: 2, startingTeam: Team.TeamA);
        Assert.Empty(state.GetEffectsOnField(tank.Position)); // Effekt (Dauer 1) ist abgelaufen ...
        Assert.Equal(1, tank.AbilityCooldownRemaining);       // ... die Abklingzeit läuft davon unabhängig weiter

        controller.ExecuteTurn(tank, agent, new Random(1), null, roundNumber: 3, startingTeam: Team.TeamA);
        Assert.Null(tank.AbilityCooldownRemaining); // nach zwei eigenen Zügen wieder einsetzbar
    }

    // -----------------------------------------------------------------
    // Effekte ohne Dauer bleiben unberührt (Aufladen, Umklammerung).
    // -----------------------------------------------------------------

    [Fact]
    public void EffekteOhneDauer_BleibenVomDauerTickDesAnwendersUnberuehrt()
    {
        var state = NewState();
        var archer = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 0), DefaultHeroes.Bogenschuetze);
        var hugger = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Hugger);
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(archer);
        state.AddUnit(hugger);
        state.AddUnit(enemy);

        var laden = archer.Definition.Ability.Modes!.Single(m => m.Id == "laden");
        var greifen = hugger.Definition.Ability.Modes!.Single(m => m.Id == "greifen");
        Assert.True(AbilitySystem.TryUseAbility(archer, new AbilityTarget.UnitTarget(archer), state, laden));
        Assert.True(AbilitySystem.TryUseAbility(hugger, new AbilityTarget.UnitTarget(enemy), state, greifen));

        for (int i = 0; i < 5; i++)
        {
            EffectSystem.TickUnitEffects(state, archer);
            EffectSystem.TickUnitEffects(state, hugger);
        }

        var aufladen = Assert.Single(state.GetEffectsOnUnit(archer.Id));
        Assert.Null(aufladen.RemainingDuration);

        // Umklammerung: eine Instanz auf dem Ziel, eine auf dem Hugger selbst - beide unbegrenzt.
        var umklammerungAmZiel = Assert.Single(state.GetEffectsOnUnit(enemy.Id));
        var umklammerungAmHugger = Assert.Single(state.GetEffectsOnUnit(hugger.Id));
        Assert.Null(umklammerungAmZiel.RemainingDuration);
        Assert.Null(umklammerungAmHugger.RemainingDuration);
    }

    // -----------------------------------------------------------------
    // Tod des Anwenders: alle seine Effekte auf anderen Einheiten enden sofort.
    // -----------------------------------------------------------------

    [Fact]
    public void StirbtDerAnwender_EndenSeineBuffsAufVerbuendetenSofort()
    {
        var state = NewState();
        var musiker = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 2), DefaultHeroes.Musiker);
        var ally = new TestUnit(Id(2), Team.TeamA, new HexCoordinate(0, 1), speed: 10);
        var enemy = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10);
        state.AddUnit(musiker);
        state.AddUnit(ally);
        state.AddUnit(enemy);
        var kampflied = musiker.Definition.Ability.Modes!.Single(m => m.Id == "kampflied");
        AbilitySystem.TryUseAbility(musiker, new AbilityTarget.FieldTarget(musiker.Position), state, kampflied);
        Assert.NotEmpty(state.GetEffectsOnUnit(ally.Id));

        CombatSystem.ApplyDamageAndHandleDefeat(state, enemy, musiker, 9999);

        Assert.True(musiker.IsDefeated);
        Assert.Empty(state.GetEffectsOnUnit(ally.Id)); // kein Warten auf einen Dauer-Tick, den es nie mehr gäbe
        Assert.Equal(ally.BaseAttackDamage, EffectiveStats.AttackDamage(ally, state));
    }

    [Fact]
    public void StirbtDerAnwender_HoertDieVerbrennungAufDemZielSofortAufZuTicken()
    {
        var state = NewState();
        var mage = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Elementarmagier);
        var enemy = new TestUnit(Id(2), Team.TeamB, new HexCoordinate(0, 2), speed: 10);
        state.AddUnit(mage);
        state.AddUnit(enemy);
        var feuer = mage.Definition.Ability.Modes!.Single(m => m.Id == "feuer");
        AbilitySystem.TryUseAbility(mage, new AbilityTarget.UnitTarget(enemy), state, feuer);
        Assert.NotEmpty(state.GetEffectsOnUnit(enemy.Id));

        CombatSystem.ApplyDamageAndHandleDefeat(state, enemy, mage, 9999);

        int hpBefore = enemy.CurrentHitPoints;
        EffectSystem.TickUnitEffects(state, enemy);
        Assert.Empty(state.GetEffectsOnUnit(enemy.Id));
        Assert.Equal(hpBefore, enemy.CurrentHitPoints); // kein weiterer Tick-Schaden
    }

    // -----------------------------------------------------------------
    // BEOBACHTUNGEN (Aufgabe 3, nur dokumentiert, NICHT geändert): Wand und
    // Schildträger auf demselben Feld, beide Fähigkeiten aktiv. Diese Tests
    // halten das AKTUELLE Verhalten der Vergeltungslogik fest - ändert der
    // angekündigte Folgeauftrag die Vergeltung, sollen sie das sichtbar
    // anzeigen und dann bewusst angepasst werden.
    // -----------------------------------------------------------------

    private static (GameState State, Hero Wand, Hero Schildtraeger, TestUnit Angreifer) NewWandUndSchildtraegerAufDemselbenFeld()
    {
        var state = NewState();
        var wand = new Hero(Id(1), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Wand);
        var schildtraeger = new Hero(Id(2), Team.TeamA, new HexCoordinate(0, 1), DefaultHeroes.Schildtraeger);
        var angreifer = new TestUnit(Id(3), Team.TeamB, new HexCoordinate(0, 3), speed: 10, attackRange: 2);
        state.AddUnit(wand);
        state.AddUnit(schildtraeger);
        state.AddUnit(angreifer);

        Assert.True(AbilitySystem.TryUseAbility(wand, new AbilityTarget.FieldTarget(wand.Position), state));
        Assert.True(AbilitySystem.TryUseAbility(schildtraeger, new AbilityTarget.FieldTarget(schildtraeger.Position), state));
        return (state, wand, schildtraeger, angreifer);
    }

    [Fact]
    public void Beobachtung_AngriffAufDenSchildtraegerWirdZurWandUmgeleitetUndLoestVergeltungAus()
    {
        var (state, wand, schildtraeger, angreifer) = NewWandUndSchildtraegerAufDemselbenFeld();

        bool attacked = CombatSystem.TryAttackDetailed(angreifer, schildtraeger, state, out var tatsaechlichesZiel, out int schadenBeiWand);

        Assert.True(attacked);
        Assert.Same(wand, tatsaechlichesZiel); // der Schaden landet bei der Wand ...
        Assert.Equal(schildtraeger.MaxHitPoints, schildtraeger.CurrentHitPoints); // ... nicht beim Schildträger
        // Beide Reduktionen wirken MULTIPLIKATIV (Schildwall-Feld 0,5 x Mauer-Selbstreduktion 0,75 = 0,375).
        Assert.Equal(0.375, EffectiveStats.IncomingDamageMultiplier(wand, state));
        // 100 x 0,375 = 37,5 - ein echter Rundungsfall (Bankers Rounding: auf die gerade Zahl 38).
        Assert.Equal(100, angreifer.BaseAttackDamage);
        Assert.Equal(38, schadenBeiWand);
        Assert.Equal(wand.MaxHitPoints - 38, wand.CurrentHitPoints);
        // Vergeltung: die Hälfte des URSPRÜNGLICHEN, unreduzierten Schadens an den Angreifer.
        Assert.Equal(angreifer.MaxHitPoints - angreifer.BaseAttackDamage / 2, angreifer.CurrentHitPoints);
    }

    [Fact]
    public void Beobachtung_DirekterAngriffAufDieWandLoestKeineVergeltungAusObwohlDieselbenReduktionenWirken()
    {
        var (state, wand, schildtraeger, angreifer) = NewWandUndSchildtraegerAufDemselbenFeld();

        bool attacked = CombatSystem.TryAttackDetailed(angreifer, wand, state, out var tatsaechlichesZiel, out int schadenBeiWand);

        Assert.True(attacked);
        Assert.Same(wand, tatsaechlichesZiel);
        Assert.Equal(38, schadenBeiWand); // identisch: 100 x 0,375 (Schildwall x Selbstreduktion) = 37,5 -> 38
        Assert.Equal(schildtraeger.MaxHitPoints, schildtraeger.CurrentHitPoints);
        // KEINE Vergeltung: ein Angriff, dessen Ziel bereits die Wand selbst ist, wird nicht "umgeleitet"
        // (CombatSystem.ResolveRedirect überspringt eine Umleitung auf das Ziel selbst) - die Vergeltung
        // hängt am UMLEITEN, nicht am Getroffenwerden der Wand.
        Assert.Equal(angreifer.MaxHitPoints, angreifer.CurrentHitPoints);
    }
}
