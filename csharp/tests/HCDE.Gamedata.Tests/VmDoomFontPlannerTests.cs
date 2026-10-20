namespace HCDE.Gamedata.Tests;

public class VmDoomFontPlannerTests
{
    [Fact]
    public void PlanSmallFont_PrefersFontAWhenPresent()
    {
        Assert.Equal(VmDoomSmallFontRecipe.FontADefault, VmDoomFontPlanner.PlanSmallFont(true, 0, 1, false, 0));
    }

    [Fact]
    public void HuFontSize_MatchesNativeRange()
    {
        Assert.Equal(223, VmDoomFontPlanner.HuFontSize);
    }
}
