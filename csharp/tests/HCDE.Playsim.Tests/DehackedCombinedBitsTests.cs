using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCombinedBitsTests
{
    [Theory]
    [InlineData("2+4+512")]
    [InlineData("2|4|512")]
    [InlineData("2,4,512")]
    [InlineData("2 4 512")]
    [InlineData("2\t4\f512")]
    public void NumericTokensCombineIntoSpawnedFlags(string text)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {text}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(518u, patch.Actors.Single(actor => actor.Index == 2).Bits);
        var actor = Spawn(patch);
        Assert.True(actor.Solid);
        Assert.True(actor.Shootable);
        Assert.True(actor.NoGravity);
        Assert.False(actor.Floating);
    }

    [Fact]
    public void UnsupportedMnemonicPreservesExistingFlagsWithoutNumericTokens()
    {
        var baseline = DehackedPatch.Apply("Thing 2\nBits = 518\n");
        var patch = DehackedPatch.Apply("Thing 2\nBits = UNKNOWN\n", baseline);
        Assert.Single(patch.Errors);
        Assert.Equal(518u, patch.Actors.Single(actor => actor.Index == 2).Bits);
        Assert.True(Spawn(patch).NoGravity);
    }

    [Fact]
    public void RepeatedAssignmentsReplaceRatherThanAccumulateFlags()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = 2+4+512\nBits = 2+4\n");
        Assert.Empty(patch.Errors);
        Assert.False(Spawn(patch).NoGravity);
        Assert.Equal(6u, patch.Actors.Single(actor => actor.Index == 2).Bits);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
