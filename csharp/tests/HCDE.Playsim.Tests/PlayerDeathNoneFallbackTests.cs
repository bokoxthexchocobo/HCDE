using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDeathNoneFallbackTests
{
    public static IEnumerable<object[]> Cases => new[] { "None", "none", "NONE" }
        .SelectMany(name => new[] { false, true }.Select(extreme => new object[] { name, extreme }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void ReservedNoneDeathSlotUsesPlayerFallbackAcrossFreshSave(string name, bool extreme)
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = name };
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            var player = world.Players.Single(); player.DeathDamageAmount = 10;
            player.Health = extreme ? -100 : -1;
            var request = Assert.Single(world.DrainSoundRequests());
            Assert.Equal(extreme ? "*xdeath" : "*death", request.Sound);
            Assert.Equal(2, request.Channel);
            Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        SoundDefinitions = new Dictionary<string, string> { ["*death"] = "DEATH", ["*xdeath"] = "XDEATH" },
    });
}
