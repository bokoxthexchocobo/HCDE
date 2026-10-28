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
    public void ClearNodeCache_DeletesTopLevelCacheEntries()
    {
        var cacheRoot = Path.Combine(Path.GetTempPath(), "hcde-nodecache-" + Guid.NewGuid().ToString("N"));
        var nodesDir = Path.Combine(cacheRoot, "doom", "nodes");
        Directory.CreateDirectory(nodesDir);
        var fileA = Path.Combine(nodesDir, "a.dat");
        var fileB = Path.Combine(nodesDir, "b.dat");
        File.WriteAllText(fileA, "x");
        File.WriteAllText(fileB, "y");

        var priorCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        Environment.SetEnvironmentVariable("XDG_CACHE_HOME", cacheRoot);
        try
        {
            VmMapLoaderGlRenderSector.ClearNodeCache();
            Assert.False(File.Exists(fileA));
            Assert.False(File.Exists(fileB));
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", priorCache);
            try
            {
                Directory.Delete(cacheRoot, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    [Fact]
    public void ClearNodeCache_NoThrowWhenCacheDirectoryMissing()
    {
        var cacheRoot = Path.Combine(Path.GetTempPath(), "hcde-nodecache-missing-" + Guid.NewGuid().ToString("N"));
        var priorCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        Environment.SetEnvironmentVariable("XDG_CACHE_HOME", cacheRoot);
        try
        {
            VmMapLoaderGlRenderSector.ClearNodeCache();
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", priorCache);
        }
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
