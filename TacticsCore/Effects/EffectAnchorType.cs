namespace TacticsCore.Effects;

/// <summary>
/// Woran ein aktiver Effekt "hängt": eine einzelne Einheit (folgt ihr,
/// endet bei ihrer Niederlage) oder ein festes Feld (unabhängig davon,
/// wer gerade darauf steht - z.B. eine Feldsperre oder eine
/// Flächen-Schadensreduktion).
/// </summary>
public enum EffectAnchorType
{
    Unit,
    Field,
}
