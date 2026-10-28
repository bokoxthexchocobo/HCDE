namespace HCDE.Playsim.Tests;

public class VmPLnspecThingHateTests
{
    [Fact]
    public void HateTarget_RejectsDormant()
    {
        Assert.False(VmPLnspecThingHate.IsHateTargetValid(true, 50, true));
    }

    [Fact]
    public void HateFlags_SetsNoSightForMode2()
    {
        Assert.NotEqual(0, VmPLnspecThingHate.ApplySightCheckFlags(0, 2) & VmPLnspecThingHate.Mf3NoSightCheck);
    }

    [Fact]
    public void HateFlags_SetsHuntPlayersForMode4()
    {
        Assert.NotEqual(0, VmPLnspecThingHate.ApplyHuntPlayersFlags(0, 4) & VmPLnspecThingHate.Mf3HuntPlayers);
    }
}
