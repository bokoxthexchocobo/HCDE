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
    public void SetScrollerPass_CreatesWhenNoMatch()
    {
        var created = false;
        VmAScroll.SetScrollerPass(
            Array.Empty<object>(),
            1,
            2,
            1,
            0,
            _ => 2,
            _ => 1,
            (_, _, _) => { },
            (_, _, _, _) => created = true);
        Assert.True(created);
    }

    [Fact]
    public void WallConstructRates_ComputesNonZero()
    {
        VmAScroll.WallConstructRates(1, 0, 10, 0, out var x, out _);
        Assert.NotEqual(0, x);
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
