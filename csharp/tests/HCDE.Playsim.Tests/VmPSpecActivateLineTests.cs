namespace HCDE.Playsim.Tests;

public class VmPSpecActivateLineTests
{
    [Fact]
    public void ExitBlockedByMonsters_WhenIncomplete() =>
        Assert.True(VmPSpecActivateLine.ExitBlockedByMonsters(true, 1, 5));

    [Fact]
    public void RemoteSpecial_Teleport7() =>
        Assert.False(VmPSpecActivateLine.RemoteSpecial(7));

    [Fact]
    public void ClearSpecialWhenNonRepeat() =>
        Assert.True(VmPSpecActivateLine.ClearSpecialWhenNonRepeat(false, true));

    [Fact]
    public void DummySwitchCompat_WhenNoSector() =>
        Assert.True(VmPSpecActivateLine.DummySwitchCompat(true, false, 10, true, false));

    [Fact]
    public void PreActivateFirstSideOnly() =>
        Assert.True(VmPSpecActivateLine.TestActivateFirstSideOnly(1, 1));
}
