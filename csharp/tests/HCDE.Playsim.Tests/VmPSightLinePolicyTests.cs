namespace HCDE.Playsim.Tests;

public class VmPSightLinePolicyTests
{
    [Fact]
    public void SightOpening_OneSidedSetsZeroRange()
    {
        var open = new VmPSightLinePolicy.SightOpeningState();
        VmPSightLinePolicy.SightOpening(open, oneSided: true, lineFlags: 0,
            frontCeilPortalOpen: false, frontFloorPortalOpen: false,
            backCeilPortalOpen: false, backFloorPortalOpen: false,
            frontCeilZ: 128, frontFloorZ: 0, backCeilZ: 0, backFloorZ: 0);
        Assert.Equal(0, open.Range);
    }

    [Fact]
    public void SightOpening_TwoSidedRangeIsOpenWhenBottomBelowTop()
    {
        var open = new VmPSightLinePolicy.SightOpeningState();
        VmPSightLinePolicy.SightOpening(open, oneSided: false, lineFlags: 0,
            frontCeilPortalOpen: false, frontFloorPortalOpen: false,
            backCeilPortalOpen: false, backFloorPortalOpen: false,
            frontCeilZ: 128, frontFloorZ: 32, backCeilZ: 96, backFloorZ: 0);
        Assert.Equal(1, open.Range);
        Assert.Equal(32, open.Bottom);
        Assert.Equal(96, open.Top);
    }

    [Fact]
    public void LineBlocksSight_BlocksOneSided()
    {
        Assert.True(VmPSightLinePolicy.LineBlocksSight(
            hasBacksector: false, lineFlags: VmPSightLinePolicy.MlTwosided,
            sightFlags: 0, activation: 0, special: 0, arg1: 0, levelNum: 1));
    }

    [Fact]
    public void LineBlocksSight_AllowsOpenTwoSided()
    {
        Assert.False(VmPSightLinePolicy.LineBlocksSight(
            hasBacksector: true, lineFlags: VmPSightLinePolicy.MlTwosided,
            sightFlags: 0, activation: 0, special: 0, arg1: 0, levelNum: 1));
    }

    [Fact]
    public void SightCheckLine_SkipsParallelTrace()
    {
        var ctx = new VmPSightLinePolicy.SightCheckLineContext
        {
            ValidCount = 1,
            TraceX = 0,
            TraceY = 0,
            TraceDx = 10,
            TraceDy = 0,
            EndpointSideOfTrace = (x, _) => x < 5 ? 0 : 1,
        };
        Assert.True(VmPSightLinePolicy.SightCheckLine(ctx, line: 1, v1x: 0, v1y: 0, v2x: 0, v2y: 10));
        Assert.Empty(ctx.InterceptLines);
    }

    [Fact]
    public void SightCheckLine_EnqueuesCrossingLine()
    {
        var ctx = new VmPSightLinePolicy.SightCheckLineContext
        {
            ValidCount = 1,
            TraceX = 0,
            TraceY = 0,
            TraceDx = 10,
            TraceDy = 0,
            EndpointSideOfTrace = (x, _) => x < 5 ? 0 : 1,
            TraceSideOfDivline = (x, _) => x < 5 ? 0 : 1,
            LineBlocksSight = _ => false,
        };
        Assert.True(VmPSightLinePolicy.SightCheckLine(ctx, line: 42, v1x: 2, v1y: -5, v2x: 8, v2y: 5));
        Assert.Single(ctx.InterceptLines);
    }

    [Fact]
    public void SightBlockLinesIterator_EarlyOutWithoutPortals()
    {
        var ctx = new VmPSightLinePolicy.SightBlockLinesIteratorContext
        {
            BlockWidth = 8,
            MapLines = _ => new object[] { 1 },
            SightCheckLine = _ => false,
        };
        Assert.Equal(0, VmPSightLinePolicy.SightBlockLinesIterator(ctx, 2, 3));
    }

    [Fact]
    public void SightBlockLinesIterator_DeferredFailWithPortals()
    {
        var ctx = new VmPSightLinePolicy.SightBlockLinesIteratorContext
        {
            BlockWidth = 8,
            BlockHasLinkedPortals = (_, _) => true,
            MapLines = _ => new object[] { 1 },
            SightCheckLine = _ => false,
        };
        Assert.Equal(-1, VmPSightLinePolicy.SightBlockLinesIterator(ctx, 1, 1));
    }
}
