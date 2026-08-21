namespace TacticsCore.Effects;

/// <summary>
/// Regelt, was passiert, wenn ein NEUER Effekt angewendet wird, während auf
/// demselben Ziel (gleicher Anker, gleiches Unit-/Feldziel) bereits ein
/// gleichnamiger Effekt aktiv ist (Anforderung 10: "Einheitliche,
/// dokumentierte Regel für gleichartige Effekte"). "Gleichartig" bedeutet
/// hier: gleicher <see cref="ActiveEffect.Name"/> UND gleiches Ziel - ein
/// Debuff von Held X und ein gleichnamiger Debuff von Held Y auf
/// UNTERSCHIEDLICHEN Zielen konkurrieren nicht miteinander.
/// </summary>
public enum StackingRule
{
    /// <summary>
    /// Der neue Effekt wird als ZUSÄTZLICHE, unabhängige Instanz angelegt -
    /// beide wirken parallel (z.B. mehrere Schaden-über-Zeit-Stapel, die
    /// jeweils eigenständig ticken und ablaufen).
    /// </summary>
    Stack,

    /// <summary>
    /// Der bestehende Effekt wird entfernt und durch den neuen vollständig
    /// ersetzt (z.B. wenn sich die Stärke geändert haben könnte).
    /// </summary>
    Replace,

    /// <summary>
    /// Der bestehende Effekt bleibt unverändert bestehen - nur seine
    /// Restdauer wird auf die Dauer des neuen Effekts zurückgesetzt. Das ist
    /// die Standardregel (siehe <see cref="EffectTemplate"/>) für die
    /// meisten Buffs/Debuffs: erneutes Anwenden verlängert die Wirkung,
    /// verändert sie aber nicht.
    /// </summary>
    RefreshDuration,

    /// <summary>
    /// Wie <see cref="Replace"/>, aber die Suche nach einer bestehenden
    /// Instanz berücksichtigt NUR Name und Quelle (Verursacher) - das ZIEL
    /// spielt keine Rolle. Für Fähigkeiten mit genau EINER exklusiven, aber
    /// auf ein NEUES Ziel VERSCHIEBBAREN Wirkung (z.B. die Anheftung des
    /// Hugger, die bei erneutem Einsatz auf einen anderen Zielt springt):
    /// mit der normalen <see cref="Replace"/>-Regel (die nach demselben Ziel
    /// sucht) würde die alte Anheftung auf dem VORHERIGEN Ziel schlicht
    /// bestehen bleiben, weil das neue Ziel ein anderes ist.
    /// </summary>
    ReplaceForSameSource,

    /// <summary>
    /// Statt zu ersetzen, wird der numerische Bonus
    /// (<see cref="ActiveEffect.BonusDamageForNextAttack"/>) der bestehenden
    /// Instanz um den Wert des neuen Effekts ERHÖHT, begrenzt durch
    /// <see cref="EffectTemplate.MaximumAccumulatedBonusDamage"/> - für einen
    /// über mehrere Einsätze ansteigenden, gedeckelten Aufladezustand (siehe
    /// Bogenschütze). Keine der drei bestehenden Regeln deckte ein
    /// "mit jeder erneuten Anwendung STÄRKER statt neu/aufgefrischt/gestapelt"
    /// ab.
    /// </summary>
    Accumulate,
}
