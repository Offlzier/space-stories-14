using Content.IntegrationTests.Fixtures;
using Content.Server.Silicons.Laws;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Stories.Silicons;

[TestFixture]
public sealed class SiliconLawsetLoadoutTest : GameTest
{
    private static readonly EntProtoId StationAiBrain = "StationAiBrain";
    private static readonly ProtoId<RoleLoadoutPrototype> StationAiRoleLoadout = "JobStationAi";
    private static readonly ProtoId<LoadoutGroupPrototype> LawsetGroup = "STStationAiLawset";
    private static readonly ProtoId<LoadoutPrototype> CrewsimovLoadout = "STStationAiLawsetCrewsimov";
    private static readonly ProtoId<SiliconLawsetPrototype> DefaultLawset = "NTDefault";
    private static readonly ProtoId<SiliconLawsetPrototype> CrewsimovLawset = "Crewsimov";

    /// <summary>
    /// Checks that the lawset selected in the station AI loadout is applied on spawn,
    /// and that the default loadout keeps the default lawset.
    /// </summary>
    [Test]
    public async Task TestStationAiLawsetLoadout()
    {
        var pair = Pair;
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var lawSystem = SEntMan.System<SiliconLawSystem>();

        await server.WaitAssertion(() =>
        {
            var session = ServerSession!;

            var defaultProfile = new HumanoidCharacterProfile();
            var defaultLoadout = new RoleLoadout(StationAiRoleLoadout);
            defaultLoadout.SetDefault(defaultProfile, session, SProtoMan);
            defaultProfile.SetLoadout(defaultLoadout);

            var customProfile = new HumanoidCharacterProfile();
            var customLoadout = new RoleLoadout(StationAiRoleLoadout);
            customLoadout.SetDefault(customProfile, session, SProtoMan);
            Assert.That(customLoadout.AddLoadout(LawsetGroup, CrewsimovLoadout, SProtoMan));
            customProfile.SetLoadout(customLoadout);

            var defaultAi = SpawnAi(testMap.GridCoords, session, defaultProfile);
            var customAi = SpawnAi(testMap.GridCoords, session, customProfile);

            Assert.Multiple(() =>
            {
                Assert.That(SEntMan.GetComponent<SiliconLawProviderComponent>(defaultAi).Laws, Is.EqualTo(DefaultLawset));
                Assert.That(SEntMan.GetComponent<SiliconLawProviderComponent>(customAi).Laws, Is.EqualTo(CrewsimovLawset));
                Assert.That(lawSystem.GetLaws(customAi).Laws, Is.EquivalentTo(lawSystem.GetLawset(CrewsimovLawset).Laws));
                Assert.That(SEntMan.GetComponent<SiliconLawProviderComponent>(customAi).Subverted, Is.False);
            });

            SEntMan.DeleteEntity(defaultAi);
            SEntMan.DeleteEntity(customAi);
        });
    }

    private EntityUid SpawnAi(EntityCoordinates coords, ICommonSession session, HumanoidCharacterProfile profile)
    {
        var ai = SEntMan.SpawnEntity(StationAiBrain, coords);
        var ev = new PlayerSpawnCompleteEvent(ai, session, "StationAi", false, true, 1, EntityUid.Invalid, profile);
        SEntMan.EventBus.RaiseLocalEvent(ai, ev);
        return ai;
    }
}
