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
}
