using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class OrdinaryBoxBlockingTests
{
    [Theory]
    [InlineData(38, 38, false)]
    [InlineData(38, 40, true)]
    public void OrdinarySolidBlockingUsesStrictDestinationBox(int targetX, int targetY, bool moves)
    {
        var (sim, mover, target) = Room(false); target.X = Fixed.FromInt(targetX); target.Y = Fixed.FromInt(targetY);
        Assert.Equal(moves, ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(moves ? 8 : 0, mover.X.ToDouble());
    }

    [Theory]
    [InlineData(false, -4, false)]
    [InlineData(true, -4, true)]
    [InlineData(false, 4, false)]
    [InlineData(true, 4, false)]
    [InlineData(false, 0, false)]
    [InlineData(true, 0, true)]
    public void OrdinaryNonPlayerAndPlayerOverlapHaveDifferentEscapeRules(bool player, int x, bool moves)
    {
        var (sim, mover, _) = Room(player);
        Assert.Equal(moves, ActorPhysics.TryMove(sim, mover, x, 0, out _));
    }

    [Fact]
    public void AiMovementReceivesNativeEscapePermission()
    {
        var (sim, mover, _) = Room(false); mover.MoveDirection = 4; mover.MovementSpeed = Fixed.FromInt(8);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(-8, mover.X.ToDouble());
    }

    [Fact]
    public void PlayerEscapeStillPushesBeforeFinalUnblocking()
    {
        var (sim, mover, target) = Room(true); target.Pushable = true;
        mover.VelocityX = Fixed.FromInt(-4); ActorPhysics.Step(sim, mover);
        Assert.Equal(-4, mover.X.ToDouble()); Assert.Equal(-1, target.VelocityX.ToDouble());
    }

    [Fact]
    public void EscapeRequiresExistingVerticalOverlapWhenHeightPassageDisabled()
    {
        var (sim, mover, target) = Room(true); mover.CanPass = false; target.Z = Fixed.FromInt(100);
        Assert.False(ActorPhysics.TryMove(sim, mover, -4, 0, out _));
    }

    [Fact]
    public void DirectPositionCheckTestsDestinationRatherThanSweepingActors()
    {
        var (sim, mover, target) = Room(false); mover.X = Fixed.FromInt(-64); target.X = default;
        Assert.True(ActorPhysics.TryMove(sim, mover, 64, 0, out _)); Assert.Equal(64, mover.X.ToDouble());
    }

    [Fact]
    public void OrdinaryPhysicsSubstepsStillStopBeforeTarget()
    {
        var (sim, mover, target) = Room(false); mover.X = Fixed.FromInt(-64); target.X = default;
        mover.VelocityX = Fixed.FromInt(128); ActorPhysics.Step(sim, mover);
        Assert.InRange(mover.X.ToDouble(), -64, -40);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room(bool player)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = player ? [new LevelThing { Type = 1 }] : [] });
        Actor mover = player ? sim.Players.Single() : sim.AddBot(0, 0);
        var target = sim.AddBot(player ? 20 : 30, 0); mover.Brain = target.Brain = null;
        return (sim, mover, target);
    }
}
