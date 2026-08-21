using TacticsCore.Hex;
using TacticsCore.Match;
using TacticsCore.Units;

namespace TacticsCore.Persistence;

/// <summary>
/// Für EINE Einheit der Ausgangsaufstellung: welche Heldendefinition (per
/// Katalog-Id) sie nutzt - oder null für eine einfache Standardfigur.
///
/// WARUM DAS NÖTIG IST: <see cref="ReplayLog"/> speichert bewusst nur
/// (UnitId, Team, Startfeld) - genug, um die Partie selbst Zug für Zug
/// nachzuvollziehen, aber NICHT, welche Heldendefinition (HP, Schaden,
/// Fähigkeit, ...) eine Einheit hatte. Das reicht für die reine
/// Simulation, aber nicht, um eine aufgezeichnete Partie später VISUELL
/// (mit korrekten Namen, HP-Balken, Fähigkeitsbeschreibungen) darzustellen -
/// dafür muss zusätzlich bekannt sein, WELCHER Held sich hinter welcher
/// UnitId verbirgt.
/// </summary>
public sealed record SavedUnitInfo(Guid UnitId, Team Team, HexCoordinate StartPosition, string? HeroId);

/// <summary>
/// Eine vollständig gespeicherte Partie: Ausgangsaufstellung MIT
/// Helden-Zuordnung plus das eigentliche Replay-Protokoll. Rein additiver,
/// von der Simulation selbst unabhängiger Baustein (ändert nichts an
/// bestehendem Verhalten) - der Zweck ist ausschließlich, eine einzelne,
/// aus einem Batch-Lauf als auffällig identifizierte Partie später extern
/// (z.B. in einer Godot-Oberfläche) laden und visuell nachvollziehen zu
/// können, ohne die Simulation dafür erneut mit denselben Zufallsagenten
/// laufen lassen zu müssen (was NICHT dieselbe Partie reproduzieren würde,
/// sobald ein Mensch oder ein anderer Agent beteiligt ist).
/// </summary>
public sealed record SavedMatch(IReadOnlyList<SavedUnitInfo> Units, ReplayLog Replay);
