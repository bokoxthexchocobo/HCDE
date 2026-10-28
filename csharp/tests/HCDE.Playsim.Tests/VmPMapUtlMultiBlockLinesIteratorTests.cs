namespace HCDE.Playsim.Tests;

public class VmPMapUtlMultiBlockLinesIteratorTests
{
    [Fact]
    public void Next_ReturnsLineFromBlockIterator()
    {
        var ctx = new VmPMapUtlMultiBlockLinesIterator.MultiBlockLinesContext
        {
            NextBlockLine = () => new object(),
        };
        ctx.State.Checklist = new[] { 1 };
        ctx.State.BaseGroup = 1;
        ctx.State.StartSector = new object();
        var item = new VmPMapUtlMultiBlockLinesIterator.CheckResult();
        Assert.True(VmPMapUtlMultiBlockLinesIterator.Next(ctx, item));
        Assert.NotNull(item.Line);
    }

    [Fact]
    public void Next_AdvancesPortalGroupWhenBlockIteratorExhausted()
    {
        var phase = 0;
        var ctx = new VmPMapUtlMultiBlockLinesIterator.MultiBlockLinesContext
        {
            NextBlockLine = () => ++phase == 3 ? new object() : null,
            SectorPortalGroup = _ => 1,
            DisplacementOffset = (_, _) => (0, 0),
            PointInSector = (_, _) => new object(),
            InitBlockIteratorBox = (_, _, _) => { },
        };
        ctx.State.Checklist = new[] { 1, 2 };
        ctx.State.BaseGroup = 1;
        ctx.State.StartSector = new object();
        var item = new VmPMapUtlMultiBlockLinesIterator.CheckResult();
        Assert.True(VmPMapUtlMultiBlockLinesIterator.Next(ctx, item));
        Assert.Equal(1, ctx.State.Index);
    }

    [Fact]
    public void GoUp_StartsUpperPortalGroup()
    {
        var ctx = new VmPMapUtlMultiBlockLinesIterator.MultiBlockLinesContext
        {
            PortalBlocksMovement = (_, _) => false,
            OppositePortalGroup = (_, _) => 3,
            SectorPortalGroup = s => 3,
            DisplacementOffset = (_, _) => (0, 0),
            PointInSector = (_, _) => new object(),
            InitBlockIteratorBox = (_, _, _) => { },
        };
        ctx.State.CurSector = new object();
        Assert.True(VmPMapUtlMultiBlockLinesIterator.GoUp(ctx, 0, 0));
        Assert.Equal(VmPMapUtlMultiBlockLinesIterator.FfcfNoFloor, ctx.State.PortalFlags);
    }

    [Fact]
    public void GoDown_StartsLowerPortalGroup()
    {
        var ctx = new VmPMapUtlMultiBlockLinesIterator.MultiBlockLinesContext
        {
            PortalBlocksMovement = (_, _) => false,
            OppositePortalGroup = (_, _) => 4,
            SectorPortalGroup = s => 4,
            DisplacementOffset = (_, _) => (0, 0),
            PointInSector = (_, _) => new object(),
            InitBlockIteratorBox = (_, _, _) => { },
        };
        ctx.State.CurSector = new object();
        Assert.True(VmPMapUtlMultiBlockLinesIterator.GoDown(ctx, 0, 0));
        Assert.Equal(VmPMapUtlMultiBlockLinesIterator.FfcfNoCeiling, ctx.State.PortalFlags);
    }

    [Fact]
    public void Reset_ReinitializesPortalWalk()
    {
        var ctx = new VmPMapUtlMultiBlockLinesIterator.MultiBlockLinesContext
        {
            SectorPortalGroup = _ => 1,
            DisplacementOffset = (_, _) => (0, 0),
            PointInSector = (_, _) => new object(),
            InitBlockIteratorBox = (_, _, _) => { },
        };
        ctx.State.BaseGroup = 1;
        ctx.State.Index = 3;
        ctx.State.ContinueUp = false;
        VmPMapUtlMultiBlockLinesIterator.Reset(ctx);
        Assert.Equal(-1, ctx.State.Index);
        Assert.True(ctx.State.ContinueUp);
    }
}
