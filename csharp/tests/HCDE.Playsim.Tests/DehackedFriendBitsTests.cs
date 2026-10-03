using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFriendBitsTests
{
    [Theory]
    [InlineData("FRIEND + SOLID + SHOOTABLE", true)]
    [InlineData("1073741830", true)]
    [InlineData("STEALTH + SOLID + SHOOTABLE", false)]
    [InlineData("FRIEND + STEALTH + 6", false)]
    [InlineData("6", false)]
    public void SharedBitUsesNativeStealthSelector(string bits, bool friendly)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(friendly, Assert.Single(Room(patch).Actors).Friendly);
    }

    [Fact]
    public void ChainedAssignmentsPreserveAndThenReplaceSelector()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = STEALTH + 6\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        var third = DehackedPatch.Apply("Thing 2\nBits = FRIEND + 6\n", second);
        Assert.True(second.Actors.Single(actor => actor.Index == 2).BitsUseStealth);
        Assert.False(Assert.Single(Room(second).Actors).Friendly);
        Assert.True(Assert.Single(Room(third).Actors).Friendly);
        Assert.True(first.Actors.Single(actor => actor.Index == 2).BitsUseStealth);
    }

    [Fact]
    public void ExplicitPlayerBitsForceFriendlyEvenWithoutFriendBit()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }],
        }, dehacked: DehackedPatch.Apply("Thing 1\nBits = 6\n"));
        Assert.True(Assert.Single(sim.Players).Friendly);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
