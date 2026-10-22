namespace HCDE.Playsim.Tests;

public class VmAScrollTests
{
    [Fact]
    public void ScrollPosAnd_IntersectsFlags() =>
        Assert.Equal(2, VmAScroll.ScrollPosAnd(3, 2));

    [Fact]
    public void RotationComp_IdentityWhenAngleZero()
    {
        VmAScroll.RotationComp(0, 4, 5, out var tdx, out var tdy);
        Assert.Equal(4, tdx);
        Assert.Equal(5, tdy);
    }

    [Fact]
    public void TickStep_ScrollsFloorWhenNonZero()
    {
        var state = new VmAScroll.ScrollState { Type = 2, Dx = 1, Dy = 0 };
        var hit = false;
        VmAScroll.TickStep(
            state,
            null,
            _ => 0,
            (_, dx, dy, _) => (dx, dy),
            (_, _) => { },
            (_, _) => { },
            (_, _) => { },
            (_, _) => hit = true,
            (_, _) => { },
            (_, _) => { },
            _ => false);
        Assert.True(hit);
    }
}
