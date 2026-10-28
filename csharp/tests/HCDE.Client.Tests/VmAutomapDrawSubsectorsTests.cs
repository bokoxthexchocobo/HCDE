namespace HCDE.Client.Tests;

public class VmAutomapDrawSubsectorsTests
{
    [Fact]
    public void ShouldSkipSubsector_HiddenWithoutCheat_Skips()
    {
        Assert.True(VmAutomapDrawSubsectors.ShouldSkipSubsector(
            0, VmAutomapDrawSubsectors.SectorHidden, 0, false, 0, 0));
    }

    [Fact]
    public void ShouldSkipSubsector_PortalOverlay_SkipsForeignGroup()
    {
        Assert.True(VmAutomapDrawSubsectors.ShouldSkipSubsector(
            0, 0, 1, true, 2, 0));
    }

    [Fact]
    public void ApplyUndrawnCheatTint_WashesOutColors()
    {
        var (r, g, b, _) = VmAutomapDrawSubsectors.ApplyUndrawnCheatTint(100, 80, 60, 0, false, 0);
        Assert.Equal((byte)177, r);
        Assert.True(g > 80);
    }

    [Fact]
    public void ResolveFadeLevel_SoftRamp_UsesDoomFormula()
    {
        var soft = VmAutomapDrawSubsectors.ResolveFadeLevel(128, true);
        var linear = VmAutomapDrawSubsectors.ResolveFadeLevel(128, false);
        Assert.NotEqual(soft, linear);
    }

    [Fact]
    public void NeedsHoleTriangulation_OnlyForHoleWithManyLines()
    {
        Assert.True(VmAutomapDrawSubsectors.NeedsHoleTriangulation(VmAutomapDrawSubsectors.SubsectorHole, 4));
        Assert.False(VmAutomapDrawSubsectors.NeedsHoleTriangulation(0, 4));
    }

    [Fact]
    public void ShouldPickRoverTop_RequiresViewBelowRover()
    {
        Assert.True(VmAutomapDrawSubsectors.ShouldPickRoverTop(64, 100, 0, 128));
        Assert.False(VmAutomapDrawSubsectors.ShouldPickRoverTop(200, 100, 0, 128));
    }

    [Fact]
    public void PlanPolygonDraw_Textured_SkipsSky()
    {
        var plan = VmAutomapDrawSubsectors.PlanPolygonDraw(
            true, true, false, true, 0, 4, 128, true, 1, 1, 1, 1, 0, 0, false);
        Assert.False(plan.Draw);
    }
}
