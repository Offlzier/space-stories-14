using Content.Shared.Nutrition.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Jittering;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Holiday;
using Robust.Shared.Configuration;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Chat;

namespace Content.Shared._Stories.XenoBiology.XenoSlime;

public sealed partial class XenoSlimeReproductionSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private HungerSystem _hunger = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IComponentFactory _componentFactory = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private ISerializationManager _serialization = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private ISharedChatManager _chat = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CheckDivision();
    }

    public void CheckDivision()
    {
        if (_net.IsClient)
            return;

        var readyDivision = new List<EntityUid>();

        var slimeQuery = EntityQueryEnumerator<XenoSlimeComponent, HungerComponent>();
        while (slimeQuery.MoveNext(out var uid, out var slime, out var hungerComp))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (_hunger.GetHunger(hungerComp) < slime.FissionThreshold)
                continue;

            if (slime.DivisionTime == null)
            {
                _jitter.DoJitter(uid, TimeSpan.FromSeconds(2), true);
                slime.DivisionTime = _gameTiming.CurTime + TimeSpan.FromSeconds(2.2);
                continue;
            }

            if (_gameTiming.CurTime < slime.DivisionTime)
                continue;

            _hunger.ModifyHunger(uid, -slime.FissionThreshold);
            slime.DivisionTime = null;
            readyDivision.Add(uid);
        }

        foreach (var uid in readyDivision)
        {
            if (!TryComp<XenoSlimeComponent>(uid, out var slime))
                continue;

            Division((uid, slime));
        }
    }

    private void Division(Entity<XenoSlimeComponent> ent)
    {
        if (_net.IsClient)
            return;

        var selectedTypeSlime = ent.Comp.TypeSlime;

        if (_random.Prob(ent.Comp.MutationChance) && ent.Comp.TypeSlimeMutation.Count > 0)
            selectedTypeSlime = _random.Pick(ent.Comp.TypeSlimeMutation);

        if (!_proto.TryIndex(selectedTypeSlime, out var typeXenoSlimeProto))
            return;

        var newSlime = SpawnNextToOrDrop(ent.Comp.BaseXenoSlimeProto, ent.Owner, null, typeXenoSlimeProto.Components);
        _metaData.SetEntityName(newSlime, typeXenoSlimeProto.Name);

        if (TryComp<XenoSlimeComponent>(newSlime, out var newComp))
            Dirty(newSlime, newComp);
    }
}
