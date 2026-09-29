using TacticsCore.Abilities;
using TacticsCore.Effects;

namespace TacticsCore.Heroes;

/// <summary>
/// Alle neun Standardhelden (je drei Tank, Support, Dmg) als Datensätze
/// über <see cref="HeroDefinition"/> - reine Daten, kein Held bringt
/// eigenen Code mit. Jede Fähigkeit ist ausschließlich über bestehende
/// <see cref="EffectTemplate"/>-Felder bzw. den <see cref="AbilityMode"/>-
/// Mechanismus beschrieben; wo eine Mechanik im System noch fehlte, wurde
/// sie GENERISCH ergänzt (siehe Kommentare bei den jeweils betroffenen
/// Feldern in <see cref="EffectTemplate"/>, <see cref="StackingRule"/>,
/// <see cref="AbilityMode"/> und <see cref="AreaTargetFilter"/>) - nirgends
/// eine Held-spezifische Sonderfallprüfung im Interpreter selbst.
///
/// ATTRIBUTWERTE SIND AUSDRÜCKLICH VORLÄUFIG: HP, Schaden, Geschwindigkeit
/// sowie Bewegungs-/Angriffsreichweite aller neun Helden sind erste,
/// plausible Platzhalter und werden über Simulationsläufe (siehe die
/// Plausibilitätsprüfungen im Konsolenprogramm, insbesondere den
/// "full-baseline"-Modus) ausbalanciert - ein erster, datengestützter
/// Balancing-Durchgang auf Basis eines vollständigen Basislaufs (alle 84
/// Kompositionen, 3.570 Paarungen, 257.040 Partien mit Zufallsagenten) ist
/// bereits erfolgt (siehe die jeweiligen "BALANCING"-Absätze bei den
/// betroffenen Helden unten); weitere Durchgänge nach erneuten Basisläufen
/// sind ausdrücklich vorgesehen, nicht ausgeschlossen. Die beiden folgenden
/// Regeln sind dabei aber bewusst bereits jetzt FEST vorgegeben, nicht
/// selbst Gegenstand der späteren Feinabstimmung:
///
/// 1. GESCHWINDIGKEIT PAARWEISE VERSCHIEDEN: alle neun Werte sind über den
///    GESAMTEN Katalog hinweg eindeutig, nach Rolle gestaffelt (Tank 4-6,
///    Support 10-12, Dmg 13-15) - <see cref="HeroCatalog"/> erzwingt das bei
///    JEDER Katalogerzeugung zur Laufzeit (auch bei abgewandelten
///    Balancing-Varianten). Grund: Geschwindigkeit bestimmt die
///    Zugreihenfolge innerhalb eines Teams (siehe <see cref="Match.TurnOrder"/>);
///    bei einem Gleichstand entschiede stattdessen die UnitId, wodurch die
///    Reihenfolge von der (an sich beliebigen) Aufstellungsreihenfolge
///    abhinge statt allein von den Heldenwerten.
///
/// 2. ROLLEN-KORRIDORE FÜR HP UND SCHADEN: damit sich die drei Rollen auch
///    MECHANISCH (nicht nur nominell über <see cref="HeroRole"/>, die laut
///    Anforderung ja explizit KEIN System beeinflussen darf) klar
///    unterscheiden - Grundlage für die spätere Untersuchung, ob sich die
///    Rolle systematisch auf die Balance auswirkt:
///
///      Tank:    HP 750-1000, Grundschaden 50-70   (siehe TankMinHp/TankMaxHp/...; ×10 skaliert, siehe Punkt 3)
///      Support: HP 450-550,  Grundschaden 80-100  (siehe SupportMinHp/...)
///      Dmg:     HP 280-450,  Grundschaden 80-150  (siehe DmgMinHp/...)
///
///    Zusätzlich gilt weiterhin: ein Fernkämpfer hat WENIGER HP als ein
///    Nahkämpfer DERSELBEN Rolle (Elfe &lt; Musiker/Hugger; Elementarmagier
///    und Bogenschütze &lt; Berserker). Diese beiden Regeln sind bewusst nur
///    als Test abgesichert (siehe HeroCatalogTests/DefaultHeroesTests),
///    nicht als Konstruktor-Zwang wie die Geschwindigkeit: ein abgewandelter
///    Katalog (z.B. eine gezielte "+50% Schaden"-Testvariante) darf einen
///    Korridor bewusst verlassen, ohne dass die Katalogerzeugung selbst
///    dafür fehlschlägt.
///
/// 3. FAKTOR-10-SKALIERUNG ALLER KAMPFWERTE (Bachelorarbeit-Korrektur,
///    Designentscheidung): ALLE Werte, die mit Schaden oder Lebenspunkten
///    verrechnet werden, sind gegenüber einer früheren Katalogfassung um den
///    Faktor 10 angehoben (aus 80 HP wurden 800, aus 8 Schaden 80, ...) -
///    das Spielverhalten ist dabei nachweislich (siehe Verifikation unten)
///    exakt identisch geblieben, nur die Zahlen selbst sind größer.
///
///    GRUND: bei den ursprünglich einstelligen/zweistelligen Ganzzahlwerten
///    wirkten kleine, für die Untersuchung gerade interessante Prozent-
///    änderungen NICHT messbar - <see cref="HeroCatalogVariants.WithAttributePercentageChange"/>
///    rundet ganzzahlig (<c>Math.Round</c>), und z.B. 5% auf 8 Schaden
///    ergeben 8,4, was auf 8 zurückgerundet wird: keine tatsächliche
///    Änderung, obwohl 5% als Untersuchungsgröße explizit gewünscht war.
///    Bei 80 Schaden (×10) ergeben dieselben 5% dagegen 84 - eine echte,
///    messbare Änderung. Die Rundungsformel selbst wurde NICHT verändert
///    (unverändert generisch, unverändert prozentual) - nur ihre
///    Eingabewerte sind jetzt groß genug, damit +1 bereits einem kleineren,
///    sinnvoll grob gerundeten Prozentsatz entspricht.
///
///    WAS SKALIERT WURDE: <see cref="HeroDefinition.MaxHitPoints"/> und
///    <see cref="HeroDefinition.BaseAttackDamage"/> aller neun Helden, die
///    Rollen-Korridore (Punkt 2 oben), alle Schadens-/Heil-/Tick-Werte und
///    deren Distanzabfall/Minimum in <see cref="EffectTemplate"/>-Instanzen,
///    additive Schadensboni aus Buffs/Debuffs (<see cref="StatModifierSet.BaseAttackDamageDelta"/>),
///    die Aufladewerte des Bogenschützen (<see cref="BogenschuetzeLadungProRunde"/>/
///    <see cref="BogenschuetzeMaximaleLadung"/>), der Multiplikator in der
///    Berserker-Passivformel sowie die Lebenspunkte einer erzeugten Einheit
///    (Klon, siehe <see cref="Units.Unit.CreateClone"/> - dort 1 -&gt; 10) und
///    die Standardfigur (<see cref="Rules.UnitDefaults"/> - dort 50 -&gt; 500
///    HP, 10 -&gt; 100 Schaden).
///
///    WAS BEWUSST NICHT SKALIERT WURDE, weil es keine Kampfwerte im Sinne
///    von "wird mit Schaden/HP verrechnet" sind: Bewegungs-/Angriffs-
///    reichweiten und Wirkungsradien (<see cref="StatModifierSet.MovementRangeDelta"/>/
///    <see cref="StatModifierSet.AttackRangeDelta"/> eingeschlossen - z.B.
///    Musiker-Marschlied und Elementarmagier-Verlangsamung), Geschwindig-
///    keitswerte (<see cref="StatModifierSet.SpeedDelta"/> eingeschlossen)
///    und die davon abhängige Zugreihenfolge, Abklingzeiten/Wirkungsdauern/
///    das Rundenlimit sowie ECHTE Prozentwerte wie
///    <see cref="EffectTemplate.IncomingDamageMultiplier"/> (Schildträger/
///    Wand) und <see cref="EffectTemplate.ReflectsFractionOfRedirectedDamage"/>
///    (Wand) - dort ändert sich nur die BEZUGSGRÖSSE (der jetzt größere
///    Schaden, auf den der unveränderte Prozentsatz angewendet wird), nicht
///    der Prozentsatz selbst. <see cref="EffectTemplate.PlateauRadius"/> ist
///    eine Distanzstufe, kein Kampfwert, bleibt daher ebenfalls unverändert.
///
///    RUNDUNG NACH DER SKALIERUNG IST UNKRITISCH, ABER NICHT VERHALTENS-
///    GLEICH ZUM VORHERIGEN STAND: jeder skalierte Wert ist ein Vielfaches
///    von 10, also gerade - jede Summe/Differenz solcher Werte (Buffs, DoT-
///    Abklingen, Ladung) bleibt ein Vielfaches von 10. Eine Halbierung
///    (aktuell die einzigen beiden Prozentwerte im Katalog: je 0.5 bei
///    Schildträger und Wand) trifft dadurch NACH der Skalierung nie mehr
///    eine echte Kommastelle - <c>Math.Round</c> (Standard-.NET-Rundung:
///    "Bankers Rounding", round-half-to-even, NICHT kaufmännisch) bekommt so
///    nie einen echten Rundungsfall zu entscheiden. Das gilt nachweislich für
///    den aktuellen (skalierten) Katalog; ein künftiger Drittel- oder
///    25%-Multiplikator wäre davon nicht mehr automatisch erfasst und müsste
///    erneut geprüft werden.
///    NACHTRAG: genau das ist mit der Designentscheidung "Mauer abschwächen"
///    eingetreten - die Selbstreduktion der Wand ist jetzt 0,75 und erzeugt bei
///    ungeraden Vielfachen von 10 echte Halbwerte, die per Bankers Rounding
///    aufgelöst werden (siehe Klassendokumentation von <see cref="Wand"/>).
///
///    WICHTIG - genau DAS macht die Skalierung für Schildträger/Wand NICHT
///    verhaltensgleich zum Stand VOR der Skalierung: der ursprüngliche
///    (unskalierte) Katalog enthielt mehrere UNGERADE Rohschadenswerte, die
///    unter der 0.5-Reduktion echte .5-Zwischenwerte erzeugten (z.B.
///    Schildträger-/Wand-Grundschaden 7 -> 3,5, Feuerball/Erdschlag 3 bzw. 7
///    -> 1,5/3,5, Verbrennungs-Ticks 3/1 -> 1,5/0,5, Musiker-Grundschaden 9
///    -> 4,5, Bogenschütze mit ungerader Ladungsstufe 12+5=17 -> 8,5). Diese
///    Fälle wurden VOR der Skalierung tatsächlich per Bankers-Rounding
///    aufgelöst (z.B. 3,5 -> 4, aber 0,5 -> 0) - eine Rundungsentscheidung,
///    die durch die ×10-Skalierung nicht mehr auftritt (aus 7 wird 70, aus
///    3,5 wird exakt 35, keine Rundung mehr nötig). Der alte gerundete Wert
///    (z.B. 4) und der neue exakte Wert (3,5×10=35, nicht 4×10=40) stimmen
///    darum NICHT überein - ein unvermeidbarer Nebeneffekt jeder reinen
///    Ganzzahl-Reskalierung von Werten, die vorher unter einer 0.5-Reduktion
///    gerundet wurden, siehe VERIFIKATION unten für den gemessenen Umfang.
///
///    VERIFIKATION (vollständiger Basislauf, alle 3.570 Paarungen, alle 72
///    Ausgangslagen, 257.040 Partien, Seed 2026, vor vs. nach der
///    Skalierung, automatisierter Diff): die 630 Paarungen OHNE Schildträger
///    UND Wand liefern für ALLE geprüften Kennzahlen (Siege, Siegquote,
///    Ø-Rundenzahl, Ø-Rest-HP-Anteil, Rundenlimit-Anteil) exakt identische
///    Werte - für diese ~18% des Katalogs ist "Spielverhalten exakt
///    identisch" also nachgewiesen. Von den verbleibenden 2.940 Paarungen
///    MIT Schildträger und/oder Wand zeigen 524 (17,8%) ein numerisch
///    abweichendes Ergebnis (anderer Sieger oder andere Rundenzahl bei
///    mindestens einer der 72 Ausgangslagen) - Ursache in JEDEM geprüften
///    Fall exakt die oben beschriebene Bankers-Rounding-Eliminierung: eine
///    einzelne, durch einen ungeraden Rohschaden abweichende reduzierte
///    Trefferzahl verändert (in einem deterministischen, aber verzweigenden
///    System aus Zufallsentscheidungen) ab diesem Zeitpunkt die gesamte
///    weitere Partie. Determinismus bei gleichem Seed und die Abwesenheit
///    JEDER Abweichung außerhalb von Schildträger/Wand-Paarungen bestätigen,
///    dass keine andere Stelle im Katalog übersehen wurde. Diese
///    Restabweichung ist mathematisch nicht vermeidbar, ohne die
///    Bankers-Rounding-Entscheidungen des alten Katalogs Wert für Wert
///    nachzubilden - was dem eigentlichen Ziel der Skalierung
///    (größere, prozentual sauber veränderbare Werte) entgegenliefe.
///
/// 4. WIRKUNGSDAUER HÄNGT AM ANWENDER (Bachelorarbeit-Korrektur,
///    Designentscheidung): die Dauer JEDES Effekts mit begrenzter Dauer zählt
///    zu Beginn des nächsten eigenen Zugs seines ANWENDERS herunter und endet
///    dort - nicht mehr (feld-verankerte Effekte) zu Beginn jeder Runde und
///    nicht (unit-verankerte Effekte) am Zug des betroffenen Ziels. Eine
///    Fähigkeit mit Dauer 1 wirkt damit immer genau eine volle Runde,
///    unabhängig vom Rang des Helden in der Zugreihenfolge. Die Zugreihenfolge
///    selbst (Speed) und die Abklingzeiten (weiterhin an den Zügen des
///    Anwenders) sind unverändert. Details und Begründung: siehe
///    <see cref="EffectSystem"/>.
///
///    FEHLERBILD VORHER: Schildwall und Mauer sind feld-verankert und wurden
///    einmal zu Beginn jeder Runde getickt. Ein Schildträger an Rang 3, der in
///    Runde N seinen Schildwall (Dauer 1) einsetzte, war in Runde N+1 bereits
///    ungeschützt, wenn ein Gegner an Rang 2 vor ihm angriff - der Effekt
///    schützte praktisch nur den Rest der eigenen Runde, bei spätem Rang
///    (Wand an Rang 5) fast nichts. Die Dokumentation beschrieb den Tick
///    ("zu Zugbeginn der betroffenen Einheit") korrekt nur für
///    unit-verankerte Effekte; für Feld-Effekte stand (und galt) der
///    Rundenbeginn - der Widerspruch bestand zwischen dieser Regel und der
///    Erwartung, nicht zwischen Dokumentation und Implementierung.
///
///    ENTSCHEIDUNG ZUR ZUORDNUNG: wirkt ein Effekt auf mehrere Einheiten (z.B.
///    Schildwall auf alle Einheiten des Feldes), hängt die Dauer am Anwender,
///    nicht an den Betroffenen: eine einzige Effektinstanz, der Schutz endet
///    für alle gleichzeitig, und der Anwender kontrolliert die Dauer seiner
///    eigenen Fähigkeit. Der SCHADENS-Tick (Verbrennung) bleibt dagegen beim
///    Zugbeginn des ZIELS - nur die Restlaufzeit wanderte zum Anwender. Stirbt
///    der Anwender, enden alle seine Effekte auf anderen Einheiten sofort
///    (<see cref="EffectSystem.RemoveEffectsRelatedTo"/>) - das war
///    schon vorher so und ist jetzt zwingend, weil sonst niemand mehr die
///    Dauer herunterzählen würde.
///
///    WIRKUNG JE EFFEKT (Dauer, Anker; "Zyklus" = ein Zug des Anwenders bis
///    zum nächsten):
///      - Schildwall (Dauer 1, Feld): schützt bis zum nächsten eigenen Zug des
///        Schildträgers, danach greift seine Abklingzeit (2) - unverändert.
///      - Mauer (Dauer 1, Feld + Wand selbst, gekoppelt): beide Instanzen enden
///        gemeinsam im Dauer-Tick der Wand. Ohne Abklingzeit kann die Wand in
///        jedem eigenen Zug neu wirken (Auffrischen) - der Schutz ist damit
///        praktisch lückenlos, und die (unveränderte) Vergeltung greift
///        entsprechend häufiger.
///      - Kampflied/Marschlied (Dauer 2, je Verbündeter): vorher wirkten sie
///        für genau EINE Aktion des Verbündeten (Tick am eigenen Zugbeginn
///        des Ziels, beim zweiten Tick bereits weg), jetzt für zwei Zyklen
///        des Musikers - der Buff deckt etwa doppelt so viele Aktionen ab.
///      - Verbrennung (Dauer 3, Ziel): der Schadens-Tick (30/20/10) bleibt
///        beim Ziel, die Dauer zählt drei Züge des Elementarmagiers - im
///        Regelfall weiterhin genau drei Schadens-Ticks, unabhängig von der
///        Reihenfolge. Nur wenn sich die relative Reihenfolge von Anwender und
///        Ziel zwischen den Runden ändert (Tod einer schnelleren Einheit
///        verschiebt die Ränge), sind 2 oder 4 Ticks möglich; der 4. Tick
///        verursacht wegen des Abklingens 0 Schaden.
///      - Erdschlag/Verlangsamung (Dauer 2, Ziel): wie Kampflied - vorher eine,
///        jetzt zwei Aktionen des Ziels mit -1 Bewegung.
///      - Klon, Aufladen, Umklammerung: keine Wirkungsdauer (unbegrenzt bzw.
///        Einheit statt Effekt) - von der Korrektur nicht berührt.
///
///    MESSUNG (vollständiger Basislauf, 257.040 Partien je Agent, Seed 2026,
///    vor vs. nach der Korrektur, Ergebnisse in Ergebnisse/effektdauer_korrektur):
///    die Partien werden im Mittel um ca. 9 % länger (byActionType 32,9 -&gt;
///    35,8 Runden, Uniform 37,8 -&gt; 41,3), der Rundenlimit-Anteil steigt
///    (2,96 -&gt; 3,95 % bzw. 4,91 -&gt; 6,60 %). Die bereinigte Siegquote der Wand
///    steigt um +4,7 (byActionType) bzw. +5,7 Prozentpunkte (Uniform), die des
///    Schildträgers um +2,1 bzw. +3,9, die des Musikers um +3,8 bzw. +3,5;
///    Bogenschütze (-4,3/-5,6), Elementarmagier (-3,7/-4,0) und Berserker
///    (-2,2/-2,7) verlieren. Die Tank-Rangfolge Illusionist &gt; Schildträger &gt;
///    Wand bleibt in beiden Agenten erhalten. Kontrolle: die 55 Paarungen ohne
///    Schildträger, Wand, Musiker und Elementarmagier (die einzigen Helden mit
///    Effekten endlicher Dauer) liefern exakt dieselben Werte wie vorher.
///
///    BEKANNTE RANDFÄLLE (bewusst unverändert, nur dokumentiert): (a) zwei
///    gleichnamige Effekt-Instanzen desselben Anwenders sind über
///    <see cref="EffectSystem.RemoveEffectAndLinkedCounterpart"/>
///    gekoppelt - läuft bei einem Mehrfach-Buff (Kampflied auf mehrere
///    Verbündete) eine versetzt aufgefrischte Instanz früher ab, wird eine
///    andere Instanz desselben Buffs mit entfernt. (b) Frischt ein ANDERER
///    Anwender einen bestehenden gleichnamigen Effekt auf (RefreshDuration,
///    z.B. zwei Schildträger auf demselben Feld), bleibt die Quelle - und
///    damit der Dauer-Tick - beim ersten Anwender.
/// </summary>
public static class DefaultHeroes
{
    // ---- Rollen-Korridore (siehe Klassendokumentation Punkt 2), ×10 skaliert (siehe Punkt 3) ----
    // TankMaxHp bewusst auf 1000 angehoben (statt 900): Balancing-Entscheidung
    // für die Wand (siehe dort, Fähigkeits-Rework) - explizit bestätigt,
    // kein Versehen.
    public const int TankMinHp = 750, TankMaxHp = 1000, TankMinDamage = 50, TankMaxDamage = 70;
    public const int SupportMinHp = 450, SupportMaxHp = 550, SupportMinDamage = 80, SupportMaxDamage = 100;
    // DmgMinDamage bewusst auf 80 abgesenkt (statt 120): Balancing-Entscheidung
    // für den Berserker (siehe dort) - explizit bestätigt, kein Versehen.
    // Bleibt bewusst ÜBER TankMaxDamage (70): das Kernprinzip "ein Dmg-Held
    // macht immer mehr Grundschaden als ein Tank" gilt damit weiterhin,
    // nur mit deutlich kleinerem Abstand als zuvor (10 statt 50) - der
    // Berserker holt seine eigentliche Stärke ohnehin über die
    // Passivfähigkeit, nicht über den Grundschaden (siehe dort).
    public const int DmgMinHp = 280, DmgMaxHp = 450, DmgMinDamage = 80, DmgMaxDamage = 150;

