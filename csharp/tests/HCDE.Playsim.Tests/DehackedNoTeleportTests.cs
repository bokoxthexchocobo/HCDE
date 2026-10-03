using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNoTeleportTests
{
    [Theory]
    [InlineData("NOTELEPORT", false)]
    [InlineData("noteleport+SOLID+SHOOTABLE", true)]
    public void ExtendedFlagReachesSpawnWithoutReplacingUnspecifiedFirstSet(string bits, bool firstSetPatched)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        var record = patch.Actors.Single(actor => actor.Index == 2);
        Assert.Equal(firstSetPatched, record.BitsPatched);
        Assert.True(record.Bits2Patched); Assert.Equal(0x80u, record.Bits2);
        var sim = Room(patch); var actor = sim.Actors.Single(actor => actor.DoomEdNum == 3004);
        Assert.True(actor.Solid); Assert.True(actor.Shootable); Assert.True(actor.NoTeleport);
        Assert.False(LineSpecials.Execute(sim, actor, LineSpecials.Teleport, 0));
    }

    [Fact]
    public void LaterNumericFirstSetDoesNotClearExtendedFlag()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = NOTELEPORT\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = 6\n", first);
        Assert.True(second.Actors.Single(actor => actor.Index == 2).Bits2Patched);
        Assert.True(Room(second).Actors.Single(actor => actor.DoomEdNum == 3004).NoTeleport);
        Assert.False(first.Actors.Single(actor => actor.Index == 2).BitsPatched);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    }, dehacked: patch);
}
