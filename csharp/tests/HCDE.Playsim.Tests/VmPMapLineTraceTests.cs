namespace HCDE.Playsim.Tests;

public class VmPMapLineTraceTests
{
    [Fact]
    public void PuffTypeMissing_WhenNotFound() =>
        Assert.True(VmPMapLineTrace.PuffTypeMissing(false));

    [Fact]
    public void CheckLineTrace_SkipsPortal() =>
        Assert.Equal(VmPMapLineTrace.ETraceStatus.Skip,
            VmPMapLineTrace.CheckLineTraceHit(VmPMapLineTrace.TraceCrossingPortal, false, false, false));

    [Fact]
    public void CheckLineTrace_SkipsThruSpecies() =>
        Assert.Equal(VmPMapLineTrace.ETraceStatus.Skip,
            VmPMapLineTrace.CheckLineTraceHit(VmPMapLineTrace.TraceHitActor, false, true, true));

    [Fact]
    public void BuildLineFlags_ClearsBlockOnThrublock() =>
        Assert.Equal(0, VmPMapLineTrace.BuildLineFlags(VmPMapLineTrace.TrfThrublock, false, true, false) & VmPMapLineTrace.MlBlockeverything);

    [Fact]
    public void PortalCount_SubtractsTwo() =>
        Assert.Equal(3, VmPMapLineTrace.PortalCountAdjust(5));

    [Fact]
    public void LinePick_ReturnsActorOnHit() =>
        Assert.True(VmPMapLineTrace.LinePickHitActor(VmPMapLineTrace.TraceHitActor));
}
