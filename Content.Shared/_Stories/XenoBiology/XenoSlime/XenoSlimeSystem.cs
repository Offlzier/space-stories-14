using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Chat;

namespace Content.Shared._Stories.XenoBiology.XenoSlime;

public sealed partial class XenobiologySystem : EntitySystem
{
    [Dependency] private HungerSystem _hunger = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoSlimeComponent, MeleeHitEvent>(OnAttacked);
    }

    private void OnAttacked(EntityUid uid, XenoSlimeComponent comp, MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!TryComp<HungerComponent>(target, out var targetHunger))
                continue;

            var steal = Math.Min(comp.NutritionSteal, _hunger.GetHunger(targetHunger));
            _hunger.ModifyHunger(target, -steal, targetHunger);

            if (TryComp<HungerComponent>(uid, out var userHunger))
                _hunger.ModifyHunger(uid, steal, userHunger);
        }
    }
}
