using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore.Agents;

/// <summary>
/// Ergebnis einer Zugbewertung, aufgeschlüsselt in alle Teilgrößen (alle in
/// Schadensäquivalenten, also in Lebenspunkten). Positive Werte sind gut für
/// das bewertende Team.
/// </summary>
/// <param name="DamageDealt">Zugefügter Schaden: Summe der verlorenen Lebenspunkte gegnerischer Einheiten.</param>
/// <param name="KillBonus">Bonus für besiegte gegnerische Einheiten (siehe <see cref="HeuristicAgentSettings.KillBonusRounds"/>).</param>
/// <param name="FutureOffense">Änderung der angekündigten offensiven Wirkungen: Schaden über Zeit auf Gegnern, Schadensbuffs und Angriffsboni auf eigenen Einheiten.</param>
/// <param name="AttackOpportunityGain">Zunahme der eigenen Angriffsmöglichkeit (Summe der eigenen Schadenswerte gegenüber Gegnern, die eine eigene Einheit im nächsten Zug erreichen kann), bereits gewichtet mit <see cref="HeuristicAgentSettings.AttackOpportunityWeight"/>.</param>
/// <param name="HitPointBalance">Änderung der eigenen Lebenspunkte: Heilung positiv, erlittener Schaden (z.B. Vergeltung) negativ, zuzüglich neu entstandener eigener Einheiten, abzüglich des Werts verlorener eigener Einheiten.</param>
/// <param name="DamagePreventedGain">NUR AUSGEWIESEN (seit Überarbeitung 6 nicht mehr Teil der Summe): Zunahme der abgefangenen Bedrohung = Bedrohungsverringerung minus Bedrohungsverringerung der ungeschützten Rechnung (alle Multiplikatoren 1). Bereits in <paramref name="ThreatReduction"/> enthalten.</param>
/// <param name="ThreatReduction">Verringerung der Bedrohung (Summe der Schadenswerte aller Gegner, die eine eigene Einheit im nächsten Zug erreichen können).</param>
/// <param name="FutureDefense">Änderung der angekündigten defensiven Wirkungen: Schwächung gegnerischer Schadenswerte, Schaden über Zeit auf eigenen Einheiten (negativ).</param>
public sealed record TurnScore(
    double DamageDealt,
    double KillBonus,
    double FutureOffense,
    double AttackOpportunityGain,
    double HitPointBalance,
    double DamagePreventedGain,
    double ThreatReduction,
    double FutureDefense)
{
    /// <summary>Offensiver Beitrag A = Schaden + Besiegt-Bonus + angekündigte Offensivwirkungen + Angriffsmöglichkeit.</summary>
    public double Offensive => DamageDealt + KillBonus + FutureOffense + AttackOpportunityGain;

    /// <summary>Defensiver Beitrag B = Lebenspunkte-Bilanz + Verringerung der (geschützt gerechneten) Bedrohung + angekündigte Defensivwirkungen.</summary>
    public double Defensive => HitPointBalance + ThreatReduction + FutureDefense; // Schutz steckt in ThreatReduction (Überarbeitung 6)

    /// <summary>Gesamtwert = w_off · A + w_def · B.</summary>
    public double Weighted(HeuristicWeights weights) => weights.Offensive * Offensive + weights.Defensive * Defensive;
}

