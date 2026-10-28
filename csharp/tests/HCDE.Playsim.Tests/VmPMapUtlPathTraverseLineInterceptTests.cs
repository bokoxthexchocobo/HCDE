namespace HCDE.Playsim.Tests;

public class VmPMapUtlPathTraverseLineInterceptTests
{
    [Fact]
    public void AddLineIntercepts_RecordsCrossingLine()
    {
        var ctx = new VmPMapUtlPathTraverseLineIntercept.AddLineInterceptsContext
        {
            Trace = new VmPMapUtlPathTraverseLineIntercept.Divline(0, 0, 10, 0),
            Lines =
            [
                new VmPMapUtlPathTraverseLineIntercept.LineSegment(1, 5, -1, 5, 1),
            ],
            PointOnDivlineSide = (x, y, _) => y < 0 ? -1 : 1,
            InterceptVector = (_, dl) => (5 - dl.X) / dl.Dx,
            MakeDivline = ld => new VmPMapUtlPathTraverseLineIntercept.Divline(ld.V1X, ld.V1Y, ld.V2X - ld.V1X, ld.V2Y - ld.V1Y),
        };
        VmPMapUtlPathTraverseLineIntercept.AddLineIntercepts(ctx);
        Assert.Single(ctx.Intercepts);
        Assert.Equal(1, ctx.Intercepts[0].LineId);
    }

    [Fact]
    public void Next_ReturnsNearestUndoneIntercept()
    {
        var list = new List<VmPMapUtlPathTraverseLineIntercept.LineIntercept>
        {
            new() { Frac = 0.8, LineId = 1 },
            new() { Frac = 0.2, LineId = 2 },
        };
        var idx = 0;
        var first = VmPMapUtlPathTraverseLineIntercept.Next(list, ref idx);
        Assert.Equal(2, first?.LineId);
        var second = VmPMapUtlPathTraverseLineIntercept.Next(list, ref idx);
        Assert.Equal(1, second?.LineId);
    }
}
