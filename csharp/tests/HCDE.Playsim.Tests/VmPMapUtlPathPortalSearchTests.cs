namespace HCDE.Playsim.Tests;

public class VmPMapUtlPathPortalSearchTests
{
    [Fact]
    public void CheckFov_AcceptsTargetInsideCone()
    {
        Assert.True(VmPMapUtlPathPortalSearch.CheckFov(90, 100, 15));
        Assert.False(VmPMapUtlPathPortalSearch.CheckFov(90, 45, 15));
    }

    [Fact]
    public void PortalRelocate_ReinitsTraceForLinkedPortal()
    {
        var line = new object();
        var ctx = new VmPMapUtlPathPortalSearch.PortalRelocateContext
        {
            IsLinePortal = (_, _, _) => true,
            PointOnLineSide = (_, _, _) => 0,
            TranslatePortalXy = (_, ref x, ref y) => { x += 10; y += 10; },
            PortalType = _ => 1,
        };
        ctx.Trace.Dx = 5;
        ctx.Intercept.IsLine = true;
        ctx.Intercept.Line = line;
        ctx.Intercept.Frac = 0.5;
        var reinit = false;
        ctx.ReinitTrace = (_, _, _, _, _, _) => reinit = true;
        var result = VmPMapUtlPathPortalSearch.PortalRelocate(ctx, 0, ref double _, false);
        Assert.Equal(1, result);
        Assert.True(reinit);
    }

    [Fact]
    public void BlockmapSearch_ScansRingWhenCenterEmpty()
    {
        var found = VmPMapUtlPathPortalSearch.BlockmapSearch(
            2,
            2,
            8,
            8,
            1,
            (_, _) => null,
            idx => idx == 2 * 8 + 3 ? "ring" : null);
        Assert.Equal("ring", found);
    }

    [Fact]
    public void BlockmapSearch_FindsCenterBlockFirst()
    {
        var found = VmPMapUtlPathPortalSearch.BlockmapSearch(
            2,
            2,
            8,
            8,
            2,
            (x, y) => x == 2 && y == 2 ? "center" : null,
            _ => null);
        Assert.Equal("center", found);
    }

    [Fact]
    public void RoughBlockCheck_ReturnsAttackableActorInFov()
    {
        var viewer = new object();
        var target = new object();
        var ctx = new VmPMapUtlPathPortalSearch.RoughBlockCheckContext
        {
            Viewer = viewer,
            ActorsInBlock = new[] { viewer, target },
            Fov = 90,
            CheckFov = (_, _, _) => true,
            IsOkayToAttack = (_, o) => ReferenceEquals(o, target),
        };
        Assert.Same(target, VmPMapUtlPathPortalSearch.RoughBlockCheck(ctx));
    }
}
