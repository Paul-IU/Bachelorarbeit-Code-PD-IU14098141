using System.Text;
using TacticsCore.Board;
using TacticsCore.Combat;
using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Movement;
using TacticsCore.Units;

namespace TacticsCore.Rendering;

/// <summary>
/// Reine Verifikationshilfe: gibt Spielfeld und Belegung (inkl. HP) als
/// ASCII-Grafik aus, damit sich Geometrie, Bewegung und Kampf ohne Godot-
/// Visualisierung von Auge prüfen lassen. Hat keinerlei Einfluss auf die
/// Spiellogik selbst.
/// </summary>
public static class AsciiRenderer
{
    /// <summary>Mindestbreite einer Zelle, auch wenn der Inhalt kürzer ist.</summary>
    private const int MinCellWidth = 13;

    /// <summary>
    /// Berechnet für ein Feld die vertikale "Zeilenposition" innerhalb der
    /// ASCII-Darstellung. Diese Zahl leitet sich direkt aus der axialen
    /// flat-top-Umrechnung y = r + q/2 ab (siehe Board.Board), skaliert mit
    /// dem Faktor 2, um ohne Kommazahlen auszukommen: rowIndex = 2r + q.
    /// Dadurch passt sich die Darstellung automatisch an, falls die
    /// Felddaten in Board.Board später geändert werden.
    /// </summary>
    private static int RowIndex(Field field) => 2 * field.Coordinate.R + field.Coordinate.Q;

