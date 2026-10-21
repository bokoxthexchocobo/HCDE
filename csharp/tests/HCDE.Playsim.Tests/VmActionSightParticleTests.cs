namespace HCDE.Playsim.Tests;

public class VmActionSightParticleTests
{
    [Fact]
    public void ClampParticleStartAlpha_BoundsUnit() =>
        Assert.Equal(1.0, VmActionSightParticle.ClampParticleStartAlpha(2));

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
    public void DoCheckSightOrRange_DistanceBeforeSight()
    {
        Assert.True(VmActionSightParticle.DoCheckSightOrRange(false, 1, 0, 0, true, 4, true, false));
        Assert.False(VmActionSightParticle.DoCheckSightOrRange(true, 1, 0, 0, true, 4, true, true));
    }

    [Fact]
    public void RotateParticleExRelPos_MatchesNative() =>
        Assert.Equal(1.0, VmActionSightParticle.RotateParticleExRelPos(1, 0, 0, 1).x);

    [Fact]
    public void SquaredRange_DoublesInput() =>
        Assert.Equal(25, VmActionSightParticle.SquaredRange(5));
}
