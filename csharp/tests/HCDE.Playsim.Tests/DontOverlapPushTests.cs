using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontOverlapPushTests
{
    [Theory]
    [InlineData(true, false, false, false, 0)]
    [InlineData(false, true, false, false, 0)]
    [InlineData(false, false, false, false, 2)]
    [InlineData(true, false, true, false, 2)]
    [InlineData(false, true, true, false, 2)]
    [InlineData(true, false, false, true, 2)]
    public void HeightPassageDontOverlapGatePrecedesPush(bool canPass, bool bridge, bool legacy, bool pickup, int expected)
    {
        var (sim, mover, target) = Room(legacy);
        mover.CanPass = canPass; target.ActsLikeBridge = bridge; target.SpecialPickup = pickup;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(expected, target.VelocityX.ToDouble());
        Assert.Equal(expected != 0, target.LastPushFromMovement);
        Assert.Equal(expected == 0 ? 0 : 1, sim.PendingSoundRequests.Count);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void OneSidedDontOverlapStillAllowsPush(bool moverFlag, bool targetFlag)
    {
        var (sim, mover, target) = Room(false);
        mover.DontOverlap = moverFlag; target.DontOverlap = targetFlag;
        ActorPhysics.Step(sim, mover);
        Assert.Equal(2, target.VelocityX.ToDouble()); Assert.True(target.LastPushFromMovement);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room(bool legacy)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] },
            compat: legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(40, 0);
        mover.Brain = target.Brain = null; mover.VelocityX = Fixed.FromInt(8);
        mover.DontOverlap = target.DontOverlap = true; target.Solid = false; target.Pushable = true;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
