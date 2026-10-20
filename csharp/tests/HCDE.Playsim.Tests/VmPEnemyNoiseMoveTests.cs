namespace HCDE.Playsim.Tests;

public class VmPEnemyNoiseMoveTests
{
    [Fact]
    public void NoiseAlert_SkipsNullEmitter() =>
        Assert.True(VmPEnemyNoiseMove.NoiseAlertEmitterNull(true));

    [Fact]
    public void NoiseAlert_SkipsNoTargetCheat() =>
        Assert.True(VmPEnemyNoiseMove.NoiseAlertNoTarget(true, true));

    [Fact]
    public void NoiseFlood_SkipsWhenAlreadyFlooded() =>
        Assert.True(VmPEnemyNoiseMove.ShouldSkipNoiseFlood(
            new VmPEnemyNoiseMove.NoiseFloodContext(true, 2, 1)));

    [Fact]
    public void NoiseSplash_RespectsNosplashalert() =>
        Assert.False(VmPEnemyNoiseMove.NoiseSplashActorAlert(true, VmPEnemyNoiseMove.Mf4Nosplashalert));

    [Fact]
    public void HitFriend_BlocksFriendlyLineTarget() =>
        Assert.True(VmPEnemyNoiseMove.HitFriendBlocksShot(true, true));

    [Fact]
    public void OnLift_DetectsSectorFlag() =>
        Assert.True(VmPEnemyNoiseMove.OnLiftSectorFlag(VmPEnemyNoiseMove.SecmfLift));

    [Fact]
    public void UnderDamage_PlayerHarmReturnsSerious() =>
        Assert.Equal(-1, VmPEnemyNoiseMove.EvaluateUnderDamage(
            new VmPEnemyNoiseMove.UnderDamageContext(0, 5, VmPEnemyNoiseMove.SecmfHurtinair, false, false, true, 0)));

    [Fact]
    public void CheckTags_PrimaryMatch() =>
        Assert.True(VmPEnemyNoiseMove.CheckTagsPrimaryMatch(7, 7));
}