/// <summary>
/// DIE GENERISCHE BEWERTUNGSFUNKTION beider heuristischer Agenten
/// (Bachelorarbeit, siehe <see cref="HeuristicAgent"/>). Bewertet wird der
/// Zustand NACH dem eigenen Zug im Vergleich zum Zustand davor - es werden
/// keine Folgezüge und keine gegnerischen Antworten simuliert.
///
/// GRUNDSATZ: Es gibt keine Regel für einen bestimmten Helden oder eine
/// bestimmte Fähigkeit. Die Bewertung liest ausschließlich die generischen
/// Größen des Spielzustands (Lebenspunkte, <see cref="EffectiveStats"/>,
/// Effektfelder von <see cref="ActiveEffect"/>). Jede Fähigkeit wird dadurch
/// über das bewertet, was sie im Zustand tatsächlich verändert - der Zug wird
/// dafür vorher auf einer Kopie ausgeführt (siehe <see cref="GameState.CloneForSimulation"/>).
///
/// Alle Wirkungen werden in SCHADENSÄQUIVALENTE (Lebenspunkte) übersetzt:
///
/// A) OFFENSIVER BEITRAG
///   - Zugefügter Schaden: verlorene Lebenspunkte gegnerischer Einheiten.
///   - Besiegt-Bonus: je besiegter gegnerischer Einheit deren UNGESCHWÄCHTER
///     Grundschaden × <see cref="HeuristicAgentSettings.KillBonusRounds"/>
///     (Überarbeitung 4, a).
///   - Angekündigte Wirkungen (Differenz vorher/nachher): Schaden über Zeit auf
///     Gegnern (Summe der verbleibenden, ggf. abklingenden Ticks, höchstens die
///     Rest-Lebenspunkte) und Einmal-Boni für den nächsten Angriff. (Schadensbuffs
///     zählen seit Überarbeitung 8 nur noch über die Angriffsmöglichkeit.) Wirkungen auf
///     Einheiten, die im bewerteten Zug besiegt werden, zählen weder vorher noch
///     nachher (Überarbeitung 4, b - dieselbe Regel wie bei der Angriffsmöglichkeit).
///   - ANGRIFFSMÖGLICHKEIT (Überarbeitung 2, Spiegelbild der Bedrohung; seit
///     Überarbeitung 6 je eigener Einheit EINMAL): für jede eigene Einheit (ohne
///     Klone) ihr effektiver Schaden × größter Schadensmultiplikator unter den
///     Gegnern (ohne Klone), die sie im nächsten Zug erreichen kann - gleiche
///     Reichweitenregel wie bei der Bedrohung. Positiv ist die
///     Zunahme nachher − vorher, gewichtet mit
///     <see cref="HeuristicAgentSettings.AttackOpportunityWeight"/>. Heranrücken
///     wird so offensiv belohnt, ohne Folgezüge durchzuspielen - es bleibt eine
///     Eigenschaft der Stellung. Gegner, die in DIESEM Zug besiegt werden,
///     zählen weder vorher noch nachher: sonst wäre der Wegfall ihrer
///     Angriffsmöglichkeit ein Verlust und der Besiegt-Bonus würde teilweise
///     aufgehoben.
///
/// B) DEFENSIVER BEITRAG
///   - Lebenspunkte-Bilanz eigener Einheiten (Heilung positiv, erlittener
///     Schaden negativ), abzüglich des Werts verlorener eigener Einheiten
///     (gleiche Formel wie der Besiegt-Bonus).
///   - VERRINGERUNG DER BEDROHUNG (seit Überarbeitung 6 ein einziger Wert, der
///     den Schutz enthält): jeder angriffsfähige Gegner (ohne Klone) geht GENAU
///     EINMAL ein, mit seinem effektiven Schaden × dem größten
///     Schadensmultiplikator m unter den eigenen Einheiten (ohne Klone), die er im
///     nächsten Zug erreichen und angreifen kann - er greift einmal an und wählt
///     das am wenigsten geschützte Ziel. Positiv ist die Differenz vorher −
///     nachher. Schutz (Schildwall, Mauer) zählt damit genau so viel, wie er
///     diesen Wert senkt - und nichts, solange kein Gegner in Reichweite steht
///     oder der Gegner noch ein ungeschütztes Ziel erreicht. Die ABGEFANGENE
///     Bedrohung wird zusätzlich als Differenz zur ungeschützten Rechnung
///     (alle m = 1) ausgewiesen (<see cref="TurnScore.DamagePreventedGain"/>),
///     geht aber nicht ein zweites Mal in die Summe ein.
///     UMLEITUNG (Mauer der Wand): steht eine eigene Einheit auf einem Feld,
///     dessen Effekt Angriffe auf einen lebenden Verbündeten umleitet, wird der
///     Schaden verschoben, nicht verhindert: er trifft das Umleitungsziel mit
///     dessen Multiplikator. Als m zählt dann min(eigener, der des
///     Umleitungsziels) - ein Gewinn entsteht nur, soweit das Umleitungsziel
///     weniger Schaden nimmt (z.B. durch seine Selbstreduktion). Berücksichtigt
///     wird nur die SICHERE Umleitung (Ziel steht auf dem Feld); die
///     angreiferabhängige Umleitung per Sichtlinie nicht, weil sie vom Standort
///     des Angreifers abhängt.
///     REICHWEITE (seit Überarbeitung 7 über die Wegsuche): eine Einheit erreicht
///     ein Ziel, wenn sie es von ihrem Standfeld oder einem im nächsten Zug
///     erreichbaren Feld aus in Angriffsreichweite hat - Feldsperren (Mauer)
///     und Bewegungsunfähigkeit sind dabei berücksichtigt; angriffsunfähige
///     Einheiten und gegen sie geschützte Ziele zählen nicht.
///   - Angekündigte Wirkungen: Schaden über Zeit auf eigenen Einheiten negativ.
///     (Schwächungen gegnerischer Schadenswerte zählen seit Überarbeitung 8 nur
///     noch über die Bedrohung.)
///
/// Angekündigter Schaden über Zeit OHNE feste Dauer zählt einen Tick, also nur
/// den nächsten Zug (Überarbeitung 9) - im aktuellen Katalog kommt das nicht vor
/// (die Verbrennung hat Dauer 3).
///
/// BEKANNTE GRENZEN (bewusst ohne Sonderregel belassen): Wirkungen auf die
/// EIGENE Bewegungsreichweite (Marschlied) und Klone (Illusionist) haben keinen
/// direkten Schadenswert und werden nur über ihre Lebenspunkte (Klon) bzw. gar
/// nicht bewertet. Schutzwirkungen, die zufällig auch Gegner auf demselben Feld
/// begünstigen, werden nicht abgezogen.
///
/// ERSTER ENTWURF UND ÜBERARBEITUNG (Befund zur Modellabhängigkeit): Der erste
/// Entwurf bewertete Schutz als Zugewinn an EFFEKTIVEN Lebenspunkten (HP / m −
/// HP, summiert über alle eigenen Einheiten) und kannte offensiv nur tatsächlich
/// zugefügten Schaden. Das ergab nahezu bewegungslose Agenten (87 bis 99 % der
/// Partien am Rundenlimit), aus zwei Gründen:
///   1. Skalenproblem: effektive Lebenspunkte sind ein BESTAND, zugefügter
///      Schaden ein FLUSS je Zug. Mauer (+1000) und Schildwall (+850)
///      übertrafen jeden Angriff (70 bis 120) um ein Vielfaches, auch bei 2:1 -
///      die Fähigkeit gewann immer, ob ein Gegner in der Nähe war oder nicht.
///   2. Heranrücken hatte keinen offensiven Wert, weil die Startblöcke zu weit
///      auseinander liegen, um im selben Zug anzugreifen; es erhöhte aber die
///      Bedrohung und war dadurch negativ. Also blieben alle stehen.
/// Überarbeitung 1 misst Schutz deshalb als Fluss (abgefangener Anteil der
/// aktuellen Bedrohung), Überarbeitung 2 ergänzt die Angriffsmöglichkeit als
/// Gegenstück zur Bedrohung. Zugefügter, abgewehrter und geheilter Schaden
/// stehen damit in derselben Größenordnung (Schaden je Runde).
///
/// ÜBERARBEITUNG 3 (Grundgewicht der Angriffsmöglichkeit, siehe
/// <see cref="HeuristicAgentSettings.AttackOpportunityWeight"/>): mit dem
/// Spiegelbild 1:1 steigen beim Heranrücken Angriffsmöglichkeit und Bedrohung um
/// denselben Betrag X. Das defensive Profil (1:2) bewertete Heranrücken damit
/// IMMER mit −X, also schlechter als Stehenbleiben - der Agent blieb in 83 % der
/// Partien bis zum Rundenlimit stehen. Das Grundgewicht 2,0 wirkt VOR der
/// Profil-Gewichtung und für beide Agenten gleich: symmetrisches Heranrücken ist
/// beim defensiven Profil neutral, die übrigen Terme entscheiden.
///
/// ÜBERARBEITUNG 4 (Fehlerkorrektur: Besiegen eines geschwächten Gegners): nach
/// Überarbeitung 3 blieb der defensive Agent passiv (72,5 % Rundenlimit). Die
/// Diagnose zeigte rund 5.000 Züge, in denen ein Angriff den Gegner besiegt
/// hätte, aber negativ bewertet wurde. Zwei Teilprobleme wirkten zusammen, am
/// deutlichsten bei der Umklammerung des Huggers (Gegner-Schaden −100, meist auf 0):
///   a) Der Besiegt-Bonus hing am EFFEKTIVEN Schaden - ein auf 0 geschwächter
///      Gegner war nichts wert. Jetzt zählt der ungeschwächte Grundschaden.
///      Begründung: Besiegen beendet eine Einheit dauerhaft, maßgeblich ist daher
///      ihr dauerhafter Kampfwert, nicht der Wert unter vorübergehenden Effekten.
///      Das wirkt in beide Richtungen - eine Schwächung senkt ihn nicht, ein
///      vorübergehender Buff bläht ihn nicht auf; die Effekte selbst sind bereits
///      über Bedrohung und angekündigte Wirkungen bewertet. (Verworfen:
///      max(effektiv, Grundwert) - zählte Buffs, aber keine Schwächungen.)
///      Folge: auch die Berserker-Passive (Schadensbonus bei niedrigen HP) geht
///      nicht mehr in den Besiegt-Bonus ein - folgerichtig, sie ist ebenfalls ein
///      vorübergehender Zustand.
///   b) Die angekündigte Schwächung zählte als künftiger Nutzen, entfiel mit dem
///      Tod des Gegners und wurde so als Verlust gebucht (beim defensiven Profil
///      doppelt gewichtet). Jetzt zählen angekündigte Wirkungen auf Einheiten, die
///      im bewerteten Zug besiegt werden, in keinem der beiden Zustände - dieselbe
///      Regel wie bei der Angriffsmöglichkeit, für eigene wie gegnerische Einheiten.
///
/// (Schritt 5 der Überarbeitung - "Umklammerung prozentual" - betraf nicht diese
/// Funktion, sondern den Helden; siehe DefaultHeroes.Hugger.)
///
/// ÜBERARBEITUNG 6 (Mehrfachzählung der Bedrohung): Bedrohung und
/// Angriffsmöglichkeit waren Summen über PAARE aus eigener Einheit und Gegner. Ein
/// Gegner, der drei eigene Einheiten erreicht, ging dreifach ein, obwohl er je Zug
/// nur einmal angreift; rückte eine Einheit in die Reichweite eines Gegners, der
/// schon andere bedrohte, wurde sein voller Schaden erneut gebucht. Das war laut
/// Diagnose die Hauptursache der Passivität des defensiven Agenten. Jetzt zählt
/// jeder Gegner einmal - gegen das am wenigsten geschützte erreichbare Ziel - und
/// spiegelbildlich jede eigene Einheit einmal. Die bisherige Aufteilung in
/// Bedrohung und abgefangene Bedrohung (Überarbeitung 1) entfällt zugunsten
/// dieses einen Werts; die abgefangene Bedrohung wird nur noch ausgewiesen.
/// Folge für den Schutz: ein Schildwall zählt nur, soweit er ALLE Ziele deckt, die
/// ein Gegner erreicht (sonst wählt der Gegner das ungeschützte).
///
/// ÜBERARBEITUNG 7 (gesperrte Felder): die Reichweite war die reine Hex-Distanz
/// (Bewegungs- + Angriffsreichweite). Ein Gegner galt auch dann als bedrohlich,
/// wenn ihm die Mauer den Weg versperrte. Jetzt wird über die bestehende
/// Wegsuche geprüft, ob er von einem erreichbaren Feld oder seinem Standfeld aus
/// in Angriffsreichweite kommt. Eine Mauer, die Wege abschneidet, senkt damit die
/// Bedrohung tatsächlich. Für die Angriffsmöglichkeit gilt dieselbe Regel.
///
/// ÜBERARBEITUNG 8 (Doppelzählung von Schadensmodifikatoren): Effekte, die den
/// Angriffsschaden verändern, gingen zweimal ein - als angekündigte Wirkung über
/// ihre Restdauer (bei unbegrenzter Dauer über den Planungshorizont von 2 Runden)
/// und über den effektiven Schaden in Bedrohung und Angriffsmöglichkeit. Betroffen:
/// Umklammerung (eigene Halbierung ×0,5, Bonus für Verbündete ×1,33, Schwächung
/// des Gegners ×0,67) und Kampflied. Beim angehefteten Hugger etwa wurde die
/// Halbierung des eigenen Schadens einmal als angekündigter Verlust (−60) und
/// einmal als geringere Angriffsmöglichkeit (−60) gebucht. Die angekündigte
/// Buchung entfällt; es bleibt die Wirkung über Bedrohung und Angriffsmöglichkeit.
/// Begründung: dasselbe Prinzip wie bei der Schadensreduktion (Überarbeitung 1) -
/// eine Wirkung zählt nur, wenn sie in der aktuellen Lage tatsächlich greift; die
/// angekündigte Buchung zählte auch ohne jede Einheit in Reichweite (etwa ein
/// Kampflied auf einen weit entfernten Verbündeten). Folge: die Wirkung zählt nur
/// noch für den nächsten Zug statt über die volle Dauer. Nicht betroffen:
/// Bewegungseffekte (wirken nur über die Reichweite), Schaden über Zeit und
/// Aufladen (gehen nur als angekündigte Wirkung ein), die Berserker-Passive (kein Effekt).
///
/// ÜBERARBEITUNG 9 (Planungshorizont entfernt): der Planungshorizont von 2 Runden
/// (HeuristicAgentSettings.PlanningHorizonRounds) war nirgends begründet. Nach
/// Überarbeitung 8 las ihn nur noch der Schaden über Zeit ohne feste Dauer - ein
/// Fall, den der Katalog nicht enthält. Er ist ersetzt durch die Regel: Schaden über
/// Zeit ohne feste Dauer zählt einen Tick (nächster Zug), derselbe Horizont wie
/// Bedrohung und Angriffsmöglichkeit. Verhaltensneutral (Stichprobe bitgleich).
/// </summary>
public static class TurnEvaluator
{
    /// <summary>Bewertet den Übergang <paramref name="before"/> → <paramref name="after"/> aus Sicht von <paramref name="team"/>.</summary>
    public static TurnScore Evaluate(GameState before, GameState after, Team team) =>
        new DecisionContext(before, team).Evaluate(after);

