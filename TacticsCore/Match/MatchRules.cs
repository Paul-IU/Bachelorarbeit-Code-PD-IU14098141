namespace TacticsCore.Match;

/// <summary>
/// Benannte Konstanten der Rundenverwaltung. An dieser einen Stelle
/// definiert, damit sie sich für Balancing-Experimente gezielt anpassen
/// lassen, ohne die Ablaufsteuerung selbst zu berühren.
/// </summary>
public static class MatchRules
{
    /// <summary>
    /// Maximale Anzahl Runden, bevor eine Partie über die Auflösungsregel
    /// (siehe <see cref="MatchController"/>: Heldenanzahl, dann
    /// prozentualer HP-Anteil, dann Unentschieden) entschieden wird, statt
    /// über die Auslöschung eines Teams.
    /// </summary>
    public const int RoundLimit = 50;
}
