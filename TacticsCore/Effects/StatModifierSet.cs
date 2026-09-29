namespace TacticsCore.Effects;

/// <summary>
/// Änderungen an den vier für Bewegung/Kampf/Zugreihenfolge relevanten
/// Attributwerten einer Einheit. Mehrere gleichzeitig aktive Modifikatoren
/// (aus Buffs, Debuffs und passiven Fähigkeiten) werden zusammengefasst
/// (siehe <see cref="EffectiveStats"/>): die Deltas werden aufsummiert.
///
/// PROZENTUALER SCHADENSMODIFIKATOR (Bachelorarbeit-Designentscheidung
/// "Umklammerung prozentual"): <see cref="AttackDamageMultiplier"/> wirkt
/// multiplikativ auf den Angriffsschaden; mehrere Multiplikatoren werden
/// miteinander multipliziert. Die früher bewusst vermiedene Reihenfolge-Frage
/// ist fest entschieden: ERST werden alle Deltas addiert, DANN wird der
/// Multiplikator angewandt, dann gerundet (siehe
/// <see cref="EffectiveStats.AttackDamage"/>). Ein prozentualer Abzug wirkt so
/// auch auf gebuffte Werte anteilig. Standardwert 1 = keine Wirkung.
/// Mehrere prozentuale Modifikatoren werden MULTIPLIZIERT - das Ergebnis hängt
/// damit nicht von der Reihenfolge ab (z.B. Kampflied 1,4 × eigene Umklammerung
/// 1,33 × gegnerische Umklammerung 0,67 = 1,2475, in jeder Reihenfolge).
/// Folge: gleich große Zu- und Abschläge heben sich NICHT auf (1,33 × 0,67 =
/// 0,8911), anders als bei einer Addition der Prozentsätze (1 + 0,33 − 0,33 = 1).
/// Prozentuale Wirkungen auf EINGEHENDEN Schaden (Schildwall, Mauer) bleiben
/// davon getrennt (siehe <see cref="ActiveEffect.IncomingDamageMultiplier"/>).
/// </summary>
public sealed record StatModifierSet(
    int BaseAttackDamageDelta = 0,
    int MovementRangeDelta = 0,
    int AttackRangeDelta = 0,
    int SpeedDelta = 0,
    double AttackDamageMultiplier = 1.0)
{
    public static readonly StatModifierSet None = new();

    public static StatModifierSet operator +(StatModifierSet a, StatModifierSet b) => new(
        a.BaseAttackDamageDelta + b.BaseAttackDamageDelta,
        a.MovementRangeDelta + b.MovementRangeDelta,
        a.AttackRangeDelta + b.AttackRangeDelta,
        a.SpeedDelta + b.SpeedDelta,
        a.AttackDamageMultiplier * b.AttackDamageMultiplier);
}
