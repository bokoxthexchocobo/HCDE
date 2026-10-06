using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSightJumpTests
{
    [Theory]
    [InlineData(LevelLine.BlockSightFlag, true)]
    [InlineData(LevelLine.BlockHitscanFlag, false)]
    [InlineData(LevelLine.BlockEverythingFlag, true)]
    public void SightJumpUsesSightBlockingRatherThanHitscanBlocking(int flags, bool jumps)
    {
        var sim = Room(flags); var actor = sim.AddBot(100, 0);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.CheckSight(actor, 2));
    }

    [Theory]
    [InlineData(LevelLine.BlockSightFlag, 100, false)]
    [InlineData(LevelLine.BlockSightFlag, 99, true)]
    [InlineData(LevelLine.BlockHitscanFlag, 99, false)]
    [InlineData(LevelLine.BlockSightFlag, -100, false)]
    public void SightOrRangeOnlyJumpsWhenNeitherConditionObservesActor(int flags, int range, bool jumps)
    {
        var sim = Room(flags); var actor = sim.AddBot(100, 0);
        Assert.Equal(jumps ? 2 : (int?)null, ActorJumpActions.CheckSightOrRange(actor, range, 2, true));
    }

    [Fact]
    public void AnyPresentPlayerCanSeeActorEvenAfterDeath()
    {
        var sim = Room(LevelLine.BlockSightFlag, twoPlayers: true); var actor = sim.AddBot(100, 0);
        var observer = sim.Players.Single(player => player.X.ToDouble() == 80);
        observer.Health = 0;
        Assert.Null(ActorJumpActions.CheckSight(actor, 2));
        Assert.Null(ActorJumpActions.CheckSightOrRange(actor, 1, 2));
        observer.Destroy();
        Assert.Equal(2, ActorJumpActions.CheckSight(actor, 2));
        Assert.Equal(2, ActorJumpActions.CheckSightOrRange(actor, 1, 2));
    }

    [Fact]
    public void NoPlayersAllowsBothJumps()
    {
        var sim = Room(LevelLine.BlockSightFlag); var actor = sim.AddBot(100, 0);
        sim.Players.Single().Destroy();
        Assert.Equal(2, ActorJumpActions.CheckSight(actor, 2));
        Assert.Equal(2, ActorJumpActions.CheckSightOrRange(actor, 1000, 2));
    }

    [Fact]
    public void HiddenActorReturnsFrameDestinationAndTwoDimensionsOmitsHeight()
    {
        var sim = Room(LevelLine.BlockSightFlag); var actor = sim.AddBot(100, 0);
        actor.Z = Fixed.FromInt(200);
        Assert.Equal(2, ActorJumpActions.CheckSightOrRange(actor, 100, 2));
        Assert.Null(ActorJumpActions.CheckSightOrRange(actor, 100, 2, true));
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckSight(self, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
    }

    private static AuthoritySimulation Room(int flags, bool twoPlayers = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Sides = [new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
            SideFront = 0, SideBack = 0, Flags = flags }],
        Things = twoPlayers ? [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 80 }]
            : [new LevelThing { Type = 1 }],
    });
}
