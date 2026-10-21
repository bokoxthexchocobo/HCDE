namespace HCDE.Playsim.Tests;

public class VmPMapUtlMultiBlockThingsIteratorTests
{
    [Fact]
    public void Next_ReturnsThingWithDisplacement()
    {
        var thing = new object();
        var ctx = new VmPMapUtlMultiBlockThingsIterator.MultiBlockThingsContext
        {
            NextThing = () => thing,
            ThingPortalGroup = _ => 2,
            DisplacementOffset = (_, g) => g == 2 ? (5, 6) : (0, 0),
        };
        ctx.State.Checklist = new[] { 1 };
        ctx.State.BaseGroup = 1;
        ctx.State.CheckpointX = 10;
        ctx.State.CheckpointY = 20;
        var item = new VmPMapUtlMultiBlockThingsIterator.CheckResult();
        Assert.True(VmPMapUtlMultiBlockThingsIterator.Next(ctx, item));
        Assert.Equal(15, item.PosX);
        Assert.Equal(26, item.PosY);
    }

    [Fact]
    public void RoughMonsterSearch_InvokesBlockmapSearch()
    {
        var called = false;
        var ctx = new VmPMapUtlMultiBlockThingsIterator.RoughMonsterSearchContext
        {
            FrontOnly = true,
            OriginX = 1,
            OriginY = 2,
            YawSin = 0,
            YawCos = 1,
            BlockmapSearch = d =>
            {
                called = true;
                Assert.Equal(4, d);
                return new object();
            },
        };
        Assert.NotNull(VmPMapUtlMultiBlockThingsIterator.RoughMonsterSearch(ctx, 4));
        Assert.True(called);
    }
}
