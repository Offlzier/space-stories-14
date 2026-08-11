using Robust.Shared.Prototypes;
using Robust.Client.GameObjects;
using Content.Shared._Stories.XenoBiology.XenoSlime;
using Content.Client.DamageState;

namespace Content.Client._Stories.XenoBiology.XenoSlime;

public sealed partial class XenoSlimeVisualizerSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IComponentFactory _componentFactory = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoSlimeComponent, AfterAutoHandleStateEvent>(OnStateHandled);
    }

    private void OnStateHandled(EntityUid uid, XenoSlimeComponent component, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_prototypeManager.Resolve(component.TypeSlime, out var typeXenoSlimeProto))
            return;

        if (!typeXenoSlimeProto.Components.TryGetValue(_componentFactory.GetComponentName<SpriteComponent>(), out var entry)
            || entry.Component is not SpriteComponent protoSprite)
            return;

        foreach (var layer in protoSprite.AllLayers)
        {
            if (layer.RsiState.Name is { } stateName)
                _sprite.LayerSetRsiState((uid, sprite), DamageStateVisualLayers.Base, stateName);
        }

        if (TryComp<DamageStateVisualsComponent>(uid, out var damageVisuals)
            && typeXenoSlimeProto.Components.TryGetValue(_componentFactory.GetComponentName<DamageStateVisualsComponent>(), out var dsvEntry)
            && dsvEntry.Component is DamageStateVisualsComponent protoDamageVisuals)
        {
            damageVisuals.States = protoDamageVisuals.States;
        }
    }
}
