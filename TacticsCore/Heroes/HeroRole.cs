namespace TacticsCore.Heroes;

/// <summary>
/// Reines Kennzeichen für die spätere statistische Auswertung (Anforderung
/// 4 aus Teil A: "Die Rolle ... darf das Verhalten nicht beeinflussen").
/// Kein System in TacticsCore verzweigt anhand dieses Werts - er dient
/// ausschließlich der Beschriftung/Gruppierung von Ergebnissen.
/// </summary>
public enum HeroRole
{
    Tank,
    Support,
    Dmg,
}
