// Demo-/Verifikationsprogramm für Heldenklassen und Fähigkeitensystem: eine
// Partie mit den drei Beispielhelden (Schildträger, Elfe, Berserker) gegen
// drei Standardfiguren, inklusive Ausgangsstellung, vollständiger
// Ereignis-Chronik (Bewegung, Angriff, Fähigkeitseinsatz, Effekt-
// Anwendung/-Ablauf, Klon-Erzeugung/-Verschwinden) und Endergebnis.
// Anschließend zwei Batch-Simulationen: (1) ein Spiegel-Match desselben
// Heldenteams gegen sich selbst über viele Partien, als Plausibilitätsprüfung
// mit ECHTEN Helden (nicht nur den einfachen Standardfiguren), und (2) der
// Nachweis, dass ein veränderter Heldenkatalog (Anforderung 3: "20% mehr
// Schaden") sich messbar auf die Siegverteilung auswirkt. Enthält keine
// eigene Spiellogik, sondern nutzt ausschließlich die öffentliche API von
// TacticsCore.

using TacticsCore;
using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Batch;
using TacticsCore.Board;
using TacticsCore.Demo;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Persistence;
using TacticsCore.Rendering;
using TacticsCore.Setup;
using TacticsCore.Units;

// Modus über den ersten Kommandozeilen-Parameter wählbar (Anforderung 15
// bzw. "Konsolenprogramm um zwei Modi erweitern"): ohne Parameter läuft wie
// bisher die vollständige, unbeaufsichtigte Demo durch. "interactive"
// startet eine selbst spielbare Partie, "batch" einen kleinen, zeit-
// gemessenen Simulationslauf über wenige Paarungen, "export-replay" speichert
// EINE konkrete, per Kommandozeile bestimmte Partie vollständig (inklusive
// Helden-Zuordnung) für die spätere Betrachtung in der Godot-Oberfläche
// (siehe TacticsCore.Godot) - genau der Anwendungsfall "eine im Batch-Lauf
// auffällig gewordene Partie gezielt visuell nachvollziehen".
string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "demo";

switch (mode)
{
    case "interactive":
        RunInteractiveMode();
        break;

    case "batch":
        RunSmallBatchDemo();
        break;

    case "export-replay":
        RunExportReplay(args.Skip(1).ToArray());
        break;

    default:
        RunHeroesVersusStandardFiguresMatch();
        Console.WriteLine();
        Console.WriteLine(new string('=', 76));
        Console.WriteLine();
        RunHeroMirrorPlausibilityCheck(matchCount: 10_000);
        Console.WriteLine();
        Console.WriteLine(new string('=', 76));
        Console.WriteLine();
        RunCatalogOverrideComparison(matchesPerCatalog: 500);
        Console.WriteLine();
        Console.WriteLine(new string('=', 76));
        Console.WriteLine();
        RunNewHeroesShowcaseMatch();
        break;
}

// =======================================================================
// Teil 1: Helden gegen Standardfiguren, vollständig nacherzählt.
// =======================================================================
static void RunHeroesVersusStandardFiguresMatch()
{
    Console.WriteLine("=== TacticsCore - Helden gegen Standardfiguren ===");
    Console.WriteLine();

    const int seed = 7; // fest gewählt, damit die Ausgabe bei jedem Lauf des Demos identisch ist
    var (state, _) = BuildHeroLineup();
    var names = BuildDisplayNames(state);

    Console.WriteLine("Ausgangsstellung (Team A: Schildträger=T, Elfe=E, Berserker=K | Team B: Standardfiguren X/Y/Z):");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));

    var controller = new MatchController(state, new RandomAgent(), new RandomAgent(), seed, recordReplay: true);
    var result = controller.Run();

    Console.WriteLine($"Seed: {seed}   Startteam: {result.StartingTeam}");
    Console.WriteLine();

    foreach (var roundGroup in result.Replay!.Entries.GroupBy(e => e.RoundNumber).OrderBy(g => g.Key))
    {
        Console.WriteLine($"Runde {roundGroup.Key}:");
        foreach (var entry in roundGroup)
        {
            Console.WriteLine($"  {DescribeEntry(entry, names)}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("--- Endergebnis ---");
    Console.WriteLine($"Sieger: {result.Winner}   Ende durch: {result.EndReason}   Runden gespielt: {result.RoundsPlayed}");
    PrintTeamResult(result.TeamAResult);
    PrintTeamResult(result.TeamBResult);
    Console.WriteLine();

    Console.WriteLine("Endstellung:");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));
}

