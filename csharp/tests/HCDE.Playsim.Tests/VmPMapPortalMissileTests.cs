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
    public void AdjustCeilingWhenLower() =>
        Assert.True(VmPMapPortalMissile.AdjustCeilingWhenLower(32, 4, 40));

    [Fact]
    public void CanAttackHurt_NoInfightBlocksMonsters()
    {
        var ctx = new VmPMapPortalMissile.CanAttackHurtContext(
            false, false, -1, 0, true, true, false, 0, 0, false, false, 0, false);
        Assert.False(VmPMapPortalMissile.EvaluateCanAttackHurt(ctx));
    }

    [Fact]
    public void CanAttackHurt_HarmFriendsAllows()
    {
        var ctx = new VmPMapPortalMissile.CanAttackHurtContext(
            false, false, 0, 0, true, true, false, 0, 0, true, true, 0, false);
        Assert.True(VmPMapPortalMissile.EvaluateCanAttackHurt(ctx));
    }
}
