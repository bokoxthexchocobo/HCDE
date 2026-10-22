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

    [Fact]
    public void PlaneCtor_AttachesAndRegisters()
    {
        var plane = new VmRInterpolatorCore.SectorPlaneInterp { Ceiling = true };
        var midtex = 0;
        var linked = 0;
        var registered = false;
        plane.FinishConstruction(
            new object(),
            true,
            () => 0,
            () => 0,
            () => 0,
            () => 0,
            (_, _, _) => midtex++,
            (_, _, _) => linked++,
            _ => registered = true);
        Assert.Equal(1, midtex);
        Assert.Equal(1, linked);
        Assert.True(registered);
    }

    [Fact]
    public void PlaneUnlink_ClearsFloorSlotAndAttached()
    {
        var plane = new VmRInterpolatorCore.SectorPlaneInterp { Ceiling = false };
        plane.Attached.Add(new object());
        int? clearedSlot = null;
        var delRefs = 0;
        plane.UnlinkPlaneFromMap(
            (slot, _) => clearedSlot = slot,
            _ => delRefs++,
            () => { });
        Assert.Equal(VmRInterpolatorCore.SectorPlaneSlotFloorMove, clearedSlot);
        Assert.Equal(1, delRefs);
        Assert.Null(plane.Sector);
        Assert.Empty(plane.Attached);
    }

    [Fact]
    public void PlanePropagateMark_WalksAttached()
    {
        var attached = new List<object> { new(), new() };
        var marks = 0;
        VmRInterpolatorCore.SectorPlaneInterp.PropagateMarkAttached(attached, _ => marks++, () => 7);
        Assert.Equal(2, marks);
    }

    [Fact]
    public void PlaneSerialize_CapturesFields()
    {
        var sector = new object();
        var attached = new List<object> { new() };
        var fields = VmRInterpolatorCore.SectorPlaneInterp.CaptureSerializeFields(sector, true, 1.5, 2.5, attached);
        Assert.Same(sector, fields.Sector);
        Assert.True(fields.Ceiling);
        Assert.Equal(1.5, fields.OldHeight);
        Assert.Same(attached, fields.Attached);
    }
}
