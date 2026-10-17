using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DontOverlapContactReturnTests
{
    [Theory]
    [InlineData(false, 4)]
    [InlineData(true, 4)]
    [InlineData(false, -4)]
    [InlineData(true, -4)]
    public void EarlyReturnBlocksBlastContactWithoutMomentumOrDamage(bool solid, int x)
    {
        var (sim, mover, target) = Room(); target.Solid = solid;
        mover.Blasted = true; mover.VelocityX = Fixed.FromInt(x);
        Assert.False(ActorPhysics.TryMove(sim, mover, x, 0, out _));
        Assert.Equal(0, target.VelocityX.Raw); Assert.Equal(1000, target.Health); Assert.Equal(1000, mover.Health);
    }

    [Fact]
    public void AiMoverCanEscapeSolidOverlapWithoutPushSideEffects()
    {
        var (sim, mover, target) = Room(); target.Pushable = true;
        mover.MoveDirection = 4; mover.MovementSpeed = Fixed.FromInt(8);
        Assert.True(ActorWanderActions.Move(sim, mover)); Assert.Equal(-8, mover.X.ToDouble());
        Assert.Equal(0, target.VelocityX.Raw); Assert.False(target.LastPushFromMovement);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void PlayerEscapeRequiresSolidTargetForNativeUnblocking(bool solid, bool moves)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); var target = sim.AddBot(20, 0); target.Brain = null;
        player.DontOverlap = target.DontOverlap = true; target.Solid = solid;
        Assert.Equal(moves, ActorPhysics.TryMove(sim, player, -4, 0, out _));
        Assert.Equal(moves ? -4 : 0, player.X.ToDouble());
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(30, 0);
        mover.Brain = target.Brain = null; mover.DontOverlap = target.DontOverlap = true;
        mover.Health = target.Health = 1000;
        return (sim, mover, target);
    }
}