    /// <summary>
    /// LAUFZEIT-OPTIMIERUNG (verhaltensneutral): innerhalb EINER Entscheidung ist der
    /// Ausgangszustand für alle geprüften Zugfolgen derselbe. Der Kontext berechnet
    /// seine Momentaufnahme deshalb nur einmal und verwendet sie wieder - je Menge
    /// der im Zug besiegten Einheiten, weil Angriffsmöglichkeit und angekündigte
    /// Wirkungen davon abhängen (fast immer die leere Menge). Die Rechnung selbst
    /// ist unverändert; nachgewiesen durch bitgleiche Stichproben vor und nach der
    /// Optimierung. Der Ausgangszustand darf sich während der Nutzung nicht ändern
    /// (der Agent simuliert ausschließlich auf Kopien).
    /// </summary>
    public sealed class DecisionContext(GameState before, Team team)
    {
        private readonly Dictionary<string, Snapshot> _beforeSnapshots = new();

        public TurnScore Evaluate(GameState after) => EvaluateCore(before, after, team, this);

        internal Snapshot BeforeSnapshot(IReadOnlySet<Guid> defeatedThisTurn)
        {
            string key = defeatedThisTurn.Count == 0 ? string.Empty : string.Join(",", defeatedThisTurn.OrderBy(id => id));
            if (!_beforeSnapshots.TryGetValue(key, out var snapshot))
            {
                snapshot = Snapshot.Of(before, team, defeatedThisTurn);
                _beforeSnapshots[key] = snapshot;
            }

            return snapshot;
        }
    }

