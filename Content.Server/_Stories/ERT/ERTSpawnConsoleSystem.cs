using Content.Server.GameTicking;
using Content.Shared._Stories.ERT;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._Stories.ERT;

public sealed partial class StoriesERTSpawnConsoleSystem : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ERTSpawnConsoleComponent, BoundUIOpenedEvent>(OnUiOpened);

        Subs.BuiEvents<ERTSpawnConsoleComponent>(ERTSpawnConsoleUiKey.Key, subs =>
        {
            subs.Event<ERTSpawnConsoleCallMessage>(OnCallMessage);
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ERTSpawnConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.CooldownEnd is not { } end || _timing.CurTime < end)
                continue;

            comp.CooldownEnd = null;
            comp.ActiveOptionIndex = -1;
            UpdateUi((uid, comp));
        }
    }

    private void OnUiOpened(Entity<ERTSpawnConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnCallMessage(Entity<ERTSpawnConsoleComponent> ent, ref ERTSpawnConsoleCallMessage args)
    {
        var comp = ent.Comp;

        if (args.OptionIndex < 0 || args.OptionIndex >= comp.Options.Count)
            return;

        if (comp.CooldownEnd != null)
            return;

        var option = comp.Options[args.OptionIndex];

        if (!CanCall(comp, option))
            return;

        _gameTicker.AddGameRule(option.Rule);
        comp.Balance -= option.Price;
        comp.ActiveOptionIndex = args.OptionIndex;
        comp.CooldownEnd = _timing.CurTime + TimeSpan.FromSeconds(comp.Delay);

        UpdateUi(ent);
    }

    private void UpdateUi(Entity<ERTSpawnConsoleComponent> ent)
    {
        var comp = ent.Comp;
        var availability = new List<bool>(comp.Options.Count);

        foreach (var option in comp.Options)
        {
            availability.Add(CanCall(comp, option));
        }

        var state = new ERTSpawnConsoleState(
            comp.Options,
            comp.CooldownEnd != null,
            comp.ActiveOptionIndex,
            comp.Balance,
            comp.AdminAccess,
            availability,
            comp.TitleName,
            comp.BalanceName);

        _ui.SetUiState(ent.Owner, ERTSpawnConsoleUiKey.Key, state);
    }

    private static bool CanCall(ERTSpawnConsoleComponent comp, ERTSpawnOption option)
    {
        if (option.RequiresAdminAllow && !comp.AdminAccess)
            return false;

        return option.Price <= comp.Balance;
    }
}
