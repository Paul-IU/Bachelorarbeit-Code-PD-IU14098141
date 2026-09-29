using System.Globalization;
using System.IO.Compression;
using System.Text;
using TacticsCore.Heroes;
using TacticsCore.Match;

namespace TacticsCore.Batch;

/// <summary>
/// Exportiert Simulationsergebnisse als CSV (Anforderung 9) für die
/// spätere statistische Auswertung außerhalb von TacticsCore (z.B. in R,
/// Python/pandas oder einer Tabellenkalkulation).
///
/// ZAHLENFORMAT (Bachelorarbeit-Korrektur): Zahlen werden mit Komma als
/// Dezimaltrennzeichen formatiert (deutsche Konvention), passend zum
/// gewählten Spalten-Trenner ';' (siehe <see cref="Separator"/>). Ein
/// früherer Versuch, stattdessen unabhängig von der Systemsprache IMMER
/// einen Punkt als Dezimaltrennzeichen zu verwenden (<see cref="CultureInfo.InvariantCulture"/>),
/// erwies sich als echter Fehler, nicht nur als Geschmacksfrage: die
/// Kombination ';'-Trenner + '.'-Dezimalzeichen ist genau die Konstellation,
/// die ein deutsch lokalisiertes Werkzeug (Excel, ein CSV-Editor in der IDE
/// o.ä.) dazu verleitet, den Punkt fälschlich als TAUSENDER-Trennzeichen
/// zu interpretieren - "18.4722" wurde dadurch nachweislich zu "184.722"
/// verstümmelt (aus "18,4722 Runden im Schnitt" wurde beim Öffnen/Speichern
/// ein unmöglicher Wert von über 180 Runden, obwohl <see cref="Match.MatchRules.RoundLimit"/>
/// jede einzelne Partie auf 100 Runden begrenzt). Mit einheitlich Komma als
/// Dezimalzeichen (','-Trenner käme mit dem Feld-Trenner ';' nie in
/// Konflikt) kann diese Fehlinterpretation nicht mehr auftreten. Nachteil:
/// ein Import in ein NICHT deutsch konfiguriertes Werkzeug (z.B. Pythons
/// <c>pandas.read_csv</c>) braucht dafür explizit <c>decimal=','</c> -
/// akzeptabler Kompromiss, da für dieses Projekt deutsch lokalisierte
/// Tabellenkalkulation der Hauptanwendungsfall ist.
/// </summary>
public static class CsvExporter
{
    private const char Separator = ';'; // ';' statt ',' - der übliche CSV-Trenner in Excel (DE); passt zusammen mit dem Komma-Dezimalzeichen (siehe Klassendoku) zur selben deutschen Konvention, kein Widerspruch zwischen beiden Trennzeichen.

