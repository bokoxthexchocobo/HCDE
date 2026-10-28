namespace HCDE.Gamedata.Tests;

public class VmDoomFontPlannerTests
{
    [Fact]
    public void PlanSmallFont_PrefersFontAWhenPresent()
    {
        Assert.Equal(VmDoomSmallFontRecipe.FontADefault, VmDoomFontPlanner.PlanSmallFont(true, 0, 1, false, 0));
    }

    [Fact]
    public void ResolveIntermissionFont_PrefersWinum0()
    {
        Assert.Equal("IntermissionFont_Doom", VmDoomFontPlanner.ResolveIntermissionFont(true, "BigFont"));
    }

    [Fact]
    public void HuFontSize_MatchesNativeRange()
    {
        Assert.Equal(223, VmDoomFontPlanner.HuFontSize);
    }

    [Fact]
    public void PlanBigFont_UsesZBigWhenNoFontB()
    {
        var (big, _) = VmDoomFontPlanner.PlanBigFont(false, true);
        Assert.Equal("ZBIGFONT", big);
    }
}