    private static TurnScore EvaluateCore(GameState before, GameState after, Team team, DecisionContext context)
    {
        // Einheiten, die in DIESEM Zug besiegt werden: ihre angekündigten
        // Wirkungen zählen in keinem der beiden Zustände (Überarbeitung 4, b).
        var aliveAfter = after.Units.Where(u => !u.IsDefeated).Select(u => u.Id).ToHashSet();
        var defeatedThisTurn = before.Units.Where(u => !u.IsDefeated && !aliveAfter.Contains(u.Id)).Select(u => u.Id).ToHashSet();

        var b = context.BeforeSnapshot(defeatedThisTurn);
        var a = Snapshot.Of(after, team, defeatedThisTurn);

        double damageDealt = 0, killBonus = 0;
        foreach (var (id, enemy) in b.Enemies)
        {
            int hpAfter = a.Enemies.TryGetValue(id, out var e) ? e.HitPoints : 0;
            damageDealt += enemy.HitPoints - hpAfter;
            if (hpAfter <= 0)
            {
                killBonus += enemy.UnitValue;
            }
        }

        double hitPointBalance = 0;
        foreach (var (id, own) in b.Own)
        {
            int hpAfter = a.Own.TryGetValue(id, out var o) ? o.HitPoints : 0;
            hitPointBalance += hpAfter - own.HitPoints;
            if (hpAfter <= 0)
            {
                hitPointBalance -= own.UnitValue;
            }
        }

        foreach (var (id, own) in a.Own)
        {
            if (!b.Own.ContainsKey(id))
            {
                hitPointBalance += own.HitPoints; // neu entstandene eigene Einheit (Klon)
            }
        }

        // Angriffsmöglichkeit: in diesem Zug besiegte Gegner sind in beiden
        // Zuständen als Ziel ausgenommen (kein Verlust an Möglichkeit, siehe Snapshot).
        double threatReduction = b.Threat - a.Threat;
        double unprotectedThreatReduction = b.UnprotectedThreat - a.UnprotectedThreat;

        return new TurnScore(
            DamageDealt: damageDealt,
            KillBonus: killBonus,
            FutureOffense: a.FutureOffense - b.FutureOffense,
            AttackOpportunityGain: HeuristicAgentSettings.AttackOpportunityWeight * (a.Opportunity - b.Opportunity),
            HitPointBalance: hitPointBalance,
            DamagePreventedGain: threatReduction - unprotectedThreatReduction, // nur ausgewiesen, steckt bereits in ThreatReduction
            ThreatReduction: threatReduction,
            FutureDefense: a.FutureDefense - b.FutureDefense);
    }

