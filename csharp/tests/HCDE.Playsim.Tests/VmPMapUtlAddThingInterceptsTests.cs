namespace HCDE.Playsim.Tests;

public class VmPMapUtlAddThingInterceptsTests
{
    [Fact]
    public void AddThingIntercepts_StartInsideBoxPushesZeroFrac()
    {
        var ctx = new VmPMapUtlAddThingIntercepts.AddThingInterceptsContext
        {
            Trace = new VmPMapUtlAddThingIntercepts.Divline(0, 0, 100, 0),
            StartFrac = 0,
            Things = new[] { new VmPMapUtlAddThingIntercepts.ThingBox(0, 0, 32) },
            PointOnDivlineSide = (_, _, _) => 0,
            InterceptVector = (t, l) => VmPMapUtlLineOpening.InterceptVector(
                new VmPMapUtlLineOpening.Divline(l.X, l.Y, l.Dx, l.Dy),
                new VmPMapUtlLineOpening.Divline(t.X, t.Y, t.Dx, t.Dy)),
        };
        VmPMapUtlAddThingIntercepts.AddThingIntercepts(ctx, compatible: false);
        Assert.Single(ctx.Intercepts);
        Assert.Equal(0, ctx.Intercepts[0].Frac);
    }

    [Fact]
    public void AddThingIntercepts_CompatibleDiagonalRecordsFrac()
    {
        var ctx = new VmPMapUtlAddThingIntercepts.AddThingInterceptsContext
        {
            Trace = new VmPMapUtlAddThingIntercepts.Divline(0, 0, 100, 100),
            StartFrac = 0,
            Things = new[] { new VmPMapUtlAddThingIntercepts.ThingBox(50, 50, 10) },
            PointOnDivlineSide = (x, y, trace) =>
                (y - trace.Y) * trace.Dx > (x - trace.X) * trace.Dy ? 1 : 0,
            InterceptVector = (t, l) => VmPMapUtlLineOpening.InterceptVector(
                new VmPMapUtlLineOpening.Divline(l.X, l.Y, l.Dx, l.Dy),
                new VmPMapUtlLineOpening.Divline(t.X, t.Y, t.Dx, t.Dy)),
        };
        VmPMapUtlAddThingIntercepts.AddThingIntercepts(ctx, compatible: true);
        Assert.Single(ctx.Intercepts);
        Assert.InRange(ctx.Intercepts[0].Frac, 0.4, 0.6);
    }
}
