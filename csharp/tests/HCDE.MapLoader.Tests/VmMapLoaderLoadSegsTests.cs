namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLoadSegsTests
{
    [Fact]
    public void ShouldAbortEmptySegs_WhenZero()
    {
        Assert.True(VmMapLoaderLoadSegs.ShouldAbortEmptySegs(0));
        Assert.False(VmMapLoaderLoadSegs.ShouldAbortEmptySegs(10));
    }

    [Fact]
    public void ValidateVertexIndex_ThrowsWhenOutOfRange()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapLoaderLoadSegs.ValidateVertexIndex(5, 5, 0));
    }

    [Fact]
    public void CountSegs_DividesLumpByRecordSize()
    {
        Assert.Equal(4, VmMapLoaderLoadSegs.CountSegs(48, 12));
    }

    [Fact]
    public void ShouldRejectZeroSubsectors()
    {
        Assert.True(VmMapLoaderLoadSegs.ShouldRejectZeroSubsectors(0));
    }

    [Fact]
    public void ShouldSnapSplitVertex_AtOneDegree()
    {
        Assert.True(VmMapLoaderLoadSegs.ShouldSnapSplitVertex(1.0));
        Assert.False(VmMapLoaderLoadSegs.ShouldSnapSplitVertex(0.5));
    }

    [Fact]
    public void IsSubsectorSegRangeInvalid_DetectsOverflow()
    {
        Assert.True(VmMapLoaderLoadSegs.IsSubsectorSegRangeInvalid(10, 5, 12));
    }
}
