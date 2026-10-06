using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPlaneJumpTests
{
    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void FloorCheckIncludesContactAndPenetration(int z, bool jumps)
    {
        var actor = Room().Players.Single(); actor.Z = Fixed.FromInt(z); actor.OnGround = false;
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.CheckFloor(actor, 2));
    }

    [Theory]
    [InlineData(71, false)]
    [InlineData(72, true)]
    [InlineData(73, true)]
    public void CeilingCheckIncludesContactAndPenetration(int z, bool jumps)
    {
        var actor = Room().Players.Single(); actor.Height = Fixed.FromInt(56); actor.Z = Fixed.FromInt(z);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.CheckCeiling(actor, 2));
    }

    [Fact]
    public void ChecksUseCurrentMovingPlanes()
    {
        var sim = Room(); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(10); actor.Height = Fixed.FromInt(56);
        Assert.Null(ActorJumpActions.CheckFloor(actor, 2));
        Assert.Null(ActorJumpActions.CheckCeiling(actor, 2));
        sim.Floors[0] = 10; sim.Ceilings[0] = 66;
        Assert.Equal(2, ActorJumpActions.CheckFloor(actor, 2));
        Assert.Equal(2, ActorJumpActions.CheckCeiling(actor, 2));
    }

    [Fact]
    public void FloorCheckRunsAsImmediateFrameJump()
    {
        var actor = Room().Players.Single();
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckFloor(self, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void DetachedActorChecksFailExplicitly()
    {
        Assert.Throws<InvalidOperationException>(() => ActorJumpActions.CheckFloor(new Actor(), 2));
        Assert.Throws<InvalidOperationException>(() => ActorJumpActions.CheckCeiling(new Actor(), 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
