using System;
using System.Collections.Generic;
using System.Linq;
using TacticsCore;
using TacticsCore.Effects;
using TacticsCore.Heroes;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCoreGodot.Shared;

/// <summary>
/// Ein aktiver Effekt, aufbereitet für die Anzeige - reine Werte, keine
/// Objektreferenz auf das TacticsCore-interne <c>ActiveEffect</c>, damit
/// diese Momentaufnahme unabhängig vom sich weiter verändernden Spiel-
/// zustand ist (siehe <see cref="BoardRenderModel"/>).
/// </summary>
public sealed record EffectInfo(string Name, int? RemainingDuration, int? BonusDamage, bool PreventsMovement, StatModifierSet? StatModifiers)
{
    /// <summary>
    /// Kurzform für die Anzeige, z.B. "Aufladen:2(+10)" oder
    /// "Umklammerung:-[ATK+4]". Zeigt Stat-Modifikatoren (Buff/Debuff)
    /// EXPLIZIT mit Betrag/Vorzeichen an, statt nur den Effektnamen ohne
    /// erkennbare Wirkung - Bachelorarbeit-Korrektur (siehe Hugger:
    /// "gewährter Buff/Debuff muss ... in der Oberfläche angezeigt werden").
    /// </summary>
    public string ShortLabel
    {
        get
        {
            string duration = RemainingDuration is int d ? d.ToString() : "-";
            string bonus = BonusDamage is int b ? $"(+{b})" : "";
            string stats = StatModifiers is { } m ? FormatStatModifiers(m) : "";
            return $"{Name}:{duration}{bonus}{stats}";
        }
    }

    private static string FormatStatModifiers(StatModifierSet m)
    {
        var parts = new List<string>();
        if (m.BaseAttackDamageDelta != 0)
        {
            parts.Add($"ATK{Signed(m.BaseAttackDamageDelta)}");
        }

        if (m.MovementRangeDelta != 0)
        {
            parts.Add($"BEW{Signed(m.MovementRangeDelta)}");
        }

        if (m.AttackRangeDelta != 0)
        {
            parts.Add($"RW{Signed(m.AttackRangeDelta)}");
        }

        if (m.SpeedDelta != 0)
        {
            parts.Add($"SPD{Signed(m.SpeedDelta)}");
        }

        return parts.Count == 0 ? "" : $"[{string.Join(" ", parts)}]";
    }

    private static string Signed(int value) => value >= 0 ? $"+{value}" : value.ToString();
}

/// <summary>Eine Einheit, aufbereitet für die Anzeige (siehe <see cref="EffectInfo"/> für die Begründung).</summary>
public sealed record UnitInfo(
    Guid Id,
    Team Team,
    HexCoordinate Position,
    int CurrentHp,
    int MaxHp,
    string DisplayName,
    bool IsClone,
    IReadOnlyList<EffectInfo> Effects,
    string? HeroId,
    int? AbilityCooldownRemaining,
    Guid? AttachedToUnitId)
{
    public bool IsImmobilized => Effects.Any(e => e.PreventsMovement);

    /// <summary>
    /// Ob diese Einheit gerade an eine andere angeheftet ist (siehe
    /// <see cref="AttachedToUnitId"/>) - rein informativ für die Anzeige,
    /// GENERISCH aus <see cref="ActiveEffect.CausesSourceToFollowTarget"/>
    /// abgeleitet (siehe <see cref="BoardRenderModel.FromGameState"/>), nicht
    /// an einen bestimmten Helden gebunden. Aktuell nutzt nur Hugger diese
    /// Mechanik, jede künftige Fähigkeit mit derselben generischen Wirkung
    /// wird von der Oberfläche automatisch gleich behandelt.
    /// </summary>
    public bool IsAttached => AttachedToUnitId is not null;
}

public sealed record LockedFieldInfo(HexCoordinate Field, Team LockedAgainst);

