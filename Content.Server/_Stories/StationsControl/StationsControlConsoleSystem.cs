using Content.Server.RoundEnd;
using Content.Shared._Stories.StationsControl;
using Content.Shared.AlertLevel;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Stories.StationsControl;

public sealed partial class StoriesStationsControlConsoleSystem : EntitySystem
{
    [Dependency] private AlertLevelSystem _alertLevel = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndChanged);
        SubscribeLocalEvent<StationsControlConsoleComponent, BoundUIOpenedEvent>(OnUiOpened);

        Subs.BuiEvents<StationsControlConsoleComponent>(StationsControlConsoleUiKey.Key, subs =>
        {
            subs.Event<StationsControlConsoleSetAlertLevelMessage>(OnSetAlertLevel);
            subs.Event<StationsControlConsoleCallEvacMessage>(OnCallEvac);
            subs.Event<StationsControlConsoleRecallEvacMessage>(OnRecallEvac);
        });
    }

    private void OnUiOpened(Entity<StationsControlConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnRoundEndChanged(RoundEndSystemChangedEvent args)
    {
        var query = EntityQueryEnumerator<StationsControlConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateUi((uid, comp));
        }
    }

    private void OnSetAlertLevel(Entity<StationsControlConsoleComponent> ent, ref StationsControlConsoleSetAlertLevelMessage args)
    {
        if (!TryGetEntity(args.Station, out var station)
            || !HasComp<StationDataComponent>(station)
            || !TryComp<AlertLevelComponent>(station, out var alert))
            return;

        if (!ent.Comp.AllowAnyAlertLevel && !ent.Comp.AllowedAlertLevels.Contains(args.AlertLevel))
            return;

        foreach (var level in alert.AvailableAlertLevels)
        {
            if (level != args.AlertLevel)
                continue;

            _alertLevel.SetLevel((station.Value, alert), level, true, true, true);
            return;
        }
    }

    private void OnCallEvac(Entity<StationsControlConsoleComponent> ent, ref StationsControlConsoleCallEvacMessage args)
    {
        if (args.CountdownTime < ent.Comp.MinEvacTime || args.CountdownTime > ent.Comp.MaxEvacTime)
            return;

        _roundEnd.RequestRoundEnd(args.CountdownTime, requester: args.Actor, machine: ent, checkCooldown: false);
    }

    private void OnRecallEvac(Entity<StationsControlConsoleComponent> ent, ref StationsControlConsoleRecallEvacMessage args)
    {
        _roundEnd.CancelRoundEndCountdown(requester: args.Actor, machine: ent, forceRecall: true);
    }

    private void UpdateUi(Entity<StationsControlConsoleComponent> ent)
    {
        var stationsList = new List<string>();
        var stationsNetIds = new List<NetEntity>();

        foreach (var station in _station.GetStations())
        {
            stationsList.Add(Name(station));
            stationsNetIds.Add(GetNetEntity(station));
        }

        var state = new StationsControlConsoleState(
            stationsList,
            stationsNetIds,
            ent.Comp.AllowedAlertLevels,
            ent.Comp.AllowAnyAlertLevel,
            _roundEnd.ShuttleTimeLeft);

        _ui.SetUiState(ent.Owner, StationsControlConsoleUiKey.Key, state);
    }
}
