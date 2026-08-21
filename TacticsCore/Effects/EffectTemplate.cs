namespace TacticsCore.Effects;

/// <summary>
/// Bauplan für einen Effekt, wie ihn eine Fähigkeitsdefinition trägt
/// (siehe <see cref="TacticsCore.Abilities.AbilityDefinition"/>). Bewusst
/// EIN datengetriebener Typ mit vielen unabhängigen, optionalen Feldern
/// statt einer Klassenhierarchie pro Wirkungsart (Anforderung 8: "ohne für
/// jeden Fall Sonderlogik zu schreiben") - <see cref="Abilities.AbilitySystem"/>
/// liest genau diese Felder in EINEM generischen Interpreter aus, unabhängig
/// davon, welcher Held/welche Fähigkeit den Effekt erzeugt hat.
///
/// Zuordnung der geforderten Wirkungsarten zu den Feldern hier (siehe
/// Aufgabenstellung Punkt 8):
///   - Sofortschaden/-heilung          -&gt; IsInstant + InstantDamage/InstantHeal
///   - distanzabhängige Wirkung        -&gt; ScaleWithDistance + DistanceFalloffPerStep (+ Minimum)
///   - Schaden über mehrere Runden     -&gt; DamagePerTick + Duration
///   - Buff/Debuff mit Dauer           -&gt; StatModifiers + Duration
///   - Flächen-Schadensreduktion       -&gt; IncomingDamageMultiplier + Duration (Feld-Anker)
///   - Selbstbeschränkung (bewegungsunfähig) -&gt; PreventsMovement + Duration
///   - Feldsperre für Gegner           -&gt; LocksFieldForEnemies (Feld-Anker)
///   - Angriffsblockade/Umleitung      -&gt; RedirectsAttacksToSource (Feld-Anker)
///   - Anheftung an eine Einheit       -&gt; ergibt sich automatisch aus Unit-Anker (kein eigenes Feld nötig, siehe ActiveEffect)
///   - Aufladender Eigenzustand        -&gt; PreventsMovement + BonusDamageForNextAttack gemeinsam auf einem Unit-Effekt
///   - Passive, wertabhängige Wirkung  -&gt; NICHT hier, sondern IHasPassiveStatContribution (kein gespeicherter Effekt, da live berechnet)
///   - Wählbarer Modus                 -&gt; AbilityDefinition.Modes trägt je Modus eine eigene Effects-Liste
///   - Klon erzeugen                   -&gt; SpawnsClone
///
/// ERWEITERUNGEN für die sechs zusätzlichen Helden (nur wo eine Mechanik
/// bislang wirklich fehlte, siehe jeweilige Begründung):
///   - StatModifiersForEnemy: EIN Effekt kann je nach Team-Zugehörigkeit des
///     tatsächlichen Ziels UNTERSCHIEDLICH wirken (Verbündeter -&gt; Bonus,
///     Gegner -&gt; Abzug). Ohne dieses Feld bräuchte eine solche Fähigkeit
///     entweder zwei feste, getrennte Zielarten (Ally/Enemy) - was aber die
///     freie Zielwahl "ein beliebiges Ziel in Reichweite" (Hugger) verhindern
///     würde - oder eine Held-spezifische Sonderabfrage im Interpreter.
///     <see cref="AbilitySystem"/> löst bei Anwendung generisch anhand des
///     TATSÄCHLICHEN Ziel-Teams auf, welches der beiden Modifikator-Sets gilt.
///   - AppliesToCaster: manche Fähigkeiten wirken NEBEN dem gewählten Ziel
///     zusätzlich auf den Anwender selbst (z.B. Hugger: reduzierter
///     eingehender Schaden für sich selbst, solange die Anheftung aktiv ist).
///     Ohne dieses Feld würden ALLE Templates einer Fähigkeit zwangsläufig
///     auf dasselbe gewählte Ziel wirken.
///   - CancelEffectName: bislang konnte eine Fähigkeit nur Effekte HINZUFÜGEN,
///     nie aktiv einen eigenen, noch laufenden Effekt BEENDEN. Für den
///     Bogenschützen ("Aufladung abbrechen" muss ein wählbarer Zug sein)
///     fehlte diese Möglichkeit vollständig - generisch als benannte
///     Löschung (statt eines Bogenschütze-Sonderfalls) umgesetzt.
///   - MaximumAccumulatedBonusDamage: Obergrenze für <see cref="StackingRule.Accumulate"/>
///     (siehe dort) - ohne feste Obergrenze könnte sich ein aufladender
///     Zustand unbegrenzt aufschaukeln.
///   - CausesSourceToFollowTarget: die Anheftung des Hugger soll ihn nicht
///     nur wirkungsmäßig (Bonus/Abzug), sondern auch RÄUMLICH an sein Ziel
///     binden - zieht das Ziel um, zieht der Verursacher (Hugger) auf
///     dasselbe Feld mit, ohne dafür eine eigene Bewegung zu verbrauchen.
///     Bislang bewegte sich laut Klassendokumentation von
///     <see cref="ActiveEffect"/> immer nur der EFFEKT automatisch mit
///     seinem Ziel mit (weil er ohne eigenen Ortsbezug an der Zieleinheit
///     hängt) - eine Einheit, die sich dadurch selbst physisch mitbewegt,
///     ist ein echter Sonderfall, der eine neue, generische Fähigkeit
///     braucht (siehe <see cref="TacticsCore.GameState.MoveUnit"/>, wo sie
///     ausgewertet wird - unabhängig davon, WER die Bewegung eigentlich
///     ausgeführt hat).
/// </summary>
public sealed record EffectTemplate(
    string Name,
    bool IsInstant,
    int? Duration = null,
    StackingRule Stacking = StackingRule.RefreshDuration,
    int? InstantDamage = null,
    int? InstantHeal = null,
    bool ScaleWithDistance = false,
    int? DistanceFalloffPerStep = null,
    int? MinimumAfterDistanceScaling = null,
    int? DamagePerTick = null,
    StatModifierSet? StatModifiers = null,
    StatModifierSet? StatModifiersForEnemy = null,
    double? IncomingDamageMultiplier = null,
    bool PreventsMovement = false,
    bool LocksFieldForEnemies = false,
    bool RedirectsAttacksToSource = false,
    int? BonusDamageForNextAttack = null,
    int? MaximumAccumulatedBonusDamage = null,
    bool SpawnsClone = false,
    bool AppliesToCaster = false,
    string? CancelEffectName = null,
    bool CausesSourceToFollowTarget = false);
