namespace HCDE.Playsim.Tests;

public class VmADecalBaseTests
{
    [Fact]
    public void ConstructFromZ_ClearsPicnum()
    {
        var d = new VmADecalBase.DecalState();
        VmADecalBase.ConstructFromZ(d, 12.5);
        Assert.Equal(12.5, d.Z);
        Assert.Equal(VmADecalBase.InvalidTexture, d.PicNum);
    }

    [Fact]
    public void GetXY_InterpolatesAlongWall()
    {
        var d = new VmADecalBase.DecalState { LeftDistance = 0.5 };
        VmADecalBase.GetXY(d, true, 0, 0, 10, 0, out var ox, out var oy);
        Assert.Equal(5, ox);
        Assert.Equal(0, oy);
    }

    [Fact]
    public void CalcFracPos_UsesDominantAxis()
    {
        var d = new VmADecalBase.DecalState();
        VmADecalBase.CalcFracPos(d, true, 5, 1, 0, 0, 10, 0);
        Assert.Equal(0.5, d.LeftDistance);
    }

    [Fact]
    public void GetRealZ_AbsoluteWhenNoRelFlags() =>
        Assert.Equal(3.0, VmADecalBase.GetRealZ(
            new VmADecalBase.DecalState { Z = 3 },
            true, 0, (_, _) => 0, (_, _) => 0, new object(), null));

    [Fact]
    public void GetRealZ_RelUpperAddsCeilingPlane()
    {
        var d = new VmADecalBase.DecalState { Z = 1, RenderFlags = VmADecalBase.RfRelUpper };
        var z = VmADecalBase.GetRealZ(d, true, 0, (_, _) => 10, (_, _) => 20, new object(), new object());
        Assert.Equal(21, z);
    }

    [Fact]
    public void StickToWall_OneSidedMidTexture()
    {
        var d = new VmADecalBase.DecalState { Z = 10 };
        object? head = null;
        var tex = VmADecalBase.StickToWall(
            d,
            new object(),
            1,
            2,
            null,
            true,
            null,
            0,
            (_, _) => 7,
            (_, _) => 4,
            (_, _) => 0,
            (_, _) => 20,
            _ => head,
            (_, v) => head = v,
            _ => null,
            (_, _) => { },
            t => t == 7,
            (_, _, _) => { });
        Assert.Equal(7, tex);
        Assert.NotEqual(0, d.RenderFlags & VmADecalBase.RfRelMid);
    }

    [Fact]
    public void ClassifyStickPlacement_LowerWhenAboveFloor()
    {
        var branch = VmADecalBase.ClassifyStickPlacement(8, 10, 20, false, true);
        Assert.Equal(VmADecalBase.StickPlacementBranch.Lower, branch);
    }

    [Fact]
    public void ResolveFfTexture_PrefersUpperFlag()
    {
        var tex = VmADecalBase.ResolveFfTexture(
            VmADecalBase.FfUppertexture,
            () => 1,
            () => 2,
            () => 3);
        Assert.Equal(1, tex);
    }

    [Fact]
    public void RunSerializeFieldsPipeline_CopiesState()
    {
        var d = new VmADecalBase.DecalState { Z = 4, PicNum = 9, Side = new object() };
        var sink = new VmADecalBase.DecalSerializeSink();
        VmADecalBase.RunSerializeFieldsPipeline(d, sink);
        Assert.Equal(4, sink.Z);
        Assert.Equal(9, sink.PicNum);
        Assert.Same(d.Side, sink.Side);
    }

    [Fact]
    public void RunOnDestroyPipeline_InvokesRemove()
    {
        var removed = false;
        VmADecalBase.RunOnDestroyPipeline(new VmADecalBase.DecalState(), _ => removed = true, null);
        Assert.True(removed);
    }
}
