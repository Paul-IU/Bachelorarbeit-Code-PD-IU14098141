namespace TacticsCore.Abilities;

/// <summary>
/// Zielart einer Fähigkeit (Anforderung 7). Bestimmt, welche Kandidaten
/// <see cref="AbilitySystem.GetLegalAbilityUses"/> als wählbare Ziele
/// anbietet.
/// </summary>
public enum AbilityTargetType
{
    /// <summary>Nur die eigene Einheit - keine echte Wahl, genau ein Kandidat.</summary>
    Self,

    /// <summary>
    /// Eine einzelne verbündete Einheit (kein Klon) innerhalb der
    /// Reichweite - DER ANWENDER SELBST ZÄHLT AUSDRÜCKLICH ALS GÜLTIGES
    /// ZIEL (Bachelorarbeit-Regel, einheitlich für jede Fähigkeit mit
    /// dieser Zielart: "verbündet" schließt "man selbst" mit ein, ein Held
    /// ist sich selbst gegenüber nicht feindlich). Betroffen ist aktuell
    /// nur die Elfe ("Heilung muss auch auf sich selbst wirken können") -
    /// für eine Fähigkeit, die den Anwender bewusst ausschließen soll, wäre
    /// stattdessen ein eigener Filter auf den zurückgegebenen Optionen nötig
    /// (aktuell von keinem Helden gebraucht). Siehe
    /// <see cref="AbilitySystem.GetLegalTargets"/>/<see cref="AbilitySystem.IsValidUnitTarget"/>
    /// für die Umsetzung dieser einen, zentralen Stelle.
    /// </summary>
    Ally,

    /// <summary>Eine einzelne gegnerische Einheit innerhalb der Reichweite.</summary>
    Enemy,

    /// <summary>
    /// Eine einzelne Einheit innerhalb der Reichweite, UNABHÄNGIG von deren
    /// Team (weder nur Ally noch nur Enemy) - für Fähigkeiten, deren Wirkung
    /// sich erst anhand des GEWÄHLTEN Ziels unterscheidet (siehe
    /// Effects.EffectTemplate.StatModifiersForEnemy und Hugger).
    /// </summary>
    AnyUnit,

    /// <summary>Ein beliebiges Feld innerhalb der Reichweite (z.B. für eine Feldsperre).</summary>
    Field,

    /// <summary>Ein Feld innerhalb der Reichweite - die Wirkung trifft alle dort stehenden Einheiten.</summary>
    AllUnitsOnField,

    /// <summary>Ein Zentrumsfeld innerhalb der Reichweite - die Wirkung trifft alle Einheiten in einem zusätzlichen Radius darum (siehe <see cref="AbilityDefinition.Radius"/>).</summary>
    AllUnitsInRadius,
}
