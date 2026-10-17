using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCheckSightMethodTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(7, true)]
    public void NativeFlagCombinationsControlBlockingLineTraversal(int flags, bool expected)
    {
        var sim = Room(true); var source = sim.Actors.Single(a => a is not PlayerPawn);
        Assert.Equal(expected, source.CheckSight(sim.Players.Single(), (ActorSightFlags)flags));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvisibleTargetUsesExactNamedStreamAndFreshContinuation(bool ignore)
    {
        var sim = Room(); var reference = Room();
        var source = sim.Actors.Single(a => a is not PlayerPawn); var target = sim.Players.Single();
        target.Invisible = true; var flags = ignore ? ActorSightFlags.IgnoreVisibility : 0;
        for (var i = 0; i < 16; i++)
            Assert.Equal(ignore || reference.NextCheckSightByte() <= 50, source.CheckSight(target, flags));
        Assert.Equal(reference.CaptureState().CheckSightRandomState, sim.CaptureState().CheckSightRandomState);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        var restoredSource = fresh.Actors.Single(a => a is not PlayerPawn);
        for (var i = 0; i < 32; i++)
            Assert.Equal(source.CheckSight(target, flags), restoredSource.CheckSight(fresh.Players.Single(), flags));
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void InvalidCallsFailBeforeConsumingVisibilityRandomness()
    {
        var sim = Room(); var source = sim.Actors.Single(a => a is not PlayerPawn); var target = sim.Players.Single();
        target.Invisible = true;
        Assert.Throws<ArgumentNullException>(() => source.CheckSight(null!));
        Assert.Throws<NotSupportedException>(() => source.CheckSight(target, (ActorSightFlags)8));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
        Assert.Throws<InvalidOperationException>(() => new Actor().CheckSight(target));
    }

    private static AuthoritySimulation Room(bool blocking = false) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = blocking ? [new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 1,
            Flags = LevelLine.TwoSidedFlag | LevelLine.BlockEverythingFlag }] : [],
        Things = [new LevelThing { Type = 1, X = 64 }, new LevelThing { Type = 3004, X = -64 }]
    });
}
