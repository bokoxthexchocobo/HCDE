namespace HCDE.Playsim.Tests;

public class VmPMobjExplodeMissile1110SliceTests
{
    [Fact]
    public void RunExplodeMissile_AbortsWhenExploCountBelowThreshold()
    {
        var ctx = new VmPMobjExplodeMissile1110Slice.ExplodeMissileContext
        {
            Flags3 = VmPMobjExplodeMissile1110Slice.Mf3ExploCount,
            Threshold = 0,
            DefThreshold = 3
        };
        VmPMobjExplodeMissile1110Slice.RunExplodeMissile(ctx);
        Assert.True(ctx.Aborted);
        Assert.Equal(1, ctx.Threshold);
    }

    [Fact]
    public void RunExplodeMissile_DestroysOnHorizonWithoutSkyExplode()
    {
        var ctx = new VmPMobjExplodeMissile1110Slice.ExplodeMissileContext
        {
            HasLine = true,
            LineSpecial = VmPMobjExplodeMissile1110Slice.LineHorizon
        };
        VmPMobjExplodeMissile1110Slice.RunExplodeMissile(ctx);
        Assert.True(ctx.DestroyOnSky);
    }

    [Fact]
    public void ClampDecalFraction_ClampsToUnitInterval()
    {
        Assert.Equal(0, VmPMobjExplodeMissile1110Slice.ClampDecalFraction(-2, 4));
        Assert.Equal(1, VmPMobjExplodeMissile1110Slice.ClampDecalFraction(8, 4));
    }

    [Fact]
    public void RunExplodeMissile_AppliesAddRocketStyle()
    {
        var ctx = new VmPMobjExplodeMissile1110Slice.ExplodeMissileContext
        {
            Flags5 = VmPMobjExplodeMissile1110Slice.Mf5DehExplosion,
            ExplosionStyle = 255,
            AddRocketExplosion = true,
            Tics = 5
        };
        VmPMobjExplodeMissile1110Slice.RunExplodeMissile(ctx);
        Assert.Equal(VmPMobjExplodeMissile1110Slice.StyleAdd, ctx.RenderStyle);
        Assert.Equal(1.0, ctx.Alpha);
    }
}
