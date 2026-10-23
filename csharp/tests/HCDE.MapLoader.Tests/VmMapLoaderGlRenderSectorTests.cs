namespace HCDE.MapLoader.Tests;

public class VmMapLoaderGlRenderSectorTests
{
    [Fact]
    public void PointOnLine_TrueWhenOnPartition()
    {
        Assert.True(VmMapLoaderGlRenderSector.PointOnLine(50, 0, 0, 0, 100, 0));
    }

    [Fact]
    public void PointOnLine_FalseWhenFarFromLine()
    {
        Assert.False(VmMapLoaderGlRenderSector.PointOnLine(0, 100, 0, 0, 100, 0));
    }

    [Fact]
    public void ClearNodeCache_DeletesInReverseOrder()
    {
        var order = new List<string>();
        var listing = new[]
        {
            new VmMapLoaderGlRenderSector.CacheEntry { FilePath = "a", IsDirectory = false },
            new VmMapLoaderGlRenderSector.CacheEntry { FilePath = "b", IsDirectory = true },
        };

        var result = VmMapLoaderGlRenderSector.ClearNodeCache(
            listing,
            path => order.Add("dir:" + path),
            path => order.Add("file:" + path));

        Assert.Equal(VmMapLoaderGlRenderSector.ClearNodeCacheResult.Cleared, result);
        Assert.Equal(new[] { "dir:b", "file:a" }, order);
    }

    [Fact]
    public void ClearNodeCache_ScanFailedWhenListingNull()
    {
        Assert.Equal(
            VmMapLoaderGlRenderSector.ClearNodeCacheResult.ScanFailed,
            VmMapLoaderGlRenderSector.ClearNodeCache(null));
    }

    [Fact]
    public void SetRenderSector_PicksSectorFromBoundarySeg()
    {
        var sector = new VmMapLoaderGlRenderSector.RenderSector { Index = 1 };
        var level = new VmMapLoaderGlRenderSector.SetRenderSectorLevel();
        var sub = new VmMapLoaderGlRenderSector.RenderSubsector { NumLines = 1, FirstSegIndex = 0 };
        level.Subsectors.Add(sub);

        var seg = new VmMapLoaderGlRenderSector.RenderSeg
        {
            V1X = 0,
            V1Y = 0,
            V2X = 10,
            V2Y = 0,
            HasLinedef = true,
            Sidedef = new VmMapLoaderGlRenderSector.RenderSide { Sector = sector },
            Subsector = sub,
        };
        level.Segs.Add(seg);

        VmMapLoaderGlRenderSector.SetRenderSector(level);
        Assert.Equal(sector, sub.RenderSector);
        Assert.NotEqual(0u, sub.Flags & VmMapLoaderGlRenderSector.SsecfDegenerate);
    }

    [Fact]
    public void SetRenderSector_FallsBackToRealSectorWhenUndetermined()
    {
        var real = new VmMapLoaderGlRenderSector.RenderSector { Index = 2 };
        var level = new VmMapLoaderGlRenderSector.SetRenderSectorLevel();
        var sub = new VmMapLoaderGlRenderSector.RenderSubsector
        {
            NumLines = 1,
            FirstSegIndex = 0,
            Sector = real,
        };
        level.Subsectors.Add(sub);
        level.Segs.Add(new VmMapLoaderGlRenderSector.RenderSeg
        {
            V1X = 0,
            V1Y = 0,
            V2X = 0,
            V2Y = 10,
            HasLinedef = false,
            Subsector = sub,
        });

        VmMapLoaderGlRenderSector.SetRenderSector(level);
        Assert.Equal(real, sub.RenderSector);
    }
}
