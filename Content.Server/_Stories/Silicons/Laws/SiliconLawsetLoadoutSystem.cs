using Content.Server.Silicons.Laws;
using Content.Shared.Administration.Logs;
using Content.Shared.Clothing;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Server._Stories.Silicons.Laws;

/// <summary>
/// Applies the initial lawset chosen by the player in the role loadout (see <see cref="LoadoutPrototype.Lawset"/>)
/// to a spawned silicon, e.g. the station AI.
/// </summary>
public sealed partial class SiliconLawsetLoadoutSystem : EntitySystem
{
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private SiliconLawSystem _siliconLaw = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconLawProviderComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(Entity<SiliconLawProviderComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        var roleLoadoutId = LoadoutSystem.GetJobPrototype(args.JobId);

        if (!args.Profile.Loadouts.TryGetValue(roleLoadoutId, out var roleLoadout))
            return;

        foreach (var loadouts in roleLoadout.SelectedLoadouts.Values)
        {
            foreach (var loadout in loadouts)
            {
                if (!ProtoMan.TryIndex(loadout.Prototype, out var loadoutProto) || loadoutProto.Lawset is not { } lawset)
                    continue;

                _siliconLaw.SetInitialLawset(ent, lawset);
                _adminLogger.Add(LogType.SiliconLaw,
                    LogImpact.Low,
                    $"{ToPrettyString(ent):entity} got initial lawset {lawset} from {args.Player.Name}'s loadout");
                return;
            }
        }
    }
}
