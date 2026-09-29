using TacticsCore.Match;
using TacticsCore.Setup;
using TacticsCore.Units;

namespace TacticsCore.Batch;

/// <summary>
/// Aggregiertes Ergebnis ALLER simulierten Partien EINER Paarung
/// (Anforderung 8): Siege je Seite, Unentschieden, durchschnittliche
/// Rundenzahl, durchschnittliche prozentuale Rest-HP je Team und der Anteil
/// der Partien, die durch Rundenlimit statt Auslöschung endeten. Reine,
/// strukturierte Daten - keine Konsolenausgabe.
/// </summary>
/// <param name="HeroActionTotals">
/// Über ALLE Partien dieser Paarung aufsummierte Bewegungs-/Angriffs-/
/// Fähigkeitseinsatz-Zahlen je Helden-ID (siehe <see cref="HeroActionCounts"/>
/// und <see cref="MatchResult.HeroActionCountsByHeroId"/>) - GENAU EINMAL je
/// Held, auch wenn er (Spiegelpaarung oder zwei unterschiedliche
/// Kompositionen mit gemeinsamem Helden) in jeder Partie dieser Paarung auf
/// BEIDEN Seiten stand, da bereits <see cref="MatchController"/> die
/// Aktionen beider physischer Einheiten je Partie zusammenführt.
/// </param>
/// <param name="RoundLimitSurvivingHeroes">
/// Partien, die am Rundenlimit über Stufe a) der Auflösung entschieden wurden
/// (mehr lebende Helden, siehe <see cref="RoundLimitResolution"/>). Stufe b)
/// steht in <paramref name="RoundLimitHitPointFraction"/>; Stufe c) sind genau die
/// <paramref name="Draws"/> (eine Auslöschung endet nie unentschieden).
/// </param>
/// <param name="RoundLimitHitPointFraction">Partien, die am Rundenlimit über Stufe b) entschieden wurden (höherer Anteil Rest-Lebenspunkte).</param>
public sealed record PairingResult(
    TeamComposition TeamA,
    TeamComposition TeamB,
    int TeamAWins,
    int TeamBWins,
    int Draws,
    int TotalMatches,
    double AverageRoundsPlayed,
    double AverageTeamARemainingHitPointsFraction,
    double AverageTeamBRemainingHitPointsFraction,
    double RoundLimitShare,
    IReadOnlyDictionary<string, HeroActionCounts> HeroActionTotals,
    int RoundLimitSurvivingHeroes = 0,
    int RoundLimitHitPointFraction = 0)
{
    public double TeamAWinRate => TotalMatches == 0 ? 0.0 : (double)TeamAWins / TotalMatches;
    public double TeamBWinRate => TotalMatches == 0 ? 0.0 : (double)TeamBWins / TotalMatches;
    public double DrawRate => TotalMatches == 0 ? 0.0 : (double)Draws / TotalMatches;
}

/// <summary>
/// EINE Partie eines Batch-Laufs (Bachelorarbeit-Erweiterung "Export je Partie"):
/// erlaubt Teilmengen beliebiger Größe - etwa um zu prüfen, ab welcher Partienzahl
/// ein eingebrachtes Ungleichgewicht sichtbar wird -, was aus den Aggregaten je
/// Paarung nicht möglich ist. Nur erhoben, wenn <see cref="BatchSimulationRunner.Run"/>
/// mit <c>recordMatches: true</c> aufgerufen wird.
/// </summary>
/// <param name="PairingIndex">Index der Paarung (= Zeile der Paarungs-CSV, 0-basiert ohne Kopfzeile).</param>
/// <param name="SetupIndex">Index der Ausgangslage (Position in AusgangslagenGenerator.GenerateAllSetups).</param>
/// <param name="TeamsSwapped">Nur bei <c>useAntitheticPairing</c>: die teamvertauschte Zweitpartie derselben Ausgangslage.</param>
/// <param name="Seed">Seed der Partie (siehe <see cref="BatchSimulationRunner.DeriveSeed"/>).</param>
/// <param name="Resolution">Entscheidende Auflösungsstufe am Rundenlimit; null bei Auslöschung.</param>
/// <param name="TeamARemainingHitPoints">Rest-Lebenspunkte der Helden von Team A am Ende (Klone zählen nicht; Maximum siehe Paarungs-CSV).</param>
public readonly record struct MatchRecord(
    int PairingIndex,
    int SetupIndex,
    bool TeamsSwapped,
    int Seed,
    Team StartingTeam,
    MatchWinner Winner,
    MatchEndReason EndReason,
    RoundLimitResolution? Resolution,
    int RoundsPlayed,
    int TeamASurvivingHeroes,
    int TeamBSurvivingHeroes,
    int TeamARemainingHitPoints,
    int TeamBRemainingHitPoints);

/// <summary>Fortschrittsmeldung eines laufenden Batch-Simulationslaufs (Anforderung 10).</summary>
public sealed record BatchProgress(int CompletedPairings, int TotalPairings, int CompletedMatches, int TotalMatches, TimeSpan Elapsed);

/// <summary>Vollständiges Ergebnis eines Simulationslaufs über alle übergebenen Paarungen, samt Gesamtlaufzeit (Anforderung 10).</summary>
/// <param name="NameIndependence">Ergebnis der optionalen Etiketten-Prüfung; null, wenn sie nicht aktiviert war (siehe <see cref="NameIndependenceCheck"/>).</param>
/// <param name="Matches">Alle Partien in fester Reihenfolge (Paarung, Ausgangslage, ggf. vertauschte Zweitpartie) - unabhängig von der Threadanzahl; null, wenn nicht angefordert (<c>recordMatches</c>).</param>
public sealed record SimulationRunResult(IReadOnlyList<PairingResult> PairingResults, TimeSpan ElapsedTime, NameIndependenceReport? NameIndependence = null, IReadOnlyList<MatchRecord>? Matches = null);
