using Godot;
using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCoreGodot.Shared;
using TacticsCoreGodot.UI;

namespace TacticsCoreGodot.Animation;

/// <summary>
/// Spielt die von der Spiellogik bereits fertig entschiedenen Ereignisse
/// ZEITLICH GESTRECKT ab, statt sie sofort auf den Endzustand springen zu
/// lassen (Vorgabe Teil B). WICHTIG: diese Klasse verändert oder verzögert
/// die Spiellogik selbst NICHT im Geringsten - <see cref="TacticsCore.Match.MatchController"/>
/// hat den Ausgang jedes Ereignisses bereits vollständig und synchron
/// berechnet, BEVOR es hier ankommt (siehe GameSession/ReplayTimeline).
/// Diese Klasse bekommt nur eine FERTIGE Ereignis-Reihenfolge samt der
/// jeweils fertigen <see cref="BoardRenderModel"/>-Momentaufnahme davor/
/// danach und entscheidet ausschließlich, WIE LANGE und WOMIT sie das
/// bereits feststehende Geschehen auf dem Spielfeld sichtbar macht.
///
/// WARTESCHLANGE (Vorgabe: "Ereignisse werden in eine Warteschlange gelegt
/// und nacheinander abgespielt"): <see cref="Enqueue"/> reiht ein Ereignis
/// ein; läuft die Warteschlange noch nicht, startet sie. Während
/// <see cref="IsBusy"/> wahr ist, sperren MatchScreen/ReplayScreen jede
/// Eingabe (siehe <see cref="HexBoardView.InteractionEnabled"/> und
/// ActionPanel) - erst wenn die komplette Warteschlange abgearbeitet ist,
/// wird <see cref="BusyChanged"/> mit <c>false</c> ausgelöst.
///
/// GESCHWINDIGKEIT/ÜBERSPRINGEN: <see cref="SpeedMultiplier"/> skaliert
/// JEDE Animationsdauer (2.0 = doppelt so schnell), <see cref="SkipAnimations"/>
/// überspringt sie vollständig (Zustand springt sofort, wie vor dieser
/// Erweiterung) - beides jederzeit umschaltbar, auch mitten in einer
/// laufenden Partie/einem laufenden Replay.
///
/// BATCH-SIMULATION UNBERÜHRT: diese Klasse existiert ausschließlich im
/// Godot-Projekt (TacticsCoreGodot) - das Konsolenprogramm
/// (TacticsCore.Demo) und die Batch-Simulation (TacticsCore.Batch) binden
/// weder dieses Skript noch Godot selbst ein und sind dadurch strukturell
/// unmöglich betroffen, nicht nur "zufällig unverändert".
/// </summary>
public partial class EventAnimationPlayer : Node
{
    private readonly Queue<AnimationJob> _queue = new();
    private bool _isBusy;

    /// <summary>Das Spielfeld, auf dem animiert wird - von MatchScreen/ReplayScreen nach dem Hinzufügen gesetzt.</summary>
    public HexBoardView Board { get; set; } = null!;

    private readonly AnimationAssetCatalog _assets = AnimationAssetCatalog.LoadCatalog();

    /// <summary>Ob gerade eine Animation läuft bzw. noch Einträge in der Warteschlange stehen - solange wahr, ist keine Eingabe möglich (Vorgabe).</summary>
    public bool IsBusy => _isBusy;

    /// <summary>Wird ausgelöst, sobald sich IsBusy ändert - MatchScreen/ReplayScreen sperren/entsperren Eingaben darüber.</summary>
    public event Action<bool>? BusyChanged;

    /// <summary>Geschwindigkeits-Multiplikator (1 = normal, 2 = doppelt so schnell, ...). Einstellbar (Vorgabe).</summary>
    public float SpeedMultiplier { get; set; } = 1f;

    /// <summary>Vollständiges Überspringen aller Animationen (Vorgabe: "Option zum vollständigen Überspringen") - Zustand springt dann sofort auf das Ergebnis, wie vor dieser Erweiterung.</summary>
    public bool SkipAnimations { get; set; }

    private readonly record struct AnimationJob(ReplayEntry Entry, BoardRenderModel? Before, BoardRenderModel After);

