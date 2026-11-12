using Content.Server.Humanoid.Systems;
using Content.Shared._Stories.ERT;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;

namespace Content.Server._Stories.ERT;

public sealed partial class StoriesERTCryostorageSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private RandomHumanoidSystem _randomHumanoid = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ERTCryostorageComponent, BoundUIOpenedEvent>(OnUiOpened);

        Subs.BuiEvents<ERTCryostorageComponent>(ERTCryostorageUiKey.Key, subs =>
        {
            subs.Event<ERTCryostorageSpawnMessage>(OnSpawnMessage);
        });
    }

    private void OnUiOpened(Entity<ERTCryostorageComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnSpawnMessage(Entity<ERTCryostorageComponent> ent, ref ERTCryostorageSpawnMessage args)
    {
        var comp = ent.Comp;

        if (args.OptionIndex < 0 || args.OptionIndex >= comp.Options.Count)
            return;

        if (comp.UseOnce && comp.Blocked)
            return;

        var option = comp.Options[args.OptionIndex];
        if (option.RequiresAdminAllow && !comp.AdminAccess)
            return;

        if (!_container.TryGetContainer(ent, comp.ContainerId, out var container))
            return;

        var humanoid = _randomHumanoid.SpawnRandomHumanoid(
            option.Settings,
            Transform(ent).Coordinates,
            Loc.GetString("stories-ert-cryostorage-default-name"));

        if (!_container.Insert(humanoid, container))
        {
            QueueDel(humanoid);
            return;
        }

        if (comp.UseOnce)
            comp.Blocked = true;

        UpdateUi(ent);
    }

    private void UpdateUi(Entity<ERTCryostorageComponent> ent)
    {
        var state = new ERTCryostorageState(
            ent.Comp.Options,
            ent.Comp.TitleName,
            ent.Comp.AdminAccess,
            ent.Comp.Blocked);

        _ui.SetUiState(ent.Owner, ERTCryostorageUiKey.Key, state);
    }
}
