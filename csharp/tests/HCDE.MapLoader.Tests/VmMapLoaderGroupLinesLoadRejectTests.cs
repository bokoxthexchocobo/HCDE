namespace HCDE.MapLoader.Tests;

public class VmMapLoaderGroupLinesTests
{
    [Fact]
    public void ShouldFlagMissingFrontSector()
    {
        Assert.True(VmMapLoaderGroupLines.ShouldFlagMissingFrontSector(true, false));
    }

    [Fact]
    public void ComputeBoundingBoxCenter_Midpoint()
    {
        var (x, y) = VmMapLoaderGroupLines.ComputeBoundingBoxCenter(
            new[] { (0.0, 0.0), (10.0, 20.0) });
        Assert.Equal(5, x);
        Assert.Equal(10, y);
    }

    [Fact]
    public void AssignLinesToSectors_Balanced()
    {
        var done = VmMapLoaderGroupLines.AssignLinesToSectors(
            2,
            new[] { 1, 1 },
            new List<(int, int, bool, bool)> { (0, 1, true, true) });
        Assert.True(VmMapLoaderGroupLines.AllSectorsBalanced(done, new[] { 1, 1 }));
    }

    [Fact]
    public void ShouldInvokeSetSlopes_WhenNotBuildMap()
    {
        Assert.True(VmMapLoaderGroupLines.ShouldInvokeSetSlopes(false));
    }
}

public class VmMapLoaderLoadRejectTests
{
    [Fact]
    public void ComputeNeededRejectBytes_ForFourSectors()
    {
        Assert.Equal(2, VmMapLoaderLoadReject.ComputeNeededRejectBytes(4));
    }

    [Fact]
    public void IsRejectMatrixAllZero_DetectsNonZeroTail()
    {
        var bytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 1 };
        Assert.False(VmMapLoaderLoadReject.IsRejectMatrixAllZero(bytes));
    }

    [Fact]
    public void ShouldClearReject_WhenLumpTooSmall()
    {
        Assert.True(VmMapLoaderLoadReject.ShouldClearReject(true, 1, 8));
    }
}