    /// <summary>
    /// Rendert das Spielfeld inklusive Feldtypen, Startfeld-Markierung und
    /// aktueller Einheiten-Belegung (Bezeichnung + HP je Einheit) als
    /// mehrzeiligen ASCII-Text. Da ein Feld von beliebig vielen Einheiten
    /// gleichzeitig belegt sein kann, wird die Zellbreite dynamisch an den
    /// längsten tatsächlich vorkommenden Zellinhalt angepasst.
    ///
    /// Wird zusätzlich <paramref name="selectedUnit"/> angegeben, markiert
    /// jede Zelle zusätzlich, ob sie innerhalb der Bewegungsreichweite
    /// dieser Einheit liegt (mit benötigter Schrittzahl, "R&lt;n&gt;") und
    /// ob sie mindestens ein gültiges Angriffsziel dieser Einheit enthält
    /// ("ATK") - so lassen sich Reichweiten- und Angriffsberechnung visuell
    /// prüfen.
    ///
    /// <paramref name="unitLabel"/> erlaubt es dem Aufrufer, statt der
    /// Standard-Kennzeichnung (Teamziffer 1/2) eine eigene, kurze
    /// Bezeichnung je Einheit anzugeben (z.B. Buchstaben-Namen wie "A"/"X"
    /// statt anonymer Teamziffern, siehe z.B. das Konsolenprogramm) - ohne
    /// Angabe bleibt das bisherige Verhalten unverändert.
    /// </summary>
    public static string Render(GameState state, Unit? selectedUnit = null, Func<Unit, string>? unitLabel = null)
    {
        var board = state.Board;
        var columns = board.Fields
            .Select(f => f.Coordinate.Q)
            .Distinct()
            .OrderBy(q => q)
            .ToList();

        var fieldsByColumn = columns.ToDictionary(
            q => q,
            q => board.Fields.Where(f => f.Coordinate.Q == q).ToList());

        int minRow = board.Fields.Min(RowIndex);
        int maxRow = board.Fields.Max(RowIndex);

        // Für die ausgewählte Einheit (falls vorhanden) einmalig die
        // erreichbaren Felder und gültigen Angriffsziele bestimmen, statt
        // das pro Zelle neu zu berechnen.
        Dictionary<HexCoordinate, int>? reachableSteps = selectedUnit is null
            ? null
            : MovementSystem.GetReachableFields(selectedUnit, state).ToDictionary(rf => rf.Coordinate, rf => rf.Steps);

        HashSet<HexCoordinate>? attackTargetFields = selectedUnit is null
            ? null
            : CombatSystem.GetValidAttackTargets(selectedUnit, state).Select(u => u.Position).ToHashSet();

        // Zellinhalte einmal vorab berechnen (statt beim Rendern doppelt),
        // damit die Spaltenbreite an den längsten Inhalt angepasst werden kann.
        var cellTexts = board.Fields.ToDictionary(
            f => f.Coordinate,
            f => FormatCell(f, state, reachableSteps, attackTargetFields, unitLabel));
        int cellWidth = Math.Max(MinCellWidth, cellTexts.Values.Max(t => t.Length) + 2);

        var sb = new StringBuilder();

        if (selectedUnit is not null)
        {
            sb.AppendLine($"Ausgewählte Einheit: {selectedUnit.Id.ToString("N")[..8]} (Team {selectedUnit.Team}, Position {selectedUnit.Position}, Reichweite {selectedUnit.MovementRange}, {selectedUnit.CurrentHitPoints}/{selectedUnit.MaxHitPoints} HP)");
        }

        // Kopfzeile mit den Spaltentiteln.
        foreach (var q in columns)
        {
            sb.Append($"Spalte q={q}".PadRight(cellWidth));
        }
        sb.AppendLine();

        for (int row = minRow; row <= maxRow; row++)
        {
            foreach (var q in columns)
            {
                var field = fieldsByColumn[q].FirstOrDefault(f => RowIndex(f) == row);
                string cell = field is null ? string.Empty : cellTexts[field.Coordinate];
                sb.Append(cell.PadRight(cellWidth));
            }
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Legende: A=Blüte A, B=Blüte B, C=Verbindungsfeld, *=Startfeld, -=leer");
        sb.AppendLine("         Einheiten als Bezeichnung:HP, mehrere durch Komma getrennt (z.B. A:50,X:30)");
        sb.AppendLine("         ~ vor der Bezeichnung = temporäre Einheit (Klon), ! nach den HP = aktuell bewegungsunfähig");
        sb.AppendLine("         {Effekt:Restdauer} = aktive Effekte auf der Einheit (Anheftungen, Auf-/Abladen, Buffs/Debuffs, ...)");
        sb.AppendLine("         (+n) hinter einem Effekt = angesammelter Angriffsbonus (z.B. Bogenschütze-Aufladestufe)");
        sb.AppendLine("         [Effekt:Restdauer] hinter der Belegung = aktiver, feld-verankerter Effekt (Feldsperre/Umleitung/Flächenwirkung)");
        if (selectedUnit is not null)
        {
            sb.AppendLine("         R<n> = für die ausgewählte Einheit in n Schritten erreichbar (0 = eigenes Feld)");
            sb.AppendLine("         ATK  = Feld enthält mind. ein gültiges Angriffsziel der ausgewählten Einheit");
        }
        sb.AppendLine();
        sb.AppendLine("Hinweis zur Form: Spalte q=-1 hat keine Reihe r=0, Spalte q=+1 keine Reihe r=5");
        sb.AppendLine("(siehe Board.Board) - die freien Ecken oben-links/unten-rechts sind daher");
        sb.AppendLine("KEINE fehlende Zeile, sondern die beabsichtigte, leicht versetzte Blütenform.");

        return sb.ToString();
    }

    private static string FormatCell(
        Field field,
        GameState state,
        Dictionary<HexCoordinate, int>? reachableSteps,
        HashSet<HexCoordinate>? attackTargetFields,
        Func<Unit, string>? unitLabel)
    {
        string typeLetter = field.Type switch
        {
            FieldType.BlossomA => "A",
            FieldType.BlossomB => "B",
            FieldType.Connector => "C",
            _ => "?",
        };
        string startMarker = field.IsStartField ? "*" : " ";

        var units = state.GetUnitsAt(field.Coordinate);
        string unitsPart = units.Count == 0
            ? "-"
            : string.Join(",", units.Select(u => FormatUnit(u, state, unitLabel)));

        string fieldEffectsPart = FormatEffects(state.GetEffectsOnField(field.Coordinate), brackets: ('[', ']'));

        string overlay = "";
        if (reachableSteps is not null && reachableSteps.TryGetValue(field.Coordinate, out int steps))
        {
            overlay += $" R{steps}";
        }
        if (attackTargetFields is not null && attackTargetFields.Contains(field.Coordinate))
        {
            overlay += " ATK";
        }

        return $"{field.Coordinate}{typeLetter}{startMarker}:{unitsPart}{fieldEffectsPart}{overlay}";
    }

    /// <summary>
    /// Bezeichnung einer einzelnen Einheit inklusive Klon-Markierung ("~"
    /// vorangestellt, siehe <see cref="Unit.IsClone"/>), aktiver, an sie
    /// gebundener Effekte (siehe <see cref="ActiveEffect"/>) und einer
    /// generischen Bewegungsunfähigkeits-Markierung ("!"), falls einer der
    /// aktiven Effekte <see cref="ActiveEffect.PreventsMovement"/> setzt
    /// (z.B. Wand während der Feldsperre, Bogenschütze während des
    /// Aufladens) - unabhängig davon, welcher Held/Effekt das auslöst.
    /// </summary>
    private static string FormatUnit(Unit unit, GameState state, Func<Unit, string>? unitLabel)
    {
        string cloneMarker = unit.IsClone ? "~" : "";
        string label = Label(unit, unitLabel);
        var unitEffects = state.GetEffectsOnUnit(unit.Id).ToList();
        string effectsPart = FormatEffects(unitEffects, brackets: ('{', '}'));
        string immobileMarker = unitEffects.Any(e => e.PreventsMovement) ? "!" : "";
        return $"{cloneMarker}{label}:{unit.CurrentHitPoints}{immobileMarker}{effectsPart}";
    }

    /// <summary>
    /// Formatiert eine Effektliste als "&lt;Klammer-auf&gt;Name:Restdauer&lt;Klammer-zu&gt;",
    /// leer wenn keine Effekte aktiv sind. Ein Effekt ohne begrenzte Dauer
    /// zeigt statt der Zahl "-". Trägt der Effekt zusätzlich einen
    /// angesammelten Angriffsbonus (siehe <see cref="ActiveEffect.BonusDamageForNextAttack"/> -
    /// z.B. die Aufladestufe des Bogenschützen), wird er als "(+n)"
    /// angehängt, damit die konkrete Ladungsstufe sichtbar ist, nicht nur
    /// die Existenz des Effekts.
    /// </summary>
    private static string FormatEffects(IEnumerable<ActiveEffect> effects, (char Open, char Close) brackets)
    {
        var list = effects.ToList();
        if (list.Count == 0)
        {
            return "";
        }

        string joined = string.Join("+", list.Select(e =>
        {
            string duration = e.RemainingDuration is int d ? d.ToString() : "-";
            string bonus = e.BonusDamageForNextAttack is int b ? $"(+{b})" : "";
            return $"{e.Name}:{duration}{bonus}";
        }));
        return $" {brackets.Open}{joined}{brackets.Close}";
    }

    /// <summary>Bezeichnung einer Einheit: <paramref name="unitLabel"/>, falls angegeben, sonst die bisherige Teamziffer (1/2).</summary>
    private static string Label(Unit unit, Func<Unit, string>? unitLabel) =>
        unitLabel is not null ? unitLabel(unit) : (unit.Team == Team.TeamA ? "1" : "2");
}
