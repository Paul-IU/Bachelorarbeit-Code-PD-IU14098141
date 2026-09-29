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
///   - Angriffsblockade/Umleitung      -&gt; RedirectsAttacksToSource (Feld-Anker; nur Angriffe von GEGNERN des Anwenders, siehe CombatSystem.ResolveRedirect)
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
///   - DamagePerTickReductionPerTick (Bachelorarbeit-Korrektur,
///     Elementarmagier-Feuer): der Schaden über Zeit soll mit jedem Tick um
///     einen festen Betrag ABKLINGEN (3, dann 2, dann 1 bei Duration 3)
///     statt konstant zu bleiben - <see cref="Effects.EffectSystem.TickDamageOverTime"/>
///     zieht diesen Betrag GENERISCH nach jedem Tick vom aktuellen
///     <see cref="DamagePerTick"/> DIESER Effekt-Instanz ab (nach unten
///     begrenzt bei 0, nie negativer Schaden). Standardwert <c>null</c>
///     bedeutet "kein Abklingen" und entspricht exakt dem bisherigen,
///     konstanten Verhalten - kein bestehendes Template ändert sein
///     Verhalten durch die Einführung dieses Feldes. WICHTIG gegen
///     Stapeln durch Mehrfacheinsatz: ein erneuter Einsatz auf ein bereits
///     brennendes Ziel erzeugt bei <see cref="StackingRule.RefreshDuration"/>
///     (wie hier) ohnehin KEINE zweite, parallele Instanz (siehe
///     <see cref="Effects.EffectSystem.Apply"/>) - zusätzlich setzt das
///     Auffrischen auch den bereits abgeklungenen Schaden wieder auf den
///     vollen Ausgangswert zurück, ein wiederholtes Anzünden liefert also
///     bei jedem Neueinsatz wieder dieselbe 3-2-1-Abklingfolge, statt sich
///     über mehrere Einsätze hinweg aufzusummieren.
///   - FieldFollowsSource (Bachelorarbeit-Designentscheidung, Mauer der Wand):
///     ein FELD-verankerter Effekt, dessen Feld dem Anwender folgt - zieht die
///     Quelle um, wandert der Effekt samt aller Wirkungen (Feldsperre,
///     Umleitung, Schadensreduktion) auf ihr neues Feld (siehe
///     <see cref="TacticsCore.GameState.MoveUnit"/>, die einzige Stelle, an der
///     sich Positionen ändern). Technisch bleibt es ein Feldeffekt, damit alle
///     Systeme, die "Effekte auf diesem Feld" lesen (Bewegungssperre,
///     Umleitung, Schadensreduktion, Anzeige), unverändert bleiben können.
///     Weil ein solcher Effekt zu genau EINEM Anwender gehört, ist der Anwender
///     Teil seiner Stapel-Identität (siehe EffectSystem.FindMatching): zwei
///     Anwender auf demselben Feld erzeugen zwei getrennte Instanzen.
///     Standardwert false = bisheriges, ortsfestes Verhalten.
///   - TargetingProtectionSuspendedByAttackRounds (Designentscheidung
///     "Hugger-Überarbeitung"): nur zusammen mit PreventsBeingTargetedByEnemies
///     sinnvoll. Greift der Träger dieses Effekts an, wird seine Unangreifbarkeit
///     für so viele Runden aufgehoben: das Kampfsystem legt dafür einen
///     Markereffekt "Exponiert" (SuspendsTargetingProtection) mit dieser Dauer
///     auf ihn. Die Dauer zählt - wie jede Effektdauer - zu Beginn der eigenen
///     Züge des Trägers herunter; Dauer 1 heißt also: angreifbar vom Angriff bis
///     zum Beginn seines NÄCHSTEN eigenen Zugs, genau eine Runde (anders als eine
///     ABKLINGZEIT, deren Wert 1 wirkungslos wäre, siehe DefaultHeroes.WandMauerAbklingzeit).
///     Der Schutzeffekt selbst bleibt bestehen. null = ein Angriff hebt nichts auf.
///   - SuspendsTargetingProtection: hebt, solange aktiv, jede
///     PreventsBeingTargetedByEnemies-Wirkung auf der Einheit auf (siehe
///     EffectiveStats.PreventsBeingTargetedBy).
///   - ReflectsFractionOfRedirectedDamage (Bachelorarbeit-Korrektur,
///     Wand-Rework): NUR gemeinsam mit <see cref="RedirectsAttacksToSource"/>
///     auf einem FELD-verankerten Effekt sinnvoll - ein Bruchteil (z.B. 0.5
///     = 50%) des URSPRÜNGLICHEN, noch unreduzierten Schadens wird an den
///     Angreifer zurückgegeben, wenn sein Angriff auf dieses Feld umgeleitet
///     wird (nur Angriffe von Gegnern werden umgeleitet; ein direkter Angriff
///     auf den Anwender löst keine Vergeltung aus) (siehe <see cref="Combat.CombatSystem.ApplyDamageAndHandleDefeat(GameState, Units.Unit, Units.Unit, int)"/>).
///     GRUND FÜR DIE ERWEITERUNG: die Wand bezahlte ihre Umleitung bislang
///     mit eigener Bewegungsunfähigkeit, ohne dafür irgendeine eigene
///     Gegenwirkung zu erhalten - ein reiner Kostenpunkt ohne Gegenwert.
///     Dieses Feld gibt der Umleitung selbst eine offensive Komponente,
///     unabhängig von der (jetzt entfallenen) Bewegungssperre. Standardwert
///     <c>null</c> bedeutet "keine Vergeltung" und entspricht exakt dem
///     bisherigen Verhalten jeder Umleitung - kein bestehendes Template
///     ändert sein Verhalten durch die Einführung dieses Feldes.
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
    int? DamagePerTickReductionPerTick = null,
    StatModifierSet? StatModifiers = null,
    StatModifierSet? StatModifiersForEnemy = null,
    double? IncomingDamageMultiplier = null,
    bool PreventsMovement = false,
    bool LocksFieldForEnemies = false,
    bool RedirectsAttacksToSource = false,
    double? ReflectsFractionOfRedirectedDamage = null,
    int? BonusDamageForNextAttack = null,
    int? MaximumAccumulatedBonusDamage = null,
    bool SpawnsClone = false,
    bool AppliesToCaster = false,
    string? CancelEffectName = null,
    bool CausesSourceToFollowTarget = false,
    bool PreventsAttacking = false,
    bool PreventsBeingTargetedByEnemies = false,
    bool FieldFollowsSource = false,
    int? TargetingProtectionSuspendedByAttackRounds = null,
    bool SuspendsTargetingProtection = false);
