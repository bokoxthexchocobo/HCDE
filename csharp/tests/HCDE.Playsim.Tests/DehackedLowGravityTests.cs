using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedLowGravityTests
{
    [Theory]
    [InlineData("6", -1.0)]
    [InlineData("LOGRAV+6", -0.25)]
    [InlineData("lograv+NOGRAVITY+6", 0.0)]
    public void LowGravityAssignmentControlsFallingWithoutOverridingNoGravity(string bits, double expectedVelocity)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        var sim = Room(patch); var actor = Assert.Single(sim.Actors);
        actor.Brain!.Enabled = false; actor.Z = Fixed.FromInt(32); actor.OnGround = false;
        sim.Tick();
        Assert.Equal(expectedVelocity, actor.VelocityZ.ToDouble());
        Assert.Equal(32, actor.Z.ToDouble());
    }

    [Fact]
    public void ReplacingExtendedBitsPreservesQuarterGravitySideEffectAndBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = LOGRAV+NOTELEPORT\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = CANSLIDE\n", first);
        var record = second.Actors.Single(actor => actor.Index == 2);
        Assert.True(record.GravityPatched); Assert.Equal(0.25, record.Gravity);
        Assert.Equal(0u, record.Bits2 & 1u);
        var actor = Assert.Single(Room(second).Actors);
        Assert.Equal(0.25, actor.Gravity.ToDouble()); Assert.False(actor.NoTeleport);
        Assert.True(Assert.Single(Room(first).Actors).NoTeleport);
    }

    private static AuthoritySimulation Room(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
