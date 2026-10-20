namespace HCDE.Playsim.Tests;

public class VmPMapCheckPositionTests
{
    [Fact]
    public void HeightFit_WhenRoomEnough() =>
        Assert.True(VmPMapCheckPosition.HeightFit(64, 0, 56));

    [Fact]
    public void PassablePortalCheck() =>
        Assert.True(VmPMapCheckPosition.PassablePortalCheck(true, false));

    [Fact]
    public void ShouldDealDamage_WhenForcePain() =>
        Assert.True(VmPMapCheckPosition.ShouldDealDamage(0, 1, 0));

    [Fact]
    public void HealFromNegativeDamage() =>
        Assert.Equal(3, VmPMapCheckPosition.HealFromNegativeDamage(-3));
}