static string DescribeEntry(ReplayEntry entry, Dictionary<Guid, string> names)
{
    string actor = Name(names, entry.ActingUnitId);
    return entry.ActionType switch
    {
        ReplayActionType.Move =>
            $"[{entry.ActingTeam}] {actor} zieht von {entry.MoveSource} nach {entry.MoveTarget}",

        ReplayActionType.Attack =>
            $"[{entry.ActingTeam}] {actor} greift {Name(names, entry.AttackTargetUnitId!.Value)} an ({entry.DamageDealt} Schaden)",

        ReplayActionType.UseAbility =>
            $"[{entry.ActingTeam}] {actor} setzt Fähigkeit '{entry.AbilityId}' ein" +
            (entry.AttackTargetUnitId is Guid targetId ? $" (Ziel: {Name(names, targetId)})" : "") +
            (entry.EffectField is not null ? $" (Zielfeld: {entry.EffectField})" : "") +
            (entry.ModeId is not null ? $" [Modus: {entry.ModeId}]" : ""),

        ReplayActionType.EffectApplied =>
            $"    -> Effekt '{entry.EffectName}' von {Name(names, entry.ActingUnitId)} angewendet" +
            (entry.RelatedUnitId is Guid onUnit ? $" auf {Name(names, onUnit)}" : "") +
            (entry.EffectField is not null ? $" auf Feld {entry.EffectField}" : ""),

        ReplayActionType.EffectExpired =>
            $"    -> Effekt '{entry.EffectName}' ist abgelaufen",

        ReplayActionType.CloneSpawned =>
            $"[{entry.ActingTeam}] {actor} erzeugt einen Klon (Erzeuger: {Name(names, entry.RelatedUnitId!.Value)})",

        ReplayActionType.CloneDespawned =>
            $"[{entry.ActingTeam}] Klon von {Name(names, entry.RelatedUnitId!.Value)} verschwindet",

        _ => $"[{entry.ActingTeam}] {actor}: unbekanntes Ereignis {entry.ActionType}",
    };
}

static void PrintTeamResult(TeamResult team)
{
    Console.WriteLine($"  {team.Team}: {team.SurvivingUnitCount} überlebende Einheit(en), " +
        $"{team.TotalCurrentHitPoints}/{team.TotalMaxHitPoints} HP ({team.RemainingHitPointsFraction:P1})");
}

static Dictionary<Guid, string> BuildDisplayNames(GameState state)
{
    var names = new Dictionary<Guid, string>();
    foreach (var unit in state.Units)
    {
        names[unit.Id] = unit switch
        {
            Hero { Definition.Id: "schildtraeger" } => "T",
            Hero { Definition.Id: "elfe" } => "E",
            Hero { Definition.Id: "berserker" } => "K",
            _ => "", // Standardfiguren bekommen unten X/Y/Z zugewiesen
        };
    }

    var unnamed = state.Units.Where(u => names[u.Id] == "").OrderBy(u => u.Position.Q).ThenBy(u => u.Position.R).ThenBy(u => u.Id).ToList();
    string[] fallbackLabels = { "X", "Y", "Z" };
    for (int i = 0; i < unnamed.Count; i++)
    {
        names[unnamed[i].Id] = i < fallbackLabels.Length ? fallbackLabels[i] : $"U{i + 1}";
    }

    return names;
}

static string Name(Dictionary<Guid, string> names, Guid id) =>
    names.TryGetValue(id, out var name) ? name : id.ToString("N")[..8];

