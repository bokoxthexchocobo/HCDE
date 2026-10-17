using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TouchyOverlapTests
{
    [Theory]
    [InlineData(38, 38, 8, 0, true)]
    [InlineData(30, 0, -8, 0, true)]
    [InlineData(30, 0, 0, 8, true)]
    [InlineData(30, 40, 8, 0, false)]
    public void DestinationBoxTriggersCornerSeparatingAndParallelContacts(int x, int y, int vx, int vy, bool dies)
    {
        var (sim, mover, mine) = Room(false); mine.X = Fixed.FromInt(x); mine.Y = Fixed.FromInt(y);
        mover.VelocityX = Fixed.FromInt(vx); mover.VelocityY = Fixed.FromInt(vy);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(dies, mine.IsDead); Assert.Equal(!dies, mine.Armed);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    public void DontOverlapEarlyReturnRetainsCompatibilityAndPickupExceptions(bool legacy, bool special, bool dies)
    {
        var (sim, mover, mine) = Room(legacy);
        mover.DontOverlap = mine.DontOverlap = true; mine.SpecialPickup = special;
        Assert.Equal(dies, ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(dies, mine.IsDead); Assert.Equal(!dies, mine.Armed);
    }

    [Fact]
    public void TouchDeathPrecedesOrdinaryPushSideEffects()
    {
        var (sim, mover, mine) = Room(false); mine.Pushable = true;
        mover.VelocityX = Fixed.FromInt(8); mover.MovementPushContacts = new HashSet<Actor>();
        Assert.True(ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.True(mine.IsDead); Assert.False(mine.LastPushFromMovement);
        Assert.Equal(0, mine.VelocityX.Raw); Assert.Empty(sim.PendingSoundRequests);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Mine) Room(bool legacy)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] },
            compat: legacy ? CompatSurface.NoPassMobj : CompatSurface.None);
        var mover = sim.AddBot(0, 0, 3001); var mine = sim.AddBot(30, 0, 3004);
        mover.Brain = mine.Brain = null; mine.SeeState = -1;
        mine.Solid = false; mine.Touchy = mine.Armed = true;
        return (sim, mover, mine);
    }
}
