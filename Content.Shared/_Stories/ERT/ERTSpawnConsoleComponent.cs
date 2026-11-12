using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Stories.ERT;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class ERTSpawnConsoleComponent : Component
{
    [DataField]
    public bool AdminAccess = false;

    [DataField]
    public int ActiveOptionIndex = -1;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? CooldownEnd;

    [DataField]
    public int Balance;

    [DataField]
    public LocId BalanceName;

    [DataField]
    public float Delay;

    [DataField]
    public List<ERTSpawnOption> Options = new();

    [DataField]
    public LocId TitleName;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ERTSpawnOption
{
    [DataField]
    public LocId Name;

    [DataField]
    public LocId Desc;

    [DataField]
    public EntProtoId Rule;

    [DataField]
    public int Price;

    [DataField]
    public bool RequiresAdminAllow = false;
}