// =======================================================================
// Gemeinsamer Aufbau: die drei ursprünglichen Beispielhelden (Schildträger,
// Elfe, Berserker) auf Team A, drei Standardfiguren auf Team B, jeweils auf
// den Startfeldern ihrer Blüte. Bewusst ein eigener, kleiner Katalog statt
// DefaultHeroes.CreateDefaultCatalog() (der inzwischen alle NEUN Helden
// enthält) - Teil 1-3 dieses Demos erzählen gezielt DIESES Trio nach (samt
// fest zugeordneter Kurzbezeichnungen T/E/K), nicht "irgendwelche drei
// Helden aus dem vollen Katalog". Die sechs neuen Helden haben ihren
// eigenen Auftritt in Teil 4 (siehe RunNewHeroesShowcaseMatch).
// =======================================================================
static HeroCatalog BuildShowcaseTrioCatalog() => new(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, DefaultHeroes.Berserker });

static (GameState State, Board Board) BuildHeroLineup(HeroCatalog? catalog = null)
{
    var board = new Board();
    var state = new GameState(board);
    catalog ??= BuildShowcaseTrioCatalog();

    var heroStartFields = board.GetStartFields(FieldType.BlossomA).ToList();
    var heroDefinitions = catalog.All; // stabil nach Id sortiert: berserker, elfe, schildtraeger
    for (int i = 0; i < heroDefinitions.Count && i < heroStartFields.Count; i++)
    {
        state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamA, heroStartFields[i].Coordinate, heroDefinitions[i]));
    }

    foreach (var startField in board.GetStartFields(FieldType.BlossomB))
    {
        state.AddUnit(new Unit(Guid.NewGuid(), Team.TeamB, startField.Coordinate));
    }

    return (state, board);
}

// =======================================================================
// Teil 2: Plausibilitätsprüfung mit einem ECHTEN Heldenteam gegen ein
// identisches Heldenteam (statt wie bisher nur mit Standardfiguren) -
// beide Seiten nutzen exakt denselben Heldenkatalog, also exakt dieselben
// Fähigkeiten und Werte. Bei symmetrischer Aufstellung und Zufallsagenten
// auf beiden Seiten sollte die Siegverteilung auch mit Fähigkeiten und
// Effekten im Spiel annähernd ausgeglichen bleiben.
// =======================================================================
static void RunHeroMirrorPlausibilityCheck(int matchCount)
{
    Console.WriteLine($"=== TacticsCore - Spiegel-Match: Heldenteam gegen identisches Heldenteam ({matchCount} Partien) ===");
    Console.WriteLine();
    Console.WriteLine("Team A UND Team B bestehen hier aus denselben drei Beispielhelden");
    Console.WriteLine("(Schildträger, Elfe, Berserker) - keine Standardfiguren mehr auf keiner");
    Console.WriteLine("Seite. Geprüft wird, ob Fähigkeiten/Effekte einen versteckten Vorteil für");
    Console.WriteLine("ein bestimmtes Team einbauen, den die einfacheren Standardfiguren-Partien");
    Console.WriteLine("nicht aufgedeckt hätten.");
    Console.WriteLine();

    var catalog = BuildShowcaseTrioCatalog();

    int teamAWins = 0;
    int teamBWins = 0;
    int draws = 0;
    int roundLimitEndings = 0;
    long totalRoundsPlayed = 0;

    for (int seed = 0; seed < matchCount; seed++)
    {
        var state = BuildMirrorHeroLineup(catalog);
        var result = new MatchController(state, new RandomAgent(), new RandomAgent(), seed).Run();

        switch (result.Winner)
        {
            case MatchWinner.TeamA:
                teamAWins++;
                break;
            case MatchWinner.TeamB:
                teamBWins++;
                break;
            default:
                draws++;
                break;
        }

        if (result.EndReason == MatchEndReason.RoundLimit)
        {
            roundLimitEndings++;
        }

        totalRoundsPlayed += result.RoundsPlayed;
    }

    double teamAWinRate = (double)teamAWins / matchCount;
    double teamBWinRate = (double)teamBWins / matchCount;
    double drawRate = (double)draws / matchCount;
    double averageRounds = (double)totalRoundsPlayed / matchCount;
    double roundLimitShare = (double)roundLimitEndings / matchCount;

    Console.WriteLine($"Team A Siege:          {teamAWins,6}  ({teamAWinRate:P1})");
    Console.WriteLine($"Team B Siege:          {teamBWins,6}  ({teamBWinRate:P1})");
    Console.WriteLine($"Unentschieden:         {draws,6}  ({drawRate:P1})");
    Console.WriteLine();
    Console.WriteLine($"Durchschnittliche Rundenzahl:      {averageRounds:F1}");
    Console.WriteLine($"Anteil Rundenlimit-Enden:          {roundLimitShare:P1}");
    Console.WriteLine();
    Console.WriteLine("Plausibilitätshinweis: Da beide Teams identisch aufgestellt sind, sollte");
    Console.WriteLine("keine Seite einen strukturellen Vorteil haben. Eine deutliche Schieflage");
    Console.WriteLine("würde auf einen Vorteil des Startteams oder einen Implementierungsfehler in");
    Console.WriteLine("Zugreihenfolge, Fähigkeiten oder Kampf hindeuten - trotz identischer");
    Console.WriteLine("Standardfiguren-Ergebnisse aus Teil 1 des vorherigen Schritts.");
}

