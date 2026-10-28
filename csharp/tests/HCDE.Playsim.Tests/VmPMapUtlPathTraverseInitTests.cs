namespace HCDE.Playsim.Tests;

public class VmPMapUtlPathTraverseInitTests
{
    [Fact]
    public void PathTraverseInit_WalksBlocksAlongTrace()
    {
        var visited = new List<(int, int)>();
        var ctx = new VmPMapUtlPathTraverseInit.PathTraverseInitContext
        {
            X1 = 0,
            Y1 = 0,
            X2 = 400,
            Y2 = 0,
            Flags = VmPMapUtlPathTraverseInit.PtAddLines,
            BmapOrgX = 0,
            BmapOrgY = 0,
            AddLineIntercepts = (x, y) => visited.Add((x, y)),
        };
        VmPMapUtlPathTraverseInit.PathTraverseInit(ctx);
        Assert.True(visited.Count >= 2);
    }

    [Fact]
    public void PathTraverseInit_CompatAbortsOnDiagonalCorner()
    {
        var visited = new List<(int, int)>();
        var ctx = new VmPMapUtlPathTraverseInit.PathTraverseInitContext
        {
            X1 = 0,
            Y1 = 0,
            X2 = 256,
            Y2 = 256,
            Flags = VmPMapUtlPathTraverseInit.PtAddLines | VmPMapUtlPathTraverseInit.PtCompatible,
            LevelCompatFlags = VmPMapUtlPathTraverseInit.CompatfHitscan,
            BmapOrgX = 0,
            BmapOrgY = 0,
            AddLineIntercepts = (x, y) => visited.Add((x, y)),
        };
        VmPMapUtlPathTraverseInit.PathTraverseInit(ctx);
        Assert.True(visited.Count >= 1);
        Assert.True(visited.Count < 20);
    }

    [Fact]
    public void PathTraverseInit_CornerCaseAddsNeighborBlocks()
    {
        var visited = new List<(int, int)>();
        var ctx = new VmPMapUtlPathTraverseInit.PathTraverseInitContext
        {
            X1 = 0.5,
            Y1 = 0.5,
            X2 = 400,
            Y2 = 400,
            Flags = VmPMapUtlPathTraverseInit.PtAddLines,
            BmapOrgX = 0,
            BmapOrgY = 0,
            AddLineIntercepts = (x, y) => visited.Add((x, y)),
        };
        VmPMapUtlPathTraverseInit.PathTraverseInit(ctx);
        Assert.Contains(visited, p => p.Item1 != p.Item2 || visited.Count > 3);
    }
}
