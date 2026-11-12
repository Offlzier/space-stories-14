// ReSharper disable CheckNamespace

using Content.Server.Shuttles.Components;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class EmergencyShuttleSystem
{
    [Dependency] private IPlayerManager _storiesPlayerManager = default!;
    [Dependency] private IPrototypeManager _storiesPrototypeManager = default!;

    private bool TryStoriesGetCentcommMapPath(StationCentcommComponent component, out ResPath path)
    {
        path = component.Map;

        if (component.GameMap is not { } gameMapId)
            return true;

        if (!_storiesPrototypeManager.TryIndex(gameMapId, out var gameMap))
        {
            Log.Error($"Failed to find centcomm game map prototype {gameMapId}");
            return false;
        }

        path = gameMap.MapPath;
        return true;
    }

    private void StoriesInitCentcommStation(StationCentcommComponent component, EntityUid grid)
    {
        if (component.GameMap is not { } gameMapId
            || !_storiesPrototypeManager.TryIndex(gameMapId, out var gameMap)
            || _storiesPlayerManager.PlayerCount < gameMap.MinPlayers)
            return;

        foreach (var config in gameMap.Stations.Values)
        {
            component.StationEntity = _station.InitializeNewStation(config, new[] { grid });
            return;
        }
    }
}
