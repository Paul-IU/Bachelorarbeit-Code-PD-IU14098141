namespace TacticsCore.Batch;

/// <summary>
/// Rein kombinatorische Erwartungswerte für die Helden-Siegquote eines
/// VÖLLIG NEUTRALEN Helden (Bachelorarbeit-Korrektur, siehe
/// <see cref="CsvExporter.ExportHeroWinRates"/> für die Einordnung der
/// tatsächlich gemessenen Werte). Beide Werte hängen AUSSCHLIESSLICH von der
/// Katalog- und der Teamgröße ab, nicht von irgendeinem Simulationsergebnis -
/// bewusst nicht fest verdrahtet, damit sie sich automatisch anpassen, wenn
/// der Heldenkatalog später erweitert wird.
///
/// HERLEITUNG DER ROHEN (UNBEREINIGTEN) ERWARTUNG:
/// Bei einem Katalog von <c>catalogSize</c> Helden und einer Teamgröße von
/// <c>teamSize</c> gibt es T = C(catalogSize, teamSize) mögliche
/// Kompositionen, von denen s = C(catalogSize-1, teamSize-1) einen
/// bestimmten Helden X enthalten (die restlichen teamSize-1 Plätze werden
/// aus den verbleibenden catalogSize-1 Helden besetzt). Unter allen
/// T*(T+1)/2 möglichen (ungeordneten, Selbstpaarung eingeschlossen)
/// Paarungen zweier Kompositionen stehen:
///   - s*(s+1)/2 Paarungen mit X auf BEIDEN Seiten zugleich (X tritt darin
///     zwangsläufig sowohl als "Sieger" als auch als "Verlierer" an - der
///     Ausgang sagt nichts über die Stärke von X aus),
///   - s*(T-s) Paarungen mit X auf GENAU EINER Seite (die eigentlich
///     aussagekräftigen, siehe <see cref="CsvExporter.ExportHeroWinRates"/>).
/// Nimmt man für die "einseitigen" Paarungen eine exakt neutrale 50%-
/// Siegquote an (per Definition seiner Neutralität), ergibt sich für die
/// ROHE Gesamt-Siegquote (beide Gruppen zusammen, wie bei der bisherigen,
/// unbereinigten Kennzahl):
///
///   RawWinRate = [s*(s+1)/2 * 1  +  s*(T-s) * 0,5] / [s*(s+1)/2 + s*(T-s)]
///
/// was sich algebraisch zu der unten implementierten geschlossenen Form
///
///   RawWinRate = (T + 1) / (2*T - s + 1)
///
/// vereinfacht. Für den Standardkatalog (9 Helden, Teamgröße 3: T=84, s=28)
/// ergibt das (84+1)/(2*84-28+1) = 85/141 ≈ 60,28% - exakt der Wert, den
/// ein neutraler Held in der bisherigen, unbereinigten Kennzahl erreicht,
/// OHNE dass er tatsächlich stärker oder schwächer als seine acht
/// Katalog-Kollegen wäre.
///
/// Die BEREINIGTE Erwartung ist unabhängig von Katalog- oder Teamgröße
/// trivial immer exakt 50% (nur "einseitige" Paarungen fließen ein) - wird
/// hier trotzdem mitgeliefert, damit ein Aufrufer beide Bezugswerte aus
/// derselben Quelle bezieht.
/// </summary>
public static class HeroWinRateExpectations
{
    /// <summary>Anzahl Kompositionen, die EINEN bestimmten Helden enthalten: C(catalogSize-1, teamSize-1).</summary>
    public static long CompositionsContainingOneHero(int catalogSize, int teamSize) =>
        BinomialCoefficient(catalogSize - 1, teamSize - 1);

    /// <summary>Gesamtzahl möglicher Kompositionen: C(catalogSize, teamSize).</summary>
    public static long TotalCompositions(int catalogSize, int teamSize) =>
        BinomialCoefficient(catalogSize, teamSize);

    /// <summary>
    /// Erwartete Siegquote eines VÖLLIG NEUTRALEN Helden - siehe
    /// Klassendokumentation für die Herleitung. <c>CleanedWinRate</c> ist
    /// unabhängig von Katalog-/Teamgröße immer exakt 0,5.
    /// </summary>
    public static (double RawWinRate, double CleanedWinRate) NeutralExpectation(int catalogSize, int teamSize)
    {
        long t = TotalCompositions(catalogSize, teamSize);
        long s = CompositionsContainingOneHero(catalogSize, teamSize);
        double raw = (t + 1.0) / (2.0 * t - s + 1.0);
        return (raw, 0.5);
    }

    /// <summary>
    /// Klassischer multiplikativer Algorithmus für den Binomialkoeffizienten
    /// C(n,k) - die Reihenfolge "erst multiplizieren, dann durch (i+1)
    /// teilen" garantiert, dass JEDES Zwischenergebnis bereits ganzzahlig
    /// ist (bekannte Eigenschaft der Pascal'schen Dreieck-Rekursion), daher
    /// keine Rundung trotz Ganzzahl-Division.
    /// </summary>
    private static long BinomialCoefficient(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }

        long result = 1;
        for (int i = 0; i < k; i++)
        {
            result = result * (n - i) / (i + 1);
        }

        return result;
    }
}
