using Content.Shared._Stories.ShowIcons;
using Content.Shared.Actions.Events;
using Content.Shared.Actions;

namespace Content.Shared._Stories.ShowIconsSystem;

public sealed partial class SharedShowIconsSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _action = default!;
    [Dependency] private IComponentFactory _factory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowIconsComponent, ComponentInit>(OnInitActions);
        SubscribeLocalEvent<ShowIconsComponent, ToggleComponents>(OnToggleComponents);
    }

    private void OnInitActions(EntityUid uid, ShowIconsComponent comp, ComponentInit args)
    {
        foreach (var action in comp.Actions)
        {
            var actionId = _action.AddAction(uid, action);
            if (actionId.HasValue)
                comp.GrantedActions.Add(actionId.Value);
        }
    }

    private void OnToggleComponents(EntityUid uid, ShowIconsComponent comp, ToggleComponents args)
    {
        comp.Enabled = !comp.Enabled;
        args.Toggle = true;
        args.Handled = true;
        Dirty(uid, comp);

        if (comp.Enabled)
        {
            var target = uid;

            if (TerminatingOrDeleted(target))
                return;

            comp.Target = target;

            EntityManager.AddComponents(target, comp.Components);
        }
        else
        {
            if (comp.Target == null)
                return;

            if (TerminatingOrDeleted(comp.Target.Value))
                return;

            EntityManager.RemoveComponents(comp.Target.Value, comp.RemoveComponents ?? comp.Components);
        }
    }
}

public sealed partial class ToggleComponents : InstantActionEvent;
