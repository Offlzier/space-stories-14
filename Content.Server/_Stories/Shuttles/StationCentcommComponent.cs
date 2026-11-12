// ReSharper disable CheckNamespace

using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Server.Shuttles.Components;

public sealed partial class StationCentcommComponent
{
    [DataField]
    public ProtoId<GameMapPrototype>? GameMap;

    [DataField]
    public EntityUid? StationEntity;
}
