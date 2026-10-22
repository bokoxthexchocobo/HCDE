namespace HCDE.Playsim.Tests;

public class VmRInterpolatorCoreTests
{
    [Fact]
    public void CountInterpolations_WalksList()
    {
        var list = new VmRInterpolatorCore.InterpolatorList();
        var a = new VmRInterpolatorCore.InterpolationNode();
        var b = new VmRInterpolatorCore.InterpolationNode();
        list.AddInterpolation(a);
        list.AddInterpolation(b);
        Assert.Equal(2, list.CountInterpolations());
    }

    [Fact]
    public void DoInterpolations_SkipsWhenRatioOne()
    {
        var list = new VmRInterpolatorCore.InterpolatorList();
        var called = false;
        list.AddInterpolation(new VmRInterpolatorCore.InterpolationNode
        {
            Interpolate = _ => called = true
        });
        list.DoInterpolations(1.0);
        Assert.False(list.DidInterp);
        Assert.False(called);
    }

    [Fact]
    public void RemoveInterpolation_UnlinksHead()
    {
        var list = new VmRInterpolatorCore.InterpolatorList();
        var a = new VmRInterpolatorCore.InterpolationNode();
        list.AddInterpolation(a);
        list.RemoveInterpolation(a);
        Assert.Null(list.Head);
    }

    [Fact]
    public void DelRef_ForcesUnlinkAtZero()
    {
        var hit = false;
        Assert.Equal(0, VmRInterpolatorCore.DelRef(1, true, () => hit = true));
        Assert.True(hit);
    }

    [Fact]
    public void PlaneInterp_LerpsFloorHeight()
    {
        var plane = new VmRInterpolatorCore.SectorPlaneInterp { Ceiling = false, OldHeight = 10, BakHeight = 20 };
        double written = 0;
        plane.InterpolateStep(1, 0.5, () => 20, d => written = d, () => 0, (_, _) => { }, () => { });
        Assert.Equal(15, written);
    }
}