/// <summary>Baut eine Partie mit demselben Heldenkatalog auf BEIDEN Seiten - echtes Spiegel-Match statt Helden gegen Standardfiguren.</summary>
static GameState BuildMirrorHeroLineup(HeroCatalog catalog)
{
    var board = new Board();
    var state = new GameState(board);
    var heroDefinitions = catalog.All; // stabil nach Id sortiert

    var teamAStartFields = board.GetStartFields(FieldType.BlossomA).ToList();
    var teamBStartFields = board.GetStartFields(FieldType.BlossomB).ToList();

    for (int i = 0; i < heroDefinitions.Count && i < teamAStartFields.Count; i++)
    {
        state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamA, teamAStartFields[i].Coordinate, heroDefinitions[i]));
    }

    for (int i = 0; i < heroDefinitions.Count && i < teamBStartFields.Count; i++)
    {
        state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamB, teamBStartFields[i].Coordinate, heroDefinitions[i]));
    }

    return state;
}

// =======================================================================
// Teil 3: Zeigt, dass ein veränderter Heldenkatalog (Anforderung 3) sich
// nachweisbar auf die Siegverteilung auswirkt - ohne jede Codeänderung,
// nur durch eine abgewandelte HeroDefinition ("20% mehr Schaden" für den
// Berserker).
// =======================================================================
static void RunCatalogOverrideComparison(int matchesPerCatalog)
{
    Console.WriteLine("=== TacticsCore - Heldenkatalog-Vergleich (Anforderung 3) ===");
    Console.WriteLine();
    Console.WriteLine("Team A nutzt in beiden Reihen denselben Heldenkatalog gegen drei");
    Console.WriteLine("Standardfiguren auf Team B - einmal mit dem Standardkatalog, einmal mit");
    Console.WriteLine("einem Katalog, in dem der Berserker 20% mehr Grundschaden hat. Beide");
    Console.WriteLine("Katalog-Objekte sind reine Daten - kein TacticsCore-Code wurde geändert.");
    Console.WriteLine();

    var defaultCatalog = BuildShowcaseTrioCatalog();
    var boostedBerserker = DefaultHeroes.Berserker with { BaseAttackDamage = (int)Math.Round(DefaultHeroes.Berserker.BaseAttackDamage * 1.2) };
    var boostedCatalog = new HeroCatalog(new[] { DefaultHeroes.Schildtraeger, DefaultHeroes.Elfe, boostedBerserker });

    Console.WriteLine($"Berserker-Grundschaden Standard: {DefaultHeroes.Berserker.BaseAttackDamage}   Verändert (+20%): {boostedBerserker.BaseAttackDamage}");
    Console.WriteLine();

    PrintCatalogWinRate("Standardkatalog", defaultCatalog, matchesPerCatalog);
    PrintCatalogWinRate("Katalog mit +20% Berserker-Schaden", boostedCatalog, matchesPerCatalog);

    Console.WriteLine();
    Console.WriteLine("Plausibilitätshinweis: Der veränderte Katalog sollte Team A eine klar");
    Console.WriteLine("höhere Siegrate verschaffen als der Standardkatalog - bleibt sie");
    Console.WriteLine("unverändert, würde das auf einen Fehler in der Katalog-Anbindung hindeuten.");
}

