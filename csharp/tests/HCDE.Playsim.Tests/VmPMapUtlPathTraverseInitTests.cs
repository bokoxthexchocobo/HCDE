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
}
