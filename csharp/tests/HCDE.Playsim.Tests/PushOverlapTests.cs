using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PushOverlapTests
{
    [Theory]
    [InlineData(30, 0, -8, 0)]
    [InlineData(30, 0, 0, 8)]
    [InlineData(38, 38, 8, 0)]
    public void DestinationBoxContactPushesDuringSeparationParallelAndCornerMoves(int targetX, int targetY, int vx, int vy)
    {
        var (sim, mover, target) = Room(targetX, targetY);
        mover.VelocityX = Fixed.FromInt(vx); mover.VelocityY = Fixed.FromInt(vy);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(vx * 0.25, target.VelocityX.ToDouble());
        Assert.Equal(vy * 0.25, target.VelocityY.ToDouble());
        Assert.Equal(vx, mover.X.ToDouble()); Assert.Equal(vy, mover.Y.ToDouble());
        Assert.True(target.LastPushFromMovement); Assert.Single(sim.PendingSoundRequests);
    }

    [Theory]
    [InlineData(40, 8)]
    [InlineData(30, 0)]
    public void ExactBoxBoundaryAndStationaryStepDoNotPush(int targetY, int vx)
    {
        var (sim, mover, target) = Room(30, targetY); mover.VelocityX = Fixed.FromInt(vx);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(0, target.VelocityX.Raw); Assert.False(target.LastPushFromMovement);
        Assert.Empty(sim.PendingSoundRequests);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room(int x, int y)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(x, y);
        mover.Brain = target.Brain = null; target.Solid = false; target.Pushable = true;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
