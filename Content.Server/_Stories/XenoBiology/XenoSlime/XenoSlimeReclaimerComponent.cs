using Content.Shared.Chemistry.Components;
using Content.Shared.Storage;

namespace Content.Server._Stories.XenoBiology.XenoSlime;

[RegisterComponent]
public sealed partial class XenoSlimeReclaimerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active = false;
}
