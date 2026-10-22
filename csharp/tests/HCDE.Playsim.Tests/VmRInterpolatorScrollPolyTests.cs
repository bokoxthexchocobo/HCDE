namespace HCDE.Playsim.Tests;

public class VmRInterpolatorScrollPolyTests
{
    [Fact]
    public void ScrollInterp_LerpsOffsets()
    {
        var scroll = new VmRInterpolatorScrollPoly.ScrollInterp2D { OldX = 0, OldY = 10 };
        double x = 0, y = 0;
        scroll.InterpolateStep(1, 0.5, () => 20, () => 30, v => x = v, v => y = v, () => { });
        Assert.Equal(10, x);
        Assert.Equal(20, y);
    }

    [Fact]
    public void ScrollInterp_UnlinksWhenUnchanged()
    {
        var scroll = new VmRInterpolatorScrollPoly.ScrollInterp2D { OldX = 5, OldY = 6 };
        var hit = false;
        scroll.InterpolateStep(0, 0.5, () => 5, () => 6, _ => { }, _ => { }, () => hit = true);
        Assert.True(hit);
    }

    [Fact]
    public void PolyInterp_DetectsVertexChange()
    {
        var poly = new VmRInterpolatorScrollPoly.PolyobjInterp();
        poly.UpdateInterpolation(i => i, i => i * 10, 2, () => 0, () => 0);
        var writes = 0;
        poly.InterpolateStep(
            1,
            0.5,
            i => i + 1,
            i => i * 10,
            2,
            (_, _, _) => writes++,
            () => 0,
            () => 0,
            (_, _) => { },
            () => { },
            () => { });
        Assert.Equal(2, writes);
    }

    [Fact]
    public void MapSectorInterpolationPosition_KnownSlots()
    {
        Assert.Equal(VmRInterpolatorScrollPoly.SectorInterpolationKind.ScrollFloor, VmRInterpolatorScrollPoly.MapSectorInterpolationPosition(VmRInterpolatorScrollPoly.SectorFloorScroll));
        Assert.Null(VmRInterpolatorScrollPoly.MapSectorInterpolationPosition(99));
    }

    [Fact]
    public void GetOrCreateSideInterpolation_CreatesOnce()
    {
        var created = 0;
        var refs = 0;
        var first = VmRInterpolatorScrollPoly.GetOrCreateSideInterpolation(
            null,
            () => { created++; return new object(); },
            _ => { },
            _ => refs++);
        var second = VmRInterpolatorScrollPoly.GetOrCreateSideInterpolation(
            first,
            () => { created++; return new object(); },
            _ => { },
            _ => refs++);
        Assert.Same(first, second);
        Assert.Equal(1, created);
        Assert.Equal(2, refs);
    }
}