    /// <summary>Reiht ein bereits feststehendes Ereignis zur zeitlich gestreckten Darstellung ein.</summary>
    public void Enqueue(ReplayEntry entry, BoardRenderModel? before, BoardRenderModel after)
    {
        _queue.Enqueue(new AnimationJob(entry, before, after));
        if (!_isBusy)
        {
            SetBusy(true);
            _ = DrainQueueAsync();
        }
    }

    /// <summary>
    /// Verwirft die Warteschlange und springt sofort auf <paramref name="model"/>
    /// - für Replay-Rücksprünge/Sprünge (Vorgabe: "Beim schrittweisen
    /// Zurückgehen darf ohne Animation direkt auf den Zustand gesprungen
    /// werden"). Räumt auch alle noch laufenden visuellen Überlagerungen
    /// (Positions-/Alpha-Overrides, Geschosse, Textfähnchen) auf, damit
    /// keine "eingefrorene" Animation vom vorherigen Zustand übrig bleibt.
    /// </summary>
    public void ShowInstantly(BoardRenderModel model)
    {
        _queue.Clear();
        Board.ClearAllAnimationOverlays();
        Board.SetRenderModel(model);
        SetBusy(false);
    }

    /// <summary>Verwirft die Warteschlange und alle visuellen Überlagerungen, OHNE einen neuen Zustand zu setzen - für den Start einer neuen Partie/eines neuen Replays (der Aufrufer setzt den Anfangszustand direkt danach selbst).</summary>
    public void Reset()
    {
        _queue.Clear();
        Board.ClearAllAnimationOverlays();
        SetBusy(false);
    }

    private void SetBusy(bool busy)
    {
        if (_isBusy == busy)
        {
            return;
        }

        _isBusy = busy;
        Board.InteractionEnabled = !busy;
        BusyChanged?.Invoke(busy);
    }

    private async Task DrainQueueAsync()
    {
        while (_queue.Count > 0)
        {
            var job = _queue.Dequeue();
            await PlayJobAsync(job);
        }

        SetBusy(false);
    }

    private float Duration(float baseSeconds) => Mathf.Max(0.02f, baseSeconds / Mathf.Max(0.05f, SpeedMultiplier));

    private static UnitInfo? FindUnit(BoardRenderModel? model, Guid id) => model?.Units.FirstOrDefault(u => u.Id == id);

    private async Task PlayJobAsync(AnimationJob job)
    {
        if (SkipAnimations)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        switch (job.Entry.ActionType)
        {
            case ReplayActionType.Move:
                await PlayMoveAsync(job);
                break;

            case ReplayActionType.Attack:
                await PlayAttackAsync(job);
                break;

            case ReplayActionType.UseAbility:
                await PlayAbilityAsync(job);
                break;

            case ReplayActionType.EffectTicked:
                // Nur im Replay verlässlich verfügbar (siehe Klassendoku von
                // ReplayTimeline/GameSession: EffectSystem protokolliert
                // Tick-Ereignisse nur über die vollständige Aufzeichnung,
                // nicht live per onEvent) - im Live-Spiel wird dieser Fall
                // praktisch nie erreicht, die HP-Änderung erscheint dort
                // stattdessen unkommentiert beim nächsten sichtbaren
                // Ereignis. Bewusst als bekannte, dokumentierte Lücke
                // hingenommen statt TacticsCore dafür anzufassen (Vorgabe:
                // "Spiellogik bleibt vollständig unverändert").
                await PlayEffectTickAsync(job);
                break;

            case ReplayActionType.UnitDefeated:
                await PlayDefeatAsync(job);
                break;

            case ReplayActionType.CloneSpawned:
                await PlayCloneSpawnedAsync(job);
                break;

            default:
                // RoundStarted/RoundOrderDetermined/TurnStarted/TurnEnded/
                // EffectApplied/EffectExpired/CloneDespawned/MatchEnded: rein
                // informativ, keine eigene Bild-Animation nötig -
                // CloneDespawned ist zudem immer von einem UnitDefeated-
                // Ereignis derselben Einheit begleitet (siehe
                // MatchController.LogUnitChanges), das bereits das
                // Ausblenden übernimmt (Vorgabe Punkt 8 "verschwunden").
                // Zustand wird direkt übernommen.
                Board.SetRenderModel(job.After);
                break;
        }
    }