    /// <summary>
    /// Tank: aktiv, gibt allen Einheiten auf seinem EIGENEN Feld
    /// (inklusive sich selbst) für eine Runde 50% Schadensreduktion. Das
    /// "eigene Feld" ergibt sich daraus, dass die Fähigkeit
    /// <see cref="AbilityTargetType.AllUnitsOnField"/> mit Reichweite 0
    /// nutzt - das einzige Feld in Reichweite 0 ist das eigene, es gibt
    /// also gar keine echte Zielwahl (ein "Ein-Klick"-Schild).
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing, siehe <see cref="AbilityDefinition.CooldownRounds"/>):
    /// 2 Runden - VORLÄUFIGER Startwert, noch nicht über Simulationen
    /// ausbalanciert. Ohne Abklingzeit ließe sich die Reduktion praktisch
    /// jede eigene Runde erneuern; 2 Runden erzwingen mindestens eine
    /// ungeschützte Zwischenrunde.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 45,82% - leicht unterdurchschnittlich. HP von 80 auf 85
    /// angehoben (weiterhin innerhalb des Tank-Korridors 75-90), Schaden am
    /// Korridor-Maximum belassen (siehe Prinzip "Tank soll nicht mehr
    /// Schaden machen als ein Dmg-Held" - dafür ist HP der einzig passende
    /// Hebel für einen Tank).
    /// </summary>
    public static readonly HeroDefinition Schildtraeger = new(
        Id: "schildtraeger",
        DisplayName: "Schildträger",
        Role: HeroRole.Tank,
        MaxHitPoints: 850, // Tank-Korridor 750-1000, ×10 skaliert (Balancing vor Skalierung: 80 -> 85, siehe Klassendokumentation der Fähigkeit)
        BaseAttackDamage: 70, // Tank-Korridor 50-70, ×10 skaliert
        Speed: 5,
        MovementRange: 1,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "schildwall",
            DisplayName: "Schildwall",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsOnField,
            Range: 0,
            CooldownRounds: 2,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Schildwall",
                    IsInstant: false,
                    Duration: 1,
                    Stacking: StackingRule.RefreshDuration,
                    IncomingDamageMultiplier: 0.5),
            }));

    /// <summary>
    /// Support: aktiv, heilt einen Verbündeten - Wirkung sinkt mit
    /// Entfernung (20 HP auf Distanz 0, 15 auf Distanz 1, 10 ab Distanz 2 -
    /// siehe <see cref="EffectTemplate.MinimumAfterDistanceScaling"/>).
    ///
    /// HP am unteren Rand des Support-Korridors (45-55) angesetzt: die
    /// größere Reichweite (2 statt 1 bei Musiker/Hugger) erlaubt ihr, sich
    /// aus der zweiten Reihe heraus in Sicherheit zu halten, was den
    /// niedrigeren Wert rechtfertigt - zugleich bleibt sie als einziger
    /// Fernkämpfer unter den Supports laut Vorgabe unterhalb der beiden
    /// Nahkämpfer Musiker (48) und Hugger (jetzt 50, siehe dort).
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 61,01% - deutlich überdurchschnittlich, stärkste
    /// Überperformerin unter den Supports. Heilung von 20 auf 17 gesenkt
    /// (Distanzabfall/Minimum unverändert) statt an den HP/Schaden-Werten zu
    /// drehen, da diese bereits am unteren Korridor-Rand liegen und die
    /// eigentliche Stärke sichtbar aus der Fernheilung selbst kommt.
    /// </summary>
    public static readonly HeroDefinition Elfe = new(
        Id: "elfe",
        DisplayName: "Elfe",
        Role: HeroRole.Support,
        MaxHitPoints: 450, // Support-Korridor 450-550, unterer Rand, ×10 skaliert (siehe oben)
        BaseAttackDamage: 80, // Support-Korridor 80-100, ×10 skaliert
        Speed: 12,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "heilung",
            DisplayName: "Heilung",
            IsPassive: false,
            TargetType: AbilityTargetType.Ally,
            Range: 2,
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Heilung",
                    IsInstant: true,
                    InstantHeal: 120, // Balancing vor Skalierung: 20 -> 12, ×10 skaliert (siehe Klassendokumentation)
                    ScaleWithDistance: true,
                    DistanceFalloffPerStep: 50, // ×10 skaliert
                    MinimumAfterDistanceScaling: 20), // ×10 skaliert
            }));

    /// <summary>
    /// Dmg: passiv, Schaden steigt bei sinkenden eigenen HP - gestuft in
    /// drei Dritteln der fehlenden HP (0/4/8, siehe Balancing unten). Wird
    /// nicht als gespeicherter Effekt geführt, sondern bei jeder Abfrage aus
    /// den AKTUELLEN HP live neu berechnet (siehe <see cref="Hero.GetPassiveStatContribution"/>),
    /// da sich die HP jederzeit ändern.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 66,36% bei BaseAttackDamage 12 (Korridor-Minimum!) und der
    /// ursprünglich linearen Passiv-Formel (+0 bis +20) - deutlichster
    /// Überperformer im gesamten Katalog trotz Mindestschaden, erkennbar vor
    /// allem am Zusammenspiel aus hohen HP (Korridor-Maximum 45) und einer
    /// Passivfähigkeit, die genau bei sinkenden HP noch stärker wird (ein
    /// Snowball-Effekt: mehr HP = mehr Runden im Hochbonus-Bereich, ohne zu
    /// sterben). Gegenmaßnahme über drei Hebel gleichzeitig statt eines
    /// einzelnen: HP von 45 auf 40 gesenkt (bleibt oberhalb der beiden
    /// Fernkämpfer Elementarmagier/Bogenschütze, siehe Klassendokumentation
    /// Punkt 2), die zwischenzeitlich gestufte Passiv-Formel (Schritte bei
    /// 1/3 und 2/3 der fehlenden HP statt linear) im Multiplikator von *5
    /// auf *4 abgeschwächt (maximal +8 statt +10/+15 kurz vor dem Tod), und
    /// BaseAttackDamage zusätzlich von 12 auf 8 gesenkt (Dmg-Korridor
    /// entsprechend auf 8-15 erweitert, siehe DmgMinDamage oben - bestätigte
    /// Balancing-Entscheidung: der Berserker soll seine Stärke erkennbar aus
    /// der Passivfähigkeit ziehen, nicht aus dem Grundangriff).
    /// </summary>
    public static readonly HeroDefinition Berserker = new(
        Id: "berserker",
        DisplayName: "Berserker",
        Role: HeroRole.Dmg,
        MaxHitPoints: 400, // Dmg-Korridor 280-450, ×10 skaliert (Balancing vor Skalierung: 45 -> 40, siehe Klassendokumentation)
        BaseAttackDamage: 80, // Dmg-Korridor 80-150, unterer Rand, ×10 skaliert (Balancing vor Skalierung: 12 -> 8, bestätigt - siehe DmgMinDamage oben)
        Speed: 13,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "blutrausch",
            DisplayName: "Blutrausch",
            IsPassive: true,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            Effects: Array.Empty<EffectTemplate>(),
            PassiveStatModifier: (unit, _) =>
            {
                double missingFraction = 1.0 - (double)unit.CurrentHitPoints / unit.MaxHitPoints;
                //int bonus = (int)Math.Round(missingFraction * 20); // urspüngliche, rein lineare Fassung
                int bonus = (int)Math.Floor(missingFraction * 3) * 40; // Balancing vor Skalierung: Multiplikator 5 -> 4, ×10 skaliert (siehe Klassendokumentation) - missingFraction selbst ist eine reine HP-Verhältniszahl und bleibt unskaliert korrekt
                return new StatModifierSet(BaseAttackDamageDelta: bonus);
            }));

    /// <summary>
    /// Anzahl Runden, die die Feldsperre/Umleitung der Wand nach Einsatz
    /// anhält (Standard laut Aufgabenstellung: 1). Bewusst als benannte
    /// Konstante statt eines Literalwerts.
    /// </summary>
    private const int WandMauerDauer = 1;

    /// <summary>
    /// Schadensmultiplikator der Selbstreduktion der Mauer: 0,75 = 25 %
    /// Reduktion (Designentscheidung "Mauer abschwächen", vorher 0,5 = 50 %).
    /// </summary>
    public const double WandMauerSchadensmultiplikator = 0.75;

    /// <summary>
    /// Abklingzeit der Mauer (Designentscheidung "Mauer abschwächen", vorher
    /// keine). ACHTUNG ZÄHLWEISE: die Abklingzeit sinkt zu Beginn JEDES eigenen
    /// Zugs, auch schon im ersten Zug nach dem Einsatz (siehe
    /// Hero.TickAbilityCooldown). Wert 1 wäre deshalb wirkungslos - die Mauer wäre
    /// im nächsten Zug sofort wieder verfügbar. Wert 2 sperrt die Fähigkeit in
    /// genau EINEM eigenen Zug, also eine Runde Abklingzeit im umgangssprachlichen
    /// Sinn (gleiche Regel wie beim Schildwall).
    /// </summary>
    public const int WandMauerAbklingzeit = 2;

    /// <summary>
    /// Tank: aktiv, sperrt das eigene Feld für Gegner
    /// (<see cref="EffectTemplate.LocksFieldForEnemies"/>) und leitet
    /// Angriffe, deren Sichtlinie durch dieses Feld verläuft - AUCH auf ein
    /// Ziel, das sich das Feld mit der Wand teilt - auf die Wand selbst um
    /// (<see cref="EffectTemplate.RedirectsAttacksToSource"/>, siehe die
    /// entsprechend erweiterte <see cref="Combat.CombatSystem"/>-Umleitung).
    /// Die Mauer hat damit zwei Wirkungen auf die Wand, beide auf Basis des
    /// URSPRÜNGLICHEN, unreduzierten Schadens (siehe <see cref="Combat.CombatSystem.ApplyDamageAndHandleDefeat(GameState, Units.Unit, Units.Unit, int)"/>
    /// für die Begründung, warum GENAU der unreduzierte Wert und nicht der
    /// jeweils andere Effekt als Grundlage dient):
    ///   - SELBSTREDUKTION (Designentscheidung, bewusst so belassen): solange
    ///     die Mauer aktiv ist, nimmt die Wand GENERELL nur 75 % des Schadens
    ///     (bis zur Designentscheidung "Mauer abschwächen" unten: halben Schaden) -
    ///     unabhängig davon, ob ein Treffer umgeleitet wurde oder sie direkt
    ///     traf, und auch bei Schaden über Zeit (<see cref="EffectTemplate.IncomingDamageMultiplier"/>,
    ///     NUR auf sie selbst wirkend, siehe <see cref="EffectTemplate.AppliesToCaster"/> -
    ///     ein Verbündeter, der sich das Feld mit ihr teilt, profitiert davon
    ///     NICHT, anders als bei Schildträgers Schildwall). Frühere Fassungen
    ///     dieser Dokumentation beschrieben die Reduktion nur für umgeleitete
    ///     Treffer; das Verhalten war aber immer generell und wurde als
    ///     Designentscheidung bestätigt: die Mauer ist der gepanzerte Körper
    ///     der Wand und schützt sie von jeder Seite.
    ///   - VERGELTUNG: nur bei einem UMGELEITETEN Treffer (ein Gegner greift
    ///     einen Verbündeten auf ihrem Feld an oder schießt durch ihr Feld)
    ///     erhält der Angreifer die Hälfte des ursprünglichen Schadens zurück
    ///     (<see cref="EffectTemplate.ReflectsFractionOfRedirectedDamage"/>).
    ///     Ein direkter Angriff auf die Wand löst keine Vergeltung aus - die
    ///     Vergeltung ist der Preis dafür, an der Wand vorbei ein anderes Ziel
    ///     treffen zu wollen, nicht eine Dornenwirkung.
    /// Bei 120 eingehendem, umgeleitetem Schaden: sie verliert 90 HP, der
    /// Angreifer erhält 60 HP Vergeltungsschaden - NICHT nur 45 (was der Fall
    /// wäre, würde die Vergeltung auf dem bereits reduzierten Wert basieren
    /// statt auf dem ursprünglichen).
    ///
    /// DESIGNENTSCHEIDUNG "MAUER ABSCHWÄCHEN" (Bachelorarbeit-Balancing nach
    /// Beobachtung in Godot): die Wand wirkte ihre Mauer in nahezu jedem Zug,
    /// die Elfe heilte den verbleibenden Schaden weitgehend weg - gegen Wand und
    /// Elfe gemeinsam kam von allen drei Schadenshelden kaum noch Schaden durch.
    /// Zwei Hebel, beide NUR an der Mauer (der Schildwall bleibt unverändert):
    ///   - Selbstreduktion 50 % -> 25 % (<see cref="WandMauerSchadensmultiplikator"/>
    ///     0,5 -> 0,75). Die Vergeltung (Hälfte des URSPRÜNGLICHEN Schadens) und
    ///     die Umleitung selbst bleiben unverändert.
    ///   - Abklingzeit neu (<see cref="WandMauerAbklingzeit"/> = 2 in der Zählweise
    ///     des Systems, also eine Runde gesperrt): bei Dauer 1 läuft die Mauer zu
    ///     Beginn des nächsten eigenen Zugs ab und kann dort nicht erneuert werden -
    ///     zwischen zwei Mauern liegt so immer mindestens eine volle ungeschützte
    ///     Runde. Vorher war ein Neueinsatz in jedem eigenen Zug möglich.
    /// RUNDUNG: 0,75 ist der erste Multiplikator im Katalog, der bei den ×10
    /// skalierten Schadenswerten echte Halbwerte erzeugt (ungerades Vielfaches
    /// von 10: 70 -> 52,5, 90 -> 67,5). CombatSystem rundet mit Math.Round
    /// (Bankers Rounding, round-half-to-even): 52,5 -> 52, 67,5 -> 68. Das ist
    /// deterministisch und im Mittel unverzerrt; die Rundungsregel wurde
    /// bewusst NICHT verändert (siehe Punkt 3 der Klassendokumentation).
    ///
    /// DESIGNENTSCHEIDUNG "UMLEITUNG NUR FÜR GEGNER": umgeleitet werden nur
    /// Angriffe von Gegnern der Wand (siehe CombatSystem.ResolveRedirect).
    /// Angriffe von Verbündeten - auch durch ihr Feld hindurch - treffen ihr
    /// eigentliches Ziel und lösen keine Vergeltung aus. Vorher traf ein
    /// solcher Angriff die eigene Wand, und der Verbündete erhielt die
    /// Vergeltung (Stichprobe: in rund einem Viertel der Partien mit Wand).
    /// Begründung: eine Mauer, die die eigene Seite am Angreifen hindert und
    /// bestraft, widerspricht ihrer Rolle als Schutz des eigenen Teams.
    ///
    /// DESIGNENTSCHEIDUNG "DIE MAUER WANDERT MIT" (Bachelorarbeit, Neugestaltung
    /// der Fähigkeit, keine reine Fehlerkorrektur): Sperre, Umleitung und
    /// Vergeltung beziehen sich auf das JEWEILS AKTUELLE Feld der Wand, nicht
    /// auf das Feld, auf dem die Fähigkeit gewirkt wurde (<see cref="EffectTemplate.FieldFollowsSource"/>).
    /// Die Wand bleibt beweglich. VORHER blieb der Feldeffekt beim Wegziehen
    /// auf dem alten Feld: ein Angriff auf eine Einheit dort traf weiterhin die
    /// Wand - auch außerhalb der Reichweite des Angreifers - und löste Vergeltung
    /// aus, und das alte Feld blieb für Gegner gesperrt. Begründung: die Mauer
    /// ist inhaltlich der Körper der Wand, der Schutz soll dort sein, wo sie
    /// steht; ein zurückgelassener Schutz ohne Wand widerspräche dem Bild und
    /// ließe Schaden einer Einheit zurechnen, die gar nicht mehr dort steht.
    /// Technisch bleibt die Mauer ein Feldeffekt, dessen Feld beim Bewegen
    /// mitwandert (siehe <see cref="GameState.MoveUnit"/>) - Bewegungssperre,
    /// Umleitung, Schadensreduktion, Anzeige und Replay lesen weiterhin
    /// "Effekte auf diesem Feld" und bleiben unverändert. Folgen:
    ///   - Die Feldsperre sperrt das jeweilige Feld der Wand. Gegner, die dort
    ///     bereits stehen, werden nicht verdrängt und können das Feld verlassen
    ///     (die Sperre verhindert nur das Betreten und Durchqueren).
    ///   - Die beiden Instanzen (Feldeffekt und Selbstreduktion) bleiben über
    ///     den Namen "Mauer" gekoppelt und laufen gemeinsam im Zug der Wand ab.
    ///   - Die Wand kann auf ein Feld mit Gegnern ziehen. Angriffe ihrer
    ///     Verbündeten auf diese Gegner treffen die Gegner (siehe "Umleitung
    ///     nur für Gegner" oben); vor dieser Entscheidung trafen sie die eigene Wand.
    ///
    /// DESIGNENTSCHEIDUNG "JEDE WAND HAT IHRE EIGENE MAUER": weil die Mauer an
    /// ihre Wand gebunden ist, gehört der Anwender zur Identität des Effekts
    /// (siehe EffectSystem.FindMatching). Stehen zwei gegnerische Wände auf
    /// demselben Feld und wirken beide, bestehen beide Mauern nebeneinander:
    /// ein Angriff auf eine Einheit dort geht zur Wand des ANGEGRIFFENEN Teams
    /// (die gegnerische Mauer leitet Angriffe ihres eigenen Teams nicht um), die
    /// Vergeltung trifft den Angreifer; beide Sperren wirken, das Feld ist also
    /// für Außenstehende beider Teams gesperrt; zieht eine Wand weg, nimmt sie
    /// nur ihre eigene Mauer mit. VORHER frischte die zweite Wand die Mauer der
    /// ersten auf, statt eine eigene zu erzeugen - sie verstärkte damit die
    /// Fähigkeit ihres Gegners (in 27,9 bzw. 18,5 % der Partien Wand gegen Wand).
    /// Die Vergeltung selbst wird nie umgeleitet; eine gegenseitige Reflexion
    /// zwischen zwei Mauern ist deshalb ausgeschlossen.
    ///
    /// FÄHIGKEITS-REWORK (Bachelorarbeit-Balancing, Basis: Siegquote 39,56%
    /// im vollständigen Basislauf - schwächster Tank und zweitschwächster
    /// Held insgesamt, siehe unten): die Wand bezahlte ihre Umleitung
    /// bislang mit eigener Bewegungsunfähigkeit für die Dauer der Sperre,
    /// OHNE dafür irgendeine eigene Gegenwirkung zu erhalten - ein reiner
    /// Kostenpunkt ohne Gegenwert, der zugleich ihre ohnehin schon
    /// geringste Bewegungsreichweite (1) im entscheidenden Moment auf 0
    /// senkte. Zwei Hebel zugleich: die Bewegungsunfähigkeit ist komplett
    /// ENTFALLEN (die Wand bleibt nach dem Einsatz normal beweglich), und
    /// die Umleitung selbst bekommt über den neuen Vergeltungsschaden
    /// erstmals eine eigene offensive Komponente. Keine Abklingzeit (wie
    /// zuvor; inzwischen durch "Mauer abschwächen" oben ersetzt, dort mit
    /// Abklingzeit) - ohne die Bewegungsunfähigkeit als natürliche Bremse wäre ein
    /// Einsatz jede eigene Runde zwar weiterhin möglich, aber
    /// <see cref="StackingRule.RefreshDuration"/> verhindert ohnehin jedes
    /// Aufsummieren mehrerer gleichzeitiger Instanzen (siehe
    /// <see cref="Effects.EffectSystem.Apply"/>) - ein täglicher Neueinsatz
    /// verlängert die Sperre lediglich, verstärkt sie nicht zusätzlich.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 39,56% - HP von 85 auf 90 angehoben (Korridor-Maximum),
    /// Schaden bereits am Korridor-Maximum belassen (siehe Prinzip "Tank
    /// soll nicht mehr Schaden machen als ein Dmg-Held" - dieser reine
    /// Stat-Hebel allein bewegte die Siegquote im ersten Balancing-Durchgang
    /// kaum, siehe Klassendokumentation von <see cref="DefaultHeroes"/> -
    /// daher zusätzlich der obenstehende Fähigkeits-Rework).
    /// </summary>
    public static readonly HeroDefinition Wand = new(
        Id: "wand",
        DisplayName: "Wand",
        Role: HeroRole.Tank,
        MaxHitPoints: 1000, // Tank-Korridor 750-1000, oberer Rand, ×10 skaliert (Balancing vor Skalierung: 90 -> 100, bestätigt - siehe TankMaxHp oben)
        BaseAttackDamage: 70, // Tank-Korridor 50-70, ×10 skaliert
        Speed: 4,
        MovementRange: 1,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "mauer",
            DisplayName: "Mauer",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsOnField,
            Range: 0,
            CooldownRounds: WandMauerAbklingzeit, // Designentscheidung "Mauer abschwächen" (siehe Klassendokumentation)
            Effects: new[]
            {
                new EffectTemplate(
                    Name: "Mauer",
                    IsInstant: false,
                    Duration: WandMauerDauer,
                    Stacking: StackingRule.RefreshDuration,
                    LocksFieldForEnemies: true,
                    RedirectsAttacksToSource: true,
                    ReflectsFractionOfRedirectedDamage: 0.5, // Balancing (Fähigkeits-Rework): neu - siehe Klassendokumentation
                    FieldFollowsSource: true), // Designentscheidung: die Mauer wandert mit der Wand (siehe Klassendokumentation)
                // Zweite, unit-verankerte Instanz (ebenfalls "Mauer", per
                // AppliesToCaster NUR auf die Wand selbst) - die vormalige
                // Bewegungsunfähigkeit an dieser Stelle ist ENTFALLEN (siehe
                // "FÄHIGKEITS-REWORK" oben), stattdessen jetzt die
                // Selbstreduktion: NUR die Wand nimmt bei einem umgeleiteten
                // Treffer 75 % des Schadens, ein Verbündeter auf
                // demselben Feld ausdrücklich NICHT (anders als bei
                // Schildträgers Schildwall, das das gesamte Feld schützt -
                // siehe dortiges IncomingDamageMultiplier, dort bewusst OHNE
                // AppliesToCaster). Gekoppelt über denselben Effektnamen
                // "Mauer" - endet automatisch gemeinsam mit der Feldsperre
                // (siehe Effects.EffectSystem.RemoveEffectAndLinkedCounterpart).
                new EffectTemplate(
                    Name: "Mauer",
                    IsInstant: false,
                    Duration: WandMauerDauer,
                    Stacking: StackingRule.RefreshDuration,
                    IncomingDamageMultiplier: WandMauerSchadensmultiplikator, // Designentscheidung "Mauer abschwächen": 0,5 -> 0,75
                    AppliesToCaster: true),
            }));

    /// <summary>
    /// Tank: aktiv, erzeugt einen Klon (siehe <see cref="Units.Unit.CreateClone"/>
    /// und <see cref="Abilities.AbilitySystem"/>). Ein erneuter Einsatz
    /// lässt einen bereits vorhandenen Klon verschwinden und erzeugt einen
    /// neuen - das ist inzwischen das generische Verhalten JEDER
    /// klon-erzeugenden Fähigkeit (siehe <see cref="Abilities.AbilitySystem"/>),
    /// nicht ein Illusionist-Sonderfall.
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing): 3 Runden - VORLÄUFIGER
    /// Startwert, noch nicht über Simulationen ausbalanciert. Am längsten
    /// von den vier mit Abklingzeit versehenen Fähigkeiten, da ein Klon
    /// (voller Schadenspuffer für einen Treffer, zusätzlicher Zug in der
    /// Reihenfolge) potenziell die stärkste Einzelwirkung hat.
    /// </summary>
    public static readonly HeroDefinition Illusionist = new(
        Id: "illusionist",
        DisplayName: "Illusionist",
        Role: HeroRole.Tank,
        MaxHitPoints: 750, // Tank-Korridor 750-1000, ×10 skaliert
        BaseAttackDamage: 60, // Tank-Korridor 50-70, ×10 skaliert
        Speed: 6,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "trugbild",
            DisplayName: "Trugbild",
            IsPassive: false,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            CooldownRounds: 3,
            Effects: new[]
            {
                new EffectTemplate(Name: "Trugbild", IsInstant: true, SpawnsClone: true),
            }));

    /// <summary>Kampflied: Schadensmultiplikator auf Distanz 0 und 1 - 1,4 = +40 % (Designentscheidung "Kampflied prozentual").</summary>
    public const double MusikerKampfliedMultiplikator = 1.4;

    /// <summary>Kampflied: Abfall je Distanzschritt hinter dem Plateau in PROZENTPUNKTEN (Distanz 2: +40 − 20 = +20 %).</summary>
    public const int MusikerKampfliedAbfallProzentpunkte = 20;

    /// <summary>Wirkungsdauer beider Lieder: 1 = bis zum Beginn des nächsten eigenen Zugs des Musikers (Designentscheidung "Echte Pausen", vorher 2).</summary>
    public const int MusikerLiedDauer = 1;

    /// <summary>
    /// Abklingzeit der Fähigkeit "Lied" - gilt für BEIDE Modi, weil sie an der
    /// Fähigkeit hängt: 2 = ein eigener Zug gesperrt (Designentscheidung "Echte
    /// Pausen", vorher 1 und damit wirkungslos).
    /// </summary>
    public const int MusikerLiedAbklingzeit = 2;

    /// <summary>
    /// Support: aktiv, wählbarer Modus, wirkt auf alle VERBÜNDETEN im
    /// Umkreis um sich selbst (<see cref="AbilityTargetType.AllUnitsInRadius"/>
    /// mit Reichweite 0 - das Zentrum ist immer das eigene Feld, es gibt also
    /// nur die Moduswahl, keine echte Zielwahl - plus
    /// <see cref="AreaTargetFilter.AlliesOnly"/>, damit anders als beim
    /// Schildträger explizit NUR Verbündete betroffen sind). Modus A gibt
    /// einen Schadensbonus, Modus B einen Bewegungsreichweiten-Bonus, beide
    /// für dieselbe Dauer.
    ///
    /// DISTANZABHÄNGIGE WIRKUNG MIT PLATEAU (<see cref="EffectTemplate.ScaleWithDistance"/>
    /// + <see cref="EffectTemplate.PlateauRadius"/>, dieselbe generische
    /// Mechanik wie bei der Elfe-Heilung, um den Plateau-Radius erweitert -
    /// siehe <see cref="Abilities.AbilitySystem.ApplyToUnit"/>, dort
    /// GENERISCH auch für dauerhafte Stat-Modifikatoren statt nur für
    /// Sofortschaden/-heilung ausgewertet):
    ///   - Kampflied: +40 % Angriffsschaden auf Distanz 0 UND 1 (Plateau-Radius
    ///     1 - beide Distanzen gleich stark), +20 % auf Distanz 2 (PlateauRadius: 1,
    ///     Abfall 20 Prozentpunkte je Schritt NACH dem Plateau: 40 - 20*(2-1) = 20).
    ///     DESIGNENTSCHEIDUNG "KAMPFLIED PROZENTUAL": vorher absolut +60 / +40 - je
    ///     nach Grundschaden +50 % (Bogenschütze, 120) bis +100 % (Illusionist, 60),
    ///     im Mittel etwa +75 %. Jetzt wirkt es bei allen Verbündeten mit demselben
    ///     Anteil; der Distanzabfall bleibt erhalten, nur die Bezugsgröße ändert sich
    ///     (siehe AbilitySystem.ScaleMultiplierByDistance).
    ///   - Marschlied: +1 Bewegungsreichweite auf Distanz 0 UND 1 (dasselbe
    ///     Plateau), keine Wirkung mehr auf Distanz 2 (PlateauRadius: 1,
    ///     DistanceFalloffPerStep: 1, zusätzlich MinimumAfterDistanceScaling:
    ///     0 als Sicherheitsnetz gegen einen negativen Bewegungsabzug).
    ///     Dieses "gleich stark auf Distanz 0 UND 1, danach abrupt 0"-Plateau
    ///     war die ursprüngliche Designabsicht und ist mit PlateauRadius jetzt
    ///     EXAKT abgebildet - keine Näherung mehr (siehe EffectTemplate für
    ///     die Begründung, warum die rein lineare Formel das vorher nicht
    ///     konnte und wieso ein zusätzlicher, aber optionaler Parameter statt
    ///     eines komplett neuen Mechanismus gewählt wurde).
    ///
    /// ABKLINGZEIT - gilt für BEIDE Modi gemeinsam (die Abklingzeit sitzt auf
    /// der Fähigkeit als Ganzes, nicht je Modus - ein Wechsel zwischen Kampf-
    /// und Marschlied umgeht die Abklingzeit also nicht).
    ///
    /// DESIGNENTSCHEIDUNG "ECHTE PAUSEN": Wirkungsdauer 2 -> 1
    /// (<see cref="MusikerLiedDauer"/>), Abklingzeit 1 -> 2 (<see cref="MusikerLiedAbklingzeit"/>).
    /// Vorher war die Abklingzeit 1 wirkungslos (sie fällt schon im ersten
    /// eigenen Zug nach dem Einsatz auf 0), und mit Dauer 2 hielten beim
    /// Wechseln BEIDE Lieder dauerhaft gleichzeitig an. ZÄHLWEISE: eine Dauer D
    /// wirkt bis zum Beginn des D-ten eigenen Zugs des Anwenders (jeder Betroffene
    /// hat genau D eigene Züge unter der Wirkung), eine Abklingzeit C sperrt C−1
    /// eigene Züge; zwischen zwei Wirkungen liegen damit C − D Runden. Jetzt:
    /// 1 Runde Lied, dann 1 Runde ohne Buff, in der auch das andere Lied gesperrt
    /// ist - beide Lieder gleichzeitig sind ausgeschlossen.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 44,04% - unterdurchschnittlich, trotz Reichweite 2 und
    /// Plateau-Wirkung ein reiner Verstärker ohne eigene Schadens- oder
    /// Kontrollwirkung. Drei Hebel: Grundschaden von 8 auf 9 angehoben
    /// (bleibt im Support-Korridor 8-10), Abklingzeit von 2 auf 1 Runde
    /// verkürzt (mehr Uptime für den Buff) und der Kampflied-Bonus von 3 auf
    /// 6 angehoben (siehe dort für die daraus resultierenden Distanzwerte).
    /// </summary>
    public static readonly HeroDefinition Musiker = new(
        Id: "musiker",
        DisplayName: "Musiker",
        Role: HeroRole.Support,
        MaxHitPoints: 480, // Support-Korridor 450-550, ×10 skaliert
        BaseAttackDamage: 90, // Support-Korridor 80-100, ×10 skaliert (Balancing vor Skalierung: 8 -> 9, siehe Klassendokumentation)
        Speed: 10,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "lied",
            DisplayName: "Lied",
            IsPassive: false,
            TargetType: AbilityTargetType.AllUnitsInRadius,
            Range: 0,
            Radius: 2,
            AreaFilter: AreaTargetFilter.AlliesOnly,
            CooldownRounds: MusikerLiedAbklingzeit, // Designentscheidung "Echte Pausen": 1 -> 2 (1 war wirkungslos; gilt für BEIDE Lieder)
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "kampflied",
                    DisplayName: "Kampflied",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Kampflied",
                            IsInstant: false,
                            Duration: MusikerLiedDauer, // Designentscheidung "Echte Pausen": 2 -> 1
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(AttackDamageMultiplier: MusikerKampfliedMultiplikator), // Designentscheidung "Kampflied prozentual": vorher absolut +60
                            ScaleWithDistance: true,
                            PlateauRadius: 1, // Distanz 0 UND 1 gleich stark (+40 %) - Distanzstufe selbst, kein Kampfwert
                            DistanceFalloffPerStep: MusikerKampfliedAbfallProzentpunkte), // ab Distanz 2 (= 1 Schritt hinter dem Plateau): 40 - 20*1 = +20 %
                    }),
                new AbilityMode(
                    Id: "marschlied",
                    DisplayName: "Marschlied",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Marschlied",
                            IsInstant: false,
                            Duration: MusikerLiedDauer, // Designentscheidung "Echte Pausen": 2 -> 1
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(MovementRangeDelta: 1),
                            ScaleWithDistance: true,
                            PlateauRadius: 1, // Distanz 0 UND 1 gleich stark (+1)
                            DistanceFalloffPerStep: 1,
                            MinimumAfterDistanceScaling: 0), // Distanz 2: 1 - 1*1 = 0 - kein negativer Bewegungsabzug möglich
                    }),
            }));

    /// <summary>Schadensmultiplikator der Umklammerung auf ein gegnerisches Ziel: 0,67 = 33 % Abzug (Designentscheidung "Umklammerung prozentual").</summary>
    public const double HuggerUmklammerungSchadensmultiplikator = 0.67;

    /// <summary>Schadensmultiplikator der Umklammerung auf ein verbündetes Ziel: 1,33 = 33 % Zuwachs, derselbe Anteil wie der Abzug beim Gegner.</summary>
    public const double HuggerUmklammerungBonusmultiplikator = 1.33;

    /// <summary>Grundschaden des Huggers (Designentscheidung "Hugger-Überarbeitung": 100 -> 60, bewusst unter dem Support-Korridor).</summary>
    public const int HuggerGrundschaden = 60;

    /// <summary>Eigener Schadensmultiplikator, solange der Hugger angeheftet ist: 0,5 = halber Schaden (60 -> 30).</summary>
    public const double HuggerAngeheftetSchadensmultiplikator = 0.5;

    /// <summary>
    /// Wie lange ein Angriff des angehefteten Huggers seine Unangreifbarkeit aufhebt:
    /// 1 = bis zum Beginn seines nächsten eigenen Zugs, also genau eine Runde. Eine
    /// EFFEKTdauer sinkt zu Beginn des eigenen Zugs und läuft bei 0 ab - Dauer 1
    /// deckt damit die Züge aller anderen Einheiten bis dahin ab. (Bei der Mauer
    /// war es die ABKLINGZEIT, deren Wert 1 wirkungslos war - eine andere Zählweise.)
    /// </summary>
    public const int HuggerExponiertRunden = 1;

    /// <summary>
    /// Support: aktiv, wählbarer Modus (Bachelorarbeit-Korrektur: Hugger
    /// braucht neben dem eigentlichen Greifen zwingend eine Möglichkeit,
    /// die Anheftung wieder aktiv zu lösen - siehe "Lösen" unten).
    ///
    /// MODUS "GREIFEN" - heftet sich an eine beliebige Einheit in Reichweite
    /// (<see cref="AbilityTargetType.AnyUnit"/> - erst das TATSÄCHLICHE Ziel
    /// entscheidet über Bonus oder Abzug, siehe
    /// <see cref="EffectTemplate.StatModifiersForEnemy"/>). Die Anheftung
    /// zieht Hugger auch RÄUMLICH mit dem Ziel mit
    /// (<see cref="EffectTemplate.CausesSourceToFollowTarget"/>: bewegt sich
    /// das Ziel, zieht Hugger automatisch auf dasselbe Feld mit, ohne dafür
    /// eine eigene Bewegung zu verbrauchen; seit der Korrektur zieht ihn
    /// bereits das ERSTMALIGE Greifen sofort auf das aktuelle Feld des
    /// Ziels, siehe <see cref="Effects.EffectSystem.Apply"/>) - dafür ist
    /// Hugger, solange die Anheftung aktiv ist, selbst bewegungsunfähig
    /// (<see cref="EffectTemplate.PreventsMovement"/>), greift nur mit halbem
    /// Schaden an (seit der Hugger-Überarbeitung; vorher gar nicht,
    /// <see cref="EffectTemplate.PreventsAttacking"/>) und ist
    /// für Gegner kein gültiges Angriffs- oder Fähigkeitsziel mehr, solange er
    /// nicht angreift (siehe "Hugger-Überarbeitung" unten)
    /// (<see cref="EffectTemplate.PreventsBeingTargetedByEnemies"/> - ERSETZT
    /// die vormalige pauschale Schadensreduktion: eine Einheit, die gar
    /// nicht erst zum Ziel werden kann, braucht keine zusätzliche Reduktion
    /// des dann ohnehin nie eintretenden Schadens). Beide generischen
    /// Effektwirkungen sind bewusst allgemeine <see cref="EffectTemplate"/>-
    /// Felder, keine Hugger-Sonderfallprüfung im Interpreter. Zusätzlich
    /// wirkt dieselbe Fähigkeit über <see cref="EffectTemplate.AppliesToCaster"/>
    /// NEBEN dem gewählten Ziel auch auf Hugger selbst. Beide Effekt-
    /// Instanzen (Ziel + Hugger selbst) tragen denselben Namen "Umklammerung"
    /// und sind damit über <see cref="Effects.EffectSystem.RemoveEffectAndLinkedCounterpart"/>
    /// gekoppelt (enden gemeinsam, wenn das Ziel besiegt wird oder die
    /// Anheftung gelöst/verschoben wird). Die Anheftung selbst nutzt
    /// <see cref="StackingRule.ReplaceForSameSource"/>, damit ein erneuter
    /// Einsatz auf ein ANDERES Ziel die alte Anheftung (auf dem alten Ziel)
    /// tatsächlich beendet, statt sie unangetastet bestehen zu lassen - das
    /// ist zugleich der "auf ein anderes Ziel wechseln"-Teil der Vorgabe.
    ///
    /// ANZEIGE DES BUFFS/DEBUFFS (Bachelorarbeit-Entscheidung): der Bonus/
    /// Abzug bleibt bewusst TEIL DESSELBEN "Umklammerung"-Effekts statt eine
    /// eigene, separat benannte Effekt-Instanz auf dem Ziel zu werden. Grund:
    /// <see cref="EffectTemplate.CancelEffectName"/> (siehe "Lösen" unten)
    /// kann laut <see cref="AbilitySystem.ApplyToUnit"/> AUSSCHLIESSLICH
    /// Effekte auf dem ANWENDER SELBST löschen, nie auf einem beliebigen
    /// anderen Ziel - eine zusätzliche, auf dem GEGRIFFENEN ZIEL verankerte
    /// Effekt-Instanz könnte "Lösen" also gar nicht mit aufräumen und bliebe
    /// nach dem Lösen fälschlich bestehen. Die Sichtbarkeits-Anforderung
    /// ("als eigener, sichtbarer Effekt ... in der Oberfläche angezeigt")
    /// wird stattdessen rein auf der Anzeigeseite erfüllt: die Godot-
    /// Oberfläche liest die Stat-Modifikatoren jedes Effekts aus und zeigt
    /// Betrag/Vorzeichen explizit an (siehe TacticsCoreGodot.Shared.EffectInfo),
    /// statt nur den Effektnamen ohne Wirkung darzustellen.
    ///
    /// MODUS "LÖSEN" - löscht die Anheftung aktiv, ohne ein neues Ziel zu
    /// wählen (<see cref="AbilityMode.IgnoresChosenTarget"/>, generisch über
    /// <see cref="EffectTemplate.CancelEffectName"/> + <see cref="EffectTemplate.AppliesToCaster"/>,
    /// exakt derselbe Mechanismus wie beim Bogenschützen "Abbrechen") und ist
    /// nur wählbar, solange tatsächlich angeheftet ist
    /// (<see cref="AbilityMode.IsLegal"/>). Angeheftet kann Hugger greifen
    /// (neues Ziel bzw. dasselbe erneut), lösen, passen - und seit der
    /// Hugger-Überarbeitung auch angreifen (siehe unten).
    ///
    /// DESIGNENTSCHEIDUNG "HUGGER-ÜBERARBEITUNG": seit der Umklammerung auf 33 %
    /// lohnte sich die Fähigkeit für einen heuristischen Agenten nicht mehr - er gab
    /// seinen eigenen Angriff vollständig auf (Angriffssperre) und erhielt nur einen
    /// Bruchteil davon zurück; in Testläufen griff er lieber an, statt sich
    /// anzuheften. Drei Änderungen:
    ///   1. Grundschaden 100 -> 60 (<see cref="HuggerGrundschaden"/>) - bewusst
    ///      UNTER dem Support-Korridor 80-100, als benannte Ausnahme (siehe
    ///      HeroCatalogTests), weil er Fähigkeit und Angriff jetzt verbinden kann.
    ///   2. Angeheftet darf er angreifen - auf einem Gegner wie auf einem
    ///      Verbündeten -, mit halbem Schaden (<see cref="HuggerAngeheftetSchadensmultiplikator"/>,
    ///      60 -> 30; generisch als Multiplikator auf der eigenen Umklammerungs-
    ///      Instanz, die Angriffssperre entfällt). Sitzt er auf einem Gegner, kann
    ///      er GENAU DIESEN angreifen (gleiches Feld, Distanz 0) - beabsichtigt.
    ///   3. Die Unangreifbarkeit hängt am Angreifen: greift er nicht an, bleibt er
    ///      unangreifbar; greift er an, wird er bis zum Beginn seines nächsten
    ///      eigenen Zugs angreifbar (<see cref="HuggerExponiertRunden"/> = 1, siehe
    ///      <see cref="EffectTemplate.TargetingProtectionSuspendedByAttackRounds"/>),
    ///      danach wieder unangreifbar, sofern er nicht erneut angreift. Die
    ///      Anheftung selbst bleibt in beiden Fällen bestehen. DAUER: Effektdauern
    ///      sinken zu Beginn der eigenen Züge des Anwenders; der Marker "Exponiert"
    ///      mit Dauer 1 läuft also genau zu Beginn seines nächsten Zugs ab und deckt
    ///      die Züge aller anderen Einheiten dazwischen ab - eine volle Runde, egal
    ///      an welcher Stelle der Reihenfolge er steht. (Bei der Mauer war der Wert
    ///      1 wirkungslos, weil es dort um eine ABKLINGZEIT ging, die schon im ersten
    ///      eigenen Zug nach dem Einsatz auf 0 fällt - eine andere Zählweise.)
    /// Unverändert: Bonus/Schwächung 33 % und die Regel "angehefteter letzter Held".
    ///
    /// BEHEBUNG EINER REGELLÜCKE "ANGEHEFTETER LETZTER HELD": war der angeheftete
    /// Hugger der letzte lebende Held seines Teams, konnte ihn niemand angreifen,
    /// er selbst nicht angreifen - die Partie stand bis zum Rundenlimit still. Er
    /// löst sich jetzt automatisch, sobald er letzter Held ist, und kann als
    /// letzter Held nicht erneut greifen (siehe
    /// <see cref="Effects.EffectSystem.ReleaseAttachmentsOfLastHeroes"/>). Die
    /// Anheftung im normalen Spielverlauf ist unverändert.
    ///
    /// DESIGNENTSCHEIDUNG "UMKLAMMERUNG PROZENTUAL": der Abzug beim gegnerischen
    /// Ziel ist ein Anteil von 33 % (<see cref="HuggerUmklammerungSchadensmultiplikator"/>
    /// = 0,67, siehe <see cref="StatModifierSet.AttackDamageMultiplier"/>) statt
    /// absolut −100. Bei Grundschaden 60 bis 120 setzte −100 fast jeden Angreifer
    /// auf 0 (in der Stichprobe des defensiven Agenten Ursache von 44 % der
    /// Passzüge, in denen ein Angriff möglich war); ein Anteil wirkt bei allen
    /// Helden gleich stark, der umklammerte Angreifer behält zwei Drittel seines
    /// Schadens. Einordnung: Schildwall 50 % mit Abklingzeit, Mauer 25 % mit
    /// Abklingzeit, Umklammerung 33 % dauerhaft, solange angeheftet. Gerundet
    /// wird wie überall mit Bankers Rounding (z.B. 80 -> 53,6 -> 54, 120 ->
    /// 80,4 -> 80).
    ///
    /// DESIGNENTSCHEIDUNG "UMKLAMMERUNG PROZENTUAL, BONUS": auch der Bonus für ein
    /// VERBÜNDETES Ziel ist jetzt ein Anteil, +33 % (<see cref="HuggerUmklammerungBonusmultiplikator"/>
    /// = 1,33) statt absolut +100. Absolut bedeutete +167 % beim Illusionisten (60)
    /// und +83 % beim Bogenschützen (120) - dieselbe Ungleichbehandlung, die beim
    /// Abzug behoben wurde. Die Fähigkeit ist damit in sich symmetrisch: sie
    /// verstärkt Verbündete um denselben Anteil, um den sie Gegner schwächt. Zugleich
    /// eine deutliche Abschwächung: Illusionist unter Umklammerung 160 -> 80,
    /// Bogenschütze 220 -> 160.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 33,99% - schwächster Held im gesamten Katalog. Hugger
    /// trägt, solange er angeheftet ist, praktisch nie eigenen Schaden bei
    /// (Angriffssperre, siehe oben) - sein gesamter Kampfbeitrag läuft über
    /// den Bonus/Abzug der Umklammerung selbst. HP von 46 auf 50 und
    /// Grundschaden von 8 auf 10 angehoben (beide auf das jeweilige
    /// Korridor-Maximum, da seine eigene Angriffskraft ohnehin selten zum
    /// Tragen kommt) sowie der Umklammerung-Bonus/Abzug von ±4 auf ±10
    /// verstärkt, um seinen einzigen tatsächlichen Hebel spürbarer zu
    /// machen.
    /// </summary>
    public static readonly HeroDefinition Hugger = new(
        Id: "hugger",
        DisplayName: "Hugger",
        Role: HeroRole.Support,
        MaxHitPoints: 500, // Support-Korridor 450-550, oberer Rand, ×10 skaliert (Balancing vor Skalierung: 46 -> 50, siehe Klassendokumentation)
        BaseAttackDamage: HuggerGrundschaden, // Designentscheidung "Hugger-Überarbeitung": 100 -> 60, bewusst unter dem Support-Korridor 80-100 (siehe Klassendokumentation)
        Speed: 11,
        MovementRange: 2,
        AttackRange: 1,
        Ability: new AbilityDefinition(
            Id: "umklammerung",
            DisplayName: "Umklammerung",
            IsPassive: false,
            TargetType: AbilityTargetType.AnyUnit,
            Range: 2,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "greifen",
                    DisplayName: "Greifen",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Umklammerung",
                            IsInstant: false,
                            Duration: null,
                            Stacking: StackingRule.ReplaceForSameSource,
                            StatModifiers: new StatModifierSet(AttackDamageMultiplier: HuggerUmklammerungBonusmultiplikator), // Designentscheidung "Umklammerung prozentual, Bonus": vorher absolut +100
                            StatModifiersForEnemy: new StatModifierSet(AttackDamageMultiplier: HuggerUmklammerungSchadensmultiplikator), // Designentscheidung "Umklammerung prozentual": vorher absolut -100 (siehe Klassendokumentation)
                            CausesSourceToFollowTarget: true),
                        new EffectTemplate(
                            Name: "Umklammerung",
                            IsInstant: false,
                            Duration: null,
                            Stacking: StackingRule.Replace,
                            AppliesToCaster: true,
                            PreventsMovement: true,
                            // Hugger-Überarbeitung: KEINE Angriffssperre mehr - angeheftet greift er
                            // mit halbem Schaden an und wird dadurch eine Runde lang angreifbar.
                            StatModifiers: new StatModifierSet(AttackDamageMultiplier: HuggerAngeheftetSchadensmultiplikator),
                            PreventsBeingTargetedByEnemies: true,
                            TargetingProtectionSuspendedByAttackRounds: HuggerExponiertRunden),
                    }),
                new AbilityMode(
                    Id: "loesen",
                    DisplayName: "Lösen",
                    IgnoresChosenTarget: true,
                    IsLegal: (unit, state) => state.GetEffectsOnUnit(unit.Id).Any(e => e.Name == "Umklammerung"),
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "UmklammerungLoesen", IsInstant: true, AppliesToCaster: true, CancelEffectName: "Umklammerung"),
                    }),
            }));

    /// <summary>
    /// Abklingzeit des Elementarschlags - gilt für BEIDE Modi (Feuer und Erde):
    /// 3 = zwei eigene Züge gesperrt (Designentscheidung "Echte Pausen", vorher 2).
    /// </summary>
    public const int ElementarmagierAbklingzeit = 3;

    /// <summary>Wirkungsdauer der Verlangsamung (Erde): 1 = das Ziel hat genau einen eigenen Zug mit −1 Bewegung (Designentscheidung "Echte Pausen", vorher 2).</summary>
    public const int ElementarmagierVerlangsamungDauer = 1;

    /// <summary>
    /// Dmg, Fernkämpfer: aktiv, wählbarer Modus auf einen Gegner in
    /// Reichweite. Feuer = Sofortschaden + ABKLINGENDER Schaden über mehrere
    /// Runden (30, dann 20, dann 10 - ×10 skaliert, siehe
    /// <see cref="EffectTemplate.DamagePerTickReductionPerTick"/>); Erde =
    /// Sofortschaden + Abzug auf die Bewegungsreichweite des Ziels. Beide
    /// Modi sind vollständig über bereits bestehende Wirkungsarten
    /// abbildbar (Sofortschaden + DoT bzw. Sofortschaden + Stat-Debuff) -
    /// keine Erweiterung des Fähigkeitensystems nötig.
    ///
    /// KEIN STAPELN DURCH MEHRFACHEINSATZ (Bachelorarbeit-Korrektur): erneutes
    /// Anzünden eines bereits brennenden Ziels darf NICHT zu einer zweiten,
    /// parallelen Schaden-über-Zeit-Instanz führen (z.B. 3x Feuer in Folge
    /// -> 3x30=90 Extra-Schaden statt der beabsichtigten Abklingfolge). Die
    /// "Verbrennung" nutzt dafür <see cref="StackingRule.RefreshDuration"/>:
    /// ein erneuter Einsatz ersetzt keine neue Instanz, sondern setzt bei der
    /// bestehenden sowohl die Restdauer als auch den bereits abgeklungenen
    /// Tick-Schaden auf den vollen Ausgangswert zurück (siehe
    /// <see cref="Effects.EffectSystem.Apply"/>) - ein wiederholtes Anzünden
    /// startet die 3-2-1-Folge also lediglich neu, statt sich aufzusummieren.
    ///
    /// ABKLINGZEIT (Bachelorarbeit-Balancing - strukturelles Problem, das
    /// diese Abklingzeit beheben soll): beide Modi liefern Sofortschaden
    /// PLUS einen anhaltenden Zusatzeffekt (DoT bzw. Bewegungsabzug) und
    /// dominieren damit den einfachen Grundangriff (nur Sofortschaden ohne
    /// Zusatzwirkung) strikt - der Elementarmagier hätte faktisch drei
    /// gleichwertig nutzbare Angriffsoptionen pro Zug, ohne dass der
    /// Grundangriff je die bessere Wahl wäre. 2 Runden Abklingzeit
    /// erzwingen, dass mindestens jede zweite Runde der (schwächere, aber
    /// abklingzeitfreie) Grundangriff die einzige Angriffsoption ist -
    /// VORLÄUFIGER Startwert, noch nicht über Simulationen ausbalanciert.
    ///
    /// BALANCING (Vollständiger Basislauf, 257.040 Partien, Zufallsagent):
    /// Siegquote 61,67% - deutlich überdurchschnittlich, zweitstärkster
    /// Überperformer. Beide Modi liefern Sofortschaden PLUS anhaltende
    /// Zusatzwirkung (siehe oben) - entsprechend an beiden Modi gedreht statt
    /// nur an einem: Feuerball 10 -> 3, Verbrennung zusätzlich von konstant
    /// 4/Tick (Summe 12 über 3 Runden) auf eine ABKLINGENDE Folge 3-2-1
    /// (Summe 6) umgestellt - sowohl inhaltlich (das Feuer verlischt
    /// natürlich, statt konstant zu brennen) als auch als weitere,
    /// eigenständige Absenkung neben der reinen Zahlenkürzung -, Erdschlag
    /// 8 -> 7 (alle vier Werte hier wie im gesamten Katalog seither ×10
    /// skaliert, siehe Klassendokumentation Punkt 3: 30/20-10-10/70).
    /// BaseAttackDamage bewusst unverändert, da die Überperformance
    /// sichtbar aus der Fähigkeit selbst kommt, nicht aus dem Grundangriff.
    ///
    /// DESIGNENTSCHEIDUNG "ECHTE PAUSEN": Verlangsamung (Erde) Dauer 2 -> 1
    /// (<see cref="ElementarmagierVerlangsamungDauer"/>), Abklingzeit 2 -> 3
    /// (<see cref="ElementarmagierAbklingzeit"/>). Vorher endete die Verlangsamung
    /// genau zu Beginn des Zugs, in dem die Fähigkeit wieder frei war - ein
    /// Elementarmagier konnte dasselbe Ziel lückenlos verlangsamen (Helden mit
    /// Bewegung 1 dauerhaft auf 0). Jetzt: eine Runde verlangsamt, danach zwei
    /// Runden, in denen keine Verlangsamung erfolgen kann (Pause = Abklingzeit −
    /// Dauer = 2 Runden). Weil die Abklingzeit an der Fähigkeit hängt, gilt sie
    /// auch für Feuer: der Feuerball ist nur noch alle drei Runden einsetzbar.
    /// ACHTUNG: die Verbrennung (Dauer 3) bleibt trotzdem LÜCKENLOS erneuerbar - sie
    /// endet genau zu Beginn des Zugs, in dem die Fähigkeit wieder frei ist (Pause
    /// 3 − 3 = 0); vorher (Abklingzeit 2) wurde sie sogar vor dem Ablauf erneuert.
    /// Eine echte Unterbrechung verlangte Abklingzeit ≥ 4 oder Dauer ≤ 2.
    /// </summary>
    public static readonly HeroDefinition Elementarmagier = new(
        Id: "elementarmagier",
        DisplayName: "Elementarmagier",
        Role: HeroRole.Dmg,
        MaxHitPoints: 320, // Dmg-Korridor 280-450, ×10 skaliert (Fernkämpfer - weniger als der Nahkämpfer Berserker)
        BaseAttackDamage: 100, // Dmg-Korridor 80-150, ×10 skaliert
        Speed: 14,
        MovementRange: 1,
        AttackRange: 2,
        Ability: new AbilityDefinition(
            Id: "elementarschlag",
            DisplayName: "Elementarschlag",
            IsPassive: false,
            TargetType: AbilityTargetType.Enemy,
            Range: 2,
            CooldownRounds: ElementarmagierAbklingzeit, // Designentscheidung "Echte Pausen": 2 -> 3 (gilt für BEIDE Modi)
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "feuer",
                    DisplayName: "Feuer",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "Feuerball", IsInstant: true, InstantDamage: 30), // Balancing vor Skalierung: 10 -> 3, ×10 skaliert
                        new EffectTemplate(
                            Name: "Verbrennung",
                            IsInstant: false,
                            Duration: 3,
                            Stacking: StackingRule.RefreshDuration,
                            DamagePerTick: 30, // Balancing vor Skalierung: konstant 4 -> abklingend, Start bei 3, ×10 skaliert
                            DamagePerTickReductionPerTick: 10), // ×10 skaliert; 30, dann 20, dann 10 (siehe Klassendokumentation)
                    }),
                new AbilityMode(
                    Id: "erde",
                    DisplayName: "Erde",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "Erdschlag", IsInstant: true, InstantDamage: 70), // Balancing vor Skalierung: 8 -> 7, ×10 skaliert
                        new EffectTemplate(
                            Name: "Verlangsamung",
                            IsInstant: false,
                            Duration: ElementarmagierVerlangsamungDauer, // Designentscheidung "Echte Pausen": 2 -> 1
                            Stacking: StackingRule.RefreshDuration,
                            StatModifiers: new StatModifierSet(MovementRangeDelta: -1)),
                    }),
            }));

    /// <summary>
    /// Anzahl HP, um die sich der Schadensbonus des Bogenschützen je
    /// Ladungsrunde erhöht, und die dabei geltende Obergrenze - siehe
    /// <see cref="StackingRule.Accumulate"/>. Beide ×10 skaliert (siehe
    /// Klassendokumentation Punkt 3: vormals 5 bzw. 20).
    /// </summary>
    private const int BogenschuetzeLadungProRunde = 50;
    private const int BogenschuetzeMaximaleLadung = 200;

    /// <summary>
    /// Dmg, Fernkämpfer: aktiv, wählbarer Modus. "Laden" erhöht mit jeder
    /// Runde, in der er erneut gewählt wird, den Schadensbonus des nächsten
    /// Angriffs um <see cref="BogenschuetzeLadungProRunde"/>, gedeckelt bei
    /// <see cref="BogenschuetzeMaximaleLadung"/> (<see cref="StackingRule.Accumulate"/>)
    /// und macht bewegungsunfähig, solange die Ladung hält
    /// (<see cref="EffectTemplate.PreventsMovement"/> - auf demselben Effekt
    /// wie der Bonus selbst, daher automatisch synchron: ein Angriff
    /// verbraucht beides gemeinsam, siehe <see cref="Combat.CombatSystem.TryAttackDetailed"/>).
    /// "Abbrechen" ist nur wählbar, solange tatsächlich aufgeladen wird
    /// (<see cref="AbilityMode.IsLegal"/>) und löscht die Ladung ohne
    /// weitere Wirkung (<see cref="EffectTemplate.CancelEffectName"/>).
    /// </summary>
    public static readonly HeroDefinition Bogenschuetze = new(
        Id: "bogenschuetze",
        DisplayName: "Bogenschütze",
        Role: HeroRole.Dmg,
        MaxHitPoints: 280, // Dmg-Korridor 280-450, unterer Rand, ×10 skaliert (reinster Fernkämpfer - geringste HP im gesamten Katalog)
        BaseAttackDamage: 120, // Dmg-Korridor 80-150, ×10 skaliert
        Speed: 15,
        MovementRange: 2,
        AttackRange: 3,
        Ability: new AbilityDefinition(
            Id: "aufladen",
            DisplayName: "Aufladen",
            IsPassive: false,
            TargetType: AbilityTargetType.Self,
            Range: 0,
            Effects: Array.Empty<EffectTemplate>(),
            Modes: new[]
            {
                new AbilityMode(
                    Id: "laden",
                    DisplayName: "Laden",
                    Effects: new[]
                    {
                        new EffectTemplate(
                            Name: "Aufladen",
                            IsInstant: false,
                            Duration: null,
                            Stacking: StackingRule.Accumulate,
                            PreventsMovement: true,
                            BonusDamageForNextAttack: BogenschuetzeLadungProRunde,
                            MaximumAccumulatedBonusDamage: BogenschuetzeMaximaleLadung),
                    }),
                new AbilityMode(
                    Id: "abbrechen",
                    DisplayName: "Abbrechen",
                    Effects: new[]
                    {
                        new EffectTemplate(Name: "AufladenAbbrechen", IsInstant: true, CancelEffectName: "Aufladen"),
                    },
                    IsLegal: (unit, state) => state.GetEffectsOnUnit(unit.Id).Any(e => e.Name == "Aufladen")),
            }));

    /// <summary>Baut einen neuen Standardkatalog mit allen neun Helden.</summary>
    public static HeroCatalog CreateDefaultCatalog() => new(new[]
    {
        Schildtraeger, Elfe, Berserker,
        Wand, Illusionist, Musiker, Hugger,
        Elementarmagier, Bogenschuetze,
    });
}
