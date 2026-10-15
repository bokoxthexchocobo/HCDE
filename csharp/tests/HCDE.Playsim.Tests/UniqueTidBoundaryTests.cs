using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class UniqueTidBoundaryTests
{
    [Theory]
    [InlineData(int.MaxValue, 1)]
    [InlineData(int.MaxValue, 5)]
    [InlineData(int.MaxValue - 1, 5)]
    public void OverflowingSearchRangeClampsWithoutLosingFreeTid(int start, int span)
    {
        var sim = Room(); Assert.Equal(start, AcsActorTid.FindUniqueTid(sim, start, span));
    }

    [Fact]
    public void OccupiedMaximumTerminatesWithoutWrapping()
    {
        var sim = Room(); sim.Players.Single().ThingId = int.MaxValue;
        Assert.Equal(0, AcsActorTid.FindUniqueTid(sim, int.MaxValue, 5));
        Assert.Equal(int.MaxValue - 1, AcsActorTid.FindUniqueTid(sim, int.MaxValue - 1, 5));
        sim.AddBot(80, 0, thingId: int.MaxValue - 1);
        Assert.Equal(0, AcsActorTid.FindUniqueTid(sim, int.MaxValue - 1, 5));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 11)]
    public void OrdinarySearchRespectsSpanAndOccupiedIds(int span, int expected)
    {
        var sim = Room(); sim.Players.Single().ThingId = 10;
        Assert.Equal(expected, AcsActorTid.FindUniqueTid(sim, 10, span));
    }

    [Fact]
    public void AcsNegativeSpanUsesSameRandomSearchAsZero()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); var negative = Query(sim, 0, -1);
        SimSavegame.Apply(sim, saved); Assert.Equal(negative, Query(sim, 0, 0)); Assert.True(negative > 0);
    }

    [Fact]
    public void AcsBoundarySearchReturnsMaximumFreeTid()
    {
        Assert.Equal(int.MaxValue, Query(Room(), int.MaxValue, 5));
    }

    private static int Query(AuthoritySimulation sim, int start, int span)
    {
        var stack = new List<int> { 77, start, span };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(), [], new(), AcsCallFunctions.UniqueTid, 2, out var result));
        Assert.Equal(new[] { 77 }, stack); return result;
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
