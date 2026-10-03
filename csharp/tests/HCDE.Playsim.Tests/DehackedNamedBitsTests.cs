using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNamedBitsTests
{
    [Theory]
    [InlineData("SPECIAL", 1u)]
    [InlineData("SOLID", 2u)]
    [InlineData("SHOOTABLE", 4u)]
    [InlineData("AMBUSH", 32u)]
    [InlineData("SPAWNCEILING", 256u)]
    [InlineData("NOGRAVITY", 512u)]
    [InlineData("DROPOFF", 1024u)]
    [InlineData("PICKUP", 2048u)]
    [InlineData("FLOAT", 16384u)]
    [InlineData("DROPPED", 131072u)]
    [InlineData("COUNTKILL", 4194304u)]
    public void NamedFlagMatchesNumericSpawnBehavior(string name, uint bits)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {name.ToLowerInvariant()}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(bits, patch.Actors.Single(actor => actor.Index == 2).Bits);
        var named = Room(patch);
        var numeric = Room(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
        named.Tick();
        numeric.Tick();
        Assert.Equal(numeric.Checksum, named.Checksum);
    }

    [Fact]
    public void MixedNamesAndNumbersCombineIntoGameplayFlags()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = SOLID + 4 | NOGRAVITY,COUNTKILL\n");
        Assert.Empty(patch.Errors);
        var actor = Assert.Single(Room(patch).Actors);
        Assert.True(actor.Solid);
        Assert.True(actor.Shootable);
        Assert.True(actor.NoGravity);
        Assert.True(actor.IsMonster);
        Assert.False(actor.Floating);
    }

    [Fact]
    public void UnknownTokenDoesNotDiscardRecognizedNames()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = SOLID | UNKNOWN | SHOOTABLE\n");
        Assert.Single(patch.Errors);
        Assert.Equal(6u, patch.Actors.Single(actor => actor.Index == 2).Bits);
        Assert.True(Assert.Single(Room(patch).Actors).Shootable);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
