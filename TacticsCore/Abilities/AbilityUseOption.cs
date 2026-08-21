namespace TacticsCore.Abilities;

/// <summary>
/// Eine für einen Agenten wählbare Fähigkeiten-Option: ein konkretes Ziel,
/// bei Fähigkeiten mit Moduswahl zusätzlich ein konkreter Modus (siehe
/// <see cref="AbilityDefinition.Modes"/>). Wird von
/// <see cref="AbilitySystem.GetLegalAbilityUses"/> stabil sortiert
/// zurückgegeben.
/// </summary>
public sealed record AbilityUseOption(AbilityTarget Target, AbilityMode? Mode);