    internal readonly record struct UnitSnapshot(int HitPoints, double UnitValue);

    /// <summary>Alle für die Bewertung nötigen Größen EINES Zustands aus Sicht eines Teams.</summary>
    internal sealed class Snapshot
    {
        public Dictionary<Guid, UnitSnapshot> Own { get; } = new();
        public Dictionary<Guid, UnitSnapshot> Enemies { get; } = new();

        /// <summary>Angriffsmöglichkeit: je eigener Einheit ihr Schaden × größter Multiplikator unter den erreichbaren Gegnern (Überarbeitung 6).</summary>
        public double Opportunity { get; private set; }

        /// <summary>Bedrohung: je Gegner sein Schaden × größter Multiplikator unter den erreichbaren eigenen Einheiten (Überarbeitung 6).</summary>
        public double Threat { get; private set; }

        /// <summary>Dieselbe Bedrohung ohne Schadensreduktion (Multiplikator 1) - nur zum Ausweisen der abgefangenen Bedrohung.</summary>
        public double UnprotectedThreat { get; private set; }
        public double FutureOffense { get; private set; }
        public double FutureDefense { get; private set; }

        /// <param name="excludedEffectTargets">Einheiten, deren angekündigte Wirkungen nicht zählen (im bewerteten Zug besiegt).</param>
        public static Snapshot Of(GameState state, Team team, IReadOnlySet<Guid> excludedEffectTargets)
        {
            var s = new Snapshot();
            foreach (var unit in state.Units)
            {
                if (unit.IsDefeated)
                {
                    continue;
                }

                var entry = new UnitSnapshot(unit.CurrentHitPoints, UnitValue(unit));
                (unit.Team == team ? s.Own : s.Enemies)[unit.Id] = entry;
            }

            var reach = new ReachCache(state);
            var ownHeroes = state.Units.Where(u => !u.IsDefeated && !u.IsClone && u.Team == team).ToList();
            var enemyHeroes = state.Units.Where(u => !u.IsDefeated && !u.IsClone && u.Team != team).ToList();

            // Bedrohung: jeder angriffsfähige Gegner GENAU EINMAL, gegen das am
            // wenigsten geschützte eigene Ziel, das er erreicht.
            foreach (var enemy in enemyHeroes)
            {
                if (WorstReachableMultiplier(enemy, ownHeroes, state, reach) is double m)
                {
                    double damage = EffectiveStats.AttackDamage(enemy, state);
                    s.Threat += damage * m;
                    s.UnprotectedThreat += damage;
                }
            }

            // Angriffsmöglichkeit spiegelbildlich: jede eigene Einheit genau einmal;
            // in diesem Zug besiegte Gegner zählen in keinem Zustand als Ziel.
            var enemyTargets = enemyHeroes.Where(u => !excludedEffectTargets.Contains(u.Id)).ToList();
            foreach (var own in ownHeroes)
            {
                if (WorstReachableMultiplier(own, enemyTargets, state, reach) is double m)
                {
                    s.Opportunity += EffectiveStats.AttackDamage(own, state) * m;
                }
            }

            s.AddAnnouncedEffects(state, team, excludedEffectTargets);
            return s;
        }

