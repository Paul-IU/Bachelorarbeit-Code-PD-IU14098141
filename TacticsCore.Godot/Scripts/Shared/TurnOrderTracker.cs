using TacticsCore.Match;

namespace TacticsCoreGodot.Shared;

/// <summary>Anzeige-Zustand eines Eintrags in der Reihenfolge-Leiste (siehe <see cref="TurnOrderTracker"/>).</summary>
public enum TurnOrderStatus
{
    /// <summary>Noch nicht an der Reihe gewesen.</summary>
    Pending,

    /// <summary>Aktuell am Zug.</summary>
    Acting,

    /// <summary>Zug bereits beendet.</summary>
    Acted,

    /// <summary>Während dieser Runde besiegt - bleibt an ihrer Position stehen, wird aber als ausgefallen markiert statt entfernt.</summary>
    Defeated,
}

/// <summary>
/// Ein einzelner Eintrag der Reihenfolge-Anzeige: die Einheit (bzw. ihr
/// letzter bekannter Stand, falls besiegt) plus ihr Anzeige-Zustand.
/// </summary>
public readonly record struct TurnOrderSlot(UnitInfo Unit, TurnOrderStatus Status);

/// <summary>
/// Verfolgt die Anzeige der "Reihenfolge dieser Runde" im Statuspanel. Die
/// Reihenfolge selbst wird AUSSCHLIESSLICH aus dem von der Kernbibliothek
/// gemeldeten <see cref="ReplayActionType.RoundOrderDetermined"/>-Ereignis
/// übernommen (siehe <see cref="TurnOrder.ComputeRoundOrder"/>, dort ein
/// einziges Mal je Runde berechnet) - diese Klasse berechnet die Reihenfolge
/// an KEINER Stelle selbst.
///
/// Behebt damit einen früheren Anzeigefehler: Die alte Fassung ergänzte noch
/// nicht an der Reihe gewesene Einheiten aus der jeweils AKTUELLEN
/// Modell-Reihenfolge (<c>model.Units</c>) - änderte sich der Spielzustand
/// während der Runde (Einheit besiegt, Speed-Buff, Klon erscheint/
/// verschwindet), fiel diese Ergänzung bei jeder Aktualisierung anders aus,
/// obwohl die tatsächlich gespielte Reihenfolge unverändert blieb. Jetzt wird
/// die zu Rundenbeginn gemeldete Liste einmal übernommen und bis zum
/// nächsten RoundOrderDetermined-Ereignis nicht mehr verändert - lediglich
/// der ANZEIGE-ZUSTAND (siehe <see cref="TurnOrderStatus"/>) einzelner
/// Einträge aktualisiert sich weiter mit dem Spielgeschehen.
///
/// Bezieht die Reihenfolge weiterhin ausschließlich aus dem ohnehin bereits
/// threadsicher an MatchScreen/ReplayScreen vorbeikommenden Ereignisstrom
/// (<see cref="ReplayEntry"/>) - ein direkter Zugriff auf den lebenden
/// GameState von der Oberfläche aus (der beim Live-Spiel nebenläufig vom
/// Hintergrund-Thread verändert wird) bleibt unnötig und vermieden.
///
/// ZUSTANDSÄNDERUNGEN INNERHALB DER RUNDE (ohne Umsortierung):
///   - Eine besiegte Einheit bleibt an ihrer Position in der Reihenfolge
///     stehen (Status <see cref="TurnOrderStatus.Defeated"/>) - ihr letzter
///     bekannter Stand wird aus dem Modell UNMITTELBAR VOR dem
///     UnitDefeated-Ereignis festgehalten, da sie im Modell DANACH bereits
///     nicht mehr enthalten ist (derselbe Grund, aus dem auch
///     EventAnimationPlayer.PlayDefeatAsync für seinen Ausblend-"Geist" auf
///     das "Vorher"-Modell zurückgreift).
///   - Ein während der Runde neu erzeugter Klon wird über das
///     CloneSpawned-Ereignis UNMITTELBAR HINTER seinem Erzeuger eingefügt -
///     das spiegelt exakt wider, wie MatchController.Run() ihn tatsächlich
///     in die laufende Runde einsortiert (siehe TurnOrder.cs: ein Klon
///     "handelt in der Zugreihenfolge unmittelbar nach seinem Erzeuger",
///     NICHT erst in der nächsten Runde, auch wenn er erst während der
///     laufenden Runde entsteht).
/// </summary>
public sealed class TurnOrderTracker
{
    private List<Guid> _fixedOrder = new();
    private readonly HashSet<Guid> _actedThisRound = new();
    private readonly Dictionary<Guid, UnitInfo> _defeatedThisRoundSnapshots = new();

