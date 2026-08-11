using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.XenoBiology.XenoSlime;

[Prototype]
public sealed partial class TypeXenoSlimePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = null!;

    [DataField(required: true)]
    public string Name { get; private set; } = null!;

    [DataField]
    public ComponentRegistry Components = new();
}
