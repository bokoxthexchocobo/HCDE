using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MappedHealthPickupSoundTests
{
    [Theory]
    [InlineData(false, 2011, "MedPatch", "misc/i_pkup")]
    [InlineData(false, 2012, "MedicalKit", "misc/i_pkup")]
    [InlineData(false, 83, "SurgeryKit", "misc/i_pkup")]
    [InlineData(true, 82, "ArtiHealth", "misc/p_pkup")]
    [InlineData(true, 32, "ArtiSuperHealth", "misc/p_pkup")]
    public void ActiveMappingSelectsInheritedClassSound(bool raven, int type, string storedType, string sound)
    {
        var (sim, player, item) = Room(raven, type); sim.Tick();
        Assert.True(item.Destroyed); Assert.Equal(1, player.Inventory.CountHealthPickup(storedType));
        Assert.Equal(50, player.Health); Assert.Equal(sound, Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Theory]
    [InlineData(false, 2011)]
    [InlineData(true, 82)]
    public void ExplicitSoundOverrideStillWinsAndReplays(bool raven, int type)
    {
        var (sim, _, item) = Room(raven, type);
        item.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Pickup] = "custom/mapped" };
        var saved = SimSavegame.Write(sim, true); sim.Tick(); var sounds = sim.DrainSoundRequests();
        Assert.Equal("custom/mapped", Assert.Single(sounds).Sound);
        SimSavegame.Apply(sim, saved); sim.Tick(); Assert.Equal(sounds, sim.DrainSoundRequests());
    }

    [Theory]
    [InlineData(2011, "misc/health_pkup")]
    [InlineData(82, "misc/w_pkup")]
    public void DoomMappingRetainsItsOwnSounds(int type, string sound)
    {
        var sim = AuthoritySimulation.Start(Level(type)); sim.Players.Single().Health = 50;
        sim.Tick(); Assert.Equal(sound, Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Item) Room(bool raven, int type)
    {
        var sim = AuthoritySimulation.Start(Level(type), spawnOptions: new SpawnOptions(
            StrifeHealthPickups: !raven, RavenHealthPickups: raven));
        var player = sim.Players.Single(); player.Health = 50; sim.DrainSoundRequests();
        return (sim, player, sim.Actors.Single(a => a.DoomEdNum == type));
    }

    private static PlayLevel Level(int type) => new() {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }]
    };
}
