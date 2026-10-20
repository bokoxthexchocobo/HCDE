namespace HCDE.Client.Tests;

public class VmAutomapDrawGeometryTests
{
    private static VmAutomapDrawGeometry.ClipContext Ctx() =>
        new(0, 0, 100, 100, 200, 150, 2.0);

    [Fact]
    public void ResolveFramebufferClearMode_Mode2_RavenColorset_UsesBackground()
    {
        var mode = VmAutomapDrawGeometry.ResolveFramebufferClearMode(
            mapBackValid: true,
            amDrawMapBack: 2,
            amCustomColors: false,
            amModDefined: false,
            amColorset: 3,
            isRavenGame: false);
        Assert.Equal(VmAutomapDrawGeometry.FramebufferClearMode.TiledMapBackground, mode);
    }

    [Fact]
    public void ResolveFramebufferClearMode_Mode2_CustomMod_UsesBackground()
    {
        var mode = VmAutomapDrawGeometry.ResolveFramebufferClearMode(
            mapBackValid: true,
            amDrawMapBack: 2,
            amCustomColors: true,
            amModDefined: true,
            amColorset: 0,
            isRavenGame: false);
        Assert.Equal(VmAutomapDrawGeometry.FramebufferClearMode.TiledMapBackground, mode);
    }

    [Fact]
    public void ResolveFramebufferClearMode_Mode2_NoQualifier_Solid()
    {
        var mode = VmAutomapDrawGeometry.ResolveFramebufferClearMode(
            mapBackValid: true,
            amDrawMapBack: 2,
            amCustomColors: false,
            amModDefined: false,
            amColorset: 1,
            isRavenGame: false);
        Assert.Equal(VmAutomapDrawGeometry.FramebufferClearMode.SolidColor, mode);
    }

    [Fact]
    public void TryClipMapLine_FullyInside_ReturnsFramePoints()
    {
        var ctx = Ctx();
        var ok = VmAutomapDrawGeometry.TryClipMapLine(
            new VmAutomapDrawGeometry.MapPoint(25, 25),
            new VmAutomapDrawGeometry.MapPoint(75, 75),
            ctx,
            out var a,
            out var b);
        Assert.True(ok);
        Assert.InRange(a.X, 0, ctx.FrameWidth);
        Assert.InRange(b.X, 0, ctx.FrameWidth);
    }

    [Fact]
    public void TryClipMapLine_TrivialOutsideY_Rejects()
    {
        var ctx = Ctx();
        var ok = VmAutomapDrawGeometry.TryClipMapLine(
            new VmAutomapDrawGeometry.MapPoint(10, -5),
            new VmAutomapDrawGeometry.MapPoint(20, -10),
            ctx,
            out _,
            out _);
        Assert.False(ok);
    }

    [Fact]
    public void TryClipMapLine_CrossingLeftEdge_Clips()
    {
        var ctx = new VmAutomapDrawGeometry.ClipContext(0, 0, 100, 100, 200, 150, 2.0);
        var ok = VmAutomapDrawGeometry.TryClipMapLine(
            new VmAutomapDrawGeometry.MapPoint(-10, 50),
            new VmAutomapDrawGeometry.MapPoint(50, 50),
            ctx,
            out var a,
            out var b);
        Assert.True(ok);
        Assert.Equal(0, a.X, 3);
        Assert.True(b.X > a.X);
    }

    [Fact]
    public void PlanDrawLine_Antialiasing_Thick_UsesAaDivideThree()
    {
        var plan = VmAutomapDrawGeometry.PlanDrawLine(
            new VmAutomapDrawGeometry.MapPoint(25, 25),
            new VmAutomapDrawGeometry.MapPoint(75, 75),
            Ctx(),
            frameOffsetX: 10,
            frameOffsetY: 20,
            amLineThickness: 4,
            lineThicknessScaled: 1,
            antialiasing: true,
            amLineAlpha: 200);
        Assert.Equal(VmAutomapDrawGeometry.DrawLineKind.AntialiasedThick, plan.Kind);
        Assert.Equal(2, plan.Thickness);
        Assert.Equal(200 * 255 / 3, plan.Alpha255);
        Assert.Equal(10 + (int)(25 * 2.0), plan.ScreenLine.X1);
    }

    [Fact]
    public void PlanDrawLine_NoAa_UsesThickWhenThicknessAtLeastTwo()
    {
        var plan = VmAutomapDrawGeometry.PlanDrawLine(
            new VmAutomapDrawGeometry.MapPoint(25, 25),
            new VmAutomapDrawGeometry.MapPoint(75, 75),
            Ctx(),
            0,
            0,
            amLineThickness: 0,
            lineThicknessScaled: 2,
            antialiasing: false,
            amLineAlpha: 1);
        Assert.Equal(VmAutomapDrawGeometry.DrawLineKind.Thick, plan.Kind);
        Assert.Equal(2, plan.Thickness);
        Assert.Equal(255, plan.Alpha255);
    }

    [Fact]
    public void PlanDrawLine_ClippedAway_ReturnsDefault()
    {
        var plan = VmAutomapDrawGeometry.PlanDrawLine(
            new VmAutomapDrawGeometry.MapPoint(-50, -50),
            new VmAutomapDrawGeometry.MapPoint(-40, -40),
            Ctx(),
            0,
            0,
            1,
            1,
            false,
            1);
        Assert.Equal(default(VmAutomapDrawGeometry.DrawLinePlan), plan);
    }
}