    /// <summary>
    /// Eine Zeile je Paarung, mit allen in <see cref="PairingResult"/>
    /// gesammelten Kennzahlen. Rest-HP wird bewusst DOPPELT geführt: als
    /// absolute Zahl (eigene Spalte je Team, direkt weiterverrechenbar ohne
    /// erst die maximalen HP der Komposition nachschlagen zu müssen) UND als
    /// Anteil an den maximalen HP (für den Vergleich zwischen KOMPOSITIONEN
    /// MIT UNTERSCHIEDLICHER Gesamt-HP, wo die absolute Zahl allein
    /// irreführend wäre). Die maximalen HP selbst sind keine über die
    /// Partien gemittelte Größe, sondern für eine Komposition schlicht
    /// konstant (Summe der MaxHitPoints ihrer drei Helden) - stehen aber
    /// trotzdem als eigene Spalte, damit die Rest-HP-Spalte ohne Rückgriff
    /// auf den Heldenkatalog eingeordnet werden kann.
    /// </summary>
    /// <param name="results">Die zu exportierenden Paarungs-Ergebnisse.</param>
    /// <param name="filePath">Zieldatei.</param>
    /// <param name="agentName">
    /// Bachelorarbeit-Erweiterung ("der verwendete Agent soll in den
    /// Ergebnisdateien vermerkt werden, damit Läufe eindeutig zuzuordnen
    /// sind"): sprechender Bezeichner (siehe <see cref="AgentNames"/> bzw.
    /// <see cref="AgentConfiguration.Name"/>), als eigene Spalte auf JEDER
    /// Zeile wiederholt statt als separate Metadatendatei - macht diese CSV
    /// allein aus sich heraus eindeutig zuordenbar, auch wenn sie von ihren
    /// Geschwisterdateien oder der Konsolenausgabe des Laufs getrennt wird.
    /// </param>
    public static void ExportPairingResults(IEnumerable<PairingResult> results, string filePath, string agentName)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Separator, "TeamA", "TeamB", "Partien", "SiegeTeamA", "SiegeTeamB", "Unentschieden",
            "SiegquoteTeamA", "SiegquoteTeamB", "DurchschnittRunden",
            "MaxHpTeamA", "DurchschnittRestHpTeamA", "DurchschnittRestHpAnteilTeamA",
            "MaxHpTeamB", "DurchschnittRestHpTeamB", "DurchschnittRestHpAnteilTeamB",
            "AnteilRundenlimit", "Agent"));

        foreach (var r in results)
        {
            int maxHpTeamA = r.TeamA.Heroes.Sum(h => h.MaxHitPoints);
            int maxHpTeamB = r.TeamB.Heroes.Sum(h => h.MaxHitPoints);

            // Exakt (keine Näherung): maxHp ist über alle Partien EINER
            // Paarung konstant (hängt nur von der Komposition ab, nie vom
            // Partieausgang) - der Mittelwert der absoluten Rest-HP lässt
            // sich daher direkt aus dem bereits vorhandenen Anteils-
            // Mittelwert zurückrechnen, ohne dass BatchSimulationRunner
            // selbst zusätzlich die rohen HP-Werte je Partie mitführen
            // müsste: avg(rest_i) = avg(rest_i / maxHp) * maxHp, da maxHp
            // bei jeder einzelnen Partie derselbe konstante Faktor ist.
            double avgRestHpTeamA = r.AverageTeamARemainingHitPointsFraction * maxHpTeamA;
            double avgRestHpTeamB = r.AverageTeamBRemainingHitPointsFraction * maxHpTeamB;

            sb.AppendLine(string.Join(Separator,
                Csv(r.TeamA.Key), Csv(r.TeamB.Key), r.TotalMatches, r.TeamAWins, r.TeamBWins, r.Draws,
                Num(r.TeamAWinRate), Num(r.TeamBWinRate), Num(r.AverageRoundsPlayed),
                maxHpTeamA, Num(avgRestHpTeamA), Num(r.AverageTeamARemainingHitPointsFraction),
                maxHpTeamB, Num(avgRestHpTeamB), Num(r.AverageTeamBRemainingHitPointsFraction),
                Num(r.RoundLimitShare), Csv(agentName)));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Eine Zeile JE KOMPOSITION mit der über ALLE ihre Paarungen
    /// aggregierten Siegquote (Anforderung 9: "separater Export je
    /// Komposition mit der aggregierten Siegquote") - eine Komposition kann
    /// in verschiedenen Paarungen sowohl als Team A als auch als Team B
    /// aufgetreten sein, beides fließt hier zusammen ein.
    ///
    /// SPIEGELPAARUNGEN (Bachelorarbeit-Korrektur, echter Bug): Bei einer
    /// Paarungs-Zeile, in der TeamA und TeamB DIESELBE Komposition sind
    /// (Spiegelpaarung gegen sich selbst, siehe RunFullBaseline), trafen
    /// die beiden Accumulate-Aufrufe unten VOR dieser Korrektur denselben
    /// Dictionary-Eintrag - die 72 Partien dieser einen Zeile wurden dadurch
    /// DOPPELT gezählt (einmal über TeamA, einmal über TeamB), jede
    /// Komposition erschien so um genau ihre eigene Spiegelpartien-Anzahl
    /// zu hoch. Jetzt wird eine Spiegelpaarung explizit erkannt und NUR
    /// EINMAL akkumuliert - TeamAWins und TeamBWins werden dabei addiert
    /// (aus Sicht der Komposition selbst hat in jeder nicht unentschiedenen
    /// Partie "sie" gewonnen, unabhängig davon, ob die tatsächliche
    /// Siegerinstanz als TeamA oder TeamB geführt wurde), aber nur EINMAL
    /// mit den echten 72 Partien statt 144.
    /// </summary>
    /// <param name="results">Die zu aggregierenden Paarungs-Ergebnisse.</param>
    /// <param name="filePath">Zieldatei.</param>
    /// <param name="agentName">Siehe <see cref="ExportPairingResults"/> - derselbe Zweck, dieselbe Konvention.</param>
    public static void ExportCompositionWinRates(IEnumerable<PairingResult> results, string filePath, string agentName)
    {
        var byComposition = new Dictionary<string, (Setup.TeamComposition Composition, int Wins, int Matches)>();

        foreach (var r in results)
        {
            if (r.TeamA.Key == r.TeamB.Key)
            {
                Accumulate(r.TeamA, r.TeamAWins + r.TeamBWins, r.TotalMatches);
            }
            else
            {
                Accumulate(r.TeamA, r.TeamAWins, r.TotalMatches);
                Accumulate(r.TeamB, r.TeamBWins, r.TotalMatches);
            }

            void Accumulate(Setup.TeamComposition composition, int wins, int matches)
            {
                (Setup.TeamComposition Composition, int Wins, int Matches) current = byComposition.TryGetValue(composition.Key, out var existing)
                    ? existing
                    : (composition, 0, 0);
                byComposition[composition.Key] = (composition, current.Wins + wins, current.Matches + matches);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Separator, "Komposition", "Partien", "Siege", "Siegquote", "Agent"));

        // Stabil nach dem Komposition-Schlüssel sortiert statt in
        // Dictionary-Durchlaufreihenfolge - Letztere ist für die konkrete
        // .NET-Implementierung zwar in der Praxis meist insertion-order,
        // aber nicht offiziell garantiert.
        foreach (var entry in byComposition.Values.OrderBy(v => v.Composition.Key, StringComparer.Ordinal))
        {
            double winRate = entry.Matches == 0 ? 0.0 : (double)entry.Wins / entry.Matches;
            sb.AppendLine(string.Join(Separator, Csv(entry.Composition.Key), entry.Matches, entry.Wins, Num(winRate), Csv(agentName)));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Eine Zeile JE HELD (nicht je Komposition) mit der über ALLE Partien
    /// aggregierten Siegquote, in denen dieser Held in irgendeiner der
    /// C(8,2) = 28 möglichen Dreier-Kompositionen mit ihm antrat.
    ///
    /// GRUND FÜR EINEN EIGENEN EXPORT (Ergänzung zu <see cref="ExportCompositionWinRates"/>):
    /// Dort verschwimmt die Wirkung eines einzelnen Helden in der jeweiligen
    /// 3er-Kombination - eine schwache Komposition sagt nicht direkt, OB
    /// UND WELCHER der drei enthaltenen Helden dafür verantwortlich ist.
    /// Hier wird über alle 28 Kombinationen gemittelt, in denen ein Held
    /// vorkommt - die naheliegendste Sicht für "ist Held X grundsätzlich
    /// stark oder schwach", unabhängig davon, mit wem er zusammen antritt.
    ///
    /// AGGREGATION: Für jede Paarungs-Zeile trägt JEDER der drei Helden von
    /// Team A mit (TeamAWins, TotalMatches) zu seiner eigenen Bilanz bei,
    /// jeder der drei Helden von Team B entsprechend mit (TeamBWins,
    /// TotalMatches) - ein einzelner Partie-Ausgang fließt also in die
    /// Bilanz von sechs verschiedenen Helden ein (drei pro Seite), nicht nur
    /// in eine einzige Zeile wie bei <see cref="ExportCompositionWinRates"/>.
    ///
    /// SORTIERUNG: Bewusst nach Siegquote ABSTEIGEND (nicht alphabetisch wie
    /// bei den beiden anderen Exporten) - das macht eine Rangfolge "stärkster
    /// bis schwächster Held" auf den ersten Blick sichtbar, ohne die Datei
    /// erst selbst sortieren zu müssen. Tie-Break bei gleicher Siegquote
    /// über die Helden-Id (aufsteigend), damit die Reihenfolge bei exaktem
    /// Gleichstand trotzdem deterministisch und reproduzierbar bleibt.
    ///
    /// SPIEGELPAARUNGEN UND GETEILTE HELDEN (Bachelorarbeit-Korrektur, echter
    /// Bug - Erweiterung der ursprünglichen Spiegelpaarungs-Korrektur):
    /// die ursprüngliche Korrektur erkannte NUR den Sonderfall "TeamA und
    /// TeamB sind exakt dieselbe Komposition" (<c>r.TeamA.Key == r.TeamB.Key</c>).
    /// Tatsächlich kann ein Held aber auch dann auf BEIDEN Seiten EINER
    /// Partie stehen, wenn TeamA und TeamB VERSCHIEDENE Kompositionen sind,
    /// die sich lediglich EINEN gemeinsamen Helden teilen (z.B. TeamA =
    /// "berserker+bogenschuetze+elfe" gegen TeamB = "berserker+wand+hugger" -
    /// der Berserker tritt in dieser Paarung gegen sich selbst an, obwohl
    /// <c>r.TeamA.Key != r.TeamB.Key</c>). Die alte Prüfung erkannte diesen
    /// Fall NICHT und zählte den Berserker über die getrennten TeamA- und
    /// TeamB-Accumulate-Aufrufe fälschlich DOPPELT (144 statt 72 Partien für
    /// genau diese eine Zeile) - bei 28 Kompositionen je Held ergeben sich
    /// C(28,2)=378 solcher Paarungen PRO HELD, ein spürbarer systematischer
    /// Fehler, keine Rundungsungenauigkeit.
    ///
    /// Die Erkennung erfolgt über die MENGE aller tatsächlich beteiligten
    /// Helden-IDs je Paarungs-Zeile (Vereinigung beider Seiten - bei einer
    /// Spiegelpaarung oder einem geteilten Helden fällt die Dopplung durch
    /// die Mengenbildung automatisch weg). Das allein behebt zwar die
    /// Doppelzählung, deckt aber eine TIEFERE, strukturelle Verzerrung nicht
    /// ab - siehe den folgenden Abschnitt.
    ///
    /// BEREINIGTE VS. ROHE (VERZERRTE) SIEGQUOTE (Bachelorarbeit-Korrektur):
    /// Jeder Held kommt in C(catalogSize-1, teamSize-1) Kompositionen vor
    /// (beim Standardkatalog: C(8,2)=28 von 84). Er steht dadurch in einem
    /// TEIL der Partien auf BEIDEN Seiten gleichzeitig (siehe oben) und
    /// "gewinnt" dort per Definition immer - dieser Ausgang sagt nichts über
    /// seine tatsächliche Stärke aus. Ein völlig neutraler Held würde dadurch
    /// NICHT 50%, sondern (beim Standardkatalog) rechnerisch ca. 60,28%
    /// erreichen (exakte Herleitung und Berechnung aus Katalog-/Teamgröße
    /// siehe <see cref="HeroWinRateExpectations"/>) - eine einzelne,
    /// undifferenzierte Siegquote lässt sich also leicht als "Held X ist
    /// überdurchschnittlich stark" fehlinterpretieren, obwohl ein Großteil
    /// der Abweichung strukturell bedingt ist. Diese Fassung trennt daher:
    ///   - "Partien"/"Siege"/"Siegquote" (ohne Zusatz, PRIMÄRE, bereinigte
    ///     Kennzahl): nur Partien mit dem Helden auf GENAU EINER Seite - hier
    ///     ist "Sieg" eine echte Aussage über seine Stärke. ALLE weiteren
    ///     Spalten dieser Zeile (Bewegungen, Angriffe, Fähigkeitseinsätze,
    ///     Runden am Zug, Partiedauer, ...) beziehen sich ebenfalls
    ///     AUSSCHLIESSLICH auf diese Partien (siehe "AKTIONSZAHLEN" unten).
    ///   - "PartienBeideSeiten": Anzahl der (rein informativ ausgewiesenen)
    ///     Partien mit dem Helden auf beiden Seiten - fließt in keine andere
    ///     Spalte ein.
    ///   - "PartienGesamtVerzerrt"/"SiegeGesamtVerzerrt"/"SiegquoteGesamtVerzerrt":
    ///     Summe beider Gruppen (Partien: einfache Summe, nicht doppelt
    ///     gezählt - siehe oben; Siege: beide Seiten zählen in "beidseitigen"
    ///     Partien als ein Sieg des Helden) - bewusst weiterhin ausgewiesen
    ///     (Vergleichbarkeit mit früheren Ständen dieser Datei), aber klar
    ///     als verzerrt benannt statt kommentarlos die Hauptkennzahl zu sein.
    ///
    /// AKTIONSZAHLEN NUR AUS EINSEITIGEN PARTIEN (Bachelorarbeit-Korrektur,
    /// echter Bug: unmöglich hohe Werte wie "35 Bewegungen bei 32,5 Runden
    /// durchschnittlicher Partiedauer"): steht ein Held auf BEIDEN Seiten
    /// einer Partie, führt <see cref="Match.MatchController"/> die Aktionen
    /// BEIDER physischer Einheiten unter derselben Helden-ID zusammen (siehe
    /// dort) - für sich genommen richtig (exakt die Aggregationsebene, die
    /// diese Datei sonst braucht), aber genau DESHALB für "Bewegungen je
    /// eigenem Zug" nicht mehr sinnvoll interpretierbar: eine "beidseitige"
    /// Partie liefert das KOMBINIERTE Ergebnis zweier unabhängiger
    /// Held-Instanzen, während z.B. die Partiedauer nur einmal zählt - eine
    /// daraus gebildete Rate kann dadurch scheinbar unmögliche Werte
    /// erreichen, OHNE dass irgendetwas im Sinne eines Bugs doppelt gezählt
    /// würde. Statt die beiden Instanzen künstlich wieder aufzutrennen,
    /// werden "beidseitige" Partien für ALLE Aktions-/Rundenkennzahlen dieser
    /// Zeile schlicht AUSGESCHLOSSEN (nur ihre Anzahl fließt in
    /// "PartienBeideSeiten" ein) - in den verbleibenden "einseitigen"
    /// Partien steht immer GENAU EINE physische Einheit für den Helden, die
    /// Zahlen sind dort unzweideutig einer einzigen Instanz zuordenbar.
    ///
    /// RUNDEN AM ZUG VS. PARTIEDAUER (Bachelorarbeit-Korrektur, Problem
    /// "was misst die Rundenzahl eines Helden eigentlich?"): "DurchschnittPartiedauer"
    /// misst die Länge der PARTIEN, in denen der Held (einseitig) vorkam -
    /// "DurchschnittRundenAmZug" dagegen, wie viele Runden der Held SELBST
    /// tatsächlich am Zug war (<see cref="Match.HeroActionCounts.Turns"/>,
    /// gezählt in <see cref="Match.MatchController.ExecuteTurn"/>). Beide
    /// können auseinanderfallen: ein früh besiegter Held hat weniger eigene
    /// Züge als die Partie insgesamt dauerte. "DurchschnittBewegungenProZug"/
    /// "DurchschnittAngriffeProZug"/"DurchschnittFaehigkeitseinsaetzeProZug"
    /// normieren konsistent auf GENAU DIESE Bezugsgröße (Runden am Zug, NICHT
    /// die Partiedauer) - nur dadurch bleibt sichergestellt, dass die
    /// Bewegungen je Zug niemals über 1 liegen können (siehe die
    /// entsprechenden Tests in BatchSimulationRunnerTests).
    ///
    /// FÄHIGKEIT VERFÜGBAR VS. TATSÄCHLICH GENUTZT (Bachelorarbeit-Korrektur):
    /// die absolute Zahl der Fähigkeitseinsätze allein ist zwischen Helden
    /// nicht vergleichbar - ein Held ohne Abklingzeit in langen Partien
    /// sammelt zwangsläufig mehr Einsätze als einer mit Abklingzeit in
    /// kürzeren. "DurchschnittFaehigkeitseinsaetzeProVerfuegbaremZug"
    /// normiert zusätzlich auf die Züge, in denen die Fähigkeit überhaupt
    /// NICHT auf Abklingzeit war (<see cref="Match.HeroActionCounts.AbilityAvailableTurns"/>) -
    /// erst der Vergleich mit "DurchschnittFaehigkeitseinsaetzeProZug"
    /// unterscheidet "der Agent nutzt sie selten" von "er konnte sie selten
    /// nutzen".
    ///
    /// RUNDENLIMIT JE HELD (Bachelorarbeit-Erweiterung "Timeouts je Held"): wie die
    /// übrigen Heldenwerte nur aus einseitigen Partien. "PartienRundenlimit" = Partien
    /// mit dem Helden, die am Rundenlimit endeten, "AnteilRundenlimit" = deren Anteil an
    /// "Partien"; die drei folgenden Spalten verteilen sie auf die Auflösungsstufen
    /// (a) mehr lebende Helden, b) höherer Anteil Rest-Lebenspunkte, c) unentschieden;
    /// siehe <see cref="Match.RoundLimitResolution"/>). Die fünf Spalten stehen bewusst
    /// HINTER "Agent": alle bisherigen Spalten behalten ihre Position, Auswertungen
    /// früherer Läufe bleiben unverändert lesbar.
    /// </summary>
    /// <param name="results">Die zu aggregierenden Paarungs-Ergebnisse.</param>
    /// <param name="filePath">Zieldatei.</param>
    /// <param name="agentName">Siehe <see cref="ExportPairingResults"/> - derselbe Zweck, dieselbe Konvention.</param>
    public static void ExportHeroWinRates(IEnumerable<PairingResult> results, string filePath, string agentName)
    {
        var byHero = new Dictionary<string, HeroAccumulator>();

        foreach (var r in results)
        {
            var idsOnA = r.TeamA.Heroes.Select(h => h.Id).ToHashSet();
            var idsOnB = r.TeamB.Heroes.Select(h => h.Id).ToHashSet();

            foreach (var heroId in idsOnA.Union(idsOnB))
            {
                bool onA = idsOnA.Contains(heroId);
                bool onB = idsOnB.Contains(heroId);
                var heroDefinition = (onA ? r.TeamA.Heroes : r.TeamB.Heroes).Single(h => h.Id == heroId);
                var current = byHero.TryGetValue(heroId, out var existing) ? existing : new HeroAccumulator(heroDefinition, default);

                if (onA && onB)
                {
                    // Beidseitig: NUR die Anzahl fließt ein (siehe
                    // Klassendokumentation) - keine Aktions-/Rundenzahlen,
                    // kein Beitrag zur bereinigten Siegquote.
                    byHero[heroId] = current with
                    {
                        BothSidesMatches = current.BothSidesMatches + r.TotalMatches,
                        BothSidesWins = current.BothSidesWins + r.TeamAWins + r.TeamBWins,
                    };
                }
                else
                {
                    int wins = onA ? r.TeamAWins : r.TeamBWins;
                    r.HeroActionTotals.TryGetValue(heroId, out var actionCounts);
                    byHero[heroId] = current with
                    {
                        OneSidedMatches = current.OneSidedMatches + r.TotalMatches,
                        OneSidedWins = current.OneSidedWins + wins,
                        OneSidedRoundsSum = current.OneSidedRoundsSum + r.AverageRoundsPlayed * r.TotalMatches,
                        Actions = current.Actions + actionCounts,
                        OneSidedRoundLimitSurvivingHeroes = current.OneSidedRoundLimitSurvivingHeroes + r.RoundLimitSurvivingHeroes,
                        OneSidedRoundLimitHitPointFraction = current.OneSidedRoundLimitHitPointFraction + r.RoundLimitHitPointFraction,
                        OneSidedRoundLimitDraws = current.OneSidedRoundLimitDraws + r.Draws,
                    };
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Separator, "Held", "Rolle",
            "Partien", "Siege", "Siegquote",
            "RundenAmZug", "Bewegungen", "Angriffe", "Faehigkeitseinsaetze", "VerfuegbareZuegeFuerFaehigkeit",
            "DurchschnittPartiedauer", "DurchschnittRundenAmZug",
            "DurchschnittBewegungenProZug", "DurchschnittAngriffeProZug",
            "DurchschnittFaehigkeitseinsaetzeProZug", "DurchschnittFaehigkeitseinsaetzeProVerfuegbaremZug",
            "PartienBeideSeiten",
            "PartienGesamtVerzerrt", "SiegeGesamtVerzerrt", "SiegquoteGesamtVerzerrt", "Agent",
            "PartienRundenlimit", "AnteilRundenlimit", "RundenlimitMehrHelden", "RundenlimitMehrRestHp", "RundenlimitUnentschieden"));

        foreach (var entry in byHero.Values
            .OrderByDescending(v => v.OneSidedMatches == 0 ? 0.0 : (double)v.OneSidedWins / v.OneSidedMatches)
            .ThenBy(v => v.Hero.Id, StringComparer.Ordinal))
        {
            double cleanedWinRate = entry.OneSidedMatches == 0 ? 0.0 : (double)entry.OneSidedWins / entry.OneSidedMatches;
            double avgDuration = entry.OneSidedMatches == 0 ? 0.0 : entry.OneSidedRoundsSum / entry.OneSidedMatches;
            double avgTurns = entry.OneSidedMatches == 0 ? 0.0 : (double)entry.Actions.Turns / entry.OneSidedMatches;
            double avgMovesPerTurn = entry.Actions.Turns == 0 ? 0.0 : (double)entry.Actions.Moves / entry.Actions.Turns;
            double avgAttacksPerTurn = entry.Actions.Turns == 0 ? 0.0 : (double)entry.Actions.Attacks / entry.Actions.Turns;
            double avgAbilityUsesPerTurn = entry.Actions.Turns == 0 ? 0.0 : (double)entry.Actions.AbilityUses / entry.Actions.Turns;
            double avgAbilityUsesPerAvailableTurn = entry.Actions.AbilityAvailableTurns == 0 ? 0.0 : (double)entry.Actions.AbilityUses / entry.Actions.AbilityAvailableTurns;

            int totalMatches = entry.OneSidedMatches + entry.BothSidesMatches;
            int totalWins = entry.OneSidedWins + entry.BothSidesWins;
            double skewedWinRate = totalMatches == 0 ? 0.0 : (double)totalWins / totalMatches;
            int roundLimitMatches = entry.OneSidedRoundLimitSurvivingHeroes + entry.OneSidedRoundLimitHitPointFraction + entry.OneSidedRoundLimitDraws;
            double roundLimitShare = entry.OneSidedMatches == 0 ? 0.0 : (double)roundLimitMatches / entry.OneSidedMatches;

            sb.AppendLine(string.Join(Separator, Csv(entry.Hero.Id), entry.Hero.Role,
                entry.OneSidedMatches, entry.OneSidedWins, Num(cleanedWinRate),
                entry.Actions.Turns, entry.Actions.Moves, entry.Actions.Attacks, entry.Actions.AbilityUses, entry.Actions.AbilityAvailableTurns,
                Num(avgDuration), Num(avgTurns),
                Num(avgMovesPerTurn), Num(avgAttacksPerTurn), Num(avgAbilityUsesPerTurn), Num(avgAbilityUsesPerAvailableTurn),
                entry.BothSidesMatches,
                totalMatches, totalWins, Num(skewedWinRate), Csv(agentName),
                roundLimitMatches, Num(roundLimitShare),
                entry.OneSidedRoundLimitSurvivingHeroes, entry.OneSidedRoundLimitHitPointFraction, entry.OneSidedRoundLimitDraws));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// EINE Zeile JE PARTIE (Bachelorarbeit-Erweiterung "Export je Partie", siehe
    /// <see cref="MatchRecord"/>), gzip-komprimiert. Ergänzt die drei Aggregat-Dateien,
    /// ersetzt sie nicht. Reihenfolge: wie übergeben (vom Runner: Paarung, dann
    /// Ausgangslage - unabhängig von der Threadanzahl).
    ///
    /// SPALTEN: Paarung (Index = Zeile der Paarungs-CSV, 0-basiert), TeamA, TeamB
    /// (Kompositionsschlüssel), Ausgangslage (Index), Vertauscht (1 = teamvertauschte
    /// Zweitpartie, nur mit useAntitheticPairing), Seed, Startteam, Sieger (TeamA, TeamB,
    /// Draw), Endgrund (Elimination, RoundLimit), Aufloesung (SurvivingHeroes,
    /// HitPointFraction, Draw; leer bei Auslöschung), Runden, UeberlebendeTeamA/B
    /// (lebende Helden am Ende, ohne Klone), RestHpTeamA/B (Rest-Lebenspunkte der Helden;
    /// das Maximum steht als MaxHpTeamA/B in der Paarungs-CSV), Agent. Ganzzahlen und
    /// englische Aufzählungswerte - maschinenlesbar, keine Dezimalzahlen, also kein
    /// Konflikt mit dem Dezimalkomma der übrigen Dateien.
    ///
    /// GRÖSSE: rund 80 Byte je Zeile unkomprimiert; gzip verkleinert das stark, weil
    /// sich Schlüssel und Aufzählungswerte wiederholen.
    /// </summary>
    public static void ExportMatches(
        IEnumerable<MatchRecord> matches,
        IReadOnlyList<(Setup.TeamComposition TeamA, Setup.TeamComposition TeamB)> pairings,
        string filePath,
        string agentName)
    {
        using var file = File.Create(filePath);
        using var gzip = new GZipStream(file, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzip, new UTF8Encoding(false));

        writer.WriteLine(string.Join(Separator, "Paarung", "TeamA", "TeamB", "Ausgangslage", "Vertauscht", "Seed", "Startteam",
            "Sieger", "Endgrund", "Aufloesung", "Runden",
            "UeberlebendeTeamA", "UeberlebendeTeamB", "RestHpTeamA", "RestHpTeamB", "Agent"));

        string agent = Csv(agentName);
        foreach (var m in matches)
        {
            var (teamA, teamB) = pairings[m.PairingIndex];
            writer.WriteLine(string.Join(Separator,
                m.PairingIndex, Csv(teamA.Key), Csv(teamB.Key), m.SetupIndex, m.TeamsSwapped ? 1 : 0, m.Seed, m.StartingTeam,
                m.Winner, m.EndReason, m.Resolution?.ToString() ?? string.Empty, m.RoundsPlayed,
                m.TeamASurvivingHeroes, m.TeamBSurvivingHeroes, m.TeamARemainingHitPoints, m.TeamBRemainingHitPoints, agent));
        }
    }

    /// <summary>
    /// Zwischenstand je Held während der Aggregation in
    /// <see cref="ExportHeroWinRates"/> - getrennt nach "einseitigen"
    /// (bereinigten, primären) und "beidseitigen" (nur als Anzahl
    /// ausgewiesenen) Partien, siehe dort.
    /// </summary>
    private readonly record struct HeroAccumulator(
        HeroDefinition Hero,
        HeroActionCounts Actions,
        int OneSidedMatches = 0,
        int OneSidedWins = 0,
        double OneSidedRoundsSum = 0,
        int BothSidesMatches = 0,
        int BothSidesWins = 0,
        int OneSidedRoundLimitSurvivingHeroes = 0,
        int OneSidedRoundLimitHitPointFraction = 0,
        int OneSidedRoundLimitDraws = 0);

    /// <summary>Siehe Klassendoku "ZAHLENFORMAT": Komma als Dezimaltrennzeichen, 4 Nachkommastellen, nie eine Tausender-Gruppierung ("F"-Formatstrings gruppieren in .NET grundsätzlich nicht).</summary>
    private static string Num(double value) => value.ToString("F4", CultureInfo.GetCultureInfo("de-DE"));

    /// <summary>Minimales CSV-Escaping (Anführungszeichen verdoppeln + umschließen, falls der Wert den Trenner oder Anführungszeichen enthält) - die hier genutzten Werte (Helden-Ids) enthalten davon zwar nichts, robust ist es trotzdem.</summary>
    private static string Csv(string value) =>
        value.Contains(Separator) || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
