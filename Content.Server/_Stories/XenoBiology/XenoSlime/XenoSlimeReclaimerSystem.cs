using Content.Shared._Stories.XenoBiology.XenoSlime;
using Content.Server._Stories.XenoBiology.XenoSlime;
using System.Numerics;
using Content.Shared.Administration.Logs;
using Content.Shared.Audio;
using Content.Shared.Body.Components;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Climbing.Events;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Materials;
using Content.Shared.Medical;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Content.Shared.Tools.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Chat;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Stories.XenoBiology.XenoSlime;

public sealed partial class XenoSlimeReclaimerSystem : EntitySystem
{
    [Dependency] private SharedChatSystem _chatSystem = default!;
    [Dependency] private ISharedChatManager _chat = default!;
    [Dependency] private IConfigurationManager _configManager = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private IRobustRandom _robustRandom = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private SharedMindSystem _minds = default!;
    [Dependency] private InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoSlimeReclaimerComponent, ClimbedOnEvent>(OnClimbedOn);
    }

    private void OnClimbedOn(Entity<XenoSlimeReclaimerComponent> reclaimer, ref ClimbedOnEvent args)
    {
        if (!HasComp<XenoSlimeComponent>(args.Climber) || reclaimer.Comp.Active)
        {
            var direction = new Vector2(_robustRandom.Next(-2, 2), _robustRandom.Next(-2, 2));
            _throwing.TryThrow(args.Climber, direction, 0.5f);
            return;
        }

        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Instigator):player} used a biomass reclaimer to gib {ToPrettyString(args.Climber):target} in {ToPrettyString(reclaimer):reclaimer}");

        StartProcessing(args.Climber, reclaimer);
    }

    private void StartProcessing(EntityUid toProcess, Entity<XenoSlimeReclaimerComponent> reclaimer)
    {
        if (!TryComp<XenoSlimeComponent>(toProcess, out var xenoSlime))
            return;

        QueueDel(toProcess);

        reclaimer.Comp.Active = true;
        _jitter.AddJitter(reclaimer, -10, 100);
        _audio.PlayPvs("/Audio/Machines/reclaimer_startup.ogg", reclaimer);
        _ambient.SetAmbience(reclaimer, true);

        Timer.Spawn(TimeSpan.FromSeconds(xenoSlime.ProcessingTime), () =>
            {
                if (Deleted(reclaimer))
                    return;

                RemComp<JitteringComponent>(reclaimer);
                _ambient.SetAmbience(reclaimer, false);
                reclaimer.Comp.Active = false;

                var extract = Spawn(xenoSlime.Extract, Transform(reclaimer).Coordinates);
                _transform.DropNextTo(extract, reclaimer.Owner);
            });
    }
}
