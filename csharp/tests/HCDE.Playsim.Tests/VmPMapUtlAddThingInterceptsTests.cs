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
}
