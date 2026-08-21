using System.Collections.Generic;
using TacticsCore;
using TacticsCore.Heroes;
using TacticsCore.Match;
using TacticsCore.Persistence;
using TacticsCore.Units;
using TacticsCoreGodot.Shared;
using GameBoard = TacticsCore.Board.Board;

namespace TacticsCoreGodot.Replay;

/// <summary>Ein einzelner Schritt der Zeitleiste: das Ereignis plus der Spielzustand unmittelbar danach.</summary>
public sealed record ReplayFrame(ReplayEntry Event, BoardRenderModel Model);

/// <summary>Die vollständig vorberechnete Zeitleiste: die Ausgangsstellung (vor dem ersten Ereignis) plus die Folge aller Schritte.</summary>
public sealed record ReplayData(BoardRenderModel InitialModel, IReadOnlyList<ReplayFrame> Frames);

/// <summary>
/// Baut aus einer gespeicherten Partie (<see cref="SavedMatch"/>) EINMALIG
/// die komplette Folge von Ereignis-/Zustands-Momentaufnahmen
/// (<see cref="ReplayFrame"/>), indem die Partie über
/// <see cref="ReplayDrivingAgent"/> tatsächlich erneut simuliert wird.
/// Vor-/Zurückspulen im Replay-Bildschirm ist danach reine Index-
/// Navigation über eine bereits fertige Liste - kein erneuter Kontakt zur
/// Simulation nötig, "zurück" ist damit genauso einfach wie "vor". Das
/// Nachbauen läuft synchron und ohne Zeitverzögerung (keine echten
/// Agenten-Wartezeiten wie beim Live-Spiel) und ist für eine einzelne
/// Partie in der Praxis eine Sache von Millisekunden - ein eigener
/// Hintergrundthread ist dafür (anders als beim Live-Spiel) nicht nötig.
/// </summary>
public static class ReplayTimeline
{
    public static ReplayData Build(SavedMatch savedMatch, HeroCatalog catalog, GameBoard board)
    {
        var state = new GameState(board);
        foreach (var unitInfo in savedMatch.Units)
        {
            Unit unit = unitInfo.HeroId is string heroId
                ? new Hero(unitInfo.UnitId, unitInfo.Team, unitInfo.StartPosition, catalog.Get(heroId))
                : new Unit(unitInfo.UnitId, unitInfo.Team, unitInfo.StartPosition);
            state.AddUnit(unit);
        }

        var initialModel = BoardRenderModel.FromGameState(state);
        var agent = ReplayDrivingAgent.Build(savedMatch.Replay);

        // WICHTIG: EffectSystem/AbilitySystem protokollieren "EffectApplied"/
        // "EffectTicked"/"EffectExpired" INTERN direkt in die von
        // MatchController verwaltete Liste - NICHT über dessen onEvent-
        // Callback (den kennen sie gar nicht). Ohne recordReplay:true bliebe
        // diese Liste null, und genau diese Ereignisse würden dadurch
        // stillschweigend verloren gehen (onEvent liefert dann nur die von
        // MatchController SELBST direkt gemeldeten Ereignisse: Bewegung,
        // Angriff, Fähigkeitseinsatz, Zug-/Rundenwechsel, Niederlage,
        // Klon-Ereignisse, Partieende). Mit recordReplay:true ist
        // result.Replay.Entries danach die VOLLSTÄNDIGE, korrekte Liste -
        // sie wird unten mit den per onEvent tatsächlich eingefangenen
        // Momentaufnahmen zusammengeführt (siehe MergeWithSnapshots).
        var mainFrames = new List<ReplayFrame>();
        var controller = new MatchController(state, agent, agent, savedMatch.Replay.Seed, recordReplay: true, onEvent: entry =>
        {
            mainFrames.Add(new ReplayFrame(entry, BoardRenderModel.FromGameState(state)));
        });

        var result = controller.Run(savedMatch.Replay.StartingTeam);
        var frames = MergeWithSnapshots(result.Replay!.Entries, mainFrames, state);
        return new ReplayData(initialModel, frames);
    }

    /// <summary>
    /// Ordnet jedem Eintrag der VOLLSTÄNDIGEN Liste eine Zustands-
    /// Momentaufnahme zu: für die per onEvent tatsächlich eingefangenen
    /// Ereignisse die dabei bereits eingefrorene Momentaufnahme
    /// (<paramref name="mainFrames"/>); für die dazwischen von EffectSystem/AbilitySystem
    /// direkt protokollierten Ereignisse (ohne eigene Momentaufnahme) die
    /// des NÄCHSTEN erfassten Hauptereignisses - dessen Momentaufnahme
    /// entsteht ohnehin ERST NACHDEM auch diese zwischenzeitlichen
    /// Effekt-Änderungen bereits vollzogen wurden, spiegelt sie also
    /// korrekt wider. <paramref name="fallbackState"/> deckt nur den
    /// theoretischen Randfall ab, dass NACH dem letzten Haupt-Ereignis noch
    /// weitere Einträge folgen (kommt nach Prüfung der Zeitpunkte in
    /// MatchController aktuell nicht vor, schadet als Absicherung aber nicht).
    /// </summary>
    private static List<ReplayFrame> MergeWithSnapshots(IReadOnlyList<ReplayEntry> completeEntries, List<ReplayFrame> mainFrames, GameState fallbackState)
    {
        var frames = new List<ReplayFrame>(completeEntries.Count);
        int mainIndex = 0;

        foreach (var entry in completeEntries)
        {
            if (mainIndex < mainFrames.Count && ReferenceEquals(entry, mainFrames[mainIndex].Event))
            {
                frames.Add(mainFrames[mainIndex]);
                mainIndex++;
            }
            else
            {
                var upcomingModel = mainIndex < mainFrames.Count
                    ? mainFrames[mainIndex].Model
                    : BoardRenderModel.FromGameState(fallbackState);
                frames.Add(new ReplayFrame(entry, upcomingModel));
            }
        }

        return frames;
    }
}
