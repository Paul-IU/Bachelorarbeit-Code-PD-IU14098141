using System;
using System.Collections.Generic;
using System.Linq;
using TacticsCore;
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
public sealed record EffectInfo(string Name, int? RemainingDuration, int? BonusDamage, bool PreventsMovement)
{
    /// <summary>Kurzform für die Anzeige, z.B. "Aufladen:2(+10)" oder "Umklammerung:-".</summary>
    public string ShortLabel
    {
        get
        {
            string duration = RemainingDuration is int d ? d.ToString() : "-";
            string bonus = BonusDamage is int b ? $"(+{b})" : "";
            return $"{Name}:{duration}{bonus}";
        }
    }
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
    IReadOnlyList<EffectInfo> Effects)
{
    public bool IsImmobilized => Effects.Any(e => e.PreventsMovement);
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
        var units = state.Units
            .Select(u => new UnitInfo(
                u.Id,
                u.Team,
                u.Position,
                u.CurrentHitPoints,
                u.MaxHitPoints,
                u is Hero hero ? hero.Definition.DisplayName : "Standardfigur",
                u.IsClone,
                state.GetEffectsOnUnit(u.Id)
                    .Select(e => new EffectInfo(e.Name, e.RemainingDuration, e.BonusDamageForNextAttack, e.PreventsMovement))
                    .ToList()))
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
