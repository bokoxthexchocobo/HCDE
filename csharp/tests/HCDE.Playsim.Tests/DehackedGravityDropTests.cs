using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedGravityDropTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(512, true, false)]
    [InlineData(131072, false, true)]
    [InlineData(131584, true, true)]
    public void ExplicitBitsInitializeGravityAndDroppedFlags(int bits, bool noGravity, bool dropped)
    {
        var sim = Room(bits);
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(noGravity, actor.NoGravity);
        Assert.Equal(dropped, actor.Dropped);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(512, false)]
    public void PatchedNoGravityControlsVerticalAcceleration(int bits, bool falls)
    {
        var sim = Room(bits);
        var actor = Assert.Single(sim.Actors);
        actor.Brain!.Enabled = false;
        var z = actor.Z.ToDouble();
        sim.Tick();
        Assert.Equal(z, actor.Z.ToDouble());
        Assert.Equal(falls, actor.VelocityZ.Raw < 0);
        sim.Tick();
        Assert.Equal(falls, actor.Z.ToDouble() < z);
    }

    [Fact]
    public void ChainedBitsAssignmentClearsBothFlagsWithoutChangingBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = 131584\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = 0\n", first);
        var a = Assert.Single(Spawn(first).Actors); var b = Assert.Single(Spawn(second).Actors);
        Assert.True(a.NoGravity); Assert.True(a.Dropped);
        Assert.False(b.NoGravity); Assert.False(b.Dropped);
    }

    private static AuthoritySimulation Room(int bits) => Spawn(DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
    private static AuthoritySimulation Spawn(DehackedPatchResult patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 3004, Z = 100 }],
    }, dehacked: patch);
}
