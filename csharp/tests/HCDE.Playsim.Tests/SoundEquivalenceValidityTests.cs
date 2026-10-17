using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SoundEquivalenceValidityTests
{
    [Theory]
    [InlineData("silentAlias", "None", true)]
    [InlineData("silentAlias", "unknown", true)]
    [InlineData("*silent", "unknown", true)]
    [InlineData("unknown", "silentAlias", true)]
    [InlineData("unknown", "otherUnknown", false)]
    [InlineData("None", "unknown", false)]
    [InlineData("silentAlias", "otherSilentAlias", false)]
    [InlineData("*silent", "silentAlias", false)]
    public void InitialValidityIsCheckedBeforeAliasResolution(string sound, string check, bool queued)
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true);
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        foreach (var world in new[] { sim, fresh })
        {
            ActorSoundActions.StartSoundIfNotSame(world.Players.Single(), sound, check);
            var requests = world.DrainSoundRequests();
            Assert.Equal(queued ? 1 : 0, requests.Length);
            if (queued) Assert.Equal(sound, requests[0].Sound);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        SoundAliases = new Dictionary<string, string> { ["silentAlias"] = "None", ["otherSilentAlias"] = "None" },
        PlayerSounds = [new("player", 0, "*silent", "None")],
    });
}
