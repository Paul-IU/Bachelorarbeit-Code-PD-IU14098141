using TacticsCore.Setup;

namespace TacticsCore.Batch;

/// <summary>
/// Aggregiertes Ergebnis ALLER simulierten Partien EINER Paarung
/// (Anforderung 8): Siege je Seite, Unentschieden, durchschnittliche
/// Rundenzahl, durchschnittliche prozentuale Rest-HP je Team und der Anteil
/// der Partien, die durch Rundenlimit statt Auslöschung endeten. Reine,
/// strukturierte Daten - keine Konsolenausgabe.
/// </summary>
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
    double RoundLimitShare)
{
    public double TeamAWinRate => TotalMatches == 0 ? 0.0 : (double)TeamAWins / TotalMatches;
    public double TeamBWinRate => TotalMatches == 0 ? 0.0 : (double)TeamBWins / TotalMatches;
    public double DrawRate => TotalMatches == 0 ? 0.0 : (double)Draws / TotalMatches;
}

/// <summary>Fortschrittsmeldung eines laufenden Batch-Simulationslaufs (Anforderung 10).</summary>
public sealed record BatchProgress(int CompletedPairings, int TotalPairings, int CompletedMatches, int TotalMatches, TimeSpan Elapsed);

/// <summary>Vollständiges Ergebnis eines Simulationslaufs über alle übergebenen Paarungen, samt Gesamtlaufzeit (Anforderung 10).</summary>
public sealed record SimulationRunResult(IReadOnlyList<PairingResult> PairingResults, TimeSpan ElapsedTime);
