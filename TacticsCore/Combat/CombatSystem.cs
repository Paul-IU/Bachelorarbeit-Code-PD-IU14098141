using TacticsCore.Hex;
using TacticsCore.Rules;
using TacticsCore.Units;

namespace TacticsCore.Combat;

/// <summary>
/// Eigene, von <see cref="Unit"/> getrennte Kampflogik: Angriffsmuster,
/// Zielermittlung und die Ausführung eines Grundangriffs inklusive
/// Niederlage und Entfernen vom Spielfeld.
/// </summary>
public static class CombatSystem
{
    /// <summary>
    /// Liefert die Felder, die vom Angriffsmuster einer Einheit abgedeckt
    /// werden: alle existierenden Spielfelder mit Hex-Distanz
    /// &lt;= <see cref="CombatRules.AttackPatternRadius"/> zur
    /// aktuellen Position. Beim aktuellen Radius 1 sind das das eigene
    /// Feld PLUS alle sechs direkt angrenzenden Felder.
    ///
    /// Bewusst über die reine Hex-Distanz und NICHT über eine Breitensuche
    /// bestimmt: Anders als bei der Bewegung geht es beim Angriff nicht um
    /// erreichbare Wege, sondern rein geometrisch darum, wie nah ein Feld
    /// am Angreifer liegt - unabhängig von der tatsächlichen
    /// Feldverbindung. Bewusst wiederverwendbar für spätere
    /// Flächenfähigkeiten, die alle Einheiten in einem Muster treffen statt
    /// nur ein einzelnes Ziel (siehe <see cref="GetValidAttackTargets"/>).
    /// </summary>
    public static IReadOnlyList<HexCoordinate> GetAttackPatternFields(Unit attacker, GameState state) =>
        state.Board.Fields
            .Select(f => f.Coordinate)
            .Where(c => HexCoordinate.Distance(attacker.Position, c) <= CombatRules.AttackPatternRadius)
            .ToList();

    /// <summary>
    /// Liefert alle aktuell gültigen Angriffsziele - einzelne Einheiten,
    /// nicht Felder - für einen Grundangriff von <paramref name="attacker"/>.
    /// Ein Ziel ist gültig, wenn es auf einem Feld des Angriffsmusters
    /// steht, nicht besiegt ist und (sofern
    /// <see cref="CombatRules.RequireEnemyTarget"/> aktiv ist) einem
    /// anderen Team angehört als der Angreifer. Stabil sortiert nach
    /// (q, r, UnitId), damit Agenten bei mehreren gleichwertigen Zielen
    /// (z.B. zwei Gegnern auf demselben Feld) reproduzierbar entscheiden
    /// können.
    ///
    /// WICHTIG - Angriffe werden durch andere Einheiten NICHT blockiert:
    /// es gibt keine Sichtlinien- oder Deckungsprüfung. Jedes Feld im
    /// Angriffsmuster zählt unabhängig davon, was dazwischen steht (bei
    /// Radius 1 gibt es ohnehin kein "Dazwischen" - jedes Zielfeld ist das
    /// eigene Feld oder ein direkter Nachbar).
    /// </summary>
    public static IReadOnlyList<Unit> GetValidAttackTargets(Unit attacker, GameState state)
    {
        if (attacker.IsDefeated)
        {
            return Array.Empty<Unit>();
        }

        return GetAttackPatternFields(attacker, state)
            .SelectMany(state.GetUnitsAt)
            .Where(u => !u.IsDefeated)
            .Where(u => !CombatRules.RequireEnemyTarget || u.Team != attacker.Team)
            .OrderBy(u => u.Position.Q)
            .ThenBy(u => u.Position.R)
            .ThenBy(u => u.Id)
            .ToList();
    }

    /// <summary>Prüft, ob <paramref name="target"/> aktuell ein gültiges Angriffsziel für <paramref name="attacker"/> ist.</summary>
    public static bool CanAttack(Unit attacker, Unit target, GameState state) =>
        GetValidAttackTargets(attacker, state).Contains(target);

    /// <summary>
    /// Führt einen Grundangriff von <paramref name="attacker"/> auf genau
    /// EIN gewähltes Ziel aus. Stehen mehrere Einheiten auf dem Zielfeld,
    /// entscheidet allein <paramref name="target"/>, welche davon
    /// getroffen wird - die übrigen bleiben unberührt (siehe
    /// <see cref="GetValidAttackTargets"/> für die Liste der Kandidaten).
    /// Ein ungültiger Angriff (Ziel außerhalb des Musters, kein gültiges
    /// Team, bereits besiegt oder gar nicht Teil der Partie) ändert weder
    /// HP noch Position irgendeiner Einheit.
    ///
    /// Fällt die HP des Ziels durch den Treffer auf 0, gilt es als besiegt
    /// und wird sofort aus dem <see cref="GameState"/> entfernt - es kann
    /// danach weder selbst handeln noch Ziel eines weiteren Angriffs sein.
    /// </summary>
    public static bool TryAttack(Unit attacker, Unit target, GameState state)
    {
        if (!CanAttack(attacker, target, state))
        {
            return false;
        }

        target.ApplyDamage(attacker.BaseAttackDamage);

        if (target.IsDefeated)
        {
            state.RemoveUnit(target);
        }

        return true;
    }
}
