using Content.Shared.AlertLevel;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.StationsControl;

[RegisterComponent]
public sealed partial class StationsControlConsoleComponent : Component
{
    [DataField]
    public List<ProtoId<AlertLevelPrototype>> AllowedAlertLevels = new();

    [DataField]
    public bool AllowAnyAlertLevel;

    [DataField]
    public TimeSpan MinEvacTime = TimeSpan.FromMinutes(2);

    [DataField]
    public TimeSpan MaxEvacTime = TimeSpan.FromMinutes(30);
}
