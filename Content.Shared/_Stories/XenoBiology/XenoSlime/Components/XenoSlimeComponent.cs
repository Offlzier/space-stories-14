using Content.Shared._Stories.XenoBiology.XenoSlime;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.XenoBiology.XenoSlime;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class XenoSlimeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float NutritionSteal = 20f;

    [DataField, AutoNetworkedField]
    public int FissionThreshold = 80;

    [DataField, AutoNetworkedField]
    public float MutationChance = 0.40f;

    [DataField, AutoNetworkedField]
    public int ProcessingTime = 5;

    [DataField(required: true), AutoNetworkedField]
    public ProtoId<TypeXenoSlimePrototype> TypeSlime = "TypeGreySlime";

    [DataField(required: true), AutoNetworkedField]
    public HashSet<ProtoId<TypeXenoSlimePrototype>> TypeSlimeMutation = new();

    [DataField]
    public EntProtoId Extract = "STGreyExtractXenoSlime";

    [DataField]
    public EntProtoId BaseXenoSlimeProto = "STMobBabyXenoSlimesGrey";

    [DataField]
    public TimeSpan? DivisionTime;
}
