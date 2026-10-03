using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedThingNumericRangeTests
{
    [Theory]
    [InlineData("4294967296")]
    [InlineData("-2147483649")]
    [InlineData("9223372036854775807")]
    [InlineData("-9223372036854775808")]
    [InlineData("9223372036854775808")]
    [InlineData("-9223372036854775809")]
    public void InvalidAssignmentsPreservePriorValuesAndSpawnDefaults(string value)
    {
        var baseline = DehackedPatch.Apply("Thing 2\nHit points = 88\nReaction time = 17\nMass = 123\n");
        var patch = DehackedPatch.Apply($"Thing 2\nHit points = {value}\nReaction time = {value}\nMass = {value}\nBits = {value}\n", baseline);
        Assert.Equal(4, patch.Errors.Count);
        var record = patch.Actors.Single(actor => actor.Index == 2);
        Assert.Equal(88, record.Health);
        Assert.Equal(17, record.ReactionTime);
        Assert.Equal(123, record.Mass);
        Assert.False(record.BitsPatched);
        var actor = Spawn(patch);
        Assert.Equal(88, actor.Health);
        Assert.Equal(17, actor.ReactionTime);
        Assert.Equal(123, actor.Mass);
        Assert.True(actor.Shootable);
        Assert.Empty(baseline.Errors);
    }

    [Theory]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("2147483648", int.MinValue)]
    [InlineData("4294967295", -1)]
    [InlineData("-2147483648", int.MinValue)]
    public void ValidSignedAndUnsignedRepresentationsReachRuntime(string value, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nReaction time = {value}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(expected, Spawn(patch).ReactionTime);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
