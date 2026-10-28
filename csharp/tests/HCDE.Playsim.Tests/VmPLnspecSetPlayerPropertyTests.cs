namespace HCDE.Playsim.Tests;

public class VmPLnspecSetPlayerPropertyTests
{
    [Fact]
    public void PowerBranch_RejectsNoneEntry()
    {
        Assert.False(VmPLnspecSetPlayerProperty.PowerEntryValid(8));
    }

    [Fact]
    public void FrozenCompat_ClearsBothFlags()
    {
        var mask = VmPLnspecSetPlayerProperty.FrozenCompatMask(VmPLnspecSetPlayerProperty.CfFrozen, true);
        Assert.Equal(VmPLnspecSetPlayerProperty.CfFrozen | VmPLnspecSetPlayerProperty.CfTotallyFrozen, mask);
    }

    [Fact]
    public void Noclip2_RequiresBothBits()
    {
        Assert.Equal(
            VmPLnspecSetPlayerProperty.CfNoclip | VmPLnspecSetPlayerProperty.CfNoclip2,
            VmPLnspecSetPlayerProperty.CheatMaskForProperty(VmPLnspecSetPlayerProperty.PropNoclip2));
    }
}
