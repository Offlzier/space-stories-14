using Content.Shared._Stories.ShowIconsSystem;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.ShowIcons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedShowIconsSystem))]
public sealed partial class ShowIconsComponent : Component
{
    [DataField]
    public HashSet<string> Actions = new();

    [DataField]
    public HashSet<EntityUid> GrantedActions = new();

    [DataField]
    public ComponentRegistry Components = new();

    [DataField, AutoNetworkedField]
    public bool Enabled { get; set; } = false;

    [DataField]
    public EntityUid? Target;

    [DataField]
    public ComponentRegistry? RemoveComponents;

    [DataField]
    public bool Parent;
}
