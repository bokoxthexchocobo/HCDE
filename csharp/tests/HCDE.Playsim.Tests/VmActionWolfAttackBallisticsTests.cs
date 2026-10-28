namespace HCDE.Playsim.Tests;

public class VmActionWolfAttackBallisticsTests
{
    [Fact]
    public void HitchanceFromSpeed_Uses256WhenSlow() =>
        Assert.Equal(256, VmActionWolfAttackBallistics.HitchanceFromSpeed(0, 1));

    [Fact]
    public void AdjustHitchanceForDistance_SubtractsDodgePenalty() =>
        Assert.Equal(240, VmActionWolfAttackBallistics.AdjustHitchanceForDistance(256, 1, dodge: true));

    [Fact]
    public void DamageFromFlags_UsesRandomWhenAllowed() =>
        Assert.Equal(3, VmActionWolfAttackBallistics.DamageFromFlags(0, 5, roll: 2));

    [Fact]
    public void ShouldSpawnBlood_FalseWhenNoBlood() =>
        Assert.False(VmActionWolfAttackBallistics.ShouldSpawnBlood(VmActionWolfAttackBallistics.MfNoblood, 0));
}