// =======================================================================
// Interaktiver Modus (Anforderung 15): eine vollständige Partie selbst
// spielen, wahlweise Mensch gegen Zufallsagent oder Mensch gegen Mensch
// (lokal am selben Bildschirm, abwechselnd). Nach jedem abgeschlossenen Zug
// wird die ASCII-Karte (HP, aktive Effekte, Klone, gesperrte Felder - siehe
// AsciiRenderer) neu ausgegeben, angestoßen über den Live-Ereignis-Callback
// von MatchController (TurnEnded) statt über eine eigene Wiederholschleife -
// das ist derselbe Mechanismus, den auch eine spätere Oberfläche nutzen
// würde (Anforderung 16/17).
// =======================================================================
static void RunInteractiveMode()
{
    Console.WriteLine("=== TacticsCore - Interaktiver Modus ===");
    Console.WriteLine();
    Console.WriteLine("Wer spielt? [1] Mensch gegen Zufallsagent   [2] Mensch gegen Mensch");
    Console.Write("Wahl: ");
    bool humanVsHuman = Console.ReadLine()?.Trim() == "2";

    var board = new Board();
    var state = new GameState(board);

    var teamAUnits = board.GetStartFields(FieldType.BlossomA).Select(f => new Unit(Guid.NewGuid(), Team.TeamA, f.Coordinate)).ToList();
    var teamBUnits = board.GetStartFields(FieldType.BlossomB).Select(f => new Unit(Guid.NewGuid(), Team.TeamB, f.Coordinate)).ToList();
    foreach (var unit in teamAUnits.Concat(teamBUnits))
    {
        state.AddUnit(unit);
    }

    var names = new Dictionary<Guid, string>();
    string[] labelsA = { "A1", "A2", "A3" };
    string[] labelsB = { "B1", "B2", "B3" };
    for (int i = 0; i < teamAUnits.Count; i++) names[teamAUnits[i].Id] = labelsA[i];
    for (int i = 0; i < teamBUnits.Count; i++) names[teamBUnits[i].Id] = labelsB[i];
    Func<Unit, string> unitLabel = u => Name(names, u.Id);

    IAgent teamAAgent = new HumanAgent(new ConsoleHumanInputSource(unitLabel));
    IAgent teamBAgent = humanVsHuman ? new HumanAgent(new ConsoleHumanInputSource(unitLabel)) : new RandomAgent();

    Console.WriteLine();
    Console.WriteLine("Ausgangsstellung (Team A: A1-A3, Team B: B1-B3):");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: unitLabel));

    // Kein fester Seed: ein manuell gespielter Einzeldurchlauf mit
    // menschlicher Eingabe ist ohnehin nicht reproduzierbar (die
    // Determinismus-Anforderung betrifft die SIMULATION, nicht das
    // interaktive Spielen) - der Zufallsagent (falls beteiligt) bekommt
    // trotzdem korrekt genau EINEN, hier einmalig gewürfelten Strom.
    int seed = new Random().Next();
    var controller = new MatchController(state, teamAAgent, teamBAgent, seed, onEvent: entry =>
    {
        if (entry.ActionType == ReplayActionType.TurnEnded)
        {
            Console.WriteLine();
            Console.WriteLine(AsciiRenderer.Render(state, unitLabel: unitLabel));
        }
    });

    var result = controller.Run();

    Console.WriteLine();
    Console.WriteLine("--- Partie beendet ---");
    Console.WriteLine($"Sieger: {result.Winner}   Ende durch: {result.EndReason}   Runden gespielt: {result.RoundsPlayed}");
}

