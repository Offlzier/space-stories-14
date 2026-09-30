using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Silicons.Laws;

public sealed partial class SiliconLawSystem
{
    /// <summary>
    /// Silently replaces the provided lawset without notifying the silicon or marking it as subverted.
    /// Intended for setting initial laws, e.g. from the player's loadout on spawn.
    /// </summary>
    public void SetInitialLawset(Entity<SiliconLawProviderComponent> ent, ProtoId<SiliconLawsetPrototype> lawset)
    {
        ent.Comp.Laws = lawset;
        ent.Comp.Lawset = GetLawset(lawset);
    }
}
