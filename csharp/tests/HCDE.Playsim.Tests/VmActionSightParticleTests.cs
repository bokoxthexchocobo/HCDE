namespace HCDE.Playsim.Tests;

public class VmActionSightParticleTests
{
    [Fact]
    public void NormalizeParticleStyle_ClampsInvalid() =>
        Assert.Equal(0, VmActionSightParticle.NormalizeParticleStyle(-1));

    [Fact]
    public void ClampFadeStep_AllowsNegativeOne() =>
        Assert.Equal(-1.0, VmActionSightParticle.ClampFadeStep(-2));

    [Fact]
    public void ShouldSpawnParticleEx_WhenLifetimeSet() =>
        Assert.True(VmActionSightParticle.ShouldSpawnParticleEx(5));

    [Fact]
    public void VerticalSightDelta_UsesTopWhenAbove() =>
        Assert.Equal(-2, VmActionSightParticle.VerticalSightDelta(10, 8, 0));

    [Fact]
    public void SquaredDistance2d_IgnoresZIn2dMode() =>
        Assert.Equal(25, VmActionSightParticle.SquaredDistance2d(3, 4, 9, true));

    [Fact]
    public void WithinRange_ComparesSquared() =>
        Assert.True(VmActionSightParticle.WithinRange(4, 9));

    [Fact]
    public void CheckIfSeenShouldJump_WhenNobodySees() =>
        Assert.True(VmActionSightParticle.CheckIfSeenShouldJump(false));

    [Fact]
    public void SquaredRange_DoublesInput() =>
        Assert.Equal(25, VmActionSightParticle.SquaredRange(5));
}
