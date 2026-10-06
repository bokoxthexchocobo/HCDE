using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDistanceJumpTests
{
    [Theory]
    [InlineData(9, 0, 0, false, true)]
    [InlineData(10, 0, 0, false, false)]
    [InlineData(6, 8, 0, false, false)]
    [InlineData(6, 7, 0, false, true)]
    [InlineData(0, 0, 29, false, true)]
    [InlineData(0, 0, 30, false, false)]
    [InlineData(0, 0, -39, false, true)]
    [InlineData(0, 0, -40, false, false)]
    [InlineData(0, 0, 100, true, true)]
    public void NativeDistanceUsesCentersAndVerticalBodyGap(int x, int y, int z, bool noZ, bool expected)
    {
        var actor = new Actor { Height = Fixed.FromInt(20) };
        var target = new Actor { X = Fixed.FromInt(x), Y = Fixed.FromInt(y), Z = Fixed.FromInt(z),
            Height = Fixed.FromInt(30), Radius = Fixed.FromInt(100) };
        Assert.Equal(expected, ActorJumpActions.CheckIfCloser(actor, target, 10, noZ));
    }

    [Fact]
    public void NullTargetAndZeroDistanceDoNotPass()
    {
        var actor = new Actor();
        Assert.False(ActorJumpActions.CheckIfCloser(actor, null, 10));
        Assert.False(ActorJumpActions.CheckIfCloser(actor, new Actor(), 0));
    }

    [Fact]
    public void PlayerAimedTargetDrivesImmediateJump()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single(); sim.AddBot(64, 0);
        player.States.Configure(player, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.JumpIfCloser(self, 65, 2)), new(-1, 2)], 0);
        player.States.Enter(player, 1);
        Assert.Equal(2, player.States.Current);
        Assert.Null(ActorJumpActions.JumpIfCloser(player, 64, 2));
    }
}
