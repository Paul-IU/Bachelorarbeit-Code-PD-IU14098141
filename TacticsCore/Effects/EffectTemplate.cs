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
///   - distanzabhängige Wirkung        -&gt; ScaleWithDistance + DistanceFalloffPerStep
///     (+ optional PlateauRadius, + Minimum); gilt GENERISCH sowohl für
///     Sofortschaden/-heilung als auch für dauerhafte StatModifiers-Deltas
///     (siehe Musiker-Lied) - ausgewertet in
///     <see cref="Abilities.AbilitySystem.ApplyToUnit"/> bzw. dessen
///     <c>ScaleByDistance</c>/<c>ScaleNonZeroByDistance</c>-Hilfsmethoden.
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
///     ausgeführt hat). Zusätzlich gilt (siehe <see cref="ActiveEffect.CausesSourceToFollowTarget"/>
///     und <see cref="Effects.EffectSystem.Apply"/>): wird ein solcher
///     Effekt neu angelegt, wird die Quelle SOFORT auf das aktuelle Feld
///     des Ziels gezogen, nicht erst bei dessen NÄCHSTER Bewegung (siehe
///     Hugger-Korrektur, Bachelorarbeit-Anforderung "sofort wechseln").
///   - PreventsAttacking: generische Angriffssperre für die tragende
///     Einheit selbst (Hugger: "kann selbst nicht angreifen, solange die
///     Anheftung aktiv ist"). Bewusst getrennt von
///     <see cref="PreventsMovement"/>, da beide Einschränkungen unabhängig
///     voneinander vorkommen können (z.B. Bogenschütze: bewegungsunfähig
///     beim Laden, aber weiterhin angriffsfähig).
///   - PreventsBeingTargetedByEnemies: generische Unangreifbarkeit GEGENÜBER
///     GEGNERN (Hugger: "kein gültiges Angriffs- oder Fähigkeitsziel für
///     Gegner", solange die Anheftung aktiv ist - Verbündete dürfen ihn
///     weiterhin z.B. heilen). Ersetzt bei Hugger die vorherige pauschale
///     Schadensreduktion: eine unangreifbare Einheit kann gar nicht erst
///     zum Ziel werden, eine Reduktion des (dann nie eintretenden)
///     Schadens wäre redundant. Team-relativ ausgewertet (siehe
///     <see cref="Effects.EffectiveStats.PreventsBeingTargetedBy"/>), nicht
///     als absolute Sperre - sonst könnten auch Verbündete die Einheit
///     nicht mehr als Ziel wählen.
///   - PlateauRadius (Bachelorarbeit-Korrektur, Musiker-Lied): Anzahl
///     Distanzstufen AB DER ERST DER lineare Abfall einsetzt - Distanz 0 bis
///     einschließlich <c>PlateauRadius</c> bleibt UNVERÄNDERT auf der vollen
///     Stärke, erst darüber hinaus greift <see cref="DistanceFalloffPerStep"/>
///     (angewendet auf <c>Distanz - PlateauRadius</c> statt auf die rohe
///     Distanz, siehe <see cref="Abilities.AbilitySystem.ApplyToUnit"/>
///     bzw. dessen private <c>ScaleByDistance</c>-Methode). Standardwert 0
///     entspricht exakt dem bisherigen Verhalten (Abfall setzt sofort ab
///     Distanz 1 ein, wie bei der Elfe-Heilung) - kein bestehendes Template
///     ändert sein Verhalten durch die Einführung dieses Feldes.
///     GRUND FÜR DIE ERWEITERUNG: Ursprünglich sollte das Musiker-Lied auf
///     Distanz 0 UND 1 GLEICH STARK wirken und erst auf Distanz 2 abfallen -
///     ein "Plateau" über zwei Distanzstufen. Eine rein lineare Formel (jeder
///     Schritt senkt um denselben Betrag) kann das strukturell NICHT
///     abbilden, unabhängig von der gewählten Schrittgröße - das war zunächst
///     als bewusste, dokumentierte Näherung hingenommen worden. Mit diesem
///     zusätzlichen, aber optionalen Parameter lässt sich das Plateau jetzt
///     EXAKT abbilden (<c>PlateauRadius: 1</c> bei Musiker), ohne einen
///     komplett neuen, zweiten Distanzmechanismus einführen zu müssen und
///     ohne die bestehende, bei der Elfe bewährte Formel für alle anderen
///     Fähigkeiten zu verändern.
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
    int PlateauRadius = 0,
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
    bool CausesSourceToFollowTarget = false,
    bool PreventsAttacking = false,
    bool PreventsBeingTargetedByEnemies = false);