        /// <summary>
        /// Größter effektiver Schadensmultiplikator unter den <paramref name="targets"/>,
        /// die <paramref name="attacker"/> im nächsten Zug erreichen und angreifen kann
        /// (das am wenigsten geschützte Ziel); null, wenn er keines erreicht oder nicht
        /// angreifen kann.
        /// </summary>
        private static double? WorstReachableMultiplier(Unit attacker, IReadOnlyList<Unit> targets, GameState state, ReachCache reach)
        {
            if (EffectiveStats.IsAttackingPrevented(attacker, state))
            {
                return null;
            }

            double? worst = null;
            foreach (var target in targets)
            {
                if (!EffectiveStats.PreventsBeingTargetedBy(target, attacker.Team, state) && reach.CanReach(attacker, target))
                {
                    worst = Math.Max(worst ?? double.MinValue, reach.Multiplier(target));
                }
            }

            return worst;
        }

        /// <summary>
        /// Wert einer Einheit für Besiegt-Bonus/-Verlust: UNGESCHWÄCHTER
        /// Grundschaden (<see cref="Unit.BaseAttackDamage"/>) über
        /// <see cref="HeuristicAgentSettings.KillBonusRounds"/> Runden (Überarbeitung
        /// 4, a - vorher der effektive Schaden, siehe Klassendokumentation).
        /// </summary>
        private static double UnitValue(Unit unit) =>
            unit.IsClone ? 0 : (double)unit.BaseAttackDamage * HeuristicAgentSettings.KillBonusRounds;

