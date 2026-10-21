namespace HCDE.Playsim.Tests;

public class VmActionFadeSpawnTests
{
    [Fact]
    public void FadeReduceDefault_UsesTenthWhenZero() =>
        Assert.Equal(0.1, VmActionFadeSpawn.FadeReduceDefault(0));

    [Fact]
    public void StepFadeToward_MovesTowardTarget() =>
        Assert.Equal(0.3, VmActionFadeSpawn.StepFadeToward(0.2, 0.5, 0.1), 10);

    [Fact]
    public void StepFadeOut_ClampsAtZero() =>
        Assert.Equal(0.0, VmActionFadeSpawn.StepFadeOut(0.05, 0.1, VmActionFadeSpawn.FtfClamp).alpha);

    [Fact]
    public void FinishFadeTo_RemovesAtTarget() =>
        Assert.True(VmActionFadeSpawn.FinishFadeTo(0.5, 0.5, VmActionFadeSpawn.FtfRemove).shouldRemove);

    [Fact]
    public void NormalizeDebrisMultipliers_DefaultsNonPositive() =>
        Assert.Equal((1.0, 1.0), VmActionFadeSpawn.NormalizeDebrisMultipliers(0, -1));

    [Fact]
    public void DebrisOffsetFromPr_ScalesAround128() =>
        Assert.Equal(0.0, VmActionFadeSpawn.DebrisOffsetFromPr(128).xo);

    [Fact]
    public void ClampParticleStartAlpha_BoundsUnit() =>
        Assert.Equal(1.0, VmActionFadeSpawn.ClampParticleStartAlpha(2));

    [Fact]
    public void RotateRelPos_MatchesSpawnParticleBranch() =>
        Assert.Equal(1.0, VmActionFadeSpawn.RotateRelPos(1, 0, 0, 1).x);

    [Fact]
    public void ShouldSpawnParticle_WhenLifetimeNonZero() =>
        Assert.True(VmActionFadeSpawn.ShouldSpawnParticle(10));
}
