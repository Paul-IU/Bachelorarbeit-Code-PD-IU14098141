using TacticsCore.Effects;
using TacticsCore.Units;

namespace TacticsCore.Abilities;

/// <summary>
/// Ein wählbarer Modus einer Fähigkeit (Anforderung 8: "Fähigkeiten mit
/// wählbarem Modus, z.B. zwei Elemente - die Wahl ist Teil des Zuges").
/// Trägt eine EIGENE Wirkungsliste statt die der Fähigkeit zu teilen, damit
/// unterschiedliche Modi wirklich unterschiedliche Effekte haben können.
/// </summary>
/// <param name="IsLegal">
/// Optionales, generisches Legalitäts-Prädikat: manche Modi sind nur unter
/// einer Zustandsbedingung wählbar (z.B. "Aufladung abbrechen" beim
/// Bogenschützen nur, solange tatsächlich aufgeladen wird). Null (Standard)
/// bedeutet "immer legal, sobald die Fähigkeit selbst legal ist" - das
/// bisherige Verhalten für alle Modi ohne Bedingung (Musiker,
/// Elementarmagier) bleibt dadurch unverändert. Nimmt bewusst <see cref="Unit"/>
/// statt <see cref="Heroes.Hero"/> entgegen, um keine (an sich unschädliche,
/// aber unnötige) Abhängigkeit dieser Datei auf den Heroes-Namespace
/// einzuführen - <see cref="AbilitySystem"/> übergibt den aufrufenden Helden
/// hier einfach als Unit.
/// </param>
public sealed record AbilityMode(string Id, string DisplayName, IReadOnlyList<EffectTemplate> Effects, Func<Unit, GameState, bool>? IsLegal = null);
