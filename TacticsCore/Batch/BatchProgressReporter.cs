using System.Diagnostics;
using System.Globalization;

namespace TacticsCore.Batch;

/// <summary>
/// FORTSCHRITTSANZEIGE für lange Batch-Läufe (Bachelorarbeit-Erweiterung):
/// abgearbeitete Partien (absolut und in Prozent), abgeschlossene Paarungen,
/// vergangene Zeit und geschätzte Restzeit aus dem bisherigen Durchsatz. Zwei
/// Ausgabewege: dieselbe Terminalzeile wird fortlaufend überschrieben, und eine
/// Fortschrittsdatei wird bei jeder Aktualisierung ersetzt - so lässt sich der
/// Stand auch abfragen, wenn die Terminalausgabe erst nach Abschluss sichtbar
/// wird (z.B. beim Start aus Claude Code heraus).
///
/// KEIN EINFLUSS AUF DIE ERGEBNISSE: der Reporter liest nur die Zählerstände, die
/// <see cref="BatchSimulationRunner"/> ohnehin meldet, und schreibt nichts zurück.
///
/// GERINGE LAST: <see cref="Report"/> wird von JEDEM Worker-Thread nach jeder
/// Partie aufgerufen. Er vergleicht dort nur einen Zeitstempel; ist das
/// Ausgabeintervall erreicht, übernimmt genau EIN Thread per Interlocked.CompareExchange
/// die Ausgabe - ohne Sperre, die anderen Threads laufen ungebremst weiter.
///
/// ETIKETTEN-PRÜFUNG: der Runner meldet eine Partie erst, nachdem auch ihre
/// Prüfpartie gespielt ist. Der Durchsatz (gemeldete Partien je Sekunde) enthält
/// die doppelte Rechenzeit damit bereits - die Restzeit ist ohne Korrektur richtig.
/// Die Anzeige weist die Prüfpartien zusätzlich aus.
/// </summary>
public sealed class BatchProgressReporter
{
    private readonly string? _progressFilePath;
    private readonly bool _verifyNameIndependence;
    private readonly long _intervalTicks;
    private long _nextReportTicks;
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    /// <param name="progressFilePath">Pfad der Fortschrittsdatei; null = keine Datei.</param>
    /// <param name="verifyNameIndependence">Ob die Etiketten-Prüfung mitläuft (nur für die Anzeige).</param>
    /// <param name="interval">Mindestabstand zwischen zwei Aktualisierungen (Standard 2 s).</param>
    public BatchProgressReporter(string? progressFilePath, bool verifyNameIndependence, TimeSpan? interval = null)
    {
        _progressFilePath = progressFilePath;
        _verifyNameIndependence = verifyNameIndependence;
        _intervalTicks = (long)((interval ?? TimeSpan.FromSeconds(2)).TotalSeconds * Stopwatch.Frequency);
    }

    /// <summary>Fortschritts-Empfänger für <see cref="BatchSimulationRunner.Run"/> - thread-sicher.</summary>
    public void Report(BatchProgress progress)
    {
        long now = _clock.ElapsedTicks;
        long due = Interlocked.Read(ref _nextReportTicks);
        bool last = progress.CompletedMatches == progress.TotalMatches;
        if (!last && now < due)
        {
            return;
        }

        if (!last && Interlocked.CompareExchange(ref _nextReportTicks, now + _intervalTicks, due) != due)
        {
            return; // ein anderer Thread gibt gerade aus
        }

        Write(progress, finished: false);
    }

    /// <summary>Abschlussmeldung (letzter Stand, Restzeit 0).</summary>
    public void Finish(BatchProgress progress) => Write(progress, finished: true);

    /// <summary>Die Anzeige als Text, z.B. "Partien 12.345 / 257.040 (4,8 %) ...".</summary>
    public string Format(BatchProgress p, bool finished)
    {
        var culture = CultureInfo.GetCultureInfo("de-DE");
        double fraction = p.TotalMatches == 0 ? 1.0 : (double)p.CompletedMatches / p.TotalMatches;
        int completedPairings = finished ? p.TotalPairings : p.CompletedPairings;
        string remaining = finished ? "0:00:00"
            : p.CompletedMatches == 0 ? "unbekannt"
            : FormatDuration(TimeSpan.FromSeconds(p.Elapsed.TotalSeconds / p.CompletedMatches * (p.TotalMatches - p.CompletedMatches)));
        string check = _verifyNameIndependence ? " (+ ebenso viele Prüfpartien)" : "";
        return string.Format(culture,
            "Partien {0:N0} / {1:N0}{2} ({3:P1}) | Paarungen {4:N0} / {5:N0} | vergangen {6} | Rest ca. {7}{8}",
            p.CompletedMatches, p.TotalMatches, check, fraction, completedPairings, p.TotalPairings,
            FormatDuration(p.Elapsed), remaining, finished ? " | ABGESCHLOSSEN" : "");
    }

    private void Write(BatchProgress progress, bool finished)
    {
        string line = Format(progress, finished);
        Console.Write("\r" + line + "   ");

        if (_progressFilePath is null)
        {
            return;
        }

        try
        {
            // Ersetzen statt Überschreiben: wer die Datei liest, sieht nie einen halb geschriebenen Stand.
            string temp = _progressFilePath + ".tmp";
            // UTF-8 MIT BOM: sonst zeigen Windows-Werkzeuge (z.B. PowerShell 5.1) die Umlaute falsch an.
            File.WriteAllText(temp, $"{line}{Environment.NewLine}Stand: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}", new System.Text.UTF8Encoding(true));
            File.Move(temp, _progressFilePath, overwrite: true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Ein gerade lesender Prozess darf die Anzeige nicht abbrechen - nächste Aktualisierung versucht es erneut.
            // UnauthorizedAccessException ausdrücklich mit abgefangen (Korrektur nach einem Absturz): Windows meldet
            // "Zugriff verweigert", wenn die Zieldatei gerade geöffnet ist (z.B. vom Editor neu eingelesen wird). Das ist
            // KEINE IOException - vorher brach deshalb ein defensiver Lauf nach 2 h 17 min ab. Die Fortschrittsanzeige
            // darf einen Lauf nie beenden.
        }
    }

    private static string FormatDuration(TimeSpan t) => $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}";
}