    // -----------------------------------------------------------------
    // 1. BEWEGUNG
    // -----------------------------------------------------------------
    private async Task PlayMoveAsync(AnimationJob job)
    {
        var entry = job.Entry;
        var path = entry.MovePath?.Fields;
        if (path is null || path.Count < 2)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        var unitId = entry.ActingUnitId;
        var tween = GetTree().CreateTween();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = BoardGeometry.HexToPixel(path[i], 1f);
            var to = BoardGeometry.HexToPixel(path[i + 1], 1f);
            tween.TweenMethod(Callable.From<Vector2>(p => Board.SetUnitPositionOverride(unitId, p)), from, to, Duration(0.16f));
        }

        await ToSignal(tween, Tween.SignalName.Finished);
        Board.ClearUnitPositionOverride(unitId);
        Board.SetRenderModel(job.After);
    }

    // -----------------------------------------------------------------
    // 2. NAHKAMPFANGRIFF (Distanz <= 1) / an PlayAbilityAsync weiterverwiesener Fernkampf-Grundangriff
    // -----------------------------------------------------------------
    private async Task PlayAttackAsync(AnimationJob job)
    {
        var entry = job.Entry;
        var before = job.Before ?? job.After;
        var attacker = FindUnit(before, entry.ActingUnitId);
        var actualTargetId = entry.AttackTargetUnitId; // ggf. durch Sichtlinien-Umleitung NICHT das ursprünglich gewählte Ziel - hier bereits der tatsächliche Empfänger (siehe CombatSystem.TryAttackDetailed)
        var target = actualTargetId is Guid tid ? FindUnit(before, tid) : null;

        if (attacker is null || target is null)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        int distance = HexCoordinate.Distance(attacker.Position, target.Position);
        if (distance <= 1)
        {
            await PlayMeleeBumpAsync(attacker.Id, attacker.Position, target.Position);
        }
        else
        {
            var texture = _assets.GetRangedAttackProjectileTexture();
            await PlayProjectileAsync(attacker.Position, target.Position, texture, _assets.DamageColor);
        }

        Board.SetRenderModel(job.After);
        ShowFloatingText(target.Position, $"-{entry.DamageDealt ?? 0}", _assets.DamageColor);
    }

    private async Task PlayMeleeBumpAsync(Guid attackerId, HexCoordinate from, HexCoordinate to)
    {
        var fromPos = BoardGeometry.HexToPixel(from, 1f);
        var towardPos = fromPos.Lerp(BoardGeometry.HexToPixel(to, 1f), 0.4f); // kurze Bewegung IN RICHTUNG Ziel, nicht bis zum Ziel selbst

        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<Vector2>(p => Board.SetUnitPositionOverride(attackerId, p)), fromPos, towardPos, Duration(0.11f));
        tween.TweenMethod(Callable.From<Vector2>(p => Board.SetUnitPositionOverride(attackerId, p)), towardPos, fromPos, Duration(0.11f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.ClearUnitPositionOverride(attackerId);
    }

    private async Task PlayProjectileAsync(HexCoordinate from, HexCoordinate to, Texture2D? texture, Color fallbackColor)
    {
        var projectile = Board.AddProjectile(from, to, texture, fallbackColor);
        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(p =>
        {
            projectile.Progress = p;
            Board.RequestRedraw();
        }), 0f, 1f, Duration(0.35f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.RemoveProjectile(projectile);
    }

    // -----------------------------------------------------------------
    // 3./4./5. FÄHIGKEITEN: Geschoss zum Ziel (bei Entfernung > 0), sonst
    // lokale Wirkung am eigenen Feld; die Art der Landewirkung (Schaden/
    // Heilung/Buff/Debuff/generisch) wird generisch aus der Zustands-
    // änderung des betroffenen Ziels abgeleitet, ohne dass diese Klasse den
    // konkreten Helden oder die konkrete Fähigkeit kennen müsste.
    // -----------------------------------------------------------------
    private async Task PlayAbilityAsync(AnimationJob job)
    {
        var entry = job.Entry;
        var before = job.Before ?? job.After;
        var caster = FindUnit(before, entry.ActingUnitId);
        if (caster is null)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        if (entry.AttackTargetUnitId is Guid targetId)
        {
            await PlayUnitTargetedAbilityAsync(job, caster, targetId);
        }
        else if (entry.EffectField is HexCoordinate field)
        {
            await PlayFieldTargetedAbilityAsync(job, caster, field);
        }
        else
        {
            // Reines Selbst-Ziel ohne separates Feld (z.B. Trugbild,
            // Aufladen - AbilityTargetType.Self liefert eine UnitTarget auf
            // den Anwender selbst, s.o. eigentlich bereits über
            // AttackTargetUnitId abgedeckt; dieser Zweig ist ein
            // zusätzliches Sicherheitsnetz).
            Board.SetRenderModel(job.After);
            PlayGenericPulse(caster.Position);
        }
    }

    private async Task PlayUnitTargetedAbilityAsync(AnimationJob job, UnitInfo caster, Guid targetId)
    {
        var before = job.Before ?? job.After;
        var targetBefore = FindUnit(before, targetId);
        if (targetBefore is null)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        string abilityId = job.Entry.AbilityId ?? "";
        int distance = HexCoordinate.Distance(caster.Position, targetBefore.Position);

        if (_assets.UsesCasterMovement(abilityId))
        {
            // Der Anwender begibt sich sichtbar selbst zum Ziel, statt ein
            // Geschoss zu verschicken (Vorgabe: Hugger "heftet sich an,
            // wirkt nicht aus der Ferne") - generisch über die zentrale
            // Konfiguration entschieden, kein Hugger-Sonderfall im Code.
            await PlayCasterMoveToTargetAsync(caster.Id, caster.Position, targetBefore.Position);
        }
        else if (distance > 0 && caster.Id != targetId)
        {
            var texture = _assets.GetAbilityProjectileTexture(abilityId);
            await PlayProjectileAsync(caster.Position, targetBefore.Position, texture, _assets.BuffColor);
        }

        var targetAfter = FindUnit(job.After, targetId);
        Board.SetRenderModel(job.After);
        PlayImpactLanding(targetBefore, targetAfter, abilityId);
    }

    /// <summary>Lässt den Anwender sichtbar direkt zum Zielfeld wechseln (siehe AnimationAssetCatalog.AbilitiesUsingCasterMovement) - dieselbe Positions-Override-Technik wie bei PlayMoveAsync/PlayMeleeBumpAsync, nur bis ans Ziel statt nur eine kurze Strecke.</summary>
    private async Task PlayCasterMoveToTargetAsync(Guid casterId, HexCoordinate from, HexCoordinate to)
    {
        if (from == to)
        {
            return; // Ziel steht bereits auf demselben Feld - keine sichtbare Bewegung nötig
        }

        var fromPos = BoardGeometry.HexToPixel(from, 1f);
        var toPos = BoardGeometry.HexToPixel(to, 1f);
        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<Vector2>(p => Board.SetUnitPositionOverride(casterId, p)), fromPos, toPos, Duration(0.22f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.ClearUnitPositionOverride(casterId);
    }

    private async Task PlayFieldTargetedAbilityAsync(AnimationJob job, UnitInfo caster, HexCoordinate field)
    {
        string abilityId = job.Entry.AbilityId ?? "";
        var before = job.Before ?? job.After;

        // Alle aktuell vorhandenen Feld-Fähigkeiten (Schildwall, Mauer,
        // Lied) zentrieren sich auf das eigene Feld (Reichweite 0) - ein
        // Geschossflug ergäbe hier keinen Sinn (Länge 0). Für ein Feld
        // außerhalb der eigenen Position (aktuell von keinem Helden
        // genutzt, aber strukturell möglich) fliegt trotzdem ein Geschoss
        // dorthin, damit die Animation auch für künftige Helden generisch
        // korrekt bleibt.
        if (field != caster.Position)
        {
            var texture = _assets.GetAbilityProjectileTexture(abilityId);
            await PlayProjectileAsync(caster.Position, field, texture, _assets.ShieldColor);
        }

        Board.SetRenderModel(job.After);

        // Betroffene Einheiten GENERISCH über einen Vorher/Nachher-Vergleich
        // ermitteln (HP oder Effektliste hat sich verändert) - NICHT über
        // "wer steht auf dem eigenen Feld", da eine Fähigkeit wie Musikers
        // Lied auch Verbündete auf ANDEREN Feldern im Umkreis erfasst
        // (Radius 2). Dupliziert damit keine Reichweiten-/Filterlogik aus
        // AbilitySystem - beobachtet nur, wer sich tatsächlich verändert hat.
        var affected = FindUnitsChangedBetween(before, job.After, caster.Id);

        bool grantsStatBuff = affected.Any(u => u.Effects.Any(e => e.StatModifiers is not null));
        var flashColor = grantsStatBuff ? _assets.BuffColor : _assets.ShieldColor;
        await PlayFieldFlashAsync(field, flashColor, _assets.GetSpecificAbilityProjectileTexture(abilityId));

        // Aufsteigendes Symbol über JEDER betroffenen Einheit (Vorgabe:
        // Musiker-Lied - "über jedem betroffenen der Noten-Effekt"; generisch
        // für jede Fähigkeit mit konfiguriertem Symbol, siehe
        // AnimationAssetCatalog.AbilityFloatingSymbols).
        var symbol = _assets.GetFloatingSymbol(abilityId);
        if (symbol is not null)
        {
            foreach (var unit in affected)
            {
                ShowFloatingText(unit.Position, symbol, flashColor);
            }
        }
    }

    /// <summary>Einheiten, deren HP oder Effektliste sich zwischen zwei Momentaufnahmen verändert hat - generische Grundlage, um "wer wurde von diesem Ereignis betroffen" abzuleiten, ohne die Reichweiten-/Zielregeln der auslösenden Fähigkeit zu kennen oder zu duplizieren.</summary>
    private static List<UnitInfo> FindUnitsChangedBetween(BoardRenderModel before, BoardRenderModel after, Guid excludeUnitId)
    {
        var beforeById = before.Units.ToDictionary(u => u.Id);
        var changed = new List<UnitInfo>();

        foreach (var afterUnit in after.Units)
        {
            if (afterUnit.Id == excludeUnitId)
            {
                continue;
            }

            if (!beforeById.TryGetValue(afterUnit.Id, out var beforeUnit))
            {
                continue; // erst durch dieses Ereignis erschienen (z.B. Klon) - wird von anderen Animationen (CloneSpawned) behandelt
            }

            if (beforeUnit.CurrentHp != afterUnit.CurrentHp || !beforeUnit.Effects.SequenceEqual(afterUnit.Effects))
            {
                changed.Add(afterUnit);
            }
        }

        return changed;
    }

    private async Task PlayFieldFlashAsync(HexCoordinate field, Color color, Texture2D? texture = null)
    {
        var flash = Board.SetFieldFlash(field, color, texture);
        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(v =>
        {
            flash.Intensity = v;
            Board.RequestRedraw();
        }), 0.7f, 0f, Duration(0.5f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.ClearFieldFlash(field);
    }

    private void PlayGenericPulse(HexCoordinate field)
    {
        _ = PlayFieldFlashAsync(field, _assets.BuffColor); // bewusst nicht abgewartet - reine Zierde, blockiert die Warteschlange nicht unnötig
    }

    /// <summary>Leitet aus der Zustandsänderung des Ziels generisch ab, welche Landewirkung zu zeigen ist (Vorgabe Punkt 4: Heilung/Buff, sowie Schaden für schädigende Fähigkeiten).</summary>
    private void PlayImpactLanding(UnitInfo before, UnitInfo? after, string abilityId)
    {
        int hpBefore = before.CurrentHp;
        int hpAfter = after?.CurrentHp ?? 0;

        if (after is null)
        {
            return; // besiegt - die eigentliche Ausblend-Animation übernimmt der separate UnitDefeated-Job
        }

        if (hpAfter < hpBefore)
        {
            ShowFloatingText(before.Position, $"-{hpBefore - hpAfter}", _assets.DamageColor);
            return;
        }

        if (hpAfter > hpBefore)
        {
            ShowFloatingText(before.Position, $"+{hpAfter - hpBefore}", _assets.HealColor);
            // Fähigkeitsspezifische Grafik (z.B. Heilung-Kugel) auch hier
            // bevorzugt zeigen - bei einer Selbstheilung (Distanz 0) ist
            // dies die EINZIGE Stelle, an der sie überhaupt sichtbar wird,
            // da davor kein Geschoss fliegt (siehe PlayUnitTargetedAbilityAsync).
            _ = PlayFieldFlashAsync(before.Position, _assets.HealColor, _assets.GetSpecificAbilityProjectileTexture(abilityId));
            return;
        }

        var newOrRefreshedEffect = after.Effects.FirstOrDefault(e => e.StatModifiers is not null);
        if (newOrRefreshedEffect?.StatModifiers is { } mods)
        {
            int net = mods.BaseAttackDamageDelta + mods.MovementRangeDelta + mods.AttackRangeDelta + mods.SpeedDelta;
            bool isBuff = net >= 0;
            _ = PlayFieldFlashAsync(before.Position, isBuff ? _assets.BuffColor : _assets.DebuffColor, _assets.GetSpecificAbilityProjectileTexture(abilityId));
            return;
        }

        // Weder HP- noch Stat-Änderung erkennbar (z.B. reine Anheftung ohne
        // eigenen sichtbaren Ausschlag) - ein schlichter, neutraler Puls
        // macht dennoch sichtbar, dass "etwas" passiert ist.
        _ = PlayFieldFlashAsync(before.Position, _assets.BuffColor, _assets.GetSpecificAbilityProjectileTexture(abilityId));
    }

    // -----------------------------------------------------------------
    // 6. EFFEKT-TICK (z.B. Verbrennen) - siehe Hinweis zur Live-Einschränkung oben.
    // -----------------------------------------------------------------
    private async Task PlayEffectTickAsync(AnimationJob job)
    {
        var entry = job.Entry;
        var before = job.Before;
        var targetId = entry.RelatedUnitId;
        var targetBefore = targetId is Guid id ? FindUnit(before, id) : null;

        Board.SetRenderModel(job.After);

        if (targetBefore is null)
        {
            return;
        }

        var targetAfter = FindUnit(job.After, targetBefore.Id);
        int damage = targetBefore.CurrentHp - (targetAfter?.CurrentHp ?? 0);
        if (damage > 0)
        {
            ShowFloatingText(targetBefore.Position, $"-{damage}", _assets.EffectTickColor);
            await PlayFieldFlashAsync(targetBefore.Position, _assets.EffectTickColor);
        }
    }

    // -----------------------------------------------------------------
    // 7. EINHEIT BESIEGT: Ausblenden statt sofortigem Verschwinden.
    // -----------------------------------------------------------------
    private async Task PlayDefeatAsync(AnimationJob job)
    {
        var entry = job.Entry;
        var before = job.Before;
        var ghost = FindUnit(before, entry.ActingUnitId);

        if (ghost is null)
        {
            Board.SetRenderModel(job.After);
            return;
        }

        Board.SetFadingGhost(ghost, 1f);
        Board.SetRenderModel(job.After); // Einheit ist im "echten" Modell bereits entfernt - der Geist übernimmt die Anzeige

        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(a =>
        {
            Board.SetUnitAlphaOverride(ghost.Id, a);
            Board.RequestRedraw();
        }), 1f, 0f, Duration(0.6f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.RemoveFadingGhost(ghost.Id);
    }

    // -----------------------------------------------------------------
    // 8. KLON ERZEUGT: kurzes Einblenden (Verschwinden siehe Hinweis bei
    // CloneDespawned oben - läuft bereits über PlayDefeatAsync).
    // -----------------------------------------------------------------
    private async Task PlayCloneSpawnedAsync(AnimationJob job)
    {
        var cloneId = job.Entry.ActingUnitId; // ForCloneSpawned setzt den Klon selbst als ActingUnitId
        Board.SetUnitAlphaOverride(cloneId, 0f);
        Board.SetRenderModel(job.After);

        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(a =>
        {
            Board.SetUnitAlphaOverride(cloneId, a);
            Board.RequestRedraw();
        }), 0f, 1f, Duration(0.4f));
        await ToSignal(tween, Tween.SignalName.Finished);
        Board.ClearUnitAlphaOverride(cloneId);
    }

    // -----------------------------------------------------------------
    // Aufsteigender Text (Schadens-/Heilzahl, Effekt-Tick, oder ein Symbol
    // wie die Musiker-Note) - fliegt kurz nach oben und blendet aus. Bewusst
    // NICHT abgewartet (fire-and-forget): reine Zierde, soll die
    // Warteschlange nicht künstlich verlangsamen, während sie im
    // Hintergrund ausklingt.
    // -----------------------------------------------------------------
    private void ShowFloatingText(HexCoordinate field, string text, Color color)
    {
        var label = Board.AddFloatingLabel(field, text, color);
        float riseDistance = Board.CurrentHexSize * 0.7f;
        var tween = GetTree().CreateTween();
        tween.TweenMethod(Callable.From<float>(t =>
        {
            label.YOffset = -riseDistance * t;
            label.Alpha = 1f - t;
            Board.RequestRedraw();
        }), 0f, 1f, Duration(0.9f));
        tween.Finished += () => Board.RemoveFloatingLabel(label);
    }
}