    /// <summary>
    /// Von MatchScreen/ReplayScreen für JEDEN Ereigniseintrag aufzurufen, in
    /// der Reihenfolge, in der er aufgetreten ist. <paramref name="before"/>
    /// ist der Modellstand UNMITTELBAR VOR diesem Eintrag (null nur ganz zu
    /// Beginn, vor dem allerersten Ereignis) - wird ausschließlich für den
    /// Besiegt-Schnappschuss gebraucht.
    /// </summary>
    public void Register(ReplayEntry entry, BoardRenderModel? before, BoardRenderModel after)
    {
        switch (entry.ActionType)
        {
            case ReplayActionType.RoundOrderDetermined:
                // Neue, von der Kernbibliothek für DIESE Runde bereits
                // feststehende Reihenfolge übernehmen - ab jetzt bis zum
                // nächsten derartigen Ereignis unverändert.
                _fixedOrder = entry.RoundOrder is { } order ? new List<Guid>(order.UnitIds) : new List<Guid>();
                _actedThisRound.Clear();
                _defeatedThisRoundSnapshots.Clear();
                break;

            case ReplayActionType.TurnEnded:
                _actedThisRound.Add(entry.ActingUnitId);
                break;

            case ReplayActionType.CloneSpawned when entry.RelatedUnitId is Guid creatorId:
                InsertCloneAfterCreator(creatorId, entry.ActingUnitId);
                break;

            case ReplayActionType.UnitDefeated:
                var snapshot = before?.Units.FirstOrDefault(u => u.Id == entry.ActingUnitId);
                if (snapshot is not null)
                {
                    _defeatedThisRoundSnapshots[entry.ActingUnitId] = snapshot;
                }

                break;
        }
    }

    private void InsertCloneAfterCreator(Guid creatorId, Guid cloneId)
    {
        int creatorIndex = _fixedOrder.IndexOf(creatorId);
        if (creatorIndex < 0)
        {
            // Sollte laut Kernlogik nicht vorkommen (der Erzeuger muss zu
            // diesem Zeitpunkt in der Reihenfolge stehen, da er gerade
            // seinen Zug hatte) - defensiv ans Ende anhängen statt eine
            // Ausnahme zu riskieren.
            _fixedOrder.Add(cloneId);
            return;
        }

        _fixedOrder.Insert(creatorIndex + 1, cloneId);
    }

    /// <summary>
    /// Baut die Anzeige für die zu Rundenbeginn festgelegte, seither
    /// unveränderte Reihenfolge: lebende Einheiten mit ihrem aktuellen Stand
    /// aus <paramref name="model"/> (Bewegung/Buffs/HP-Änderungen bleiben so
    /// sichtbar, ohne die Reihenfolge zu beeinflussen), besiegte mit ihrem
    /// festgehaltenen Schnappschuss.
    /// </summary>
    public IReadOnlyList<TurnOrderSlot> BuildDisplayOrder(BoardRenderModel model)
    {
        var byId = model.Units.ToDictionary(u => u.Id);
        var result = new List<TurnOrderSlot>(_fixedOrder.Count);

        foreach (var id in _fixedOrder)
        {
            UnitInfo? unit = byId.TryGetValue(id, out var live) ? live
                : _defeatedThisRoundSnapshots.TryGetValue(id, out var dead) ? dead
                : null;

            if (unit is null)
            {
                continue; // sollte nicht vorkommen - defensiv statt eines Absturzes
            }

            var status =
                _defeatedThisRoundSnapshots.ContainsKey(id) ? TurnOrderStatus.Defeated :
                _actedThisRound.Contains(id) ? TurnOrderStatus.Acted :
                model.ActingUnitId == id ? TurnOrderStatus.Acting :
                TurnOrderStatus.Pending;

            result.Add(new TurnOrderSlot(unit, status));
        }

        return result;
    }
}
