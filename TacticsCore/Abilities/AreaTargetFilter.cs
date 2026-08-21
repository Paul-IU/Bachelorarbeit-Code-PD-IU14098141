namespace TacticsCore.Abilities;

/// <summary>
/// Schränkt bei einer flächenwirkenden Fähigkeit (<see cref="AbilityTargetType.AllUnitsOnField"/>,
/// <see cref="AbilityTargetType.AllUnitsInRadius"/>) ein, WELCHE der auf den
/// betroffenen Feldern stehenden Einheiten die Wirkung tatsächlich trifft.
///
/// Bislang trafen Flächenwirkungen ausnahmslos ALLE dort stehenden Einheiten
/// (Schildträger: "alle Einheiten auf dem eigenen Feld", unabhängig vom
/// Team - ein Gegner, der zufällig dasselbe Feld teilt, würde ebenfalls
/// profitieren, was für einen ortsbezogenen Schild plausibel ist). Für den
/// Musiker ("Wirkung auf alle VERBÜNDETEN im Umkreis") reicht das nicht -
/// eine reine Teamfilter-Erweiterung an der Fähigkeitsdefinition, KEIN
/// Sonderfall im Interpreter (siehe <see cref="AbilitySystem"/>): der
/// Filter wird an genau einer Stelle generisch ausgewertet und ist für
/// jede künftige Flächenfähigkeit (auch eine reine Gegner-Fläche) wieder
/// nutzbar.
/// </summary>
public enum AreaTargetFilter
{
    /// <summary>Alle Einheiten auf den betroffenen Feldern, unabhängig vom Team (bisheriges Verhalten, Standard).</summary>
    Any,

    /// <summary>Nur Einheiten desselben Teams wie der Anwender.</summary>
    AlliesOnly,

    /// <summary>Nur Einheiten eines anderen Teams als der Anwender.</summary>
    EnemiesOnly,
}
