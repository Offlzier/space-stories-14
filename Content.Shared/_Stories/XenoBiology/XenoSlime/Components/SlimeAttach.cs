using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.XenoBiology.XenoSlime;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class AttachableComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? AttachedTo; // К кому прикреплен моб

    [DataField, AutoNetworkedField]
    public bool IsAttached = false; // Прикреплен ли моб
}