// =======================================================================
// Kleiner Batch-Lauf (VERIFIKATION b): wenige Paarungen mit begrenzter
// Ausgangslagen-Zahl, dafür mit Laufzeitmessung UND Hochrechnung auf einen
// vollständigen Lauf über alle 72 Ausgangslagen bzw. über alle möglichen
// Paarungen - damit lässt sich abschätzen, wie lange ein vollständiger
// Untersuchungslauf tatsächlich dauern würde, ohne ihn dafür schon einmal
// vollständig ausführen zu müssen.
// =======================================================================
static void RunSmallBatchDemo()
{
    Console.WriteLine("=== TacticsCore - Kleiner Batch-Lauf mit Laufzeitmessung ===");
    Console.WriteLine();

    var board = new Board();
    var catalog = DefaultHeroes.CreateDefaultCatalog();
    var allCompositions = CompositionGenerator.GenerateAllCompositions(catalog);

    // Fünf willkürliche, aber feste Paarungen statt aller 84x84 möglichen -
    // genug für eine belastbare Laufzeit-Hochrechnung, bei überschaubarer
    // Gesamtlaufzeit dieser Demo selbst.
    var pairings = Enumerable.Range(0, Math.Min(5, allCompositions.Count))
        .Select(i => (TeamA: allCompositions[i], TeamB: allCompositions[allCompositions.Count - 1 - i]))
        .ToList();

    const int setupsPerPairing = 12; // von 72 möglichen je Paarung

    Console.WriteLine($"{pairings.Count} Paarungen x {setupsPerPairing} (von 72) Ausgangslagen = {pairings.Count * setupsPerPairing} Partien, {Environment.ProcessorCount} Threads.");
    Console.WriteLine();

    var result = BatchSimulationRunner.Run(
        board, pairings, AgentConfiguration.RandomVsRandom(),
        baseSeed: 2026, maxSetupsPerPairing: setupsPerPairing, degreeOfParallelism: Environment.ProcessorCount,
        onProgress: p => Console.Write($"\rFortschritt: {p.CompletedMatches}/{p.TotalMatches} Partien ({p.Elapsed.TotalSeconds:F1}s)   "));

    Console.WriteLine();
    Console.WriteLine();

    int playedMatches = pairings.Count * setupsPerPairing;
    Console.WriteLine($"Gesamtlaufzeit: {result.ElapsedTime.TotalSeconds:F2} s für {playedMatches} Partien ({result.ElapsedTime.TotalMilliseconds / playedMatches:F2} ms/Partie im Schnitt).");

    double secondsPerPairingSample = result.ElapsedTime.TotalSeconds / pairings.Count;
    double secondsPerPairingFull = secondsPerPairingSample / setupsPerPairing * 72;
    long allPossiblePairings = (long)allCompositions.Count * allCompositions.Count;
    double secondsAllPairingsFull = secondsPerPairingFull * allPossiblePairings;

    Console.WriteLine();
    Console.WriteLine("Hochrechnung (linear aus der obigen Stichprobe, keine echte Messung):");
    Console.WriteLine($"  Eine Paarung mit allen 72 Ausgangslagen:                 ca. {secondsPerPairingFull:F2} s");
    Console.WriteLine($"  ALLE {allCompositions.Count}x{allCompositions.Count} = {allPossiblePairings} möglichen Paarungen mit je 72 Ausgangslagen: ca. {TimeSpan.FromSeconds(secondsAllPairingsFull):g}");
    Console.WriteLine();

    foreach (var pr in result.PairingResults)
    {
        Console.WriteLine($"{pr.TeamA,-45} vs {pr.TeamB,-45}: A {pr.TeamAWinRate,6:P1} / B {pr.TeamBWinRate,6:P1} / Remis {pr.DrawRate,5:P1}   Runden im Schnitt {pr.AverageRoundsPlayed:F1}");
    }
}

