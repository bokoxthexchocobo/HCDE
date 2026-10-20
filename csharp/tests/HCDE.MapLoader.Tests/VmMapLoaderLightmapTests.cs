namespace HCDE.MapLoader.Tests;

public class VmMapLoaderLightmapTests
{
    [Fact]
    public void ShouldSkipLoad_WhenNolightmaps()
    {
        Assert.True(VmMapLoaderLightmap.ShouldSkipLoad(true, 100));
    }

    [Fact]
    public void ComputeTextureByteCount_MatchesFormula()
    {
        Assert.Equal(6u, VmMapLoaderLightmap.ComputeTextureByteCount(1, 1));
    }

    [Fact]
    public void CompareLightProbesByCell_SortsYThenX()
    {
        var a = new VmMapLoaderLightmap.LightProbe(1, 0);
        var b = new VmMapLoaderLightmap.LightProbe(0, 1);
        Assert.True(VmMapLoaderLightmap.CompareLightProbesByCell(a, b, 1.0) < 0);
    }

    [Fact]
    public void ResolveSideLightmapSlot_MiddleDuplicates()
    {
        Assert.True(VmMapLoaderLightmap.ShouldDuplicateMiddleWallSlots(VmMapLoaderLightmap.SurfaceMiddleWall));
    }

    [Fact]
    public void ShouldApplySubsectorSurface_FloorOrCeiling()
    {
        Assert.True(VmMapLoaderLightmap.ShouldApplySubsectorSurface(VmMapLoaderLightmap.SurfaceFloor));
    }
}
