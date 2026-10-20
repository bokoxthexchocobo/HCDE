namespace HCDE.Playsim.Tests;

public class VmPMapAimLineAttackTests
{
    [Fact]
    public void SpectralSkip_WhenPuffNotSpectral() =>
        Assert.True(VmPMapAimLineAttack.SpectralSkip(false, true));

    [Fact]
    public void WeaponNoAutoaim_Flag() =>
        Assert.True(VmPMapAimLineAttack.WeaponNoAutoaim(true, VmPMapAimLineAttack.WifNoautoaim));

    [Fact]
    public void SetResultWhenCloser() =>
        Assert.True(VmPMapAimLineAttack.SetResultWhenCloser(0.5, 1.0));

    [Fact]
    public void ShouldSkipCheckForActor_Spectral()
    {
        var ctx = new VmPMapAimLineAttack.CheckForActorSkipContext(
            false, false, true, false, false, false, false, false, false, false, false, false, false);
        Assert.True(VmPMapAimLineAttack.ShouldSkipCheckForActor(ctx));
    }

    [Fact]
    public void TraceHitActor_StopsOnActor() =>
        Assert.True(VmPMapAimLineAttack.TraceHitActor(2));

    [Fact]
    public void HitscanPreFired_Blocks() =>
        Assert.True(VmPMapAimLineAttack.HitscanPreFired(true));

    [Fact]
    public void SuppressLocalPlayerHitscan_Clears() =>
        Assert.True(VmPMapAimLineAttack.SuppressLocalPlayerHitscan(true));
}
