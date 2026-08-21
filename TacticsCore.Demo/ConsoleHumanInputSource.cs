using TacticsCore.Abilities;
using TacticsCore.Agents;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Demo;

/// <summary>
/// Konsolen-Umsetzung von <see cref="IHumanInputSource"/> (Anforderung 14):
/// gibt die legalen Züge nummeriert aus, liest die Auswahl über die
/// Konsole ein und wiederholt die Abfrage bei ungültiger Eingabe. Kennt
/// selbst keinerlei Spielregel - sie bekommt die bereits fertig berechnete
/// Liste legaler Züge von <see cref="HumanAgent"/> übergeben und liefert nur
/// die Auswahl zurück.
/// </summary>
public sealed class ConsoleHumanInputSource : IHumanInputSource
{
    /// <summary>Optionale, kurze Bezeichnungen je Einheit (z.B. "T"/"X" statt der vollen Guid) - rein kosmetisch für die Anzeige.</summary>
    private readonly Func<Unit, string>? _unitLabel;

    public ConsoleHumanInputSource(Func<Unit, string>? unitLabel = null) => _unitLabel = unitLabel;

    public TurnAction ChooseAction(Unit unit, GameState state, IReadOnlyList<TurnAction> legalActions)
    {
        Console.WriteLine();
        Console.WriteLine($">>> {Label(unit)} ist am Zug ({unit.CurrentHitPoints}/{unit.MaxHitPoints} HP, Position {unit.Position})");

        for (int i = 0; i < legalActions.Count; i++)
        {
            Console.WriteLine($"  [{i}] {Describe(legalActions[i])}");
        }

        while (true)
        {
            Console.Write("Wahl (Zahl eingeben): ");
            string? input = Console.ReadLine();

            // Console.ReadLine() liefert null, wenn die Eingabe unerwartet
            // endet (z.B. EOF bei umgeleiteter/erschöpfter Standardeingabe) -
            // ohne diese Prüfung würde die Wiederholschleife unten
            // andernfalls endlos "ungültige Eingabe" melden, ohne je wieder
            // eine echte Eingabemöglichkeit zu bekommen.
            if (input is null)
            {
                throw new InvalidOperationException("Eingabe unerwartet beendet (EOF) - der interaktive Modus benötigt eine tatsächliche Konsoleneingabe.");
            }

            if (int.TryParse(input, out int index) && index >= 0 && index < legalActions.Count)
            {
                return legalActions[index];
            }

            Console.WriteLine($"Ungültige Eingabe - bitte eine Zahl von 0 bis {legalActions.Count - 1} eingeben.");
        }
    }

    private string Describe(TurnAction action) => action switch
    {
        TurnAction.Move move => $"Bewegen nach {move.Target}",
        TurnAction.Attack attack => $"Angreifen: {Label(attack.Target)} ({attack.Target.CurrentHitPoints}/{attack.Target.MaxHitPoints} HP auf {attack.Target.Position})",
        TurnAction.UseAbility useAbility => DescribeAbility(useAbility),
        TurnAction.EndTurn => "Passen / Zug beenden",
        _ => action.GetType().Name,
    };

    private string DescribeAbility(TurnAction.UseAbility useAbility)
    {
        string targetPart = useAbility.Target switch
        {
            AbilityTarget.UnitTarget u => $"Ziel: {Label(u.Unit)}",
            AbilityTarget.FieldTarget f => $"Zielfeld: {f.Field}",
            _ => "",
        };
        string modePart = useAbility.Mode is not null ? $" [Modus: {useAbility.Mode.DisplayName}]" : "";
        return $"Fähigkeit einsetzen ({targetPart}){modePart}";
    }

    private string Label(Unit unit) => _unitLabel is not null ? _unitLabel(unit) : unit.Id.ToString("N")[..8];
}
