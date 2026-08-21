using TacticsCore.Effects;
using TacticsCore.Hex;
using TacticsCore.Units;

namespace TacticsCore.Heroes;

/// <summary>
/// Eine Einheit mit individuellen Werten aus einer <see cref="HeroDefinition"/>
/// statt der einheitlichen Standardfigur-Werte. Nutzt die dafür
/// vorgesehene geschützte Konstruktor-Überladung von <see cref="Unit"/>.
///
/// Implementiert <see cref="IHasPassiveStatContribution"/>, damit
/// <see cref="EffectiveStats"/> passive, wertabhängige Fähigkeiten (z.B.
/// den Berserker) generisch berücksichtigen kann, OHNE dass der Namespace
/// TacticsCore.Effects seinerseits von TacticsCore.Heroes abhängen müsste
/// (Abhängigkeitsrichtung bleibt Heroes -&gt; Effects).
/// </summary>
public sealed class Hero : Unit, IHasPassiveStatContribution
{
    public HeroDefinition Definition { get; }

    /// <summary>Reines Auswertungs-Kennzeichen, siehe <see cref="HeroRole"/> - beeinflusst kein System.</summary>
    public HeroRole Role => Definition.Role;

    /// <summary>
    /// Verbleibende Runden, bis die aktive Fähigkeit wieder einsetzbar ist;
    /// null/0, wenn keine Abklingzeit läuft. Wird zu Beginn jedes eigenen
    /// Zugs verringert (siehe <see cref="TickAbilityCooldown"/>), zum
    /// selben Zeitpunkt wie Effekt-Ticks (siehe EffectSystem).
    /// </summary>
    public int? AbilityCooldownRemaining { get; private set; }

    public Hero(Guid id, Team team, HexCoordinate startPosition, HeroDefinition definition)
        : base(id, team, startPosition, definition.MovementRange, definition.MaxHitPoints, definition.BaseAttackDamage, definition.Speed, definition.AttackRange)
    {
        Definition = definition;
    }

    /// <summary>Ausschließlich für <see cref="Abilities.AbilitySystem"/> gedacht.</summary>
    internal void SetAbilityCooldown(int rounds) => AbilityCooldownRemaining = rounds;

    /// <summary>Ausschließlich für <see cref="Match.MatchController"/> gedacht (Zeitpunkt siehe dort und EffectSystem).</summary>
    internal void TickAbilityCooldown()
    {
        if (AbilityCooldownRemaining is int remaining)
        {
            AbilityCooldownRemaining = remaining > 1 ? remaining - 1 : null;
        }
    }

    /// <summary>
    /// Live berechneter Stat-Beitrag der passiven Fähigkeit dieses Helden
    /// (falls vorhanden) - siehe <see cref="IHasPassiveStatContribution"/>.
    /// </summary>
    public StatModifierSet GetPassiveStatContribution(GameState state) =>
        Definition.Ability is { IsPassive: true, PassiveStatModifier: { } compute }
            ? compute(this, state)
            : StatModifierSet.None;
}
