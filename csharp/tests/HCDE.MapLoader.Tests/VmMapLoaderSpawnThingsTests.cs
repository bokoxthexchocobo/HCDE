namespace HCDE.MapLoader.Tests;

public class VmMapLoaderSpawnThingsTests
{
    [Fact]
    public void IsWritableUserField_RejectsNativeOrStatic()
    {
        Assert.False(VmMapLoaderSpawnThings.IsWritableUserField(true, false, false, false, true));
    }

    [Fact]
    public void EnumerateUserDataChain_StopsAtEmptyKey()
    {
        var table = new[]
        {
            new VmMapLoaderSpawnThings.UserDataEntry("hp", 100, 0, ""),
            new VmMapLoaderSpawnThings.UserDataEntry("", 0, 0, ""),
        };
        Assert.Single(VmMapLoaderSpawnThings.EnumerateUserDataChain(table, 0));
    }
}

public class VmMapLoaderAllocateSideDefsTests
{
    [Fact]
    public void ShouldReportUnusedSidedefs_WhenLumpHasExtra()
    {
        Assert.True(VmMapLoaderAllocateSideDefs.ShouldReportUnusedSidedefs(10, 7));
    }

    [Fact]
    public void ResolveSideLineAttachment_PicksVertexBySide()
    {
        var (side, vert) = VmMapLoaderAllocateSideDefs.ResolveSideLineAttachment(true, 1, 2);
        Assert.Equal(1, side);
        Assert.Equal(2, vert);
    }

    [Fact]
    public void ShouldWarnUnconnectedRightEdge_OnlyOnFirstLoop()
    {
        Assert.True(VmMapLoaderAllocateSideDefs.ShouldWarnUnconnectedRightEdge(true, false));
        Assert.False(VmMapLoaderAllocateSideDefs.ShouldWarnUnconnectedRightEdge(false, false));
    }
}
