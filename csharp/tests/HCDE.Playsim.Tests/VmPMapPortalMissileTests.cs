namespace HCDE.Playsim.Tests;

public class VmPMapPortalMissileTests
{
    [Fact]
    public void IgnorePortalInVerticalSlice_WhenNoFloor() =>
        Assert.True(VmPMapPortalMissile.IgnorePortalInVerticalSlice(VmPMapPortalMissile.FfcfNofloor));

    [Fact]
    public void CheckRipLevel_RejectsLowProjectile() =>
        Assert.False(VmPMapPortalMissile.CheckRipLevel(5, 0, 2));

    [Fact]
    public void ProjectileImmuneSameGroup_WhenGrouped() =>
        Assert.True(VmPMapPortalMissile.ProjectileImmuneSameGroup(3, 3));

    [Fact]
    public void FriendsBlockedUnlessHarmFriends() =>
        Assert.True(VmPMapPortalMissile.FriendsBlockedUnlessHarmFriends(true, false));

    [Fact]
    public void MonstersBlockedWhenNoInfight() =>
        Assert.True(VmPMapPortalMissile.MonstersBlockedWhenNoInfight(-1, true));

    [Fact]
    public void ShouldDealDamage_WhenForcePain() =>
        Assert.True(VmPMapPortalMissile.ShouldDealDamage(0, VmPMapPortalMissile.Mf6Forcepain, 0));

    [Fact]
    public void AdjustCeilingWhenLower() =>
        Assert.True(VmPMapPortalMissile.AdjustCeilingWhenLower(32, 4, 40));

    [Fact]
    public void HealAmountFromNegativeDamage() =>
        Assert.Equal(7, VmPMapPortalMissile.HealAmountFromNegativeDamage(-7));
}
