using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.ERT;

[RegisterComponent, NetworkedComponent]
public sealed partial class ERTCryostorageComponent : Component
{
    [DataField]
    public bool AdminAccess = false;

    [DataField]
    public bool Blocked = false;

    [DataField(required: true)]
    public string ContainerId = string.Empty;

    [DataField]
    public List<ERTCryostorageOption> Options = new();

    [DataField]
    public LocId TitleName;

    [DataField]
    public bool UseOnce = true;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class ERTCryostorageOption
{
    [DataField]
    public LocId Name;

    [DataField]
    public SpriteSpecifier Sprite;

    [DataField(required: true)]
    public ProtoId<RandomHumanoidSettingsPrototype> Settings;

    [DataField]
    public bool RequiresAdminAllow = false;
}