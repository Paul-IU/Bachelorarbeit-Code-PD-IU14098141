using TacticsCore.Effects;
using TacticsCore.Units;

namespace TacticsCore.Abilities;

/// <summary>
/// Datengetriebene Definition einer Heldenfähigkeit (Anforderung 7). Ein
/// Held (siehe <see cref="Heroes.HeroDefinition"/>) trägt genau eine
/// solche Definition - ob sie aktiv (kostet die Aktion) oder passiv
/// (wirkt dauerhaft, siehe <see cref="PassiveStatModifier"/>) ist, steht in
/// <see cref="IsPassive"/>.
/// </summary>
/// <param name="Id">Stabiler, technischer Schlüssel (z.B. für das Replay-Protokoll).</param>
/// <param name="DisplayName">Menschenlesbarer Name für ASCII-Ausgabe/Replay.</param>
/// <param name="IsPassive">
/// Passive Fähigkeiten werden nie als Zug angeboten (kein Eintrag in
/// <see cref="AbilitySystem.GetLegalAbilityUses"/>) - ihre Wirkung ergibt
/// sich stattdessen live aus <see cref="PassiveStatModifier"/>.
/// </param>
/// <param name="TargetType">Welche Ziele die Fähigkeit annehmen kann.</param>
/// <param name="Range">Maximale Hex-Distanz vom Anwender zum Ziel.</param>
/// <param name="Effects">
/// Anzuwendende Wirkungen, sofern <see cref="Modes"/> null ist (keine
/// Moduswahl). Bei mehreren Wirkungen werden sie in Listenreihenfolge
/// angewendet (deterministisch).
/// </param>
/// <param name="CooldownRounds">Optionale Abklingzeit in Runden nach Einsatz; null = keine.</param>
/// <param name="Modes">
/// Wenn gesetzt, muss beim Einsatz zusätzlich einer dieser Modi gewählt
/// werden (Anforderung 8) - <see cref="Effects"/> wird dann ignoriert,
/// stattdessen gilt die Effektliste des gewählten Modus.
/// </param>
/// <param name="Radius">Nur bei <see cref="AbilityTargetType.AllUnitsInRadius"/> relevant: zusätzlicher Radius um das gewählte Zentrumsfeld.</param>
/// <param name="PassiveStatModifier">Nur bei <see cref="IsPassive"/>: live berechneter Stat-Beitrag (siehe <see cref="IHasPassiveStatContribution"/>).</param>
/// <param name="AreaFilter">
/// Nur bei einer flächenwirkenden Zielart (<see cref="AbilityTargetType.AllUnitsOnField"/>,
/// <see cref="AbilityTargetType.AllUnitsInRadius"/>) relevant: schränkt ein,
/// welche der betroffenen Einheiten die Wirkung tatsächlich trifft (siehe
/// <see cref="AreaTargetFilter"/>). Standard <see cref="AreaTargetFilter.Any"/>
/// entspricht dem bisherigen Verhalten (z.B. Schildträger).
/// </param>
public sealed record AbilityDefinition(
    string Id,
    string DisplayName,
    bool IsPassive,
    AbilityTargetType TargetType,
    int Range,
    IReadOnlyList<EffectTemplate> Effects,
    int? CooldownRounds = null,
    IReadOnlyList<AbilityMode>? Modes = null,
    int? Radius = null,
    Func<Unit, GameState, StatModifierSet>? PassiveStatModifier = null,
    AreaTargetFilter AreaFilter = AreaTargetFilter.Any);