        /// <summary>
        /// Anteil eines Treffers, der bei der Einheit bzw. ihrem Team tatsächlich
        /// ankommt: der eigene Schadensmultiplikator, bei sicherer Umleitung der
        /// kleinere von eigenem und dem des Umleitungsziels (der Schaden wird
        /// verschoben, nicht verhindert).
        /// </summary>
        private static double EffectiveMultiplier(Unit unit, GameState state)
        {
            double m = Multiplier(unit, state);

            foreach (var effect in state.GetEffectsOnField(unit.Position))
            {
                if (!effect.RedirectsAttacksToSource || effect.SourceUnitId == unit.Id)
                {
                    continue;
                }

                var absorber = state.Units.FirstOrDefault(u => u.Id == effect.SourceUnitId);
                if (absorber is not null && !absorber.IsDefeated && absorber.Team == unit.Team)
                {
                    m = Math.Min(m, Multiplier(absorber, state));
                }
            }

            return m;
        }

        private static double Multiplier(Unit unit, GameState state) =>
            Math.Max(0.0, EffectiveStats.IncomingDamageMultiplier(unit, state));

        /// <summary>
        /// Ob eine Einheit ein Ziel im nächsten Zug erreichen und angreifen kann
        /// (Überarbeitung 7): sie kann von ihrem Standfeld oder einem über die
        /// bestehende Wegsuche erreichbaren Feld aus (<see cref="Unit.GetLegalMoves"/> -
        /// beachtet Bewegungsreichweite, Bewegungsunfähigkeit und Feldsperren wie
        /// die Mauer) das Ziel in Angriffsreichweite nehmen. Die erreichbaren Felder
        /// werden EINMAL je Einheit und Zustand berechnet, nicht je Paar.
        /// </summary>
        private sealed class ReachCache(GameState state)
        {
            private readonly Dictionary<Guid, IReadOnlyList<HexCoordinate>> _fields = new();
            private readonly Dictionary<Guid, double> _multipliers = new();
            private readonly Dictionary<Team, bool> _lockedAgainst = new();