// =======================================================================
// Export einer einzelnen, konkreten Partie samt Helden-Zuordnung als
// SavedMatch-JSON (siehe TacticsCore.Persistence) - Bindeglied zwischen
// Batch-Lauf (Konsole) und Godot-Oberfläche (Anforderung 8/9 der Godot-
// Aufgabenstellung: "Godot ist nur für einzelne Partien und Replays
// gedacht"). Aufruf z.B.:
//   dotnet run --project TacticsCore.Demo -- export-replay ^
//     schildtraeger elfe berserker wand hugger musiker 5 42 match.json
// (drei Helden Team A, drei Helden Team B, Ausgangslagen-Index 0-71,
// Seed, Zieldatei - siehe die Fehlermeldung unten für die genaue Syntax.)
// =======================================================================
static void RunExportReplay(string[] exportArgs)
{
    if (exportArgs.Length != 9)
    {
        Console.WriteLine("Verwendung: export-replay <heldA1> <heldA2> <heldA3> <heldB1> <heldB2> <heldB3> <ausgangslagenIndex 0-71> <seed> <zieldatei.json>");
        Console.WriteLine("Helden-Ids: " + string.Join(", ", DefaultHeroes.CreateDefaultCatalog().All.Select(h => h.Id)));
        return;
    }

    var catalog = DefaultHeroes.CreateDefaultCatalog();
    var teamA = new TeamComposition(new[] { catalog.Get(exportArgs[0]), catalog.Get(exportArgs[1]), catalog.Get(exportArgs[2]) });
    var teamB = new TeamComposition(new[] { catalog.Get(exportArgs[3]), catalog.Get(exportArgs[4]), catalog.Get(exportArgs[5]) });
    int setupIndex = int.Parse(exportArgs[6]);
    int seed = int.Parse(exportArgs[7]);
    string outputPath = exportArgs[8];

    var board = new Board();
    var allSetups = AusgangslagenGenerator.GenerateAllSetups(teamA, teamB, board);
    if (setupIndex < 0 || setupIndex >= allSetups.Count)
    {
        Console.WriteLine($"Ausgangslagen-Index muss zwischen 0 und {allSetups.Count - 1} liegen.");
        return;
    }

    var setup = allSetups[setupIndex];
    var state = AusgangslagenGenerator.BuildGameState(setup, board);
    var result = new MatchController(state, new RandomAgent(), new RandomAgent(), seed, recordReplay: true).Run(setup.StartingTeam);

    // Held-Zuordnung exakt nach dem Vergabeschema von
    // AusgangslagenGenerator.BuildGameState (Team A: 1-3, Team B: 101-103,
    // in der Reihenfolge der Ausgangslage) rekonstruieren.
    var units = new List<SavedUnitInfo>();
    int id = 1;
    foreach (var (hero, position) in setup.TeamAUnits)
    {
        units.Add(new SavedUnitInfo(new Guid(id++, 0, 0, new byte[8]), Team.TeamA, position, hero.Id));
    }
    id = 101;
    foreach (var (hero, position) in setup.TeamBUnits)
    {
        units.Add(new SavedUnitInfo(new Guid(id++, 0, 0, new byte[8]), Team.TeamB, position, hero.Id));
    }

    SavedMatchIO.Save(new SavedMatch(units, result.Replay!), outputPath);

    Console.WriteLine($"Partie gespeichert: {outputPath}");
    Console.WriteLine($"Sieger: {result.Winner}   Ende durch: {result.EndReason}   Runden: {result.RoundsPlayed}   Ereignisse: {result.Replay!.Entries.Count}");
}

static void PrintCatalogWinRate(string label, HeroCatalog catalog, int matchCount)
{
    int teamAWins = 0;
    for (int seed = 0; seed < matchCount; seed++)
    {
        var (state, _) = BuildHeroLineup(catalog);
        var result = new MatchController(state, new RandomAgent(), new RandomAgent(), seed).Run();
        if (result.Winner == MatchWinner.TeamA)
        {
            teamAWins++;
        }
    }

    double winRate = (double)teamAWins / matchCount;
    Console.WriteLine($"{label,-38}: Team-A-Siegrate {winRate:P1} ({teamAWins}/{matchCount})");
}

