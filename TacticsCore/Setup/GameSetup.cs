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
    /// WAS DAS BEWEIST UND WAS NICHT (Bachelorarbeit-Klarstellung): die
    /// vertauschte Partie ist physisch eine exakte DUBLETTE der ursprünglichen
    /// (dieselben Einheiten auf denselben Feldern, derselbe Seed, dieselbe
    /// beginnende Seite - nur die Etiketten und Ids sind getauscht). Kommen
    /// beide Läufe zu spiegelbildlichen Ergebnissen, ist das der Nachweis,
    /// dass das Regelwerk NAMENSUNABHÄNGIG ist: nichts im Spielablauf hängt
    /// vom Team-Etikett oder vom Id-Bereich ab. So wurde ein Fehler in der
    /// Sortierung der Fähigkeitsoptionen gefunden (siehe
    /// <see cref="Abilities.AbilitySystem.GetLegalAbilityUses"/>).
    ///
    /// Daraus folgt: (a) bei einer SPIEGELPAARUNG (identische Komposition auf
    /// beiden Seiten) gewinnen beide Etiketten exakt gleich oft - eine
    /// Konsequenz der Namensunabhängigkeit, KEINE Aussage darüber, ob die
    /// Aufstellung auf Blüte A gegen dieselbe Komposition auf Blüte B im
    /// Mittel 50% erreicht (das ist nur statistisch prüfbar). (b) bei einer
    /// Paarung VERSCHIEDENER Kompositionen ist die Paarbildung KEINE
    /// Varianzreduktion, sondern degeneriert: jede Partie zählt einmal für
    /// jedes Etikett, das Ergebnis ist IMMER exakt 50:50 (gemessen: ohne
    /// Paarbildung 58:14, mit 72:72) und verliert seine Aussage. Sie ist
    /// deshalb nur für Spiegelpaarungen bzw. als reiner Namensunabhängigkeits-
    /// test sinnvoll. Die 72 Ausgangslagen einer Paarung sind unter dem
    /// Teamtausch NICHT abgeschlossen (Team A steht immer auf Blüte A, Team B
    /// auf Blüte B) - keine Ausgangslage ist ihr eigenes Gegenstück.
    /// </summary>
    public GameSetup WithSwappedTeams() =>
        new(TeamBUnits, TeamAUnits, StartingTeam == Team.TeamA ? Team.TeamB : Team.TeamA);
}
