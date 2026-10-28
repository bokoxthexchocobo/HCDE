namespace HCDE.Playsim.Tests;

public class VmActionDoRadiusGiveTests
{
    [Fact]
    public void RadiusGiveAbortsEarly_WhenDistanceZero() =>
        Assert.True(VmActionDoRadiusGive.RadiusGiveAbortsEarly(new object(), 1, 0, 0, true));

    [Fact]
    public void NormalizeGiveAmount_DefaultsToOne() =>
        Assert.Equal(1, VmActionDoRadiusGive.NormalizeGiveAmount(0));

    [Fact]
    public void TryPassSelf_RequiresFlag() =>
        Assert.False(VmActionDoRadiusGive.TryPassSelf(true, 0));

    [Fact]
    public void TryPassMissile_WhenFlagged() =>
        Assert.True(VmActionDoRadiusGive.TryPassMissile(true, VmActionRadiusGiveEligibility.RgfMissiles));

    [Fact]
    public void TryPassLivingActor_PlayerBranch() =>
        Assert.True(VmActionDoRadiusGive.TryPassLivingActor(
            VmActionRadiusGiveEligibility.RgfPlayers, 10, isPlayerMo: true, isVoodoo: false,
            isMonster: false, isShootableObject: false));

    [Fact]
    public void FilterSpeciesGate_AllowsSelfDespiteFilter() =>
        Assert.True(VmActionDoRadiusGive.FilterSpeciesGate(false, false, 0, isSelf: true));

    [Fact]
    public void PointerRelationGate_BlocksNotargetHit() =>
        Assert.False(VmActionDoRadiusGive.PointerRelationGate(
            isSelf: false,
            isTarget: true,
            isMaster: false,
            isTracer: false,
            VmActionRadiusGiveEligibility.RgfNotarget));

    [Fact]
    public void OutsideRange_UsesCubeWhenFlagged() =>
        Assert.True(VmActionDoRadiusGive.OutsideRange(
            VmActionRadiusGiveEligibility.RgfCube, 200, 0, 0, 0, 100, 0));

    [Fact]
    public void ShouldIterateMissiles_WhenFlagged() =>
        Assert.True(VmActionDoRadiusGive.ShouldIterateMissiles(VmActionRadiusGiveEligibility.RgfMissiles));
}