// =======================================================================
// Teil 4: die sechs neuen Helden (Wand, Illusionist, Musiker, Hugger,
// Elementarmagier, Bogenschütze) im Einsatz - eine Partie mit drei
// beliebigen von ihnen je Team, vollständig nacherzählt inklusive
// Fähigkeits- und Effektverlauf (dieselbe DescribeEntry-Chronik wie in
// Teil 1). Bewusst so gewählt, dass auf beiden Seiten unterschiedliche,
// neu hinzugekommene Mechaniken sichtbar werden: Team A Feldsperre/
// Umleitung (Wand), Anheftung (Hugger) und wählbarer Fernkampf-Modus
// (Elementarmagier); Team B Klon-Ersetzen (Illusionist), Flächen-Buff nur
// für Verbündete (Musiker) und aufladender/abbrechbarer Zustand
// (Bogenschütze).
// =======================================================================
static void RunNewHeroesShowcaseMatch()
{
    Console.WriteLine("=== TacticsCore - Die sechs neuen Helden im Einsatz ===");
    Console.WriteLine();

    const int seed = 21; // fest gewählt, damit die Ausgabe bei jedem Lauf des Demos identisch ist
    var state = BuildNewHeroesLineup();
    var names = BuildNewHeroesDisplayNames(state);

    Console.WriteLine("Ausgangsstellung (Team A: Wand=W, Hugger=H, Elementarmagier=M | Team B: Illusionist=I, Musiker=U, Bogenschütze=G):");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));

    var controller = new MatchController(state, new RandomAgent(), new RandomAgent(), seed, recordReplay: true);
    var result = controller.Run();

    Console.WriteLine($"Seed: {seed}   Startteam: {result.StartingTeam}");
    Console.WriteLine();

    foreach (var roundGroup in result.Replay!.Entries.GroupBy(e => e.RoundNumber).OrderBy(g => g.Key))
    {
        Console.WriteLine($"Runde {roundGroup.Key}:");
        foreach (var entry in roundGroup)
        {
            Console.WriteLine($"  {DescribeEntry(entry, names)}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("--- Endergebnis ---");
    Console.WriteLine($"Sieger: {result.Winner}   Ende durch: {result.EndReason}   Runden gespielt: {result.RoundsPlayed}");
    PrintTeamResult(result.TeamAResult);
    PrintTeamResult(result.TeamBResult);
    Console.WriteLine();

    Console.WriteLine("Endstellung:");
    Console.WriteLine(AsciiRenderer.Render(state, unitLabel: u => Name(names, u.Id)));
}

/// <summary>Team A: Wand, Hugger, Elementarmagier - Team B: Illusionist, Musiker, Bogenschütze - je auf den Startfeldern ihrer Blüte.</summary>
static GameState BuildNewHeroesLineup()
{
    var board = new Board();
    var state = new GameState(board);

    var teamA = new[] { DefaultHeroes.Wand, DefaultHeroes.Hugger, DefaultHeroes.Elementarmagier };
    var teamB = new[] { DefaultHeroes.Illusionist, DefaultHeroes.Musiker, DefaultHeroes.Bogenschuetze };

    var teamAStartFields = board.GetStartFields(FieldType.BlossomA).ToList();
    var teamBStartFields = board.GetStartFields(FieldType.BlossomB).ToList();

    for (int i = 0; i < teamA.Length; i++)
    {
        state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamA, teamAStartFields[i].Coordinate, teamA[i]));
    }

    for (int i = 0; i < teamB.Length; i++)
    {
        state.AddUnit(new Hero(Guid.NewGuid(), Team.TeamB, teamBStartFields[i].Coordinate, teamB[i]));
    }

    return state;
}

static Dictionary<Guid, string> BuildNewHeroesDisplayNames(GameState state)
{
    var names = new Dictionary<Guid, string>();
    foreach (var unit in state.Units)
    {
        names[unit.Id] = unit switch
        {
            Hero { Definition.Id: "wand" } => "W",
            Hero { Definition.Id: "hugger" } => "H",
            Hero { Definition.Id: "elementarmagier" } => "M",
            Hero { Definition.Id: "illusionist" } => "I",
            Hero { Definition.Id: "musiker" } => "U",
            Hero { Definition.Id: "bogenschuetze" } => "G",
            _ => unit.Id.ToString("N")[..8],
        };
    }

    return names;
}
