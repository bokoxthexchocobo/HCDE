namespace HCDE.MapLoader.Tests;

public class VmMapLoaderProcessSideTexturesTests
{
    [Fact]
    public void ResolveSpecialDispatch_TransferHeights()
    {
        Assert.Equal(1, VmMapLoaderProcessSideTextures.ResolveSpecialDispatch(209, true));
    }

    [Fact]
    public void IsTranMapLumpName_MatchesPrefix()
    {
        Assert.True(VmMapLoaderProcessSideTextures.IsTranMapLumpName("TRANMAP1"));
    }
}

public class VmMapLoaderLoadBlockMapTests
{
    [Fact]
    public void ShouldGenerateBlockMap_WhenLumpEmpty()
    {
        Assert.True(VmMapLoaderLoadBlockMap.ShouldGenerateBlockMap(false, false, 0, false, 10));
    }

    [Fact]
    public void ExpandWadOffset_NegativeOneBecomesMaxUInt()
    {
        Assert.Equal(unchecked((int)0xffffffff), VmMapLoaderLoadBlockMap.ExpandWadOffset(-1));
    }

    [Fact]
    public void IsLineIndexOutOfRange_RejectsHighIndex()
    {
        Assert.True(VmMapLoaderLoadBlockMap.IsLineIndexOutOfRange(99, 10));
    }

    [Fact]
    public void RequiresLeadingZeroEntry_RejectsNonZero()
    {
        Assert.True(VmMapLoaderLoadBlockMap.RequiresLeadingZeroEntry(1));
    }
}

public class VmMapLoaderCreateBlockMapTests
{
    [Fact]
    public void IsSingleBlock_WhenSameCell()
    {
        Assert.True(VmMapLoaderCreateBlockMap.IsSingleBlock(1, 2, 1, 2));
    }

    [Fact]
    public void IsHorizontalSpan_WhenSameRow()
    {
        Assert.True(VmMapLoaderCreateBlockMap.IsHorizontalSpan(3, 3));
    }
}

public class VmMapLoaderLoadSideDefs2Tests
{
    [Fact]
    public void ShouldPatchStrifeLaserRow_ForStrifeGame()
    {
        Assert.True(VmMapLoaderLoadSideDefs2.ShouldPatchStrifeLaserRow(1, "LASERB01", 1));
    }
}

public class VmMapLoaderLoopSidedefsRightEdgeTests
{
    [Fact]
    public void IsBetterAngle_RejectsZero()
    {
        Assert.False(VmMapLoaderLoopSidedefsRightEdge.IsBetterAngle(0, 90));
    }
}
