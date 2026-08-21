using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Setup;

/// <summary>
/// Eine vollständig bestimmte, konkrete Ausgangslage (Anforderung 3): für
/// jedes Team die drei Helden MIT ihrem jeweiligen Startfeld, sowie das
/// beginnende Team. Bewusst als reine Positions-/Zuordnungsliste modelliert
/// (Helden-Definition + Koordinate, noch keine <see cref="Unit"/>-Objekte
/// mit vergebener Id) - die eigentlichen Spieleinheiten inklusive
/// deterministischer Id-Vergabe entstehen erst beim tatsächlichen Aufbau
/// eines <see cref="GameState"/> (siehe <see cref="AusgangslagenGenerator.BuildGameState"/>).
/// </summary>
/// <param name="TeamAUnits">Die drei (Held, Startfeld)-Paare für Team A.</param>
/// <param name="TeamBUnits">Die drei (Held, Startfeld)-Paare für Team B.</param>
/// <param name="StartingTeam">Das Team, das in dieser Partie zuerst am Zug ist.</param>
public sealed record GameSetup(
    IReadOnlyList<(HeroDefinition Hero, HexCoordinate Position)> TeamAUnits,
    IReadOnlyList<(HeroDefinition Hero, HexCoordinate Position)> TeamBUnits,
    Team StartingTeam)
{
    /// <summary>
    /// Liefert dieselbe Ausgangslage mit VERTAUSCHTEN Teams: die Einheiten
    /// bleiben exakt auf ihren bisherigen Feldern stehen, wechseln aber die
    /// Team-Zugehörigkeit (ehemals Team A -&gt; jetzt Team B und umgekehrt),
    /// und das beginnende Team wird entsprechend umbenannt.
    ///
    /// WARUM DAS EXAKT SPIEGELBILDLICHE ERGEBNISSE LIEFERT: Team-
    /// Zugehörigkeit und Spielfeld-Position sind in TacticsCore vollständig
    /// unabhängige Konzepte - kein Bewegungs-, Kampf- oder Zugreihenfolge-
    /// System fragt jemals nach dem Feldtyp (Blüte A/B) einer Position,
    /// sondern ausschließlich nach der Team-Zugehörigkeit der jeweiligen
    /// Einheit. Wird eine Partie mit demselben Seed einmal normal und einmal
    /// mit vertauschten Teams gespielt, laufen daher exakt dieselben
    /// physischen Einheiten in derselben Reihenfolge dieselben Entscheidungen
    /// durch - nur die Etiketten "Team A"/"Team B" sind vertauscht. Der
    /// Sieger kippt dadurch GARANTIERT exakt um (aus einem Sieg von Team A
    /// wird ein Sieg von Team B, ein Unentschieden bleibt ein
    /// Unentschieden) - eine reine Umbenennung, keine geometrische
    /// Spiegelung des Spielfelds, die (anders als eine echte Koordinaten-
    /// Spiegelung) auch keinerlei Rücksicht auf Sortierreihenfolgen o.ä.
    /// nehmen muss.
    ///
    /// Nutzen: (a) Beweis-Grundlage für die Plausibilitätsprüfung "identische
    /// Komposition auf beiden Seiten -&gt; exakt 50% Siegquote" (siehe
    /// <see cref="Batch.BatchSimulationRunner"/>, Option
    /// "AntitheticPairing"), (b) allgemein als Varianzreduktions-Technik
    /// ("antithetische Paare") für BELIEBIGE Paarungen: pro Partie wird mit
    /// demselben Seed zusätzlich die teamvertauschte Partie gespielt, was
    /// die Ergebnisse über viele Paarungen hinweg schneller konvergieren
    /// lässt, ohne die Reproduzierbarkeit zu gefährden.
    /// </summary>
    public GameSetup WithSwappedTeams() =>
        new(TeamBUnits, TeamAUnits, StartingTeam == Team.TeamA ? Team.TeamB : Team.TeamA);
}
