using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorPlayerRangeJumpTests
{
    [Theory]
    [InlineData(10, 0, 0, 10, false, false)]
    [InlineData(11, 0, 0, 10, false, true)]
    [InlineData(6, 8, 0, 10, false, false)]
    [InlineData(0, 0, 20, 10, false, false)]
    [InlineData(0, 0, 21, 10, false, true)]
    [InlineData(0, 0, -21, 10, false, true)]
    [InlineData(0, 0, 100, 10, true, false)]
    [InlineData(10, 0, 0, -10, false, false)]
    public void RangeUsesInclusiveDistanceFromPlayerCenterToActorBody(
        int x, int y, int z, int distance, bool twoDimension, bool jumps)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Height = Fixed.FromInt(20);
        var actor = sim.AddBot(x, y); actor.Z = Fixed.FromInt(z); actor.Height = Fixed.FromInt(20);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.CheckRange(actor, distance, 2, twoDimension));
    }

    [Fact]
    public void AnyPlayerInRangePreventsJumpRegardlessOfHealth()
    {
        var sim = Room(true); var actor = sim.AddBot(100, 0);
        Assert.Null(ActorJumpActions.CheckRange(actor, 1, 2, true));
        var nearby = sim.Players.Single(p => p.X.ToDouble() == 100); nearby.Health = 0;
        Assert.Null(ActorJumpActions.CheckRange(actor, 1, 2, true));
        nearby.Destroy();
        Assert.Equal(2, ActorJumpActions.CheckRange(actor, 1, 2, true));
    }

    [Fact]
    public void OutOfRangeJumpRunsOnFrameAndNoPlayersAlsoJumps()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckRange(self, 10, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        sim.Players.Single().Destroy();
        Assert.Equal(2, ActorJumpActions.CheckRange(actor, double.PositiveInfinity, 2));
    }

    private static AuthoritySimulation Room(bool twoPlayers = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = twoPlayers ? [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 100 }]
            : [new LevelThing { Type = 1 }],
    });
}