/// <summary>
/// Vollständige, UNVERÄNDERLICHE Momentaufnahme dessen, was auf dem
/// Spielfeld dargestellt werden muss - die einzige Schnittstelle zwischen
/// "was sagt die Spiellogik gerade" und "wie zeichnet Godot das". Sowohl
/// der Live-Spielmodus (nach jedem Ereignis frisch aus dem aktuellen
/// <see cref="GameState"/> gebaut) als auch der Replay-Modus (einmalig im
/// Voraus für JEDEN Ereignis-Schritt gebaut, siehe
/// <see cref="TacticsCoreGodot.Replay.ReplayTimeline"/>) nutzen GENAU
/// dieselbe Aufbereitung - die Darstellung selbst (siehe
/// <see cref="TacticsCoreGodot.UI.HexBoardView"/>) kennt daher keinen
/// Unterschied zwischen "live" und "abgespielt".
/// </summary>
public sealed record BoardRenderModel(
    IReadOnlyList<UnitInfo> Units,
    IReadOnlyList<LockedFieldInfo> LockedFields,
    int RoundNumber,
    Guid? ActingUnitId,
    bool ActingCanMove,
    bool ActingCanAct)
{
    /// <summary>
    /// Liest den übergebenen Spielzustand vollständig über seine öffentliche
    /// API aus (Anforderung: "Godot liest den Spielzustand ... es kennt die
    /// Regeln nicht") - keine Regel wird hier ausgewertet, nur bereits von
    /// TacticsCore berechnete Werte abgegriffen.
    /// </summary>
    public static BoardRenderModel FromGameState(GameState state)
    {
        // Generische Ableitung "Einheit X ist an Einheit Y angeheftet" -
        // ausschließlich aus der bereits vorhandenen, allgemeinen
        // ActiveEffect.CausesSourceToFollowTarget-Markierung (siehe Hugger),
        // OHNE eine bestimmte Fähigkeit oder Heldenkennung abzufragen. Jede
        // künftige Fähigkeit mit derselben Wirkung wird dadurch automatisch
        // gleich dargestellt (Bachelorarbeit-Vorgabe: "generisch behandeln").
        var attachedToByUnitId = state.ActiveEffects
            .Where(e => e.CausesSourceToFollowTarget)
            .ToDictionary(e => e.SourceUnitId, e => e.TargetUnitId);

        var units = state.Units
            .Select(u => new UnitInfo(
                u.Id,
                u.Team,
                u.Position,
                u.CurrentHitPoints,
                u.MaxHitPoints,
                // Klone tragen einen eigenen, vom Erzeuger abgeleiteten Namen
                // (siehe Unit.DisplayNameOverride) statt der anonymen
                // "Standardfigur" - Bachelorarbeit-Korrektur (Illusionist-Klon).
                u.DisplayNameOverride ?? (u is Hero hero ? hero.Definition.DisplayName : "Standardfigur"),
                u.IsClone,
                state.GetEffectsOnUnit(u.Id)
                    .Select(e => new EffectInfo(e.Name, e.RemainingDuration, e.BonusDamageForNextAttack, e.PreventsMovement, e.StatModifiers))
                    .ToList(),
                // Heldenkennung aus dem Katalog (siehe HeroDefinition.Id) -
                // für die Zuordnung einer Heldengrafik in HexBoardView
                // (VisualAssetCatalog). Null bei Nicht-Helden (Standardfigur
                // in Tests) - dort greift immer die Form-/Farb-Darstellung.
                u is Hero heroForArt ? heroForArt.Definition.Id : null,
                // Verbleibende Fähigkeits-Abklingzeit (Punkt E) - Teil des
                // Spielzustands (Hero.AbilityCooldownRemaining), hier nur
                // ausgelesen, nicht berechnet. Null bei Nicht-Helden.
                u is Hero heroForCooldown ? heroForCooldown.AbilityCooldownRemaining : null,
                attachedToByUnitId.TryGetValue(u.Id, out var attachedTo) ? attachedTo : null))
            // Dieselbe stabile Reihenfolge wie überall in TacticsCore (q, r, Id) -
            // damit z.B. die Stapel-Reihenfolge mehrerer Einheiten auf einem
            // Feld zwischen zwei Momentaufnahmen nicht ohne Grund wechselt.
            .OrderBy(u => u.Position.Q).ThenBy(u => u.Position.R).ThenBy(u => u.Id)
            .ToList();

        var lockedFields = state.GetLockedFields()
            .Select(f => new LockedFieldInfo(f.Field, f.LockedAgainst))
            .ToList();

        return new BoardRenderModel(
            units,
            lockedFields,
            state.CurrentRoundNumber,
            state.CurrentActingUnitId,
            state.CurrentTurnBudget.CanMove,
            state.CurrentTurnBudget.CanAct);
    }
}
