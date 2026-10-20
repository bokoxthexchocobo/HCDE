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
}