            public bool CanReach(Unit attacker, Unit target)
            {
                if (!_fields.TryGetValue(attacker.Id, out var fields))
                {
                    fields = ReachableFields(attacker);
                    _fields[attacker.Id] = fields;
                }

                int range = EffectiveStats.AttackRange(attacker, state);
                foreach (var field in fields)
                {
                    if (HexCoordinate.Distance(field, target.Position) <= range)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>Effektiver Schadensmultiplikator je Einheit, einmal je Zustand berechnet.</summary>
            public double Multiplier(Unit unit)
            {
                if (!_multipliers.TryGetValue(unit.Id, out double m))
                {
                    m = EffectiveMultiplier(unit, state);
                    _multipliers[unit.Id] = m;
                }

                return m;
            }

            /// <summary>
            /// Standfeld plus alle im nächsten Zug erreichbaren Felder - inhaltlich
            /// genau <see cref="Unit.GetLegalMoves"/>. LAUFZEIT: die Wegsuche hängt nur
            /// vom Brett, der Bewegungsreichweite, der Bewegungsunfähigkeit und den
            /// Feldsperren ab (Einheiten blockieren nicht, <see cref="OccupancyRules.UnitsBlockMovement"/>).
            /// Liegt keine Sperre gegen das Team der Einheit, sind die erreichbaren Felder
            /// daher genau die Brettfelder mit Wegdistanz ≤ Bewegungsreichweite - die
            /// Wegdistanzen des festen Bretts werden einmal vorab berechnet
            /// (<see cref="BoardDistances"/>). Nur mit Sperre läuft die volle Wegsuche.
            /// </summary>
            private IReadOnlyList<HexCoordinate> ReachableFields(Unit unit)
            {
                if (EffectiveStats.IsMovementPrevented(unit, state))
                {
                    return new[] { unit.Position };
                }

                if (OccupancyRules.UnitsBlockMovement || HasLockAgainst(unit.Team))
                {
                    var moves = unit.GetLegalMoves(state);
                    return moves.Count == 0 ? new[] { unit.Position } : moves;
                }

                int range = EffectiveStats.MovementRange(unit, state);
                return BoardDistances.From(state.Board, unit.Position).Where(kv => kv.Value <= range).Select(kv => kv.Key).ToList();
            }

            private bool HasLockAgainst(Team team)
            {
                if (!_lockedAgainst.TryGetValue(team, out bool locked))
                {
                    locked = state.ActiveEffects.Any(e => e.FieldLockAgainstTeam == team);
                    _lockedAgainst[team] = locked;
                }

                return locked;
            }
        }

        /// <summary>
        /// Wegdistanzen zwischen allen Feldern eines (unveränderlichen) Bretts ohne
        /// Sperren, einmal je Brett berechnet (Breitensuche über Nachbarfelder, wie
        /// <see cref="Movement.MovementSystem"/>). Threadsicher über ConditionalWeakTable.
        /// </summary>
        private static class BoardDistances
        {
            private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Board.Board, Dictionary<HexCoordinate, Dictionary<HexCoordinate, int>>> Cache = new();

            public static Dictionary<HexCoordinate, int> From(Board.Board board, HexCoordinate start) =>
                Cache.GetValue(board, Compute).TryGetValue(start, out var d) ? d : new Dictionary<HexCoordinate, int> { [start] = 0 };

            private static Dictionary<HexCoordinate, Dictionary<HexCoordinate, int>> Compute(Board.Board board)
            {
                var all = new Dictionary<HexCoordinate, Dictionary<HexCoordinate, int>>();
                foreach (var field in board.Fields)
                {
                    var distances = new Dictionary<HexCoordinate, int> { [field.Coordinate] = 0 };
                    var queue = new Queue<HexCoordinate>();
                    queue.Enqueue(field.Coordinate);
                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        foreach (var neighbor in current.GetNeighbors())
                        {
                            if (board.Contains(neighbor) && !distances.ContainsKey(neighbor))
                            {
                                distances[neighbor] = distances[current] + 1;
                                queue.Enqueue(neighbor);
                            }
                        }
                    }

                    all[field.Coordinate] = distances;
                }

                return all;
            }
        }

        private void AddAnnouncedEffects(GameState state, Team team, IReadOnlySet<Guid> excludedEffectTargets)
        {
            foreach (var effect in state.ActiveEffects)
            {
                if (effect.AnchorType != EffectAnchorType.Unit || effect.TargetUnitId is not Guid targetId)
                {
                    continue;
                }

                var target = state.Units.FirstOrDefault(u => u.Id == targetId);
                if (target is null || target.IsDefeated || excludedEffectTargets.Contains(target.Id))
                {
                    continue;
                }

                bool own = target.Team == team;
                // Schaden über Zeit ohne feste Dauer zählt EINEN Tick, also nur den
                // nächsten Zug (Überarbeitung 9) - derselbe Horizont wie Bedrohung und
                // Angriffsmöglichkeit.
                int rounds = effect.RemainingDuration ?? 1;

                if (effect.DamagePerTick is int tick)
                {
                    double total = 0;
                    int reduction = effect.DamagePerTickReductionPerTick ?? 0;
                    for (int k = 0; k < rounds; k++)
                    {
                        total += Math.Max(0, tick - k * reduction);
                    }

                    total = Math.Min(total, target.CurrentHitPoints);
                    if (own) FutureDefense -= total; else FutureOffense += total;
                }

                // KEINE Schadensmodifikatoren mehr (Überarbeitung 8): Buffs und
                // Schwächungen des Angriffsschadens (Kampflied, Umklammerung) wirken
                // bereits über den effektiven Schaden in Bedrohung und
                // Angriffsmöglichkeit - hier gebucht, zählten sie doppelt.

                if (effect.BonusDamageForNextAttack is int bonus)
                {
                    if (own) FutureOffense += bonus; else FutureDefense -= bonus;
                }
            }
        }
    }
}
