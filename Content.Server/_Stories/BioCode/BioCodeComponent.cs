using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.BioCode;

[RegisterComponent]
public sealed partial class BioCodeComponent : Component
{
    [DataField]
    public bool AutoRecordDNA;

    [DataField]
    public bool AllowOverwriting = true;

    [DataField]
    public float BeepInterval = 1;

    [DataField]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField]
    public DamageSpecifier DamagePerRecord = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Piercing", 2 },
        },
    };

    [DataField]
    public float Delay = 3f;

    [DataField]
    public bool HardDefense;

    [DataField]
    public float? InitialBeepDelay = 0;

    [DataField]
    public bool ItemBioCode;

    [DataField]
    public string? RecordedDNA;
}
