namespace HCDE.Gamedata.Tests;

public class VmDehackedThingGroupLimitsTests
{
    [Fact]
    public void ClampInfightingGroup_RejectsNegative()
    {
        Assert.Equal(0, VmDehackedThingGroupLimits.ClampInfightingGroup(-1));
        Assert.Equal(3, VmDehackedThingGroupLimits.ClampInfightingGroup(3));
    }
}
