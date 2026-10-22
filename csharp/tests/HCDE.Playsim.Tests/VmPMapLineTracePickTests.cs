namespace HCDE.Playsim.Tests;

public class VmPMapLineTracePickTests
{
    [Fact]
    public void CheckLineTrace_SkipsPortalCrossing()
    {
        var data = new VmPMapLineTracePick.CheckLineData();
        Assert.Equal(VmPMapLineTracePick.ETraceStatus.Skip,
            VmPMapLineTracePick.CheckLineTrace(new VmPMapLineTracePick.TraceResults
            {
                HitType = VmPMapLineTrace.TraceCrossingPortal,
            }, data));
        Assert.Equal(1, data.NumPortals);
    }

    [Fact]
    public void LineAttackByPuffName_PrintsWhenMissing()
    {
        var printed = false;
        var ctx = new VmPMapLineTracePick.LineAttackContext
        {
            PuffTypeName = "Missing",
            FindActorClass = _ => null,
            PrintUnknown = _ => printed = true,
        };
        Assert.Null(VmPMapLineTracePick.LineAttackByPuffName(ctx));
        Assert.True(printed);
    }

    [Fact]
    public void LineTrace_FillsOutdata()
    {
        var outdata = new VmPMapLineTracePick.LineTraceData();
        VmPMapLineTracePick.LineTrace(new VmPMapLineTracePick.LineTraceContext
        {
            Flags = VmPMapLineTrace.TrfNosky,
            Distance = 100,
            PitchCos = 1,
            AngleCos = 1,
            StartZ = 32,
            Trace = req =>
            {
                req.Trace.HitType = VmPMapLineTrace.TraceCrossingPortal;
                VmPMapLineTracePick.CheckLineTrace(req.Trace, req.CallbackData);
                req.Trace.HitType = VmPMapLineTrace.TraceHitActor;
                req.Trace.Actor = new object();
                req.Trace.Distance = 50;
                return true;
            },
        }, outdata);
        Assert.Equal(-1, outdata.NumPortals);
        Assert.NotNull(outdata.HitActor);
        Assert.Equal(50, outdata.Distance);
    }

    [Fact]
    public void LinePickActor_ReturnsHitActor()
    {
        var hit = new object();
        var actor = VmPMapLineTracePick.LinePickActor(new VmPMapLineTracePick.LinePickContext
        {
            PitchCos = 1,
            AngleCos = 1,
            Center = 40,
            Distance = 64,
            ActorMask = VmPMapLineTrace.MfShootable,
            Trace = (_, _, _, _, _, _, _, _, _, trace, _, _, _) =>
            {
                trace.HitType = VmPMapLineTrace.TraceHitActor;
                trace.Actor = hit;
                return true;
            },
        });
        Assert.Same(hit, actor);
    }
}
